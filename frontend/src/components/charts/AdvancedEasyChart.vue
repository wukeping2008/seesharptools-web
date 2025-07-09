<template>
  <div class="advanced-easy-chart professional-control">
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
      </div>
      
      <div class="toolbar-center">
        <el-button-group>
          <el-button size="small" @click="showMathPanel = !showMathPanel" :type="showMathPanel ? 'primary' : ''">
            <el-icon><DataAnalysis /></el-icon>
            数学分析
          </el-button>
          <el-button size="small" @click="showFittingPanel = !showFittingPanel" :type="showFittingPanel ? 'primary' : ''">
            <el-icon><TrendCharts /></el-icon>
            数据拟合
          </el-button>
          <el-button size="small" @click="showFilterPanel = !showFilterPanel" :type="showFilterPanel ? 'primary' : ''">
            <el-icon><Filter /></el-icon>
            滤波器
          </el-button>
        </el-button-group>
      </div>
      
      <div class="toolbar-right">
        <el-tooltip content="游标测量">
          <el-button size="small" @click="toggleCursor" :type="cursorEnabled ? 'primary' : ''">
            <el-icon><Aim /></el-icon>
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

    <!-- 数学分析面板 -->
    <div class="math-panel" v-if="showMathPanel && showControls">
      <el-card>
        <template #header>
          <div class="panel-header">
            <span>📊 统计分析</span>
            <el-button size="small" @click="calculateStatistics">
              <el-icon><Refresh /></el-icon>
              计算
            </el-button>
          </div>
        </template>
        
        <div v-if="statisticsResult" class="statistics-grid">
          <div class="stat-item">
            <label>平均值:</label>
            <span>{{ statisticsResult.mean.toFixed(4) }}</span>
          </div>
          <div class="stat-item">
            <label>中位数:</label>
            <span>{{ statisticsResult.median.toFixed(4) }}</span>
          </div>
          <div class="stat-item">
            <label>标准差:</label>
            <span>{{ statisticsResult.standardDeviation.toFixed(4) }}</span>
          </div>
          <div class="stat-item">
            <label>方差:</label>
            <span>{{ statisticsResult.variance.toFixed(4) }}</span>
          </div>
          <div class="stat-item">
            <label>RMS:</label>
            <span>{{ statisticsResult.rms.toFixed(4) }}</span>
          </div>
          <div class="stat-item">
            <label>极差:</label>
            <span>{{ statisticsResult.range.toFixed(4) }}</span>
          </div>
          <div class="stat-item">
            <label>偏度:</label>
            <span>{{ statisticsResult.skewness.toFixed(4) }}</span>
          </div>
          <div class="stat-item">
            <label>峰度:</label>
            <span>{{ statisticsResult.kurtosis.toFixed(4) }}</span>
          </div>
          <div class="stat-item">
            <label>Q1:</label>
            <span>{{ statisticsResult.q1.toFixed(4) }}</span>
          </div>
          <div class="stat-item">
            <label>Q3:</label>
            <span>{{ statisticsResult.q3.toFixed(4) }}</span>
          </div>
          <div class="stat-item">
            <label>IQR:</label>
            <span>{{ statisticsResult.iqr.toFixed(4) }}</span>
          </div>
          <div class="stat-item">
            <label>异常值:</label>
            <span>{{ statisticsResult.outliers.length }}个</span>
          </div>
        </div>
        
        <div v-else class="no-result">
          <el-empty description="点击计算按钮进行统计分析" />
        </div>
      </el-card>
    </div>

    <!-- 数据拟合面板 -->
    <div class="fitting-panel" v-if="showFittingPanel && showControls">
      <el-card>
        <template #header>
          <div class="panel-header">
            <span>📈 数据拟合</span>
            <div class="fitting-controls">
              <el-select v-model="fittingType" size="small" style="width: 120px;">
                <el-option label="线性拟合" value="linear" />
                <el-option label="多项式拟合" value="polynomial" />
              </el-select>
              <el-input-number 
                v-if="fittingType === 'polynomial'"
                v-model="polynomialDegree" 
                :min="2" 
                :max="6" 
                size="small"
                style="width: 80px; margin-left: 8px;"
              />
              <el-button size="small" @click="performFitting" style="margin-left: 8px;">
                <el-icon><TrendCharts /></el-icon>
                拟合
              </el-button>
            </div>
          </div>
        </template>
        
        <div v-if="fittingResult" class="fitting-result">
          <div class="fitting-info">
            <div class="info-item">
              <label>拟合方程:</label>
              <span class="equation">{{ fittingResult.equation }}</span>
            </div>
            <div class="info-grid">
              <div class="info-item">
                <label>R²:</label>
                <span>{{ fittingResult.rSquared.toFixed(6) }}</span>
              </div>
              <div class="info-item">
                <label>RMSE:</label>
                <span>{{ fittingResult.rmse.toFixed(6) }}</span>
              </div>
              <div class="info-item">
                <label>MAE:</label>
                <span>{{ fittingResult.mae.toFixed(6) }}</span>
              </div>
            </div>
          </div>
          
          <div class="fitting-actions">
            <el-button size="small" @click="showFittingCurve = !showFittingCurve" :type="showFittingCurve ? 'primary' : ''">
              {{ showFittingCurve ? '隐藏' : '显示' }}拟合曲线
            </el-button>
            <el-button size="small" @click="showResiduals = !showResiduals" :type="showResiduals ? 'primary' : ''">
              {{ showResiduals ? '隐藏' : '显示' }}残差
            </el-button>
          </div>
        </div>
        
        <div v-else class="no-result">
          <el-empty description="选择拟合类型并点击拟合按钮" />
        </div>
      </el-card>
    </div>

    <!-- 滤波器面板 -->
    <div class="filter-panel" v-if="showFilterPanel && showControls">
      <el-card>
        <template #header>
          <div class="panel-header">
            <span>🔧 数字滤波器</span>
            <el-button size="small" @click="applyFilter">
              <el-icon><Filter /></el-icon>
              应用滤波
            </el-button>
          </div>
        </template>
        
        <el-row :gutter="16">
          <el-col :span="6">
            <div class="control-group">
              <label>滤波器类型:</label>
              <el-select v-model="filterType" size="small">
                <el-option label="移动平均" value="moving_average" />
                <el-option label="中值滤波" value="median" />
                <el-option label="高斯滤波" value="gaussian" />
                <el-option label="低通滤波" value="lowpass" />
                <el-option label="高通滤波" value="highpass" />
                <el-option label="数据平滑" value="smooth" />
              </el-select>
            </div>
          </el-col>
          
          <el-col :span="6">
            <div class="control-group">
              <label v-if="filterType === 'moving_average' || filterType === 'median' || filterType === 'smooth'">窗口大小:</label>
              <label v-else-if="filterType === 'gaussian'">Sigma值:</label>
              <label v-else-if="filterType === 'lowpass' || filterType === 'highpass'">截止频率:</label>
              <el-input-number 
                v-model="filterParameter" 
                :min="filterType === 'gaussian' ? 0.1 : 1" 
                :max="filterType === 'lowpass' || filterType === 'highpass' ? 1000 : 50"
                :step="filterType === 'gaussian' ? 0.1 : 1"
                size="small"
              />
            </div>
          </el-col>
          
          <el-col :span="6" v-if="filterType === 'lowpass' || filterType === 'highpass'">
            <div class="control-group">
              <label>采样率:</label>
              <el-input-number 
                v-model="sampleRate" 
                :min="1" 
                :max="10000"
                size="small"
              />
            </div>
          </el-col>
          
          <el-col :span="6">
            <div class="control-group">
              <label>操作:</label>
              <div class="filter-actions">
                <el-button size="small" @click="showFilteredData = !showFilteredData" :type="showFilteredData ? 'primary' : ''">
                  {{ showFilteredData ? '隐藏' : '显示' }}滤波结果
                </el-button>
                <el-button size="small" @click="replaceWithFiltered" v-if="filteredData.length > 0">
                  替换原数据
                </el-button>
              </div>
            </div>
          </el-col>
        </el-row>
      </el-card>
    </div>

    <!-- 游标测量面板 -->
    <div class="cursor-panel" v-if="cursorEnabled && showControls">
      <el-card>
        <template #header>
          <span>📏 游标测量</span>
        </template>
        
        <div v-if="cursorData" class="cursor-info">
          <div class="cursor-item">
            <label>X坐标:</label>
            <span>{{ cursorData.x.toFixed(4) }}</span>
          </div>
          <div class="cursor-item">
            <label>Y坐标:</label>
            <span>{{ cursorData.y.toFixed(4) }}</span>
          </div>
          <div class="cursor-item">
            <label>数据点索引:</label>
            <span>{{ cursorData.index }}</span>
          </div>
        </div>
        
        <div v-else class="no-result">
          <el-empty description="在图表上移动鼠标查看数据" />
        </div>
      </el-card>
    </div>

    <!-- 状态信息 -->
    <div class="chart-status" v-if="showStatus">
      <div class="status-left">
        <span>数据点数: {{ dataPointsCount }}</span>
        <span>系列数: {{ seriesCount }}</span>
        <span v-if="statisticsResult">平均值: {{ statisticsResult.mean.toFixed(3) }}</span>
        <span v-if="fittingResult">R²: {{ fittingResult.rSquared.toFixed(3) }}</span>
      </div>
      <div class="status-right">
        <span v-if="cursorData">
          游标: X={{ cursorData.x.toFixed(3) }}, Y={{ cursorData.y.toFixed(3) }}
        </span>
        <span v-if="filteredData.length > 0">已应用滤波</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed, nextTick } from 'vue'
import * as echarts from 'echarts'
import { 
  Refresh, Download, Document, FullScreen, DataAnalysis, 
  TrendCharts, Filter, Aim
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import type { 
  ChartData, 
  ChartOptions, 
  SeriesConfig, 
  ChartEvents 
} from '@/types/chart'
import { 
  MathAnalyzer, 
  type StatisticsResult, 
  type FittingResult 
} from '@/utils/math/MathAnalyzer'

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
}

const props = withDefaults(defineProps<Props>(), {
  showToolbar: true,
  showControls: true,
  showStatus: true,
  width: '100%',
  height: '400px'
})

// Emits
const emit = defineEmits<{
  dataUpdate: [data: ChartData]
  zoom: [range: { xMin: number; xMax: number; yMin: number; yMax: number }]
  cursorMove: [position: { x: number; y: number }]
  seriesToggle: [seriesIndex: number, visible: boolean]
  statisticsCalculated: [result: StatisticsResult]
  fittingCompleted: [result: FittingResult]
}>()

// 响应式数据
const chartContainer = ref<HTMLDivElement>()
const chart = ref<echarts.ECharts>()
const isFullscreen = ref(false)

// 面板显示状态
const showMathPanel = ref(false)
const showFittingPanel = ref(false)
const showFilterPanel = ref(false)

// 游标功能
const cursorEnabled = ref(false)
const cursorData = ref<{ x: number; y: number; index: number }>()

// 数学分析
const statisticsResult = ref<StatisticsResult>()

// 数据拟合
const fittingType = ref<'linear' | 'polynomial'>('linear')
const polynomialDegree = ref(2)
const fittingResult = ref<FittingResult>()
const showFittingCurve = ref(true)
const showResiduals = ref(false)

// 滤波器
const filterType = ref<'moving_average' | 'median' | 'gaussian' | 'lowpass' | 'highpass' | 'smooth'>('moving_average')
const filterParameter = ref(5)
const sampleRate = ref(1000)
const filteredData = ref<number[]>([])
const showFilteredData = ref(true)

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
    if (params.componentType === 'series' && cursorEnabled.value) {
      const position = { x: params.value[0], y: params.value[1] }
      cursorData.value = {
        x: position.x,
        y: position.y,
        index: params.dataIndex
      }
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

// 更新图表
const updateChart = () => {
  if (!chart.value || !props.data) return

  const option = generateChartOption()
  chart.value.setOption(option, true)
}

// 生成图表配置
const generateChartOption = () => {
  const { data } = props
  if (!data) return {}

  // 处理数据
  const series = processSeriesData(data)
  
  // 添加拟合曲线
  if (fittingResult.value && showFittingCurve.value) {
    const xData = generateXAxisData()
    const fittingCurveData = xData.map((x, i) => [parseFloat(x), fittingResult.value!.fittedValues[i]])
    
    series.push({
      name: '拟合曲线',
      type: 'line',
      data: fittingCurveData,
      lineStyle: { color: '#e74c3c', width: 2, type: 'dashed' },
      itemStyle: { color: '#e74c3c' },
      symbol: 'none',
      smooth: false
    })
  }
  
  // 添加残差
  if (fittingResult.value && showResiduals.value) {
    const xData = generateXAxisData()
    const residualsData = xData.map((x, i) => [parseFloat(x), fittingResult.value!.residuals[i]])
    
    series.push({
      name: '残差',
      type: 'scatter',
      data: residualsData,
      itemStyle: { color: '#f39c12' },
      symbol: 'circle',
      symbolSize: 4
    })
  }
  
  // 添加滤波结果
  if (filteredData.value.length > 0 && showFilteredData.value) {
    const xData = generateXAxisData()
    const filteredCurveData = xData.map((x, i) => [parseFloat(x), filteredData.value[i]])
    
    series.push({
      name: '滤波结果',
      type: 'line',
      data: filteredCurveData,
      lineStyle: { color: '#27ae60', width: 2 },
      itemStyle: { color: '#27ae60' },
      symbol: 'none',
      smooth: false
    })
  }
  
  return {
    title: {
      show: false
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: cursorEnabled.value ? 'cross' : 'shadow'
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
    link.download = `advanced_chart_${Date.now()}.png`
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
  link.download = `chart_data_${Date.now()}.csv`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

// 切换全屏
const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value
  nextTick(() => {
    chart.value?.resize()
  })
}

// 切换游标
const toggleCursor = () => {
  cursorEnabled.value = !cursorEnabled.value
  if (!cursorEnabled.value) {
    cursorData.value = undefined
  }
}

// 计算统计分析
const calculateStatistics = () => {
  if (!props.data) {
    ElMessage.warning('没有可分析的数据')
    return
  }

  try {
    let dataToAnalyze: number[]
    
    if (Array.isArray(props.data.series[0])) {
      // 多系列数据，分析第一个系列
      dataToAnalyze = (props.data.series as number[][])[0] || []
    } else {
      // 单系列数据
      dataToAnalyze = props.data.series as number[]
    }

    if (dataToAnalyze.length === 0) {
      ElMessage.warning('数据为空')
      return
    }

    statisticsResult.value = MathAnalyzer.calculateStatistics(dataToAnalyze)
    emit('statisticsCalculated', statisticsResult.value)
    ElMessage.success('统计分析完成')
  } catch (error) {
    console.error('统计分析失败:', error)
    ElMessage.error('统计分析失败')
  }
}

// 执行数据拟合
const performFitting = () => {
  if (!props.data) {
    ElMessage.warning('没有可拟合的数据')
    return
  }

  try {
    let dataToFit: number[]
    
    if (Array.isArray(props.data.series[0])) {
      // 多系列数据，拟合第一个系列
      dataToFit = (props.data.series as number[][])[0] || []
    } else {
      // 单系列数据
      dataToFit = props.data.series as number[]
    }

    if (dataToFit.length === 0) {
      ElMessage.warning('数据为空')
      return
    }

    // 生成X轴数据
    const xData = Array.from({ length: dataToFit.length }, (_, i) => i)
    
    if (fittingType.value === 'linear') {
      fittingResult.value = MathAnalyzer.linearFit(xData, dataToFit)
    } else if (fittingType.value === 'polynomial') {
      fittingResult.value = MathAnalyzer.polynomialFit(xData, dataToFit, polynomialDegree.value)
    }
    
    if (fittingResult.value) {
      emit('fittingCompleted', fittingResult.value)
    }
    updateChart()
    ElMessage.success('数据拟合完成')
  } catch (error) {
    console.error('数据拟合失败:', error)
    ElMessage.error('数据拟合失败')
  }
}

// 应用滤波器
const applyFilter = () => {
  if (!props.data) {
    ElMessage.warning('没有可滤波的数据')
    return
  }

  try {
    let dataToFilter: number[]
    
    if (Array.isArray(props.data.series[0])) {
      // 多系列数据，滤波第一个系列
      dataToFilter = (props.data.series as number[][])[0] || []
    } else {
      // 单系列数据
      dataToFilter = props.data.series as number[]
    }

    if (dataToFilter.length === 0) {
      ElMessage.warning('数据为空')
      return
    }

    switch (filterType.value) {
      case 'moving_average':
        filteredData.value = MathAnalyzer.movingAverageFilter(dataToFilter, filterParameter.value)
        break
      case 'median':
        filteredData.value = MathAnalyzer.medianFilter(dataToFilter, filterParameter.value)
        break
      case 'gaussian':
        filteredData.value = MathAnalyzer.gaussianFilter(dataToFilter, filterParameter.value)
        break
      case 'lowpass':
        filteredData.value = MathAnalyzer.lowPassFilter(dataToFilter, filterParameter.value, sampleRate.value)
        break
      case 'highpass':
        filteredData.value = MathAnalyzer.highPassFilter(dataToFilter, filterParameter.value, sampleRate.value)
        break
      case 'smooth':
        filteredData.value = MathAnalyzer.smoothData(dataToFilter, filterParameter.value)
        break
      default:
        ElMessage.warning('未知的滤波器类型')
        return
    }
    
    updateChart()
    ElMessage.success('滤波处理完成')
  } catch (error) {
    console.error('滤波处理失败:', error)
    ElMessage.error('滤波处理失败')
  }
}

// 用滤波结果替换原数据
const replaceWithFiltered = () => {
  if (filteredData.value.length === 0) {
    ElMessage.warning('没有滤波结果可替换')
    return
  }

  const newData: ChartData = {
    ...props.data!,
    series: filteredData.value
  }
  
  emit('dataUpdate', newData)
  filteredData.value = []
  showFilteredData.value = false
  updateChart()
  ElMessage.success('已用滤波结果替换原数据')
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

// 监听拟合显示状态变化
watch([showFittingCurve, showResiduals], () => {
  updateChart()
})

// 监听滤波显示状态变化
watch(showFilteredData, () => {
  updateChart()
})

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
  calculateStatistics,
  performFitting,
  applyFilter,
  toggleCursor,
  toggleFullscreen,
  getChart: () => chart.value,
  getStatistics: () => statisticsResult.value,
  getFittingResult: () => fittingResult.value,
  getFilteredData: () => filteredData.value
})
</script>

<style lang="scss" scoped>
.advanced-easy-chart {
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
    .toolbar-center,
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
  
  .math-panel,
  .fitting-panel,
  .filter-panel,
  .cursor-panel {
    margin-top: 12px;
    
    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      
      .fitting-controls {
        display: flex;
        align-items: center;
      }
    }
    
    .statistics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 12px;
      
      .stat-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 8px 12px;
        background: #f8f9fa;
        border-radius: 6px;
        
        label {
          font-weight: 500;
          color: #606266;
        }
        
        span {
          font-family: 'Courier New', monospace;
          color: #409eff;
          font-weight: bold;
        }
      }
    }
    
    .fitting-result {
      .fitting-info {
        margin-bottom: 16px;
        
        .info-item {
          display: flex;
          justify-content: space-between;
          align-items: center;
          margin-bottom: 8px;
          
          label {
            font-weight: 500;
            color: #606266;
          }
          
          .equation {
            font-family: 'Courier New', monospace;
            color: #e74c3c;
            font-weight: bold;
            font-size: 14px;
          }
          
          span {
            font-family: 'Courier New', monospace;
            color: #409eff;
            font-weight: bold;
          }
        }
        
        .info-grid {
          display: grid;
          grid-template-columns: repeat(3, 1fr);
          gap: 12px;
          margin-top: 12px;
        }
      }
      
      .fitting-actions {
        display: flex;
        gap: 8px;
      }
    }
    
    .control-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
      
      label {
        font-size: 12px;
        font-weight: 500;
        color: #606266;
      }
      
      .filter-actions {
        display: flex;
        flex-direction: column;
        gap: 4px;
      }
    }
    
    .cursor-info {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 12px;
      
      .cursor-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 8px 12px;
        background: #f8f9fa;
        border-radius: 6px;
        
        label {
          font-weight: 500;
          color: #606266;
        }
        
        span {
          font-family: 'Courier New', monospace;
          color: #409eff;
          font-weight: bold;
        }
      }
    }
    
    .no-result {
      display: flex;
      align-items: center;
      justify-content: center;
      height: 120px;
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
}

@media (max-width: 768px) {
  .advanced-easy-chart {
    .chart-toolbar {
      flex-direction: column;
      gap: 8px;
      
      .toolbar-left,
      .toolbar-center,
      .toolbar-right {
        width: 100%;
        justify-content: center;
      }
    }
    
    .statistics-grid {
      grid-template-columns: 1fr;
    }
    
    .cursor-info {
      grid-template-columns: 1fr;
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
