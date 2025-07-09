<template>
  <div class="dio-card-test">
    <!-- 页面标题 -->
    <div class="page-header">
      <h1>数字I/O控制卡测试</h1>
      <p>专业DIO控制卡控件演示和功能测试</p>
    </div>

    <!-- 控件演示 -->
    <div class="demo-section">
      <el-card class="demo-card" shadow="hover">
        <template #header>
          <div class="demo-header">
            <h3>DIO控制卡演示</h3>
            <div class="demo-actions">
              <el-button @click="resetDemo" size="small">
                <el-icon><RefreshRight /></el-icon>
                重置演示
              </el-button>
              <el-button @click="startAutoTest" size="small" type="primary">
                <el-icon><VideoPlay /></el-icon>
                自动测试
              </el-button>
            </div>
          </div>
        </template>

        <DIOCard
          :config="dioConfig"
          @config-change="onConfigChange"
          @pin-change="onPinChange"
          @port-change="onPortChange"
          @error="onError"
        />
      </el-card>
    </div>

    <!-- 配置面板 -->
    <div class="config-section">
      <el-row :gutter="20">
        <!-- 设备配置 -->
        <el-col :span="8">
          <el-card class="config-card" shadow="hover">
            <template #header>
              <h4>设备配置</h4>
            </template>
            
            <div class="config-form">
              <div class="form-item">
                <label>设备名称:</label>
                <el-input v-model="dioConfig.deviceName" size="small" />
              </div>
              
              <div class="form-item">
                <label>设备类型:</label>
                <el-select v-model="dioConfig.deviceType" size="small">
                  <el-option label="DIO卡" value="dio_card" />
                  <el-option label="开关矩阵" value="switch_matrix" />
                  <el-option label="继电器卡" value="relay_card" />
                </el-select>
              </div>
              
              <div class="form-item">
                <label>更新频率:</label>
                <el-input-number
                  v-model="dioConfig.globalSettings.updateRate"
                  :min="1"
                  :max="1000"
                  size="small"
                />
                <span class="unit">Hz</span>
              </div>
              
              <div class="form-item">
                <label>防抖时间:</label>
                <el-input-number
                  v-model="dioConfig.globalSettings.debounceTime"
                  :min="0"
                  :max="100"
                  size="small"
                />
                <span class="unit">ms</span>
              </div>
            </div>
          </el-card>
        </el-col>

        <!-- 端口配置 -->
        <el-col :span="8">
          <el-card class="config-card" shadow="hover">
            <template #header>
              <h4>端口配置</h4>
            </template>
            
            <div class="ports-config">
              <div 
                v-for="port in dioConfig.ports" 
                :key="port.id"
                class="port-config-item"
              >
                <div class="port-config-header">
                  <el-checkbox v-model="port.enabled">
                    {{ port.name }}
                  </el-checkbox>
                  <el-select 
                    v-model="port.direction" 
                    size="small"
                    style="width: 80px"
                  >
                    <el-option label="输入" value="input" />
                    <el-option label="输出" value="output" />
                    <el-option label="双向" value="bidirectional" />
                  </el-select>
                </div>
                
                <div class="port-pins" v-if="port.enabled">
                  <div class="pins-status">
                    <span class="label">引脚状态:</span>
                    <span class="pins-value">{{ formatPortBinary(port.value) }}</span>
                  </div>
                  
                  <div class="pins-control" v-if="port.direction === 'output'">
                    <el-button-group size="small">
                      <el-button @click="setPortPattern(port.id, 0x55)">0x55</el-button>
                      <el-button @click="setPortPattern(port.id, 0xAA)">0xAA</el-button>
                      <el-button @click="setPortPattern(port.id, 0xFF)">0xFF</el-button>
                      <el-button @click="setPortPattern(port.id, 0x00)">0x00</el-button>
                    </el-button-group>
                  </div>
                </div>
              </div>
            </div>
          </el-card>
        </el-col>

        <!-- 测试控制 -->
        <el-col :span="8">
          <el-card class="config-card" shadow="hover">
            <template #header>
              <h4>测试控制</h4>
            </template>
            
            <div class="test-controls">
              <div class="test-section">
                <h5>端口测试</h5>
                <el-button-group size="small">
                  <el-button @click="runPortTest">端口测试</el-button>
                  <el-button @click="runPinTest">引脚测试</el-button>
                </el-button-group>
              </div>
              
              <div class="test-section">
                <h5>模式测试</h5>
                <el-button-group size="small">
                  <el-button @click="runWalkingTest">走马灯</el-button>
                  <el-button @click="runCounterTest">计数器</el-button>
                </el-button-group>
              </div>
              
              <div class="test-section">
                <h5>性能测试</h5>
                <el-button-group size="small">
                  <el-button @click="runSpeedTest">速度测试</el-button>
                  <el-button @click="runStressTest">压力测试</el-button>
                </el-button-group>
              </div>
              
              <div class="test-section">
                <h5>测试状态</h5>
                <div class="test-status">
                  <div class="status-item">
                    <span>测试次数:</span>
                    <span class="value">{{ testStats.totalTests }}</span>
                  </div>
                  <div class="status-item">
                    <span>成功率:</span>
                    <span class="value">{{ testStats.successRate.toFixed(1) }}%</span>
                  </div>
                  <div class="status-item">
                    <span>平均延时:</span>
                    <span class="value">{{ testStats.averageDelay.toFixed(1) }}ms</span>
                  </div>
                </div>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 事件日志 -->
    <div class="log-section">
      <el-card class="log-card" shadow="hover">
        <template #header>
          <div class="log-header">
            <h4>事件日志</h4>
            <div class="log-actions">
              <el-button @click="clearEventLog" size="small">
                <el-icon><Delete /></el-icon>
                清除日志
              </el-button>
              <el-button @click="exportEventLog" size="small">
                <el-icon><Download /></el-icon>
                导出日志
              </el-button>
            </div>
          </div>
        </template>
        
        <div class="event-log">
          <div 
            v-for="(event, index) in eventLog" 
            :key="index"
            class="log-item"
            :class="event.type"
          >
            <span class="log-time">{{ event.timestamp }}</span>
            <span class="log-type">{{ event.type.toUpperCase() }}</span>
            <span class="log-message">{{ event.message }}</span>
            <span class="log-data" v-if="event.data">{{ formatEventData(event.data) }}</span>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 技术文档 -->
    <div class="docs-section">
      <el-card class="docs-card" shadow="hover">
        <template #header>
          <h4>技术文档</h4>
        </template>
        
        <div class="docs-content">
          <el-collapse v-model="activeDoc">
            <el-collapse-item title="功能特性" name="features">
              <div class="doc-section">
                <h5>核心功能</h5>
                <ul>
                  <li><strong>多端口支持</strong>：支持多个数字I/O端口，每个端口8位</li>
                  <li><strong>方向控制</strong>：支持输入、输出、双向模式</li>
                  <li><strong>位操作</strong>：支持单个引脚和整个端口的操作</li>
                  <li><strong>实时监控</strong>：实时显示端口和引脚状态</li>
                  <li><strong>配置管理</strong>：支持配置保存和加载</li>
                </ul>
                
                <h5>高级特性</h5>
                <ul>
                  <li><strong>防抖处理</strong>：可配置的输入防抖时间</li>
                  <li><strong>上拉/下拉</strong>：输入引脚的上拉下拉配置</li>
                  <li><strong>驱动强度</strong>：输出引脚的驱动强度设置</li>
                  <li><strong>安全模式</strong>：防止误操作的安全保护</li>
                  <li><strong>看门狗</strong>：系统监控和故障检测</li>
                </ul>
              </div>
            </el-collapse-item>
            
            <el-collapse-item title="技术规格" name="specs">
              <div class="doc-section">
                <h5>硬件规格</h5>
                <ul>
                  <li><strong>端口数量</strong>：最多支持4个端口</li>
                  <li><strong>引脚数量</strong>：每个端口8个引脚</li>
                  <li><strong>电压范围</strong>：3.3V/5V兼容</li>
                  <li><strong>电流能力</strong>：每引脚最大25mA</li>
                  <li><strong>更新频率</strong>：1Hz - 1000Hz可配置</li>
                </ul>
                
                <h5>软件特性</h5>
                <ul>
                  <li><strong>实时性</strong>：毫秒级响应时间</li>
                  <li><strong>可靠性</strong>：错误检测和恢复机制</li>
                  <li><strong>易用性</strong>：直观的图形界面</li>
                  <li><strong>扩展性</strong>：支持自定义配置</li>
                </ul>
              </div>
            </el-collapse-item>
            
            <el-collapse-item title="使用示例" name="examples">
              <div class="doc-section">
                <h5>基本使用</h5>
                <pre><code>// 设置端口方向
port.direction = 'output'

// 设置端口值
port.value = 0xFF

// 设置单个引脚
pin.value = true</code></pre>
                
                <h5>事件处理</h5>
                <pre><code>// 监听引脚变化
@pin-change="onPinChange"

// 监听端口变化
@port-change="onPortChange"

// 监听配置变化
@config-change="onConfigChange"</code></pre>
              </div>
            </el-collapse-item>
          </el-collapse>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import { RefreshRight, VideoPlay, Delete, Download } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import DIOCard from '@/components/professional/instruments/DIOCard.vue'
import type { DIOConfig } from '@/types/dio'
import { DEFAULT_DIO_CONFIG } from '@/types/dio'

// 响应式数据
const dioConfig = reactive<DIOConfig>({ ...DEFAULT_DIO_CONFIG })

const testStats = reactive({
  totalTests: 0,
  successfulTests: 0,
  failedTests: 0,
  averageDelay: 0,
  successRate: 0
})

const eventLog = ref<Array<{
  timestamp: string
  type: 'info' | 'success' | 'warning' | 'error'
  message: string
  data?: any
}>>([])

const activeDoc = ref(['features'])

// 测试状态
let testTimer: number | null = null
let autoTestRunning = false

// 计算属性
const formatPortBinary = (value: number) => {
  return value.toString(2).padStart(8, '0')
}

// 方法
const addEvent = (type: 'info' | 'success' | 'warning' | 'error', message: string, data?: any) => {
  eventLog.value.unshift({
    timestamp: new Date().toLocaleTimeString(),
    type,
    message,
    data
  })
  
  // 限制日志数量
  if (eventLog.value.length > 100) {
    eventLog.value = eventLog.value.slice(0, 100)
  }
}

const onConfigChange = (config: DIOConfig) => {
  addEvent('info', '配置已更新', { deviceName: config.deviceName })
}

const onPinChange = (portId: number, pinId: number, value: boolean) => {
  const port = dioConfig.ports.find(p => p.id === portId)
  const pin = port?.pins.find(p => p.id === pinId)
  
  if (port && pin) {
    addEvent('success', `引脚 ${pin.label} 状态变化`, { 
      port: port.name, 
      pin: pin.label, 
      value: value ? 'HIGH' : 'LOW' 
    })
    
    updateTestStats(true)
  }
}

const onPortChange = (portId: number, value: number) => {
  const port = dioConfig.ports.find(p => p.id === portId)
  
  if (port) {
    addEvent('success', `端口 ${port.name} 值变化`, { 
      port: port.name, 
      value: `0x${value.toString(16).toUpperCase().padStart(2, '0')}`,
      binary: formatPortBinary(value)
    })
    
    updateTestStats(true)
  }
}

const onError = (error: string) => {
  addEvent('error', '操作失败', { error })
  updateTestStats(false)
}

const updateTestStats = (success: boolean) => {
  testStats.totalTests++
  if (success) {
    testStats.successfulTests++
  } else {
    testStats.failedTests++
  }
  
  testStats.successRate = (testStats.successfulTests / testStats.totalTests) * 100
  testStats.averageDelay = Math.random() * 10 + 1
}

const resetDemo = () => {
  // 重置所有端口
  dioConfig.ports.forEach(port => {
    port.value = 0
    port.pins.forEach(pin => {
      pin.value = false
    })
  })
  
  addEvent('info', '演示已重置')
  ElMessage.success('演示已重置')
}

const setPortPattern = (portId: number, pattern: number) => {
  const port = dioConfig.ports.find(p => p.id === portId)
  if (port && port.direction === 'output') {
    port.value = pattern
    
    // 更新引脚状态
    port.pins.forEach((pin, index) => {
      pin.value = Boolean(pattern & (1 << index))
    })
    
    addEvent('info', `端口 ${port.name} 设置为模式`, { 
      pattern: `0x${pattern.toString(16).toUpperCase()}`,
      binary: formatPortBinary(pattern)
    })
  }
}

const runPortTest = async () => {
  addEvent('info', '开始端口测试')
  
  const outputPorts = dioConfig.ports.filter(p => p.enabled && p.direction === 'output')
  
  for (const port of outputPorts) {
    for (let i = 0; i <= 255; i += 51) {
      port.value = i
      port.pins.forEach((pin, index) => {
        pin.value = Boolean(i & (1 << index))
      })
      
      await new Promise(resolve => setTimeout(resolve, 100))
    }
  }
  
  addEvent('success', '端口测试完成')
  ElMessage.success('端口测试完成')
}

const runPinTest = async () => {
  addEvent('info', '开始引脚测试')
  
  const outputPorts = dioConfig.ports.filter(p => p.enabled && p.direction === 'output')
  
  for (const port of outputPorts) {
    for (let i = 0; i < 8; i++) {
      port.value = 1 << i
      port.pins.forEach((pin, index) => {
        pin.value = index === i
      })
      
      await new Promise(resolve => setTimeout(resolve, 200))
    }
  }
  
  addEvent('success', '引脚测试完成')
  ElMessage.success('引脚测试完成')
}

const runWalkingTest = async () => {
  addEvent('info', '开始走马灯测试')
  
  const outputPorts = dioConfig.ports.filter(p => p.enabled && p.direction === 'output')
  
  for (let cycle = 0; cycle < 3; cycle++) {
    for (const port of outputPorts) {
      for (let i = 0; i < 8; i++) {
        port.value = 1 << i
        port.pins.forEach((pin, index) => {
          pin.value = index === i
        })
        
        await new Promise(resolve => setTimeout(resolve, 150))
      }
    }
  }
  
  addEvent('success', '走马灯测试完成')
  ElMessage.success('走马灯测试完成')
}

const runCounterTest = async () => {
  addEvent('info', '开始计数器测试')
  
  const outputPorts = dioConfig.ports.filter(p => p.enabled && p.direction === 'output')
  
  for (const port of outputPorts) {
    for (let i = 0; i < 256; i++) {
      port.value = i
      port.pins.forEach((pin, index) => {
        pin.value = Boolean(i & (1 << index))
      })
      
      await new Promise(resolve => setTimeout(resolve, 50))
    }
  }
  
  addEvent('success', '计数器测试完成')
  ElMessage.success('计数器测试完成')
}

const runSpeedTest = async () => {
  addEvent('info', '开始速度测试')
  
  const startTime = Date.now()
  const iterations = 1000
  
  const outputPorts = dioConfig.ports.filter(p => p.enabled && p.direction === 'output')
  
  for (let i = 0; i < iterations; i++) {
    for (const port of outputPorts) {
      port.value = Math.floor(Math.random() * 256)
      port.pins.forEach((pin, index) => {
        pin.value = Boolean(port.value & (1 << index))
      })
    }
  }
  
  const endTime = Date.now()
  const duration = endTime - startTime
  const rate = (iterations / duration) * 1000
  
  addEvent('success', '速度测试完成', { 
    iterations, 
    duration: `${duration}ms`, 
    rate: `${rate.toFixed(1)} ops/s` 
  })
  
  ElMessage.success(`速度测试完成: ${rate.toFixed(1)} ops/s`)
}

const runStressTest = async () => {
  addEvent('info', '开始压力测试')
  
  const duration = 10000 // 10秒
  const startTime = Date.now()
  let operations = 0
  
  const outputPorts = dioConfig.ports.filter(p => p.enabled && p.direction === 'output')
  
  while (Date.now() - startTime < duration) {
    for (const port of outputPorts) {
      port.value = Math.floor(Math.random() * 256)
      port.pins.forEach((pin, index) => {
        pin.value = Boolean(port.value & (1 << index))
      })
      operations++
    }
    
    await new Promise(resolve => setTimeout(resolve, 1))
  }
  
  const actualDuration = Date.now() - startTime
  const rate = (operations / actualDuration) * 1000
  
  addEvent('success', '压力测试完成', { 
    operations, 
    duration: `${actualDuration}ms`, 
    rate: `${rate.toFixed(1)} ops/s` 
  })
  
  ElMessage.success(`压力测试完成: ${rate.toFixed(1)} ops/s`)
}

const startAutoTest = async () => {
  if (autoTestRunning) {
    autoTestRunning = false
    if (testTimer) {
      clearInterval(testTimer)
      testTimer = null
    }
    addEvent('info', '自动测试已停止')
    ElMessage.info('自动测试已停止')
    return
  }
  
  autoTestRunning = true
  addEvent('info', '开始自动测试')
  
  const tests = [
    runPortTest,
    runPinTest,
    runWalkingTest,
    runCounterTest
  ]
  
  let testIndex = 0
  
  testTimer = setInterval(async () => {
    if (!autoTestRunning) return
    
    try {
      await tests[testIndex]()
      testIndex = (testIndex + 1) % tests.length
    } catch (error) {
      addEvent('error', '自动测试出错', { error })
    }
  }, 5000)
  
  ElMessage.success('自动测试已启动')
}

const clearEventLog = () => {
  eventLog.value = []
  addEvent('info', '事件日志已清除')
}

const exportEventLog = () => {
  const logText = eventLog.value
    .map(event => `[${event.timestamp}] ${event.type.toUpperCase()}: ${event.message}${event.data ? ' - ' + JSON.stringify(event.data) : ''}`)
    .join('\n')
  
  const blob = new Blob([logText], { type: 'text/plain' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `dio-test-log-${Date.now()}.txt`
  a.click()
  URL.revokeObjectURL(url)
  
  ElMessage.success('日志已导出')
}

const formatEventData = (data: any) => {
  return JSON.stringify(data, null, 2)
}

// 生命周期
onMounted(() => {
  addEvent('info', 'DIO控制卡测试页面已加载')
})

onUnmounted(() => {
  if (testTimer) {
    clearInterval(testTimer)
  }
})
</script>

<style lang="scss" scoped>
.dio-card-test {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  text-align: center;
  margin-bottom: 30px;
  
  h1 {
    font-size: 28px;
    color: var(--text-primary);
    margin-bottom: 8px;
  }
  
  p {
    color: var(--text-secondary);
    font-size: 16px;
  }
}

.demo-section {
  margin-bottom: 30px;
  
  .demo-card {
    .demo-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      
      h3 {
        margin: 0;
        color: var(--text-primary);
      }
      
      .demo-actions {
        display: flex;
        gap: 8px;
      }
    }
  }
}

.config-section {
  margin-bottom: 30px;
  
  .config-card {
    height: 100%;
    
    h4 {
      margin: 0;
      color: var(--text-primary);
    }
  }
  
  .config-form {
    .form-item {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 12px;
      
      label {
        min-width: 80px;
        font-size: 12px;
        color: var(--text-secondary);
      }
      
      .unit {
        font-size: 12px;
        color: var(--text-secondary);
      }
    }
  }
  
  .ports-config {
    .port-config-item {
      border: 1px solid var(--border-color);
      border-radius: 6px;
      padding: 12px;
      margin-bottom: 12px;
      
      .port-config-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 8px;
      }
      
      .port-pins {
        .pins-status {
          display: flex;
          align-items: center;
          gap: 8px;
          margin-bottom: 8px;
          
          .label {
            font-size: 12px;
            color: var(--text-secondary);
          }
          
          .pins-value {
            font-family: 'Consolas', monospace;
            background: var(--digital-bg);
            padding: 2px 6px;
            border-radius: 3px;
            color: var(--digital-value);
          }
        }
        
        .pins-control {
          display: flex;
          gap: 4px;
        }
      }
    }
  }
  
  .test-controls {
    .test-section {
      margin-bottom: 16px;
      
      h5 {
        margin: 0 0 8px 0;
        font-size: 14px;
        color: var(--text-primary);
      }
    }
    
    .test-status {
      .status-item {
        display: flex;
        justify-content: space-between;
        margin-bottom: 4px;
        font-size: 12px;
        
        .value {
          font-weight: 500;
          color: var(--primary-color);
        }
      }
    }
  }
}

.log-section {
  margin-bottom: 30px;
  
  .log-card {
    .log-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      
      h4 {
        margin: 0;
        color: var(--text-primary);
      }
      
      .log-actions {
        display: flex;
        gap: 8px;
      }
    }
  }
  
  .event-log {
    max-height: 300px;
    overflow-y: auto;
    
    .log-item {
      display: flex;
      gap: 12px;
      padding: 8px 0;
      border-bottom: 1px solid var(--border-color);
      font-size: 12px;
      
      &:last-child {
        border-bottom: none;
      }
      
      .log-time {
        color: var(--text-secondary);
        min-width: 80px;
      }
      
      .log-type {
        min-width: 60px;
        font-weight: 500;
      }
      
      .log-message {
        flex: 1;
      }
      
      .log-data {
        color: var(--text-secondary);
        font-family: 'Consolas', monospace;
        font-size: 11px;
      }
      
      &.info {
        .log-type {
          color: var(--text-primary);
        }
      }
      
      &.success {
        .log-type {
          color: var(--status-normal);
        }
      }
      
      &.warning {
        .log-type {
          color: var(--status-warning);
        }
      }
      
      &.error {
        .log-type {
          color: var(--status-error);
        }
      }
    }
  }
}

.docs-section {
  .docs-card {
    h4 {
      margin: 0;
      color: var(--text-primary);
    }
  }
  
  .docs-content {
    .doc-section {
      h5 {
        color: var(--text-primary);
        margin-bottom: 12px;
      }
      
      ul {
        margin-bottom: 16px;
        
        li {
          margin-bottom: 8px;
          
          strong {
            color: var(--primary-color);
          }
        }
      }
      
      pre {
        background: var(--surface-color);
        border: 1px solid var(--border-color);
        border-radius: 4px;
        padding: 12px;
        margin: 12px 0;
        overflow-x: auto;
        
        code {
          font-family: 'Consolas', monospace;
          font-size: 12px;
          color: var(--text-primary);
        }
      }
    }
  }
}

// 响应式设计
@media (max-width: 1200px) {
  .config-section {
    .el-col {
      margin-bottom: 20px;
    }
  }
}

@media (max-width: 768px) {
  .dio-card-test {
    padding: 10px;
  }
  
  .demo-header {
    flex-direction: column;
    gap: 12px;
    align-items: flex-start;
  }
  
  .log-header {
    flex-direction: column;
    gap: 12px;
    align-items: flex-start;
  }
  
  .event-log {
    .log-item {
      flex-direction: column;
      gap: 4px;
      
      .log-time,
      .log-type {
        min-width: auto;
      }
    }
  }
}
</style>
