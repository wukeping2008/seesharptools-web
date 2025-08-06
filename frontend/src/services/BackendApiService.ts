/**
 * 后端API服务
 * 提供与SeeSharpTools后端服务的完整集成
 */

import axios from 'axios'
import type { AxiosInstance, AxiosResponse } from 'axios'

// API响应类型定义
export interface ApiResponse<T = any> {
  success: boolean
  data: T
  message?: string
  error?: string
}

// 设备信息类型
export interface HardwareDevice {
  id: number
  model: string
  serialNumber: string
  status: 'Unknown' | 'Online' | 'Offline' | 'Error' | 'Busy' | 'Ready'
  deviceType: string
  capabilities: string[]
  configuration: Record<string, any>
  lastUpdated: string
}

// 任务配置类型
export interface MISDTaskConfiguration {
  id?: number
  taskName: string
  deviceId: number
  taskType: string
  channels: Array<{
    channelId: number
    rangeLow: number
    rangeHigh: number
    terminal: string
    coupling: string
    enableIEPE: boolean
  }>
  sampling: {
    sampleRate: number
    samplesToAcquire: number
    mode: string
    bufferSize: number
  }
  trigger: {
    type: string
    source: string
    edge: string
    level: number
    preTriggerSamples: number
  }
}

// 任务状态类型
export interface TaskStatus {
  taskId: number
  status: 'Created' | 'Configured' | 'Started' | 'Running' | 'Stopped' | 'Error' | 'Completed'
  progress: number
  samplesAcquired: number
  totalSamples: number
  elapsedTime: number
  errorMessage?: string
}

// 数据读取结果类型
export interface DataReadResult {
  taskId: number
  channelCount: number
  sampleCount: number
  data: number[][]
  timestamps: number[]
  sampleRate: number
}

/**
 * 后端API服务类
 */
export class BackendApiService {
  private api: AxiosInstance
  private baseURL: string

  constructor(baseURL: string = 'http://localhost:5001') {
    this.baseURL = baseURL
    this.api = axios.create({
      baseURL: this.baseURL,
      timeout: 30000,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      }
    })

    // 请求拦截器
    this.api.interceptors.request.use(
      (config: any) => {
        console.log(`[API] ${config.method?.toUpperCase()} ${config.url}`)
        return config
      },
      (error: any) => {
        console.error('[API] Request error:', error)
        return Promise.reject(error)
      }
    )

    // 响应拦截器
    this.api.interceptors.response.use(
      (response: AxiosResponse) => {
        console.log(`[API] Response ${response.status}:`, response.data)
        return response
      },
      (error: any) => {
        console.error('[API] Response error:', error.response?.data || error.message)
        return Promise.reject(error)
      }
    )
  }

  /**
   * 检查后端服务连接状态
   */
  async checkConnection(): Promise<boolean> {
    try {
      const response = await this.api.get('/health')
      return response.status === 200
    } catch (error) {
      console.error('Backend connection failed:', error)
      return false
    }
  }

  // ==================== 设备管理 API ====================

  /**
   * 获取所有设备列表
   */
  async getDevices(): Promise<HardwareDevice[]> {
    try {
      const response = await this.api.get<HardwareDevice[]>('/api/misd/devices')
      return response.data
    } catch (error) {
      console.error('Failed to get devices:', error)
      throw error
    }
  }

  /**
   * 获取指定设备信息
   */
  async getDevice(deviceId: number): Promise<HardwareDevice> {
    try {
      const response = await this.api.get<HardwareDevice>(`/api/misd/devices/${deviceId}`)
      return response.data
    } catch (error) {
      console.error(`Failed to get device ${deviceId}:`, error)
      throw error
    }
  }

  /**
   * 获取设备支持的功能
   */
  async getDeviceFunctions(deviceId: number): Promise<string[]> {
    try {
      const response = await this.api.get<string[]>(`/api/misd/devices/${deviceId}/functions`)
      return response.data
    } catch (error) {
      console.error(`Failed to get device ${deviceId} functions:`, error)
      throw error
    }
  }

  /**
   * 刷新设备发现
   */
  async refreshDevices(): Promise<HardwareDevice[]> {
    try {
      const response = await this.api.post<HardwareDevice[]>('/api/misd/devices/refresh')
      return response.data
    } catch (error) {
      console.error('Failed to refresh devices:', error)
      throw error
    }
  }

  // ==================== 任务管理 API ====================

  /**
   * 创建新任务
   */
  async createTask(config: MISDTaskConfiguration): Promise<number> {
    try {
      const response = await this.api.post<MISDTaskConfiguration>('/api/misd/tasks', config)
      return response.data.id || 0
    } catch (error) {
      console.error('Failed to create task:', error)
      throw error
    }
  }

  /**
   * 启动任务
   */
  async startTask(taskId: number): Promise<void> {
    try {
      await this.api.post(`/api/misd/tasks/${taskId}/start`)
    } catch (error) {
      console.error(`Failed to start task ${taskId}:`, error)
      throw error
    }
  }

  /**
   * 停止任务
   */
  async stopTask(taskId: number): Promise<void> {
    try {
      await this.api.post(`/api/misd/tasks/${taskId}/stop`)
    } catch (error) {
      console.error(`Failed to stop task ${taskId}:`, error)
      throw error
    }
  }

  /**
   * 获取任务状态
   */
  async getTaskStatus(taskId: number): Promise<TaskStatus> {
    try {
      const response = await this.api.get<TaskStatus>(`/api/misd/tasks/${taskId}/status`)
      return response.data
    } catch (error) {
      console.error(`Failed to get task ${taskId} status:`, error)
      throw error
    }
  }

  /**
   * 删除任务
   */
  async deleteTask(taskId: number): Promise<void> {
    try {
      await this.api.delete(`/api/misd/tasks/${taskId}`)
    } catch (error) {
      console.error(`Failed to delete task ${taskId}:`, error)
      throw error
    }
  }

  // ==================== 数据操作 API ====================

  /**
   * 读取数据
   */
  async readData(taskId: number, samplesToRead: number = 1000, timeout: number = 10000): Promise<DataReadResult> {
    try {
      const response = await this.api.get<DataReadResult>(
        `/api/misd/tasks/${taskId}/data`,
        {
          params: { samplesToRead, timeout },
          timeout: timeout + 5000 // 给额外的网络超时时间
        }
      )
      return response.data
    } catch (error) {
      console.error(`Failed to read data from task ${taskId}:`, error)
      throw error
    }
  }

  /**
   * 写入数据
   */
  async writeData(taskId: number, data: number[][], timeout: number = 10000): Promise<void> {
    try {
      await this.api.post(
        `/api/misd/tasks/${taskId}/data`,
        { data, timeout },
        { timeout: timeout + 5000 }
      )
    } catch (error) {
      console.error(`Failed to write data to task ${taskId}:`, error)
      throw error
    }
  }

  /**
   * 获取可用样本数
   */
  async getAvailableSamples(taskId: number): Promise<number> {
    try {
      const response = await this.api.get<{ availableSamples: number }>(`/api/misd/tasks/${taskId}/available-samples`)
      return response.data.availableSamples
    } catch (error) {
      console.error(`Failed to get available samples for task ${taskId}:`, error)
      throw error
    }
  }

  /**
   * 发送软件触发
   */
  async sendSoftwareTrigger(taskId: number): Promise<void> {
    try {
      await this.api.post(`/api/misd/tasks/${taskId}/trigger`)
    } catch (error) {
      console.error(`Failed to send software trigger to task ${taskId}:`, error)
      throw error
    }
  }

  /**
   * 等待任务完成
   */
  async waitUntilDone(taskId: number, timeout: number = 30000): Promise<void> {
    try {
      await this.api.post(
        `/api/misd/tasks/${taskId}/wait`,
        { timeout },
        { timeout: timeout + 5000 }
      )
    } catch (error) {
      console.error(`Failed to wait for task ${taskId}:`, error)
      throw error
    }
  }

  // ==================== 工具方法 ====================

  /**
   * 获取API基础URL
   */
  getBaseURL(): string {
    return this.baseURL
  }

  /**
   * 设置API基础URL
   */
  setBaseURL(baseURL: string): void {
    this.baseURL = baseURL
    this.api.defaults.baseURL = baseURL
  }

  /**
   * 获取Swagger文档URL
   */
  getSwaggerURL(): string {
    return `${this.baseURL}/swagger`
  }

  /**
   * 通用POST请求方法
   */
  async post<T = any>(url: string, data?: any): Promise<T> {
    const response = await this.api.post<T>(url, data)
    return response.data
  }

  /**
   * 通用GET请求方法
   */
  async get<T = any>(url: string, params?: any): Promise<T> {
    const response = await this.api.get<T>(url, { params })
    return response.data
  }
}

// 创建默认实例
export const backendApi = new BackendApiService()
