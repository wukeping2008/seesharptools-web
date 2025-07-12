// AI控件生成服务
import type { AIControlRequest, AIControlResponse, ControlTemplate } from '@/types/ai'
import { backendApi } from './BackendApiService'

// 预定义的控件模板（作为备用）
const templates: ControlTemplate[] = [
  {
    id: 'gauge',
    name: '仪表盘',
    description: '圆形仪表盘，显示数值和刻度',
    code: `<template>
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
</style>`
  },
  {
    id: 'led',
    name: 'LED指示灯',
    description: 'LED状态指示灯，支持多种颜色',
    code: `<template>
  <div class="led-container">
    <div 
      class="led"
      :class="{ 'led-on': isOn }"
      :style="{ backgroundColor: isOn ? color : '#ccc' }"
    >
      <div class="led-light" v-if="isOn"></div>
    </div>
    <div class="led-label">{{ label }}</div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  isOn: boolean
  color?: string
  label?: string
}

withDefaults(defineProps<Props>(), {
  color: '#00ff00',
  label: 'LED'
})
</script>

<style scoped>
.led-container {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.led {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  border: 2px solid #999;
  position: relative;
  transition: all 0.3s ease;
}

.led-on {
  box-shadow: 0 0 10px currentColor;
}

.led-light {
  position: absolute;
  top: 4px;
  left: 4px;
  width: 8px;
  height: 8px;
  background: rgba(255, 255, 255, 0.8);
  border-radius: 50%;
}

.led-label {
  font-size: 12px;
  color: #666;
}
</style>`
  },
  {
    id: 'button',
    name: '按钮控件',
    description: '工业风格按钮',
    code: `<template>
  <button 
    class="industrial-button"
    :class="{ 'button-pressed': isPressed }"
    @mousedown="handleMouseDown"
    @mouseup="handleMouseUp"
    @mouseleave="handleMouseUp"
  >
    <span class="button-text">{{ text }}</span>
  </button>
</template>

<script setup lang="ts">
import { ref } from 'vue'

interface Props {
  text?: string
}

const props = withDefaults(defineProps<Props>(), {
  text: '按钮'
})

const emit = defineEmits<{
  click: []
}>()

const isPressed = ref(false)

const handleMouseDown = () => {
  isPressed.value = true
}

const handleMouseUp = () => {
  if (isPressed.value) {
    emit('click')
  }
  isPressed.value = false
}
</script>

<style scoped>
.industrial-button {
  padding: 12px 24px;
  background: linear-gradient(135deg, #f0f0f0 0%, #d0d0d0 100%);
  border: 2px solid #999;
  border-radius: 4px;
  cursor: pointer;
  user-select: none;
  transition: all 0.1s ease;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.industrial-button:hover {
  background: linear-gradient(135deg, #f5f5f5 0%, #d5d5d5 100%);
}

.button-pressed {
  background: linear-gradient(135deg, #d0d0d0 0%, #b0b0b0 100%);
  box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.3);
  transform: translateY(1px);
}

.button-text {
  font-size: 14px;
  font-weight: 600;
  color: #333;
}
</style>`
  }
]

export class AIControlService {
  // 生成控件
  async generateControl(request: AIControlRequest): Promise<AIControlResponse> {
    try {
      // 首先尝试调用真实的Claude API
      const data = await backendApi.post<AIControlResponse>('/api/ai/generate-control', {
        description: request.description
      })
      
      if (data.success && data.code) {
        return data
      }
      
      // 如果API调用失败或没有返回代码，使用备用模板
      console.warn('Claude API调用失败，使用备用模板')
      return this.generateFromTemplate(request)
      
    } catch (error) {
      console.error('AI生成控件错误:', error)
      
      // 如果出错，使用备用模板
      return this.generateFromTemplate(request)
    }
  }
  
  // 使用模板生成（备用方案）
  private generateFromTemplate(request: AIControlRequest): AIControlResponse {
    try {
      // 简单的关键词匹配来选择模板
      const keywords = request.description.toLowerCase()
      
      let selectedTemplate: ControlTemplate | undefined
      
      if (keywords.includes('仪表') || keywords.includes('gauge') || keywords.includes('表盘')) {
        selectedTemplate = templates.find(t => t.id === 'gauge')
      } else if (keywords.includes('led') || keywords.includes('指示灯') || keywords.includes('灯')) {
        selectedTemplate = templates.find(t => t.id === 'led')
      } else if (keywords.includes('按钮') || keywords.includes('button')) {
        selectedTemplate = templates.find(t => t.id === 'button')
      }
      
      if (selectedTemplate) {
        return {
          success: true,
          code: selectedTemplate.code
        }
      }
      
      // 如果没有匹配的模板，返回默认的仪表盘
      return {
        success: true,
        code: templates[0].code
      }
      
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : '生成失败'
      }
    }
  }
  
  // 获取所有模板
  getTemplates(): ControlTemplate[] {
    return templates
  }
}

// 导出服务实例
export const aiControlService = new AIControlService()
