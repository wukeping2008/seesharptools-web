/**
 * 信号生成器 - USB-1601前端模拟数据生成
 * 支持多种波形类型和参数配置
 */

export enum SignalType {
  SINE = 'sine',
  SQUARE = 'square',
  TRIANGLE = 'triangle',
  SAWTOOTH = 'sawtooth',
  PULSE = 'pulse',
  NOISE = 'noise',
  DC = 'dc',
  CHIRP = 'chirp',
  MODULATED = 'modulated',
  CUSTOM = 'custom'
}

export interface SignalConfig {
  type: SignalType
  frequency: number // Hz
  amplitude: number // V
  offset: number // V
  phase: number // radians
  dutyCycle?: number // for pulse wave (0-1)
  noiseLevel?: number // noise amplitude (0-1)
  harmonics?: number[] // for custom harmonics
  modulation?: {
    type: 'AM' | 'FM' | 'PM'
    frequency: number
    index: number
  }
  chirp?: {
    startFreq: number
    endFreq: number
    duration: number
  }
}

export interface ChannelConfig {
  id: string
  name: string
  enabled: boolean
  signal: SignalConfig
  color: string
  unit: string
}

export class SignalGenerator {
  private sampleRate: number
  private timeBase: number = 0
  private channels: Map<string, ChannelConfig>
  private buffer: Map<string, number[]>
  private bufferSize: number

  constructor(sampleRate: number = 10000, bufferSize: number = 1000) {
    this.sampleRate = sampleRate
    this.bufferSize = bufferSize
    this.channels = new Map()
    this.buffer = new Map()
  }

  /**
   * 添加通道
   */
  addChannel(config: ChannelConfig): void {
    this.channels.set(config.id, config)
    this.buffer.set(config.id, [])
  }

  /**
   * 移除通道
   */
  removeChannel(channelId: string): void {
    this.channels.delete(channelId)
    this.buffer.delete(channelId)
  }

  /**
   * 更新通道配置
   */
  updateChannel(channelId: string, config: Partial<ChannelConfig>): void {
    const channel = this.channels.get(channelId)
    if (channel) {
      this.channels.set(channelId, { ...channel, ...config })
    }
  }

  /**
   * 生成单个样本点
   */
  private generateSample(signal: SignalConfig, time: number): number {
    let value = 0
    const omega = 2 * Math.PI * signal.frequency

    switch (signal.type) {
      case SignalType.SINE:
        value = Math.sin(omega * time + signal.phase)
        break

      case SignalType.SQUARE:
        value = Math.sin(omega * time + signal.phase) >= 0 ? 1 : -1
        break

      case SignalType.TRIANGLE:
        const t = (time * signal.frequency + signal.phase / (2 * Math.PI)) % 1
        value = 4 * (t < 0.5 ? t : 1 - t) - 1
        break

      case SignalType.SAWTOOTH:
        const st = (time * signal.frequency + signal.phase / (2 * Math.PI)) % 1
        value = 2 * st - 1
        break

      case SignalType.PULSE:
        const duty = signal.dutyCycle || 0.5
        const pt = (time * signal.frequency + signal.phase / (2 * Math.PI)) % 1
        value = pt < duty ? 1 : -1
        break

      case SignalType.NOISE:
        value = 2 * Math.random() - 1
        break

      case SignalType.DC:
        value = 1
        break

      case SignalType.CHIRP:
        if (signal.chirp) {
          const { startFreq, endFreq, duration } = signal.chirp
          const t = time % duration
          const freq = startFreq + (endFreq - startFreq) * (t / duration)
          value = Math.sin(2 * Math.PI * freq * time + signal.phase)
        }
        break

      case SignalType.MODULATED:
        if (signal.modulation) {
          const carrier = Math.sin(omega * time + signal.phase)
          const modulator = Math.sin(2 * Math.PI * signal.modulation.frequency * time)
          
          switch (signal.modulation.type) {
            case 'AM':
              value = carrier * (1 + signal.modulation.index * modulator)
              break
            case 'FM':
              const phaseShift = signal.modulation.index * modulator
              value = Math.sin(omega * time + signal.phase + phaseShift)
              break
            case 'PM':
              value = Math.sin(omega * time + signal.phase + signal.modulation.index * modulator)
              break
          }
        }
        break

      case SignalType.CUSTOM:
        // 自定义谐波合成
        if (signal.harmonics) {
          signal.harmonics.forEach((harmonic, index) => {
            value += harmonic * Math.sin(omega * (index + 1) * time + signal.phase)
          })
        }
        break
    }

    // 应用幅度和偏移
    value = value * signal.amplitude + signal.offset

    // 添加噪声
    if (signal.noiseLevel && signal.noiseLevel > 0) {
      value += (Math.random() - 0.5) * 2 * signal.noiseLevel * signal.amplitude
    }

    return value
  }

  /**
   * 生成下一批数据点
   */
  generateNext(points: number = 1): Map<string, number[]> {
    const result = new Map<string, number[]>()
    
    this.channels.forEach((channel, channelId) => {
      if (!channel.enabled) return
      
      const data: number[] = []
      for (let i = 0; i < points; i++) {
        const time = (this.timeBase + i) / this.sampleRate
        const value = this.generateSample(channel.signal, time)
        data.push(value)
        
        // 更新缓冲区
        const buffer = this.buffer.get(channelId) || []
        buffer.push(value)
        if (buffer.length > this.bufferSize) {
          buffer.shift()
        }
        this.buffer.set(channelId, buffer)
      }
      
      result.set(channelId, data)
    })
    
    this.timeBase += points
    return result
  }

  /**
   * 生成实时数据流（返回格式化的数据对象）
   */
  generateRealtimeData(): any {
    const timestamp = Date.now()
    const data: any = { timestamp }
    
    this.channels.forEach((channel, channelId) => {
      if (!channel.enabled) return
      
      const time = this.timeBase / this.sampleRate
      const value = this.generateSample(channel.signal, time)
      data[channelId] = value
      
      // 更新缓冲区
      const buffer = this.buffer.get(channelId) || []
      buffer.push(value)
      if (buffer.length > this.bufferSize) {
        buffer.shift()
      }
      this.buffer.set(channelId, buffer)
    })
    
    this.timeBase++
    return data
  }

  /**
   * 获取通道缓冲区数据
   */
  getChannelBuffer(channelId: string): number[] {
    return this.buffer.get(channelId) || []
  }

  /**
   * 获取所有缓冲区数据
   */
  getAllBuffers(): Map<string, number[]> {
    return new Map(this.buffer)
  }

  /**
   * 重置时间基准
   */
  reset(): void {
    this.timeBase = 0
    this.buffer.clear()
    this.channels.forEach((_, channelId) => {
      this.buffer.set(channelId, [])
    })
  }

  /**
   * 获取当前时间（秒）
   */
  getCurrentTime(): number {
    return this.timeBase / this.sampleRate
  }

  /**
   * 设置采样率
   */
  setSampleRate(rate: number): void {
    this.sampleRate = rate
  }

  /**
   * 获取采样率
   */
  getSampleRate(): number {
    return this.sampleRate
  }
}

/**
 * 预设信号配置
 */
export const SignalPresets = {
  // 基础测试信号
  testSine: {
    type: SignalType.SINE,
    frequency: 10,
    amplitude: 5,
    offset: 0,
    phase: 0
  } as SignalConfig,

  // 心电图模拟
  ecg: {
    type: SignalType.CUSTOM,
    frequency: 1.2, // 72 BPM
    amplitude: 1,
    offset: 0,
    phase: 0,
    harmonics: [1, 0.3, 0.1, 0.05, 0.02],
    noiseLevel: 0.02
  } as SignalConfig,

  // PWM信号
  pwm: {
    type: SignalType.PULSE,
    frequency: 1000,
    amplitude: 5,
    offset: 2.5,
    phase: 0,
    dutyCycle: 0.3
  } as SignalConfig,

  // 扫频信号
  sweep: {
    type: SignalType.CHIRP,
    frequency: 1,
    amplitude: 3,
    offset: 0,
    phase: 0,
    chirp: {
      startFreq: 1,
      endFreq: 100,
      duration: 10
    }
  } as SignalConfig,

  // AM调制信号
  amModulated: {
    type: SignalType.MODULATED,
    frequency: 100,
    amplitude: 2,
    offset: 0,
    phase: 0,
    modulation: {
      type: 'AM',
      frequency: 10,
      index: 0.5
    }
  } as SignalConfig,

  // 噪声信号
  whiteNoise: {
    type: SignalType.NOISE,
    frequency: 0,
    amplitude: 0.5,
    offset: 0,
    phase: 0
  } as SignalConfig,

  // 直流信号
  dcVoltage: {
    type: SignalType.DC,
    frequency: 0,
    amplitude: 3.3,
    offset: 0,
    phase: 0
  } as SignalConfig
}

/**
 * 创建默认通道配置
 */
export function createDefaultChannels(): ChannelConfig[] {
  return [
    {
      id: 'AI0',
      name: '通道0 - 正弦波',
      enabled: true,
      signal: {
        ...SignalPresets.testSine,
        frequency: 5,
        amplitude: 2
      },
      color: '#1f77b4',
      unit: 'V'
    },
    {
      id: 'AI1',
      name: '通道1 - 方波',
      enabled: true,
      signal: {
        type: SignalType.SQUARE,
        frequency: 10,
        amplitude: 3,
        offset: 0,
        phase: 0,
        noiseLevel: 0.05
      },
      color: '#ff7f0e',
      unit: 'V'
    },
    {
      id: 'AI2',
      name: '通道2 - 三角波',
      enabled: true,
      signal: {
        type: SignalType.TRIANGLE,
        frequency: 8,
        amplitude: 2.5,
        offset: 1,
        phase: 0
      },
      color: '#2ca02c',
      unit: 'V'
    },
    {
      id: 'AI3',
      name: '通道3 - 噪声',
      enabled: false,
      signal: SignalPresets.whiteNoise,
      color: '#d62728',
      unit: 'V'
    }
  ]
}

/**
 * 数据分析工具
 */
export class SignalAnalyzer {
  /**
   * 计算RMS值
   */
  static calculateRMS(data: number[]): number {
    if (data.length === 0) return 0
    const sum = data.reduce((acc, val) => acc + val * val, 0)
    return Math.sqrt(sum / data.length)
  }

  /**
   * 计算峰峰值
   */
  static calculatePeakToPeak(data: number[]): number {
    if (data.length === 0) return 0
    const max = Math.max(...data)
    const min = Math.min(...data)
    return max - min
  }

  /**
   * 计算平均值
   */
  static calculateMean(data: number[]): number {
    if (data.length === 0) return 0
    return data.reduce((acc, val) => acc + val, 0) / data.length
  }

  /**
   * 计算频率（通过过零检测）
   */
  static calculateFrequency(data: number[], sampleRate: number): number {
    if (data.length < 3) return 0
    
    let zeroCrossings = 0
    for (let i = 1; i < data.length; i++) {
      if ((data[i - 1] < 0 && data[i] >= 0) || (data[i - 1] >= 0 && data[i] < 0)) {
        zeroCrossings++
      }
    }
    
    const duration = data.length / sampleRate
    return zeroCrossings / (2 * duration)
  }

  /**
   * 简单FFT（用于频谱分析）
   */
  static simpleFFT(data: number[]): { frequencies: number[], magnitudes: number[] } {
    // 这里使用简化的DFT计算，实际应用中建议使用专门的FFT库
    const N = data.length
    const frequencies: number[] = []
    const magnitudes: number[] = []
    
    for (let k = 0; k < N / 2; k++) {
      let real = 0
      let imag = 0
      
      for (let n = 0; n < N; n++) {
        const angle = -2 * Math.PI * k * n / N
        real += data[n] * Math.cos(angle)
        imag += data[n] * Math.sin(angle)
      }
      
      frequencies.push(k)
      magnitudes.push(Math.sqrt(real * real + imag * imag) / N)
    }
    
    return { frequencies, magnitudes }
  }
}