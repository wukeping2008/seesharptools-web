/**
 * Test execution related types
 */

export interface TestExecutionRecord {
  id: number
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
  createdAt: string
  updatedAt: string
}

export interface TestExecutionHistoryQuery {
  maxCount?: number
  startDate?: string
  endDate?: string
  deviceType?: string
  testType?: string
  success?: boolean
  userId?: string
  aiProvider?: string
  minCodeQualityScore?: number
  sortOrder?: TestExecutionSortOrder
}

export enum TestExecutionSortOrder {
  NewestFirst = 0,
  OldestFirst = 1,
  QualityScoreDesc = 2,
  QualityScoreAsc = 3,
  ExecutionTimeDesc = 4,
  ExecutionTimeAsc = 5
}

export interface TestExecutionStatistics {
  totalExecutions: number
  successfulExecutions: number
  failedExecutions: number
  successRate: number
  averageCodeQuality: number
  averageExecutionTime: number
  totalTokensUsed: number
  executionsByDeviceType: Record<string, number>
  executionsByTestType: Record<string, number>
  executionsByAIProvider: Record<string, number>
  dailyTrends: ExecutionTrend[]
}

export interface ExecutionTrend {
  date: string
  executionCount: number
  successCount: number
  averageQuality: number
  tokensUsed: number
}

export interface ExecutionPerformanceMetrics {
  fastestExecutionMs: number
  slowestExecutionMs: number
  averageExecutionMs: number
  medianExecutionMs: number
  p95ExecutionMs: number
  performanceByDevice: Record<string, DevicePerformanceMetrics>
  performanceByTestType: Record<string, TestTypePerformanceMetrics>
}

export interface DevicePerformanceMetrics {
  averageExecutionMs: number
  successRate: number
  averageCodeQuality: number
  totalExecutions: number
}

export interface TestTypePerformanceMetrics {
  averageExecutionMs: number
  successRate: number
  averageCodeQuality: number
  totalExecutions: number
  averageTokensUsed: number
}

export interface TestExecutionRequest {
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
}