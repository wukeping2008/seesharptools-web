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
        
        <!-- 图表类型选择 -->
        <el-select v-model="currentChartType" @change="updateChart" size="small" style="width: 120px;">
          <el-option label="折线图" value="line" />
          <el-option label="柱状图" value="bar" />
          <el-option label="散点图" value="scatter" />
          <el-option label="面积图" value="area" />
          <el-option label="直方图" value="histogram" />
        </el-select>
      </div>
      
      <div class="toolbar-right">
        <el-tooltip content="添加注释">
          <el-button size="small" @click="toggleAnnotationMode">
            <el-icon><EditPen /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="保存模板">
          <el-button size="small" @click="saveTemplate">
            <el-icon><FolderAdd /></el-icon>
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
      @click="handleChartClick"
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
              <el-checkbox v-model="showAnnotations" @change="updateChart">
                显示注释
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
              <el-checkbox v-model="localOptions.minorGridEnabled" @change="updateChart">
                显示次网格
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
            <label>数据缓冲:</label>
            <div>
              <el-input-number 
                v-model="bufferConfig.maxSize" 
                :min="100" 
                :max="100000" 
                size="small"
                @change="updateBufferConfig"
              />
              <span style="font-size: 12px;">最大点数</span>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 状态信息 -->
    <div class="chart-status" v-if="showStatus">
      <span>数据点数: {{ dataPointsCount }}</span>
      <span>系列数: {{ seriesCount }}</span>
      <span>缓冲使用: {{ bufferUsage }}%</span>
      <span v-if="cursorPosition">
        游标位置: X={{ cursorPosition.x.toFixed(3) }}, Y={{ cursorPosition.y.toFixed(3) }}
      </span>
    </div>

    <!-- 注释对话框 -->
    <el-dialog v-model="annotationDialogVisible" title="添加注释" width="400px">
      <el-form :model="newAnnotation" label-width="80px">
        <el-form-item label="文本">
          <el-input v-model="newAnnotation.text" placeholder="输入注释文本" />
        </el-form-item>
        <el-form-item label="位置">
          <el-select v-model="newAnnotation.position" placeholder="选择位置">
            <el-option label="顶部" value="top" />
            <el-option label="底部" value="bottom" />
            <el-option label="左侧" value="left" />
            <el-option label="右侧" value="right" />
          </el-select>
        </el-form-item>
        <el-form-item label="颜色">
          <el-color-picker v-model="newAnnotation.color" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="annotationDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="addAnnotation">确定</el-button>
      </template>
    </el-dialog>

    <!-- 模板保存对话框 -->
    <el-dialog v-model="templateDialogVisible" title="保存图表模板" width="400px">
      <el-form :model="newTemplate" label-width="80px">
        <el-form-item label="模板名称">
          <el-input v-model="newTemplate.name" placeholder="输入模板名称" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="newTemplate.description" type="textarea" placeholder="输入模板描述" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="templateDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveChartTemplate">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed, nextTick } from 'vue'
import * as echarts from 'echarts'
import { 
  Refresh, Download, Document, FullScreen, EditPen, FolderAdd 
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import type { 
  ChartData, 
  ChartOptions, 
  SeriesConfig, 
  DataAnnotation,
  ChartTemplate,
  DataBufferConfig
} from '@/types/chart'

// Props
interface Props {
  data?: ChartData
  options?: Partial<ChartOptions>
  seriesConfigs?: SeriesConfig[]
  annotations?: DataAnnotation[]
  showToolbar?: boolean
  showControls?: boolean
  showStatus?: boolean
  width?: string | number
  height?: string | number
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
  annotationAdd: [annotation: DataAnnotation]
  templateSave: [template: ChartTemplate]
}>()

// 响应式数据
const chartContainer = ref<HTMLDivElement>()
const chart = ref<echarts.ECharts>()
const isFullscreen = ref(false)
const cursorPosition = ref<{ x: number; y: number }>()
const currentChartType = ref<string>('line')
const showAnnotations = ref(true)
const annotationMode = ref(false)
const annotationDialogVisible = ref(false)
const templateDialogVisible = ref(false)

// 数据缓冲区配置
const bufferConfig = ref<DataBufferConfig>({
  maxSize: 10000,
  autoResize: true,
  compressionEnabled: false,
  compressionRatio: 0.5,
  retentionTime: 3600000 // 1小时
})

// 注释相关
const annotations = ref<DataAnnotation[]>(props.annotations || [])
const newAnnotation = ref<Partial<DataAnnotation>>({
  text: '',
  position: 'top',
  color: '#409eff'
})

// 模板相关
const newTemplate = ref<Partial<ChartTemplate>>({
  name: '',
  description: ''
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

const bufferUsage = computed(() => {
  if (!bufferConfig.value.maxSize) return 0
  return Math.round((dataPointsCount.value / bufferConfig.value.maxSize) * 100)
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
      trigger: currentChartType.value === 'scatter' ? 'item' : 'axis',
      axisPointer: {
        type: localOptions.value.cursorMode === 'cursor' ? 'cross' : 'shadow'
      },
      formatter: (params: any) => {
        if (Array.isArray(params)) {
          let result = `X: ${params[0].axisValue}<br/>`
          params.forEach((param: any) => {
            result += `${param.seriesName}: ${param.value}<br/>`
          })
          return result
        } else {
          return `${params.seriesName}<br/>X: ${params.value[0]}<br/>Y: ${params.value[1]}`
        }
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
      type: currentChartType.value === 'bar' ? 'category' : 'value',
      boundaryGap: currentChartType.value === 'bar',
      data: currentChartType.value === 'bar' ? generateXAxisData() : undefined,
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
    series,
    graphic: showAnnotations.value ? generateAnnotations() : []
  }
}

// 处理系列数据
const processSeriesData = (data: ChartData) => {
  const { series } = data
  const result: any[] = []
  const chartType = currentChartType.value || data.chartType || 'line'

  if (Array.isArray(series[0])) {
    // 多系列数据
    (series as number[][]).forEach((seriesData, index) => {
      const config = props.seriesConfigs?.[index]
      const seriesConfig = createSeriesConfig(seriesData, config, index, chartType)
      result.push(seriesConfig)
    })
  } else {
    // 单系列数据
    const config = props.seriesConfigs?.[0]
    const seriesConfig = createSeriesConfig(series as number[], config, 0, chartType)
    result.push(seriesConfig)
  }

  return result
}

// 创建系列配置
const createSeriesConfig = (data: number[], config: SeriesConfig | undefined, index: number, chartType: string) => {
  const baseConfig = {
    name: config?.name || `系列 ${index + 1}`,
    data: chartType === 'scatter' ? data.map((value, i) => [i, value]) : data,
    itemStyle: {
      color: config?.color || getDefaultColor(index)
    }
  }

  switch (chartType) {
    case 'line':
      return {
        ...baseConfig,
        type: 'line',
        lineStyle: {
          color: config?.color || getDefaultColor(index),
          width: config?.lineWidth || 2,
          type: config?.lineType || 'solid'
        },
        symbol: getMarkerSymbol(config?.markerType || 'circle'),
        symbolSize: config?.markerSize || 4,
        showSymbol: config?.markerType !== 'none',
        smooth: false,
        connectNulls: false
      }
    
    case 'bar':
      return {
        ...baseConfig,
        type: 'bar',
        barWidth: config?.barWidth || 'auto',
        stack: config?.stack
      }
    
    case 'scatter':
      return {
        ...baseConfig,
        type: 'scatter',
        symbol: getMarkerSymbol(config?.markerType || 'circle'),
        symbolSize: config?.markerSize || 6
      }
    
    case 'area':
      return {
        ...baseConfig,
        type: 'line',
        areaStyle: {
          opacity: config?.fillOpacity || 0.3
        },
        lineStyle: {
          color: config?.color || getDefaultColor(index),
          width: config?.lineWidth || 2
        },
        smooth: true
      }
    
    case 'histogram':
      // 简单的直方图实现
      const histogramData = calculateHistogram(data)
      return {
        ...baseConfig,
        type: 'bar',
        data: histogramData,
        barWidth: '90%'
      }
    
    default:
      return baseConfig
  }
}

// 计算直方图数据
const calculateHistogram = (data: number[], bins: number = 20) => {
  const min = Math.min(...data)
  const max = Math.max(...data)
  const binWidth = (max - min) / bins
  const histogram = new Array(bins).fill(0)
  
  data.forEach(value => {
    const binIndex = Math.min(Math.floor((value - min) / binWidth), bins - 1)
    histogram[binIndex]++
  })
  
  return histogram
}

// 生成注释
const generateAnnotations = () => {
  return annotations.value.filter(ann => ann.visible).map(annotation => ({
    type: 'text',
    left: annotation.x,
    top: annotation.y || 50,
    style: {
      text: annotation.text,
      fill: annotation.color || '#409eff',
      fontSize: annotation.fontSize || 12
    }
  }))
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
  })

  // 鼠标移动事件
  chart.value.on('mousemove', (params: any) => {
    if (params.componentType === 'series') {
      const position = { x: params.value[0], y: params.value[1] }
      cursorPosition.value = position
      emit('cursorMove', position)
    }
  })

  // 图例点击事件
  chart.value.on('legendselectchanged', (params: any) => {
    Object.keys(params.selected).forEach((name, index) => {
      const visible = params.selected[name]
      emit('seriesToggle', index, visible)
    })
  })
}

// 处理图表点击
const handleChartClick = (event: MouseEvent) => {
  if (!annotationMode.value || !chart.value) return
  
  const rect = chartContainer.value!.getBoundingClientRect()
  const x = event.clientX - rect.left
  const y = event.clientY - rect.top
  
  newAnnotation.value.x = x
  newAnnotation.value.y = y
  annotationDialogVisible.value = true
}

// 切换注释模式
const toggleAnnotationMode = () => {
  annotationMode.value = !annotationMode.value
  ElMessage.info(annotationMode.value ? '注释模式已开启，点击图表添加注释' : '注释模式已关闭')
}

// 添加注释
const addAnnotation = () => {
  if (!newAnnotation.value.text) {
    ElMessage.warning('请输入注释文本')
    return
  }
  
  const annotation: DataAnnotation = {
    id: Date.now().toString(),
    x: newAnnotation.value.x || 0,
    y: newAnnotation.value.y,
    text: newAnnotation.value.text || '',
    color: newAnnotation.value.color,
    position: newAnnotation.value.position as any,
    visible: true
  }
  
  annotations.value.push(annotation)
  emit('annotationAdd', annotation)
  
  // 重置表单
  newAnnotation.value = {
    text: '',
    position: 'top',
    color: '#409eff'
  }
  
  annotationDialogVisible.value = false
  updateChart()
  ElMessage.success('注释添加成功')
}

// 保存模板
const saveTemplate = () => {
  templateDialogVisible.value = true
}

// 保存图表模板
const saveChartTemplate = () => {
  if (!newTemplate.value.name) {
    ElMessage.warning('请输入模板名称')
    return
  }
  
  const template: ChartTemplate = {
    id: Date.now().toString(),
    name: newTemplate.value.name || '',
    description: newTemplate.value.description,
    chartOptions: { ...localOptions.value },
    seriesConfigs: props.seriesConfigs || [],
    annotations: [...annotations.value],
    createdAt: new Date(),
    updatedAt: new Date()
  }
  
  emit('templateSave', template)
  
  // 保存到本地存储
  const savedTemplates = JSON.parse(localStorage.getItem('chartTemplates') || '[]')
  savedTemplates.push(template)
  localStorage.setItem('chartTemplates', JSON.stringify(savedTemplates))
  
  // 重置表单
  newTemplate.value = {
    name: '',
    description: ''
  }
  
  templateDialogVisible.value = false
  ElMessage.success('模板保存成功')
}

// 更新缓冲区配置
const updateBufferConfig = () => {
  // 这里可以实现数据压缩和缓冲区管理逻辑
  ElMessage.info('缓冲区配置已更新')
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

// 监听图表类型变化
watch(() => props.data?.chartType, (newType) => {
  if (newType) {
    currentChartType.value = newType
    updateChart()
  }
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
  addAnnotation,
  saveTemplate,
  getChart: () => chart.value,
  getAnnotations: () => annotations.value,
  getBufferConfig: () => bufferConfig.value
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
  
  .chart-container {
    flex: 1;
    min-height: 300px;
    cursor: pointer;
    
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
  .enhanced-easy-chart-wrapper {
    .chart-toolbar {
      .toolbar-left,
      .toolbar-right {
        flex-direction: column;
        gap: 4px;
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
  }
}
</style>
