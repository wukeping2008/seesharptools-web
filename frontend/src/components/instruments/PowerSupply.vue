<template>
  <div class="power-supply-wrapper professional-instrument">
    <!-- 电源标题栏 -->
    <div class="power-header">
      <div class="header-left">
        <h3>可编程直流电源</h3>
        <div class="model-info">PSU-3000 Series</div>
      </div>
      <div class="header-right">
        <div class="status-indicators">
          <div class="status-item" :class="{ active: isOutputEnabled }">
            <div class="status-dot"></div>
            <span>OUTPUT</span>
          </div>
          <div class="status-item" :class="{ active: isRemoteMode }">
            <div class="status-dot"></div>
            <span>REMOTE</span>
          </div>
          <div class="status-item" :class="{ active: isProtectionActive }">
            <div class="status-dot protection"></div>
            <span>PROTECT</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 主显示面板 -->
    <div class="display-panel">
      <el-row :gutter="20">
        <!-- 电压显示 -->
        <el-col :span="8">
          <div class="measurement-display voltage">
            <div class="display-header">
              <h4>电压 (V)</h4>
              <div class="mode-indicator" :class="{ active: voltageMode === 'cv' }">CV</div>
            </div>
            <div class="digital-display">
              <div class="main-value">{{ actualVoltage.toFixed(3) }}</div>
              <div class="set-value">设定: {{ setVoltage.toFixed(3) }}</div>
            </div>
            <div class="progress-bar">
              <div 
                class="progress-fill voltage-fill" 
                :style="{ width: `${(actualVoltage / maxVoltage) * 100}%` }"
              ></div>
            </div>
          </div>
        </el-col>

        <!-- 电流显示 -->
        <el-col :span="8">
          <div class="measurement-display current">
            <div class="display-header">
              <h4>电流 (A)</h4>
              <div class="mode-indicator" :class="{ active: currentMode === 'cc' }">CC</div>
            </div>
            <div class="digital-display">
              <div class="main-value">{{ actualCurrent.toFixed(3) }}</div>
              <div class="set-value">设定: {{ setCurrent.toFixed(3) }}</div>
            </div>
            <div class="progress-bar">
              <div 
                class="progress-fill current-fill" 
                :style="{ width: `${(actualCurrent / maxCurrent) * 100}%` }"
              ></div>
            </div>
          </div>
        </el-col>

        <!-- 功率显示 -->
        <el-col :span="8">
          <div class="measurement-display power">
            <div class="display-header">
              <h4>功率 (W)</h4>
              <div class="efficiency">效率: {{ efficiency.toFixed(1) }}%</div>
            </div>
            <div class="digital-display">
              <div class="main-value">{{ actualPower.toFixed(2) }}</div>
              <div class="set-value">最大: {{ maxPower.toFixed(0) }}</div>
            </div>
            <div class="progress-bar">
              <div 
                class="progress-fill power-fill" 
                :style="{ width: `${(actualPower / maxPower) * 100}%` }"
              ></div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 控制面板 -->
    <div class="control-panel">
      <el-row :gutter="16">
        <!-- 电压控制 -->
        <el-col :span="12">
          <div class="control-section">
            <h4>电压控制</h4>
            <div class="control-group">
              <div class="control-item">
                <label>设定电压:</label>
                <el-input-number 
                  v-model="setVoltage" 
                  :min="0" 
                  :max="maxVoltage"
                  :step="0.001"
                  :precision="3"
                  @change="updateVoltage"
                  size="small"
                  style="width: 120px;"
                />
                <span class="unit">V</span>
              </div>
              
              <div class="control-item">
                <label>电压限制:</label>
                <el-input-number 
                  v-model="voltageLimit" 
                  :min="0" 
                  :max="maxVoltage"
                  :step="0.1"
                  :precision="1"
                  @change="updateLimits"
                  size="small"
                  style="width: 120px;"
                />
                <span class="unit">V</span>
              </div>
              
              <div class="control-item">
                <label>上升时间:</label>
                <el-input-number 
                  v-model="voltageRiseTime" 
                  :min="0.001" 
                  :max="10"
                  :step="0.001"
                  :precision="3"
                  @change="updateRiseTime"
                  size="small"
                  style="width: 120px;"
                />
                <span class="unit">s</span>
              </div>
            </div>
          </div>
        </el-col>

        <!-- 电流控制 -->
        <el-col :span="12">
          <div class="control-section">
            <h4>电流控制</h4>
            <div class="control-group">
              <div class="control-item">
                <label>设定电流:</label>
                <el-input-number 
                  v-model="setCurrent" 
                  :min="0" 
                  :max="maxCurrent"
                  :step="0.001"
                  :precision="3"
                  @change="updateCurrent"
                  size="small"
                  style="width: 120px;"
                />
                <span class="unit">A</span>
              </div>
              
              <div class="control-item">
                <label>电流限制:</label>
                <el-input-number 
                  v-model="currentLimit" 
                  :min="0" 
                  :max="maxCurrent"
                  :step="0.01"
                  :precision="2"
                  @change="updateLimits"
                  size="small"
                  style="width: 120px;"
                />
                <span class="unit">A</span>
              </div>
              
              <div class="control-item">
                <label>上升时间:</label>
                <el-input-number 
                  v-model="currentRiseTime" 
                  :min="0.001" 
                  :max="10"
                  :step="0.001"
                  :precision="3"
                  @change="updateRiseTime"
                  size="small"
                  style="width: 120px;"
                />
                <span class="unit">s</span>
              </div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 保护设置 -->
    <div class="protection-panel">
      <h4>保护设置</h4>
      <el-row :gutter="16">
        <el-col :span="6">
          <div class="protection-item">
            <el-checkbox v-model="ovpEnabled" @change="updateProtection">
              过压保护 (OVP)
            </el-checkbox>
            <el-input-number 
              v-model="ovpLevel" 
              :min="0" 
              :max="maxVoltage * 1.2"
              :step="0.1"
              :precision="1"
              :disabled="!ovpEnabled"
              @change="updateProtection"
              size="small"
              style="width: 100px; margin-left: 8px;"
            />
            <span class="unit">V</span>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="protection-item">
            <el-checkbox v-model="ocpEnabled" @change="updateProtection">
              过流保护 (OCP)
            </el-checkbox>
            <el-input-number 
              v-model="ocpLevel" 
              :min="0" 
              :max="maxCurrent * 1.2"
              :step="0.01"
              :precision="2"
              :disabled="!ocpEnabled"
              @change="updateProtection"
              size="small"
              style="width: 100px; margin-left: 8px;"
            />
            <span class="unit">A</span>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="protection-item">
            <el-checkbox v-model="oppEnabled" @change="updateProtection">
              过功率保护 (OPP)
            </el-checkbox>
            <el-input-number 
              v-model="oppLevel" 
              :min="0" 
              :max="maxPower * 1.2"
              :step="1"
              :precision="0"
              :disabled="!oppEnabled"
              @change="updateProtection"
              size="small"
              style="width: 100px; margin-left: 8px;"
            />
            <span class="unit">W</span>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="protection-item">
            <el-checkbox v-model="otpEnabled" @change="updateProtection">
              过温保护 (OTP)
            </el-checkbox>
            <el-input-number 
              v-model="otpLevel" 
              :min="0" 
              :max="100"
              :step="1"
              :precision="0"
              :disabled="!otpEnabled"
              @change="updateProtection"
              size="small"
              style="width: 100px; margin-left: 8px;"
            />
            <span class="unit">°C</span>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 输出控制 -->
    <div class="output-panel">
      <el-row :gutter="16">
        <el-col :span="8">
          <div class="output-controls">
            <el-button 
              :type="isOutputEnabled ? 'danger' : 'success'" 
              @click="toggleOutput"
              size="large"
              :disabled="isProtectionActive"
            >
              <el-icon><Switch /></el-icon>
              {{ isOutputEnabled ? '关闭输出' : '开启输出' }}
            </el-button>
            
            <el-button @click="clearProtection" size="large" v-if="isProtectionActive">
              <el-icon><RefreshRight /></el-icon>
              清除保护
            </el-button>
          </div>
        </el-col>
        
        <el-col :span="8">
          <div class="preset-controls">
            <label>预设:</label>
            <el-select v-model="selectedPreset" @change="loadPreset" size="small">
              <el-option 
                v-for="preset in presets" 
                :key="preset.id"
                :label="preset.name" 
                :value="preset.id" 
              />
            </el-select>
            <el-button @click="savePreset" size="small">保存</el-button>
          </div>
        </el-col>
        
        <el-col :span="8">
          <div class="sequence-controls">
            <el-checkbox v-model="sequenceEnabled" @change="updateSequence">
              序列模式
            </el-checkbox>
            <el-button 
              @click="startSequence" 
              size="small" 
              :disabled="!sequenceEnabled || isSequenceRunning"
            >
              开始序列
            </el-button>
            <el-button 
              @click="stopSequence" 
              size="small" 
              :disabled="!isSequenceRunning"
            >
              停止序列
            </el-button>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 状态栏 -->
    <div class="status-bar">
      <div class="status-left">
        <span>输出: {{ isOutputEnabled ? '开启' : '关闭' }}</span>
        <span>模式: {{ voltageMode === 'cv' ? 'CV' : 'CC' }}</span>
        <span>温度: {{ temperature.toFixed(1) }}°C</span>
      </div>
      <div class="status-right">
        <span>保护: {{ isProtectionActive ? '激活' : '正常' }}</span>
        <span>序列: {{ isSequenceRunning ? '运行中' : '停止' }}</span>
        <span>时间: {{ currentTime }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { Switch, RefreshRight } from '@element-plus/icons-vue'

// Props
interface Props {
  width?: string | number
  height?: string | number
}

const props = withDefaults(defineProps<Props>(), {
  width: '100%',
  height: '700px'
})

// 响应式数据
const isOutputEnabled = ref(false)
const isRemoteMode = ref(true)
const isProtectionActive = ref(false)
const currentTime = ref('')

// 设定值
const setVoltage = ref(12.000)
const setCurrent = ref(1.000)
const voltageLimit = ref(30.0)
const currentLimit = ref(3.0)
const voltageRiseTime = ref(0.001)
const currentRiseTime = ref(0.001)

// 实际值
const actualVoltage = ref(0.000)
const actualCurrent = ref(0.000)
const temperature = ref(25.0)

// 规格参数
const maxVoltage = 30.0
const maxCurrent = 3.0
const maxPower = 90.0

// 保护设置
const ovpEnabled = ref(true)
const ocpEnabled = ref(true)
const oppEnabled = ref(true)
const otpEnabled = ref(true)
const ovpLevel = ref(33.0)
const ocpLevel = ref(3.3)
const oppLevel = ref(100)
const otpLevel = ref(80)

// 预设和序列
const selectedPreset = ref(1)
const sequenceEnabled = ref(false)
const isSequenceRunning = ref(false)

// 预设配置
const presets = [
  { id: 1, name: '预设1 (5V/1A)', voltage: 5.0, current: 1.0 },
  { id: 2, name: '预设2 (12V/0.5A)', voltage: 12.0, current: 0.5 },
  { id: 3, name: '预设3 (24V/0.25A)', voltage: 24.0, current: 0.25 },
  { id: 4, name: '预设4 (3.3V/2A)', voltage: 3.3, current: 2.0 }
]

// 定时器
let updateTimer: number | null = null
let timeTimer: number | null = null

// 计算属性
const actualPower = computed(() => actualVoltage.value * actualCurrent.value)
const efficiency = computed(() => {
  const inputPower = actualPower.value / 0.85 // 假设85%效率
  return inputPower > 0 ? (actualPower.value / inputPower) * 100 : 0
})

const voltageMode = computed(() => {
  return actualVoltage.value >= (setVoltage.value - 0.01) ? 'cv' : 'cc'
})

const currentMode = computed(() => {
  return actualCurrent.value >= (setCurrent.value - 0.001) ? 'cc' : 'cv'
})

// 方法
const updateVoltage = () => {
  if (isOutputEnabled.value) {
    simulateOutput()
  }
}

const updateCurrent = () => {
  if (isOutputEnabled.value) {
    simulateOutput()
  }
}

const updateLimits = () => {
  checkProtection()
}

const updateRiseTime = () => {
  // 上升时间更新逻辑
}

const updateProtection = () => {
  checkProtection()
}

const toggleOutput = () => {
  if (isProtectionActive.value) return
  
  isOutputEnabled.value = !isOutputEnabled.value
  
  if (isOutputEnabled.value) {
    simulateOutput()
  } else {
    actualVoltage.value = 0
    actualCurrent.value = 0
  }
}

const clearProtection = () => {
  isProtectionActive.value = false
}

const loadPreset = () => {
  const preset = presets.find(p => p.id === selectedPreset.value)
  if (preset) {
    setVoltage.value = preset.voltage
    setCurrent.value = preset.current
    updateVoltage()
  }
}

const savePreset = () => {
  // 保存当前设置到预设
  const preset = presets.find(p => p.id === selectedPreset.value)
  if (preset) {
    preset.voltage = setVoltage.value
    preset.current = setCurrent.value
  }
}

const updateSequence = () => {
  if (!sequenceEnabled.value) {
    stopSequence()
  }
}

const startSequence = () => {
  if (!sequenceEnabled.value) return
  isSequenceRunning.value = true
  // 序列执行逻辑
}

const stopSequence = () => {
  isSequenceRunning.value = false
}

const simulateOutput = () => {
  if (!isOutputEnabled.value) return
  
  // 模拟电压上升
  const targetVoltage = Math.min(setVoltage.value, voltageLimit.value)
  const voltageDiff = targetVoltage - actualVoltage.value
  const voltageStep = voltageDiff * 0.1
  
  if (Math.abs(voltageDiff) > 0.001) {
    actualVoltage.value += voltageStep
  } else {
    actualVoltage.value = targetVoltage
  }
  
  // 模拟电流（基于负载阻抗）
  const loadResistance = 10 + Math.random() * 20 // 模拟负载变化
  let targetCurrent = actualVoltage.value / loadResistance
  targetCurrent = Math.min(targetCurrent, setCurrent.value, currentLimit.value)
  
  const currentDiff = targetCurrent - actualCurrent.value
  const currentStep = currentDiff * 0.1
  
  if (Math.abs(currentDiff) > 0.001) {
    actualCurrent.value += currentStep
  } else {
    actualCurrent.value = targetCurrent
  }
  
  // 模拟温度变化
  const powerDissipation = actualPower.value * 0.15 // 15%功率损耗
  const targetTemp = 25 + powerDissipation * 2
  temperature.value += (targetTemp - temperature.value) * 0.05
  
  checkProtection()
}

const checkProtection = () => {
  let protectionTriggered = false
  
  if (ovpEnabled.value && actualVoltage.value > ovpLevel.value) {
    protectionTriggered = true
  }
  
  if (ocpEnabled.value && actualCurrent.value > ocpLevel.value) {
    protectionTriggered = true
  }
  
  if (oppEnabled.value && actualPower.value > oppLevel.value) {
    protectionTriggered = true
  }
  
  if (otpEnabled.value && temperature.value > otpLevel.value) {
    protectionTriggered = true
  }
  
  if (protectionTriggered && !isProtectionActive.value) {
    isProtectionActive.value = true
    isOutputEnabled.value = false
    actualVoltage.value = 0
    actualCurrent.value = 0
  }
}

const updateTime = () => {
  currentTime.value = new Date().toLocaleTimeString()
}

// 生命周期
onMounted(() => {
  updateTime()
  timeTimer = setInterval(updateTime, 1000)
  updateTimer = setInterval(() => {
    if (isOutputEnabled.value) {
      simulateOutput()
    }
  }, 100)
})

onUnmounted(() => {
  if (timeTimer) {
    clearInterval(timeTimer)
  }
  if (updateTimer) {
    clearInterval(updateTimer)
  }
})
</script>

<style lang="scss" scoped>
.power-supply-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #f8f9fa;
  border: 2px solid #dee2e6;
  border-radius: 12px;
  overflow: hidden;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  
  .power-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px 20px;
    background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
    color: white;
    
    .header-left {
      h3 {
        margin: 0;
        font-size: 18px;
        font-weight: 600;
      }
      
      .model-info {
        font-size: 12px;
        opacity: 0.8;
        margin-top: 2px;
      }
    }
    
    .header-right {
      .status-indicators {
        display: flex;
        gap: 16px;
        
        .status-item {
          display: flex;
          align-items: center;
          gap: 6px;
          padding: 4px 8px;
          border-radius: 4px;
          background: rgba(255, 255, 255, 0.1);
          font-size: 12px;
          font-weight: 500;
          
          .status-dot {
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: #6c757d;
            transition: background-color 0.3s;
            
            &.protection {
              background: #ffc107;
            }
          }
          
          &.active .status-dot {
            background: #28a745;
            box-shadow: 0 0 8px rgba(40, 167, 69, 0.6);
            
            &.protection {
              background: #dc3545;
              box-shadow: 0 0 8px rgba(220, 53, 69, 0.6);
            }
          }
        }
      }
    }
  }
  
  .display-panel,
  .control-panel,
  .protection-panel,
  .output-panel {
    padding: 16px 20px;
    background: #ffffff;
    border-bottom: 1px solid #dee2e6;
    
    h4 {
      margin: 0 0 12px 0;
      font-size: 14px;
      font-weight: 600;
      color: #495057;
    }
  }
  
  .measurement-display {
    border: 2px solid #e9ecef;
    border-radius: 8px;
    padding: 16px;
    text-align: center;
    transition: all 0.3s;
    
    &.voltage {
      border-color: #007bff;
      background: rgba(0, 123, 255, 0.02);
    }
    
    &.current {
      border-color: #28a745;
      background: rgba(40, 167, 69, 0.02);
    }
    
    &.power {
      border-color: #ffc107;
      background: rgba(255, 193, 7, 0.02);
    }
    
    .display-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 12px;
      
      h4 {
        margin: 0;
        font-size: 14px;
        font-weight: 600;
      }
      
      .mode-indicator {
        padding: 2px 8px;
        border-radius: 4px;
        font-size: 10px;
        font-weight: bold;
        background: #e9ecef;
        color: #6c757d;
        
        &.active {
          background: #28a745;
          color: white;
        }
      }
      
      .efficiency {
        font-size: 10px;
        color: #6c757d;
      }
    }
    
    .digital-display {
      margin-bottom: 12px;
      
      .main-value {
        font-family: 'Courier New', monospace;
        font-size: 24px;
        font-weight: bold;
        color: #2c3e50;
        line-height: 1;
      }
      
      .set-value {
        font-size: 12px;
        color: #6c757d;
        margin-top: 4px;
      }
    }
    
    .progress-bar {
      height: 6px;
      background: #e9ecef;
      border-radius: 3px;
      overflow: hidden;
      
      .progress-fill {
        height: 100%;
        transition: width 0.3s;
        
        &.voltage-fill {
          background: linear-gradient(90deg, #007bff, #0056b3);
        }
        
        &.current-fill {
          background: linear-gradient(90deg, #28a745, #1e7e34);
        }
        
        &.power-fill {
          background: linear-gradient(90deg, #ffc107, #e0a800);
        }
      }
    }
  }
  
  .control-section {
    .control-group {
      display: flex;
      flex-direction: column;
      gap: 12px;
      
      .control-item {
        display: flex;
        align-items: center;
        gap: 8px;
        
        label {
          min-width: 80px;
          font-size: 12px;
          color: #6c757d;
        }
        
        .unit {
          margin-left: 4px;
          font-size: 12px;
          color: #6c757d;
        }
      }
    }
  }
  
  .protection-panel {
    background: #fff3cd;
    border-color: #ffeaa7;
    
    .protection-item {
      display: flex;
      align-items: center;
      margin-bottom: 8px;
      
      .unit {
        margin-left: 4px;
        font-size: 12px;
        color: #6c757d;
      }
    }
  }
  
  .output-panel {
    .output-controls,
    .preset-controls,
    .sequence-controls {
      display: flex;
      align-items: center;
      gap: 8px;
      
      label {
        font-size: 12px;
        color: #6c757d;
        margin-right: 8px;
      }
    }
  }
  
  .status-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 20px;
    background: #e9ecef;
    font-size: 11px;
    color: #6c757d;
    font-family: 'Courier New', monospace;
    
    .status-left,
    .status-right {
      display: flex;
      gap: 16px;
    }
    
    span {
      white-space: nowrap;
    }
  }
}

// 专业仪器样式
.professional-instrument {
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}

// 响应式设计
@media (max-width: 1200px) {
  .power-supply-wrapper {
    .display-panel,
    .control-panel {
      .measurement-display,
      .control-section {
        margin-bottom: 16px;
      }
    }
  }
}

@media (max-width: 768px) {
  .power-supply-wrapper {
    .power-header {
      flex-direction: column;
      gap: 8px;
      text-align: center;
    }
    
    .display-panel,
    .control-panel,
    .protection-panel,
    .output-panel {
      .measurement-display {
        margin-bottom: 12px;
        
        .digital-display {
          .main-value {
            font-size: 20px;
          }
        }
      }
      
      .control-section {
        .control-group {
          .control-item {
            flex-direction: column;
            align-items: flex-start;
            
            label {
              margin-bottom: 4px;
            }
          }
        }
      }
      
      .protection-item {
        flex-direction: column;
        align-items: flex-start;
        margin-bottom: 12px;
      }
      
      .output-controls,
      .preset-controls,
      .sequence-controls {
        flex-direction: column;
        align-items: flex-start;
      }
    }
    
    .status-bar {
      flex-direction: column;
      gap: 8px;
      
      .status-left,
      .status-right {
        justify-content: center;
        flex-wrap: wrap;
      }
    }
  }
}
</style>
