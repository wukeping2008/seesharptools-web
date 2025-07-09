<template>
  <div class="signal-generator-test">
    <!-- 页面标题 -->
    <div class="page-header">
      <div class="header-content">
        <div class="title-section">
          <h1 class="page-title">
            <el-icon><Timer /></el-icon>
            任意波形信号发生器测试
          </h1>
          <p class="page-description">
            专业任意波形信号发生器控件演示，支持多种标准波形、调制功能、扫频等高级特性
          </p>
        </div>
        <div class="header-actions">
          <el-button @click="showHelp">
            <el-icon><QuestionFilled /></el-icon>
            使用说明
          </el-button>
          <el-button @click="exportConfig">
            <el-icon><Download /></el-icon>
            导出配置
          </el-button>
        </div>
      </div>
    </div>

    <!-- 主要内容区域 -->
    <div class="test-content">
      <!-- 信号发生器控件 -->
      <div class="generator-section">
        <SignalGenerator
          :width="1000"
          :height="700"
          :config="generatorConfig"
          @config-change="onConfigChange"
          @output-change="onOutputChange"
          @waveform-change="onWaveformChange"
          @error="onError"
        />
      </div>

      <!-- 测试控制面板 -->
      <div class="test-panel">
        <el-card class="test-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Setting /></el-icon>
              <span>测试控制</span>
            </div>
          </template>
          
          <div class="test-controls">
            <!-- 预设配置 -->
            <div class="control-group">
              <label class="group-label">预设配置</label>
              <div class="preset-buttons">
                <el-button 
                  v-for="preset in presetConfigs" 
                  :key="preset.name"
                  @click="loadPreset(preset)"
                  size="small"
                >
                  {{ preset.name }}
                </el-button>
              </div>
            </div>

            <!-- 测试场景 -->
            <div class="control-group">
              <label class="group-label">测试场景</label>
              <div class="scenario-buttons">
                <el-button @click="runBasicTest" size="small" type="primary">
                  基础功能测试
                </el-button>
                <el-button @click="runModulationTest" size="small" type="success">
                  调制功能测试
                </el-button>
                <el-button @click="runSweepTest" size="small" type="warning">
                  扫频功能测试
                </el-button>
                <el-button @click="runPerformanceTest" size="small" type="info">
                  性能压力测试
                </el-button>
              </div>
            </div>

            <!-- 实时监控 -->
            <div class="control-group">
              <label class="group-label">实时监控</label>
              <div class="monitor-grid">
                <div class="monitor-item">
                  <div class="monitor-label">输出状态</div>
                  <div class="monitor-value" :class="{ active: outputEnabled }">
                    {{ outputEnabled ? '开启' : '关闭' }}
                  </div>
                </div>
                <div class="monitor-item">
                  <div class="monitor-label">当前频率</div>
                  <div class="monitor-value">{{ formatFrequency(currentFrequency) }}</div>
                </div>
                <div class="monitor-item">
                  <div class="monitor-label">当前幅度</div>
                  <div class="monitor-value">{{ currentAmplitude.toFixed(3) }} Vpp</div>
                </div>
                <div class="monitor-item">
                  <div class="monitor-label">波形类型</div>
                  <div class="monitor-value">{{ currentWaveformType }}</div>
                </div>
              </div>
            </div>

            <!-- 测试日志 -->
            <div class="control-group">
              <label class="group-label">测试日志</label>
              <div class="log-container">
                <div 
                  v-for="(log, index) in testLogs" 
                  :key="index"
                  class="log-item"
                  :class="log.type"
                >
                  <span class="log-time">{{ formatTime(log.timestamp) }}</span>
                  <span class="log-message">{{ log.message }}</span>
                </div>
              </div>
              <div class="log-actions">
                <el-button @click="clearLogs" size="small">清空日志</el-button>
                <el-button @click="exportLogs" size="small">导出日志</el-button>
              </div>
            </div>
          </div>
        </el-card>
      </div>
    </div>

    <!-- 技术说明对话框 -->
    <el-dialog
      v-model="helpDialogVisible"
      title="信号发生器使用说明"
      width="800px"
      :before-close="closeHelp"
    >
      <div class="help-content">
        <h3>功能特性</h3>
        <ul>
          <li><strong>多种波形类型</strong>：支持正弦波、方波、三角波、锯齿波、脉冲波、噪声、直流等</li>
          <li><strong>宽频率范围</strong>：1mHz - 100MHz，高精度频率控制</li>
          <li><strong>精确幅度控制</strong>：1mVpp - 10Vpp，支持直流偏置</li>
          <li><strong>调制功能</strong>：AM、FM、PM、FSK、ASK、PSK等多种调制方式</li>
          <li><strong>扫频功能</strong>：线性、对数、步进扫频模式</li>
          <li><strong>实时波形预览</strong>：Canvas绘制的高质量波形显示</li>
        </ul>

        <h3>操作指南</h3>
        <ol>
          <li><strong>基本设置</strong>：选择波形类型，设置频率、幅度、相位等参数</li>
          <li><strong>调制设置</strong>：启用调制功能，选择调制类型和参数</li>
          <li><strong>扫频设置</strong>：配置扫频范围和时间</li>
          <li><strong>输出控制</strong>：点击输出按钮开启/关闭信号输出</li>
          <li><strong>快速设置</strong>：使用预设频率和幅度按钮快速配置</li>
        </ol>

        <h3>技术规格</h3>
        <table class="spec-table">
          <tr><td>频率范围</td><td>1mHz - 100MHz</td></tr>
          <tr><td>频率精度</td><td>±1ppm</td></tr>
          <tr><td>幅度范围</td><td>1mVpp - 10Vpp</td></tr>
          <tr><td>幅度精度</td><td>±1%</td></tr>
          <tr><td>相位范围</td><td>0° - 360°</td></tr>
          <tr><td>相位精度</td><td>±0.1°</td></tr>
          <tr><td>输出阻抗</td><td>50Ω / 高阻</td></tr>
          <tr><td>谐波失真</td><td>&lt;-60dBc</td></tr>
        </table>

        <h3>应用场景</h3>
        <ul>
          <li><strong>电路测试</strong>：为电路提供标准测试信号</li>
          <li><strong>系统调试</strong>：模拟各种信号条件</li>
          <li><strong>频响测试</strong>：使用扫频功能测试频率响应</li>
          <li><strong>调制解调测试</strong>：验证通信系统性能</li>
          <li><strong>教学演示</strong>：信号处理和通信原理教学</li>
        </ul>
      </div>
      <template #footer>
        <el-button @click="closeHelp">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted } from 'vue'
import { Timer, QuestionFilled, Download, Setting } from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import SignalGenerator from '@/components/professional/instruments/SignalGenerator.vue'
import type { 
  SignalGeneratorConfig, 
  WaveformType,
  ModulationType,
  SweepMode
} from '@/types/signal-generator'
import { 
  DEFAULT_SIGNAL_GENERATOR_CONFIG,
  WaveformType as WaveformTypeEnum,
  ModulationType as ModulationTypeEnum,
  SweepMode as SweepModeEnum
} from '@/types/signal-generator'

// 响应式数据
const generatorConfig = reactive<SignalGeneratorConfig>({
  ...DEFAULT_SIGNAL_GENERATOR_CONFIG
})

const outputEnabled = ref(false)
const currentFrequency = ref(1000)
const currentAmplitude = ref(1.0)
const currentWaveformType = ref('正弦波')

const helpDialogVisible = ref(false)

// 测试日志
interface TestLog {
  timestamp: Date
  type: 'info' | 'success' | 'warning' | 'error'
  message: string
}

const testLogs = ref<TestLog[]>([])

// 预设配置
const presetConfigs = [
  {
    name: '1kHz正弦波',
    config: {
      waveform: {
        type: 'sine' as WaveformType,
        frequency: 1000,
        amplitude: 1.0,
        offset: 0,
        phase: 0,
        dutyCycle: 50,
        symmetry: 50,
        riseTime: 10,
        fallTime: 10
      }
    }
  },
  {
    name: '10kHz方波',
    config: {
      waveform: {
        type: 'square' as WaveformType,
        frequency: 10000,
        amplitude: 2.0,
        offset: 0,
        phase: 0,
        dutyCycle: 50,
        symmetry: 50,
        riseTime: 10,
        fallTime: 10
      }
    }
  },
  {
    name: '100Hz三角波',
    config: {
      waveform: {
        type: 'triangle' as WaveformType,
        frequency: 100,
        amplitude: 3.0,
        offset: 0,
        phase: 0,
        dutyCycle: 50,
        symmetry: 50,
        riseTime: 10,
        fallTime: 10
      }
    }
  },
  {
    name: 'AM调制信号',
    config: {
      waveform: {
        type: 'sine' as WaveformType,
        frequency: 10000,
        amplitude: 2.0,
        offset: 0,
        phase: 0,
        dutyCycle: 50,
        symmetry: 50,
        riseTime: 10,
        fallTime: 10
      },
      modulation: {
        type: 'am',
        enabled: true,
        frequency: 1000,
        depth: 50,
        deviation: 1000,
        phaseDeviation: 90,
        source: 'internal'
      }
    }
  }
]

// 定时器
let testTimer: number | null = null

// 方法
const onConfigChange = (config: SignalGeneratorConfig) => {
  Object.assign(generatorConfig, config)
  addLog('info', '配置已更新')
}

const onOutputChange = (enabled: boolean) => {
  outputEnabled.value = enabled
  addLog(enabled ? 'success' : 'info', `输出${enabled ? '开启' : '关闭'}`)
}

const onWaveformChange = (type: WaveformType, frequency: number, amplitude: number) => {
  currentFrequency.value = frequency
  currentAmplitude.value = amplitude
  currentWaveformType.value = getWaveformTypeName(type)
  addLog('info', `波形参数更新: ${getWaveformTypeName(type)}, ${formatFrequency(frequency)}, ${amplitude.toFixed(3)}Vpp`)
}

const onError = (error: string) => {
  addLog('error', `错误: ${error}`)
  ElMessage.error(error)
}

const loadPreset = (preset: any) => {
  Object.assign(generatorConfig, DEFAULT_SIGNAL_GENERATOR_CONFIG, preset.config)
  addLog('success', `已加载预设: ${preset.name}`)
  ElMessage.success(`已加载预设: ${preset.name}`)
}

const runBasicTest = async () => {
  addLog('info', '开始基础功能测试...')
  
  // 测试不同波形类型
  const waveforms: WaveformType[] = [WaveformTypeEnum.SINE, WaveformTypeEnum.SQUARE, WaveformTypeEnum.TRIANGLE, WaveformTypeEnum.SAWTOOTH]
  
  for (const waveform of waveforms) {
    generatorConfig.waveform.type = waveform
    await new Promise(resolve => setTimeout(resolve, 1000))
    addLog('info', `测试波形: ${getWaveformTypeName(waveform)}`)
  }
  
  addLog('success', '基础功能测试完成')
  ElMessage.success('基础功能测试完成')
}

const runModulationTest = async () => {
  addLog('info', '开始调制功能测试...')
  
  // 启用AM调制
  generatorConfig.modulation.enabled = true
  generatorConfig.modulation.type = ModulationTypeEnum.AM
  generatorConfig.modulation.frequency = 1000
  generatorConfig.modulation.depth = 50
  
  await new Promise(resolve => setTimeout(resolve, 2000))
  addLog('info', '测试AM调制')
  
  // 切换到FM调制
  generatorConfig.modulation.type = ModulationTypeEnum.FM
  generatorConfig.modulation.deviation = 5000
  
  await new Promise(resolve => setTimeout(resolve, 2000))
  addLog('info', '测试FM调制')
  
  // 关闭调制
  generatorConfig.modulation.enabled = false
  
  addLog('success', '调制功能测试完成')
  ElMessage.success('调制功能测试完成')
}

const runSweepTest = async () => {
  addLog('info', '开始扫频功能测试...')
  
  // 配置线性扫频
  generatorConfig.sweep.enabled = true
  generatorConfig.sweep.mode = SweepModeEnum.LINEAR
  generatorConfig.sweep.startFrequency = 100
  generatorConfig.sweep.stopFrequency = 10000
  generatorConfig.sweep.sweepTime = 5.0
  
  addLog('info', '配置线性扫频: 100Hz - 10kHz, 5秒')
  
  await new Promise(resolve => setTimeout(resolve, 3000))
  
  // 关闭扫频
  generatorConfig.sweep.enabled = false
  
  addLog('success', '扫频功能测试完成')
  ElMessage.success('扫频功能测试完成')
}

const runPerformanceTest = async () => {
  addLog('info', '开始性能压力测试...')
  
  let testCount = 0
  const maxTests = 100
  
  testTimer = setInterval(() => {
    // 随机改变参数
    generatorConfig.waveform.frequency = Math.random() * 100000 + 100
    generatorConfig.waveform.amplitude = Math.random() * 5 + 0.1
    generatorConfig.waveform.phase = Math.random() * 360
    
    testCount++
    
    if (testCount >= maxTests) {
      if (testTimer) {
        clearInterval(testTimer)
        testTimer = null
      }
      addLog('success', `性能测试完成: ${maxTests}次参数变更`)
      ElMessage.success('性能压力测试完成')
    }
  }, 50)
  
  addLog('info', `执行${maxTests}次快速参数变更测试...`)
}

const addLog = (type: TestLog['type'], message: string) => {
  testLogs.value.unshift({
    timestamp: new Date(),
    type,
    message
  })
  
  // 限制日志数量
  if (testLogs.value.length > 100) {
    testLogs.value = testLogs.value.slice(0, 100)
  }
}

const clearLogs = () => {
  testLogs.value = []
  ElMessage.success('日志已清空')
}

const exportLogs = () => {
  const logText = testLogs.value
    .map(log => `[${formatTime(log.timestamp)}] ${log.type.toUpperCase()}: ${log.message}`)
    .join('\n')
  
  const blob = new Blob([logText], { type: 'text/plain' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `signal-generator-test-logs-${Date.now()}.txt`
  a.click()
  URL.revokeObjectURL(url)
  
  ElMessage.success('日志已导出')
}

const exportConfig = () => {
  const configText = JSON.stringify(generatorConfig, null, 2)
  const blob = new Blob([configText], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `signal-generator-config-${Date.now()}.json`
  a.click()
  URL.revokeObjectURL(url)
  
  ElMessage.success('配置已导出')
}

const showHelp = () => {
  helpDialogVisible.value = true
}

const closeHelp = () => {
  helpDialogVisible.value = false
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

const formatTime = (date: Date): string => {
  return date.toLocaleTimeString()
}

// 生命周期
onMounted(() => {
  addLog('info', '信号发生器测试页面已加载')
})

onUnmounted(() => {
  if (testTimer) {
    clearInterval(testTimer)
    testTimer = null
  }
})
</script>

<style lang="scss" scoped>
.signal-generator-test {
  min-height: 100vh;
  background: var(--background-color);
}

.page-header {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  padding: 32px 0;
  
  .header-content {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 24px;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  
  .title-section {
    .page-title {
      display: flex;
      align-items: center;
      gap: 12px;
      margin: 0 0 8px 0;
      font-size: 28px;
      font-weight: 700;
    }
    
    .page-description {
      margin: 0;
      font-size: 16px;
      opacity: 0.9;
      line-height: 1.5;
    }
  }
  
  .header-actions {
    display: flex;
    gap: 12px;
  }
}

.test-content {
  max-width: 1400px;
  margin: 0 auto;
  padding: 24px;
  display: grid;
  grid-template-columns: 1fr 400px;
  gap: 24px;
}

.generator-section {
  .signal-generator {
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }
}

.test-panel {
  .test-card {
    height: fit-content;
    
    .card-header {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 600;
    }
  }
}

.test-controls {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.control-group {
  .group-label {
    display: block;
    font-size: 14px;
    font-weight: 600;
    color: var(--text-primary);
    margin-bottom: 12px;
  }
  
  .preset-buttons,
  .scenario-buttons {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }
  
  .monitor-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
    
    .monitor-item {
      padding: 12px;
      background: var(--surface-color);
      border-radius: 8px;
      border: 1px solid var(--border-color);
      
      .monitor-label {
        font-size: 12px;
        color: var(--text-secondary);
        margin-bottom: 4px;
      }
      
      .monitor-value {
        font-size: 14px;
        font-weight: 600;
        color: var(--text-primary);
        
        &.active {
          color: #10b981;
        }
      }
    }
  }
  
  .log-container {
    max-height: 200px;
    overflow-y: auto;
    border: 1px solid var(--border-color);
    border-radius: 8px;
    background: var(--surface-color);
    
    .log-item {
      padding: 8px 12px;
      border-bottom: 1px solid var(--border-color);
      font-size: 13px;
      display: flex;
      gap: 12px;
      
      &:last-child {
        border-bottom: none;
      }
      
      .log-time {
        color: var(--text-secondary);
        min-width: 80px;
      }
      
      .log-message {
        flex: 1;
      }
      
      &.info {
        .log-message {
          color: var(--text-primary);
        }
      }
      
      &.success {
        .log-message {
          color: #10b981;
        }
      }
      
      &.warning {
        .log-message {
          color: #f59e0b;
        }
      }
      
      &.error {
        .log-message {
          color: #ef4444;
        }
      }
    }
  }
  
  .log-actions {
    display: flex;
    gap: 8px;
    margin-top: 8px;
  }
}

.help-content {
  h3 {
    color: var(--text-primary);
    margin: 20px 0 12px 0;
    font-size: 16px;
    
    &:first-child {
      margin-top: 0;
    }
  }
  
  ul, ol {
    margin: 0 0 16px 0;
    padding-left: 20px;
    
    li {
      margin-bottom: 8px;
      line-height: 1.5;
      
      strong {
        color: var(--text-primary);
      }
    }
  }
  
  .spec-table {
    width: 100%;
    border-collapse: collapse;
    margin: 16px 0;
    
    td {
      padding: 8px 12px;
      border: 1px solid var(--border-color);
      
      &:first-child {
        background: var(--surface-color);
        font-weight: 600;
        width: 30%;
      }
    }
  }
}

// 响应式设计
@media (max-width: 1200px) {
  .test-content {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto;
  }
  
  .generator-section {
    order: 2;
  }
  
  .test-panel {
    order: 1;
  }
}

@media (max-width: 768px) {
  .page-header {
    .header-content {
      flex-direction: column;
      gap: 16px;
      text-align: center;
    }
  }
  
  .test-content {
    padding: 16px;
    gap: 16px;
  }
  
  .control-group {
    .monitor-grid {
      grid-template-columns: 1fr;
    }
  }
}

// Element Plus 样式覆盖
:deep(.el-card__header) {
  padding: 16px 20px;
  background: var(--surface-color);
  border-bottom: 1px solid var(--border-color);
}

:deep(.el-card__body) {
  padding: 20px;
}

:deep(.el-button--small) {
  width: 100%;
  justify-content: flex-start;
}

:deep(.el-dialog__body) {
  max-height: 60vh;
  overflow-y: auto;
}
</style>
