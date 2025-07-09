<template>
  <div class="dio-card">
    <!-- 控件标题栏 -->
    <div class="dio-header">
      <div class="header-left">
        <h3 class="dio-title">
          <el-icon><Grid /></el-icon>
          数字I/O控制卡
        </h3>
        <div class="dio-model">{{ config.deviceName }} - {{ deviceTypeLabel }}</div>
      </div>
      <div class="header-right">
        <el-button 
          :type="status.connected ? 'success' : 'danger'"
          @click="toggleConnection"
          :loading="connectionChanging"
        >
          <el-icon><Link v-if="status.connected" /><CircleClose v-else /></el-icon>
          {{ status.connected ? '已连接' : '未连接' }}
        </el-button>
        <el-button @click="resetAllPorts" :disabled="!status.connected">
          <el-icon><RefreshRight /></el-icon>
          复位
        </el-button>
        <el-dropdown @command="handleMenuCommand">
          <el-button circle>
            <el-icon><More /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="save">保存配置</el-dropdown-item>
              <el-dropdown-item command="load">加载配置</el-dropdown-item>
              <el-dropdown-item command="export">导出数据</el-dropdown-item>
              <el-dropdown-item command="test">自检测试</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>

    <!-- 主要内容区域 -->
    <div class="dio-content">
      <!-- 左侧控制面板 -->
      <div class="control-panel">
        <!-- 端口控制 -->
        <el-card class="control-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Grid /></el-icon>
              <span>端口控制</span>
            </div>
          </template>
          
          <div class="ports-control">
            <div 
              v-for="port in config.ports" 
              :key="port.id"
              class="port-item"
              :class="{ disabled: !port.enabled }"
            >
              <div class="port-header">
                <el-checkbox 
                  v-model="port.enabled"
                  @change="onPortEnabledChange(port.id)"
                >
                  <span class="port-label">{{ port.name }}</span>
                </el-checkbox>
                <div class="port-direction">
                  <el-select 
                    v-model="port.direction" 
                    @change="onPortDirectionChange(port.id)"
                    size="small"
                    :disabled="!port.enabled"
                  >
                    <el-option
                      v-for="option in DIO_DIRECTIONS"
                      :key="option.value"
                      :label="option.label"
                      :value="option.value"
                    />
                  </el-select>
                </div>
              </div>
              
              <div class="port-value" v-if="port.enabled">
                <div class="value-display">
                  <span class="label">端口值:</span>
                  <span class="hex-value">{{ formatPortValue(port.value, 8) }}</span>
                  <span class="binary-value">{{ formatBinaryValue(port.value, 8) }}</span>
                </div>
                
                <div class="port-input" v-if="port.direction === 'output'">
                  <el-input-number
                    v-model="port.value"
                    :min="0"
                    :max="255"
                    @change="onPortValueChange(port.id)"
                    size="small"
                  />
                  <el-button 
                    size="small" 
                    @click="setPortValueDirect(port.id, 0)"
                    :disabled="!status.connected"
                  >
                    清零
                  </el-button>
                  <el-button 
                    size="small" 
                    @click="setPortValueDirect(port.id, 255)"
                    :disabled="!status.connected"
                  >
                    全置1
                  </el-button>
                </div>
              </div>
              
              <!-- 引脚控制 -->
              <div class="pins-control" v-if="port.enabled">
                <div class="pins-grid">
                  <div 
                    v-for="pin in port.pins" 
                    :key="pin.id"
                    class="pin-item"
                    :class="{ 
                      active: pin.value, 
                      input: pin.direction === 'input',
                      output: pin.direction === 'output',
                      disabled: !pin.enabled 
                    }"
                  >
                    <div class="pin-header">
                      <span class="pin-label">{{ pin.label }}</span>
                      <el-switch
                        v-if="pin.direction === 'output'"
                        v-model="pin.value"
                        @change="onPinValueChange(port.id, pin.id)"
                        :disabled="!pin.enabled || !status.connected"
                        size="small"
                      />
                      <div v-else class="pin-indicator" :class="{ active: pin.value }">
                        {{ formatPinValue(pin.value) }}
                      </div>
                    </div>
                    
                    <div class="pin-config" v-if="showPinConfig">
                      <el-checkbox v-model="pin.enabled" size="small">启用</el-checkbox>
                      <el-checkbox v-model="pin.inverted" size="small">反相</el-checkbox>
                      <el-checkbox 
                        v-model="pin.pullup" 
                        size="small"
                        v-if="pin.direction === 'input'"
                      >
                        上拉
                      </el-checkbox>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </el-card>

        <!-- 全局设置 -->
        <el-card class="control-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Setting /></el-icon>
              <span>全局设置</span>
            </div>
          </template>
          
          <div class="global-settings">
            <div class="setting-item">
              <label>更新频率:</label>
              <el-input-number
                v-model="config.globalSettings.updateRate"
                :min="1"
                :max="1000"
                @change="onGlobalSettingChange"
                size="small"
              />
              <span class="unit">Hz</span>
            </div>
            
            <div class="setting-item">
              <label>防抖时间:</label>
              <el-input-number
                v-model="config.globalSettings.debounceTime"
                :min="0"
                :max="100"
                @change="onGlobalSettingChange"
                size="small"
              />
              <span class="unit">ms</span>
            </div>
            
            <div class="setting-item">
              <label>安全模式:</label>
              <el-switch
                v-model="config.globalSettings.safetyMode"
                @change="onGlobalSettingChange"
              />
            </div>
            
            <div class="setting-item">
              <label>看门狗:</label>
              <el-switch
                v-model="config.globalSettings.watchdogEnabled"
                @change="onGlobalSettingChange"
              />
            </div>
          </div>
        </el-card>
      </div>

      <!-- 右侧显示区域 -->
      <div class="display-panel">
        <!-- 状态显示 -->
        <el-card class="display-card status-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Monitor /></el-icon>
              <span>设备状态</span>
              <div class="header-actions">
                <el-button size="small" @click="showPinConfig = !showPinConfig">
                  <el-icon><Setting /></el-icon>
                  {{ showPinConfig ? '隐藏配置' : '显示配置' }}
                </el-button>
              </div>
            </div>
          </template>
          
          <div class="status-grid">
            <div class="status-item">
              <div class="status-label">连接状态</div>
              <div class="status-value" :class="{ active: status.connected }">
                <el-icon><Link v-if="status.connected" /><CircleClose v-else /></el-icon>
                {{ status.connected ? '已连接' : '未连接' }}
              </div>
            </div>

            <div class="status-item">
              <div class="status-label">设备就绪</div>
              <div class="status-value" :class="{ active: status.deviceReady }">
                <el-icon><CircleCheck v-if="status.deviceReady" /><CircleClose v-else /></el-icon>
                {{ status.deviceReady ? '就绪' : '未就绪' }}
              </div>
            </div>

            <div class="status-item">
              <div class="status-label">总操作数</div>
              <div class="status-value">{{ status.statistics.totalOperations }}</div>
            </div>

            <div class="status-item">
              <div class="status-label">成功率</div>
              <div class="status-value">
                {{ ((status.statistics.successfulOperations / Math.max(status.statistics.totalOperations, 1)) * 100).toFixed(1) }}%
              </div>
            </div>

            <div class="status-item">
              <div class="status-label">平均响应时间</div>
              <div class="status-value">{{ status.statistics.averageResponseTime.toFixed(1) }}ms</div>
            </div>

            <div class="status-item">
              <div class="status-label">设备温度</div>
              <div class="status-value" :class="{ warning: status.hardware.temperature > 60 }">
                {{ status.hardware.temperature.toFixed(1) }}°C
              </div>
            </div>

            <div class="status-item">
              <div class="status-label">电源电压</div>
              <div class="status-value" :class="{ warning: status.hardware.voltage < 4.5 || status.hardware.voltage > 5.5 }">
                {{ status.hardware.voltage.toFixed(2) }}V
              </div>
            </div>

            <div class="status-item">
              <div class="status-label">功耗</div>
              <div class="status-value">{{ status.hardware.powerConsumption.toFixed(1) }}W</div>
            </div>
          </div>
        </el-card>

        <!-- 端口状态可视化 -->
        <el-card class="display-card visualization-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><DataLine /></el-icon>
              <span>端口状态可视化</span>
            </div>
          </template>
          
          <div class="port-visualization">
            <div 
              v-for="port in config.ports.filter(p => p.enabled)" 
              :key="port.id"
              class="port-visual"
            >
              <div class="port-visual-header">
                <h4>{{ port.name }}</h4>
                <span class="port-direction-badge" :class="port.direction">
                  {{ getDIODirectionLabel(port.direction) }}
                </span>
              </div>
              
              <div class="pins-visual">
                <div 
                  v-for="(pin, index) in port.pins" 
                  :key="pin.id"
                  class="pin-visual"
                  :class="{ 
                    active: pin.value, 
                    input: pin.direction === 'input',
                    output: pin.direction === 'output' 
                  }"
                  @click="togglePin(port.id, pin.id)"
                >
                  <div class="pin-visual-label">{{ index }}</div>
                  <div class="pin-visual-indicator"></div>
                  <div class="pin-visual-value">{{ formatPinValue(pin.value) }}</div>
                </div>
              </div>
            </div>
          </div>
        </el-card>

        <!-- 操作日志 -->
        <el-card class="display-card log-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Document /></el-icon>
              <span>操作日志</span>
              <div class="header-actions">
                <el-button size="small" @click="clearLog">
                  <el-icon><Delete /></el-icon>
                  清除
                </el-button>
                <el-button size="small" @click="exportLog">
                  <el-icon><Download /></el-icon>
                  导出
                </el-button>
              </div>
            </div>
          </template>
          
          <div class="log-content">
            <div 
              v-for="(log, index) in operationLog" 
              :key="index"
              class="log-item"
              :class="log.type"
            >
              <span class="log-time">{{ log.timestamp }}</span>
              <span class="log-message">{{ log.message }}</span>
            </div>
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import { 
  Grid, Link, RefreshRight, More, Switch, Timer, Setting, 
  Monitor, DataLine, Document, VideoPlay, VideoPause, Edit, Plus, 
  Delete, Download, CircleCheck, CircleClose
} from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type {
  DIOConfig,
  DIOStatus,
  DIOPin,
  DIOPort,
  RelayChannel,
  TimingSequence
} from '@/types/dio'
import {
  DEFAULT_DIO_CONFIG,
  DIO_DIRECTIONS,
  RELAY_TYPES,
  formatPinValue,
  formatPortValue,
  formatBinaryValue,
  formatRelayState,
  calculatePortValue,
  setPortValue as setPortValueUtil
} from '@/types/dio'

// Props
interface Props {
  width?: number
  height?: number
  config?: Partial<DIOConfig>
}

const props = withDefaults(defineProps<Props>(), {
  width: 1200,
  height: 800,
  config: () => ({})
})

// Emits
const emit = defineEmits<{
  'config-change': [config: DIOConfig]
  'pin-change': [portId: number, pinId: number, value: boolean]
  'port-change': [portId: number, value: number]
  'relay-change': [relayId: number, state: 'open' | 'closed']
  'sequence-start': [sequenceId: string]
  'sequence-complete': [sequenceId: string]
  'error': [error: string]
}>()

// 响应式数据
const config = reactive<DIOConfig>({
  ...DEFAULT_DIO_CONFIG,
  ...props.config
})

const status = reactive<DIOStatus>({
  connected: false,
  deviceReady: false,
  lastUpdate: '',
  errorCode: 0,
  errorMessage: '',
  statistics: {
    totalOperations: 0,
    successfulOperations: 0,
    failedOperations: 0,
    averageResponseTime: 2.5,
    uptime: 0
  },
  hardware: {
    temperature: 28.5,
    voltage: 5.0,
    current: 0.8,
    powerConsumption: 4.0
  },
  relayStatus: {
    totalSwitches: 0,
    activeSwitches: 0,
    faultySwitches: 0,
    averageLifetime: 100000
  }
})

// 控制状态
const connectionChanging = ref(false)
const showPinConfig = ref(false)
const runningSequence = ref<string | null>(null)

// 操作日志
const operationLog = ref<Array<{
  timestamp: string
  message: string
  type: 'info' | 'success' | 'warning' | 'error'
}>>([])

// 定时器
let statusTimer: number | null = null

// 计算属性
const deviceTypeLabel = computed(() => {
  switch (config.deviceType) {
    case 'dio_card': return 'DIO卡'
    case 'switch_matrix': return '开关矩阵'
    case 'relay_card': return '继电器卡'
    default: return '未知设备'
  }
})

// 工具函数
const addLog = (message: string, type: 'info' | 'success' | 'warning' | 'error' = 'info') => {
  operationLog.value.unshift({
    timestamp: new Date().toLocaleTimeString(),
    message,
    type
  })
  
  // 限制日志数量
  if (operationLog.value.length > 100) {
    operationLog.value = operationLog.value.slice(0, 100)
  }
}

const updateStatistics = () => {
  status.statistics.totalOperations++
  status.statistics.successfulOperations++
  status.statistics.averageResponseTime = Math.random() * 5 + 1
}

const getDIODirectionLabel = (direction: string) => {
  const option = DIO_DIRECTIONS.find(d => d.value === direction)
  return option ? option.label : direction
}

const getRelayTypeLabel = (type: string) => {
  const option = RELAY_TYPES.find(t => t.value === type)
  return option ? option.label : type
}

// 方法
const toggleConnection = async () => {
  connectionChanging.value = true
  try {
    await new Promise(resolve => setTimeout(resolve, 1000))
    status.connected = !status.connected
    status.deviceReady = status.connected
    
    addLog(
      status.connected ? '设备连接成功' : '设备已断开连接',
      status.connected ? 'success' : 'info'
    )
    
    emit('config-change', config)
  } catch (error) {
    addLog('连接操作失败', 'error')
    emit('error', '连接操作失败')
  } finally {
    connectionChanging.value = false
  }
}

const resetAllPorts = async () => {
  await ElMessageBox.confirm('确定要复位所有端口吗？', '确认复位', {
    type: 'warning'
  })
  
  config.ports.forEach(port => {
    if (port.direction === 'output') {
      port.value = 0
      port.pins.forEach(pin => {
        if (pin.direction === 'output') {
          pin.value = false
        }
      })
    }
  })
  
  addLog('所有端口已复位', 'success')
  emit('config-change', config)
}

const onPortEnabledChange = (portId: number) => {
  const port = config.ports.find(p => p.id === portId)
  if (port) {
    addLog(`端口 ${port.name} ${port.enabled ? '已启用' : '已禁用'}`, 'info')
    emit('config-change', config)
  }
}

const onPortDirectionChange = (portId: number) => {
  const port = config.ports.find(p => p.id === portId)
  if (port) {
    // 更新所有引脚的方向
    port.pins.forEach(pin => {
      pin.direction = port.direction
    })
    
    addLog(`端口 ${port.name} 方向设置为 ${getDIODirectionLabel(port.direction)}`, 'info')
    emit('config-change', config)
  }
}

const onPortValueChange = (portId: number) => {
  const port = config.ports.find(p => p.id === portId)
  if (port) {
    setPortValueUtil(port.pins, port.value)
    addLog(`端口 ${port.name} 值设置为 ${formatPortValue(port.value, 8)}`, 'success')
    emit('port-change', portId, port.value)
    updateStatistics()
  }
}

const setPortValueDirect = (portId: number, value: number) => {
  const port = config.ports.find(p => p.id === portId)
  if (port) {
    port.value = value
    setPortValueUtil(port.pins, value)
    addLog(`端口 ${port.name} 值设置为 ${formatPortValue(value, 8)}`, 'success')
    emit('port-change', portId, value)
    updateStatistics()
  }
}

const onPinValueChange = (portId: number, pinId: number) => {
  const port = config.ports.find(p => p.id === portId)
  const pin = port?.pins.find(p => p.id === pinId)
  
  if (port && pin) {
    // 更新端口值
    port.value = calculatePortValue(port.pins)
    
    addLog(`引脚 ${pin.label} 设置为 ${formatPinValue(pin.value)}`, 'success')
    emit('pin-change', portId, pinId, pin.value)
    updateStatistics()
  }
}

const togglePin = (portId: number, pinId: number) => {
  const port = config.ports.find(p => p.id === portId)
  const pin = port?.pins.find(p => p.id === pinId)
  
  if (port && pin && pin.direction === 'output' && pin.enabled && status.connected) {
    pin.value = !pin.value
    onPinValueChange(portId, pinId)
  }
}

const onGlobalSettingChange = () => {
  addLog('全局设置已更新', 'info')
  emit('config-change', config)
}

const handleMenuCommand = (command: string) => {
  switch (command) {
    case 'save':
      ElMessage.success('配置已保存')
      break
    case 'load':
      ElMessage.success('配置已加载')
      break
    case 'export':
      exportData()
      break
    case 'test':
      runSelfTest()
      break
  }
}

const exportData = () => {
  const data = {
    config,
    status,
    log: operationLog.value,
    timestamp: new Date().toISOString()
  }
  
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `dio-data-${Date.now()}.json`
  a.click()
  URL.revokeObjectURL(url)
  
  ElMessage.success('数据已导出')
}

const runSelfTest = async () => {
  addLog('开始自检测试...', 'info')
  
  // 模拟自检流程
  const tests = [
    '检查硬件连接',
    '测试端口功能',
    '验证继电器状态',
    '检查电源电压',
    '测试通信接口'
  ]
  
  for (const test of tests) {
    addLog(`正在执行: ${test}`, 'info')
    await new Promise(resolve => setTimeout(resolve, 500))
  }
  
  addLog('自检测试完成，所有功能正常', 'success')
}

const clearLog = () => {
  operationLog.value = []
  addLog('日志已清除', 'info')
}

const exportLog = () => {
  const logText = operationLog.value
    .map(log => `[${log.timestamp}] ${log.type.toUpperCase()}: ${log.message}`)
    .join('\n')
  
  const blob = new Blob([logText], { type: 'text/plain' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `dio-log-${Date.now()}.txt`
  a.click()
  URL.revokeObjectURL(url)
  
  ElMessage.success('日志已导出')
}

// 生命周期
onMounted(() => {
  addLog('DIO控制卡组件已加载', 'info')
  
  // 模拟状态更新
  statusTimer = setInterval(() => {
    if (status.connected) {
      status.hardware.temperature = 28 + Math.random() * 10
      status.hardware.voltage = 4.9 + Math.random() * 0.2
      status.hardware.current = 0.5 + Math.random() * 0.6
      status.hardware.powerConsumption = status.hardware.voltage * status.hardware.current
      status.statistics.uptime += 1
      
      // 模拟输入引脚状态变化
      config.ports.forEach(port => {
        if (port.direction === 'input' && port.enabled) {
          port.pins.forEach(pin => {
            if (pin.direction === 'input' && Math.random() < 0.1) {
              pin.value = Math.random() > 0.5
            }
          })
          port.value = calculatePortValue(port.pins)
        }
      })
    }
  }, 1000)
})

onUnmounted(() => {
  if (statusTimer) {
    clearInterval(statusTimer)
  }
})
</script>

<style lang="scss" scoped>
.dio-card {
  width: 100%;
  height: 100%;
  background: var(--instrument-bg);
  border: 1px solid var(--instrument-border);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.dio-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  background: var(--header-bg);
  border-bottom: 1px solid var(--border-color);
  border-radius: 8px 8px 0 0;

  .header-left {
    .dio-title {
      margin: 0;
      font-size: 18px;
      font-weight: 600;
      color: var(--text-primary);
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .dio-model {
      font-size: 12px;
      color: var(--text-secondary);
      margin-top: 4px;
    }
  }

  .header-right {
    display: flex;
    gap: 8px;
  }
}

.dio-content {
  flex: 1;
  display: grid;
  grid-template-columns: 400px 1fr;
  gap: 16px;
  padding: 16px;
  overflow: hidden;
}

.control-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
  overflow-y: auto;
}

.display-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
  overflow-y: auto;
}

.control-card,
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

.ports-control {
  .port-item {
    border: 1px solid var(--border-color);
    border-radius: 6px;
    padding: 12px;
    margin-bottom: 12px;

    &.disabled {
      opacity: 0.6;
    }

    .port-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;

      .port-label {
        font-weight: 500;
      }

      .port-direction {
        width: 100px;
      }
    }

    .port-value {
      margin-bottom: 12px;

      .value-display {
        display: flex;
        align-items: center;
        gap: 8px;
        margin-bottom: 8px;
        font-family: 'Consolas', monospace;

        .label {
          font-size: 12px;
          color: var(--text-secondary);
        }

        .hex-value {
          background: var(--digital-bg);
          padding: 2px 6px;
          border-radius: 3px;
          font-weight: bold;
          color: var(--digital-value);
        }

        .binary-value {
          background: var(--digital-bg);
          padding: 2px 6px;
          border-radius: 3px;
          font-family: 'Consolas', monospace;
          font-size: 11px;
          color: var(--digital-unit);
        }
      }

      .port-input {
        display: flex;
        gap: 8px;
        align-items: center;
      }
    }

    .pins-control {
      .pins-grid {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 8px;

        .pin-item {
          border: 1px solid var(--border-color);
          border-radius: 4px;
          padding: 8px;
          text-align: center;
          transition: all 0.3s ease;

          &.active {
            background: rgba(16, 185, 129, 0.1);
            border-color: var(--status-normal);
          }

          &.input {
            background: rgba(59, 130, 246, 0.1);
          }

          &.output {
            background: rgba(245, 158, 11, 0.1);
          }

          &.disabled {
            opacity: 0.5;
          }

          .pin-header {
            display: flex;
            flex-direction: column;
            gap: 4px;

            .pin-label {
              font-size: 10px;
              font-weight: 500;
            }

            .pin-indicator {
              font-size: 8px;
              padding: 2px 4px;
              border-radius: 2px;
              background: var(--digital-bg);
              color: var(--digital-unit);

              &.active {
                background: var(--status-normal);
                color: white;
              }
            }
          }

          .pin-config {
            margin-top: 8px;
            display: flex;
            flex-direction: column;
            gap: 4px;
          }
        }
      }
    }
  }
}

.global-settings {
  .setting-item {
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

.status-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;

  .status-item {
    .status-label {
      font-size: 12px;
      color: var(--text-secondary);
      margin-bottom: 4px;
    }

    .status-value {
      font-weight: 500;
      display: flex;
      align-items: center;
      gap: 4px;

      &.active {
        color: var(--status-normal);
      }

      &.warning {
        color: var(--status-warning);
      }
    }
  }
}

.port-visualization {
  .port-visual {
    border: 1px solid var(--border-color);
    border-radius: 6px;
    padding: 12px;
    margin-bottom: 12px;

    .port-visual-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 12px;

      h4 {
        margin: 0;
        font-size: 14px;
        color: var(--text-primary);
      }

      .port-direction-badge {
        padding: 2px 8px;
        border-radius: 12px;
        font-size: 10px;
        font-weight: 500;

        &.input {
          background: rgba(59, 130, 246, 0.2);
          color: #3b82f6;
        }

        &.output {
          background: rgba(245, 158, 11, 0.2);
          color: #f59e0b;
        }

        &.bidirectional {
          background: rgba(139, 69, 19, 0.2);
          color: #8b4513;
        }
      }
    }

    .pins-visual {
      display: grid;
      grid-template-columns: repeat(8, 1fr);
      gap: 4px;

      .pin-visual {
        display: flex;
        flex-direction: column;
        align-items: center;
        padding: 8px 4px;
        border: 1px solid var(--border-color);
        border-radius: 4px;
        cursor: pointer;
        transition: all 0.3s ease;

        &:hover {
          background: rgba(0, 0, 0, 0.05);
        }

        &.active {
          background: rgba(16, 185, 129, 0.2);
          border-color: var(--status-normal);

          .pin-visual-indicator {
            background: var(--status-normal);
          }
        }

        &.input {
          border-left: 3px solid #3b82f6;
        }

        &.output {
          border-left: 3px solid #f59e0b;
        }

        .pin-visual-label {
          font-size: 10px;
          font-weight: 500;
          margin-bottom: 4px;
        }

        .pin-visual-indicator {
          width: 12px;
          height: 12px;
          border-radius: 50%;
          background: var(--border-color);
          margin-bottom: 4px;
          transition: all 0.3s ease;
        }

        .pin-visual-value {
          font-size: 8px;
          color: var(--text-secondary);
        }
      }
    }
  }
}

.log-content {
  max-height: 200px;
  overflow-y: auto;

  .log-item {
    display: flex;
    gap: 8px;
    padding: 4px 0;
    border-bottom: 1px solid var(--border-color);
    font-size: 12px;

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
        color: var(--status-normal);
      }
    }

    &.warning {
      .log-message {
        color: var(--status-warning);
      }
    }

    &.error {
      .log-message {
        color: var(--status-error);
      }
    }
  }
}

// 响应式设计
@media (max-width: 1024px) {
  .dio-content {
    grid-template-columns: 1fr;
  }

  .status-grid {
    grid-template-columns: 1fr;
  }

  .pins-visual {
    grid-template-columns: repeat(4, 1fr) !important;
  }
}
</style>

// 响应式设计
@media (max-width: 768px) {
  .dio-header {
    flex-direction: column;
    gap: 12px;
    align-items: flex-start;

    .header-right {
      width: 100%;
      justify-content: flex-end;
    }
  }

  .pins-grid {
    grid-template-columns: repeat(2, 1fr) !important;
  }

  .pins-visual {
    grid-template-columns: repeat(4, 1fr) !important;
  }
}
