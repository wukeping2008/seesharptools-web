<template>
  <div class="tank-wrapper professional-control">
    <!-- 工具栏 -->
    <div class="tank-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <span class="tank-title">{{ data.title || '储罐' }}</span>
      </div>
      <div class="toolbar-right">
        <el-tooltip content="切换储罐类型">
          <el-button size="small" @click="toggleTankType">
            <el-icon><Switch /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="设置报警">
          <el-button size="small" @click="showAlarmDialog = true">
            <el-icon><Bell /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="3D视图">
          <el-button size="small" @click="toggle3DView">
            <el-icon><View /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="导出数据">
          <el-button size="small" @click="exportData">
            <el-icon><Download /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>

    <!-- 储罐容器 -->
    <div 
      ref="tankContainer" 
      class="tank-container"
      :style="containerStyle"
    >
      <!-- 储罐主体 -->
      <div class="tank-body" :style="bodyStyle">
        <!-- 储罐外壳 -->
        <div class="tank-shell" :style="shellStyle" :class="{ 'view-3d': is3DView }">
          <!-- 顶部盖子 -->
          <div class="tank-top" :style="topStyle">
            <div class="tank-inlet" v-if="localOptions.showInlet">
              <div class="inlet-pipe"></div>
              <div class="inlet-valve" :class="{ active: inletActive }"></div>
            </div>
            <div class="tank-vent" v-if="localOptions.showVent">
              <div class="vent-pipe"></div>
            </div>
          </div>
          
          <!-- 储罐壁 -->
          <div class="tank-wall" :style="wallStyle">
            <!-- 刻度标识 -->
            <div class="tank-scale" v-if="localOptions.showScale">
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
                <!-- 液面波浪效果 -->
                <div 
                  v-if="localOptions.showWave && waveEnabled" 
                  class="wave-surface"
                  :style="waveStyle"
                >
                  <div class="wave wave1" :style="wave1Style"></div>
                  <div class="wave wave2" :style="wave2Style"></div>
                </div>
                
                <!-- 液体流动效果 -->
                <div v-if="localOptions.showFlow && flowEnabled" class="flow-particles">
                  <div 
                    v-for="(particle, index) in flowParticles" 
                    :key="index"
                    class="flow-particle"
                    :style="getParticleStyle(particle)"
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
            
            <!-- 储罐信息显示 -->
            <div class="tank-info" v-if="localOptions.showInfo" :style="infoDisplayStyle">
              <div class="capacity-info">
                <div class="current-level">{{ formatValue(currentValue) }}{{ data.unit }}</div>
                <div class="percentage">{{ formatPercentage(currentPercentage) }}%</div>
                <div class="tank-type">{{ currentTankType.name }}</div>
              </div>
            </div>
          </div>
          
          <!-- 底部 -->
          <div class="tank-bottom" :style="bottomStyle">
            <div class="tank-outlet" v-if="localOptions.showOutlet">
              <div class="outlet-pipe"></div>
              <div class="outlet-valve" :class="{ active: outletActive }"></div>
            </div>
            <div class="tank-drain" v-if="localOptions.showDrain">
              <div class="drain-pipe"></div>
              <div class="drain-valve" :class="{ active: drainActive }"></div>
            </div>
          </div>
          
          <!-- 储罐支架 -->
          <div class="tank-support" v-if="localOptions.showSupport" :style="supportStyle">
            <div class="support-leg" v-for="i in 4" :key="i" :style="getSupportLegStyle(i)"></div>
          </div>
        </div>
        
        <!-- 储罐状态显示 -->
        <div class="tank-status" v-if="localOptions.showStatus" :style="statusDisplayStyle">
          <div class="status-item">
            <span class="status-label">当前液位:</span>
            <span class="status-value">{{ formatValue(currentValue) }}{{ data.unit }}</span>
          </div>
          <div class="status-item">
            <span class="status-label">储罐类型:</span>
            <span class="status-value">{{ currentTankType.name }}</span>
          </div>
          <div class="status-item">
            <span class="status-label">容量利用率:</span>
            <span class="status-value">{{ formatPercentage(currentPercentage) }}%</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 控制面板 -->
    <div class="tank-controls" v-if="showControls">
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
            <label>储罐类型:</label>
            <el-select v-model="currentTankType" @change="updateTankType" size="small">
              <el-option 
                v-for="type in tankTypes" 
                :key="type.id"
                :label="type.name" 
                :value="type" 
              />
            </el-select>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>储罐高度:</label>
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
            <label>储罐直径:</label>
            <el-input-number 
              v-model="localOptions.diameter" 
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
          <el-checkbox v-model="localOptions.showFlow" @change="updateDisplay">
            流动效果
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showInfo" @change="updateDisplay">
            显示信息
          </el-checkbox>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showInlet" @change="updateDisplay">
            进料口
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showOutlet" @change="updateDisplay">
            出料口
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showSupport" @change="updateDisplay">
            支架结构
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <div class="control-group">
            <label>储罐颜色:</label>
            <el-color-picker v-model="localOptions.tankColor" @change="updateDisplay" size="small" />
          </div>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="8">
          <div class="valve-controls">
            <label>阀门控制:</label>
            <div class="valve-buttons">
              <el-button size="small" @click="toggleInletValve" :type="inletActive ? 'success' : 'default'">
                进料阀
              </el-button>
              <el-button size="small" @click="toggleOutletValve" :type="outletActive ? 'success' : 'default'">
                出料阀
              </el-button>
              <el-button size="small" @click="toggleDrainValve" :type="drainActive ? 'success' : 'default'">
                排放阀
              </el-button>
            </div>
          </div>
        </el-col>
        
        <el-col :span="8">
          <div class="control-group">
            <label>液体颜色:</label>
            <el-color-picker v-model="currentTankType.liquidColor" @change="updateDisplay" size="small" />
          </div>
        </el-col>
        
        <el-col :span="8">
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
    <div class="tank-status-bar" v-if="showStatus">
      <span>储罐类型: {{ currentTankType.name }}</span>
      <span>当前液位: {{ formatValue(currentValue) }}{{ data.unit }}</span>
      <span>容量利用率: {{ formatPercentage(currentPercentage) }}%</span>
      <span>总容量: {{ formatValue(data.max || 100) }}{{ data.unit }}</span>
      <span v-if="currentAlarm">状态: <el-tag :type="currentAlarm.type">{{ currentAlarm.message }}</el-tag></span>
    </div>

    <!-- 报警设置对话框 -->
    <el-dialog v-model="showAlarmDialog" title="储罐报警设置" width="600px">
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
import { Switch, Bell, Download, Delete, Plus, View } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import type { GaugeData, AlarmZone, GaugeEvents } from '@/types/gauge'

// 储罐配置接口
interface TankOptions {
  height: number
  diameter: number
  showScale: boolean
  showWave: boolean
  showFlow: boolean
  showInfo: boolean
  showStatus: boolean
  showInlet: boolean
  showOutlet: boolean
  showDrain: boolean
  showVent: boolean
  showSupport: boolean
  showLevelLine: boolean
  tankColor: string
  animation: boolean
  animationDuration: number
  waveSpeed: number
}

// 储罐类型接口
interface TankType {
  id: string
  name: string
  liquidColor: string
  material: string
  capacity: number
  pressure: number
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
  options?: Partial<TankOptions>
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
  width: '400px',
  height: '500px'
})

// Emits
const emit = defineEmits<{
  valueChange: [value: number]
  alarmTrigger: [zone: AlarmZone, value: number]
  tankTypeChange: [type: TankType]
  valveChange: [valve: string, state: boolean]
}>()

// 响应式数据
const tankContainer = ref<HTMLDivElement>()
const currentValue = ref(props.data.value)
const showAlarmDialog = ref(false)
const alarmZones = ref<AlarmZone[]>(props.alarmZones || [])
const waveEnabled = ref(true)
const flowEnabled = ref(true)
const is3DView = ref(false)
const flowParticles = ref<FlowParticle[]>([])

// 阀门状态
const inletActive = ref(false)
const outletActive = ref(false)
const drainActive = ref(false)

// 储罐类型定义
const tankTypes = ref<TankType[]>([
  { id: 'water', name: '水储罐', liquidColor: '#4fc3f7', material: '不锈钢', capacity: 1000, pressure: 1.0 },
  { id: 'oil', name: '油储罐', liquidColor: '#ffb74d', material: '碳钢', capacity: 2000, pressure: 1.5 },
  { id: 'chemical', name: '化学储罐', liquidColor: '#f06292', material: '防腐钢', capacity: 500, pressure: 2.0 },
  { id: 'gas', name: '气体储罐', liquidColor: '#81c784', material: '高压钢', capacity: 100, pressure: 10.0 },
  { id: 'food', name: '食品储罐', liquidColor: '#ffcc02', material: '食品级钢', capacity: 800, pressure: 0.5 },
  { id: 'waste', name: '废料储罐', liquidColor: '#90a4ae', material: '耐腐蚀钢', capacity: 1500, pressure: 1.2 }
])

const currentTankType = ref<TankType>(tankTypes.value[0])

// 默认配置
const defaultOptions: TankOptions = {
  height: 350,
  diameter: 200,
  showScale: true,
  showWave: true,
  showFlow: true,
  showInfo: true,
  showStatus: true,
  showInlet: true,
  showOutlet: true,
  showDrain: true,
  showVent: true,
  showSupport: true,
  showLevelLine: true,
  tankColor: '#e0e0e0',
  animation: true,
  animationDuration: 1000,
  waveSpeed: 3
}

const localOptions = ref<TankOptions>({ ...defaultOptions, ...props.options })

// 动画定时器
let waveAnimationId: number | null = null
let flowAnimationId: number | null = null

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
  width: `${localOptions.value.diameter}px`,
  height: `${localOptions.value.height}px`,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center'
}))

const shellStyle = computed(() => ({
  width: '100%',
  height: '100%',
  position: 'relative' as const,
  borderRadius: '8px 8px 20px 20px',
  overflow: 'hidden' as const,
  transform: is3DView.value ? 'perspective(800px) rotateY(-15deg) rotateX(5deg)' : 'none',
  transformStyle: 'preserve-3d' as const,
  transition: 'transform 0.5s ease'
}))

const topStyle = computed(() => ({
  position: 'absolute' as const,
  top: '0',
  left: '0',
  width: '100%',
  height: '30px',
  backgroundColor: localOptions.value.tankColor,
  borderRadius: '8px 8px 0 0',
  border: `2px solid ${localOptions.value.tankColor}`,
  display: 'flex',
  justifyContent: 'space-around',
  alignItems: 'center',
  zIndex: 10
}))

const wallStyle = computed(() => ({
  position: 'absolute' as const,
  top: '30px',
  left: '0',
  width: '100%',
  height: `${localOptions.value.height - 60}px`,
  border: `3px solid ${localOptions.value.tankColor}`,
  borderTop: 'none',
  borderBottom: 'none',
  backgroundColor: '#f8f9fa',
  overflow: 'hidden' as const
}))

const bottomStyle = computed(() => ({
  position: 'absolute' as const,
  bottom: '0',
  left: '0',
  width: '100%',
  height: '30px',
  backgroundColor: localOptions.value.tankColor,
  borderRadius: '0 0 20px 20px',
  border: `2px solid ${localOptions.value.tankColor}`,
  display: 'flex',
  justifyContent: 'space-around',
  alignItems: 'center',
  zIndex: 10
}))

const supportStyle = computed(() => ({
  position: 'absolute' as const,
  bottom: '-40px',
  left: '50%',
  transform: 'translateX(-50%)',
  width: '120%',
  height: '40px',
  display: 'flex',
  justifyContent: 'space-between',
  alignItems: 'flex-end'
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
  backgroundColor: currentTankType.value.liquidColor,
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

const infoDisplayStyle = computed(() => ({
  position: 'absolute' as const,
  top: '50%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  textAlign: 'center' as const,
  color: '#333',
  fontWeight: 'bold',
  zIndex: 5,
  backgroundColor: 'rgba(255,255,255,0.9)',
  padding: '12px 16px',
  borderRadius: '8px',
  backdropFilter: 'blur(2px)',
  border: '1px solid rgba(0,0,0,0.1)'
}))

const statusDisplayStyle = computed(() => ({
  position: 'absolute' as const,
  bottom: '-80px',
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
  background: `linear-gradient(90deg, transparent, ${currentTankType.value.liquidColor}aa, transparent)`,
  borderRadius: '50%',
  animation: `tank-wave1 ${10 / localOptions.value.waveSpeed}s linear infinite`
}))

const wave2Style = computed(() => ({
  position: 'absolute' as const,
  top: '10px',
  left: '0',
  width: '200%',
  height: '100%',
  background: `linear-gradient(90deg, transparent, ${currentTankType.value.liquidColor}66, transparent)`,
  borderRadius: '50%',
  animation: `tank-wave2 ${8 / localOptions.value.waveSpeed}s linear infinite reverse`
}))

const currentAlarm = computed(() => {
  const value = currentValue.value
  for (const zone of alarmZones.value) {
    if (value >= zone.min && value <= zone.max) {
      return {
        type: 'danger' as const,
        message: zone.label || '储罐报警'
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

const getSupportLegStyle = (index: number) => {
  const positions = ['0%', '33%', '66%', '100%']
  return {
    position: 'absolute' as const,
    left: positions[index - 1],
    bottom: '0',
    width: '8px',
    height: '100%',
    backgroundColor: '#666',
    borderRadius: '4px'
  }
}

const getParticleStyle = (particle: FlowParticle) => ({
  position: 'absolute' as const,
  left: `${particle.x}%`,
  bottom: `${particle.y}%`,
  width: `${particle.size}px`,
  height: `${particle.size}px`,
  borderRadius: '50%',
  backgroundColor: 'rgba(255,255,255,0.8)',
  opacity: particle.opacity,
  transform: `rotate(${particle.direction}deg)`,
  animation: `tank-flow ${particle.speed}s linear infinite`
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

const updateTankType = () => {
  emit('tankTypeChange', currentTankType.value)
  ElMessage.info(`储罐类型已更改为: ${currentTankType.value.name}`)
}

const updateDisplay = () => {
  // 触发重新渲染
}

const toggleTankType = () => {
  const currentIndex = tankTypes.value.findIndex(type => type.id === currentTankType.value.id)
  currentTankType.value = tankTypes.value[(currentIndex + 1) % tankTypes.value.length]
  updateTankType()
}

const toggle3DView = () => {
  is3DView.value = !is3DView.value
  ElMessage.info(`3D视图已${is3DView.value ? '开启' : '关闭'}`)
}

const toggleInletValve = () => {
  inletActive.value = !inletActive.value
  emit('valveChange', 'inlet', inletActive.value)
  ElMessage.info(`进料阀已${inletActive.value ? '开启' : '关闭'}`)
}

const toggleOutletValve = () => {
  outletActive.value = !outletActive.value
  emit('valveChange', 'outlet', outletActive.value)
  ElMessage.info(`出料阀已${outletActive.value ? '开启' : '关闭'}`)
}

const toggleDrainValve = () => {
  drainActive.value = !drainActive.value
  emit('valveChange', 'drain', drainActive.value)
  ElMessage.info(`排放阀已${drainActive.value ? '开启' : '关闭'}`)
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
  ElMessage.success('储罐报警设置已保存')
}

const exportData = () => {
  const data = {
    current: {
      value: currentValue.value,
      percentage: currentPercentage.value,
      tankType: currentTankType.value,
      timestamp: new Date()
    },
    range: {
      min: props.data.min || 0,
      max: props.data.max || 100
    },
    options: localOptions.value,
    alarms: alarmZones.value,
    valves: {
      inlet: inletActive.value,
      outlet: outletActive.value,
      drain: drainActive.value
    }
  }
  
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `tank_data_${Date.now()}.json`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

// 流动粒子动画
const createFlowParticles = () => {
  if (!flowEnabled.value || !localOptions.value.showFlow) return
  
  const particleCount = 8
  flowParticles.value = []
  
  for (let i = 0; i < particleCount; i++) {
    flowParticles.value.push({
      id: i,
      x: Math.random() * 80 + 10, // 10-90%
      y: Math.random() * 100,
      size: Math.random() * 4 + 2, // 2-6px
      speed: Math.random() * 2 + 1, // 1-3s
      opacity: Math.random() * 0.6 + 0.2, // 0.2-0.8
      direction: Math.random() * 360
    })
  }
}

const animateFlowParticles = () => {
  if (!flowEnabled.value) return
  
  flowParticles.value.forEach(particle => {
    particle.y += 0.3
    particle.x += Math.sin(particle.direction) * 0.1
    if (particle.y > 100) {
      particle.y = 0
      particle.x = Math.random() * 80 + 10
    }
  })
  
  flowAnimationId = requestAnimationFrame(animateFlowParticles)
}

// 监听数据变化
watch(() => props.data, (newData) => {
  currentValue.value = newData.value
}, { deep: true })

// 生命周期
onMounted(() => {
  createFlowParticles()
  if (localOptions.value.showFlow) {
    animateFlowParticles()
  }
})

onUnmounted(() => {
  if (waveAnimationId) {
    cancelAnimationFrame(waveAnimationId)
  }
  if (flowAnimationId) {
    cancelAnimationFrame(flowAnimationId)
  }
})

// 暴露方法
defineExpose({
  updateValue,
  toggleTankType,
  toggle3DView,
  toggleInletValve,
  toggleOutletValve,
  toggleDrainValve,
  exportData,
  getCurrentValue: () => currentValue.value,
  getCurrentPercentage: () => currentPercentage.value,
  getCurrentTankType: () => currentTankType.value,
  getValveStates: () => ({
    inlet: inletActive.value,
    outlet: outletActive.value,
    drain: drainActive.value
  })
})
</script>

<style lang="scss" scoped>
.tank-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  
  .tank-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    border-bottom: 1px solid #e4e7ed;
    background: #fafafa;
    
    .tank-title {
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
  
  .tank-container {
    flex: 1;
    padding: 20px;
    position: relative;
  }
  
  .tank-body {
    .tank-shell {
      &.view-3d {
        .tank-wall {
          box-shadow: 0 10px 30px rgba(0,0,0,0.2);
        }
      }
      
      .tank-top,
      .tank-bottom {
        .tank-inlet,
        .tank-outlet,
        .tank-drain,
        .tank-vent {
          position: relative;
          
          .inlet-pipe,
          .outlet-pipe,
          .drain-pipe,
          .vent-pipe {
            width: 20px;
            height: 8px;
            background: #666;
            border-radius: 4px;
          }
          
          .inlet-valve,
          .outlet-valve,
          .drain-valve {
            width: 12px;
            height: 12px;
            background: #ccc;
            border-radius: 50%;
            margin-top: 2px;
            transition: background-color 0.3s ease;
            
            &.active {
              background: #67c23a;
            }
          }
        }
      }
      
      .tank-wall {
        .tank-scale {
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
            .wave-surface {
              .wave {
                transform-origin: center;
              }
            }
            
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
        
        .tank-info {
          .capacity-info {
            .current-level {
              font-size: 18px;
              font-weight: bold;
              color: #303133;
              margin-bottom: 4px;
            }
            
            .percentage {
              font-size: 14px;
              color: #409eff;
              margin-bottom: 4px;
            }
            
            .tank-type {
              font-size: 12px;
              color: #909399;
            }
          }
        }
      }
      
      .tank-support {
        .support-leg {
          background: linear-gradient(to bottom, #888, #555);
          box-shadow: 2px 0 4px rgba(0,0,0,0.3);
        }
      }
    }
    
    .tank-status {
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
  
  .tank-controls {
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
    
    .valve-controls {
      .valve-buttons {
        display: flex;
        gap: 8px;
        margin-top: 4px;
      }
    }
  }
  
  .tank-status-bar {
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

// 储罐波浪动画
@keyframes tank-wave1 {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(0%); }
}

@keyframes tank-wave2 {
  0% { transform: translateX(0%); }
  100% { transform: translateX(-100%); }
}

// 流动粒子动画
@keyframes tank-flow {
  0% {
    transform: translateY(0) rotate(0deg);
    opacity: 0.8;
  }
  50% {
    transform: translateY(-20px) rotate(180deg);
    opacity: 0.6;
  }
  100% {
    transform: translateY(-40px) rotate(360deg);
    opacity: 0;
  }
}

@media (max-width: 768px) {
  .tank-wrapper {
    .tank-toolbar {
      flex-direction: column;
      gap: 8px;
      align-items: flex-start;
      
      .toolbar-left,
      .toolbar-right {
        flex-wrap: wrap;
      }
    }
    
    .tank-controls {
      .el-row {
        .el-col {
          margin-bottom: 12px;
        }
      }
    }
    
    .tank-status-bar {
      flex-direction: column;
      gap: 8px;
    }
  }
}
</style>
