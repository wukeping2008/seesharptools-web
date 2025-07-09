# 📈 StripChart 条带图控件技术设计方案

## 概述
StripChart是SeeSharpTools Web平台的核心图表控件，专门设计用于高速数据流的实时显示。支持1GS/s采样率、16-32通道同步显示，是整个平台的技术核心和性能基准。

## 🎯 设计目标

### 性能指标
- **数据吞吐量**: 支持1GS/s采样率数据显示
- **通道数量**: 同时显示16-32个通道
- **延迟要求**: 数据显示延迟<10ms
- **渲染性能**: 稳定60fps刷新率
- **内存使用**: 大数据量场景下<2GB内存占用

### 功能特性
- **实时数据流**: 连续数据流实时显示
- **历史数据回放**: 支持历史数据加载和回放
- **多通道管理**: 灵活的通道配置和显示控制
- **时间轴控制**: 精确的时间范围和缩放控制
- **数据压缩**: 智能数据采样和压缩算法
- **导出功能**: 多格式数据和图像导出

## 🏗️ 架构设计

### 整体架构图
```
┌─────────────────────────────────────────────────────────────────┐
│                    StripChart 组件架构                           │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │
│  │ 用户界面层  │ │ 控制面板    │ │ 工具栏      │ │ 状态栏      │ │
│  │ UI Layer    │ │ Control     │ │ Toolbar     │ │ Status      │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │
│  │ 数据管理层  │ │ 渲染引擎    │ │ 时间轴      │ │ 交互控制    │ │
│  │ Data Mgmt   │ │ Renderer    │ │ TimeAxis    │ │ Interaction │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │
│  │ 数据处理层  │ │ 缓冲管理    │ │ 压缩算法    │ │ 内存管理    │ │
│  │ Processing  │ │ Buffer      │ │ Compression │ │ Memory      │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │
│  │ 数据源层    │ │ WebSocket   │ │ HTTP API    │ │ 本地存储    │ │
│  │ Data Source │ │ Real-time   │ │ Historical  │ │ Local       │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### 核心模块设计

#### 1. 数据管理模块 (DataManager)
```typescript
class StripChartDataManager {
  private channels: Map<number, ChannelData>
  private buffer: CircularBuffer
  private timeRange: TimeRange
  private dataProcessor: DataProcessor
  
  // 数据接收和处理
  addData(channelId: number, data: number[], timestamp: number): void
  getData(channelId: number, timeRange: TimeRange): number[]
  getVisibleData(): VisibleData
  
  // 缓冲区管理
  setBufferSize(size: number): void
  clearBuffer(): void
  getBufferStatus(): BufferStatus
}

interface ChannelData {
  id: number
  name: string
  unit: string
  color: string
  visible: boolean
  data: CircularBuffer<DataPoint>
  statistics: ChannelStatistics
}

interface DataPoint {
  value: number
  timestamp: number
  quality: DataQuality
}
```

#### 2. 渲染引擎 (RenderEngine)
```typescript
class StripChartRenderer {
  private canvas: HTMLCanvasElement
  private context: CanvasRenderingContext2D | WebGLRenderingContext
  private renderMode: 'canvas2d' | 'webgl'
  private viewport: Viewport
  
  // 渲染控制
  render(data: VisibleData, options: RenderOptions): void
  setRenderMode(mode: 'canvas2d' | 'webgl'): void
  updateViewport(viewport: Viewport): void
  
  // 性能优化
  enableLOD(enabled: boolean): void  // Level of Detail
  setMaxPoints(maxPoints: number): void
  enableAntiAliasing(enabled: boolean): void
}

interface Viewport {
  x: number
  y: number
  width: number
  height: number
  timeRange: [number, number]
  valueRange: [number, number]
}
```

#### 3. 数据压缩算法 (CompressionEngine)
```typescript
class DataCompressionEngine {
  // LTTB (Largest Triangle Three Buckets) 算法
  lttbDownsample(data: DataPoint[], targetPoints: number): DataPoint[]
  
  // 自适应采样算法
  adaptiveSample(data: DataPoint[], viewport: Viewport): DataPoint[]
  
  // 实时压缩
  realtimeCompress(data: DataPoint[], compressionRatio: number): DataPoint[]
  
  // 质量控制
  calculateCompressionQuality(original: DataPoint[], compressed: DataPoint[]): number
}
```

## 🚀 核心技术实现

### 1. 高性能数据缓冲

#### 环形缓冲区实现
```typescript
class CircularBuffer<T> {
  private buffer: T[]
  private head: number = 0
  private tail: number = 0
  private size: number
  private capacity: number
  
  constructor(capacity: number) {
    this.capacity = capacity
    this.buffer = new Array(capacity)
    this.size = 0
  }
  
  push(item: T): void {
    this.buffer[this.tail] = item
    this.tail = (this.tail + 1) % this.capacity
    
    if (this.size < this.capacity) {
      this.size++
    } else {
      // 缓冲区满，移动头指针
      this.head = (this.head + 1) % this.capacity
    }
  }
  
  getRange(start: number, count: number): T[] {
    const result: T[] = []
    for (let i = 0; i < count; i++) {
      const index = (this.head + start + i) % this.capacity
      if (index < this.size) {
        result.push(this.buffer[index])
      }
    }
    return result
  }
  
  // 时间范围查询
  getTimeRange(startTime: number, endTime: number): T[] {
    // 二分查找优化的时间范围查询
    const startIndex = this.binarySearchTime(startTime)
    const endIndex = this.binarySearchTime(endTime)
    return this.getRange(startIndex, endIndex - startIndex)
  }
}
```

#### 多级缓存策略
```typescript
class MultiLevelCache {
  private l1Cache: Map<string, DataPoint[]>  // 最近数据
  private l2Cache: Map<string, DataPoint[]>  // 压缩数据
  private l3Cache: Map<string, DataPoint[]>  // 历史数据
  
  private l1Size: number = 1000000  // 100万点
  private l2Size: number = 10000000 // 1000万点
  private l3Size: number = 100000000 // 1亿点
  
  getData(channelId: string, timeRange: TimeRange, resolution: number): DataPoint[] {
    // 根据时间范围和分辨率选择合适的缓存级别
    if (this.isRecentData(timeRange)) {
      return this.getFromL1Cache(channelId, timeRange)
    } else if (this.isMediumTermData(timeRange)) {
      return this.getFromL2Cache(channelId, timeRange, resolution)
    } else {
      return this.getFromL3Cache(channelId, timeRange, resolution)
    }
  }
}
```

### 2. LTTB数据压缩算法

#### 核心算法实现
```typescript
class LTTBAlgorithm {
  /**
   * Largest Triangle Three Buckets 算法
   * 保持数据的视觉特征，适合时序数据压缩
   */
  downsample(data: DataPoint[], targetPoints: number): DataPoint[] {
    if (data.length <= targetPoints) {
      return data
    }
    
    const sampled: DataPoint[] = []
    const bucketSize = (data.length - 2) / (targetPoints - 2)
    
    // 保留第一个点
    sampled.push(data[0])
    
    for (let i = 0; i < targetPoints - 2; i++) {
      // 计算当前桶的范围
      const bucketStart = Math.floor(i * bucketSize) + 1
      const bucketEnd = Math.floor((i + 1) * bucketSize) + 1
      
      // 计算下一个桶的平均点
      const nextBucketStart = Math.floor((i + 1) * bucketSize) + 1
      const nextBucketEnd = Math.floor((i + 2) * bucketSize) + 1
      
      let avgX = 0, avgY = 0, avgCount = 0
      for (let j = nextBucketStart; j < nextBucketEnd && j < data.length; j++) {
        avgX += data[j].timestamp
        avgY += data[j].value
        avgCount++
      }
      
      if (avgCount > 0) {
        avgX /= avgCount
        avgY /= avgCount
      }
      
      // 在当前桶中找到形成最大三角形面积的点
      let maxArea = -1
      let maxAreaIndex = bucketStart
      
      const prevPoint = sampled[sampled.length - 1]
      
      for (let j = bucketStart; j < bucketEnd && j < data.length; j++) {
        const area = Math.abs(
          (prevPoint.timestamp - avgX) * (data[j].value - prevPoint.value) -
          (prevPoint.timestamp - data[j].timestamp) * (avgY - prevPoint.value)
        ) * 0.5
        
        if (area > maxArea) {
          maxArea = area
          maxAreaIndex = j
        }
      }
      
      sampled.push(data[maxAreaIndex])
    }
    
    // 保留最后一个点
    sampled.push(data[data.length - 1])
    
    return sampled
  }
}
```

### 3. WebGL高性能渲染

#### WebGL渲染器实现
```typescript
class WebGLStripChartRenderer {
  private gl: WebGLRenderingContext
  private shaderProgram: WebGLProgram
  private vertexBuffer: WebGLBuffer
  private colorBuffer: WebGLBuffer
  
  constructor(canvas: HTMLCanvasElement) {
    this.gl = canvas.getContext('webgl')!
    this.initShaders()
    this.initBuffers()
  }
  
  private initShaders(): void {
    const vertexShaderSource = `
      attribute vec2 a_position;
      attribute vec3 a_color;
      uniform mat3 u_transform;
      varying vec3 v_color;
      
      void main() {
        vec3 position = u_transform * vec3(a_position, 1.0);
        gl_Position = vec4(position.xy, 0.0, 1.0);
        v_color = a_color;
      }
    `
    
    const fragmentShaderSource = `
      precision mediump float;
      varying vec3 v_color;
      
      void main() {
        gl_FragColor = vec4(v_color, 1.0);
      }
    `
    
    this.shaderProgram = this.createShaderProgram(
      vertexShaderSource, 
      fragmentShaderSource
    )
  }
  
  render(channels: ChannelData[], viewport: Viewport): void {
    this.gl.clear(this.gl.COLOR_BUFFER_BIT)
    this.gl.useProgram(this.shaderProgram)
    
    // 设置变换矩阵
    const transform = this.calculateTransform(viewport)
    const transformLocation = this.gl.getUniformLocation(this.shaderProgram, 'u_transform')
    this.gl.uniformMatrix3fv(transformLocation, false, transform)
    
    // 渲染每个通道
    channels.forEach(channel => {
      if (channel.visible) {
        this.renderChannel(channel)
      }
    })
  }
  
  private renderChannel(channel: ChannelData): void {
    // 准备顶点数据
    const vertices = this.prepareVertices(channel.data)
    const colors = this.prepareColors(channel.color, vertices.length / 2)
    
    // 更新缓冲区
    this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.vertexBuffer)
    this.gl.bufferData(this.gl.ARRAY_BUFFER, vertices, this.gl.DYNAMIC_DRAW)
    
    this.gl.bindBuffer(this.gl.ARRAY_BUFFER, this.colorBuffer)
    this.gl.bufferData(this.gl.ARRAY_BUFFER, colors, this.gl.DYNAMIC_DRAW)
    
    // 绘制线条
    this.gl.drawArrays(this.gl.LINE_STRIP, 0, vertices.length / 2)
  }
}
```

### 4. 实时数据流处理

#### WebSocket数据接收
```typescript
class RealtimeDataReceiver {
  private websocket: WebSocket
  private dataQueue: Queue<DataPacket>
  private processingWorker: Worker
  
  constructor(url: string) {
    this.websocket = new WebSocket(url)
    this.dataQueue = new Queue()
    this.processingWorker = new Worker('/workers/dataProcessor.js')
    this.setupEventHandlers()
  }
  
  private setupEventHandlers(): void {
    this.websocket.onmessage = (event) => {
      const packet = JSON.parse(event.data) as DataPacket
      this.dataQueue.enqueue(packet)
    }
    
    // 使用Web Worker处理数据
    this.processingWorker.onmessage = (event) => {
      const processedData = event.data
      this.onDataProcessed(processedData)
    }
    
    // 定时处理队列中的数据
    setInterval(() => {
      this.processQueuedData()
    }, 16) // 60fps
  }
  
  private processQueuedData(): void {
    const batchSize = 1000
    const batch: DataPacket[] = []
    
    for (let i = 0; i < batchSize && !this.dataQueue.isEmpty(); i++) {
      batch.push(this.dataQueue.dequeue())
    }
    
    if (batch.length > 0) {
      // 发送到Web Worker处理
      this.processingWorker.postMessage({
        type: 'processBatch',
        data: batch
      })
    }
  }
}
```

#### Web Worker数据处理
```javascript
// workers/dataProcessor.js
class DataProcessor {
  constructor() {
    this.compressionEngine = new LTTBAlgorithm()
  }
  
  processBatch(packets) {
    const processedChannels = new Map()
    
    packets.forEach(packet => {
      packet.channels.forEach(channelData => {
        const channelId = channelData.id
        
        if (!processedChannels.has(channelId)) {
          processedChannels.set(channelId, [])
        }
        
        // 数据验证和清理
        const cleanedData = this.validateAndCleanData(channelData.values)
        
        // 添加时间戳
        const timestampedData = cleanedData.map((value, index) => ({
          value,
          timestamp: packet.timestamp + index * packet.interval,
          quality: this.assessDataQuality(value)
        }))
        
        processedChannels.get(channelId).push(...timestampedData)
      })
    })
    
    // 返回处理结果
    return {
      type: 'batchProcessed',
      channels: Array.from(processedChannels.entries()).map(([id, data]) => ({
        id,
        data: this.compressionEngine.adaptiveSample(data, this.getViewport())
      }))
    }
  }
}

// Web Worker消息处理
self.onmessage = function(event) {
  const processor = new DataProcessor()
  
  switch (event.data.type) {
    case 'processBatch':
      const result = processor.processBatch(event.data.data)
      self.postMessage(result)
      break
  }
}
```

## 🎨 用户界面设计

### Vue组件结构
```vue
<template>
  <div class="strip-chart" ref="chartContainer">
    <!-- 工具栏 -->
    <div class="chart-toolbar">
      <el-button-group>
        <el-button @click="startRealtime" :disabled="isRealtime">
          <el-icon><VideoPlay /></el-icon>
          实时
        </el-button>
        <el-button @click="pauseRealtime" :disabled="!isRealtime">
          <el-icon><VideoPause /></el-icon>
          暂停
        </el-button>
        <el-button @click="resetView">
          <el-icon><Refresh /></el-icon>
          重置
        </el-button>
      </el-button-group>
      
      <el-select v-model="timeRange" @change="onTimeRangeChange">
        <el-option label="最近1分钟" value="1m" />
        <el-option label="最近5分钟" value="5m" />
        <el-option label="最近1小时" value="1h" />
        <el-option label="自定义" value="custom" />
      </el-select>
      
      <el-button @click="showChannelConfig">
        <el-icon><Setting /></el-icon>
        通道配置
      </el-button>
    </div>
    
    <!-- 主图表区域 -->
    <div class="chart-main" ref="chartMain">
      <canvas 
        ref="chartCanvas"
        :width="canvasWidth"
        :height="canvasHeight"
        @mousedown="onMouseDown"
        @mousemove="onMouseMove"
        @mouseup="onMouseUp"
        @wheel="onWheel"
      />
      
      <!-- 游标和测量 -->
      <div class="chart-cursors" v-if="cursorsVisible">
        <div 
          v-for="cursor in cursors" 
          :key="cursor.id"
          class="cursor"
          :style="{ left: cursor.x + 'px' }"
        >
          <div class="cursor-line"></div>
          <div class="cursor-label">{{ cursor.label }}</div>
        </div>
      </div>
    </div>
    
    <!-- 时间轴 -->
    <div class="chart-timeaxis" ref="timeAxis">
      <canvas ref="timeAxisCanvas" :width="canvasWidth" :height="30" />
    </div>
    
    <!-- 通道列表 -->
    <div class="chart-channels">
      <div 
        v-for="channel in channels" 
        :key="channel.id"
        class="channel-item"
        :class="{ active: channel.visible }"
      >
        <el-checkbox 
          v-model="channel.visible"
          @change="onChannelVisibilityChange(channel)"
        />
        <div 
          class="channel-color"
          :style="{ backgroundColor: channel.color }"
          @click="showColorPicker(channel)"
        />
        <span class="channel-name">{{ channel.name }}</span>
        <span class="channel-value">{{ channel.currentValue }}</span>
        <span class="channel-unit">{{ channel.unit }}</span>
      </div>
    </div>
    
    <!-- 状态栏 -->
    <div class="chart-statusbar">
      <span>数据率: {{ dataRate }} Hz</span>
      <span>缓冲区: {{ bufferUsage }}%</span>
      <span>渲染: {{ renderFps }} fps</span>
      <span>内存: {{ memoryUsage }} MB</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { StripChartDataManager } from './dataManager'
import { StripChartRenderer } from './renderer'
import { RealtimeDataReceiver } from './dataReceiver'

// 组件属性
interface Props {
  deviceId?: string
  channels?: ChannelConfig[]
  realtime?: boolean
  bufferSize?: number
  maxPoints?: number
  renderMode?: 'canvas2d' | 'webgl'
}

const props = withDefaults(defineProps<Props>(), {
  realtime: true,
  bufferSize: 1000000,
  maxPoints: 10000,
  renderMode: 'webgl'
})

// 响应式数据
const chartContainer = ref<HTMLElement>()
const chartCanvas = ref<HTMLCanvasElement>()
const timeAxisCanvas = ref<HTMLCanvasElement>()

const isRealtime = ref(props.realtime)
const timeRange = ref('1m')
const channels = ref<ChannelData[]>([])
const cursors = ref<Cursor[]>([])
const cursorsVisible = ref(false)

// 性能监控
const dataRate = ref(0)
const bufferUsage = ref(0)
const renderFps = ref(0)
const memoryUsage = ref(0)

// 核心实例
let dataManager: StripChartDataManager
let renderer: StripChartRenderer
let dataReceiver: RealtimeDataReceiver

// 组件生命周期
onMounted(() => {
  initializeChart()
  startPerformanceMonitoring()
})

onUnmounted(() => {
  cleanup()
})

// 初始化图表
const initializeChart = () => {
  dataManager = new StripChartDataManager({
    bufferSize: props.bufferSize,
    maxChannels: 32
  })
  
  renderer = new StripChartRenderer(chartCanvas.value!, {
    renderMode: props.renderMode,
    maxPoints: props.maxPoints
  })
  
  if (props.deviceId && isRealtime.value) {
    dataReceiver = new RealtimeDataReceiver(`ws://api/devices/${props.deviceId}/stream`)
    dataReceiver.onData = (data) => {
      dataManager.addData(data)
      requestAnimationFrame(renderChart)
    }
  }
  
  // 初始渲染
  renderChart()
}

// 渲染图表
const renderChart = () => {
  const visibleData = dataManager.getVisibleData()
  renderer.render(visibleData, {
    viewport: getCurrentViewport(),
    channels: channels.value
  })
}

// 性能监控
const startPerformanceMonitoring = () => {
  setInterval(() => {
    dataRate.value = dataManager.getDataRate()
    bufferUsage.value = dataManager.getBufferUsage()
    renderFps.value = renderer.getFPS()
    memoryUsage.value = getMemoryUsage()
  }, 1000)
}
</script>

<style scoped>
.strip-chart {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #fff;
  border: 1px solid #ddd;
}

.chart-toolbar {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 12px;
  border-bottom: 1px solid #eee;
  background: #fafafa;
}

.chart-main {
  flex: 1;
  position: relative;
  overflow: hidden;
}

.chart-main canvas {
  display: block;
  cursor: crosshair;
}

.chart-cursors {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
}

.cursor {
  position: absolute;
  top: 0;
  bottom: 0;
}

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
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 12px;
}

.chart-timeaxis {
  border-top: 1px solid #eee;
}

.chart-channels {
  width: 200px;
  border-left: 1px solid #eee;
  background: #fafafa;
  overflow-y: auto;
}

.channel-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  border-bottom: 1px solid #eee;
  font-size: 12px;
}

.channel-item.active {
  background: #f0f9ff;
}

.channel-color {
  width: 12px;
  height: 12px;
  border-radius: 2px;
  cursor: pointer;
}

.channel-name {
  flex: 1;
  font-weight: 500;
}

.channel-value {
  font-family: 'Courier New', monospace;
  color: #333;
}

.channel-unit {
  color: #666;
  font-size: 11px;
}

.chart-statusbar {
  display: flex;
  gap: 16px;
  padding: 4px 12px;
  border-top: 1px solid #eee;
  background: #fafafa;
  font-size: 11px;
  color: #666;
}
</style>
```

## 📊 性能优化策略

### 1. 渲染优化
- **视口裁剪**: 只渲染可见区域的数据点
- **LOD (Level of Detail)**: 根据缩放级别调整渲染精度
- **批量渲染**: 合并多个通道的渲染调用
- **帧率控制**: 智能帧率控制，避免过度渲染

### 2. 内存优化
- **对象池**: 重用数据点对象，减少GC压力
- **延迟加载**: 按需加载历史数据
- **内存监控**: 实时监控内存使用，及时清理
- **数据分片**: 大数据集分片处理

### 3. 网络优化
- **数据压缩**: WebSocket数据压缩传输
- **增量更新**: 只传输变化的数据
- **连接复用**: 多通道共享WebSocket连接
- **断线重连**: 智能重连机制

## 🧪 测试策略

### 单元测试
```typescript
describe('StripChart DataManager', () => {
  let dataManager: StripChartDataManager
  
  beforeEach(() => {
    dataManager = new StripChartDataManager({ bufferSize: 1000 })
  })
  
  test('should handle high-frequency data', () => {
    const testData = generateTestData(10000, 1000) // 1kHz, 10s
    
    testData.forEach(point => {
      dataManager.addData(0, [point.value], point.timestamp)
    })
    
    expect(dataManager.getDataCount(0)).toBe(1000) // 缓冲区限制
    expect(dataManager.getDataRate()).toBeCloseTo(1000, 1)
  })
  
  test('should compress data correctly', () => {
    const originalData = generateTestData(10000, 1000)
    const compressed = dataManager.getCompressedData(0, 1000)
    
    expect(compressed.length).toBeLessThanOrEqual(1000)
    expect(calculateCompressionQuality(originalData, compressed)).toBeGreaterThan(0.95)
  })
})
```

### 性能测试
```typescript
describe('StripChart Performance', () => {
  test('should handle 1GS/s data rate', async () => {
    const chart = new StripChart({
      bufferSize: 10000000,
      maxPoints: 100000,
      renderMode: 'webgl'
