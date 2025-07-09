<template>
  <div class="waveform-view example-page">
    <div class="page-header">
      <h1>波形图表控件示例</h1>
      <p class="description">
        展示 EasyChart 专业图表控件的各种功能，包括实时数据绘制、多系列显示、缩放交互等科学图表功能。
      </p>
    </div>

    <!-- 全局控制面板 -->
    <div class="example-section">
      <el-card class="control-card">
        <template #header>
          <h3>全局控制</h3>
        </template>
        
        <el-row :gutter="24">
          <el-col :span="8">
            <div class="control-group">
              <label>波形显示:</label>
              <div class="wave-controls">
                <el-checkbox v-model="showSineWave" @change="updateBasicData">
                  显示正弦波
                </el-checkbox>
                <el-checkbox v-model="showSquareWave" @change="updateBasicData">
                  显示方波
                </el-checkbox>
              </div>
            </div>
          </el-col>
          
          <el-col :span="8">
            <div class="control-group">
              <label>实时控制:</label>
              <div class="realtime-controls">
                <el-button size="small" @click="generateAllData">
                  <el-icon><Refresh /></el-icon>
                  重新生成
                </el-button>
                <el-button size="small" @click="startRealTime" :disabled="isRealTimeRunning">
                  <el-icon><VideoPlay /></el-icon>
                  实时更新
                </el-button>
                <el-button size="small" @click="stopRealTime" :disabled="!isRealTimeRunning">
                  <el-icon><VideoPause /></el-icon>
                  停止
                </el-button>
              </div>
            </div>
          </el-col>
          
          <el-col :span="8">
            <div class="control-group">
              <label>参数设置:</label>
              <div class="param-controls">
                <el-input-number 
                  v-model="seriesCount" 
                  :min="1" 
                  :max="8" 
                  size="small"
                  @change="generateMultiSeriesData"
                />
                <span style="margin-left: 8px;">多系列数量</span>
              </div>
            </div>
          </el-col>
        </el-row>
      </el-card>
    </div>

    <!-- 四个图表同时显示 -->
    <div class="charts-grid">
      <el-row :gutter="16">
        <!-- 基础波形图 -->
        <el-col :span="12">
          <el-card class="chart-card">
            <template #header>
              <div class="card-header">
                <span>基础波形图</span>
                <el-tag v-if="isRealTimeRunning" type="success" size="small">实时更新中</el-tag>
              </div>
            </template>
            
            <WaveformChart
              :data="basicChartData"
              :series-configs="visibleBasicSeriesConfigs"
              :options="basicChartOptions"
              :show-toolbar="false"
              :show-controls="false"
              :show-status="false"
              :show-measurements="false"
              :show-cursors="false"
              :height="350"
              @zoom="handleZoom"
              @cursor-move="handleCursorMove"
            />
          </el-card>
        </el-col>

        <!-- 多系列数据图表 -->
        <el-col :span="12">
          <el-card class="chart-card">
            <template #header>
              <div class="card-header">
                <span>多系列数据图表</span>
                <el-tag type="info" size="small">{{ seriesCount }} 个系列</el-tag>
              </div>
            </template>
            
            <WaveformChart
              :data="multiSeriesData"
              :series-configs="multiSeriesConfigs"
              :options="multiSeriesOptions"
              :show-toolbar="false"
              :show-controls="false"
              :show-status="false"
              :show-measurements="false"
              :show-cursors="false"
              :height="350"
            />
          </el-card>
        </el-col>
      </el-row>

      <el-row :gutter="16" style="margin-top: 16px;">
        <!-- 频谱分析图 -->
        <el-col :span="12">
          <el-card class="chart-card">
            <template #header>
              <div class="card-header">
                <span>频谱分析图</span>
                <el-select v-model="selectedSignal" @change="generateSpectrumData" size="small">
                  <el-option label="单频信号" value="single" />
                  <el-option label="多频信号" value="multi" />
                  <el-option label="噪声信号" value="noise" />
                </el-select>
              </div>
            </template>
            
            <WaveformChart
              :data="spectrumData"
              :series-configs="spectrumSeriesConfigs"
              :options="spectrumOptions"
              :show-toolbar="false"
              :show-controls="false"
              :show-status="false"
              :show-measurements="false"
              :show-cursors="false"
              :height="350"
            />
          </el-card>
        </el-col>

        <!-- 对数坐标图 -->
        <el-col :span="12">
          <el-card class="chart-card">
            <template #header>
              <div class="card-header">
                <span>对数坐标图</span>
                <el-tag type="warning" size="small">指数函数</el-tag>
              </div>
            </template>
            
            <WaveformChart
              :data="logData"
              :series-configs="logSeriesConfigs"
              :options="logOptions"
              :show-toolbar="false"
              :show-controls="false"
              :show-status="false"
              :show-measurements="false"
              :show-cursors="false"
              :height="350"
            />
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 控制面板 -->
    <div class="example-section">
      <h2 class="section-title">全局控制</h2>
      <el-card>
        <div class="global-controls">
          <el-row :gutter="16">
            <el-col :span="8">
              <div class="control-group">
                <label>数据点数:</label>
                <el-slider 
                  v-model="dataPoints" 
                  :min="100" 
                  :max="10000" 
                  :step="100"
                  @change="generateAllData"
                />
                <span>{{ dataPoints }}</span>
              </div>
            </el-col>
            
            <el-col :span="8">
              <div class="control-group">
                <label>更新频率:</label>
                <el-slider 
                  v-model="updateInterval" 
                  :min="50" 
                  :max="1000" 
                  :step="50"
                />
                <span>{{ updateInterval }}ms</span>
              </div>
            </el-col>
            
            <el-col :span="8">
              <div class="control-group">
                <label>噪声强度:</label>
                <el-slider 
                  v-model="noiseLevel" 
                  :min="0" 
                  :max="1" 
                  :step="0.1"
                  @change="generateAllData"
                />
                <span>{{ noiseLevel.toFixed(1) }}</span>
              </div>
            </el-col>
          </el-row>
        </div>
      </el-card>
    </div>

    <!-- 性能统计 -->
    <div class="example-section">
      <h2 class="section-title">性能统计</h2>
      <el-card>
        <el-descriptions :column="4" border>
          <el-descriptions-item label="总数据点">{{ totalDataPoints }}</el-descriptions-item>
          <el-descriptions-item label="渲染时间">{{ renderTime }}ms</el-descriptions-item>
          <el-descriptions-item label="更新次数">{{ updateCount }}</el-descriptions-item>
          <el-descriptions-item label="内存使用">{{ memoryUsage }}MB</el-descriptions-item>
        </el-descriptions>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { Refresh, VideoPlay, VideoPause } from '@element-plus/icons-vue'
import WaveformChart from '@/components/charts/WaveformChart.vue'
import type { WaveformData, WaveformOptions, WaveformSeriesConfig } from '@/types/chart'

// 响应式数据
const dataPoints = ref(1000)
const seriesCount = ref(3)
const updateInterval = ref(100)
const noiseLevel = ref(0.1)
const selectedSignal = ref('single')
const isRealTimeRunning = ref(false)
const updateCount = ref(0)
const renderTime = ref(0)

// 波形显示控制
const showSineWave = ref(true)
const showSquareWave = ref(true)

// 实时更新定时器
let realTimeTimer: number | null = null

// 基础图表数据
const basicChartData = ref<WaveformData>({
  series: [[], []],
  xStart: 0,
  xInterval: 0.01,
  sampleRate: 1000
})

const basicSeriesConfigs = ref<WaveformSeriesConfig[]>([
  {
    name: '正弦波',
    color: '#409eff',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'none',
    markerSize: 4,
    visible: true,
    yOffset: 0,
    yScale: 1,
    coupling: 'DC',
    bandwidth: 100000000,
    probe: 1
  },
  {
    name: '方波',
    color: '#67c23a',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'none',
    markerSize: 4,
    visible: true,
    yOffset: 0,
    yScale: 1,
    coupling: 'DC',
    bandwidth: 100000000,
    probe: 1
  }
])

// 计算可见的基础系列配置
const visibleBasicSeriesConfigs = computed(() => {
  const configs: WaveformSeriesConfig[] = []
  if (showSineWave.value) {
    configs.push(basicSeriesConfigs.value[0])
  }
  if (showSquareWave.value) {
    configs.push(basicSeriesConfigs.value[1])
  }
  return configs
})

const basicChartOptions = ref<WaveformOptions>({
  autoScale: true,
  legendVisible: true,
  cursorMode: 'zoom',
  gridEnabled: true,
  theme: 'light'
})

// 多系列数据
const multiSeriesData = ref<WaveformData>({
  series: [],
  xStart: 0,
  xInterval: 1,
  sampleRate: 1000
})

const multiSeriesConfigs = ref<WaveformSeriesConfig[]>([])

const multiSeriesOptions = ref<WaveformOptions>({
  autoScale: true,
  legendVisible: true,
  cursorMode: 'zoom',
  gridEnabled: true
})

// 频谱数据
const spectrumData = ref<WaveformData>({
  series: [],
  xStart: 0,
  xInterval: 1,
  sampleRate: 1000
})

const spectrumSeriesConfigs = ref<WaveformSeriesConfig[]>([
  {
    name: '幅度谱',
    color: '#e6a23c',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'none',
    markerSize: 4,
    visible: true,
    yOffset: 0,
    yScale: 1,
    coupling: 'DC',
    bandwidth: 100000000,
    probe: 1
  }
])

const spectrumOptions = ref<WaveformOptions>({
  autoScale: true,
  legendVisible: true,
  cursorMode: 'zoom',
  gridEnabled: true
})

// 对数坐标数据
const logData = ref<WaveformData>({
  series: [],
  xStart: 0.1,
  xInterval: 0.1,
  sampleRate: 1000
})

const logSeriesConfigs = ref<WaveformSeriesConfig[]>([
  {
    name: '指数函数',
    color: '#f56c6c',
    lineWidth: 3,
    lineType: 'solid',
    markerType: 'circle',
    markerSize: 6,
    visible: true,
    yOffset: 0,
    yScale: 1,
    coupling: 'DC',
    bandwidth: 100000000,
    probe: 1
  }
])

const logOptions = ref<WaveformOptions>({
  autoScale: true,
  logarithmic: true,
  legendVisible: true,
  cursorMode: 'cursor',
  gridEnabled: true
})

// 计算属性
const totalDataPoints = computed(() => {
  let total = 0
  if (basicChartData.value.series) {
    if (Array.isArray(basicChartData.value.series[0])) {
      total += (basicChartData.value.series as number[][]).reduce((sum, series) => sum + series.length, 0)
    } else {
      total += (basicChartData.value.series as number[]).length
    }
  }
  return total
})

const memoryUsage = computed(() => {
  // 简单估算内存使用
  return (totalDataPoints.value * 8 / 1024 / 1024).toFixed(2)
})

// 生成基础数据
const generateBasicData = () => {
  const startTime = performance.now()
  
  const sineWave: number[] = []
  const squareWave: number[] = []
  
  for (let i = 0; i < dataPoints.value; i++) {
    const x = i * 0.01
    const noise = (Math.random() - 0.5) * noiseLevel.value
    
    // 正弦波
    sineWave.push(Math.sin(2 * Math.PI * 5 * x) + noise)
    
    // 方波
    squareWave.push((Math.sin(2 * Math.PI * 3 * x) > 0 ? 1 : -1) + noise)
  }
  
  updateBasicChartData(sineWave, squareWave)
  
  renderTime.value = Math.round(performance.now() - startTime)
  updateCount.value++
}

// 更新基础图表数据
const updateBasicChartData = (sineWave: number[], squareWave: number[]) => {
  const series: number[][] = []
  
  if (showSineWave.value) {
    series.push(sineWave)
  }
  if (showSquareWave.value) {
    series.push(squareWave)
  }
  
  basicChartData.value = {
    series,
    xStart: 0,
    xInterval: 0.01,
    sampleRate: 1000
  }
}

// 更新基础数据（当显示选项改变时）
const updateBasicData = () => {
  generateBasicData()
}

// 生成所有数据
const generateAllData = () => {
  generateBasicData()
  generateMultiSeriesData()
  generateSpectrumData()
  generateLogData()
}

// 生成多系列数据
const generateMultiSeriesData = () => {
  const series: number[][] = []
  const configs: WaveformSeriesConfig[] = []
  const colors = ['#409eff', '#67c23a', '#e6a23c', '#f56c6c', '#909399', '#c71585', '#ff8c00', '#32cd32']
  
  for (let s = 0; s < seriesCount.value; s++) {
    const seriesData: number[] = []
    const frequency = 1 + s * 0.5
    const amplitude = 1 + s * 0.2
    
    for (let i = 0; i < dataPoints.value; i++) {
      const x = i * 0.02
      const noise = (Math.random() - 0.5) * noiseLevel.value
      seriesData.push(amplitude * Math.sin(2 * Math.PI * frequency * x) + noise)
    }
    
    series.push(seriesData)
    configs.push({
      name: `通道 ${s + 1}`,
      color: colors[s % colors.length],
      lineWidth: 2,
      lineType: 'solid',
      markerType: 'none',
      markerSize: 4,
      visible: true,
      yOffset: 0,
      yScale: 1,
      coupling: 'DC',
      bandwidth: 100000000,
      probe: 1
    })
  }
  
  multiSeriesData.value = {
    series,
    xStart: 0,
    xInterval: 0.02,
    sampleRate: 1000
  }
  
  multiSeriesConfigs.value = configs
}

// 生成频谱数据
const generateSpectrumData = () => {
  const frequencies: number[] = []
  const magnitudes: number[] = []
  
  const maxFreq = 50
  const freqStep = maxFreq / dataPoints.value
  
  for (let i = 0; i < dataPoints.value; i++) {
    const freq = i * freqStep
    frequencies.push(freq)
    
    let magnitude = 0
    
    switch (selectedSignal.value) {
      case 'single':
        // 单频信号在 10Hz 处有峰值
        magnitude = freq === 10 ? 1 : Math.exp(-Math.pow((freq - 10) / 2, 2)) * 0.8
        break
      case 'multi':
        // 多频信号在 5Hz, 15Hz, 25Hz 处有峰值
        magnitude = Math.exp(-Math.pow((freq - 5) / 1.5, 2)) * 0.6 +
                   Math.exp(-Math.pow((freq - 15) / 2, 2)) * 0.8 +
                   Math.exp(-Math.pow((freq - 25) / 1.8, 2)) * 0.4
        break
      case 'noise':
        // 白噪声
        magnitude = Math.random() * 0.3
        break
    }
    
    magnitudes.push(magnitude + Math.random() * 0.05)
  }
  
  spectrumData.value = {
    series: magnitudes,
    xStart: 0,
    xInterval: freqStep,
    labels: frequencies.map(f => f.toFixed(1)),
    sampleRate: 1000
  }
}

// 生成对数坐标数据
const generateLogData = () => {
  const data: number[] = []
  
  for (let i = 1; i <= dataPoints.value / 10; i++) {
    const x = i * 0.1
    data.push(Math.exp(x * 0.1))
  }
  
  logData.value = {
    series: data,
    xStart: 0.1,
    xInterval: 0.1,
    sampleRate: 1000
  }
}

// 开始实时更新
const startRealTime = () => {
  if (isRealTimeRunning.value) return
  
  isRealTimeRunning.value = true
  let phase = 0
  
  realTimeTimer = setInterval(() => {
    const sineWave: number[] = []
    const squareWave: number[] = []
    
    for (let i = 0; i < dataPoints.value; i++) {
      const x = (i + phase) * 0.01
      const noise = (Math.random() - 0.5) * noiseLevel.value
      
      sineWave.push(Math.sin(2 * Math.PI * 5 * x) + noise)
      squareWave.push((Math.sin(2 * Math.PI * 3 * x) > 0 ? 1 : -1) + noise)
    }
    
    updateBasicChartData(sineWave, squareWave)
    
    phase += 10
    updateCount.value++
  }, updateInterval.value)
}

// 停止实时更新
const stopRealTime = () => {
  if (realTimeTimer) {
    clearInterval(realTimeTimer)
    realTimeTimer = null
  }
  isRealTimeRunning.value = false
}

// 事件处理
const handleZoom = (range: any) => {
  console.log('缩放范围:', range)
}

const handleCursorMove = (position: any) => {
  console.log('游标位置:', position)
}

// 生命周期
onMounted(() => {
  generateAllData()
})

onUnmounted(() => {
  stopRealTime()
})
</script>

<style lang="scss" scoped>
.waveform-view {
  .control-card {
    margin-bottom: 24px;
    
    .control-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
      
      label {
        font-weight: 500;
        color: #606266;
        margin-bottom: 8px;
      }
      
      .wave-controls,
      .realtime-controls,
      .param-controls {
        display: flex;
        align-items: center;
        gap: 12px;
        flex-wrap: wrap;
      }
    }
  }
  
  .charts-grid {
    .chart-card {
      height: 400px;
      
      .card-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        
        span {
          font-weight: 500;
          color: #303133;
        }
      }
      
      :deep(.el-card__body) {
        height: calc(100% - 60px);
        padding: 12px;
      }
    }
  }
  
  .global-controls {
    .control-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
      
      label {
        font-weight: 500;
        color: #606266;
        margin-bottom: 8px;
      }
      
      span {
        margin-left: 8px;
        font-size: 12px;
        color: #909399;
        text-align: center;
      }
    }
  }
}

@media (max-width: 1200px) {
  .waveform-view {
    .charts-grid {
      .el-col {
        margin-bottom: 16px;
      }
    }
  }
}

@media (max-width: 768px) {
  .waveform-view {
    .control-card {
      .control-group {
        margin-bottom: 16px;
        
        .wave-controls,
        .realtime-controls,
        .param-controls {
          flex-direction: column;
          align-items: flex-start;
        }
      }
    }
    
    .charts-grid {
      .chart-card {
        height: 300px;
      }
    }
    
    .global-controls {
      .control-group {
        margin-bottom: 16px;
      }
    }
  }
}
</style>
