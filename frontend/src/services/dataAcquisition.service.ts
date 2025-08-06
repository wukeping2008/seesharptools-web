import axios from 'axios'

// 数据采集配置接口 - 匹配后端AcquisitionConfiguration
export interface AcquisitionConfiguration {
  deviceId: number
  sampleRate: number
  channels: ChannelConfiguration[]
  mode: 'Continuous' | 'Finite' | 'Triggered'
  samplesToAcquire?: number
  triggerConfig?: TriggerConfiguration
  bufferSize: number
  threadPriority?: 'Lowest' | 'BelowNormal' | 'Normal' | 'AboveNormal' | 'Highest'
  enableCompression: boolean
  enableQualityCheck: boolean
}

export interface ChannelConfiguration {
  channelId: number
  name: string
  enabled: boolean
  rangeMin: number
  rangeMax: number
  coupling: 'DC' | 'AC' | 'Ground'
  impedance: 'HighZ' | 'Ohm50' | 'Ohm75'
  calibrationOffset?: number
  calibrationGain?: number
}

export interface TriggerConfiguration {
  type: 'Immediate' | 'Edge' | 'Level' | 'PulseWidth' | 'External'
  sourceChannel: number
  level: number
  slope: 'Rising' | 'Falling' | 'Both'
  preTriggerSamples: number
  delay: number
  timeout: number
}

// 任务状态接口
export interface TaskStatus {
  taskId: number
  status: 'Running' | 'Stopped' | 'Paused' | 'Error'
  createdAt: string
  startedAt?: string
  stoppedAt?: string
  samplesAcquired: number
  errorMessage?: string
  configuration?: AcquisitionConfiguration
}

// 性能统计接口
export interface PerformanceStats {
  taskId: number
  averageSampleRate: number
  actualSampleRate: number
  dataThroughput: number
  cpuUsage: number
  memoryUsage: number
  packetLossRate: number
  averageLatency: number
  maxLatency: number
  threadCount: number
  statisticsWindow: number
  lastUpdated: string
}

// 缓冲区状态接口
export interface BufferStatus {
  bufferSize: number
  availableSamples: number
  transferredSamples: number
  isOverflow: boolean
  usagePercentage: number
  overflowCount: number
  lastUpdated: string
}

// 数据质量报告接口
export interface DataQualityReport {
  taskId: number
  totalSamples: number
  validSamples: number
  crcErrors: number
  timestampErrors: number
  packetLoss: number
  rangeErrors: number
  qualityScore: number
  generatedAt: string
  errorDetails: string[]
}

// API基础URL
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001'

class DataAcquisitionService {
  private apiClient = axios.create({
    baseURL: `${API_BASE_URL}/api/dataacquisition`,
    timeout: 30000,
    headers: {
      'Content-Type': 'application/json'
    }
  })

  constructor() {
    // 请求拦截器
    this.apiClient.interceptors.request.use(
      (config) => {
        console.log(`[DataAcquisition] API请求: ${config.method?.toUpperCase()} ${config.url}`, config.data)
        return config
      },
      (error) => {
        console.error('[DataAcquisition] 请求错误:', error)
        return Promise.reject(error)
      }
    )

    // 响应拦截器
    this.apiClient.interceptors.response.use(
      (response) => {
        console.log(`[DataAcquisition] API响应: ${response.status}`, response.data)
        return response
      },
      (error) => {
        console.error('[DataAcquisition] 响应错误:', error.response?.data || error.message)
        return Promise.reject(error)
      }
    )
  }

  /**
   * 启动数据采集任务
   */
  async startAcquisition(taskId: number, config: AcquisitionConfiguration): Promise<any> {
    try {
      const response = await this.apiClient.post('/start', {
        taskId,
        configuration: config
      })
      return response.data
    } catch (error) {
      console.error('启动数据采集失败:', error)
      throw error
    }
  }

  /**
   * 停止数据采集任务
   */
  async stopAcquisition(taskId: number): Promise<any> {
    try {
      const response = await this.apiClient.post(`/stop/${taskId}`)
      return response.data
    } catch (error) {
      console.error('停止数据采集失败:', error)
      throw error
    }
  }

  /**
   * 暂停数据采集任务
   */
  async pauseAcquisition(taskId: number): Promise<any> {
    try {
      const response = await this.apiClient.post(`/pause/${taskId}`)
      return response.data
    } catch (error) {
      console.error('暂停数据采集失败:', error)
      throw error
    }
  }

  /**
   * 恢复数据采集任务
   */
  async resumeAcquisition(taskId: number): Promise<any> {
    try {
      const response = await this.apiClient.post(`/resume/${taskId}`)
      return response.data
    } catch (error) {
      console.error('恢复数据采集失败:', error)
      throw error
    }
  }

  /**
   * 获取任务状态
   */
  async getTaskStatus(taskId: number): Promise<TaskStatus> {
    try {
      const response = await this.apiClient.get(`/status/${taskId}`)
      return response.data
    } catch (error) {
      console.error('获取任务状态失败:', error)
      throw error
    }
  }

  /**
   * 获取性能统计
   */
  async getPerformanceStats(taskId: number): Promise<PerformanceStats> {
    try {
      const response = await this.apiClient.get(`/performance/${taskId}`)
      return response.data
    } catch (error) {
      console.error('获取性能统计失败:', error)
      throw error
    }
  }

  /**
   * 获取缓冲区状态
   */
  async getBufferStatus(taskId: number): Promise<BufferStatus> {
    try {
      const response = await this.apiClient.get(`/buffer-status/${taskId}`)
      return response.data
    } catch (error) {
      console.error('获取缓冲区状态失败:', error)
      throw error
    }
  }

  /**
   * 获取数据质量报告
   */
  async getDataQualityReport(taskId: number): Promise<DataQualityReport> {
    try {
      const response = await this.apiClient.get(`/quality-report/${taskId}`)
      return response.data
    } catch (error) {
      console.error('获取数据质量报告失败:', error)
      throw error
    }
  }

  /**
   * 获取活跃任务列表
   */
  async getActiveTasks(): Promise<number[]> {
    try {
      const response = await this.apiClient.get('/active-tasks')
      return response.data.tasks || []
    } catch (error) {
      console.error('获取活跃任务失败:', error)
      // 返回空数组作为fallback
      return []
    }
  }

  /**
   * 清空缓冲区
   */
  async clearBuffer(taskId: number): Promise<any> {
    try {
      const response = await this.apiClient.post(`/clear-buffer/${taskId}`)
      return response.data
    } catch (error) {
      console.error('清空缓冲区失败:', error)
      throw error
    }
  }

  /**
   * 配置缓冲区
   */
  async configureBuffer(taskId: number, bufferConfig: any): Promise<any> {
    try {
      const response = await this.apiClient.post(`/configure-buffer/${taskId}`, bufferConfig)
      return response.data
    } catch (error) {
      console.error('配置缓冲区失败:', error)
      throw error
    }
  }

  /**
   * 设置数据质量检查
   */
  async setDataQualityCheck(taskId: number, qualityConfig: any): Promise<any> {
    try {
      const response = await this.apiClient.post(`/quality-check/${taskId}`, qualityConfig)
      return response.data
    } catch (error) {
      console.error('设置数据质量检查失败:', error)
      throw error
    }
  }

  /**
   * 获取实时数据流URL (Server-Sent Events)
   */
  getDataStreamUrl(taskId: number): string {
    return `${API_BASE_URL}/api/dataacquisition/stream/${taskId}`
  }

  /**
   * 创建EventSource连接用于实时数据
   */
  createDataStream(taskId: number, onData: (data: any) => void, onError?: (error: any) => void): EventSource {
    const eventSource = new EventSource(this.getDataStreamUrl(taskId))
    
    eventSource.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data)
        onData(data)
      } catch (error) {
        console.error('解析数据流消息失败:', error)
        if (onError) onError(error)
      }
    }

    eventSource.onerror = (error) => {
      console.error('数据流连接错误:', error)
      if (onError) onError(error)
    }

    return eventSource
  }
}

// 导出单例实例
export const dataAcquisitionService = new DataAcquisitionService()
export default dataAcquisitionService
