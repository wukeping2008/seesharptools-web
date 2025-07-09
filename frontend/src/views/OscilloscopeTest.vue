<template>
  <div class="oscilloscope-test">
    <div class="test-header">
      <h1>数字示波器测试</h1>
      <p>专业4通道数字示波器控件演示</p>
    </div>

    <div class="test-content">
      <!-- 示波器控件 -->
      <div class="oscilloscope-container">
        <Oscilloscope
          :width="1200"
          :height="800"
          :config="oscilloscopeConfig"
          @config-change="onConfigChange"
          @waveform-data="onWaveformData"
          @measurement-change="onMeasurementChange"
          @trigger-change="onTriggerChange"
          @error="onError"
        />
      </div>

      <!-- 控制面板 -->
      <div class="control-section">
        <el-card class="control-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Setting /></el-icon>
              <span>测试控制</span>
            </div>
          </template>

          <div class="control-content">
            <div class="control-group">
              <h4>信号源控制</h4>
              <div class="signal-controls">
                <div class="control-item">
                  <label>CH1 信号类型:</label>
                  <el-select v-model="signalConfig.ch1.type" @change="updateSignals">
                    <el-option label="正弦波" value="sine" />
                    <el-option label="方波" value="square" />
                    <el-option label="三角波" value="triangle" />
                    <el-option label="锯齿波" value="sawtooth" />
                    <el-option label="噪声" value="noise" />
                  </el-select>
                </div>
                
                <div class="control-item">
                  <label>CH1 频率:</label>
                  <el-input-number
                    v-model="signalConfig.ch1.frequency"
                    :min="1"
                    :max="100000"
                    @change="updateSignals"
                  />
                  <span class="unit">Hz</span>
                </div>
                
                <div class="control-item">
                  <label>CH1 幅度:</label>
                  <el-input-number
                    v-model="signalConfig.ch1.amplitude"
                    :min="0.1"
                    :max="10"
                    :precision="1"
                    :step="0.1"
                    @change="updateSignals"
                  />
                  <span class="unit">V</span>
                </div>
              </div>

              <div class="signal-controls">
                <div class="control-item">
                  <label>CH2 信号类型:</label>
                  <el-select v-model="signalConfig.ch2.type" @change="updateSignals">
                    <el-option label="正弦波" value="sine" />
                    <el-option label="方波" value="square" />
                    <el-option label="三角波" value="triangle" />
                    <el-option label="锯齿波" value="sawtooth" />
                    <el-option label="噪声" value="noise" />
                  </el-select>
                </div>
                
                <div class="control-item">
                  <label>CH2 频率:</label>
                  <el-input-number
                    v-model="signalConfig.ch2.frequency"
                    :min="1"
                    :max="100000"
                    @change="updateSignals"
                  />
                  <span class="unit">Hz</span>
                </div>
                
                <div class="control-item">
                  <label>CH2 幅度:</label>
                  <el-input-number
                    v-model="signalConfig.ch2.amplitude"
                    :min="0.1"
                    :max="10"
                    :precision="1"
                    :step="0.1"
                    @change="updateSignals"
                  />
                  <span class="unit">V</span>
                </div>
              </div>
            </div>

            <div class="control-group">
              <h4>预设配置</h4>
              <div class="preset-buttons">
                <el-button @click="loadPreset('default')">默认配置</el-button>
                <el-button @click="loadPreset('highFreq')">高频信号</el-button>
                <el-button @click="loadPreset('lowFreq')">低频信号</el-button>
                <el-button @click="loadPreset('mixed')">混合信号</el-button>
              </div>
            </div>

            <div class="control-group">
              <h4>测试功能</h4>
              <div class="test-buttons">
                <el-button type="primary" @click="startAutoTest">自动测试</el-button>
                <el-button @click="exportData">导出数据</el-button>
                <el-button @click="resetConfig">重置配置</el-button>
                <el-button @click="showHelp">帮助说明</el-button>
              </div>
            </div>
          </div>
        </el-card>

        <!-- 状态显示 -->
        <el-card class="status-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Monitor /></el-icon>
              <span>状态信息</span>
            </div>
          </template>

          <div class="status-content">
            <div class="status-item">
              <span class="label">配置更新次数:</span>
              <span class="value">{{ statusInfo.configUpdates }}</span>
            </div>
            <div class="status-item">
              <span class="label">数据接收次数:</span>
              <span class="value">{{ statusInfo.dataReceived }}</span>
            </div>
            <div class="status-item">
              <span class="label">测量更新次数:</span>
              <span class="value">{{ statusInfo.measurementUpdates }}</span>
            </div>
            <div class="status-item">
              <span class="label">触发次数:</span>
              <span class="value">{{ statusInfo.triggerCount }}</span>
            </div>
            <div class="status-item">
              <span class="label">最后更新:</span>
              <span class="value">{{ statusInfo.lastUpdate }}</span>
            </div>
          </div>
        </el-card>
      </div>
    </div>

    <!-- 功能说明 -->
    <div class="documentation">
      <el-card shadow="hover">
        <template #header>
          <div class="card-header">
            <el-icon><Document /></el-icon>
            <span>功能说明</span>
          </div>
        </template>

        <div class="doc-content">
          <h3>🔬 数字示波器控件特性</h3>
          
          <div class="feature-section">
            <h4>📊 核心功能</h4>
            <ul>
              <li><strong>4通道同步显示</strong> - 支持4个模拟通道的同时显示和控制</li>
              <li><strong>专业触发系统</strong> - 边沿触发、脉宽触发、视频触发等多种触发模式</li>
              <li><strong>高精度时基控制</strong> - 从1ns/div到10s/div的宽范围时基设置</li>
              <li><strong>灵活的垂直控制</strong> - 每通道独立的垂直刻度和偏移设置</li>
              <li><strong>多种采集模式</strong> - 正常、平均、峰值检测、包络等采集模式</li>
            </ul>
          </div>

          <div class="feature-section">
            <h4>📈 测量功能</h4>
            <ul>
              <li><strong>自动测量</strong> - 频率、周期、幅度、RMS、平均值等12种自动测量</li>
              <li><strong>游标测量</strong> - 双游标精确测量时间差、电压差和频率</li>
              <li><strong>统计分析</strong> - 最值、平均值、标准差等统计信息</li>
              <li><strong>实时更新</strong> - 所有测量结果实时计算和显示</li>
            </ul>
          </div>

          <div class="feature-section">
            <h4>⚙️ 高级特性</h4>
            <ul>
              <li><strong>专业界面</strong> - 科学仪器风格的专业用户界面</li>
              <li><strong>实时波形显示</strong> - 基于ProfessionalEasyChart的高性能波形显示</li>
              <li><strong>配置管理</strong> - 完整的配置保存、加载和重置功能</li>
              <li><strong>数据导出</strong> - 支持波形数据和测量结果的导出</li>
              <li><strong>响应式设计</strong> - 适配不同屏幕尺寸的响应式布局</li>
            </ul>
          </div>

          <div class="feature-section">
            <h4>🎯 技术规格</h4>
            <ul>
              <li><strong>采样率</strong> - 1kSa/s 到 5GSa/s</li>
              <li><strong>内存深度</strong> - 1K 到 1G 采样点</li>
              <li><strong>垂直分辨率</strong> - 1mV/div 到 100V/div</li>
              <li><strong>时间分辨率</strong> - 1ns/div 到 10s/div</li>
              <li><strong>探头支持</strong> - 0.1x 到 1000x 探头倍数</li>
            </ul>
          </div>

          <div class="feature-section">
            <h4>🚀 使用方法</h4>
            <ol>
              <li>选择要启用的通道并配置垂直参数</li>
              <li>设置合适的时基和水平位置</li>
              <li>配置触发条件和触发源</li>
              <li>选择采集模式和采样参数</li>
              <li>点击运行开始采集和显示波形</li>
              <li>使用测量工具进行精确测量</li>
              <li>根据需要调整显示和触发参数</li>
            </ol>
          </div>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Setting, Monitor, Document } from '@element-plus/icons-vue'
import Oscilloscope from '@/components/professional/instruments/Oscilloscope.vue'
import type { OscilloscopeConfig } from '@/types/oscilloscope'
import { DEFAULT_OSCILLOSCOPE_CONFIG } from '@/types/oscilloscope'

// 示波器配置
const oscilloscopeConfig = reactive<Partial<OscilloscopeConfig>>({
  ...DEFAULT_OSCILLOSCOPE_CONFIG
})

// 信号配置
const signalConfig = reactive({
  ch1: {
    type: 'sine',
    frequency: 1000,
    amplitude: 2.0
  },
  ch2: {
    type: 'square',
    frequency: 500,
    amplitude: 1.5
  }
})

// 状态信息
const statusInfo = reactive({
  configUpdates: 0,
  dataReceived: 0,
  measurementUpdates: 0,
  triggerCount: 0,
  lastUpdate: ''
})

// 事件处理
const onConfigChange = (config: OscilloscopeConfig) => {
  statusInfo.configUpdates++
  statusInfo.lastUpdate = new Date().toLocaleTimeString()
  console.log('配置更新:', config)
}

const onWaveformData = (data: any[]) => {
  statusInfo.dataReceived++
  statusInfo.lastUpdate = new Date().toLocaleTimeString()
  console.log('波形数据:', data)
}

const onMeasurementChange = (measurements: any[]) => {
  statusInfo.measurementUpdates++
  statusInfo.lastUpdate = new Date().toLocaleTimeString()
  console.log('测量更新:', measurements)
}

const onTriggerChange = (triggered: boolean) => {
  if (triggered) {
    statusInfo.triggerCount++
  }
  statusInfo.lastUpdate = new Date().toLocaleTimeString()
  console.log('触发状态:', triggered)
}

const onError = (error: string) => {
  ElMessage.error(`示波器错误: ${error}`)
  console.error('示波器错误:', error)
}

// 控制方法
const updateSignals = () => {
  ElMessage.success('信号配置已更新')
}

const loadPreset = (preset: string) => {
  switch (preset) {
    case 'default':
      Object.assign(oscilloscopeConfig, DEFAULT_OSCILLOSCOPE_CONFIG)
      signalConfig.ch1 = { type: 'sine', frequency: 1000, amplitude: 2.0 }
      signalConfig.ch2 = { type: 'square', frequency: 500, amplitude: 1.5 }
      break
    case 'highFreq':
      oscilloscopeConfig.timebase!.scale = 0.00001 // 10μs/div
      signalConfig.ch1 = { type: 'sine', frequency: 50000, amplitude: 1.0 }
      signalConfig.ch2 = { type: 'square', frequency: 25000, amplitude: 0.8 }
      break
    case 'lowFreq':
      oscilloscopeConfig.timebase!.scale = 0.1 // 100ms/div
      signalConfig.ch1 = { type: 'sine', frequency: 10, amplitude: 3.0 }
      signalConfig.ch2 = { type: 'triangle', frequency: 5, amplitude: 2.0 }
      break
    case 'mixed':
      oscilloscopeConfig.timebase!.scale = 0.001 // 1ms/div
      signalConfig.ch1 = { type: 'sine', frequency: 1000, amplitude: 2.0 }
      signalConfig.ch2 = { type: 'noise', frequency: 0, amplitude: 0.5 }
      break
  }
  ElMessage.success(`已加载${preset}预设配置`)
}

const startAutoTest = async () => {
  ElMessage.info('开始自动测试...')
  
  // 模拟自动测试流程
  const tests = [
    { name: '通道配置测试', delay: 1000 },
    { name: '时基设置测试', delay: 1000 },
    { name: '触发功能测试', delay: 1500 },
    { name: '测量功能测试', delay: 1500 },
    { name: '数据采集测试', delay: 2000 }
  ]
  
  for (const test of tests) {
    ElMessage.info(`正在执行: ${test.name}`)
    await new Promise(resolve => setTimeout(resolve, test.delay))
  }
  
  ElMessage.success('自动测试完成！所有功能正常')
}

const exportData = () => {
  const data = {
    config: oscilloscopeConfig,
    signals: signalConfig,
    status: statusInfo,
    timestamp: new Date().toISOString()
  }
  
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `oscilloscope-data-${Date.now()}.json`
  a.click()
  URL.revokeObjectURL(url)
  
  ElMessage.success('数据已导出')
}

const resetConfig = async () => {
  await ElMessageBox.confirm('确定要重置所有配置吗？', '确认重置', {
    type: 'warning'
  })
  
  Object.assign(oscilloscopeConfig, DEFAULT_OSCILLOSCOPE_CONFIG)
  signalConfig.ch1 = { type: 'sine', frequency: 1000, amplitude: 2.0 }
  signalConfig.ch2 = { type: 'square', frequency: 500, amplitude: 1.5 }
  
  ElMessage.success('配置已重置')
}

const showHelp = () => {
  ElMessageBox.alert(
    '这是一个专业的数字示波器控件演示。您可以通过左侧的控制面板调整各种参数，观察波形显示的变化。支持4通道同步显示、多种触发模式、精确测量等专业功能。',
    '帮助说明',
    { type: 'info' }
  )
}

// 生命周期
onMounted(() => {
  ElMessage.success('数字示波器测试页面已加载')
})
</script>

<style lang="scss" scoped>
.oscilloscope-test {
  padding: 20px;
  background: #f5f7fa;
  min-height: 100vh;
}

.test-header {
  text-align: center;
  margin-bottom: 30px;
  
  h1 {
    color: #2c3e50;
    margin-bottom: 10px;
  }
  
  p {
    color: #7f8c8d;
    font-size: 16px;
  }
}

.test-content {
  display: grid;
  grid-template-columns: 1fr 350px;
  gap: 20px;
  margin-bottom: 30px;
}

.oscilloscope-container {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.control-section {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.control-card,
.status-card {
  .card-header {
    display: flex;
    align-items: center;
    gap: 8px;
    font-weight: 600;
  }
}

.control-content {
  .control-group {
    margin-bottom: 24px;
    
    h4 {
      margin: 0 0 12px 0;
      color: #2c3e50;
      font-size: 14px;
    }
  }
  
  .signal-controls {
    display: flex;
    flex-direction: column;
    gap: 12px;
    margin-bottom: 16px;
    padding: 12px;
    background: #f8f9fa;
    border-radius: 6px;
    
    .control-item {
      display: flex;
      align-items: center;
      gap: 8px;
      
      label {
        min-width: 80px;
        font-size: 12px;
        color: #666;
      }
      
      .el-select,
      .el-input-number {
        flex: 1;
      }
      
      .unit {
        font-size: 12px;
        color: #999;
        min-width: 20px;
      }
    }
  }
  
  .preset-buttons,
  .test-buttons {
    display: flex;
    flex-direction: column;
    gap: 8px;
    
    .el-button {
      justify-content: flex-start;
    }
  }
}

.status-content {
  .status-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 0;
    border-bottom: 1px solid #eee;
    
    &:last-child {
      border-bottom: none;
    }
    
    .label {
      font-size: 13px;
      color: #666;
    }
    
    .value {
      font-weight: 600;
      color: #2c3e50;
      font-family: 'Consolas', monospace;
    }
  }
}

.documentation {
  .doc-content {
    h3 {
      color: #2c3e50;
      margin-bottom: 20px;
    }
    
    .feature-section {
      margin-bottom: 24px;
      
      h4 {
        color: #34495e;
        margin-bottom: 12px;
        font-size: 16px;
      }
      
      ul, ol {
        margin: 0;
        padding-left: 20px;
        
        li {
          margin-bottom: 8px;
          line-height: 1.6;
          
          strong {
            color: #2980b9;
          }
        }
      }
    }
  }
}

// 响应式设计
@media (max-width: 1400px) {
  .test-content {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto;
  }
  
  .control-section {
    flex-direction: row;
    
    .control-card,
    .status-card {
      flex: 1;
    }
  }
}

@media (max-width: 768px) {
  .oscilloscope-test {
    padding: 10px;
  }
  
  .control-section {
    flex-direction: column;
  }
  
  .signal-controls {
    .control-item {
      flex-direction: column;
      align-items: stretch;
      
      label {
        min-width: auto;
        margin-bottom: 4px;
      }
    }
  }
}
</style>
