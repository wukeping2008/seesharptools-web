// 示波器相关类型定义

// 触发类型
export type TriggerType = 'edge' | 'pulse' | 'video' | 'external' | 'auto' | 'normal' | 'single'

// 触发边沿
export type TriggerEdge = 'rising' | 'falling' | 'both'

// 触发模式
export type TriggerMode = 'auto' | 'normal' | 'single'

// 耦合方式
export type CouplingType = 'dc' | 'ac' | 'gnd'

// 通道配置
export interface ChannelConfig {
  id: number
  enabled: boolean
  label: string
  color: string
  coupling: CouplingType
  verticalScale: number // V/div
  verticalOffset: number // V
  probe: number // 探头倍数 (1x, 10x, 100x)
  bandwidth: number // MHz
  invert: boolean
  visible: boolean
}

// 时基配置
export interface TimebaseConfig {
  scale: number // s/div
  position: number // 水平位置 (%)
  mode: 'yt' | 'xy' | 'roll'
  reference: 'left' | 'center' | 'right'
}

// 触发配置
export interface TriggerConfig {
  type: TriggerType
  mode: TriggerMode
  source: number // 触发源通道
  level: number // 触发电平 (V)
  edge: TriggerEdge
  holdoff: number // 触发抑制时间 (s)
  position: number // 触发位置 (%)
  coupling: CouplingType
  noise: boolean // 噪声抑制
  hf: boolean // 高频抑制
}

// 脉宽触发配置
export interface PulseWidthTriggerConfig {
  condition: 'greater' | 'less' | 'equal'
  width: number // 脉宽 (s)
  polarity: 'positive' | 'negative'
}

// 视频触发配置
export interface VideoTriggerConfig {
  standard: 'ntsc' | 'pal' | 'secam'
  sync: 'line' | 'field' | 'odd' | 'even'
  lineNumber: number
}

// 采集配置
export interface AcquisitionConfig {
  mode: 'normal' | 'average' | 'peak' | 'envelope'
  sampleRate: number // Sa/s
  memoryDepth: number // 采样点数
  averageCount: number // 平均次数
  interpolation: 'linear' | 'sinx' | 'none'
}

// 测量类型
export type MeasurementType = 
  | 'frequency' | 'period' | 'amplitude' | 'peak_to_peak' | 'rms' | 'mean'
  | 'rise_time' | 'fall_time' | 'pulse_width' | 'duty_cycle' | 'overshoot'
  | 'preshoot' | 'phase' | 'delay' | 'skew'

// 自动测量配置
export interface AutoMeasurement {
  id: string
  type: MeasurementType
  channel: number
  enabled: boolean
  value: number
  unit: string
  statistics: {
    current: number
    minimum: number
    maximum: number
    mean: number
    stdDev: number
    count: number
  }
}

// 游标测量
export interface CursorMeasurement {
  enabled: boolean
  type: 'voltage' | 'time' | 'both'
  cursor1: {
    x: number // 时间位置
    y: number // 电压位置
  }
  cursor2: {
    x: number // 时间位置
    y: number // 电压位置
  }
  delta: {
    x: number // 时间差
    y: number // 电压差
  }
  frequency: number // 1/时间差
}

// 数学运算
export interface MathFunction {
  id: string
  enabled: boolean
  expression: string // 数学表达式，如 "CH1 + CH2", "FFT(CH1)"
  label: string
  color: string
  verticalScale: number
  verticalOffset: number
  visible: boolean
}

// 示波器状态
export interface OscilloscopeStatus {
  running: boolean
  triggered: boolean
  triggerRate: number // 触发频率 Hz
  sampleRate: number // 实际采样率 Sa/s
  memoryUsage: number // 内存使用率 %
  temperature: number // 设备温度 °C
  calibrationDate: string
  errorCode: number
  errorMessage: string
}

// 波形数据
export interface WaveformData {
  channelId: number
  data: Float32Array
  timebase: {
    start: number // 起始时间
    interval: number // 时间间隔
    points: number // 数据点数
  }
  verticalInfo: {
    scale: number // V/div
    offset: number // V
    unit: string
  }
  triggerInfo: {
    position: number // 触发位置索引
    level: number // 触发电平
    time: number // 触发时间
  }
}

// 示波器配置
export interface OscilloscopeConfig {
  channels: ChannelConfig[]
  timebase: TimebaseConfig
  trigger: TriggerConfig
  pulseWidthTrigger: PulseWidthTriggerConfig
  videoTrigger: VideoTriggerConfig
  acquisition: AcquisitionConfig
  measurements: AutoMeasurement[]
  cursors: CursorMeasurement
  mathFunctions: MathFunction[]
  display: {
    persistence: boolean
    intensity: number // 0-100
    grid: 'full' | 'half' | 'none'
    labels: boolean
    vectors: boolean
  }
}

// 默认配置
export const DEFAULT_OSCILLOSCOPE_CONFIG: OscilloscopeConfig = {
  channels: [
    {
      id: 1,
      enabled: true,
      label: 'CH1',
      color: '#FFD700',
      coupling: 'dc',
      verticalScale: 1.0, // 1V/div
      verticalOffset: 0,
      probe: 1,
      bandwidth: 100, // 100MHz
      invert: false,
      visible: true
    },
    {
      id: 2,
      enabled: true,
      label: 'CH2',
      color: '#00CED1',
      coupling: 'dc',
      verticalScale: 1.0,
      verticalOffset: 0,
      probe: 1,
      bandwidth: 100,
      invert: false,
      visible: true
    },
    {
      id: 3,
      enabled: false,
      label: 'CH3',
      color: '#FF69B4',
      coupling: 'dc',
      verticalScale: 1.0,
      verticalOffset: 0,
      probe: 1,
      bandwidth: 100,
      invert: false,
      visible: false
    },
    {
      id: 4,
      enabled: false,
      label: 'CH4',
      color: '#32CD32',
      coupling: 'dc',
      verticalScale: 1.0,
      verticalOffset: 0,
      probe: 1,
      bandwidth: 100,
      invert: false,
      visible: false
    }
  ],
  timebase: {
    scale: 0.001, // 1ms/div
    position: 50, // 50% (中心)
    mode: 'yt',
    reference: 'center'
  },
  trigger: {
    type: 'edge',
    mode: 'auto',
    source: 1,
    level: 0,
    edge: 'rising',
    holdoff: 0.000001, // 1μs
    position: 10, // 10%
    coupling: 'dc',
    noise: false,
    hf: false
  },
  pulseWidthTrigger: {
    condition: 'greater',
    width: 0.000001, // 1μs
    polarity: 'positive'
  },
  videoTrigger: {
    standard: 'ntsc',
    sync: 'line',
    lineNumber: 1
  },
  acquisition: {
    mode: 'normal',
    sampleRate: 1000000000, // 1GSa/s
    memoryDepth: 1000000, // 1M points
    averageCount: 16,
    interpolation: 'sinx'
  },
  measurements: [],
  cursors: {
    enabled: false,
    type: 'both',
    cursor1: { x: 0, y: 0 },
    cursor2: { x: 0, y: 0 },
    delta: { x: 0, y: 0 },
    frequency: 0
  },
  mathFunctions: [],
  display: {
    persistence: false,
    intensity: 80,
    grid: 'full',
    labels: true,
    vectors: true
  }
}

// 时基选项 (s/div)
export const TIMEBASE_OPTIONS = [
  { value: 0.000000001, label: '1ns/div' },
  { value: 0.000000002, label: '2ns/div' },
  { value: 0.000000005, label: '5ns/div' },
  { value: 0.00000001, label: '10ns/div' },
  { value: 0.00000002, label: '20ns/div' },
  { value: 0.00000005, label: '50ns/div' },
  { value: 0.0000001, label: '100ns/div' },
  { value: 0.0000002, label: '200ns/div' },
  { value: 0.0000005, label: '500ns/div' },
  { value: 0.000001, label: '1μs/div' },
  { value: 0.000002, label: '2μs/div' },
  { value: 0.000005, label: '5μs/div' },
  { value: 0.00001, label: '10μs/div' },
  { value: 0.00002, label: '20μs/div' },
  { value: 0.00005, label: '50μs/div' },
  { value: 0.0001, label: '100μs/div' },
  { value: 0.0002, label: '200μs/div' },
  { value: 0.0005, label: '500μs/div' },
  { value: 0.001, label: '1ms/div' },
  { value: 0.002, label: '2ms/div' },
  { value: 0.005, label: '5ms/div' },
  { value: 0.01, label: '10ms/div' },
  { value: 0.02, label: '20ms/div' },
  { value: 0.05, label: '50ms/div' },
  { value: 0.1, label: '100ms/div' },
  { value: 0.2, label: '200ms/div' },
  { value: 0.5, label: '500ms/div' },
  { value: 1, label: '1s/div' },
  { value: 2, label: '2s/div' },
  { value: 5, label: '5s/div' },
  { value: 10, label: '10s/div' }
]

// 垂直刻度选项 (V/div)
export const VERTICAL_SCALE_OPTIONS = [
  { value: 0.001, label: '1mV/div' },
  { value: 0.002, label: '2mV/div' },
  { value: 0.005, label: '5mV/div' },
  { value: 0.01, label: '10mV/div' },
  { value: 0.02, label: '20mV/div' },
  { value: 0.05, label: '50mV/div' },
  { value: 0.1, label: '100mV/div' },
  { value: 0.2, label: '200mV/div' },
  { value: 0.5, label: '500mV/div' },
  { value: 1, label: '1V/div' },
  { value: 2, label: '2V/div' },
  { value: 5, label: '5V/div' },
  { value: 10, label: '10V/div' },
  { value: 20, label: '20V/div' },
  { value: 50, label: '50V/div' },
  { value: 100, label: '100V/div' }
]

// 探头倍数选项
export const PROBE_OPTIONS = [
  { value: 0.1, label: '0.1x' },
  { value: 1, label: '1x' },
  { value: 10, label: '10x' },
  { value: 100, label: '100x' },
  { value: 1000, label: '1000x' }
]

// 采样率选项 (Sa/s)
export const SAMPLE_RATE_OPTIONS = [
  { value: 1000, label: '1kSa/s' },
  { value: 10000, label: '10kSa/s' },
  { value: 100000, label: '100kSa/s' },
  { value: 1000000, label: '1MSa/s' },
  { value: 10000000, label: '10MSa/s' },
  { value: 100000000, label: '100MSa/s' },
  { value: 1000000000, label: '1GSa/s' },
  { value: 2000000000, label: '2GSa/s' },
  { value: 5000000000, label: '5GSa/s' }
]

// 内存深度选项
export const MEMORY_DEPTH_OPTIONS = [
  { value: 1000, label: '1K' },
  { value: 10000, label: '10K' },
  { value: 100000, label: '100K' },
  { value: 1000000, label: '1M' },
  { value: 10000000, label: '10M' },
  { value: 100000000, label: '100M' },
  { value: 1000000000, label: '1G' }
]

// 测量类型选项
export const MEASUREMENT_TYPES = [
  { value: 'frequency', label: '频率', unit: 'Hz' },
  { value: 'period', label: '周期', unit: 's' },
  { value: 'amplitude', label: '幅度', unit: 'V' },
  { value: 'peak_to_peak', label: '峰峰值', unit: 'V' },
  { value: 'rms', label: 'RMS', unit: 'V' },
  { value: 'mean', label: '平均值', unit: 'V' },
  { value: 'rise_time', label: '上升时间', unit: 's' },
  { value: 'fall_time', label: '下降时间', unit: 's' },
  { value: 'pulse_width', label: '脉宽', unit: 's' },
  { value: 'duty_cycle', label: '占空比', unit: '%' },
  { value: 'overshoot', label: '过冲', unit: '%' },
  { value: 'preshoot', label: '预冲', unit: '%' }
]

// 工具函数
export const formatTimeValue = (value: number): string => {
  if (value >= 1) {
    return `${value.toFixed(3)}s`
  } else if (value >= 0.001) {
    return `${(value * 1000).toFixed(3)}ms`
  } else if (value >= 0.000001) {
    return `${(value * 1000000).toFixed(3)}μs`
  } else {
    return `${(value * 1000000000).toFixed(3)}ns`
  }
}

export const formatVoltageValue = (value: number): string => {
  if (Math.abs(value) >= 1000) {
    return `${(value / 1000).toFixed(3)}kV`
  } else if (Math.abs(value) >= 1) {
    return `${value.toFixed(3)}V`
  } else if (Math.abs(value) >= 0.001) {
    return `${(value * 1000).toFixed(3)}mV`
  } else {
    return `${(value * 1000000).toFixed(3)}μV`
  }
}

export const formatFrequencyValue = (value: number): string => {
  if (value >= 1000000000) {
    return `${(value / 1000000000).toFixed(3)}GHz`
  } else if (value >= 1000000) {
    return `${(value / 1000000).toFixed(3)}MHz`
  } else if (value >= 1000) {
    return `${(value / 1000).toFixed(3)}kHz`
  } else {
    return `${value.toFixed(3)}Hz`
  }
}
