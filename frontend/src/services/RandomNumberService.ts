import { backendApi } from './BackendApiService'

export interface RandomNumberResponse {
  success: boolean
  value: number
  min: number
  max: number
  generatedAt: string
  message: string
}

export interface BatchRandomNumberResponse {
  success: boolean
  values: number[]
  count: number
  min: number
  max: number
  generatedAt: string
  message: string
  statistics: {
    average: number
    minimum: number
    maximum: number
  }
}

export class RandomNumberService {
  private static baseUrl = '/api/Random'

  /**
   * 生成单个随机数
   */
  static async generateRandomNumber(min: number = 1, max: number = 100): Promise<RandomNumberResponse> {
    try {
      const params = {
        min: min,
        max: max
      }
      
      const response = await backendApi.get<RandomNumberResponse>(
        `${this.baseUrl}/single`, params
      )
      
      return response
    } catch (error) {
      console.error('生成随机数失败:', error)
      throw new Error('生成随机数失败，请检查网络连接')
    }
  }

  /**
   * 批量生成随机数
   */
  static async generateBatchRandomNumbers(
    count: number = 5,
    min: number = 1,
    max: number = 100
  ): Promise<BatchRandomNumberResponse> {
    try {
      const params = {
        count: count,
        min: min,
        max: max
      }
      
      const response = await backendApi.get<BatchRandomNumberResponse>(
        `${this.baseUrl}/batch`, params
      )
      
      return response
    } catch (error) {
      console.error('批量生成随机数失败:', error)
      throw new Error('生成批量随机数失败，请检查网络连接')
    }
  }
}
