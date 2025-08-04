<template>
  <div class="random-number-view">
    <div class="container">
      <!-- 页面标题 -->
      <div class="header">
        <h1>随机数生成器</h1>
        <p class="subtitle">点击按钮生成随机数</p>
      </div>

      <!-- 单个随机数生成 -->
      <div class="card">
        <h2>单个随机数</h2>
        
        <!-- 参数设置 -->
        <div class="controls">
          <div class="input-group">
            <label for="min">最小值:</label>
            <input 
              id="min"
              v-model.number="singleConfig.min" 
              type="number" 
              class="form-input"
            >
          </div>
          <div class="input-group">
            <label for="max">最大值:</label>
            <input 
              id="max"
              v-model.number="singleConfig.max" 
              type="number" 
              class="form-input"
            >
          </div>
        </div>

        <!-- 显示区域 -->
        <div class="display-area">
          <div class="number-display">
            <span class="current-number">{{ currentNumber }}</span>
          </div>
          <div class="info-text">
            <p v-if="lastSingleResponse">{{ lastSingleResponse.message }}</p>
            <p v-if="lastSingleResponse" class="timestamp">
              生成时间: {{ formatTime(lastSingleResponse.generatedAt) }}
            </p>
          </div>
        </div>

        <!-- 生成按钮 -->
        <div class="button-group">
          <button 
            @click="generateSingle" 
            :disabled="isLoading"
            class="btn btn-primary btn-large"
          >
            <span v-if="isLoading">生成中...</span>
            <span v-else>生成随机数</span>
          </button>
        </div>
      </div>

      <!-- 批量随机数生成 -->
      <div class="card">
        <h2>批量随机数</h2>
        
        <!-- 参数设置 -->
        <div class="controls">
          <div class="input-group">
            <label for="count">数量:</label>
            <input 
              id="count"
              v-model.number="batchConfig.count" 
              type="number" 
              min="1" 
              max="1000"
              class="form-input"
            >
          </div>
          <div class="input-group">
            <label for="batchMin">最小值:</label>
            <input 
              id="batchMin"
              v-model.number="batchConfig.min" 
              type="number" 
              class="form-input"
            >
          </div>
          <div class="input-group">
            <label for="batchMax">最大值:</label>
            <input 
              id="batchMax"
              v-model.number="batchConfig.max" 
              type="number" 
              class="form-input"
            >
          </div>
        </div>

        <!-- 显示区域 -->
        <div class="display-area">
          <div v-if="batchNumbers.length > 0" class="batch-display">
            <div class="statistics">
              <div class="stat-item">
                <span class="stat-label">数量:</span>
                <span class="stat-value">{{ batchNumbers.length }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">平均值:</span>
                <span class="stat-value">{{ batchAverage.toFixed(2) }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">最小值:</span>
                <span class="stat-value">{{ Math.min(...batchNumbers) }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">最大值:</span>
                <span class="stat-value">{{ Math.max(...batchNumbers) }}</span>
              </div>
            </div>
            
            <div class="numbers-grid">
              <span 
                v-for="(number, index) in batchNumbers" 
                :key="index"
                class="number-item"
              >
                {{ number }}
              </span>
            </div>
          </div>
          <div v-else class="empty-state">
            <p>点击按钮生成批量随机数</p>
          </div>
        </div>

        <!-- 生成按钮 -->
        <div class="button-group">
          <button 
            @click="generateBatch" 
            :disabled="isBatchLoading"
            class="btn btn-secondary btn-large"
          >
            <span v-if="isBatchLoading">生成中...</span>
            <span v-else>生成批量随机数</span>
          </button>
        </div>
      </div>

      <!-- 错误提示 -->
      <div v-if="errorMessage" class="error-message">
        {{ errorMessage }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { RandomNumberService, type RandomNumberResponse, type BatchRandomNumberResponse } from '@/services/RandomNumberService'

// 响应式数据
const currentNumber = ref<number>(0)
const batchNumbers = ref<number[]>([])
const isLoading = ref(false)
const isBatchLoading = ref(false)
const errorMessage = ref('')
const lastSingleResponse = ref<RandomNumberResponse | null>(null)

// 配置参数
const singleConfig = ref({
  min: 1,
  max: 100
})

const batchConfig = ref({
  count: 10,
  min: 1,
  max: 100
})

// 计算属性 
const batchAverage = computed(() => {
  if (batchNumbers.value.length === 0) return 0
  return batchNumbers.value.reduce((sum, num) => sum + num, 0) / batchNumbers.value.length
})

// 生成单个随机数
const generateSingle = async () => {
  try {
    isLoading.value = true
    errorMessage.value = ''
    
    const response = await RandomNumberService.generateRandomNumber(
      singleConfig.value.min, 
      singleConfig.value.max
    )
    
    currentNumber.value = response.value
    lastSingleResponse.value = response
    
  } catch (error) {
    console.error('生成随机数失败:', error)
    errorMessage.value = '生成随机数失败，请检查网络连接'
  } finally {
    isLoading.value = false
  }
}

// 生成批量随机数
const generateBatch = async () => {
  try {
    isBatchLoading.value = true
    errorMessage.value = ''
    
    const response = await RandomNumberService.generateBatchRandomNumbers(
      batchConfig.value.count,
      batchConfig.value.min, 
      batchConfig.value.max
    )
    
    batchNumbers.value = response.values
    
  } catch (error) {
    console.error('生成批量随机数失败:', error)
    errorMessage.value = '生成批量随机数失败，请检查网络连接'
  } finally {
    isBatchLoading.value = false
  }
}

// 格式化时间
const formatTime = (timeString: string) => {
  const date = new Date(timeString)
  return date.toLocaleString('zh-CN')
}

// 页面加载时生成一个默认随机数
generateSingle()
</script>

<style scoped>
.random-number-view {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 2rem 1rem;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
}

.header {
  text-align: center;
  margin-bottom: 3rem;
  color: white;
}

.header h1 {
  font-size: 3rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
  text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
}

.subtitle {
  font-size: 1.2rem;
  opacity: 0.9;
}

.card {
  background: white;
  border-radius: 20px;
  padding: 2rem;
  margin-bottom: 2rem;
  box-shadow: 0 10px 30px rgba(0,0,0,0.2);
  transition: transform 0.3s ease;
}

.card:hover {
  transform: translateY(-5px);
}

.card h2 {
  color: #333;
  font-size: 1.8rem;
  margin-bottom: 1.5rem;
  text-align: center;
}

.controls {
  display: flex;
  gap: 1rem;
  margin-bottom: 2rem;
  flex-wrap: wrap;
  justify-content: center;
}

.input-group {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.input-group label {
  font-weight: 600;
  margin-bottom: 0.5rem;
  color: #555;
}

.form-input {
  padding: 0.75rem;
  border: 2px solid #e1e5e9;
  border-radius: 10px;
  font-size: 1rem;
  width: 120px;
  text-align: center;
  transition: border-color 0.3s ease;
}

.form-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.display-area {
  margin-bottom: 2rem;
  text-align: center;
}

.number-display {
  margin-bottom: 1rem;
}

.current-number {
  font-size: 4rem;
  font-weight: 700;
  color: #667eea;
  text-shadow: 2px 2px 4px rgba(0,0,0,0.1);
  display: inline-block;
  padding: 1rem 2rem;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
  border-radius: 20px;
  min-width: 200px;
}

.info-text {
  margin-top: 1rem;
  color: #666;
}

.timestamp {
  font-size: 0.9rem;
  opacity: 0.8;
}

.batch-display {
  text-align: left;
}

.statistics {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 10px;
}

.stat-item {
  text-align: center;
}

.stat-label {
  display: block;
  font-size: 0.9rem;
  color: #666;
  margin-bottom: 0.25rem;
}

.stat-value {
  display: block;
  font-size: 1.5rem;
  font-weight: 600;
  color: #333;
}

.numbers-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  justify-content: center;
  max-height: 300px;
  overflow-y: auto;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 10px;
}

.number-item {
  display: inline-block;
  padding: 0.5rem 1rem;
  background: white;
  border-radius: 8px;
  font-weight: 600;
  color: #333;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  transition: transform 0.2s ease;
}

.number-item:hover {
  transform: translateY(-2px);
}

.empty-state {
  padding: 2rem;
  color: #999;
  font-style: italic;
}

.button-group {
  text-align: center;
}

.btn {
  padding: 1rem 2rem;
  border: none;
  border-radius: 50px;
  font-size: 1.1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-large {
  padding: 1.25rem 3rem;
  font-size: 1.2rem;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(102, 126, 234, 0.5);
}

.btn-secondary {
  background: linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%);
  color: #333;
  box-shadow: 0 4px 15px rgba(252, 182, 159, 0.4);
}

.btn-secondary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(252, 182, 159, 0.5);
}

.error-message {
  background: #fee;
  color: #c33;
  padding: 1rem;
  border-radius: 10px;
  text-align: center;
  margin-top: 1rem;
  border: 1px solid #fcc;
}

@media (max-width: 768px) {
  .header h1 {
    font-size: 2rem;
  }
  
  .current-number {
    font-size: 2.5rem;
    min-width: 150px;
  }
  
  .controls {
    flex-direction: column;
    align-items: center;
  }
  
  .btn-large {
    padding: 1rem 2rem;
    font-size: 1rem;
  }
}
</style>
