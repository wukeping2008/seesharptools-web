import axios from 'axios'

const API_BASE = 'http://localhost:5000/api'

export interface AnalysisRequest {
  signalType: string
  frequency: number
  amplitude: number
  useAdvancedModel: boolean
  enableVisualAnalysis: boolean
}

export interface WaveformAnalysisResult {
  success: boolean
  message?: string
  signalType?: string
  quality?: string
  analysis?: string
  features?: any
  modelUsed?: string
  processingLevel?: string
  timestamp?: string
}

export interface TestReport {
  success: boolean
  title?: string
  content?: string
  modelUsed?: string
  processingLevel?: string
  estimatedCost?: number
  generatedAt?: string
}

export const aiService = {
  // 测试AI连接
  async testConnection() {
    try {
      const response = await axios.get(`${API_BASE}/AITest/test-connection`)
      return response.data
    } catch (error) {
      console.error('AI连接测试失败:', error)
      throw error
    }
  },

  // 使用ERNIE-4.5进行高级分析
  async performAdvancedAnalysis(request: AnalysisRequest) {
    try {
      const response = await axios.post(`${API_BASE}/AITest/test-advanced-analysis`, request)
      return response.data
    } catch (error) {
      console.error('高级分析失败:', error)
      throw error
    }
  },

  // 测试模型降级机制
  async testFallback() {
    try {
      const response = await axios.get(`${API_BASE}/AITest/test-fallback`)
      return response.data
    } catch (error) {
      console.error('降级测试失败:', error)
      throw error
    }
  },

  // 实时流分析
  async testStreamAnalysis() {
    try {
      const response = await axios.post(`${API_BASE}/AITest/test-stream-analysis`)
      return response.data
    } catch (error) {
      console.error('流分析失败:', error)
      throw error
    }
  },

  // 原有功能（适配新的API端点）
  async analyzeWaveform(data: any) {
    try {
      const response = await axios.post(`${API_BASE}/dataacquisition/analyze`, data)
      return response.data
    } catch (error) {
      console.error('波形分析失败:', error)
      throw error
    }
  },

  // 异常检测
  async detectAnomalies(data: any) {
    try {
      const response = await axios.post(`${API_BASE}/dataacquisition/detect-anomaly`, data)
      return response.data
    } catch (error) {
      console.error('异常检测失败:', error)
      throw error
    }
  },

  // 生成报告
  async generateReport() {
    try {
      const response = await axios.post(`${API_BASE}/dataacquisition/generate-report`)
      return response.data
    } catch (error) {
      console.error('报告生成失败:', error)
      throw error
    }
  }
}