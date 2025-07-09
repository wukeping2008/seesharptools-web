<template>
  <div class="flow-meter-wrapper professional-control">
    <!-- 工具栏 -->
    <div class="flow-meter-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <span class="flow-meter-title">{{ data.title || '流量计' }}</span>
      </div>
      <div class="toolbar-right">
        <el-tooltip content="切换流量类型">
          <el-button size="small" @click="toggleFlowType">
            <el-icon><Switch /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="设置报警">
          <el-button size="small" @click="showAlarmDialog = true">
            <el-icon><Bell /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="累计流量">
          <el-button size="small" @click="showTotalFlow = !showTotalFlow">
            <el-icon><DataAnalysis /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="导出数据">
          <el-button size="small" @click="exportData">
            <el-icon><Download /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>

    <!-- 流量计容器 -->
    <div 
      ref="flowMeterContainer" 
      class="flow-meter-container"
      :style="containerStyle"
    >
      <!-- 流量计主体 -->
      <div class="flow-meter-body" :style="bodyStyle">
        <!-- 管道系统 -->
        <div class="pipe-system" :style="pipeSystemStyle">
          <!-- 入口管道 -->
          <div class="inlet-pipe" :style="inletPipeStyle">
            <div class="pipe-section" :style="pipeStyle">
              <!-- 流动粒子 -->
              <div v-if="localOptions.showFlow && flowEnabled" class="flow-particles">
                <div 
                  v-for="(particle, index) in inletParticles" 
                  :key="`inlet-${index}`"
                  class="flow-particle"
                  :style="getParticleStyle(particle, 'horizontal')"
                ></div>
              </div>
            </div>
            <div class="pipe-arrow" :style="arrowStyle">→</div>
          </div>
          
          <!-- 流量计主体 -->
          <div class="meter-body" :style="meterBodyStyle">
            <!-- 流量计外壳 -->
            <div class="meter-shell" :style="shellStyle">
              <!-- 显示屏 -->
              <div class="display-screen" v-if="localOptions.showDisplay" :style="displayStyle">
                <div class="display-content">
                  <div class="flow-value">
                    <span class="value">{{ formatValue(currentValue) }}</span>
                    <span class="unit">{{ data.unit }}/{{ currentFlowType.timeUnit }}</span>
                  </div>
                  <div class="flow-type">{{ currentFlowType.name }}</div>
                  <div class="total-flow" v-if="showTotalFlow">
                    <span class="label">累计:</span>
                    <span class="total">{{ formatValue(totalFlowValue) }}{{ data.unit }}</span>
                  </div>
                </div>
              </div>
              
              <!-- 流量指示器 -->
              <div class="flow-indicator" v-if="localOptions.showIndicator" :style="indicatorStyle">
                <div class="indicator-bar" :style="indicatorBarStyle">
                  <div class="flow-level" :style="flowLevelStyle"></div>
                </div>
                <div class="indicator-scale" v-if="localOptions.showScale">
                  <div 
                    v-for="(mark, index) in scaleMarks" 
                    :key="index"
                    class="scale-mark"
                    :style="getScaleMarkStyle(mark)"
                  >
                    <div class="mark-line"></div>
                    <div class="mark-label" v-if="mark.showLabel">
                      {{ formatScaleValue(mark.value) }}
                    </div>
                  </div>
                </div>
              </div>
              
              <!-- 流量计内部流动 -->
              <div class="internal-flow" v-if="localOptions.showInternalFlow">
                <div class="flow-chamber" :style="flowChamberStyle">
                  <div 
                    v-for="(particle, index) in internalParticles" 
                    :key="`internal-${index}`"
                    class="internal-particle"
                    :style="getInternalParticleStyle(particle)"
                  ></div>
                </div>
              </div>
              
              <!-- 报警指示 -->
              <div 
                v-for="(zone, index) in alarmZones" 
                :key="index"
                class="alarm-indicator"
                :style="getAlarmIndicatorStyle(zone)"
                :class="{ active: isAlarmActive(zone) }"
                :title="zone.label"
              ></div>
            </div>
            
            <!-- 流量计信息 -->
            <div class="meter-info" v-if="localOptions.showInfo" :style="infoStyle">
              <div class="info-item">
                <span class="label">瞬时流量:</span>
                <span class="value">{{ formatValue(currentValue) }} {{ data.unit }}/{{ currentFlowType.timeUnit }}</span>
              </div>
              <div class="info-item" v-if="showTotalFlow">
                <span class="label">累计流量:</span>
                <span class="value">{{ formatValue(totalFlowValue) }} {{ data.unit }}</span>
              </div>
              <div class="info-item">
                <span class="label">流量类型:</span>
                <span class="value">{{ currentFlowType.name }}</span>
              </div>
            </div>
          </div>
          
          <!-- 出口管道 -->
          <div class="outlet-pipe" :style="outletPipeStyle">
            <div class="pipe-arrow" :style="arrowStyle">→</div>
            <div class="pipe-section" :style="pipeStyle">
              <!-- 流动粒子 -->
              <div v-if="localOptions.showFlow && flowEnabled" class="flow-particles">
                <div 
                  v-for="(particle, index) in outletParticles" 
                  :key="`outlet-${index}`"
                  class="flow-particle"
                  :style="getParticleStyle(particle, 'horizontal')"
                ></div>
              </div>
            </div>
          </div>
        </div>
        
        <!-- 流量计状态 -->
        <div class="meter-status" v-if="localOptions.showStatus" :style="statusStyle">
          <div class="status-item">
            <span class="status-label">当前流量:</span>
            <span class="status-value">{{ formatValue(currentValue) }} {{ data.unit }}/{{ currentFlowType.timeUnit }}</span>
          </div>
          <div class="status-item">
            <span class="status-label">流量类型:</span>
            <span class="status-value">{{ currentFlowType.name }}</span>
          </div>
          <div class="status-item" v-if="showTotalFlow">
            <span class="status-label">累计流量:</span>
            <span class="status-value">{{ formatValue(totalFlowValue) }} {{ data.unit }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 控制面板 -->
    <div class="flow-meter-controls" v-if="showControls">
      <el-row :gutter="16">
        <el-col :span="6">
          <div class="control-group">
            <label>当前流量:</label>
            <el-input-number 
              v-model="currentValue" 
              :min="data.min || 0" 
              :max="data.max || 100"
              :precision="2"
              @change="updateValue"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>流量类型:</label>
            <el-select v-model="currentFlowType" @change="updateFlowType" size="small">
              <el-option 
                v-for="type in flowTypes" 
                :key="type.id"
                :label="type.name" 
                :value="type" 
              />
            </el-select>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>管道直径:</label>
            <el-input-number 
              v-model="localOptions.pipeWidth" 
              :min="20" 
              :max="80"
              @change="updateDisplay"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>流量计尺寸:</label>
            <el-input-number 
              v-model="localOptions.meterSize" 
              :min="100" 
              :max="200"
              @change="updateDisplay"
              size="small"
            />
          </div>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showDisplay" @change="updateDisplay">
            显示屏
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showIndicator" @change="updateDisplay">
            流量指示器
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showFlow" @change="updateDisplay">
            流动效果
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showInternalFlow" @change="updateDisplay">
            内部流动
          </el-checkbox>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showScale" @change="updateDisplay">
            显示刻度
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showInfo" @change="updateDisplay">
            显示信息
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="showTotalFlow" @change="updateDisplay">
            累计流量
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <div class="control-group">
            <label>流动速度:</label>
            <el-slider 
              v-model="localOptions.flowSpeed" 
              :min="1" 
              :max="10"
              @change="updateDisplay"
              size="small"
            />
          </div>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="8">
          <div class="control-group">
            <label>管道颜色:</label>
            <el-color-picker v-model="localOptions.pipeColor" @change="updateDisplay" size="small" />
          </div>
        </el-col>
        
        <el-col :span="8">
          <div class="control-group">
            <label>流体颜色:</label>
            <el-color-picker v-model="currentFlowType.fluidColor" @change="updateDisplay" size="small" />
          </div>
        </el-col>
        
        <el-col :span="8">
          <div class="control-group">
            <label>流量计颜色:</label>
            <el-color-picker v-model="localOptions.meterColor" @change="updateDisplay" size="small" />
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 状态信息 -->
    <div class="flow-meter-status-bar" v-if="showStatus">
      <span>流量类型: {{ currentFlowType.name }}</span>
      <span>瞬时流量: {{ formatValue(currentValue) }} {{ data.unit }}/{{ currentFlowType.timeUnit }}</span>
      <span v-if="showTotalFlow">累计流量: {{ formatValue(totalFlowValue) }} {{ data.unit }}</span>
      <span>流量范围: {{ formatValue(data.min || 0) }} - {{ formatValue(data.max || 100) }} {{ data.unit }}/{{ currentFlowType.timeUnit }}</span>
      <span v-if="currentAlarm">状态: <el-tag :type="currentAlarm.type">{{ currentAlarm.message }}</el-tag></span>
    </div>

    <!-- 报警设置对话框 -->
    <el-dialog v-model="showAlarmDialog" title="流量计报警设置" width="600px">
      <div class="alarm-settings">
        <h4>流量报警区域设置</h4>
        <div v-for="(zone, index) in alarmZones" :key="index" class="alarm-zone-config">
          <el-row :gutter="16">
            <el-col :span="5">
              <el-input-number v-model="zone.min" placeholder="最低流量" size="small" />
            </el-col>
            <el-col :span="5">
              <el-input-number v-model="zone.max" placeholder="最高流量" size="small" />
            </el-col>
            <el-col :span="4">
              <el-color-picker v-model="zone.color" />
            </el-col>
            <el-col :span="6">
              <el-input v-model="zone.label" placeholder="标签" size="small" />
            </el-col>
            <el-col :span="2">
              <el-button size="small" type="danger" @click="removeAlarmZone(index)">
                <el-icon><Delete /></el-icon>
              </el-button>
            </el-col>
          </el-row>
        </div>
        <el-button @click="addAlarmZone" type="primary" size="small">
          <el-icon><Plus /></el-icon>
          添加报警区域
        </el-button>
      </div>
      <template #footer>
        <el-button @click="showAlarmDialog = false">取消</el-button>
        <el-button type="primary" @click="saveAlarmSettings">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { Switch, Bell, Download, Delete, Plus, DataAnalysis } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import type { GaugeData, AlarmZone, GaugeEvents } from '@/types/gauge'

// 流量计配置接口
interface FlowMeterOptions {
  pipeWidth: number
  meterSize: number
  showDisplay: boolean
  showIndicator: boolean
  showFlow: boolean
  showInternalFlow: boolean
  showScale: boolean
  showInfo: boolean
  showStatus: boolean
  pipeColor: string
  meterColor: string
  animation: boolean
  animationDuration: number
  flowSpeed: number
}

// 流量类型接口
interface FlowType {
  id: string
  name: string
  fluidColor: string
  density: number
  viscosity: number
  timeUnit: string
}

// 流动粒子接口
interface FlowParticle {
  id: number
  x: number
  y: number
  size: number
  speed: number
  opacity: number
  direction: number
}

// Props
interface Props {
  data: GaugeData
  options?: Partial<FlowMeterOptions>
  alarmZones?: AlarmZone[]
  events?: GaugeEvents
  showToolbar?: boolean
  showControls?: boolean
  showStatus?: boolean
  width?: string | number
  height?: string | number
}

const props = withDefaults(defineProps<Props>(), {
  showToolbar: true,
  showControls: true,
  showStatus: true,
  width: '500px',
  height: '300px'
})

// Emits
const emit = defineEmits<{
  valueChange: [value: number]
  alarmTrigger: [zone: AlarmZone, value: number]
  flowTypeChange: [type: FlowType]
  totalFlowUpdate: [total: number]
}>()

// 响应式数据
const flowMeterContainer = ref<HTMLDivElement>()
const currentValue = ref(props.data.value)
const showAlarmDialog = ref(false)
const alarmZones = ref<AlarmZone[]>(props.alarmZones || [])
const flowEnabled = ref(true)
const showTotalFlow = ref(false)
const totalFlowValue = ref(0)

// 流动粒子
const inletParticles = ref<FlowParticle[]>([])
const outletParticles = ref<FlowParticle[]>([])
const internalParticles = ref<FlowParticle[]>([])

// 流量类型定义
const flowTypes = ref<FlowType[]>([
  { id: 'water', name: '水流量', fluidColor: '#4fc3f7', density: 1.0, viscosity: 1.0, timeUnit: 's' },
  { id: 'oil', name: '油流量', fluidColor: '#ffb74d', density: 0.8, viscosity: 5.0, timeUnit: 's' },
  { id: 'gas', name: '气体流量', fluidColor: '#81c784', density: 0.001, viscosity: 0.1, timeUnit: 's' },
  { id: 'steam', name: '蒸汽流量', fluidColor: '#e0e0e0', density: 0.6, viscosity: 0.2, timeUnit: 's' },
  { id: 'chemical', name: '化学流量', fluidColor: '#f06292', density: 1.2, viscosity: 3.0, timeUnit: 's' },
  { id: 'air', name: '空气流量', fluidColor: '#90caf9', density: 0.0012, viscosity: 0.018, timeUnit: 's' }
])

const currentFlowType = ref<FlowType>(flowTypes.value[0])

// 默认配置
const defaultOptions: FlowMeterOptions = {
  pipeWidth: 40,
  meterSize: 120,
  showDisplay: true,
  showIndicator: true,
  showFlow: true,
  showInternalFlow: true,
  showScale: true,
  showInfo: true,
  showStatus: true,
  pipeColor: '#666666',
  meterColor: '#e0e0e0',
  animation: true,
  animationDuration: 1000,
  flowSpeed: 5
}

const localOptions = ref<FlowMeterOptions>({ ...defaultOptions, ...props.options })

// 动画定时器
let flowAnimationId: number | null = null
let totalFlowTimer: number | null = null

// 计算属性
const containerStyle = computed(() => ({
  width: typeof props.width === 'number' ? `${props.width}px` : props.width,
  height: typeof props.height === 'number' ? `${props.height}px` : props.height,
  display: 'flex',
  flexDirection: 'column' as const,
  alignItems: 'center',
  justifyContent: 'center'
}))

const bodyStyle = computed(() => ({
  position: 'relative' as const,
  width: '100%',
  height: '100%',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center'
}))

const pipeSystemStyle = computed(() => ({
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  width: '100%',
  height: `${localOptions.value.pipeWidth + 40}px`
}))

const pipeStyle = computed(() => ({
  width: '120px',
  height: `${localOptions.value.pipeWidth}px`,
  backgroundColor: localOptions.value.pipeColor,
  border: '2px solid #333',
  borderRadius: '4px',
  position: 'relative' as const,
  overflow: 'hidden' as const
}))

const inletPipeStyle = computed(() => ({
  display: 'flex',
  alignItems: 'center',
  gap: '8px'
}))

const outletPipeStyle = computed(() => ({
  display: 'flex',
  alignItems: 'center',
  gap: '8px'
}))

const arrowStyle = computed(() => ({
  fontSize: '20px',
  fontWeight: 'bold',
  color: currentFlowType.value.fluidColor
}))

const meterBodyStyle = computed(() => ({
  position: 'relative' as const,
  margin: '0 16px'
}))

const shellStyle = computed(() => ({
  width: `${localOptions.value.meterSize}px`,
  height: `${localOptions.value.meterSize}px`,
  backgroundColor: localOptions.value.meterColor,
  border: '3px solid #333',
  borderRadius: '8px',
  position: 'relative' as const,
  display: 'flex',
  flexDirection: 'column' as const,
  alignItems: 'center',
  justifyContent: 'center'
}))

const displayStyle = computed(() => ({
  position: 'absolute' as const,
  top: '8px',
  left: '8px',
  right: '8px',
  height: '60px',
  backgroundColor: '#000',
  color: '#00ff00',
  borderRadius: '4px',
  padding: '4px 8px',
  fontSize: '12px',
  fontFamily: 'monospace',
  display: 'flex',
  flexDirection: 'column' as const,
  justifyContent: 'center'
}))

const indicatorStyle = computed(() => ({
  position: 'absolute' as const,
  bottom: '8px',
  left: '8px',
  right: '8px',
  height: '30px',
  display: 'flex',
  alignItems: 'center',
  gap: '8px'
}))

const indicatorBarStyle = computed(() => ({
  flex: 1,
  height: '8px',
  backgroundColor: '#ddd',
  borderRadius: '4px',
  position: 'relative' as const,
  overflow: 'hidden' as const
}))

const flowLevelStyle = computed(() => {
  const percentage = ((currentValue.value - (props.data.min || 0)) / ((props.data.max || 100) - (props.data.min || 0))) * 100
  return {
    width: `${Math.max(0, Math.min(100, percentage))}%`,
    height: '100%',
    backgroundColor: currentFlowType.value.fluidColor,
    borderRadius: '4px',
    transition: localOptions.value.animation ? `width ${localOptions.value.animationDuration}ms ease` : 'none'
  }
})

const flowChamberStyle = computed(() => ({
  position: 'absolute' as const,
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  width: '60px',
  height: '60px',
  borderRadius: '50%',
  border: '2px solid #333',
  backgroundColor: 'rgba(255,255,255,0.1)',
  overflow: 'hidden' as const
}))

const infoStyle = computed(() => ({
  position: 'absolute' as const,
  bottom: '-60px',
  left: '50%',
  transform: 'translateX(-50%)',
  textAlign: 'center' as const,
  fontSize: '12px',
  color: '#606266',
  whiteSpace: 'nowrap' as const
}))

const statusStyle = computed(() => ({
  position: 'absolute' as const,
  bottom: '-40px',
  left: '50%',
  transform: 'translateX(-50%)',
  textAlign: 'center' as const,
  fontSize: '12px',
  color: '#606266'
}))

const scaleMarks = computed(() => {
  const min = props.data.min || 0
  const max = props.data.max || 100
  const range = max - min
  const markCount = 5
  
  const marks: Array<{
    value: number
    position: number
    showLabel: boolean
  }> = []
  
  for (let i = 0; i <= markCount; i++) {
    const value = min + (range * i / markCount)
    const position = (i / markCount) * 100
    marks.push({
      value,
      position,
      showLabel: i % 2 === 0
    })
  }
  
  return marks
})

const currentAlarm = computed(() => {
  const value = currentValue.value
  for (const zone of alarmZones.value) {
    if (value >= zone.min && value <= zone.max) {
      return {
        type: 'danger' as const,
        message: zone.label || '流量报警'
      }
    }
  }
  return null
})

// 方法
const getScaleMarkStyle = (mark: any) => ({
  position: 'absolute' as const,
  left: `${mark.position}%`,
  top: '0',
  transform: 'translateX(-50%)',
  display: 'flex',
  flexDirection: 'column' as const,
  alignItems: 'center',
  fontSize: '10px'
})

const getAlarmIndicatorStyle = (zone: AlarmZone) => {
  const min = props.data.min || 0
  const max = props.data.max || 100
  const range = max - min
  
  const startPercent = ((zone.min - min) / range) * 100
  const endPercent = ((zone.max - min) / range) * 100
  const width = endPercent - startPercent
  
  return {
    position: 'absolute' as const,
    top: '2px',
    left: `${startPercent}%`,
    width: `${width}%`,
    height: '4px',
    backgroundColor: zone.color,
    opacity: 0.7,
    borderRadius: '2px'
  }
}

const isAlarmActive = (zone: AlarmZone) => {
  const value = currentValue.value
  return value >= zone.min && value <= zone.max
}

const getParticleStyle = (particle: FlowParticle, direction: 'horizontal' | 'vertical') => ({
  position: 'absolute' as const,
  left: direction === 'horizontal' ? `${particle.x}%` : '50%',
  top: direction === 'horizontal' ? '50%' : `${particle.y}%`,
  transform: direction === 'horizontal' ? 'translateY(-50%)' : 'translateX(-50%)',
  width: `${particle.size}px`,
  height: `${particle.size}px`,
  borderRadius: '50%',
  backgroundColor: currentFlowType.value.fluidColor,
  opacity: particle.opacity,
  animation: `flow-particle-${direction} ${particle.speed}s linear infinite`
})

const getInternalParticleStyle = (particle: FlowParticle) => ({
  position: 'absolute' as const,
  left: `${particle.x}%`,
  top: `${particle.y}%`,
  width: `${particle.size}px`,
  height: `${particle.size}px`,
  borderRadius: '50%',
  backgroundColor: currentFlowType.value.fluidColor,
  opacity: particle.opacity,
  transform: `rotate(${particle.direction}deg)`,
  animation: `internal-flow ${particle.speed}s linear infinite`
})

const formatValue = (value: number) => {
  return value.toFixed(2)
}

const formatScaleValue = (value: number) => {
  return value.toFixed(0)
}

const updateValue = (value: number) => {
  currentValue.value = value
  emit('valueChange', value)
  props.events?.onValueChange?.(value)
  
  // 检查报警
  checkAlarms(value)
}

const updateFlowType = () => {
  emit('flowTypeChange', currentFlowType.value)
  ElMessage.info(`流量类型已更改为: ${currentFlowType.value.name}`)
}

const updateDisplay = () => {
  // 触发重新渲染
}

const toggleFlowType = () => {
  const currentIndex = flowTypes.value.findIndex(type => type.id === currentFlowType.value.id)
  currentFlowType.value = flowTypes.value[(currentIndex + 1) % flowTypes.value.length]
  updateFlowType()
}

const checkAlarms = (value: number) => {
  for (const zone of alarmZones.value) {
    if (value >= zone.min && value <= zone.max) {
      emit('alarmTrigger', zone, value)
      props.events?.onAlarmTrigger?.(zone, value)
      break
    }
  }
}

const addAlarmZone = () => {
  alarmZones.value.push({
    min: 80,
    max: 100,
    color: '#f56c6c',
    label: '流量过高'
  })
}

const removeAlarmZone = (index: number) => {
  alarmZones.value.splice(index, 1)
}

const saveAlarmSettings = () => {
  showAlarmDialog.value = false
  ElMessage.success('流量计报警设置已保存')
}

const exportData = () => {
  const data = {
    current: {
      value: currentValue.value,
      flowType: currentFlowType.value,
      totalFlow: totalFlowValue.value,
      timestamp: new Date()
    },
    range: {
      min: props.data.min || 0,
      max: props.data.max || 100
    },
    options: localOptions.value,
    alarms: alarmZones.value
  }
  
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `flow_meter_data_${Date.now()}.json`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

// 流动粒子动画
const createFlowParticles = () => {
  if (!flowEnabled.value || !localOptions.value.showFlow) return
  
  const particleCount = 6
  
  // 入口粒子
  inletParticles.value = []
  for (let i = 0; i < particleCount; i++) {
    inletParticles.value.push({
      id: i,
      x: Math.random() * 100,
      y: 50,
      size: Math.random() * 4 + 2,
      speed: Math.random() * 2 + 1,
      opacity: Math.random() * 0.6 + 0.4,
      direction: 0
    })
  }
  
  // 出口粒子
  outletParticles.value = []
  for (let i = 0; i < particleCount; i++) {
    outletParticles.value.push({
      id: i,
      x: Math.random() * 100,
      y: 50,
      size: Math.random() * 4 + 2,
      speed: Math.random() * 2 + 1,
      opacity: Math.random() * 0.6 + 0.4,
      direction: 0
    })
  }
  
  // 内部粒子
  internalParticles.value = []
  for (let i = 0; i < 8; i++) {
    internalParticles.value.push({
      id: i,
      x: Math.random() * 80 + 10,
      y: Math.random() * 80 + 10,
      size: Math.random() * 3 + 1,
      speed: Math.random() * 3 + 2,
      opacity: Math.random() * 0.8 + 0.2,
      direction: Math.random() * 360
    })
  }
}

const animateFlowParticles = () => {
  if (!flowEnabled.value) return
  
  // 动画入口粒子
  inletParticles.value.forEach(particle => {
    particle.x += localOptions.value.flowSpeed * 0.5
    if (particle.x > 100) {
      particle.x = -10
    }
  })
  
  // 动画出口粒子
  outletParticles.value.forEach(particle => {
    particle.x += localOptions.value.flowSpeed * 0.5
    if (particle.x > 100) {
      particle.x = -10
    }
  })
  
  // 动画内部粒子
  internalParticles.value.forEach(particle => {
    particle.direction += 2
    particle.x += Math.cos(particle.direction * Math.PI / 180) * 0.5
    particle.y += Math.sin(particle.direction * Math.PI / 180) * 0.5
    
    if (particle.x < 0 || particle.x > 100) particle.x = Math.random() * 100
    if (particle.y < 0 || particle.y > 100) particle.y = Math.random() * 100
  })
  
  flowAnimationId = requestAnimationFrame(animateFlowParticles)
}

// 累计流量计算
const updateTotalFlow = () => {
  if (showTotalFlow.value) {
    const flowRate = currentValue.value / 3600 // 转换为每秒流量
    totalFlowValue.value += flowRate
    emit('totalFlowUpdate', totalFlowValue.value)
  }
}

// 监听数据变化
watch(() => props.data, (newData) => {
  currentValue.value = newData.value
}, { deep: true })

watch(() => showTotalFlow.value, (newValue) => {
  if (newValue) {
    totalFlowTimer = setInterval(updateTotalFlow, 1000)
  } else {
    if (totalFlowTimer) {
      clearInterval(totalFlowTimer)
      totalFlowTimer = null
    }
  }
})

// 生命周期
onMounted(() => {
  createFlowParticles()
  if (localOptions.value.showFlow) {
    animateFlowParticles()
  }
})

onUnmounted(() => {
  if (flowAnimationId) {
    cancelAnimationFrame(flowAnimationId)
  }
  if (totalFlowTimer) {
    clearInterval(totalFlowTimer)
  }
})

// 暴露方法
defineExpose({
  updateValue,
  toggleFlowType,
  exportData,
  getCurrentValue: () => currentValue.value,
  getCurrentFlowType: () => currentFlowType.value,
  getTotalFlow: () => totalFlowValue.value,
  resetTotalFlow: () => { totalFlowValue.value = 0 }
})
</script>

<style lang="scss" scoped>
.flow-meter-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  
  .flow-meter-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    border-bottom: 1px solid #e4e7ed;
    background: #fafafa;
    
    .flow-meter-title {
      font-weight: 500;
      color: #303133;
    }
    
    .toolbar-left,
    .toolbar-right {
      display: flex;
      align-items: center;
      gap: 8px;
    }
  }
  
  .flow-meter-container {
    flex: 1;
    padding: 20px;
    position: relative;
  }
  
  .flow-meter-body {
    .pipe-system {
      .inlet-pipe,
      .outlet-pipe {
        .pipe-section {
          .flow-particles {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            
            .flow-particle {
              border: 1px solid rgba(255,255,255,0.3);
            }
          }
        }
        
        .pipe-arrow {
          text-shadow: 0 0 4px currentColor;
        }
      }
      
      .meter-body {
        .meter-shell {
          box-shadow: 0 4px 12px rgba(0,0,0,0.15);
          
          .display-screen {
            .display-content {
              .flow-value {
                display: flex;
                align-items: baseline;
                gap: 4px;
                margin-bottom: 2px;
                
                .value {
                  font-size: 16px;
                  font-weight: bold;
                }
                
                .unit {
                  font-size: 10px;
                  opacity: 0.8;
                }
              }
              
              .flow-type {
                font-size: 10px;
                opacity: 0.7;
                margin-bottom: 2px;
              }
              
              .total-flow {
                font-size: 9px;
                opacity: 0.6;
                
                .label {
                  margin-right: 4px;
                }
              }
            }
          }
          
          .flow-indicator {
            .indicator-bar {
              .flow-level {
                background: linear-gradient(90deg, transparent, currentColor, transparent);
                animation: flow-pulse 2s ease-in-out infinite;
              }
            }
            
            .indicator-scale {
              .scale-mark {
                .mark-line {
                  width: 1px;
                  height: 8px;
                  background-color: #666;
                }
                
                .mark-label {
                  font-size: 8px;
                  color: #666;
                  margin-top: 2px;
                }
              }
            }
          }
          
          .internal-flow {
            .flow-chamber {
              .internal-particle {
                border: 1px solid rgba(255,255,255,0.5);
              }
            }
          }
          
          .alarm-indicator {
            &.active {
              animation: alarm-blink 1s ease-in-out infinite;
            }
          }
        }
        
        .meter-info {
          .info-item {
            margin-bottom: 4px;
            
            .label {
              font-weight: 500;
              color: #606266;
            }
            
            .value {
              margin-left: 4px;
              color: #303133;
            }
          }
        }
      }
    }
    
    .meter-status {
      .status-item {
        margin-bottom: 4px;
        
        .status-label {
          font-weight: 500;
          color: #606266;
        }
        
        .status-value {
          margin-left: 4px;
          color: #303133;
        }
      }
    }
  }
  
  .flow-meter-controls {
    padding: 16px;
    background: #f5f7fa;
    border-top: 1px solid #e4e7ed;
    
    .control-group {
      display: flex;
      flex-direction: column;
      gap: 4px;
      
      label {
        font-size: 12px;
        color: #606266;
        font-weight: 500;
      }
    }
  }
  
  .flow-meter-status-bar {
    padding: 8px 16px;
    background: #f0f2f5;
    border-top: 1px solid #e4e7ed;
    display: flex;
    flex-wrap: wrap;
    gap: 16px;
    font-size: 12px;
    color: #606266;
  }
  
  .alarm-settings {
    .alarm-zone-config {
      margin-bottom: 12px;
      padding: 12px;
      border: 1px solid #e4e7ed;
      border-radius: 4px;
      background: #fafafa;
    }
    
    h4 {
      margin: 0 0 16px 0;
      color: #303133;
      font-size: 14px;
    }
  }
}

// 流动粒子动画
@keyframes flow-particle-horizontal {
  0% { transform: translateX(-100%) translateY(-50%); }
  100% { transform: translateX(100%) translateY(-50%); }
}

@keyframes flow-particle-vertical {
  0% { transform: translateY(-100%) translateX(-50%); }
  100% { transform: translateY(100%) translateX(-50%); }
}

@keyframes internal-flow {
  0% { transform: rotate(0deg) scale(1); opacity: 0.8; }
  50% { transform: rotate(180deg) scale(1.2); opacity: 0.6; }
  100% { transform: rotate(360deg) scale(1); opacity: 0.8; }
}

@keyframes flow-pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.6; }
}

@keyframes alarm-blink {
  0%, 100% { opacity: 0.7; }
  50% { opacity: 1; }
}

@media (max-width: 768px) {
  .flow-meter-wrapper {
    .flow-meter-toolbar {
      flex-direction: column;
      gap: 8px;
      align-items: flex-start;
      
      .toolbar-left,
      .toolbar-right {
        flex-wrap: wrap;
      }
    }
    
    .flow-meter-controls {
      .el-row {
        .el-col {
          margin-bottom: 12px;
        }
      }
    }
    
    .flow-meter-status-bar {
      flex-direction: column;
      gap: 8px;
    }
  }
}
</style>
