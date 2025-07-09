<template>
  <div class="enhanced-charts-view">
    <div class="page-header">
      <h1 class="page-title">增强型EasyChart</h1>
      <p class="page-subtitle">集成FFT频谱分析、高级测量工具和双Y轴显示的科学级图表控件</p>
    </div>

    <!-- 功能演示区域 -->
    <section class="demo-section">
      <div class="section-header">
        <h2 class="section-title">实时数据演示</h2>
        <p class="section-description">
          展示增强型EasyChart的FFT分析、峰值检测、统计分析等高级功能
        </p>
      </div>

      <div class="demo-container">
        <EnhancedEasyChart
          :data="chartData"
          :series-configs="seriesConfigs"
          :sample-rate="sampleRate"
          :height="500"
          @fft-update="handleFFTUpdate"
          @measurement-update="handleMeasurementUpdate"
        />
      </div>
    </section>

    <!-- 控制面板 -->
    <section class="control-section">
      <div class="section-header">
        <h2 class="section-title">信号控制</h2>
        <p class="section-description">调节信号参数，观察FFT分析和测量结果的变化</p>
      </div>

      <div class="control-panel">
        <el-row :gutter="24">
          <el-col :span="8">
            <div class="control-group">
              <h4>信号1 (正弦波)</h4>
              <div class="control-item">
                <label>频率 (Hz):</label>
                <el-slider
                  v-model="signal1.frequency"
                  :min="1"
                  :max="200"
                  :step="1"
                  show-input
                  @change="updateSignals"
                />
              </div>
              <div class="control-item">
                <label>幅度:</label>
                <el-slider
                  v-model="signal1.amplitude"
                  :min="0.1"
                  :max="5"
                  :step="0.1"
                  show-input
                  @change="updateSignals"
                />
              </div>
              <div class="control-item">
                <label>相位 (度):</label>
                <el-slider
                  v-model="signal1.phase"
                  :min="0"
                  :max="360"
                  :step="1"
                  show-input
                  @change="updateSignals"
                />
              </div>
            </div>
          </el-col>

          <el-col :span="8">
            <div class="control-group">
              <h4>信号2 (方波)</h4>
              <div class="control-item">
                <label>频率 (Hz):</label>
                <el-slider
                  v-model="signal2.frequency"
                  :min="1"
                  :max="100"
                  :step="1"
                  show-input
                  @change="updateSignals"
                />
              </div>
              <div class="control-item">
                <label>幅度:</label>
                <el-slider
                  v-model="signal2.amplitude"
                  :min="0.1"
                  :max="3"
                  :step="0.1"
                  show-input
                  @change="updateSignals"
                />
              </div>
              <div class="control-item">
                <label>占空比 (%):</label>
                <el-slider
                  v-model="signal2.dutyCycle"
                  :min="10"
                  :max="90"
                  :step="5"
                  show-input
                  @change="updateSignals"
                />
              </div>
            </div>
          </el-col>

          <el-col :span="8">
            <div class="control-group">
              <h4>噪声和采样</h4>
              <div class="control-item">
                <label>噪声水平:</label>
                <el-slider
                  v-model="noiseLevel"
                  :min="0"
                  :max="0.5"
                  :step="0.01"
                  show-input
                  @change="updateSignals"
                />
              </div>
              <div class="control-item">
                <label>采样率 (Hz):</label>
                <el-select v-model="sampleRate" @change="updateSignals">
                  <el-option label="500 Hz" :value="500" />
                  <el-option label="1000 Hz" :value="1000" />
                  <el-option label="2000 Hz" :value="2000" />
                  <el-option label="5000 Hz" :value="5000" />
                </el-select>
              </div>
              <div class="control-item">
                <label>数据点数:</label>
                <el-select v-model="dataPoints" @change="updateSignals">
                  <el-option label="512" :value="512" />
                  <el-option label="1024" :value="1024" />
                  <el-option label="2048" :value="2048" />
                  <el-option label="4096" :value="4096" />
                </el-select>
              </div>
            </div>
          </el-col>
        </el-row>

        <div class="control-actions">
          <el-button type="primary" @click="startAnimation">
            {{ isAnimating ? '停止动画' : '开始动画' }}
          </el-button>
          <el-button @click="resetSignals">重置参数</el-button>
          <el-button @click="addRandomNoise">添加随机噪声</el-button>
        </div>
      </div>
    </section>

    <!-- FFT分析结果 -->
    <section class="analysis-section" v-if="fftResult">
      <div class="section-header">
        <h2 class="section-title">FFT分析结果</h2>
        <p class="section-description">频域分析结果和主要频率成分</p>
      </div>

      <div class="analysis-results">
        <el-row :gutter="16">
          <el-col :span="12">
            <div class="result-card">
              <h4>主要频率成分</h4>
              <div class="frequency-list">
                <div 
                  v-for="(peak, index) in dominantFrequencies" 
                  :key="index" 
                  class="frequency-item"
                >
                  <span class="frequency-value">{{ peak.frequency.toFixed(1) }} Hz</span>
                  <span class="magnitude-value">{{ peak.magnitude.toFixed(2) }}</span>
                </div>
              </div>
            </div>
          </el-col>

          <el-col :span="12">
            <div class="result-card">
              <h4>频谱统计</h4>
              <div class="stats-list">
                <div class="stat-item">
                  <span class="stat-label">峰值频率:</span>
                  <span class="stat-value">{{ peakFrequency.toFixed(1) }} Hz</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">峰值幅度:</span>
                  <span class="stat-value">{{ peakMagnitude.toFixed(2) }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">频率分辨率:</span>
                  <span class="stat-value">{{ frequencyResolution.toFixed(2) }} Hz</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">有效带宽:</span>
                  <span class="stat-value">{{ effectiveBandwidth.toFixed(1) }} Hz</span>
                </div>
              </div>
            </div>
          </el-col>
        </el-row>
      </div>
    </section>

    <!-- 功能特性说明 -->
    <section class="features-section">
      <div class="section-header">
        <h2 class="section-title">增强功能特性</h2>
      </div>

      <div class="features-grid">
        <div class="feature-item">
          <div class="feature-icon">🔬</div>
          <h4>FFT频谱分析</h4>
          <p>实时FFT计算，支持多种窗函数（汉宁、汉明、布莱克曼等），可配置FFT大小和重叠率</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">📏</div>
          <h4>专业测量工具</h4>
          <p>游标测量、峰值检测、统计分析（均值、RMS、标准差），支持多点测量和自动标记</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">📊</div>
          <h4>双Y轴显示</h4>
          <p>支持左右双Y轴独立显示，适用于不同量程和单位的多系列数据同时显示</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">🎛️</div>
          <h4>高级数学功能</h4>
          <p>数据拟合、滤波器应用、相位测量、功率谱密度计算等专业数学分析功能</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">⚡</div>
          <h4>高性能渲染</h4>
          <p>基于ECharts优化渲染，支持大数据量显示，LTTB采样算法，60fps流畅动画</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">🎨</div>
          <h4>科学主题</h4>
          <p>专业的科学仪器主题，支持浅色、深色、科学三种主题模式，适合不同使用场景</p>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import EnhancedEasyChart from '@/components/charts/EnhancedEasyChart.vue'
import type { ChartData, SeriesConfig } from '@/types/chart'

// 响应式数据
const chartData = ref<ChartData>({
  series: [[], []],
  xStart: 0,
  xInterval: 0.001,
  sampleRate: 1000
})

const seriesConfigs = ref<SeriesConfig[]>([
  {
    name: '信号1 (正弦波)',
    color: '#409eff',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'none',
    markerSize: 4,
    visible: true
  },
  {
    name: '信号2 (方波)',
    color: '#67c23a',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'none',
    markerSize: 4,
    visible: true
  }
])

// 信号参数
const signal1 = ref({
  frequency: 50,
  amplitude: 2,
  phase: 0
})

const signal2 = ref({
  frequency: 25,
  amplitude: 1.5,
  dutyCycle: 50
})

const noiseLevel = ref(0.1)
const sampleRate = ref(1000)
const dataPoints = ref(1024)
const isAnimating = ref(false)

// FFT分析结果
const fftResult = ref<{ frequencies: number[]; magnitudes: number[] }>()

// 动画定时器
let animationTimer: number | null = null

// 计算属性
const dominantFrequencies = computed(() => {
  if (!fftResult.value) return []
  
  const { frequencies, magnitudes } = fftResult.value
  const peaks: { frequency: number; magnitude: number }[] = []
  
  // 找到前5个峰值
  for (let i = 1; i < magnitudes.length - 1; i++) {
    if (magnitudes[i] > magnitudes[i - 1] && magnitudes[i] > magnitudes[i + 1] && magnitudes[i] > 0.1) {
      peaks.push({
        frequency: frequencies[i],
        magnitude: magnitudes[i]
      })
    }
  }
  
  return peaks.sort((a, b) => b.magnitude - a.magnitude).slice(0, 5)
})

const peakFrequency = computed(() => {
  if (!fftResult.value) return 0
  const maxIndex = fftResult.value.magnitudes.indexOf(Math.max(...fftResult.value.magnitudes))
  return fftResult.value.frequencies[maxIndex] || 0
})

const peakMagnitude = computed(() => {
  if (!fftResult.value) return 0
  return Math.max(...fftResult.value.magnitudes)
})

const frequencyResolution = computed(() => {
  return sampleRate.value / dataPoints.value
})

const effectiveBandwidth = computed(() => {
  return sampleRate.value / 2
})

// 生成信号数据
const generateSignalData = () => {
  const points = dataPoints.value
  const dt = 1 / sampleRate.value
  const series1: number[] = []
  const series2: number[] = []
  
  for (let i = 0; i < points; i++) {
    const t = i * dt
    
    // 信号1: 正弦波
    const sine = signal1.value.amplitude * Math.sin(
      2 * Math.PI * signal1.value.frequency * t + signal1.value.phase * Math.PI / 180
    )
    
    // 信号2: 方波
    const period = 1 / signal2.value.frequency
    const phase = (t % period) / period
    const square = phase < (signal2.value.dutyCycle / 100) ? 
      signal2.value.amplitude : -signal2.value.amplitude
    
    // 添加噪声
    const noise1 = (Math.random() - 0.5) * noiseLevel.value * 2
    const noise2 = (Math.random() - 0.5) * noiseLevel.value * 2
    
    series1.push(sine + noise1)
    series2.push(square + noise2)
  }
  
  chartData.value = {
    series: [series1, series2],
    xStart: 0,
    xInterval: dt,
    sampleRate: sampleRate.value
  }
}

// 更新信号
const updateSignals = () => {
  generateSignalData()
}

// 重置参数
const resetSignals = () => {
  signal1.value = { frequency: 50, amplitude: 2, phase: 0 }
  signal2.value = { frequency: 25, amplitude: 1.5, dutyCycle: 50 }
  noiseLevel.value = 0.1
  sampleRate.value = 1000
  dataPoints.value = 1024
  updateSignals()
}

// 添加随机噪声
const addRandomNoise = () => {
  noiseLevel.value = Math.min(0.5, noiseLevel.value + 0.05)
  updateSignals()
}

// 开始/停止动画
const startAnimation = () => {
  if (isAnimating.value) {
    if (animationTimer) {
      clearInterval(animationTimer)
      animationTimer = null
    }
    isAnimating.value = false
  } else {
    animationTimer = setInterval(() => {
      // 随机变化频率
      signal1.value.frequency += (Math.random() - 0.5) * 2
      signal1.value.frequency = Math.max(10, Math.min(200, signal1.value.frequency))
      
      signal2.value.frequency += (Math.random() - 0.5) * 1
      signal2.value.frequency = Math.max(5, Math.min(100, signal2.value.frequency))
      
      updateSignals()
    }, 500)
    isAnimating.value = true
  }
}

// 处理FFT更新
const handleFFTUpdate = (data: { frequencies: number[]; magnitudes: number[] }) => {
  fftResult.value = data
}

// 处理测量更新
const handleMeasurementUpdate = (measurements: any[]) => {
  console.log('测量更新:', measurements)
}

// 生命周期
onMounted(() => {
  generateSignalData()
})

onUnmounted(() => {
  if (animationTimer) {
    clearInterval(animationTimer)
  }
})
</script>

<style lang="scss" scoped>
.enhanced-charts-view {
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  text-align: center;
  margin-bottom: 48px;
  
  .page-title {
    font-size: 36px;
    font-weight: 700;
    color: var(--text-primary);
    margin-bottom: 12px;
  }
  
  .page-subtitle {
    font-size: 18px;
    color: var(--text-secondary);
    line-height: 1.6;
  }
}

.demo-section,
.control-section,
.analysis-section,
.features-section {
  margin-bottom: 48px;
  
  .section-header {
    margin-bottom: 24px;
    
    .section-title {
      font-size: 24px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 8px;
    }
    
    .section-description {
      font-size: 16px;
      color: var(--text-secondary);
      line-height: 1.6;
    }
  }
}

.demo-container {
  background: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.control-panel {
  background: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 24px;
  
  .control-group {
    h4 {
      font-size: 16px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 16px;
      text-align: center;
    }
    
    .control-item {
      margin-bottom: 16px;
      
      label {
        display: block;
        font-size: 14px;
        font-weight: 500;
        color: var(--text-primary);
        margin-bottom: 8px;
      }
    }
  }
  
  .control-actions {
    margin-top: 24px;
    text-align: center;
    
    .el-button {
      margin: 0 8px;
    }
  }
}

.analysis-results {
  .result-card {
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 8px;
    padding: 16px;
    
    h4 {
      font-size: 16px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 12px;
    }
    
    .frequency-list {
      .frequency-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 4px 0;
        border-bottom: 1px solid var(--border-color);
        
        &:last-child {
          border-bottom: none;
        }
        
        .frequency-value {
          font-family: 'Consolas', 'Monaco', monospace;
          color: var(--primary-color);
          font-weight: 600;
        }
        
        .magnitude-value {
          font-family: 'Consolas', 'Monaco', monospace;
          color: var(--text-secondary);
        }
      }
    }
    
    .stats-list {
      .stat-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 4px 0;
        
        .stat-label {
          font-size: 14px;
          color: var(--text-secondary);
        }
        
        .stat-value {
          font-family: 'Consolas', 'Monaco', monospace;
          color: var(--primary-color);
          font-weight: 600;
        }
      }
    }
  }
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 24px;
  
  .feature-item {
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 12px;
    padding: 24px;
    text-align: center;
    transition: all 0.3s ease;
    
    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
    }
    
    .feature-icon {
      font-size: 32px;
      margin-bottom: 16px;
    }
    
    h4 {
      font-size: 16px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 8px;
    }
    
    p {
      font-size: 14px;
      color: var(--text-secondary);
      line-height: 1.5;
      margin: 0;
    }
  }
}

@media (max-width: 768px) {
  .enhanced-charts-view {
    padding: 16px;
  }
  
  .page-header {
    margin-bottom: 32px;
    
    .page-title {
      font-size: 28px;
    }
    
    .page-subtitle {
      font-size: 16px;
    }
  }
  
  .control-panel {
    padding: 16px;
    
    .control-actions {
      .el-button {
        margin: 4px;
        width: calc(50% - 8px);
      }
    }
  }
  
  .features-grid {
    grid-template-columns: 1fr;
    gap: 16px;
  }
}
</style>
