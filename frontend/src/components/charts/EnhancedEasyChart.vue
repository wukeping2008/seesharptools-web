<template>
  <div class="enhanced-easy-chart-wrapper professional-control">
    <!-- 工具栏 -->
    <div class="chart-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <el-button-group>
          <el-button size="small" @click="resetZoom">
            <el-icon><Refresh /></el-icon>
            重置
          </el-button>
          <el-button size="small" @click="exportChart('png')">
            <el-icon><Download /></el-icon>
            导出图片
          </el-button>
          <el-button size="small" @click="exportChart('csv')">
            <el-icon><Document /></el-icon>
            导出CSV
          </el-button>
        </el-button-group>
        
        <el-divider direction="vertical" />
        
        <!-- FFT控制 -->
        <el-button-group>
          <el-button 
            size="small" 
            :type="showFFT ? 'primary' : 'default'"
            @click="toggleFFT"
          >
            <el-icon><TrendCharts /></el-icon>
            FFT分析
          </el-button>
          <el-button 
            size="small" 
            :disabled="!showFFT"
            @click="showFFTSettings = true"
          >
            <el-icon><Setting /></el-icon>
            FFT设置
          </el-button>
        </el-button-group>
        
        <el-divider direction="vertical" />
        
        <!-- 测量工具 -->
        <el-button-group>
          <el-button 
            size="small" 
            :type="measurementMode === 'cursor' ? 'primary' : 'default'"
            @click="toggleMeasurement('cursor')"
          >
            <el-icon><Aim /></el-icon>
            游标
          </el-button>
          <el-button 
            size="small" 
            :type="measurementMode === 'peak' ? 'primary' : 'default'"
            @click="toggleMeasurement('peak')"
          >
            <el-icon><Top /></el-icon>
            峰值
          </el-button>
        </el-button-group>
      </div>
      
      <div class="toolbar-right">
        <el-tooltip content="双Y轴显示">
          <el-button 
            size="small" 
            :type="dualYAxis ? 'primary' : 'default'"
            @click="toggleDualYAxis"
          >
            <el-icon><Grid /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="全屏显示">
          <el-button size="small" @click="toggleFullscreen">
            <el-icon><FullScreen /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>

    <!-- 图表容器 -->
    <div class="chart-content">
      <!-- 主图表 -->
      <div 
        ref="chartContainer" 
        class="chart-container main-chart"
        :class="{ fullscreen: isFullscreen, 'split-view': showFFT }"
      ></div>
      
      <!-- FFT图表 -->
      <div 
        v-if="showFFT"
        ref="fftContainer" 
        class="chart-container fft-chart"
      ></div>
    </div>

    <!-- 控制面板 -->
    <div class="chart-controls" v-if="showControls">
      <el-row :gutter="16">
        <el-col :span="6">
          <div class="control-group">
            <label>显示选项:</label>
            <div>
              <el-checkbox v-model="localOptions.autoScale" @change="updateChart">
                自动缩放
              </el-checkbox>
              <el-checkbox v-model="localOptions.legendVisible" @change="updateChart">
                显示图例
              </el-checkbox>
              <el-checkbox v-model="localOptions.gridEnabled" @change="updateChart">
                显示网格
              </el-checkbox>
            </div>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>坐标轴:</label>
            <div>
              <el-checkbox v-model="localOptions.logarithmic" @change="updateChart">
                对数坐标
              </el-checkbox>
              <el-checkbox v-model="dualYAxis" @change="updateChart">
                双Y轴
              </el-checkbox>
            </div>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>数学功能:</label>
            <div>
              <el-checkbox v-model="showStatistics" @change="updateStatistics">
                统计分析
              </el-checkbox>
              <el-checkbox v-model="showFFT" @change="toggleFFT">
                FFT分析
              </el-checkbox>
            </div>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>主题:</label>
            <el-select v-model="localOptions.theme" @change="updateChart" size="small">
              <el-option label="浅色" value="light" />
              <el-option label="深色" value="dark" />
              <el-option label="科学" value="scientific" />
            </el-select>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 测量结果显示 -->
    <div class="measurement-panel" v-if="showMeasurements">
      <div class="measurement-tabs">
        <el-tabs v-model="activeMeasurementTab" size="small">
          <el-tab-pane label="游标测量" name="cursor" v-if="cursorMeasurements.length > 0">
            <div class="measurement-results">
              <div v-for="(measurement, index) in cursorMeasurements" :key="index" class="measurement-item">
                <span class="measurement-label">游标{{ index + 1 }}:</span>
                <span class="measurement-value">
                  X={{ measurement.x.toFixed(3) }}, Y={{ measurement.y.toFixed(3) }}
                </span>
              </div>
            </div>
          </el-tab-pane>
          
          <el-tab-pane label="峰值检测" name="peak" v-if="peakMeasurements.length > 0">
            <div class="measurement-results">
              <div v-for="(peak, index) in peakMeasurements" :key="index" class="measurement-item">
                <span class="measurement-label">峰值{{ index + 1 }}:</span>
                <span class="measurement-value">
                  X={{ peak.x.toFixed(3) }}, Y={{ peak.y.toFixed(3) }}
                </span>
              </div>
            </div>
          </el-tab-pane>
          
          <el-tab-pane label="统计分析" name="statistics" v-if="showStatistics">
            <div class="measurement-results">
              <div class="measurement-item">
                <span class="measurement-label">平均值:</span>
                <span class="measurement-value">{{ statistics.mean.toFixed(3) }}</span>
              </div>
              <div class="measurement-item">
                <span class="measurement-label">RMS:</span>
                <span class="measurement-value">{{ statistics.rms.toFixed(3) }}</span>
              </div>
              <div class="measurement-item">
                <span class="measurement-label">标准差:</span>
                <span class="measurement-value">{{ statistics.std.toFixed(3) }}</span>
              </div>
              <div class="measurement-item">
                <span class="measurement-label">最大值:</span>
                <span class="measurement-value">{{ statistics.max.toFixed(3) }}</span>
              </div>
              <div class="measurement-item">
                <span class="measurement-label">最小值:</span>
                <span class="measurement-value">{{ statistics.min.toFixed(3) }}</span>
              </div>
            </div>
          </el-tab-pane>
        </el-tabs>
      </div>
    </div>

    <!-- 状态信息 -->
    <div class="chart-status" v-if="showStatus">
      <span>数据点数: {{ dataPointsCount }}</span>
      <span>系列数: {{ seriesCount }}</span>
      <span v-if="showFFT">FFT点数: {{ fftSize }}</span>
      <span v-if="cursorPosition">
        游标位置: X={{ cursorPosition.x.toFixed(3) }}, Y={{ cursorPosition.y.toFixed(3) }}
      </span>
    </div>

    <!-- FFT设置对话框 -->
    <el-dialog v-model="showFFTSettings" title="FFT分析设置" width="500px">
      <el-form :model="fftConfig" label-width="120px">
        <el-form-item label="FFT大小:">
          <el-select v-model="fftConfig.size">
            <el-option label="512" :value="512" />
            <el-option label="1024" :value="1024" />
            <el-option label="2048" :value="2048" />
            <el-option label="4096" :value="4096" />
            <el-option label="8192" :value="8192" />
          </el-select>
        </el-form-item>
        
        <el-form-item label="窗函数:">
          <el-select v-model="fftConfig.window">
            <el-option label="矩形窗" value="rectangular" />
            <el-option label="汉宁窗" value="hanning" />
            <el-option label="汉明窗" value="hamming" />
            <el-option label="布莱克曼窗" value="blackman" />
            <el-option label="凯泽窗" value="kaiser" />
          </el-select>
        </el-form-item>
        
        <el-form-item label="重叠率:">
          <el-slider 
            v-model="fftConfig.overlap" 
            :min="0" 
            :max="75" 
            :step="25"
            show-stops
            show-input
          />
          <span class="form-help">%</span>
        </el-form-item>
        
        <el-form-item label="显示模式:">
          <el-radio-group v-model="fftConfig.displayMode">
            <el-radio label="magnitude">幅度谱</el-radio>
            <el-radio label="power">功率谱</el-radio>
            <el-radio label="phase">相位谱</el-radio>
          </el-radio-group>
        </el-form-item>
        
        <el-form-item label="频率单位:">
          <el-radio-group v-model="fftConfig.frequencyUnit">
            <el-radio label="Hz">Hz</el-radio>
            <el-radio label="kHz">kHz</el-radio>
            <el-radio label="MHz">MHz</el-radio>
          </el-radio-group>
        </el-form-item>
      </el-form>
      
      <template #footer>
        <el-button @click="showFFTSettings = false">取消</el-button>
        <el-button type="primary" @click="applyFFTSettings">应用</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed, nextTick } from 'vue'
import * as echarts from 'echarts'
import { 
  Refresh, Download, Document, FullScreen, TrendCharts, 
  Setting, Aim, Top, Grid 
} from '@element-plus/icons-vue'
import type { 
  ChartData, 
  ChartOptions, 
  SeriesConfig, 
  ExportOptions,
  ChartEvents 
} from '@/types/chart'

// FFT相关类型
interface FFTConfig {
  size: number
  window: 'rectangular' | 'hanning' | 'hamming' | 'blackman' | 'kaiser'
  overlap: number
  displayMode: 'magnitude' | 'power' | 'phase'
  frequencyUnit: 'Hz' | 'kHz' | 'MHz'
}

interface MeasurementPoint {
  x: number
  y: number
  seriesIndex?: number
}

interface Statistics {
  mean: number
  rms: number
  std: number
  max: number
  min: number
}

// Props
interface Props {
  data?: ChartData
  options?: Partial<ChartOptions>
  seriesConfigs?: SeriesConfig[]
  showToolbar?: boolean
  showControls?: boolean
  showStatus?: boolean
  width?: string | number
  height?: string | number
  events?: ChartEvents
  sampleRate?: number // 采样率，用于FFT频率计算
}

const props = withDefaults(defineProps<Props>(), {
  showToolbar: true,
  showControls: true,
  showStatus: true,
  width: '100%',
  height: '400px',
  sampleRate: 1000 // 默认1kHz采样率
})

// Emits
const emit = defineEmits<{
  dataUpdate: [data: ChartData]
  zoom: [range: { xMin: number; xMax: number; yMin: number; yMax: number }]
  cursorMove: [position: { x: number; y: number }]
  seriesToggle: [seriesIndex: number, visible: boolean]
  fftUpdate: [fftData: { frequencies: number[]; magnitudes: number[] }]
  measurementUpdate: [measurements: MeasurementPoint[]]
}>()

// 响应式数据
const chartContainer = ref<HTMLDivElement>()
const fftContainer = ref<HTMLDivElement>()
const chart = ref<echarts.ECharts>()
const fftChart = ref<echarts.ECharts>()
const isFullscreen = ref(false)
const cursorPosition = ref<{ x: number; y: number }>()

// FFT相关状态
const showFFT = ref(false)
const showFFTSettings = ref(false)
const fftConfig = ref<FFTConfig>({
  size: 1024,
  window: 'hanning',
  overlap: 50,
  displayMode: 'magnitude',
  frequencyUnit: 'Hz'
})

// 测量相关状态
const measurementMode = ref<'none' | 'cursor' | 'peak'>('none')
const cursorMeasurements = ref<MeasurementPoint[]>([])
const peakMeasurements = ref<MeasurementPoint[]>([])
const showStatistics = ref(false)
const showMeasurements = ref(false)
const activeMeasurementTab = ref('cursor')

// 其他状态
const dualYAxis = ref(false)

// 默认配置
const defaultOptions: ChartOptions = {
  autoScale: true,
  logarithmic: false,
  splitView: false,
  legendVisible: true,
  cursorMode: 'zoom',
  gridEnabled: true,
  minorGridEnabled: false,
  theme: 'light'
}

const localOptions = ref<ChartOptions>({ ...defaultOptions, ...props.options })

// 计算属性
const dataPointsCount = computed(() => {
  if (!props.data?.series) return 0
  if (Array.isArray(props.data.series[0])) {
    return (props.data.series as number[][])[0]?.length || 0
  }
  return (props.data.series as number[]).length
})

const seriesCount = computed(() => {
  if (!props.data?.series) return 0
  if (Array.isArray(props.data.series[0])) {
    return (props.data.series as number[][]).length
  }
  return 1
})

const fftSize = computed(() => fftConfig.value.size)

const statistics = computed<Statistics>(() => {
  if (!props.data?.series) {
    return { mean: 0, rms: 0, std: 0, max: 0, min: 0 }
  }

  let data: number[] = []
  if (Array.isArray(props.data.series[0])) {
    // 使用第一个系列的数据
    data = (props.data.series as number[][])[0] || []
  } else {
    data = props.data.series as number[]
  }

  if (data.length === 0) {
    return { mean: 0, rms: 0, std: 0, max: 0, min: 0 }
  }

  const mean = data.reduce((sum, val) => sum + val, 0) / data.length
  const rms = Math.sqrt(data.reduce((sum, val) => sum + val * val, 0) / data.length)
  const variance = data.reduce((sum, val) => sum + Math.pow(val - mean, 2), 0) / data.length
  const std = Math.sqrt(variance)
  const max = Math.max(...data)
  const min = Math.min(...data)

  return { mean, rms, std, max, min }
})

// FFT计算函数
const computeFFT = (data: number[]): { frequencies: number[]; magnitudes: number[] } => {
  const size = Math.min(fftConfig.value.size, data.length)
  const windowedData = applyWindow(data.slice(0, size), fftConfig.value.window)
  
  // 简化的FFT实现（实际项目中应使用专业的FFT库）
  const frequencies: number[] = []
  const magnitudes: number[] = []
  
  const sampleRate = props.sampleRate
  const frequencyResolution = sampleRate / size
  
  for (let k = 0; k < size / 2; k++) {
    let realSum = 0
    let imagSum = 0
    
    for (let n = 0; n < size; n++) {
      const angle = -2 * Math.PI * k * n / size
      realSum += windowedData[n] * Math.cos(angle)
      imagSum += windowedData[n] * Math.sin(angle)
    }
    
    const magnitude = Math.sqrt(realSum * realSum + imagSum * imagSum)
    const frequency = k * frequencyResolution
    
    frequencies.push(frequency)
    
    // 根据显示模式计算幅度
    switch (fftConfig.value.displayMode) {
      case 'magnitude':
        magnitudes.push(magnitude)
        break
      case 'power':
        magnitudes.push(magnitude * magnitude)
        break
      case 'phase':
        magnitudes.push(Math.atan2(imagSum, realSum) * 180 / Math.PI)
        break
    }
  }
  
  return { frequencies, magnitudes }
}

// 应用窗函数
const applyWindow = (data: number[], windowType: string): number[] => {
  const size = data.length
  const windowed = [...data]
  
  switch (windowType) {
    case 'hanning':
      for (let i = 0; i < size; i++) {
        const window = 0.5 * (1 - Math.cos(2 * Math.PI * i / (size - 1)))
        windowed[i] *= window
      }
      break
    case 'hamming':
      for (let i = 0; i < size; i++) {
        const window = 0.54 - 0.46 * Math.cos(2 * Math.PI * i / (size - 1))
        windowed[i] *= window
      }
      break
    case 'blackman':
      for (let i = 0; i < size; i++) {
        const window = 0.42 - 0.5 * Math.cos(2 * Math.PI * i / (size - 1)) + 
                      0.08 * Math.cos(4 * Math.PI * i / (size - 1))
        windowed[i] *= window
      }
      break
    case 'rectangular':
    default:
      // 不应用窗函数
      break
  }
  
  return windowed
}

// 峰值检测
const detectPeaks = (data: number[], threshold = 0.1): MeasurementPoint[] => {
  const peaks: MeasurementPoint[] = []
  const xData = generateXAxisData()
  
  for (let i = 1; i < data.length - 1; i++) {
    if (data[i] > data[i - 1] && data[i] > data[i + 1] && data[i] > threshold) {
      peaks.push({
        x: parseFloat(xData[i]),
        y: data[i]
      })
    }
  }
  
  // 按幅度排序，取前10个峰值
  return peaks.sort((a, b) => b.y - a.y).slice(0, 10)
}

// 初始化图表
const initChart = () => {
  if (!chartContainer.value) return

  // 销毁现有图表
  if (chart.value) {
    chart.value.dispose()
  }

  // 创建主图表
  chart.value = echarts.init(chartContainer.value, localOptions.value.theme)
  
  // 设置初始配置
  updateChart()
  
  // 绑定事件
  bindEvents()
  
  // 初始化FFT图表
  if (showFFT.value) {
    initFFTChart()
  }
}

// 初始化FFT图表
const initFFTChart = () => {
  if (!fftContainer.value) return

  if (fftChart.value) {
    fftChart.value.dispose()
  }

  fftChart.value = echarts.init(fftContainer.value, localOptions.value.theme)
  updateFFTChart()
}

// 更新主图表
const updateChart = () => {
  if (!chart.value || !props.data) return

  const option = generateEChartsOption()
  chart.value.setOption(option, true)
  
  // 更新FFT
  if (showFFT.value) {
    updateFFTChart()
  }
  
  // 更新测量
  updateMeasurements()
}

// 更新FFT图表
const updateFFTChart = () => {
  if (!fftChart.value || !props.data) return

  let data: number[] = []
  if (Array.isArray(props.data.series[0])) {
    data = (props.data.series as number[][])[0] || []
  } else {
    data = props.data.series as number[]
  }

  if (data.length === 0) return

  const fftResult = computeFFT(data)
  
  // 转换频率单位
  const frequencies = fftResult.frequencies.map(freq => {
    switch (fftConfig.value.frequencyUnit) {
      case 'kHz': return freq / 1000
      case 'MHz': return freq / 1000000
      default: return freq
    }
  })

  const option = {
    title: {
      text: `FFT分析 (${fftConfig.value.displayMode})`,
      left: 'center',
      textStyle: { fontSize: 14 }
    },
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        const freq = params[0].axisValue
        const mag = params[0].value
        return `频率: ${freq} ${fftConfig.value.frequencyUnit}<br/>幅度: ${mag.toFixed(3)}`
      }
    },
    grid: {
      left: '10%',
      right: '10%',
      bottom: '15%',
      top: '20%'
    },
    xAxis: {
      type: 'category',
      data: frequencies.map(f => f.toFixed(2)),
      name: `频率 (${fftConfig.value.frequencyUnit})`,
      nameLocation: 'middle',
      nameGap: 30
    },
    yAxis: {
      type: 'value',
      name: getYAxisLabel(),
      nameLocation: 'middle',
      nameGap: 50,
      scale: true
    },
    series: [{
      name: 'FFT',
      type: 'line',
      data: fftResult.magnitudes,
      lineStyle: { width: 1 },
      symbol: 'none',
      sampling: 'lttb'
    }]
  }

  fftChart.value.setOption(option, true)
  
  // 发射FFT更新事件
  emit('fftUpdate', fftResult)
}

// 获取Y轴标签
const getYAxisLabel = (): string => {
  switch (fftConfig.value.displayMode) {
    case 'magnitude': return '幅度'
    case 'power': return '功率'
    case 'phase': return '相位 (度)'
    default: return '幅度'
  }
}

// 生成 ECharts 配置
const generateEChartsOption = () => {
  const { data } = props
  if (!data) return {}

  const series = processSeriesData(data)
  
  return {
    title: {
      show: false
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: measurementMode.value === 'cursor' ? 'cross' : 'shadow'
      },
      formatter: (params: any) => {
        let result = `X: ${params[0].axisValue}<br/>`
        params.forEach((param: any) => {
          result += `${param.seriesName}: ${param.value}<br/>`
        })
        return result
      }
    },
    legend: {
      show: localOptions.value.legendVisible,
      top: 10,
      type: 'scroll'
    },
    grid: {
      left: '10%',
      right: dualYAxis.value ? '15%' : '10%',
      bottom: '15%',
      top: localOptions.value.legendVisible ? '15%' : '10%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: generateXAxisData(),
      axisLine: { show: true },
      axisTick: { show: true },
      axisLabel: { show: true },
      splitLine: { 
        show: localOptions.value.gridEnabled,
        lineStyle: { type: 'solid', color: '#e0e6ed' }
      }
    },
    yAxis: dualYAxis.value ? [
      {
        type: localOptions.value.logarithmic ? 'log' : 'value',
        scale: !localOptions.value.autoScale,
        position: 'left',
        axisLine: { show: true },
        axisTick: { show: true },
        axisLabel: { show: true },
        splitLine: { 
          show: localOptions.value.gridEnabled,
          lineStyle: { type: 'solid', color: '#e0e6ed' }
        }
      },
      {
        type: localOptions.value.logarithmic ? 'log' : 'value',
        scale: !localOptions.value.autoScale,
        position: 'right',
        axisLine: { show: true },
        axisTick: { show: true },
        axisLabel: { show: true },
        splitLine: { show: false }
      }
    ] : {
      type: localOptions.value.logarithmic ? 'log' : 'value',
      scale: !localOptions.value.autoScale,
      axisLine: { show: true },
      axisTick: { show: true },
      axisLabel: { show: true },
      splitLine: { 
        show: localOptions.value.gridEnabled,
        lineStyle: { type: 'solid', color: '#e0e6ed' }
      }
    },
    dataZoom: [
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
    ],
    series
  }
}

// 处理系列数据
const processSeriesData = (data: ChartData) => {
  const { series } = data
  const result: any[] = []

  if (Array.isArray(series[0])) {
    // 多系列数据
    (series as number[][]).forEach((seriesData, index) => {
      const config = props.seriesConfigs?.[index]
      result.push({
        name: config?.name || `系列 ${index + 1}`,
        type: 'line',
        data: seriesData,
        yAxisIndex: dualYAxis.value && index % 2 === 1 ? 1 : 0,
        lineStyle: {
          color: config?.color || getDefaultColor(index),
          width: config?.lineWidth || 2,
          type: config?.lineType || 'solid'
        },
        itemStyle: {
          color: config?.color || getDefaultColor(index)
        },
        symbol: getMarkerSymbol(config?.markerType || 'circle'),
        symbolSize: config?.markerSize || 4,
        showSymbol: config?.markerType !== 'none',
        smooth: false,
        connectNulls: false
      })
    })
  } else {
    // 单系列数据
    const config = props.seriesConfigs?.[0]
    result.push({
      name: config?.name || '数据',
      type: 'line',
      data: series as number[],
      lineStyle: {
        color: config?.color || '#409eff',
        width: config?.lineWidth || 2,
        type: config?.lineType || 'solid'
      },
      itemStyle: {
        color: config?.color || '#409eff'
      },
      symbol: getMarkerSymbol(config?.markerType || 'circle'),
      symbolSize: config?.markerSize || 4,
      showSymbol: config?.markerType !== 'none',
      smooth: false,
      connectNulls: false
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
  const xInterval = data.xInterval || 1

  return Array.from({ length }, (_, i) => (xStart + i * xInterval).toString())
}

// 获取默认颜色
const getDefaultColor = (index: number) => {
  const colors = [
    '#409eff', '#67c23a', '#e6a23c', '#f56c6c', 
    '#909399', '#c71585', '#ff8c00', '#32cd32'
  ]
  return colors[index % colors.length]
}

// 获取标记符号
const getMarkerSymbol = (type: string) => {
  const symbolMap: Record<string, string> = {
    'none': 'none',
    'circle': 'circle',
    'square': 'rect',
    'triangle': 'triangle',
    'diamond': 'diamond'
  }
  return symbolMap[type] || 'circle'
}

// 绑定事件
const bindEvents = () => {
  if (!chart.value) return

  // 缩放事件
  chart.value.on('dataZoom', (params: any) => {
    const range = {
      xMin: params.start,
      xMax: params.end,
      yMin: 0,
      yMax: 100
    }
    emit('zoom', range)
    props.events?.onZoom?.(range)
  })

  // 鼠标移动事件
  chart.value.on('mousemove', (params: any) => {
    if (params.componentType === 'series') {
      const position = { x: params.value[0], y: params.value[1] }
      cursorPosition.value = position
      emit('cursorMove', position)
      props.events?.onCursorMove?.(position)
    }
  })

  // 图例点击事件
  chart.value.on('legendselectchanged', (params: any) => {
    Object.keys(params.selected).forEach((name, index) => {
      const visible = params.selected[name]
      emit('seriesToggle', index, visible)
      props.events?.onSeriesToggle?.(index, visible)
    })
  })
}

// 更新测量
const updateMeasurements = () => {
  if (!props.data) return

  let data: number[] = []
  if (Array.isArray(props.data.series[0])) {
    data = (props.data.series as number[][])[0] || []
  } else {
    data = props.data.series as number[]
  }

  if (measurementMode.value === 'peak') {
    peakMeasurements.value = detectPeaks(data)
    showMeasurements.value = peakMeasurements.value.length > 0
    activeMeasurementTab.value = 'peak'
  }

  emit('measurementUpdate', [...cursorMeasurements.value, ...peakMeasurements.value])
}

// 更新统计
const updateStatistics = () => {
  showMeasurements.value = showStatistics.value
  if (showStatistics.value) {
    activeMeasurementTab.value = 'statistics'
  }
}

// 切换FFT
const toggleFFT = () => {
  showFFT.value = !showFFT.value
  
  if (showFFT.value) {
    nextTick(() => {
      initFFTChart()
    })
  } else {
    if (fftChart.value) {
      fftChart.value.dispose()
      fftChart.value = undefined
    }
  }
}

// 切换测量模式
const toggleMeasurement = (mode: 'cursor' | 'peak') => {
  if (measurementMode.value === mode) {
    measurementMode.value = 'none'
    showMeasurements.value = false
  } else {
    measurementMode.value = mode
    updateMeasurements()
  }
}

// 切换双Y轴
const toggleDualYAxis = () => {
  dualYAxis.value = !dualYAxis.value
  updateChart()
}

// 应用FFT设置
const applyFFTSettings = () => {
  showFFTSettings.value = false
  if (showFFT.value) {
    updateFFTChart()
  }
}

// 重置缩放
const resetZoom = () => {
  if (!chart.value) return
  chart.value.dispatchAction({
    type: 'dataZoom',
    start: 0,
    end: 100
  })
}

// 导出图表
const exportChart = (format: 'png' | 'csv') => {
  if (!chart.value) return

  if (format === 'png') {
    const url = chart.value.getDataURL({
      type: 'png',
      pixelRatio: 2,
      backgroundColor: '#fff'
    })
    
    const link = document.createElement('a')
    link.download = `enhanced_chart_${Date.now()}.png`
    link.href = url
    link.click()
  } else if (format === 'csv') {
    exportToCSV()
  }
}

// 导出 CSV
const exportToCSV = () => {
  if (!props.data) return

  let csvContent = 'X,Y\n'
  const xData = generateXAxisData()
  
  if (Array.isArray(props.data.series[0])) {
    // 多系列数据
    const headers = ['X', ...(props.data.series as number[][]).map((_, i) => `Series${i + 1}`)]
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
  link.download = `enhanced_chart_data_${Date.now()}.csv`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

// 切换全屏
const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value
  nextTick(() => {
    chart.value?.resize()
    fftChart.value?.resize()
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
  fftChart.value?.resize()
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
  if (fftChart.value) {
    fftChart.value.dispose()
  }
  resizeObserver.disconnect()
})

// 暴露方法
defineExpose({
  resetZoom,
  exportChart,
  updateChart,
  toggleFFT,
  getChart: () => chart.value,
  getFFTChart: () => fftChart.value
})
</script>

<style lang="scss" scoped>
.enhanced-easy-chart-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  
  .chart-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    border-bottom: 1px solid #e4e7ed;
    background: #fafafa;
    
    .toolbar-left,
    .toolbar-right {
      display: flex;
      align-items: center;
      gap: 8px;
    }
  }
  
  .chart-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    
    .chart-container {
      flex: 1;
      min-height: 200px;
      
      &.main-chart.split-view {
        flex: 0.6;
      }
      
      &.fft-chart {
        flex: 0.4;
        border-top: 1px solid #e4e7ed;
      }
      
      &.fullscreen {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw !important;
        height: 100vh !important;
        z-index: 9999;
        background: white;
      }
    }
  }
  
  .chart-controls {
    padding: 12px;
    border-top: 1px solid #e4e7ed;
    background: #fafafa;
    
    .control-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
      
      label {
        font-size: 12px;
        font-weight: 500;
        color: #606266;
      }
      
      > div {
        display: flex;
        flex-direction: column;
        gap: 4px;
      }
    }
  }
  
  .measurement-panel {
    border-top: 1px solid #e4e7ed;
    background: #f8f9fa;
    
    .measurement-tabs {
      padding: 8px 12px;
      
      .measurement-results {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 8px;
        
        .measurement-item {
          display: flex;
          justify-content: space-between;
          align-items: center;
          padding: 4px 8px;
          background: white;
          border-radius: 4px;
          border: 1px solid #e4e7ed;
          
          .measurement-label {
            font-size: 12px;
            color: #606266;
            font-weight: 500;
          }
          
          .measurement-value {
            font-size: 12px;
            color: #409eff;
            font-family: 'Consolas', 'Monaco', monospace;
          }
        }
      }
    }
  }
  
  .chart-status {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 4px 12px;
    font-size: 12px;
    color: #909399;
    background: #f5f7fa;
    border-top: 1px solid #e4e7ed;
    
    span {
      margin-right: 16px;
      
      &:last-child {
        margin-right: 0;
      }
    }
  }
}

.form-help {
  font-size: 12px;
  color: #909399;
  margin-left: 8px;
}

@media (max-width: 768px) {
  .enhanced-easy-chart-wrapper {
    .chart-toolbar {
      flex-direction: column;
      gap: 8px;
      
      .toolbar-left,
      .toolbar-right {
        width: 100%;
        justify-content: center;
      }
    }
    
    .chart-controls {
      .control-group {
        margin-bottom: 12px;
      }
    }
    
    .chart-status {
      flex-direction: column;
      align-items: flex-start;
      gap: 4px;
      
      span {
        margin-right: 0;
      }
    }
    
    .measurement-panel {
      .measurement-results {
        grid-template-columns: 1fr;
      }
    }
  }
}
</style>
