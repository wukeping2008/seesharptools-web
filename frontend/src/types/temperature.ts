// 热电偶类型
export type ThermocoupleType = 'K' | 'J' | 'T' | 'E' | 'R' | 'S' | 'B' | 'N'

// 温度单位
export type TemperatureUnit = 'C' | 'F' | 'K'

// 温度通道配置
export interface TemperatureChannelConfig {
  id: number
  name: string
  enabled: boolean
  thermocoupleType: ThermocoupleType
  unit: TemperatureUnit
  range: {
    min: number
    max: number
  }
  offset: number // 温度偏移校准
  gain: number // 增益校准
  coldJunctionCompensation: boolean // 冷端补偿
  linearization: boolean // 线性化处理
  alarmEnabled: boolean
  alarmLow: number
  alarmHigh: number
  color: string
}

// 温度数据
export interface TemperatureData {
  channelId: number
  value: number
  unit: TemperatureUnit
  timestamp: number
  quality: 'good' | 'bad' | 'uncertain'
  alarmStatus: 'normal' | 'low' | 'high'
}

// 温度采集卡配置
export interface TemperatureAcquisitionConfig {
  channels: TemperatureChannelConfig[]
  sampleRate: number // 采样率 (Hz)
  averagingCount: number // 平均次数
  autoRange: boolean
  coldJunctionSource: 'internal' | 'external' | 'fixed'
  coldJunctionTemperature: number // 外部冷端温度
  filterEnabled: boolean
  filterCutoff: number // 滤波器截止频率
}

// 温度统计信息
export interface TemperatureStatistics {
  channelId: number
  current: number
  min: number
  max: number
  average: number
  standardDeviation: number
  trend: 'rising' | 'falling' | 'stable'
  trendRate: number // 变化率 (°C/min)
}

// 温度校准数据
export interface TemperatureCalibration {
  channelId: number
  points: Array<{
    reference: number // 参考温度
    measured: number // 测量温度
  }>
  coefficients: {
    offset: number
    gain: number
    quadratic?: number // 二次项系数
  }
  accuracy: number // 校准精度
  lastCalibrated: Date
}

// 温度报警配置
export interface TemperatureAlarmConfig {
  channelId: number
  enabled: boolean
  lowThreshold: number
  highThreshold: number
  hysteresis: number // 迟滞
  delay: number // 报警延时 (秒)
  action: 'log' | 'email' | 'sound' | 'relay'
  message: string
}

// 热电偶特性参数
export interface ThermocoupleCharacteristics {
  type: ThermocoupleType
  name: string
  temperatureRange: {
    min: number
    max: number
  }
  sensitivity: number // 灵敏度 (μV/°C)
  accuracy: number // 精度 (°C)
  color: {
    positive: string
    negative: string
  }
  polynomialCoefficients: number[] // 多项式系数
}

// 温度趋势数据
export interface TemperatureTrend {
  channelId: number
  data: Array<{
    timestamp: number
    value: number
  }>
  timeRange: number // 时间范围 (分钟)
  resolution: number // 分辨率 (秒)
}

// 温度采集卡事件
export interface TemperatureEvents {
  onDataUpdate?: (data: TemperatureData[]) => void
  onAlarm?: (channelId: number, type: 'low' | 'high', value: number) => void
  onChannelConfigChange?: (channelId: number, config: TemperatureChannelConfig) => void
  onCalibrationComplete?: (channelId: number, calibration: TemperatureCalibration) => void
  onError?: (error: string) => void
}

// 导出选项
export interface TemperatureExportOptions {
  format: 'csv' | 'xlsx' | 'json'
  channels: number[]
  timeRange: {
    start: Date
    end: Date
  }
  includeStatistics: boolean
  includeAlarms: boolean
  filename?: string
}

// 热电偶类型常量
export const THERMOCOUPLE_TYPES: Record<ThermocoupleType, ThermocoupleCharacteristics> = {
  K: {
    type: 'K',
    name: 'Type K (Chromel-Alumel)',
    temperatureRange: { min: -270, max: 1372 },
    sensitivity: 41,
    accuracy: 2.2,
    color: { positive: 'yellow', negative: 'red' },
    polynomialCoefficients: [0, 0.039450128, 2.3622373e-5, -3.2858906e-7, 2.0618243e-9, -6.0480262e-12]
  },
  J: {
    type: 'J',
    name: 'Type J (Iron-Constantan)',
    temperatureRange: { min: -210, max: 1200 },
    sensitivity: 51,
    accuracy: 2.2,
    color: { positive: 'white', negative: 'red' },
    polynomialCoefficients: [0, 0.050381187, 3.0475836e-5, -8.5681065e-8, 1.3228195e-10, -1.7052958e-13]
  },
  T: {
    type: 'T',
    name: 'Type T (Copper-Constantan)',
    temperatureRange: { min: -270, max: 400 },
    sensitivity: 43,
    accuracy: 1.0,
    color: { positive: 'blue', negative: 'red' },
    polynomialCoefficients: [0, 0.038748106, 4.4194434e-5, 1.1844323e-7, 2.0016723e-9, 9.0138019e-12]
  },
  E: {
    type: 'E',
    name: 'Type E (Chromel-Constantan)',
    temperatureRange: { min: -270, max: 1000 },
    sensitivity: 68,
    accuracy: 1.7,
    color: { positive: 'purple', negative: 'red' },
    polynomialCoefficients: [0, 0.058665508, 4.5410977e-5, -7.7998048e-7, 4.2783036e-9, -1.1916761e-11]
  },
  R: {
    type: 'R',
    name: 'Type R (Platinum-Rhodium 13%)',
    temperatureRange: { min: -50, max: 1768 },
    sensitivity: 10,
    accuracy: 1.5,
    color: { positive: 'black', negative: 'red' },
    polynomialCoefficients: [0, 0.005289617, 1.3916881e-5, -2.3889860e-8, 3.5690228e-11, -4.6244019e-14]
  },
  S: {
    type: 'S',
    name: 'Type S (Platinum-Rhodium 10%)',
    temperatureRange: { min: -50, max: 1768 },
    sensitivity: 10,
    accuracy: 1.5,
    color: { positive: 'black', negative: 'red' },
    polynomialCoefficients: [0, 0.005403133, 1.2593428e-5, -2.3288906e-8, 3.2275479e-11, -3.3149086e-14]
  },
  B: {
    type: 'B',
    name: 'Type B (Platinum-Rhodium 30%/6%)',
    temperatureRange: { min: 0, max: 1820 },
    sensitivity: 10,
    accuracy: 0.5,
    color: { positive: 'gray', negative: 'red' },
    polynomialCoefficients: [0, -0.000246508, 5.9040421e-6, -1.3257931e-8, 1.5668291e-11, -1.6944529e-14]
  },
  N: {
    type: 'N',
    name: 'Type N (Nicrosil-Nisil)',
    temperatureRange: { min: -270, max: 1300 },
    sensitivity: 39,
    accuracy: 2.2,
    color: { positive: 'orange', negative: 'red' },
    polynomialCoefficients: [0, 0.038819346, 1.1012855e-5, 5.2229312e-8, 7.2060525e-11, 5.8488586e-14]
  }
}

// 温度单位转换函数
export const convertTemperature = (value: number, from: TemperatureUnit, to: TemperatureUnit): number => {
  if (from === to) return value
  
  // 先转换为摄氏度
  let celsius = value
  if (from === 'F') {
    celsius = (value - 32) * 5 / 9
  } else if (from === 'K') {
    celsius = value - 273.15
  }
  
  // 再转换为目标单位
  if (to === 'F') {
    return celsius * 9 / 5 + 32
  } else if (to === 'K') {
    return celsius + 273.15
  }
  
  return celsius
}

// 热电偶温度计算函数
export const calculateThermocoupleTemperature = (
  voltage: number, // 热电偶电压 (mV)
  type: ThermocoupleType,
  coldJunctionTemp: number = 25 // 冷端温度 (°C)
): number => {
  const characteristics = THERMOCOUPLE_TYPES[type]
  const coefficients = characteristics.polynomialCoefficients
  
  // 使用多项式逆函数计算温度
  // 这是一个简化的实现，实际应用中需要更精确的算法
  let temperature = 0
  for (let i = 0; i < coefficients.length; i++) {
    temperature += coefficients[i] * Math.pow(voltage, i)
  }
  
  // 加上冷端补偿
  return temperature + coldJunctionTemp
}
