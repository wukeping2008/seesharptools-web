<template>
  <div class="linear-gauge-wrapper professional-control">
    <!-- 工具栏 -->
    <div class="gauge-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <span class="gauge-title">{{ data.title || '线性仪表' }}</span>
      </div>
      <div class="toolbar-right">
        <el-tooltip content="切换方向">
          <el-button size="small" @click="toggleOrientation">
            <el-icon><Switch /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="设置报警">
          <el-button size="small" @click="showAlarmDialog = true">
            <el-icon><Bell /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="导出数据">
          <el-button size="small" @click="exportData">
            <el-icon><Download /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>

    <!-- 线性仪表容器 -->
    <div 
      ref="gaugeContainer" 
      class="linear-gauge-container"
      :class="[`orientation-${localOptions.orientation}`, { 'with-scale': localOptions.showScale }]"
      :style="containerStyle"
    >
      <!-- 背景轨道 -->
      <div class="gauge-track" :style="trackStyle">
        <!-- 报警区域 -->
        <div 
          v-for="(zone, index) in alarmZones" 
          :key="index"
          class="alarm-zone"
          :style="getAlarmZoneStyle(zone)"
          :title="zone.label"
        ></div>
        
        <!-- 进度条 -->
        <div 
          class="gauge-progress" 
          :style="progressStyle"
          v-if="localOptions.showProgress"
        ></div>
        
        <!-- 指针 -->
        <div 
          class="gauge-pointer" 
          :style="pointerStyle"
          v-if="localOptions.showPointer"
        ></div>
      </div>
      
      <!-- 刻度 -->
      <div class="gauge-scale" v-if="localOptions.showScale">
        <div 
          v-for="(tick, index) in scaleTicks" 
          :key="index"
          class="scale-tick"
          :class="{ 'major-tick': tick.major }"
          :style="getTickStyle(tick)"
        >
          <span class="tick-label" v-if="tick.showLabel">{{ formatTickValue(tick.value) }}</span>
        </div>
      </div>
      
      <!-- 数值显示 -->
      <div class="gauge-value" v-if="localOptions.showLabel" :style="valueStyle">
        <span class="value-text">{{ formatValue(currentValue) }}</span>
        <span class="value-unit" v-if="data.unit">{{ data.unit }}</span>
      </div>
    </div>

    <!-- 控制面板 -->
    <div class="gauge-controls" v-if="showControls">
      <el-row :gutter="16">
        <el-col :span="6">
          <div class="control-group">
            <label>当前值:</label>
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
            <label>方向:</label>
            <el-select v-model="localOptions.orientation" @change="updateGauge" size="small">
              <el-option label="水平" value="horizontal" />
              <el-option label="垂直" value="vertical" />
            </el-select>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>宽度:</label>
            <el-input-number 
              v-model="localOptions.width" 
              :min="10" 
              :max="50"
              @change="updateGauge"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>高度:</label>
            <el-input-number 
              v-model="localOptions.height" 
              :min="100" 
              :max="500"
              @change="updateGauge"
              size="small"
            />
          </div>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showPointer" @change="updateGauge">
            显示指针
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showProgress" @change="updateGauge">
            显示进度
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showScale" @change="updateGauge">
            显示刻度
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.animation" @change="updateGauge">
            启用动画
          </el-checkbox>
        </el-col>
      </el-row>
    </div>

    <!-- 状态信息 -->
    <div class="gauge-status" v-if="showStatus">
      <span>当前值: {{ formatValue(currentValue) }}</span>
      <span>范围: {{ formatValue(data.min || 0) }} - {{ formatValue(data.max || 100) }}</span>
      <span>方向: {{ localOptions.orientation === 'horizontal' ? '水平' : '垂直' }}</span>
      <span v-if="currentAlarm">状态: <el-tag :type="currentAlarm.type">{{ currentAlarm.message }}</el-tag></span>
    </div>

    <!-- 报警设置对话框 -->
    <el-dialog v-model="showAlarmDialog" title="报警设置" width="500px">
      <div class="alarm-settings">
        <h4>报警区域设置</h4>
        <div v-for="(zone, index) in alarmZones" :key="index" class="alarm-zone-config">
          <el-row :gutter="16">
            <el-col :span="6">
              <el-input-number v-model="zone.min" placeholder="最小值" size="small" />
            </el-col>
            <el-col :span="6">
              <el-input-number v-model="zone.max" placeholder="最大值" size="small" />
            </el-col>
            <el-col :span="6">
              <el-color-picker v-model="zone.color" />
            </el-col>
            <el-col :span="4">
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
import { ref, computed, watch, onMounted } from 'vue'
import { Switch, Bell, Download, Delete, Plus } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import type { GaugeData, LinearGaugeOptions, AlarmZone, GaugeEvents } from '@/types/gauge'

// Props
interface Props {
  data: GaugeData
  options?: Partial<LinearGaugeOptions>
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
  width: '100%',
  height: '200px'
})

// Emits
const emit = defineEmits<{
  valueChange: [value: number]
  alarmTrigger: [zone: AlarmZone, value: number]
  orientationChange: [orientation: 'horizontal' | 'vertical']
}>()

// 响应式数据
const gaugeContainer = ref<HTMLDivElement>()
const currentValue = ref(props.data.value)
const showAlarmDialog = ref(false)
const alarmZones = ref<AlarmZone[]>(props.alarmZones || [])

// 默认配置
const defaultOptions: LinearGaugeOptions = {
  orientation: 'horizontal',
  width: 20,
  height: 200,
  showScale: true,
  showPointer: true,
  showProgress: true,
  showLabel: true,
  animation: true,
  theme: 'light'
}

const localOptions = ref<LinearGaugeOptions>({ ...defaultOptions, ...props.options })

// 计算属性
const containerStyle = computed(() => {
  const isHorizontal = localOptions.value.orientation === 'horizontal'
  return {
    width: typeof props.width === 'number' ? `${props.width}px` : props.width,
    height: typeof props.height === 'number' ? `${props.height}px` : props.height,
    flexDirection: (isHorizontal ? 'column' : 'row') as 'column' | 'row'
  }
})

const trackStyle = computed(() => {
  const isHorizontal = localOptions.value.orientation === 'horizontal'
  return {
    width: isHorizontal ? '100%' : `${localOptions.value.width}px`,
    height: isHorizontal ? `${localOptions.value.width}px` : '100%',
    backgroundColor: localOptions.value.theme === 'dark' ? '#2c3e50' : '#e4e7ed',
    borderRadius: `${localOptions.value.width / 2}px`
  }
})

const progressStyle = computed(() => {
  const percentage = ((currentValue.value - (props.data.min || 0)) / 
                     ((props.data.max || 100) - (props.data.min || 0))) * 100
  const isHorizontal = localOptions.value.orientation === 'horizontal'
  
  const style: any = {
    backgroundColor: getProgressColor(percentage),
    borderRadius: `${localOptions.value.width / 2}px`,
    transition: localOptions.value.animation ? 'all 0.3s ease' : 'none'
  }
  
  if (isHorizontal) {
    style.width = `${Math.max(0, Math.min(100, percentage))}%`
    style.height = '100%'
  } else {
    style.width = '100%'
    style.height = `${Math.max(0, Math.min(100, percentage))}%`
    style.alignSelf = 'flex-end'
  }
  
  return style
})

const pointerStyle = computed(() => {
  const percentage = ((currentValue.value - (props.data.min || 0)) / 
                     ((props.data.max || 100) - (props.data.min || 0))) * 100
  const isHorizontal = localOptions.value.orientation === 'horizontal'
  const pointerSize = localOptions.value.width + 4
  
  const style: any = {
    width: `${pointerSize}px`,
    height: `${pointerSize}px`,
    backgroundColor: '#409eff',
    borderRadius: '50%',
    border: '2px solid white',
    boxShadow: '0 2px 4px rgba(0,0,0,0.2)',
    transition: localOptions.value.animation ? 'all 0.3s ease' : 'none',
    position: 'absolute',
    zIndex: 10
  }
  
  if (isHorizontal) {
    style.left = `calc(${Math.max(0, Math.min(100, percentage))}% - ${pointerSize/2}px)`
    style.top = `${-2}px`
  } else {
    style.bottom = `calc(${Math.max(0, Math.min(100, percentage))}% - ${pointerSize/2}px)`
    style.left = `${-2}px`
  }
  
  return style
})

const valueStyle = computed(() => {
  const isHorizontal = localOptions.value.orientation === 'horizontal'
  return {
    marginTop: isHorizontal ? '8px' : '0',
    marginLeft: isHorizontal ? '0' : '8px',
    textAlign: 'center' as const
  }
})

const scaleTicks = computed(() => {
  const min = props.data.min || 0
  const max = props.data.max || 100
  const range = max - min
  const majorTickCount = 5
  const minorTickCount = 4
  
  const ticks: Array<{
    value: number
    position: number
    major: boolean
    showLabel: boolean
  }> = []
  
  // 主刻度
  for (let i = 0; i <= majorTickCount; i++) {
    const value = min + (range * i / majorTickCount)
    const position = (i / majorTickCount) * 100
    ticks.push({
      value,
      position,
      major: true,
      showLabel: true
    })
    
    // 次刻度
    if (i < majorTickCount) {
      for (let j = 1; j <= minorTickCount; j++) {
        const minorValue = value + (range / majorTickCount * j / (minorTickCount + 1))
        const minorPosition = position + (100 / majorTickCount * j / (minorTickCount + 1))
        ticks.push({
          value: minorValue,
          position: minorPosition,
          major: false,
          showLabel: false
        })
      }
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
        message: zone.label || '报警'
      }
    }
  }
  return null
})

// 方法
const getProgressColor = (percentage: number) => {
  if (percentage < 30) return '#67c23a'
  if (percentage < 70) return '#e6a23c'
  return '#f56c6c'
}

const getAlarmZoneStyle = (zone: AlarmZone) => {
  const min = props.data.min || 0
  const max = props.data.max || 100
  const range = max - min
  
  const startPercent = ((zone.min - min) / range) * 100
  const endPercent = ((zone.max - min) / range) * 100
  const sizePercent = endPercent - startPercent
  
  const isHorizontal = localOptions.value.orientation === 'horizontal'
  
  const style: any = {
    backgroundColor: zone.color,
    opacity: 0.3,
    position: 'absolute'
  }
  
  if (isHorizontal) {
    style.left = `${Math.max(0, startPercent)}%`
    style.width = `${Math.min(100 - Math.max(0, startPercent), sizePercent)}%`
    style.height = '100%'
  } else {
    style.bottom = `${Math.max(0, startPercent)}%`
    style.height = `${Math.min(100 - Math.max(0, startPercent), sizePercent)}%`
    style.width = '100%'
  }
  
  return style
}

const getTickStyle = (tick: any) => {
  const isHorizontal = localOptions.value.orientation === 'horizontal'
  const style: any = {
    position: 'absolute'
  }
  
  if (isHorizontal) {
    style.left = `${tick.position}%`
    style.transform = 'translateX(-50%)'
  } else {
    style.bottom = `${tick.position}%`
    style.transform = 'translateY(50%)'
  }
  
  return style
}

const formatValue = (value: number) => {
  return value.toFixed(1)
}

const formatTickValue = (value: number) => {
  return Math.round(value).toString()
}

const updateValue = (value: number) => {
  currentValue.value = value
  emit('valueChange', value)
  props.events?.onValueChange?.(value)
  
  // 检查报警
  checkAlarms(value)
}

const updateGauge = () => {
  // 触发重新渲染
}

const toggleOrientation = () => {
  localOptions.value.orientation = localOptions.value.orientation === 'horizontal' ? 'vertical' : 'horizontal'
  emit('orientationChange', localOptions.value.orientation)
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
    label: '危险'
  })
}

const removeAlarmZone = (index: number) => {
  alarmZones.value.splice(index, 1)
}

const saveAlarmSettings = () => {
  showAlarmDialog.value = false
  ElMessage.success('报警设置已保存')
}

const exportData = () => {
  const data = {
    current: {
      value: currentValue.value,
      timestamp: new Date(),
      unit: props.data.unit
    },
    range: {
      min: props.data.min || 0,
      max: props.data.max || 100
    },
    orientation: localOptions.value.orientation,
    alarms: alarmZones.value
  }
  
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `linear_gauge_data_${Date.now()}.json`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

// 监听数据变化
watch(() => props.data, (newData) => {
  currentValue.value = newData.value
}, { deep: true })

// 生命周期
onMounted(() => {
  // 初始化
})

// 暴露方法
defineExpose({
  updateValue,
  toggleOrientation,
  exportData,
  getCurrentValue: () => currentValue.value
})
</script>

<style lang="scss" scoped>
.linear-gauge-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  
  .gauge-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    border-bottom: 1px solid #e4e7ed;
    background: #fafafa;
    
    .gauge-title {
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
  
  .linear-gauge-container {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 20px;
    position: relative;
    
    &.orientation-horizontal {
      flex-direction: column;
      
      .gauge-track {
        position: relative;
      }
      
      .gauge-scale {
        display: flex;
        justify-content: space-between;
        width: 100%;
        margin-top: 8px;
        
        .scale-tick {
          display: flex;
          flex-direction: column;
          align-items: center;
          
          &::before {
            content: '';
            width: 2px;
            height: 8px;
            background: #909399;
            margin-bottom: 4px;
          }
          
          &.major-tick::before {
            height: 12px;
            background: #606266;
          }
          
          .tick-label {
            font-size: 12px;
            color: #606266;
          }
        }
      }
    }
    
    &.orientation-vertical {
      flex-direction: row;
      
      .gauge-track {
        position: relative;
        display: flex;
        flex-direction: column;
        justify-content: flex-end;
      }
      
      .gauge-scale {
        display: flex;
        flex-direction: column;
        justify-content: space-between;
        height: 100%;
        margin-left: 8px;
        
        .scale-tick {
          display: flex;
          align-items: center;
          
          &::before {
            content: '';
            width: 8px;
            height: 2px;
            background: #909399;
            margin-right: 4px;
          }
          
          &.major-tick::before {
            width: 12px;
            background: #606266;
          }
          
          .tick-label {
            font-size: 12px;
            color: #606266;
          }
        }
      }
    }
  }
  
  .gauge-track {
    position: relative;
    overflow: hidden;
  }
  
  .gauge-progress {
    position: relative;
    z-index: 5;
  }
  
  .gauge-pointer {
    z-index: 10;
  }
  
  .alarm-zone {
    z-index: 2;
  }
  
  .gauge-value {
    .value-text {
      font-size: 18px;
      font-weight: bold;
      color: #409eff;
    }
    
    .value-unit {
      font-size: 14px;
      color: #909399;
      margin-left: 4px;
    }
  }
  
  .gauge-controls {
    padding: 12px;
    border-top: 1px solid #e4e7ed;
    background: #fafafa;
    
    .control-group {
      display: flex;
      flex-direction: column;
      gap: 4px;
      
      label {
        font-size: 12px;
        font-weight: 500;
        color: #606266;
      }
    }
  }
  
  .gauge-status {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 4px 12px;
    font-size: 12px;
    color: #909399;
    background: #f5f7fa;
    border-top: 1px solid #e4e7ed;
    
    span {
      margin-right: 16px;
      
      &:last-child {
        margin-right: 0;
      }
    }
  }
  
  .alarm-settings {
    .alarm-zone-config {
      margin-bottom: 12px;
      padding: 8px;
      border: 1px solid #e4e7ed;
      border-radius: 4px;
    }
  }
}

@media (max-width: 768px) {
  .linear-gauge-wrapper {
    .gauge-toolbar {
      flex-direction: column;
      gap: 8px;
      align-items: flex-start;
    }
    
    .gauge-status {
      flex-direction: column;
      align-items: flex-start;
      gap: 4px;
      
      span {
        margin-right: 0;
      }
    }
  }
}
</style>
