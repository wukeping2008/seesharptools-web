<template>
  <div class="ai-analysis">
    <h2>🤖 百度 ERNIE-4.5 AI 智能分析</h2>
    
    <!-- AI状态卡片 -->
    <div class="status-card">
      <div class="status-header">
        <span class="status-title">AI服务状态</span>
        <span :class="['status-badge', connectionStatus]">
          {{ connectionStatus === 'connected' ? '✅ 已连接' : 
             connectionStatus === 'connecting' ? '⏳ 连接中...' : '❌ 未连接' }}
        </span>
      </div>
      <div v-if="lastAnalysis" class="status-info">
        <p>模型: {{ lastAnalysis.modelUsed || 'ERNIE-4.5-turbo-vl' }}</p>
        <p>处理级别: {{ lastAnalysis.processingLevel }}</p>
        <p v-if="lastAnalysis.estimatedCost">预估成本: ¥{{ lastAnalysis.estimatedCost.toFixed(4) }}</p>
      </div>
    </div>

    <!-- 控制面板 -->
    <div class="control-panel">
      <h3>📊 信号参数设置</h3>
      <div class="controls-grid">
        <div class="control-group">
          <label>信号类型</label>
          <select v-model="analysisParams.signalType">
            <option value="sine">正弦波</option>
            <option value="square">方波</option>
            <option value="triangle">三角波</option>
            <option value="noise">噪声</option>
          </select>
        </div>
        
        <div class="control-group">
          <label>频率 (Hz)</label>
          <input 
            type="number" 
            v-model.number="analysisParams.frequency" 
            min="1" 
            max="1000"
            step="10"
          />
        </div>
        
        <div class="control-group">
          <label>幅度 (V)</label>
          <input 
            type="number" 
            v-model.number="analysisParams.amplitude" 
            min="0.1" 
            max="10"
            step="0.1"
          />
        </div>
        
        <div class="control-group">
          <label>
            <input type="checkbox" v-model="analysisParams.useAdvancedModel" />
            使用高级模型 (ERNIE-4.5)
          </label>
        </div>
        
        <div class="control-group">
          <label>
            <input type="checkbox" v-model="analysisParams.enableVisualAnalysis" />
            启用视觉分析
          </label>
        </div>
      </div>
    </div>

    <!-- 操作按钮 -->
    <div class="actions">
      <button @click="testConnection" :disabled="loading" class="btn btn-secondary">
        🔌 测试连接
      </button>
      <button @click="performAnalysis" :disabled="loading" class="btn btn-primary">
        🚀 开始分析
      </button>
      <button @click="testFallback" :disabled="loading" class="btn btn-info">
        🔄 测试降级
      </button>
      <button @click="testStream" :disabled="loading" class="btn btn-warning">
        📡 实时流分析
      </button>
    </div>

    <!-- 加载状态 -->
    <div v-if="loading" class="loading">
      <div class="spinner"></div>
      <p>{{ loadingMessage }}</p>
    </div>

    <!-- 分析结果 -->
    <div v-if="analysisResult && !loading" class="results">
      <h3>📈 分析结果</h3>
      
      <!-- 快速分析结果 -->
      <div v-if="analysisResult.quickAnalysis" class="result-section">
        <h4>快速分析</h4>
        <div class="result-grid">
          <div class="result-item">
            <span class="label">信号类型:</span>
            <span class="value">{{ analysisResult.quickAnalysis.signalType || '-' }}</span>
          </div>
          <div class="result-item">
            <span class="label">信号质量:</span>
            <span class="value">{{ analysisResult.quickAnalysis.quality || '-' }}</span>
          </div>
          <div class="result-item">
            <span class="label">处理级别:</span>
            <span class="value">{{ analysisResult.quickAnalysis.processingLevel || '-' }}</span>
          </div>
        </div>
      </div>
      
      <!-- 专业报告 -->
      <div v-if="analysisResult.report" class="result-section">
        <h4>{{ analysisResult.report.title }}</h4>
        <div class="report-meta">
          <span class="badge">{{ analysisResult.report.modelUsed }}</span>
          <span class="badge">{{ analysisResult.report.processingLevel }}</span>
          <span v-if="analysisResult.report.estimatedCost" class="badge cost">
            成本: ¥{{ analysisResult.report.estimatedCost.toFixed(4) }}
          </span>
        </div>
        <div class="report-content" v-html="formatReport(analysisResult.report.content)"></div>
      </div>
      
      <!-- 视觉分析结果 -->
      <div v-if="analysisResult.visualAnalysis" class="result-section">
        <h4>视觉分析</h4>
        <div class="visual-analysis">
          <p>{{ analysisResult.visualAnalysis.analysis }}</p>
          <span class="model-tag">使用模型: {{ analysisResult.visualAnalysis.modelUsed }}</span>
        </div>
      </div>
      
      <!-- 模型层级结果 -->
      <div v-if="analysisResult.modelHierarchy" class="result-section">
        <h4>模型层级测试</h4>
        <div class="hierarchy-list">
          <div v-for="(item, index) in analysisResult.modelHierarchy" :key="index" class="hierarchy-item">
            <div class="hierarchy-header">
              <span class="level">{{ item.level }}</span>
              <span class="model">{{ item.model }}</span>
            </div>
            <div class="hierarchy-result">
              <pre>{{ JSON.stringify(item.result, null, 2) }}</pre>
            </div>
          </div>
        </div>
      </div>
      
      <!-- 实时流分析结果 -->
      <div v-if="streamResults.length > 0" class="result-section">
        <h4>实时流分析结果</h4>
        <div class="stream-results">
          <div v-for="(point, index) in streamResults" :key="index" class="stream-point">
            <span class="time">{{ formatTime(point.timestamp) }}</span>
            <span class="value">{{ point.value.toFixed(3) }}V</span>
            <span :class="['status', point.status.toLowerCase()]">{{ point.status }}</span>
            <span class="confidence">置信度: {{ (point.confidence * 100).toFixed(0) }}%</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 错误提示 -->
    <div v-if="error" class="error">
      <p>❌ {{ error }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { aiService, AnalysisRequest } from '../services/aiService'

// 状态
const connectionStatus = ref<'disconnected' | 'connecting' | 'connected'>('disconnected')
const loading = ref(false)
const loadingMessage = ref('处理中...')
const error = ref('')
const analysisResult = ref<any>(null)
const streamResults = ref<any[]>([])
const lastAnalysis = ref<any>(null)

// 分析参数
const analysisParams = ref<AnalysisRequest>({
  signalType: 'sine',
  frequency: 100,
  amplitude: 5,
  useAdvancedModel: true,
  enableVisualAnalysis: false
})

// 测试连接
async function testConnection() {
  loading.value = true
  loadingMessage.value = '正在测试AI服务连接...'
  error.value = ''
  connectionStatus.value = 'connecting'
  
  try {
    const result = await aiService.testConnection()
    if (result.success) {
      connectionStatus.value = 'connected'
      analysisResult.value = result
      showMessage('✅ AI服务连接成功！')
    } else {
      connectionStatus.value = 'disconnected'
      error.value = '连接失败'
    }
  } catch (err: any) {
    connectionStatus.value = 'disconnected'
    error.value = err.message || '连接测试失败'
  } finally {
    loading.value = false
  }
}

// 执行分析
async function performAnalysis() {
  loading.value = true
  loadingMessage.value = '正在使用 ERNIE-4.5 进行智能分析...'
  error.value = ''
  streamResults.value = []
  
  try {
    const result = await aiService.performAdvancedAnalysis(analysisParams.value)
    if (result.success) {
      analysisResult.value = result
      lastAnalysis.value = result.report
      showMessage(`✅ 分析完成！使用模型: ${result.report?.modelUsed || 'ERNIE-4.5'}`)
    } else {
      error.value = result.message || '分析失败'
    }
  } catch (err: any) {
    error.value = err.message || '分析请求失败'
  } finally {
    loading.value = false
  }
}

// 测试降级机制
async function testFallback() {
  loading.value = true
  loadingMessage.value = '正在测试模型降级机制...'
  error.value = ''
  streamResults.value = []
  
  try {
    const result = await aiService.testFallback()
    if (result.success) {
      analysisResult.value = result
      showMessage('✅ 降级测试完成！')
    } else {
      error.value = result.message || '降级测试失败'
    }
  } catch (err: any) {
    error.value = err.message || '降级测试请求失败'
  } finally {
    loading.value = false
  }
}

// 测试实时流分析
async function testStream() {
  loading.value = true
  loadingMessage.value = '正在进行实时流分析...'
  error.value = ''
  streamResults.value = []
  
  try {
    const result = await aiService.testStreamAnalysis()
    if (result.success) {
      streamResults.value = result.results || []
      analysisResult.value = { ...analysisResult.value, streamAnalysis: result }
      showMessage(`✅ 实时流分析完成！处理了 ${result.dataPoints} 个数据点`)
    } else {
      error.value = result.message || '流分析失败'
    }
  } catch (err: any) {
    error.value = err.message || '流分析请求失败'
  } finally {
    loading.value = false
  }
}

// 格式化报告内容（将Markdown转换为HTML）
function formatReport(content: string): string {
  if (!content) return ''
  
  // 简单的Markdown到HTML转换
  return content
    .replace(/^### (.*$)/gim, '<h4>$1</h4>')
    .replace(/^## (.*$)/gim, '<h3>$1</h3>')
    .replace(/^# (.*$)/gim, '<h2>$1</h2>')
    .replace(/\*\*(.*)\*\*/gim, '<strong>$1</strong>')
    .replace(/\*(.*)\*/gim, '<em>$1</em>')
    .replace(/\n\n/gim, '</p><p>')
    .replace(/^- (.*$)/gim, '<li>$1</li>')
    .replace(/(<li>.*<\/li>)/s, '<ul>$1</ul>')
    .replace(/\n/gim, '<br>')
}

// 格式化时间
function formatTime(timestamp: string): string {
  const date = new Date(timestamp)
  return date.toLocaleTimeString('zh-CN')
}

// 显示消息
function showMessage(msg: string) {
  // 可以集成一个通知组件
  console.log(msg)
}

// 初始化
onMounted(() => {
  testConnection()
})
</script>

<style scoped>
.ai-analysis {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

h2 {
  color: #333;
  margin-bottom: 20px;
  font-size: 24px;
}

/* 状态卡片 */
.status-card {
  background: white;
  border-radius: 8px;
  padding: 15px;
  margin-bottom: 20px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.status-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.status-title {
  font-weight: bold;
  font-size: 16px;
}

.status-badge {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 500;
}

.status-badge.connected {
  background: #e7f5e7;
  color: #2e7d2e;
}

.status-badge.connecting {
  background: #fff3cd;
  color: #856404;
}

.status-badge.disconnected {
  background: #f8d7da;
  color: #721c24;
}

.status-info p {
  margin: 5px 0;
  font-size: 14px;
  color: #666;
}

/* 控制面板 */
.control-panel {
  background: white;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.control-panel h3 {
  margin-bottom: 15px;
  color: #333;
  font-size: 18px;
}

.controls-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 15px;
}

.control-group {
  display: flex;
  flex-direction: column;
}

.control-group label {
  margin-bottom: 5px;
  color: #666;
  font-size: 14px;
}

.control-group input[type="number"],
.control-group select {
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

.control-group input[type="checkbox"] {
  margin-right: 8px;
}

/* 操作按钮 */
.actions {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background: #007bff;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #0056b3;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-secondary:hover:not(:disabled) {
  background: #545b62;
}

.btn-info {
  background: #17a2b8;
  color: white;
}

.btn-info:hover:not(:disabled) {
  background: #117a8b;
}

.btn-warning {
  background: #ffc107;
  color: #212529;
}

.btn-warning:hover:not(:disabled) {
  background: #e0a800;
}

/* 加载状态 */
.loading {
  text-align: center;
  padding: 40px;
  background: white;
  border-radius: 8px;
  margin-bottom: 20px;
}

.spinner {
  width: 40px;
  height: 40px;
  margin: 0 auto 20px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #007bff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 分析结果 */
.results {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.results h3 {
  margin-bottom: 20px;
  color: #333;
  font-size: 20px;
}

.result-section {
  margin-bottom: 30px;
  padding-bottom: 20px;
  border-bottom: 1px solid #eee;
}

.result-section:last-child {
  border-bottom: none;
}

.result-section h4 {
  margin-bottom: 15px;
  color: #555;
  font-size: 16px;
}

.result-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 15px;
}

.result-item {
  display: flex;
  justify-content: space-between;
  padding: 10px;
  background: #f8f9fa;
  border-radius: 4px;
}

.result-item .label {
  color: #666;
  font-size: 14px;
}

.result-item .value {
  font-weight: bold;
  color: #333;
}

/* 报告内容 */
.report-meta {
  display: flex;
  gap: 10px;
  margin-bottom: 15px;
}

.badge {
  padding: 4px 12px;
  background: #e9ecef;
  border-radius: 20px;
  font-size: 12px;
  color: #495057;
}

.badge.cost {
  background: #fff3cd;
  color: #856404;
}

.report-content {
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
  font-size: 14px;
  line-height: 1.8;
  color: #333;
  max-height: 600px;
  overflow-y: auto;
}

.report-content h2 {
  font-size: 18px;
  margin-top: 20px;
  margin-bottom: 10px;
}

.report-content h3 {
  font-size: 16px;
  margin-top: 15px;
  margin-bottom: 10px;
}

.report-content h4 {
  font-size: 14px;
  margin-top: 10px;
  margin-bottom: 8px;
}

.report-content ul {
  padding-left: 20px;
  margin: 10px 0;
}

.report-content li {
  margin: 5px 0;
}

/* 视觉分析 */
.visual-analysis {
  padding: 15px;
  background: #f8f9fa;
  border-radius: 8px;
}

.model-tag {
  display: inline-block;
  margin-top: 10px;
  padding: 4px 8px;
  background: #007bff;
  color: white;
  border-radius: 4px;
  font-size: 12px;
}

/* 模型层级 */
.hierarchy-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.hierarchy-item {
  border: 1px solid #dee2e6;
  border-radius: 8px;
  overflow: hidden;
}

.hierarchy-header {
  display: flex;
  justify-content: space-between;
  padding: 10px 15px;
  background: #f8f9fa;
  border-bottom: 1px solid #dee2e6;
}

.hierarchy-header .level {
  font-weight: bold;
  color: #007bff;
}

.hierarchy-header .model {
  color: #666;
  font-size: 14px;
}

.hierarchy-result {
  padding: 15px;
  background: white;
}

.hierarchy-result pre {
  margin: 0;
  font-size: 12px;
  color: #333;
  white-space: pre-wrap;
}

/* 实时流结果 */
.stream-results {
  max-height: 300px;
  overflow-y: auto;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  padding: 10px;
}

.stream-point {
  display: grid;
  grid-template-columns: 120px 100px 80px 1fr;
  gap: 10px;
  padding: 8px;
  margin-bottom: 5px;
  background: #f8f9fa;
  border-radius: 4px;
  font-size: 14px;
}

.stream-point .time {
  color: #666;
}

.stream-point .value {
  font-weight: bold;
  color: #333;
}

.stream-point .status {
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
  text-align: center;
}

.stream-point .status.正常 {
  background: #d4edda;
  color: #155724;
}

.stream-point .status.警告 {
  background: #fff3cd;
  color: #856404;
}

.stream-point .status.危险 {
  background: #f8d7da;
  color: #721c24;
}

.stream-point .confidence {
  color: #6c757d;
  font-size: 12px;
}

/* 错误提示 */
.error {
  padding: 15px;
  background: #f8d7da;
  border: 1px solid #f5c6cb;
  border-radius: 8px;
  color: #721c24;
  margin-top: 20px;
}

.error p {
  margin: 0;
}
</style>