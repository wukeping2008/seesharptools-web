// 数据分析相关类型定义

// 分析请求
export interface AnalysisRequest {
  data: number[]
  analysisType: 'statistical' | 'trend' | 'anomaly' | 'frequency' | 'basic'
  options?: AnalysisOptions
}

// 分析选项
export interface AnalysisOptions {
  confidence?: number
  threshold?: number
  windowSize?: number
  method?: string
}

// 分析结果
export interface AnalysisResult {
  success: boolean
  analysisType: string
  result?: any
  error?: string
  timestamp: string
  source?: string
}

// 统计指标
export interface StatisticalMetrics {
  count: number
  sum: number
  mean: number
  median: number
  mode: number[]
  variance: number
  standardDeviation: number
  min: number
  max: number
  range: number
  q1: number
  q3: number
  iqr: number
  skewness: number
  kurtosis: number
}

// 趋势分析
export interface TrendAnalysis {
  slope: number
  intercept: number
  rSquared: number
  direction: 'increasing' | 'decreasing' | 'stable'
  changeRate: number
  confidence: 'high' | 'medium' | 'low'
  prediction: number[]
}

// 异常检测
export interface AnomalyDetection {
  anomalies: Array<{
    index: number
    value: number
    type: string
    severity: string
  }>
  anomalyRate: number
  method: string
  thresholds: {
    iqr?: { lower: number; upper: number }
    zscore?: number
  }
}

// 报告配置
export interface ReportConfig {
  title?: string
  dataSource: string
  analysisTypes: string[]
  format: 'pdf' | 'html' | 'json'
  includeCharts: boolean
  dateRange?: {
    start: string
    end: string
  }
  filters?: Record<string, any>
}

// 报告数据
export interface ReportData {
  success: boolean
  config: ReportConfig
  data?: {
    rawData: number[]
    analyses: Record<string, any>
    charts: ChartData[]
    summary: string
    metadata: ReportMetadata
  }
  error?: string
}

// 报告元数据
export interface ReportMetadata {
  generatedAt: string
  dataPoints: number
  analysisCount: number
  version?: string
  author?: string
}

// 图表数据
export interface ChartData {
  type: 'line' | 'bar' | 'histogram' | 'boxplot' | 'scatter'
  title: string
  data: any
  options?: ChartOptions
}

// 图表选项
export interface ChartOptions {
  width?: number
  height?: number
  colors?: string[]
  showLegend?: boolean
  showGrid?: boolean
  xAxis?: AxisConfig
  yAxis?: AxisConfig
}

// 坐标轴配置
export interface AxisConfig {
  title?: string
  min?: number
  max?: number
  format?: string
}

// 数据质量评估
export interface DataQuality {
  completeness: number
  uniqueness: number
  validity: number
  consistency: number
  accuracy?: number
  timeliness?: number
}

// 频率分析结果
export interface FrequencyAnalysis {
  distribution: Array<{
    value: number
    count: number
    percentage: number
  }>
  histogram: Array<{
    bin: number
    range: [number, number]
    count: number
    percentage: number
  }>
  uniqueValues: number
  mostFrequent: {
    value: number
    count: number
    percentage: number
  }
  entropy: number
}

// 相关性分析
export interface CorrelationAnalysis {
  correlationMatrix: number[][]
  significantCorrelations: Array<{
    variable1: string
    variable2: string
    correlation: number
    pValue: number
    significance: 'high' | 'medium' | 'low'
  }>
  method: 'pearson' | 'spearman' | 'kendall'
}

// 时间序列分析
export interface TimeSeriesAnalysis {
  trend: TrendAnalysis
  seasonality: {
    detected: boolean
    period?: number
    strength?: number
  }
  stationarity: {
    isStationary: boolean
    pValue: number
    testStatistic: number
  }
  forecast: {
    values: number[]
    confidence: Array<{
      lower: number
      upper: number
    }>
    horizon: number
  }
}

// 聚类分析
export interface ClusterAnalysis {
  clusters: Array<{
    id: number
    center: number[]
    members: number[]
    size: number
  }>
  method: 'kmeans' | 'hierarchical' | 'dbscan'
  optimalClusters: number
  silhouetteScore: number
  inertia?: number
}

// 回归分析
export interface RegressionAnalysis {
  coefficients: number[]
  intercept: number
  rSquared: number
  adjustedRSquared: number
  pValues: number[]
  standardErrors: number[]
  residuals: number[]
  predictions: number[]
  diagnostics: {
    normality: boolean
    homoscedasticity: boolean
    independence: boolean
  }
}

// 假设检验
export interface HypothesisTest {
  testType: string
  statistic: number
  pValue: number
  criticalValue: number
  rejected: boolean
  confidence: number
  effect: {
    size: number
    interpretation: string
  }
}

// 数据变换
export interface DataTransformation {
  method: 'log' | 'sqrt' | 'boxcox' | 'standardize' | 'normalize'
  parameters?: Record<string, number>
  transformedData: number[]
  inverseTransform?: (data: number[]) => number[]
}

// 特征工程
export interface FeatureEngineering {
  features: Array<{
    name: string
    type: 'numerical' | 'categorical' | 'derived'
    importance?: number
    correlation?: number
  }>
  selectedFeatures: string[]
  transformations: DataTransformation[]
  encodings?: Record<string, any>
}

// 模型评估
export interface ModelEvaluation {
  metrics: {
    accuracy?: number
    precision?: number
    recall?: number
    f1Score?: number
    mse?: number
    rmse?: number
    mae?: number
    r2?: number
  }
  crossValidation?: {
    folds: number
    scores: number[]
    mean: number
    std: number
  }
  confusionMatrix?: number[][]
  rocCurve?: Array<{ fpr: number; tpr: number }>
  featureImportance?: Array<{ feature: string; importance: number }>
}

// 数据可视化配置
export interface VisualizationConfig {
  type: 'static' | 'interactive' | 'animated'
  theme: 'light' | 'dark' | 'custom'
  responsive: boolean
  export: {
    formats: ('png' | 'svg' | 'pdf')[]
    quality: 'low' | 'medium' | 'high'
  }
  accessibility: {
    colorBlind: boolean
    highContrast: boolean
    screenReader: boolean
  }
}

// 实时分析配置
export interface RealTimeAnalysisConfig {
  enabled: boolean
  interval: number // 毫秒
  bufferSize: number
  triggers: Array<{
    condition: string
    action: string
    threshold: number
  }>
  alerts: {
    email?: string[]
    webhook?: string
    dashboard?: boolean
  }
}

// 分析管道
export interface AnalysisPipeline {
  id: string
  name: string
  description?: string
  steps: Array<{
    id: string
    type: string
    config: Record<string, any>
    dependencies?: string[]
  }>
  schedule?: {
    enabled: boolean
    cron: string
    timezone: string
  }
  status: 'idle' | 'running' | 'completed' | 'failed'
  lastRun?: string
  nextRun?: string
}
