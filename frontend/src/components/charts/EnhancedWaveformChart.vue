<template>
  <div class="enhanced-waveform-chart" :style="{ height: totalHeight }">
    <!-- 工具栏 -->
    <div class="chart-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <el-button-group size="small">
          <el-button @click="togglePause" :type="isPaused ? 'warning' : 'primary'">
            <el-icon><VideoPlay v-if="isPaused" /><VideoPause v-else /></el-icon>
            {{ isPaused ? '继续' : '暂停' }}
          </el-button>
          <el-button @click="clearChart">
            <el-icon><Delete /></el-icon>
            清除
          </el-button>
          <el-button @click="autoScale">
            <el-icon><FullScreen /></el-icon>
            自动缩放
          </el-button>
        </el-button-group>
        
        <el-divider direction="vertical" />
        
        <el-button-group size="small">
          <el-button @click="zoomIn">
            <el-icon><ZoomIn /></el-icon>
          </el-button>
          <el-button @click="zoomOut">
            <el-icon><ZoomOut /></el-icon>
          </el-button>
          <el-button @click="resetZoom">
            <el-icon><Refresh /></el-icon>
          </el-button>
        </el-button-group>
        
        <el-divider direction="vertical" />
        
        <el-dropdown @command="handleMeasurement" size="small">
          <el-button size="small">
            <el-icon><Ruler /></el-icon>
            测量
            <el-icon class="el-icon--right"><ArrowDown /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="cursor">光标测量</el-dropdown-item>
              <el-dropdown-item command="peak">峰值测量</el-dropdown-item>
              <el-dropdown-item command="rms">RMS测量</el-dropdown-item>
              <el-dropdown-item command="frequency">频率测量</el-dropdown-item>
              <el-dropdown-item divided command="clear">清除测量</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
      
      <div class="toolbar-right">
        <el-select v-model="displayMode" size="small" style="width: 120px">
          <el-option label="时域" value="time" />
          <el-option label="频域(FFT)" value="frequency" />
          <el-option label="XY模式" value="xy" />
        </el-select>
        
        <el-button @click="exportData" size="small">
          <el-icon><Download /></el-icon>
          导出
        </el-button>
      </div>
    </div>
    
    <!-- 主图表区域 -->
    <div class="chart-main" ref="chartContainer">
      <div ref="chartDiv" :style="{ width: '100%', height: chartHeight }"></div>
    </div>
    
    <!-- 测量结果面板 -->
    <div class="measurement-panel" v-if="showMeasurements && measurements.length > 0">
      <div class="measurement-item" v-for="(item, index) in measurements" :key="index">
        <span class="measurement-label">{{ item.label }}:</span>
        <span class="measurement-value" :style="{ color: item.color }">
          {{ item.value }}{{ item.unit }}
        </span>
      </div>
    </div>
    
    <!-- 通道控制面板 -->
    <div class="channel-panel" v-if="showChannelPanel">
      <div class="channel-item" v-for="channel in channels" :key="channel.id">
        <el-switch 
          v-model="channel.enabled" 
          @change="onChannelToggle(channel.id)"
          size="small"
        />
        <div 
          class="channel-color" 
          :style="{ backgroundColor: channel.color }"
        ></div>
        <span class="channel-name">{{ channel.name }}</span>
        <div class="channel-stats">
          <span class="stat-value">{{ getChannelStats(channel.id).last }}{{ channel.unit }}</span>
          <span class="stat-info">
            峰峰值: {{ getChannelStats(channel.id).pp }}{{ channel.unit }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed, nextTick } from 'vue'
import * as echarts from 'echarts'
import { 
  VideoPlay, 
  VideoPause, 
  Delete, 
  FullScreen, 
  ZoomIn, 
  ZoomOut,
  Refresh,
  Ruler,
  ArrowDown,
  Download
} from '@element-plus/icons-vue'
import { SignalAnalyzer } from '@/utils/signalGenerator'

interface Channel {
  id: string
  name: string
  color: string
  enabled: boolean
  unit: string
  data?: number[]
}

interface Measurement {
  label: string
  value: string
  unit: string
  color: string
}

const props = defineProps({
  data: {
    type: Array,
    default: () => []
  },
  channels: {
    type: Array as () => Channel[],
    default: () => []
  },
  height: {
    type: String,
    default: '400px'
  },
  showToolbar: {
    type: Boolean,
    default: true
  },
  showChannelPanel: {
    type: Boolean,
    default: true
  },
  showMeasurements: {
    type: Boolean,
    default: true
  },
  bufferSize: {
    type: Number,
    default: 1000
  },
  sampleRate: {
    type: Number,
    default: 10000
  }
})

const emit = defineEmits(['channelToggle', 'export'])

// 状态管理
const chartContainer = ref()
const chartDiv = ref()
const isPaused = ref(false)
const displayMode = ref('time')
const measurements = ref<Measurement[]>([])
const measurementMode = ref('')
const zoomLevel = ref(1)

// ECharts实例
let chartInstance: echarts.ECharts | null = null
const dataBuffer = new Map<string, number[]>()
const timeBuffer: number[] = []

// 计算属性
const totalHeight = computed(() => {
  const baseHeight = parseInt(props.height)
  const toolbarHeight = props.showToolbar ? 40 : 0
  const panelHeight = props.showChannelPanel ? 100 : 0
  const measurementHeight = showMeasurements.value && measurements.value.length > 0 ? 40 : 0
  return `${baseHeight + toolbarHeight + panelHeight + measurementHeight}px`
})

const chartHeight = computed(() => props.height)

const showMeasurements = computed(() => props.showMeasurements)

// 生命周期
onMounted(() => {
  initChart()
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  if (chartInstance) {
    chartInstance.dispose()
  }
  window.removeEventListener('resize', handleResize)
})

// 监听数据变化
watch(() => props.data, (newData) => {
  if (!isPaused.value && newData && newData.length > 0) {
    updateChart(newData)
  }
}, { deep: true })

watch(displayMode, () => {
  updateChartDisplay()
})

// 初始化图表
function initChart() {
  if (!chartDiv.value) return
  
  chartInstance = echarts.init(chartDiv.value, 'dark')
  
  const option = {
    backgroundColor: '#1e1e1e',
    animation: false,
    grid: {
      top: 40,
      right: 60,
      bottom: 60,
      left: 80
    },
    xAxis: {
      type: 'value',
      name: displayMode.value === 'frequency' ? '频率 (Hz)' : '时间 (ms)',
      nameLocation: 'center',
      nameGap: 35,
      splitLine: {
        show: true,
        lineStyle: {
          color: '#333',
          type: 'dashed'
        }
      },
      axisLine: {
        lineStyle: {
          color: '#666'
        }
      },
      axisLabel: {
        color: '#ccc'
      }
    },
    yAxis: {
      type: 'value',
      name: displayMode.value === 'frequency' ? '幅度' : '电压 (V)',
      nameLocation: 'center',
      nameGap: 50,
      splitLine: {
        show: true,
        lineStyle: {
          color: '#333',
          type: 'dashed'
        }
      },
      axisLine: {
        lineStyle: {
          color: '#666'
        }
      },
      axisLabel: {
        color: '#ccc'
      }
    },
    tooltip: {
      trigger: 'axis',
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      borderColor: '#333',
      textStyle: {
        color: '#fff'
      },
      formatter: (params: any) => {
        if (!params || params.length === 0) return ''
        
        const time = params[0].value[0]
        let result = displayMode.value === 'frequency' 
          ? `频率: ${time.toFixed(1)} Hz<br/>` 
          : `时间: ${time.toFixed(2)} ms<br/>`
        
        params.forEach((item: any) => {
          result += `${item.seriesName}: ${item.value[1].toFixed(3)} ${
            displayMode.value === 'frequency' ? '' : 'V'
          }<br/>`
        })
        
        return result
      }
    },
    legend: {
      top: 10,
      textStyle: {
        color: '#ccc'
      }
    },
    series: []
  }
  
  chartInstance.setOption(option)
}

// 更新图表数据
function updateChart(newData: any[]) {
  if (!chartInstance || !newData || newData.length === 0) return
  
  // 更新数据缓冲区
  newData.forEach(dataPoint => {
    const timestamp = dataPoint.timestamp || Date.now()
    timeBuffer.push(timestamp)
    
    props.channels.forEach(channel => {
      if (!channel.enabled) return
      
      const value = dataPoint[channel.id]
      if (value !== undefined) {
        if (!dataBuffer.has(channel.id)) {
          dataBuffer.set(channel.id, [])
        }
        const buffer = dataBuffer.get(channel.id)!
        buffer.push(value)
        
        // 限制缓冲区大小
        if (buffer.length > props.bufferSize) {
          buffer.shift()
        }
      }
    })
  })
  
  // 限制时间缓冲区大小
  if (timeBuffer.length > props.bufferSize) {
    timeBuffer.splice(0, timeBuffer.length - props.bufferSize)
  }
  
  updateChartDisplay()
}

// 更新图表显示
function updateChartDisplay() {
  if (!chartInstance) return
  
  const series: any[] = []
  
  if (displayMode.value === 'time') {
    // 时域显示
    props.channels.forEach(channel => {
      if (!channel.enabled) return
      
      const buffer = dataBuffer.get(channel.id) || []
      const data: [number, number][] = []
      
      buffer.forEach((value, index) => {
        const time = index * (1000 / props.sampleRate) // 转换为毫秒
        data.push([time, value])
      })
      
      series.push({
        name: channel.name,
        type: 'line',
        data: data,
        symbol: 'none',
        lineStyle: {
          color: channel.color,
          width: 2
        },
        emphasis: {
          focus: 'series'
        }
      })
    })
  } else if (displayMode.value === 'frequency') {
    // 频域显示（FFT）
    props.channels.forEach(channel => {
      if (!channel.enabled) return
      
      const buffer = dataBuffer.get(channel.id) || []
      if (buffer.length < 2) return
      
      const fftResult = SignalAnalyzer.simpleFFT(buffer)
      const data: [number, number][] = []
      
      fftResult.frequencies.forEach((freq, index) => {
        const actualFreq = freq * props.sampleRate / buffer.length
        if (actualFreq <= props.sampleRate / 2) { // Nyquist频率
          data.push([actualFreq, fftResult.magnitudes[index]])
        }
      })
      
      series.push({
        name: `${channel.name} (FFT)`,
        type: 'line',
        data: data,
        symbol: 'none',
        lineStyle: {
          color: channel.color,
          width: 2
        },
        emphasis: {
          focus: 'series'
        }
      })
    })
  } else if (displayMode.value === 'xy') {
    // XY模式（李萨如图）
    if (props.channels.length >= 2) {
      const ch1 = props.channels[0]
      const ch2 = props.channels[1]
      
      if (ch1.enabled && ch2.enabled) {
        const buffer1 = dataBuffer.get(ch1.id) || []
        const buffer2 = dataBuffer.get(ch2.id) || []
        const minLength = Math.min(buffer1.length, buffer2.length)
        
        const data: [number, number][] = []
        for (let i = 0; i < minLength; i++) {
          data.push([buffer1[i], buffer2[i]])
        }
        
        series.push({
          name: `${ch1.name} vs ${ch2.name}`,
          type: 'line',
          data: data,
          symbol: 'none',
          lineStyle: {
            color: '#00ff00',
            width: 2
          }
        })
      }
    }
  }
  
  // 更新图表配置
  chartInstance.setOption({
    xAxis: {
      name: displayMode.value === 'frequency' ? '频率 (Hz)' : 
             displayMode.value === 'xy' ? `${props.channels[0]?.name || 'X'} (V)` : 
             '时间 (ms)'
    },
    yAxis: {
      name: displayMode.value === 'frequency' ? '幅度' : 
             displayMode.value === 'xy' ? `${props.channels[1]?.name || 'Y'} (V)` : 
             '电压 (V)'
    },
    series: series
  })
}

// 控制功能
function togglePause() {
  isPaused.value = !isPaused.value
}

function clearChart() {
  dataBuffer.clear()
  timeBuffer.length = 0
  props.channels.forEach(channel => {
    dataBuffer.set(channel.id, [])
  })
  measurements.value = []
  updateChartDisplay()
}

function autoScale() {
  if (chartInstance) {
    chartInstance.dispatchAction({
      type: 'dataZoom',
      start: 0,
      end: 100
    })
  }
  zoomLevel.value = 1
}

function zoomIn() {
  zoomLevel.value *= 1.2
  applyZoom()
}

function zoomOut() {
  zoomLevel.value /= 1.2
  applyZoom()
}

function resetZoom() {
  zoomLevel.value = 1
  autoScale()
}

function applyZoom() {
  if (chartInstance) {
    const center = 50
    const range = 50 / zoomLevel.value
    chartInstance.dispatchAction({
      type: 'dataZoom',
      start: Math.max(0, center - range),
      end: Math.min(100, center + range)
    })
  }
}

// 测量功能
function handleMeasurement(command: string) {
  measurementMode.value = command
  
  if (command === 'clear') {
    measurements.value = []
    return
  }
  
  // 执行测量
  performMeasurement(command)
}

function performMeasurement(type: string) {
  measurements.value = []
  
  props.channels.forEach(channel => {
    if (!channel.enabled) return
    
    const buffer = dataBuffer.get(channel.id) || []
    if (buffer.length === 0) return
    
    let measurement: Measurement | null = null
    
    switch (type) {
      case 'peak':
        const pp = SignalAnalyzer.calculatePeakToPeak(buffer)
        measurement = {
          label: `${channel.name} 峰峰值`,
          value: pp.toFixed(3),
          unit: channel.unit,
          color: channel.color
        }
        break
        
      case 'rms':
        const rms = SignalAnalyzer.calculateRMS(buffer)
        measurement = {
          label: `${channel.name} RMS`,
          value: rms.toFixed(3),
          unit: channel.unit,
          color: channel.color
        }
        break
        
      case 'frequency':
        const freq = SignalAnalyzer.calculateFrequency(buffer, props.sampleRate)
        measurement = {
          label: `${channel.name} 频率`,
          value: freq.toFixed(1),
          unit: 'Hz',
          color: channel.color
        }
        break
        
      case 'cursor':
        const last = buffer[buffer.length - 1]
        measurement = {
          label: `${channel.name} 当前值`,
          value: last.toFixed(3),
          unit: channel.unit,
          color: channel.color
        }
        break
    }
    
    if (measurement) {
      measurements.value.push(measurement)
    }
  })
}

// 通道控制
function onChannelToggle(channelId: string) {
  emit('channelToggle', channelId)
  updateChartDisplay()
}

function getChannelStats(channelId: string) {
  const buffer = dataBuffer.get(channelId) || []
  const last = buffer.length > 0 ? buffer[buffer.length - 1] : 0
  const pp = buffer.length > 0 ? SignalAnalyzer.calculatePeakToPeak(buffer) : 0
  
  return {
    last: last.toFixed(2),
    pp: pp.toFixed(2)
  }
}

// 导出功能
function exportData() {
  const exportObj: any = {
    timestamp: new Date().toISOString(),
    sampleRate: props.sampleRate,
    channels: []
  }
  
  props.channels.forEach(channel => {
    if (!channel.enabled) return
    
    const buffer = dataBuffer.get(channel.id) || []
    exportObj.channels.push({
      id: channel.id,
      name: channel.name,
      unit: channel.unit,
      data: buffer
    })
  })
  
  emit('export', exportObj)
  
  // 下载JSON文件
  const blob = new Blob([JSON.stringify(exportObj, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `waveform_data_${Date.now()}.json`
  a.click()
  URL.revokeObjectURL(url)
}

// 响应式处理
function handleResize() {
  if (chartInstance) {
    chartInstance.resize()
  }
}
</script>

<style scoped>
.enhanced-waveform-chart {
  background: #1a1a1a;
  border-radius: 8px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.chart-toolbar {
  background: #2a2a2a;
  padding: 8px 12px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #333;
}

.toolbar-left, .toolbar-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.chart-main {
  flex: 1;
  position: relative;
  background: #1e1e1e;
}

.measurement-panel {
  background: #252525;
  padding: 8px 16px;
  display: flex;
  gap: 24px;
  border-top: 1px solid #333;
  flex-wrap: wrap;
}

.measurement-item {
  display: flex;
  gap: 8px;
  align-items: center;
}

.measurement-label {
  color: #999;
  font-size: 12px;
}

.measurement-value {
  font-weight: 600;
  font-size: 14px;
  font-family: 'Courier New', monospace;
}

.channel-panel {
  background: #2a2a2a;
  padding: 12px;
  border-top: 1px solid #333;
  max-height: 100px;
  overflow-y: auto;
}

.channel-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 6px 0;
  border-bottom: 1px solid #333;
}

.channel-item:last-child {
  border-bottom: none;
}

.channel-color {
  width: 16px;
  height: 16px;
  border-radius: 2px;
}

.channel-name {
  color: #ccc;
  font-size: 13px;
  flex: 1;
}

.channel-stats {
  display: flex;
  gap: 16px;
  align-items: center;
}

.stat-value {
  color: #fff;
  font-weight: 600;
  font-size: 13px;
  font-family: 'Courier New', monospace;
}

.stat-info {
  color: #999;
  font-size: 11px;
}

/* 深色主题支持 */
.el-button-group :deep(.el-button) {
  background: #333;
  border-color: #444;
  color: #ccc;
}

.el-button-group :deep(.el-button:hover) {
  background: #444;
  border-color: #555;
  color: #fff;
}

.el-select :deep(.el-input__inner) {
  background: #333;
  border-color: #444;
  color: #ccc;
}

.el-dropdown :deep(.el-button) {
  background: #333;
  border-color: #444;
  color: #ccc;
}
</style>