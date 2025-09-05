<template>
  <div class="waveform-display">
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
    </div>
    
    <div ref="chartDiv" :style="{ width: '100%', height: height }"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import * as echarts from 'echarts'

const props = defineProps({
  height: {
    type: String,
    default: '400px'
  },
  maxPoints: {
    type: Number,
    default: 1000
  }
})

const chartDiv = ref<HTMLElement>()
let chart: echarts.ECharts | null = null
const isPaused = ref(false)
const dataBuffer: number[][] = []
let xData: number[] = []

const initChart = () => {
  if (!chartDiv.value) return
  
  chart = echarts.init(chartDiv.value)
  
  const option = {
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'cross'
      }
    },
    legend: {
      data: ['通道1'],
      top: 10
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: xData,
      name: '时间 (ms)'
    },
    yAxis: {
      type: 'value',
      name: '电压 (V)',
      axisLabel: {
        formatter: '{value} V'
      }
    },
    series: [
      {
        name: '通道1',
        type: 'line',
        sampling: 'average',
        symbol: 'none',
        lineStyle: {
          width: 1
        },
        data: []
      }
    ]
  }
  
  chart.setOption(option)
}

const updateChart = (newData: number[]) => {
  if (!chart || isPaused.value) return
  
  // 更新数据缓冲区
  dataBuffer.push(newData)
  
  // 限制缓冲区大小
  if (dataBuffer.length > props.maxPoints) {
    dataBuffer.shift()
  }
  
  // 更新X轴数据
  xData = dataBuffer.map((_, index) => index.toString())
  
  // 平坦化数据用于显示
  const flatData = dataBuffer.flat()
  
  chart.setOption({
    xAxis: {
      data: xData
    },
    series: [{
      data: flatData
    }]
  })
}

const togglePause = () => {
  isPaused.value = !isPaused.value
}

const clearChart = () => {
  dataBuffer.length = 0
  xData = []
  if (chart) {
    chart.setOption({
      xAxis: { data: [] },
      series: [{ data: [] }]
    })
  }
}

const autoScale = () => {
  if (!chart || dataBuffer.length === 0) return
  
  const flatData = dataBuffer.flat()
  const min = Math.min(...flatData)
  const max = Math.max(...flatData)
  const padding = (max - min) * 0.1
  
  chart.setOption({
    yAxis: {
      min: min - padding,
      max: max + padding
    }
  })
}

// 暴露方法供父组件调用
defineExpose({
  updateChart,
  clearChart,
  togglePause
})

onMounted(() => {
  initChart()
  window.addEventListener('resize', () => {
    chart?.resize()
  })
})

onUnmounted(() => {
  chart?.dispose()
  window.removeEventListener('resize', () => {
    chart?.resize()
  })
})
</script>

<style scoped>
.waveform-display {
  background: white;
  border-radius: 8px;
  padding: 16px;
}

.chart-toolbar {
  margin-bottom: 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>