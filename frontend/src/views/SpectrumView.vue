<template>
  <div class="spectrum-view">
    <div class="page-header">
      <h1 class="page-title">频谱分析仪</h1>
      <p class="page-subtitle">频谱显示、标记功能、参考电平等频域分析组件</p>
    </div>

    <div class="spectrum-container">
      <!-- 频谱分析仪主面板 -->
      <div class="spectrum-panel">
        <div class="panel-header">
          <div class="instrument-title">
            <h3>频谱分析仪</h3>
            <span class="model">SA-3000 Series</span>
          </div>
          <div class="status-indicators">
            <div class="status-item">
              <div class="status-dot status-sweep"></div>
              <span>SWEEP</span>
            </div>
            <div class="status-item">
              <div class="status-dot status-cal"></div>
              <span>CAL</span>
            </div>
          </div>
        </div>

        <!-- 频谱显示区域 -->
        <div class="spectrum-display">
          <div class="display-screen">
            <svg width="100%" height="350" viewBox="0 0 800 350" class="spectrum-svg">
              <!-- 网格 -->
              <defs>
                <pattern id="spectrumGrid" width="80" height="35" patternUnits="userSpaceOnUse">
                  <path d="M 80 0 L 0 0 0 35" fill="none" stroke="#1a4d2e" stroke-width="0.5" opacity="0.6"/>
                </pattern>
                <pattern id="spectrumMajorGrid" width="160" height="70" patternUnits="userSpaceOnUse">
                  <path d="M 160 0 L 0 0 0 70" fill="none" stroke="#1a4d2e" stroke-width="1" opacity="0.8"/>
                </pattern>
                <linearGradient id="spectrumGradient" x1="0%" y1="0%" x2="0%" y2="100%">
                  <stop offset="0%" style="stop-color:#00ff00;stop-opacity:0.8" />
                  <stop offset="50%" style="stop-color:#ffff00;stop-opacity:0.6" />
                  <stop offset="100%" style="stop-color:#ff0000;stop-opacity:0.4" />
                </linearGradient>
              </defs>
              
              <rect width="100%" height="100%" fill="#001a0d"/>
              <rect width="100%" height="100%" fill="url(#spectrumGrid)"/>
              <rect width="100%" height="100%" fill="url(#spectrumMajorGrid)"/>
              
              <!-- 频谱曲线 -->
              <path :d="spectrumPath" fill="url(#spectrumGradient)" stroke="#00ff00" stroke-width="1" opacity="0.9"/>
              
              <!-- 参考电平线 -->
              <line x1="0" :y1="referenceLevel" x2="800" :y2="referenceLevel" stroke="#ff6600" stroke-width="1" stroke-dasharray="5,5" opacity="0.8"/>
              
              <!-- 标记点 -->
              <g v-for="(marker, index) in markers" :key="index">
                <circle :cx="marker.x" :cy="marker.y" r="4" :fill="marker.color" stroke="white" stroke-width="1"/>
                <text :x="marker.x + 8" :y="marker.y - 8" fill="white" font-size="10" font-family="monospace">
                  M{{ index + 1 }}: {{ marker.frequency }}MHz
                </text>
                <text :x="marker.x + 8" :y="marker.y + 16" fill="white" font-size="10" font-family="monospace">
                  {{ marker.amplitude }}dBm
                </text>
              </g>
              
              <!-- 频率标尺 -->
              <g class="frequency-scale">
                <text v-for="(freq, index) in frequencyScale" :key="index" 
                      :x="index * 160 + 80" y="340" 
                      fill="#00ff00" font-size="10" font-family="monospace" text-anchor="middle">
                  {{ freq }}
                </text>
              </g>
              
              <!-- 幅度标尺 -->
              <g class="amplitude-scale">
                <text v-for="(amp, index) in amplitudeScale" :key="index" 
                      x="10" :y="index * 70 + 20" 
                      fill="#00ff00" font-size="10" font-family="monospace">
                  {{ amp }}
                </text>
              </g>
            </svg>
            
            <!-- 测量信息显示 -->
            <div class="measurement-overlay">
              <div class="measurement-info">
                <div class="info-item">
                  <span class="label">中心频率:</span>
                  <span class="value">{{ centerFrequency }} MHz</span>
                </div>
                <div class="info-item">
                  <span class="label">扫描宽度:</span>
                  <span class="value">{{ spanFrequency }} MHz</span>
                </div>
                <div class="info-item">
                  <span class="label">分辨率:</span>
                  <span class="value">{{ rbw }} kHz</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 控制面板 -->
        <div class="control-panel">
          <!-- 频率控制 -->
          <div class="control-section">
            <h4>频率控制</h4>
            <div class="frequency-controls">
              <div class="control-row">
                <span>中心频率:</span>
                <input type="number" v-model="centerFrequency" step="1" min="1" max="6000" />
                <span>MHz</span>
              </div>
              <div class="control-row">
                <span>扫描宽度:</span>
                <select v-model="spanFrequency">
                  <option value="10">10 MHz</option>
                  <option value="50">50 MHz</option>
                  <option value="100">100 MHz</option>
                  <option value="500">500 MHz</option>
                  <option value="1000">1 GHz</option>
                  <option value="2000">2 GHz</option>
                </select>
              </div>
              <div class="control-row">
                <span>起始频率:</span>
                <span class="readonly-value">{{ startFrequency }} MHz</span>
              </div>
              <div class="control-row">
                <span>停止频率:</span>
                <span class="readonly-value">{{ stopFrequency }} MHz</span>
              </div>
            </div>
          </div>

          <!-- 幅度控制 -->
          <div class="control-section">
            <h4>幅度控制</h4>
            <div class="amplitude-controls">
              <div class="control-row">
                <span>参考电平:</span>
                <input type="number" v-model="refLevel" step="1" min="-100" max="30" />
                <span>dBm</span>
              </div>
              <div class="control-row">
                <span>衰减:</span>
                <select v-model="attenuation">
                  <option value="0">0 dB</option>
                  <option value="10">10 dB</option>
                  <option value="20">20 dB</option>
                  <option value="30">30 dB</option>
                </select>
              </div>
              <div class="control-row">
                <span>刻度/格:</span>
                <select v-model="scalePerDiv">
                  <option value="1">1 dB/div</option>
                  <option value="2">2 dB/div</option>
                  <option value="5">5 dB/div</option>
                  <option value="10">10 dB/div</option>
                </select>
              </div>
            </div>
          </div>

          <!-- 带宽控制 -->
          <div class="control-section">
            <h4>带宽控制</h4>
            <div class="bandwidth-controls">
              <div class="control-row">
                <span>分辨率带宽:</span>
                <select v-model="rbw">
                  <option value="1">1 kHz</option>
                  <option value="3">3 kHz</option>
                  <option value="10">10 kHz</option>
                  <option value="30">30 kHz</option>
                  <option value="100">100 kHz</option>
                  <option value="300">300 kHz</option>
                  <option value="1000">1 MHz</option>
                </select>
              </div>
              <div class="control-row">
                <span>视频带宽:</span>
                <select v-model="vbw">
                  <option value="1">1 kHz</option>
                  <option value="3">3 kHz</option>
                  <option value="10">10 kHz</option>
                  <option value="30">30 kHz</option>
                  <option value="100">100 kHz</option>
                  <option value="300">300 kHz</option>
                </select>
              </div>
              <div class="control-row">
                <span>扫描时间:</span>
                <select v-model="sweepTime">
                  <option value="0.1">100 ms</option>
                  <option value="0.5">500 ms</option>
                  <option value="1">1 s</option>
                  <option value="2">2 s</option>
                  <option value="5">5 s</option>
                  <option value="10">10 s</option>
                </select>
              </div>
            </div>
          </div>

          <!-- 标记控制 -->
          <div class="control-section">
            <h4>标记控制</h4>
            <div class="marker-controls">
              <div class="control-row">
                <button @click="addMarker" :disabled="markers.length >= 4">
                  添加标记
                </button>
                <button @click="clearMarkers" :disabled="markers.length === 0">
                  清除标记
                </button>
              </div>
              <div class="control-row">
                <button @click="peakSearch">
                  峰值搜索
                </button>
                <button @click="nextPeak">
                  下一峰值
                </button>
              </div>
              <div class="marker-list" v-if="markers.length > 0">
                <div v-for="(marker, index) in markers" :key="index" class="marker-item">
                  <span class="marker-label" :style="{ color: marker.color }">M{{ index + 1 }}:</span>
                  <span class="marker-freq">{{ marker.frequency }}MHz</span>
                  <span class="marker-amp">{{ marker.amplitude }}dBm</span>
                  <button @click="removeMarker(index)" class="remove-btn">×</button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'

// 响应式数据
const centerFrequency = ref(1000)
const spanFrequency = ref(100)
const refLevel = ref(0)
const attenuation = ref(10)
const scalePerDiv = ref(10)
const rbw = ref(100)
const vbw = ref(100)
const sweepTime = ref(1)

// 标记数据
const markers = ref([
  { x: 200, y: 100, frequency: '950.0', amplitude: '-25.3', color: '#ff0000' },
  { x: 400, y: 150, frequency: '1000.0', amplitude: '-35.7', color: '#00ff00' },
  { x: 600, y: 120, frequency: '1050.0', amplitude: '-28.9', color: '#0000ff' }
])

// 动画定时器
let animationTimer: number | null = null

// 计算属性
const startFrequency = computed(() => {
  return centerFrequency.value - spanFrequency.value / 2
})

const stopFrequency = computed(() => {
  return centerFrequency.value + spanFrequency.value / 2
})

const referenceLevel = computed(() => {
  return 50 + (refLevel.value / scalePerDiv.value) * 10
})

const frequencyScale = computed(() => {
  const start = startFrequency.value
  const step = spanFrequency.value / 5
  const result = []
  for (let i = 0; i < 6; i++) {
    result.push((start + i * step).toFixed(0))
  }
  return result
})

const amplitudeScale = computed(() => {
  const ref = refLevel.value
  const scale = scalePerDiv.value
  const result = []
  for (let i = 0; i < 6; i++) {
    result.push((ref - i * scale).toFixed(0))
  }
  return result
})

const spectrumPath = ref('')

// 生成频谱数据
const generateSpectrum = () => {
  const points = []
  const time = Date.now() / 1000
  
  // 基础噪声底
  const noiseFloor = 300 + Math.sin(time * 0.5) * 10
  
  for (let x = 0; x <= 800; x += 2) {
    let y = noiseFloor
    
    // 添加几个信号峰
    const freq = (x / 800) * spanFrequency.value + startFrequency.value
    
    // 主信号峰 (中心频率附近)
    if (Math.abs(freq - centerFrequency.value) < 10) {
      y -= 80 + Math.sin(time + freq / 100) * 20
    }
    
    // 其他信号峰
    if (Math.abs(freq - (centerFrequency.value - 30)) < 5) {
      y -= 40 + Math.sin(time * 1.5 + freq / 50) * 15
    }
    
    if (Math.abs(freq - (centerFrequency.value + 25)) < 8) {
      y -= 60 + Math.cos(time * 0.8 + freq / 80) * 10
    }
    
    // 添加随机噪声
    y += (Math.random() - 0.5) * 10
    
    // 限制范围
    y = Math.max(50, Math.min(350, y))
    
    points.push(`${x},${y}`)
  }
  
  // 闭合路径到底部
  points.push('800,350')
  points.push('0,350')
  
  spectrumPath.value = `M ${points.join(' L ')} Z`
}

// 标记控制方法
const addMarker = () => {
  if (markers.value.length < 4) {
    const colors = ['#ff0000', '#00ff00', '#0000ff', '#ffff00']
    const x = 200 + Math.random() * 400
    const y = 100 + Math.random() * 150
    const freq = ((x / 800) * spanFrequency.value + startFrequency.value).toFixed(1)
    const amp = (refLevel.value - ((y - 50) / 300) * 100).toFixed(1)
    
    markers.value.push({
      x,
      y,
      frequency: freq,
      amplitude: amp,
      color: colors[markers.value.length]
    })
  }
}

const removeMarker = (index: number) => {
  markers.value.splice(index, 1)
}

const clearMarkers = () => {
  markers.value = []
}

const peakSearch = () => {
  // 模拟峰值搜索
  const peakX = 300 + Math.random() * 200
  const peakY = 80 + Math.random() * 40
  const freq = ((peakX / 800) * spanFrequency.value + startFrequency.value).toFixed(1)
  const amp = (refLevel.value - ((peakY - 50) / 300) * 100).toFixed(1)
  
  markers.value = [{
    x: peakX,
    y: peakY,
    frequency: freq,
    amplitude: amp,
    color: '#ff0000'
  }]
}

const nextPeak = () => {
  if (markers.value.length > 0) {
    const currentMarker = markers.value[0]
    const newX = currentMarker.x + 50 + Math.random() * 100
    const newY = currentMarker.y + (Math.random() - 0.5) * 40
    
    if (newX < 800) {
      const freq = ((newX / 800) * spanFrequency.value + startFrequency.value).toFixed(1)
      const amp = (refLevel.value - ((newY - 50) / 300) * 100).toFixed(1)
      
      markers.value[0] = {
        x: newX,
        y: Math.max(50, Math.min(300, newY)),
        frequency: freq,
        amplitude: amp,
        color: '#ff0000'
      }
    }
  }
}

// 启动动画
const startAnimation = () => {
  animationTimer = setInterval(() => {
    generateSpectrum()
  }, 100)
}

// 停止动画
const stopAnimation = () => {
  if (animationTimer) {
    clearInterval(animationTimer)
    animationTimer = null
  }
}

// 生命周期
onMounted(() => {
  generateSpectrum()
  startAnimation()
})

onUnmounted(() => {
  stopAnimation()
})
</script>

<style lang="scss" scoped>
@import "@/styles/variables.scss";

.spectrum-view {
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  text-align: center;
  margin-bottom: 32px;
  
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

.spectrum-container {
  background: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  overflow: hidden;
}

.spectrum-panel {
  .panel-header {
    background: linear-gradient(135deg, #065f46, #047857);
    color: white;
    padding: 16px 24px;
    display: flex;
    justify-content: space-between;
    align-items: center;

    .instrument-title {
      h3 {
        font-size: 20px;
        font-weight: 600;
        margin: 0 0 4px 0;
      }

      .model {
        font-size: 14px;
        opacity: 0.8;
      }
    }

    .status-indicators {
      display: flex;
      gap: 16px;

      .status-item {
        display: flex;
        align-items: center;
        gap: 6px;
        font-size: 12px;

        .status-dot {
          width: 8px;
          height: 8px;
          border-radius: 50%;

          &.status-sweep {
            background: #10B981;
            box-shadow: 0 0 8px #10B981;
            animation: pulse 2s infinite;
          }

          &.status-cal {
            background: #3B82F6;
            box-shadow: 0 0 8px #3B82F6;
          }
        }
      }
    }
  }

  .spectrum-display {
    background: #001a0d;
    position: relative;
    padding: 16px;

    .display-screen {
      position: relative;
      border: 2px solid #1a4d2e;
      border-radius: 4px;
      overflow: hidden;
    }

    .spectrum-svg {
      display: block;
      width: 100%;
      height: 350px;
    }

    .measurement-overlay {
      position: absolute;
      top: 24px;
      right: 24px;
      background: rgba(0, 0, 0, 0.7);
      border: 1px solid #1a4d2e;
      border-radius: 4px;
      padding: 12px;

      .measurement-info {
        display: flex;
        flex-direction: column;
        gap: 6px;

        .info-item {
          display: flex;
          justify-content: space-between;
          gap: 12px;
          font-size: 11px;
          font-family: monospace;

          .label {
            color: #00ff00;
          }

          .value {
            color: white;
            font-weight: bold;
          }
        }
      }
    }
  }

  .control-panel {
    padding: 24px;
    background: var(--background-color);
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 24px;

    .control-section {
      h4 {
        font-size: 16px;
        font-weight: 600;
        color: var(--text-primary);
        margin-bottom: 16px;
        padding-bottom: 8px;
        border-bottom: 2px solid #047857;
      }

      .control-row {
        display: flex;
        align-items: center;
        gap: 8px;
        margin-bottom: 12px;

        span {
          font-size: 14px;
          color: var(--text-secondary);
          min-width: 80px;

          &.readonly-value {
            color: var(--text-primary);
            font-weight: 500;
            font-family: var(--font-family-mono);
          }
        }

        input, select {
          flex: 1;
          padding: 4px 8px;
          border: 1px solid var(--border-color);
          border-radius: 4px;
          background: var(--surface-color);
          color: var(--text-primary);
          font-size: 12px;
        }

        button {
          padding: 6px 12px;
          border: 1px solid var(--border-color);
          border-radius: 4px;
          background: var(--surface-color);
          color: var(--text-secondary);
          font-size: 12px;
          cursor: pointer;
          transition: all 0.2s ease;

          &:hover:not(:disabled) {
            border-color: #047857;
            color: #047857;
          }

          &:disabled {
            opacity: 0.5;
            cursor: not-allowed;
          }
        }
      }

      .marker-list {
        margin-top: 12px;
        padding: 12px;
        background: var(--surface-color);
        border: 1px solid var(--border-color);
        border-radius: 6px;

        .marker-item {
          display: flex;
          align-items: center;
          gap: 8px;
          padding: 4px 0;
          font-size: 11px;
          font-family: var(--font-family-mono);

          .marker-label {
            font-weight: bold;
            min-width: 24px;
          }

          .marker-freq, .marker-amp {
            color: var(--text-primary);
            min-width: 60px;
          }

          .remove-btn {
            width: 20px;
            height: 20px;
            padding: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            background: #dc2626;
            color: white;
            border: none;
            border-radius: 50%;
            font-size: 14px;
            line-height: 1;

            &:hover {
              background: #b91c1c;
            }
          }
        }
      }
    }
  }
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

// 响应式设计
@media (max-width: 768px) {
  .spectrum-view {
    padding: 16px;
  }

  .control-panel {
    grid-template-columns: 1fr;
    padding: 16px;
  }

  .page-header {
    .page-title {
      font-size: 28px;
    }

    .page-subtitle {
      font-size: 16px;
    }
  }

  .measurement-overlay {
    position: static !important;
    margin-top: 12px;
    background: rgba(0, 0, 0, 0.8) !important;
  }
}
</style>
