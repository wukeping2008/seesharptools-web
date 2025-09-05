import axios from 'axios';

const API_BASE_URL = 'http://localhost:5100';

/**
 * USB-1601硬件服务 - 用于AIGen-SeeSharp-Suite项目
 * 提供与真实USB-1601硬件的连接和数据采集接口
 */
export class USB1601Service {
  private baseUrl: string;
  private isConnected: boolean = false;

  constructor(baseUrl?: string) {
    this.baseUrl = baseUrl || `${API_BASE_URL}/api/usb1601`;
  }

  /**
   * 获取设备连接状态
   */
  async getStatus() {
    try {
      const response = await axios.get(`${this.baseUrl}/status`);
      this.isConnected = response.data.isConnected;
      return response.data;
    } catch (error) {
      console.error('Failed to get device status:', error);
      return { isConnected: false, error: error.message };
    }
  }

  /**
   * 初始化并连接USB-1601设备
   */
  async connect(deviceId: string = '0') {
    try {
      const response = await axios.post(`${this.baseUrl}/connect`, { deviceId });
      this.isConnected = response.data.success;
      return response.data;
    } catch (error) {
      console.error('Failed to connect device:', error);
      return { success: false, error: error.message };
    }
  }

  /**
   * 断开设备连接
   */
  async disconnect() {
    try {
      const response = await axios.post(`${this.baseUrl}/disconnect`);
      this.isConnected = false;
      return response.data;
    } catch (error) {
      console.error('Failed to disconnect device:', error);
      return { success: false, error: error.message };
    }
  }

  /**
   * 发现所有可用的USB-1601设备
   */
  async discoverDevices() {
    try {
      const response = await axios.get(`${this.baseUrl}/discover`);
      return response.data;
    } catch (error) {
      console.error('Failed to discover devices:', error);
      return { devices: [], error: error.message };
    }
  }

  /**
   * 配置模拟输入通道
   */
  async configureAI(config: {
    channels: number[];
    sampleRate: number;
    range: number;
  }) {
    try {
      const response = await axios.post(`${this.baseUrl}/ai/configure`, config);
      return response.data;
    } catch (error) {
      console.error('Failed to configure AI:', error);
      return { success: false, error: error.message };
    }
  }

  /**
   * 开始连续数据采集（用于实时显示）
   */
  async startContinuousAI(config: {
    channels: number[];
    sampleRate: number;
    bufferSize: number;
  }) {
    try {
      const response = await axios.post(`${this.baseUrl}/ai/start-continuous`, config);
      return response.data;
    } catch (error) {
      console.error('Failed to start continuous AI:', error);
      return { success: false, error: error.message };
    }
  }

  /**
   * 停止连续数据采集
   */
  async stopContinuousAI() {
    try {
      const response = await axios.post(`${this.baseUrl}/ai/stop-continuous`);
      return response.data;
    } catch (error) {
      console.error('Failed to stop continuous AI:', error);
      return { success: false, error: error.message };
    }
  }

  /**
   * 读取最新的数据缓冲区（用于实时更新）
   */
  async readLatestData() {
    try {
      const response = await axios.get(`${this.baseUrl}/ai/read-latest`);
      return response.data;
    } catch (error) {
      console.error('Failed to read latest data:', error);
      return { data: [], timestamp: Date.now(), error: error.message };
    }
  }

  /**
   * 单点读取AI（用于测试）
   */
  async readSinglePoint(channel: number) {
    try {
      const response = await axios.get(`${this.baseUrl}/ai/read-single/${channel}`);
      return response.data;
    } catch (error) {
      console.error('Failed to read single point:', error);
      return { value: 0, channel, error: error.message };
    }
  }

  /**
   * 有限点采集（用于批量数据采集）
   */
  async readFiniteSamples(config: {
    channels: number[];
    samplesPerChannel: number;
    sampleRate: number;
  }) {
    try {
      const response = await axios.post(`${this.baseUrl}/ai/read-finite`, config);
      return response.data;
    } catch (error) {
      console.error('Failed to read finite samples:', error);
      return { data: [], error: error.message };
    }
  }

  /**
   * 获取设备信息
   */
  async getDeviceInfo() {
    try {
      const response = await axios.get(`${this.baseUrl}/info`);
      return response.data;
    } catch (error) {
      console.error('Failed to get device info:', error);
      return { 
        model: 'USB-1601',
        channels: 16,
        maxSampleRate: 200000,
        resolution: 12,
        error: error.message 
      };
    }
  }

  /**
   * 测试连接（用于验证硬件是否可用）
   */
  async testConnection() {
    try {
      const response = await axios.get(`${this.baseUrl}/test`);
      return response.data;
    } catch (error) {
      console.error('Connection test failed:', error);
      return { success: false, message: 'Unable to connect to hardware', error: error.message };
    }
  }
}

// 创建默认实例
export const usb1601Service = new USB1601Service();

// WebSocket连接用于实时数据流
export class USB1601WebSocket {
  private ws: WebSocket | null = null;
  private reconnectTimer: any = null;
  private handlers: { [key: string]: Function[] } = {};

  constructor(private url: string = `ws://localhost:5100/dataHub`) {}

  connect() {
    try {
      this.ws = new WebSocket(this.url);

      this.ws.onopen = () => {
        console.log('WebSocket connected to USB-1601 data stream');
        this.emit('connected');
      };

      this.ws.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data);
          this.emit('data', data);
        } catch (error) {
          console.error('Failed to parse WebSocket data:', error);
        }
      };

      this.ws.onerror = (error) => {
        console.error('WebSocket error:', error);
        this.emit('error', error);
      };

      this.ws.onclose = () => {
        console.log('WebSocket disconnected');
        this.emit('disconnected');
        this.attemptReconnect();
      };
    } catch (error) {
      console.error('Failed to create WebSocket connection:', error);
      this.attemptReconnect();
    }
  }

  private attemptReconnect() {
    if (this.reconnectTimer) return;
    
    this.reconnectTimer = setTimeout(() => {
      console.log('Attempting to reconnect WebSocket...');
      this.reconnectTimer = null;
      this.connect();
    }, 5000);
  }

  disconnect() {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }
    if (this.ws) {
      this.ws.close();
      this.ws = null;
    }
  }

  on(event: string, handler: Function) {
    if (!this.handlers[event]) {
      this.handlers[event] = [];
    }
    this.handlers[event].push(handler);
  }

  off(event: string, handler: Function) {
    if (!this.handlers[event]) return;
    const index = this.handlers[event].indexOf(handler);
    if (index >= 0) {
      this.handlers[event].splice(index, 1);
    }
  }

  private emit(event: string, data?: any) {
    if (!this.handlers[event]) return;
    this.handlers[event].forEach(handler => handler(data));
  }

  send(message: any) {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(message));
    }
  }
}

export const usb1601WebSocket = new USB1601WebSocket();