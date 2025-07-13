<template>
  <div class="simple-linear-gauge" :style="containerStyle">
    <svg :width="width" :height="height" class="gauge-svg">
      <!-- 背景条 -->
      <rect
        :x="padding"
        :y="(height - barHeight) / 2"
        :width="barWidth"
        :height="barHeight"
        :fill="backgroundColor"
        :rx="barHeight / 2"
      />
      
      <!-- 进度条 -->
      <rect
        :x="padding"
        :y="(height - barHeight) / 2"
        :width="progressWidth"
        :height="barHeight"
        :fill="progressColor"
        :rx="barHeight / 2"
        class="progress-bar"
      />
      
      <!-- 刻度线 -->
      <g v-if="showScale">
        <line
          v-for="(tick, index) in ticks"
          :key="index"
          :x1="tick.x"
          :y1="(height - barHeight) / 2 - 5"
          :x2="tick.x"
          :y2="(height - barHeight) / 2 + barHeight + 5"
          :stroke="scaleColor"
          stroke-width="1"
        />
        <text
          v-for="(tick, index) in ticks"
          :key="`label-${index}`"
          :x="tick.x"
          :y="(height - barHeight) / 2 + barHeight + 20"
          text-anchor="middle"
          :font-size="12"
          :fill="textColor"
        >
          {{ tick.value }}
        </text>
      </g>
      
      <!-- 当前值指示器 -->
      <circle
        :cx="indicatorX"
        :cy="height / 2"
        :r="6"
        :fill="indicatorColor"
        stroke="#fff"
        stroke-width="2"
        class="indicator"
      />
      
      <!-- 数值文本 -->
      <text
        :x="width / 2"
        :y="20"
        text-anchor="middle"
        :font-size="14"
        :fill="textColor"
        font-weight="bold"
      >
        {{ displayValue }}{{ unit }}
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
  width?: number
  height?: number
  backgroundColor?: string
  progressColor?: string
  indicatorColor?: string
  scaleColor?: string
  textColor?: string
  showScale?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  value: 50,
  min: 0,
  max: 100,
  unit: '',
  width: 300,
  height: 80,
  backgroundColor: '#e4e7ed',
  progressColor: '#409eff',
  indicatorColor: '#f56c6c',
  scaleColor: '#909399',
  textColor: '#303133',
  showScale: true
})

const emit = defineEmits<{
  'update:value': [value: number]
}>()

// 计算属性
const containerStyle = computed(() => ({
  width: `${props.width}px`,
  height: `${props.height}px`
}))

const padding = 20
const barHeight = 20
const barWidth = computed(() => props.width - padding * 2)

const progress = computed(() => {
  return Math.max(0, Math.min(1, (props.value - props.min) / (props.max - props.min)))
})

const progressWidth = computed(() => {
  return barWidth.value * progress.value
})

const indicatorX = computed(() => {
  return padding + progressWidth.value
})

const displayValue = computed(() => {
  return props.value.toFixed(1)
})

// 刻度计算
const ticks = computed(() => {
  const tickCount = 5
  const result = []
  
  for (let i = 0; i <= tickCount; i++) {
    const value = props.min + (props.max - props.min) * (i / tickCount)
    const x = padding + barWidth.value * (i / tickCount)
    result.push({ value: value.toFixed(0), x })
  }
  
  return result
})
</script>

<style scoped>
.simple-linear-gauge {
  display: inline-block;
  user-select: none;
}

.gauge-svg {
  display: block;
}

.progress-bar {
  transition: width 0.3s ease;
}

.indicator {
  transition: cx 0.3s ease;
}
</style>
