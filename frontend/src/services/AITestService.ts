import { backendApi } from './BackendApiService'
import type { TestRequirement, TestTemplate, CodeQuality } from '@/types/ai'

/**
 * AI测试服务 - 处理AI智能测试平台的所有API调用
 */
export class AITestService {
  /**
   * 生成测试代码
   */
  static async generateTestCode(request: {
    requirement: string
    deviceType?: string
    testType?: string
    templateId?: string
  }) {
    try {
      const response = await backendApi.post('/api/ai-test/generate', request)
      return {
        success: true,
        code: response.generatedCode,
        quality: response.codeQuality,
        template: response.selectedTemplate
      }
    } catch (error) {
      console.error('生成测试代码失败:', error)
      return {
        success: false,
        error: error instanceof Error ? error.message : '生成失败'
      }
    }
  }

  /**
   * 获取推荐模板
   */
  static async getRecommendedTemplates(requirement?: string) {
    try {
      const params = requirement ? { requirement } : {}
      const response = await backendApi.get('/api/ai-test/templates/recommended', { params })
      return {
        success: true,
        templates: response.templates || []
      }
    } catch (error) {
      console.error('获取推荐模板失败:', error)
      return {
        success: false,
        templates: []
      }
    }
  }

  /**
   * 保存用户模板
   */
  static async saveUserTemplate(template: {
    name: string
    description: string
    deviceType: string
    testType: string
    code: string
    tags?: string[]
  }) {
    try {
      const response = await backendApi.post('/api/ai-test/templates', template)
      return {
        success: true,
        templateId: response.id
      }
    } catch (error) {
      console.error('保存模板失败:', error)
      return {
        success: false,
        error: error instanceof Error ? error.message : '保存失败'
      }
    }
  }

  /**
   * 获取模板统计信息
   */
  static async getTemplateStatistics() {
    try {
      const response = await backendApi.get('/api/ai-test/templates/statistics')
      return {
        success: true,
        statistics: response
      }
    } catch (error) {
      console.error('获取模板统计失败:', error)
      return {
        success: false,
        statistics: null
      }
    }
  }

  /**
   * 执行生成的测试代码
   */
  static async executeTestCode(code: string) {
    try {
      const response = await backendApi.post('/api/csharp-runner/execute', {
        Code: code,
        Timeout: 30000
      })
      return {
        success: response.Success,
        result: response.Output,
        error: response.Error,
        output: response.Output,
        elapsedMs: response.ElapsedMilliseconds
      }
    } catch (error) {
      console.error('执行测试代码失败:', error)
      return {
        success: false,
        error: error instanceof Error ? error.message : '执行失败'
      }
    }
  }

  /**
   * 分析需求并提取关键信息
   */
  static async analyzeRequirement(requirement: string) {
    try {
      const response = await backendApi.post('/api/ai-test/analyze-requirement', {
        requirement
      })
      return {
        success: true,
        analysis: response
      }
    } catch (error) {
      console.error('分析需求失败:', error)
      return {
        success: false,
        analysis: null
      }
    }
  }

  /**
   * 获取设备支持的测试类型
   */
  static getDeviceTestTypes(deviceType: string) {
    const deviceTestMap: Record<string, string[]> = {
      'JY5500': ['electrical', 'signal', 'thd_analysis', 'frequency_response'],
      'JYUSB1601': ['vibration', 'temperature', 'data_acquisition', 'spectrum_analysis'],
      '通用': ['custom', 'general']
    }
    
    return deviceTestMap[deviceType] || ['custom']
  }

  /**
   * 验证生成的代码质量
   */
  static validateCodeQuality(code: string): CodeQuality {
    const issues: string[] = []
    const suggestions: string[] = []
    let score = 100

    // 基本语法检查
    if (!code.includes('using System')) {
      issues.push('缺少必要的using语句')
      score -= 10
    }

    if (!code.includes('class ') && !code.includes('static void Main')) {
      issues.push('缺少主类或Main方法')
      score -= 20
    }

    // 异常处理检查
    if (!code.includes('try') || !code.includes('catch')) {
      suggestions.push('建议添加异常处理机制')
      score -= 5
    }

    // 资源释放检查
    if (code.includes('new ') && !code.includes('using') && !code.includes('Dispose')) {
      suggestions.push('建议使用using语句或手动释放资源')
      score -= 5
    }

    // 代码长度检查
    const lines = code.split('\n').length
    if (lines > 200) {
      suggestions.push('代码较长，建议拆分为多个方法')
      score -= 3
    }

    // 注释检查
    const commentLines = code.split('\n').filter(line => 
      line.trim().startsWith('//') || line.trim().startsWith('/*')
    ).length
    const commentRatio = commentLines / lines
    if (commentRatio < 0.1) {
      suggestions.push('建议增加代码注释以提高可读性')
      score -= 2
    }

    return {
      score: Math.max(score, 0),
      issues,
      suggestions
    }
  }

  /**
   * 格式化测试结果
   */
  static formatTestResult(result: any) {
    if (!result) return null

    return {
      deviceType: result.deviceType || '未知设备',
      analysisType: result.analysisType || '通用分析',
      timestamp: result.timestamp || new Date().toISOString(),
      spectrumData: result.spectrumData || null,
      measurements: result.measurements || {},
      summary: result.summary || '测试完成'
    }
  }

  /**
   * 获取测试历史记录
   */
  static async getTestHistory(limit: number = 10) {
    try {
      const response = await backendApi.get('/api/ai-test/history', {
        params: { limit }
      })
      return {
        success: true,
        history: response.history || []
      }
    } catch (error) {
      console.error('获取测试历史失败:', error)
      return {
        success: false,
        history: []
      }
    }
  }

  /**
   * 保存测试结果
   */
  static async saveTestResult(result: {
    requirement: string
    generatedCode: string
    executionResult: any
    deviceType: string
    testType: string
  }) {
    try {
      const response = await backendApi.post('/api/ai-test/results', result)
      return {
        success: true,
        resultId: response.id
      }
    } catch (error) {
      console.error('保存测试结果失败:', error)
      return {
        success: false,
        error: error instanceof Error ? error.message : '保存失败'
      }
    }
  }
}

export default AITestService
