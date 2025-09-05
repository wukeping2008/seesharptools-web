import * as signalR from '@microsoft/signalr'

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private dataCallbacks: ((data: any) => void)[] = []

  async connect() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/data')
      .withAutomaticReconnect()
      .build()

    // 注册事件处理
    this.connection.on('DataReceived', (data) => {
      this.dataCallbacks.forEach(callback => callback(data))
    })

    this.connection.on('Connected', (connectionId) => {
      console.log('SignalR连接成功:', connectionId)
    })

    this.connection.on('Error', (error) => {
      console.error('SignalR错误:', error)
    })

    this.connection.on('AnomalyDetected', (anomaly) => {
      console.warn('检测到异常:', anomaly)
    })

    // 开始连接
    await this.connection.start()
    console.log('SignalR连接已建立')
  }

  async disconnect() {
    if (this.connection) {
      await this.connection.stop()
      console.log('SignalR连接已断开')
    }
  }

  async startDataStream(channelCount: number, sampleRate: number) {
    if (this.connection) {
      await this.connection.invoke('StartDataStream', channelCount, sampleRate)
    }
  }

  async stopDataStream() {
    if (this.connection) {
      await this.connection.invoke('StopDataStream')
    }
  }

  onDataReceived(callback: (data: any) => void) {
    this.dataCallbacks.push(callback)
  }

  async sendCommand(command: string, parameters: any) {
    if (this.connection) {
      await this.connection.invoke('SendCommand', command, parameters)
    }
  }
}

export const signalRService = new SignalRService()