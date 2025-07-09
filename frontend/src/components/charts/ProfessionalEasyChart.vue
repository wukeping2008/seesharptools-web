<template>
  <div class="professional-easy-chart">
    <!-- 工具栏 -->
    <div class="chart-toolbar">
      <div class="toolbar-section">
        <span class="section-title">📏 测量工具</span>
        <el-button-group>
          <el-button 
            :type="cursorMode ? 'primary' : 'default'"
            @click="toggleCursorMode"
            size="small"
          >
            <el-icon><Position /></el-icon>
            游标测量
          </el-button>
          <el-button 
            @click="performPeakDetection"
            size="small"
          >
            <el-icon><TrendCharts /></el-icon>
            峰值检测
          </el-button>
          <el-button 
            @click="performFrequencyAnalysis"
            size="small"
          >
            <el-icon><DataAnalysis /></el-icon>
            频率分析
          </el-button>
          <el-button 
            @click="performAutoMeasurement"
            size="small"
          >
            <el-icon><Monitor /></el-icon>
            自动测量
          </el-button>
        </el-button-group>
      </div>

      <div class="toolbar-section">
        <span class="section-title">📊 显示选项</span>
        <el-button-group>
          <el-button 
            :type="showPeaks ? 'primary' : 'default'"
            @click="showPeaks = !showPeaks"
            size="small"
          >
            峰值标记
          </el-button>
          <el-button 
            :type="showGrid ? 'primary' : 'default'"
            @click="showGrid = !showGrid"
            size="small"
          >
            网格
          </el-button>
          <el-button 
            :type="showCursors ? 'primary' : 'default'"
            @click="showCursors = !showCursors"
            size="small"
          >
            游标
          </el-button>
        </el-button-group>
      </div>

      <div class="toolbar-section">
        <span class="section-title">🔧 操作</span>
        <el-button-group>
          <el-button @click="resetZoom" size="small">
            <el-icon><RefreshLeft /></el-icon>
            重置缩放
          </el-button>
          <el-button @click="exportChart" size="small">
            <el-icon><Download /></el-icon>
            导出
          </el-button>
          <el-button @click="toggleFullscreen" size="small">
            <el-icon><FullScreen /></el-icon>
            全屏
          </el-button>
        </el-button-group>
      </div>
    </div>

    <!-- 主图表区域 -->
    <div class="chart-container" :class="{ fullscreen: isFullscreen }">
      <div 
        ref="chartRef" 
        class="chart-main"
        :style="{ height: height + 'px' }"
        @mousemove="onMouseMove"
        @click="onChartClick"
      ></div>
      
      <!-- 游标线 -->
      <div v-if="showCursors && cursors.length > 0" class="cursors-overlay">
        <div 
          v-for="(cursor, index) in cursors" 
          :key="index"
          class="cursor-line"
          :style="{ 
            left: cursor.x + 'px',
            backgroundColor: cursor.color 
          }"
        >
          <div class="cursor-label">
            {{ cursor.label }}
          </div>
        </div>
      </div>
    </div>

    <!-- 测量结果面板 -->
    <div v-if="showMeasurementPanel" class="measurement-panel">
      <el-tabs v-model="activeMeasurementTab" type="border-card">
        <!-- 游标测量 -->
        <el-tab-pane label="游标测量" name="cursor">
          <div v-if="cursorMeasurement" class="measurement-content">
            <div class="measurement-grid">
              <div class="measurement-item">
                <span class="label">游标1:</span>
                <span class="value">X={{ cursorMeasurement.x1.toFixed(3) }}, Y={{ cursorMeasurement.y1.toFixed(3) }}</span>
              </div>
              <div class="measurement-item">
                <span class="label">游标2:</span>
                <span class="value">X={{ cursorMeasurement.x2.toFixed(3) }}, Y={{ cursorMeasurement.y2.toFixed(3) }}</span>
              </div>
              <div class="measurement-item">
                <span class="label">ΔX:</span>
                <span class="value">{{ cursorMeasurement.deltaX.toFixed(3) }}</span>
              </div>
              <div class="measurement-item">
                <span class="label">ΔY:</span>
                <span class="value">{{ cursorMeasurement.deltaY.toFixed(3) }}</span>
              </div>
              <div v-if="cursorMeasurement.frequency" class="measurement-item">
                <span class="label">频率:</span>
                <span class="value">{{ cursorMeasurement.frequency.toFixed(3) }} Hz</span>
              </div>
              <div v-if="cursorMeasurement.period" class="measurement-item">
                <span class="label">周期:</span>
                <span class="value">{{ cursorMeasurement.period.toFixed(3) }} s</span>
              </div>
              <div v-if="cursorMeasurement.slope" class="measurement-item">
                <span class="label">斜率:</span>
                <span class="value">{{ cursorMeasurement.slope.toFixed(3) }}</span>
              </div>
            </div>
          </div>
          <div v-else class="no-measurement">
            点击图表设置游标进行测量
          </div>
        </el-tab-pane>

        <!-- 峰值检测 -->
        <el-tab-pane label="峰值检测" name="peaks">
          <div v-if="peakDetectionResult" class="measurement-content">
            <div class="peak-statistics">
              <h4>统计信息</h4>
              <div class="stats-grid">
                <div class="stat-item">
                  <span class="label">峰值数量:</span>
                  <span class="value">{{ peakDetectionResult.statistics.peakCount }}</span>
                </div>
                <div class="stat-item">
                  <span class="label">谷值数量:</span>
                  <span class="value">{{ peakDetectionResult.statistics.valleyCount }}</span>
                </div>
                <div class="stat-item">
                  <span class="label">平均峰值:</span>
                  <span class="value">{{ peakDetectionResult.statistics.averagePeakValue.toFixed(3) }}</span>
                </div>
                <div class="stat-item">
                  <span class="label">峰峰值:</span>
                  <span class="value">{{ peakDetectionResult.statistics.peakToPeakAmplitude.toFixed(3) }}</span>
                </div>
              </div>
            </div>
            
            <div class="peak-list">
              <h4>峰值列表 (前10个)</h4>
              <el-table :data="peakDetectionResult.peaks.slice(0, 10)" size="small" max-height="200">
                <el-table-column prop="index" label="索引" width="60" />
                <el-table-column prop="value" label="数值" width="80">
                  <template #default="{ row }">
                    {{ row.value.toFixed(3) }}
                  </template>
                </el-table-column>
                <el-table-column prop="prominence" label="突出度" width="80">
                  <template #default="{ row }">
                    {{ row.prominence.toFixed(3) }}
                  </template>
                </el-table-column>
                <el-table-column prop="width" label="宽度" width="60" />
              </el-table>
            </div>
          </div>
          <div v-else class="no-measurement">
            点击"峰值检测"按钮进行分析
          </div>
        </el-tab-pane>

        <!-- 频率分析 -->
        <el-tab-pane label="频率分析" name="frequency">
          <div v-if="frequencyAnalysisResult" class="measurement-content">
            <div class="frequency-info">
              <div class="freq-item">
                <span class="label">基频:</span>
                <span class="value">{{ frequencyAnalysisResult.fundamentalFrequency.toFixed(3) }} Hz</span>
              </div>
              <div class="freq-item">
                <span class="label">主频:</span>
                <span class="value">{{ frequencyAnalysisResult.dominantFrequency.toFixed(3) }} Hz</span>
              </div>
              <div class="freq-item">
                <span class="label">THD:</span>
                <span class="value">{{ frequencyAnalysisResult.thd.toFixed(2) }}%</span>
              </div>
              <div class="freq-item">
                <span class="label">SNR:</span>
                <span class="value">{{ frequencyAnalysisResult.snr.toFixed(1) }} dB</span>
              </div>
              <div class="freq-item">
                <span class="label">带宽:</span>
                <span class="value">{{ frequencyAnalysisResult.bandwidth.toFixed(1) }} Hz</span>
              </div>
            </div>
            
            <div class="harmonics-list">
              <h4>谐波分量</h4>
              <el-table :data="frequencyAnalysisResult.harmonics.slice(0, 5)" size="small" max-height="150">
                <el-table-column prop="harmonic" label="次数" width="60" />
                <el-table-column prop="frequency" label="频率 (Hz)" width="100">
                  <template #default="{ row }">
                    {{ row.frequency.toFixed(2) }}
                  </template>
                </el-table-column>
                <el-table-column prop="amplitude" label="幅度" width="80">
                  <template #default="{ row }">
                    {{ row.amplitude.toFixed(3) }}
                  </template>
                </el-table-column>
              </el-table>
            </div>
          </div>
          <div v-else class="no-measurement">
            点击"频率分析"按钮进行分析
          </div>
        </el-tab-pane>

        <!-- 自动测量 -->
        <el-tab-pane label="自动测量" name="auto">
          <div v-if="autoMeasurementResult" class="measurement-content">
            <div class="auto-measurement-grid">
              <div class="measurement-group">
                <h4>基本参数</h4>
                <div class="param-item">
                  <span class="label">频率:</span>
                  <span class="value">{{ autoMeasurementResult.frequency.toFixed(3) }} Hz</span>
                </div>
                <div class="param-item">
                  <span class="label">周期:</span>
                  <span class="value">{{ autoMeasurementResult.period.toFixed(3) }} s</span>
                </div>
                <div class="param-item">
                  <span class="label">幅度:</span>
                  <span class="value">{{ autoMeasurementResult.amplitude.toFixed(3) }}</span>
                </div>
                <div class="param-item">
                  <span class="label">RMS:</span>
                  <span class="value">{{ autoMeasurementResult.rms.toFixed(3) }}</span>
                </div>
              </div>
              
              <div class="measurement-group">
                <h4>统计量</h4>
                <div class="param-item">
                  <span class="label">平均值:</span>
                  <span class="value">{{ autoMeasurementResult.mean.toFixed(3) }}</span>
                </div>
                <div class="param-item">
                  <span class="label">最小值:</span>
                  <span class="value">{{ autoMeasurementResult.min.toFixed(3) }}</span>
                </div>
                <div class="param-item">
                  <span class="label">最大值:</span>
                  <span class="value">{{ autoMeasurementResult.max.toFixed(3) }}</span>
                </div>
                <div class="param-item">
                  <span class="label">峰峰值:</span>
                  <span class="value">{{ autoMeasurementResult.peakToPeak.toFixed(3) }}</span>
                </div>
              </div>
              
              <div class="measurement-group">
                <h4>时序参数</h4>
                <div class="param-item">
                  <span class="label">占空比:</span>
                  <span class="value">{{ autoMeasurementResult.dutyCycle.toFixed(1) }}%</span>
                </div>
                <div class="param-item">
                  <span class="label">上升时间:</span>
                  <span class="value">{{ (autoMeasurementResult.riseTime * 1000).toFixed(2) }} ms</span>
                </div>
                <div class="param-item">
                  <span class="label">下降时间:</span>
                  <span class="value">{{ (autoMeasurementResult.fallTime * 1000).toFixed(2) }} ms</span>
                </div>
                <div class="param-item">
                  <span class="label">脉冲宽度:</span>
                  <span class="value">{{ (autoMeasurementResult.pulseWidth * 1000).toFixed(2) }} ms</span>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="no-measurement">
            点击"自动测量"按钮进行分析
          </div>
        </el-tab-pane>
      </el-tabs>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { 
  Position, TrendCharts, DataAnalysis, Monitor, 
  RefreshLeft, Download, FullScreen 
} from '@element-plus/icons-vue'
import * as echarts from 'echarts'
import type { ChartData, ChartOptions, SeriesConfig } from '@/types/chart'
import { 
  MeasurementTools,
  type CursorMeasurement,
  type PeakDetectionResult,
  type FrequencyAnalysisResult,
  type AutoMeasurementResult
} from '@/utils/measurement/MeasurementTools'

// Props
interface Props {
  data?: ChartData
  options?: ChartOptions
  seriesConfigs?: SeriesConfig[]
  height?: number
  sampleRate?: number
}

const props = withDefaults(defineProps<Props>(), {
  height: 400,
  sampleRate: 1000
})

// Emits
const emit = defineEmits<{
  'cursor-measurement': [result: CursorMeasurement]
  'peak-detection': [result: PeakDetectionResult]
  'frequency-analysis': [result: FrequencyAnalysisResult]
  'auto-measurement': [result: AutoMeasurementResult]
}>()

// 响应式数据
const chartRef = ref<HTMLDivElement>()
const chart = ref<echarts.ECharts>()
const isFullscreen = ref(false)
const showMeasurementPanel = ref(false)
const activeMeasurementTab = ref('cursor')

// 测量工具状态
const cursorMode = ref(false)
const showPeaks = ref(false)
const showGrid = ref(true)
const showCursors = ref(true)

// 游标相关
const cursors = ref<Array<{
  x: number
  y: number
  label: string
  color: string
}>>([])
const cursorMeasurement = ref<CursorMeasurement>()

// 测量结果
const peakDetectionResult = ref<PeakDetectionResult>()
const frequencyAnalysisResult = ref<FrequencyAnalysisResult>()
const autoMeasurementResult = ref<AutoMeasurementResult>()

// 获取数据数组（处理可能的多维数组）
const getDataArray = (): number[] => {
  if (!props.data) return []
  
  const series = props.data.series
  if (Array.isArray(series) && series.length > 0) {
    // 如果是二维数组，取第一个系列
    if (Array.isArray(series[0])) {
      return series[0] as number[]
    }
    // 如果是一维数组，直接返回
    return series as number[]
  }
  return []
}

// 图表配置
const getChartOption = () => {
  if (!props.data) return {}
  
  const dataArray = getDataArray()
  const xStart = props.data.xStart || 0
  const xInterval = props.data.xInterval || 1
  
  const option: any = {
    grid: {
      left: 60,
      right: 40,
      top: 40,
      bottom: 60,
      show: showGrid.value,
      borderColor: '#e0e0e0'
    },
    xAxis: {
      type: 'category',
      data: props.data.labels || Array.from({ length: dataArray.length }, (_, i) => 
        (xStart + i * xInterval).toString()
      ),
      axisLine: { lineStyle: { color: '#666' } },
      axisTick: { lineStyle: { color: '#666' } },
      axisLabel: { color: '#666' }
    },
    yAxis: {
      type: 'value',
      axisLine: { lineStyle: { color: '#666' } },
      axisTick: { lineStyle: { color: '#666' } },
      axisLabel: { color: '#666' },
      splitLine: { 
        show: showGrid.value,
        lineStyle: { color: '#e0e0e0', type: 'dashed' }
      }
    },
    series: [
      {
        name: props.seriesConfigs?.[0]?.name || '数据',
        type: 'line',
        data: dataArray,
        lineStyle: {
          color: props.seriesConfigs?.[0]?.color || '#409eff',
          width: props.seriesConfigs?.[0]?.lineWidth || 2
        },
        symbol: 'none',
        animation: false
      }
    ],
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        const point = params[0]
        return `X: ${point.name}<br/>Y: ${point.value.toFixed(4)}`
      }
    },
    toolbox: {
      show: false
    },
    dataZoom: [
      {
        type: 'inside',
        xAxisIndex: 0,
        filterMode: 'none'
      },
      {
        type: 'slider',
        xAxisIndex: 0,
        height: 20,
        bottom: 10
      }
    ]
  }
  
  // 添加峰值标记
  if (showPeaks.value && peakDetectionResult.value) {
    const peakData = peakDetectionResult.value.peaks.map(peak => ({
      name: `峰值 ${peak.value.toFixed(3)}`,
      coord: [peak.index, peak.value]
    }))
    
    const valleyData = peakDetectionResult.value.valleys.map(valley => ({
      name: `谷值 ${valley.value.toFixed(3)}`,
      coord: [valley.index, valley.value]
    }))
    
    option.series[0].markPoint = {
      data: [
        ...peakData.map(p => ({ ...p, symbol: 'triangle', symbolSize: 8, itemStyle: { color: '#f56c6c' } })),
        ...valleyData.map(v => ({ ...v, symbol: 'triangle', symbolSize: 8, itemStyle: { color: '#67c23a' } }))
      ]
    }
  }
  
  return option
}

// 初始化图表
const initChart = () => {
  if (!chartRef.value) return
  
  chart.value = echarts.init(chartRef.value)
  updateChart()
  
  // 监听窗口大小变化
  window.addEventListener('resize', handleResize)
}

// 更新图表
const updateChart = () => {
  if (!chart.value) return
  
  const option = getChartOption()
  chart.value.setOption(option, true)
}

// 窗口大小变化处理
const handleResize = () => {
  if (chart.value) {
    chart.value.resize()
  }
}

// 鼠标移动处理
const onMouseMove = (event: MouseEvent) => {
  if (!cursorMode.value || !chart.value) return
  
  const rect = chartRef.value!.getBoundingClientRect()
  const x = event.clientX - rect.left
  const y = event.clientY - rect.top
  
  // 更新游标位置（这里简化处理）
  // 实际应该转换为图表坐标系
}

// 图表点击处理
const onChartClick = (event: MouseEvent) => {
  if (!cursorMode.value || !chart.value || !props.data) return
  
  const rect = chartRef.value!.getBoundingClientRect()
  const x = event.clientX - rect.left
  
  const dataArray = getDataArray()
  const xStart = props.data.xStart || 0
  const xInterval = props.data.xInterval || 1
  
  // 简化的坐标转换
  const chartWidth = rect.width - 100 // 减去左右边距
  const dataIndex = Math.round((x - 60) / chartWidth * (dataArray.length - 1))
  
  if (dataIndex >= 0 && dataIndex < dataArray.length) {
    // 添加游标
    if (cursors.value.length < 2) {
      cursors.value.push({
        x,
        y: 0, // 简化处理
        label: `C${cursors.value.length + 1}`,
        color: cursors.value.length === 0 ? '#409eff' : '#f56c6c'
      })
    } else {
      // 重置游标
      cursors.value = [{
        x,
        y: 0,
        label: 'C1',
        color: '#409eff'
      }]
    }
    
    // 计算游标测量
    if (cursors.value.length === 2) {
      const x1 = xStart + Math.round((cursors.value[0].x - 60) / chartWidth * (dataArray.length - 1)) * xInterval
      const x2 = xStart + Math.round((cursors.value[1].x - 60) / chartWidth * (dataArray.length - 1)) * xInterval
      
      cursorMeasurement.value = MeasurementTools.cursorMeasurement(
        dataArray,
        x1,
        x2,
        xStart,
        xInterval
      )
      
      emit('cursor-measurement', cursorMeasurement.value)
      showMeasurementPanel.value = true
      activeMeasurementTab.value = 'cursor'
    }
  }
}

// 切换游标模式
const toggleCursorMode = () => {
  cursorMode.value = !cursorMode.value
  if (!cursorMode.value) {
    cursors.value = []
    cursorMeasurement.value = undefined
  }
}

// 峰值检测
const performPeakDetection = () => {
  if (!props.data) return
  
  const dataArray = getDataArray()
  const result = MeasurementTools.peakDetection(dataArray, 0, 5, 0.1)
  peakDetectionResult.value = result
  emit('peak-detection', result)
  
  showMeasurementPanel.value = true
  activeMeasurementTab.value = 'peaks'
  updateChart() // 更新图表显示峰值标记
}

// 频率分析
const performFrequencyAnalysis = () => {
  if (!props.data) return
  
  const dataArray = getDataArray()
  const result = MeasurementTools.frequencyAnalysis(dataArray, props.sampleRate)
  frequencyAnalysisResult.value = result
  emit('frequency-analysis', result)
  
  showMeasurementPanel.value = true
  activeMeasurementTab.value = 'frequency'
}

// 自动测量
const performAutoMeasurement = () => {
  if (!props.data) return
  
  const dataArray = getDataArray()
  const result = MeasurementTools.autoMeasurement(dataArray, props.sampleRate)
  autoMeasurementResult.value = result
  emit('auto-measurement', result)
  
  showMeasurementPanel.value = true
  activeMeasurementTab.value = 'auto'
}

// 重置缩放
const resetZoom = () => {
  if (chart.value) {
    chart.value.dispatchAction({
      type: 'dataZoom',
      start: 0,
      end: 100
    })
  }
}

// 导出图表
const exportChart = () => {
  if (chart.value) {
    const url = chart.value.getDataURL({
      type: 'png',
      pixelRatio: 2,
      backgroundColor: '#fff'
    })
    
    const link = document.createElement('a')
    link.download = 'professional-chart.png'
    link.href = url
    link.click()
  }
}

// 切换全屏
const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value
  nextTick(() => {
    handleResize()
  })
}

// 监听数据变化
watch(() => props.data, () => {
  updateChart()
}, { deep: true })

watch([showGrid, showPeaks], () => {
  updateChart()
})

// 生命周期
onMounted(() => {
  initChart()
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
  if (chart.value) {
    chart.value.dispose()
  }
})
</script>

<style lang="scss" scoped>
.professional-easy-chart {
  display: flex;
  flex-direction: column;
  gap: 16px;
  
  .chart-toolbar {
    display: flex;
    flex-wrap: wrap;
    gap: 16px;
    padding: 12px;
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 8px;
    
    .toolbar-section {
      display: flex;
      align-items: center;
      gap: 8px;
      
      .section-title {
        font-size: 14px;
        font-weight: 500;
        color: var(--text-primary);
        margin-right: 8px;
      }
    }
  }
  
  .chart-container {
    position: relative;
    border: 1px solid var(--border-color);
    border-radius: 8px;
    background: white;
    
    &.fullscreen {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      z-index: 9999;
      border-radius: 0;
    }
    
    .chart-main {
      width: 100%;
    }
    
    .cursors-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      pointer-events: none;
      
      .cursor-line {
        position: absolute;
        top: 0;
        bottom: 0;
        width: 2px;
        opacity: 0.8;
        
        .cursor-label {
          position: absolute;
          top: 10px;
          left: 4px;
          background: rgba(0, 0, 0, 0.7);
          color: white;
          padding: 2px 6px;
          border-radius: 3px;
          font-size: 12px;
        }
      }
    }
  }
  
  .measurement-panel {
    border: 1px solid var(--border-color);
    border-radius: 8px;
    background: var(--surface-color);
    
    .measurement-content {
      padding: 16px;
      
      .measurement-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 12px;
        
        .measurement-item {
          display: flex;
          justify-content: space-between;
          padding: 8px 12px;
          background: white;
          border-radius: 6px;
          border: 1px solid var(--border-color);
          
          .label {
            font-weight: 500;
            color: var(--text-secondary);
          }
          
          .value {
            font-family: 'Consolas', 'Monaco', monospace;
            color: var(--primary-color);
            font-weight: bold;
          }
        }
      }
      
      .peak-statistics {
        margin-bottom: 16px;
        
        h4 {
          margin-bottom: 12px;
          color: var(--text-primary);
        }
        
        .stats-grid {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
          gap: 8px;
          
          .stat-item {
            display: flex;
            justify-content: space-between;
            padding: 6px 10px;
            background: white;
            border-radius: 4px;
            border: 1px solid var(--border-color);
            
            .label {
              font-size: 13px;
              color: var(--text-secondary);
            }
            
            .value {
              font-family: 'Consolas', 'Monaco', monospace;
              color: var(--primary-color);
              font-weight: bold;
              font-size: 13px;
            }
          }
        }
      }
      
      .peak-list {
        h4 {
          margin-bottom: 12px;
          color: var(--text-primary);
        }
      }
      
      .frequency-info {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 12px;
        margin-bottom: 16px;
        
        .freq-item {
          display: flex;
          justify-content: space-between;
          padding: 8px 12px;
          background: white;
          border-radius: 6px;
          border: 1px solid var(--border-color);
          
          .label {
            font-weight: 500;
            color: var(--text-secondary);
          }
          
          .value {
            font-family: 'Consolas', 'Monaco', monospace;
            color: var(--primary-color);
            font-weight: bold;
          }
        }
      }
      
      .harmonics-list {
        h4 {
          margin-bottom: 12px;
          color: var(--text-primary);
        }
      }
      
      .auto-measurement-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
        gap: 20px;
        
        .measurement-group {
          h4 {
            margin-bottom: 12px;
            color: var(--text-primary);
            border-bottom: 2px solid var(--primary-color);
            padding-bottom: 4px;
          }
          
          .param-item {
            display: flex;
            justify-content: space-between;
            padding: 6px 10px;
            margin-bottom: 6px;
            background: white;
            border-radius: 4px;
            border: 1px solid var(--border-color);
            
            .label {
              font-size: 13px;
              color: var(--text-secondary);
            }
            
            .value {
              font-family: 'Consolas', 'Monaco', monospace;
              color: var(--primary-color);
              font-weight: bold;
              font-size: 13px;
            }
          }
        }
      }
    }
    
    .no-measurement {
      text-align: center;
      padding: 40px 20px;
      color: var(--text-secondary);
      font-style: italic;
    }
  }
}

@media (max-width: 768px) {
  .professional-easy-chart {
    .chart-toolbar {
      flex-direction: column;
      gap: 12px;
      
      .toolbar-section {
        flex-wrap: wrap;
      }
    }
    
    .measurement-panel {
      .measurement-content {
        .measurement-grid {
          grid-template-columns: 1fr;
        }
        
        .auto-measurement-grid {
          grid-template-columns: 1fr;
        }
        
        .frequency-info {
          grid-template-columns: 1fr;
        }
      }
    }
  }
}
</style>
