<template>
  <div class="button-array-test example-page">
    <div class="page-header">
      <h1>按钮阵列控件测试</h1>
      <p class="description">
        测试专业按钮阵列控件的各种功能和布局
      </p>
    </div>

    <!-- 基础按钮阵列测试 -->
    <div class="example-section">
      <h2 class="section-title">基础按钮阵列测试</h2>
      <el-row :gutter="20">
        <el-col :span="12">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>现代风格按钮阵列</span>
                <div class="header-controls">
                  <el-button size="small" @click="toggleRandomModern">
                    <el-icon><Refresh /></el-icon>
                    随机状态
                  </el-button>
                  <el-button size="small" @click="resetModern">
                    <el-icon><Close /></el-icon>
                    重置
                  </el-button>
                </div>
              </div>
            </template>
            <div class="array-container">
              <ButtonArray
                :data="modernArrayData"
                :options="modernArrayOptions"
                @button-click="handleModernButtonClick"
                @array-change="handleModernArrayChange"
                @status-change="handleModernStatusChange"
              />
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="12">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>工业风格按钮阵列</span>
                <div class="header-controls">
                  <el-button size="small" @click="toggleRandomIndustrial">
                    <el-icon><Setting /></el-icon>
                    随机状态
                  </el-button>
                  <el-button size="small" @click="resetIndustrial">
                    <el-icon><Close /></el-icon>
                    重置
                  </el-button>
                </div>
              </div>
            </template>
            <div class="array-container">
              <ButtonArray
                :data="industrialArrayData"
                :options="industrialArrayOptions"
                @button-click="handleIndustrialButtonClick"
                @array-change="handleIndustrialArrayChange"
              />
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 形状和样式展示 -->
    <div class="example-section">
      <h2 class="section-title">形状和样式展示</h2>
      <el-row :gutter="20">
        <el-col :span="8" v-for="(config, index) in shapeConfigs" :key="index">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>{{ config.name }}</span>
                <el-tag :type="getActiveButtonsCount(config.data.buttons) > 0 ? 'success' : 'info'" size="small">
                  {{ getActiveButtonsCount(config.data.buttons) }}/{{ config.data.buttons.length }}
                </el-tag>
              </div>
            </template>
            <div class="array-container">
              <ButtonArray
                :data="config.data"
                :options="config.options"
                @button-click="(button, idx) => handleShapeButtonClick(index, button, idx)"
              />
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 功能按钮阵列 -->
    <div class="example-section">
      <h2 class="section-title">功能按钮阵列</h2>
      <el-row :gutter="20">
        <el-col :span="16">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>设备控制面板</span>
                <div class="header-controls">
                  <el-button size="small" @click="emergencyStop" type="danger">
                    <el-icon><Warning /></el-icon>
                    紧急停止
                  </el-button>
                  <el-button size="small" @click="systemReset" type="primary">
                    <el-icon><Refresh /></el-icon>
                    系统重置
                  </el-button>
                </div>
              </div>
            </template>
            <div class="array-container">
              <ButtonArray
                :data="controlPanelData"
                :options="controlPanelOptions"
                @button-click="handleControlButtonClick"
                @status-change="handleControlStatusChange"
              />
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>状态监控</span>
                <el-button size="small" @click="clearEventLog">
                  <el-icon><Delete /></el-icon>
                  清除日志
                </el-button>
              </div>
            </template>
            
            <div class="status-monitor">
              <div class="monitor-stats">
                <div class="stat-item">
                  <span class="stat-label">激活设备:</span>
                  <span class="stat-value">{{ controlStatus.activeCount }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">总设备数:</span>
                  <span class="stat-value">{{ controlStatus.totalCount }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">系统状态:</span>
                  <span class="stat-value" :class="systemStatusClass">{{ systemStatus }}</span>
                </div>
              </div>
              
              <div class="event-log">
                <div class="log-header">操作日志:</div>
                <div class="log-content">
                  <div 
                    v-for="(log, index) in eventLogs" 
                    :key="index" 
                    class="log-entry"
                    :class="getLogEntryClass(log)"
                  >
                    <span class="log-time">{{ log.time }}</span>
                    <span class="log-device">{{ log.device }}</span>
                    <span class="log-action">{{ log.action }}</span>
                  </div>
                </div>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 可控制按钮阵列 -->
    <div class="example-section">
      <h2 class="section-title">可控制按钮阵列</h2>
      <el-card>
        <template #header>
          <div class="card-header">
            <span>交互式按钮阵列</span>
            <div class="header-controls">
              <el-button size="small" @click="addButton">
                <el-icon><Plus /></el-icon>
                添加按钮
              </el-button>
              <el-button size="small" @click="removeButton" :disabled="customArrayData.buttons.length <= 1">
                <el-icon><Minus /></el-icon>
                删除按钮
              </el-button>
            </div>
          </div>
        </template>
        
        <div class="array-container">
          <ButtonArray
            :data="customArrayData"
            :options="customArrayOptions"
            @button-click="handleCustomButtonClick"
            @array-change="handleCustomArrayChange"
          />
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { Refresh, Close, Setting, Warning, Delete, Plus, Minus } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import ButtonArray from '@/components/professional/controls/ButtonArray.vue'

// 现代风格按钮阵列
const modernArrayData = ref({
  title: '现代控制面板',
  buttons: [
    { id: 1, label: '电源', active: false, icon: 'el-icon-switch-button' },
    { id: 2, label: '网络', active: true, icon: 'el-icon-connection' },
    { id: 3, label: '蓝牙', active: false, icon: 'el-icon-bluetooth' },
    { id: 4, label: 'WiFi', active: true, icon: 'el-icon-wifi' },
    { id: 5, label: '声音', active: false, icon: 'el-icon-microphone' },
    { id: 6, label: '显示', active: true, icon: 'el-icon-monitor' },
    { id: 7, label: '存储', active: false, icon: 'el-icon-folder' },
    { id: 8, label: '安全', active: false, icon: 'el-icon-lock' }
  ]
})

const modernArrayOptions = ref({
  style: 'modern' as const,
  shape: 'rounded' as const,
  columns: 4,
  buttonSize: 60,
  gap: 12,
  showStatusIndicator: true,
  showStatusInfo: true,
  glowEffect: true,
  animation: true,
  multiSelect: true
})

// 工业风格按钮阵列
const industrialArrayData = ref({
  title: '工业设备控制',
  buttons: [
    { id: 1, label: '泵1', active: false, type: 'toggle' as const },
    { id: 2, label: '泵2', active: true, type: 'toggle' as const },
    { id: 3, label: '阀门A', active: false, type: 'toggle' as const },
    { id: 4, label: '阀门B', active: true, type: 'toggle' as const },
    { id: 5, label: '加热器', active: false, type: 'toggle' as const },
    { id: 6, label: '冷却器', active: false, type: 'toggle' as const }
  ]
})

const industrialArrayOptions = ref({
  style: 'industrial' as const,
  shape: 'square' as const,
  columns: 3,
  buttonSize: 70,
  gap: 15,
  showStatusIndicator: true,
  showStatusInfo: true,
  glowEffect: true,
  animation: true,
  multiSelect: true
})

// 形状配置
const shapeConfigs = ref([
  {
    name: '圆形按钮',
    data: {
      title: '圆形',
      buttons: [
        { id: 1, label: 'A', active: false },
        { id: 2, label: 'B', active: true },
        { id: 3, label: 'C', active: false },
        { id: 4, label: 'D', active: false }
      ]
    },
    options: {
      style: 'classic' as const,
      shape: 'circle' as const,
      columns: 2,
      buttonSize: 50,
      gap: 10
    }
  },
  {
    name: '方形按钮',
    data: {
      title: '方形',
      buttons: [
        { id: 1, label: '1', active: true },
        { id: 2, label: '2', active: false },
        { id: 3, label: '3', active: true },
        { id: 4, label: '4', active: false }
      ]
    },
    options: {
      style: 'minimal' as const,
      shape: 'square' as const,
      columns: 2,
      buttonSize: 50,
      gap: 10
    }
  },
  {
    name: '圆角方形',
    data: {
      title: '圆角',
      buttons: [
        { id: 1, label: 'ON', active: false },
        { id: 2, label: 'OFF', active: true },
        { id: 3, label: 'AUTO', active: false },
        { id: 4, label: 'MAN', active: false }
      ]
    },
    options: {
      style: 'modern' as const,
      shape: 'rounded' as const,
      columns: 2,
      buttonSize: 50,
      gap: 10
    }
  }
])

// 控制面板数据
const controlPanelData = ref({
  title: '设备控制中心',
  buttons: [
    { id: 1, label: '主电机', active: false, type: 'toggle' as const },
    { id: 2, label: '副电机', active: false, type: 'toggle' as const },
    { id: 3, label: '输送带', active: false, type: 'toggle' as const },
    { id: 4, label: '压缩机', active: false, type: 'toggle' as const },
    { id: 5, label: '照明', active: true, type: 'toggle' as const },
    { id: 6, label: '通风', active: false, type: 'toggle' as const },
    { id: 7, label: '报警', active: false, type: 'indicator' as const, disabled: false },
    { id: 8, label: '维护', active: false, type: 'indicator' as const, disabled: true }
  ]
})

const controlPanelOptions = ref({
  style: 'industrial' as const,
  shape: 'rounded' as const,
  columns: 4,
  buttonSize: 65,
  gap: 12,
  showStatusIndicator: true,
  showStatusInfo: true,
  glowEffect: true,
  animation: true,
  multiSelect: true,
  soundEnabled: true,
  hapticFeedback: true
})

// 自定义按钮阵列
const customArrayData = ref({
  title: '自定义按钮阵列',
  buttons: [
    { id: 1, label: '按钮1', active: false },
    { id: 2, label: '按钮2', active: false },
    { id: 3, label: '按钮3', active: false },
    { id: 4, label: '按钮4', active: false }
  ]
})

const customArrayOptions = ref({
  style: 'modern' as const,
  shape: 'rounded' as const,
  columns: 4,
  buttonSize: 55,
  gap: 10,
  showStatusIndicator: true,
  showStatusInfo: true,
  showControls: true,
  glowEffect: true,
  animation: true,
  multiSelect: true
})

// 状态数据
const controlStatus = ref({
  activeCount: 1,
  totalCount: 8
})

const eventLogs = ref<Array<{
  time: string
  device: string
  action: string
  type: 'start' | 'stop' | 'error' | 'warning'
}>>([])

const systemStatus = computed(() => {
  const activeCount = controlStatus.value.activeCount
  if (activeCount === 0) return '待机'
  if (activeCount <= 2) return '运行'
  if (activeCount <= 4) return '高负载'
  return '满负载'
})

const systemStatusClass = computed(() => ({
  'status-standby': systemStatus.value === '待机',
  'status-running': systemStatus.value === '运行',
  'status-high-load': systemStatus.value === '高负载',
  'status-full-load': systemStatus.value === '满负载'
}))

// 方法
const getActiveButtonsCount = (buttons: any[]) => {
  return buttons.filter(btn => btn.active).length
}

const getLogEntryClass = (log: any) => ({
  [`log-${log.type}`]: true
})

const handleModernButtonClick = (button: any, index: number) => {
  ElMessage.info(`现代面板: ${button.label} ${button.active ? '开启' : '关闭'}`)
}

const handleModernArrayChange = (activeButtons: any[]) => {
  console.log('现代面板激活按钮:', activeButtons)
}

const handleModernStatusChange = (status: any) => {
  console.log('现代面板状态:', status)
}

const handleIndustrialButtonClick = (button: any, index: number) => {
  ElMessage.info(`工业设备: ${button.label} ${button.active ? '启动' : '停止'}`)
  logEvent(button.label, button.active ? '启动' : '停止', button.active ? 'start' : 'stop')
}

const handleIndustrialArrayChange = (activeButtons: any[]) => {
  console.log('工业设备激活:', activeButtons)
}

const handleShapeButtonClick = (configIndex: number, button: any, buttonIndex: number) => {
  const configName = shapeConfigs.value[configIndex].name
  ElMessage.info(`${configName}: ${button.label} ${button.active ? '激活' : '取消'}`)
}

const handleControlButtonClick = (button: any, index: number) => {
  ElMessage.success(`设备控制: ${button.label} ${button.active ? '启动' : '停止'}`)
  logEvent(button.label, button.active ? '启动' : '停止', button.active ? 'start' : 'stop')
}

const handleControlStatusChange = (status: any) => {
  controlStatus.value = {
    activeCount: status.activeCount,
    totalCount: status.totalCount
  }
}

const handleCustomButtonClick = (button: any, index: number) => {
  ElMessage.info(`自定义按钮: ${button.label} ${button.active ? '激活' : '取消'}`)
}

const handleCustomArrayChange = (activeButtons: any[]) => {
  console.log('自定义阵列激活按钮:', activeButtons)
}

const toggleRandomModern = () => {
  modernArrayData.value.buttons.forEach(button => {
    button.active = Math.random() > 0.5
  })
  ElMessage.success('现代面板状态已随机化')
}

const resetModern = () => {
  modernArrayData.value.buttons.forEach(button => {
    button.active = false
  })
  ElMessage.info('现代面板已重置')
}

const toggleRandomIndustrial = () => {
  industrialArrayData.value.buttons.forEach(button => {
    button.active = Math.random() > 0.5
  })
  ElMessage.success('工业设备状态已随机化')
}

const resetIndustrial = () => {
  industrialArrayData.value.buttons.forEach(button => {
    button.active = false
  })
  ElMessage.info('工业设备已重置')
}

const emergencyStop = () => {
  controlPanelData.value.buttons.forEach(button => {
    if (!button.disabled) {
      button.active = false
    }
  })
  logEvent('系统', '紧急停止', 'error')
  ElMessage.error('紧急停止已执行！')
}

const systemReset = () => {
  controlPanelData.value.buttons.forEach(button => {
    button.active = false
  })
  // 重新启动照明
  controlPanelData.value.buttons[4].active = true
  logEvent('系统', '重置完成', 'warning')
  ElMessage.warning('系统已重置')
}

const addButton = () => {
  const newId = customArrayData.value.buttons.length + 1
  customArrayData.value.buttons.push({
    id: newId,
    label: `按钮${newId}`,
    active: false
  })
  ElMessage.success(`已添加按钮${newId}`)
}

const removeButton = () => {
  if (customArrayData.value.buttons.length > 1) {
    const removed = customArrayData.value.buttons.pop()
    ElMessage.info(`已删除${removed?.label}`)
  }
}

const logEvent = (device: string, action: string, type: 'start' | 'stop' | 'error' | 'warning') => {
  const now = new Date()
  const time = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}:${now.getSeconds().toString().padStart(2, '0')}`
  
  eventLogs.value.unshift({
    time,
    device,
    action,
    type
  })
  
  // 限制日志条数
  if (eventLogs.value.length > 15) {
    eventLogs.value = eventLogs.value.slice(0, 15)
  }
}

const clearEventLog = () => {
  eventLogs.value = []
  ElMessage.success('事件日志已清除')
}
</script>

<style lang="scss" scoped>
.button-array-test {
  .array-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 200px;
    background: #f5f7fa;
    border-radius: 8px;
    margin: 16px 0;
    padding: 20px;
  }
  
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    
    .header-controls {
      display: flex;
      gap: 8px;
    }
  }
  
  .status-monitor {
    .monitor-stats {
      display: flex;
      flex-direction: column;
      gap: 12px;
      margin-bottom: 20px;
      padding: 16px;
      background: #f9fafb;
      border-radius: 8px;
      
      .stat-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        
        .stat-label {
          font-size: 12px;
          color: #6b7280;
        }
        
        .stat-value {
          font-size: 14px;
          font-weight: bold;
          color: #374151;
          
          &.status-standby {
            color: #6b7280;
          }
          
          &.status-running {
            color: #059669;
          }
          
          &.status-high-load {
            color: #d97706;
          }
          
          &.status-full-load {
            color: #dc2626;
          }
        }
      }
    }
    
    .event-log {
      .log-header {
        font-size: 14px;
        font-weight: 600;
        color: #374151;
        margin-bottom: 12px;
      }
      
      .log-content {
        max-height: 250px;
        overflow-y: auto;
        border: 1px solid #e5e7eb;
        border-radius: 6px;
        
        .log-entry {
          display: flex;
          justify-content: space-between;
          align-items: center;
          padding: 8px 12px;
          border-bottom: 1px solid #f3f4f6;
          font-size: 11px;
          
          &:last-child {
            border-bottom: none;
          }
          
          &.log-start {
            background-color: #f0fdf4;
            border-left: 3px solid #22c55e;
          }
          
          &.log-stop {
            background-color: #fef2f2;
            border-left: 3px solid #ef4444;
          }
          
          &.log-error {
            background-color: #fef2f2;
            border-left: 3px solid #dc2626;
          }
          
          &.log-warning {
            background-color: #fffbeb;
            border-left: 3px solid #f59e0b;
          }
          
          .log-time {
            color: #6b7280;
            font-family: monospace;
            min-width: 60px;
          }
          
          .log-device {
            color: #374151;
            font-weight: 500;
            flex: 1;
            text-align: center;
          }
          
          .log-action {
            color: #374151;
            font-weight: 600;
            min-width: 60px;
            text-align: right;
          }
        }
      }
    }
  }
}

@media (max-width: 768px) {
  .button-array-test {
    .array-container {
      padding: 10px;
      min-height: 150px;
    }
    
    .card-header {
      flex-direction: column;
      gap: 8px;
      align-items: stretch;
      
      .header-controls {
        justify-content: center;
      }
    }
    
    .status-monitor {
      .monitor-stats {
        .stat-item {
          flex-direction: column;
          gap: 4px;
          text-align: center;
        }
      }
      
      .event-log {
        .log-content {
          .log-entry {
            flex-direction: column;
            gap: 4px;
            text-align: center;
            
            .log-time,
            .log-device,
            .log-action {
              min-width: auto;
              text-align: center;
            }
          }
        }
      }
    }
  }
}
</style>
