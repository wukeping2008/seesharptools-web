import type { 
  TestExecutionRecord, 
  TestExecutionHistoryQuery, 
  TestExecutionStatistics,
  ExecutionPerformanceMetrics 
} from '@/types/test-execution'

const API_BASE = '/api/TestExecution'

/**
 * Test history service for managing test execution records
 */
export class TestHistoryService {
  /**
   * Save test execution result
   */
  static async saveExecutionResult(request: {
    testRequirement: string
    generatedCode: string
    deviceType?: string
    testType?: string
    aiProvider?: string
    tokensUsed?: number
    codeQualityScore?: number
    userId?: string
    resultData?: string
    success: boolean
    errorMessage?: string
    executionTimeMs?: number
  }): Promise<number> {
    const response = await fetch(API_BASE, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to save execution result')
    }

    return await response.json()
  }

  /**
   * Get test execution result by ID
   */
  static async getExecutionResult(id: number): Promise<TestExecutionRecord> {
    const response = await fetch(`${API_BASE}/${id}`)

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to get execution result')
    }

    return await response.json()
  }

  /**
   * Get test execution history
   */
  static async getExecutionHistory(query: TestExecutionHistoryQuery): Promise<TestExecutionRecord[]> {
    const response = await fetch(`${API_BASE}/history`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(query),
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to get execution history')
    }

    return await response.json()
  }

  /**
   * Get execution statistics
   */
  static async getExecutionStatistics(startDate: Date, endDate: Date): Promise<TestExecutionStatistics> {
    const params = new URLSearchParams({
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
    })

    const response = await fetch(`${API_BASE}/statistics?${params}`)

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to get execution statistics')
    }

    return await response.json()
  }

  /**
   * Delete test execution record
   */
  static async deleteExecutionRecord(id: number): Promise<void> {
    const response = await fetch(`${API_BASE}/${id}`, {
      method: 'DELETE',
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to delete execution record')
    }
  }

  /**
   * Get executions by device type
   */
  static async getExecutionsByDeviceType(deviceType: string, maxCount = 50): Promise<TestExecutionRecord[]> {
    const params = new URLSearchParams({ maxCount: maxCount.toString() })
    const response = await fetch(`${API_BASE}/device/${encodeURIComponent(deviceType)}?${params}`)

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to get executions by device type')
    }

    return await response.json()
  }

  /**
   * Get executions by test type
   */
  static async getExecutionsByTestType(testType: string, maxCount = 50): Promise<TestExecutionRecord[]> {
    const params = new URLSearchParams({ maxCount: maxCount.toString() })
    const response = await fetch(`${API_BASE}/test/${encodeURIComponent(testType)}?${params}`)

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to get executions by test type')
    }

    return await response.json()
  }

  /**
   * Update execution result
   */
  static async updateExecutionResult(
    id: number, 
    resultData: string, 
    success: boolean, 
    errorMessage?: string
  ): Promise<void> {
    const response = await fetch(`${API_BASE}/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        resultData,
        success,
        errorMessage,
      }),
    })

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to update execution result')
    }
  }

  /**
   * Get performance metrics
   */
  static async getPerformanceMetrics(startDate: Date, endDate: Date): Promise<ExecutionPerformanceMetrics> {
    const params = new URLSearchParams({
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
    })

    const response = await fetch(`${API_BASE}/performance?${params}`)

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to get performance metrics')
    }

    return await response.json()
  }

  /**
   * Get recent executions (last 24 hours)
   */
  static async getRecentExecutions(maxCount = 20): Promise<TestExecutionRecord[]> {
    const params = new URLSearchParams({ maxCount: maxCount.toString() })
    const response = await fetch(`${API_BASE}/recent?${params}`)

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to get recent executions')
    }

    return await response.json()
  }

  /**
   * Get execution summary for dashboard
   */
  static async getExecutionSummary(): Promise<{
    statistics: TestExecutionStatistics
    recentExecutions: TestExecutionRecord[]
    period: { startDate: string; endDate: string }
  }> {
    const response = await fetch(`${API_BASE}/summary`)

    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.error || 'Failed to get execution summary')
    }

    return await response.json()
  }

  /**
   * Export execution history to CSV
   */
  static async exportExecutionHistory(query: TestExecutionHistoryQuery): Promise<Blob> {
    const executions = await this.getExecutionHistory(query)
    
    const headers = [
      'ID', 'Test Requirement', 'Device Type', 'Test Type', 'AI Provider',
      'Success', 'Execution Time (ms)', 'Code Quality Score', 'Tokens Used',
      'Created At', 'Error Message'
    ]

    const csvContent = [
      headers.join(','),
      ...executions.map(execution => [
        execution.id,
        `"${execution.testRequirement.replace(/"/g, '""')}"`,
        execution.deviceType || '',
        execution.testType || '',
        execution.aiProvider || '',
        execution.success,
        execution.executionTimeMs || '',
        execution.codeQualityScore || '',
        execution.tokensUsed || '',
        execution.createdAt,
        `"${(execution.errorMessage || '').replace(/"/g, '""')}"`
      ].join(','))
    ].join('\n')

    return new Blob([csvContent], { type: 'text/csv;charset=utf-8' })
  }

  /**
   * Get execution trends for chart visualization
   */
  static async getExecutionTrends(days = 30): Promise<{
    labels: string[]
    successData: number[]
    failureData: number[]
    qualityData: number[]
  }> {
    const endDate = new Date()
    const startDate = new Date()
    startDate.setDate(startDate.getDate() - days)

    const statistics = await this.getExecutionStatistics(startDate, endDate)
    
    return {
      labels: statistics.dailyTrends.map(trend => 
        new Date(trend.date).toLocaleDateString('zh-CN', { month: 'short', day: 'numeric' })
      ),
      successData: statistics.dailyTrends.map(trend => trend.successCount),
      failureData: statistics.dailyTrends.map(trend => trend.executionCount - trend.successCount),
      qualityData: statistics.dailyTrends.map(trend => Math.round(trend.averageQuality * 10) / 10)
    }
  }
}