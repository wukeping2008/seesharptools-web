// AI控件生成相关类型定义

// 控件生成请求
export interface AIControlRequest {
  description: string  // 控件描述
  type?: string       // 控件类型（可选）
}

// 控件生成响应
export interface AIControlResponse {
  success: boolean
  code?: string      // 生成的Vue组件代码
  error?: string     // 错误信息
}

// 预定义控件模板
export interface ControlTemplate {
  id: string
  name: string
  description: string
  code: string
  preview?: string
}
