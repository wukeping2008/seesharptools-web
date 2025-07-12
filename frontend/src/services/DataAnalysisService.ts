// 数据分析和报告服务
import type { 
  AnalysisRequest, 
  AnalysisResult, 
  ReportConfig, 
  ReportData,
  StatisticalMetrics,
  TrendAnalysis,
  AnomalyDetection
} from '@/types/analysis'
import { backendApi } from './BackendApiService'

export class DataAnalysisService {
  // 执行数据分析
  async analyzeData(request: AnalysisRequest): Promise<AnalysisResult> {
    try {
      // 调用后端分析API
      const result = await backendApi.post<AnalysisResult>('/api/analysis/analyze', request)
      
      // 如果后端分析失败，使用前端分析
      if (!result.success) {
        return this.performClientSideAnalysis(request)
      }
      
      return result
    } catch (error) {
      console.warn('后端分析失败，使用前端分析:', error)
      return this.performClientSideAnalysis(request)
    }
  }

  // 前端数据分析（备用方案）
  private performClientSideAnalysis(request: AnalysisRequest): AnalysisResult {
    const { data, analysisType } = request
    
    try {
      let result: any = {}
      
      switch (analysisType) {
        case 'statistical':
          result = this.calculateStatistics(data)
          break
        case 'trend':
          result = this.analyzeTrend(data)
          break
        case 'anomaly':
          result = this.detectAnomalies(data)
          break
        case 'frequency':
          result = this.analyzeFrequency(data)
          break
        default:
          result = this.calculateBasicMetrics(data)
      }
      
      return {
        success: true,
        analysisType,
        result,
        timestamp: new Date().toISOString(),
        source: 'client-side'
      }
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : '分析失败',
        analysisType,
        timestamp: new Date().toISOString()
      }
    }
  }

  // 统计分析
  private calculateStatistics(data: number[]): StatisticalMetrics {
    if (!data.length) {
      throw new Error('数据为空')
    }

    const sorted = [...data].sort((a, b) => a - b)
    const n = data.length
    
    // 基本统计量
    const sum = data.reduce((acc, val) => acc + val, 0)
    const mean = sum / n
    const variance = data.reduce((acc, val) => acc + Math.pow(val - mean, 2), 0) / n
    const stdDev = Math.sqrt(variance)
    
    // 分位数
    const q1 = this.percentile(sorted, 25)
    const median = this.percentile(sorted, 50)
    const q3 = this.percentile(sorted, 75)
    const iqr = q3 - q1
    
    return {
      count: n,
      sum,
      mean,
      median,
      mode: this.calculateMode(data),
      variance,
      standardDeviation: stdDev,
      min: sorted[0],
      max: sorted[n - 1],
      range: sorted[n - 1] - sorted[0],
      q1,
      q3,
      iqr,
      skewness: this.calculateSkewness(data, mean, stdDev),
      kurtosis: this.calculateKurtosis(data, mean, stdDev)
    }
  }

  // 趋势分析
  private analyzeTrend(data: number[]): TrendAnalysis {
    if (data.length < 2) {
      throw new Error('趋势分析需要至少2个数据点')
    }

    // 线性回归
    const n = data.length
    const x = Array.from({ length: n }, (_, i) => i)
    const sumX = x.reduce((acc, val) => acc + val, 0)
    const sumY = data.reduce((acc, val) => acc + val, 0)
    const sumXY = x.reduce((acc, val, i) => acc + val * data[i], 0)
    const sumXX = x.reduce((acc, val) => acc + val * val, 0)
    
    const slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX)
    const intercept = (sumY - slope * sumX) / n
    
    // R平方
    const yMean = sumY / n
    const ssTotal = data.reduce((acc, val) => acc + Math.pow(val - yMean, 2), 0)
    const ssResidual = data.reduce((acc, val, i) => {
      const predicted = slope * i + intercept
      return acc + Math.pow(val - predicted, 2)
    }, 0)
    const rSquared = 1 - (ssResidual / ssTotal)
    
    // 趋势方向
    let direction: 'increasing' | 'decreasing' | 'stable'
    if (Math.abs(slope) < 0.01) {
      direction = 'stable'
    } else if (slope > 0) {
      direction = 'increasing'
    } else {
      direction = 'decreasing'
    }
    
    // 变化率
    const changeRate = ((data[n - 1] - data[0]) / data[0]) * 100
    
    return {
      slope,
      intercept,
      rSquared,
      direction,
      changeRate,
      confidence: rSquared > 0.7 ? 'high' : rSquared > 0.4 ? 'medium' : 'low',
      prediction: this.generatePrediction(slope, intercept, n)
    }
  }

  // 异常检测
  private detectAnomalies(data: number[]): AnomalyDetection {
    if (data.length < 3) {
      throw new Error('异常检测需要至少3个数据点')
    }

    const stats = this.calculateStatistics(data)
    const { mean, standardDeviation, q1, q3, iqr } = stats
    
    // IQR方法检测异常值
    const lowerBound = q1 - 1.5 * iqr
    const upperBound = q3 + 1.5 * iqr
    
    // Z-score方法检测异常值
    const zThreshold = 2.5
    
    const anomalies: Array<{ index: number; value: number; type: string; severity: string }> = []
    
    data.forEach((value, index) => {
      const zScore = Math.abs((value - mean) / standardDeviation)
      let isAnomaly = false
      let type = ''
      let severity = 'low'
      
      // IQR异常检测
      if (value < lowerBound || value > upperBound) {
        isAnomaly = true
        type = 'iqr_outlier'
        severity = 'medium'
      }
      
      // Z-score异常检测
      if (zScore > zThreshold) {
        isAnomaly = true
        type = type ? `${type},z_score` : 'z_score'
        severity = zScore > 3 ? 'high' : 'medium'
      }
      
      if (isAnomaly) {
        anomalies.push({ index, value, type, severity })
      }
    })
    
    return {
      anomalies,
      anomalyRate: (anomalies.length / data.length) * 100,
      method: 'iqr_zscore',
      thresholds: {
        iqr: { lower: lowerBound, upper: upperBound },
        zscore: zThreshold
      }
    }
  }

  // 频率分析
  private analyzeFrequency(data: number[]): any {
    // 创建频率分布
    const frequency = new Map<number, number>()
    data.forEach(value => {
      frequency.set(value, (frequency.get(value) || 0) + 1)
    })
    
    // 转换为数组并排序
    const distribution = Array.from(frequency.entries())
      .map(([value, count]) => ({ value, count, percentage: (count / data.length) * 100 }))
      .sort((a, b) => b.count - a.count)
    
    // 创建直方图数据
    const histogram = this.createHistogram(data, 10)
    
    return {
      distribution,
      histogram,
      uniqueValues: frequency.size,
      mostFrequent: distribution[0],
      entropy: this.calculateEntropy(distribution)
    }
  }

  // 基本指标计算
  private calculateBasicMetrics(data: number[]): any {
    const stats = this.calculateStatistics(data)
    const trend = data.length > 1 ? this.analyzeTrend(data) : null
    
    return {
      statistics: stats,
      trend,
      dataQuality: this.assessDataQuality(data)
    }
  }

  // 生成报告
  async generateReport(config: ReportConfig): Promise<ReportData> {
    try {
      const { dataSource, analysisTypes, format, includeCharts } = config
      
      // 获取数据
      const data = await this.fetchReportData(dataSource)
      
      // 执行多种分析
      const analyses: any = {}
      for (const analysisType of analysisTypes) {
        const result = await this.analyzeData({ data, analysisType })
        if (result.success) {
          analyses[analysisType] = result.result
        }
      }
      
      // 生成图表数据
      const charts = includeCharts ? this.generateChartData(data, analyses) : []
      
      // 生成摘要
      const summary = this.generateSummary(analyses)
      
      return {
        success: true,
        config,
        data: {
          rawData: data,
          analyses,
          charts,
          summary,
          metadata: {
            generatedAt: new Date().toISOString(),
            dataPoints: data.length,
            analysisCount: Object.keys(analyses).length
          }
        }
      }
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : '报告生成失败',
        config
      }
    }
  }

  // 辅助方法
  private percentile(sortedData: number[], p: number): number {
    const index = (p / 100) * (sortedData.length - 1)
    const lower = Math.floor(index)
    const upper = Math.ceil(index)
    const weight = index - lower
    
    if (lower === upper) {
      return sortedData[lower]
    }
    
    return sortedData[lower] * (1 - weight) + sortedData[upper] * weight
  }

  private calculateMode(data: number[]): number[] {
    const frequency = new Map<number, number>()
    data.forEach(value => {
      frequency.set(value, (frequency.get(value) || 0) + 1)
    })
    
    const maxFreq = Math.max(...frequency.values())
    return Array.from(frequency.entries())
      .filter(([_, freq]) => freq === maxFreq)
      .map(([value, _]) => value)
  }

  private calculateSkewness(data: number[], mean: number, stdDev: number): number {
    const n = data.length
    const sum = data.reduce((acc, val) => acc + Math.pow((val - mean) / stdDev, 3), 0)
    return (n / ((n - 1) * (n - 2))) * sum
  }

  private calculateKurtosis(data: number[], mean: number, stdDev: number): number {
    const n = data.length
    const sum = data.reduce((acc, val) => acc + Math.pow((val - mean) / stdDev, 4), 0)
    return ((n * (n + 1)) / ((n - 1) * (n - 2) * (n - 3))) * sum - (3 * Math.pow(n - 1, 2)) / ((n - 2) * (n - 3))
  }

  private generatePrediction(slope: number, intercept: number, currentLength: number): number[] {
    const predictions = []
    for (let i = currentLength; i < currentLength + 5; i++) {
      predictions.push(slope * i + intercept)
    }
    return predictions
  }

  private createHistogram(data: number[], bins: number): any[] {
    const min = Math.min(...data)
    const max = Math.max(...data)
    const binWidth = (max - min) / bins
    
    const histogram = Array.from({ length: bins }, (_, i) => ({
      bin: i,
      range: [min + i * binWidth, min + (i + 1) * binWidth],
      count: 0,
      percentage: 0
    }))
    
    data.forEach(value => {
      const binIndex = Math.min(Math.floor((value - min) / binWidth), bins - 1)
      histogram[binIndex].count++
    })
    
    histogram.forEach(bin => {
      bin.percentage = (bin.count / data.length) * 100
    })
    
    return histogram
  }

  private calculateEntropy(distribution: any[]): number {
    return -distribution.reduce((entropy, item) => {
      const p = item.percentage / 100
      return entropy + (p > 0 ? p * Math.log2(p) : 0)
    }, 0)
  }

  private assessDataQuality(data: number[]): any {
    const nullCount = data.filter(val => val === null || val === undefined || isNaN(val)).length
    const duplicateCount = data.length - new Set(data).size
    
    return {
      completeness: ((data.length - nullCount) / data.length) * 100,
      uniqueness: ((data.length - duplicateCount) / data.length) * 100,
      validity: 100, // 简化实现
      consistency: 100 // 简化实现
    }
  }

  private async fetchReportData(dataSource: string): Promise<number[]> {
    // 这里应该根据dataSource获取实际数据
    // 暂时返回模拟数据
    return Array.from({ length: 100 }, () => Math.random() * 100)
  }

  private generateChartData(data: number[], analyses: any): any[] {
    const charts = []
    
    // 时间序列图
    charts.push({
      type: 'line',
      title: '数据趋势',
      data: data.map((value, index) => ({ x: index, y: value }))
    })
    
    // 直方图
    if (analyses.frequency) {
      charts.push({
        type: 'histogram',
        title: '数据分布',
        data: analyses.frequency.histogram
      })
    }
    
    // 箱线图数据
    if (analyses.statistical) {
      const stats = analyses.statistical
      charts.push({
        type: 'boxplot',
        title: '统计摘要',
        data: {
          min: stats.min,
          q1: stats.q1,
          median: stats.median,
          q3: stats.q3,
          max: stats.max
        }
      })
    }
    
    return charts
  }

  private generateSummary(analyses: any): string {
    const summaryParts = []
    
    if (analyses.statistical) {
      const stats = analyses.statistical
      summaryParts.push(`数据包含${stats.count}个观测值，平均值为${stats.mean.toFixed(2)}，标准差为${stats.standardDeviation.toFixed(2)}。`)
    }
    
    if (analyses.trend) {
      const trend = analyses.trend
      const direction = trend.direction === 'increasing' ? '上升' : 
                      trend.direction === 'decreasing' ? '下降' : '稳定'
      summaryParts.push(`数据呈现${direction}趋势，变化率为${trend.changeRate.toFixed(2)}%。`)
    }
    
    if (analyses.anomaly) {
      const anomaly = analyses.anomaly
      summaryParts.push(`检测到${anomaly.anomalies.length}个异常值，异常率为${anomaly.anomalyRate.toFixed(2)}%。`)
    }
    
    return summaryParts.join(' ')
  }

  // 导出数据
  exportData(data: any, format: 'csv' | 'json' | 'excel'): string | Blob {
    switch (format) {
      case 'csv':
        return this.exportToCSV(data)
      case 'json':
        return JSON.stringify(data, null, 2)
      case 'excel':
        return this.exportToExcel(data)
      default:
        throw new Error('不支持的导出格式')
    }
  }

  private exportToCSV(data: any): string {
    // 简化的CSV导出实现
    if (Array.isArray(data)) {
      const headers = Object.keys(data[0] || {})
      const csvContent = [
        headers.join(','),
        ...data.map(row => headers.map(header => row[header]).join(','))
      ].join('\n')
      return csvContent
    }
    return ''
  }

  private exportToExcel(data: any): Blob {
    // 这里应该使用专门的Excel库，暂时返回CSV格式的Blob
    const csvContent = this.exportToCSV(data)
    return new Blob([csvContent], { type: 'application/vnd.ms-excel' })
  }
}

// 导出服务实例
export const dataAnalysisService = new DataAnalysisService()
