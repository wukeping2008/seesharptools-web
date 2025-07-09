<template>
  <div class="instruments-view">
    <div class="page-header">
      <h1 class="page-title">仪表控件</h1>
      <p class="page-subtitle">精密测量仪表控件，支持高精度显示和多种测量单位</p>
    </div>

    <!-- 精密圆形仪表展示 -->
    <section class="instrument-section">
      <div class="section-header">
        <h2 class="section-title">精密圆形仪表</h2>
        <p class="section-description">
          专业级圆形仪表，具有精密刻度系统、阈值标记、数字显示窗口等特性
        </p>
      </div>

      <div class="instruments-grid">
        <!-- 电压表 -->
        <div class="instrument-card">
          <div class="card-header">
            <h3>电压表</h3>
            <div class="controls">
              <el-button size="small" @click="toggleVoltageAnimation">
                {{ voltageAnimated ? '停止' : '开始' }}
              </el-button>
            </div>
          </div>
          <div class="card-content">
            <PrecisionCircularGauge
              :value="voltageValue"
              :min="0"
              :max="100"
              unit="V"
              title="电压测量"
              :precision="2"
              :size="280"
              :thresholds="voltageThresholds"
              :animated="true"
            />
          </div>
          <div class="card-footer">
            <div class="value-display">
              当前值: <span class="value">{{ voltageValue.toFixed(2) }} V</span>
            </div>
          </div>
        </div>

        <!-- 频率表 -->
        <div class="instrument-card">
          <div class="card-header">
            <h3>频率表</h3>
            <div class="controls">
              <el-button size="small" @click="toggleFrequencyAnimation">
                {{ frequencyAnimated ? '停止' : '开始' }}
              </el-button>
            </div>
          </div>
          <div class="card-content">
            <PrecisionCircularGauge
              :value="frequencyValue"
              :min="0"
              :max="1000"
              unit="Hz"
              title="频率测量"
              :precision="1"
              :size="280"
              :thresholds="frequencyThresholds"
              :animated="true"
            />
          </div>
          <div class="card-footer">
            <div class="value-display">
              当前值: <span class="value">{{ frequencyValue.toFixed(1) }} Hz</span>
            </div>
          </div>
        </div>

        <!-- 温度表 -->
        <div class="instrument-card">
          <div class="card-header">
            <h3>温度表</h3>
            <div class="controls">
              <el-button size="small" @click="toggleTemperatureAnimation">
                {{ temperatureAnimated ? '停止' : '开始' }}
              </el-button>
            </div>
          </div>
          <div class="card-content">
            <PrecisionCircularGauge
              :value="temperatureValue"
              :min="-20"
              :max="120"
              unit="°C"
              title="温度测量"
              :precision="1"
              :size="280"
              :thresholds="temperatureThresholds"
              :animated="true"
            />
          </div>
          <div class="card-footer">
            <div class="value-display">
              当前值: <span class="value">{{ temperatureValue.toFixed(1) }} °C</span>
            </div>
          </div>
        </div>

        <!-- 压力表 -->
        <div class="instrument-card">
          <div class="card-header">
            <h3>压力表</h3>
            <div class="controls">
              <el-button size="small" @click="togglePressureAnimation">
                {{ pressureAnimated ? '停止' : '开始' }}
              </el-button>
            </div>
          </div>
          <div class="card-content">
            <PrecisionCircularGauge
              :value="pressureValue"
              :min="0"
              :max="10"
              unit="bar"
              title="压力测量"
              :precision="2"
              :size="280"
              :thresholds="pressureThresholds"
              :animated="true"
            />
          </div>
          <div class="card-footer">
            <div class="value-display">
              当前值: <span class="value">{{ pressureValue.toFixed(2) }} bar</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 控制面板 -->
    <section class="control-section">
      <div class="section-header">
        <h2 class="section-title">控制面板</h2>
        <p class="section-description">手动调节各个仪表的数值</p>
      </div>

      <div class="control-panel">
        <div class="control-group">
          <label>电压 (V):</label>
          <el-slider
            v-model="voltageValue"
            :min="0"
            :max="100"
            :step="0.1"
            show-input
            :input-size="'small'"
          />
        </div>

        <div class="control-group">
          <label>频率 (Hz):</label>
          <el-slider
            v-model="frequencyValue"
            :min="0"
            :max="1000"
            :step="1"
            show-input
            :input-size="'small'"
          />
        </div>

        <div class="control-group">
          <label>温度 (°C):</label>
          <el-slider
            v-model="temperatureValue"
            :min="-20"
            :max="120"
            :step="0.1"
            show-input
            :input-size="'small'"
          />
        </div>

        <div class="control-group">
          <label>压力 (bar):</label>
          <el-slider
            v-model="pressureValue"
            :min="0"
            :max="10"
            :step="0.01"
            show-input
            :input-size="'small'"
          />
        </div>
      </div>
    </section>

    <!-- 特性说明 -->
    <section class="features-section">
      <div class="section-header">
        <h2 class="section-title">控件特性</h2>
      </div>

      <div class="features-grid">
        <div class="feature-item">
          <div class="feature-icon">📏</div>
          <h4>精密刻度系统</h4>
          <p>主刻度、次刻度、微刻度的完整体系，提供精确的读数参考</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">🎯</div>
          <h4>阈值标记</h4>
          <p>支持多个阈值设置，自动变色提醒，适用于报警和监控场景</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">📱</div>
          <h4>数字显示</h4>
          <p>内置数字显示窗口，支持科学计数法和工程计数法</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">🎨</div>
          <h4>专业外观</h4>
          <p>参考SeeSharpTools设计风格，科学仪器级别的视觉效果</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">⚡</div>
          <h4>实时更新</h4>
          <p>支持高频数据更新，平滑动画过渡，适合实时监控</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">🔧</div>
          <h4>高度可配置</h4>
          <p>支持自定义颜色、尺寸、精度等多种配置选项</p>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import PrecisionCircularGauge from '@/components/instruments/PrecisionCircularGauge.vue'
import type { ThresholdConfig } from '@/types/instrument'

// 响应式数据
const voltageValue = ref(75.2)
const frequencyValue = ref(450.0)
const temperatureValue = ref(25.5)
const pressureValue = ref(3.45)

const voltageAnimated = ref(false)
const frequencyAnimated = ref(false)
const temperatureAnimated = ref(false)
const pressureAnimated = ref(false)

// 动画定时器
let voltageTimer: number | null = null
let frequencyTimer: number | null = null
let temperatureTimer: number | null = null
let pressureTimer: number | null = null

// 阈值配置
const voltageThresholds: ThresholdConfig[] = [
  { value: 80, color: '#F59E0B', label: '警告', type: 'warning' },
  { value: 90, color: '#EF4444', label: '危险', type: 'error' }
]

const frequencyThresholds: ThresholdConfig[] = [
  { value: 800, color: '#F59E0B', label: '高频', type: 'warning' },
  { value: 950, color: '#EF4444', label: '超限', type: 'error' }
]

const temperatureThresholds: ThresholdConfig[] = [
  { value: 60, color: '#F59E0B', label: '高温', type: 'warning' },
  { value: 80, color: '#EF4444', label: '过热', type: 'error' }
]

const pressureThresholds: ThresholdConfig[] = [
  { value: 7, color: '#F59E0B', label: '高压', type: 'warning' },
  { value: 9, color: '#EF4444', label: '危险', type: 'error' }
]

// 动画控制方法
const toggleVoltageAnimation = () => {
  voltageAnimated.value = !voltageAnimated.value
  if (voltageAnimated.value) {
    voltageTimer = setInterval(() => {
      voltageValue.value = 50 + Math.sin(Date.now() / 1000) * 30 + Math.random() * 10
    }, 100)
  } else {
    if (voltageTimer) {
      clearInterval(voltageTimer)
      voltageTimer = null
    }
  }
}

const toggleFrequencyAnimation = () => {
  frequencyAnimated.value = !frequencyAnimated.value
  if (frequencyAnimated.value) {
    frequencyTimer = setInterval(() => {
      frequencyValue.value = 500 + Math.sin(Date.now() / 800) * 200 + Math.random() * 50
    }, 150)
  } else {
    if (frequencyTimer) {
      clearInterval(frequencyTimer)
      frequencyTimer = null
    }
  }
}

const toggleTemperatureAnimation = () => {
  temperatureAnimated.value = !temperatureAnimated.value
  if (temperatureAnimated.value) {
    temperatureTimer = setInterval(() => {
      temperatureValue.value = 25 + Math.sin(Date.now() / 2000) * 15 + Math.random() * 5
    }, 200)
  } else {
    if (temperatureTimer) {
      clearInterval(temperatureTimer)
      temperatureTimer = null
    }
  }
}

const togglePressureAnimation = () => {
  pressureAnimated.value = !pressureAnimated.value
  if (pressureAnimated.value) {
    pressureTimer = setInterval(() => {
      pressureValue.value = 3 + Math.sin(Date.now() / 1500) * 2 + Math.random() * 1
    }, 120)
  } else {
    if (pressureTimer) {
      clearInterval(pressureTimer)
      pressureTimer = null
    }
  }
}

// 清理定时器
const clearAllTimers = () => {
  if (voltageTimer) clearInterval(voltageTimer)
  if (frequencyTimer) clearInterval(frequencyTimer)
  if (temperatureTimer) clearInterval(temperatureTimer)
  if (pressureTimer) clearInterval(pressureTimer)
}

// 生命周期
onMounted(() => {
  // 启动默认动画
  toggleVoltageAnimation()
})

onUnmounted(() => {
  clearAllTimers()
})
</script>

<style lang="scss" scoped>
.instruments-view {
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

.instrument-section,
.control-section,
.features-section {
  margin-bottom: 48px;
  
  .section-header {
    margin-bottom: 32px;
    
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

.instruments-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 32px;
  
  .instrument-card {
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 16px;
    padding: 24px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    transition: all 0.3s ease;
    
    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
    }
    
    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      
      h3 {
        font-size: 18px;
        font-weight: 600;
        color: var(--text-primary);
        margin: 0;
      }
      
      .controls {
        display: flex;
        gap: 8px;
      }
    }
    
    .card-content {
      display: flex;
      justify-content: center;
      margin-bottom: 20px;
    }
    
    .card-footer {
      text-align: center;
      
      .value-display {
        font-size: 14px;
        color: var(--text-secondary);
        
        .value {
          font-family: 'Consolas', 'Monaco', monospace;
          font-weight: bold;
          color: var(--primary-color);
        }
      }
    }
  }
}

.control-panel {
  background: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 32px;
  
  .control-group {
    margin-bottom: 24px;
    
    &:last-child {
      margin-bottom: 0;
    }
    
    label {
      display: block;
      font-size: 14px;
      font-weight: 500;
      color: var(--text-primary);
      margin-bottom: 8px;
    }
  }
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
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
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
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

// 响应式设计
@media (max-width: 768px) {
  .instruments-view {
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
  
  .instruments-grid {
    grid-template-columns: 1fr;
    gap: 24px;
  }
  
  .instrument-card {
    padding: 16px;
    
    .card-header {
      flex-direction: column;
      gap: 12px;
      align-items: flex-start;
    }
  }
  
  .control-panel {
    padding: 20px;
  }
  
  .features-grid {
    grid-template-columns: 1fr;
    gap: 16px;
  }
}
</style>
