<template>
  <div class="gauge-container">
    <svg :width="size" :height="size" viewBox="0 0 200 200">
      <!-- 背景圆弧 -->
      <path
        :d="backgroundArc"
        fill="none"
        stroke="#e0e0e0"
        :stroke-width="strokeWidth"
      />
      <!-- 值圆弧 -->
      <path
        :d="valueArc"
        fill="none"
        :stroke="color"
        :stroke-width="strokeWidth"
        stroke-linecap="round"
      />
      <!-- 中心文本 -->
      <text x="100" y="110" text-anchor="middle" class="gauge-value">
        {{ value }}
      </text>
      <text x="100" y="130" text-anchor="middle" class="gauge-unit">
        {{ unit }}
      </text>
    </svg>
    <div class="gauge-title">{{ title }}</div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  value: number
  min?: number
  max?: number
  unit?: string
  title?: string
  size?: number
  color?: string
}

const props = withDefaults(defineProps<Props>(), {
  min: 0,
  max: 100,
  unit: '',
  title: '仪表盘',
  size: 200,
  color: '#409eff'
})

const strokeWidth = 20
const radius = 80
const startAngle = -135
const endAngle = 135

const valueAngle = computed(() => {
  const percentage = (props.value - props.min) / (props.max - props.min)
  return startAngle + (endAngle - startAngle) * percentage
})

const polarToCartesian = (angle: number) => {
  const angleInRadians = (angle - 90) * Math.PI / 180
  return {
    x: 100 + radius * Math.cos(angleInRadians),
    y: 100 + radius * Math.sin(angleInRadians)
  }
}

const describeArc = (startAngle: number, endAngle: number) => {
  const start = polarToCartesian(endAngle)
  const end = polarToCartesian(startAngle)
  const largeArcFlag = endAngle - startAngle <= 180 ? "0" : "1"
  return [
    "M", start.x, start.y,
    "A", radius, radius, 0, largeArcFlag, 0, end.x, end.y
  ].join(" ")
}

const backgroundArc = computed(() => describeArc(startAngle, endAngle))
const valueArc = computed(() => describeArc(startAngle, valueAngle.value))
</script>

<style scoped>
.gauge-container {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
}

.gauge-value {
  font-size: 32px;
  font-weight: bold;
  fill: #333;
}

.gauge-unit {
  font-size: 14px;
  fill: #666;
}

.gauge-title {
  margin-top: 10px;
  font-size: 16px;
  color: #333;
}
</style>
