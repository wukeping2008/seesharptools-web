<template>
  <div class="signal-generator professional-control">
    <!-- 控件标题 -->
    <div class="generator-header">
      <div class="header-left">
        <h3 class="generator-title">
          <el-icon class="title-icon"><Timer /></el-icon>
          任意波形信号发生器
        </h3>
        <div class="generator-status">
          <span class="status-indicator" :class="outputEnabled ? 'active' : 'inactive'">
            {{ outputEnabled ? '输出开启' : '输出关闭' }}
          </span>
        </div>
      </div>
      <div class="header-right">
        <el-button 
          :type="outputEnabled ? 'danger' : 'primary'" 
          @click="toggleOutput"
          size="large"
        >
          <el-icon><Switch /></el-icon>
          {{ outputEnabled ? '关闭输出' : '开启输出' }}
        </el-button>
      </div>
    </div>

    <!-- 主控制面板 -->
    <div class="generator-body">
      <el-row :gutter="24">
        <!-- 左侧：波形选择和参数控制 -->
        <el-col :span="12">
          <div class="control-panel">
            <div class="panel-section">
              <h4 class="section-title">波形类型</h4>
              <div class="waveform-selector">
                <div 
                  v-for="waveform in waveformTypes" 
                  :key="waveform.type"
                  class="waveform-option"
                  :class="{ active: selectedWaveform === waveform.type }"
                  @click="selectWaveform(waveform.type)"
                >
                  <div class="waveform-icon">{{ waveform.icon }}</div>
                  <span class="waveform-name">{{ waveform.name }}</span>
                </div>
              </div>
            </div>

            <div class="panel-section">
              <h4 class="section-title">基本参数</h4>
              <div class="parameter-grid">
                <div class="parameter-item">
                  <label>频率</label>
                  <div class="parameter-input">
                    <el-input-number
                      v-model="frequency"
                      :min="frequencyRange.min"
                      :max="frequencyRange.max"
                      :step="frequencyStep"
                      :precision="frequencyPrecision"
                      size="small"
                      @change="updateWaveform"
                    />
                    <el-select v-model="frequencyUnit" size="small" style="width: 80px">
                      <el-option label="Hz" value="Hz" />
                      <el-option label="kHz" value="kHz" />
                      <el-option label="MHz" value="MHz" />
                    </el-select>
                  </div>
                </div>

                <div class="parameter-item">
                  <label>幅度</label>
                  <div class="parameter-input">
                    <el-input-number
                      v-model="amplitude"
                      :min="0.001"
                      :max="10"
                      :step="0.001"
                      :precision="3"
                      size="small"
                      @change="updateWaveform"
                    />
                    <span class="unit">Vpp</span>
                  </div>
                </div>

                <div class="parameter-item">
                  <label>直流偏置</label>
                  <div class="parameter-input">
                    <el-input-number
                      v-model="offset"
                      :min="-5"
                      :max="5"
                      :step="0.001"
                      :precision="3"
                      size="small"
                      @change="updateWaveform"
                    />
                    <span class="unit">V</span>
                  </div>
                </div>

                <div class="parameter-item">
                  <label>相位</label>
                  <div class="parameter-input">
                    <el-input-number
                      v-model="phase"
                      :min="0"
                      :max="360"
                      :step="1"
                      :precision="1"
                      size="small"
                      @change="updateWaveform"
                    />
                    <span class="unit">°</span>
                  </div>
                </div>

                <!-- 方波占空比 -->
                <div v-if="selectedWaveform === 'square'" class="parameter-item">
                  <label>占空比</label>
                  <div class="parameter-input">
                    <el-input-number
                      v-model="dutyCycle"
                      :min="1"
                      :max="99"
                      :step="1"
                      :precision="1"
                      size="small"
                      @change="updateWaveform"
                    />
                    <span class="unit">%</span>
                  </div>
                </div>

                <!-- 噪声水平 -->
                <div v-if="selectedWaveform === 'noise'" class="parameter-item">
                  <label>噪声水平</label>
                  <div class="parameter-input">
                    <el-input-number
                      v-model="noiseLevel"
                      :min="0"
                      :max="1"
                      :step="0.01"
                      :precision="2"
                      size="small"
                      @change="updateWaveform"
                    />
                    <span class="unit">V</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- 调制功能 -->
            <div class="panel-section">
              <h4 class="section-title">
                调制功能
                <el-switch 
                  v-model="modulation.enabled" 
                  size="small"
                  @change="updateWaveform"
                />
              </h4>
              <div v-if="modulation.enabled" class="modulation-controls">
                <div class="parameter-item">
                  <label>调制类型</label>
                  <el-select v-model="modulation.type" size="small" @change="updateWaveform">
                    <el-option label="幅度调制 (AM)" value="AM" />
                    <el-option label="频率调制 (FM)" value="FM" />
                    <el-option label="相位调制 (PM)" value="PM" />
                  </el-select>
                </div>
                <div class="parameter-item">
                  <label>调制频率</label>
                  <div class="parameter-input">
                    <el-input-number
                      v-model="modulation.frequency"
                      :min="0.1"
                      :max="1000"
                      :step="0.1"
                      :precision="1"
                      size="small"
                      @change="updateWaveform"
                    />
                    <span class="unit">Hz</span>
                  </div>
                </div>
                <div class="parameter-item">
                  <label>调制深度</label>
                  <div class="parameter-input">
                    <el-input-number
                      v-model="modulation.depth"
                      :min="0"
                      :max="100"
                      :step="1"
                      :precision="1"
                      size="small"
                      @change="updateWaveform"
                    />
                    <span class="unit">%</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- 扫频功能 -->
            <div class="panel-section">
              <h4 class="section-title">
                扫频功能
                <el-switch 
                  v-model="sweep.enabled" 
                  size="small"
                  @change="updateSweep"
                />
              </h4>
              <div v-if="sweep.enabled" class="sweep-controls">
                <div class="parameter-item">
                  <label>起始频率</label>
                  <div class="parameter-input">
                    <el-input-number
                      v-model="sweep.startFreq"
                      :min="0.001"
                      :max="100000"
                      :step="1"
                      :precision="3"
                      size="small"
                      @change="updateSweep"
                    />
                    <span class="unit">Hz</span>
                  </div>
                </div>
                <div class="parameter-item">
                  <label>结束频率</label>
                  <div class="parameter-input">
                    <el-input-number
                      v-model="sweep.stopFreq"
                      :min="0.001"
                      :max="100000"
                      :step="1"
                      :precision="3"
                      size="small"
                      @change="updateSweep"
                    />
                    <span class="unit">Hz</span>
                  </div>
                </div>
                <div class="parameter-item">
                  <label>扫频时间</label>
                  <div class="parameter-input">
                    <el-input-number
                      v-model="sweep.duration"
                      :min="0.1"
                      :max="60"
                      :step="0.1"
                      :precision="1"
                      size="small"
                      @change="updateSweep"
                    />
                    <span class="unit">s</span>
                  </div>
                </div>
                <div class="parameter-item">
                  <label>扫频类型</label>
                  <el-select v-model="sweep.type" size="small" @change="updateSweep">
                    <el-option label="线性扫频" value="linear" />
                    <el-option label="对数扫频" value="logarithmic" />
                  </el-select>
                </div>
              </div>
            </div>
          </div>
        </el-col>

        <!-- 右侧：波形预览和状态显示 -->
        <el-col :span="12">
          <div class="preview-panel">
            <div class="panel-section">
              <h4 class="section-title">波形预览</h4>
              <div class="waveform-preview">
                <canvas 
                  ref="previewCanvas" 
                  :width="canvasWidth" 
                  :height="canvasHeight"
                  class="preview-canvas"
                ></canvas>
              </div>
            </div>

            <div class="panel-section">
              <h4 class="section-title">输出状态</h4>
              <div class="status-grid">
                <div class="status-item">
                  <span class="status-label">输出状态:</span>
                  <span class="status-value" :class="outputEnabled ? 'enabled' : 'disabled'">
                    {{ outputEnabled ? '开启' : '关闭' }}
                  </span>
                </div>
                <div class="status-item">
                  <span class="status-label">当前频率:</span>
                  <span class="status-value">{{ formatFrequency(currentFrequency) }}</span>
                </div>
                <div class="status-item">
                  <span class="status-label">当前幅度:</span>
                  <span class="status-value">{{ amplitude.toFixed(3) }} Vpp</span>
                </div>
                <div class="status-item">
                  <span class="status-label">波形类型:</span>
                  <span class="status-value">{{ getWaveformName(selectedWaveform) }}</span>
                </div>
                <div v-if="modulation.enabled" class="status-item">
                  <span class="status-label">调制状态:</span>
                  <span class="status-value">{{ modulation.type }} - {{ modulation.depth }}%</span>
                </div>
                <div v-if="sweep.enabled" class="status-item">
                  <span class="status-label">扫频状态:</span>
                  <span class="status-value">{{ formatFrequency(sweep.startFreq) }} → {{ formatFrequency(sweep.stopFreq) }}</span>
                </div>
              </div>
            </div>

            <div class="panel-section">
              <h4 class="section-title">快速设置</h4>
              <div class="preset-buttons">
                <el-button size="small" @click="loadPreset('sine_1khz')">1kHz正弦波</el-button>
                <el-button size="small" @click="loadPreset('square_100hz')">100Hz方波</el-button>
                <el-button size="small" @click="loadPreset('triangle_500hz')">500Hz三角波</el-button>
                <el-button size="small" @click="loadPreset('noise_white')">白噪声</el-button>
              </div>
            </div>

            <div class="panel-section">
              <h4 class="section-title">控制操作</h4>
              <div class="control-buttons">
                <el-button type="primary" @click="applySettings">应用设置</el-button>
                <el-button @click="resetToDefault">重置默认</el-button>
                <el-button @click="savePreset">保存预设</el-button>
                <el-button @click="exportWaveform">导出波形</el-button>
              </div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed, nextTick } from 'vue'
import { Timer, Switch } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

// 波形类型定义
interface WaveformType {
  type: string
  name: string
  icon: string
}

// 调制配置
interface ModulationConfig {
  enabled: boolean
  type: 'AM' | 'FM' | 'PM'
  frequency: number
  depth: number
}

// 扫频配置
interface SweepConfig {
  enabled: boolean
  startFreq: number
  stopFreq: number
  duration: number
  type: 'linear' | 'logarithmic'
}

// Props
interface Props {
  width?: number
  height?: number
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  width: 800,
  height: 600,
  disabled: false
})

// Emits
const emit = defineEmits<{
  outputChange: [enabled: boolean]
  waveformChange: [config: any]
  frequencyChange: [frequency: number]
}>()

// 响应式数据
const outputEnabled = ref(false)
const selectedWaveform = ref('sine')

// 基本参数
const frequency = ref(1000)
const frequencyUnit = ref('Hz')
const amplitude = ref(1.0)
const offset = ref(0)
const phase = ref(0)
const dutyCycle = ref(50) // 方波占空比
const noiseLevel = ref(0.1) // 噪声水平

// 调制配置
const modulation = ref<ModulationConfig>({
  enabled: false,
  type: 'AM',
  frequency: 10,
  depth: 50
})

// 扫频配置
const sweep = ref<SweepConfig>({
  enabled: false,
  startFreq: 100,
  stopFreq: 10000,
  duration: 5,
  type: 'linear'
})

// 画布相关
const previewCanvas = ref<HTMLCanvasElement>()
const canvasWidth = 400
const canvasHeight = 200

// 动画和定时器
let animationFrame: number | null = null
let sweepTimer: number | null = null
const currentFrequency = ref(1000)

// 波形类型配置
const waveformTypes: WaveformType[] = [
  { type: 'sine', name: '正弦波', icon: '〜' },
  { type: 'square', name: '方波', icon: '⊓' },
  { type: 'triangle', name: '三角波', icon: '△' },
  { type: 'sawtooth', name: '锯齿波', icon: '⟋' },
  { type: 'pulse', name: '脉冲', icon: '|' },
  { type: 'noise', name: '噪声', icon: '※' }
]

// 频率范围配置
const frequencyRange = computed(() => {
  switch (frequencyUnit.value) {
    case 'kHz':
      return { min: 0.001, max: 100 }
    case 'MHz':
      return { min: 0.000001, max: 0.1 }
    default:
      return { min: 0.001, max: 100000 }
  }
})

const frequencyStep = computed(() => {
  switch (frequencyUnit.value) {
    case 'kHz':
      return 0.001
    case 'MHz':
      return 0.000001
    default:
      return 0.001
  }
})

const frequencyPrecision = computed(() => {
  switch (frequencyUnit.value) {
    case 'kHz':
      return 3
    case 'MHz':
      return 6
    default:
      return 3
  }
})

// 实际频率（转换为Hz）
const actualFrequency = computed(() => {
  switch (frequencyUnit.value) {
    case 'kHz':
      return frequency.value * 1000
    case 'MHz':
      return frequency.value * 1000000
    default:
      return frequency.value
  }
})

// 方法
const selectWaveform = (type: string) => {
  selectedWaveform.value = type
  updateWaveform()
}

const toggleOutput = () => {
  outputEnabled.value = !outputEnabled.value
  emit('outputChange', outputEnabled.value)
  
  if (outputEnabled.value) {
    ElMessage.success('信号输出已开启')
    startWaveformGeneration()
  } else {
    ElMessage.info('信号输出已关闭')
    stopWaveformGeneration()
  }
}

const updateWaveform = () => {
  currentFrequency.value = actualFrequency.value
  drawWaveformPreview()
  
  const config = {
    type: selectedWaveform.value,
    frequency: actualFrequency.value,
    amplitude: amplitude.value,
    offset: offset.value,
    phase: phase.value,
    dutyCycle: dutyCycle.value,
    noiseLevel: noiseLevel.value,
    modulation: modulation.value
  }
  
  emit('waveformChange', config)
  emit('frequencyChange', actualFrequency.value)
}

const updateSweep = () => {
  if (sweep.value.enabled) {
    startSweep()
  } else {
    stopSweep()
  }
}

const startSweep = () => {
  if (sweepTimer) {
    clearInterval(sweepTimer)
  }
  
  const startTime = Date.now()
  const duration = sweep.value.duration * 1000 // 转换为毫秒
  
  sweepTimer = setInterval(() => {
    const elapsed = Date.now() - startTime
    const progress = Math.min(elapsed / duration, 1)
    
    let newFreq: number
    if (sweep.value.type === 'linear') {
      newFreq = sweep.value.startFreq + (sweep.value.stopFreq - sweep.value.startFreq) * progress
    } else {
      // 对数扫频
      const logStart = Math.log10(sweep.value.startFreq)
      const logStop = Math.log10(sweep.value.stopFreq)
      newFreq = Math.pow(10, logStart + (logStop - logStart) * progress)
    }
    
    currentFrequency.value = newFreq
    
    if (progress >= 1) {
      // 扫频完成，重新开始
      clearInterval(sweepTimer!)
      setTimeout(() => startSweep(), 100)
    }
  }, 50)
}

const stopSweep = () => {
  if (sweepTimer) {
    clearInterval(sweepTimer)
    sweepTimer = null
  }
  currentFrequency.value = actualFrequency.value
}

const startWaveformGeneration = () => {
  if (animationFrame) {
    cancelAnimationFrame(animationFrame)
  }
  
  const animate = () => {
    drawWaveformPreview()
    if (outputEnabled.value) {
      animationFrame = requestAnimationFrame(animate)
    }
  }
  
  animate()
}

const stopWaveformGeneration = () => {
  if (animationFrame) {
    cancelAnimationFrame(animationFrame)
    animationFrame = null
  }
}

const drawWaveformPreview = () => {
  if (!previewCanvas.value) return
  
  const canvas = previewCanvas.value
  const ctx = canvas.getContext('2d')
  if (!ctx) return
  
  // 清空画布
  ctx.clearRect(0, 0, canvasWidth, canvasHeight)
  
  // 绘制网格
  drawGrid(ctx)
  
  // 绘制波形
  drawWaveform(ctx)
  
  // 绘制标签
  drawLabels(ctx)
}

const drawGrid = (ctx: CanvasRenderingContext2D) => {
  ctx.strokeStyle = '#e0e0e0'
  ctx.lineWidth = 1
  
  // 垂直网格线
  for (let x = 0; x <= canvasWidth; x += canvasWidth / 10) {
    ctx.beginPath()
    ctx.moveTo(x, 0)
    ctx.lineTo(x, canvasHeight)
    ctx.stroke()
  }
  
  // 水平网格线
  for (let y = 0; y <= canvasHeight; y += canvasHeight / 8) {
    ctx.beginPath()
    ctx.moveTo(0, y)
    ctx.lineTo(canvasWidth, y)
    ctx.stroke()
  }
  
  // 中心线
  ctx.strokeStyle = '#666'
  ctx.lineWidth = 2
  ctx.beginPath()
  ctx.moveTo(0, canvasHeight / 2)
  ctx.lineTo(canvasWidth, canvasHeight / 2)
  ctx.stroke()
}

const drawWaveform = (ctx: CanvasRenderingContext2D) => {
  const points = 1000
  const timeSpan = 4 / currentFrequency.value // 显示4个周期
  const dt = timeSpan / points
  
  ctx.strokeStyle = '#409eff'
  ctx.lineWidth = 2
  ctx.beginPath()
  
  for (let i = 0; i < points; i++) {
    const t = i * dt
    const x = (i / points) * canvasWidth
    let y = generateWaveformValue(t)
    
    // 应用调制
    if (modulation.value.enabled) {
      y = applyModulation(y, t)
    }
    
    // 转换到画布坐标
    const canvasY = canvasHeight / 2 - (y * canvasHeight / 4)
    
    if (i === 0) {
      ctx.moveTo(x, canvasY)
    } else {
      ctx.lineTo(x, canvasY)
    }
  }
  
  ctx.stroke()
}

const generateWaveformValue = (t: number): number => {
  const freq = currentFrequency.value
  const phaseRad = (phase.value * Math.PI) / 180
  const omega = 2 * Math.PI * freq
  
  let value = 0
  
  switch (selectedWaveform.value) {
    case 'sine':
      value = Math.sin(omega * t + phaseRad)
      break
    case 'square':
      const period = 1 / freq
      const phaseTime = (phaseRad / (2 * Math.PI)) * period
      const adjustedTime = (t + phaseTime) % period
      value = adjustedTime < (period * dutyCycle.value / 100) ? 1 : -1
      break
    case 'triangle':
      const trianglePeriod = 1 / freq
      const trianglePhaseTime = (phaseRad / (2 * Math.PI)) * trianglePeriod
      const triangleAdjustedTime = (t + trianglePhaseTime) % trianglePeriod
      const triangleProgress = triangleAdjustedTime / trianglePeriod
      value = triangleProgress < 0.5 ? 
        4 * triangleProgress - 1 : 
        3 - 4 * triangleProgress
      break
    case 'sawtooth':
      const sawPeriod = 1 / freq
      const sawPhaseTime = (phaseRad / (2 * Math.PI)) * sawPeriod
      const sawAdjustedTime = (t + sawPhaseTime) % sawPeriod
      value = 2 * (sawAdjustedTime / sawPeriod) - 1
      break
    case 'pulse':
      const pulsePeriod = 1 / freq
      const pulsePhaseTime = (phaseRad / (2 * Math.PI)) * pulsePeriod
      const pulseAdjustedTime = (t + pulsePhaseTime) % pulsePeriod
      value = pulseAdjustedTime < (pulsePeriod * 0.1) ? 1 : 0
      break
    case 'noise':
      value = (Math.random() - 0.5) * 2
      break
  }
  
  return value * amplitude.value + offset.value
}

const applyModulation = (baseValue: number, t: number): number => {
  const modFreq = modulation.value.frequency
  const modDepth = modulation.value.depth / 100
  const modOmega = 2 * Math.PI * modFreq
  
  switch (modulation.value.type) {
    case 'AM':
      return baseValue * (1 + modDepth * Math.sin(modOmega * t))
    case 'FM':
      // FM需要在频率上调制，这里简化处理
      return baseValue
    case 'PM':
      // PM需要在相位上调制，这里简化处理
      return baseValue
    default:
      return baseValue
  }
}

const drawLabels = (ctx: CanvasRenderingContext2D) => {
  ctx.fillStyle = '#666'
  ctx.font = '12px Arial'
  
  // 幅度标签
  ctx.fillText(`+${amplitude.value.toFixed(1)}V`, 5, 15)
  ctx.fillText('0V', 5, canvasHeight / 2 + 5)
  ctx.fillText(`-${amplitude.value.toFixed(1)}V`, 5, canvasHeight - 5)
  
  // 频率标签
  ctx.fillText(`${formatFrequency(currentFrequency.value)}`, canvasWidth - 80, 15)
}

const formatFrequency = (freq: number): string => {
  if (freq >= 1000000) {
    return `${(freq / 1000000).toFixed(3)} MHz`
  } else if (freq >= 1000) {
    return `${(freq / 1000).toFixed(3)} kHz`
  } else {
    return `${freq.toFixed(3)} Hz`
  }
}

const getWaveformName = (type: string): string => {
  const waveform = waveformTypes.find(w => w.type === type)
  return waveform ? waveform.name : type
}

const loadPreset = (presetName: string) => {
  switch (presetName) {
    case 'sine_1khz':
      selectedWaveform.value = 'sine'
      frequency.value = 1
      frequencyUnit.value = 'kHz'
      amplitude.value = 1.0
      offset.value = 0
      phase.value = 0
      break
    case 'square_100hz':
      selectedWaveform.value = 'square'
      frequency.value = 100
      frequencyUnit.value = 'Hz'
      amplitude.value = 2.0
      offset.value = 0
      dutyCycle.value = 50
      break
    case 'triangle_500hz':
      selectedWaveform.value = 'triangle'
      frequency.value = 500
      frequencyUnit.value = 'Hz'
      amplitude.value = 1.5
      offset.value = 0
      break
    case 'noise_white':
      selectedWaveform.value = 'noise'
      amplitude.value = 0.5
      noiseLevel.value = 1.0
      break
  }
  updateWaveform()
  ElMessage.success('预设加载成功')
}

const applySettings = () => {
  updateWaveform()
  ElMessage.success('设置已应用')
}

const resetToDefault = () => {
  selectedWaveform.value = 'sine'
  frequency.value = 1000
  frequencyUnit.value = 'Hz'
  amplitude.value = 1.0
  offset.value = 0
  phase.value = 0
  dutyCycle.value = 50
  noiseLevel.value = 0.1
  modulation.value.enabled = false
  sweep.value.enabled = false
  updateWaveform()
  ElMessage.success('已重置为默认设置')
}

const savePreset = () => {
  // 保存当前设置为预设
  const preset = {
    waveform: selectedWaveform.value,
    frequency: frequency.value,
    frequencyUnit: frequencyUnit.value,
    amplitude: amplitude.value,
    offset: offset.value,
    phase: phase.value,
    dutyCycle: dutyCycle.value,
    noiseLevel: noiseLevel.value,
    modulation: { ...modulation.value },
    sweep: { ...sweep.value }
  }
  
  // 这里可以保存到本地存储或发送到服务器
  localStorage.setItem('signalGeneratorPreset', JSON.stringify(preset))
  ElMessage.success('预设保存成功')
}

const exportWaveform = () => {
  // 导出当前波形数据
  const points = 1000
  const timeSpan = 4 / currentFrequency.value
  const dt = timeSpan / points
  const waveformData: number[] = []
  
  for (let i = 0; i < points; i++) {
    const t = i * dt
    let y = generateWaveformValue(t)
    
    if (modulation.value.enabled) {
      y = applyModulation(y, t)
    }
    
    waveformData.push(y)
  }
  
  // 创建CSV格式的数据
  let csvContent = 'Time(s),Amplitude(V)\n'
  for (let i = 0; i < points; i++) {
    const t = i * dt
    csvContent += `${t.toFixed(6)},${waveformData[i].toFixed(6)}\n`
  }
  
  // 下载文件
  const blob = new Blob([csvContent], { type: 'text/csv' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `waveform_${selectedWaveform.value}_${formatFrequency(currentFrequency.value)}.csv`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
  
  ElMessage.success('波形数据导出成功')
}

// 监听频率单位变化
watch(frequencyUnit, () => {
  updateWaveform()
})

// 生命周期
onMounted(() => {
  nextTick(() => {
    drawWaveformPreview()
  })
})

onUnmounted(() => {
  stopWaveformGeneration()
  stopSweep()
})

// 暴露方法
defineExpose({
  toggleOutput,
  updateWaveform,
  loadPreset,
  resetToDefault,
  savePreset,
  exportWaveform
})
</script>

<style lang="scss" scoped>
.signal-generator {
  background: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  
  .generator-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 24px;
    padding-bottom: 16px;
    border-bottom: 1px solid var(--border-color);
    
    .header-left {
      display: flex;
      flex-direction: column;
      gap: 8px;
      
      .generator-title {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 18px;
        font-weight: 600;
        color: var(--text-primary);
        margin: 0;
        
        .title-icon {
          font-size: 20px;
          color: var(--primary-color);
        }
      }
      
      .generator-status {
        .status-indicator {
          padding: 4px 12px;
          border-radius: 12px;
          font-size: 12px;
          font-weight: 500;
          
          &.active {
            background: rgba(103, 194, 58, 0.1);
            color: #67c23a;
          }
          
          &.inactive {
            background: rgba(144, 147, 153, 0.1);
            color: #909399;
          }
        }
      }
    }
    
    .header-right {
      .el-button {
        font-weight: 600;
      }
    }
  }
  
  .generator-body {
    .control-panel,
    .preview-panel {
      background: #fafafa;
      border: 1px solid #e4e7ed;
      border-radius: 8px;
      padding: 16px;
      height: 100%;
    }
    
    .panel-section {
      margin-bottom: 24px;
      
      &:last-child {
        margin-bottom: 0;
      }
      
      .section-title {
        font-size: 14px;
        font-weight: 600;
        color: var(--text-primary);
        margin-bottom: 12px;
        display: flex;
        align-items: center;
        justify-content: space-between;
      }
    }
    
    .waveform-selector {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 8px;
      
      .waveform-option {
        display: flex;
        flex-direction: column;
        align-items: center;
        padding: 12px 8px;
        border: 2px solid #e4e7ed;
        border-radius: 8px;
        cursor: pointer;
        transition: all 0.2s ease;
        
        &:hover {
          border-color: var(--primary-color);
          background: rgba(64, 158, 255, 0.05);
        }
        
        &.active {
          border-color: var(--primary-color);
          background: rgba(64, 158, 255, 0.1);
          
          .waveform-icon {
            color: var(--primary-color);
          }
          
          .waveform-name {
            color: var(--primary-color);
            font-weight: 600;
          }
        }
        
        .waveform-icon {
          font-size: 24px;
          font-weight: bold;
          margin-bottom: 4px;
          color: #666;
        }
        
        .waveform-name {
          font-size: 12px;
          color: var(--text-secondary);
        }
      }
    }
    
    .parameter-grid {
      display: grid;
      grid-template-columns: 1fr;
      gap: 12px;
      
      .parameter-item {
        display: flex;
        flex-direction: column;
        gap: 4px;
        
        label {
          font-size: 12px;
          font-weight: 500;
          color: var(--text-primary);
        }
        
        .parameter-input {
          display: flex;
          align-items: center;
          gap: 8px;
          
          .el-input-number {
            flex: 1;
          }
          
          .unit {
            font-size: 12px;
            color: var(--text-secondary);
            min-width: 30px;
          }
        }
      }
    }
    
    .modulation-controls,
    .sweep-controls {
      margin-top: 12px;
      padding: 12px;
      background: rgba(255, 255, 255, 0.5);
      border-radius: 6px;
      border: 1px solid #e4e7ed;
    }
    
    .waveform-preview {
      display: flex;
      justify-content: center;
      align-items: center;
      background: white;
      border: 1px solid #e4e7ed;
      border-radius: 6px;
      padding: 8px;
      
      .preview-canvas {
        border: 1px solid #ddd;
        border-radius: 4px;
      }
    }
    
    .status-grid {
      display: grid;
      grid-template-columns: 1fr;
      gap: 8px;
      
      .status-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 6px 0;
        
        .status-label {
          font-size: 12px;
          color: var(--text-secondary);
        }
        
        .status-value {
          font-size: 12px;
          font-family: 'Consolas', 'Monaco', monospace;
          font-weight: 600;
          
          &.enabled {
            color: #67c23a;
          }
          
          &.disabled {
            color: #909399;
          }
        }
      }
    }
    
    .preset-buttons,
    .control-buttons {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 8px;
      
      .el-button {
        font-size: 12px;
      }
    }
  }
}

@media (max-width: 768px) {
  .signal-generator {
    padding: 12px;
    
    .generator-header {
      flex-direction: column;
      gap: 12px;
      align-items: flex-start;
      
      .header-right {
        width: 100%;
        
        .el-button {
          width: 100%;
        }
      }
    }
    
    .waveform-selector {
      grid-template-columns: repeat(2, 1fr);
    }
    
    .preset-buttons,
    .control-buttons {
      grid-template-columns: 1fr;
    }
  }
}
</style>
