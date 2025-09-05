<template>
  <div class="optimized-waveform">
    <div class="chart-toolbar">
      <el-button-group size="small">
        <el-button @click="togglePause" :type="isPaused ? 'warning' : 'primary'">
          <el-icon><component :is="isPaused ? 'VideoPlay' : 'VideoPause'" /></el-icon>
          {{ isPaused ? '继续' : '暂停' }}
        </el-button>
        <el-button @click="clearChart">
          <el-icon><Delete /></el-icon>
          清除
        </el-button>
        <el-button @click="autoScale">
          <el-icon><FullScreen /></el-icon>
          自动缩放
        </el-button>
      </el-button-group>
      
      <div class="chart-info">
        <el-tag size="small">更新率: {{ updateRate }}Hz</el-tag>
        <el-tag size="small" type="info">数据点: {{ totalPoints }}</el-tag>
        <el-tag size="small" type="success">FPS: {{ fps.toFixed(1) }}</el-tag>
      </div>
    </div>
    
    <div ref="chartDiv" :style="{ width: '100%', height: height }"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import * as echarts from 'echarts'

const props = defineProps({
  height: {
    type: String,
    default: '400px'
  },
  maxPoints: {
    type: Number,
    default: 2000
  },
  updateInterval: {
    type: Number,
    default: 100 // 更新间隔(ms)
  }
})

const chartDiv = ref<HTMLElement>()
let chart: echarts.ECharts | null = null
const isPaused = ref(false)

// 数据管理
const dataBuffer = ref<number[]>([])
const pendingData: number[] = [] // 待处理数据队列
let updateTimer: number | null = null
let lastUpdateTime = Date.now()

// 性能监控
const fps = ref(0)
const totalPoints = computed(() => dataBuffer.value.length)
const updateRate = ref(10)

// 帧率计算
let frameCount = 0
let fpsTimer: number | null = null

const initChart = () => {
  if (!chartDiv.value) return
  
  // 使用Canvas渲染器以获得更好的性能
  chart = echarts.init(chartDiv.value, null, {
    renderer: 'canvas',
    useDirtyRect: true // 启用脏矩形渲染优化
  })
  
  const option = {
    animation: false, // 关闭动画以减少CPU使用
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'cross',
        animation: false
      }
    },
    legend: {
      data: ['信号'],
      top: 10
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    },
    xAxis: {
      type: 'value',
      name: '采样点',
      axisLabel: {
        formatter: (value: number) => value.toFixed(0)
      },
      min: 0,
      max: props.maxPoints
    },
    yAxis: {
      type: 'value',
      name: '电压 (V)',
      axisLabel: {
        formatter: '{value} V'
      },
      scale: true // 自动缩放
    },
    series: [
      {
        name: '信号',
        type: 'line',
        sampling: 'lttb', // 使用LTTB降采样算法
        symbol: 'none',
        lineStyle: {
          width: 1,
          color: '#409EFF'
        },
        emphasis: {
          focus: 'none' // 禁用高亮以提升性能
        },
        data: [],
        large: true, // 启用大数据量优化
        largeThreshold: 1000
      }
    ],
    dataZoom: [
      {
        type: 'inside',
        start: 90,
        end: 100,
        minValueSpan: 100
      }
    ]
  }
  
  chart.setOption(option)
  
  // 启动定时更新
  startUpdateTimer()
  
  // 启动FPS计算
  startFPSTimer()
}

// 批量更新图表
const batchUpdateChart = () => {
  if (!chart || isPaused.value || pendingData.length === 0) return
  
  // 合并待处理数据
  dataBuffer.value.push(...pendingData)
  
  // 限制数据长度
  if (dataBuffer.value.length > props.maxPoints) {
    dataBuffer.value = dataBuffer.value.slice(-props.maxPoints)
  }
  
  // 准备图表数据（索引-值对）
  const chartData = dataBuffer.value.map((value, index) => [index, value])
  
  // 使用setOption的replace模式，避免合并导致的性能问题
  chart.setOption({
    series: [{
      data: chartData
    }],
    xAxis: {
      max: dataBuffer.value.length
    }
  }, {
    replaceMerge: ['series'] // 替换而不是合并series数据
  })
  
  // 清空待处理队列
  pendingData.length = 0
  
  // 更新帧计数
  frameCount++
  
  // 计算实际更新率
  const now = Date.now()
  const timeDiff = now - lastUpdateTime
  if (timeDiff > 0) {
    updateRate.value = Math.round(1000 / timeDiff)
  }
  lastUpdateTime = now
}

// 启动定时更新
const startUpdateTimer = () => {
  if (updateTimer) return
  
  updateTimer = window.setInterval(() => {
    batchUpdateChart()
  }, props.updateInterval)
}

// 停止定时更新
const stopUpdateTimer = () => {
  if (updateTimer) {
    clearInterval(updateTimer)
    updateTimer = null
  }
}

// 启动FPS计算
const startFPSTimer = () => {
  fpsTimer = window.setInterval(() => {
    fps.value = frameCount * (1000 / props.updateInterval) / 10 // 转换为每秒帧数
    frameCount = 0
  }, 1000)
}

// 添加新数据（外部调用）
const updateChart = (newData: number[]) => {
  if (isPaused.value) return
  
  // 将新数据添加到待处理队列
  pendingData.push(...newData)
  
  // 如果待处理数据太多，进行降采样
  if (pendingData.length > props.maxPoints / 10) {
    const step = Math.floor(pendingData.length / (props.maxPoints / 10))
    pendingData.splice(0, pendingData.length, ...pendingData.filter((_, i) => i % step === 0))
  }
}

const togglePause = () => {
  isPaused.value = !isPaused.value
  
  if (isPaused.value) {
    stopUpdateTimer()
  } else {
    startUpdateTimer()
  }
}

const clearChart = () => {
  dataBuffer.value = []
  pendingData.length = 0
  
  if (chart) {
    chart.setOption({
      series: [{
        data: []
      }]
    })
  }
  
  frameCount = 0
  fps.value = 0
}

const autoScale = () => {
  if (!chart || dataBuffer.value.length === 0) return
  
  const min = Math.min(...dataBuffer.value)
  const max = Math.max(...dataBuffer.value)
  const padding = (max - min) * 0.1
  
  chart.setOption({
    yAxis: {
      min: min - padding,
      max: max + padding
    }
  })
}

// 优化窗口大小调整
let resizeTimer: number | null = null
const handleResize = () => {
  if (resizeTimer) {
    clearTimeout(resizeTimer)
  }
  resizeTimer = window.setTimeout(() => {
    chart?.resize()
  }, 200)
}

// 暴露方法供父组件调用
defineExpose({
  updateChart,
  clearChart,
  togglePause
})

onMounted(() => {
  initChart()
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  stopUpdateTimer()
  if (fpsTimer) {
    clearInterval(fpsTimer)
  }
  if (resizeTimer) {
    clearTimeout(resizeTimer)
  }
  chart?.dispose()
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.optimized-waveform {
  background: white;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.1);
}

.chart-toolbar {
  margin-bottom: 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.chart-info {
  display: flex;
  gap: 8px;
}

/* 添加GPU加速 */
.optimized-waveform > div {
  transform: translateZ(0);
  will-change: transform;
}
</style>