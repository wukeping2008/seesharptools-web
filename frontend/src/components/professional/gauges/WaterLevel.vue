<template>
  <div class="water-level-wrapper professional-control">
    <!-- 工具栏 -->
    <div class="water-level-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <span class="water-level-title">{{ data.title || '液位计' }}</span>
      </div>
      <div class="toolbar-right">
        <el-tooltip content="切换液体类型">
          <el-button size="small" @click="toggleLiquidType">
            <el-icon><Switch /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="设置报警">
          <el-button size="small" @click="showAlarmDialog = true">
            <el-icon><Bell /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="波动效果">
          <el-button size="small" @click="toggleWaveEffect">
            <el-icon><MagicStick /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="导出数据">
          <el-button size="small" @click="exportData">
            <el-icon><Download /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>

    <!-- 液位计容器 -->
    <div 
      ref="waterLevelContainer" 
      class="water-level-container"
      :style="containerStyle"
    >
      <!-- 液位计主体 -->
      <div class="water-level-body" :style="bodyStyle">
        <!-- 容器外壳 -->
        <div class="tank-shell" :style="shellStyle">
          <!-- 刻度标识 -->
          <div class="level-scale" v-if="localOptions.showScale">
            <div 
              v-for="(mark, index) in scaleMarks" 
              :key="index"
              class="scale-mark"
              :style="getScaleMarkStyle(mark)"
            >
              <div class="mark-line"></div>
              <div class="mark-label" v-if="mark.showLabel">
                {{ formatScaleValue(mark.value) }}{{ data.unit }}
              </div>
            </div>
          </div>
          
          <!-- 液体区域 -->
          <div class="liquid-area" :style="liquidAreaStyle">
            <!-- 液体主体 -->
            <div class="liquid-body" :style="liquidBodyStyle">
              <!-- 波浪效果 -->
              <div 
                v-if="localOptions.showWave && waveEnabled" 
                class="wave-effect"
                :style="waveStyle"
              >
                <div class="wave wave1" :style="wave1Style"></div>
                <div class="wave wave2" :style="wave2Style"></div>
                <div class="wave wave3" :style="wave3Style"></div>
              </div>
              
              <!-- 气泡效果 -->
              <div v-if="localOptions.showBubbles && bubblesEnabled" class="bubbles-container">
                <div 
                  v-for="(bubble, index) in bubbles" 
                  :key="index"
                  class="bubble"
                  :style="getBubbleStyle(bubble)"
                ></div>
              </div>
            </div>
            
            <!-- 液位指示线 -->
            <div 
              v-if="localOptions.showLevelLine" 
              class="level-indicator"
              :style="levelIndicatorStyle"
            ></div>
          </div>
          
          <!-- 报警区域指示 -->
          <div 
            v-for="(zone, index) in alarmZones" 
            :key="index"
            class="alarm-zone-indicator"
            :style="getAlarmZoneStyle(zone)"
            :title="zone.label"
          ></div>
          
          <!-- 容量百分比显示 -->
          <div class="capacity-display" v-if="localOptions.showPercentage" :style="capacityDisplayStyle">
            <div class="percentage-text">{{ formatPercentage(currentPercentage) }}%</div>
            <div class="capacity-text" v-if="localOptions.showCapacity">
              {{ formatValue(currentValue) }}/{{ formatValue(data.max || 100) }}{{ data.unit }}
            </div>
          </div>
        </div>
        
        <!-- 液位数值显示 -->
        <div class="level-value-display" v-if="localOptions.showValue" :style="valueDisplayStyle">
          <div class="current-level">
            <span class="level-label">当前液位:</span>
            <span class="level-value">{{ formatValue(currentValue) }}{{ data.unit }}</span>
          </div>
          <div class="liquid-type" v-if="localOptions.showLiquidType">
            <span class="type-label">液体类型:</span>
            <span class="type-value">{{ currentLiquidType.name }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 控制面板 -->
    <div class="water-level-controls" v-if="showControls">
      <el-row :gutter="16">
        <el-col :span="6">
          <div class="control-group">
            <label>当前液位:</label>
            <el-input-number 
              v-model="currentValue" 
              :min="data.min || 0" 
              :max="data.max || 100"
              :precision="1"
              @change="updateValue"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>液体类型:</label>
            <el-select v-model="currentLiquidType" @change="updateLiquidType" size="small">
              <el-option 
                v-for="type in liquidTypes" 
                :key="type.id"
                :label="type.name" 
                :value="type" 
              />
            </el-select>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>容器高度:</label>
            <el-input-number 
              v-model="localOptions.height" 
              :min="200" 
              :max="500"
              @change="updateDisplay"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>容器宽度:</label>
            <el-input-number 
              v-model="localOptions.width" 
              :min="100" 
              :max="300"
              @change="updateDisplay"
              size="small"
            />
          </div>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showScale" @change="updateDisplay">
            显示刻度
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showWave" @change="updateDisplay">
            波浪效果
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showBubbles" @change="updateDisplay">
            气泡效果
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showPercentage" @change="updateDisplay">
            显示百分比
          </el-checkbox>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="6">
          <el-checkbox v-model="localOptions.animation" @change="updateDisplay">
            启用动画
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showLevelLine" @change="updateDisplay">
            液位指示线
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <div class="control-group">
            <label>容器颜色:</label>
            <el-color-picker v-model="localOptions.tankColor" @change="updateDisplay" size="small" />
          </div>
        </el-col>
        <el-col :span="6">
          <div class="control-group">
            <label>波浪速度:</label>
            <el-slider 
              v-model="localOptions.waveSpeed" 
              :min="1" 
              :max="10"
              @change="updateDisplay"
              size="small"
            />
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 状态信息 -->
    <div class="water-level-status" v-if="showStatus">
      <span>当前液位: {{ formatValue(currentValue) }}{{ data.unit }}</span>
      <span>容量百分比: {{ formatPercentage(currentPercentage) }}%</span>
      <span>液体类型: {{ currentLiquidType.name }}</span>
      <span>容器容量: {{ formatValue(data.max || 100) }}{{ data.unit }}</span>
      <span v-if="currentAlarm">状态: <el-tag :type="currentAlarm.type">{{ currentAlarm.message }}</el-tag></span>
    </div>

    <!-- 报警设置对话框 -->
    <el-dialog v-model="showAlarmDialog" title="液位报警设置" width="600px">
      <div class="alarm-settings">
        <h4>液位报警区域设置</h4>
        <div v-for="(zone, index) in alarmZones" :key="index" class="alarm-zone-config">
          <el-row :gutter="16">
            <el-col :span="5">
              <el-input-number v-model="zone.min" placeholder="最低液位" size="small" />
            </el-col>
            <el-col :span="5">
              <el-input-number v-model="zone.max" placeholder="最高液位" size="small" />
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
import { Switch, Bell, Download, Delete, Plus, MagicStick } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import type { GaugeData, AlarmZone, GaugeEvents } from '@/types/gauge'

// 液位计配置接口
interface WaterLevelOptions {
  height: number
  width: number
  showScale: boolean
  showWave: boolean
  showBubbles: boolean
  showPercentage: boolean
  showCapacity: boolean
  showValue: boolean
  showLiquidType: boolean
  showLevelLine: boolean
  tankColor: string
  animation: boolean
  animationDuration: number
  waveSpeed: number
}

// 液体类型接口
interface LiquidType {
  id: string
  name: string
  color: string
  density: number
  viscosity: number
}

// 气泡接口
interface Bubble {
  id: number
  x: number
  y: number
  size: number
  speed: number
  opacity: number
}

// Props
interface Props {
  data: GaugeData
  options?: Partial<WaterLevelOptions>
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
  width: '300px',
  height: '400px'
})

// Emits
const emit = defineEmits<{
  valueChange: [value: number]
  alarmTrigger: [zone: AlarmZone, value: number]
  liquidTypeChange: [type: LiquidType]
}>()

// 响应式数据
const waterLevelContainer = ref<HTMLDivElement>()
const currentValue = ref(props.data.value)
const showAlarmDialog = ref(false)
const alarmZones = ref<AlarmZone[]>(props.alarmZones || [])
const waveEnabled = ref(true)
const bubblesEnabled = ref(true)
const bubbles = ref<Bubble[]>([])

// 液体类型定义
const liquidTypes = ref<LiquidType[]>([
  { id: 'water', name: '水', color: '#4fc3f7', density: 1.0, viscosity: 1.0 },
  { id: 'oil', name: '油', color: '#ffb74d', density: 0.8, viscosity: 5.0 },
  { id: 'acid', name: '酸', color: '#f06292', density: 1.2, viscosity: 1.5 },
  { id: 'alcohol', name: '酒精', color: '#81c784', density: 0.79, viscosity: 1.2 },
  { id: 'mercury', name: '汞', color: '#90a4ae', density: 13.6, viscosity: 1.5 },
  { id: 'gasoline', name: '汽油', color: '#ffcc02', density: 0.75, viscosity: 0.6 }
])

const currentLiquidType = ref<LiquidType>(liquidTypes.value[0])

// 默认配置
const defaultOptions: WaterLevelOptions = {
  height: 300,
  width: 150,
  showScale: true,
  showWave: true,
  showBubbles: true,
  showPercentage: true,
  showCapacity: true,
  showValue: true,
  showLiquidType: true,
  showLevelLine: true,
  tankColor: '#e0e0e0',
  animation: true,
  animationDuration: 1000,
  waveSpeed: 3
}

const localOptions = ref<WaterLevelOptions>({ ...defaultOptions, ...props.options })

// 动画定时器
let waveAnimationId: number | null = null
let bubbleAnimationId: number | null = null

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
  width: `${localOptions.value.width}px`,
  height: `${localOptions.value.height}px`,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center'
}))

const shellStyle = computed(() => ({
  width: '100%',
  height: '100%',
  border: `3px solid ${localOptions.value.tankColor}`,
  borderRadius: '8px',
  position: 'relative' as const,
  overflow: 'hidden' as const,
  backgroundColor: '#f5f5f5'
}))

const currentPercentage = computed(() => {
  const min = props.data.min || 0
  const max = props.data.max || 100
  return ((currentValue.value - min) / (max - min)) * 100
})

const liquidAreaStyle = computed(() => ({
  position: 'absolute' as const,
  bottom: '0',
  left: '0',
  width: '100%',
  height: `${Math.max(0, Math.min(100, currentPercentage.value))}%`,
  transition: localOptions.value.animation ? `height ${localOptions.value.animationDuration}ms ease` : 'none'
}))

const liquidBodyStyle = computed(() => ({
  width: '100%',
  height: '100%',
  backgroundColor: currentLiquidType.value.color,
  position: 'relative' as const,
  overflow: 'hidden' as const
}))

const levelIndicatorStyle = computed(() => ({
  position: 'absolute' as const,
  top: '0',
  left: '0',
  width: '100%',
  height: '2px',
  backgroundColor: '#fff',
  boxShadow: '0 0 4px rgba(0,0,0,0.3)',
  zIndex: 10
}))

const capacityDisplayStyle = computed(() => ({
  position: 'absolute' as const,
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  textAlign: 'center' as const,
  color: '#333',
  fontWeight: 'bold',
  zIndex: 5,
  backgroundColor: 'rgba(255,255,255,0.8)',
  padding: '8px 12px',
  borderRadius: '4px',
  backdropFilter: 'blur(2px)'
}))

const valueDisplayStyle = computed(() => ({
  position: 'absolute' as const,
  bottom: '-60px',
  left: '50%',
  transform: 'translateX(-50%)',
  textAlign: 'center' as const,
  fontSize: '14px',
  color: '#606266'
}))

const scaleMarks = computed(() => {
  const min = props.data.min || 0
  const max = props.data.max || 100
  const range = max - min
  const markCount = 10
  
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
      position: 100 - position, // 从底部开始
      showLabel: i % 2 === 0 // 每隔一个显示标签
    })
  }
  
  return marks
})

const waveStyle = computed(() => ({
  position: 'absolute' as const,
  top: '-20px',
  left: '0',
  width: '100%',
  height: '40px',
  overflow: 'hidden' as const
}))

const wave1Style = computed(() => ({
  position: 'absolute' as const,
  top: '0',
  left: '0',
  width: '200%',
  height: '100%',
  background: `linear-gradient(90deg, transparent, ${currentLiquidType.value.color}aa, transparent)`,
  borderRadius: '50%',
  animation: `wave1 ${10 / localOptions.value.waveSpeed}s linear infinite`
}))

const wave2Style = computed(() => ({
  position: 'absolute' as const,
  top: '5px',
  left: '0',
  width: '200%',
  height: '100%',
  background: `linear-gradient(90deg, transparent, ${currentLiquidType.value.color}66, transparent)`,
  borderRadius: '50%',
  animation: `wave2 ${8 / localOptions.value.waveSpeed}s linear infinite reverse`
}))

const wave3Style = computed(() => ({
  position: 'absolute' as const,
  top: '10px',
  left: '0',
  width: '200%',
  height: '100%',
  background: `linear-gradient(90deg, transparent, ${currentLiquidType.value.color}44, transparent)`,
  borderRadius: '50%',
  animation: `wave3 ${12 / localOptions.value.waveSpeed}s linear infinite`
}))

const currentAlarm = computed(() => {
  const value = currentValue.value
  for (const zone of alarmZones.value) {
    if (value >= zone.min && value <= zone.max) {
      return {
        type: 'danger' as const,
        message: zone.label || '液位报警'
      }
    }
  }
  return null
})

// 方法
const getScaleMarkStyle = (mark: any) => ({
  position: 'absolute' as const,
  right: '100%',
  top: `${mark.position}%`,
  transform: 'translateY(-50%)',
  display: 'flex',
  alignItems: 'center',
  gap: '4px'
})

const getAlarmZoneStyle = (zone: AlarmZone) => {
  const min = props.data.min || 0
  const max = props.data.max || 100
  const range = max - min
  
  const startPercent = ((zone.min - min) / range) * 100
  const endPercent = ((zone.max - min) / range) * 100
  const height = endPercent - startPercent
  
  return {
    position: 'absolute' as const,
    left: '0',
    bottom: `${startPercent}%`,
    width: '4px',
    height: `${height}%`,
    backgroundColor: zone.color,
    opacity: 0.7,
    borderRadius: '2px'
  }
}

const getBubbleStyle = (bubble: Bubble) => ({
  position: 'absolute' as const,
  left: `${bubble.x}%`,
  bottom: `${bubble.y}%`,
  width: `${bubble.size}px`,
  height: `${bubble.size}px`,
  borderRadius: '50%',
  backgroundColor: 'rgba(255,255,255,0.6)',
  opacity: bubble.opacity,
  animation: `bubble-rise ${bubble.speed}s linear infinite`
})

const formatValue = (value: number) => {
  return value.toFixed(1)
}

const formatPercentage = (value: number) => {
  return value.toFixed(1)
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

const updateLiquidType = () => {
  emit('liquidTypeChange', currentLiquidType.value)
  ElMessage.info(`液体类型已更改为: ${currentLiquidType.value.name}`)
}

const updateDisplay = () => {
  // 触发重新渲染
}

const toggleLiquidType = () => {
  const currentIndex = liquidTypes.value.findIndex(type => type.id === currentLiquidType.value.id)
  currentLiquidType.value = liquidTypes.value[(currentIndex + 1) % liquidTypes.value.length]
  updateLiquidType()
}

const toggleWaveEffect = () => {
  waveEnabled.value = !waveEnabled.value
  ElMessage.info(`波浪效果已${waveEnabled.value ? '开启' : '关闭'}`)
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
    label: '液位过高'
  })
}

const removeAlarmZone = (index: number) => {
  alarmZones.value.splice(index, 1)
}

const saveAlarmSettings = () => {
  showAlarmDialog.value = false
  ElMessage.success('液位报警设置已保存')
}

const exportData = () => {
  const data = {
    current: {
      value: currentValue.value,
      percentage: currentPercentage.value,
      liquidType: currentLiquidType.value,
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
  link.download = `water_level_data_${Date.now()}.json`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

// 气泡动画
const createBubbles = () => {
  if (!bubblesEnabled.value || !localOptions.value.showBubbles) return
  
  const bubbleCount = 5
  bubbles.value = []
  
  for (let i = 0; i < bubbleCount; i++) {
    bubbles.value.push({
      id: i,
      x: Math.random() * 80 + 10, // 10-90%
      y: 0,
      size: Math.random() * 6 + 4, // 4-10px
      speed: Math.random() * 3 + 2, // 2-5s
      opacity: Math.random() * 0.5 + 0.3 // 0.3-0.8
    })
  }
}

const animateBubbles = () => {
  if (!bubblesEnabled.value) return
  
  bubbles.value.forEach(bubble => {
    bubble.y += 0.5
    if (bubble.y > 100) {
      bubble.y = 0
      bubble.x = Math.random() * 80 + 10
    }
  })
  
  bubbleAnimationId = requestAnimationFrame(animateBubbles)
}

// 监听数据变化
watch(() => props.data, (newData) => {
  currentValue.value = newData.value
}, { deep: true })

// 生命周期
onMounted(() => {
  createBubbles()
  if (localOptions.value.showBubbles) {
    animateBubbles()
  }
})

onUnmounted(() => {
  if (waveAnimationId) {
    cancelAnimationFrame(waveAnimationId)
  }
  if (bubbleAnimationId) {
    cancelAnimationFrame(bubbleAnimationId)
  }
})

// 暴露方法
defineExpose({
  updateValue,
  toggleLiquidType,
  exportData,
  getCurrentValue: () => currentValue.value,
  getCurrentPercentage: () => currentPercentage.value,
  getCurrentLiquidType: () => currentLiquidType.value
})
</script>

<style lang="scss" scoped>
.water-level-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  
  .water-level-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    border-bottom: 1px solid #e4e7ed;
    background: #fafafa;
    
    .water-level-title {
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
  
  .water-level-container {
    flex: 1;
    padding: 20px;
    position: relative;
  }
  
  .water-level-body {
    .tank-shell {
      .level-scale {
        .scale-mark {
          .mark-line {
            width: 12px;
            height: 1px;
            background-color: #606266;
          }
          
          .mark-label {
            font-size: 12px;
            color: #606266;
            white-space: nowrap;
            margin-right: 4px;
          }
        }
      }
      
      .liquid-area {
        .liquid-body {
          .wave-effect {
            .wave {
              transform-origin: center;
            }
          }
          
          .bubbles-container {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            
            .bubble {
              border: 1px solid rgba(255,255,255,0.3);
            }
          }
        }
        
        .level-indicator {
          &::before {
            content: '';
            position: absolute;
            left: -8px;
            top: -2px;
            width: 0;
            height: 0;
            border-left: 6px solid #fff;
            border-top: 3px solid transparent;
            border-bottom: 3px solid transparent;
          }
        }
      }
      
      .alarm-zone-indicator {
        &::after {
          content: '';
          position: absolute;
          right: -8px;
          top: 50%;
          transform: translateY(-50%);
          width: 0;
          height: 0;
          border-right: 6px solid currentColor;
          border-top: 3px solid transparent;
          border-bottom: 3px solid transparent;
        }
      }
      
      .capacity-display {
        .percentage-text {
          font-size: 18px;
          font-weight: bold;
          color: #303133;
        }
        
        .capacity-text {
          font-size: 12px;
          color: #909399;
          margin-top: 4px;
        }
      }
    }
    
    .level-value-display {
      .current-level,
      .liquid-type {
        margin-bottom: 4px;
        
        .level-label,
        .type-label {
          font-weight: 500;
          color: #606266;
        }
        
        .level-value,
        .type-value {
          margin-left: 4px;
          color: #303133;
        }
      }
    }
  }
  
  .water-level-controls {
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
  
  .water-level-status {
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

// 波浪动画
@keyframes wave1 {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(0%); }
}

@keyframes wave2 {
  0% { transform: translateX(0%); }
  100% { transform: translateX(-100%); }
}

@keyframes wave3 {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(0%); }
}

// 气泡上升动画
@keyframes bubble-rise {
  0% {
    transform: translateY(0) scale(1);
    opacity: 0.8;
  }
  50% {
    transform: translateY(-50px) scale(1.1);
    opacity: 0.6;
  }
  100% {
    transform: translateY(-100px) scale(0.8);
    opacity: 0;
  }
}

@media (max-width: 768px) {
  .water-level-wrapper {
    .water-level-toolbar {
      flex-direction: column;
      gap: 8px;
      align-items: flex-start;
      
      .toolbar-left,
      .toolbar-right {
        flex-wrap: wrap;
      }
    }
    
    .water-level-controls {
      .el-row {
        .el-col {
          margin-bottom: 12px;
        }
      }
    }
    
    .water-level-status {
      flex-direction: column;
      gap: 8px;
    }
  }
}
</style>
