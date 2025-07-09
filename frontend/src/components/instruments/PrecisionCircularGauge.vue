<template>
  <div class="precision-circular-gauge" :style="containerStyle">
    <svg :width="size" :height="size" class="gauge-svg">
      <!-- 外圆环背景 -->
      <circle
        :cx="center"
        :cy="center"
        :r="outerRadius"
        fill="none"
        :stroke="colors.background"
        :stroke-width="2"
        class="gauge-outer-ring"
      />
      
      <!-- 内圆环轨道 -->
      <circle
        :cx="center"
        :cy="center"
        :r="gaugeRadius"
        fill="none"
        :stroke="colors.track"
        :stroke-width="trackWidth"
        class="gauge-track"
      />
      
      <!-- 进度弧 -->
      <path
        :d="progressPath"
        fill="none"
        :stroke="getProgressColor()"
        :stroke-width="trackWidth"
        stroke-linecap="round"
        class="gauge-progress"
        :class="{ 'animated': animated }"
      />
      
      <!-- 主刻度线 -->
      <g class="major-ticks">
        <g
          v-for="tick in majorTicks"
          :key="`major-${tick.index}`"
          class="major-tick-group"
        >
          <line
            :x1="tick.x1"
            :y1="tick.y1"
            :x2="tick.x2"
            :y2="tick.y2"
            :stroke="colors.majorTick"
            :stroke-width="2"
            class="major-tick"
          />
          <!-- 主刻度数值 -->
          <text
            :x="tick.labelX"
            :y="tick.labelY"
            :fill="colors.label"
            :font-size="labelFontSize"
            text-anchor="middle"
            dominant-baseline="middle"
            class="tick-label"
            font-family="Segoe UI, sans-serif"
          >
            {{ formatTickValue(tick.value) }}
          </text>
        </g>
      </g>
      
      <!-- 次刻度线 -->
      <g class="minor-ticks">
        <line
          v-for="tick in minorTicks"
          :key="`minor-${tick.index}`"
          :x1="tick.x1"
          :y1="tick.y1"
          :x2="tick.x2"
          :y2="tick.y2"
          :stroke="colors.minorTick"
          :stroke-width="1"
          class="minor-tick"
        />
      </g>
      
      <!-- 微刻度线 -->
      <g class="micro-ticks">
        <line
          v-for="tick in microTicks"
          :key="`micro-${tick.index}`"
          :x1="tick.x1"
          :y1="tick.y1"
          :x2="tick.x2"
          :y2="tick.y2"
          :stroke="colors.minorTick"
          :stroke-width="0.5"
          class="micro-tick"
          opacity="0.6"
        />
      </g>
      
      <!-- 阈值标记 -->
      <g v-if="showThresholds && thresholds.length > 0" class="threshold-markers">
        <g
          v-for="threshold in thresholds"
          :key="`threshold-${threshold.value}`"
          class="threshold-group"
        >
          <line
            :x1="threshold.x1"
            :y1="threshold.y1"
            :x2="threshold.x2"
            :y2="threshold.y2"
            :stroke="threshold.color"
            :stroke-width="4"
            class="threshold-line"
          />
          <circle
            :cx="threshold.x2"
            :cy="threshold.y2"
            :r="5"
            :fill="threshold.color"
            class="threshold-marker"
          />
          <!-- 阈值标签 -->
          <text
            v-if="threshold.label"
            :x="threshold.labelX"
            :y="threshold.labelY"
            :fill="threshold.color"
            :font-size="10"
            text-anchor="middle"
            class="threshold-label"
            font-family="Segoe UI, sans-serif"
            font-weight="600"
          >
            {{ threshold.label }}
          </text>
        </g>
      </g>
      
      <!-- 指针 -->
      <g v-if="showPointer" class="pointer-group">
        <!-- 指针阴影 -->
        <polygon
          :points="pointerShadowPoints"
          :fill="colors.pointerShadow"
          class="pointer-shadow"
          :transform="`rotate(${pointerAngle} ${center} ${center})`"
        />
        <!-- 指针主体 -->
        <polygon
          :points="pointerPoints"
          :fill="colors.pointer"
          class="pointer"
          :transform="`rotate(${pointerAngle} ${center} ${center})`"
        />
        <!-- 中心圆 -->
        <circle
          :cx="center"
          :cy="center"
          :r="10"
          :fill="colors.pointerCenter"
          :stroke="colors.pointer"
          :stroke-width="2"
          class="pointer-center"
        />
        <!-- 中心小圆 -->
        <circle
          :cx="center"
          :cy="center"
          :r="4"
          :fill="colors.pointer"
          class="pointer-center-dot"
        />
      </g>
      
    </svg>
    
    <!-- 外部信息显示区域 -->
    <div class="gauge-info-panel">
      <!-- 状态指示器 -->
      <div v-if="showStatus" class="status-indicators">
        <div 
          class="status-indicator"
          :class="getStatusClass()"
          :title="getStatusText()"
        >
          <div class="status-dot"></div>
          <span class="status-text">{{ getStatusText() }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { InstrumentGaugeProps, ThresholdConfig } from '@/types/instrument'

interface Props extends InstrumentGaugeProps {
  showStatus?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  min: 0,
  max: 100,
  size: 280,
  precision: 2,
  showPointer: true,
  showDigitalDisplay: true,
  showThresholds: true,
  showStatus: true,
  animated: true,
  thresholds: () => [],
  colors: () => ({})
})

// 默认颜色方案 - 测试测量仪器风格
const defaultColors = {
  background: '#E5E7EB',
  track: '#F3F4F6',
  progress: '#2E86AB',
  pointer: '#1F2937',
  pointerCenter: '#FFFFFF',
  pointerShadow: 'rgba(0, 0, 0, 0.2)',
  majorTick: '#374151',
  minorTick: '#9CA3AF',
  label: '#374151',
  title: '#1F2937',
  unit: '#6B7280',
  digitalValue: '#2E86AB',
  digitalUnit: '#6B7280',
  digitalBackground: '#F9FAFB'
}

const colors = computed(() => ({ ...defaultColors, ...props.colors }))

// 基础计算
const center = computed(() => props.size / 2)
const outerRadius = computed(() => props.size / 2 - 15)
const gaugeRadius = computed(() => props.size / 2 - 50)
const trackWidth = computed(() => 14)

// 角度计算 (270度弧，从-135度到135度)
const startAngle = computed(() => -135)
const endAngle = computed(() => 135)
const totalAngle = computed(() => endAngle.value - startAngle.value)

const normalizedValue = computed(() => {
  const clampedValue = Math.max(props.min, Math.min(props.max, props.value))
  return (clampedValue - props.min) / (props.max - props.min)
})

const pointerAngle = computed(() => {
  return startAngle.value + normalizedValue.value * totalAngle.value
})

// 路径计算
const progressPath = computed(() => {
  const progressAngle = startAngle.value + normalizedValue.value * totalAngle.value
  const startRad = (startAngle.value * Math.PI) / 180
  const endRad = (progressAngle * Math.PI) / 180
  
  const x1 = center.value + gaugeRadius.value * Math.cos(startRad)
  const y1 = center.value + gaugeRadius.value * Math.sin(startRad)
  const x2 = center.value + gaugeRadius.value * Math.cos(endRad)
  const y2 = center.value + gaugeRadius.value * Math.sin(endRad)
  
  const largeArcFlag = Math.abs(progressAngle - startAngle.value) > 180 ? 1 : 0
  
  return `M ${x1} ${y1} A ${gaugeRadius.value} ${gaugeRadius.value} 0 ${largeArcFlag} 1 ${x2} ${y2}`
})

// 主刻度计算
const majorTicks = computed(() => {
  const ticks = []
  const tickCount = 11 // 主刻度数量
  
  for (let i = 0; i < tickCount; i++) {
    const progress = i / (tickCount - 1)
    const angle = startAngle.value + progress * totalAngle.value
    const rad = (angle * Math.PI) / 180
    const value = props.min + progress * (props.max - props.min)
    
    const innerRadius = gaugeRadius.value - trackWidth.value / 2 - 20
    const outerRadius = gaugeRadius.value - trackWidth.value / 2 - 5
    const labelRadius = gaugeRadius.value - trackWidth.value / 2 - 35
    
    const x1 = center.value + innerRadius * Math.cos(rad)
    const y1 = center.value + innerRadius * Math.sin(rad)
    const x2 = center.value + outerRadius * Math.cos(rad)
    const y2 = center.value + outerRadius * Math.sin(rad)
    const labelX = center.value + labelRadius * Math.cos(rad)
    const labelY = center.value + labelRadius * Math.sin(rad)
    
    ticks.push({ x1, y1, x2, y2, labelX, labelY, value, index: i })
  }
  
  return ticks
})

// 次刻度计算
const minorTicks = computed(() => {
  const ticks = []
  const majorTickCount = 11
  const minorPerMajor = 4 // 每个主刻度间的次刻度数
  
  for (let i = 0; i < majorTickCount - 1; i++) {
    for (let j = 1; j <= minorPerMajor; j++) {
      const progress = (i + j / (minorPerMajor + 1)) / (majorTickCount - 1)
      const angle = startAngle.value + progress * totalAngle.value
      const rad = (angle * Math.PI) / 180
      
      const innerRadius = gaugeRadius.value - trackWidth.value / 2 - 15
      const outerRadius = gaugeRadius.value - trackWidth.value / 2 - 5
      
      const x1 = center.value + innerRadius * Math.cos(rad)
      const y1 = center.value + innerRadius * Math.sin(rad)
      const x2 = center.value + outerRadius * Math.cos(rad)
      const y2 = center.value + outerRadius * Math.sin(rad)
      
      ticks.push({ x1, y1, x2, y2, index: `${i}-${j}` })
    }
  }
  
  return ticks
})

// 微刻度计算
const microTicks = computed(() => {
  const ticks = []
  const majorTickCount = 11
  const minorPerMajor = 4
  const microPerMinor = 4
  
  for (let i = 0; i < majorTickCount - 1; i++) {
    for (let j = 1; j <= minorPerMajor; j++) {
      for (let k = 1; k <= microPerMinor; k++) {
        const progress = (i + (j + k / (microPerMinor + 1)) / (minorPerMajor + 1)) / (majorTickCount - 1)
        const angle = startAngle.value + progress * totalAngle.value
        const rad = (angle * Math.PI) / 180
        
        const innerRadius = gaugeRadius.value - trackWidth.value / 2 - 10
        const outerRadius = gaugeRadius.value - trackWidth.value / 2 - 5
        
        const x1 = center.value + innerRadius * Math.cos(rad)
        const y1 = center.value + innerRadius * Math.sin(rad)
        const x2 = center.value + outerRadius * Math.cos(rad)
        const y2 = center.value + outerRadius * Math.sin(rad)
        
        ticks.push({ x1, y1, x2, y2, index: `${i}-${j}-${k}` })
      }
    }
  }
  
  return ticks
})

// 阈值标记
const thresholds = computed(() => {
  return props.thresholds.map(threshold => {
    const progress = (threshold.value - props.min) / (props.max - props.min)
    const angle = startAngle.value + progress * totalAngle.value
    const rad = (angle * Math.PI) / 180
    
    const innerRadius = gaugeRadius.value + trackWidth.value / 2 + 5
    const outerRadius = gaugeRadius.value + trackWidth.value / 2 + 20
    // 限制标签半径，确保不超出容器边界
    const maxLabelRadius = Math.min(
      gaugeRadius.value + trackWidth.value / 2 + 25,
      props.size / 2 - 30
    )
    const labelRadius = maxLabelRadius
    
    const x1 = center.value + innerRadius * Math.cos(rad)
    const y1 = center.value + innerRadius * Math.sin(rad)
    const x2 = center.value + outerRadius * Math.cos(rad)
    const y2 = center.value + outerRadius * Math.sin(rad)
    
    // 计算标签位置，确保在容器内
    let labelX = center.value + labelRadius * Math.cos(rad)
    let labelY = center.value + labelRadius * Math.sin(rad)
    
    // 边界检查和调整
    const margin = 15 // 边界留白
    const containerBounds = {
      left: margin,
      right: props.size - margin,
      top: margin,
      bottom: props.size - margin
    }
    
    // 水平边界检查
    if (labelX < containerBounds.left) {
      labelX = containerBounds.left
    } else if (labelX > containerBounds.right) {
      labelX = containerBounds.right
    }
    
    // 垂直边界检查
    if (labelY < containerBounds.top) {
      labelY = containerBounds.top
    } else if (labelY > containerBounds.bottom) {
      labelY = containerBounds.bottom
    }
    
    return {
      ...threshold,
      x1, y1, x2, y2, labelX, labelY
    }
  })
})

// 指针形状
const pointerPoints = computed(() => {
  const length = gaugeRadius.value - trackWidth.value / 2 - 25
  const width = 4
  
  return `${center.value},${center.value - length} ${center.value - width},${center.value + 15} ${center.value + width},${center.value + 15}`
})

const pointerShadowPoints = computed(() => {
  const length = gaugeRadius.value - trackWidth.value / 2 - 25
  const width = 4
  const offset = 2
  
  return `${center.value + offset},${center.value - length + offset} ${center.value - width + offset},${center.value + 15 + offset} ${center.value + width + offset},${center.value + 15 + offset}`
})

// 样式计算
const containerStyle = computed(() => ({
  width: `${props.size}px`,
  height: `${props.size}px`,
  position: 'relative' as const
}))

const digitalDisplayStyle = computed(() => ({
  position: 'absolute' as const,
  bottom: '16px',
  left: '50%',
  transform: 'translateX(-50%)',
  background: colors.value.digitalBackground,
  border: '2px solid #E5E7EB',
  borderRadius: '8px',
  padding: '8px 16px',
  minWidth: '120px',
  textAlign: 'center' as const,
  boxShadow: '0 2px 4px rgba(0, 0, 0, 0.1)',
  zIndex: 10
}))

// 字体大小
const titleFontSize = computed(() => Math.max(14, props.size * 0.05))
const labelFontSize = computed(() => Math.max(10, props.size * 0.035))
const unitFontSize = computed(() => Math.max(12, props.size * 0.04))

// 位置计算
const titleY = computed(() => center.value - gaugeRadius.value + 35)
const unitOffset = computed(() => gaugeRadius.value - 70)

// 方法
const formatTickValue = (value: number) => {
  if (Math.abs(value) >= 1000000) {
    return `${(value / 1000000).toFixed(1)}M`
  } else if (Math.abs(value) >= 1000) {
    return `${(value / 1000).toFixed(1)}k`
  }
  return value.toFixed(0)
}

const formatDigitalValue = () => {
  const value = props.value
  if (props.precision === 0) {
    return value.toFixed(0)
  }
  
  // 科学计数法
  if (Math.abs(value) >= 1000000 || (Math.abs(value) < 0.001 && value !== 0)) {
    return value.toExponential(props.precision)
  }
  
  return value.toFixed(props.precision)
}

const getProgressColor = () => {
  // 根据阈值改变颜色
  for (const threshold of props.thresholds) {
    if (props.value >= threshold.value) {
      return threshold.color
    }
  }
  return colors.value.progress
}

const getStatusClass = () => {
  if (props.thresholds.length === 0) return 'normal'
  
  for (const threshold of props.thresholds) {
    if (props.value >= threshold.value) {
      return threshold.type || 'warning'
    }
  }
  return 'normal'
}

const getStatusText = () => {
  if (props.thresholds.length === 0) return '正常'
  
  for (const threshold of props.thresholds) {
    if (props.value >= threshold.value) {
      return threshold.label || '报警'
    }
  }
  return '正常'
}
</script>

<style lang="scss" scoped>
.precision-circular-gauge {
  display: flex;
  flex-direction: column;
  align-items: center;
  background: var(--instrument-bg);
  border: 1px solid var(--instrument-border);
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 4px 12px var(--instrument-shadow);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  min-height: 360px;
}

.gauge-svg {
  display: block;
  flex-shrink: 0;
}

.gauge-info-panel {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  margin-top: 16px;
  width: 100%;
}

.gauge-progress.animated {
  transition: d 0.5s ease-in-out;
}

.major-tick {
  opacity: 0.9;
}

.minor-tick {
  opacity: 0.7;
}

.micro-tick {
  opacity: 0.5;
}

.tick-label {
  font-family: 'Segoe UI', sans-serif;
  font-weight: 500;
}

.gauge-title {
  font-family: 'Segoe UI', sans-serif;
  font-weight: 600;
}

.unit-label {
  font-family: 'Segoe UI', sans-serif;
  font-weight: 500;
}

.threshold-label {
  font-family: 'Segoe UI', sans-serif;
  font-weight: 600;
  font-size: 10px;
}

.pointer {
  filter: drop-shadow(1px 1px 2px rgba(0, 0, 0, 0.3));
}

.pointer-shadow {
  opacity: 0.3;
}

.digital-display {
  font-family: 'Consolas', 'Monaco', monospace;
  background: var(--digital-bg, #F9FAFB);
  border: 2px solid #E5E7EB;
  border-radius: 8px;
  padding: 12px 20px;
  text-align: center;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  min-width: 140px;
  
  .display-value {
    font-size: 24px;
    font-weight: bold;
    line-height: 1;
    margin-bottom: 4px;
  }
  
  .display-unit {
    font-size: 14px;
    font-weight: 500;
  }
}

.status-indicators {
  display: flex;
  justify-content: center;
  width: 100%;
  
  .status-indicator {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 6px 12px;
    border-radius: 12px;
    font-size: 12px;
    font-weight: 500;
    
    &.normal {
      background: rgba(16, 185, 129, 0.1);
      color: var(--status-normal, #10B981);
    }
    
    &.warning {
      background: rgba(245, 158, 11, 0.1);
      color: var(--status-warning, #F59E0B);
    }
    
    &.error {
      background: rgba(239, 68, 68, 0.1);
      color: var(--status-error, #EF4444);
    }
    
    &.info {
      background: rgba(59, 130, 246, 0.1);
      color: var(--status-info, #3B82F6);
    }
    
    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: currentColor;
    }
    
    .status-text {
      font-size: 11px;
    }
  }
}

// 响应式设计
@media (max-width: 768px) {
  .precision-circular-gauge {
    padding: 12px;
  }
  
  .digital-display {
    .display-value {
      font-size: 16px;
    }
    
    .display-unit {
      font-size: 10px;
    }
  }
}
</style>
