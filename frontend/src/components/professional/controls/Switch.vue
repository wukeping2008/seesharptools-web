<template>
  <div class="professional-switch" :class="containerClass">
    <div class="switch-container" :style="containerStyle">
      <!-- 开关主体 -->
      <div 
        class="switch-body"
        :class="switchBodyClass"
        :style="switchBodyStyle"
        @click="handleClick"
        @mousedown="handleMouseDown"
        @mouseup="handleMouseUp"
        @mouseleave="handleMouseLeave"
      >
        <!-- 开关滑块 -->
        <div 
          class="switch-slider"
          :class="sliderClass"
          :style="sliderStyle"
        >
          <!-- 滑块内部指示器 -->
          <div class="slider-indicator" :style="indicatorStyle"></div>
        </div>
        
        <!-- 开关轨道 -->
        <div class="switch-track" :style="trackStyle">
          <!-- 轨道内部装饰 -->
          <div class="track-decoration" :style="decorationStyle"></div>
        </div>
        
        <!-- 状态文字 -->
        <div v-if="options.showLabels" class="switch-labels">
          <span 
            class="label-on" 
            :class="{ active: data.value }"
            :style="labelStyle"
          >
            {{ options.onLabel || 'ON' }}
          </span>
          <span 
            class="label-off" 
            :class="{ active: !data.value }"
            :style="labelStyle"
          >
            {{ options.offLabel || 'OFF' }}
          </span>
        </div>
      </div>
      
      <!-- 开关标题 -->
      <div v-if="data.title" class="switch-title" :style="titleStyle">
        {{ data.title }}
      </div>
      
      <!-- 状态指示灯 -->
      <div v-if="options.showStatusLight" class="status-light" :class="statusLightClass" :style="statusLightStyle">
        <div class="light-core" :style="lightCoreStyle"></div>
      </div>
    </div>
    
    <!-- 控制面板 -->
    <div v-if="options.showControls" class="control-panel">
      <div class="control-group">
        <label>状态:</label>
        <el-switch
          v-model="localValue"
          @change="handleControlChange"
        />
      </div>
      
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
        <label>大小:</label>
        <el-slider
          v-model="localOptions.size"
          :min="20"
          :max="80"
          :step="5"
          @change="updateSize"
        />
      </div>
      
      <div class="control-group">
        <el-button @click="toggleDisabled" :type="localOptions.disabled ? 'danger' : 'primary'">
          {{ localOptions.disabled ? '启用' : '禁用' }}
        </el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'

interface SwitchData {
  value: boolean
  title?: string
  min?: number
  max?: number
}

interface SwitchOptions {
  size?: number
  style?: 'modern' | 'classic' | 'industrial' | 'minimal'
  onColor?: string
  offColor?: string
  sliderColor?: string
  trackColor?: string
  disabled?: boolean
  showLabels?: boolean
  showStatusLight?: boolean
  showControls?: boolean
  onLabel?: string
  offLabel?: string
  animation?: boolean
  hapticFeedback?: boolean
  soundEnabled?: boolean
}

const props = withDefaults(defineProps<{
  data: SwitchData
  options?: SwitchOptions
}>(), {
  options: () => ({})
})

const emit = defineEmits<{
  'value-change': [value: boolean]
  'switch-click': [value: boolean]
  'state-change': [state: { value: boolean; timestamp: number }]
}>()

// 响应式数据
const localValue = ref(props.data.value)
const isPressed = ref(false)
const localOptions = ref({
  size: 40,
  style: 'modern' as const,
  onColor: '#67c23a',
  offColor: '#dcdfe6',
  sliderColor: '#ffffff',
  trackColor: '#f0f0f0',
  disabled: false,
  showLabels: true,
  showStatusLight: true,
  showControls: false,
  onLabel: 'ON',
  offLabel: 'OFF',
  animation: true,
  hapticFeedback: true,
  soundEnabled: false,
  ...props.options
})

// 计算属性
const containerClass = computed(() => ({
  'switch-disabled': localOptions.value.disabled,
  'switch-pressed': isPressed.value,
  [`switch-${localOptions.value.style}`]: true
}))

const containerStyle = computed(() => ({
  opacity: localOptions.value.disabled ? 0.6 : 1,
  cursor: localOptions.value.disabled ? 'not-allowed' : 'pointer'
}))

const switchBodyClass = computed(() => ({
  'switch-on': localValue.value,
  'switch-off': !localValue.value,
  'switch-animated': localOptions.value.animation
}))

const switchBodyStyle = computed(() => {
  const size = localOptions.value.size
  return {
    width: `${size * 2}px`,
    height: `${size}px`,
    borderRadius: `${size / 2}px`,
    backgroundColor: localValue.value ? localOptions.value.onColor : localOptions.value.offColor,
    border: `2px solid ${localValue.value ? localOptions.value.onColor : '#d1d5db'}`,
    transition: localOptions.value.animation ? 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)' : 'none',
    boxShadow: localValue.value 
      ? `0 0 20px ${localOptions.value.onColor}40, inset 0 2px 4px rgba(0,0,0,0.1)`
      : 'inset 0 2px 4px rgba(0,0,0,0.1), 0 1px 2px rgba(0,0,0,0.1)'
  }
})

const sliderClass = computed(() => ({
  'slider-on': localValue.value,
  'slider-off': !localValue.value
}))

const sliderStyle = computed(() => {
  const size = localOptions.value.size
  const sliderSize = size - 8
  const translateX = localValue.value ? size - 4 : 4
  
  return {
    width: `${sliderSize}px`,
    height: `${sliderSize}px`,
    borderRadius: '50%',
    backgroundColor: localOptions.value.sliderColor,
    transform: `translateX(${translateX}px)`,
    transition: localOptions.value.animation ? 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)' : 'none',
    boxShadow: '0 2px 8px rgba(0,0,0,0.15), 0 1px 4px rgba(0,0,0,0.1)',
    border: '1px solid rgba(255,255,255,0.8)'
  }
})

const indicatorStyle = computed(() => {
  const size = localOptions.value.size
  const indicatorSize = (size - 8) * 0.3
  
  return {
    width: `${indicatorSize}px`,
    height: `${indicatorSize}px`,
    borderRadius: '50%',
    backgroundColor: localValue.value ? localOptions.value.onColor : '#9ca3af',
    margin: 'auto',
    transition: localOptions.value.animation ? 'all 0.3s ease' : 'none'
  }
})

const trackStyle = computed(() => {
  const size = localOptions.value.size
  return {
    width: `${size * 2 - 8}px`,
    height: `${size - 8}px`,
    borderRadius: `${(size - 8) / 2}px`,
    backgroundColor: localOptions.value.trackColor,
    position: 'absolute' as const,
    top: '4px',
    left: '4px',
    zIndex: 0
  }
})

const decorationStyle = computed(() => {
  const size = localOptions.value.size
  return {
    width: '100%',
    height: '2px',
    borderRadius: '1px',
    backgroundColor: 'rgba(255,255,255,0.3)',
    position: 'absolute' as const,
    top: '2px'
  }
})

const labelStyle = computed(() => ({
  fontSize: `${localOptions.value.size * 0.25}px`,
  fontWeight: 'bold',
  color: localValue.value ? '#ffffff' : '#6b7280',
  transition: localOptions.value.animation ? 'all 0.3s ease' : 'none'
}))

const titleStyle = computed(() => ({
  fontSize: `${localOptions.value.size * 0.35}px`,
  fontWeight: '600',
  color: '#374151',
  marginTop: '8px',
  textAlign: 'center' as const
}))

const statusLightClass = computed(() => ({
  'status-on': localValue.value,
  'status-off': !localValue.value
}))

const statusLightStyle = computed(() => {
  const lightSize = localOptions.value.size * 0.3
  return {
    width: `${lightSize}px`,
    height: `${lightSize}px`,
    borderRadius: '50%',
    backgroundColor: localValue.value ? localOptions.value.onColor : '#e5e7eb',
    border: '2px solid #ffffff',
    boxShadow: localValue.value 
      ? `0 0 10px ${localOptions.value.onColor}80`
      : '0 1px 3px rgba(0,0,0,0.1)',
    transition: localOptions.value.animation ? 'all 0.3s ease' : 'none',
    marginTop: '8px'
  }
})

const lightCoreStyle = computed(() => {
  const coreSize = localOptions.value.size * 0.15
  return {
    width: `${coreSize}px`,
    height: `${coreSize}px`,
    borderRadius: '50%',
    backgroundColor: '#ffffff',
    margin: 'auto',
    opacity: localValue.value ? 1 : 0.3,
    transition: localOptions.value.animation ? 'all 0.3s ease' : 'none'
  }
})

// 方法
const handleClick = () => {
  if (localOptions.value.disabled) return
  
  const newValue = !localValue.value
  localValue.value = newValue
  
  // 触觉反馈
  if (localOptions.value.hapticFeedback && 'vibrate' in navigator) {
    navigator.vibrate(50)
  }
  
  // 音效反馈
  if (localOptions.value.soundEnabled) {
    playClickSound()
  }
  
  // 发送事件
  emit('value-change', newValue)
  emit('switch-click', newValue)
  emit('state-change', {
    value: newValue,
    timestamp: Date.now()
  })
}

const handleMouseDown = () => {
  if (!localOptions.value.disabled) {
    isPressed.value = true
  }
}

const handleMouseUp = () => {
  isPressed.value = false
}

const handleMouseLeave = () => {
  isPressed.value = false
}

const handleControlChange = (value: boolean) => {
  localValue.value = value
  emit('value-change', value)
}

const updateStyle = () => {
  // 样式更新逻辑
}

const updateSize = () => {
  // 大小更新逻辑
}

const toggleDisabled = () => {
  localOptions.value.disabled = !localOptions.value.disabled
}

const playClickSound = () => {
  // 创建音效
  const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)()
  const oscillator = audioContext.createOscillator()
  const gainNode = audioContext.createGain()
  
  oscillator.connect(gainNode)
  gainNode.connect(audioContext.destination)
  
  oscillator.frequency.setValueAtTime(localValue.value ? 800 : 400, audioContext.currentTime)
  gainNode.gain.setValueAtTime(0.1, audioContext.currentTime)
  gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.1)
  
  oscillator.start(audioContext.currentTime)
  oscillator.stop(audioContext.currentTime + 0.1)
}

// 监听器
watch(() => props.data.value, (newValue) => {
  localValue.value = newValue
})

watch(() => props.options, (newOptions) => {
  Object.assign(localOptions.value, newOptions)
}, { deep: true })

// 生命周期
onMounted(() => {
  localValue.value = props.data.value
})
</script>

<style scoped lang="scss">
.professional-switch {
  display: inline-block;
  user-select: none;
  
  .switch-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    
    .switch-body {
      position: relative;
      display: flex;
      align-items: center;
      cursor: pointer;
      overflow: hidden;
      
      &.switch-animated {
        .switch-slider {
          transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        }
      }
      
      &:hover:not(.switch-disabled) {
        transform: scale(1.02);
      }
      
      &:active:not(.switch-disabled) {
        transform: scale(0.98);
      }
      
      .switch-slider {
        position: relative;
        z-index: 2;
        display: flex;
        align-items: center;
        justify-content: center;
        
        .slider-indicator {
          position: relative;
          
          &::after {
            content: '';
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: 2px;
            height: 2px;
            border-radius: 50%;
            background-color: rgba(255, 255, 255, 0.8);
          }
        }
      }
      
      .switch-track {
        .track-decoration {
          opacity: 0.6;
        }
      }
      
      .switch-labels {
        position: absolute;
        top: 50%;
        left: 0;
        right: 0;
        transform: translateY(-50%);
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0 8px;
        z-index: 1;
        pointer-events: none;
        
        .label-on,
        .label-off {
          transition: all 0.3s ease;
          opacity: 0.7;
          
          &.active {
            opacity: 1;
          }
        }
        
        .label-on {
          margin-left: 4px;
        }
        
        .label-off {
          margin-right: 4px;
        }
      }
    }
    
    .switch-title {
      text-align: center;
    }
    
    .status-light {
      display: flex;
      align-items: center;
      justify-content: center;
      
      .light-core {
        position: relative;
        
        &::before {
          content: '';
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translate(-50%, -50%);
          width: 150%;
          height: 150%;
          border-radius: 50%;
          background: radial-gradient(circle, rgba(255,255,255,0.8) 0%, transparent 70%);
          opacity: 0.6;
        }
      }
    }
  }
  
  .control-panel {
    margin-top: 20px;
    padding: 15px;
    background-color: rgba(255, 255, 255, 0.1);
    border-radius: 8px;
    display: flex;
    flex-wrap: wrap;
    gap: 15px;
    align-items: center;
    
    .control-group {
      display: flex;
      align-items: center;
      gap: 8px;
      
      label {
        color: #374151;
        font-size: 12px;
        white-space: nowrap;
        font-weight: 500;
      }
    }
  }
  
  // 样式变体
  &.switch-modern {
    .switch-body {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border: none !important;
      
      &.switch-on {
        background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
      }
    }
  }
  
  &.switch-classic {
    .switch-body {
      border: 3px solid #d1d5db !important;
      background: #f9fafb !important;
      
      &.switch-on {
        border-color: v-bind('localOptions.onColor') !important;
        background: v-bind('localOptions.onColor') !important;
      }
    }
  }
  
  &.switch-industrial {
    .switch-body {
      background: #374151 !important;
      border: 2px solid #6b7280 !important;
      box-shadow: inset 0 2px 4px rgba(0,0,0,0.3) !important;
      
      &.switch-on {
        background: #059669 !important;
        border-color: #10b981 !important;
        box-shadow: 0 0 20px #10b98140, inset 0 2px 4px rgba(0,0,0,0.2) !important;
      }
    }
  }
  
  &.switch-minimal {
    .switch-body {
      background: transparent !important;
      border: 1px solid #d1d5db !important;
      
      &.switch-on {
        background: v-bind('localOptions.onColor') !important;
        border-color: v-bind('localOptions.onColor') !important;
      }
    }
  }
  
  &.switch-disabled {
    .switch-body {
      cursor: not-allowed;
      
      &:hover {
        transform: none !important;
      }
      
      &:active {
        transform: none !important;
      }
    }
  }
  
  &.switch-pressed {
    .switch-body {
      transform: scale(0.95);
    }
  }
}

@media (max-width: 768px) {
  .professional-switch {
    .control-panel {
      flex-direction: column;
      align-items: stretch;
      
      .control-group {
        justify-content: space-between;
      }
    }
  }
}
</style>
