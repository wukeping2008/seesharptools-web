<template>
  <div class="dual-axis-easy-chart">
    <!-- 工具栏 -->
    <div v-if="showToolbar" class="chart-toolbar">
      <div class="toolbar-section">
        <span class="section-title">双Y轴控制</span>
        <el-switch
          v-model="localOptions.dualYAxis"
          active-text="双Y轴"
          inactive-text="单Y轴"
          @change="updateChart"
        />
      </div>
      
      <div class="toolbar-section" v-if="localOptions.dualYAxis">
        <span class="section-title">Y轴配置</span>
        <el-button-group>
          <el-button 
            :type="selectedYAxis === 0 ? 'primary' : 'default'"
            @click="selectedYAxis = 0"
            size="small"
          >
            左Y轴
          </el-button>
          <el-button 
            :type="selectedYAxis === 1 ? 'primary' : 'default'"
            @click="selectedYAxis = 1"
            size="small"
          >
            右Y轴
          </el-button>
        </el-button-group>
      </div>

      <div class="toolbar-section">
        <span class="section-title">显示选项</span>
        <el-switch
          v-model="localOptions.legendVisible"
          active-text="图例"
          @change="updateChart"
        />
        <el-switch
          v-model="localOptions.gridEnabled"
          active-text="网格"
          @change="updateChart"
        />
      </div>

      <div class="toolbar-section">
        <el-button @click="autoScale" size="small">自动缩放</el-button>
        <el-button @click="exportChart" size="small">导出</el-button>
        <el-button @click="toggleFullscreen" size="small">
          {{ isFullscreen ? '退出全屏' : '全屏' }}
        </el-button>
      </div>
    </div>

    <!-- Y轴配置面板 -->
    <div v-if="showControls && localOptions.dualYAxis" class="y-axis-controls">
      <div class="axis-config" v-for="(axis, index) in yAxes" :key="index">
        <h4>{{ index === 0 ? '左Y轴' : '右Y轴' }} 配置</h4>
        <div class="config-row">
          <label>标签:</label>
          <el-input v-model="axis.label" size="small" style="width: 120px" @change="updateChart" />
        </div>
        <div class="config-row">
          <label>单位:</label>
          <el-input v-model="axis.unit" size="small" style="width: 80px" @change="updateChart" />
        </div>
        <div class="config-row">
          <label>最小值:</label>
          <el-input-number 
            v-model="axis.min" 
            size="small" 
            style="width: 100px"
            :disabled="axis.autoScale"
            @change="updateChart"
          />
        </div>
        <div class="config-row">
          <label>最大值:</label>
          <el-input-number 
            v-model="axis.max" 
            size="small" 
            style="width: 100px"
            :disabled="axis.autoScale"
            @change="updateChart"
          />
        </div>
        <div class="config-row">
          <el-checkbox v-model="axis.autoScale" @change="updateChart">自动缩放</el-checkbox>
          <el-checkbox v-model="axis.logarithmic" @change="updateChart">对数坐标</el-checkbox>
        </div>
      </div>
    </div>

    <!-- 系列配置面板 -->
    <div v-if="showControls && localOptions.dualYAxis" class="series-controls">
      <h4>系列Y轴分配</h4>
      <div class="series-assignment" v-for="(series, index) in seriesConfigs" :key="index">
        <span class="series-name" :style="{ color: series.color }">{{ series.name }}</span>
        <el-radio-group v-model="series.yAxisIndex" @change="updateChart">
          <el-radio :label="0">左Y轴</el-radio>
          <el-radio :label="1">右Y轴</el-radio>
        </el-radio-group>
      </div>
    </div>

    <!-- 图表容器 -->
    <div 
      ref="chartContainer" 
      class="chart-container"
      :class="{ 'fullscreen': isFullscreen }"
      :style="{ height: chartHeight + 'px' }"
    ></div>

    <!-- 状态栏 -->
    <div v-if="showStatus" class="chart-status">
      <span>数据点数: {{ dataPointsCount }}</span>
      <span v-if="localOptions.dualYAxis">双Y轴模式</span>
      <span>{{ localOptions.theme }} 主题</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import * as echarts from 'echarts'
import type { ChartData, ChartOptions, SeriesConfig, AxisConfig } from '@/types/chart'

// Props
interface Props {
  data: ChartData
  seriesConfigs: SeriesConfig[]
  options?: Partial<ChartOptions>
  showToolbar?: boolean
  showControls?: boolean
  showStatus?: boolean
  height?: number
}

const props = withDefaults(defineProps<Props>(), {
  showToolbar: true,
  showControls: true,
  showStatus: true,
  height: 400
})

// Emits
const emit = defineEmits<{
  optionsChange: [options: ChartOptions]
  dataExport: [data: any]
}>()

// 响应式数据
const chartContainer = ref<HTMLElement>()
let chartInstance: echarts.ECharts | null = null
const isFullscreen = ref(false)
const selectedYAxis = ref(0)

// 本地选项
const localOptions = reactive<ChartOptions>({
  autoScale: true,
  logarithmic: false,
  splitView: false,
  legendVisible: true,
  cursorMode: 'zoom',
  gridEnabled: true,
  minorGridEnabled: false,
  theme: 'light',
  dualYAxis: false,
  yAxes: [],
  ...props.options
})

// Y轴配置
const yAxes = reactive<AxisConfig[]>([
  {
    autoScale: true,
    logarithmic: false,
    label: '左Y轴',
    unit: '',
    format: '{value}',
    division: 10,
    offset: 0
  },
  {
    autoScale: true,
    logarithmic: false,
    label: '右Y轴',
    unit: '',
    format: '{value}',
    division: 10,
    offset: 0
  }
])

// 计算属性
const chartHeight = computed(() => {
  let height = props.height
  if (props.showToolbar) height -= 60
  if (props.showControls && localOptions.dualYAxis) height -= 200
  if (props.showStatus) height -= 30
  return Math.max(height, 200)
})

const dataPointsCount = computed(() => {
  if (Array.isArray(props.data.series)) {
    if (Array.isArray(props.data.series[0])) {
      return (props.data.series as number[][]).reduce((sum, series) => sum + series.length, 0)
    } else {
      return (props.data.series as number[]).length
    }
  }
  return 0
})

// 生成图表配置
const generateChartOption = () => {
  const seriesData = Array.isArray(props.data.series[0]) 
    ? props.data.series as number[][]
    : [props.data.series as number[]]

  // 生成X轴数据
  const maxLength = Math.max(...seriesData.map(s => s.length))
  const xData = Array.from({ length: maxLength }, (_, i) => {
    const x = (props.data.xStart || 0) + i * (props.data.xInterval || 1)
    return x
  })

  // 生成系列数据
  const series = seriesData.map((data, index) => {
    const config = props.seriesConfigs[index] || {
      name: `Series ${index + 1}`,
      color: `hsl(${index * 60}, 70%, 50%)`,
      lineWidth: 2,
      lineType: 'solid',
      markerType: 'none',
      markerSize: 4,
      visible: true,
      yAxisIndex: 0
    }

    return {
      name: config.name,
      type: 'line',
      data: data.map((value, i) => [xData[i], value]),
      lineStyle: {
        color: config.color,
        width: config.lineWidth,
        type: config.lineType
      },
      itemStyle: {
        color: config.color
      },
      symbol: config.markerType === 'none' ? 'none' : config.markerType,
      symbolSize: config.markerSize,
      yAxisIndex: localOptions.dualYAxis ? (config.yAxisIndex || 0) : 0,
      showSymbol: config.markerType !== 'none'
    }
  }).filter((_, index) => props.seriesConfigs[index]?.visible !== false)

  // Y轴配置
  const yAxisConfigs = localOptions.dualYAxis ? [
    {
      type: 'value',
      name: yAxes[0].label + (yAxes[0].unit ? ` (${yAxes[0].unit})` : ''),
      position: 'left',
      min: yAxes[0].autoScale ? null : yAxes[0].min,
      max: yAxes[0].autoScale ? null : yAxes[0].max,
      logBase: yAxes[0].logarithmic ? 10 : undefined,
      axisLabel: {
        formatter: yAxes[0].format
      },
      splitLine: {
        show: localOptions.gridEnabled
      }
    },
    {
      type: 'value',
      name: yAxes[1].label + (yAxes[1].unit ? ` (${yAxes[1].unit})` : ''),
      position: 'right',
      min: yAxes[1].autoScale ? null : yAxes[1].min,
      max: yAxes[1].autoScale ? null : yAxes[1].max,
      logBase: yAxes[1].logarithmic ? 10 : undefined,
      axisLabel: {
        formatter: yAxes[1].format
      },
      splitLine: {
        show: false // 右Y轴不显示网格线，避免重复
      }
    }
  ] : [
    {
      type: 'value',
      name: yAxes[0].label + (yAxes[0].unit ? ` (${yAxes[0].unit})` : ''),
      min: yAxes[0].autoScale ? null : yAxes[0].min,
      max: yAxes[0].autoScale ? null : yAxes[0].max,
      logBase: yAxes[0].logarithmic ? 10 : undefined,
      axisLabel: {
        formatter: yAxes[0].format
      },
      splitLine: {
        show: localOptions.gridEnabled
      }
    }
  ]

  return {
    backgroundColor: localOptions.backgroundColor || 'transparent',
    grid: {
      left: '10%',
      right: localOptions.dualYAxis ? '10%' : '5%',
      top: '10%',
      bottom: '15%',
      containLabel: true
    },
    xAxis: {
      type: 'value',
      name: 'X轴',
      splitLine: {
        show: localOptions.gridEnabled
      },
      minorSplitLine: {
        show: localOptions.minorGridEnabled
      }
    },
    yAxis: yAxisConfigs,
    series,
    legend: {
      show: localOptions.legendVisible,
      top: 'top',
      type: 'scroll'
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'cross'
      },
      formatter: (params: any) => {
        let result = `X: ${params[0].data[0].toFixed(3)}<br/>`
        params.forEach((param: any) => {
          const yAxisIndex = param.seriesIndex < props.seriesConfigs.length 
            ? props.seriesConfigs[param.seriesIndex].yAxisIndex || 0 
            : 0
          const unit = yAxes[yAxisIndex].unit || ''
          result += `${param.seriesName}: ${param.data[1].toFixed(3)}${unit}<br/>`
        })
        return result
      }
    },
    toolbox: {
      show: true,
      feature: {
        dataZoom: {
          yAxisIndex: 'all'
        },
        restore: {},
        saveAsImage: {}
      }
    },
    dataZoom: [
      {
        type: 'inside',
        xAxisIndex: 0,
        yAxisIndex: localOptions.dualYAxis ? [0, 1] : [0]
      },
      {
        type: 'slider',
        xAxisIndex: 0,
        bottom: '5%'
      }
    ]
  }
}

// 更新图表
const updateChart = async () => {
  if (!chartInstance) return
  
  await nextTick()
  const option = generateChartOption()
  chartInstance.setOption(option, true)
  
  emit('optionsChange', { ...localOptions })
}

// 自动缩放
const autoScale = () => {
  yAxes.forEach(axis => {
    axis.autoScale = true
  })
  updateChart()
}

// 导出图表
const exportChart = () => {
  if (!chartInstance) return
  
  const dataURL = chartInstance.getDataURL({
    type: 'png',
    pixelRatio: 2,
    backgroundColor: '#fff'
  })
  
  const link = document.createElement('a')
  link.download = 'dual-axis-chart.png'
  link.href = dataURL
  link.click()
  
  emit('dataExport', {
    image: dataURL,
    data: props.data,
    options: localOptions
  })
}

// 全屏切换
const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value
  setTimeout(() => {
    if (chartInstance) {
      chartInstance.resize()
    }
  }, 100)
}

// 初始化图表
const initChart = () => {
  if (!chartContainer.value) return
  
  chartInstance = echarts.init(chartContainer.value, localOptions.theme)
  updateChart()
  
  // 监听窗口大小变化
  window.addEventListener('resize', () => {
    if (chartInstance) {
      chartInstance.resize()
    }
  })
}

// 监听数据变化
watch(() => props.data, updateChart, { deep: true })
watch(() => props.seriesConfigs, updateChart, { deep: true })
watch(() => localOptions.theme, () => {
  if (chartInstance) {
    chartInstance.dispose()
    initChart()
  }
})

// 生命周期
onMounted(() => {
  nextTick(() => {
    initChart()
  })
})

onUnmounted(() => {
  if (chartInstance) {
    chartInstance.dispose()
  }
  window.removeEventListener('resize', () => {})
})
</script>

<style lang="scss" scoped>
.dual-axis-easy-chart {
  width: 100%;
  border: 1px solid #e4e7ed;
  border-radius: 4px;
  background: white;

  .chart-toolbar {
    display: flex;
    align-items: center;
    gap: 20px;
    padding: 10px 15px;
    border-bottom: 1px solid #e4e7ed;
    background: #f8f9fa;
    flex-wrap: wrap;

    .toolbar-section {
      display: flex;
      align-items: center;
      gap: 8px;

      .section-title {
        font-size: 12px;
        color: #606266;
        margin-right: 8px;
      }
    }
  }

  .y-axis-controls {
    display: flex;
    gap: 20px;
    padding: 15px;
    border-bottom: 1px solid #e4e7ed;
    background: #fafafa;

    .axis-config {
      flex: 1;
      
      h4 {
        margin: 0 0 10px 0;
        color: #303133;
        font-size: 14px;
      }

      .config-row {
        display: flex;
        align-items: center;
        gap: 8px;
        margin-bottom: 8px;

        label {
          width: 60px;
          font-size: 12px;
          color: #606266;
        }
      }
    }
  }

  .series-controls {
    padding: 15px;
    border-bottom: 1px solid #e4e7ed;
    background: #fafafa;

    h4 {
      margin: 0 0 10px 0;
      color: #303133;
      font-size: 14px;
    }

    .series-assignment {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 8px;
      padding: 5px 0;

      .series-name {
        font-weight: 500;
        font-size: 13px;
      }
    }
  }

  .chart-container {
    width: 100%;
    position: relative;

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

  .chart-status {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 5px 15px;
    background: #f8f9fa;
    border-top: 1px solid #e4e7ed;
    font-size: 12px;
    color: #909399;

    span {
      margin-right: 15px;
    }
  }
}
</style>
