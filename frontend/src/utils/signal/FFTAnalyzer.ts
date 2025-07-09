/**
 * FFT频谱分析器
 * 支持实时FFT计算、多种窗函数和功率谱密度分析
 */

export interface FFTOptions {
  sampleRate: number // 采样率 (Hz)
  windowType: WindowType // 窗函数类型
  windowSize: number // 窗口大小 (必须是2的幂)
  overlap: number // 重叠比例 (0-1)
  averageCount: number // 平均次数
  enableZeroPadding: boolean // 是否启用零填充
}

export type WindowType = 'rectangular' | 'hanning' | 'hamming' | 'blackman' | 'kaiser' | 'flattop'

export interface FFTResult {
  frequencies: Float32Array // 频率轴 (Hz)
  magnitudes: Float32Array // 幅度谱
  phases: Float32Array // 相位谱
  powerSpectralDensity: Float32Array // 功率谱密度
  peakFrequency: number // 峰值频率
  peakMagnitude: number // 峰值幅度
  totalPower: number // 总功率
  snr: number // 信噪比
}

export interface PeakInfo {
  frequency: number
  magnitude: number
  index: number
}

export class FFTAnalyzer {
  private options: FFTOptions
  private windowFunction: Float32Array
  private fftSize: number
  private averageBuffer: Float32Array[]
  private averageIndex: number = 0
  
  constructor(options: FFTOptions) {
    this.options = { ...options }
    this.fftSize = this.options.windowSize
    
    // 确保FFT大小是2的幂
    if (!this.isPowerOfTwo(this.fftSize)) {
      this.fftSize = this.nextPowerOfTwo(this.fftSize)
      console.warn(`FFT size adjusted to ${this.fftSize} (next power of 2)`)
    }
    
    // 生成窗函数
    this.windowFunction = this.generateWindow(this.options.windowType, this.fftSize)
    
    // 初始化平均缓冲区
    this.averageBuffer = []
    for (let i = 0; i < this.options.averageCount; i++) {
      this.averageBuffer.push(new Float32Array(this.fftSize / 2))
    }
  }
  
  /**
   * 执行FFT分析
   */
  public analyze(signal: Float32Array): FFTResult {
    // 预处理信号
    const processedSignal = this.preprocessSignal(signal)
    
    // 执行FFT
    const fftResult = this.performFFT(processedSignal)
    
    // 计算频率轴
    const frequencies = this.generateFrequencyAxis()
    
    // 计算幅度谱和相位谱
    const magnitudes = this.calculateMagnitudes(fftResult)
    const phases = this.calculatePhases(fftResult)
    
    // 应用平均
    const averagedMagnitudes = this.applyAveraging(magnitudes)
    
    // 计算功率谱密度
    const powerSpectralDensity = this.calculatePSD(averagedMagnitudes)
    
    // 查找峰值
    const peakInfo = this.findPeak(averagedMagnitudes, frequencies)
    
    // 计算总功率
    const totalPower = this.calculateTotalPower(powerSpectralDensity)
    
    // 计算信噪比
    const snr = this.calculateSNR(averagedMagnitudes, peakInfo)
    
    return {
      frequencies,
      magnitudes: averagedMagnitudes,
      phases,
      powerSpectralDensity,
      peakFrequency: peakInfo.frequency,
      peakMagnitude: peakInfo.magnitude,
      totalPower,
      snr
    }
  }
  
  /**
   * 查找多个峰值
   */
  public findPeaks(magnitudes: Float32Array, frequencies: Float32Array, threshold: number = 0.1): PeakInfo[] {
    const peaks: PeakInfo[] = []
    const minDistance = Math.floor(magnitudes.length * 0.01) // 最小距离为1%的频谱长度
    
    for (let i = 1; i < magnitudes.length - 1; i++) {
      const current = magnitudes[i]
      const prev = magnitudes[i - 1]
      const next = magnitudes[i + 1]
      
      // 检查是否为局部最大值
      if (current > prev && current > next && current > threshold) {
        // 检查与已找到峰值的距离
        const tooClose = peaks.some(peak => Math.abs(peak.index - i) < minDistance)
        
        if (!tooClose) {
          peaks.push({
            frequency: frequencies[i],
            magnitude: current,
            index: i
          })
        }
      }
    }
    
    // 按幅度排序
    return peaks.sort((a, b) => b.magnitude - a.magnitude)
  }
  
  /**
   * 计算谐波失真
   */
  public calculateTHD(magnitudes: Float32Array, frequencies: Float32Array, fundamentalFreq: number): number {
    const peaks = this.findPeaks(magnitudes, frequencies, 0.01)
    
    // 找到基频
    let fundamentalMagnitude = 0
    let harmonicPowerSum = 0
    
    for (const peak of peaks) {
      const ratio = peak.frequency / fundamentalFreq
      
      if (Math.abs(ratio - 1) < 0.1) {
        // 基频
        fundamentalMagnitude = peak.magnitude
      } else if (Math.abs(ratio - Math.round(ratio)) < 0.1 && ratio > 1) {
        // 谐波
        harmonicPowerSum += peak.magnitude * peak.magnitude
      }
    }
    
    if (fundamentalMagnitude === 0) return 0
    
    const fundamentalPower = fundamentalMagnitude * fundamentalMagnitude
    return Math.sqrt(harmonicPowerSum / fundamentalPower) * 100 // 百分比
  }
  
  /**
   * 预处理信号
   */
  private preprocessSignal(signal: Float32Array): Float32Array {
    let processedSignal = new Float32Array(this.fftSize)
    
    // 复制或截断信号到FFT大小
    const copyLength = Math.min(signal.length, this.fftSize)
    processedSignal.set(signal.subarray(0, copyLength))
    
    // 零填充（如果启用）
    if (this.options.enableZeroPadding && signal.length < this.fftSize) {
      // 已经通过初始化为0实现了零填充
    }
    
    // 应用窗函数
    for (let i = 0; i < this.fftSize; i++) {
      processedSignal[i] *= this.windowFunction[i]
    }
    
    return processedSignal
  }
  
  /**
   * 执行FFT计算
   */
  private performFFT(signal: Float32Array): { real: Float32Array; imag: Float32Array } {
    const real = new Float32Array(signal)
    const imag = new Float32Array(this.fftSize)
    
    // 使用Cooley-Tukey FFT算法
    this.fft(real, imag)
    
    return { real, imag }
  }
  
  /**
   * Cooley-Tukey FFT算法实现
   */
  private fft(real: Float32Array, imag: Float32Array): void {
    const n = real.length
    
    // 位反转
    this.bitReverse(real, imag, n)
    
    // FFT计算
    for (let size = 2; size <= n; size *= 2) {
      const halfSize = size / 2
      const step = Math.PI / halfSize
      
      for (let i = 0; i < n; i += size) {
        for (let j = 0; j < halfSize; j++) {
          const u = i + j
          const v = i + j + halfSize
          
          const angle = -j * step
          const cos = Math.cos(angle)
          const sin = Math.sin(angle)
          
          const realTemp = real[v] * cos - imag[v] * sin
          const imagTemp = real[v] * sin + imag[v] * cos
          
          real[v] = real[u] - realTemp
          imag[v] = imag[u] - imagTemp
          real[u] += realTemp
          imag[u] += imagTemp
        }
      }
    }
  }
  
  /**
   * 位反转
   */
  private bitReverse(real: Float32Array, imag: Float32Array, n: number): void {
    let j = 0
    for (let i = 1; i < n - 1; i++) {
      let bit = n >> 1
      while (j & bit) {
        j ^= bit
        bit >>= 1
      }
      j ^= bit
      
      if (i < j) {
        [real[i], real[j]] = [real[j], real[i]]
        ;[imag[i], imag[j]] = [imag[j], imag[i]]
      }
    }
  }
  
  /**
   * 生成窗函数
   */
  private generateWindow(type: WindowType, size: number): Float32Array {
    const window = new Float32Array(size)
    
    switch (type) {
      case 'rectangular':
        window.fill(1)
        break
        
      case 'hanning':
        for (let i = 0; i < size; i++) {
          window[i] = 0.5 * (1 - Math.cos(2 * Math.PI * i / (size - 1)))
        }
        break
        
      case 'hamming':
        for (let i = 0; i < size; i++) {
          window[i] = 0.54 - 0.46 * Math.cos(2 * Math.PI * i / (size - 1))
        }
        break
        
      case 'blackman':
        for (let i = 0; i < size; i++) {
          const factor = 2 * Math.PI * i / (size - 1)
          window[i] = 0.42 - 0.5 * Math.cos(factor) + 0.08 * Math.cos(2 * factor)
        }
        break
        
      case 'kaiser':
        // 简化的Kaiser窗，beta = 8.6
        const beta = 8.6
        const alpha = (size - 1) / 2
        const i0Beta = this.besselI0(beta)
        
        for (let i = 0; i < size; i++) {
          const x = (i - alpha) / alpha
          const arg = beta * Math.sqrt(1 - x * x)
          window[i] = this.besselI0(arg) / i0Beta
        }
        break
        
      case 'flattop':
        for (let i = 0; i < size; i++) {
          const factor = 2 * Math.PI * i / (size - 1)
          window[i] = 0.21557895 
            - 0.41663158 * Math.cos(factor)
            + 0.277263158 * Math.cos(2 * factor)
            - 0.083578947 * Math.cos(3 * factor)
            + 0.006947368 * Math.cos(4 * factor)
        }
        break
    }
    
    return window
  }
  
  /**
   * 贝塞尔函数I0近似
   */
  private besselI0(x: number): number {
    let sum = 1
    let term = 1
    const xSquaredOver4 = (x * x) / 4
    
    for (let i = 1; i <= 50; i++) {
      term *= xSquaredOver4 / (i * i)
      sum += term
      if (term < 1e-12) break
    }
    
    return sum
  }
  
  /**
   * 生成频率轴
   */
  private generateFrequencyAxis(): Float32Array {
    const frequencies = new Float32Array(this.fftSize / 2)
    const df = this.options.sampleRate / this.fftSize
    
    for (let i = 0; i < frequencies.length; i++) {
      frequencies[i] = i * df
    }
    
    return frequencies
  }
  
  /**
   * 计算幅度谱
   */
  private calculateMagnitudes(fftResult: { real: Float32Array; imag: Float32Array }): Float32Array {
    const { real, imag } = fftResult
    const magnitudes = new Float32Array(this.fftSize / 2)
    
    for (let i = 0; i < magnitudes.length; i++) {
      magnitudes[i] = Math.sqrt(real[i] * real[i] + imag[i] * imag[i])
    }
    
    return magnitudes
  }
  
  /**
   * 计算相位谱
   */
  private calculatePhases(fftResult: { real: Float32Array; imag: Float32Array }): Float32Array {
    const { real, imag } = fftResult
    const phases = new Float32Array(this.fftSize / 2)
    
    for (let i = 0; i < phases.length; i++) {
      phases[i] = Math.atan2(imag[i], real[i])
    }
    
    return phases
  }
  
  /**
   * 应用平均
   */
  private applyAveraging(magnitudes: Float32Array): Float32Array {
    // 存储当前结果到缓冲区
    this.averageBuffer[this.averageIndex].set(magnitudes)
    this.averageIndex = (this.averageIndex + 1) % this.options.averageCount
    
    // 计算平均
    const averaged = new Float32Array(magnitudes.length)
    for (let i = 0; i < magnitudes.length; i++) {
      let sum = 0
      for (let j = 0; j < this.averageBuffer.length; j++) {
        sum += this.averageBuffer[j][i]
      }
      averaged[i] = sum / this.averageBuffer.length
    }
    
    return averaged
  }
  
  /**
   * 计算功率谱密度
   */
  private calculatePSD(magnitudes: Float32Array): Float32Array {
    const psd = new Float32Array(magnitudes.length)
    const windowPower = this.calculateWindowPower()
    const df = this.options.sampleRate / this.fftSize
    
    for (let i = 0; i < magnitudes.length; i++) {
      // PSD = |X(f)|^2 / (fs * window_power)
      psd[i] = (magnitudes[i] * magnitudes[i]) / (this.options.sampleRate * windowPower)
      
      // 对于双边谱转单边谱，除DC和Nyquist外都要乘以2
      if (i > 0 && i < magnitudes.length - 1) {
        psd[i] *= 2
      }
    }
    
    return psd
  }
  
  /**
   * 计算窗函数功率
   */
  private calculateWindowPower(): number {
    let power = 0
    for (let i = 0; i < this.windowFunction.length; i++) {
      power += this.windowFunction[i] * this.windowFunction[i]
    }
    return power / this.windowFunction.length
  }
  
  /**
   * 查找峰值
   */
  private findPeak(magnitudes: Float32Array, frequencies: Float32Array): PeakInfo {
    let maxMagnitude = 0
    let maxIndex = 0
    
    for (let i = 1; i < magnitudes.length; i++) { // 跳过DC分量
      if (magnitudes[i] > maxMagnitude) {
        maxMagnitude = magnitudes[i]
        maxIndex = i
      }
    }
    
    return {
      frequency: frequencies[maxIndex],
      magnitude: maxMagnitude,
      index: maxIndex
    }
  }
  
  /**
   * 计算总功率
   */
  private calculateTotalPower(psd: Float32Array): number {
    let totalPower = 0
    const df = this.options.sampleRate / this.fftSize
    
    for (let i = 0; i < psd.length; i++) {
      totalPower += psd[i] * df
    }
    
    return totalPower
  }
  
  /**
   * 计算信噪比
   */
  private calculateSNR(magnitudes: Float32Array, peakInfo: PeakInfo): number {
    const signalPower = peakInfo.magnitude * peakInfo.magnitude
    
    // 计算噪声功率（排除峰值附近的频率）
    let noisePower = 0
    let noiseCount = 0
    const excludeRange = Math.floor(magnitudes.length * 0.02) // 排除峰值附近2%的频率
    
    for (let i = 1; i < magnitudes.length; i++) {
      if (Math.abs(i - peakInfo.index) > excludeRange) {
        noisePower += magnitudes[i] * magnitudes[i]
        noiseCount++
      }
    }
    
    if (noiseCount === 0) return Infinity
    
    const avgNoisePower = noisePower / noiseCount
    return 10 * Math.log10(signalPower / avgNoisePower)
  }
  
  /**
   * 检查是否为2的幂
   */
  private isPowerOfTwo(n: number): boolean {
    return n > 0 && (n & (n - 1)) === 0
  }
  
  /**
   * 获取下一个2的幂
   */
  private nextPowerOfTwo(n: number): number {
    return Math.pow(2, Math.ceil(Math.log2(n)))
  }
  
  /**
   * 更新配置
   */
  public updateOptions(newOptions: Partial<FFTOptions>): void {
    this.options = { ...this.options, ...newOptions }
    
    // 重新生成窗函数
    if (newOptions.windowType || newOptions.windowSize) {
      this.fftSize = this.options.windowSize
      if (!this.isPowerOfTwo(this.fftSize)) {
        this.fftSize = this.nextPowerOfTwo(this.fftSize)
      }
      this.windowFunction = this.generateWindow(this.options.windowType, this.fftSize)
    }
    
    // 重新初始化平均缓冲区
    if (newOptions.averageCount) {
      this.averageBuffer = []
      this.averageIndex = 0
      for (let i = 0; i < this.options.averageCount; i++) {
        this.averageBuffer.push(new Float32Array(this.fftSize / 2))
      }
    }
  }
  
  /**
   * 获取当前配置
   */
  public getOptions(): FFTOptions {
    return { ...this.options }
  }
}
