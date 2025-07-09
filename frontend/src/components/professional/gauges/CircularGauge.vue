<template>
  <div class="circular-gauge-wrapper professional-control">
    <!-- 工具栏 -->
    <div class="gauge-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <span class="gauge-title">{{ data.title || '圆形仪表盘' }}</span>
      </div>
      <div class="toolbar-right">
        <el-tooltip content="设置报警">
          <el-button size="small" @click="showAlarmDialog = true">
            <el-icon><Bell /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="历史数据">
          <el-button size="small" @click="showHistoryDialog = true">
            <el-icon><TrendCharts /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="导出数据">
          <el-button size="small" @click="exportData">
            <el-icon><Download /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>

    <!-- 仪表盘容器 -->
    <div 
      ref="gaugeContainer" 
      class="gauge-container"
      :style="containerStyle"
    ></div>

    <!-- 控制面板 -->
    <div class="gauge-controls" v-if="showControls">
      <el-row :gutter="16">
        <el-col :span="8">
          <div class="control-group">
            <label>当前值:</label>
            <el-input-number 
              v-model="currentValue" 
              :min="localAxis.min || 0" 
              :max="localAxis.max || 100"
              :precision="2"
              @change="updateValue"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="8">
          <div class="control-group">
            <label>最小值:</label>
            <el-input-number 
              v-model="minValue" 
              :max="maxValue - 1"
              @change="updateRange"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="8">
          <div class="control-group">
            <label>最大值:</label>
            <el-input-number 
              v-model="maxValue" 
              :min="minValue + 1"
              @change="updateRange"
              size="small"
            />
          </div>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showPointer" @change="updateGauge">
            显示指针
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showProgress" @change="updateGauge">
            显示进度
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showAxis" @change="updateGauge">
            显示刻度
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.animation" @change="updateGauge">
            启用动画
          </el-checkbox>
        </el-col>
      </el-row>
    </div>

    <!-- 状态信息 -->
    <div class="gauge-status" v-if="showStatus">
      <span>当前值: {{ formatValue(data.value) }}</span>
      <span>范围: {{ formatValue(data.min || 0) }} - {{ formatValue(data.max || 100) }}</span>
      <span v-if="currentAlarm">状态: <el-tag :type="currentAlarm.type">{{ currentAlarm.message }}</el-tag></span>
    </div>

    <!-- 报警设置对话框 -->
    <el-dialog v-model="showAlarmDialog" title="报警设置" width="500px">
      <div class="alarm-settings">
        <h4>报警区域设置</h4>
        <div v-for="(zone, index) in alarmZones" :key="index" class="alarm-zone">
          <el-row :gutter="16">
            <el-col :span="6">
              <el-input-number v-model="zone.min" placeholder="最小值" size="small" />
            </el-col>
            <el-col :span="6">
              <el-input-number v-model="zone.max" placeholder="最大值" size="small" />
            </el-col>
            <el-col :span="6">
              <el-color-picker v-model="zone.color" />
            </el-col>
            <el-col :span="4">
              <el-input v-model="zone.label" placeholder="标签" size="small" />
            </el-col>
            <el-col :span="2">
              <el-button size="small" type="danger" @click="removeAlarmZone(index)">
                <el-icon><Delete /></el-icon>
              </el-button>
            </el-col>
          </el-row>
        </div>
        <el-button @click="addAlarmZone" type="primary" size="small">
          <el-icon><Plus /></el-icon>
          添加报警区域
        </el-button>
      </div>
      <template #footer>
        <el-button @click="showAlarmDialog = false">取消</el-button>
        <el-button type="primary" @click="saveAlarmSettings">保存</el-button>
      </template>
    </el-dialog>

    <!-- 历史数据对话框 -->
    <el-dialog v-model="showHistoryDialog" title="历史数据" width="800px">
      <div class="history-chart" ref="historyContainer" style="height: 300px;"></div>
      <template #footer>
        <el-button @click="showHistoryDialog = false">关闭</el-button>
        <el-button type="primary" @click="exportHistory">导出历史数据</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed, nextTick } from 'vue'
import * as echarts from 'echarts'
import { Bell, TrendCharts, Download, Delete, Plus } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import type { 
  GaugeData, 
  GaugeOptions, 
  GaugeStyle,
  PointerConfig,
  ProgressConfig,
  AxisConfig,
  TitleConfig,
  DetailConfig,
  AlarmZone,
  GaugeEvents,
  GaugeHistoryData
} from '@/types/gauge'

// Props
interface Props {
  data: GaugeData
  options?: Partial<GaugeOptions>
  style?: GaugeStyle
  pointer?: Partial<PointerConfig>
  progress?: Partial<ProgressConfig>
  axis?: Partial<AxisConfig>
  title?: Partial<TitleConfig>
  detail?: Partial<DetailConfig>
  alarmZones?: AlarmZone[]
  events?: GaugeEvents
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
  height: '300px'
})

// Emits
const emit = defineEmits<{
  valueChange: [value: number]
  alarmTrigger: [zone: AlarmZone, value: number]
  pointerClick: [value: number]
}>()

// 响应式数据
const gaugeContainer = ref<HTMLDivElement>()
const historyContainer = ref<HTMLDivElement>()
const gauge = ref<echarts.ECharts>()
const historyChart = ref<echarts.ECharts>()
const currentValue = ref(props.data.value)
const minValue = ref(props.data.min || 0)
const maxValue = ref(props.data.max || 100)
const showAlarmDialog = ref(false)
const showHistoryDialog = ref(false)
const alarmZones = ref<AlarmZone[]>(props.alarmZones || [])
const historyData = ref<GaugeHistoryData[]>([])

// 默认配置
const defaultOptions: GaugeOptions = {
  size: 200,
  startAngle: 225,
  endAngle: -45,
  clockwise: true,
  showPointer: true,
  showProgress: true,
  showAxis: true,
  showAxisLabel: true,
  showTitle: true,
  showDetail: true,
  animation: true,
  animationDuration: 1000,
  theme: 'light'
}

const defaultPointer: PointerConfig = {
  show: true,
  length: '80%',
  width: 6,
  color: '#409eff',
  shadowColor: 'rgba(0, 0, 0, 0.3)',
  shadowBlur: 10,
  shadowOffsetX: 2,
  shadowOffsetY: 2
}

const defaultProgress: ProgressConfig = {
  show: true,
  width: 10,
  color: ['#67c23a', '#e6a23c', '#f56c6c'],
  backgroundColor: '#e4e7ed',
  roundCap: true
}

const defaultAxis: AxisConfig = {
  show: true,
  min: 0,
  max: 100,
  splitNumber: 10,
  lineStyle: {
    color: '#909399',
    width: 2,
    type: 'solid'
  },
  label: {
    show: true,
    distance: 15,
    color: '#606266',
    fontSize: 12
  },
  tick: {
    show: true,
    length: 8,
    width: 2,
    color: '#909399'
  },
  minorTick: {
    show: true,
    length: 4,
    width: 1,
    color: '#c0c4cc',
    splitNumber: 5
  }
}

const defaultTitle: TitleConfig = {
  show: true,
  text: '',
  offsetCenter: [0, '20%'],
  color: '#303133',
  fontSize: 16,
  fontWeight: 'bold',
  fontFamily: 'Arial'
}

const defaultDetail: DetailConfig = {
  show: true,
  offsetCenter: [0, '40%'],
  color: '#409eff',
  fontSize: 24,
  fontWeight: 'bold',
  fontFamily: 'Arial',
  formatter: '{value}'
}

// 合并配置
const localOptions = ref<GaugeOptions>({ ...defaultOptions, ...props.options })
const localPointer = ref<PointerConfig>({ ...defaultPointer, ...props.pointer })
const localProgress = ref<ProgressConfig>({ ...defaultProgress, ...props.progress })
const localAxis = ref<AxisConfig>({ ...defaultAxis, ...props.axis })
const localTitle = ref<TitleConfig>({ ...defaultTitle, ...props.title })
const localDetail = ref<DetailConfig>({ ...defaultDetail, ...props.detail })

// 计算属性
const containerStyle = computed(() => ({
  width: typeof props.width === 'number' ? `${props.width}px` : props.width,
  height: typeof props.height === 'number' ? `${props.height}px` : props.height
}))

const currentAlarm = computed(() => {
  const value = currentValue.value
  for (const zone of alarmZones.value) {
    if (value >= zone.min && value <= zone.max) {
      return {
        type: 'danger' as const,
        message: zone.label || '报警'
      }
    }
  }
  return null
})

// 初始化仪表盘
const initGauge = () => {
  if (!gaugeContainer.value) return

  // 销毁现有图表
  if (gauge.value) {
    gauge.value.dispose()
  }

  // 创建新图表
  gauge.value = echarts.init(gaugeContainer.value, localOptions.value.theme)
  
  // 设置初始配置
  updateGauge()
  
  // 绑定事件
  bindEvents()
}

// 更新仪表盘
const updateGauge = () => {
  if (!gauge.value) return

  const option = generateGaugeOption()
  gauge.value.setOption(option, true)
}

// 生成仪表盘配置
const generateGaugeOption = () => {
  const series: any[] = []
  
  // 主仪表盘
  const mainGauge: any = {
    type: 'gauge',
    center: props.style?.center || ['50%', '50%'],
    radius: props.style?.radius || '80%',
    min: localAxis.value.min,
    max: localAxis.value.max,
    splitNumber: localAxis.value.splitNumber,
    startAngle: localOptions.value.startAngle,
    endAngle: localOptions.value.endAngle,
    clockwise: localOptions.value.clockwise,
    data: [{
      value: currentValue.value,
      name: props.data.title || '',
      title: {
        show: localOptions.value.showTitle && localTitle.value.show,
        offsetCenter: localTitle.value.offsetCenter,
        color: localTitle.value.color,
        fontSize: localTitle.value.fontSize,
        fontWeight: localTitle.value.fontWeight,
        fontFamily: localTitle.value.fontFamily
      },
      detail: {
        show: localOptions.value.showDetail && localDetail.value.show,
        offsetCenter: localDetail.value.offsetCenter,
        color: localDetail.value.color,
        fontSize: localDetail.value.fontSize,
        fontWeight: localDetail.value.fontWeight,
        fontFamily: localDetail.value.fontFamily,
        formatter: (value: number) => {
          if (typeof localDetail.value.formatter === 'function') {
            return localDetail.value.formatter(value)
          }
          return `${value.toFixed(1)}${props.data.unit || ''}`
        }
      }
    }],
    pointer: {
      show: localOptions.value.showPointer && localPointer.value.show,
      length: localPointer.value.length,
      width: localPointer.value.width,
      itemStyle: {
        color: localPointer.value.color,
        shadowColor: localPointer.value.shadowColor,
        shadowBlur: localPointer.value.shadowBlur,
        shadowOffsetX: localPointer.value.shadowOffsetX,
        shadowOffsetY: localPointer.value.shadowOffsetY
      }
    },
    progress: {
      show: localOptions.value.showProgress && localProgress.value.show,
      width: localProgress.value.width,
      itemStyle: {
        color: localProgress.value.color
      },
      roundCap: localProgress.value.roundCap
    },
    axisLine: {
      show: localOptions.value.showAxis,
      lineStyle: {
        width: localProgress.value.width,
        color: [[1, localProgress.value.backgroundColor || '#e4e7ed']]
      }
    },
    axisTick: {
      show: localOptions.value.showAxis && localAxis.value.tick.show,
      length: localAxis.value.tick.length,
      lineStyle: {
        color: localAxis.value.tick.color,
        width: localAxis.value.tick.width
      }
    },
    splitLine: {
      show: localOptions.value.showAxis,
      length: localAxis.value.tick.length * 1.5,
      lineStyle: {
        color: localAxis.value.lineStyle.color,
        width: localAxis.value.lineStyle.width,
        type: localAxis.value.lineStyle.type
      }
    },
    axisLabel: {
      show: localOptions.value.showAxisLabel && localAxis.value.label.show,
      distance: localAxis.value.label.distance,
      color: localAxis.value.label.color,
      fontSize: localAxis.value.label.fontSize,
      formatter: localAxis.value.label.formatter
    },
    animation: localOptions.value.animation,
    animationDuration: localOptions.value.animationDuration
  }

  series.push(mainGauge)

  // 添加报警区域
  if (alarmZones.value.length > 0) {
    alarmZones.value.forEach((zone, index) => {
      const alarmGauge = {
        type: 'gauge',
        center: props.style?.center || ['50%', '50%'],
        radius: `${(80 - index * 5)}%`,
        min: localAxis.value.min,
        max: localAxis.value.max,
        startAngle: localOptions.value.startAngle,
        endAngle: localOptions.value.endAngle,
        clockwise: localOptions.value.clockwise,
        data: [{ value: zone.max, name: zone.label || '' }],
        pointer: { show: false },
        progress: { show: false },
        axisLine: {
          show: true,
          lineStyle: {
            width: 3,
            color: [
              [zone.min / localAxis.value.max, 'transparent'],
              [zone.max / localAxis.value.max, zone.color],
              [1, 'transparent']
            ]
          }
        },
        axisTick: { show: false },
        splitLine: { show: false },
        axisLabel: { show: false },
        detail: { show: false },
        title: { show: false }
      }
      series.push(alarmGauge)
    })
  }

  return {
    series,
    backgroundColor: props.style?.backgroundColor || 'transparent'
  }
}

// 绑定事件
const bindEvents = () => {
  if (!gauge.value) return

  gauge.value.on('click', (params: any) => {
    if (params.componentType === 'series') {
      emit('pointerClick', params.value)
      props.events?.onPointerClick?.(params.value)
    }
  })
}

// 更新数值
const updateValue = (value: number) => {
  currentValue.value = value
  updateGauge()
  emit('valueChange', value)
  props.events?.onValueChange?.(value)
  
  // 记录历史数据
  addHistoryData(value)
  
  // 检查报警
  checkAlarms(value)
}

// 更新范围
const updateRange = () => {
  localAxis.value.min = minValue.value
  localAxis.value.max = maxValue.value
  updateGauge()
}

// 添加历史数据
const addHistoryData = (value: number) => {
  historyData.value.push({
    timestamp: new Date(),
    value,
    min: minValue.value,
    max: maxValue.value
  })
  
  // 保持最近100条记录
  if (historyData.value.length > 100) {
    historyData.value.shift()
  }
}

// 检查报警
const checkAlarms = (value: number) => {
  for (const zone of alarmZones.value) {
    if (value >= zone.min && value <= zone.max) {
      emit('alarmTrigger', zone, value)
      props.events?.onAlarmTrigger?.(zone, value)
      break
    }
  }
}

// 添加报警区域
const addAlarmZone = () => {
  alarmZones.value.push({
    min: 80,
    max: 100,
    color: '#f56c6c',
    label: '危险'
  })
}

// 移除报警区域
const removeAlarmZone = (index: number) => {
  alarmZones.value.splice(index, 1)
}

// 保存报警设置
const saveAlarmSettings = () => {
  updateGauge()
  showAlarmDialog.value = false
  ElMessage.success('报警设置已保存')
}

// 格式化数值
const formatValue = (value: number) => {
  return `${value.toFixed(1)}${props.data.unit || ''}`
}

// 导出数据
const exportData = () => {
  const data = {
    current: {
      value: currentValue.value,
      timestamp: new Date(),
      unit: props.data.unit
    },
    range: {
      min: minValue.value,
      max: maxValue.value
    },
    alarms: alarmZones.value
  }
  
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `gauge_data_${Date.now()}.json`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

// 导出历史数据
const exportHistory = () => {
  let csvContent = 'Timestamp,Value,Min,Max\n'
  historyData.value.forEach(record => {
    csvContent += `${record.timestamp.toISOString()},${record.value},${record.min},${record.max}\n`
  })
  
  const blob = new Blob([csvContent], { type: 'text/csv' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `gauge_history_${Date.now()}.csv`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

// 初始化历史图表
const initHistoryChart = () => {
  if (!historyContainer.value) return
  
  historyChart.value = echarts.init(historyContainer.value)
  
  const option = {
    title: {
      text: '历史数据趋势',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    xAxis: {
      type: 'time'
    },
    yAxis: {
      type: 'value',
      min: minValue.value,
      max: maxValue.value
    },
    series: [{
      name: '数值',
      type: 'line',
      data: historyData.value.map(item => [item.timestamp, item.value]),
      smooth: true,
      lineStyle: {
        color: '#409eff'
      }
    }]
  }
  
  historyChart.value.setOption(option)
}

// 监听数据变化
watch(() => props.data, (newData) => {
  currentValue.value = newData.value
  updateGauge()
}, { deep: true })

// 监听历史对话框
watch(showHistoryDialog, (show) => {
  if (show) {
    nextTick(() => {
      initHistoryChart()
    })
  }
})

// 监听容器大小变化
const resizeObserver = new ResizeObserver(() => {
  gauge.value?.resize()
  historyChart.value?.resize()
})

// 生命周期
onMounted(() => {
  initGauge()
  if (gaugeContainer.value) {
    resizeObserver.observe(gaugeContainer.value)
  }
})

onUnmounted(() => {
  if (gauge.value) {
    gauge.value.dispose()
  }
  if (historyChart.value) {
    historyChart.value.dispose()
  }
  resizeObserver.disconnect()
})

// 暴露方法
defineExpose({
  updateValue,
  updateRange,
  exportData,
  getGauge: () => gauge.value,
  getHistoryData: () => historyData.value
})
</script>

<style lang="scss" scoped>
.circular-gauge-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  
  .gauge-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    border-bottom: 1px solid #e4e7ed;
    background: #fafafa;
    
    .gauge-title {
      font-weight: 500;
      color: #303133;
    }
    
    .toolbar-left,
    .toolbar-right {
      display: flex;
      align-items: center;
      gap: 8px;
    }
  }
  
  .gauge-container {
    flex: 1;
    min-height: 200px;
  }
  
  .gauge-controls {
    padding: 12px;
    border-top: 1px solid #e4e7ed;
    background: #fafafa;
    
    .control-group {
      display: flex;
      flex-direction: column;
      gap: 4px;
      
      label {
        font-size: 12px;
        font-weight: 500;
        color: #606266;
      }
    }
  }
  
  .gauge-status {
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
  
  .alarm-settings {
    .alarm-zone {
      margin-bottom: 12px;
      padding: 8px;
      border: 1px solid #e4e7ed;
      border-radius: 4px;
    }
  }
}

@media (max-width: 768px) {
  .circular-gauge-wrapper {
    .gauge-toolbar {
      flex-direction: column;
      gap: 8px;
      align-items: flex-start;
    }
    
    .gauge-status {
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
