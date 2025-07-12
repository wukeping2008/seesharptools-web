/**
 * SignalR实时通信服务
 * 提供与后端的WebSocket实时数据传输
 */

import * as signalR from '@microsoft/signalr'

// 实时数据类型定义
export interface RealTimeData {
  taskId: number
  timestamp: number
  channelData: number[][]
  sampleRate: number
  totalSamples: number
}

// 设备状态更新类型
export interface DeviceStatusUpdate {
  deviceId: number
  status: string
  timestamp: number
  message?: string
}

// 任务状态更新类型
export interface TaskStatusUpdate {
  taskId: number
  status: string
  progress: number
  timestamp: number
  message?: string
}

// 事件回调类型
export type DataUpdateCallback = (data: RealTimeData) => void
export type DeviceStatusCallback = (update: DeviceStatusUpdate) => void
export type TaskStatusCallback = (update: TaskStatusUpdate) => void
export type ConnectionCallback = (connected: boolean) => void

/**
 * SignalR实时通信服务类
 */
export class SignalRService {
  private connection: signalR.HubConnection | null = null
  private baseURL: string
  private isConnected: boolean = false
  private reconnectAttempts: number = 0
  private maxReconnectAttempts: number = 5
  private reconnectInterval: number = 5000

  // 事件回调
  private dataUpdateCallbacks: Set<DataUpdateCallback> = new Set()
  private deviceStatusCallbacks: Set<DeviceStatusCallback> = new Set()
  private taskStatusCallbacks: Set<TaskStatusCallback> = new Set()
  private connectionCallbacks: Set<ConnectionCallback> = new Set()

  constructor(baseURL: string = 'http://localhost:5001') {
    this.baseURL = baseURL
  }

  /**
   * 建立SignalR连接
   */
  async connect(): Promise<boolean> {
    try {
      if (this.connection && this.isConnected) {
        console.log('[SignalR] Already connected')
        return true
      }

      console.log('[SignalR] Connecting to:', `${this.baseURL}/hubs/datastream`)

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${this.baseURL}/hubs/datastream`, {
          skipNegotiation: true,
          transport: signalR.HttpTransportType.WebSockets
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext: any) => {
            if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
              return this.reconnectInterval
            }
            return null // 停止重连
          }
        })
        .configureLogging(signalR.LogLevel.Information)
        .build()

      // 设置事件处理器
      this.setupEventHandlers()

      // 启动连接
      await this.connection.start()
      this.isConnected = true
      this.reconnectAttempts = 0

      console.log('[SignalR] Connected successfully')
      this.notifyConnectionCallbacks(true)

      return true
    } catch (error) {
      console.error('[SignalR] Connection failed:', error)
      this.isConnected = false
      this.notifyConnectionCallbacks(false)
      return false
    }
  }

  /**
   * 断开SignalR连接
   */
  async disconnect(): Promise<void> {
    try {
      if (this.connection) {
        await this.connection.stop()
        this.connection = null
      }
      this.isConnected = false
      console.log('[SignalR] Disconnected')
      this.notifyConnectionCallbacks(false)
    } catch (error) {
      console.error('[SignalR] Disconnect error:', error)
    }
  }

  /**
   * 设置事件处理器
   */
  private setupEventHandlers(): void {
    if (!this.connection) return

    // 连接状态事件
    this.connection.onclose((error: any) => {
      console.log('[SignalR] Connection closed:', error)
      this.isConnected = false
      this.notifyConnectionCallbacks(false)
    })

    this.connection.onreconnecting((error: any) => {
      console.log('[SignalR] Reconnecting:', error)
      this.isConnected = false
      this.notifyConnectionCallbacks(false)
    })

    this.connection.onreconnected((connectionId: any) => {
      console.log('[SignalR] Reconnected:', connectionId)
      this.isConnected = true
      this.reconnectAttempts = 0
      this.notifyConnectionCallbacks(true)
    })

    // 数据更新事件
    this.connection.on('DataUpdate', (data: RealTimeData) => {
      console.log('[SignalR] Data update received:', data)
      this.notifyDataUpdateCallbacks(data)
    })

    // 设备状态更新事件
    this.connection.on('DeviceStatusUpdate', (update: DeviceStatusUpdate) => {
      console.log('[SignalR] Device status update:', update)
      this.notifyDeviceStatusCallbacks(update)
    })

    // 任务状态更新事件
    this.connection.on('TaskStatusUpdate', (update: TaskStatusUpdate) => {
      console.log('[SignalR] Task status update:', update)
      this.notifyTaskStatusCallbacks(update)
    })

    // 错误事件
    this.connection.on('Error', (error: string) => {
      console.error('[SignalR] Server error:', error)
    })
  }

  /**
   * 加入数据组（订阅特定任务的数据）
   */
  async joinDataGroup(taskId: number): Promise<boolean> {
    try {
      if (!this.connection || !this.isConnected) {
        console.error('[SignalR] Not connected')
        return false
      }

      await this.connection.invoke('JoinGroup', `task_${taskId}`)
      console.log(`[SignalR] Joined data group for task ${taskId}`)
      return true
    } catch (error) {
      console.error(`[SignalR] Failed to join data group for task ${taskId}:`, error)
      return false
    }
  }

  /**
   * 离开数据组（取消订阅特定任务的数据）
   */
  async leaveDataGroup(taskId: number): Promise<boolean> {
    try {
      if (!this.connection || !this.isConnected) {
        console.error('[SignalR] Not connected')
        return false
      }

      await this.connection.invoke('LeaveGroup', `task_${taskId}`)
      console.log(`[SignalR] Left data group for task ${taskId}`)
      return true
    } catch (error) {
      console.error(`[SignalR] Failed to leave data group for task ${taskId}:`, error)
      return false
    }
  }

  /**
   * 加入设备组（订阅特定设备的状态）
   */
  async joinDeviceGroup(deviceId: number): Promise<boolean> {
    try {
      if (!this.connection || !this.isConnected) {
        console.error('[SignalR] Not connected')
        return false
      }

      await this.connection.invoke('JoinGroup', `device_${deviceId}`)
      console.log(`[SignalR] Joined device group for device ${deviceId}`)
      return true
    } catch (error) {
      console.error(`[SignalR] Failed to join device group for device ${deviceId}:`, error)
      return false
    }
  }

  /**
   * 离开设备组
   */
  async leaveDeviceGroup(deviceId: number): Promise<boolean> {
    try {
      if (!this.connection || !this.isConnected) {
        console.error('[SignalR] Not connected')
        return false
      }

      await this.connection.invoke('LeaveGroup', `device_${deviceId}`)
      console.log(`[SignalR] Left device group for device ${deviceId}`)
      return true
    } catch (error) {
      console.error(`[SignalR] Failed to leave device group for device ${deviceId}:`, error)
      return false
    }
  }

  // ==================== 事件订阅管理 ====================

  /**
   * 订阅数据更新事件
   */
  onDataUpdate(callback: DataUpdateCallback): () => void {
    this.dataUpdateCallbacks.add(callback)
    return () => this.dataUpdateCallbacks.delete(callback)
  }

  /**
   * 订阅设备状态更新事件
   */
  onDeviceStatusUpdate(callback: DeviceStatusCallback): () => void {
    this.deviceStatusCallbacks.add(callback)
    return () => this.deviceStatusCallbacks.delete(callback)
  }

  /**
   * 订阅任务状态更新事件
   */
  onTaskStatusUpdate(callback: TaskStatusCallback): () => void {
    this.taskStatusCallbacks.add(callback)
    return () => this.taskStatusCallbacks.delete(callback)
  }

  /**
   * 订阅连接状态变化事件
   */
  onConnectionChange(callback: ConnectionCallback): () => void {
    this.connectionCallbacks.add(callback)
    return () => this.connectionCallbacks.delete(callback)
  }

  // ==================== 私有方法 ====================

  private notifyDataUpdateCallbacks(data: RealTimeData): void {
    this.dataUpdateCallbacks.forEach(callback => {
      try {
        callback(data)
      } catch (error) {
        console.error('[SignalR] Error in data update callback:', error)
      }
    })
  }

  private notifyDeviceStatusCallbacks(update: DeviceStatusUpdate): void {
    this.deviceStatusCallbacks.forEach(callback => {
      try {
        callback(update)
      } catch (error) {
        console.error('[SignalR] Error in device status callback:', error)
      }
    })
  }

  private notifyTaskStatusCallbacks(update: TaskStatusUpdate): void {
    this.taskStatusCallbacks.forEach(callback => {
      try {
        callback(update)
      } catch (error) {
        console.error('[SignalR] Error in task status callback:', error)
      }
    })
  }

  private notifyConnectionCallbacks(connected: boolean): void {
    this.connectionCallbacks.forEach(callback => {
      try {
        callback(connected)
      } catch (error) {
        console.error('[SignalR] Error in connection callback:', error)
      }
    })
  }

  // ==================== 状态查询 ====================

  /**
   * 获取连接状态
   */
  getConnectionState(): signalR.HubConnectionState | null {
    return this.connection?.state || null
  }

  /**
   * 是否已连接
   */
  isConnectionActive(): boolean {
    return this.isConnected && this.connection?.state === signalR.HubConnectionState.Connected
  }

  /**
   * 获取连接ID
   */
  getConnectionId(): string | null {
    return this.connection?.connectionId || null
  }

  /**
   * 设置基础URL
   */
  setBaseURL(baseURL: string): void {
    this.baseURL = baseURL
  }

  /**
   * 获取基础URL
   */
  getBaseURL(): string {
    return this.baseURL
  }
}

// 创建默认实例
export const signalRService = new SignalRService()
