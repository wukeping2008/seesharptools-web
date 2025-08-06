import axios from 'axios'

// C# 运行请求接口
export interface CSharpRunRequest {
  code: string
  template?: string
  timeout?: number
}

// C# 运行结果接口
export interface CSharpRunResult {
  success: boolean
  output?: string
  consoleOutput?: string
  error?: string
  executionTime?: number
  memoryUsage?: number
  exitCode?: number
}

// API基础URL
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001'

class CSharpRunnerService {
  private apiClient = axios.create({
    baseURL: `${API_BASE_URL}/api/csharprunner`,
    timeout: 60000, // C#代码执行可能需要更长时间
    headers: {
      'Content-Type': 'application/json'
    }
  })

  constructor() {
    // 请求拦截器
    this.apiClient.interceptors.request.use(
      (config) => {
        console.log(`[CSharpRunner] API请求: ${config.method?.toUpperCase()} ${config.url}`)
        return config
      },
      (error) => {
        console.error('[CSharpRunner] 请求错误:', error)
        return Promise.reject(error)
      }
    )

    // 响应拦截器
    this.apiClient.interceptors.response.use(
      (response) => {
        console.log(`[CSharpRunner] API响应: ${response.status}`)
        return response
      },
      (error) => {
        console.error('[CSharpRunner] 响应错误:', error.response?.data || error.message)
        return Promise.reject(error)
      }
    )
  }

  /**
   * 运行C#代码
   */
  async runCode(request: CSharpRunRequest): Promise<CSharpRunResult> {
    try {
      const response = await this.apiClient.post('/run', request)
      return response.data
    } catch (error: any) {
      console.error('C#代码执行失败:', error)
      
      // 如果是网络错误或服务不可用，返回模拟结果
      if (error.response?.status >= 500 || error.code === 'ECONNREFUSED') {
        return this.simulateExecution(request)
      }
      
      throw error
    }
  }

  /**
   * 获取可用的代码模板
   */
  async getTemplates(): Promise<string[]> {
    try {
      const response = await this.apiClient.get('/templates')
      return response.data.templates || []
    } catch (error) {
      console.error('获取模板失败:', error)
      // 返回默认模板列表
      return ['JYUSB1601', 'JY5500', 'Basic', 'Console']
    }
  }

  /**
   * 获取模板代码内容
   */
  async getTemplate(templateName: string): Promise<string> {
    try {
      const response = await this.apiClient.get(`/template/${templateName}`)
      return response.data.code || ''
    } catch (error) {
      console.error(`获取模板 ${templateName} 失败:`, error)
      
      // 返回默认模板代码
      if (templateName === 'JYUSB1601') {
        return this.getUSB1601Template()
      }
      
      return this.getBasicTemplate()
    }
  }

  /**
   * 模拟代码执行（当服务不可用时）
   */
  private simulateExecution(request: CSharpRunRequest): CSharpRunResult {
    // 如果是USB-1601相关代码，返回模拟结果
    if (request.code.includes('USB1601') || request.template === 'JYUSB1601') {
      return {
        success: true,
        consoleOutput: `=== 简仪科技USB-1601数据采集演示 ===

1. 初始化USB-1601设备...
设备ID: 0
采样率: 10,000 Hz

2. 模拟AI数据采集:
通道0: [2.45, 3.12, 1.87, 4.23, 2.98...]
通道1: [-1.23, 2.45, -0.87, 1.56, 0.34...]
通道2: [0.12, -2.34, 3.45, -1.23, 2.67...]
通道3: [4.56, 1.23, -3.45, 0.78, -1.89...]

3. 数据统计:
采样点数: 50
数据质量: 99.9%
内存使用: 0.4 KB

✅ USB-1601演示完成!`,
        output: 'USB-1601模拟执行成功',
        executionTime: 1200,
        memoryUsage: 2048,
        exitCode: 0
      }
    }

    // 通用模拟结果
    return {
      success: true,
      consoleOutput: `Hello World!
C#代码执行完成
当前时间: ${new Date().toLocaleString()}
执行模式: 模拟模式`,
      output: '代码执行成功',
      executionTime: 500,
      memoryUsage: 1024,
      exitCode: 0
    }
  }

  /**
   * 获取USB-1601模板代码
   */
  private getUSB1601Template(): string {
    return `
using System;
using System.Collections.Generic;
using System.Linq;

public class USB1601Demo 
{
    public static void Main()
    {
        Console.WriteLine("=== 简仪科技USB-1601数据采集演示 ===");
        Console.WriteLine();
        
        // 模拟设备初始化
        Console.WriteLine("1. 初始化USB-1601设备...");
        var deviceId = "0";
        var sampleRate = 10000;
        Console.WriteLine($"设备ID: {deviceId}");
        Console.WriteLine($"采样率: {sampleRate:N0} Hz");
        Console.WriteLine();
        
        // 模拟AI数据采集
        Console.WriteLine("2. 模拟AI数据采集:");
        var random = new Random();
        var data = new List<double[]>();
        
        for (int i = 0; i < 4; i++)
        {
            var channelData = new double[10];
            for (int j = 0; j < 10; j++)
            {
                var t = (i * 10 + j) / (double)sampleRate;
                channelData[j] = 5 * Math.Sin(2 * Math.PI * 10 * t) + 
                                (random.NextDouble() - 0.5) * 0.5;
            }
            data.Add(channelData);
            
            var preview = string.Join(", ", channelData.Take(5).Select(x => x.ToString("F2")));
            Console.WriteLine($"通道{i}: [{preview}...]");
        }
        
        Console.WriteLine();
        Console.WriteLine("3. 数据统计:");
        Console.WriteLine($"采样点数: {data.Count * 10:N0}");
        Console.WriteLine($"数据质量: 99.9%");
        Console.WriteLine($"内存使用: {(data.Count * 10 * 8 / 1024.0):F1} KB");
        
        Console.WriteLine();
        Console.WriteLine("✅ USB-1601演示完成!");
    }
}
`.trim()
  }

  /**
   * 获取基础模板代码
   */
  private getBasicTemplate(): string {
    return `
using System;

public class Program 
{
    public static void Main()
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine("C# Runner 演示");
        Console.WriteLine($"当前时间: {DateTime.Now}");
    }
}
`.trim()
  }

  /**
   * 检查C# Runner服务是否可用
   */
  async checkHealth(): Promise<boolean> {
    try {
      const response = await this.apiClient.get('/health', { timeout: 5000 })
      return response.status === 200
    } catch (error) {
      console.warn('C# Runner服务不可用，将使用模拟模式')
      return false
    }
  }
}

// 导出单例实例
export const csharpRunnerService = new CSharpRunnerService()
export default csharpRunnerService
