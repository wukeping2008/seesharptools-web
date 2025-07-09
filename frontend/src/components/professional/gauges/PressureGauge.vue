<template>
  <div class="pressure-gauge-wrapper professional-control">
    <!-- 工具栏 -->
    <div class="pressure-gauge-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <span class="pressure-gauge-title">{{ data.title || '压力表' }}</span>
      </div>
      <div class="toolbar-right">
        <el-tooltip content="切换单位">
          <el-button size="small" @click="toggleUnit">
            <el-icon><Switch /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="设置报警">
          <el-button size="small" @click="showAlarmDialog = true">
            <el-icon><Bell /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="记录最值">
          <el-button size="small" @click="recordMinMax">
            <el-icon><TrendCharts /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="导出数据">
          <el-button size="small" @click="exportData">
            <el-icon><Download /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>

    <!-- 压力表容器 -->
    <div 
      ref="pressureGaugeContainer" 
      class="pressure-gauge-container"
      :style="containerStyle"
    >
      <!-- 压力表主体 -->
      <div class="pressure-gauge-body" :style="bodyStyle">
        <!-- 外圈刻度 -->
        <div class="pressure-gauge-scale" v-if="localOptions.showScale">
          <div 
            v-for="(tick, index) in scaleTicks" 
            :key="index"
            class="scale-tick"
            :class="{ 'major-tick': tick.major }"
            :style="getTickStyle(tick)"
          >
            <div class="tick-line" :style="getTickLineStyle(tick)"></div>
            <div class="tick-label" v-if="tick.showLabel" :style="getTickLabelStyle(tick)">
              {{ formatTickValue(tick.value) }}
            </div>
          </div>
        </div>
        
        <!-- 压力表表盘 -->
        <div class="pressure-gauge-dial" :style="dialStyle">
          <!-- 安全区域 -->
          <div 
            v-for="(zone, index) in safetyZones" 
            :key="index"
            class="safety-zone"
            :style="getSafetyZoneStyle(zone)"
            :title="zone.label"
          ></div>
          
          <!-- 报警区域 -->
          <div 
            v-for="(zone, index) in alarmZones" 
            :key="index"
            class="alarm-zone"
            :style="getAlarmZoneStyle(zone)"
            :title="zone.label"
          ></div>
          
          <!-- 指针 -->
          <div 
            class="pressure-gauge-pointer" 
            v-if="localOptions.showPointer"
            :style="pointerStyle"
          >
            <div class="pointer-needle" :style="needleStyle"></div>
            <div class="pointer-center" :style="centerStyle"></div>
          </div>
          
          <!-- 中心显示区域 -->
          <div class="pressure-gauge-center" v-if="localOptions.showValue" :style="centerDisplayStyle">
            <div class="current-value">
              <span class="value-text">{{ formatValue(currentValue) }}</span>
              <span class="value-unit">{{ currentUnit }}</span>
            </div>
            <div class="pressure-type" v-if="localOptions.showPressureType">
              {{ localOptions.pressureType }}
            </div>
          </div>
        </div>
        
        <!-- 最值显示 -->
        <div class="min-max-display" v-if="localOptions.showMinMax && (minValue !== null || maxValue !== null)">
          <div class="min-value" v-if="minValue !== null">
            <span class="label">最小:</span>
            <span class="value">{{ formatValue(minValue) }}{{ currentUnit }}</span>
          </div>
          <div class="max-value" v-if="maxValue !== null">
            <span class="label">最大:</span>
            <span class="value">{{ formatValue(maxValue) }}{{ currentUnit }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 控制面板 -->
    <div class="pressure-gauge-controls" v-if="showControls">
      <el-row :gutter="16">
        <el-col :span="6">
          <div class="control-group">
            <label>当前压力:</label>
            <el-input-number 
              v-model="currentValue" 
              :min="data.min || 0" 
              :max="data.max || 10"
              :precision="2"
              @change="updateValue"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>压力单位:</label>
            <el-select v-model="currentUnit" @change="updateUnit" size="small">
              <el-option label="兆帕 (MPa)" value="MPa" />
              <el-option label="千帕 (kPa)" value="kPa" />
              <el-option label="帕斯卡 (Pa)" value="Pa" />
              <el-option label="巴 (bar)" value="bar" />
              <el-option label="磅力/平方英寸 (psi)" value="psi" />
              <el-option label="大气压 (atm)" value="atm" />
            </el-select>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>表盘大小:</label>
            <el-input-number 
              v-model="localOptions.size" 
              :min="100" 
              :max="300"
              @change="updateGauge"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>压力类型:</label>
            <el-select v-model="localOptions.pressureType" @change="updateGauge" size="small">
              <el-option label="表压" value="表压" />
              <el-option label="绝压" value="绝压" />
              <el-option label="差压" value="差压" />
              <el-option label="真空" value="真空" />
            </el-select>
          </div>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showScale" @change="updateGauge">
            显示刻度
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showPointer" @change="updateGauge">
            显示指针
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showValue" @change="updateGauge">
            显示数值
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showMinMax" @change="updateGauge">
            显示最值
          </el-checkbox>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="6">
          <el-checkbox v-model="localOptions.animation" @change="updateGauge">
            启用动画
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showSafetyZones" @change="updateGauge">
            显示安全区域
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <div class="control-group">
            <label>指针颜色:</label>
            <el-color-picker v-model="localOptions.pointerColor" @change="updateGauge" size="small" />
          </div>
        </el-col>
        <el-col :span="6">
          <div class="control-group">
            <label>表盘颜色:</label>
            <el-color-picker v-model="localOptions.dialColor" @change="updateGauge" size="small" />
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 状态信息 -->
    <div class="pressure-gauge-status" v-if="showStatus">
      <span>当前压力: {{ formatValue(currentValue) }}{{ currentUnit }}</span>
      <span>压力类型: {{ localOptions.pressureType }}</span>
      <span>安全范围: {{ formatValue(data.min || 0) }} - {{ formatValue(data.max || 10) }}{{ currentUnit }}</span>
      <span v-if="currentAlarm">状态: <el-tag :type="currentAlarm.type">{{ currentAlarm.message }}</el-tag></span>
      <span v-if="minValue !== null">历史最小: {{ formatValue(minValue) }}{{ currentUnit }}</span>
      <span v-if="maxValue !== null">历史最大: {{ formatValue(maxValue) }}{{ currentUnit }}</span>
    </div>

    <!-- 报警设置对话框 -->
    <el-dialog v-model="showAlarmDialog" title="压力报警设置" width="600px">
      <div class="alarm-settings">
        <h4>压力报警区域设置</h4>
        <div v-for="(zone, index) in alarmZones" :key="index" class="alarm-zone-config">
          <el-row :gutter="16">
            <el-col :span="5">
              <el-input-number v-model="zone.min" placeholder="最低压力" size="small" />
            </el-col>
            <el-col :span="5">
              <el-input-number v-model="zone.max" placeholder="最高压力" size="small" />
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
        
        <h4 style="margin-top: 20px;">安全区域设置</h4>
        <div v-for="(zone, index) in safetyZones" :key="index" class="safety-zone-config">
          <el-row :gutter="16">
            <el-col :span="5">
              <el-input-number v-model="zone.min" placeholder="最低压力" size="small" />
            </el-col>
            <el-col :span="5">
              <el-input-number v-model="zone.max" placeholder="最高压力" size="small" />
            </el-col>
            <el-col :span="4">
              <el-color-picker v-model="zone.color" />
            </el-col>
            <el-col :span="6">
              <el-input v-model="zone.label" placeholder="标签" size="small" />
            </el-col>
            <el-col :span="2">
              <el-button size="small" type="danger" @click="removeSafetyZone(index)">
                <el-icon><Delete /></el-icon>
              </el-button>
            </el-col>
          </el-row>
        </div>
        <el-button @click="addSafetyZone" type="success" size="small">
          <el-icon><Plus /></el-icon>
          添加安全区域
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
import { ref, computed, watch, onMounted } from 'vue'
import { Switch, Bell, Download, Delete, Plus, TrendCharts } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import type { GaugeData, PressureGaugeOptions, AlarmZone, SafetyZone, GaugeEvents } from '@/types/gauge'

// Props
interface Props {
  data: GaugeData
  options?: Partial<PressureGaugeOptions>
  alarmZones?: AlarmZone[]
  safetyZones?: SafetyZone[]
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
  width: '250px',
  height: '250px'
})

// Emits
const emit = defineEmits<{
  valueChange: [value: number]
  alarmTrigger: [zone: AlarmZone, value: number]
  unitChange: [unit: string]
  minMaxRecord: [min: number, max: number]
}>()

// 响应式数据
const pressureGaugeContainer = ref<HTMLDivElement>()
const currentValue = ref(props.data.value)
const currentUnit = ref(props.data.unit || 'MPa')
const showAlarmDialog = ref(false)
const alarmZones = ref<AlarmZone[]>(props.alarmZones || [])
const safetyZones = ref<SafetyZone[]>(props.safetyZones || [])
const minValue = ref<number | null>(null)
const maxValue = ref<number | null>(null)

// 默认配置
const defaultOptions: PressureGaugeOptions = {
  size: 200,
  startAngle: 225,
  endAngle: -45,
  showScale: true,
  showPointer: true,
  showValue: true,
  showMinMax: false,
  showSafetyZones: true,
  showPressureType: true,
  pressureType: '表压',
  pointerColor: '#409eff',
  dialColor: '#f5f7fa',
  scaleColor: '#606266',
  animation: true,
  animationDuration: 1000
}

const localOptions = ref<PressureGaugeOptions>({ ...defaultOptions, ...props.options })

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
  width: `${localOptions.value.size}px`,
  height: `${localOptions.value.size}px`,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center'
}))

const dialStyle = computed(() => ({
  width: `${localOptions.value.size}px`,
  height: `${localOptions.value.size}px`,
  borderRadius: '50%',
  backgroundColor: localOptions.value.dialColor,
  border: '3px solid #999',
  position: 'relative' as const,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center'
}))

const pointerStyle = computed(() => {
  const angle = getPointerAngle()
  return {
    position: 'absolute' as const,
    width: '100%',
    height: '100%',
    transform: `rotate(${angle}deg)`,
    transition: localOptions.value.animation ? `transform ${localOptions.value.animationDuration}ms ease` : 'none'
  }
})

const needleStyle = computed(() => ({
  position: 'absolute' as const,
  top: '50%',
  left: '50%',
  width: '2px',
  height: `${localOptions.value.size * 0.35}px`,
  backgroundColor: localOptions.value.pointerColor,
  transformOrigin: 'bottom center',
  transform: 'translate(-50%, -100%)',
  borderRadius: '1px'
}))

const centerStyle = computed(() => ({
  position: 'absolute' as const,
  top: '50%',
  left: '50%',
  width: '12px',
  height: '12px',
  borderRadius: '50%',
  backgroundColor: localOptions.value.pointerColor,
  transform: 'translate(-50%, -50%)',
  zIndex: 10
}))

const centerDisplayStyle = computed(() => ({
  position: 'absolute' as const,
  top: '60%',
  left: '50%',
  transform: 'translate(-50%, -50%)',
  textAlign: 'center' as const,
  zIndex: 5
}))

const scaleTicks = computed(() => {
  const min = props.data.min || 0
  const max = props.data.max || 10
  const range = max - min
  const majorTickCount = 10
  const minorTickCount = 50
  
  const ticks: Array<{
    value: number
    angle: number
    major: boolean
    showLabel: boolean
  }> = []
  
  // 主刻度
  for (let i = 0; i <= majorTickCount; i++) {
    const value = min + (range * i / majorTickCount)
    const angle = localOptions.value.startAngle + 
                  (localOptions.value.endAngle - localOptions.value.startAngle) * (i / majorTickCount)
    ticks.push({
      value,
      angle,
      major: true,
      showLabel: true
    })
  }
  
  // 次刻度
  for (let i = 0; i <= minorTickCount; i++) {
    const value = min + (range * i / minorTickCount)
    const angle = localOptions.value.startAngle + 
                  (localOptions.value.endAngle - localOptions.value.startAngle) * (i / minorTickCount)
    
    // 避免与主刻度重复
    const isMainTick = ticks.some(tick => Math.abs(tick.angle - angle) < 1)
    if (!isMainTick) {
      ticks.push({
        value,
        angle,
        major: false,
        showLabel: false
      })
    }
  }
  
  return ticks
})

const currentAlarm = computed(() => {
  const value = currentValue.value
  for (const zone of alarmZones.value) {
    if (value >= zone.min && value <= zone.max) {
      return {
        type: 'danger' as const,
        message: zone.label || '压力报警'
      }
    }
  }
  return null
})

// 方法
const getPointerAngle = () => {
  const min = props.data.min || 0
  const max = props.data.max || 10
  const percentage = (currentValue.value - min) / (max - min)
  const angleRange = localOptions.value.endAngle - localOptions.value.startAngle
  return localOptions.value.startAngle + angleRange * percentage
}

const getTickStyle = (tick: any) => {
  const radius = localOptions.value.size / 2 - 15
  const angle = tick.angle * Math.PI / 180
  const x = Math.cos(angle) * radius
  const y = Math.sin(angle) * radius
  
  return {
    position: 'absolute' as const,
    left: `${localOptions.value.size / 2 + x}px`,
    top: `${localOptions.value.size / 2 + y}px`,
    transform: 'translate(-50%, -50%)'
  }
}

const getTickLineStyle = (tick: any) => ({
  width: tick.major ? '3px' : '1px',
  height: tick.major ? '15px' : '8px',
  backgroundColor: localOptions.value.scaleColor,
  transform: `rotate(${tick.angle + 90}deg)`
})

const getTickLabelStyle = (tick: any) => {
  const labelRadius = localOptions.value.size / 2 - 35
  const angle = tick.angle * Math.PI / 180
  const x = Math.cos(angle) * labelRadius
  const y = Math.sin(angle) * labelRadius
  
  return {
    position: 'absolute' as const,
    left: `${x}px`,
    top: `${y}px`,
    fontSize: '12px',
    color: localOptions.value.scaleColor,
    fontWeight: '500'
  }
}

const getSafetyZoneStyle = (zone: SafetyZone) => {
  const min = props.data.min || 0
  const max = props.data.max || 10
  const range = max - min
  
  const startPercent = (zone.min - min) / range
  const endPercent = (zone.max - min) / range
  
  const startAngle = localOptions.value.startAngle + 
                    (localOptions.value.endAngle - localOptions.value.startAngle) * startPercent
  const endAngle = localOptions.value.startAngle + 
                  (localOptions.value.endAngle - localOptions.value.startAngle) * endPercent
  
  return {
    position: 'absolute' as const,
    width: `${localOptions.value.size - 20}px`,
    height: `${localOptions.value.size - 20}px`,
    borderRadius: '50%',
    border: `8px solid transparent`,
    borderImage: `conic-gradient(transparent ${startAngle + 90}deg, ${zone.color} ${startAngle + 90}deg ${endAngle + 90}deg, transparent ${endAngle + 90}deg) 1`,
    opacity: 0.6,
    pointerEvents: 'none' as const
  }
}

const getAlarmZoneStyle = (zone: AlarmZone) => {
  const min = props.data.min || 0
  const max = props.data.max || 10
  const range = max - min
  
  const startPercent = (zone.min - min) / range
  const endPercent = (zone.max - min) / range
  
  const startAngle = localOptions.value.startAngle + 
                    (localOptions.value.endAngle - localOptions.value.startAngle) * startPercent
  const endAngle = localOptions.value.startAngle + 
                  (localOptions.value.endAngle - localOptions.value.startAngle) * endPercent
  
  return {
    position: 'absolute' as const,
    width: `${localOptions.value.size - 10}px`,
    height: `${localOptions.value.size - 10}px`,
    borderRadius: '50%',
    border: `5px solid transparent`,
    borderImage: `conic-gradient(transparent ${startAngle + 90}deg, ${zone.color} ${startAngle + 90}deg ${endAngle + 90}deg, transparent ${endAngle + 90}deg) 1`,
    opacity: 0.8,
    pointerEvents: 'none' as const
  }
}

const formatValue = (value: number) => {
  return value.toFixed(2)
}

const formatTickValue = (value: number) => {
  return value.toFixed(1)
}

const updateValue = (value: number) => {
  currentValue.value = value
  
  // 更新最值
  if (minValue.value === null || value < minValue.value) {
    minValue.value = value
  }
  if (maxValue.value === null || value > maxValue.value) {
    maxValue.value = value
  }
  
  emit('valueChange', value)
  props.events?.onValueChange?.(value)
  
  // 检查报警
  checkAlarms(value)
}

const updateUnit = () => {
  emit('unitChange', currentUnit.value)
}

const updateGauge = () => {
  // 触发重新渲染
}

const toggleUnit = () => {
  const units = ['MPa', 'kPa', 'Pa', 'bar', 'psi', 'atm']
  const currentIndex = units.indexOf(currentUnit.value)
  currentUnit.value = units[(currentIndex + 1) % units.length]
  updateUnit()
  ElMessage.info(`压力单位已切换为: ${currentUnit.value}`)
}

const recordMinMax = () => {
  if (minValue.value !== null && maxValue.value !== null) {
    emit('minMaxRecord', minValue.value, maxValue.value)
    ElMessage.success(`已记录最值: 最小 ${formatValue(minValue.value)}${currentUnit.value}, 最大 ${formatValue(maxValue.value)}${currentUnit.value}`)
  } else {
    ElMessage.warning('暂无最值数据')
  }
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
    min: 8,
    max: 10,
    color: '#f56c6c',
    label: '超压警告'
  })
}

const removeAlarmZone = (index: number) => {
  alarmZones.value.splice(index, 1)
}

const addSafetyZone = () => {
  safetyZones.value.push({
    min: 2,
    max: 6,
    color: '#67c23a',
    label: '安全区域'
  })
}

const removeSafetyZone = (index: number) => {
  safetyZones.value.splice(index, 1)
}

const saveAlarmSettings = () => {
  showAlarmDialog.value = false
  ElMessage.success('压力报警设置已保存')
}

const exportData = () => {
  const data = {
    current: {
      value: currentValue.value,
      unit: currentUnit.value,
      timestamp: new Date()
    },
    range: {
      min: props.data.min || 0,
      max: props.data.max || 10
    },
    minMax: {
      min: minValue.value,
      max: maxValue.value
    },
    options: localOptions.value,
    alarms: alarmZones.value,
    safetyZones: safetyZones.value
  }
  
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `pressure_gauge_data_${Date.now()}.json`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

// 监听数据变化
watch(() => props.data, (newData) => {
  currentValue.value = newData.value
  if (newData.unit) {
    currentUnit.value = newData.unit
  }
}, { deep: true })

// 生命周期
onMounted(() => {
  // 初始化最值
  minValue.value = currentValue.value
  maxValue.value = currentValue.value
})

// 暴露方法
defineExpose({
  updateValue,
  toggleUnit,
  recordMinMax,
  exportData,
  getCurrentValue: () => currentValue.value,
  getCurrentUnit: () => currentUnit.value,
  getMinMax: () => ({ min: minValue.value, max: maxValue.value })
})
</script>

<style lang="scss" scoped>
.pressure-gauge-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  
  .pressure-gauge-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    border-bottom: 1px solid #e4e7ed;
    background: #fafafa;
    
    .pressure-gauge-title {
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
  
  .pressure-gauge-container {
    flex: 1;
    padding: 20px;
    position: relative;
  }
  
  .pressure-gauge-body {
    .pressure-gauge-scale {
      position: absolute;
      width: 100%;
      height: 100%;
      
      .scale-tick {
        .tick-line {
          border-radius: 1px;
        }
        
        .tick-label {
          white-space: nowrap;
          font-size: 12px;
          color: #606266;
          font-weight: 500;
        }
      }
    }
    
    .pressure-gauge-dial {
      .safety-zone,
      .alarm-zone {
        pointer-events: none;
      }
      
      .pressure-gauge-pointer {
        .pointer-needle {
          box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
        }
        
        .pointer-center {
          box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
        }
      }
      
      .pressure-gauge-center {
        .current-value {
          .value-text {
            font-size: 18px;
            font-weight: bold;
            color: #303133;
          }
          
          .value-unit {
            font-size: 12px;
            color: #909399;
            margin-left: 2px;
          }
        }
        
        .pressure-type {
          font-size: 10px;
          color: #909399;
          margin-top: 4px;
        }
      }
    }
    
    .min-max-display {
      position: absolute;
      bottom: -40px;
      left: 50%;
      transform: translateX(-50%);
      display: flex;
      gap: 20px;
      font-size: 12px;
      color: #606266;
      
      .min-value,
      .max-value {
        .label {
          font-weight: 500;
        }
        
        .value {
          margin-left: 4px;
          color: #303133;
        }
      }
    }
  }
  
  .pressure-gauge-controls {
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
  
  .pressure-gauge-status {
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
    .alarm-zone-config,
    .safety-zone-config {
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

@media (max-width: 768px) {
  .pressure-gauge-wrapper {
    .pressure-gauge-toolbar {
      flex-direction: column;
      gap: 8px;
      align-items: flex-start;
      
      .toolbar-left,
      .toolbar-right {
        flex-wrap: wrap;
      }
    }
    
    .pressure-gauge-controls {
      .el-row {
        .el-col {
          margin-bottom: 12px;
        }
      }
    }
    
    .pressure-gauge-status {
      flex-direction: column;
      gap: 8px;
    }
  }
}
</style>
