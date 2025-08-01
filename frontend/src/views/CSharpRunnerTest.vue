<template>
  <div class="csharp-runner-test">
    <div class="page-header">
      <h1>C# Runner 测试页面</h1>
      <p>JYUSB1601 在线编程与执行测试</p>
    </div>

    <!-- 服务状态 -->
    <div class="status-section">
      <h2>服务状态</h2>
      <div class="status-grid">
        <div class="status-card" :class="{ 'online': backendStatus, 'offline': !backendStatus }">
          <h3>后端服务</h3>
          <div class="status-indicator">
            <span class="dot"></span>
            {{ backendStatus ? '在线' : '离线' }}
          </div>
        </div>
        <div class="status-card" :class="{ 'online': runnerStatus, 'offline': !runnerStatus }">
          <h3>C# Runner</h3>
          <div class="status-indicator">
            <span class="dot"></span>
            {{ runnerStatus ? '在线' : '离线' }}
          </div>
        </div>
      </div>
    </div>

    <!-- 设备选择 -->
    <div class="device-section">
      <h2>支持的设备</h2>
      <div class="device-grid">
        <div 
          v-for="device in devices" 
          :key="device.type"
          class="device-card"
          :class="{ 'selected': selectedDevice?.type === device.type }"
          @click="selectDevice(device)"
        >
          <h3>{{ device.name }}</h3>
          <p>{{ device.description }}</p>
          <div class="templates">
            <span v-for="template in device.templates" :key="template" class="template-tag">
              {{ template }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- 代码编辑器 -->
    <div v-if="selectedDevice" class="editor-section">
      <h2>代码编辑器</h2>
      <div class="editor-controls">
        <select v-model="selectedTemplate" @change="loadTemplate">
          <option value="">选择模板...</option>
          <option v-for="template in selectedDevice.templates" :key="template" :value="template">
            {{ template }} 模板
          </option>
        </select>
        <button @click="executeCode" :disabled="!code || executing" class="execute-btn">
          <span v-if="executing">执行中...</span>
          <span v-else>执行代码</span>
        </button>
      </div>
      
      <div class="code-editor">
        <textarea 
          v-model="code" 
          placeholder="请选择模板或输入 C# 代码..."
          rows="20"
          spellcheck="false"
        ></textarea>
      </div>
    </div>

    <!-- 执行结果 -->
    <div v-if="executionResult" class="result-section">
      <h2>执行结果</h2>
      <div class="result-card" :class="executionResult.success ? 'success' : 'error'">
        <div class="result-header">
          <h3>{{ executionResult.success ? '执行成功' : '执行失败' }}</h3>
          <span class="timestamp">{{ new Date(executionResult.timestamp).toLocaleString() }}</span>
        </div>
        
        <div v-if="executionResult.success" class="result-content">
          <h4>返回数据：</h4>
          <pre>{{ JSON.stringify(executionResult.data, null, 2) }}</pre>
          
          <h4>控制台输出：</h4>
          <pre class="console-output">{{ executionResult.output }}</pre>
        </div>
        
        <div v-else class="error-content">
          <h4>错误信息：</h4>
          <pre class="error-message">{{ executionResult.error }}</pre>
          
          <h4>编译错误：</h4>
          <pre class="compile-errors">{{ executionResult.compileErrors }}</pre>
        </div>
      </div>
    </div>

    <!-- 执行历史 -->
    <div v-if="executionHistory.length > 0" class="history-section">
      <h2>执行历史</h2>
      <div class="history-list">
        <div 
          v-for="(item, index) in executionHistory" 
          :key="index"
          class="history-item"
          :class="{ 'success': item.success, 'error': !item.success }"
        >
          <div class="history-header">
            <span class="status">{{ item.success ? '✓' : '✗' }}</span>
            <span class="device">{{ item.device }}</span>
            <span class="template">{{ item.template }}</span>
            <span class="time">{{ new Date(item.timestamp).toLocaleTimeString() }}</span>
          </div>
          <div class="history-preview">
            {{ item.success ? '执行成功' : item.error?.substring(0, 100) + '...' }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'

interface Device {
  type: string
  name: string
  description: string
  templates: string[]
}

interface ExecutionResult {
  success: boolean
  data?: any
  output?: string
  error?: string
  compileErrors?: string
  timestamp: string
}

interface HistoryItem {
  device: string
  template: string
  success: boolean
  error?: string
  timestamp: string
}

const backendStatus = ref(false)
const runnerStatus = ref(false)
const devices = ref<Device[]>([])
const selectedDevice = ref<Device | null>(null)
const selectedTemplate = ref('')
const code = ref('')
const executing = ref(false)
const executionResult = ref<ExecutionResult | null>(null)
const executionHistory = ref<HistoryItem[]>([])

const checkBackendStatus = async () => {
  try {
    const response = await fetch('http://localhost:5001/api/CSharpRunner/devices')
    backendStatus.value = response.ok
    if (response.ok) {
      devices.value = await response.json()
    }
  } catch (error) {
    console.error('Backend status check failed:', error)
    backendStatus.value = false
  }
}

const checkRunnerStatus = async () => {
  try {
    const response = await fetch('http://localhost:5050/api/status')
    runnerStatus.value = response.ok
  } catch (error) {
    console.error('Runner status check failed:', error)
    runnerStatus.value = false
  }
}

const selectDevice = (device: Device) => {
  selectedDevice.value = device
  selectedTemplate.value = ''
  code.value = ''
  executionResult.value = null
}

const loadTemplate = async () => {
  if (!selectedDevice.value || !selectedTemplate.value) return
  
  try {
    const response = await fetch(
      `http://localhost:5001/api/CSharpRunner/template/${selectedDevice.value.type}/${selectedTemplate.value}`
    )
    if (response.ok) {
      const template = await response.json()
      code.value = template.code
    }
  } catch (error) {
    console.error('Failed to load template:', error)
  }
}

const executeCode = async () => {
  if (!selectedDevice.value || !code.value) return
  
  executing.value = true
  executionResult.value = null
  
  try {
    const response = await fetch('http://localhost:5001/api/CSharpRunner/execute-instrument', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        deviceType: selectedDevice.value.type,
        code: code.value,
        timeout: 30
      })
    })
    
    const result = await response.json()
    executionResult.value = {
      success: result.success,
      data: result.data,
      output: result.output,
      error: result.error,
      compileErrors: result.compileErrors,
      timestamp: new Date().toISOString()
    }
    
    // 添加到历史记录
    executionHistory.value.unshift({
      device: selectedDevice.value.type,
      template: selectedTemplate.value,
      success: result.success,
      error: result.error,
      timestamp: new Date().toISOString()
    })
    
    // 限制历史记录数量
    if (executionHistory.value.length > 10) {
      executionHistory.value = executionHistory.value.slice(0, 10)
    }
    
  } catch (error) {
    console.error('Execution failed:', error)
    executionResult.value = {
      success: false,
      error: '网络请求失败：' + error,
      timestamp: new Date().toISOString()
    }
  } finally {
    executing.value = false
  }
}

onMounted(async () => {
  await checkBackendStatus()
  await checkRunnerStatus()
})
</script>

<style scoped>
.csharp-runner-test {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.page-header {
  text-align: center;
  margin-bottom: 30px;
}

.page-header h1 {
  color: #2c3e50;
  margin-bottom: 10px;
}

.page-header p {
  color: #7f8c8d;
  font-size: 16px;
}

.status-section, .device-section, .editor-section, .result-section, .history-section {
  margin-bottom: 30px;
  padding: 20px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

h2 {
  color: #2c3e50;
  border-bottom: 2px solid #3498db;
  padding-bottom: 8px;
  margin-bottom: 20px;
}

.status-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
}

.status-card {
  padding: 15px;
  border-radius: 6px;
  text-align: center;
  border: 2px solid #ddd;
}

.status-card.online {
  border-color: #27ae60;
  background-color: #d5f4e6;
}

.status-card.offline {
  border-color: #e74c3c;
  background-color: #fadadd;
}

.status-indicator {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin-top: 10px;
}

.status-indicator .dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.status-card.online .dot {
  background-color: #27ae60;
}

.status-card.offline .dot {
  background-color: #e74c3c;
}

.device-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
}

.device-card {
  padding: 20px;
  border: 2px solid #ddd;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.device-card:hover {
  border-color: #3498db;
  transform: translateY(-2px);
}

.device-card.selected {
  border-color: #2980b9;
  background-color: #ebf3fd;
}

.device-card h3 {
  color: #2c3e50;
  margin-bottom: 10px;
}

.device-card p {
  color: #7f8c8d;
  margin-bottom: 15px;
}

.templates {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.template-tag {
  background-color: #3498db;
  color: white;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
}

.editor-controls {
  display: flex;
  gap: 15px;
  margin-bottom: 15px;
  align-items: center;
}

.editor-controls select {
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

.execute-btn {
  background-color: #27ae60;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.3s ease;
}

.execute-btn:hover:not(:disabled) {
  background-color: #229954;
}

.execute-btn:disabled {
  background-color: #bdc3c7;
  cursor: not-allowed;
}

.code-editor {
  border: 1px solid #ddd;
  border-radius: 4px;
  overflow: hidden;
}

.code-editor textarea {
  width: 100%;
  border: none;
  padding: 15px;
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  font-size: 14px;
  line-height: 1.5;
  background-color: #f8f9fa;
  resize: vertical;
  outline: none;
}

.result-card {
  border-radius: 6px;
  padding: 20px;
  border-left: 4px solid;
}

.result-card.success {
  border-left-color: #27ae60;
  background-color: #d5f4e6;
}

.result-card.error {
  border-left-color: #e74c3c;
  background-color: #fadadd;
}

.result-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.result-header h3 {
  margin: 0;
  color: #2c3e50;
}

.timestamp {
  color: #7f8c8d;
  font-size: 14px;
}

.result-content h4, .error-content h4 {
  color: #2c3e50;
  margin: 15px 0 8px 0;
}

.result-content pre, .error-content pre {
  background-color: #2c3e50;
  color: #ecf0f1;
  padding: 15px;
  border-radius: 4px;
  overflow-x: auto;
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  font-size: 13px;
  line-height: 1.4;
}

.console-output {
  background-color: #34495e !important;
}

.error-message {
  background-color: #c0392b !important;
}

.compile-errors {
  background-color: #e67e22 !important;
}

.history-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.history-item {
  padding: 15px;
  border-radius: 6px;
  border-left: 4px solid;
}

.history-item.success {
  border-left-color: #27ae60;
  background-color: #d5f4e6;
}

.history-item.error {
  border-left-color: #e74c3c;
  background-color: #fadadd;
}

.history-header {
  display: flex;
  gap: 15px;
  align-items: center;
  margin-bottom: 8px;
}

.history-header .status {
  font-weight: bold;
  font-size: 16px;
}

.history-item.success .status {
  color: #27ae60;
}

.history-item.error .status {
  color: #e74c3c;
}

.history-header .device,
.history-header .template {
  background-color: rgba(0,0,0,0.1);
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 12px;
}

.history-header .time {
  color: #7f8c8d;
  font-size: 12px;
  margin-left: auto;
}

.history-preview {
  color: #2c3e50;
  font-size: 14px;
}
</style>
