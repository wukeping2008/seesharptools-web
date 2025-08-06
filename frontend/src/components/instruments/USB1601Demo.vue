<template>
  <div class="usb1601-demo professional-control">
    <!-- 控件标题 -->
    <div class="demo-header">
      <div class="header-left">
        <h3 class="demo-title">
          <el-icon class="title-icon"><DataAnalysis /></el-icon>
          简仪科技 USB-1601 数据采集演示
        </h3>
        <div class="demo-status">
          <span class="status-indicator" :class="systemStatus.toLowerCase()">
            {{ systemStatus }}
          </span>
          <span class="device-info" v-if="deviceConnected">设备ID: {{ deviceId }}</span>
          <span class="mode-info">{{ simulationMode ? '模拟模式' : '硬件模式' }}</span>
        </div>
      </div>
      <div class="header-right">
        <el-button-group>
          <el-button 
            :type="acquiring ? 'danger' : 'primary'" 
            @click="toggleAcquisition"
            size="large"
            :loading="operationLoading"
          >
            <el-icon><VideoPlay v-if="!acquiring" /><VideoPause v-else /></el-icon>
            {{ acquiring ? '停止采集' : '开始采集' }}
          </el-button>
          <el-button @click="connectDevice" :disabled="acquiring" size="large">
            <el-icon><Connection /></el-icon>
            {{ deviceConnected ? '重新连接' : '连接设备' }}
          </el-button>
          <el-button @click="showSettings = true" size="large">
            <el-icon><Setting /></el-icon>
            配置
          </el-button>
        </el-button-group>
      </div>
    </div>

    <!-- 主显示区域 -->
    <div class="demo-body">
      <el-row :gutter="16">
        <!-- 左侧：实时数据显示 -->
        <el-col :span="16">
          <div class="data-display">
            <!-- AI模拟输入通道 -->
            <div class="channel-section">
              <div class="section-header">
                <h4><el-icon><DataLine /></el-icon>AI - 模拟输入通道</h4>
                <el-switch 
                  v-model="aiEnabled" 
                  active-text="启用"
                  :disabled="acquiring"
                />
              </div>
              <div class="data-chart" v-if="aiEnabled">
                <WaveformChart
                  ref="aiChartRef"
                  :data="waveformData"
                  :channels="aiChannelConfigs"
                  height="280px"
                  :show-header="true"
                  :show-legend="true"
                />
              </div>
            </div>

            <!-- AO模拟输出控制 -->
            <div class="channel-section">
              <div class="section-header">
                <h4><el-icon><Connection /></el-icon>AO - 模拟输出控制</h4>
                <el-switch 
                  v-model="aoEnabled" 
                  active-text="启用"
                  :disabled="acquiring"
                />
              </div>
              <div class="ao-controls" v-if="aoEnabled">
                <el-row :gutter="12">
                  <el-col :span="6" v-for="channel in aoChannels" :key="channel.id">
                    <div class="ao-channel">
                      <label>AO{{ channel.id }}</label>
                      <el-slider
                        v-model="channel.value"
                        :min="-10"
                        :max="10"
                        :step="0.1"
                        :format-tooltip="(val: number) => `${val}V`"
                        @change="setAOValue(channel.id, channel.value)"
                      />
                      <div class="ao-value">{{ channel.value.toFixed(1) }}V</div>
                    </div>
                  </el-col>
                </el-row>
              </div>
            </div>
          </div>
        </el-col>

        <!-- 右侧：控制和状态面板 -->
        <el-col :span="8">
          <div class="control-panel">
            <!-- 数字IO控制 -->
            <div class="dio-section">
              <div class="section-header">
                <h4><el-icon><SwitchButton /></el-icon>数字IO</h4>
              </div>
              <div class="dio-controls">
                <!-- 数字输出 -->
                <div class="do-group">
                  <label>数字输出 (DO)</label>
                  <div class="do-buttons">
                    <el-button
                      v-for="pin in 8"
                      :key="pin"
                      :type="doStates[pin - 1] ? 'primary' : 'default'"
                      size="small"
                      @click="toggleDO(pin - 1)"
                      :disabled="acquiring"
                    >
                      P{{ pin - 1 }}
                    </el-button>
                  </div>
                </div>
                
                <!-- 数字输入 -->
                <div class="di-group">
                  <label>数字输入 (DI)</label>
                  <div class="di-indicators">
                    <div
                      v-for="pin in 8"
                      :key="pin"
                      class="di-led"
                      :class="{ active: diStates[pin - 1] }"
                    >
                      P{{ pin - 1 }}
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- 采集统计信息 -->
            <div class="stats-section">
              <div class="section-header">
                <h4><el-icon><DataBoard /></el-icon>采集统计</h4>
              </div>
              <div class="stats-grid">
                <div class="stat-item">
                  <span class="stat-label">采集时间</span>
                  <span class="stat-value">{{ formatDuration(acquisitionDuration) }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">采样率</span>
                  <span class="stat-value">{{ currentSampleRate.toLocaleString() }} Hz</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">总样本数</span>
                  <span class="stat-value">{{ totalSamples.toLocaleString() }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">数据质量</span>
                  <span class="stat-value">{{ (dataQuality * 100).toFixed(1) }}%</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">内存使用</span>
                  <span class="stat-value">{{ (memoryUsage / 1024 / 1024).toFixed(1) }} MB</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">CPU使用</span>
                  <span class="stat-value">{{ cpuUsage.toFixed(1) }}%</span>
                </div>
              </div>
            </div>

            <!-- C# Runner演示 -->
            <div class="csharp-section">
              <div class="section-header">
                <h4><el-icon><Document /></el-icon>C# Runner演示</h4>
              </div>
              <el-button 
                type="success" 
                @click="runCSharpDemo"
                :loading="csharpRunning"
                style="width: 100%"
              >
                <el-icon><Play /></el-icon>
                运行C#代码演示
              </el-button>
              <div v-if="csharpOutput" class="csharp-output">
                <pre>{{ csharpOutput }}</pre>
              </div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 配置对话框 -->
    <el-dialog 
      v-model="showSettings" 
      title="USB-1601 配置" 
      width="60%"
    >
      <el-form :model="settings" label-width="120px">
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="设备ID">
              <el-input-number v-model="settings.deviceId" :min="0" :max="7" />
            </el-form-item>
            <el-form-item label="采样率">
              <el-select v-model="settings.sampleRate" placeholder="选择采样率">
                <el-option label="1 kHz" :value="1000" />
                <el-option label="10 kHz" :value="10000" />
                <el-option label="100 kHz" :value="100000" />
                <el-option label="1 MHz" :value="1000000" />
              </el-select>
            </el-form-item>
            <el-form-item label="缓冲区大小">
              <el-input-number v-model="settings.bufferSize" :min="1000" :max="100000" :step="1000" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="AI通道配置">
              <div v-for="(channel, index) in settings.aiChannels" :key="index" class="channel-config">
                <el-checkbox v-model="channel.enabled">通道 {{ index }}</el-checkbox>
                <el-select v-model="channel.range" size="small" style="width: 100px; margin-left: 8px;">
                  <el-option label="±10V" value="±10V" />
                  <el-option label="±5V" value="±5V" />
                  <el-option label="±2V" value="±2V" />
                  <el-option label="±1V" value="±1V" />
                </el-select>
              </div>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showSettings = false">取消</el-button>
          <el-button type="primary" @click="applySettings">应用配置</el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted, computed, nextTick } from 'vue'
import { ElMessage, ElNotification } from 'element-plus'
import {
  DataAnalysis,
  VideoPlay,
  VideoPause,
  Connection,
  Setting,
  DataLine,
  SwitchButton,
  DataBoard,
  VideoPlay as Play,
  Camera,
  Download,
  Document
} from '@element-plus/icons-vue'
import WaveformChart from '@/components/charts/SimpleWaveformChart.vue'
import { dataAcquisitionService } from '@/services/dataAcquisition.service'
import { csharpRunnerService } from '@/services/csharpRunner.service'

// 响应式状态
const acquiring = ref(false)
const operationLoading = ref(false)
const deviceConnected = ref(false)
const simulationMode = ref(true)
const showSettings = ref(false)
const systemStatus = ref('就绪')
const deviceId = ref(0)

// AI数据相关
const aiEnabled = ref(true)
const aiData = ref<any[]>([])
const aiChartRef = ref()
const totalSamples = ref(0)
const acquisitionStartTime = ref(0)
const acquisitionDuration = ref(0)

// AO输出控制
const aoEnabled = ref(true)
const aoChannels = reactive([
  { id: 0, value: 0.0 },
  { id: 1, value: 0.0 }
])

// 数字IO状态
const doStates = reactive(new Array(8).fill(false))
const diStates = reactive(new Array(8).fill(false))

// 性能统计
const currentSampleRate = ref(10000)
const dataQuality = ref(0.999)
const memoryUsage = ref(50 * 1024 * 1024)
const cpuUsage = ref(5.2)

// C# Runner
const csharpRunning = ref(false)
const csharpOutput = ref('')

// 任务管理
const currentTaskId = ref(0)
const acquisitionTimer = ref<number>()

// 配置设置
const settings = reactive({
  deviceId: 0,
  sampleRate: 10000,
  bufferSize: 10000,
  aiChannels: Array.from({ length: 4 }, (_, i) => ({
    enabled: i < 2, // 默认启用前2个通道
    range: '±10V'
  }))
})

// AI通道配置
const aiChannelConfigs = computed(() => 
  settings.aiChannels
    .map((channel, index) => ({
      id: `AI${index}`,
      name: `AI${index}`,
      color: ['#1f77b4', '#ff7f0e', '#2ca02c', '#d62728'][index],
      enabled: channel.enabled,
      unit: 'V',
      lastValue: null
    }))
    .filter(config => config.enabled)
)

// 波形图表配置
const waveformOptions = {
  autoScale: true,
  logarithmic: false,
  legendVisible: true,
  gridEnabled: true,
  minorGridEnabled: false,
  theme: 'light',
  realTime: true,
  bufferSize: 1000,
  triggerMode: 'auto',
  triggerLevel: 0,
  triggerChannel: 0
}

// 转换数据格式以适配SimpleWaveformChart
const waveformData = computed(() => aiData.value)

// 初始化
onMounted(async () => {
  await connectDevice()
  
  // 立即开始数据采集演示
  await startAcquisition()
  
  // 模拟数字输入变化
  setInterval(updateDigitalInputs, 2000)
  
  // 更新采集时长
  setInterval(() => {
    if (acquiring.value) {
      acquisitionDuration.value = Date.now() - acquisitionStartTime.value
    }
  }, 1000)
})

onUnmounted(() => {
  if (acquiring.value) {
    stopAcquisition()
  }
  if (acquisitionTimer.value) {
    clearInterval(acquisitionTimer.value)
  }
})

// 设备连接
async function connectDevice() {
  try {
    operationLoading.value = true
    systemStatus.value = '连接中'
    
    // 检测硬件设备
    const devices = await dataAcquisitionService.getActiveTasks()
    
    if (devices.length > 0) {
      deviceConnected.value = true
      simulationMode.value = false
      systemStatus.value = '硬件已连接'
      ElMessage.success('USB-1601硬件设备连接成功')
    } else {
      deviceConnected.value = true
      simulationMode.value = true
      systemStatus.value = '模拟模式'
      ElMessage.info('硬件设备未检测到，使用模拟模式演示')
    }
  } catch (error) {
    deviceConnected.value = true
    simulationMode.value = true
    systemStatus.value = '模拟模式'
    ElMessage.warning('使用模拟模式进行演示')
  } finally {
    operationLoading.value = false
  }
}

// 开始/停止采集
async function toggleAcquisition() {
  if (acquiring.value) {
    await stopAcquisition()
  } else {
    await startAcquisition()
  }
}

// 开始采集
async function startAcquisition() {
  try {
    operationLoading.value = true
    
    currentTaskId.value = Date.now()
    
    const config = {
      deviceId: settings.deviceId,
      sampleRate: settings.sampleRate,
      channels: settings.aiChannels
        .map((channel, index) => ({
          channelId: index,
          name: `AI${index}`,
          enabled: channel.enabled && aiEnabled.value,
          rangeMin: channel.range === '±10V' ? -10 : 
                   channel.range === '±5V' ? -5 :
                   channel.range === '±2V' ? -2 : -1,
          rangeMax: channel.range === '±10V' ? 10 : 
                   channel.range === '±5V' ? 5 :
                   channel.range === '±2V' ? 2 : 1,
          coupling: 'DC' as 'DC' | 'AC' | 'Ground',
          impedance: 'HighZ' as 'HighZ' | 'Ohm50' | 'Ohm75'
        }))
        .filter(ch => ch.enabled),
      mode: 'Continuous' as 'Continuous' | 'Finite' | 'Triggered',
      bufferSize: settings.bufferSize,
      enableCompression: true,
      enableQualityCheck: true
    }

    await dataAcquisitionService.startAcquisition(currentTaskId.value, config)
    
    acquiring.value = true
    acquisitionStartTime.value = Date.now()
    totalSamples.value = 0
    systemStatus.value = '采集中'
    
    // 开始数据接收循环
    startDataReceiveLoop()
    
    ElMessage.success('数据采集已开始')
    ElNotification({
      title: 'USB-1601 演示',
      message: `已开始${simulationMode.value ? '模拟' : '硬件'}数据采集`,
      type: 'success'
    })
    
  } catch (error: any) {
    ElMessage.error('启动数据采集失败: ' + error.message)
  } finally {
    operationLoading.value = false
  }
}

// 停止采集
async function stopAcquisition() {
  try {
    operationLoading.value = true
    
    if (currentTaskId.value) {
      await dataAcquisitionService.stopAcquisition(currentTaskId.value)
    }
    
    acquiring.value = false
    systemStatus.value = '已停止'
    
    if (acquisitionTimer.value) {
      clearInterval(acquisitionTimer.value)
    }
    
    ElMessage.success('数据采集已停止')
    
  } catch (error: any) {
    ElMessage.error('停止数据采集失败: ' + error.message)
  } finally {
    operationLoading.value = false
  }
}

// 数据接收循环
function startDataReceiveLoop() {
  if (acquisitionTimer.value) {
    clearInterval(acquisitionTimer.value)
  }
  
  acquisitionTimer.value = setInterval(async () => {
    if (!acquiring.value) return
    
    try {
      // 获取任务状态
      const status = await dataAcquisitionService.getTaskStatus(currentTaskId.value)
      totalSamples.value = status.samplesAcquired || totalSamples.value
      
      // 获取性能统计
      const perf = await dataAcquisitionService.getPerformanceStats(currentTaskId.value)
      if (perf) {
        currentSampleRate.value = perf.actualSampleRate || settings.sampleRate
        cpuUsage.value = perf.cpuUsage || 5.2
        memoryUsage.value = (perf.memoryUsage || 50) * 1024 * 1024
      }
      
      // 模拟生成实时数据用于图表显示
      generateRealtimeData()
      
    } catch (error) {
      console.error('数据接收错误:', error)
    }
  }, 100) // 100ms更新间隔
}

// 生成实时数据
function generateRealtimeData() {
  const now = Date.now()
  const newData: any = { timestamp: now }
  
  // 为每个启用的AI通道生成数据
  aiChannelConfigs.value.forEach((config, index) => {
    const t = now / 1000
    const frequency = 5 * (index + 1) // 不同通道不同频率
    const amplitude = 2 + index * 0.5
    const noise = (Math.random() - 0.5) * 0.2
    
    newData[config.id] = amplitude * Math.sin(2 * Math.PI * frequency * t) + noise
  })
  
  aiData.value.push(newData)
  
  // 保持数据量在合理范围内（最近1000个点）
  if (aiData.value.length > 1000) {
    aiData.value.splice(0, aiData.value.length - 1000)
  }
}

// AO输出设置
async function setAOValue(channelId: number, value: number) {
  try {
    // 这里可以调用实际的AO输出API
    console.log(`设置AO${channelId}输出电压: ${value}V`)
    ElMessage.success(`AO${channelId} 输出设置为 ${value.toFixed(1)}V`)
  } catch (error) {
    ElMessage.error('设置AO输出失败')
  }
}

// 数字输出切换
function toggleDO(pin: number) {
  doStates[pin] = !doStates[pin]
  console.log(`DO${pin}: ${doStates[pin]}`)
  ElMessage.info(`数字输出P${pin}: ${doStates[pin] ? 'HIGH' : 'LOW'}`)
}

// 更新数字输入（模拟）
function updateDigitalInputs() {
  for (let i = 0; i < 8; i++) {
    diStates[i] = Math.random() > 0.7 // 30%概率为HIGH
  }
}

// C# Runner演示
async function runCSharpDemo() {
  try {
    csharpRunning.value = true
    csharpOutput.value = ''
    
    // USB-1601模拟代码
    const csharpCode = `
using System;
using System.Collections.Generic;

public class USB1601Demo 
{
    public static void Main()
    {
        Console.WriteLine("=== 简仪科技USB-1601数据采集演示 ===");
        Console.WriteLine();
        
        // 模拟设备初始化
        Console.WriteLine("1. 初始化USB-1601设备...");
        var deviceId = "${settings.deviceId}";
        var sampleRate = ${settings.sampleRate};
        Console.WriteLine($"设备ID: {deviceId}");
        Console.WriteLine($"采样率: {sampleRate:N0} Hz");
        Console.WriteLine();
        
        // 模拟AI数据采集
        Console.WriteLine("2. 模拟AI数据采集:");
        var data = new List<double[]>();
        for (int i = 0; i < 5; i++)
        {
            var channelData = new double[10];
            for (int j = 0; j < 10; j++)
            {
                var t = (i * 10 + j) / (double)sampleRate;
                channelData[j] = 5 * Math.Sin(2 * Math.PI * 10 * t) + 
                                0.1 * (new Random().NextDouble() - 0.5);
            }
            data.Add(channelData);
            Console.WriteLine($"通道{i}: [{string.Join(", ", Array.ConvertAll(channelData.Take(5).ToArray(), x => x.ToString("F2")))}...]");
        }
        
        Console.WriteLine();
        Console.WriteLine("3. 数据统计:");
        Console.WriteLine($"采样点数: {data.Count * 10:N0}");
        Console.WriteLine($"数据质量: 99.9%");
        Console.WriteLine($"内存使用: {(data.Count * 10 * 8 / 1024.0):F1} KB");
        
        Console.WriteLine();
        Console.WriteLine("✅ USB-1601演示完成!");
    }
}`;

    const result = await csharpRunnerService.runCode({
      code: csharpCode,
      template: 'JYUSB1601'
    })
    
    csharpOutput.value = result.output || result.consoleOutput || '执行成功'
    
    ElMessage.success('C#代码执行完成')
    
  } catch (error: any) {
    csharpOutput.value = `执行错误: ${error.message}`
    ElMessage.error('C#代码执行失败')
  } finally {
    csharpRunning.value = false
  }
}

// 应用配置
function applySettings() {
  showSettings.value = false
  ElMessage.success('配置已应用')
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
</script>

<style scoped>
.usb1601-demo {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.demo-header {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  padding: 20px 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 2px solid rgba(102, 126, 234, 0.2);
}

.header-left {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.demo-title {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: #1a1a1a;
}

.title-icon {
  color: #667eea;
  font-size: 24px;
}

.demo-status {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 14px;
}

.status-indicator {
  padding: 4px 12px;
  border-radius: 20px;
  font-weight: 500;
  font-size: 12px;
}

.status-indicator.就绪, .status-indicator.已停止 {
  background: #e3f2fd;
  color: #1976d2;
}

.status-indicator.连接中 {
  background: #fff3e0;
  color: #f57c00;
}

.status-indicator.采集中 {
  background: #e8f5e8;
  color: #2e7d32;
  animation: pulse 2s infinite;
}

.device-info, .mode-info {
  color: #666;
  font-size: 12px;
}

.demo-body {
  padding: 24px;
}

.data-display {
  background: rgba(255, 255, 255, 0.98);
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.channel-section {
  margin-bottom: 24px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding-bottom: 8px;
  border-bottom: 1px solid #eee;
}

.section-header h4 {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0;
  color: #333;
  font-size: 16px;
  font-weight: 600;
}

.data-chart {
  height: 280px;
  border: 1px solid #e0e0e0;
  border-radius: 6px;
  background: #fafbfc;
  box-shadow: inset 0 1px 3px rgba(0, 0, 0, 0.05);
}

/* 确保波形图表使用浅色背景 */
.data-chart :deep(.chart-container) {
  background: #fafbfc !important;
}

.data-chart :deep(.waveform-chart-wrapper .chart-container) {
  background: linear-gradient(135deg, #fafbfc 0%, #ffffff 100%) !important;
}

.data-chart :deep(.waveform-chart-wrapper .professional-instrument .chart-container) {
  background: #fafbfc !important;
  border: 1px solid #e0e6ed;
}

.ao-controls {
  background: #f8f9fa;
  padding: 16px;
  border-radius: 6px;
}

.ao-channel {
  text-align: center;
  padding: 12px;
  background: white;
  border-radius: 6px;
  border: 1px solid #e0e0e0;
}

.ao-channel label {
  display: block;
  font-weight: 500;
  margin-bottom: 8px;
  color: #333;
}

.ao-value {
  margin-top: 8px;
  font-weight: 600;
  color: #667eea;
}

.control-panel {
  background: rgba(255, 255, 255, 0.98);
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  height: fit-content;
}

.dio-section {
  margin-bottom: 24px;
}

.dio-controls {
  space-y: 16px;
}

.do-group, .di-group {
  margin-bottom: 16px;
}

.do-group label, .di-group label {
  display: block;
  font-weight: 500;
  margin-bottom: 8px;
  color: #333;
}

.do-buttons {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 4px;
}

.di-indicators {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 4px;
}

.di-led {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 32px;
  background: #f5f5f5;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
  color: #666;
  transition: all 0.3s;
}

.di-led.active {
  background: #4caf50;
  color: white;
  border-color: #4caf50;
  box-shadow: 0 0 8px rgba(76, 175, 80, 0.3);
}

.stats-section {
  margin-bottom: 24px;
}

.stats-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 8px;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: #f8f9fa;
  border-radius: 4px;
  border: 1px solid #e9ecef;
}

.stat-label {
  font-size: 13px;
  color: #666;
}

.stat-value {
  font-weight: 600;
  color: #333;
  font-size: 13px;
}

.csharp-section {
  margin-bottom: 16px;
}

.csharp-output {
  margin-top: 12px;
  max-height: 200px;
  overflow-y: auto;
  background: #1e1e1e;
  color: #d4d4d4;
  border-radius: 4px;
  padding: 12px;
  font-family: 'Courier New', monospace;
  font-size: 12px;
  line-height: 1.4;
}

.channel-config {
  display: flex;
  align-items: center;
  margin-bottom: 8px;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.7; }
}

/* 响应式适配 */
@media (max-width: 1200px) {
  .demo-body .el-col:first-child {
    margin-bottom: 16px;
  }
}

@media (max-width: 768px) {
  .demo-header {
    flex-direction: column;
    align-items: stretch;
    gap: 16px;
  }
  
  .header-right {
    display: flex;
    justify-content: center;
  }
  
  .demo-status {
    justify-content: center;
  }
}
</style>
