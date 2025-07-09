<template>
  <div class="spectrum-chart-wrapper professional-control">
    <!-- 工具栏 -->
    <div class="chart-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <el-button-group>
          <el-button size="small" @click="resetZoom">
            <el-icon><Refresh /></el-icon>
            重置
          </el-button>
          <el-button size="small" @click="toggleFFTMode" :type="isFFTMode ? 'primary' : ''">
            <el-icon><DataAnalysis /></el-icon>
            {{ isFFTMode ? '时域' : 'FFT' }}
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
      </div>
      
      <div class="toolbar-right">
        <el-tooltip content="FFT设置">
          <el-button size="small" @click="showFFTSettings = true" v-if="isFFTMode">
            <el-icon><Setting /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="峰值检测">
          <el-button size="small" @click="togglePeakDetection" v-if="isFFTMode" :type="peakDetectionEnabled ? 'primary' : ''">
            <el-icon><TrendCharts /></el-icon>
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
    <div 
      ref="chartContainer" 
      class="chart-container"
      :class="{ fullscreen: isFullscreen }"
    ></div>

    <!-- FFT控制面板 -->
    <div class="fft-controls" v-if="showControls && isFFTMode">
      <el-row :gutter="16">
        <el-col :span="4">
          <div class="control-group">
            <label>窗函数:</label>
            <el-select v-model="fftOptions.windowType" @change="updateFFT" size="small">
              <el-option label="矩形窗" value="rectangular" />
              <el-option label="汉宁窗" value="hanning" />
              <el-option label="汉明窗" value="hamming" />
              <el-option label="布莱克曼窗" value="blackman" />
              <el-option label="Kaiser窗" value="kaiser" />
              <el-option label="平顶窗" value="flattop" />
            </el-select>
          </div>
        </el-col>
        
        <el-col :span="4">
          <div class="control-group">
            <label>FFT大小:</label>
            <el-select v-model="fftOptions.windowSize" @change="updateFFT" size="small">
              <el-option label="512" :value="512" />
              <el-option label="1024" :value="1024" />
              <el-option label="2048" :value="2048" />
              <el-option label="4096" :value="4096" />
              <el-option label="8192" :value="8192" />
            </el-select>
          </div>
        </el-col>
        
        <el-col :span="4">
          <div class="control-group">
            <label>平均次数:</label>
            <el-input-number 
              v-model="fftOptions.averageCount" 
              :min="1" 
              :max="100" 
              @change="updateFFT"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="4">
          <div class="control-group">
            <label>显示模式:</label>
            <el-select v-model="displayMode" @change="updateChart" size="small">
              <el-option label="幅度谱" value="magnitude" />
              <el-option label="功率谱密度" value="psd" />
              <el-option label="相位谱" value="phase" />
              <el-option label="dB幅度" value="magnitude_db" />
            </el-select>
          </div>
        </el-col>
        
        <el-col :span="4">
          <div class="control-group">
            <label>Y轴刻度:</label>
            <el-select v-model="yAxisScale" @change="updateChart" size="small">
              <el-option label="线性" value="linear" />
              <el-option label="对数" value="log" />
            </el-select>
          </div>
        </el-col>
        
        <el-col :span="4">
          <div class="control-group">
            <label>采样率 (Hz):</label>
            <el-input-number 
              v-model="fftOptions.sampleRate" 
              :min="1" 
              :max="1000000000"
              @change="updateFFT"
              size="small"
            />
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 常规控制面板 -->
    <div class="chart-controls" v-if="showControls && !isFFTMode">
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
              <el-checkbox v-model="localOptions.gridEnabled" @change="updateChart">
                显示网格
              </el-checkbox>
            </div>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>交互模式:</label>
            <el-select v-model="localOptions.cursorMode" @change="updateChart" size="small">
              <el-option label="缩放" value="zoom" />
              <el-option label="游标" value="cursor" />
              <el-option label="禁用" value="disabled" />
            </el-select>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>主题:</label>
            <el-select v-model="localOptions.theme" @change="updateChart" size="small">
              <el-option label="浅色" value="light" />
              <el-option label="深色" value="dark" />
            </el-select>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 状态信息 -->
    <div class="chart-status" v-if="showStatus">
      <div class="status-left">
        <span v-if="!isFFTMode">数据点数: {{ dataPointsCount }}</span>
        <span v-if="!isFFTMode">系列数: {{ seriesCount }}</span>
        <span v-if="isFFTMode && fftResult">峰值频率: {{ fftResult.peakFrequency.toFixed(2) }} Hz</span>
        <span v-if="isFFTMode && fftResult">峰值幅度: {{ fftResult.peakMagnitude.toFixed(3) }}</span>
        <span v-if="isFFTMode && fftResult">总功率: {{ fftResult.totalPower.toFixed(6) }}</span>
        <span v-if="isFFTMode && fftResult">SNR: {{ fftResult.snr.toFixed(1) }} dB</span>
      </div>
      <div class="status-right">
        <span v-if="cursorPosition">
          游标位置: X={{ cursorPosition.x.toFixed(3) }}, Y={{ cursorPosition.y.toFixed(3) }}
        </span>
        <span v-if="isFFTMode && thdValue !== null">THD: {{ thdValue.toFixed(2) }}%</span>
      </div>
    </div>

    <!-- 峰值列表 -->
    <div class="peaks-panel" v-if="isFFTMode && peakDetectionEnabled && detectedPeaks.length > 0">
      <el-card class="peaks-card">
        <template #header>
          <span>检测到的峰值</span>
        </template>
        <el-table :data="detectedPeaks.slice(0, 10)" size="small" style="width: 100%">
          <el-table-column prop="frequency" label="频率 (Hz)" width="120">
            <template #default="scope">
              {{ scope.row.frequency.toFixed(2) }}
            </template>
          </el-table-column>
          <el-table-column prop="magnitude" label="幅度" width="100">
            <template #default="scope">
              {{ scope.row.magnitude.toFixed(3) }}
            </template>
          </el-table-column>
          <el-table-column label="dB" width="100">
            <template #default="scope">
              {{ (20 * Math.log10(scope.row.magnitude)).toFixed(1) }}
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </div>

    <!-- FFT设置对话框 -->
    <el-dialog v-model="showFFTSettings" title="FFT设置" width="600px">
      <el-form :model="fftOptions" label-width="120px">
        <el-form-item label="采样率 (Hz)">
          <el-input-number v-model="fftOptions.sampleRate" :min="1" :max="1000000000" />
        </el-form-item>
        <el-form-item label="窗函数类型">
          <el-select v-model="fftOptions.windowType">
            <el-option label="矩形窗" value="rectangular" />
            <el-option label="汉宁窗" value="hanning" />
            <el-option label="汉明窗" value="hamming" />
            <el-option label="布莱克曼窗" value="blackman" />
            <el-option label="Kaiser窗" value="kaiser" />
            <el-option label="平顶窗" value="flattop" />
          </el-select>
        </el-form-item>
        <el-form-item label="FFT大小">
          <el-select v-model="fftOptions.windowSize">
            <el-option label="512" :value="512" />
            <el-option label="1024" :value="1024" />
            <el-option label="2048" :value="2048" />
            <el-option label="4096" :value="4096" />
            <el-option label="8192" :value="8192" />
          </el-select>
        </el-form-item>
        <el-form-item label="重叠比例">
          <el-slider v-model="fftOptions.overlap" :min="0" :max="0.9" :step="0.1" />
        </el-form-item>
        <el-form-item label="平均次数">
          <el-input-number v-model="fftOptions.averageCount" :min="1" :max="100" />
        </el-form-item>
        <el-form-item label="零填充">
          <el-switch v-model="fftOptions.enableZeroPadding" />
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
  Refresh, Download, Document, FullScreen, DataAnalysis, 
  Setting, TrendCharts 
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import type { 
  ChartData, 
  ChartOptions, 
  SeriesConfig, 
  ExportOptions,
  ChartEvents 
} from '@/types/chart'
import { FFTAnalyzer, type FFTOptions, type FFTResult, type PeakInfo } from '@/utils/signal/FFTAnalyzer'

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
  enableFFT?: boolean
  defaultSampleRate?: number
}

const props = withDefaults(defineProps<Props>(), {
  showToolbar: true,
  showControls: true,
  showStatus: true,
  width: '100%',
  height: '400px',
  enableFFT: true,
  defaultSampleRate: 1000
})

// Emits
const emit = defineEmits<{
  dataUpdate: [data: ChartData]
  zoom: [range: { xMin: number; xMax: number; yMin: number; yMax: number }]
  cursorMove: [position: { x: number; y: number }]
  seriesToggle: [seriesIndex: number, visible: boolean]
  fftResult: [result: FFTResult]
  peaksDetected: [peaks: PeakInfo[]]
}>()

// 响应式数据
const chartContainer = ref<HTMLDivElement>()
const chart = ref<echarts.ECharts>()
const isFullscreen = ref(false)
const cursorPosition = ref<{ x: number; y: number }>()
const isFFTMode = ref(false)
const showFFTSettings = ref(false)
const peakDetectionEnabled = ref(false)

// FFT相关
const fftAnalyzer = ref<FFTAnalyzer>()
const fftResult = ref<FFTResult>()
const detectedPeaks = ref<PeakInfo[]>([])
const thdValue = ref<number | null>(null)

// 显示模式
const displayMode = ref<'magnitude' | 'psd' | 'phase' | 'magnitude_db'>('magnitude')
const yAxisScale = ref<'linear' | 'log'>('linear')

// FFT配置
const fftOptions = ref<FFTOptions>({
  sampleRate: props.defaultSampleRate,
  windowType: 'hanning',
  windowSize: 1024,
  overlap: 0.5,
  averageCount: 4,
  enableZeroPadding: true
})

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

// 初始化FFT分析器
const initFFTAnalyzer = () => {
  fftAnalyzer.value = new FFTAnalyzer(fftOptions.value)
}

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

// 切换FFT模式
const toggleFFTMode = () => {
  isFFTMode.value = !isFFTMode.value
  
  if (isFFTMode.value && !fftAnalyzer.value) {
    initFFTAnalyzer()
  }
  
  updateChart()
  
  ElMessage.info(`已切换到${isFFTMode.value ? 'FFT频域' : '时域'}模式`)
}

// 切换峰值检测
const togglePeakDetection = () => {
  peakDetectionEnabled.value = !peakDetectionEnabled.value
  
  if (peakDetectionEnabled.value && isFFTMode.value) {
    performPeakDetection()
  }
  
  updateChart()
}

// 执行FFT分析
const performFFT = () => {
  if (!props.data || !fftAnalyzer.value || !isFFTMode.value) return null

  try {
    // 获取第一个系列的数据进行FFT分析
    let signalData: number[]
    if (Array.isArray(props.data.series[0])) {
      signalData = (props.data.series as number[][])[0] || []
    } else {
      signalData = props.data.series as number[]
    }

    if (signalData.length === 0) return null

    // 转换为Float32Array
    const signal = new Float32Array(signalData)
    
    // 执行FFT分析
    const result = fftAnalyzer.value.analyze(signal)
    fftResult.value = result
    
    // 发射FFT结果事件
    emit('fftResult', result)
    
    // 如果启用峰值检测，执行峰值检测
    if (peakDetectionEnabled.value) {
      performPeakDetection()
    }
    
    // 计算THD
    if (result.peakFrequency > 0) {
      thdValue.value = fftAnalyzer.value.calculateTHD(
        result.magnitudes, 
        result.frequencies, 
        result.peakFrequency
      )
    }
    
    return result
  } catch (error) {
    console.error('FFT分析失败:', error)
    ElMessage.error('FFT分析失败')
    return null
  }
}

// 执行峰值检测
const performPeakDetection = () => {
  if (!fftResult.value || !fftAnalyzer.value) return

  const peaks = fftAnalyzer.value.findPeaks(
    fftResult.value.magnitudes,
    fftResult.value.frequencies,
    fftResult.value.peakMagnitude * 0.1 // 阈值为峰值的10%
  )
  
  detectedPeaks.value = peaks
  emit('peaksDetected', peaks)
}

// 更新图表
const updateChart = () => {
  if (!chart.value) return

  let option: any

  if (isFFTMode.value) {
    const fftData = performFFT()
    if (fftData) {
      option = generateFFTOption(fftData)
    } else {
      return
    }
  } else {
    if (!props.data) return
    option = generateTimeOption()
  }

  chart.value.setOption(option, true)
}

// 生成FFT图表配置
const generateFFTOption = (fftData: FFTResult) => {
  let yData: Float32Array
  let yAxisName: string
  
  switch (displayMode.value) {
    case 'magnitude':
      yData = fftData.magnitudes
      yAxisName = '幅度'
      break
    case 'magnitude_db':
      yData = new Float32Array(fftData.magnitudes.length)
      for (let i = 0; i < fftData.magnitudes.length; i++) {
        yData[i] = 20 * Math.log10(Math.max(fftData.magnitudes[i], 1e-10))
      }
      yAxisName = '幅度 (dB)'
      break
    case 'psd':
      yData = fftData.powerSpectralDensity
      yAxisName = '功率谱密度'
      break
    case 'phase':
      yData = fftData.phases
      yAxisName = '相位 (rad)'
      break
    default:
      yData = fftData.magnitudes
      yAxisName = '幅度'
  }

  // 准备数据
  const seriesData = Array.from(fftData.frequencies).map((freq, index) => [freq, yData[index]])
  
  // 添加峰值标记
  const peakMarkers: any[] = []
  if (peakDetectionEnabled.value && detectedPeaks.value.length > 0) {
    detectedPeaks.value.slice(0, 10).forEach((peak, index) => {
      const yValue = displayMode.value === 'magnitude_db' 
        ? 20 * Math.log10(Math.max(peak.magnitude, 1e-10))
        : peak.magnitude
      
      peakMarkers.push({
        coord: [peak.frequency, yValue],
        name: `峰值${index + 1}`,
        itemStyle: { color: '#ff4757' }
      })
    })
  }

  return {
    title: {
      show: false
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'cross' },
      formatter: (params: any) => {
        const point = params[0]
        return `频率: ${point.value[0].toFixed(2)} Hz<br/>${yAxisName}: ${point.value[1].toFixed(6)}`
      }
    },
    legend: {
      show: localOptions.value.legendVisible,
      top: 10
    },
    grid: {
      left: '10%',
      right: '10%',
      bottom: '15%',
      top: localOptions.value.legendVisible ? '15%' : '10%',
      containLabel: true
    },
    xAxis: {
      type: 'value',
      name: '频率 (Hz)',
      nameLocation: 'middle',
      nameGap: 30,
      axisLine: { show: true },
      axisTick: { show: true },
      axisLabel: { show: true },
      splitLine: { 
        show: localOptions.value.gridEnabled,
        lineStyle: { type: 'solid', color: '#e0e6ed' }
      }
    },
    yAxis: {
      type: yAxisScale.value === 'log' ? 'log' : 'value',
      name: yAxisName,
      nameLocation: 'middle',
      nameGap: 50,
      scale: !localOptions.value.autoScale,
      axisLine: { show: true },
      axisTick: { show: true },
      axisLabel: { show: true },
      splitLine: { 
        show: localOptions.value.gridEnabled,
        lineStyle: { type: 'solid', color: '#e0e6ed' }
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
    series: [
      {
        name: 'FFT频谱',
        type: 'line',
        data: seriesData,
        lineStyle: { color: '#409eff', width: 1 },
        itemStyle: { color: '#409eff' },
        symbol: 'none',
        smooth: false,
        markPoint: peakMarkers.length > 0 ? {
          data: peakMarkers,
          symbol: 'pin',
          symbolSize: 30
        } : undefined
      }
    ]
  }
}

// 生成时域图表配置
const generateTimeOption = () => {
  const { data } = props
  if (!data) return {}

  // 处理数据
  const series = processSeriesData(data)
  
  return {
    title: {
      show: false
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: localOptions.value.cursorMode === 'cursor' ? 'cross' : 'shadow'
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
      right: '10%',
      bottom: '15%',
      top: localOptions.value.legendVisible ? '15%' : '10%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: generateXAxisData(),
      name: '时间',
      nameLocation: 'middle',
      nameGap: 30,
      axisLine: { show: true },
      axisTick: { show: true },
      axisLabel: { show: true },
      splitLine: { 
        show: localOptions.value.gridEnabled,
        lineStyle: { type: 'solid', color: '#e0e6ed' }
      }
    },
    yAxis: {
      type: localOptions.value.logarithmic ? 'log' : 'value',
      name: '幅度',
      nameLocation: 'middle',
      nameGap: 50,
      scale: !localOptions.value.autoScale,
      axisLine: { show: true },
      axisTick: { show: true },
      axisLabel: { show: true },
      splitLine: { 
        show: localOptions.value.gridEnabled,
        lineStyle: { type: 'solid', color: '#e0e6ed' }
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
    link.download = `spectrum_chart_${Date.now()}.png`
    link.href = url
    link.click()
  } else if (format === 'csv') {
    exportToCSV()
  }
}

// 导出 CSV
const exportToCSV = () => {
  if (isFFTMode.value && fftResult.value) {
    // 导出FFT结果
    let csvContent = 'Frequency(Hz),Magnitude,Phase(rad),PSD\n'
    
    for (let i = 0; i < fftResult.value.frequencies.length; i++) {
      csvContent += `${fftResult.value.frequencies[i]},${fftResult.value.magnitudes[i]},${fftResult.value.phases[i]},${fftResult.value.powerSpectralDensity[i]}\n`
    }
    
    const blob = new Blob([csvContent], { type: 'text/csv' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.download = `fft_spectrum_${Date.now()}.csv`
    link.href = url
    link.click()
    URL.revokeObjectURL(url)
  } else if (props.data) {
    // 导出时域数据
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
    link.download = `chart_data_${Date.now()}.csv`
    link.href = url
    link.click()
    URL.revokeObjectURL(url)
  }
}

// 切换全屏
const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value
  nextTick(() => {
    chart.value?.resize()
  })
}

// 更新FFT配置
const updateFFT = () => {
  if (fftAnalyzer.value) {
    fftAnalyzer.value.updateOptions(fftOptions.value)
  } else {
    initFFTAnalyzer()
  }
  
  if (isFFTMode.value) {
    updateChart()
  }
}

// 应用FFT设置
const applyFFTSettings = () => {
  updateFFT()
  showFFTSettings.value = false
  ElMessage.success('FFT设置已应用')
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
  exportChart,
  updateChart,
  toggleFFTMode,
  getChart: () => chart.value,
  getFFTResult: () => fftResult.value
})
</script>

<style lang="scss" scoped>
.spectrum-chart-wrapper {
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
  
  .chart-container {
    flex: 1;
    min-height: 300px;
    
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
  
  .fft-controls,
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
  
  .chart-status {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 4px 12px;
    font-size: 12px;
    color: #909399;
    background: #f5f7fa;
    border-top: 1px solid #e4e7ed;
    
    .status-left,
    .status-right {
      display: flex;
      gap: 16px;
    }
    
    span {
      white-space: nowrap;
    }
  }
  
  .peaks-panel {
    margin-top: 12px;
    
    .peaks-card {
      max-height: 300px;
      overflow-y: auto;
    }
  }
}

@media (max-width: 768px) {
  .spectrum-chart-wrapper {
    .fft-controls,
    .chart-controls {
      .control-group {
        margin-bottom: 12px;
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
</style>
