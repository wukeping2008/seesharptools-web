import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001'

/**
 * USB-1601硬件服务
 * 提供统一的前端API接口
 */
export class USB1601Service {
  private baseUrl: string

  constructor(baseUrl?: string) {
    this.baseUrl = baseUrl || `${API_BASE_URL}/api/usb1601`
  }

  /**
   * 获取设备状态
   */
  async getStatus() {
    const response = await axios.get(`${this.baseUrl}/status`)
    return response.data
  }

  /**
   * 初始化设备
   */
  async initialize(deviceId: string = '0') {
    const response = await axios.post(`${this.baseUrl}/initialize`, { deviceId })
    return response.data
  }

  /**
   * 断开设备连接
   */
  async disconnect() {
    const response = await axios.post(`${this.baseUrl}/disconnect`)
    return response.data
  }

  /**
   * 发现所有设备
   */
  async discoverDevices() {
    const response = await axios.get(`${this.baseUrl}/discover`)
    return response.data
  }

  /**
   * 配置AI通道
   */
  async configureAI(channels: number[], range: number = 10) {
    const response = await axios.post(`${this.baseUrl}/ai/configure`, {
      channels,
      range,
      terminalMode: 'Differential'
    })
    return response.data
  }

  /**
   * 单点读取AI
   */
  async readAISingle(channels: number[]) {
    const response = await axios.post(`${this.baseUrl}/ai/read-single`, { channels })
    return response.data
  }

  /**
   * 有限点采集AI
   */
  async readAIFinite(channels: number[], samplesPerChannel: number, sampleRate: number) {
    const response = await axios.post(`${this.baseUrl}/ai/read-finite`, {
      channels,
      samplesPerChannel,
      sampleRate
    })
    return response.data
  }

  /**
   * 配置AO通道
   */
  async configureAO(channels: number[], range: number = 10) {
    const response = await axios.post(`${this.baseUrl}/ao/configure`, {
      channels,
      range
    })
    return response.data
  }

  /**
   * 单点输出AO
   */
  async writeAOSingle(channel: number, value: number) {
    const response = await axios.post(`${this.baseUrl}/ao/write-single`, {
      channel,
      value
    })
    return response.data
  }

  /**
   * 多通道输出AO
   */
  async writeAOMultiple(channels: number[], values: number[]) {
    const response = await axios.post(`${this.baseUrl}/ao/write-multiple`, {
      channels,
      values
    })
    return response.data
  }

  /**
   * 配置DIO端口
   */
  async configureDIO(port: number, direction: 'Input' | 'Output') {
    const response = await axios.post(`${this.baseUrl}/dio/configure`, {
      port,
      direction
    })
    return response.data
  }

  /**
   * 读取DI端口
   */
  async readDIPort(port: number) {
    const response = await axios.get(`${this.baseUrl}/dio/read/${port}`)
    return response.data
  }

  /**
   * 写入DO端口
   */
  async writeDOPort(port: number, value: number) {
    const response = await axios.post(`${this.baseUrl}/dio/write`, {
      port,
      value
    })
    return response.data
  }

  /**
   * 配置计数器
   */
  async configureCounter(counter: number, mode: 'EdgeCounting' | 'PulseWidth' | 'Frequency' | 'Period') {
    const response = await axios.post(`${this.baseUrl}/counter/configure`, {
      counter,
      mode
    })
    return response.data
  }

  /**
   * 读取计数值
   */
  async readCounter(counter: number) {
    const response = await axios.get(`${this.baseUrl}/counter/read/${counter}`)
    return response.data
  }

  /**
   * 重置计数器
   */
  async resetCounter(counter: number) {
    const response = await axios.post(`${this.baseUrl}/counter/reset/${counter}`)
    return response.data
  }
}

// 导出默认实例
export const usb1601Service = new USB1601Service()

// 导出类型定义
export interface DeviceInfo {
  deviceId: string
  deviceName: string
  serialNumber: string
  firmwareVersion: string
  isConnected: boolean
}

export interface DeviceStatus {
  isInitialized: boolean
  isSimulationMode: boolean
  deviceId: string
  deviceInfo?: DeviceInfo
}

export interface AIData {
  success: boolean
  channels: number[]
  data: number[]
  timestamp: string
}

export interface DIOData {
  success: boolean
  port: number
  value: number
  binaryString?: string
}