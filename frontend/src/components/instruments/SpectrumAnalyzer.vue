<template>
  <div class="spectrum-analyzer-wrapper professional-instrument">
    <!-- 频谱分析仪标题栏 -->
    <div class="analyzer-header">
      <div class="header-left">
        <h3>频谱分析仪</h3>
        <div class="model-info">SA-3000 Series</div>
      </div>
      <div class="header-right">
        <div class="status-indicators">
          <div class="status-item" :class="{ active: isRunning }">
            <div class="status-dot"></div>
            <span>{{ isRunning ? 'SWEEP' : 'STOP' }}</span>
          </div>
          <div class="status-item" :class="{ active: isCalibrated }">
            <div class="status-dot"></div>
            <span>CAL</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 主控制面板 -->
    <div class="control-panel">
      <el-row :gutter="16">
        <!-- 频率控制 -->
        <el-col :span="8">
          <div class="control-section">
            <h4>频率设置</h4>
            <div class="frequency-controls">
              <div class="control-row">
                <label>起始频率:</label>
                <el-input-number 
                  v-model="startFrequency" 
                  :min="0" 
                  :max="6000000000"
                  :step="1000000"
                  @change="updateFrequency"
                  size="small"
                  style="width: 120px;"
                />
                <span class="unit">Hz</span>
              </div>
              <div class="control-row">
                <label>停止频率:</label>
                <el-input-number 
                  v-model="stopFrequency" 
                  :min="startFrequency" 
                  :max="6000000000"
                  :step="1000000"
                  @change="updateFrequency"
                  size="small"
                  style="width: 120px;"
                />
                <span class="unit">Hz</span>
              </div>
              <div class="control-row">
                <label>中心频率:</label>
                <el-input-number 
                  v-model="centerFrequency" 
                  :min="0" 
                  :max="6000000000"
                  :step="1000000"
                  @change="updateCenterFrequency"
                  size="small"
                  style="width: 120px;"
                />
                <span class="unit">Hz</span>
              </div>
              <div class="control-row">
                <label>频率跨度:</label>
                <el-select v-model="frequencySpan" @change="updateSpan" size="small">
                  <el-option 
                    v-for="span in spanOptions" 
                    :key="span.value"
                    :label="span.label" 
                    :value="span.value" 
                  />
                </el-select>
              </div>
            </div>
          </div>
        </el-col>

        <!-- 幅度控制 -->
        <el-col :span="8">
          <div class="control-section">
            <h4>幅度设置</h4>
            <div class="amplitude-controls">
              <div class="control-row">
                <label>参考电平:</label>
                <el-input-number 
                  v-model="referenceLevel" 
                  :min="-100" 
                  :max="30"
                  :step="1"
                  @change="updateAmplitude"
                  size="small"
                  style="width: 100px;"
                />
                <span class="unit">dBm</span>
              </div>
              <div class="control-row">
                <label>刻度/格:</label>
                <el-select v-model="scalePerDiv" @change="updateAmplitude" size="small">
                  <el-option label="1dB/div" value="1" />
                  <el-option label="2dB/div" value="2" />
                  <el-option label="5dB/div" value="5" />
                  <el-option label="10dB/div" value="10" />
                </el-select>
              </div>
              <div class="control-row">
                <label>衰减器:</label>
                <el-select v-model="attenuator" @change="updateAmplitude" size="small">
                  <el-option label="自动" value="auto" />
                  <el-option label="0dB" value="0" />
                  <el-option label="10dB" value="10" />
                  <el-option label="20dB" value="20" />
                  <el-option label="30dB" value="30" />
                </el-select>
              </div>
              <div class="control-row">
                <label>单位:</label>
                <el-select v-model="amplitudeUnit" @change="updateAmplitude" size="small">
                  <el-option label="dBm" value="dbm" />
                  <el-option label="dBV" value="dbv" />
                  <el-option label="V" value="v" />
                  <el-option label="W" value="w" />
                </el-select>
              </div>
            </div>
          </div>
        </el-col>

        <!-- 扫描控制 -->
        <el-col :span="8">
          <div class="control-section">
            <h4>扫描设置</h4>
            <div class="sweep-controls">
              <div class="control-row">
                <label>扫描时间:</label>
                <el-select v-model="sweepTime" @change="updateSweep" size="small">
                  <el-option 
                    v-for="time in sweepTimeOptions" 
                    :key="time.value"
                    :label="time.label" 
                    :value="time.value" 
                  />
                </el-select>
              </div>
              <div class="control-row">
                <label>分辨率带宽:</label>
                <el-select v-model="rbw" @change="updateSweep" size="small">
                  <el-option 
                    v-for="bw in rbwOptions" 
                    :key="bw.value"
                    :label="bw.label" 
                    :value="bw.value" 
                  />
                </el-select>
              </div>
              <div class="control-row">
                <label>视频带宽:</label>
                <el-select v-model="vbw" @change="updateSweep" size="small">
                  <el-option 
                    v-for="bw in vbwOptions" 
                    :key="bw.value"
                    :label="bw.label" 
                    :value="bw.value" 
                  />
                </el-select>
              </div>
              <div class="control-row">
                <label>检波器:</label>
                <el-select v-model="detector" @change="updateSweep" size="small">
                  <el-option label="峰值" value="peak" />
                  <el-option label="平均" value="average" />
                  <el-option label="负峰值" value="negative" />
                  <el-option label="采样" value="sample" />
                </el-select>
              </div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 运行控制 -->
    <div class="run-control-panel">
      <div class="control-buttons">
        <el-button 
          :type="isRunning ? 'danger' : 'success'" 
          @click="toggleRun"
          size="small"
        >
          <el-icon><VideoPlay v-if="!isRunning" /><VideoPause v-else /></el-icon>
          {{ isRunning ? '停止扫描' : '开始扫描' }}
        </el-button>
        <el-button @click="singleSweep" :disabled="isRunning" size="small">
          <el-icon><Position /></el-icon>
          单次扫描
        </el-button>
        <el-button @click="autoScale" size="small">
          <el-icon><MagicStick /></el-icon>
          自动设置
        </el-button>
        <el-button @click="peakSearch" size="small">
          <el-icon><Search /></el-icon>
          峰值搜索
        </el-button>
        <el-button @click="calibrate" size="small">
          <el-icon><Tools /></el-icon>
          校准
        </el-button>
      </div>
      
      <div class="sweep-info">
        <span>扫描点数: {{ sweepPoints }}</span>
        <span>扫描时间: {{ formatSweepTime(sweepTime) }}</span>
        <span>频率分辨率: {{ formatFrequency(frequencyResolution) }}</span>
      </div>
    </div>

    <!-- 频谱显示区域 -->
    <div class="spectrum-display">
      <WaveformChart
        :data="spectrumData"
        :series-configs="seriesConfigs"
        :options="chartOptions"
        :show-toolbar="false"
        :show-controls="false"
        :show-status="false"
        :show-measurements="false"
        :show-cursors="showMarkers"
        :height="450"
        @zoom="handleZoom"
        @cursor-move="handleCursorMove"
      />
    </div>

    <!-- 标记和测量面板 -->
    <div class="marker-panel">
      <div class="panel-header">
        <h4>标记和测量</h4>
        <div class="marker-controls">
          <el-button @click="addMarker" size="small">
            <el-icon><Plus /></el-icon>
            添加标记
          </el-button>
          <el-button @click="clearMarkers" size="small">
            <el-icon><Delete /></el-icon>
            清除标记
          </el-button>
          <el-button @click="showMarkers = !showMarkers" size="small">
            <el-icon><Aim /></el-icon>
            显示标记
          </el-button>
          <el-button @click="deltaMode = !deltaMode" :type="deltaMode ? 'primary' : ''" size="small">
            <el-icon><Connection /></el-icon>
            Δ模式
          </el-button>
        </div>
      </div>
      
      <div class="markers-grid">
        <div 
          v-for="(marker, index) in markers" 
          :key="index"
          class="marker-item"
          :class="{ active: marker.active, delta: marker.isDelta }"
        >
          <div class="marker-header">
            <span class="marker-name">{{ marker.name }}</span>
            <span class="marker-type">{{ marker.isDelta ? 'Δ' : 'M' }}{{ index + 1 }}</span>
            <el-button 
              @click="removeMarker(index)" 
              size="small" 
              text
              class="remove-btn"
            >
              <el-icon><Close /></el-icon>
            </el-button>
          </div>
          <div class="marker-values">
            <div class="marker-frequency">
              {{ formatFrequency(marker.frequency) }}
            </div>
            <div class="marker-amplitude">
              {{ formatAmplitude(marker.amplitude, amplitudeUnit) }}
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 状态栏 -->
    <div class="status-bar">
      <div class="status-left">
        <span>频率范围: {{ formatFrequency(startFrequency) }} - {{ formatFrequency(stopFrequency) }}</span>
        <span>RBW: {{ formatFrequency(rbw) }}</span>
        <span>VBW: {{ formatFrequency(vbw) }}</span>
      </div>
      <div class="status-right">
        <span>扫描模式: {{ isRunning ? '连续' : '停止' }}</span>
        <span>检波器: {{ getDetectorName(detector) }}</span>
        <span>时间: {{ currentTime }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, watch } from 'vue'
import { 
  VideoPlay, VideoPause, Position, MagicStick, Search, Tools,
  Plus, Delete, Aim, Connection, Close
} from '@element-plus/icons-vue'
import WaveformChart from '@/components/charts/WaveformChart.vue'
import type { WaveformData, WaveformOptions, WaveformSeriesConfig } from '@/types/chart'

// Props
interface Props {
  width?: string | number
  height?: string | number
}

const props = withDefaults(defineProps<Props>(), {
  width: '100%',
  height: '900px'
})

// 响应式数据
const isRunning = ref(false)
const isCalibrated = ref(true)
const showMarkers = ref(false)
const deltaMode = ref(false)
const currentTime = ref('')

// 频率控制
const startFrequency = ref(100000000) // 100MHz
const stopFrequency = ref(1000000000) // 1GHz
const centerFrequency = ref(550000000) // 550MHz
const frequencySpan = ref(900000000) // 900MHz

// 幅度控制
const referenceLevel = ref(0) // 0dBm
const scalePerDiv = ref('10') // 10dB/div
const attenuator = ref('auto')
const amplitudeUnit = ref('dbm')

// 扫描控制
const sweepTime = ref(1) // 1s
const rbw = ref(1000000) // 1MHz
const vbw = ref(1000000) // 1MHz
const detector = ref('peak')
const sweepPoints = ref(1001)

// 标记数据
const markers = ref<Array<{
  name: string
  frequency: number
  amplitude: number
  active: boolean
  isDelta: boolean
}>>([])

// 频率跨度选项
const spanOptions = [
  { label: '1MHz', value: 1000000 },
  { label: '10MHz', value: 10000000 },
  { label: '100MHz', value: 100000000 },
  { label: '500MHz', value: 500000000 },
  { label: '1GHz', value: 1000000000 },
  { label: '2GHz', value: 2000000000 },
  { label: '5GHz', value: 5000000000 },
  { label: '全频段', value: 6000000000 }
]

// 扫描时间选项
const sweepTimeOptions = [
  { label: '10ms', value: 0.01 },
  { label: '100ms', value: 0.1 },
  { label: '1s', value: 1 },
  { label: '10s', value: 10 },
  { label: '100s', value: 100 },
  { label: '自动', value: -1 }
]

// RBW选项
const rbwOptions = [
  { label: '1kHz', value: 1000 },
  { label: '10kHz', value: 10000 },
  { label: '100kHz', value: 100000 },
  { label: '1MHz', value: 1000000 },
  { label: '10MHz', value: 10000000 },
  { label: '自动', value: -1 }
]

// VBW选项
const vbwOptions = [
  { label: '1kHz', value: 1000 },
  { label: '10kHz', value: 10000 },
  { label: '100kHz', value: 100000 },
  { label: '1MHz', value: 1000000 },
  { label: '10MHz', value: 10000000 },
  { label: '自动', value: -1 }
]

// 计算属性
const frequencyResolution = computed(() => {
  return (stopFrequency.value - startFrequency.value) / sweepPoints.value
})

const seriesConfigs = computed<WaveformSeriesConfig[]>(() => [
  {
    name: '频谱',
    color: '#409eff',
    lineWidth: 1.5,
    lineType: 'solid',
    markerType: 'none',
    markerSize: 4,
    visible: true,
    yOffset: 0,
    yScale: 1,
    coupling: 'DC',
    bandwidth: 100000000,
    probe: 1
  }
])

const chartOptions = computed<WaveformOptions>(() => ({
  autoScale: false,
  logarithmic: false,
  splitView: false,
  legendVisible: false,
  cursorMode: 'cursor',
  gridEnabled: true,
  minorGridEnabled: true,
  theme: 'light',
  realTime: isRunning.value,
  bufferSize: sweepPoints.value,
  triggerMode: 'auto',
  triggerLevel: 0,
  triggerChannel: 0
}))

// 频谱数据
const spectrumData = ref<WaveformData>({
  series: [],
  xStart: startFrequency.value,
  xInterval: frequencyResolution.value,
  sampleRate: 1000000
})

// 定时器
let sweepTimer: number | null = null
let timeTimer: number | null = null

// 格式化函数
const formatFrequency = (freq: number) => {
  if (freq >= 1e9) {
    return `${(freq / 1e9).toFixed(2)}GHz`
  } else if (freq >= 1e6) {
    return `${(freq / 1e6).toFixed(1)}MHz`
  } else if (freq >= 1e3) {
    return `${(freq / 1e3).toFixed(1)}kHz`
  } else {
    return `${freq.toFixed(0)}Hz`
  }
}

const formatAmplitude = (amp: number, unit: string) => {
  switch (unit) {
    case 'dbm':
      return `${amp.toFixed(2)}dBm`
    case 'dbv':
      return `${amp.toFixed(2)}dBV`
    case 'v':
      return `${(Math.pow(10, amp/20) * 1e-3).toFixed(3)}V`
    case 'w':
      return `${(Math.pow(10, amp/10) * 1e-3).toFixed(6)}W`
    default:
      return `${amp.toFixed(2)}dBm`
  }
}

const formatSweepTime = (time: number) => {
  if (time < 0) return '自动'
  if (time >= 1) {
    return `${time.toFixed(1)}s`
  } else {
    return `${(time * 1000).toFixed(0)}ms`
  }
}

const getDetectorName = (det: string) => {
  const names: Record<string, string> = {
    peak: '峰值',
    average: '平均',
    negative: '负峰值',
    sample: '采样'
  }
  return names[det] || det
}

// 生成频谱数据
const generateSpectrumData = () => {
  const data: number[] = []
  const freqStep = (stopFrequency.value - startFrequency.value) / sweepPoints.value
  
  for (let i = 0; i < sweepPoints.value; i++) {
    const freq = startFrequency.value + i * freqStep
    let amplitude = referenceLevel.value - 60 // 基础噪声电平
    
    // 添加一些信号峰值
    const signals = [
      { freq: 200000000, amp: -20, bw: 1000000 }, // 200MHz, -20dBm
      { freq: 500000000, amp: -10, bw: 2000000 }, // 500MHz, -10dBm
      { freq: 800000000, amp: -30, bw: 500000 },  // 800MHz, -30dBm
    ]
    
    signals.forEach(signal => {
      const freqDiff = Math.abs(freq - signal.freq)
      if (freqDiff < signal.bw) {
        const factor = Math.exp(-Math.pow(freqDiff / (signal.bw / 3), 2))
        amplitude = Math.max(amplitude, signal.amp * factor)
      }
    })
    
    // 添加噪声
    amplitude += (Math.random() - 0.5) * 5
    
    // 应用检波器
    switch (detector.value) {
      case 'peak':
        amplitude += Math.random() * 2
        break
      case 'average':
        amplitude -= Math.random() * 1
        break
      case 'negative':
        amplitude -= Math.random() * 3
        break
    }
    
    data.push(amplitude)
  }
  
  spectrumData.value = {
    series: data,
    xStart: startFrequency.value,
    xInterval: freqStep,
    labels: Array.from({ length: sweepPoints.value }, (_, i) => 
      formatFrequency(startFrequency.value + i * freqStep)
    ),
    sampleRate: 1000000
  }
}

// 控制方法
const toggleRun = () => {
  isRunning.value = !isRunning.value
  
  if (isRunning.value) {
    startSweep()
  } else {
    stopSweep()
  }
}

const singleSweep = () => {
  if (isRunning.value) return
  
  generateSpectrumData()
}

const autoScale = () => {
  // 自动设置最佳参数
  const data = spectrumData.value.series as number[]
  if (data.length > 0) {
    const maxAmp = Math.max(...data)
    const minAmp = Math.min(...data)
    referenceLevel.value = Math.ceil(maxAmp / 10) * 10
    scalePerDiv.value = Math.ceil((maxAmp - minAmp) / 80).toString()
  }
  
  generateSpectrumData()
}

const peakSearch = () => {
  const data = spectrumData.value.series as number[]
  if (data.length === 0) return
  
  let maxIndex = 0
  let maxValue = data[0]
  
  for (let i = 1; i < data.length; i++) {
    if (data[i] > maxValue) {
      maxValue = data[i]
      maxIndex = i
    }
  }
  
  const peakFreq = startFrequency.value + maxIndex * frequencyResolution.value
  
  // 添加峰值标记
  addMarkerAt(peakFreq, maxValue)
}

const calibrate = () => {
  isCalibrated.value = false
  
  // 模拟校准过程
  setTimeout(() => {
    isCalibrated.value = true
  }, 2000)
}

const updateFrequency = () => {
  centerFrequency.value = (startFrequency.value + stopFrequency.value) / 2
  frequencySpan.value = stopFrequency.value - startFrequency.value
  generateSpectrumData()
}

const updateCenterFrequency = () => {
  const halfSpan = frequencySpan.value / 2
  startFrequency.value = centerFrequency.value - halfSpan
  stopFrequency.value = centerFrequency.value + halfSpan
  generateSpectrumData()
}

const updateSpan = () => {
  const halfSpan = frequencySpan.value / 2
  startFrequency.value = centerFrequency.value - halfSpan
  stopFrequency.value = centerFrequency.value + halfSpan
  generateSpectrumData()
}

const updateAmplitude = () => {
  generateSpectrumData()
}

const updateSweep = () => {
  generateSpectrumData()
}

const addMarker = () => {
  const freq = centerFrequency.value
  const data = spectrumData.value.series as number[]
  const index = Math.floor((freq - startFrequency.value) / frequencyResolution.value)
  const amp = data[index] || referenceLevel.value - 50
  
  addMarkerAt(freq, amp)
}

const addMarkerAt = (frequency: number, amplitude: number) => {
  const markerCount = markers.value.length
  const isDelta = deltaMode.value && markerCount > 0
  
  markers.value.push({
    name: isDelta ? `Δ${markerCount + 1}` : `M${markerCount + 1}`,
    frequency,
    amplitude,
    active: true,
    isDelta
  })
}

const removeMarker = (index: number) => {
  markers.value.splice(index, 1)
}

const clearMarkers = () => {
  markers.value = []
}

const startSweep = () => {
  if (sweepTimer) return
  
  const interval = sweepTime.value > 0 ? sweepTime.value * 1000 : 1000
  
  sweepTimer = setInterval(() => {
    generateSpectrumData()
  }, interval)
}

const stopSweep = () => {
  if (sweepTimer) {
    clearInterval(sweepTimer)
    sweepTimer = null
  }
}

const updateTime = () => {
  currentTime.value = new Date().toLocaleTimeString()
}

// 事件处理
const handleZoom = (range: any) => {
  console.log('缩放:', range)
}

const handleCursorMove = (position: any) => {
  console.log('游标:', position)
}

// 生命周期
onMounted(() => {
  generateSpectrumData()
  updateTime()
  
  timeTimer = setInterval(updateTime, 1000)
  
  // 添加一些默认标记
  addMarkerAt(200000000, -20)
  addMarkerAt(500000000, -10)
})

onUnmounted(() => {
  stopSweep()
  if (timeTimer) {
    clearInterval(timeTimer)
  }
})

// 监听运行状态
watch(isRunning, (running) => {
  if (running) {
    startSweep()
  } else {
    stopSweep()
  }
})
</script>

<style lang="scss" scoped>
.spectrum-analyzer-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #f8f9fa;
  border: 2px solid #dee2e6;
  border-radius: 12px;
  overflow: hidden;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  
  .analyzer-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px 20px;
    background: linear-gradient(135deg, #6f42c1 0%, #8e44ad 100%);
    color: white;
    
    .header-left {
      h3 {
        margin: 0;
        font-size: 18px;
        font-weight: 600;
      }
      
      .model-info {
        font-size: 12px;
        opacity: 0.8;
        margin-top: 2px;
      }
    }
    
    .header-right {
      .status-indicators {
        display: flex;
        gap: 16px;
        
        .status-item {
          display: flex;
          align-items: center;
          gap: 6px;
          padding: 4px 8px;
          border-radius: 4px;
          background: rgba(255, 255, 255, 0.1);
          font-size: 12px;
          font-weight: 500;
          
          .status-dot {
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: #6c757d;
            transition: background-color 0.3s;
          }
          
          &.active .status-dot {
            background: #28a745;
            box-shadow: 0 0 8px rgba(40, 167, 69, 0.6);
          }
        }
      }
    }
  }
  
  .control-panel {
    padding: 16px 20px;
    background: #ffffff;
    border-bottom: 1px solid #dee2e6;
    
    .control-section {
      h4 {
        margin: 0 0 12px 0;
        font-size: 14px;
        font-weight: 600;
        color: #495057;
        border-bottom: 2px solid #e9ecef;
        padding-bottom: 4px;
      }
      
      .frequency-controls,
      .amplitude-controls,
      .sweep-controls {
        .control-row {
          display: flex;
          align-items: center;
          margin-bottom: 8px;
          
          label {
            min-width: 90px;
            font-size: 12px;
            color: #6c757d;
            margin-right: 8px;
          }
          
          .unit {
            margin-left: 4px;
            font-size: 12px;
            color: #6c757d;
          }
        }
      }
    }
  }
  
  .run-control-panel {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px 20px;
    background: #ffffff;
    border-bottom: 1px solid #dee2e6;
    
    .control-buttons {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }
    
    .sweep-info {
      display: flex;
      gap: 16px;
      font-size: 12px;
      color: #6c757d;
      font-family: 'Courier New', monospace;
    }
  }
  
  .spectrum-display {
    flex: 1;
    padding: 16px 20px;
    background: #ffffff;
    border-bottom: 1px solid #dee2e6;
  }
  
  .marker-panel {
    padding: 16px 20px;
    background: #ffffff;
    border-bottom: 1px solid #dee2e6;
    
    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
      
      h4 {
        margin: 0;
        font-size: 14px;
        font-weight: 600;
        color: #495057;
      }
      
      .marker-controls {
        display: flex;
        gap: 8px;
      }
    }
    
    .markers-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 12px;
      
      .marker-item {
        border: 2px solid #e9ecef;
        border-radius: 8px;
        padding: 12px;
        background: #f8f9fa;
        transition: all 0.3s;
        
        &.active {
          border-color: #007bff;
          background: rgba(0, 123, 255, 0.05);
        }
        
        &.delta {
          border-color: #fd7e14;
          background: rgba(253, 126, 20, 0.05);
        }
        
        .marker-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          margin-bottom: 8px;
          
          .marker-name {
            font-size: 12px;
            font-weight: 600;
            color: #495057;
          }
          
          .marker-type {
            font-size: 11px;
            color: #6c757d;
            background: #e9ecef;
            padding: 2px 6px;
            border-radius: 3px;
            font-family: 'Courier New', monospace;
          }
          
          .remove-btn {
            padding: 2px;
            min-height: auto;
            
            .el-icon {
              font-size: 12px;
            }
          }
        }
        
        .marker-values {
          .marker-frequency {
            font-size: 14px;
            font-weight: 700;
            color: #212529;
            font-family: 'Courier New', monospace;
            margin-bottom: 4px;
          }
          
          .marker-amplitude {
            font-size: 13px;
            font-weight: 600;
            color: #6f42c1;
            font-family: 'Courier New', monospace;
          }
        }
      }
    }
  }
  
  .status-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 20px;
    background: #e9ecef;
    font-size: 11px;
    color: #6c757d;
    font-family: 'Courier New', monospace;
    
    .status-left,
    .status-right {
      display: flex;
      gap: 16px;
    }
    
    span {
      white-space: nowrap;
    }
  }
}

// 专业仪器样式
.professional-instrument {
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}

// 响应式设计
@media (max-width: 1200px) {
  .spectrum-analyzer-wrapper {
    .control-panel {
      .control-section {
        margin-bottom: 16px;
      }
    }
  }
}

@media (max-width: 768px) {
  .spectrum-analyzer-wrapper {
    .analyzer-header {
      flex-direction: column;
      gap: 8px;
      text-align: center;
    }
    
    .control-panel {
      .control-section {
        .frequency-controls,
        .amplitude-controls,
        .sweep-controls {
          .control-row {
            flex-direction: column;
            align-items: flex-start;
            
            label {
              margin-bottom: 4px;
            }
          }
        }
      }
    }
    
    .run-control-panel {
      flex-direction: column;
      gap: 12px;
      
      .control-buttons {
        justify-content: center;
      }
      
      .sweep-info {
        justify-content: center;
        flex-wrap: wrap;
      }
    }
    
    .markers-grid {
      grid-template-columns: 1fr;
    }
    
    .status-bar {
      flex-direction: column;
      gap: 8px;
      
      .status-left,
      .status-right {
        justify-content: center;
        flex-wrap: wrap;
      }
    }
  }
}
</style>
