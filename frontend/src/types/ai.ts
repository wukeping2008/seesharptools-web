// AI控件生成相关类型定义

// 控件生成请求
export interface AIControlRequest {
  description: string  // 控件描述
  type?: string       // 控件类型（可选）
  keywords?: string[] // 提取的关键词
  category?: string   // 控件分类
}

// 控件生成响应
export interface AIControlResponse {
  success: boolean
  code?: string      // 生成的Vue组件代码
  error?: string     // 错误信息
  source?: string    // 代码来源：'claude-api' | 'template'
  quality?: CodeQuality // 代码质量评估
  template?: ControlTemplate // 使用的模板（如果是模板生成）
}

// 代码质量评估
export interface CodeQuality {
  score: number      // 质量分数 0-100
  issues: string[]   // 发现的问题
  suggestions: string[] // 改进建议
}

// 预定义控件模板
export interface ControlTemplate {
  id: string
  name: string
  description: string
  keywords?: string[]
  code: string
  preview?: string
  category?: string  // 控件分类
  complexity?: number // 复杂度 1-5
}

// 代码验证结果
export interface ValidationResult {
  isValid: boolean
  quality: CodeQuality
  errors: string[]
}
