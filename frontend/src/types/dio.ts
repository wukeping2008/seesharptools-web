// DIO/开关卡控件类型定义
export interface DIOPin {
  id: number
  label: string
  direction: 'input' | 'output' | 'bidirectional'
  value: boolean
  enabled: boolean
  inverted: boolean
  pullup: boolean
  pulldown: boolean
  driveStrength: 'low' | 'medium' | 'high'
  slewRate: 'slow' | 'medium' | 'fast'
}

export interface DIOPort {
  id: number
  name: string
  pins: DIOPin[]
  enabled: boolean
  direction: 'input' | 'output' | 'bidirectional'
  value: number // 32位端口值
  mask: number // 端口掩码
}

export interface RelayChannel {
  id: number
  label: string
  state: 'open' | 'closed'
  enabled: boolean
  type: 'spst' | 'spdt' | 'dpdt'
  maxVoltage: number
  maxCurrent: number
  resistance: number
  actuationTime: number
  releaseTime: number
}

export interface SwitchMatrix {
  id: number
  name: string
  rows: number
  columns: number
  channels: RelayChannel[]
  connections: Array<{
    input: number
    output: number
    relay: number
  }>
}

export interface SequenceStep {
  id: number
  name: string
  type: 'set_pin' | 'set_port' | 'delay' | 'wait_pin' | 'relay_control'
  parameters: {
    pin?: number
    port?: number
    value?: boolean | number
    delay?: number
    condition?: 'high' | 'low' | 'rising' | 'falling'
    relay?: number
    state?: 'open' | 'closed'
  }
  enabled: boolean
}

export interface TimingSequence {
  id: string
  name: string
  steps: SequenceStep[]
  repeat: boolean
  repeatCount: number
  enabled: boolean
}

export interface DIOConfig {
  // 基本配置
  deviceName: string
  deviceType: 'dio_card' | 'switch_matrix' | 'relay_card'
  
  // 端口配置
  ports: DIOPort[]
  
  // 继电器配置
  relays: RelayChannel[]
  
  // 开关矩阵配置
  switchMatrix: SwitchMatrix[]
  
  // 时序控制
  sequences: TimingSequence[]
  
  // 全局设置
  globalSettings: {
    updateRate: number // Hz
    debounceTime: number // ms
    defaultDirection: 'input' | 'output'
    powerOnState: 'maintain' | 'reset' | 'custom'
    safetyMode: boolean
    watchdogEnabled: boolean
    watchdogTimeout: number // ms
  }
  
  // 报警设置
  alarms: {
    enabled: boolean
    overcurrent: boolean
    overvoltage: boolean
    shortCircuit: boolean
    openCircuit: boolean
    temperatureHigh: boolean
  }
}

export interface DIOStatus {
  connected: boolean
  deviceReady: boolean
  lastUpdate: string
  errorCode: number
  errorMessage: string
  
  // 状态统计
  statistics: {
    totalOperations: number
    successfulOperations: number
    failedOperations: number
    averageResponseTime: number
    uptime: number
  }
  
  // 硬件状态
  hardware: {
    temperature: number
    voltage: number
    current: number
    powerConsumption: number
  }
  
  // 继电器状态
  relayStatus: {
    totalSwitches: number
    activeSwitches: number
    faultySwitches: number
    averageLifetime: number
  }
}

// 默认配置
export const DEFAULT_DIO_CONFIG: DIOConfig = {
  deviceName: 'DIO-32',
  deviceType: 'dio_card',
  
  ports: [
    {
      id: 0,
      name: 'Port A',
      enabled: true,
      direction: 'output',
      value: 0,
      mask: 0xFFFFFFFF,
      pins: Array.from({ length: 8 }, (_, i) => ({
        id: i,
        label: `PA${i}`,
        direction: 'output' as const,
        value: false,
        enabled: true,
        inverted: false,
        pullup: false,
        pulldown: false,
        driveStrength: 'medium' as const,
        slewRate: 'medium' as const
      }))
    },
    {
      id: 1,
      name: 'Port B',
      enabled: true,
      direction: 'input',
      value: 0,
      mask: 0xFFFFFFFF,
      pins: Array.from({ length: 8 }, (_, i) => ({
        id: i + 8,
        label: `PB${i}`,
        direction: 'input' as const,
        value: false,
        enabled: true,
        inverted: false,
        pullup: true,
        pulldown: false,
        driveStrength: 'medium' as const,
        slewRate: 'medium' as const
      }))
    }
  ],
  
  relays: [],
  switchMatrix: [],
  sequences: [],
  
  globalSettings: {
    updateRate: 100,
    debounceTime: 10,
    defaultDirection: 'input',
    powerOnState: 'reset',
    safetyMode: true,
    watchdogEnabled: false,
    watchdogTimeout: 1000
  },
  
  alarms: {
    enabled: true,
    overcurrent: true,
    overvoltage: true,
    shortCircuit: true,
    openCircuit: false,
    temperatureHigh: true
  }
}

// 工具函数
export const formatPinValue = (value: boolean): string => {
  return value ? 'HIGH' : 'LOW'
}

export const formatPortValue = (value: number, bits: number = 8): string => {
  return `0x${value.toString(16).toUpperCase().padStart(Math.ceil(bits / 4), '0')}`
}

export const formatBinaryValue = (value: number, bits: number = 8): string => {
  return value.toString(2).padStart(bits, '0')
}

export const calculatePortValue = (pins: DIOPin[]): number => {
  let value = 0
  pins.forEach((pin, index) => {
    if (pin.value) {
      value |= (1 << index)
    }
  })
  return value
}

export const setPortValue = (pins: DIOPin[], value: number): void => {
  pins.forEach((pin, index) => {
    pin.value = Boolean(value & (1 << index))
  })
}

export const formatRelayState = (state: 'open' | 'closed'): string => {
  return state === 'closed' ? '闭合' : '断开'
}

export const formatTime = (milliseconds: number): string => {
  if (milliseconds < 1000) {
    return `${milliseconds}ms`
  } else if (milliseconds < 60000) {
    return `${(milliseconds / 1000).toFixed(1)}s`
  } else {
    const minutes = Math.floor(milliseconds / 60000)
    const seconds = Math.floor((milliseconds % 60000) / 1000)
    return `${minutes}m ${seconds}s`
  }
}

// 常量定义
export const DIO_DIRECTIONS = [
  { value: 'input', label: '输入' },
  { value: 'output', label: '输出' },
  { value: 'bidirectional', label: '双向' }
] as const

export const DRIVE_STRENGTHS = [
  { value: 'low', label: '低驱动' },
  { value: 'medium', label: '中等驱动' },
  { value: 'high', label: '高驱动' }
] as const

export const SLEW_RATES = [
  { value: 'slow', label: '慢速' },
  { value: 'medium', label: '中速' },
  { value: 'fast', label: '快速' }
] as const

export const RELAY_TYPES = [
  { value: 'spst', label: 'SPST (单刀单掷)' },
  { value: 'spdt', label: 'SPDT (单刀双掷)' },
  { value: 'dpdt', label: 'DPDT (双刀双掷)' }
] as const

export const SEQUENCE_STEP_TYPES = [
  { value: 'set_pin', label: '设置引脚' },
  { value: 'set_port', label: '设置端口' },
  { value: 'delay', label: '延时' },
  { value: 'wait_pin', label: '等待引脚' },
  { value: 'relay_control', label: '继电器控制' }
] as const

export const PIN_CONDITIONS = [
  { value: 'high', label: '高电平' },
  { value: 'low', label: '低电平' },
  { value: 'rising', label: '上升沿' },
  { value: 'falling', label: '下降沿' }
] as const
