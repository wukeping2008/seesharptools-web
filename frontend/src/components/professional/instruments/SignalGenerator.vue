<template>
  <div class="signal-generator">
    <!-- 控件标题栏 -->
    <div class="generator-header">
      <div class="header-left">
        <h3 class="generator-title">
          <el-icon><Timer /></el-icon>
          任意波形信号发生器
        </h3>
        <div class="generator-model">SG-2000 Series</div>
      </div>
      <div class="header-right">
        <el-button 
          :type="config.output.enabled ? 'danger' : 'primary'"
          @click="toggleOutput"
          :loading="outputChanging"
        >
          <el-icon><Switch /></el-icon>
          {{ config.output.enabled ? '关闭输出' : '开启输出' }}
        </el-button>
        <el-dropdown @command="handleMenuCommand">
          <el-button circle>
            <el-icon><More /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="reset">重置配置</el-dropdown-item>
              <el-dropdown-item command="save">保存配置</el-dropdown-item>
              <el-dropdown-item command="load">加载配置</el-dropdown-item>
              <el-dropdown-item command="calibrate">校准设备</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>

    <!-- 主要控制区域 -->
    <div class="generator-content">
      <!-- 左侧控制面板 -->
      <div class="control-panel">
        <!-- 基本波形参数 -->
        <el-card class="control-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><DataLine /></el-icon>
              <span>基本波形参数</span>
            </div>
          </template>
          
          <div class="param-group">
            <div class="param-row">
              <label class="param-label">波形类型</label>
              <el-select 
                v-model="config.waveform.type" 
                @change="onWaveformTypeChange"
                class="param-control"
              >
                <el-option
                  v-for="type in waveformTypes"
                  :key="type.value"
                  :label="type.label"
                  :value="type.value"
                >
                  <span class="option-content">
                    <el-icon><component :is="type.icon" /></el-icon>
                    {{ type.label }}
                  </span>
                </el-option>
              </el-select>
            </div>

            <div class="param-row">
              <label class="param-label">频率</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.waveform.frequency"
                  :min="0.001"
                  :max="100000000"
                  :precision="3"
                  :step="getFrequencyStep()"
                  @change="onFrequencyChange"
                  class="param-number"
                />
                <el-select v-model="frequencyUnit" @change="onFrequencyUnitChange" class="unit-select">
                  <el-option label="Hz" value="Hz" />
                  <el-option label="kHz" value="kHz" />
                  <el-option label="MHz" value="MHz" />
                </el-select>
              </div>
            </div>

            <div class="param-row">
              <label class="param-label">幅度</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.waveform.amplitude"
                  :min="0.001"
                  :max="10.0"
                  :precision="3"
                  :step="0.001"
                  @change="onAmplitudeChange"
                  class="param-number"
                />
                <span class="unit-label">Vpp</span>
              </div>
            </div>

            <div class="param-row">
              <label class="param-label">直流偏置</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.waveform.offset"
                  :min="-5.0"
                  :max="5.0"
                  :precision="3"
                  :step="0.001"
                  @change="onOffsetChange"
                  class="param-number"
                />
                <span class="unit-label">V</span>
              </div>
            </div>

            <div class="param-row">
              <label class="param-label">相位</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.waveform.phase"
                  :min="0"
                  :max="360"
                  :precision="1"
                  :step="1"
                  @change="onPhaseChange"
                  class="param-number"
                />
                <span class="unit-label">°</span>
              </div>
            </div>

            <!-- 特殊波形参数 -->
            <div v-if="config.waveform.type === 'square' || config.waveform.type === 'pulse'" class="param-row">
              <label class="param-label">占空比</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.waveform.dutyCycle"
                  :min="0.1"
                  :max="99.9"
                  :precision="1"
                  :step="0.1"
                  class="param-number"
                />
                <span class="unit-label">%</span>
              </div>
            </div>

            <div v-if="config.waveform.type === 'triangle'" class="param-row">
              <label class="param-label">对称性</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.waveform.symmetry"
                  :min="0.1"
                  :max="99.9"
                  :precision="1"
                  :step="0.1"
                  class="param-number"
                />
                <span class="unit-label">%</span>
              </div>
            </div>
          </div>
        </el-card>

        <!-- 调制功能 -->
        <el-card class="control-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Connection /></el-icon>
              <span>调制功能</span>
              <el-switch 
                v-model="config.modulation.enabled"
                @change="onModulationEnabledChange"
                class="header-switch"
              />
            </div>
          </template>
          
          <div class="param-group" :class="{ disabled: !config.modulation.enabled }">
            <div class="param-row">
              <label class="param-label">调制类型</label>
              <el-select 
                v-model="config.modulation.type" 
                @change="onModulationTypeChange"
                :disabled="!config.modulation.enabled"
                class="param-control"
              >
                <el-option label="幅度调制 (AM)" value="am" />
                <el-option label="频率调制 (FM)" value="fm" />
                <el-option label="相位调制 (PM)" value="pm" />
                <el-option label="频移键控 (FSK)" value="fsk" />
                <el-option label="幅移键控 (ASK)" value="ask" />
                <el-option label="相移键控 (PSK)" value="psk" />
              </el-select>
            </div>

            <div class="param-row">
              <label class="param-label">调制频率</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.modulation.frequency"
                  :min="0.1"
                  :max="100000"
                  :precision="1"
                  :step="0.1"
                  :disabled="!config.modulation.enabled"
                  class="param-number"
                />
                <span class="unit-label">Hz</span>
              </div>
            </div>

            <div class="param-row">
              <label class="param-label">调制深度</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.modulation.depth"
                  :min="0"
                  :max="100"
                  :precision="1"
                  :step="1"
                  :disabled="!config.modulation.enabled"
                  class="param-number"
                />
                <span class="unit-label">%</span>
              </div>
            </div>

            <div v-if="config.modulation.type === 'fm'" class="param-row">
              <label class="param-label">频偏</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.modulation.deviation"
                  :min="1"
                  :max="1000000"
                  :precision="0"
                  :step="1"
                  :disabled="!config.modulation.enabled"
                  class="param-number"
                />
                <span class="unit-label">Hz</span>
              </div>
            </div>

            <div v-if="config.modulation.type === 'pm'" class="param-row">
              <label class="param-label">相偏</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.modulation.phaseDeviation"
                  :min="0"
                  :max="360"
                  :precision="1"
                  :step="1"
                  :disabled="!config.modulation.enabled"
                  class="param-number"
                />
                <span class="unit-label">°</span>
              </div>
            </div>
          </div>
        </el-card>

        <!-- 扫频功能 -->
        <el-card class="control-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><TrendCharts /></el-icon>
              <span>扫频功能</span>
              <el-switch 
                v-model="config.sweep.enabled"
                @change="onSweepEnabledChange"
                class="header-switch"
              />
            </div>
          </template>
          
          <div class="param-group" :class="{ disabled: !config.sweep.enabled }">
            <div class="param-row">
              <label class="param-label">扫频模式</label>
              <el-select 
                v-model="config.sweep.mode" 
                @change="onSweepModeChange"
                :disabled="!config.sweep.enabled"
                class="param-control"
              >
                <el-option label="线性扫频" value="linear" />
                <el-option label="对数扫频" value="log" />
                <el-option label="步进扫频" value="step" />
              </el-select>
            </div>

            <div class="param-row">
              <label class="param-label">起始频率</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.sweep.startFrequency"
                  :min="0.001"
                  :max="100000000"
                  :precision="3"
                  :step="1"
                  :disabled="!config.sweep.enabled"
                  class="param-number"
                />
                <span class="unit-label">Hz</span>
              </div>
            </div>

            <div class="param-row">
              <label class="param-label">终止频率</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.sweep.stopFrequency"
                  :min="0.001"
                  :max="100000000"
                  :precision="3"
                  :step="1"
                  :disabled="!config.sweep.enabled"
                  class="param-number"
                />
                <span class="unit-label">Hz</span>
              </div>
            </div>

            <div class="param-row">
              <label class="param-label">扫频时间</label>
              <div class="param-input-group">
                <el-input-number
                  v-model="config.sweep.sweepTime"
                  :min="0.001"
                  :max="3600"
                  :precision="3"
                  :step="0.001"
                  :disabled="!config.sweep.enabled"
                  class="param-number"
                />
                <span class="unit-label">s</span>
              </div>
            </div>
          </div>
        </el-card>
      </div>

      <!-- 右侧显示区域 -->
      <div class="display-panel">
        <!-- 波形预览 -->
        <el-card class="display-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><DataLine /></el-icon>
              <span>波形预览</span>
              <div class="header-actions">
                <el-button size="small" @click="refreshWaveform">
                  <el-icon><Refresh /></el-icon>
                  刷新
                </el-button>
              </div>
            </div>
          </template>
          
          <div class="waveform-container">
            <canvas 
              ref="waveformCanvas" 
              class="waveform-canvas"
              :width="canvasWidth"
              :height="canvasHeight"
            ></canvas>
          </div>
        </el-card>

        <!-- 状态监控 -->
        <el-card class="display-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Monitor /></el-icon>
              <span>状态监控</span>
            </div>
          </template>
          
          <div class="status-grid">
            <div class="status-item">
              <div class="status-label">输出状态</div>
              <div class="status-value" :class="{ active: status.outputEnabled }">
                <el-icon><Switch /></el-icon>
                {{ status.outputEnabled ? '开启' : '关闭' }}
              </div>
            </div>

            <div class="status-item">
              <div class="status-label">当前频率</div>
              <div class="status-value">{{ formatFrequency(status.frequency) }}</div>
            </div>

            <div class="status-item">
              <div class="status-label">当前幅度</div>
              <div class="status-value">{{ status.amplitude.toFixed(3) }} Vpp</div>
            </div>

            <div class="status-item">
              <div class="status-label">波形类型</div>
              <div class="status-value">{{ getWaveformTypeName(status.waveformType) }}</div>
            </div>

            <div class="status-item">
              <div class="status-label">设备温度</div>
              <div class="status-value" :class="{ warning: status.temperature > 60 }">
                {{ status.temperature.toFixed(1) }}°C
              </div>
            </div>

            <div class="status-item">
              <div class="status-label">系统状态</div>
              <div class="status-value" :class="{ error: status.errorCode !== 0 }">
                {{ status.errorCode === 0 ? '正常' : status.errorMessage }}
              </div>
            </div>
          </div>
        </el-card>

        <!-- 快速设置 -->
        <el-card class="display-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Setting /></el-icon>
              <span>快速设置</span>
            </div>
          </template>
          
          <div class="quick-settings">
            <div class="setting-group">
              <label class="setting-label">标准频率</label>
              <div class="frequency-buttons">
                <el-button 
                  v-for="freq in standardFrequencies" 
                  :key="freq"
                  size="small"
                  @click="setFrequency(freq)"
                >
                  {{ formatFrequency(freq) }}
                </el-button>
              </div>
            </div>

            <div class="setting-group">
              <label class="setting-label">标准幅度</label>
              <div class="amplitude-buttons">
                <el-button 
                  v-for="amp in standardAmplitudes" 
                  :key="amp"
                  size="small"
                  @click="setAmplitude(amp)"
                >
                  {{ amp }} Vpp
                </el-button>
              </div>
            </div>

            <div class="setting-group">
              <label class="setting-label">输出配置</label>
              <div class="output-controls">
                <el-checkbox 
                  v-model="config.output.invert"
                  @change="onOutputConfigChange"
                >
                  输出反相
                </el-checkbox>
                <el-checkbox 
                  v-model="config.output.sync"
                  @change="onOutputConfigChange"
                >
                  同步输出
                </el-checkbox>
                <el-checkbox 
                  v-model="config.output.protection"
                  @change="onOutputConfigChange"
                >
                  过载保护
                </el-checkbox>
              </div>
            </div>
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { 
  Timer, Switch, More, DataLine, Connection, TrendCharts, 
  Monitor, Setting, Refresh 
} from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type {
  SignalGeneratorConfig,
  SignalGeneratorStatus,
  WaveformType,
  ModulationType,
  SweepMode
} from '@/types/signal-generator'
import { 
  DEFAULT_SIGNAL_GENERATOR_CONFIG,
  STANDARD_FREQUENCIES,
  STANDARD_AMPLITUDES
} from '@/types/signal-generator'

// Props
interface Props {
  width?: number
  height?: number
  config?: Partial<SignalGeneratorConfig>
}

const props = withDefaults(defineProps<Props>(), {
  width: 800,
  height: 600,
  config: () => ({})
})

// Emits
const emit = defineEmits<{
  'config-change': [config: SignalGeneratorConfig]
  'output-change': [enabled: boolean]
  'waveform-change': [type: WaveformType, frequency: number, amplitude: number]
  'error': [error: string]
}>()

// 响应式数据
const config = reactive<SignalGeneratorConfig>({
  ...DEFAULT_SIGNAL_GENERATOR_CONFIG,
  ...props.config
})

const status = reactive<SignalGeneratorStatus>({
  outputEnabled: false,
  frequency: 1000,
  amplitude: 1.0,
  waveformType: 'sine' as WaveformType,
  modulationEnabled: false,
  sweepEnabled: false,
  burstEnabled: false,
  temperature: 25.0,
  errorCode: 0,
  errorMessage: ''
})

// 控制状态
const outputChanging = ref(false)
const frequencyUnit = ref('Hz')

// Canvas相关
const waveformCanvas = ref<HTMLCanvasElement>()
const canvasWidth = 400
const canvasHeight = 200

// 定时器
let statusTimer: number | null = null
let waveformTimer: number | null = null

// 波形类型选项
const waveformTypes = [
  { value: 'sine', label: '正弦波', icon: 'DataLine' },
  { value: 'square', label: '方波', icon: 'Grid' },
  { value: 'triangle', label: '三角波', icon: 'TrendCharts' },
  { value: 'sawtooth', label: '锯齿波', icon: 'DataLine' },
  { value: 'pulse', label: '脉冲波', icon: 'Timer' },
  { value: 'noise', label: '噪声', icon: 'Connection' },
  { value: 'dc', label: '直流', icon: 'Minus' }
]

// 标准频率和幅度
const standardFrequencies = STANDARD_FREQUENCIES
const standardAmplitudes = STANDARD_AMPLITUDES

// 计算属性
const displayFrequency = computed(() => {
  const freq = config.waveform.frequency
  switch (frequencyUnit.value) {
    case 'kHz': return freq / 1000
    case 'MHz': return freq / 1000000
    default: return freq
  }
})

// 方法
const toggleOutput = async () => {
  outputChanging.value = true
  try {
    await new Promise(resolve => setTimeout(resolve, 500)) // 模拟异步操作
    config.output.enabled = !config.output.enabled
    status.outputEnabled = config.output.enabled
    
    emit('output-change', config.output.enabled)
    emit('config-change', config)
    
    ElMessage.success(config.output.enabled ? '输出已开启' : '输出已关闭')
  } catch (error) {
    ElMessage.error('输出状态切换失败')
    emit('error', '输出状态切换失败')
  } finally {
    outputChanging.value = false
  }
}

const onWaveformTypeChange = () => {
  status.waveformType = config.waveform.type
  updateWaveformPreview()
  emit('waveform-change', config.waveform.type, config.waveform.frequency, config.waveform.amplitude)
  emit('config-change', config)
}

const onFrequencyChange = () => {
  status.frequency = config.waveform.frequency
  updateWaveformPreview()
  emit('waveform-change', config.waveform.type, config.waveform.frequency, config.waveform.amplitude)
  emit('config-change', config)
}

const onAmplitudeChange = () => {
  status.amplitude = config.waveform.amplitude
  updateWaveformPreview()
  emit('waveform-change', config.waveform.type, config.waveform.frequency, config.waveform.amplitude)
  emit('config-change', config)
}

const onOffsetChange = () => {
  updateWaveformPreview()
  emit('config-change', config)
}

const onPhaseChange = () => {
  updateWaveformPreview()
  emit('config-change', config)
}

const onModulationEnabledChange = () => {
  status.modulationEnabled = config.modulation.enabled
  updateWaveformPreview()
  emit('config-change', config)
}

const onModulationTypeChange = () => {
  updateWaveformPreview()
  emit('config-change', config)
}

const onSweepEnabledChange = () => {
  status.sweepEnabled = config.sweep.enabled
  emit('config-change', config)
}

const onSweepModeChange = () => {
  emit('config-change', config)
}

const onOutputConfigChange = () => {
  emit('config-change', config)
}

const onFrequencyUnitChange = () => {
  const currentFreq = config.waveform.frequency
  switch (frequencyUnit.value) {
    case 'kHz':
      config.waveform.frequency = currentFreq * 1000
      break
    case 'MHz':
      config.waveform.frequency = currentFreq * 1000000
      break
    default:
      // Hz - 不需要转换
      break
  }
  onFrequencyChange()
}

const getFrequencyStep = () => {
  switch (frequencyUnit.value) {
    case 'kHz': return 0.001
    case 'MHz': return 0.000001
    default: return 1
  }
}

const setFrequency = (frequency: number) => {
  config.waveform.frequency = frequency
  onFrequencyChange()
}

const setAmplitude = (amplitude: number) => {
  config.waveform.amplitude = amplitude
  onAmplitudeChange()
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

const getWaveformTypeName = (type: WaveformType): string => {
  const typeMap: Record<WaveformType, string> = {
    sine: '正弦波',
    square: '方波',
    triangle: '三角波',
    sawtooth: '锯齿波',
    pulse: '脉冲波',
    noise: '噪声',
    dc: '直流',
    arbitrary: '任意波形'
  }
  return typeMap[type] || '未知'
}

const refreshWaveform = () => {
  updateWaveformPreview()
  ElMessage.success('波形预览已刷新')
}

const handleMenuCommand = async (command: string) => {
  switch (command) {
    case 'reset':
      await ElMessageBox.confirm('确定要重置所有配置吗？', '确认重置', {
        type: 'warning'
      })
      Object.assign(config, DEFAULT_SIGNAL_GENERATOR_CONFIG)
      updateWaveformPreview()
      ElMessage.success('配置已重置')
      break
    case 'save':
      ElMessage.success('配置已保存')
      break
    case 'load':
      ElMessage.success('配置已加载')
      break
    case 'calibrate':
      ElMessage.info('设备校准功能开发中...')
      break
  }
}

// 波形预览绘制
const updateWaveformPreview = () => {
  if (!waveformCanvas.value) return
  
  const canvas = waveformCanvas.value
  const ctx = canvas.getContext('2d')
  if (!ctx) return
  
  // 清空画布
  ctx.clearRect(0, 0, canvasWidth, canvasHeight)
  
  // 设置样式
  ctx.strokeStyle = '#2E86AB'
  ctx.lineWidth = 2
  ctx.fillStyle = '#f0f9ff'
  
  // 绘制背景网格
  drawGrid(ctx)
  
  // 绘制波形
  drawWaveform(ctx)
  
  // 绘制标签
  drawLabels(ctx)
}

const drawGrid = (ctx: CanvasRenderingContext2D) => {
  ctx.strokeStyle = '#e5e7eb'
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
  ctx.strokeStyle = '#9ca3af'
  ctx.lineWidth = 1
  ctx.beginPath()
  ctx.moveTo(0, canvasHeight / 2)
  ctx.lineTo(canvasWidth, canvasHeight / 2)
  ctx.stroke()
}

const drawWaveform = (ctx: CanvasRenderingContext2D) => {
  const { type, frequency, amplitude, offset, phase, dutyCycle, symmetry } = config.waveform
  const { enabled: modEnabled, type: modType, frequency: modFreq, depth: modDepth } = config.modulation
  
  ctx.strokeStyle = '#2E86AB'
  ctx.lineWidth = 2
  ctx.beginPath()
  
  const points = 1000
  const cycles = 2 // 显示2个周期
  
  for (let i = 0; i <= points; i++) {
    const t = (i / points) * cycles * 2 * Math.PI
    const x = (i / points) * canvasWidth
    
    let y = 0
    
    // 生成基础波形
    switch (type) {
      case 'sine':
        y = Math.sin(t + phase * Math.PI / 180)
        break
      case 'square':
        y = Math.sin(t + phase * Math.PI / 180) >= 0 ? 1 : -1
        break
      case 'triangle':
        const trianglePhase = (t + phase * Math.PI / 180) % (2 * Math.PI)
        if (trianglePhase < Math.PI) {
          y = (2 / Math.PI) * trianglePhase - 1
        } else {
          y = 3 - (2 / Math.PI) * trianglePhase
        }
        break
      case 'sawtooth':
        const sawPhase = (t + phase * Math.PI / 180) % (2 * Math.PI)
        y = (sawPhase / Math.PI) - 1
        break
      case 'pulse':
        const pulsePhase = (t + phase * Math.PI / 180) % (2 * Math.PI)
        y = pulsePhase < (2 * Math.PI * dutyCycle / 100) ? 1 : -1
        break
      case 'noise':
        y = (Math.random() - 0.5) * 2
        break
      case 'dc':
        y = 0
        break
    }
    
    // 应用调制
    if (modEnabled && modType !== 'none') {
      const modTime = (i / points) * cycles * 2 * Math.PI * (modFreq / frequency)
      let modulation = 1
      
      switch (modType) {
        case 'am':
          modulation = 1 + (modDepth / 100) * Math.sin(modTime)
          y *= modulation
          break
        case 'fm':
          // 频率调制通过相位调制实现
          const phaseModulation = (config.modulation.deviation / frequency) * Math.sin(modTime)
          y = Math.sin(t + phase * Math.PI / 180 + phaseModulation)
          break
        case 'pm':
          const phaseOffset = (config.modulation.phaseDeviation * Math.PI / 180) * Math.sin(modTime)
          y = Math.sin(t + phase * Math.PI / 180 + phaseOffset)
          break
      }
    }
    
    // 应用幅度和偏置
    y = y * amplitude + offset
    
    // 转换为画布坐标
    const canvasY = canvasHeight / 2 - (y / (amplitude + Math.abs(offset) + 1)) * (canvasHeight / 2 - 20)
    
    if (i === 0) {
      ctx.moveTo(x, canvasY)
    } else {
      ctx.lineTo(x, canvasY)
    }
  }
  
  ctx.stroke()
}

const drawLabels = (ctx: CanvasRenderingContext2D) => {
  ctx.fillStyle = '#374151'
  ctx.font = '12px Arial'
  
  // 频率标签
  ctx.fillText(`${formatFrequency(config.waveform.frequency)}`, 10, 20)
  
  // 幅度标签
  ctx.fillText(`${config.waveform.amplitude.toFixed(3)} Vpp`, 10, canvasHeight - 10)
  
  // 波形类型标签
  ctx.fillText(getWaveformTypeName(config.waveform.type), canvasWidth - 100, 20)
}

// 状态更新
const updateStatus = () => {
  // 模拟设备状态更新
  status.temperature = 25 + Math.random() * 10
  
  // 同步配置到状态
  status.frequency = config.waveform.frequency
  status.amplitude = config.waveform.amplitude
  status.waveformType = config.waveform.type
  status.modulationEnabled = config.modulation.enabled
  status.sweepEnabled = config.sweep.enabled
  status.outputEnabled = config.output.enabled
}

// 生命周期
onMounted(() => {
  nextTick(() => {
    updateWaveformPreview()
    
    // 启动状态更新定时器
    statusTimer = setInterval(updateStatus, 1000)
    
    // 启动波形更新定时器（仅噪声需要）
    waveformTimer = setInterval(() => {
      if (config.waveform.type === 'noise') {
        updateWaveformPreview()
      }
    }, 100)
  })
})

onUnmounted(() => {
  if (statusTimer) {
    clearInterval(statusTimer)
    statusTimer = null
  }
  if (waveformTimer) {
    clearInterval(waveformTimer)
    waveformTimer = null
  }
})

// 监听配置变化
watch(() => config.waveform.type, () => {
  updateWaveformPreview()
}, { deep: true })

watch(() => config.modulation.enabled, () => {
  updateWaveformPreview()
})
</script>

<style lang="scss" scoped>
.signal-generator {
  width: 100%;
  height: 100%;
  background: var(--instrument-bg);
  border: 1px solid var(--instrument-border);
  border-radius: 8px;
  overflow: hidden;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.generator-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  background: linear-gradient(135deg, #2E86AB 0%, #A23B72 100%);
  color: white;
  
  .header-left {
    .generator-title {
      display: flex;
      align-items: center;
      gap: 8px;
      margin: 0;
      font-size: 18px;
      font-weight: 600;
    }
    
    .generator-model {
      font-size: 12px;
      opacity: 0.8;
      margin-top: 4px;
    }
  }
  
  .header-right {
    display: flex;
    gap: 12px;
    align-items: center;
  }
}

.generator-content {
  display: grid;
  grid-template-columns: 400px 1fr;
  gap: 16px;
  padding: 16px;
  height: calc(100% - 80px);
  overflow: hidden;
}

.control-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
  overflow-y: auto;
  
  .control-card {
    .card-header {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 600;
      
      .header-switch {
        margin-left: auto;
      }
    }
  }
  
  .param-group {
    display: flex;
    flex-direction: column;
    gap: 12px;
    
    &.disabled {
      opacity: 0.5;
      pointer-events: none;
    }
  }
  
  .param-row {
    display: flex;
    align-items: center;
    gap: 12px;
    
    .param-label {
      min-width: 80px;
      font-size: 13px;
      font-weight: 500;
      color: var(--text-secondary);
    }
    
    .param-control {
      flex: 1;
    }
    
    .param-input-group {
      display: flex;
      align-items: center;
      gap: 8px;
      flex: 1;
      
      .param-number {
        flex: 1;
      }
      
      .unit-select {
        width: 80px;
      }
      
      .unit-label {
        font-size: 12px;
        color: var(--text-secondary);
        min-width: 30px;
      }
    }
  }
  
  .option-content {
    display: flex;
    align-items: center;
    gap: 8px;
  }
}

.display-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
  overflow-y: auto;
  
  .display-card {
    .card-header {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 600;
      
      .header-actions {
        margin-left: auto;
      }
    }
  }
}

.waveform-container {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 16px;
  background: #f8fafc;
  border-radius: 8px;
  
  .waveform-canvas {
    border: 1px solid #e2e8f0;
    border-radius: 4px;
    background: white;
  }
}

.status-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 16px;
  
  .status-item {
    .status-label {
      font-size: 12px;
      color: var(--text-secondary);
      margin-bottom: 4px;
    }
    
    .status-value {
      font-size: 14px;
      font-weight: 600;
      color: var(--text-primary);
      display: flex;
      align-items: center;
      gap: 4px;
      
      &.active {
        color: #10b981;
      }
      
      &.warning {
        color: #f59e0b;
      }
      
      &.error {
        color: #ef4444;
      }
    }
  }
}

.quick-settings {
  display: flex;
  flex-direction: column;
  gap: 16px;
  
  .setting-group {
    .setting-label {
      display: block;
      font-size: 13px;
      font-weight: 500;
      color: var(--text-secondary);
      margin-bottom: 8px;
    }
    
    .frequency-buttons,
    .amplitude-buttons {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }
    
    .output-controls {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
  }
}

// 响应式设计
@media (max-width: 1200px) {
  .generator-content {
    grid-template-columns: 1fr;
    grid-template-rows: auto 1fr;
  }
  
  .control-panel {
    max-height: 400px;
  }
}

@media (max-width: 768px) {
  .generator-header {
    flex-direction: column;
    gap: 12px;
    text-align: center;
    
    .header-right {
      width: 100%;
      justify-content: center;
    }
  }
  
  .generator-content {
    padding: 12px;
    gap: 12px;
  }
  
  .param-row {
    flex-direction: column;
    align-items: stretch;
    gap: 8px;
    
    .param-label {
      min-width: auto;
    }
  }
  
  .status-grid {
    grid-template-columns: 1fr;
  }
  
  .frequency-buttons,
  .amplitude-buttons {
    justify-content: center;
  }
}

// Element Plus 样式覆盖
:deep(.el-card__header) {
  padding: 12px 16px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
}

:deep(.el-card__body) {
  padding: 16px;
}

:deep(.el-input-number) {
  width: 100%;
  
  .el-input__inner {
    text-align: left;
  }
}

:deep(.el-select) {
  width: 100%;
}

:deep(.el-button--small) {
  padding: 4px 8px;
  font-size: 12px;
}

:deep(.el-checkbox) {
  margin-right: 0;
  
  .el-checkbox__label {
    font-size: 13px;
  }
}
</style>
