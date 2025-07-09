// 测试测量仪器控件类型定义

export interface InstrumentGaugeProps {
  value: number
  min?: number
  max?: number
  unit?: string
  title?: string
  precision?: number
  size?: number
  showPointer?: boolean
  showDigitalDisplay?: boolean
  showThresholds?: boolean
  animated?: boolean
  thresholds?: ThresholdConfig[]
  colors?: GaugeColors
}

export interface ThresholdConfig {
  value: number
  color: string
  label?: string
  type?: 'warning' | 'error' | 'info'
}

export interface GaugeColors {
  background?: string
  track?: string
  progress?: string
  pointer?: string
  pointerCenter?: string
  majorTick?: string
  minorTick?: string
  label?: string
  title?: string
  unit?: string
  digitalValue?: string
  digitalUnit?: string
  digitalBackground?: string
}

export interface DigitalDisplayProps {
  value: number
  unit?: string
  precision?: number
  scientific?: boolean
  engineering?: boolean
  size?: 'small' | 'medium' | 'large'
  colors?: {
    value?: string
    unit?: string
    background?: string
    border?: string
  }
}

export interface LEDIndicatorProps {
  status: 'normal' | 'warning' | 'error' | 'info' | 'measuring'
  label?: string
  size?: 'small' | 'medium' | 'large'
  blinking?: boolean
  colors?: {
    normal?: string
    warning?: string
    error?: string
    info?: string
    measuring?: string
  }
}

export interface PrecisionKnobProps {
  value: number
  min?: number
  max?: number
  step?: number
  precision?: number
  size?: number
  showValue?: boolean
  showTicks?: boolean
  disabled?: boolean
  colors?: {
    knob?: string
    track?: string
    tick?: string
    value?: string
  }
}

export interface OscilloscopeProps {
  channels: ChannelData[]
  timebase: number
  triggerLevel?: number
  triggerChannel?: number
  running?: boolean
  width?: number
  height?: number
  gridColor?: string
  backgroundColor?: string
}

export interface ChannelData {
  id: string
  name: string
  data: number[]
  color: string
  enabled: boolean
  scale: number
  offset: number
}

export interface SpectrumAnalyzerProps {
  data: SpectrumData[]
  frequencyRange: [number, number]
  amplitudeRange: [number, number]
  markers?: MarkerConfig[]
  referenceLevel?: number
  width?: number
  height?: number
}

export interface SpectrumData {
  frequency: number
  amplitude: number
}

export interface MarkerConfig {
  id: string
  frequency: number
  amplitude: number
  label?: string
  color?: string
}

// 测量单位枚举
export enum MeasurementUnit {
  // 电学单位
  VOLT = 'V',
  MILLIVOLT = 'mV',
  MICROVOLT = 'μV',
  AMPERE = 'A',
  MILLIAMPERE = 'mA',
  MICROAMPERE = 'μA',
  OHM = 'Ω',
  KILOOHM = 'kΩ',
  MEGAOHM = 'MΩ',
  WATT = 'W',
  MILLIWATT = 'mW',
  
  // 频率单位
  HERTZ = 'Hz',
  KILOHERTZ = 'kHz',
  MEGAHERTZ = 'MHz',
  GIGAHERTZ = 'GHz',
  
  // 时间单位
  SECOND = 's',
  MILLISECOND = 'ms',
  MICROSECOND = 'μs',
  NANOSECOND = 'ns',
  
  // 温度单位
  CELSIUS = '°C',
  FAHRENHEIT = '°F',
  KELVIN = 'K',
  
  // 压力单位
  PASCAL = 'Pa',
  KILOPASCAL = 'kPa',
  BAR = 'bar',
  PSI = 'psi',
  
  // 分贝单位
  DECIBEL = 'dB',
  DBM = 'dBm',
  DBV = 'dBV',
  
  // 百分比
  PERCENT = '%',
  
  // 无单位
  NONE = ''
}

// 数值格式化选项
export interface NumberFormatOptions {
  precision: number
  scientific: boolean
  engineering: boolean
  unit: MeasurementUnit
  prefix?: string
  suffix?: string
}

// 仪器状态
export enum InstrumentStatus {
  DISCONNECTED = 'disconnected',
  CONNECTING = 'connecting',
  CONNECTED = 'connected',
  MEASURING = 'measuring',
  ERROR = 'error',
  CALIBRATING = 'calibrating'
}

// 事件回调类型
export interface InstrumentEvents {
  onValueChange?: (value: number) => void
  onThresholdExceeded?: (threshold: ThresholdConfig, value: number) => void
  onStatusChange?: (status: InstrumentStatus) => void
  onError?: (error: Error) => void
}
