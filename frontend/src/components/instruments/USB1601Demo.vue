<template>
  <div class="usb1601-simulator-demo professional-control">
    <!-- 控件标题 -->
    <div class="control-header">
      <div class="header-left">
        <div class="title-section">
          <h2 class="control-title">
            <el-icon class="title-icon"><Cpu /></el-icon>
            USB-1601 高精度数据采集卡
          </h2>
          <p class="control-subtitle">16通道AI/2通道AO/8路DIO - 支持真实硬件与模拟模式</p>
        </div>
      </div>
      <div class="header-right">
        <el-switch
          v-model="useHardware"
          active-text="硬件模式"
          inactive-text="模拟模式"
          :loading="modeSwitching"
          @change="switchMode"
          style="margin-right: 12px"
        />
        <el-tag :type="useHardware ? 'danger' : 'success'" effect="dark">
          {{ useHardware ? '硬件模式' : '模拟模式' }}
        </el-tag>
        <el-tag :type="isRunning ? 'warning' : 'info'" effect="plain">
          {{ isRunning ? '运行中' : '已停止' }}
        </el-tag>
      </div>
    </div>

    <div class="simulator-layout">
      <!-- 左侧控制面板 -->
      <div class="control-panel">
        <!-- 采集控制区 -->
        <el-card class="control-section">
          <template #header>
            <div class="section-header">
              <el-icon><Setting /></el-icon>
              <span>采集控制</span>
            </div>
          </template>
          
          <div class="control-group">
            <div class="control-row">
              <el-button 
                type="primary" 
                :icon="isRunning ? VideoPause : VideoPlay"
                @click="toggleAcquisition"
                size="large"
                class="action-button"
              >
                {{ isRunning ? '停止采集' : '开始采集' }}
              </el-button>
              <el-button 
                :icon="Delete" 
                @click="clearData"
                size="large"
                class="action-button"
              >
                清除数据
              </el-button>
            </div>
            
            <div class="control-row" v-if="useHardware">
              <el-checkbox v-model="selfTestMode">
                自发自收测试模式
              </el-checkbox>
              <el-tooltip content="AO通道生成测试信号，AI通道采集" placement="right">
                <el-icon style="margin-left: 4px"><InfoFilled /></el-icon>
              </el-tooltip>
            </div>
            
            <div class="control-row">
              <label class="control-label">采样频率:</label>
              <el-select v-model="sampleRate" @change="onSampleRateChange">
                <el-option label="1 kHz" :value="1000" />
                <el-option label="10 kHz" :value="10000" />
                <el-option label="50 kHz" :value="50000" />
                <el-option label="100 kHz" :value="100000" />
              </el-select>
            </div>
            
            <div class="control-row">
              <label class="control-label">缓冲区大小:</label>
              <el-select v-model="bufferSize" @change="onBufferSizeChange">
                <el-option label="1000 点" :value="1000" />
                <el-option label="5000 点" :value="5000" />
                <el-option label="10000 点" :value="10000" />
              </el-select>
            </div>
          </div>
        </el-card>

        <!-- 通道配置区 -->
        <el-card class="control-section">
          <template #header>
            <div class="section-header">
              <el-icon><DataAnalysis /></el-icon>
              <span>AI通道配置</span>
            </div>
          </template>
          
          <div class="channel-config">
            <div v-for="channel in channels" :key="channel.id" class="channel-item">
              <div class="channel-header">
                <el-switch 
                  v-model="channel.enabled"
                  @change="onChannelToggle(channel.id)"
                />
                <div class="channel-color" :style="{ backgroundColor: channel.color }"></div>
                <span class="channel-name">{{ channel.name }}</span>
              </div>
              
              <div class="channel-controls" v-if="channel.enabled">
                <div class="signal-type">
                  <el-select v-model="channel.signalType" size="small" @change="updateSignal">
                    <el-option label="正弦波" value="sine" />
                    <el-option label="方波" value="square" />
                    <el-option label="三角波" value="triangle" />
                    <el-option label="锯齿波" value="sawtooth" />
                    <el-option label="噪声" value="noise" />
                  </el-select>
                </div>
                <div class="signal-params">
                  <div class="param-group">
                    <label>频率: {{ channel.frequency }}Hz</label>
                    <el-slider 
                      v-model="channel.frequency"
                      :min="0.1" 
                      :max="100" 
                      :step="0.1"
                      @change="updateSignal"
                      size="small"
                    />
                  </div>
                  <div class="param-group">
                    <label>幅度: {{ channel.amplitude }}V</label>
                    <el-slider 
                      v-model="channel.amplitude"
                      :min="0.1" 
                      :max="10" 
                      :step="0.1"
                      @change="updateSignal"
                      size="small"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </el-card>

        <!-- 统计信息 -->
        <el-card class="control-section">
          <template #header>
            <div class="section-header">
              <el-icon><DataBoard /></el-icon>
              <span>实时统计</span>
            </div>
          </template>
          
          <div class="statistics">
            <div class="stat-item">
              <span class="stat-label">采集点数:</span>
              <span class="stat-value">{{ totalPoints }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">采集时间:</span>
              <span class="stat-value">{{ formatTime(runningTime) }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">数据速率:</span>
              <span class="stat-value">{{ dataRate }} KB/s</span>
            </div>
          </div>
        </el-card>
      </div>

      <!-- 右侧波形显示区 -->
      <div class="waveform-panel">
        <el-card class="waveform-container">
          <template #header>
            <div class="section-header">
              <el-icon><TrendCharts /></el-icon>
              <span>实时波形显示</span>
              <div class="waveform-controls">
                <el-button-group size="small">
                  <el-button @click="zoomIn" :icon="ZoomIn" />
                  <el-button @click="zoomOut" :icon="ZoomOut" />
                  <el-button @click="autoScale" :icon="FullScreen" />
                </el-button-group>
                <el-select v-model="displayMode" size="small" style="width: 120px; margin-left: 12px">
                  <el-option label="时域" value="time" />
                  <el-option label="频域" value="frequency" />
                  <el-option label="XY模式" value="xy" />
                </el-select>
              </div>
            </div>
          </template>
          
          <div class="chart-area">
            <!-- 简化的SVG波形显示 -->
            <div class="simple-waveform">
              <svg width="100%" height="400" viewBox="0 0 800 400">
                <!-- 网格 -->
                <defs>
                  <pattern id="grid" width="40" height="40" patternUnits="userSpaceOnUse">
                    <path d="M 40 0 L 0 0 0 40" fill="none" stroke="#e5e7eb" stroke-width="1"/>
                  </pattern>
                </defs>
                <rect width="100%" height="100%" fill="url(#grid)" />
                
                <!-- 波形 -->
                <g v-for="(channel, index) in enabledChannels" :key="channel.id">
                  <path 
                    :d="generateWaveform(channel, index)"
                    fill="none" 
                    :stroke="channel.color" 
                    stroke-width="2"
                  />
                </g>
                
                <!-- 数值显示 -->
                <g v-for="(channel, index) in enabledChannels" :key="`value-${channel.id}`">
                  <text 
                    :x="20" 
                    :y="30 + index * 20" 
                    :fill="channel.color"
                    font-size="14"
                    font-weight="bold"
                  >
                    {{ channel.name }}: {{ getCurrentValue(channel).toFixed(2) }}V
                  </text>
                </g>
              </svg>
            </div>
          </div>
        </el-card>
      </div>
    </div>

    <!-- 底部信息栏 -->
    <div class="status-bar">
      <div class="status-left">
        <el-icon class="status-icon"><Monitor /></el-icon>
        <span>USB-1601 模拟器已连接</span>
        <el-divider direction="vertical" />
        <span>活跃通道: {{ enabledChannels.length }}</span>
        <el-divider direction="vertical" />
        <span>{{ sampleRate/1000 }}kHz @ 16bit</span>
      </div>
      <div class="status-right">
        <span class="version-info">SeeSharpTools Web v1.0.0</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import { 
  Cpu, Setting, DataAnalysis, DataBoard, TrendCharts, Monitor,
  VideoPlay, VideoPause, Delete, ZoomIn, ZoomOut, FullScreen, InfoFilled
} from '@element-plus/icons-vue'
import axios from 'axios'

// 简化的通道接口
interface SimpleChannel {
  id: string
  name: string
  enabled: boolean
  color: string
  signalType: 'sine' | 'square' | 'triangle' | 'sawtooth' | 'noise'
  frequency: number
  amplitude: number
  phase: number
}

// 响应式数据
const isRunning = ref(false)
const sampleRate = ref(10000)
const bufferSize = ref(1000)
const totalPoints = ref(0)
const runningTime = ref(0)
const displayMode = ref('time')
const currentTime = ref(0)
const useHardware = ref(false)
const modeSwitching = ref(false)
const selfTestMode = ref(false)
const currentTaskId = ref<number | null>(null)

// 简化的通道配置
const channels = ref<SimpleChannel[]>([
  { id: 'AI0', name: 'AI0', enabled: true, color: '#3B82F6', signalType: 'sine', frequency: 5, amplitude: 5, phase: 0 },
  { id: 'AI1', name: 'AI1', enabled: true, color: '#EF4444', signalType: 'sine', frequency: 2, amplitude: 3, phase: Math.PI/2 },
  { id: 'AI2', name: 'AI2', enabled: false, color: '#10B981', signalType: 'square', frequency: 1, amplitude: 4, phase: 0 },
  { id: 'AI3', name: 'AI3', enabled: false, color: '#F59E0B', signalType: 'triangle', frequency: 3, amplitude: 2, phase: 0 }
])

let updateTimer: number | null = null
let statsTimer: number | null = null

// 计算属性
const enabledChannels = computed(() => 
  channels.value.filter(ch => ch.enabled)
)

const dataRate = computed(() => {
  if (!isRunning.value) return '0'
  const bytesPerSecond = enabledChannels.value.length * sampleRate.value * 2
  return (bytesPerSecond / 1024).toFixed(1)
})

// 生命周期钩子
onUnmounted(() => {
  stopAcquisition()
})

// 控制方法
function toggleAcquisition() {
  if (isRunning.value) {
    stopAcquisition()
  } else {
    startAcquisition()
  }
}

async function startAcquisition() {
  try {
    if (useHardware.value) {
      // 硬件模式：调用后端API
      const taskId = Date.now()
      const enabledChs = channels.value.filter(ch => ch.enabled)
      
      const config = {
        taskId,
        configuration: {
          deviceId: 1,
          sampleRate: sampleRate.value,
          channels: enabledChs.map((ch, idx) => ({
            channelId: idx,
            name: ch.name,
            enabled: true,
            rangeMin: -10,
            rangeMax: 10
          })),
          mode: 0, // Continuous
          bufferSize: bufferSize.value,
          selfTestMode: selfTestMode.value
        }
      }
      
      const response = await axios.post('http://localhost:5001/api/DataAcquisition/start', config)
      
      if (response.data.success) {
        currentTaskId.value = taskId
        isRunning.value = true
        totalPoints.value = 0
        runningTime.value = 0
        
        updateTimer = window.setInterval(() => {
          currentTime.value += 0.05
          totalPoints.value += enabledChannels.value.length
        }, 50)
        
        statsTimer = window.setInterval(() => {
          runningTime.value += 0.1
        }, 100)
        
        ElMessage.success('硬件采集已启动')
      } else {
        ElMessage.error('启动硬件采集失败')
      }
    } else {
      // 模拟模式
      isRunning.value = true
      totalPoints.value = 0
      runningTime.value = 0
      
      updateTimer = window.setInterval(() => {
        currentTime.value += 0.05
        totalPoints.value += enabledChannels.value.length
      }, 50)
      
      statsTimer = window.setInterval(() => {
        runningTime.value += 0.1
      }, 100)
      
      ElMessage.success('模拟数据采集已开始')
    }
  } catch (error) {
    console.error('启动采集失败:', error)
    ElMessage.error('启动采集失败')
  }
}

async function stopAcquisition() {
  try {
    if (useHardware.value && currentTaskId.value) {
      // 硬件模式：调用后端API
      const response = await axios.post(`http://localhost:5001/api/DataAcquisition/stop/${currentTaskId.value}`)
      
      if (response.data.success) {
        currentTaskId.value = null
        ElMessage.info('硬件采集已停止')
      }
    } else {
      ElMessage.info('模拟数据采集已停止')
    }
    
    isRunning.value = false
    
    if (updateTimer) {
      clearInterval(updateTimer)
      updateTimer = null
    }
    
    if (statsTimer) {
      clearInterval(statsTimer)
      statsTimer = null
    }
  } catch (error) {
    console.error('停止采集失败:', error)
    ElMessage.error('停止采集失败')
  }
}

function clearData() {
  currentTime.value = 0
  totalPoints.value = 0
  runningTime.value = 0
  ElMessage.success('数据已清除')
}

// 配置变更处理
function onSampleRateChange() {
  ElMessage.success(`采样频率已更改为 ${sampleRate.value/1000}kHz`)
}

function onBufferSizeChange() {
  ElMessage.success(`缓冲区大小已更改为 ${bufferSize.value} 点`)
}

function onChannelToggle(channelId: string) {
  const channel = channels.value.find(ch => ch.id === channelId)
  if (channel) {
    ElMessage.info(`通道 ${channel.name} ${channel.enabled ? '已启用' : '已禁用'}`)
  }
}

function updateSignal() {
  // 信号参数已更新
}

// 波形生成函数
function generateWaveform(channel: SimpleChannel, index: number): string {
  const points: string[] = []
  const centerY = 200 + index * 50
  const timeSpan = 10 // 10秒的时间跨度
  const pointCount = 200
  
  for (let i = 0; i < pointCount; i++) {
    const t = (i / pointCount) * timeSpan + currentTime.value
    let value = 0
    
    switch (channel.signalType) {
      case 'sine':
        value = Math.sin(2 * Math.PI * channel.frequency * t + channel.phase)
        break
      case 'square':
        value = Math.sign(Math.sin(2 * Math.PI * channel.frequency * t + channel.phase))
        break
      case 'triangle':
        const trianglePhase = (channel.frequency * t + channel.phase / (2 * Math.PI)) % 1
        value = trianglePhase < 0.5 ? 4 * trianglePhase - 1 : 3 - 4 * trianglePhase
        break
      case 'sawtooth':
        value = 2 * ((channel.frequency * t + channel.phase / (2 * Math.PI)) % 1) - 1
        break
      case 'noise':
        value = (Math.random() - 0.5) * 2
        break
    }
    
    const x = (i / pointCount) * 760 + 20
    const y = centerY - value * channel.amplitude * 20
    
    points.push(i === 0 ? `M ${x} ${y}` : `L ${x} ${y}`)
  }
  
  return points.join(' ')
}

function getCurrentValue(channel: SimpleChannel): number {
  let value = 0
  const t = currentTime.value
  
  switch (channel.signalType) {
    case 'sine':
      value = Math.sin(2 * Math.PI * channel.frequency * t + channel.phase)
      break
    case 'square':
      value = Math.sign(Math.sin(2 * Math.PI * channel.frequency * t + channel.phase))
      break
    case 'triangle':
      const trianglePhase = (channel.frequency * t + channel.phase / (2 * Math.PI)) % 1
      value = trianglePhase < 0.5 ? 4 * trianglePhase - 1 : 3 - 4 * trianglePhase
      break
    case 'sawtooth':
      value = 2 * ((channel.frequency * t + channel.phase / (2 * Math.PI)) % 1) - 1
      break
    case 'noise':
      value = (Math.random() - 0.5) * 2
      break
  }
  
  return value * channel.amplitude
}

// 图表控制
function zoomIn() {
  ElMessage.info('缩放功能')
}

function zoomOut() {
  ElMessage.info('缩小功能')
}

function autoScale() {
  ElMessage.info('自动缩放功能')
}

// 工具函数
function formatTime(seconds: number): string {
  const mins = Math.floor(seconds / 60)
  const secs = (seconds % 60).toFixed(1)
  return `${mins}:${secs.padStart(4, '0')}`
}

// 切换模式
async function switchMode() {
  modeSwitching.value = true
  
  try {
    // 如果正在运行，先停止
    if (isRunning.value) {
      await stopAcquisition()
    }
    
    // 调用后端API切换模式
    const response = await axios.post('http://localhost:5001/api/DataAcquisition/set-mode', {
      useHardware: useHardware.value
    })
    
    if (response.data.success) {
      ElMessage.success(response.data.message)
    } else {
      // 切换失败，恢复原状态
      useHardware.value = !useHardware.value
      ElMessage.error('模式切换失败')
    }
  } catch (error) {
    console.error('模式切换失败:', error)
    // 切换失败，恢复原状态
    useHardware.value = !useHardware.value
    ElMessage.error('模式切换失败')
  } finally {
    modeSwitching.value = false
  }
}

// 检查当前模式
async function checkCurrentMode() {
  try {
    const response = await axios.get('http://localhost:5001/api/DataAcquisition/current-mode')
    if (response.data) {
      useHardware.value = response.data.isHardware
    }
  } catch (error) {
    console.error('获取当前模式失败:', error)
  }
}

// 修改生命周期
onMounted(() => {
  checkCurrentMode()
  ElMessage.success('USB-1601 数据采集卡已初始化')
})
</script>

<style scoped>
.usb1601-simulator-demo {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 16px;
  padding: 24px;
  color: white;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(10px);
}

.control-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 2px solid rgba(255, 255, 255, 0.2);
}

.title-section .control-title {
  display: flex;
  align-items: center;
  gap: 12px;
  margin: 0 0 8px 0;
  font-size: 24px;
  font-weight: 700;
  color: white;
}

.title-icon {
  font-size: 28px;
  color: #FFD700;
}

.control-subtitle {
  margin: 0;
  font-size: 14px;
  color: rgba(255, 255, 255, 0.8);
  font-weight: 400;
}

.header-right {
  display: flex;
  gap: 12px;
  align-items: center;
}

.simulator-layout {
  display: grid;
  grid-template-columns: 350px 1fr;
  gap: 24px;
  min-height: 600px;
}

.control-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.control-section {
  background: rgba(255, 255, 255, 0.95);
  border: none;
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
}

.section-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  color: #333;
}

.control-group {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.control-row {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
}

.action-button {
  flex: 1;
  min-width: 120px;
}

.control-label {
  font-size: 13px;
  font-weight: 600;
  color: #555;
  min-width: 80px;
}

.channel-config {
  max-height: 300px;
  overflow-y: auto;
}

.channel-item {
  border-bottom: 1px solid #eee;
  padding: 12px 0;
}

.channel-item:last-child {
  border-bottom: none;
}

.channel-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
}

.channel-color {
  width: 16px;
  height: 16px;
  border-radius: 3px;
}

.channel-name {
  font-weight: 600;
  color: #333;
  flex: 1;
}

.channel-controls {
  padding-left: 40px;
}

.signal-type {
  margin-bottom: 12px;
}

.signal-params {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.param-group label {
  font-size: 12px;
  color: #666;
  margin-bottom: 4px;
  display: block;
}

.statistics {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.stat-label {
  font-size: 13px;
  color: #666;
}

.stat-value {
  font-weight: 600;
  color: #333;
  font-family: 'Courier New', monospace;
}

.waveform-panel {
  display: flex;
  flex-direction: column;
}

.waveform-container {
  flex: 1;
  background: rgba(255, 255, 255, 0.95);
  border: none;
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
}

.waveform-controls {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-left: auto;
}

.chart-area {
  height: 500px;
  border-radius: 8px;
  overflow: hidden;
}

.status-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 24px;
  padding: 12px 16px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  backdrop-filter: blur(10px);
  font-size: 13px;
}

.status-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.status-icon {
  color: #4ade80;
}

.version-info {
  color: rgba(255, 255, 255, 0.7);
  font-weight: 500;
}

/* 深色主题适配 */
:deep(.el-card__header) {
  background: rgba(248, 249, 250, 0.8);
  border-bottom: 1px solid #e5e7eb;
}

:deep(.el-select),
:deep(.el-slider) {
  width: 100%;
}

:deep(.el-slider__runway) {
  background-color: #e5e7eb;
}

:deep(.el-slider__bar) {
  background-color: #667eea;
}

:deep(.el-slider__button) {
  border-color: #667eea;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .simulator-layout {
    grid-template-columns: 300px 1fr;
  }
}

@media (max-width: 992px) {
  .simulator-layout {
    grid-template-columns: 1fr;
    gap: 16px;
  }
  
  .control-panel {
    order: 2;
  }
  
  .waveform-panel {
    order: 1;
  }
}

@media (max-width: 768px) {
  .usb1601-simulator-demo {
    padding: 16px;
  }
  
  .control-header {
    flex-direction: column;
    gap: 16px;
    text-align: center;
  }
  
  .title-section .control-title {
    font-size: 20px;
  }
  
  .chart-area {
    height: 400px;
  }
  
  .status-bar {
    flex-direction: column;
    gap: 8px;
    text-align: center;
  }
}
</style>
