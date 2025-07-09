<template>
  <div class="digital-display" :class="containerClass">
    <div class="display-container">
      <!-- 数码管显示区域 -->
      <div class="digits-container">
        <div
          v-for="(digit, index) in displayDigits"
          :key="index"
          class="digit-wrapper"
        >
          <!-- 七段数码管 -->
          <div class="seven-segment" :style="{ width: segmentSize + 'px', height: (segmentSize * 1.6) + 'px' }">
            <!-- 顶部横段 (a) -->
            <div
              class="segment horizontal top"
              :class="{ active: digit.segments.a }"
            ></div>
            
            <!-- 左上竖段 (f) -->
            <div
              class="segment vertical top-left"
              :class="{ active: digit.segments.f }"
            ></div>
            
            <!-- 右上竖段 (b) -->
            <div
              class="segment vertical top-right"
              :class="{ active: digit.segments.b }"
            ></div>
            
            <!-- 中间横段 (g) -->
            <div
              class="segment horizontal middle"
              :class="{ active: digit.segments.g }"
            ></div>
            
            <!-- 左下竖段 (e) -->
            <div
              class="segment vertical bottom-left"
              :class="{ active: digit.segments.e }"
            ></div>
            
            <!-- 右下竖段 (c) -->
            <div
              class="segment vertical bottom-right"
              :class="{ active: digit.segments.c }"
            ></div>
            
            <!-- 底部横段 (d) -->
            <div
              class="segment horizontal bottom"
              :class="{ active: digit.segments.d }"
            ></div>
            
            <!-- 小数点 -->
            <div
              v-if="digit.hasDecimal"
              class="decimal-point"
              :class="{ active: digit.decimal }"
            ></div>
          </div>
        </div>
      </div>
      
      <!-- 显示标题 -->
      <div v-if="data.title" class="display-title">
        {{ data.title }}
      </div>
      
      <!-- 单位显示 -->
      <div v-if="data.unit" class="display-unit">
        {{ data.unit }}
      </div>
    </div>
    
    <!-- 控制面板 -->
    <div v-if="options.showControls" class="control-panel">
      <div class="control-group">
        <label>显示值:</label>
        <el-input
          v-model="controlValue"
          type="number"
          :step="options.step || 0.1"
          @input="handleValueChange"
          style="width: 120px"
        />
      </div>
      
      <div class="control-group">
        <label>数字位数:</label>
        <el-slider
          v-model="localOptions.digitCount"
          :min="1"
          :max="8"
          :step="1"
          @change="updateDigitCount"
          style="width: 100px"
        />
      </div>
      
      <div class="control-group">
        <label>小数位数:</label>
        <el-slider
          v-model="localOptions.decimalPlaces"
          :min="0"
          :max="4"
          :step="1"
          @change="updateDisplay"
          style="width: 100px"
        />
      </div>
      
      <div class="control-group">
        <el-button @click="toggleBlinking" :type="isBlinking ? 'danger' : 'primary'">
          {{ isBlinking ? '停止闪烁' : '开始闪烁' }}
        </el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'

// 定义数码管段的映射
const DIGIT_SEGMENTS: Record<string, { a: boolean; b: boolean; c: boolean; d: boolean; e: boolean; f: boolean; g: boolean }> = {
  '0': { a: true, b: true, c: true, d: true, e: true, f: true, g: false },
  '1': { a: false, b: true, c: true, d: false, e: false, f: false, g: false },
  '2': { a: true, b: true, c: false, d: true, e: true, f: false, g: true },
  '3': { a: true, b: true, c: true, d: true, e: false, f: false, g: true },
  '4': { a: false, b: true, c: true, d: false, e: false, f: true, g: true },
  '5': { a: true, b: false, c: true, d: true, e: false, f: true, g: true },
  '6': { a: true, b: false, c: true, d: true, e: true, f: true, g: true },
  '7': { a: true, b: true, c: true, d: false, e: false, f: false, g: false },
  '8': { a: true, b: true, c: true, d: true, e: true, f: true, g: true },
  '9': { a: true, b: true, c: true, d: true, e: false, f: true, g: true },
  '-': { a: false, b: false, c: false, d: false, e: false, f: false, g: true },
  ' ': { a: false, b: false, c: false, d: false, e: false, f: false, g: false },
  'E': { a: true, b: false, c: false, d: true, e: true, f: true, g: true },
  'r': { a: false, b: false, c: false, d: false, e: true, f: false, g: true }
}

interface DigitalDisplayData {
  value: number
  title?: string
  unit?: string
  min?: number
  max?: number
}

interface DigitalDisplayOptions {
  digitCount?: number
  decimalPlaces?: number
  size?: number
  color?: string
  backgroundColor?: string
  segmentColor?: string
  activeColor?: string
  showControls?: boolean
  blinkInterval?: number
  step?: number
  padding?: number
  borderRadius?: number
  showTitle?: boolean
  showUnit?: boolean
}

const props = withDefaults(defineProps<{
  data: DigitalDisplayData
  options?: DigitalDisplayOptions
}>(), {
  options: () => ({})
})

const emit = defineEmits<{
  'value-change': [value: number]
  'digit-click': [index: number]
}>()

// 响应式数据
const controlValue = ref(props.data.value)
const localOptions = ref({
  digitCount: 4,
  decimalPlaces: 1,
  size: 60,
  color: '#00ff00',
  backgroundColor: '#000000',
  segmentColor: '#003300',
  activeColor: '#00ff00',
  showControls: false,
  blinkInterval: 500,
  step: 0.1,
  padding: 20,
  borderRadius: 8,
  showTitle: true,
  showUnit: true,
  ...props.options
})

const isBlinking = ref(false)
const blinkState = ref(true)
let blinkTimer: number | null = null

// 计算属性
const containerClass = computed(() => ({
  'blinking': isBlinking.value && !blinkState.value
}))

const segmentSize = computed(() => localOptions.value.size)

// 计算显示的数字
const displayDigits = computed(() => {
  let value = props.data.value
  
  // 处理闪烁状态
  if (isBlinking.value && !blinkState.value) {
    value = 0
  }
  
  // 格式化数值
  const formattedValue = value.toFixed(localOptions.value.decimalPlaces)
  const [integerPart, decimalPart] = formattedValue.split('.')
  
  // 构建显示字符串
  let displayString = integerPart.padStart(
    localOptions.value.digitCount - (decimalPart ? decimalPart.length : 0),
    ' '
  )
  
  if (decimalPart) {
    displayString += decimalPart
  }
  
  // 转换为数码管段数据
  const digits = []
  let hasDecimalNext = false
  
  for (let i = displayString.length - 1; i >= 0; i--) {
    const char = displayString[i]
    const segments = DIGIT_SEGMENTS[char] || DIGIT_SEGMENTS[' ']
    
    digits.unshift({
      segments,
      hasDecimal: Boolean(decimalPart && i === integerPart.length - 1),
      decimal: Boolean(hasDecimalNext)
    })
    
    hasDecimalNext = Boolean(decimalPart && i === integerPart.length)
  }
  
  return digits.slice(0, localOptions.value.digitCount)
})

// 方法
const handleValueChange = (value: string) => {
  const numValue = parseFloat(value) || 0
  emit('value-change', numValue)
}

const updateDigitCount = () => {
  updateDisplay()
}

const updateDisplay = () => {
  // 触发重新计算
}

const toggleBlinking = () => {
  if (isBlinking.value) {
    stopBlinking()
  } else {
    startBlinking()
  }
}

const startBlinking = () => {
  isBlinking.value = true
  blinkTimer = setInterval(() => {
    blinkState.value = !blinkState.value
  }, localOptions.value.blinkInterval)
}

const stopBlinking = () => {
  isBlinking.value = false
  blinkState.value = true
  if (blinkTimer) {
    clearInterval(blinkTimer)
    blinkTimer = null
  }
}

// 监听器
watch(() => props.data.value, (newValue) => {
  controlValue.value = newValue
})

watch(() => props.options, (newOptions) => {
  Object.assign(localOptions.value, newOptions)
}, { deep: true })

// 生命周期
onMounted(() => {
  controlValue.value = props.data.value
})

onUnmounted(() => {
  if (blinkTimer) {
    clearInterval(blinkTimer)
  }
})
</script>

<style scoped lang="scss">
.digital-display {
  font-family: 'Courier New', monospace;
  user-select: none;
  padding: v-bind('localOptions.padding + "px"');
  background-color: v-bind('localOptions.backgroundColor');
  border-radius: v-bind('localOptions.borderRadius + "px"');
  border: 2px solid #333;
  display: inline-block;
  
  &.blinking {
    .segment.active {
      opacity: 0.3;
    }
    
    .decimal-point.active {
      opacity: 0.3;
    }
  }
  
  .display-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 10px;
    
    .digits-container {
      display: flex;
      align-items: center;
      justify-content: center;
      
      .digit-wrapper {
        margin: 0 2px;
        
        .seven-segment {
          position: relative;
          display: inline-block;
          
          .segment {
            position: absolute;
            background-color: v-bind('localOptions.segmentColor');
            transition: all 0.2s ease;
            
            &.horizontal {
              width: v-bind('(localOptions.size * 0.7) + "px"');
              height: v-bind('(localOptions.size * 0.1) + "px"');
              left: v-bind('(localOptions.size * 0.15) + "px"');
              clip-path: polygon(10% 0%, 90% 0%, 100% 50%, 90% 100%, 10% 100%, 0% 50%);
              
              &.top {
                top: 0;
              }
              
              &.middle {
                top: 50%;
                transform: translateY(-50%);
              }
              
              &.bottom {
                bottom: 0;
              }
            }
            
            &.vertical {
              width: v-bind('(localOptions.size * 0.1) + "px"');
              height: v-bind('(localOptions.size * 0.7) + "px"');
              clip-path: polygon(0% 10%, 50% 0%, 100% 10%, 100% 90%, 50% 100%, 0% 90%);
              
              &.top-left {
                left: 0;
                top: 5%;
              }
              
              &.top-right {
                right: 0;
                top: 5%;
              }
              
              &.bottom-left {
                left: 0;
                bottom: 5%;
              }
              
              &.bottom-right {
                right: 0;
                bottom: 5%;
              }
            }
            
            &.active {
              background-color: v-bind('localOptions.activeColor');
              box-shadow: 0 0 10px v-bind('localOptions.activeColor');
            }
          }
          
          .decimal-point {
            position: absolute;
            width: v-bind('(localOptions.size * 0.15) + "px"');
            height: v-bind('(localOptions.size * 0.15) + "px"');
            background-color: v-bind('localOptions.segmentColor');
            border-radius: 50%;
            right: v-bind('(-localOptions.size * 0.2) + "px"');
            bottom: v-bind('(localOptions.size * 0.05) + "px"');
            transition: all 0.2s ease;
            
            &.active {
              background-color: v-bind('localOptions.activeColor');
              box-shadow: 0 0 5px v-bind('localOptions.activeColor');
            }
          }
        }
      }
    }
    
    .display-title {
      color: v-bind('localOptions.color');
      font-size: v-bind('(localOptions.size * 0.25) + "px"');
      font-weight: bold;
      text-align: center;
      margin-top: 10px;
    }
    
    .display-unit {
      color: v-bind('localOptions.color');
      font-size: v-bind('(localOptions.size * 0.2) + "px"');
      text-align: center;
      margin-top: 5px;
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
        color: #fff;
        font-size: 12px;
        white-space: nowrap;
      }
    }
  }
}
</style>
