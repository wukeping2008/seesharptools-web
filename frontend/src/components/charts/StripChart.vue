<template>
  <div class="strip-chart-wrapper professional-control">
    <!-- 工具栏 -->
    <div class="chart-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <el-button-group>
          <el-button 
            size="small" 
            :type="isRealtime ? 'success' : ''"
            @click="toggleRealtime"
          >
            <el-icon><VideoPlay v-if="!isRealtime" /><VideoPause v-else /></el-icon>
            {{ isRealtime ? '暂停' : '开始' }}
          </el-button>
          <el-button size="small" @click="resetView">
            <el-icon><Refresh /></el-icon>
            重置
          </el-button>
          <el-button size="small" @click="clearData">
            <el-icon><Delete /></el-icon>
            清除
          </el-button>
        </el-button-group>
        
        <el-select v-model="timeWindow" @change="onTimeWindowChange" size="small" style="width: 120px; margin-left: 12px;">
          <el-option label="1秒" value="1s" />
          <el-option label="5秒" value="5s" />
          <el-option label="10秒" value="10s" />
          <el-option label="30秒" value="30s" />
          <el-option label="1分钟" value="1m" />
          <el-option label="5分钟" value="5m" />
        </el-select>
      </div>
      
      <div class="toolbar-right">
        <el-tooltip content="通道配置">
          <el-button size="small" @click="showChannelConfig = true">
            <el-icon><Setting /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="导出数据">
          <el-button size="small" @click="exportData">
            <el-icon><Download /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="全屏显示">
          <el-button size="small" @click="toggleFullscreen">
            <el-icon><FullScreen /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>

    <!-- 主图表区域 -->
    <div 
      ref="chartContainer" 
      class="chart-container"
      :class="{ fullscreen: isFullscreen }"
    ></div>

    <!-- 通道列表 -->
    <div class="channel-panel" v-if="showChannels">
      <div class="panel-header">
        <h4>通道配置</h4>
        <el-button size="small" @click="addChannel">
          <el-icon><Plus /></el-icon>
          添加通道
        </el-button>
      </div>
      
      <div class="channel-list">
        <div 
          v-for="(channel, index) in channels" 
          :key="channel.id"
          class="channel-item"
          :class="{ active: channel.visible }"
        >
          <div class="channel-controls">
            <el-checkbox 
              v-model="channel.visible"
              @change="updateChannelVisibility(index)"
            />
            <div 
              class="channel-color"
              :style="{ backgroundColor: channel.color }"
              @click="showColorPicker(index)"
            />
            <el-input 
              v-model="channel.name" 
              size="small" 
              style="width: 80px;"
              @change="updateChart"
            />
          </div>
          
          <div class="channel-stats">
            <span class="stat-label">当前值:</span>
            <span class="stat-value">{{ formatValue(channel.currentValue, channel.unit) }}</span>
            <span class="stat-label">最大:</span>
            <span class="stat-value">{{ formatValue(channel.maxValue, channel.unit) }}</span>
            <span class="stat-label">最小:</span>
            <span class="stat-value">{{ formatValue(channel.minValue, channel.unit) }}</span>
          </div>
          
          <div class="channel-actions">
            <el-button size="small" @click="resetChannelStats(index)">
              <el-icon><Refresh /></el-icon>
            </el-button>
            <el-button size="small" type="danger" @click="removeChannel(index)">
              <el-icon><Delete /></el-icon>
            </el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- 状态栏 -->
    <div class="status-bar" v-if="showStatus">
      <div class="status-left">
        <span>数据率: {{ dataRate.toFixed(1) }} Hz</span>
        <span>缓冲区: {{ bufferUsage.toFixed(1) }}%</span>
        <span>通道数: {{ activeChannelCount }}</span>
        <span>数据点: {{ totalDataPoints }}</span>
      </div>
      <div class="status-right">
        <span>渲染: {{ renderFps }} fps</span>
        <span>内存: {{ memoryUsage.toFixed(1) }} MB</span>
        <span>时间: {{ currentTime }}</span>
      </div>
    </div>

    <!-- 通道配置对话框 -->
    <el-dialog v-model="showChannelConfig" title="通道配置" width="600px">
      <div class="channel-config">
        <el-table :data="channels" style="width: 100%">
          <el-table-column prop="name" label="名称" width="100">
            <template #default="scope">
              <el-input v-model="scope.row.name" size="small" />
            </template>
          </el-table-column>
          <el-table-column prop="color" label="颜色" width="80">
            <template #default="scope">
              <el-color-picker v-model="scope.row.color" size="small" />
            </template>
          </el-table-column>
          <el-table-column prop="unit" label="单位" width="80">
            <template #default="scope">
              <el-input v-model="scope.row.unit" size="small" />
            </template>
          </el-table-column>
          <el-table-column prop="scale" label="缩放" width="100">
            <template #default="scope">
              <el-input-number v-model="scope.row.scale" :step="0.1" size="small" />
            </template>
          </el-table-column>
          <el-table-column prop="offset" label="偏移" width="100">
            <template #default="scope">
              <el-input-number v-model="scope.row.offset" :step="0.1" size="small" />
            </template>
          </el-table-column>
          <el-table-column prop="visible" label="显示" width="60">
            <template #default="scope">
              <el-checkbox v-model="scope.row.visible" />
            </template>
          </el-table-column>
          <el-table-column label="操作" width="80">
            <template #default="scope">
              <el-button size="small" type="danger" @click="removeChannel(scope.$index)">
                删除
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
      <template #footer>
        <el-button @click="showChannelConfig = false">取消</el-button>
        <el-button type="primary" @click="saveChannelConfig">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed, nextTick } from 'vue'
import * as echarts from 'echarts'
import { 
  VideoPlay, VideoPause, Refresh, Delete, Setting, Download, 
  FullScreen, Plus 
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

// 数据结构定义
interface ChannelData {
  id: number
  name: string
  color: string
  unit: string
  visible: boolean
  data: CircularBuffer<DataPoint>
  currentValue: number
  maxValue: number
  minValue: number
  scale: number
  offset: number
}

interface DataPoint {
  timestamp: number
  value: number
  quality: number
}

interface StripChartOptions {
  bufferSize: number
  maxChannels: number
  sampleRate: number
  timeWindow: string
  autoScale: boolean
  showGrid: boolean
  theme: 'light' | 'dark'
}

// 环形缓冲区实现
class CircularBuffer<T> {
  private buffer: T[]
  private head: number = 0
  private tail: number = 0
  private size: number = 0
  private capacity: number

  constructor(capacity: number) {
    this.capacity = capacity
    this.buffer = new Array(capacity)
  }

  push(item: T): void {
    this.buffer[this.tail] = item
    this.tail = (this.tail + 1) % this.capacity
    
    if (this.size < this.capacity) {
      this.size++
    } else {
      this.head = (this.head + 1) % this.capacity
    }
  }

  getAll(): T[] {
    const result: T[] = []
    for (let i = 0; i < this.size; i++) {
      const index = (this.head + i) % this.capacity
      result.push(this.buffer[index])
    }
    return result
  }

  getTimeRange(startTime: number, endTime: number): T[] {
    const result: T[] = []
    for (let i = 0; i < this.size; i++) {
      const index = (this.head + i) % this.capacity
      const item = this.buffer[index] as any
      if (item.timestamp >= startTime && item.timestamp <= endTime) {
        result.push(item)
      }
    }
    return result
  }

  clear(): void {
    this.head = 0
    this.tail = 0
    this.size = 0
  }

  getSize(): number {
    return this.size
  }

  getCapacity(): number {
    return this.capacity
  }
}

// Props
interface Props {
  options?: Partial<StripChartOptions>
  showToolbar?: boolean
  showChannels?: boolean
  showStatus?: boolean
  width?: string | number
  height?: string | number
  deviceId?: string
}

const props = withDefaults(defineProps<Props>(), {
  showToolbar: true,
  showChannels: true,
  showStatus: true,
  width: '100%',
  height: '400px'
})

// Emits
const emit = defineEmits<{
  dataUpdate: [channels: ChannelData[]]
  channelAdd: [channel: ChannelData]
  channelRemove: [channelId: number]
  realtimeToggle: [enabled: boolean]
}>()

// 响应式数据
const chartContainer = ref<HTMLDivElement>()
const chart = ref<echarts.ECharts>()
const isFullscreen = ref(false)
const isRealtime = ref(true)
const showChannelConfig = ref(false)
const timeWindow = ref('10s')
const currentTime = ref('')

// 默认配置
const defaultOptions: StripChartOptions = {
  bufferSize: 1000000, // 100万点缓冲
  maxChannels: 32,
  sampleRate: 1000, // 1kHz
  timeWindow: '10s',
  autoScale: true,
  showGrid: true,
  theme: 'light'
}

const localOptions = ref<StripChartOptions>({ ...defaultOptions, ...props.options })

// 通道数据
const channels = ref<ChannelData[]>([
  {
    id: 0,
    name: 'CH1',
    color: '#409eff',
    unit: 'V',
    visible: true,
    data: new CircularBuffer<DataPoint>(localOptions.value.bufferSize),
    currentValue: 0,
    maxValue: -Infinity,
    minValue: Infinity,
    scale: 1,
    offset: 0
  },
  {
    id: 1,
    name: 'CH2',
    color: '#67c23a',
    unit: 'V',
    visible: true,
    data: new CircularBuffer<DataPoint>(localOptions.value.bufferSize),
    currentValue: 0,
    maxValue: -Infinity,
    minValue: Infinity,
    scale: 1,
    offset: 0
  }
])

// 性能监控
const dataRate = ref(0)
const bufferUsage = ref(0)
const renderFps = ref(60)
const memoryUsage = ref(0)

// 数据生成定时器
let dataTimer: number | null = null
let timeTimer: number | null = null
let fpsTimer: number | null = null
let frameCount = 0

// 计算属性
const activeChannelCount = computed(() => {
  return channels.value.filter(ch => ch.visible).length
})

const totalDataPoints = computed(() => {
  return channels.value.reduce((total, ch) => total + ch.data.getSize(), 0)
})

// 时间窗口转换为毫秒
const getTimeWindowMs = (window: string): number => {
  const value = parseInt(window)
  const unit = window.slice(-1)
  switch (unit) {
    case 's': return value * 1000
    case 'm': return value * 60 * 1000
    case 'h': return value * 60 * 60 * 1000
    default: return 10000
  }
}

// 格式化数值
const formatValue = (value: number, unit: string): string => {
  if (Math.abs(value) === Infinity || isNaN(value)) return '---'
  return `${value.toFixed(3)}${unit}`
}

// 初始化图表
const initChart = () => {
  if (!chartContainer.value) return

  if (chart.value) {
    chart.value.dispose()
  }

  chart.value = echarts.init(chartContainer.value, localOptions.value.theme)
  updateChart()
  bindEvents()
}

// 更新图表
const updateChart = () => {
  if (!chart.value) return

  const now = Date.now()
  const windowMs = getTimeWindowMs(timeWindow.value)
  const startTime = now - windowMs

  const series = channels.value
    .filter(ch => ch.visible)
    .map(channel => {
      const data = channel.data.getTimeRange(startTime, now)
      const chartData = data.map(point => [
        point.timestamp,
        point.value * channel.scale + channel.offset
      ])

      return {
        name: channel.name,
        type: 'line',
        data: chartData,
        lineStyle: {
          color: channel.color,
          width: 2
        },
        symbol: 'none',
        smooth: false,
        animation: false
      }
    })

  const option = {
    title: {
      show: false
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'cross'
      },
      formatter: (params: any) => {
        let result = `时间: ${new Date(params[0].axisValue).toLocaleTimeString()}<br/>`
        params.forEach((param: any) => {
          result += `${param.seriesName}: ${param.value[1].toFixed(3)}<br/>`
        })
        return result
      }
    },
    legend: {
      show: true,
      top: 10,
      type: 'scroll'
    },
    grid: {
      left: '10%',
      right: '10%',
      bottom: '15%',
      top: '15%',
      containLabel: true
    },
    xAxis: {
      type: 'time',
      boundaryGap: false,
      axisLine: { show: true },
      axisTick: { show: true },
      axisLabel: { 
        show: true,
        formatter: (value: number) => {
          return new Date(value).toLocaleTimeString()
        }
      },
      splitLine: { 
        show: localOptions.value.showGrid,
        lineStyle: { type: 'solid', color: '#e0e6ed' }
      },
      min: startTime,
      max: now
    },
    yAxis: {
      type: 'value',
      scale: localOptions.value.autoScale,
      axisLine: { show: true },
      axisTick: { show: true },
      axisLabel: { show: true },
      splitLine: { 
        show: localOptions.value.showGrid,
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

  chart.value.setOption(option, true)
  frameCount++
}

// 绑定事件
const bindEvents = () => {
  if (!chart.value) return

  chart.value.on('legendselectchanged', (params: any) => {
    Object.keys(params.selected).forEach((name) => {
      const channel = channels.value.find(ch => ch.name === name)
      if (channel) {
        channel.visible = params.selected[name]
      }
    })
  })
}

// 生成模拟数据
const generateData = () => {
  if (!isRealtime.value) return

  const now = Date.now()
  
  channels.value.forEach((channel, index) => {
    // 生成模拟数据
    const baseFreq = 0.5 + index * 0.3 // 不同频率
    const amplitude = 2 + index * 0.5
    const noise = (Math.random() - 0.5) * 0.1
    
    const value = amplitude * Math.sin(now * baseFreq / 1000) + noise

    const dataPoint: DataPoint = {
      timestamp: now,
      value,
      quality: 1
    }

    channel.data.push(dataPoint)
    channel.currentValue = value
    channel.maxValue = Math.max(channel.maxValue, value)
    channel.minValue = Math.min(channel.minValue, value)
  })

  // 更新性能指标
  updatePerformanceMetrics()
}

// 更新性能指标
const updatePerformanceMetrics = () => {
  // 计算数据率
  dataRate.value = localOptions.value.sampleRate

  // 计算缓冲区使用率
  const totalUsed = channels.value.reduce((sum, ch) => sum + ch.data.getSize(), 0)
  const totalCapacity = channels.value.length * localOptions.value.bufferSize
  bufferUsage.value = (totalUsed / totalCapacity) * 100

  // 估算内存使用
  memoryUsage.value = (totalUsed * 24) / (1024 * 1024) // 每个数据点约24字节
}

// 控制方法
const toggleRealtime = () => {
  isRealtime.value = !isRealtime.value
  emit('realtimeToggle', isRealtime.value)
  
  if (isRealtime.value) {
    startDataGeneration()
  } else {
    stopDataGeneration()
  }
}

const resetView = () => {
  if (chart.value) {
    chart.value.dispatchAction({
      type: 'dataZoom',
      start: 0,
      end: 100
    })
  }
}

const clearData = () => {
  channels.value.forEach(channel => {
    channel.data.clear()
    channel.currentValue = 0
    channel.maxValue = -Infinity
    channel.minValue = Infinity
  })
  updateChart()
}

const onTimeWindowChange = () => {
  updateChart()
}

const addChannel = () => {
  if (channels.value.length >= localOptions.value.maxChannels) {
    ElMessage.warning(`最多支持${localOptions.value.maxChannels}个通道`)
    return
  }

  const newChannel: ChannelData = {
    id: Date.now(),
    name: `CH${channels.value.length + 1}`,
    color: getChannelColor(channels.value.length),
    unit: 'V',
    visible: true,
    data: new CircularBuffer<DataPoint>(localOptions.value.bufferSize),
    currentValue: 0,
    maxValue: -Infinity,
    minValue: Infinity,
    scale: 1,
    offset: 0
  }

  channels.value.push(newChannel)
  emit('channelAdd', newChannel)
  updateChart()
}

const removeChannel = (index: number) => {
  if (channels.value.length <= 1) {
    ElMessage.warning('至少需要保留一个通道')
    return
  }

  const channel = channels.value[index]
  channels.value.splice(index, 1)
  emit('channelRemove', channel.id)
  updateChart()
}

const getChannelColor = (index: number): string => {
  const colors = [
    '#409eff', '#67c23a', '#e6a23c', '#f56c6c',
    '#909399', '#c71585', '#ff8c00', '#32cd32'
  ]
  return colors[index % colors.length]
}

const updateChannelVisibility = (index: number) => {
  updateChart()
}

const showColorPicker = (index: number) => {
  // 这里可以实现颜色选择器的逻辑
  // 目前通过通道配置对话框来修改颜色
  showChannelConfig.value = true
}

const resetChannelStats = (index: number) => {
  const channel = channels.value[index]
  channel.maxValue = -Infinity
  channel.minValue = Infinity
}

const saveChannelConfig = () => {
  showChannelConfig.value = false
  updateChart()
  ElMessage.success('通道配置已保存')
}

const exportData = () => {
  const now = Date.now()
  const windowMs = getTimeWindowMs(timeWindow.value)
  const startTime = now - windowMs

  let csvContent = 'Timestamp,Time'
  channels.value.forEach(ch => {
    if (ch.visible) {
      csvContent += `,${ch.name}(${ch.unit})`
    }
  })
  csvContent += '\n'

  // 获取时间范围内的所有时间戳
  const allTimestamps = new Set<number>()
  channels.value.forEach(ch => {
    if (ch.visible) {
      const data = ch.data.getTimeRange(startTime, now)
      data.forEach(point => allTimestamps.add(point.timestamp))
    }
  })

  // 按时间排序
  const sortedTimestamps = Array.from(allTimestamps).sort()

  sortedTimestamps.forEach(timestamp => {
    csvContent += `${timestamp},${new Date(timestamp).toISOString()}`
    
    channels.value.forEach(ch => {
      if (ch.visible) {
        const data = ch.data.getTimeRange(timestamp, timestamp)
        const value = data.length > 0 ? data[0].value : ''
        csvContent += `,${value}`
      }
    })
    csvContent += '\n'
  })

  const blob = new Blob([csvContent], { type: 'text/csv' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `stripchart_data_${Date.now()}.csv`
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

const startDataGeneration = () => {
  if (dataTimer) return
  
  dataTimer = setInterval(() => {
    generateData()
    updateChart()
  }, 1000 / localOptions.value.sampleRate)
}

const stopDataGeneration = () => {
  if (dataTimer) {
    clearInterval(dataTimer)
    dataTimer = null
  }
}

const updateTime = () => {
  currentTime.value = new Date().toLocaleTimeString()
}

const updateFPS = () => {
  renderFps.value = frameCount
  frameCount = 0
}

// 监听容器大小变化
const resizeObserver = new ResizeObserver(() => {
  chart.value?.resize()
})

// 生命周期
onMounted(() => {
  initChart()
  startDataGeneration()
  
  // 启动定时器
  timeTimer = setInterval(updateTime, 1000)
  fpsTimer = setInterval(updateFPS, 1000)
  
  if (chartContainer.value) {
    resizeObserver.observe(chartContainer.value)
  }
})

onUnmounted(() => {
  stopDataGeneration()
  
  if (timeTimer) clearInterval(timeTimer)
  if (fpsTimer) clearInterval(fpsTimer)
  
  if (chart.value) {
    chart.value.dispose()
  }
  
  resizeObserver.disconnect()
})

// 监听配置变化
watch(() => props.options, (newOptions) => {
  if (newOptions) {
    localOptions.value = { ...localOptions.value, ...newOptions }
    updateChart()
  }
}, { deep: true })

// 暴露方法
defineExpose({
  addChannel,
  removeChannel,
  clearData,
  toggleRealtime,
  exportData,
  getChart: () => chart.value,
  getChannels: () => channels.value
})
</script>

<style lang="scss" scoped>
.strip-chart-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #fff;
  border: 1px solid #ddd;
  border-radius: 8px;
  overflow: hidden;
  
  .chart-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    border-bottom: 1px solid #eee;
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
  
  .channel-panel {
    width: 300px;
    border-left: 1px solid #eee;
    background: #fafafa;
    display: flex;
    flex-direction: column;
    
    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px;
      border-bottom: 1px solid #eee;
      
      h4 {
        margin: 0;
        font-size: 14px;
        color: #303133;
      }
    }
    
    .channel-list {
      flex: 1;
      overflow-y: auto;
      
      .channel-item {
        padding: 12px;
        border-bottom: 1px solid #eee;
        
        &.active {
          background: #f0f9ff;
        }
        
        .channel-controls {
          display: flex;
          align-items: center;
          gap: 8px;
          margin-bottom: 8px;
          
          .channel-color {
            width: 16px;
            height: 16px;
            border-radius: 2px;
            cursor: pointer;
            border: 1px solid #ddd;
          }
        }
        
        .channel-stats {
          font-size: 12px;
          color: #666;
          margin-bottom: 8px;
          
          .stat-label {
            margin-right: 4px;
          }
          
          .stat-value {
            font-weight: 500;
            color: #333;
            margin-right: 12px;
          }
        }
        
        .channel-actions {
          display: flex;
          gap: 4px;
        }
      }
    }
  }
  
  .status-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 4px 12px;
    font-size: 11px;
    color: #666;
    background: #f5f7fa;
    border-top: 1px solid #eee;
    
    .status-left,
    .status-right {
      display: flex;
      gap: 16px;
    }
    
    span {
      white-space: nowrap;
    }
  }
  
  .channel-config {
    .el-table {
      font-size: 12px;
    }
  }
}

// 响应式设计
@media (max-width: 768px) {
  .strip-chart-wrapper {
    .chart-toolbar {
      flex-direction: column;
      gap: 8px;
      align-items: flex-start;
    }
    
    .channel-panel {
      width: 100%;
      border-left: none;
      border-top: 1px solid #eee;
    }
    
    .status-bar {
      flex-direction: column;
      gap: 4px;
      
      .status-left,
      .status-right {
        justify-content: center;
        flex-wrap: wrap;
      }
    }
  }
}
</style>
