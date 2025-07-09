/**
 * 数学分析工具库
 * 提供统计分析、数据拟合、滤波器等高级数学功能
 */

// 统计分析结果接口
export interface StatisticsResult {
  mean: number           // 平均值
  median: number         // 中位数
  mode: number[]         // 众数
  variance: number       // 方差
  standardDeviation: number  // 标准差
  skewness: number       // 偏度
  kurtosis: number       // 峰度
  rms: number           // 均方根值
  min: number           // 最小值
  max: number           // 最大值
  range: number         // 极差
  q1: number            // 第一四分位数
  q3: number            // 第三四分位数
  iqr: number           // 四分位距
  outliers: number[]    // 异常值
}

// 拟合结果接口
export interface FittingResult {
  coefficients: number[]     // 拟合系数
  rSquared: number          // 决定系数 R²
  residuals: number[]       // 残差
  fittedValues: number[]    // 拟合值
  equation: string          // 拟合方程字符串
  rmse: number             // 均方根误差
  mae: number              // 平均绝对误差
}

// 滤波器类型
export type FilterType = 'lowpass' | 'highpass' | 'bandpass' | 'bandstop'

// 滤波器参数接口
export interface FilterOptions {
  type: FilterType
  cutoffFrequency: number | [number, number]  // 截止频率
  sampleRate: number                          // 采样率
  order: number                              // 滤波器阶数
  ripple?: number                            // 通带纹波 (dB)
  stopbandAttenuation?: number               // 阻带衰减 (dB)
}

// 滤波器设计结果
export interface FilterDesign {
  numerator: number[]    // 分子系数
  denominator: number[]  // 分母系数
  frequencyResponse: {
    frequencies: number[]
    magnitude: number[]
    phase: number[]
  }
}

/**
 * 数学分析器类
 */
export class MathAnalyzer {
  
  /**
   * 计算完整的统计分析
   */
  static calculateStatistics(data: number[]): StatisticsResult {
    if (data.length === 0) {
      throw new Error('数据数组不能为空')
    }

    const sortedData = [...data].sort((a, b) => a - b)
    const n = data.length
    
    // 基本统计量
    const mean = this.calculateMean(data)
    const median = this.calculateMedian(sortedData)
    const mode = this.calculateMode(data)
    const variance = this.calculateVariance(data, mean)
    const standardDeviation = Math.sqrt(variance)
    const rms = this.calculateRMS(data)
    const min = sortedData[0]
    const max = sortedData[n - 1]
    const range = max - min
    
    // 四分位数
    const q1 = this.calculatePercentile(sortedData, 25)
    const q3 = this.calculatePercentile(sortedData, 75)
    const iqr = q3 - q1
    
    // 高阶统计量
    const skewness = this.calculateSkewness(data, mean, standardDeviation)
    const kurtosis = this.calculateKurtosis(data, mean, standardDeviation)
    
    // 异常值检测 (使用IQR方法)
    const outliers = this.detectOutliers(data, q1, q3, iqr)
    
    return {
      mean,
      median,
      mode,
      variance,
      standardDeviation,
      skewness,
      kurtosis,
      rms,
      min,
      max,
      range,
      q1,
      q3,
      iqr,
      outliers
    }
  }

  /**
   * 计算平均值
   */
  static calculateMean(data: number[]): number {
    return data.reduce((sum, value) => sum + value, 0) / data.length
  }

  /**
   * 计算中位数
   */
  static calculateMedian(sortedData: number[]): number {
    const n = sortedData.length
    if (n % 2 === 0) {
      return (sortedData[n / 2 - 1] + sortedData[n / 2]) / 2
    } else {
      return sortedData[Math.floor(n / 2)]
    }
  }

  /**
   * 计算众数
   */
  static calculateMode(data: number[]): number[] {
    const frequency = new Map<number, number>()
    
    // 统计频率
    data.forEach(value => {
      frequency.set(value, (frequency.get(value) || 0) + 1)
    })
    
    // 找到最高频率
    const maxFreq = Math.max(...frequency.values())
    
    // 返回所有最高频率的值
    return Array.from(frequency.entries())
      .filter(([_, freq]) => freq === maxFreq)
      .map(([value, _]) => value)
  }

  /**
   * 计算方差
   */
  static calculateVariance(data: number[], mean?: number): number {
    const m = mean ?? this.calculateMean(data)
    const sumSquaredDiff = data.reduce((sum, value) => sum + Math.pow(value - m, 2), 0)
    return sumSquaredDiff / (data.length - 1) // 样本方差
  }

  /**
   * 计算均方根值
   */
  static calculateRMS(data: number[]): number {
    const sumSquares = data.reduce((sum, value) => sum + value * value, 0)
    return Math.sqrt(sumSquares / data.length)
  }

  /**
   * 计算百分位数
   */
  static calculatePercentile(sortedData: number[], percentile: number): number {
    const index = (percentile / 100) * (sortedData.length - 1)
    const lower = Math.floor(index)
    const upper = Math.ceil(index)
    
    if (lower === upper) {
      return sortedData[lower]
    }
    
    const weight = index - lower
    return sortedData[lower] * (1 - weight) + sortedData[upper] * weight
  }

  /**
   * 计算偏度
   */
  static calculateSkewness(data: number[], mean: number, stdDev: number): number {
    if (stdDev === 0) return 0
    
    const n = data.length
    const sumCubedDeviations = data.reduce((sum, value) => {
      return sum + Math.pow((value - mean) / stdDev, 3)
    }, 0)
    
    return (n / ((n - 1) * (n - 2))) * sumCubedDeviations
  }

  /**
   * 计算峰度
   */
  static calculateKurtosis(data: number[], mean: number, stdDev: number): number {
    if (stdDev === 0) return 0
    
    const n = data.length
    const sumFourthPowers = data.reduce((sum, value) => {
      return sum + Math.pow((value - mean) / stdDev, 4)
    }, 0)
    
    const kurtosis = (n * (n + 1) / ((n - 1) * (n - 2) * (n - 3))) * sumFourthPowers
    const correction = 3 * (n - 1) * (n - 1) / ((n - 2) * (n - 3))
    
    return kurtosis - correction // 超额峰度
  }

  /**
   * 异常值检测
   */
  static detectOutliers(data: number[], q1: number, q3: number, iqr: number): number[] {
    const lowerBound = q1 - 1.5 * iqr
    const upperBound = q3 + 1.5 * iqr
    
    return data.filter(value => value < lowerBound || value > upperBound)
  }

  /**
   * 线性拟合 (最小二乘法)
   */
  static linearFit(xData: number[], yData: number[]): FittingResult {
    if (xData.length !== yData.length) {
      throw new Error('X和Y数据长度必须相同')
    }

    const n = xData.length
    const sumX = xData.reduce((sum, x) => sum + x, 0)
    const sumY = yData.reduce((sum, y) => sum + y, 0)
    const sumXY = xData.reduce((sum, x, i) => sum + x * yData[i], 0)
    const sumXX = xData.reduce((sum, x) => sum + x * x, 0)
    const sumYY = yData.reduce((sum, y) => sum + y * y, 0)

    // 计算斜率和截距
    const slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX)
    const intercept = (sumY - slope * sumX) / n

    // 计算拟合值和残差
    const fittedValues = xData.map(x => slope * x + intercept)
    const residuals = yData.map((y, i) => y - fittedValues[i])

    // 计算R²
    const meanY = sumY / n
    const ssTotal = yData.reduce((sum, y) => sum + Math.pow(y - meanY, 2), 0)
    const ssResidual = residuals.reduce((sum, r) => sum + r * r, 0)
    const rSquared = 1 - (ssResidual / ssTotal)

    // 计算误差指标
    const rmse = Math.sqrt(ssResidual / n)
    const mae = residuals.reduce((sum, r) => sum + Math.abs(r), 0) / n

    return {
      coefficients: [intercept, slope],
      rSquared,
      residuals,
      fittedValues,
      equation: `y = ${slope.toFixed(4)}x + ${intercept.toFixed(4)}`,
      rmse,
      mae
    }
  }

  /**
   * 多项式拟合
   */
  static polynomialFit(xData: number[], yData: number[], degree: number): FittingResult {
    if (xData.length !== yData.length) {
      throw new Error('X和Y数据长度必须相同')
    }

    const n = xData.length
    if (degree >= n) {
      throw new Error('多项式阶数不能大于等于数据点数')
    }

    // 构建范德蒙德矩阵
    const matrix: number[][] = []
    for (let i = 0; i < n; i++) {
      const row: number[] = []
      for (let j = 0; j <= degree; j++) {
        row.push(Math.pow(xData[i], j))
      }
      matrix.push(row)
    }

    // 使用最小二乘法求解系数
    const coefficients = this.leastSquares(matrix, yData)

    // 计算拟合值和残差
    const fittedValues = xData.map(x => {
      return coefficients.reduce((sum, coeff, power) => sum + coeff * Math.pow(x, power), 0)
    })
    const residuals = yData.map((y, i) => y - fittedValues[i])

    // 计算R²
    const meanY = yData.reduce((sum, y) => sum + y, 0) / n
    const ssTotal = yData.reduce((sum, y) => sum + Math.pow(y - meanY, 2), 0)
    const ssResidual = residuals.reduce((sum, r) => sum + r * r, 0)
    const rSquared = 1 - (ssResidual / ssTotal)

    // 计算误差指标
    const rmse = Math.sqrt(ssResidual / n)
    const mae = residuals.reduce((sum, r) => sum + Math.abs(r), 0) / n

    // 生成方程字符串
    const equation = this.generatePolynomialEquation(coefficients)

    return {
      coefficients,
      rSquared,
      residuals,
      fittedValues,
      equation,
      rmse,
      mae
    }
  }

  /**
   * 最小二乘法求解线性方程组
   */
  private static leastSquares(A: number[][], b: number[]): number[] {
    // 计算 A^T * A 和 A^T * b
    const AT = this.transpose(A)
    const ATA = this.matrixMultiply(AT, A)
    const ATb = this.matrixVectorMultiply(AT, b)

    // 求解 ATA * x = ATb
    return this.gaussianElimination(ATA, ATb)
  }

  /**
   * 矩阵转置
   */
  private static transpose(matrix: number[][]): number[][] {
    const rows = matrix.length
    const cols = matrix[0].length
    const result: number[][] = []

    for (let j = 0; j < cols; j++) {
      const row: number[] = []
      for (let i = 0; i < rows; i++) {
        row.push(matrix[i][j])
      }
      result.push(row)
    }

    return result
  }

  /**
   * 矩阵乘法
   */
  private static matrixMultiply(A: number[][], B: number[][]): number[][] {
    const rowsA = A.length
    const colsA = A[0].length
    const colsB = B[0].length
    const result: number[][] = []

    for (let i = 0; i < rowsA; i++) {
      const row: number[] = []
      for (let j = 0; j < colsB; j++) {
        let sum = 0
        for (let k = 0; k < colsA; k++) {
          sum += A[i][k] * B[k][j]
        }
        row.push(sum)
      }
      result.push(row)
    }

    return result
  }

  /**
   * 矩阵向量乘法
   */
  private static matrixVectorMultiply(A: number[][], b: number[]): number[] {
    const rows = A.length
    const result: number[] = []

    for (let i = 0; i < rows; i++) {
      let sum = 0
      for (let j = 0; j < A[i].length; j++) {
        sum += A[i][j] * b[j]
      }
      result.push(sum)
    }

    return result
  }

  /**
   * 高斯消元法求解线性方程组
   */
  private static gaussianElimination(A: number[][], b: number[]): number[] {
    const n = A.length
    const augmented: number[][] = []

    // 构建增广矩阵
    for (let i = 0; i < n; i++) {
      augmented.push([...A[i], b[i]])
    }

    // 前向消元
    for (let i = 0; i < n; i++) {
      // 选择主元
      let maxRow = i
      for (let k = i + 1; k < n; k++) {
        if (Math.abs(augmented[k][i]) > Math.abs(augmented[maxRow][i])) {
          maxRow = k
        }
      }

      // 交换行
      if (maxRow !== i) {
        [augmented[i], augmented[maxRow]] = [augmented[maxRow], augmented[i]]
      }

      // 消元
      for (let k = i + 1; k < n; k++) {
        const factor = augmented[k][i] / augmented[i][i]
        for (let j = i; j <= n; j++) {
          augmented[k][j] -= factor * augmented[i][j]
        }
      }
    }

    // 回代求解
    const x: number[] = new Array(n)
    for (let i = n - 1; i >= 0; i--) {
      x[i] = augmented[i][n]
      for (let j = i + 1; j < n; j++) {
        x[i] -= augmented[i][j] * x[j]
      }
      x[i] /= augmented[i][i]
    }

    return x
  }

  /**
   * 生成多项式方程字符串
   */
  private static generatePolynomialEquation(coefficients: number[]): string {
    const terms: string[] = []
    
    for (let i = coefficients.length - 1; i >= 0; i--) {
      const coeff = coefficients[i]
      if (Math.abs(coeff) < 1e-10) continue // 忽略接近零的系数
      
      let term = ''
      const absCoeff = Math.abs(coeff)
      const sign = coeff >= 0 ? '+' : '-'
      
      if (i === 0) {
        // 常数项
        term = `${sign} ${absCoeff.toFixed(4)}`
      } else if (i === 1) {
        // 一次项
        if (absCoeff === 1) {
          term = `${sign} x`
        } else {
          term = `${sign} ${absCoeff.toFixed(4)}x`
        }
      } else {
        // 高次项
        if (absCoeff === 1) {
          term = `${sign} x^${i}`
        } else {
          term = `${sign} ${absCoeff.toFixed(4)}x^${i}`
        }
      }
      
      terms.push(term)
    }
    
    let equation = terms.join(' ')
    
    // 处理首项的符号
    if (equation.startsWith('+ ')) {
      equation = equation.substring(2)
    } else if (equation.startsWith('- ')) {
      equation = '-' + equation.substring(2)
    }
    
    return `y = ${equation}`
  }

  /**
   * 移动平均滤波器
   */
  static movingAverageFilter(data: number[], windowSize: number): number[] {
    if (windowSize <= 0 || windowSize > data.length) {
      throw new Error('窗口大小必须大于0且不超过数据长度')
    }

    const result: number[] = []
    const halfWindow = Math.floor(windowSize / 2)

    for (let i = 0; i < data.length; i++) {
      const start = Math.max(0, i - halfWindow)
      const end = Math.min(data.length - 1, i + halfWindow)
      
      let sum = 0
      let count = 0
      for (let j = start; j <= end; j++) {
        sum += data[j]
        count++
      }
      
      result.push(sum / count)
    }

    return result
  }

  /**
   * 中值滤波器
   */
  static medianFilter(data: number[], windowSize: number): number[] {
    if (windowSize <= 0 || windowSize > data.length) {
      throw new Error('窗口大小必须大于0且不超过数据长度')
    }

    const result: number[] = []
    const halfWindow = Math.floor(windowSize / 2)

    for (let i = 0; i < data.length; i++) {
      const start = Math.max(0, i - halfWindow)
      const end = Math.min(data.length - 1, i + halfWindow)
      
      const window: number[] = []
      for (let j = start; j <= end; j++) {
        window.push(data[j])
      }
      
      window.sort((a, b) => a - b)
      result.push(this.calculateMedian(window))
    }

    return result
  }

  /**
   * 高斯滤波器
   */
  static gaussianFilter(data: number[], sigma: number): number[] {
    // 计算高斯核
    const kernelSize = Math.ceil(6 * sigma) | 1 // 确保是奇数
    const kernel: number[] = []
    const center = Math.floor(kernelSize / 2)
    
    let sum = 0
    for (let i = 0; i < kernelSize; i++) {
      const x = i - center
      const value = Math.exp(-(x * x) / (2 * sigma * sigma))
      kernel.push(value)
      sum += value
    }
    
    // 归一化核
    for (let i = 0; i < kernelSize; i++) {
      kernel[i] /= sum
    }

    // 应用卷积
    const result: number[] = []
    for (let i = 0; i < data.length; i++) {
      let value = 0
      for (let j = 0; j < kernelSize; j++) {
        const dataIndex = i + j - center
        if (dataIndex >= 0 && dataIndex < data.length) {
          value += data[dataIndex] * kernel[j]
        }
      }
      result.push(value)
    }

    return result
  }

  /**
   * 简单的低通滤波器 (一阶RC滤波器)
   */
  static lowPassFilter(data: number[], cutoffFreq: number, sampleRate: number): number[] {
    const dt = 1 / sampleRate
    const rc = 1 / (2 * Math.PI * cutoffFreq)
    const alpha = dt / (rc + dt)

    const result: number[] = []
    let previousOutput = data[0]

    for (let i = 0; i < data.length; i++) {
      const output = alpha * data[i] + (1 - alpha) * previousOutput
      result.push(output)
      previousOutput = output
    }

    return result
  }

  /**
   * 简单的高通滤波器
   */
  static highPassFilter(data: number[], cutoffFreq: number, sampleRate: number): number[] {
    const dt = 1 / sampleRate
    const rc = 1 / (2 * Math.PI * cutoffFreq)
    const alpha = rc / (rc + dt)

    const result: number[] = []
    let previousInput = data[0]
    let previousOutput = 0

    for (let i = 0; i < data.length; i++) {
      const output = alpha * (previousOutput + data[i] - previousInput)
      result.push(output)
      previousInput = data[i]
      previousOutput = output
    }

    return result
  }

  /**
   * 数据平滑 (Savitzky-Golay滤波器的简化版本)
   */
  static smoothData(data: number[], windowSize: number = 5): number[] {
    if (windowSize % 2 === 0) {
      windowSize += 1 // 确保窗口大小为奇数
    }

    const result: number[] = []
    const halfWindow = Math.floor(windowSize / 2)

    for (let i = 0; i < data.length; i++) {
      const start = Math.max(0, i - halfWindow)
      const end = Math.min(data.length - 1, i + halfWindow)
      
      // 简单的加权平均，中心权重更大
      let sum = 0
      let weightSum = 0
      
      for (let j = start; j <= end; j++) {
        const distance = Math.abs(j - i)
        const weight = Math.exp(-distance * distance / (2 * (halfWindow / 2) * (halfWindow / 2)))
        sum += data[j] * weight
        weightSum += weight
      }
      
      result.push(sum / weightSum)
    }

    return result
  }

  /**
   * 计算数据的导数 (数值微分)
   */
  static calculateDerivative(data: number[], dx: number = 1): number[] {
    const result: number[] = []
    
    // 前向差分 (第一个点)
    result.push((data[1] - data[0]) / dx)
    
    // 中心差分 (中间点)
    for (let i = 1; i < data.length - 1; i++) {
      result.push((data[i + 1] - data[i - 1]) / (2 * dx))
    }
    
    // 后向差分 (最后一个点)
    result.push((data[data.length - 1] - data[data.length - 2]) / dx)
    
    return result
  }

  /**
   * 计算数据的积分 (数值积分 - 梯形法则)
   */
  static calculateIntegral(data: number[], dx: number = 1): number[] {
    const result: number[] = []
    let sum = 0
    
    result.push(0) // 初始积分值为0
    
    for (let i = 1; i < data.length; i++) {
      sum += (data[i] + data[i - 1]) * dx / 2 // 梯形法则
      result.push(sum)
    }
    
    return result
  }
}
