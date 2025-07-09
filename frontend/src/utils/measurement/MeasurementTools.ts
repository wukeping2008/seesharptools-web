/**
 * 专业测量工具库
 * 提供游标测量、峰值检测、频率分析、相位测量等功能
 */

// 游标测量结果接口
export interface CursorMeasurement {
  x1: number
  y1: number
  x2: number
  y2: number
  deltaX: number
  deltaY: number
  frequency?: number
  period?: number
  slope?: number
}

// 峰值检测结果接口
export interface PeakDetectionResult {
  peaks: Array<{
    index: number
    value: number
    x: number
    prominence: number
    width: number
  }>
  valleys: Array<{
    index: number
    value: number
    x: number
    prominence: number
    width: number
  }>
  statistics: {
    peakCount: number
    valleyCount: number
    averagePeakValue: number
    averageValleyValue: number
    peakToPeakAmplitude: number
  }
}

// 频率分析结果接口
export interface FrequencyAnalysisResult {
  fundamentalFrequency: number
  dominantFrequency: number
  harmonics: Array<{
    frequency: number
    amplitude: number
    phase: number
    harmonic: number
  }>
  thd: number // 总谐波失真
  snr: number // 信噪比
  bandwidth: number
}

// 相位测量结果接口
export interface PhaseMeasurementResult {
  phase: number // 相位差（度）
  phaseRad: number // 相位差（弧度）
  delay: number // 时间延迟
  correlation: number // 相关系数
  coherence: number // 相干性
}

// 自动测量结果接口
export interface AutoMeasurementResult {
  frequency: number
  period: number
  amplitude: number
  rms: number
  mean: number
  min: number
  max: number
  peakToPeak: number
  dutyCycle: number
  riseTime: number
  fallTime: number
  pulseWidth: number
}

/**
 * 专业测量工具类
 */
export class MeasurementTools {
  
  /**
   * 游标测量
   * @param data 数据数组
   * @param x1 游标1的X坐标
   * @param x2 游标2的X坐标
   * @param xStart X轴起始值
   * @param xInterval X轴间隔
   * @returns 游标测量结果
   */
  static cursorMeasurement(
    data: number[], 
    x1: number, 
    x2: number, 
    xStart: number = 0, 
    xInterval: number = 1
  ): CursorMeasurement {
    // 将X坐标转换为数组索引
    const index1 = Math.round((x1 - xStart) / xInterval)
    const index2 = Math.round((x2 - xStart) / xInterval)
    
    // 确保索引在有效范围内
    const i1 = Math.max(0, Math.min(data.length - 1, index1))
    const i2 = Math.max(0, Math.min(data.length - 1, index2))
    
    const y1 = data[i1]
    const y2 = data[i2]
    
    const deltaX = x2 - x1
    const deltaY = y2 - y1
    
    const result: CursorMeasurement = {
      x1,
      y1,
      x2,
      y2,
      deltaX,
      deltaY
    }
    
    // 计算频率和周期（如果ΔX > 0）
    if (Math.abs(deltaX) > 0) {
      result.period = Math.abs(deltaX)
      result.frequency = 1 / result.period
    }
    
    // 计算斜率
    if (Math.abs(deltaX) > 1e-10) {
      result.slope = deltaY / deltaX
    }
    
    return result
  }
  
  /**
   * 峰值检测
   * @param data 数据数组
   * @param minHeight 最小峰值高度
   * @param minDistance 最小峰值距离
   * @param prominence 最小突出度
   * @returns 峰值检测结果
   */
  static peakDetection(
    data: number[], 
    minHeight: number = 0, 
    minDistance: number = 1,
    prominence: number = 0
  ): PeakDetectionResult {
    const peaks: Array<{index: number, value: number, x: number, prominence: number, width: number}> = []
    const valleys: Array<{index: number, value: number, x: number, prominence: number, width: number}> = []
    
    // 寻找局部极大值（峰值）
    for (let i = 1; i < data.length - 1; i++) {
      if (data[i] > data[i - 1] && data[i] > data[i + 1] && data[i] >= minHeight) {
        // 检查最小距离约束
        if (peaks.length === 0 || i - peaks[peaks.length - 1].index >= minDistance) {
          // 计算突出度
          const peakProminence = this.calculateProminence(data, i)
          if (peakProminence >= prominence) {
            // 计算峰宽
            const width = this.calculatePeakWidth(data, i)
            peaks.push({
              index: i,
              value: data[i],
              x: i,
              prominence: peakProminence,
              width
            })
          }
        }
      }
    }
    
    // 寻找局部极小值（谷值）
    for (let i = 1; i < data.length - 1; i++) {
      if (data[i] < data[i - 1] && data[i] < data[i + 1]) {
        // 检查最小距离约束
        if (valleys.length === 0 || i - valleys[valleys.length - 1].index >= minDistance) {
          // 计算突出度
          const valleyProminence = this.calculateProminence(data, i, false)
          const width = this.calculatePeakWidth(data, i, false)
          valleys.push({
            index: i,
            value: data[i],
            x: i,
            prominence: valleyProminence,
            width
          })
        }
      }
    }
    
    // 计算统计信息
    const peakValues = peaks.map(p => p.value)
    const valleyValues = valleys.map(v => v.value)
    
    const statistics = {
      peakCount: peaks.length,
      valleyCount: valleys.length,
      averagePeakValue: peakValues.length > 0 ? peakValues.reduce((a, b) => a + b, 0) / peakValues.length : 0,
      averageValleyValue: valleyValues.length > 0 ? valleyValues.reduce((a, b) => a + b, 0) / valleyValues.length : 0,
      peakToPeakAmplitude: peakValues.length > 0 && valleyValues.length > 0 ? 
        Math.max(...peakValues) - Math.min(...valleyValues) : 0
    }
    
    return {
      peaks,
      valleys,
      statistics
    }
  }
  
  /**
   * 频率分析
   * @param data 数据数组
   * @param sampleRate 采样率
   * @param windowType 窗函数类型
   * @returns 频率分析结果
   */
  static frequencyAnalysis(
    data: number[], 
    sampleRate: number = 1000,
    windowType: 'hanning' | 'hamming' | 'blackman' = 'hanning'
  ): FrequencyAnalysisResult {
    // 应用窗函数
    const windowedData = this.applyWindow(data, windowType)
    
    // 执行FFT
    const fftResult = this.fft(windowedData)
    
    // 计算功率谱
    const powerSpectrum = fftResult.map(complex => 
      Math.sqrt(complex.real * complex.real + complex.imag * complex.imag)
    )
    
    // 只取前半部分（正频率）
    const halfSpectrum = powerSpectrum.slice(0, Math.floor(powerSpectrum.length / 2))
    
    // 生成频率轴
    const frequencies = halfSpectrum.map((_, i) => i * sampleRate / data.length)
    
    // 寻找主要频率分量
    const peaks = this.findSpectralPeaks(halfSpectrum, frequencies)
    
    // 确定基频和主频
    const fundamentalFrequency = peaks.length > 0 ? peaks[0].frequency : 0
    const dominantFrequency = peaks.reduce((max, peak) => 
      peak.amplitude > max.amplitude ? peak : max, peaks[0] || {frequency: 0, amplitude: 0}
    ).frequency
    
    // 识别谐波
    const harmonics = this.identifyHarmonics(peaks, fundamentalFrequency)
    
    // 计算THD
    const thd = this.calculateTHD(harmonics)
    
    // 计算SNR
    const snr = this.calculateSNR(halfSpectrum, peaks)
    
    // 计算带宽
    const bandwidth = this.calculateBandwidth(halfSpectrum, frequencies)
    
    return {
      fundamentalFrequency,
      dominantFrequency,
      harmonics,
      thd,
      snr,
      bandwidth
    }
  }
  
  /**
   * 相位测量
   * @param signal1 信号1
   * @param signal2 信号2
   * @param sampleRate 采样率
   * @returns 相位测量结果
   */
  static phaseMeasurement(
    signal1: number[], 
    signal2: number[], 
    sampleRate: number = 1000
  ): PhaseMeasurementResult {
    // 确保两个信号长度相同
    const minLength = Math.min(signal1.length, signal2.length)
    const s1 = signal1.slice(0, minLength)
    const s2 = signal2.slice(0, minLength)
    
    // 计算互相关
    const correlation = this.crossCorrelation(s1, s2)
    
    // 寻找最大相关值的位置
    const maxCorrelationIndex = correlation.indexOf(Math.max(...correlation))
    const delay = (maxCorrelationIndex - correlation.length / 2) / sampleRate
    
    // 计算相关系数
    const correlationCoeff = this.correlationCoefficient(s1, s2)
    
    // 使用Hilbert变换计算瞬时相位
    const phase1 = this.instantaneousPhase(s1)
    const phase2 = this.instantaneousPhase(s2)
    
    // 计算平均相位差
    const phaseDifferences = phase1.map((p1, i) => {
      const diff = p1 - phase2[i]
      // 将相位差归一化到[-π, π]范围
      return ((diff + Math.PI) % (2 * Math.PI)) - Math.PI
    })
    
    const avgPhaseDiff = phaseDifferences.reduce((a, b) => a + b, 0) / phaseDifferences.length
    const phaseInDegrees = avgPhaseDiff * 180 / Math.PI
    
    // 计算相干性
    const coherence = this.calculateCoherence(s1, s2)
    
    return {
      phase: phaseInDegrees,
      phaseRad: avgPhaseDiff,
      delay,
      correlation: correlationCoeff,
      coherence
    }
  }
  
  /**
   * 自动测量
   * @param data 数据数组
   * @param sampleRate 采样率
   * @returns 自动测量结果
   */
  static autoMeasurement(data: number[], sampleRate: number = 1000): AutoMeasurementResult {
    // 基本统计量
    const mean = data.reduce((a, b) => a + b, 0) / data.length
    const min = Math.min(...data)
    const max = Math.max(...data)
    const peakToPeak = max - min
    
    // RMS值
    const rms = Math.sqrt(data.reduce((sum, val) => sum + val * val, 0) / data.length)
    
    // 频率和周期测量
    const peaks = this.peakDetection(data, mean, 2)
    let frequency = 0
    let period = 0
    
    if (peaks.peaks.length >= 2) {
      const peakIntervals = []
      for (let i = 1; i < peaks.peaks.length; i++) {
        peakIntervals.push(peaks.peaks[i].index - peaks.peaks[i-1].index)
      }
      const avgInterval = peakIntervals.reduce((a, b) => a + b, 0) / peakIntervals.length
      period = avgInterval / sampleRate
      frequency = 1 / period
    }
    
    // 幅度（峰值到平均值）
    const amplitude = Math.max(max - mean, mean - min)
    
    // 占空比计算（假设为方波信号）
    const threshold = (max + min) / 2
    let highTime = 0
    let inHighState = data[0] > threshold
    
    for (let i = 1; i < data.length; i++) {
      const currentHigh = data[i] > threshold
      if (currentHigh) {
        highTime++
      }
      if (currentHigh !== inHighState) {
        inHighState = currentHigh
      }
    }
    
    const dutyCycle = (highTime / data.length) * 100
    
    // 上升时间和下降时间（10%-90%）
    const riseTime = this.calculateRiseTime(data, sampleRate)
    const fallTime = this.calculateFallTime(data, sampleRate)
    
    // 脉冲宽度
    const pulseWidth = this.calculatePulseWidth(data, sampleRate, threshold)
    
    return {
      frequency,
      period,
      amplitude,
      rms,
      mean,
      min,
      max,
      peakToPeak,
      dutyCycle,
      riseTime,
      fallTime,
      pulseWidth
    }
  }
  
  // 私有辅助方法
  
  /**
   * 计算突出度
   */
  private static calculateProminence(data: number[], peakIndex: number, isPeak: boolean = true): number {
    const peakValue = data[peakIndex]
    let leftMin = peakValue
    let rightMin = peakValue
    
    // 向左搜索
    for (let i = peakIndex - 1; i >= 0; i--) {
      if (isPeak) {
        leftMin = Math.min(leftMin, data[i])
        if (data[i] > peakValue) break
      } else {
        leftMin = Math.max(leftMin, data[i])
        if (data[i] < peakValue) break
      }
    }
    
    // 向右搜索
    for (let i = peakIndex + 1; i < data.length; i++) {
      if (isPeak) {
        rightMin = Math.min(rightMin, data[i])
        if (data[i] > peakValue) break
      } else {
        rightMin = Math.max(rightMin, data[i])
        if (data[i] < peakValue) break
      }
    }
    
    const baseLevel = isPeak ? Math.max(leftMin, rightMin) : Math.min(leftMin, rightMin)
    return Math.abs(peakValue - baseLevel)
  }
  
  /**
   * 计算峰宽
   */
  private static calculatePeakWidth(data: number[], peakIndex: number, isPeak: boolean = true): number {
    const peakValue = data[peakIndex]
    const halfHeight = isPeak ? peakValue * 0.5 : peakValue * 0.5
    
    let leftIndex = peakIndex
    let rightIndex = peakIndex
    
    // 向左搜索半高点
    for (let i = peakIndex - 1; i >= 0; i--) {
      if ((isPeak && data[i] <= halfHeight) || (!isPeak && data[i] >= halfHeight)) {
        leftIndex = i
        break
      }
    }
    
    // 向右搜索半高点
    for (let i = peakIndex + 1; i < data.length; i++) {
      if ((isPeak && data[i] <= halfHeight) || (!isPeak && data[i] >= halfHeight)) {
        rightIndex = i
        break
      }
    }
    
    return rightIndex - leftIndex
  }
  
  /**
   * 应用窗函数
   */
  private static applyWindow(data: number[], windowType: string): number[] {
    const N = data.length
    const windowed = new Array(N)
    
    for (let i = 0; i < N; i++) {
      let window = 1
      
      switch (windowType) {
        case 'hanning':
          window = 0.5 * (1 - Math.cos(2 * Math.PI * i / (N - 1)))
          break
        case 'hamming':
          window = 0.54 - 0.46 * Math.cos(2 * Math.PI * i / (N - 1))
          break
        case 'blackman':
          window = 0.42 - 0.5 * Math.cos(2 * Math.PI * i / (N - 1)) + 0.08 * Math.cos(4 * Math.PI * i / (N - 1))
          break
      }
      
      windowed[i] = data[i] * window
    }
    
    return windowed
  }
  
  /**
   * 简化的FFT实现
   */
  private static fft(data: number[]): Array<{real: number, imag: number}> {
    const N = data.length
    const result = new Array(N)
    
    for (let k = 0; k < N; k++) {
      let real = 0
      let imag = 0
      
      for (let n = 0; n < N; n++) {
        const angle = -2 * Math.PI * k * n / N
        real += data[n] * Math.cos(angle)
        imag += data[n] * Math.sin(angle)
      }
      
      result[k] = { real, imag }
    }
    
    return result
  }
  
  /**
   * 寻找频谱峰值
   */
  private static findSpectralPeaks(spectrum: number[], frequencies: number[]): Array<{frequency: number, amplitude: number, phase: number}> {
    const peaks = []
    const threshold = Math.max(...spectrum) * 0.1 // 10%阈值
    
    for (let i = 1; i < spectrum.length - 1; i++) {
      if (spectrum[i] > spectrum[i-1] && spectrum[i] > spectrum[i+1] && spectrum[i] > threshold) {
        peaks.push({
          frequency: frequencies[i],
          amplitude: spectrum[i],
          phase: 0 // 简化，实际需要从复数FFT结果计算
        })
      }
    }
    
    // 按幅度排序
    return peaks.sort((a, b) => b.amplitude - a.amplitude)
  }
  
  /**
   * 识别谐波
   */
  private static identifyHarmonics(peaks: Array<{frequency: number, amplitude: number, phase: number}>, fundamental: number): Array<{frequency: number, amplitude: number, phase: number, harmonic: number}> {
    const harmonics = []
    const tolerance = fundamental * 0.05 // 5%容差
    
    for (const peak of peaks) {
      for (let h = 1; h <= 10; h++) {
        const expectedFreq = fundamental * h
        if (Math.abs(peak.frequency - expectedFreq) < tolerance) {
          harmonics.push({
            ...peak,
            harmonic: h
          })
          break
        }
      }
    }
    
    return harmonics.sort((a, b) => a.harmonic - b.harmonic)
  }
  
  /**
   * 计算总谐波失真
   */
  private static calculateTHD(harmonics: Array<{frequency: number, amplitude: number, phase: number, harmonic: number}>): number {
    const fundamental = harmonics.find(h => h.harmonic === 1)
    if (!fundamental) return 0
    
    const harmonicSum = harmonics
      .filter(h => h.harmonic > 1)
      .reduce((sum, h) => sum + h.amplitude * h.amplitude, 0)
    
    return Math.sqrt(harmonicSum) / fundamental.amplitude * 100
  }
  
  /**
   * 计算信噪比
   */
  private static calculateSNR(spectrum: number[], peaks: Array<{frequency: number, amplitude: number, phase: number}>): number {
    const signalPower = peaks.reduce((sum, peak) => sum + peak.amplitude * peak.amplitude, 0)
    const totalPower = spectrum.reduce((sum, val) => sum + val * val, 0)
    const noisePower = totalPower - signalPower
    
    return noisePower > 0 ? 10 * Math.log10(signalPower / noisePower) : Infinity
  }
  
  /**
   * 计算带宽
   */
  private static calculateBandwidth(spectrum: number[], frequencies: number[]): number {
    const maxAmplitude = Math.max(...spectrum)
    const threshold = maxAmplitude * 0.707 // -3dB点
    
    let lowFreq = 0
    let highFreq = frequencies[frequencies.length - 1]
    
    // 寻找低频-3dB点
    for (let i = 0; i < spectrum.length; i++) {
      if (spectrum[i] >= threshold) {
        lowFreq = frequencies[i]
        break
      }
    }
    
    // 寻找高频-3dB点
    for (let i = spectrum.length - 1; i >= 0; i--) {
      if (spectrum[i] >= threshold) {
        highFreq = frequencies[i]
        break
      }
    }
    
    return highFreq - lowFreq
  }
  
  /**
   * 互相关计算
   */
  private static crossCorrelation(signal1: number[], signal2: number[]): number[] {
    const N = signal1.length
    const correlation = new Array(2 * N - 1)
    
    for (let lag = -(N-1); lag < N; lag++) {
      let sum = 0
      let count = 0
      
      for (let i = 0; i < N; i++) {
        const j = i + lag
        if (j >= 0 && j < N) {
          sum += signal1[i] * signal2[j]
          count++
        }
      }
      
      correlation[lag + N - 1] = count > 0 ? sum / count : 0
    }
    
    return correlation
  }
  
  /**
   * 相关系数计算
   */
  private static correlationCoefficient(signal1: number[], signal2: number[]): number {
    const mean1 = signal1.reduce((a, b) => a + b, 0) / signal1.length
    const mean2 = signal2.reduce((a, b) => a + b, 0) / signal2.length
    
    let numerator = 0
    let sum1Sq = 0
    let sum2Sq = 0
    
    for (let i = 0; i < signal1.length; i++) {
      const diff1 = signal1[i] - mean1
      const diff2 = signal2[i] - mean2
      
      numerator += diff1 * diff2
      sum1Sq += diff1 * diff1
      sum2Sq += diff2 * diff2
    }
    
    const denominator = Math.sqrt(sum1Sq * sum2Sq)
    return denominator > 0 ? numerator / denominator : 0
  }
  
  /**
   * 瞬时相位计算（简化版Hilbert变换）
   */
  private static instantaneousPhase(signal: number[]): number[] {
    // 简化实现，实际应使用Hilbert变换
    const phases = new Array(signal.length)
    
    for (let i = 0; i < signal.length; i++) {
      // 使用简单的相位估计
      if (i > 0) {
        const derivative = signal[i] - signal[i-1]
        phases[i] = Math.atan2(derivative, signal[i])
      } else {
        phases[i] = 0
      }
    }
    
    return phases
  }
  
  /**
   * 相干性计算
   */
  private static calculateCoherence(signal1: number[], signal2: number[]): number {
    // 简化的相干性计算
    const correlation = this.correlationCoefficient(signal1, signal2)
    return Math.abs(correlation)
  }
  
  /**
   * 计算上升时间
   */
  private static calculateRiseTime(data: number[], sampleRate: number): number {
    const min = Math.min(...data)
    const max = Math.max(...data)
    const level10 = min + (max - min) * 0.1
    const level90 = min + (max - min) * 0.9
    
    let start10 = -1
    let end90 = -1
    
    for (let i = 0; i < data.length - 1; i++) {
      if (start10 === -1 && data[i] <= level10 && data[i+1] > level10) {
        start10 = i
      }
      if (start10 !== -1 && data[i] <= level90 && data[i+1] > level90) {
        end90 = i
        break
      }
    }
    
    return (start10 !== -1 && end90 !== -1) ? (end90 - start10) / sampleRate : 0
  }
  
  /**
   * 计算下降时间
   */
  private static calculateFallTime(data: number[], sampleRate: number): number {
    const min = Math.min(...data)
    const max = Math.max(...data)
    const level90 = min + (max - min) * 0.9
    const level10 = min + (max - min) * 0.1
    
    let start90 = -1
    let end10 = -1
    
    for (let i = 0; i < data.length - 1; i++) {
      if (start90 === -1 && data[i] >= level90 && data[i+1] < level90) {
        start90 = i
      }
      if (start90 !== -1 && data[i] >= level10 && data[i+1] < level10) {
        end10 = i
        break
      }
    }
    
    return (start90 !== -1 && end10 !== -1) ? (end10 - start90) / sampleRate : 0
  }
  
  /**
   * 计算脉冲宽度
   */
  private static calculatePulseWidth(data: number[], sampleRate: number, threshold: number): number {
    let pulseStart = -1
    let totalWidth = 0
    let pulseCount = 0
    
    for (let i = 0; i < data.length; i++) {
      if (data[i] > threshold && pulseStart === -1) {
        pulseStart = i
      } else if (data[i] <= threshold && pulseStart !== -1) {
        totalWidth += (i - pulseStart) / sampleRate
        pulseCount++
        pulseStart = -1
      }
    }
    
    return pulseCount > 0 ? totalWidth / pulseCount : 0
  }
}
