<template>
  <div class="simple-circular-gauge" :style="containerStyle">
    <svg :width="size" :height="size" class="gauge-svg">
      <!-- 背景圆弧 -->
      <path
        :d="backgroundArcPath"
        :stroke="backgroundColor"
        :stroke-width="strokeWidth"
        fill="none"
        stroke-linecap="round"
      />
      
      <!-- 进度圆弧 -->
      <path
        :d="progressArcPath"
        :stroke="progressColor"
        :stroke-width="strokeWidth"
        fill="none"
        stroke-linecap="round"
        class="progress-arc"
      />
      
      <!-- 中心圆 -->
      <circle
        :cx="centerX"
        :cy="centerY"
        :r="centerRadius"
        :fill="centerColor"
        stroke="#ddd"
        stroke-width="2"
      />
      
      <!-- 指针 -->
      <line
        :x1="centerX"
        :y1="centerY"
        :x2="pointerEndX"
        :y2="pointerEndY"
        :stroke="pointerColor"
        :stroke-width="pointerWidth"
        stroke-linecap="round"
        class="pointer"
      />
      
      <!-- 数值文本 -->
      <text
        :x="centerX"
        :y="centerY + 5"
        text-anchor="middle"
        :font-size="fontSize"
        :fill="textColor"
        class="value-text"
      >
        {{ displayValue }}
      </text>
      
      <!-- 单位文本 -->
      <text
        v-if="unit"
        :x="centerX"
        :y="centerY + fontSize + 10"
        text-anchor="middle"
        :font-size="fontSize * 0.7"
        :fill="textColor"
        class="unit-text"
      >
        {{ unit }}
      </text>
    </svg>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  value?: number
  min?: number
  max?: number
  unit?: string
  size?: number
  strokeWidth?: number
  backgroundColor?: string
  progressColor?: string
  pointerColor?: string
  centerColor?: string
  textColor?: string
  startAngle?: number
  endAngle?: number
}

const props = withDefaults(defineProps<Props>(), {
  value: 50,
  min: 0,
  max: 100,
  unit: '',
  size: 200,
  strokeWidth: 20,
  backgroundColor: '#e4e7ed',
  progressColor: '#409eff',
  pointerColor: '#303133',
  centerColor: '#f5f7fa',
  textColor: '#303133',
  startAngle: 135,
  endAngle: 45
})

const emit = defineEmits<{
  'update:value': [value: number]
}>()

// 计算属性
const containerStyle = computed(() => ({
  width: `${props.size}px`,
  height: `${props.size}px`
}))

const centerX = computed(() => props.size / 2)
const centerY = computed(() => props.size / 2)
const radius = computed(() => (props.size - props.strokeWidth) / 2 - 10)
const centerRadius = computed(() => props.strokeWidth / 2)
const fontSize = computed(() => Math.max(12, props.size / 10))

// 角度转弧度
const toRadians = (degrees: number) => (degrees * Math.PI) / 180

// 计算圆弧路径
const createArcPath = (startAngle: number, endAngle: number, radius: number) => {
  const start = toRadians(startAngle)
  const end = toRadians(endAngle)
  
  const x1 = centerX.value + radius * Math.cos(start)
  const y1 = centerY.value + radius * Math.sin(start)
  const x2 = centerX.value + radius * Math.cos(end)
  const y2 = centerY.value + radius * Math.sin(end)
  
  const largeArcFlag = Math.abs(endAngle - startAngle) > 180 ? 1 : 0
  
  return `M ${x1} ${y1} A ${radius} ${radius} 0 ${largeArcFlag} 1 ${x2} ${y2}`
}

// 背景圆弧路径
const backgroundArcPath = computed(() => {
  return createArcPath(props.startAngle, props.endAngle, radius.value)
})

// 进度圆弧路径
const progressArcPath = computed(() => {
  const totalAngle = props.endAngle - props.startAngle
  const progress = (props.value - props.min) / (props.max - props.min)
  const progressAngle = props.startAngle + totalAngle * progress
  
  return createArcPath(props.startAngle, progressAngle, radius.value)
})

// 指针位置
const pointerAngle = computed(() => {
  const totalAngle = props.endAngle - props.startAngle
  const progress = (props.value - props.min) / (props.max - props.min)
  return props.startAngle + totalAngle * progress
})

const pointerEndX = computed(() => {
  const angle = toRadians(pointerAngle.value)
  return centerX.value + (radius.value - 10) * Math.cos(angle)
})

const pointerEndY = computed(() => {
  const angle = toRadians(pointerAngle.value)
  return centerY.value + (radius.value - 10) * Math.sin(angle)
})

const pointerWidth = computed(() => Math.max(2, props.strokeWidth / 4))

// 显示值
const displayValue = computed(() => {
  return props.value.toFixed(1)
})
</script>

<style scoped>
.simple-circular-gauge {
  display: inline-block;
  user-select: none;
}

.gauge-svg {
  display: block;
}

.progress-arc {
  transition: stroke-dasharray 0.3s ease;
}

.pointer {
  transition: transform 0.3s ease;
  transform-origin: 50% 50%;
}

.value-text {
  font-weight: bold;
  font-family: Arial, sans-serif;
}

.unit-text {
  font-family: Arial, sans-serif;
  opacity: 0.7;
}
</style>
