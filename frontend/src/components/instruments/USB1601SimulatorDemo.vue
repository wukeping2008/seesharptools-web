<template>
  <div class="usb1601-simulator professional-instrument">
    <!-- 主标题栏 -->
    <div class="instrument-header">
      <div class="header-content">
        <div class="title-section">
          <el-icon class="brand-icon"><Monitor /></el-icon>
          <div>
            <h2 class="instrument-title">USB-1601 数据采集系统</h2>
            <span class="instrument-subtitle">高精度多通道数据采集模拟器</span>
          </div>
        </div>
        
        <div class="status-section">
          <div class="status-item">
            <span class="status-label">模式</span>
            <el-tag :type="dataSource === 'simulator' ? 'success' : 'primary'">
              {{ dataSource === 'simulator' ? '模拟器' : '硬件' }}
            </el-tag>
          </div>
          <div class="status-item">
            <span class="status-label">状态</span>
            <el-tag :type="isAcquiring ? 'success' : 'info'">
              {{ isAcquiring ? '采集中' : '就绪' }}
            </el-tag>
          </div>
          <div class="status-item">
            <span class="status-label">采样率</span>
            <span class="status-value">{{ sampleRate.toLocaleString() }} Hz</span>
          </div>
        </div>
        
        <div class="control-section">
          <el-button-group>
            <el-button 
              :type="isAcquiring ? 'danger' : 'success'" 
              @click="toggleAcquisition"
              size="large"
            >
              <el-icon><VideoPlay v-if="!isAcquiring" /><VideoPause v-else /></el-icon>
              {{ isAcquiring ? '停止' : '开始' }}
            </el-button>
            <el-button @click="showSettings = true" size="large">
              <el-icon><Setting /></el-icon>
              配置
            </el-button>
          </el-button-group>
        </div>
      </div>
    </div>
    
    <!-- 主体内容 -->
    <div class="instrument-body">
      <el-row :gutter="16">
        <!-- 左侧：波形显示 -->
        <el-col :span="18">
          <div class="waveform-section">
            <EnhancedWaveformChart
              ref="waveformChart"
              :data="waveformData"
              :channels="activeChannels"
              :sample-rate="sampleRate"
              height="450px"
              :show-toolbar="true"
              :show-channel-panel="true"
              :show-measurements="true"
              @channel-toggle="onChannelToggle"
              @export="onExportData"
            />
          </div>
        </el-col>
        
        <!-- 右侧：控制面板 -->
        <el-col :span="6">
          <div class="control-panel">
            <!-- 数据源选择 -->
            <div class="panel-section">
              <h4 class="section-title">
                <el-icon><Connection /></el-icon>
                数据源
              </h4>
              <el-radio-group v-model="dataSource" @change="onDataSourceChange">
                <el-radio-button value="simulator">模拟器</el-radio-button>
                <el-radio-button value="hardware" :disabled="!hardwareAvailable">
                  硬件
                </el-radio-button>
              </el-radio-group>
            </div>
            
            <!-- 信号配置（模拟器模式） -->
            <div class="panel-section" v-if="dataSource === 'simulator'">
              <h4 class="section-title">
                <el-icon><DataLine /></el-icon>
                信号配置
              </h4>
              <div class="signal-presets">
                <el-select 
                  v-model="selectedPreset" 
                  @change="applyPreset"
                  placeholder="选择预设"
                  size="small"
                  style="width: 100%"
                >
                  <el-option label="基础测试" value="basic" />
                  <el-option label="心电图模拟" value="ecg" />
                  <el-option label="PWM信号" value="pwm" />
                  <el-option label="扫频信号" value="sweep" />
                  <el-option label="调制信号" value="modulated" />
                  <el-option label="混合信号" value="mixed" />
                  <el-option label="自定义" value="custom" />
                </el-select>
              </div>
              
              <!-- 通道配置 -->
              <div class="channel-configs">
                <div 
                  v-for="(channel, index) in channelConfigs" 
                  :key="channel.id"
                  class="channel-config-item"
                >
                  <div class="channel-header">
                    <el-checkbox v-model="channel.enabled">
                      {{ channel.name }}
                    </el-checkbox>
                    <el-button 
                      text 
                      size="small" 
                      @click="editChannel(index)"
                    >
                      <el-icon><Edit /></el-icon>
                    </el-button>
                  </div>
                  <div class="channel-info" v-if="channel.enabled">
                    <span>{{ getSignalTypeLabel(channel.signal.type) }}</span>
                    <span>{{ channel.signal.frequency }}Hz</span>
                    <span>±{{ channel.signal.amplitude }}V</span>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- 采集统计 -->
            <div class="panel-section">
              <h4 class="section-title">
                <el-icon><DataBoard /></el-icon>
                采集统计
              </h4>
              <div class="stats-grid">
                <div class="stat-item">
                  <span class="stat-label">采集时间</span>
                  <span class="stat-value">{{ formatDuration(acquisitionTime) }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">数据点数</span>
                  <span class="stat-value">{{ totalSamples.toLocaleString() }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">缓冲区</span>
                  <span class="stat-value">{{ bufferUsage }}%</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">CPU</span>
                  <span class="stat-value">{{ cpuUsage.toFixed(1) }}%</span>
                </div>
              </div>
            </div>
            
            <!-- 快捷操作 -->
            <div class="panel-section">
              <h4 class="section-title">
                <el-icon><Operation /></el-icon>
                快捷操作
              </h4>
              <div class="quick-actions">
                <el-button @click="saveConfiguration" size="small" style="width: 100%">
                  <el-icon><Document /></el-icon>
                  保存配置
                </el-button>
                <el-button @click="loadConfiguration" size="small" style="width: 100%">
                  <el-icon><Upload /></el-icon>
                  加载配置
                </el-button>
                <el-button @click="resetAll" size="small" style="width: 100%" type="warning">
                  <el-icon><Refresh /></el-icon>
                  重置全部
                </el-button>
              </div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>
    
    <!-- 配置对话框 -->
    <el-dialog
      v-model="showSettings"
      title="采集系统配置"
      width="70%"
      :close-on-click-modal="false"
    >
      <el-tabs v-model="activeSettingTab">
        <el-tab-pane label="基础设置" name="basic">
          <el-form :model="settings" label-width="120px">
            <el-form-item label="采样率">
              <el-select v-model="settings.sampleRate" style="width: 200px">
                <el-option label="100 Hz" :value="100" />
                <el-option label="1 kHz" :value="1000" />
                <el-option label="10 kHz" :value="10000" />
                <el-option label="100 kHz" :value="100000" />
                <el-option label="1 MHz" :value="1000000" />
              </el-select>
            </el-form-item>
            <el-form-item label="缓冲区大小">
              <el-input-number 
                v-model="settings.bufferSize" 
                :min="100" 
                :max="10000" 
                :step="100"
                style="width: 200px"
              />
            </el-form-item>
            <el-form-item label="更新间隔">
              <el-input-number 
                v-model="settings.updateInterval" 
                :min="10" 
                :max="1000" 
                :step="10"
                style="width: 200px"
              />
              <span style="margin-left: 8px">ms</span>
            </el-form-item>
          </el-form>
        </el-tab-pane>
        
        <el-tab-pane label="通道设置" name="channels" v-if="dataSource === 'simulator'">
          <ChannelEditor
            v-if="editingChannel !== null"
            :channel="channelConfigs[editingChannel]"
            @save="saveChannelConfig"
            @cancel="editingChannel = null"
          />
          <div v-else class="channel-list">
            <el-button @click="addChannel" type="primary" size="small">
              <el-icon><Plus /></el-icon>
              添加通道
            </el-button>
            <div class="channel-items">
              <div 
                v-for="(channel, index) in channelConfigs" 
                :key="channel.id"
                class="channel-item-config"
              >
                <span>{{ channel.name }}</span>
                <div>
                  <el-button text size="small" @click="editChannel(index)">
                    编辑
                  </el-button>
                  <el-button text size="small" type="danger" @click="removeChannel(index)">
                    删除
                  </el-button>
                </div>
              </div>
            </div>
          </div>
        </el-tab-pane>
      </el-tabs>
      
      <template #footer>
        <el-button @click="showSettings = false">取消</el-button>
        <el-button type="primary" @click="applySettings">应用</el-button>
      </template>
    </el-dialog>
    
    <!-- 通道编辑器对话框 -->
    <el-dialog
      v-model="showChannelEditor"
      :title="`编辑通道: ${editingChannelData?.name || ''}`"
      width="500px"
    >
      <el-form :model="editingChannelData" label-width="100px" v-if="editingChannelData">
        <el-form-item label="通道名称">
          <el-input v-model="editingChannelData.name" />
        </el-form-item>
        <el-form-item label="信号类型">
          <el-select v-model="editingChannelData.signal.type" style="width: 100%">
            <el-option label="正弦波" value="sine" />
            <el-option label="方波" value="square" />
            <el-option label="三角波" value="triangle" />
            <el-option label="锯齿波" value="sawtooth" />
            <el-option label="脉冲" value="pulse" />
            <el-option label="噪声" value="noise" />
            <el-option label="直流" value="dc" />
            <el-option label="扫频" value="chirp" />
            <el-option label="调制" value="modulated" />
          </el-select>
        </el-form-item>
        <el-form-item label="频率 (Hz)">
          <el-input-number 
            v-model="editingChannelData.signal.frequency" 
            :min="0.1" 
            :max="100000"
            :step="1"
            style="width: 100%"
          />
        </el-form-item>
        <el-form-item label="幅度 (V)">
          <el-slider 
            v-model="editingChannelData.signal.amplitude" 
            :min="0" 
            :max="10"
            :step="0.1"
            show-input
          />
        </el-form-item>
        <el-form-item label="偏移 (V)">
          <el-slider 
            v-model="editingChannelData.signal.offset" 
            :min="-10" 
            :max="10"
            :step="0.1"
            show-input
          />
        </el-form-item>
        <el-form-item label="噪声水平">
          <el-slider 
            v-model="editingChannelData.signal.noiseLevel" 
            :min="0" 
            :max="1"
            :step="0.01"
            show-input
          />
        </el-form-item>
      </el-form>
      
      <template #footer>
        <el-button @click="showChannelEditor = false">取消</el-button>
        <el-button type="primary" @click="saveEditingChannel">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { 
  Monitor,
  VideoPlay,
  VideoPause,
  Setting,
  Connection,
  DataLine,
  DataBoard,
  Operation,
  Document,
  Upload,
  Refresh,
  Edit,
  Plus
} from '@element-plus/icons-vue'
import EnhancedWaveformChart from '@/components/charts/EnhancedWaveformChart.vue'
import { 
  SignalGenerator, 
  SignalType, 
  SignalPresets,
  createDefaultChannels,
  type ChannelConfig,
  type SignalConfig
} from '@/utils/signalGenerator'

// 状态管理
const isAcquiring = ref(false)
const dataSource = ref<'simulator' | 'hardware'>('simulator')
const hardwareAvailable = ref(false)
const showSettings = ref(false)
const showChannelEditor = ref(false)
const activeSettingTab = ref('basic')
const selectedPreset = ref('basic')
const editingChannel = ref<number | null>(null)
const editingChannelData = ref<ChannelConfig | null>(null)

// 性能指标
const sampleRate = ref(10000)
const acquisitionTime = ref(0)
const totalSamples = ref(0)
const bufferUsage = ref(0)
const cpuUsage = ref(2.5)

// 波形数据
const waveformData = ref<any[]>([])
const waveformChart = ref()

// 信号生成器
let signalGenerator: SignalGenerator | null = null
let acquisitionTimer: number | null = null
let startTime = 0

// 配置
const settings = reactive({
  sampleRate: 10000,
  bufferSize: 1000,
  updateInterval: 50
})

// 通道配置
const channelConfigs = ref<ChannelConfig[]>(createDefaultChannels())

// 计算属性
const activeChannels = computed(() => 
  channelConfigs.value.filter(ch => ch.enabled)
)

// 生命周期
onMounted(() => {
  initializeSimulator()
  checkHardwareAvailability()
})

onUnmounted(() => {
  if (isAcquiring.value) {
    stopAcquisition()
  }
})

// 初始化模拟器
function initializeSimulator() {
  signalGenerator = new SignalGenerator(settings.sampleRate, settings.bufferSize)
  
  // 添加默认通道
  channelConfigs.value.forEach(config => {
    signalGenerator!.addChannel(config)
  })
}

// 检查硬件可用性
async function checkHardwareAvailability() {
  // 这里可以调用后端API检查硬件
  // 暂时设置为false
  hardwareAvailable.value = false
}

// 开始/停止采集
function toggleAcquisition() {
  if (isAcquiring.value) {
    stopAcquisition()
  } else {
    startAcquisition()
  }
}

// 开始采集
function startAcquisition() {
  if (!signalGenerator) {
    initializeSimulator()
  }
  
  isAcquiring.value = true
  startTime = Date.now()
  totalSamples.value = 0
  waveformData.value = []
  
  // 更新通道配置
  channelConfigs.value.forEach(config => {
    signalGenerator!.updateChannel(config.id, config)
  })
  
  // 开始数据生成循环
  if (acquisitionTimer) {
    clearInterval(acquisitionTimer)
  }
  
  acquisitionTimer = setInterval(() => {
    generateData()
    updateStats()
  }, settings.updateInterval)
  
  ElMessage.success('数据采集已开始')
}

// 停止采集
function stopAcquisition() {
  isAcquiring.value = false
  
  if (acquisitionTimer) {
    clearInterval(acquisitionTimer)
    acquisitionTimer = null
  }
  
  ElMessage.info('数据采集已停止')
}

// 生成数据
function generateData() {
  if (!signalGenerator || !isAcquiring.value) return
  
  // 根据更新间隔计算需要生成的点数
  const pointsToGenerate = Math.floor(settings.sampleRate * settings.updateInterval / 1000)
  
  // 生成数据点
  for (let i = 0; i < pointsToGenerate; i++) {
    const data = signalGenerator.generateRealtimeData()
    waveformData.value.push(data)
    totalSamples.value++
  }
  
  // 限制缓冲区大小
  if (waveformData.value.length > settings.bufferSize) {
    waveformData.value = waveformData.value.slice(-settings.bufferSize)
  }
  
  // 更新缓冲区使用率
  bufferUsage.value = Math.floor((waveformData.value.length / settings.bufferSize) * 100)
}

// 更新统计信息
function updateStats() {
  if (!isAcquiring.value) return
  
  acquisitionTime.value = Date.now() - startTime
  
  // 模拟CPU使用率
  cpuUsage.value = 2 + Math.random() * 3 + activeChannels.value.length * 0.5
}

// 数据源切换
function onDataSourceChange(value: string) {
  if (isAcquiring.value) {
    ElMessage.warning('请先停止采集')
    dataSource.value = dataSource.value === 'simulator' ? 'hardware' : 'simulator'
    return
  }
  
  if (value === 'hardware' && !hardwareAvailable.value) {
    ElMessage.error('硬件设备不可用')
    dataSource.value = 'simulator'
    return
  }
  
  ElMessage.success(`已切换到${value === 'simulator' ? '模拟器' : '硬件'}模式`)
}

// 应用预设
function applyPreset(preset: string) {
  switch (preset) {
    case 'basic':
      channelConfigs.value = createDefaultChannels()
      break
    case 'ecg':
      channelConfigs.value = [
        {
          id: 'AI0',
          name: 'ECG Lead I',
          enabled: true,
          signal: { ...SignalPresets.ecg },
          color: '#ff0000',
          unit: 'mV'
        },
        {
          id: 'AI1',
          name: 'ECG Lead II',
          enabled: true,
          signal: { ...SignalPresets.ecg, phase: Math.PI / 4 },
          color: '#00ff00',
          unit: 'mV'
        }
      ]
      break
    case 'pwm':
      channelConfigs.value = [
        {
          id: 'AI0',
          name: 'PWM 30%',
          enabled: true,
          signal: { ...SignalPresets.pwm, dutyCycle: 0.3 },
          color: '#1f77b4',
          unit: 'V'
        },
        {
          id: 'AI1',
          name: 'PWM 70%',
          enabled: true,
          signal: { ...SignalPresets.pwm, dutyCycle: 0.7 },
          color: '#ff7f0e',
          unit: 'V'
        }
      ]
      break
    case 'mixed':
      channelConfigs.value = [
        {
          id: 'AI0',
          name: '正弦波',
          enabled: true,
          signal: SignalPresets.testSine,
          color: '#1f77b4',
          unit: 'V'
        },
        {
          id: 'AI1',
          name: '方波',
          enabled: true,
          signal: { type: SignalType.SQUARE, frequency: 20, amplitude: 3, offset: 0, phase: 0 },
          color: '#ff7f0e',
          unit: 'V'
        },
        {
          id: 'AI2',
          name: '噪声',
          enabled: true,
          signal: SignalPresets.whiteNoise,
          color: '#2ca02c',
          unit: 'V'
        }
      ]
      break
  }
  
  // 重新初始化信号生成器
  if (signalGenerator) {
    signalGenerator.reset()
    channelConfigs.value.forEach(config => {
      signalGenerator!.removeChannel(config.id)
      signalGenerator!.addChannel(config)
    })
  }
  
  ElMessage.success(`已应用预设: ${preset}`)
}

// 通道操作
function onChannelToggle(channelId: string) {
  const channel = channelConfigs.value.find(ch => ch.id === channelId)
  if (channel && signalGenerator) {
    signalGenerator.updateChannel(channelId, channel)
  }
}

function editChannel(index: number) {
  editingChannelData.value = { ...channelConfigs.value[index] }
  showChannelEditor.value = true
  editingChannel.value = index
}

function saveEditingChannel() {
  if (editingChannel.value !== null && editingChannelData.value) {
    channelConfigs.value[editingChannel.value] = { ...editingChannelData.value }
    
    if (signalGenerator) {
      signalGenerator.updateChannel(
        editingChannelData.value.id,
        editingChannelData.value
      )
    }
    
    showChannelEditor.value = false
    editingChannel.value = null
    editingChannelData.value = null
    
    ElMessage.success('通道配置已更新')
  }
}

function addChannel() {
  const newChannel: ChannelConfig = {
    id: `AI${channelConfigs.value.length}`,
    name: `通道 ${channelConfigs.value.length}`,
    enabled: false,
    signal: { ...SignalPresets.testSine },
    color: `#${Math.floor(Math.random()*16777215).toString(16)}`,
    unit: 'V'
  }
  
  channelConfigs.value.push(newChannel)
  
  if (signalGenerator) {
    signalGenerator.addChannel(newChannel)
  }
  
  ElMessage.success('通道已添加')
}

function removeChannel(index: number) {
  const channel = channelConfigs.value[index]
  
  ElMessageBox.confirm(
    `确定要删除通道 "${channel.name}" 吗？`,
    '确认删除',
    {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    }
  ).then(() => {
    channelConfigs.value.splice(index, 1)
    
    if (signalGenerator) {
      signalGenerator.removeChannel(channel.id)
    }
    
    ElMessage.success('通道已删除')
  })
}

function saveChannelConfig(config: ChannelConfig) {
  if (editingChannel.value !== null) {
    channelConfigs.value[editingChannel.value] = config
    editingChannel.value = null
    
    if (signalGenerator) {
      signalGenerator.updateChannel(config.id, config)
    }
    
    ElMessage.success('通道配置已保存')
  }
}

// 配置操作
function applySettings() {
  sampleRate.value = settings.sampleRate
  
  if (signalGenerator) {
    signalGenerator.setSampleRate(settings.sampleRate)
  }
  
  showSettings.value = false
  ElMessage.success('配置已应用')
}

function saveConfiguration() {
  const config = {
    settings,
    channels: channelConfigs.value
  }
  
  const blob = new Blob([JSON.stringify(config, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `usb1601_config_${Date.now()}.json`
  a.click()
  URL.revokeObjectURL(url)
  
  ElMessage.success('配置已保存')
}

function loadConfiguration() {
  const input = document.createElement('input')
  input.type = 'file'
  input.accept = '.json'
  input.onchange = (e: any) => {
    const file = e.target.files[0]
    if (file) {
      const reader = new FileReader()
      reader.onload = (event: any) => {
        try {
          const config = JSON.parse(event.target.result)
          Object.assign(settings, config.settings)
          channelConfigs.value = config.channels
          
          // 重新初始化
          initializeSimulator()
          
          ElMessage.success('配置已加载')
        } catch (error) {
          ElMessage.error('配置文件格式错误')
        }
      }
      reader.readAsText(file)
    }
  }
  input.click()
}

function resetAll() {
  ElMessageBox.confirm(
    '确定要重置所有配置吗？',
    '确认重置',
    {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    }
  ).then(() => {
    // 停止采集
    if (isAcquiring.value) {
      stopAcquisition()
    }
    
    // 重置配置
    settings.sampleRate = 10000
    settings.bufferSize = 1000
    settings.updateInterval = 50
    
    // 重置通道
    channelConfigs.value = createDefaultChannels()
    
    // 重新初始化
    initializeSimulator()
    
    // 清空数据
    waveformData.value = []
    totalSamples.value = 0
    acquisitionTime.value = 0
    bufferUsage.value = 0
    
    ElMessage.success('已重置为默认配置')
  })
}

// 数据导出
function onExportData(data: any) {
  console.log('导出数据:', data)
  ElMessage.success('数据已导出')
}

// 工具函数
function formatDuration(ms: number): string {
  const seconds = Math.floor(ms / 1000)
  const minutes = Math.floor(seconds / 60)
  const hours = Math.floor(minutes / 60)
  
  if (hours > 0) {
    return `${hours}:${String(minutes % 60).padStart(2, '0')}:${String(seconds % 60).padStart(2, '0')}`
  } else {
    return `${minutes}:${String(seconds % 60).padStart(2, '0')}`
  }
}

function getSignalTypeLabel(type: string): string {
  const labels: Record<string, string> = {
    sine: '正弦波',
    square: '方波',
    triangle: '三角波',
    sawtooth: '锯齿波',
    pulse: '脉冲',
    noise: '噪声',
    dc: '直流',
    chirp: '扫频',
    modulated: '调制'
  }
  return labels[type] || type
}
</script>

<style scoped>
.usb1601-simulator {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
}

.instrument-header {
  background: rgba(255, 255, 255, 0.98);
  backdrop-filter: blur(10px);
  border-bottom: 2px solid rgba(102, 126, 234, 0.2);
}

.header-content {
  padding: 20px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 24px;
}

.title-section {
  display: flex;
  align-items: center;
  gap: 16px;
}

.brand-icon {
  font-size: 48px;
  color: #667eea;
}

.instrument-title {
  margin: 0;
  font-size: 24px;
  font-weight: 700;
  color: #1a1a1a;
}

.instrument-subtitle {
  font-size: 14px;
  color: #666;
  margin-top: 4px;
}

.status-section {
  display: flex;
  gap: 24px;
  align-items: center;
}

.status-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.status-label {
  font-size: 12px;
  color: #999;
}

.status-value {
  font-size: 16px;
  font-weight: 600;
  color: #333;
}

.instrument-body {
  padding: 24px;
  background: rgba(255, 255, 255, 0.95);
  min-height: 600px;
}

.waveform-section {
  background: #1a1a1a;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.control-panel {
  background: white;
  border-radius: 8px;
  padding: 16px;
  height: 100%;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.panel-section {
  margin-bottom: 24px;
}

.panel-section:last-child {
  margin-bottom: 0;
}

.section-title {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0 0 12px 0;
  font-size: 14px;
  font-weight: 600;
  color: #333;
  padding-bottom: 8px;
  border-bottom: 1px solid #eee;
}

.signal-presets {
  margin-bottom: 12px;
}

.channel-configs {
  space-y: 8px;
}

.channel-config-item {
  padding: 8px;
  background: #f5f7fa;
  border-radius: 4px;
  margin-bottom: 8px;
}

.channel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.channel-info {
  display: flex;
  gap: 12px;
  margin-top: 4px;
  margin-left: 24px;
  font-size: 12px;
  color: #666;
}

.stats-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-label {
  font-size: 12px;
  color: #999;
}

.stat-value {
  font-size: 16px;
  font-weight: 600;
  color: #333;
  font-family: 'Courier New', monospace;
}

.quick-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.channel-list {
  padding: 16px;
}

.channel-items {
  margin-top: 16px;
}

.channel-item-config {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px;
  background: #f5f7fa;
  border-radius: 4px;
  margin-bottom: 8px;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .header-content {
    flex-direction: column;
    gap: 16px;
  }
  
  .status-section {
    width: 100%;
    justify-content: center;
  }
}

@media (max-width: 768px) {
  .instrument-body .el-col {
    margin-bottom: 16px;
  }
}
</style>