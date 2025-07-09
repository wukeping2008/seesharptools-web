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
        
        <el-select v-model="renderMode" @change="onRenderModeChange" size="small" style="width: 100px; margin-left: 8px;">
          <el-option label="WebGL" value="webgl" />
          <el-option label="Canvas" value="canvas" />
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
            <span class="metric-label">内存使用:</span>
            <span class="metric-value" :class="{ warning: memoryUsage > 500, error: memoryUsage > 1000 }">
              {{ memoryUsage.toFixed(1) }}MB
            </span>
          </div>
          <div class="metric-item">
            <span class="metric-label">压缩比:</span>
            <span class="metric-value">{{ (compressionRatio * 100).toFixed(1) }}%</span>
          </div>
          <div class="metric-item">
            <span class="metric-label">数据点数:</span>
            <span class="metric-value">{{ formatNumber(totalDataPoints) }}</span>
          </div>
          <div class="metric-item">
            <span class="metric-label">渲染模式:</span>
            <span class="metric-value">{{ renderMode.toUpperCase() }}</span>
          </div>
        </div>
      </div>
      
      <!-- 游标和测量 -->
      <div class="chart-cursors" v-if="cursorsVisible">
        <div 
          v-for="cursor in cursors" 
          :key="cursor.id"
          class="cursor"
          :style="{ left: cursor.x + 'px' }"
        >
          <div class="cursor-line"></div>
          <div class="cursor-label">
            <div>时间: {{ formatTime(cursor.time) }}</div>
            <div v-for="(value, index) in cursor.values" :key="index">
              CH{{ index + 1 }}: {{ formatValue(value) }}
            </div>
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
              :style="{ backgroundColor: rgbaToHex(channel.color) }"
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
        <span>数据率: {{ formatDataRate(dataRate) }}</span>
        <span>缓冲区: {{ bufferUsage.toFixed(1) }}%</span>
        <span>通道数: {{ activeChannelCount }}</span>
        <span>数据点: {{ formatNumber(totalDataPoints) }}</span>
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
import { 
  VideoPlay, VideoPause, Refresh, Delete, Setting, Download, 
  FullScreen, Plus, Monitor, Close
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { WebGLRenderer, type WebGLRendererOptions, type Viewport, type ChannelData as WebGLChannelData } from '@/utils/webgl/WebGLRenderer'
import { DataBufferManager, type BufferConfig, type DataPoint, type TimeRange } from '@/utils/data/DataBufferManager'

// 通道配置接口
interface ChannelConfig {
  id: number
  name: string
  color: [number, number, number, number] // RGBA
  colorHex: string // 用于颜色选择器
  visible: boolean
  lineWidth: number
  currentValue: number
  maxValue: number
  minValue: number
}

// 游标接口
interface Cursor {
  id: number
  x: number
  time: number
  values: number[]
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
  maxSampleRate: 1000000000, // 1GS/s
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
const renderMode = ref<'webgl' | 'canvas'>('webgl')
const currentTime = ref('')
const cursorsVisible = ref(false)
const cursors = ref<Cursor[]>([])

// 渲染器和数据管理器
let webglRenderer: WebGLRenderer | null = null
let dataBufferManager: DataBufferManager | null = null

// 通道数据
const channels = ref<ChannelConfig[]>([
  {
    id: 0,
    name: 'CH1',
    color: [1.0, 0.84, 0.0, 1.0], // 金色
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
    color: [0.0, 0.81, 0.82, 1.0], // 青色
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
const dataRate = ref(0)
const memoryUsage = ref(0)
const compressionRatio = ref(1)
const totalDataPoints = ref(0)
const bufferUsage = ref(0)

// 定时器
let dataTimer: number | null = null
let timeTimer: number | null = null
let renderTimer: number | null = null

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

// 初始化
const initialize = async () => {
  if (!chartCanvas.value) return

  try {
    // 初始化WebGL渲染器
    const webglOptions: WebGLRendererOptions = {
      canvas: chartCanvas.value,
      maxPoints: 100000,
      maxChannels: props.maxChannels,
      enableAntiAliasing: true,
      enableLOD: true
    }
    
    webglRenderer = new WebGLRenderer(webglOptions)
    
    // 初始化数据缓冲管理器
    const bufferConfig: BufferConfig = {
      maxChannels: props.maxChannels,
      bufferSize: props.bufferSize,
      compressionThreshold: 50000,
      maxMemoryUsage: 1024 // 1GB
    }
    
    dataBufferManager = new DataBufferManager(bufferConfig)
    
    // 设置初始视口
    updateViewport()
    
    // 开始数据生成和渲染
    startDataGeneration()
    startRendering()
    
    ElMessage.success('高性能StripChart初始化成功')
  } catch (error) {
    console.error('初始化失败:', error)
    ElMessage.error('初始化失败，请检查WebGL支持')
    
    // 降级到Canvas模式
    renderMode.value = 'canvas'
  }
}

// 更新视口
const updateViewport = () => {
  if (!webglRenderer || !chartCanvas.value) return

  const canvas = chartCanvas.value
  const rect = canvas.getBoundingClientRect()
  
  // 设置canvas尺寸
  canvas.width = rect.width * window.devicePixelRatio
  canvas.height = rect.height * window.devicePixelRatio
  canvas.style.width = rect.width + 'px'
  canvas.style.height = rect.height + 'px'

  const now = Date.now()
  const windowMs = getTimeWindowMs(timeWindow.value)
  
  const viewport: Viewport = {
    x: 0,
    y: 0,
    width: canvas.width,
    height: canvas.height,
    timeRange: [now - windowMs, now],
    valueRange: [-10, 10] // 自动调整
  }
  
  webglRenderer.setViewport(viewport)
}

// 开始数据生成
const startDataGeneration = () => {
  if (dataTimer) return
  
  dataTimer = setInterval(() => {
    if (!isRealtime.value || !dataBufferManager) return
    
    generateHighSpeedData()
  }, 1) // 1ms间隔，模拟高速数据
}

// 生成高速数据
const generateHighSpeedData = () => {
  if (!dataBufferManager) return

  const now = Date.now()
  const pointsPerBatch = 1000 // 每批1000个点
  
  channels.value.forEach((channel, channelIndex) => {
    if (!channel.visible) return
    
    const data: DataPoint[] = []
    
    for (let i = 0; i < pointsPerBatch; i++) {
      const t = now + i * 0.001 // 1μs间隔
      let value = 0
      
      // 生成不同类型的测试信号
      switch (channelIndex) {
        case 0: // 高频正弦波
          value = Math.sin(2 * Math.PI * 10000 * t / 1000) * 5
          break
        case 1: // 调制信号
          value = Math.sin(2 * Math.PI * 1000 * t / 1000) * Math.sin(2 * Math.PI * 100 * t / 1000) * 3
          break
        case 2: // 脉冲信号
          value = Math.sin(2 * Math.PI * 500 * t / 1000) > 0.8 ? 8 : -2
          break
        case 3: // 噪声信号
          value = (Math.random() - 0.5) * 4
          break
        default:
          value = Math.sin(2 * Math.PI * (100 + channelIndex * 50) * t / 1000) * (2 + channelIndex)
      }
      
      // 添加噪声
      value += (Math.random() - 0.5) * 0.1
      
      data.push({
        timestamp: t,
        value,
        quality: 1
      })
      
      // 更新统计
      channel.currentValue = value
      channel.maxValue = Math.max(channel.maxValue, value)
      channel.minValue = Math.min(channel.minValue, value)
    }
    
    dataBufferManager?.addData(channel.id, data)
  })
  
  // 更新性能指标
  updatePerformanceMetrics()
}

// 开始渲染
const startRendering = () => {
  if (renderTimer) return
  
  const render = () => {
    if (webglRenderer && dataBufferManager) {
      // 准备渲染数据
      const webglChannels: WebGLChannelData[] = []
      const now = Date.now()
      const windowMs = getTimeWindowMs(timeWindow.value)
      
      channels.value.forEach(channel => {
        if (!channel.visible) return
        
        const timeRange: TimeRange = {
          start: now - windowMs,
          end: now
        }
        
        const data = dataBufferManager!.getData(channel.id, timeRange, 10000)
        const values = new Float32Array(data.map(d => d.value))
        
        webglChannels.push({
          id: channel.id,
          name: channel.name,
          color: channel.color,
          data: values,
          visible: channel.visible,
          lineWidth: channel.lineWidth
        })
      })
      
      webglRenderer.setChannels(webglChannels)
      webglRenderer.render()
      
      // 更新FPS
      renderFps.value = webglRenderer.getFPS()
    }
    
    renderTimer = requestAnimationFrame(render)
  }
  
  render()
}

// 更新性能指标
const updatePerformanceMetrics = () => {
  if (!dataBufferManager) return
  
  const stats = dataBufferManager.getStatistics()
  dataRate.value = dataBufferManager.getDataRate()
  memoryUsage.value = stats.memoryUsage
  compressionRatio.value = stats.compressionRatio
  totalDataPoints.value = stats.totalPoints
  bufferUsage.value = Math.min(100, (stats.totalPoints * 16) / (props.bufferSize * props.maxChannels) * 100)
  
  emit('performanceUpdate', {
    renderFps: renderFps.value,
    dataRate: dataRate.value,
    memoryUsage: memoryUsage.value,
    compressionRatio: compressionRatio.value,
    totalDataPoints: totalDataPoints.value
  })
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
  updateViewport()
}

const clearData = () => {
  if (dataBufferManager) {
    dataBufferManager.clearAllChannels()
  }
  
  channels.value.forEach(channel => {
    channel.currentValue = 0
    channel.maxValue = -Infinity
    channel.minValue = Infinity
  })
  
  ElMessage.success('数据已清除')
}

const onTimeWindowChange = () => {
  updateViewport()
}

const onRenderModeChange = () => {
  // 重新初始化渲染器
  if (webglRenderer) {
    webglRenderer.dispose()
    webglRenderer = null
  }
  
  nextTick(() => {
    initialize()
  })
}

const addChannel = () => {
  if (channels.value.length >= props.maxChannels) {
    ElMessage.warning(`最多支持${props.maxChannels}个通道`)
    return
  }

  const newChannel: ChannelConfig = {
    id: Date.now(),
    name: `CH${channels.value.length + 1}`,
    color: getChannelColor(channels.value.length),
    colorHex: rgbaToHex(getChannelColor(channels.value.length)),
    visible: true,
    lineWidth: 2,
    currentValue: 0,
    maxValue: -Infinity,
    minValue: Infinity
  }

  channels.value.push(newChannel)
  emit('channelAdd', newChannel)
}

const removeChannel = (index: number) => {
  if (channels.value.length <= 1) {
    ElMessage.warning('至少需要保留一个通道')
    return
  }

  const channel = channels.value[index]
  channels.value.splice(index, 1)
  emit('channelRemove', channel.id)
}

const getChannelColor = (index: number): [number, number, number, number] => {
  const colors: [number, number, number, number][] = [
    [1.0, 0.84, 0.0, 1.0], // 金色
    [0.0, 0.81, 0.82, 1.0], // 青色
    [1.0, 0.41, 0.71, 1.0], // 粉色
    [0.2, 0.8, 0.2, 1.0], // 绿色
    [0.56, 0.56, 0.56, 1.0], // 灰色
    [0.78, 0.08, 0.52, 1.0], // 深粉色
    [1.0, 0.55, 0.0, 1.0], // 橙色
    [0.2, 0.8, 0.2, 1.0] // 浅绿色
  ]
  return colors[index % colors.length]
}

const updateChannelVisibility = (index: number) => {
  // 通道可见性更新逻辑
}

const updateChannelColor = (index: number) => {
  const channel = channels.value[index]
  channel.color = hexToRgba(channel.colorHex)
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

const exportData = () => {
  if (!dataBufferManager) {
    ElMessage.warning('没有可导出的数据')
    return
  }
  
  // 导出逻辑
  ElMessage.success('数据导出功能开发中')
}

const toggleFullscreen = () => {
  isFullscreen.value = !isFullscreen.value
  nextTick(() => {
    updateViewport()
  })
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
  // 滚轮缩放事件
  event.preventDefault()
}

// 工具函数
const rgbaToHex = (rgba: [number, number, number, number]): string => {
  const r = Math.round(rgba[0] * 255).toString(16).padStart(2, '0')
  const g = Math.round(rgba[1] * 255).toString(16).padStart(2, '0')
  const b = Math.round(rgba[2] * 255).toString(16).padStart(2, '0')
  return `#${r}${g}${b}`
}

const hexToRgba = (hex: string): [number, number, number, number] => {
  const r = parseInt(hex.slice(1, 3), 16) / 255
  const g = parseInt(hex.slice(3, 5), 16) / 255
  const b = parseInt(hex.slice(5, 7), 16) / 255
  return [r, g, b, 1.0]
}

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

const formatTime = (timestamp: number): string => {
  return new Date(timestamp).toLocaleTimeString()
}

const updateTime = () => {
  currentTime.value = new Date().toLocaleTimeString()
}

const updateChart = () => {
  // 图表更新逻辑
}

// 监听容器大小变化
const resizeObserver = new ResizeObserver(() => {
  updateViewport()
})

// 生命周期
onMounted(() => {
  initialize()
  
  // 启动定时器
  timeTimer = setInterval(updateTime, 1000)
  
  if (chartCanvas.value) {
    resizeObserver.observe(chartCanvas.value.parentElement!)
  }
})

onUnmounted(() => {
  // 清理定时器
  if (dataTimer) clearInterval(dataTimer)
  if (timeTimer) clearInterval(timeTimer)
  if (renderTimer) cancelAnimationFrame(renderTimer)
  
  // 清理资源
  if (webglRenderer) {
    webglRenderer.dispose()
  }
  
  if (dataBufferManager) {
    dataBufferManager.dispose()
  }
  
  resizeObserver.disconnect()
})

// 暴露方法
defineExpose({
  addChannel,
  removeChannel,
  clearData,
  toggleRealtime,
  exportData,
  getChannels: () => channels.value,
  getPerformanceMetrics: () => ({
    renderFps: renderFps.value,
    dataRate: dataRate.value,
    memoryUsage: memoryUsage.value,
    compressionRatio: compressionRatio.value,
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
