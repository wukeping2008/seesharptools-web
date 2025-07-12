// AI控件生成服务
import type { 
  AIControlRequest, 
  AIControlResponse, 
  ControlTemplate, 
  ValidationResult, 
  CodeQuality 
} from '@/types/ai'
import { backendApi } from './BackendApiService'

// 预定义的控件模板（作为备用）
const templates: ControlTemplate[] = [
  {
    id: 'gauge',
    name: '仪表盘',
    description: '圆形仪表盘，显示数值和刻度',
    keywords: ['仪表', 'gauge', '表盘', '圆形', '刻度', '数值显示'],
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
    keywords: ['led', '指示灯', '灯', '状态', '指示', '信号灯'],
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
    keywords: ['按钮', 'button', '控制', '操作', '点击'],
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
  },
  {
    id: 'chart',
    name: '图表控件',
    description: '实时数据图表',
    keywords: ['图表', 'chart', '曲线', '数据', '实时', '波形'],
    code: `<template>
  <div class="chart-container" ref="chartRef">
    <canvas 
      ref="canvasRef"
      :width="width"
      :height="height"
      @mousemove="handleMouseMove"
    ></canvas>
    <div class="chart-legend" v-if="showLegend">
      <div 
        v-for="(series, index) in data" 
        :key="index"
        class="legend-item"
      >
        <span 
          class="legend-color" 
          :style="{ backgroundColor: series.color }"
        ></span>
        <span class="legend-label">{{ series.name }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'

interface ChartData {
  name: string
  data: number[]
  color: string
}

interface Props {
  data: ChartData[]
  width?: number
  height?: number
  showLegend?: boolean
  gridLines?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  width: 400,
  height: 200,
  showLegend: true,
  gridLines: true
})

const chartRef = ref<HTMLDivElement>()
const canvasRef = ref<HTMLCanvasElement>()
let ctx: CanvasRenderingContext2D | null = null

const drawChart = () => {
  if (!ctx || !props.data.length) return
  
  ctx.clearRect(0, 0, props.width, props.height)
  
  // 绘制网格线
  if (props.gridLines) {
    ctx.strokeStyle = '#e0e0e0'
    ctx.lineWidth = 1
    
    // 垂直网格线
    for (let i = 0; i <= 10; i++) {
      const x = (props.width / 10) * i
      ctx.beginPath()
      ctx.moveTo(x, 0)
      ctx.lineTo(x, props.height)
      ctx.stroke()
    }
    
    // 水平网格线
    for (let i = 0; i <= 5; i++) {
      const y = (props.height / 5) * i
      ctx.beginPath()
      ctx.moveTo(0, y)
      ctx.lineTo(props.width, y)
      ctx.stroke()
    }
  }
  
  // 绘制数据线
  props.data.forEach(series => {
    if (!series.data.length) return
    
    ctx!.strokeStyle = series.color
    ctx!.lineWidth = 2
    ctx!.beginPath()
    
    const stepX = props.width / (series.data.length - 1)
    const maxValue = Math.max(...series.data)
    const minValue = Math.min(...series.data)
    const range = maxValue - minValue || 1
    
    series.data.forEach((value, index) => {
      const x = index * stepX
      const y = props.height - ((value - minValue) / range) * props.height
      
      if (index === 0) {
        ctx!.moveTo(x, y)
      } else {
        ctx!.lineTo(x, y)
      }
    })
    
    ctx!.stroke()
  })
}

const handleMouseMove = (event: MouseEvent) => {
  // 可以添加鼠标悬停显示数值的功能
}

onMounted(() => {
  if (canvasRef.value) {
    ctx = canvasRef.value.getContext('2d')
    drawChart()
  }
})

watch(() => props.data, drawChart, { deep: true })
</script>

<style scoped>
.chart-container {
  position: relative;
  display: inline-block;
}

.chart-legend {
  display: flex;
  gap: 16px;
  margin-top: 8px;
  justify-content: center;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 4px;
}

.legend-color {
  width: 12px;
  height: 12px;
  border-radius: 2px;
}

.legend-label {
  font-size: 12px;
  color: #666;
}
</style>`
  },
  {
    id: 'progress',
    name: '进度条',
    description: '进度显示控件',
    keywords: ['进度', 'progress', '进度条', '百分比', '完成度'],
    code: `<template>
  <div class="progress-container">
    <div class="progress-label" v-if="showLabel">
      {{ label }} {{ percentage }}%
    </div>
    <div class="progress-bar">
      <div 
        class="progress-fill"
        :style="{ 
          width: percentage + '%',
          backgroundColor: color 
        }"
      ></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  value: number
  max?: number
  label?: string
  color?: string
  showLabel?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  max: 100,
  label: '进度',
  color: '#409eff',
  showLabel: true
})

const percentage = computed(() => {
  return Math.min(100, Math.max(0, (props.value / props.max) * 100))
})
</script>

<style scoped>
.progress-container {
  width: 100%;
}

.progress-label {
  font-size: 14px;
  color: #333;
  margin-bottom: 8px;
}

.progress-bar {
  width: 100%;
  height: 20px;
  background-color: #f0f0f0;
  border-radius: 10px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  transition: width 0.3s ease;
  border-radius: 10px;
}
</style>`
  },
  {
    id: 'switch',
    name: '开关控件',
    description: '开关切换控件',
    keywords: ['开关', 'switch', '切换', '开启', '关闭', '状态切换'],
    code: `<template>
  <div class="switch-container">
    <label class="switch">
      <input 
        type="checkbox" 
        :checked="modelValue"
        @change="handleChange"
      />
      <span class="slider"></span>
    </label>
    <span class="switch-label" v-if="label">{{ label }}</span>
  </div>
</template>

<script setup lang="ts">
interface Props {
  modelValue: boolean
  label?: string
}

defineProps<Props>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

const handleChange = (event: Event) => {
  const target = event.target as HTMLInputElement
  emit('update:modelValue', target.checked)
}
</script>

<style scoped>
.switch-container {
  display: flex;
  align-items: center;
  gap: 8px;
}

.switch {
  position: relative;
  display: inline-block;
  width: 60px;
  height: 34px;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #ccc;
  transition: .4s;
  border-radius: 34px;
}

.slider:before {
  position: absolute;
  content: "";
  height: 26px;
  width: 26px;
  left: 4px;
  bottom: 4px;
  background-color: white;
  transition: .4s;
  border-radius: 50%;
}

input:checked + .slider {
  background-color: #409eff;
}

input:checked + .slider:before {
  transform: translateX(26px);
}

.switch-label {
  font-size: 14px;
  color: #333;
}
</style>`
  }
]

export class AIControlService {
  // 生成控件 - 增强版本
  async generateControl(request: AIControlRequest): Promise<AIControlResponse> {
    try {
      // 预处理用户输入 - 提取关键信息
      const processedRequest = this.preprocessRequest(request)
      
      // 首先尝试调用真实的Claude API
      const data = await backendApi.post<AIControlResponse>('/api/ai/generate-control', {
        description: processedRequest.description
      })
      
      if (data.success && data.code) {
        // 验证生成的代码质量
        const validationResult = this.validateGeneratedCode(data.code)
        if (validationResult.isValid) {
          return {
            ...data,
            source: data.source || 'claude-api',
            quality: validationResult.quality
          }
        } else {
          console.warn('Claude API生成的代码质量不符合要求，使用备用模板')
          return this.generateFromTemplate(processedRequest)
        }
      }
      
      // 如果API调用失败或没有返回代码，使用备用模板
      console.warn('Claude API调用失败，使用备用模板')
      return this.generateFromTemplate(processedRequest)
      
    } catch (error) {
      console.error('AI生成控件错误:', error)
      
      // 如果出错，使用备用模板
      return this.generateFromTemplate(request)
    }
  }
  
  // 使用模板生成（备用方案）
  private generateFromTemplate(request: AIControlRequest): AIControlResponse {
    try {
      const selectedTemplate = this.findBestMatchingTemplate(request.description)
      
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

  // 智能匹配最佳模板
  private findBestMatchingTemplate(description: string): ControlTemplate | undefined {
    const normalizedDesc = description.toLowerCase()
    let bestMatch: ControlTemplate | undefined
    let highestScore = 0

    for (const template of templates) {
      const score = this.calculateMatchScore(normalizedDesc, template)
      if (score > highestScore) {
        highestScore = score
        bestMatch = template
      }
    }

    // 只有当匹配分数超过阈值时才返回匹配结果
    return highestScore > 0.3 ? bestMatch : undefined
  }

  // 计算匹配分数
  private calculateMatchScore(description: string, template: ControlTemplate): number {
    let score = 0
    const keywords = template.keywords || []

    // 关键词匹配
    for (const keyword of keywords) {
      if (description.includes(keyword.toLowerCase())) {
        score += 1
      }
    }

    // 模板名称匹配
    if (description.includes(template.name.toLowerCase())) {
      score += 2
    }

    // 模板描述匹配
    const descWords = template.description.toLowerCase().split(/\s+/)
    for (const word of descWords) {
      if (description.includes(word) && word.length > 2) {
        score += 0.5
      }
    }

    // 归一化分数
    return score / (keywords.length + 3)
  }
  
  // 预处理用户请求 - 提取关键信息
  private preprocessRequest(request: AIControlRequest): AIControlRequest {
    const description = request.description.trim()
    
    // 提取关键词
    const keywords = this.extractKeywords(description)
    
    // 确定控件分类
    const category = this.determineCategory(description, keywords)
    
    return {
      ...request,
      description,
      keywords,
      category
    }
  }

  // 提取关键词
  private extractKeywords(description: string): string[] {
    const keywords: string[] = []
    const normalizedDesc = description.toLowerCase()
    
    // 预定义的关键词映射
    const keywordMap = {
      '仪表': ['仪表', 'gauge', '表盘', '刻度', '指针'],
      '按钮': ['按钮', 'button', '控制', '点击', '操作'],
      '指示灯': ['led', '指示灯', '状态灯', '信号灯', '灯'],
      '图表': ['图表', 'chart', '曲线', '波形', '数据'],
      '进度': ['进度', 'progress', '百分比', '完成度'],
      '开关': ['开关', 'switch', '切换', '状态切换']
    }
    
    for (const [category, words] of Object.entries(keywordMap)) {
      for (const word of words) {
        if (normalizedDesc.includes(word)) {
          keywords.push(word)
        }
      }
    }
    
    return [...new Set(keywords)] // 去重
  }

  // 确定控件分类
  private determineCategory(description: string, keywords: string[]): string {
    const normalizedDesc = description.toLowerCase()
    
    if (keywords.some(k => ['仪表', 'gauge', '表盘'].includes(k))) {
      return 'gauge'
    }
    if (keywords.some(k => ['按钮', 'button', '控制'].includes(k))) {
      return 'button'
    }
    if (keywords.some(k => ['led', '指示灯', '状态灯'].includes(k))) {
      return 'led'
    }
    if (keywords.some(k => ['图表', 'chart', '曲线'].includes(k))) {
      return 'chart'
    }
    if (keywords.some(k => ['进度', 'progress'].includes(k))) {
      return 'progress'
    }
    if (keywords.some(k => ['开关', 'switch', '切换'].includes(k))) {
      return 'switch'
    }
    
    return 'unknown'
  }

  // 验证生成的代码质量
  private validateGeneratedCode(code: string): ValidationResult {
    const issues: string[] = []
    const suggestions: string[] = []
    let score = 100

    // 基本结构检查
    if (!code.includes('<template>') || !code.includes('</template>')) {
      issues.push('缺少template部分')
      score -= 30
    }

    if (!code.includes('<script setup') || !code.includes('</script>')) {
      issues.push('缺少script setup部分')
      score -= 30
    }

    if (!code.includes('<style') || !code.includes('</style>')) {
      issues.push('缺少style部分')
      score -= 20
    }

    // TypeScript检查
    if (!code.includes('lang="ts"')) {
      suggestions.push('建议使用TypeScript')
      score -= 10
    }

    // Props接口检查
    if (code.includes('defineProps') && !code.includes('interface Props')) {
      suggestions.push('建议定义Props接口')
      score -= 5
    }

    // 响应式检查
    if (!code.includes('ref(') && !code.includes('reactive(') && !code.includes('computed(')) {
      suggestions.push('建议使用Vue 3响应式API')
      score -= 10
    }

    // 样式检查
    if (!code.includes('scoped')) {
      suggestions.push('建议使用scoped样式')
      score -= 5
    }

    // 代码长度检查
    if (code.length < 500) {
      issues.push('代码过于简单')
      score -= 15
    }

    // 确保分数在合理范围内
    score = Math.max(0, Math.min(100, score))

    const quality: CodeQuality = {
      score,
      issues,
      suggestions
    }

    return {
      isValid: score >= 60, // 60分以上认为是有效的
      quality,
      errors: issues
    }
  }

  // 获取所有模板
  getTemplates(): ControlTemplate[] {
    return templates
  }

  // 获取模板统计信息
  getTemplateStats() {
    return {
      total: templates.length,
      categories: [...new Set(templates.map(t => t.category).filter(Boolean))],
      mostComplex: templates.reduce((max, t) => 
        (t.complexity || 0) > (max.complexity || 0) ? t : max
      )
    }
  }

  // 搜索模板
  searchTemplates(query: string): ControlTemplate[] {
    const normalizedQuery = query.toLowerCase()
    return templates.filter(template => 
      template.name.toLowerCase().includes(normalizedQuery) ||
      template.description.toLowerCase().includes(normalizedQuery) ||
      template.keywords?.some(k => k.toLowerCase().includes(normalizedQuery))
    )
  }
}

// 导出服务实例
export const aiControlService = new AIControlService()
