<template>
  <div class="easy-chart-wrapper professional-control">
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
      
      <div class="toolbar-right">
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
      <span>数据点数: {{ dataPointsCount }}</span>
      <span>系列数: {{ seriesCount }}</span>
      <span v-if="cursorPosition">
        游标位置: X={{ cursorPosition.x.toFixed(3) }}, Y={{ cursorPosition.y.toFixed(3) }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed, nextTick } from 'vue'
import * as echarts from 'echarts'
import { Refresh, Download, Document, FullScreen } from '@element-plus/icons-vue'
import type { 
  ChartData, 
  ChartOptions, 
  SeriesConfig, 
  ExportOptions,
  ChartEvents 
} from '@/types/chart'

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
}>()

// 响应式数据
const chartContainer = ref<HTMLDivElement>()
const chart = ref<echarts.ECharts>()
const isFullscreen = ref(false)
const cursorPosition = ref<{ x: number; y: number }>()

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
      axisLine: { show: true },
      axisTick: { show: true },
      axisLabel: { show: true },
      splitLine: { 
        show: localOptions.value.gridEnabled,
        lineStyle: { type: 'solid', color: '#e0e6ed' }
      },
      minorSplitLine: {
        show: localOptions.value.minorGridEnabled,
        lineStyle: { type: 'dashed', color: '#f0f2f5' }
      }
    },
    yAxis: {
      type: localOptions.value.logarithmic ? 'log' : 'value',
      scale: !localOptions.value.autoScale,
      axisLine: { show: true },
      axisTick: { show: true },
      axisLabel: { show: true },
      splitLine: { 
        show: localOptions.value.gridEnabled,
        lineStyle: { type: 'solid', color: '#e0e6ed' }
      },
      minorSplitLine: {
        show: localOptions.value.minorGridEnabled,
        lineStyle: { type: 'dashed', color: '#f0f2f5' }
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
    link.download = `chart_${Date.now()}.png`
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
  getChart: () => chart.value
})
</script>

<style lang="scss" scoped>
.easy-chart-wrapper {
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
    
    span {
      margin-right: 16px;
      
      &:last-child {
        margin-right: 0;
      }
    }
  }
}

@media (max-width: 768px) {
  .easy-chart-wrapper {
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
  }
}
</style>
