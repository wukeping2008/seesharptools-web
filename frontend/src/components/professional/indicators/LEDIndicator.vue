<template>
  <div class="led-indicator-wrapper professional-control">
    <!-- 工具栏 -->
    <div class="led-toolbar" v-if="showToolbar">
      <div class="toolbar-left">
        <span class="led-title">{{ data.title || 'LED指示器' }}</span>
      </div>
      <div class="toolbar-right">
        <el-tooltip content="切换状态">
          <el-button size="small" @click="toggleState">
            <el-icon><Switch /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="闪烁模式">
          <el-button size="small" @click="toggleBlink">
            <el-icon><Timer /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="设置颜色">
          <el-button size="small" @click="showColorDialog = true">
            <el-icon><Brush /></el-icon>
          </el-button>
        </el-tooltip>
        <el-tooltip content="导出配置">
          <el-button size="small" @click="exportConfig">
            <el-icon><Download /></el-icon>
          </el-button>
        </el-tooltip>
      </div>
    </div>

    <!-- LED指示器容器 -->
    <div 
      ref="ledContainer" 
      class="led-container"
      :style="containerStyle"
    >
      <!-- LED主体 -->
      <div class="led-body" :style="bodyStyle">
        <!-- LED灯组 -->
        <div class="led-group" :style="ledGroupStyle">
          <div 
            v-for="(led, index) in ledArray" 
            :key="index"
            class="led-item"
            :style="getLedItemStyle(led, index)"
            :class="getLedItemClass(led)"
            @click="handleLedClick(index)"
          >
            <!-- LED核心 -->
            <div class="led-core" :style="getLedCoreStyle(led)">
              <!-- LED光晕 -->
              <div 
                v-if="led.state && localOptions.showGlow" 
                class="led-glow"
                :style="getLedGlowStyle(led)"
              ></div>
              
              <!-- LED反射 -->
              <div 
                v-if="localOptions.showReflection" 
                class="led-reflection"
                :style="getLedReflectionStyle(led)"
              ></div>
              
              <!-- LED状态指示 -->
              <div 
                v-if="localOptions.showStatusText && led.label" 
                class="led-status-text"
                :style="getStatusTextStyle(led)"
              >
                {{ led.label }}
              </div>
            </div>
            
            <!-- LED标签 -->
            <div 
              v-if="localOptions.showLabels && led.label" 
              class="led-label"
              :style="getLedLabelStyle(led)"
            >
              {{ led.label }}
            </div>
          </div>
        </div>
        
        <!-- LED控制面板 -->
        <div class="led-control-panel" v-if="localOptions.showControlPanel" :style="controlPanelStyle">
          <div class="panel-header">
            <span class="panel-title">LED控制面板</span>
            <el-button size="small" @click="toggleAllLeds">
              {{ allLedsOn ? '全部关闭' : '全部开启' }}
            </el-button>
          </div>
          
          <div class="panel-content">
            <div class="led-controls">
              <div 
                v-for="(led, index) in ledArray" 
                :key="index"
                class="led-control-item"
              >
                <el-switch 
                  v-model="led.state" 
                  @change="handleLedStateChange(index, $event)"
                  :active-color="led.color"
                />
                <span class="control-label">{{ led.label || `LED ${index + 1}` }}</span>
              </div>
            </div>
          </div>
        </div>
        
        <!-- LED状态显示 -->
        <div class="led-status-display" v-if="localOptions.showStatusDisplay" :style="statusDisplayStyle">
          <div class="status-item">
            <span class="status-label">总数:</span>
            <span class="status-value">{{ ledArray.length }}</span>
          </div>
          <div class="status-item">
            <span class="status-label">开启:</span>
            <span class="status-value">{{ activeLedsCount }}</span>
          </div>
          <div class="status-item">
            <span class="status-label">关闭:</span>
            <span class="status-value">{{ inactiveLedsCount }}</span>
          </div>
          <div class="status-item">
            <span class="status-label">闪烁:</span>
            <span class="status-value">{{ blinkingLedsCount }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 控制面板 -->
    <div class="led-controls" v-if="showControls">
      <el-row :gutter="16">
        <el-col :span="6">
          <div class="control-group">
            <label>LED数量:</label>
            <el-input-number 
              v-model="localOptions.ledCount" 
              :min="1" 
              :max="20"
              @change="updateLedCount"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>LED大小:</label>
            <el-input-number 
              v-model="localOptions.ledSize" 
              :min="10" 
              :max="100"
              @change="updateDisplay"
              size="small"
            />
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>排列方式:</label>
            <el-select v-model="localOptions.layout" @change="updateDisplay" size="small">
              <el-option label="水平" value="horizontal" />
              <el-option label="垂直" value="vertical" />
              <el-option label="网格" value="grid" />
              <el-option label="圆形" value="circular" />
            </el-select>
          </div>
        </el-col>
        
        <el-col :span="6">
          <div class="control-group">
            <label>LED形状:</label>
            <el-select v-model="localOptions.ledShape" @change="updateDisplay" size="small">
              <el-option label="圆形" value="circle" />
              <el-option label="方形" value="square" />
              <el-option label="菱形" value="diamond" />
              <el-option label="六边形" value="hexagon" />
            </el-select>
          </div>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showGlow" @change="updateDisplay">
            显示光晕
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showReflection" @change="updateDisplay">
            显示反射
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showLabels" @change="updateDisplay">
            显示标签
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showControlPanel" @change="updateDisplay">
            控制面板
          </el-checkbox>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="6">
          <el-checkbox v-model="localOptions.clickable" @change="updateDisplay">
            可点击
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showStatusText" @change="updateDisplay">
            状态文字
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <el-checkbox v-model="localOptions.showStatusDisplay" @change="updateDisplay">
            状态统计
          </el-checkbox>
        </el-col>
        <el-col :span="6">
          <div class="control-group">
            <label>闪烁速度:</label>
            <el-slider 
              v-model="localOptions.blinkSpeed" 
              :min="100" 
              :max="2000"
              @change="updateBlinkSpeed"
              size="small"
            />
          </div>
        </el-col>
      </el-row>
      
      <el-row :gutter="16" style="margin-top: 12px;">
        <el-col :span="8">
          <div class="control-group">
            <label>默认颜色:</label>
            <el-color-picker v-model="localOptions.defaultColor" @change="updateDisplay" size="small" />
          </div>
        </el-col>
        
        <el-col :span="8">
          <div class="control-group">
            <label>关闭颜色:</label>
            <el-color-picker v-model="localOptions.offColor" @change="updateDisplay" size="small" />
          </div>
        </el-col>
        
        <el-col :span="8">
          <div class="control-group">
            <label>背景颜色:</label>
            <el-color-picker v-model="localOptions.backgroundColor" @change="updateDisplay" size="small" />
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 状态信息 -->
    <div class="led-status-bar" v-if="showStatus">
      <span>LED总数: {{ ledArray.length }}</span>
      <span>开启: {{ activeLedsCount }}</span>
      <span>关闭: {{ inactiveLedsCount }}</span>
      <span>闪烁: {{ blinkingLedsCount }}</span>
      <span>排列: {{ layoutNames[localOptions.layout] }}</span>
      <span>形状: {{ shapeNames[localOptions.ledShape] }}</span>
    </div>

    <!-- 颜色设置对话框 -->
    <el-dialog v-model="showColorDialog" title="LED颜色设置" width="600px">
      <div class="color-settings">
        <h4>LED颜色配置</h4>
        <div v-for="(led, index) in ledArray" :key="index" class="color-config-item">
          <el-row :gutter="16">
            <el-col :span="4">
              <span>LED {{ index + 1 }}:</span>
            </el-col>
            <el-col :span="6">
              <el-color-picker v-model="led.color" />
            </el-col>
            <el-col :span="8">
              <el-input v-model="led.label" placeholder="标签" size="small" />
            </el-col>
            <el-col :span="4">
              <el-switch v-model="led.blinking" active-text="闪烁" />
            </el-col>
            <el-col :span="2">
              <el-button size="small" @click="resetLedColor(index)">
                <el-icon><Refresh /></el-icon>
              </el-button>
            </el-col>
          </el-row>
        </div>
        
        <div class="color-presets">
          <h5>颜色预设</h5>
          <div class="preset-buttons">
            <el-button size="small" @click="applyColorPreset('traffic')">交通灯</el-button>
            <el-button size="small" @click="applyColorPreset('status')">状态灯</el-button>
            <el-button size="small" @click="applyColorPreset('rainbow')">彩虹色</el-button>
            <el-button size="small" @click="applyColorPreset('warning')">警告色</el-button>
            <el-button size="small" @click="applyColorPreset('cool')">冷色调</el-button>
            <el-button size="small" @click="applyColorPreset('warm')">暖色调</el-button>
          </div>
        </div>
      </div>
      <template #footer>
        <el-button @click="showColorDialog = false">取消</el-button>
        <el-button type="primary" @click="saveColorSettings">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { Switch, Timer, Brush, Download, Refresh } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import type { GaugeData, GaugeEvents } from '@/types/gauge'

// LED配置接口
interface LEDOptions {
  ledCount: number
  ledSize: number
  layout: 'horizontal' | 'vertical' | 'grid' | 'circular'
  ledShape: 'circle' | 'square' | 'diamond' | 'hexagon'
  showGlow: boolean
  showReflection: boolean
  showLabels: boolean
  showControlPanel: boolean
  showStatusText: boolean
  showStatusDisplay: boolean
  clickable: boolean
  defaultColor: string
  offColor: string
  backgroundColor: string
  blinkSpeed: number
  animation: boolean
}

// LED项目接口
interface LEDItem {
  id: number
  state: boolean
  color: string
  label: string
  blinking: boolean
  brightness: number
}

// Props
interface Props {
  data: GaugeData
  options?: Partial<LEDOptions>
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
  height: '200px'
})

// Emits
const emit = defineEmits<{
  stateChange: [index: number, state: boolean]
  ledClick: [index: number, led: LEDItem]
  allStateChange: [allOn: boolean]
  blinkToggle: [blinking: boolean]
}>()

// 响应式数据
const ledContainer = ref<HTMLDivElement>()
const showColorDialog = ref(false)
const ledArray = ref<LEDItem[]>([])

// 默认配置
const defaultOptions: LEDOptions = {
  ledCount: 8,
  ledSize: 30,
  layout: 'horizontal',
  ledShape: 'circle',
  showGlow: true,
  showReflection: true,
  showLabels: true,
  showControlPanel: false,
  showStatusText: false,
  showStatusDisplay: true,
  clickable: true,
  defaultColor: '#00ff00',
  offColor: '#333333',
  backgroundColor: 'transparent',
  blinkSpeed: 500,
  animation: true
}

const localOptions = ref<LEDOptions>({ ...defaultOptions, ...props.options })

// 布局和形状名称映射
const layoutNames = {
  horizontal: '水平',
  vertical: '垂直',
  grid: '网格',
  circular: '圆形'
}

const shapeNames = {
  circle: '圆形',
  square: '方形',
  diamond: '菱形',
  hexagon: '六边形'
}

// 闪烁定时器
let blinkTimer: number | null = null

// 计算属性
const containerStyle = computed(() => ({
  width: typeof props.width === 'number' ? `${props.width}px` : props.width,
  height: typeof props.height === 'number' ? `${props.height}px` : props.height,
  display: 'flex',
  flexDirection: 'column' as const,
  alignItems: 'center',
  justifyContent: 'center',
  backgroundColor: localOptions.value.backgroundColor
}))

const bodyStyle = computed(() => ({
  position: 'relative' as const,
  width: '100%',
  height: '100%',
  display: 'flex',
  flexDirection: 'column' as const,
  alignItems: 'center',
  justifyContent: 'center'
}))

const ledGroupStyle = computed(() => {
  const { layout, ledCount, ledSize } = localOptions.value
  
  switch (layout) {
    case 'horizontal':
      return {
        display: 'flex',
        flexDirection: 'row' as const,
        gap: `${ledSize * 0.3}px`,
        alignItems: 'center',
        justifyContent: 'center'
      }
    case 'vertical':
      return {
        display: 'flex',
        flexDirection: 'column' as const,
        gap: `${ledSize * 0.3}px`,
        alignItems: 'center',
        justifyContent: 'center'
      }
    case 'grid':
      const cols = Math.ceil(Math.sqrt(ledCount))
      return {
        display: 'grid',
        gridTemplateColumns: `repeat(${cols}, 1fr)`,
        gap: `${ledSize * 0.3}px`,
        alignItems: 'center',
        justifyContent: 'center'
      }
    case 'circular':
      return {
        position: 'relative' as const,
        width: `${ledSize * 4}px`,
        height: `${ledSize * 4}px`,
        borderRadius: '50%'
      }
    default:
      return {}
  }
})

const controlPanelStyle = computed(() => ({
  marginTop: '20px',
  padding: '16px',
  border: '1px solid #e4e7ed',
  borderRadius: '8px',
  backgroundColor: '#f5f7fa',
  minWidth: '300px'
}))

const statusDisplayStyle = computed(() => ({
  marginTop: '16px',
  padding: '12px',
  backgroundColor: '#f0f2f5',
  borderRadius: '6px',
  display: 'flex',
  gap: '16px',
  fontSize: '12px',
  color: '#606266'
}))

const allLedsOn = computed(() => {
  return ledArray.value.every(led => led.state)
})

const activeLedsCount = computed(() => {
  return ledArray.value.filter(led => led.state).length
})

const inactiveLedsCount = computed(() => {
  return ledArray.value.filter(led => !led.state).length
})

const blinkingLedsCount = computed(() => {
  return ledArray.value.filter(led => led.blinking).length
})

// 方法
const getLedItemStyle = (led: LEDItem, index: number) => {
  const { layout, ledSize } = localOptions.value
  
  const baseStyle = {
    position: layout === 'circular' ? 'absolute' as const : 'relative' as const,
    cursor: localOptions.value.clickable ? 'pointer' : 'default',
    transition: 'all 0.3s ease'
  }
  
  if (layout === 'circular') {
    const angle = (index / ledArray.value.length) * 2 * Math.PI
    const radius = ledSize * 1.5
    const x = Math.cos(angle) * radius
    const y = Math.sin(angle) * radius
    
    return {
      ...baseStyle,
      left: `calc(50% + ${x}px)`,
      top: `calc(50% + ${y}px)`,
      transform: 'translate(-50%, -50%)'
    }
  }
  
  return baseStyle
}

const getLedItemClass = (led: LEDItem) => {
  const classes = ['led-item']
  
  if (led.state) {
    classes.push('led-on')
  } else {
    classes.push('led-off')
  }
  
  if (led.blinking) {
    classes.push('led-blinking')
  }
  
  return classes
}

const getLedCoreStyle = (led: LEDItem) => {
  const { ledSize, ledShape } = localOptions.value
  
  const baseStyle = {
    width: `${ledSize}px`,
    height: `${ledSize}px`,
    backgroundColor: led.state ? led.color : localOptions.value.offColor,
    position: 'relative' as const,
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    transition: 'all 0.3s ease',
    opacity: led.state ? (led.brightness / 100) : 0.3
  }
  
  switch (ledShape) {
    case 'circle':
      return {
        ...baseStyle,
        borderRadius: '50%'
      }
    case 'square':
      return {
        ...baseStyle,
        borderRadius: '4px'
      }
    case 'diamond':
      return {
        ...baseStyle,
        borderRadius: '4px',
        transform: 'rotate(45deg)'
      }
    case 'hexagon':
      return {
        ...baseStyle,
        clipPath: 'polygon(50% 0%, 100% 25%, 100% 75%, 50% 100%, 0% 75%, 0% 25%)'
      }
    default:
      return baseStyle
  }
}

const getLedGlowStyle = (led: LEDItem) => {
  const { ledSize } = localOptions.value
  
  return {
    position: 'absolute' as const,
    top: '50%',
    left: '50%',
    transform: 'translate(-50%, -50%)',
    width: `${ledSize * 1.8}px`,
    height: `${ledSize * 1.8}px`,
    borderRadius: '50%',
    backgroundColor: led.color,
    opacity: 0.3,
    filter: 'blur(8px)',
    zIndex: -1,
    animation: led.blinking ? `led-glow-pulse ${localOptions.value.blinkSpeed * 2}ms ease-in-out infinite` : 'none'
  }
}

const getLedReflectionStyle = (led: LEDItem) => {
  const { ledSize } = localOptions.value
  
  return {
    position: 'absolute' as const,
    top: '20%',
    left: '30%',
    width: `${ledSize * 0.3}px`,
    height: `${ledSize * 0.3}px`,
    borderRadius: '50%',
    backgroundColor: 'rgba(255, 255, 255, 0.6)',
    filter: 'blur(2px)'
  }
}

const getStatusTextStyle = (led: LEDItem) => ({
  fontSize: '10px',
  color: led.state ? '#fff' : '#999',
  fontWeight: 'bold',
  textShadow: led.state ? '0 0 2px rgba(0,0,0,0.5)' : 'none'
})

const getLedLabelStyle = (led: LEDItem) => ({
  marginTop: '8px',
  fontSize: '12px',
  color: '#606266',
  textAlign: 'center' as const,
  fontWeight: '500'
})

const initializeLeds = () => {
  ledArray.value = []
  for (let i = 0; i < localOptions.value.ledCount; i++) {
    ledArray.value.push({
      id: i,
      state: false,
      color: localOptions.value.defaultColor,
      label: `LED ${i + 1}`,
      blinking: false,
      brightness: 100
    })
  }
}

const updateLedCount = () => {
  const currentCount = ledArray.value.length
  const newCount = localOptions.value.ledCount
  
  if (newCount > currentCount) {
    // 添加新的LED
    for (let i = currentCount; i < newCount; i++) {
      ledArray.value.push({
        id: i,
        state: false,
        color: localOptions.value.defaultColor,
        label: `LED ${i + 1}`,
        blinking: false,
        brightness: 100
      })
    }
  } else if (newCount < currentCount) {
    // 移除多余的LED
    ledArray.value = ledArray.value.slice(0, newCount)
  }
}

const updateDisplay = () => {
  // 触发重新渲染
}

const updateBlinkSpeed = () => {
  if (blinkTimer) {
    clearInterval(blinkTimer)
    startBlinkAnimation()
  }
}

const handleLedClick = (index: number) => {
  if (!localOptions.value.clickable) return
  
  const led = ledArray.value[index]
  led.state = !led.state
  
  emit('ledClick', index, led)
  emit('stateChange', index, led.state)
  props.events?.onValueChange?.(led.state ? 1 : 0)
}

const handleLedStateChange = (index: number, state: boolean) => {
  const led = ledArray.value[index]
  led.state = state
  
  emit('stateChange', index, state)
  props.events?.onValueChange?.(state ? 1 : 0)
}

const toggleState = () => {
  const firstLed = ledArray.value[0]
  if (firstLed) {
    firstLed.state = !firstLed.state
    emit('stateChange', 0, firstLed.state)
  }
}

const toggleAllLeds = () => {
  const newState = !allLedsOn.value
  ledArray.value.forEach((led, index) => {
    led.state = newState
    emit('stateChange', index, newState)
  })
  emit('allStateChange', newState)
}

const toggleBlink = () => {
  const anyBlinking = ledArray.value.some(led => led.blinking)
  ledArray.value.forEach(led => {
    led.blinking = !anyBlinking
  })
  
  if (!anyBlinking) {
    startBlinkAnimation()
  } else {
    stopBlinkAnimation()
  }
  
  emit('blinkToggle', !anyBlinking)
}

const startBlinkAnimation = () => {
  if (blinkTimer) return
  
  blinkTimer = setInterval(() => {
    ledArray.value.forEach(led => {
      if (led.blinking) {
        led.brightness = led.brightness === 100 ? 30 : 100
      }
    })
  }, localOptions.value.blinkSpeed)
}

const stopBlinkAnimation = () => {
  if (blinkTimer) {
    clearInterval(blinkTimer)
    blinkTimer = null
  }
  
  // 重置亮度
  ledArray.value.forEach(led => {
    led.brightness = 100
  })
}

const resetLedColor = (index: number) => {
  const led = ledArray.value[index]
  led.color = localOptions.value.defaultColor
}

const applyColorPreset = (preset: string) => {
  const presets = {
    traffic: ['#ff0000', '#ffff00', '#00ff00'],
    status: ['#00ff00', '#ffff00', '#ff0000', '#0000ff'],
    rainbow: ['#ff0000', '#ff8000', '#ffff00', '#00ff00', '#0000ff', '#8000ff', '#ff00ff'],
    warning: ['#ff0000', '#ff8000', '#ffff00'],
    cool: ['#00ffff', '#0080ff', '#0000ff', '#8000ff'],
    warm: ['#ff0000', '#ff4000', '#ff8000', '#ffff00']
  }
  
  const colors = presets[preset as keyof typeof presets] || []
  ledArray.value.forEach((led, index) => {
    if (colors[index]) {
      led.color = colors[index]
    }
  })
}

const saveColorSettings = () => {
  showColorDialog.value = false
  ElMessage.success('LED颜色设置已保存')
}

const exportConfig = () => {
  const config = {
    options: localOptions.value,
    leds: ledArray.value,
    timestamp: new Date()
  }
  
  const blob = new Blob([JSON.stringify(config, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `led_config_${Date.now()}.json`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

// 监听数据变化
watch(() => props.data, (newData) => {
  if (newData.value !== undefined) {
    const index = Math.floor(newData.value)
    if (ledArray.value[index]) {
      ledArray.value[index].state = newData.value > 0
    }
  }
}, { deep: true })

// 生命周期
onMounted(() => {
  initializeLeds()
  
  // 检查是否有闪烁的LED
  if (ledArray.value.some(led => led.blinking)) {
    startBlinkAnimation()
  }
})

onUnmounted(() => {
  stopBlinkAnimation()
})

// 暴露方法
defineExpose({
  toggleState,
  toggleAllLeds,
  toggleBlink,
  setLedState: (index: number, state: boolean) => {
    if (ledArray.value[index]) {
      ledArray.value[index].state = state
    }
  },
  setLedColor: (index: number, color: string) => {
    if (ledArray.value[index]) {
      ledArray.value[index].color = color
    }
  },
  setLedBlinking: (index: number, blinking: boolean) => {
    if (ledArray.value[index]) {
      ledArray.value[index].blinking = blinking
      if (blinking) {
        startBlinkAnimation()
      }
    }
  },
  getLedArray: () => ledArray.value,
  resetAllLeds: () => {
    ledArray.value.forEach(led => {
      led.state = false
      led.blinking = false
      led.brightness = 100
    })
    stopBlinkAnimation()
  }
})
</script>

<style lang="scss" scoped>
.led-indicator-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  
  .led-toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    border-bottom: 1px solid #e4e7ed;
    background: #fafafa;
    
    .led-title {
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
  
  .led-container {
    flex: 1;
    padding: 20px;
    position: relative;
  }
  
  .led-body {
    .led-group {
      .led-item {
        display: flex;
        flex-direction: column;
        align-items: center;
        
        &.led-on {
          .led-core {
            box-shadow: 0 0 10px currentColor;
          }
        }
        
        &.led-off {
          .led-core {
            box-shadow: inset 0 2px 4px rgba(0,0,0,0.2);
          }
        }
        
        &.led-blinking {
          .led-core {
            animation: led-blink 1s ease-in-out infinite;
          }
        }
        
        .led-core {
          border: 2px solid rgba(0,0,0,0.1);
          
          .led-glow {
            pointer-events: none;
          }
          
          .led-reflection {
            pointer-events: none;
          }
          
          .led-status-text {
            pointer-events: none;
            user-select: none;
          }
        }
        
        .led-label {
          user-select: none;
        }
      }
    }
    
    .led-control-panel {
      .panel-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 12px;
        
        .panel-title {
          font-weight: 500;
          color: #303133;
        }
      }
      
      .panel-content {
        .led-controls {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
          gap: 12px;
          
          .led-control-item {
            display: flex;
            align-items: center;
            gap: 8px;
            
            .control-label {
              font-size: 12px;
              color: #606266;
            }
          }
        }
      }
    }
    
    .led-status-display {
      .status-item {
        display: flex;
        align-items: center;
        gap: 4px;
        
        .status-label {
          font-weight: 500;
        }
        
        .status-value {
          color: #409eff;
          font-weight: bold;
        }
      }
    }
  }
  
  .led-controls {
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
  
  .led-status-bar {
    padding: 8px 16px;
    background: #f0f2f5;
    border-top: 1px solid #e4e7ed;
    display: flex;
    flex-wrap: wrap;
    gap: 16px;
    font-size: 12px;
    color: #606266;
  }
  
  .color-settings {
    .color-config-item {
      margin-bottom: 12px;
      padding: 12px;
      border: 1px solid #e4e7ed;
      border-radius: 4px;
      background: #fafafa;
      
      span {
        font-weight: 500;
        color: #303133;
      }
    }
    
    .color-presets {
      margin-top: 20px;
      padding-top: 16px;
      border-top: 1px solid #e4e7ed;
      
      h5 {
        margin: 0 0 12px 0;
        color: #303133;
        font-size: 14px;
      }
      
      .preset-buttons {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
      }
    }
    
    h4 {
      margin: 0 0 16px 0;
      color: #303133;
      font-size: 16px;
    }
  }
}

// LED动画
@keyframes led-blink {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.3; }
}

@keyframes led-glow-pulse {
  0%, 100% { 
    opacity: 0.3; 
    transform: translate(-50%, -50%) scale(1);
  }
  50% { 
    opacity: 0.6; 
    transform: translate(-50%, -50%) scale(1.1);
  }
}

// 响应式设计
@media (max-width: 768px) {
  .led-indicator-wrapper {
    .led-toolbar {
      flex-direction: column;
      gap: 8px;
      align-items: flex-start;
      
      .toolbar-left,
      .toolbar-right {
        flex-wrap: wrap;
      }
    }
    
    .led-controls {
      .el-row {
        .el-col {
          margin-bottom: 12px;
        }
      }
    }
    
    .led-status-bar {
      flex-direction: column;
      gap: 8px;
    }
    
    .color-settings {
      .color-config-item {
        .el-row {
          .el-col {
            margin-bottom: 8px;
          }
        }
      }
      
      .preset-buttons {
        flex-direction: column;
        
        .el-button {
          width: 100%;
        }
      }
    }
  }
}
</style>
