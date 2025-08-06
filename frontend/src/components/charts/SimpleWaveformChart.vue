<template>
  <div class="simple-waveform-chart" :style="{ height: height }">
    <div class="chart-header" v-if="showHeader">
      <div class="chart-title">{{ title }}</div>
      <div class="chart-controls">
        <el-button-group size="small">
          <el-button @click="togglePause" :type="isPaused ? 'warning' : 'primary'">
            {{ isPaused ? '继续' : '暂停' }}
          </el-button>
          <el-button @click="clearChart">清除</el-button>
        </el-button-group>
      </div>
    </div>
    <div class="chart-container" ref="chartContainer">
      <canvas 
        ref="chartCanvas" 
        :width="canvasWidth" 
        :height="canvasHeight"
        @mousemove="onMouseMove"
        @mouseleave="onMouseLeave"
      ></canvas>
      <div class="chart-legend" v-if="showLegend">
        <div 
          v-for="channel in channels" 
          :key="channel.id" 
          class="legend-item"
        >
          <div 
            class="legend-color" 
            :style="{ backgroundColor: channel.color }"
          ></div>
          <span class="legend-label">{{ channel.name }}</span>
          <span class="legend-value" v-if="channel.lastValue !== null">
            {{ channel.lastValue.toFixed(2) }}{{ channel.unit }}
          </span>
        </div>
      </div>
      <div class="cursor-info" v-if="cursorInfo" :style="cursorInfo.style">
        <div>时间: {{ cursorInfo.time }}ms</div>
        <div v-for="value in cursorInfo.values" :key="value.channel">
          {{ value.channel }}: {{ value.value }}{{ value.unit }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, nextTick } from 'vue'

interface Channel {
  id: string
  name: string
  color: string
  enabled: boolean
  unit: string
  lastValue: number | null
}

interface DataPoint {
  timestamp: number
  [key: string]: number
}

interface Props {
  data?: DataPoint[]
  channels?: Channel[]
  title?: string
  height?: string
  showHeader?: boolean
  showLegend?: boolean
  maxPoints?: number
  timeWindow?: number
  yAxisRange?: { min: number; max: number }
  backgroundColor?: string
  gridColor?: string
  textColor?: string
}

const props = withDefaults(defineProps<Props>(), {
  data: () => [],
  channels: () => [],
  title: '波形图',
  height: '300px',
  showHeader: true,
  showLegend: true,
  maxPoints: 1000,
  timeWindow: 10000, // 10秒
  yAxisRange: () => ({ min: -10, max: 10 }),
  backgroundColor: '#ffffff',
  gridColor: '#e0e0e0',
  textColor: '#333333'
})

const chartContainer = ref<HTMLDivElement>()
const chartCanvas = ref<HTMLCanvasElement>()
const canvasWidth = ref(800)
const canvasHeight = ref(250)
const isPaused = ref(false)
const cursorInfo = ref<any>(null)

let ctx: CanvasRenderingContext2D | null = null
let animationId: number | null = null
let lastDrawTime = 0

onMounted(async () => {
  await nextTick()
  initChart()
  window.addEventListener('resize', handleResize)
  startAnimation()
})

onUnmounted(() => {
  stopAnimation()
  window.removeEventListener('resize', handleResize)
})

watch(() => props.data, () => {
  if (!isPaused.value) {
    updateChannelValues()
  }
}, { deep: true })

function initChart() {
  if (!chartCanvas.value || !chartContainer.value) return
  
  ctx = chartCanvas.value.getContext('2d')
  handleResize()
}

function handleResize() {
  if (!chartCanvas.value || !chartContainer.value) return
  
  const container = chartContainer.value
  const rect = container.getBoundingClientRect()
  
  canvasWidth.value = Math.floor(rect.width)
  canvasHeight.value = Math.floor(rect.height - (props.showLegend ? 60 : 20))
  
  // 设置高DPI支持
  const dpr = window.devicePixelRatio || 1
  chartCanvas.value.width = canvasWidth.value * dpr
  chartCanvas.value.height = canvasHeight.value * dpr
  
  if (ctx) {
    ctx.scale(dpr, dpr)
  }
}

function startAnimation() {
  function animate(timestamp: number) {
    if (timestamp - lastDrawTime >= 50) { // 20fps更新
      drawChart()
      lastDrawTime = timestamp
    }
    animationId = requestAnimationFrame(animate)
  }
  animationId = requestAnimationFrame(animate)
}

function stopAnimation() {
  if (animationId) {
    cancelAnimationFrame(animationId)
    animationId = null
  }
}

function drawChart() {
  if (!ctx || !chartCanvas.value) return
  
  const width = canvasWidth.value
  const height = canvasHeight.value
  
  // 清除画布
  ctx.clearRect(0, 0, width, height)
  
  // 绘制背景
  ctx.fillStyle = props.backgroundColor
  ctx.fillRect(0, 0, width, height)
  
  // 绘制网格
  drawGrid()
  
  // 绘制波形
  drawWaveforms()
  
  // 绘制坐标轴标签
  drawAxisLabels()
}

function drawGrid() {
  if (!ctx) return
  
  const width = canvasWidth.value
  const height = canvasHeight.value
  const padding = 40
  
  ctx.strokeStyle = props.gridColor
  ctx.lineWidth = 1
  ctx.setLineDash([2, 2])
  
  // 垂直网格线（时间轴）
  for (let i = 0; i <= 10; i++) {
    const x = padding + (width - 2 * padding) * i / 10
    ctx.beginPath()
    ctx.moveTo(x, padding)
    ctx.lineTo(x, height - padding)
    ctx.stroke()
  }
  
  // 水平网格线（电压轴）
  for (let i = 0; i <= 10; i++) {
    const y = padding + (height - 2 * padding) * i / 10
    ctx.beginPath()
    ctx.moveTo(padding, y)
    ctx.lineTo(width - padding, y)
    ctx.stroke()
  }
  
  ctx.setLineDash([])
}

function drawWaveforms() {
  if (!ctx || !props.data || props.data.length === 0) return
  
  const width = canvasWidth.value
  const height = canvasHeight.value
  const padding = 40
  const plotWidth = width - 2 * padding
  const plotHeight = height - 2 * padding
  
  // 绘制每个通道的波形
  props.channels?.forEach(channel => {
    if (!channel.enabled || !ctx) return
    
    ctx.strokeStyle = channel.color
    ctx.lineWidth = 2
    ctx.beginPath()
    
    let firstPoint = true
    
    props.data?.forEach((point, index) => {
      const value = point[channel.id]
      if (value === undefined) return
      
      // 计算坐标
      const x = padding + (plotWidth * index / Math.max(props.data!.length - 1, 1))
      const normalizedY = (value - props.yAxisRange.min) / (props.yAxisRange.max - props.yAxisRange.min)
      const y = padding + plotHeight - (normalizedY * plotHeight)
      
      if (firstPoint) {
        ctx.moveTo(x, y)
        firstPoint = false
      } else {
        ctx.lineTo(x, y)
      }
    })
    
    ctx.stroke()
  })
}

function drawAxisLabels() {
  if (!ctx) return
  
  const width = canvasWidth.value
  const height = canvasHeight.value
  const padding = 40
  
  ctx.fillStyle = props.textColor
  ctx.font = '12px Arial'
  ctx.textAlign = 'center'
  
  // Y轴标签（电压）
  for (let i = 0; i <= 5; i++) {
    const value = props.yAxisRange.min + (props.yAxisRange.max - props.yAxisRange.min) * i / 5
    const y = padding + (height - 2 * padding) * (1 - i / 5)
    ctx.fillText(value.toFixed(1) + 'V', 20, y + 4)
  }
  
  // X轴标签（时间）
  ctx.textAlign = 'center'
  for (let i = 0; i <= 5; i++) {
    const time = i * props.timeWindow / 5
    const x = padding + (width - 2 * padding) * i / 5
    ctx.fillText((time / 1000).toFixed(1) + 's', x, height - 10)
  }
}

function updateChannelValues() {
  if (!props.data || props.data.length === 0) return
  
  const lastPoint = props.data[props.data.length - 1]
  props.channels?.forEach(channel => {
    if (channel.enabled && lastPoint[channel.id] !== undefined) {
      channel.lastValue = lastPoint[channel.id]
    }
  })
}

function togglePause() {
  isPaused.value = !isPaused.value
}

function clearChart() {
  if (ctx) {
    ctx.clearRect(0, 0, canvasWidth.value, canvasHeight.value)
  }
}

function onMouseMove(event: MouseEvent) {
  // 鼠标悬停时显示数值
  // 这里可以实现鼠标悬停显示具体数值的功能
}

function onMouseLeave() {
  cursorInfo.value = null
}
</script>

<style scoped>
.simple-waveform-chart {
  display: flex;
  flex-direction: column;
  background: #ffffff;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  background: #f8f9fa;
  border-bottom: 1px solid #e0e0e0;
}

.chart-title {
  font-weight: 600;
  color: #333;
}

.chart-container {
  position: relative;
  flex: 1;
  min-height: 200px;
}

.chart-legend {
  position: absolute;
  top: 10px;
  right: 10px;
  background: rgba(255, 255, 255, 0.9);
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  padding: 8px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
}

.legend-color {
  width: 12px;
  height: 12px;
  border-radius: 2px;
}

.legend-label {
  font-weight: 500;
}

.legend-value {
  color: #666;
  margin-left: auto;
}

.cursor-info {
  position: absolute;
  background: rgba(0, 0, 0, 0.8);
  color: white;
  padding: 8px;
  border-radius: 4px;
  font-size: 12px;
  pointer-events: none;
  z-index: 10;
}

canvas {
  display: block;
  width: 100%;
  height: 100%;
}
</style>
