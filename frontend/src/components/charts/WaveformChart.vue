<template>
  <div class="waveform-chart-wrapper professional-instrument">
    <!-- 工具栏 -->
    <div class="chart-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <el-button-group size="small">
          <el-button @click="resetZoom" :disabled="!isZoomed">
            <el-icon><Refresh /></el-icon>
            复位
          </el-button>
          <el-button @click="autoScale">
            <el-icon><Expand /></el-icon>
            自动缩放
          </el-button>
          <el-button @click="toggleRun" :type="isRunning ? 'danger' : 'success'">
            <el-icon><VideoPlay v-if="!isRunning" /><VideoPause v-else /></el-icon>
            {{ isRunning ? '停止' : '运行' }}
          </el-button>
          <el-button @click="singleTrigger" :disabled="isRunning">
            <el-icon><Position /></el-icon>
            单次
          </el-button>
        </el-button-group>
        
        <el-divider direction="vertical" />
        
        <el-button-group size="small">
          <el-button @click="exportChart('png')">
            <el-icon><Download /></el-icon>
            导出图片
          </el-button>
          <el-button @click="exportChart('csv')">
            <el-icon><Document /></el-icon>
            导出数据
          </el-button>
        </el-button-group>
      </div>
      
      <div class="toolbar-right">
        <div class="trigger-status" :class="triggerStatus">
          <div class="status-dot"></div>
          <span>{{ getTriggerStatusText() }}</span>
        </div>
        
        <el-tooltip content="全屏显示">
          <el-button size="small" @click="toggleFullscreen">
            <el-icon><FullScreen /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>

    <!-- 图表容器 -->
    <div 
      ref="chartContainer" 
      class="chart-container"
      :class="{ fullscreen: isFullscreen }"
      :style="containerStyle"
    ></div>

    <!-- 测量面板 -->
    <div class="measurement-panel" v-if="showMeasurements && measurements.length > 0">
      <div class="panel-header">
        <span>测量结果</span>
        <el-button size="small" text @click="clearMeasurements">
          <el-icon><Delete /></el-icon>
        </el-button>
      </div>
      <div class="measurements-grid">
        <div 
          v-for="(measurement, index) in measurements" 
          :key="index"
          class="measurement-item"
          :style="{ borderColor: measurement.color }"
        >
          <div class="measurement-label">{{ measurement.label }}</div>
          <div class="measurement-value">
            {{ formatMeasurementValue(measurement.value, measurement.unit) }}
          </div>
        </div>
      </div>
    </div>

    <!-- 游标信息 -->
    <div class="cursor-info" v-if="showCursors && cursorData">
      <div class="cursor-group">
        <div class="cursor-item">
          <span class="cursor-label">X1:</span>
          <span class="cursor-value">{{ formatCursorValue(cursorData.x1, 's') }}</span>
        </div>
        <div class="cursor-item">
          <span class="cursor-label">X2:</span>
          <span class="cursor-value">{{ formatCursorValue(cursorData.x2, 's') }}</span>
        </div>
        <div class="cursor-item">
          <span class="cursor-label">ΔX:</span>
          <span class="cursor-value">{{ formatCursorValue(cursorData.deltaX, 's') }}</span>
        </div>
        <div class="cursor-item">
          <span class="cursor-label">1/ΔX:</span>
          <span class="cursor-value">{{ formatCursorValue(1/cursorData.deltaX, 'Hz') }}</span>
        </div>
      </div>
    </div>

    <!-- 控制面板 -->
    <div class="chart-controls" v-if="showControls">
      <el-row :gutter="16">
        <!-- 时基控制 -->
        <el-col :span="6">
          <div class="control-group">
            <label>时基 (s/div):</label>
            <el-select v-model="timebaseScale" @change="updateTimebase" size="small">
              <el-option 
                v-for="scale in timebaseOptions" 
                :key="scale.value"
                :label="scale.label" 
                :value="scale.value" 
              />
            </el-select>
          </div>
        </el-col>
        
        <!-- 触发控制 -->
        <el-col :span="6">
          <div class="control-group">
            <label>触发:</label>
            <div class="trigger-controls">
              <el-select v-model="triggerMode" @change="updateTrigger" size="small">
                <el-option label="自动" value="auto" />
                <el-option label="正常" value="normal" />
                <el-option label="单次" value="single" />
              </el-select>
              <el-input-number 
                v-model="triggerLevel" 
                @change="updateTrigger"
                :step="0.1" 
                size="small"
                style="width: 80px; margin-top: 4px;"
              />
            </div>
          </div>
        </el-col>
        
        <!-- 显示选项 -->
        <el-col :span="6">
          <div class="control-group">
            <label>显示:</label>
            <div class="display-options">
              <el-checkbox v-model="localOptions.gridEnabled" @change="updateChart">
                网格
              </el-checkbox>
              <el-checkbox v-model="localOptions.legendVisible" @change="updateChart">
                图例
              </el-checkbox>
              <el-checkbox :model-value="showCursors" @change="(val) => emit('update:showCursors', val)">
                游标
              </el-checkbox>
            </div>
          </div>
        </el-col>
        
        <!-- 采集控制 -->
        <el-col :span="6">
          <div class="control-group">
            <label>采集:</label>
            <div class="acquisition-controls">
              <el-select v-model="acquisitionMode" @change="updateAcquisition" size="small">
                <el-option label="采样" value="sample" />
                <el-option label="平均" value="average" />
                <el-option label="包络" value="envelope" />
              </el-select>
              <div class="sample-rate">
                <span>{{ formatSampleRate(sampleRate) }}</span>
              </div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 状态栏 -->
    <div class="chart-status" v-if="showStatus">
      <div class="status-left">
        <span>采样率: {{ formatSampleRate(sampleRate) }}</span>
        <span>数据点: {{ dataPointsCount }}</span>
        <span>通道: {{ activeChannels }}</span>
      </div>
      <div class="status-right">
        <span>内存使用: {{ formatMemoryUsage(memoryUsage) }}</span>
        <span v-if="acquisitionTime">采集时间: {{ formatTime(acquisitionTime) }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed, nextTick } from 'vue'
import * as echarts from 'echarts'
import { 
  Refresh, Download, Document, FullScreen, Expand,
  VideoPlay, VideoPause, Position, Delete
} from '@element-plus/icons-vue'
import type { 
  WaveformData, 
  WaveformOptions, 
  WaveformSeriesConfig, 
  ExportOptions,
  WaveformEvents,
  TriggerConfig,
  MeasurementConfig
} from '@/types/chart'

// Props
interface Props {
  data?: WaveformData
  options?: Partial<WaveformOptions>
  seriesConfigs?: WaveformSeriesConfig[]
  showToolbar?: boolean
  showControls?: boolean
  showStatus?: boolean
  showMeasurements?: boolean
  showCursors?: boolean
  width?: string | number
  height?: string | number
  events?: WaveformEvents
  realTime?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  showToolbar: true,
  showControls: true,
  showStatus: true,
  showMeasurements: true,
  showCursors: false,
  width: '100%',
  height: '400px',
  realTime: false
})

// Emits
const emit = defineEmits<{
  dataUpdate: [data: WaveformData]
  zoom: [range: { xMin: number; xMax: number; yMin: number; yMax: number }]
  cursorMove: [position: { x: number; y: number }]
  seriesToggle: [seriesIndex: number, visible: boolean]
  trigger: [triggerInfo: { time: number; level: number; channel: number }]
  measurement: [measurement: { type: string; value: number; unit: string }]
  runStateChange: [isRunning: boolean]
  'update:showCursors': [value: boolean]
}>()

// 响应式数据
const chartContainer = ref<HTMLDivElement>()
const chart = ref<echarts.ECharts>()
const isFullscreen = ref(false)
const isRunning = ref(false)
const isZoomed = ref(false)
const triggerStatus = ref<'waiting' | 'triggered' | 'stopped'>('waiting')

// 控制参数
const timebaseScale = ref(0.001) // 1ms/div
const triggerMode = ref<'auto' | 'normal' | 'single'>('auto')
const triggerLevel = ref(0)
const acquisitionMode = ref<'sample' | 'average' | 'envelope'>('sample')
const sampleRate = ref(1000000) // 1MHz

// 测量和游标数据
const measurements = ref<Array<{
  label: string
  value: number
  unit: string
  color: string
}>>([])

const cursorData = ref<{
  x1: number
  x2: number
  deltaX: number
  y1: number
  y2: number
  deltaY: number
} | null>(null)

// 默认配置
const defaultOptions: WaveformOptions = {
  autoScale: true,
  logarithmic: false,
  splitView: false,
  legendVisible: true,
  cursorMode: 'zoom',
  gridEnabled: true,
  minorGridEnabled: true,
  theme: 'light',
  realTime: false,
  bufferSize: 10000,
  triggerMode: 'auto',
  triggerLevel: 0,
  triggerChannel: 0
}

const localOptions = ref<WaveformOptions>({ ...defaultOptions, ...props.options })

// 时基选项
const timebaseOptions = [
  { label: '1ns/div', value: 1e-9 },
  { label: '2ns/div', value: 2e-9 },
  { label: '5ns/div', value: 5e-9 },
  { label: '10ns/div', value: 1e-8 },
  { label: '20ns/div', value: 2e-8 },
  { label: '50ns/div', value: 5e-8 },
  { label: '100ns/div', value: 1e-7 },
  { label: '200ns/div', value: 2e-7 },
  { label: '500ns/div', value: 5e-7 },
  { label: '1μs/div', value: 1e-6 },
  { label: '2μs/div', value: 2e-6 },
  { label: '5μs/div', value: 5e-6 },
  { label: '10μs/div', value: 1e-5 },
  { label: '20μs/div', value: 2e-5 },
  { label: '50μs/div', value: 5e-5 },
  { label: '100μs/div', value: 1e-4 },
  { label: '200μs/div', value: 2e-4 },
  { label: '500μs/div', value: 5e-4 },
  { label: '1ms/div', value: 1e-3 },
  { label: '2ms/div', value: 2e-3 },
  { label: '5ms/div', value: 5e-3 },
  { label: '10ms/div', value: 1e-2 },
  { label: '20ms/div', value: 2e-2 },
  { label: '50ms/div', value: 5e-2 },
  { label: '100ms/div', value: 1e-1 },
  { label: '200ms/div', value: 2e-1 },
  { label: '500ms/div', value: 5e-1 },
  { label: '1s/div', value: 1 },
  { label: '2s/div', value: 2 },
  { label: '5s/div', value: 5 },
  { label: '10s/div', value: 10 }
]

// 计算属性
const dataPointsCount = computed(() => {
  if (!props.data?.series) return 0
  if (Array.isArray(props.data.series[0])) {
    return (props.data.series as number[][])[0]?.length || 0
  }
  return (props.data.series as number[]).length
})

const activeChannels = computed(() => {
  if (!props.data?.series) return 0
  if (Array.isArray(props.data.series[0])) {
    return (props.data.series as number[][]).length
  }
  return 1
})

const memoryUsage = computed(() => {
  return dataPointsCount.value * activeChannels.value * 8 // 8 bytes per double
})

const acquisitionTime = computed(() => {
  if (!props.data?.sampleRate || !dataPointsCount.value) return 0
  return dataPointsCount.value / props.data.sampleRate
})

const containerStyle = computed(() => ({
  width: typeof props.width === 'number' ? `${props.width}px` : props.width,
  height: typeof props.height === 'number' ? `${props.height}px` : props.height
}))

// 初始化图表
const initChart = () => {
  if (!chartContainer.value) return

  // 销毁现有图表
  if (chart.value) {
    chart.value.dispose()
  }

  // 创建新图表
  chart.value = echarts.init(chartContainer.value, localOptions.value.theme)
  
  // 设置初始配置
  updateChart()
  
  // 绑定事件
  bindEvents()
}

// 更新图表
const updateChart = () => {
  if (!chart.value || !props.data) return

  const option = generateEChartsOption()
  chart.value.setOption(option, true)
}

// 生成 ECharts 配置
const generateEChartsOption = () => {
  const { data } = props
  if (!data) return {}

  // 处理数据
  const series = processSeriesData(data)
  const xAxisData = generateXAxisData()
  
  return {
    title: {
      show: false
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'cross',
        lineStyle: {
          color: '#999',
          width: 1,
          type: 'dashed'
        }
      },
      formatter: (params: any) => {
        let result = `时间: ${formatTime(params[0].axisValue)}<br/>`
        params.forEach((param: any) => {
          result += `${param.seriesName}: ${formatVoltage(param.value)}<br/>`
        })
        return result
      }
    },
    legend: {
      show: localOptions.value.legendVisible,
      top: 5,
      type: 'scroll',
      textStyle: {
        fontSize: 12
      }
    },
    grid: {
      left: '8%',
      right: '5%',
      bottom: '12%',
      top: localOptions.value.legendVisible ? '12%' : '8%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: xAxisData,
      axisLine: { 
        show: true,
        lineStyle: { color: '#333' }
      },
      axisTick: { 
        show: true,
        lineStyle: { color: '#333' }
      },
      axisLabel: { 
        show: true,
        formatter: (value: string) => formatTime(parseFloat(value)),
        fontSize: 11
      },
      splitLine: { 
        show: localOptions.value.gridEnabled,
        lineStyle: { 
          type: 'solid', 
          color: '#e0e6ed',
          width: 1
        }
      },
      minorSplitLine: {
        show: localOptions.value.minorGridEnabled,
        lineStyle: { 
          type: 'dashed', 
          color: '#f0f2f5',
          width: 0.5
        }
      }
    },
    yAxis: {
      type: localOptions.value.logarithmic ? 'log' : 'value',
      scale: !localOptions.value.autoScale,
      axisLine: { 
        show: true,
        lineStyle: { color: '#333' }
      },
      axisTick: { 
        show: true,
        lineStyle: { color: '#333' }
      },
      axisLabel: { 
        show: true,
        formatter: (value: number) => formatVoltage(value),
        fontSize: 11
      },
      splitLine: { 
        show: localOptions.value.gridEnabled,
        lineStyle: { 
          type: 'solid', 
          color: '#e0e6ed',
          width: 1
        }
      },
      minorSplitLine: {
        show: localOptions.value.minorGridEnabled,
        lineStyle: { 
          type: 'dashed', 
          color: '#f0f2f5',
          width: 0.5
        }
      }
    },
    dataZoom: localOptions.value.cursorMode === 'zoom' ? [
      {
        type: 'inside',
        xAxisIndex: 0,
        filterMode: 'none'
      },
      {
        type: 'inside',
        yAxisIndex: 0,
        filterMode: 'none'
      }
    ] : [],
    series
  }
}

// 处理系列数据
const processSeriesData = (data: WaveformData) => {
  const { series } = data
  const result: any[] = []

  if (Array.isArray(series[0])) {
    // 多系列数据
    (series as number[][]).forEach((seriesData, index) => {
      const config = props.seriesConfigs?.[index]
      result.push({
        name: config?.name || `CH${index + 1}`,
        type: 'line',
        data: seriesData,
        lineStyle: {
          color: config?.color || getDefaultChannelColor(index),
          width: config?.lineWidth || 1.5,
          type: config?.lineType || 'solid'
        },
        itemStyle: {
          color: config?.color || getDefaultChannelColor(index)
        },
        symbol: 'none',
        smooth: false,
        connectNulls: false,
        sampling: 'lttb', // 大数据量优化
        large: dataPointsCount.value > 1000,
        largeThreshold: 1000
      })
    })
  } else {
    // 单系列数据
    const config = props.seriesConfigs?.[0]
    result.push({
      name: config?.name || 'CH1',
      type: 'line',
      data: series as number[],
      lineStyle: {
        color: config?.color || '#409eff',
        width: config?.lineWidth || 1.5,
        type: config?.lineType || 'solid'
      },
      itemStyle: {
        color: config?.color || '#409eff'
      },
      symbol: 'none',
      smooth: false,
      connectNulls: false,
      sampling: 'lttb',
      large: dataPointsCount.value > 1000,
      largeThreshold: 1000
    })
  }

  return result
}

// 生成 X 轴数据
const generateXAxisData = () => {
  const { data } = props
  if (!data) return []

  if (data.labels) {
    return data.labels
  }

  const length = dataPointsCount.value
  const xStart = data.xStart || 0
  const xInterval = data.xInterval || (1 / (data.sampleRate || 1000000))

  const result = []
  for (let i = 0; i < length; i++) {
    result.push((xStart + i * xInterval).toString())
  }
  return result
}

// 获取默认通道颜色
const getDefaultChannelColor = (index: number) => {
  const colors = [
    '#409eff', // 蓝色 - CH1
    '#67c23a', // 绿色 - CH2  
    '#e6a23c', // 橙色 - CH3
    '#f56c6c', // 红色 - CH4
    '#909399', // 灰色 - CH5
    '#c71585', // 深粉 - CH6
    '#ff8c00', // 深橙 - CH7
    '#32cd32'  // 青绿 - CH8
  ]
  return colors[index % colors.length]
}

// 格式化函数
const formatTime = (value: number) => {
  if (Math.abs(value) >= 1) {
    return `${value.toFixed(3)}s`
  } else if (Math.abs(value) >= 1e-3) {
    return `${(value * 1e3).toFixed(1)}ms`
  } else if (Math.abs(value) >= 1e-6) {
    return `${(value * 1e6).toFixed(1)}μs`
  } else {
    return `${(value * 1e9).toFixed(1)}ns`
  }
}

const formatVoltage = (value: number) => {
  if (Math.abs(value) >= 1) {
    return `${value.toFixed(3)}V`
  } else if (Math.abs(value) >= 1e-3) {
    return `${(value * 1e3).toFixed(1)}mV`
  } else if (Math.abs(value) >= 1e-6) {
    return `${(value * 1e6).toFixed(1)}μV`
  } else {
    return `${(value * 1e9).toFixed(1)}nV`
  }
}

const formatSampleRate = (rate: number) => {
  if (rate >= 1e9) {
    return `${(rate / 1e9).toFixed(1)}GS/s`
  } else if (rate >= 1e6) {
    return `${(rate / 1e6).toFixed(1)}MS/s`
  } else if (rate >= 1e3) {
    return `${(rate / 1e3).toFixed(1)}kS/s`
  } else {
    return `${rate}S/s`
  }
}

const formatMemoryUsage = (bytes: number) => {
  if (bytes >= 1024 * 1024) {
    return `${(bytes / (1024 * 1024)).toFixed(1)}MB`
  } else if (bytes >= 1024) {
    return `${(bytes / 1024).toFixed(1)}KB`
  } else {
    return `${bytes}B`
  }
}

const formatMeasurementValue = (value: number, unit: string) => {
  if (unit === 'Hz') {
    if (value >= 1e6) {
      return `${(value / 1e6).toFixed(3)}MHz`
    } else if (value >= 1e3) {
      return `${(value / 1e3).toFixed(3)}kHz`
    } else {
      return `${value.toFixed(3)}Hz`
    }
  } else if (unit === 'V') {
    return formatVoltage(value)
  } else if (unit === 's') {
    return formatTime(value)
  }
  return `${value.toFixed(3)}${unit}`
}

const formatCursorValue = (value: number, unit: string) => {
  if (unit === 's') {
    return formatTime(value)
  } else if (unit === 'Hz') {
    return formatMeasurementValue(value, 'Hz')
  }
  return `${value.toFixed(3)}${unit}`
}

// 控制方法
const resetZoom = () => {
  if (!chart.value) return
  chart.value.dispatchAction({
    type: 'dataZoom',
    start: 0,
    end: 100
  })
  isZoomed.value = false
}

const autoScale = () => {
  localOptions.value.autoScale = true
  updateChart()
}

const toggleRun = () => {
  isRunning.value = !isRunning.value
  emit('runStateChange', isRunning.value)
  
  if (isRunning.value) {
    triggerStatus.value = 'waiting'
  } else {
    triggerStatus.value = 'stopped'
  }
}

const singleTrigger = () => {
  if (isRunning.value) return
  
  triggerStatus.value = 'triggered'
  // 模拟单次触发
  setTimeout(() => {
    triggerStatus.value = 'waiting'
  }, 1000)
}

const updateTimebase = () => {
  // 更新时基逻辑
  updateChart()
}

const updateTrigger = () => {
  // 更新触发逻辑
  const triggerInfo = {
    time: Date.now(),
    level: triggerLevel.value,
    channel: 0
  }
  emit('trigger', triggerInfo)
}

const updateAcquisition = () => {
  // 更新采集模式逻辑
  updateChart()
}

const getTriggerStatusText = () => {
  switch (triggerStatus.value) {
    case 'waiting': return '等待触发'
    case 'triggered': return '已触发'
    case 'stopped': return '已停止'
    default: return '未知'
  }
}

const clearMeasurements = () => {
  measurements.value = []
}

// 导出功能
const exportChart = (format: 'png' | 'csv') => {
  if (!chart.value) return

  if (format === 'png') {
    const url = chart.value.getDataURL({
      type: 'png',
      pixelRatio: 2,
      backgroundColor: '#fff'
    })
    
    const link = document.createElement('a')
    link.download = `waveform_${Date.now()}.png`
    link.href = url
    link.click()
  } else if (format === 'csv') {
    exportToCSV()
  }
}

const exportToCSV = () => {
  if (!props.data) return

  let csvContent = 'Time,Voltage\n'
  const xData = generateXAxisData()
  
  if (Array.isArray(props.data.series[0])) {
    // 多系列数据
    const headers = ['Time', ...(props.data.series as number[][]).map((_, i) => `CH${i + 1}`)]
    csvContent = headers.join(',') + '\n'
    
    const maxLength = Math.max(...(props.data.series as number[][]).map(s => s.length))
    for (let i = 0; i < maxLength; i++) {
      const row = [xData[i]]
      ;(props.data.series as number[][]).forEach(series => {
        row.push(series[i]?.toString() || '')
      })
      csvContent += row.join(',') + '\n'
    }
  } else {
    // 单系列数据
    ;(props.data.series as number[]).forEach((value, index) => {
      csvContent += `${xData[index]},${value}\n`
    })
  }

  const blob = new Blob([csvContent], { type: 'text/csv' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `waveform_data_${Date.now()}.csv`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value
  nextTick(() => {
    chart.value?.resize()
  })
}

// 绑定事件
const bindEvents = () => {
  if (!chart.value) return

  // 缩放事件
  chart.value.on('dataZoom', (params: any) => {
    isZoomed.value = params.start > 0 || params.end < 100
    const range = {
      xMin: params.start,
      xMax: params.end,
      yMin: 0,
      yMax: 100
    }
    emit('zoom', range)
  })

  // 鼠标移动事件
  chart.value.on('mousemove', (params: any) => {
    if (params.componentType === 'series') {
      const position = { x: params.value[0], y: params.value[1] }
      emit('cursorMove', position)
    }
  })
}

// 监听数据变化
watch(() => props.data, () => {
  updateChart()
}, { deep: true })

// 监听配置变化
watch(() => props.options, (newOptions) => {
  if (newOptions) {
    localOptions.value = { ...localOptions.value, ...newOptions }
    updateChart()
  }
}, { deep: true })

// 监听容器大小变化
const resizeObserver = new ResizeObserver(() => {
  chart.value?.resize()
})

// 生命周期
onMounted(() => {
  initChart()
  if (chartContainer.value) {
    resizeObserver.observe(chartContainer.value)
  }
})

onUnmounted(() => {
  if (chart.value) {
    chart.value.dispose()
  }
  resizeObserver.disconnect()
})

// 暴露方法
defineExpose({
  resetZoom,
  autoScale,
  exportChart,
  updateChart,
  toggleRun,
  singleTrigger,
  getChart: () => chart.value
})
</script>

<style lang="scss" scoped>
.waveform-chart-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: var(--instrument-bg);
  border: 1px solid var(--instrument-border);
  border-radius: 8px;
  overflow: hidden;
  
  .chart-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    border-bottom: 1px solid var(--border-color);
    background: var(--surface-color);
    
    .toolbar-left,
    .toolbar-right {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .trigger-status {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
      
      .status-dot {
        width: 8px;
        height: 8px;
        border-radius: 50%;
        background: currentColor;
      }
      
      &.waiting {
        color: var(--warning-color);
        background: rgba(245, 158, 11, 0.1);
      }
      
      &.triggered {
        color: var(--success-color);
        background: rgba(16, 185, 129, 0.1);
      }
      
      &.stopped {
        color: var(--error-color);
        background: rgba(239, 68, 68, 0.1);
      }
    }
  }
  
  .chart-container {
    flex: 1;
    min-height: 300px;
    background: #ffffff;
    position: relative;
    
    &.fullscreen {
      position: fixed;
      top: 0;
      left: 0;
      width: 100vw !important;
      height: 100vh !important;
      z-index: 9999;
      background: #ffffff;
    }
  }
  
  .measurement-panel {
    border-top: 1px solid var(--border-color);
    background: var(--surface-color);
    
    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 8px 12px;
      border-bottom: 1px solid var(--border-color);
      font-size: 12px;
      font-weight: 600;
      color: var(--text-primary);
    }
    
    .measurements-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
      gap: 8px;
      padding: 8px;
      
      .measurement-item {
        padding: 6px 8px;
        border-left: 3px solid;
        background: var(--background-color);
        border-radius: 4px;
        
        .measurement-label {
          font-size: 10px;
          color: var(--text-secondary);
          margin-bottom: 2px;
        }
        
        .measurement-value {
          font-size: 12px;
          font-weight: 600;
          color: var(--text-primary);
          font-family: var(--font-family-mono);
        }
      }
    }
  }
  
  .cursor-info {
    padding: 8px 12px;
    border-top: 1px solid var(--border-color);
    background: var(--surface-color);
    
    .cursor-group {
      display: flex;
      gap: 16px;
      
      .cursor-item {
        display: flex;
        align-items: center;
        gap: 4px;
        font-size: 12px;
        
        .cursor-label {
          color: var(--text-secondary);
          font-weight: 500;
        }
        
        .cursor-value {
          color: var(--text-primary);
          font-family: var(--font-family-mono);
          font-weight: 600;
        }
      }
    }
  }
  
  .chart-controls {
    padding: 12px;
    border-top: 1px solid var(--border-color);
    background: var(--surface-color);
    
    .control-group {
      display: flex;
      flex-direction: column;
      gap: 6px;
      
      label {
        font-size: 12px;
        font-weight: 500;
        color: var(--text-primary);
      }
      
      .trigger-controls,
      .display-options,
      .acquisition-controls {
        display: flex;
        flex-direction: column;
        gap: 4px;
      }
      
      .display-options {
        .el-checkbox {
          margin-right: 8px;
        }
      }
      
      .sample-rate {
        font-size: 11px;
        color: var(--text-secondary);
        font-family: var(--font-family-mono);
        margin-top: 2px;
      }
    }
  }
  
  .chart-status {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 4px 12px;
    font-size: 11px;
    color: var(--text-secondary);
    background: var(--background-color);
    border-top: 1px solid var(--border-color);
    font-family: var(--font-family-mono);
    
    .status-left,
    .status-right {
      display: flex;
      gap: 12px;
    }
    
    span {
      white-space: nowrap;
    }
  }
}

// 专业仪器样式
.professional-instrument {
  box-shadow: 0 4px 12px var(--instrument-shadow);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  
  .chart-container {
    // EasyChart风格：简洁白色背景
    background: #ffffff;
    border: 1px solid #e0e6ed;
  }
}

// 响应式设计
@media (max-width: 1024px) {
  .waveform-chart-wrapper {
    .chart-controls {
      .control-group {
        margin-bottom: 12px;
      }
    }
  }
}

@media (max-width: 768px) {
  .waveform-chart-wrapper {
    .chart-toolbar {
      flex-direction: column;
      gap: 8px;
      align-items: stretch;
      
      .toolbar-left,
      .toolbar-right {
        justify-content: center;
      }
    }
    
    .measurements-grid {
      grid-template-columns: 1fr;
    }
    
    .cursor-info {
      .cursor-group {
        flex-direction: column;
        gap: 8px;
      }
    }
    
    .chart-status {
      flex-direction: column;
      align-items: flex-start;
      gap: 4px;
      
      .status-left,
      .status-right {
        flex-direction: column;
        gap: 4px;
      }
    }
  }
}

// 暗色主题适配
@media (prefers-color-scheme: dark) {
  .professional-instrument {
    .chart-container {
      background: radial-gradient(circle at center, #002200 0%, #001100 100%);
    }
  }
}
</style>
