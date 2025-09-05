import axios from 'axios'

// 尝试使用简化的API，如果失败则回退到原API
const API_BASE = '/api/simpledata'
const FALLBACK_API = '/api/dataacquisition'

export const dataService = {
  // 初始化硬件
  async initialize() {
    try {
      const response = await axios.post(`${API_BASE}/initialize`)
      return response.data
    } catch (error) {
      console.log('使用备用API')
      const response = await axios.post(`${FALLBACK_API}/initialize`)
      return response.data
    }
  },

  // 开始采集
  async startAcquisition(config: any) {
    try {
      const response = await axios.post(`${API_BASE}/start`, config)
      return response.data
    } catch (error) {
      console.log('使用备用API')
      const response = await axios.post(`${FALLBACK_API}/start`, config)
      return response.data
    }
  },

  // 停止采集
  async stopAcquisition() {
    try {
      const response = await axios.post(`${API_BASE}/stop`)
      return response.data
    } catch (error) {
      console.log('使用备用API')
      const response = await axios.post(`${FALLBACK_API}/stop`)
      return response.data
    }
  },

  // 获取状态
  async getStatus() {
    try {
      const response = await axios.get(`${API_BASE}/status`)
      return response.data
    } catch (error) {
      console.log('使用备用API')
      const response = await axios.get(`${FALLBACK_API}/status`)
      return response.data
    }
  }
}