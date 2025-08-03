<template>
  <div class="enhanced-strip-chart professional-control">
    <!-- 工具栏 -->
    <div class="chart-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <el-button-group>
          <el-button 
            :type="isRealtime ? 'success' : ''"
            @click="toggleRealtime"
            size="small"
          >
            <el-icon><VideoPlay v-if="!isRealtime" /><VideoPause v-else /></el-icon>
            {{ isRealtime ? '暂停' : '开始' }}
          </el-button>
          <el-button @click="resetView" size="small">
            <el-icon><Refresh /></el-icon>
            重置
          </el-button>
          <el-button @click="clearData" size="small">
            <el-icon><Delete /></el-icon>
            清除
          </el-button>
        </el-button-group>
        
        <el-select v-model="timeWindow" @change="onTimeWindowChange" size="small" style="width: 120px; margin-left: 12px;">
          <el-option label="100ms" value="100ms" />
          <el-option label="1秒" value="1s" />
          <el-option label="5秒" value="5s" />
          <el-option label="10秒" value="10s" />
          <el-option label="30秒" value="30s" />
          <el-option label="1分钟" value="1m" />
        </el-select>
      </div>
      
      <div class="toolbar-right">
        <el-tooltip content="性能监控">
          <el-button size="small" @click="showPerformancePanel = !showPerformancePanel">
            <el-icon><Monitor /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="通道配置">
          <el-button size="small" @click="showChannelConfig = true">
            <el-icon><Setting /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>

    <!-- 主图表区域 -->
    <div class="chart-main" :class="{ fullscreen: isFullscreen }">
      <canvas 
        ref="chartCanvas"
        class="chart-canvas"
        @mousedown="onMouseDown"
        @mousemove="onMouseMove"
        @mouseup="onMouseUp"
        @wheel="onWheel"
        @contextmenu.prevent
      />
      
      <!-- 性能监控面板 -->
      <div v-if="showPerformancePanel" class="performance-panel">
        <div class="panel-header">
          <h4>性能监控</h4>
          <el-button size="small" @click="showPerformancePanel = false">
            <el-icon><Close /></el-icon>
          </el-button>
        </div>
        <div class="performance-metrics">
          <div class="metric-item">
            <span class="metric-label">渲染FPS:</span>
            <span class="metric-value" :class="{ warning: renderFps < 30, error: renderFps < 15 }">
              {{ renderFps }}
            </span>
          </div>
          <div class="metric-item">
            <span class="metric-label">数据率:</span>
            <span class="metric-value">{{ formatDataRate(dataRate) }}</span>
          </div>
          <div class="metric-item">
            <span class="metric-label">数据点数:</span>
            <span class="metric-value">{{ formatNumber(totalDataPoints) }}</span>
          </div>
        </div>
      </div>
    </div>

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
              :style="{ backgroundColor: channel.colorHex }"
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
            <span class="stat-value">{{ formatValue(channel.currentValue) }}</span>
            <span class="stat-label">最大:</span>
            <span class="stat-value">{{ formatValue(channel.maxValue) }}</span>
            <span class="stat-label">最小:</span>
            <span class="stat-value">{{ formatValue(channel.minValue) }}</span>
          </div>
          
          <div class="channel-actions">
            <el-button size="small" @click="resetChannelStats(index)">
              <el-icon><Refresh /></el-icon>
            </el-button>
            <el-button size="small" type="danger" @click="removeChannel(index)" v-if="channels.length > 1">
              <el-icon><Delete /></el-icon>
            </el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- 状态栏 -->
    <div class="status-bar" v-if="showStatus">
      <div class="status-left">
        <span>数据率: {{ formatDataRate(dataRate) }}</span>
        <span>通道数: {{ activeChannelCount }}</span>
        <span>数据点: {{ formatNumber(totalDataPoints) }}</span>
      </div>
      <div class="status-right">
        <span>渲染: {{ renderFps }} fps</span>
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
              <el-color-picker 
                v-model="scope.row.colorHex" 
                size="small" 
                @change="updateChannelColor(scope.$index)"
              />
            </template>
          </el-table-column>
          <el-table-column prop="lineWidth" label="线宽" width="80">
            <template #default="scope">
              <el-input-number v-model="scope.row.lineWidth" :min="1" :max="10" size="small" />
            </template>
          </el-table-column>
          <el-table-column prop="visible" label="显示" width="60">
            <template #default="scope">
              <el-checkbox v-model="scope.row.visible" />
            </template>
          </el-table-column>
          <el-table-column label="操作" width="80">
            <template #default="scope">
              <el-button size="small" type="danger" @click="removeChannel(scope.$index)" v-if="channels.length > 1">
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
import { ref, onMounted, onUnmounted, computed, nextTick } from 'vue'
import { 
  VideoPlay, VideoPause, Refresh, Delete, Setting,
  Plus, Monitor, Close
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

// 通道配置接口
interface ChannelConfig {
  id: number
  name: string
  colorHex: string
  visible: boolean
  lineWidth: number
  currentValue: number
  maxValue: number
  minValue: number
}

// Props
interface Props {
  maxChannels?: number
  bufferSize?: number
  maxSampleRate?: number
  showToolbar?: boolean
  showChannels?: boolean
  showStatus?: boolean
  width?: string | number
  height?: string | number
}

const props = withDefaults(defineProps<Props>(), {
  maxChannels: 32,
  bufferSize: 1000000,
  maxSampleRate: 1000000000,
  showToolbar: true,
  showChannels: true,
  showStatus: true,
  width: '100%',
  height: '500px'
})

// Emits
const emit = defineEmits<{
  dataUpdate: [channels: ChannelConfig[]]
  channelAdd: [channel: ChannelConfig]
  channelRemove: [channelId: number]
  realtimeToggle: [enabled: boolean]
  performanceUpdate: [metrics: any]
}>()

// 响应式数据
const chartCanvas = ref<HTMLCanvasElement>()
const isFullscreen = ref(false)
const isRealtime = ref(true)
const showChannelConfig = ref(false)
const showPerformancePanel = ref(false)
const timeWindow = ref('10s')
const currentTime = ref('')

// Canvas 2D 渲染
let ctx: CanvasRenderingContext2D | null = null
let animationId: number | null = null

// 通道数据
const channels = ref<ChannelConfig[]>([
  {
    id: 0,
    name: 'CH1',
    colorHex: '#FFD700',
    visible: true,
    lineWidth: 2,
    currentValue: 0,
    maxValue: -Infinity,
    minValue: Infinity
  },
  {
    id: 1,
    name: 'CH2',
    colorHex: '#00CED1',
    visible: true,
    lineWidth: 2,
    currentValue: 0,
    maxValue: -Infinity,
    minValue: Infinity
  }
])

// 性能监控
const renderFps = ref(60)
const dataRate = ref(1000000) 
const totalDataPoints = ref(0)

// 数据存储
const channelData = ref<Map<number, Array<{x: number, y: number}>>>(new Map())

// 定时器
let dataTimer: number | null = null
let timeTimer: number | null = null
let frameTime = 0
let frameCount = 0

// 计算属性
const activeChannelCount = computed(() => {
  return channels.value.filter(ch => ch.visible).length
})

// 时间窗口转换为毫秒
const getTimeWindowMs = (window: string): number => {
  const value = parseInt(window)
  const unit = window.slice(-2)
  switch (unit) {
    case 'ms': return value
    case 's': return value * 1000
    case 'm': return value * 60 * 1000
    case 'h': return value * 60 * 60 * 1000
    default: return 10000
  }
}

// 初始化Canvas
const initialize = () => {
  if (!chartCanvas.value) return

  ctx = chartCanvas.value.getContext('2d')
  if (!ctx) {
    ElMessage.error('无法获取Canvas 2D上下文')
    return
  }

  // 初始化数据存储
  channels.value.forEach(channel => {
    channelData.value.set(channel.id, [])
  })

  updateCanvasSize()
  startDataGeneration()
  startRendering()
  
  ElMessage.success('StripChart初始化成功')
}

// 更新Canvas尺寸
const updateCanvasSize = () => {
  if (!chartCanvas.value || !ctx) return

  const canvas = chartCanvas.value
  const rect = canvas.getBoundingClientRect()
  
  canvas.width = rect.width
  canvas.height = rect.height
  canvas.style.width = rect.width + 'px'
  canvas.style.height = rect.height + 'px'
}

// 开始数据生成
const startDataGeneration = () => {
  if (dataTimer) return
  
  dataTimer = setInterval(() => {
    if (!isRealtime.value) return
    
    generateTestData()
  }, 16) // ~60fps 数据更新
}

// 生成测试数据
const generateTestData = () => {
  const now = Date.now()
  const windowMs = getTimeWindowMs(timeWindow.value)
  
  channels.value.forEach((channel, index) => {
    if (!channel.visible) return
    
    const data = channelData.value.get(channel.id) || []
    
    // 生成新的数据点
    let value = 0
    switch (index) {
      case 0:
        value = Math.sin(now * 0.005) * 5 + Math.random() * 0.5
        break
      case 1:
        value = Math.cos(now * 0.003) * 3 + Math.sin(now * 0.01) * 2
        break
      default:
        value = Math.sin(now * 0.002 * (index + 1)) * (3 + index)
    }
    
    // 添加新数据点
    data.push({ x: now, y: value })
    
    // 移除过期数据点
    const cutoffTime = now - windowMs
    while (data.length > 0 && data[0].x < cutoffTime) {
      data.shift()
    }
    
    // 限制数据点数量
    if (data.length > 1000) {
      data.splice(0, data.length - 1000)
    }
    
    // 更新统计
    channel.currentValue = value
    channel.maxValue = Math.max(channel.maxValue, value)
    channel.minValue = Math.min(channel.minValue, value)
    
    channelData.value.set(channel.id, data)
  })
  
  // 更新总数据点数和数据率
  let total = 0
  channelData.value.forEach(data => total += data.length)
  totalDataPoints.value = total
  
  // 更新数据率统计 (每秒数据点数)
  updateDataRate()
  
  // 发送性能更新事件
  emit('performanceUpdate', {
    renderFps: renderFps.value,
    dataRate: dataRate.value,
    totalDataPoints: totalDataPoints.value
  })
}

// 数据率统计
let lastDataTime = 0
let dataPointCount = 0

const updateDataRate = () => {
  const now = performance.now()
  dataPointCount += channels.value.filter(ch => ch.visible).length // 每个可见通道一个数据点
  
  if (now - lastDataTime >= 1000) { // 每秒更新一次
    dataRate.value = Math.round((dataPointCount * 1000) / (now - lastDataTime))
    dataPointCount = 0
    lastDataTime = now
  }
}

// 开始渲染
const startRendering = () => {
  if (animationId) return
  
  const render = (timestamp: number) => {
    // 计算FPS
    if (timestamp - frameTime >= 1000) {
      renderFps.value = Math.round((frameCount * 1000) / (timestamp - frameTime))
      frameTime = timestamp
      frameCount = 0
    }
    frameCount++
    
    drawChart()
    animationId = requestAnimationFrame(render)
  }
  
  animationId = requestAnimationFrame(render)
}

// 绘制图表
const drawChart = () => {
  if (!ctx || !chartCanvas.value) return
  
  const canvas = chartCanvas.value
  const width = canvas.width
  const height = canvas.height
  
  // 清除画布
  ctx.fillStyle = '#ffffff'
  ctx.fillRect(0, 0, width, height)
  
  // 绘制网格
  drawGrid(ctx, width, height)
  
  // 绘制波形
  channels.value.forEach(channel => {
    if (!channel.visible) return
    
    const data = channelData.value.get(channel.id) || []
    if (data.length < 2) return
    
    drawWaveform(ctx!, data, channel, width, height)
  })
}

// 绘制网格
const drawGrid = (ctx: CanvasRenderingContext2D, width: number, height: number) => {
  ctx.strokeStyle = '#f0f0f0'
  ctx.lineWidth = 1
  
  // 垂直网格线
  const gridCountX = 10
  for (let i = 0; i <= gridCountX; i++) {
    const x = (width / gridCountX) * i
    ctx.beginPath()
    ctx.moveTo(x, 0)
    ctx.lineTo(x, height)
    ctx.stroke()
  }
  
  // 水平网格线
  const gridCountY = 8
  for (let i = 0; i <= gridCountY; i++) {
    const y = (height / gridCountY) * i
    ctx.beginPath()
    ctx.moveTo(0, y)
    ctx.lineTo(width, y)
    ctx.stroke()
  }
}

// 绘制波形
const drawWaveform = (
  ctx: CanvasRenderingContext2D, 
  data: Array<{x: number, y: number}>, 
  channel: ChannelConfig, 
  width: number, 
  height: number
) => {
  if (data.length < 2) return
  
  const now = Date.now()
  const windowMs = getTimeWindowMs(timeWindow.value)
  const startTime = now - windowMs
  
  // 找到值的范围
  let minY = Infinity, maxY = -Infinity
  data.forEach(point => {
    minY = Math.min(minY, point.y)
    maxY = Math.max(maxY, point.y)
  })
  
  // 添加一些边距
  const range = maxY - minY
  if (range > 0) {
    minY -= range * 0.1
    maxY += range * 0.1
  } else {
    minY -= 1
    maxY += 1
  }
  
  ctx.strokeStyle = channel.colorHex
  ctx.lineWidth = channel.lineWidth
  ctx.beginPath()
  
  let firstPoint = true
  data.forEach((point, index) => {
    const x = ((point.x - startTime) / windowMs) * width
    const y = height - ((point.y - minY) / (maxY - minY)) * height
    
    if (firstPoint) {
      ctx.moveTo(x, y)
      firstPoint = false
    } else {
      ctx.lineTo(x, y)
    }
  })
  
  ctx.stroke()
}

// 控制方法
const toggleRealtime = () => {
  isRealtime.value = !isRealtime.value
  emit('realtimeToggle', isRealtime.value)
  
  if (!isRealtime.value && dataTimer) {
    clearInterval(dataTimer)
    dataTimer = null
  } else if (isRealtime.value) {
    startDataGeneration()
  }
}

const resetView = () => {
  updateCanvasSize()
}

const clearData = () => {
  channelData.value.clear()
  channels.value.forEach(channel => {
    channelData.value.set(channel.id, [])
    channel.currentValue = 0
    channel.maxValue = -Infinity
    channel.minValue = Infinity
  })
  totalDataPoints.value = 0
  ElMessage.success('数据已清除')
}

const onTimeWindowChange = () => {
  // 时间窗口改变时，清理过期数据
  const now = Date.now()
  const windowMs = getTimeWindowMs(timeWindow.value)
  const cutoffTime = now - windowMs
  
  channelData.value.forEach((data, channelId) => {
    const filteredData = data.filter(point => point.x >= cutoffTime)
    channelData.value.set(channelId, filteredData)
  })
}

const addChannel = () => {
  if (channels.value.length >= props.maxChannels) {
    ElMessage.warning(`最多支持${props.maxChannels}个通道`)
    return
  }

  const colors = ['#FFD700', '#00CED1', '#FF69B4', '#32CD32', '#8B8B8B', '#C71585', '#FF8C00', '#90EE90']
  const newChannel: ChannelConfig = {
    id: Date.now(),
    name: `CH${channels.value.length + 1}`,
    colorHex: colors[channels.value.length % colors.length],
    visible: true,
    lineWidth: 2,
    currentValue: 0,
    maxValue: -Infinity,
    minValue: Infinity
  }

  channels.value.push(newChannel)
  channelData.value.set(newChannel.id, [])
  emit('channelAdd', newChannel)
}

const removeChannel = (index: number) => {
  if (channels.value.length <= 1) {
    ElMessage.warning('至少需要保留一个通道')
    return
  }

  const channel = channels.value[index]
  channels.value.splice(index, 1)
  channelData.value.delete(channel.id)
  emit('channelRemove', channel.id)
}

const updateChannelVisibility = (index: number) => {
  // 通道可见性更新
}

const updateChannelColor = (index: number) => {
  // 颜色更新
}

const showColorPicker = (index: number) => {
  showChannelConfig.value = true
}

const resetChannelStats = (index: number) => {
  const channel = channels.value[index]
  channel.maxValue = -Infinity
  channel.minValue = Infinity
}

const saveChannelConfig = () => {
  showChannelConfig.value = false
  ElMessage.success('通道配置已保存')
}

// 鼠标事件处理
const onMouseDown = (event: MouseEvent) => {
  // 鼠标按下事件
}

const onMouseMove = (event: MouseEvent) => {
  // 鼠标移动事件
}

const onMouseUp = (event: MouseEvent) => {
  // 鼠标释放事件
}

const onWheel = (event: WheelEvent) => {
  event.preventDefault()
}

// 工具函数
const formatValue = (value: number): string => {
  if (Math.abs(value) === Infinity || isNaN(value)) return '---'
  return value.toFixed(3)
}

const formatDataRate = (rate: number): string => {
  if (rate >= 1000000000) {
    return `${(rate / 1000000000).toFixed(1)}GS/s`
  } else if (rate >= 1000000) {
    return `${(rate / 1000000).toFixed(1)}MS/s`
  } else if (rate >= 1000) {
    return `${(rate / 1000).toFixed(1)}kS/s`
  }
  return `${rate.toFixed(0)}S/s`
}

const formatNumber = (num: number): string => {
  if (num >= 1000000000) {
    return `${(num / 1000000000).toFixed(1)}G`
  } else if (num >= 1000000) {
    return `${(num / 1000000).toFixed(1)}M`
  } else if (num >= 1000) {
    return `${(num / 1000).toFixed(1)}K`
  }
  return num.toString()
}

const updateTime = () => {
  currentTime.value = new Date().toLocaleTimeString()
}

const updateChart = () => {
  // 图表更新逻辑
}

// 监听容器大小变化
const resizeObserver = new ResizeObserver(() => {
  updateCanvasSize()
})

// 生命周期
onMounted(() => {
  nextTick(() => {
    initialize()
    
    // 启动定时器
    timeTimer = setInterval(updateTime, 1000)
    
    if (chartCanvas.value?.parentElement) {
      resizeObserver.observe(chartCanvas.value.parentElement)
    }
  })
})

onUnmounted(() => {
  // 清理定时器
  if (dataTimer) clearInterval(dataTimer)
  if (timeTimer) clearInterval(timeTimer)
  if (animationId) cancelAnimationFrame(animationId)
  
  resizeObserver.disconnect()
})

// 暴露方法
defineExpose({
  addChannel,
  removeChannel,
  clearData,
  toggleRealtime,
  getChannels: () => channels.value,
  getPerformanceMetrics: () => ({
    renderFps: renderFps.value,
    dataRate: dataRate.value,
    totalDataPoints: totalDataPoints.value
  })
})
</script>

<style lang="scss" scoped>
.enhanced-strip-chart {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  overflow: hidden;
  
  .chart-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    border-bottom: 1px solid var(--border-color);
    background: var(--surface-secondary);
    
    .toolbar-left,
    .toolbar-right {
      display: flex;
      align-items: center;
      gap: 8px;
    }
  }
  
  .chart-main {
    flex: 1;
    position: relative;
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
    
    .chart-canvas {
      width: 100%;
      height: 100%;
      display: block;
      cursor: crosshair;
    }
    
    .performance-panel {
      position: absolute;
      top: 10px;
      right: 10px;
      width: 250px;
      background: rgba(255, 255, 255, 0.95);
      border: 1px solid var(--border-color);
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      z-index: 100;
      
      .panel-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 8px 12px;
        border-bottom: 1px solid var(--border-color);
        background: var(--surface-secondary);
        border-radius: 8px 8px 0 0;
        
        h4 {
          margin: 0;
          font-size: 14px;
          color: var(--text-primary);
        }
      }
      
      .performance-metrics {
        padding: 12px;
        
        .metric-item {
          display: flex;
          justify-content: space-between;
          align-items: center;
          margin-bottom: 8px;
          
          &:last-child {
            margin-bottom: 0;
          }
          
          .metric-label {
            font-size: 12px;
            color: var(--text-secondary);
          }
          
          .metric-value {
            font-size: 12px;
            font-weight: 600;
            color: var(--text-primary);
            font-family: 'Consolas', 'Monaco', monospace;
            
            &.warning {
              color: #e6a23c;
            }
            
            &.error {
              color: #f56c6c;
            }
          }
        }
      }
    }
    
    .chart-cursors {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      pointer-events: none;
      
      .cursor {
        position: absolute;
        top: 0;
        bottom: 0;
        
        .cursor-line {
          width: 1px;
          height: 100%;
          background: #ff6b6b;
          opacity: 0.8;
        }
        
        .cursor-label {
          position: absolute;
          top: 4px;
          left: 4px;
          background: rgba(255, 107, 107, 0.9);
          color: white;
          padding: 4px 8px;
          border-radius: 4px;
          font-size: 11px;
          white-space: nowrap;
        }
      }
    }
  }
  
  .channel-panel {
    width: 250px;
    border-left: 1px solid var(--border-color);
    background: var(--surface-secondary);
    display: flex;
    flex-direction: column;
    
    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px;
      border-bottom: 1px solid var(--border-color);
      
      h4 {
        margin: 0;
        font-size: 14px;
        color: var(--text-primary);
      }
    }
    
    .channel-list {
      flex: 1;
      overflow-y: auto;
      
      .channel-item {
        padding: 12px;
        border-bottom: 1px solid var(--border-color);
        
        &.active {
          background: rgba(64, 158, 255, 0.05);
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
            border: 1px solid var(--border-color);
          }
        }
        
        .channel-stats {
          font-size: 11px;
          color: var(--text-secondary);
          margin-bottom: 8px;
          
          .stat-label {
            margin-right: 4px;
          }
          
          .stat-value {
            font-weight: 500;
            color: var(--text-primary);
            margin-right: 12px;
            font-family: 'Consolas', 'Monaco', monospace;
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
    color: var(--text-secondary);
    background: var(--surface-secondary);
    border-top: 1px solid var(--border-color);
    
    .status-left,
    .status-right {
      display: flex;
      gap: 16px;
    }
    
    span {
      white-space: nowrap;
      font-family: 'Consolas', 'Monaco', monospace;
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
  .enhanced-strip-chart {
    .chart-toolbar {
      flex-direction: column;
      gap: 8px;
      align-items: flex-start;
    }
    
    .chart-main {
      .performance-panel {
        width: 200px;
        right: 5px;
        top: 5px;
      }
    }
    
    .channel-panel {
      width: 100%;
      border-left: none;
      border-top: 1px solid var(--border-color);
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
