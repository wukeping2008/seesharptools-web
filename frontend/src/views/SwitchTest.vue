<template>
  <div class="switch-test example-page">
    <div class="page-header">
      <h1>开关控件测试</h1>
      <p class="description">
        测试专业开关控件的各种功能和样式
      </p>
    </div>

    <!-- 基础开关测试 -->
    <div class="example-section">
      <h2 class="section-title">基础开关测试</h2>
      <el-row :gutter="20">
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>现代风格开关</span>
                <el-button size="small" @click="toggleModernSwitch">
                  <el-icon><Switch /></el-icon>
                  切换状态
                </el-button>
              </div>
            </template>
            <div class="switch-container">
              <ProfessionalSwitch
                :data="modernSwitchData"
                :options="modernSwitchOptions"
                @value-change="handleModernSwitchChange"
                @switch-click="handleSwitchClick"
              />
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>工业风格开关</span>
                <el-button size="small" @click="toggleIndustrialSwitch">
                  <el-icon><Setting /></el-icon>
                  切换状态
                </el-button>
              </div>
            </template>
            <div class="switch-container">
              <ProfessionalSwitch
                :data="industrialSwitchData"
                :options="industrialSwitchOptions"
                @value-change="handleIndustrialSwitchChange"
              />
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>可控制开关</span>
                <el-button size="small" @click="toggleControlSwitch">
                  <el-icon><Tools /></el-icon>
                  切换状态
                </el-button>
              </div>
            </template>
            <div class="switch-container">
              <ProfessionalSwitch
                :data="controlSwitchData"
                :options="controlSwitchOptions"
                @value-change="handleControlSwitchChange"
              />
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 样式展示 -->
    <div class="example-section">
      <h2 class="section-title">样式展示</h2>
      <el-row :gutter="20">
        <el-col :span="6" v-for="(style, index) in switchStyles" :key="index">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>{{ style.name }}</span>
                <el-tag :type="style.data.value ? 'success' : 'info'" size="small">
                  {{ style.data.value ? 'ON' : 'OFF' }}
                </el-tag>
              </div>
            </template>
            <div class="switch-container">
              <ProfessionalSwitch
                :data="style.data"
                :options="style.options"
                @value-change="(value) => handleStyleSwitchChange(index, value)"
              />
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 尺寸展示 -->
    <div class="example-section">
      <h2 class="section-title">尺寸展示</h2>
      <el-card>
        <template #header>
          <div class="card-header">
            <span>不同尺寸的开关</span>
            <div class="header-controls">
              <el-button size="small" @click="toggleAllSizes">
                <el-icon><Switch /></el-icon>
                全部切换
              </el-button>
              <el-button size="small" @click="resetAllSizes">
                <el-icon><Refresh /></el-icon>
                重置
              </el-button>
            </div>
          </div>
        </template>
        
        <div class="size-showcase">
          <div v-for="(size, index) in sizeVariants" :key="index" class="size-item">
            <div class="size-label">{{ size.label }}</div>
            <ProfessionalSwitch
              :data="size.data"
              :options="size.options"
              @value-change="(value) => handleSizeChange(index, value)"
            />
            <div class="size-info">{{ size.options.size }}px</div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 功能测试 -->
    <div class="example-section">
      <h2 class="section-title">功能测试</h2>
      <el-row :gutter="20">
        <el-col :span="12">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>交互功能测试</span>
                <el-button size="small" @click="enableSoundFeedback">
                  <el-icon><VideoPlay /></el-icon>
                  {{ soundEnabled ? '关闭音效' : '开启音效' }}
                </el-button>
              </div>
            </template>
            
            <div class="function-test">
              <div class="test-item">
                <span class="test-label">音效反馈:</span>
                <ProfessionalSwitch
                  :data="{ value: soundTestSwitch, title: '音效测试' }"
                  :options="{ 
                    soundEnabled: true, 
                    hapticFeedback: true,
                    showStatusLight: true,
                    size: 50
                  }"
                  @value-change="handleSoundTestChange"
                />
              </div>
              
              <div class="test-item">
                <span class="test-label">禁用状态:</span>
                <ProfessionalSwitch
                  :data="{ value: false, title: '禁用开关' }"
                  :options="{ 
                    disabled: true,
                    size: 50,
                    showStatusLight: true
                  }"
                />
              </div>
              
              <div class="test-item">
                <span class="test-label">无动画:</span>
                <ProfessionalSwitch
                  :data="{ value: noAnimationSwitch, title: '无动画' }"
                  :options="{ 
                    animation: false,
                    size: 50,
                    showStatusLight: true
                  }"
                  @value-change="handleNoAnimationChange"
                />
              </div>
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="12">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>状态监控</span>
                <el-button size="small" @click="clearLog">
                  <el-icon><Delete /></el-icon>
                  清除日志
                </el-button>
              </div>
            </template>
            
            <div class="status-monitor">
              <div class="monitor-stats">
                <div class="stat-item">
                  <span class="stat-label">总切换次数:</span>
                  <span class="stat-value">{{ totalSwitches }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">开启状态:</span>
                  <span class="stat-value">{{ onSwitches }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">关闭状态:</span>
                  <span class="stat-value">{{ offSwitches }}</span>
                </div>
              </div>
              
              <div class="event-log">
                <div class="log-header">事件日志:</div>
                <div class="log-content">
                  <div 
                    v-for="(log, index) in eventLogs" 
                    :key="index" 
                    class="log-entry"
                    :class="{ 'log-on': log.value, 'log-off': !log.value }"
                  >
                    <span class="log-time">{{ log.time }}</span>
                    <span class="log-action">{{ log.action }}</span>
                    <span class="log-value">{{ log.value ? 'ON' : 'OFF' }}</span>
                  </div>
                </div>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { Switch, Setting, Tools, VideoPlay, Refresh, Delete } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import ProfessionalSwitch from '@/components/professional/controls/Switch.vue'

// 基础开关数据
const modernSwitchData = ref({
  value: false,
  title: '现代开关'
})

const modernSwitchOptions = ref({
  style: 'modern' as const,
  size: 50,
  showLabels: true,
  showStatusLight: true,
  animation: true
})

const industrialSwitchData = ref({
  value: true,
  title: '工业开关'
})

const industrialSwitchOptions = ref({
  style: 'industrial' as const,
  size: 55,
  showLabels: true,
  showStatusLight: true,
  onColor: '#059669',
  animation: true
})

const controlSwitchData = ref({
  value: false,
  title: '控制开关'
})

const controlSwitchOptions = ref({
  style: 'classic' as const,
  size: 45,
  showLabels: true,
  showStatusLight: true,
  showControls: true,
  animation: true
})

// 样式展示数据
const switchStyles = ref([
  {
    name: '现代风格',
    data: { value: true, title: '现代' },
    options: { style: 'modern' as const, size: 40, showStatusLight: true }
  },
  {
    name: '经典风格',
    data: { value: false, title: '经典' },
    options: { style: 'classic' as const, size: 40, showStatusLight: true }
  },
  {
    name: '工业风格',
    data: { value: true, title: '工业' },
    options: { style: 'industrial' as const, size: 40, showStatusLight: true }
  },
  {
    name: '极简风格',
    data: { value: false, title: '极简' },
    options: { style: 'minimal' as const, size: 40, showStatusLight: true }
  }
])

// 尺寸展示数据
const sizeVariants = ref([
  {
    label: '小型',
    data: { value: false, title: '小型开关' },
    options: { size: 25, showStatusLight: true }
  },
  {
    label: '标准',
    data: { value: true, title: '标准开关' },
    options: { size: 40, showStatusLight: true }
  },
  {
    label: '大型',
    data: { value: false, title: '大型开关' },
    options: { size: 60, showStatusLight: true }
  },
  {
    label: '超大',
    data: { value: true, title: '超大开关' },
    options: { size: 80, showStatusLight: true }
  }
])

// 功能测试数据
const soundTestSwitch = ref(false)
const noAnimationSwitch = ref(false)
const soundEnabled = ref(false)

// 状态监控
const totalSwitches = ref(0)
const eventLogs = ref<Array<{
  time: string
  action: string
  value: boolean
}>>([])

// 计算属性
const onSwitches = computed(() => 
  eventLogs.value.filter(log => log.value).length
)

const offSwitches = computed(() => 
  eventLogs.value.filter(log => !log.value).length
)

// 方法
const toggleModernSwitch = () => {
  modernSwitchData.value.value = !modernSwitchData.value.value
  logEvent('现代开关切换', modernSwitchData.value.value)
}

const toggleIndustrialSwitch = () => {
  industrialSwitchData.value.value = !industrialSwitchData.value.value
  logEvent('工业开关切换', industrialSwitchData.value.value)
}

const toggleControlSwitch = () => {
  controlSwitchData.value.value = !controlSwitchData.value.value
  logEvent('控制开关切换', controlSwitchData.value.value)
}

const handleModernSwitchChange = (value: boolean) => {
  ElMessage.info(`现代开关状态: ${value ? 'ON' : 'OFF'}`)
  logEvent('现代开关变更', value)
}

const handleIndustrialSwitchChange = (value: boolean) => {
  ElMessage.info(`工业开关状态: ${value ? 'ON' : 'OFF'}`)
  logEvent('工业开关变更', value)
}

const handleControlSwitchChange = (value: boolean) => {
  ElMessage.info(`控制开关状态: ${value ? 'ON' : 'OFF'}`)
  logEvent('控制开关变更', value)
}

const handleSwitchClick = (value: boolean) => {
  ElMessage.success(`开关被点击: ${value ? 'ON' : 'OFF'}`)
}

const handleStyleSwitchChange = (index: number, value: boolean) => {
  switchStyles.value[index].data.value = value
  logEvent(`${switchStyles.value[index].name}切换`, value)
}

const handleSizeChange = (index: number, value: boolean) => {
  sizeVariants.value[index].data.value = value
  logEvent(`${sizeVariants.value[index].label}开关切换`, value)
}

const handleSoundTestChange = (value: boolean) => {
  soundTestSwitch.value = value
  logEvent('音效测试开关', value)
}

const handleNoAnimationChange = (value: boolean) => {
  noAnimationSwitch.value = value
  logEvent('无动画开关', value)
}

const toggleAllSizes = () => {
  const newState = !sizeVariants.value[0].data.value
  sizeVariants.value.forEach((size, index) => {
    size.data.value = newState
    logEvent(`${size.label}开关批量切换`, newState)
  })
}

const resetAllSizes = () => {
  sizeVariants.value.forEach((size, index) => {
    size.data.value = false
    logEvent(`${size.label}开关重置`, false)
  })
}

const enableSoundFeedback = () => {
  soundEnabled.value = !soundEnabled.value
  ElMessage.info(`音效反馈${soundEnabled.value ? '已开启' : '已关闭'}`)
}

const logEvent = (action: string, value: boolean) => {
  totalSwitches.value++
  const now = new Date()
  const time = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}:${now.getSeconds().toString().padStart(2, '0')}`
  
  eventLogs.value.unshift({
    time,
    action,
    value
  })
  
  // 限制日志条数
  if (eventLogs.value.length > 20) {
    eventLogs.value = eventLogs.value.slice(0, 20)
  }
}

const clearLog = () => {
  eventLogs.value = []
  totalSwitches.value = 0
  ElMessage.success('日志已清除')
}
</script>

<style lang="scss" scoped>
.switch-test {
  .switch-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 120px;
    background: #f5f7fa;
    border-radius: 8px;
    margin: 16px 0;
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
  
  .size-showcase {
    display: flex;
    justify-content: space-around;
    align-items: center;
    flex-wrap: wrap;
    gap: 30px;
    padding: 20px;
    
    .size-item {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 12px;
      
      .size-label {
        font-size: 14px;
        font-weight: 600;
        color: #374151;
      }
      
      .size-info {
        font-size: 12px;
        color: #6b7280;
        background: #f3f4f6;
        padding: 4px 8px;
        border-radius: 4px;
      }
    }
  }
  
  .function-test {
    .test-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 0;
      border-bottom: 1px solid #e5e7eb;
      
      &:last-child {
        border-bottom: none;
      }
      
      .test-label {
        font-size: 14px;
        font-weight: 500;
        color: #374151;
      }
    }
  }
  
  .status-monitor {
    .monitor-stats {
      display: flex;
      justify-content: space-around;
      margin-bottom: 20px;
      padding: 16px;
      background: #f9fafb;
      border-radius: 8px;
      
      .stat-item {
        text-align: center;
        
        .stat-label {
          display: block;
          font-size: 12px;
          color: #6b7280;
          margin-bottom: 4px;
        }
        
        .stat-value {
          font-size: 18px;
          font-weight: bold;
          color: #374151;
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
        max-height: 200px;
        overflow-y: auto;
        border: 1px solid #e5e7eb;
        border-radius: 6px;
        
        .log-entry {
          display: flex;
          justify-content: space-between;
          align-items: center;
          padding: 8px 12px;
          border-bottom: 1px solid #f3f4f6;
          font-size: 12px;
          
          &:last-child {
            border-bottom: none;
          }
          
          &.log-on {
            background-color: #f0fdf4;
            
            .log-value {
              color: #16a34a;
              font-weight: bold;
            }
          }
          
          &.log-off {
            background-color: #fef2f2;
            
            .log-value {
              color: #dc2626;
              font-weight: bold;
            }
          }
          
          .log-time {
            color: #6b7280;
            font-family: monospace;
          }
          
          .log-action {
            color: #374151;
            flex: 1;
            text-align: center;
          }
          
          .log-value {
            font-weight: 600;
          }
        }
      }
    }
  }
}

@media (max-width: 768px) {
  .switch-test {
    .size-showcase {
      flex-direction: column;
      gap: 20px;
    }
    
    .function-test {
      .test-item {
        flex-direction: column;
        gap: 12px;
        align-items: flex-start;
      }
    }
  }
}
</style>
