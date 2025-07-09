<template>
  <div class="professional-button-array" :class="containerClass">
    <div class="array-container" :style="containerStyle">
      <!-- 标题 -->
      <div v-if="data.title" class="array-title" :style="titleStyle">
        {{ data.title }}
      </div>
      
      <!-- 按钮网格 -->
      <div class="button-grid" :style="gridStyle">
        <div 
          v-for="(button, index) in buttons" 
          :key="index"
          class="button-item"
          :class="getButtonClass(button, index)"
          :style="getButtonStyle(button, index)"
          @click="handleButtonClick(button, index)"
          @mousedown="handleMouseDown(index)"
          @mouseup="handleMouseUp(index)"
          @mouseleave="handleMouseLeave(index)"
          @touchstart="handleTouchStart(index)"
          @touchend="handleTouchEnd(index)"
        >
          <!-- 按钮内容 -->
          <div class="button-content" :style="getContentStyle(button)">
            <!-- 图标 -->
            <div v-if="button.icon" class="button-icon" :style="getIconStyle(button)">
              <component :is="button.icon" v-if="typeof button.icon === 'object'" />
              <i v-else :class="button.icon"></i>
            </div>
            
            <!-- 文字 -->
            <div v-if="button.label" class="button-label" :style="getLabelStyle(button)">
              {{ button.label }}
            </div>
            
            <!-- 状态指示器 -->
            <div v-if="options.showStatusIndicator" class="status-indicator" :style="getIndicatorStyle(button)">
              <div class="indicator-dot" :style="getIndicatorDotStyle(button)"></div>
            </div>
          </div>
          
          <!-- 按钮边框效果 -->
          <div class="button-border" :style="getBorderStyle(button)"></div>
          
          <!-- 按钮发光效果 -->
          <div v-if="button.active && options.glowEffect" class="button-glow" :style="getGlowStyle(button)"></div>
        </div>
      </div>
      
      <!-- 状态信息 -->
      <div v-if="options.showStatusInfo" class="status-info" :style="statusInfoStyle">
        <div class="info-item">
          <span class="info-label">激活:</span>
          <span class="info-value">{{ activeCount }}</span>
        </div>
        <div class="info-item">
          <span class="info-label">总数:</span>
          <span class="info-value">{{ buttons.length }}</span>
        </div>
        <div class="info-item">
          <span class="info-label">最后点击:</span>
          <span class="info-value">{{ lastClickedLabel || '无' }}</span>
        </div>
      </div>
    </div>
    
    <!-- 控制面板 -->
    <div v-if="options.showControls" class="control-panel">
      <div class="control-section">
        <h4>布局控制</h4>
        <div class="control-group">
          <label>列数:</label>
          <el-slider
            v-model="localOptions.columns"
            :min="1"
            :max="8"
            :step="1"
            @change="updateLayout"
          />
        </div>
        
        <div class="control-group">
          <label>按钮大小:</label>
          <el-slider
            v-model="localOptions.buttonSize"
            :min="30"
            :max="100"
            :step="5"
            @change="updateSize"
          />
        </div>
        
        <div class="control-group">
          <label>间距:</label>
          <el-slider
            v-model="localOptions.gap"
            :min="2"
            :max="20"
            :step="2"
            @change="updateGap"
          />
        </div>
      </div>
      
      <div class="control-section">
        <h4>样式控制</h4>
        <div class="control-group">
          <label>样式:</label>
          <el-select v-model="localOptions.style" @change="updateStyle">
            <el-option label="现代" value="modern" />
            <el-option label="经典" value="classic" />
            <el-option label="工业" value="industrial" />
            <el-option label="极简" value="minimal" />
          </el-select>
        </div>
        
        <div class="control-group">
          <label>形状:</label>
          <el-select v-model="localOptions.shape" @change="updateShape">
            <el-option label="圆形" value="circle" />
            <el-option label="方形" value="square" />
            <el-option label="圆角方形" value="rounded" />
          </el-select>
        </div>
      </div>
      
      <div class="control-section">
        <h4>功能控制</h4>
        <div class="control-group">
          <el-button @click="toggleAllButtons" type="primary">
            {{ allActive ? '全部关闭' : '全部激活' }}
          </el-button>
          <el-button @click="resetAllButtons" type="warning">
            重置状态
          </el-button>
          <el-button @click="randomizeButtons" type="success">
            随机状态
          </el-button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'

interface ButtonData {
  id?: string | number
  label?: string
  icon?: any
  active?: boolean
  disabled?: boolean
  color?: string
  backgroundColor?: string
  value?: any
  type?: 'toggle' | 'momentary' | 'indicator'
}

interface ButtonArrayData {
  title?: string
  buttons: ButtonData[]
}

interface ButtonArrayOptions {
  columns?: number
  rows?: number
  buttonSize?: number
  gap?: number
  style?: 'modern' | 'classic' | 'industrial' | 'minimal'
  shape?: 'circle' | 'square' | 'rounded'
  showStatusIndicator?: boolean
  showStatusInfo?: boolean
  showControls?: boolean
  glowEffect?: boolean
  animation?: boolean
  multiSelect?: boolean
  soundEnabled?: boolean
  hapticFeedback?: boolean
}

const props = withDefaults(defineProps<{
  data: ButtonArrayData
  options?: ButtonArrayOptions
}>(), {
  options: () => ({})
})

const emit = defineEmits<{
  'button-click': [button: ButtonData, index: number]
  'button-change': [button: ButtonData, index: number, active: boolean]
  'array-change': [activeButtons: ButtonData[]]
  'status-change': [status: { activeCount: number; totalCount: number; lastClicked?: ButtonData }]
}>()

// 响应式数据
const buttons = ref<ButtonData[]>([...props.data.buttons])
const pressedButtons = ref<Set<number>>(new Set())
const lastClickedIndex = ref<number>(-1)
const lastClickedLabel = ref<string>('')

const localOptions = ref({
  columns: 4,
  rows: 0,
  buttonSize: 50,
  gap: 8,
  style: 'modern' as const,
  shape: 'rounded' as const,
  showStatusIndicator: true,
  showStatusInfo: true,
  showControls: false,
  glowEffect: true,
  animation: true,
  multiSelect: true,
  soundEnabled: false,
  hapticFeedback: true,
  ...props.options
})

// 计算属性
const containerClass = computed(() => ({
  [`array-${localOptions.value.style}`]: true,
  [`shape-${localOptions.value.shape}`]: true,
  'array-animated': localOptions.value.animation
}))

const containerStyle = computed(() => ({
  padding: `${localOptions.value.gap * 2}px`
}))

const titleStyle = computed(() => ({
  fontSize: `${localOptions.value.buttonSize * 0.3}px`,
  fontWeight: '600',
  color: '#374151',
  marginBottom: `${localOptions.value.gap * 2}px`,
  textAlign: 'center' as const
}))

const gridStyle = computed(() => {
  const columns = localOptions.value.columns
  const gap = localOptions.value.gap
  
  return {
    display: 'grid',
    gridTemplateColumns: `repeat(${columns}, 1fr)`,
    gap: `${gap}px`,
    justifyItems: 'center',
    alignItems: 'center'
  }
})

const statusInfoStyle = computed(() => ({
  marginTop: `${localOptions.value.gap * 2}px`,
  padding: `${localOptions.value.gap}px`,
  backgroundColor: 'rgba(0, 0, 0, 0.05)',
  borderRadius: '8px',
  display: 'flex',
  justifyContent: 'space-around',
  fontSize: '12px'
}))

const activeCount = computed(() => 
  buttons.value.filter(btn => btn.active).length
)

const allActive = computed(() => 
  buttons.value.every(btn => btn.active || btn.disabled)
)

// 按钮样式计算
const getButtonClass = (button: ButtonData, index: number) => ({
  'button-active': button.active,
  'button-disabled': button.disabled,
  'button-pressed': pressedButtons.value.has(index),
  'button-last-clicked': lastClickedIndex.value === index
})

const getButtonStyle = (button: ButtonData, index: number) => {
  const size = localOptions.value.buttonSize
  const isPressed = pressedButtons.value.has(index)
  const scale = isPressed ? 0.95 : 1
  
  let borderRadius = '0px'
  if (localOptions.value.shape === 'circle') {
    borderRadius = '50%'
  } else if (localOptions.value.shape === 'rounded') {
    borderRadius = `${size * 0.2}px`
  }
  
  let backgroundColor = button.backgroundColor || getDefaultBackgroundColor(button)
  let borderColor = button.active ? getActiveBorderColor() : '#d1d5db'
  
  return {
    width: `${size}px`,
    height: `${size}px`,
    borderRadius,
    backgroundColor,
    border: `2px solid ${borderColor}`,
    transform: `scale(${scale})`,
    transition: localOptions.value.animation ? 'all 0.2s cubic-bezier(0.4, 0, 0.2, 1)' : 'none',
    cursor: button.disabled ? 'not-allowed' : 'pointer',
    opacity: button.disabled ? 0.5 : 1,
    boxShadow: button.active 
      ? `0 0 20px ${backgroundColor}40, 0 4px 8px rgba(0,0,0,0.1)`
      : '0 2px 4px rgba(0,0,0,0.1)',
    position: 'relative' as const,
    overflow: 'hidden' as const
  }
}

const getContentStyle = (button: ButtonData) => ({
  display: 'flex',
  flexDirection: 'column' as const,
  alignItems: 'center',
  justifyContent: 'center',
  height: '100%',
  color: button.color || (button.active ? '#ffffff' : '#374151'),
  zIndex: 2,
  position: 'relative' as const
})

const getIconStyle = (button: ButtonData) => ({
  fontSize: `${localOptions.value.buttonSize * 0.4}px`,
  marginBottom: button.label ? '4px' : '0'
})

const getLabelStyle = (button: ButtonData) => ({
  fontSize: `${localOptions.value.buttonSize * 0.2}px`,
  fontWeight: '600',
  textAlign: 'center' as const,
  lineHeight: '1.2'
})

const getIndicatorStyle = (button: ButtonData) => ({
  position: 'absolute' as const,
  top: '4px',
  right: '4px',
  zIndex: 3
})

const getIndicatorDotStyle = (button: ButtonData) => {
  const dotSize = localOptions.value.buttonSize * 0.15
  return {
    width: `${dotSize}px`,
    height: `${dotSize}px`,
    borderRadius: '50%',
    backgroundColor: button.active ? '#10b981' : '#e5e7eb',
    boxShadow: button.active ? '0 0 8px #10b98180' : 'none',
    transition: localOptions.value.animation ? 'all 0.3s ease' : 'none'
  }
}

const getBorderStyle = (button: ButtonData) => ({
  position: 'absolute' as const,
  top: '0',
  left: '0',
  right: '0',
  bottom: '0',
  border: '1px solid rgba(255, 255, 255, 0.2)',
  borderRadius: 'inherit',
  pointerEvents: 'none' as const,
  zIndex: 1
})

const getGlowStyle = (button: ButtonData) => {
  const glowColor = button.backgroundColor || getDefaultBackgroundColor(button)
  return {
    position: 'absolute' as const,
    top: '-2px',
    left: '-2px',
    right: '-2px',
    bottom: '-2px',
    background: `radial-gradient(circle, ${glowColor}40 0%, transparent 70%)`,
    borderRadius: 'inherit',
    pointerEvents: 'none' as const,
    zIndex: 0,
    animation: localOptions.value.animation ? 'pulse 2s infinite' : 'none'
  }
}

// 辅助函数
const getDefaultBackgroundColor = (button: ButtonData) => {
  if (button.active) {
    switch (localOptions.value.style) {
      case 'modern': return '#3b82f6'
      case 'classic': return '#059669'
      case 'industrial': return '#dc2626'
      case 'minimal': return '#6b7280'
      default: return '#3b82f6'
    }
  }
  return '#f3f4f6'
}

const getActiveBorderColor = () => {
  switch (localOptions.value.style) {
    case 'modern': return '#2563eb'
    case 'classic': return '#047857'
    case 'industrial': return '#b91c1c'
    case 'minimal': return '#4b5563'
    default: return '#2563eb'
  }
}

// 事件处理
const handleButtonClick = (button: ButtonData, index: number) => {
  if (button.disabled) return
  
  const newActive = !button.active
  
  // 更新按钮状态
  if (localOptions.value.multiSelect || button.type === 'toggle') {
    buttons.value[index].active = newActive
  } else {
    // 单选模式：关闭其他按钮
    buttons.value.forEach((btn, i) => {
      btn.active = i === index ? newActive : false
    })
  }
  
  lastClickedIndex.value = index
  lastClickedLabel.value = button.label || `按钮${index + 1}`
  
  // 触觉反馈
  if (localOptions.value.hapticFeedback && 'vibrate' in navigator) {
    navigator.vibrate(30)
  }
  
  // 音效反馈
  if (localOptions.value.soundEnabled) {
    playClickSound(newActive)
  }
  
  // 发送事件
  emit('button-click', button, index)
  emit('button-change', button, index, newActive)
  emit('array-change', buttons.value.filter(btn => btn.active))
  emit('status-change', {
    activeCount: activeCount.value,
    totalCount: buttons.value.length,
    lastClicked: button
  })
}

const handleMouseDown = (index: number) => {
  pressedButtons.value.add(index)
}

const handleMouseUp = (index: number) => {
  pressedButtons.value.delete(index)
}

const handleMouseLeave = (index: number) => {
  pressedButtons.value.delete(index)
}

const handleTouchStart = (index: number) => {
  pressedButtons.value.add(index)
}

const handleTouchEnd = (index: number) => {
  pressedButtons.value.delete(index)
}

const playClickSound = (active: boolean) => {
  try {
    const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)()
    const oscillator = audioContext.createOscillator()
    const gainNode = audioContext.createGain()
    
    oscillator.connect(gainNode)
    gainNode.connect(audioContext.destination)
    
    oscillator.frequency.setValueAtTime(active ? 800 : 400, audioContext.currentTime)
    gainNode.gain.setValueAtTime(0.1, audioContext.currentTime)
    gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.1)
    
    oscillator.start(audioContext.currentTime)
    oscillator.stop(audioContext.currentTime + 0.1)
  } catch (error) {
    console.warn('Audio context not available:', error)
  }
}

// 控制方法
const updateLayout = () => {
  // 布局更新逻辑
}

const updateSize = () => {
  // 大小更新逻辑
}

const updateGap = () => {
  // 间距更新逻辑
}

const updateStyle = () => {
  // 样式更新逻辑
}

const updateShape = () => {
  // 形状更新逻辑
}

const toggleAllButtons = () => {
  const newState = !allActive.value
  buttons.value.forEach(button => {
    if (!button.disabled) {
      button.active = newState
    }
  })
}

const resetAllButtons = () => {
  buttons.value.forEach(button => {
    button.active = false
  })
  lastClickedIndex.value = -1
  lastClickedLabel.value = ''
}

const randomizeButtons = () => {
  buttons.value.forEach(button => {
    if (!button.disabled) {
      button.active = Math.random() > 0.5
    }
  })
}

// 监听器
watch(() => props.data.buttons, (newButtons) => {
  buttons.value = [...newButtons]
}, { deep: true })

watch(() => props.options, (newOptions) => {
  Object.assign(localOptions.value, newOptions)
}, { deep: true })

// 生命周期
onMounted(() => {
  buttons.value = [...props.data.buttons]
})
</script>

<style scoped lang="scss">
.professional-button-array {
  display: inline-block;
  user-select: none;
  
  .array-container {
    background: rgba(255, 255, 255, 0.8);
    border-radius: 12px;
    backdrop-filter: blur(10px);
    border: 1px solid rgba(255, 255, 255, 0.2);
    
    .array-title {
      text-align: center;
      font-weight: 600;
    }
    
    .button-grid {
      .button-item {
        position: relative;
        display: flex;
        align-items: center;
        justify-content: center;
        
        &:hover:not(.button-disabled) {
          transform: scale(1.05) !important;
        }
        
        &:active:not(.button-disabled) {
          transform: scale(0.95) !important;
        }
        
        &.button-last-clicked {
          animation: highlight 0.5s ease-out;
        }
        
        .button-content {
          .button-icon {
            display: flex;
            align-items: center;
            justify-content: center;
          }
          
          .button-label {
            word-break: break-all;
            max-width: 100%;
          }
        }
        
        .status-indicator {
          .indicator-dot {
            position: relative;
            
            &::after {
              content: '';
              position: absolute;
              top: 50%;
              left: 50%;
              transform: translate(-50%, -50%);
              width: 150%;
              height: 150%;
              border-radius: 50%;
              background: inherit;
              opacity: 0.3;
              animation: ripple 2s infinite;
            }
          }
        }
      }
    }
    
    .status-info {
      .info-item {
        text-align: center;
        
        .info-label {
          display: block;
          color: #6b7280;
          margin-bottom: 2px;
        }
        
        .info-value {
          font-weight: bold;
          color: #374151;
        }
      }
    }
  }
  
  .control-panel {
    margin-top: 20px;
    padding: 20px;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 12px;
    backdrop-filter: blur(10px);
    
    .control-section {
      margin-bottom: 20px;
      
      &:last-child {
        margin-bottom: 0;
      }
      
      h4 {
        margin: 0 0 15px 0;
        color: #374151;
        font-size: 14px;
        font-weight: 600;
      }
      
      .control-group {
        display: flex;
        align-items: center;
        margin-bottom: 12px;
        gap: 12px;
        
        &:last-child {
          margin-bottom: 0;
        }
        
        label {
          min-width: 60px;
          color: #374151;
          font-size: 12px;
          font-weight: 500;
        }
        
        .el-slider {
          flex: 1;
        }
        
        .el-select {
          flex: 1;
        }
      }
    }
  }
  
  // 样式变体
  &.array-modern {
    .button-item {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      
      &.button-active {
        background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
      }
    }
  }
  
  &.array-classic {
    .button-item {
      background: #f9fafb;
      border: 2px solid #d1d5db !important;
      
      &.button-active {
        background: #059669;
        border-color: #047857 !important;
      }
    }
  }
  
  &.array-industrial {
    .button-item {
      background: #374151;
      border: 2px solid #6b7280 !important;
      
      &.button-active {
        background: #dc2626;
        border-color: #b91c1c !important;
      }
    }
  }
  
  &.array-minimal {
    .button-item {
      background: transparent;
      border: 1px solid #d1d5db !important;
      
      &.button-active {
        background: #6b7280;
        border-color: #4b5563 !important;
      }
    }
  }
  
  &.shape-circle {
    .button-item {
      border-radius: 50% !important;
    }
  }
  
  &.shape-square {
    .button-item {
      border-radius: 0 !important;
    }
  }
  
  &.array-animated {
    .button-item {
      transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    }
  }
}

@keyframes highlight {
  0% { box-shadow: 0 0 0 0 rgba(59, 130, 246, 0.7); }
  70% { box-shadow: 0 0 0 10px rgba(59, 130, 246, 0); }
  100% { box-shadow: 0 0 0 0 rgba(59, 130, 246, 0); }
}

@keyframes ripple {
  0% { transform: translate(-50%, -50%) scale(0); opacity: 1; }
  100% { transform: translate(-50%, -50%) scale(1); opacity: 0; }
}

@keyframes pulse {
  0%, 100% { opacity: 0.5; }
  50% { opacity: 1; }
}

@media (max-width: 768px) {
  .professional-button-array {
    .control-panel {
      .control-section {
        .control-group {
          flex-direction: column;
          align-items: stretch;
          
          label {
            min-width: auto;
            margin-bottom: 4px;
          }
        }
      }
    }
  }
}
</style>
