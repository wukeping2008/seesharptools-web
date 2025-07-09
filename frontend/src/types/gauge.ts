// 仪表控件基础类型定义

// 仪表盘数据类型
export interface GaugeData {
  value: number
  min?: number
  max?: number
  unit?: string
  title?: string
}

// 仪表盘配置选项
export interface GaugeOptions {
  size: number
  startAngle: number
  endAngle: number
  clockwise: boolean
  showPointer: boolean
  showProgress: boolean
  showAxis: boolean
  showAxisLabel: boolean
  showTitle: boolean
  showDetail: boolean
  animation: boolean
  animationDuration: number
  theme: 'light' | 'dark'
}

// 仪表盘样式配置
export interface GaugeStyle {
  backgroundColor?: string
  borderColor?: string
  borderWidth?: number
  radius?: string | number
  center?: [string | number, string | number]
}

// 指针配置
export interface PointerConfig {
  show: boolean
  length: string | number
  width: number
  color: string
  shadowColor?: string
  shadowBlur?: number
  shadowOffsetX?: number
  shadowOffsetY?: number
}

// 进度条配置
export interface ProgressConfig {
  show: boolean
  width: number
  color: string | string[]
  backgroundColor?: string
  roundCap?: boolean
}

// 刻度配置
export interface AxisConfig {
  show: boolean
  min: number
  max: number
  splitNumber: number
  lineStyle: {
    color: string
    width: number
    type: 'solid' | 'dashed' | 'dotted'
  }
  label: {
    show: boolean
    distance: number
    color: string
    fontSize: number
    formatter?: string | ((value: number) => string)
  }
  tick: {
    show: boolean
    length: number
    width: number
    color: string
  }
  minorTick: {
    show: boolean
    length: number
    width: number
    color: string
    splitNumber: number
  }
}

// 标题配置
export interface TitleConfig {
  show: boolean
  text: string
  offsetCenter: [string | number, string | number]
  color: string
  fontSize: number
  fontWeight: string | number
  fontFamily: string
}

// 详情配置
export interface DetailConfig {
  show: boolean
  offsetCenter: [string | number, string | number]
  color: string
  fontSize: number
  fontWeight: string | number
  fontFamily: string
  formatter?: string | ((value: number) => string)
  backgroundColor?: string
  borderColor?: string
  borderWidth?: number
  borderRadius?: number
  padding?: number[]
}

// 报警区域配置
export interface AlarmZone {
  min: number
  max: number
  color: string
  label?: string
}

// 线性仪表配置
export interface LinearGaugeOptions {
  orientation: 'horizontal' | 'vertical'
  width: number
  height: number
  showScale: boolean
  showPointer: boolean
  showProgress: boolean
  showLabel: boolean
  animation: boolean
  theme: 'light' | 'dark'
}

// 温度计配置
export interface ThermometerOptions {
  height: number
  width: number
  bulbRadius: number
  tubeWidth: number
  showScale: boolean
  showLabel: boolean
  liquidColor: string
  tubeColor: string
  backgroundColor: string
  animation: boolean
  animationDuration: number
}

// 安全区域配置
export interface SafetyZone {
  min: number
  max: number
  color: string
  label?: string
}

// 压力表配置
export interface PressureGaugeOptions {
  size: number
  startAngle: number
  endAngle: -45
  showScale: boolean
  showPointer: boolean
  showValue: boolean
  showMinMax: boolean
  showSafetyZones: boolean
  showPressureType: boolean
  pressureType: string
  pointerColor: string
  dialColor: string
  scaleColor: string
  animation: boolean
  animationDuration: number
}

// 仪表事件
export interface GaugeEvents {
  onValueChange?: (value: number) => void
  onAlarmTrigger?: (zone: AlarmZone, value: number) => void
  onPointerClick?: (value: number) => void
  onScaleClick?: (value: number) => void
}

// 仪表控件基础接口
export interface BaseGaugeProps {
  data: GaugeData
  options?: Partial<GaugeOptions>
  style?: GaugeStyle
  pointer?: Partial<PointerConfig>
  progress?: Partial<ProgressConfig>
  axis?: Partial<AxisConfig>
  title?: Partial<TitleConfig>
  detail?: Partial<DetailConfig>
  alarmZones?: AlarmZone[]
  events?: GaugeEvents
  width?: string | number
  height?: string | number
}

// 历史数据记录
export interface GaugeHistoryData {
  timestamp: Date
  value: number
  min?: number
  max?: number
}

// 仪表数据源配置
export interface GaugeDataSource {
  type: 'static' | 'realtime' | 'api'
  url?: string
  updateInterval?: number
  dataPath?: string
  transform?: (data: any) => GaugeData
}
