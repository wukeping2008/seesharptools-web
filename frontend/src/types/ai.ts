// AI驱动控件生成系统类型定义

export interface AIControlRequest {
  // 用户自然语言描述
  description: string
  // 控件类型提示
  type?: 'gauge' | 'chart' | 'indicator' | 'control' | 'instrument' | 'custom'
  // 参数约束
  constraints?: {
    width?: number
    height?: number
    dataType?: 'number' | 'string' | 'boolean' | 'array'
    realtime?: boolean
    interactive?: boolean
  }
  // 样式偏好
  stylePreference?: 'professional' | 'modern' | 'classic' | 'minimal'
  // 功能要求
  features?: string[]
}

export interface AIControlResponse {
  // 生成状态
  success: boolean
  // 错误信息
  error?: string
  // 生成的控件信息
  control?: {
    // 控件名称
    name: string
    // 控件描述
    description: string
    // 控件类型
    type: string
    // Vue组件代码
    componentCode: string
    // TypeScript类型定义
    typeDefinitions: string
    // 样式代码
    styleCode: string
    // 使用示例
    exampleCode: string
    // 属性配置
    props: AIControlProp[]
    // 事件定义
    events: AIControlEvent[]
  }
  // 技术可行性评估
  feasibility?: {
    score: number // 0-100
    issues: string[]
    suggestions: string[]
  }
  // 生成过程信息
  metadata?: {
    processingTime: number
    tokensUsed: number
    confidence: number
  }
}

export interface AIControlProp {
  name: string
  type: string
  required: boolean
  default?: any
  description: string
  validation?: {
    min?: number
    max?: number
    pattern?: string
    options?: string[]
  }
}

export interface AIControlEvent {
  name: string
  description: string
  payload: string
  example: string
}

export interface AIControlTemplate {
  id: string
  name: string
  description: string
  category: string
  baseComponent: string
  parameters: {
    [key: string]: {
      type: string
      description: string
      default: any
      required: boolean
    }
  }
  codeTemplate: string
  styleTemplate: string
}

export interface AIGenerationContext {
  // 已有控件库信息
  availableComponents: string[]
  // 技术栈信息
  techStack: {
    framework: 'vue3'
    language: 'typescript'
    styling: 'scss'
    charts: 'echarts'
    ui: 'element-plus'
  }
  // 项目约束
  constraints: {
    maxComplexity: number
    performanceRequirements: string[]
    compatibilityRequirements: string[]
  }
  // 最佳实践
  bestPractices: string[]
}

export interface AIControlValidation {
  // 代码质量检查
  codeQuality: {
    syntax: boolean
    typescript: boolean
    vue3Composition: boolean
    performance: boolean
  }
  // 安全性检查
  security: {
    xssProtection: boolean
    inputValidation: boolean
    noEval: boolean
  }
  // 兼容性检查
  compatibility: {
    vue3: boolean
    typescript: boolean
    elementPlus: boolean
    echarts: boolean
  }
  // 性能评估
  performance: {
    renderComplexity: number
    memoryUsage: number
    bundleSize: number
  }
}

export interface AIConversationHistory {
  id: string
  timestamp: number
  userMessage: string
  aiResponse: AIControlResponse
  feedback?: {
    rating: number // 1-5
    comments: string
    improvements: string[]
  }
}

export interface AIControlLibrary {
  // 生成的控件库
  controls: {
    [controlId: string]: {
      metadata: {
        id: string
        name: string
        description: string
        author: 'AI'
        version: string
        createdAt: number
        updatedAt: number
        tags: string[]
      }
      code: {
        component: string
        types: string
        styles: string
        example: string
      }
      usage: {
        downloadCount: number
        rating: number
        feedback: string[]
      }
    }
  }
  // 控件分类
  categories: {
    [categoryName: string]: string[]
  }
  // 搜索索引
  searchIndex: {
    [keyword: string]: string[]
  }
}

// AI服务配置
export interface AIServiceConfig {
  // API配置
  apiKey: string
  apiUrl: string
  model: string
  // 生成参数
  maxTokens: number
  temperature: number
  topP: number
  // 安全设置
  contentFilter: boolean
  rateLimiting: {
    requestsPerMinute: number
    tokensPerHour: number
  }
  // 缓存设置
  caching: {
    enabled: boolean
    ttl: number // 缓存时间（秒）
  }
}

// AI提示词模板
export interface AIPromptTemplate {
  id: string
  name: string
  description: string
  category: 'generation' | 'validation' | 'optimization' | 'explanation'
  template: string
  variables: string[]
  examples: {
    input: any
    output: any
  }[]
}

// AI控件生成统计
export interface AIGenerationStats {
  totalRequests: number
  successfulGenerations: number
  failedGenerations: number
  averageProcessingTime: number
  popularControlTypes: {
    [type: string]: number
  }
  userSatisfactionRating: number
  commonFailureReasons: {
    [reason: string]: number
  }
}
