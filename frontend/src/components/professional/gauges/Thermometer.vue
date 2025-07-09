<template>
  <div class="thermometer-wrapper professional-control">
    <!-- 工具栏 -->
    <div class="thermometer-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <span class="thermometer-title">{{ data.title || '温度计' }}</span>
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
        <el-tooltip content="导出数据">
          <el-button size="small" @click="exportData">
            <el-icon><Download /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>

    <!-- 温度计容器 -->
    <div 
      ref="thermometerContainer" 
      class="thermometer-container"
      :style="containerStyle"
    >
      <!-- 温度计主体 -->
      <div class="thermometer-body" :style="bodyStyle">
        <!-- 刻度 -->
        <div class="thermometer-scale" v-if="localOptions.showScale">
          <div 
            v-for="(tick, index) in scaleTicks" 
            :key="index"
            class="scale-tick"
            :class="{ 'major-tick': tick.major }"
            :style="getTickStyle(tick)"
          >
            <span class="tick-line"></span>
            <span class="tick-label" v-if="tick.showLabel">{{ formatTickValue(tick.value) }}</span>
          </div>
        </div>
        
        <!-- 温度计管 -->
        <div class="thermometer-tube" :style="tubeStyle">
          <!-- 报警区域 -->
          <div 
            v-for="(zone, index) in alarmZones" 
            :key="index"
            class="alarm-zone"
            :style="getAlarmZoneStyle(zone)"
            :title="zone.label"
          ></div>
          
          <!-- 液体 -->
          <div 
            class="thermometer-liquid" 
            :style="liquidStyle"
          ></div>
        </div>
        
        <!-- 温度计球泡 -->
        <div class="thermometer-bulb" :style="bulbStyle">
          <div class="bulb-liquid" :style="bulbLiquidStyle"></div>
        </div>
      </div>
      
      <!-- 数值显示 -->
      <div class="thermometer-value" v-if="localOptions.showLabel" :style="valueStyle">
        <span class="value-text">{{ formatValue(currentValue) }}</span>
        <span class="value-unit">{{ currentUnit }}</span>
      </div>
    </div>

    <!-- 控制面板 -->
    <div class="thermometer-controls" v-if="showControls">
      <el-row :gutter="16">
        <el-col :span="6">
          <div class="control-group">
            <label>当前温度:</label>
            <el-input-number 
              v-model="currentValue" 
              :min="data.min || -50" 
              :max="data.max || 150"
              :precision="1"
              @change="updateValue"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>温度单位:</label>
            <el-select v-model="currentUnit" @change="updateUnit" size="small">
              <el-option label="摄氏度 (°C)" value="°C" />
              <el-option label="华氏度 (°F)" value="°F" />
              <el-option label="开尔文 (K)" value="K" />
            </el-select>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>管宽度:</label>
            <el-input-number 
              v-model="localOptions.tubeWidth" 
              :min="10" 
              :max="30"
              @change="updateThermometer"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>高度:</label>
            <el-input-number 
              v-model="localOptions.height" 
              :min="200" 
              :max="400"
              @change="updateThermometer"
              size="small"
            />
          </div>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showScale" @change="updateThermometer">
            显示刻度
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showLabel" @change="updateThermometer">
            显示数值
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.animation" @change="updateThermometer">
            启用动画
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <div class="control-group">
            <label>液体颜色:</label>
            <el-color-picker v-model="localOptions.liquidColor" @change="updateThermometer" size="small" />
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 状态信息 -->
    <div class="thermometer-status" v-if="showStatus">
      <span>当前温度: {{ formatValue(currentValue) }}{{ currentUnit }}</span>
      <span>范围: {{ formatValue(data.min || -50) }} - {{ formatValue(data.max || 150) }}{{ currentUnit }}</span>
      <span>液体颜色: {{ localOptions.liquidColor }}</span>
      <span v-if="currentAlarm">状态: <el-tag :type="currentAlarm.type">{{ currentAlarm.message }}</el-tag></span>
    </div>

    <!-- 报警设置对话框 -->
    <el-dialog v-model="showAlarmDialog" title="温度报警设置" width="500px">
      <div class="alarm-settings">
        <h4>温度报警区域设置</h4>
        <div v-for="(zone, index) in alarmZones" :key="index" class="alarm-zone-config">
          <el-row :gutter="16">
            <el-col :span="6">
              <el-input-number v-model="zone.min" placeholder="最低温度" size="small" />
            </el-col>
            <el-col :span="6">
              <el-input-number v-model="zone.max" placeholder="最高温度" size="small" />
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
import type { GaugeData, ThermometerOptions, AlarmZone, GaugeEvents } from '@/types/gauge'

// Props
interface Props {
  data: GaugeData
  options?: Partial<ThermometerOptions>
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
  width: '200px',
  height: '300px'
})

// Emits
const emit = defineEmits<{
  valueChange: [value: number]
  alarmTrigger: [zone: AlarmZone, value: number]
  unitChange: [unit: string]
}>()

// 响应式数据
const thermometerContainer = ref<HTMLDivElement>()
const currentValue = ref(props.data.value)
const currentUnit = ref(props.data.unit || '°C')
const showAlarmDialog = ref(false)
const alarmZones = ref<AlarmZone[]>(props.alarmZones || [])

// 默认配置
const defaultOptions: ThermometerOptions = {
  height: 300,
  width: 60,
  bulbRadius: 25,
  tubeWidth: 20,
  showScale: true,
  showLabel: true,
  liquidColor: '#ff4757',
  tubeColor: '#ddd',
  backgroundColor: 'transparent',
  animation: true,
  animationDuration: 1000
}

const localOptions = ref<ThermometerOptions>({ ...defaultOptions, ...props.options })

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
  flexDirection: 'column' as const,
  alignItems: 'center'
}))

const tubeStyle = computed(() => ({
  width: `${localOptions.value.tubeWidth}px`,
  height: `${localOptions.value.height - localOptions.value.bulbRadius}px`,
  backgroundColor: localOptions.value.tubeColor,
  borderRadius: `${localOptions.value.tubeWidth / 2}px ${localOptions.value.tubeWidth / 2}px 0 0`,
  position: 'relative' as const,
  overflow: 'hidden' as const,
  border: '2px solid #999'
}))

const bulbStyle = computed(() => ({
  width: `${localOptions.value.bulbRadius * 2}px`,
  height: `${localOptions.value.bulbRadius * 2}px`,
  borderRadius: '50%',
  backgroundColor: localOptions.value.tubeColor,
  border: '2px solid #999',
  position: 'relative' as const,
  marginTop: '-2px'
}))

const bulbLiquidStyle = computed(() => ({
  width: '100%',
  height: '100%',
  borderRadius: '50%',
  backgroundColor: localOptions.value.liquidColor,
  transform: 'scale(0.9)'
}))

const liquidStyle = computed(() => {
  const percentage = ((currentValue.value - (props.data.min || -50)) / 
                     ((props.data.max || 150) - (props.data.min || -50))) * 100
  
  return {
    position: 'absolute' as const,
    bottom: '0',
    left: '0',
    width: '100%',
    height: `${Math.max(0, Math.min(100, percentage))}%`,
    backgroundColor: localOptions.value.liquidColor,
    transition: localOptions.value.animation ? `height ${localOptions.value.animationDuration}ms ease` : 'none',
    borderRadius: percentage >= 95 ? `${localOptions.value.tubeWidth / 2}px ${localOptions.value.tubeWidth / 2}px 0 0` : '0'
  }
})

const valueStyle = computed(() => ({
  marginTop: '12px',
  textAlign: 'center' as const,
  fontSize: '16px',
  fontWeight: 'bold'
}))

const scaleTicks = computed(() => {
  const min = props.data.min || -50
  const max = props.data.max || 150
  const range = max - min
  const majorTickCount = 10
  
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
      position: 100 - position, // 从上到下
      major: true,
      showLabel: true
    })
  }
  
  return ticks
})

const currentAlarm = computed(() => {
  const value = currentValue.value
  for (const zone of alarmZones.value) {
    if (value >= zone.min && value <= zone.max) {
      return {
        type: 'danger' as const,
        message: zone.label || '温度报警'
      }
    }
  }
  return null
})

// 方法
const getTickStyle = (tick: any) => ({
  position: 'absolute' as const,
  right: `${localOptions.value.width + 5}px`,
  top: `${tick.position}%`,
  transform: 'translateY(-50%)',
  display: 'flex',
  alignItems: 'center',
  fontSize: '12px',
  color: '#666'
})

const getAlarmZoneStyle = (zone: AlarmZone) => {
  const min = props.data.min || -50
  const max = props.data.max || 150
  const range = max - min
  
  const startPercent = ((zone.min - min) / range) * 100
  const endPercent = ((zone.max - min) / range) * 100
  const sizePercent = endPercent - startPercent
  
  return {
    position: 'absolute' as const,
    bottom: `${Math.max(0, startPercent)}%`,
    left: '0',
    width: '100%',
    height: `${Math.min(100 - Math.max(0, startPercent), sizePercent)}%`,
    backgroundColor: zone.color,
    opacity: 0.3,
    zIndex: 1
  }
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

const updateUnit = () => {
  emit('unitChange', currentUnit.value)
}

const updateThermometer = () => {
  // 触发重新渲染
}

const toggleUnit = () => {
  const units = ['°C', '°F', 'K']
  const currentIndex = units.indexOf(currentUnit.value)
  currentUnit.value = units[(currentIndex + 1) % units.length]
  updateUnit()
  ElMessage.info(`温度单位已切换为: ${currentUnit.value}`)
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
    label: '高温警告'
  })
}

const removeAlarmZone = (index: number) => {
  alarmZones.value.splice(index, 1)
}

const saveAlarmSettings = () => {
  showAlarmDialog.value = false
  ElMessage.success('温度报警设置已保存')
}

const exportData = () => {
  const data = {
    current: {
      value: currentValue.value,
      unit: currentUnit.value,
      timestamp: new Date()
    },
    range: {
      min: props.data.min || -50,
      max: props.data.max || 150
    },
    options: localOptions.value,
    alarms: alarmZones.value
  }
  
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `thermometer_data_${Date.now()}.json`
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
  // 初始化
})

// 暴露方法
defineExpose({
  updateValue,
  toggleUnit,
  exportData,
  getCurrentValue: () => currentValue.value,
  getCurrentUnit: () => currentUnit.value
})
</script>

<style lang="scss" scoped>
.thermometer-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  
  .thermometer-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    border-bottom: 1px solid #e4e7ed;
    background: #fafafa;
    
    .thermometer-title {
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
  
  .thermometer-container {
    flex: 1;
    padding: 20px;
    position: relative;
  }
  
  .thermometer-body {
    .thermometer-scale {
      position: absolute;
      left: 0;
      top: 0;
      height: 100%;
      
      .scale-tick {
        .tick-line {
          display: inline-block;
          width: 8px;
          height: 2px;
          background: #909399;
          margin-right: 4px;
        }
        
        &.major-tick .tick-line {
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
  
  .thermometer-tube {
    .alarm-zone {
      pointer-events: none;
    }
  }
  
  .thermometer-value {
    .value-text {
      color: #409eff;
    }
    
    .value-unit {
      color: #909399;
      margin-left: 4px;
    }
  }
  
  .thermometer-controls {
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
  
  .thermometer-status {
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
  .thermometer-wrapper {
    .thermometer-toolbar {
      flex-direction: column;
      gap: 8px;
      align-items: flex-start;
    }
    
    .thermometer-status {
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
