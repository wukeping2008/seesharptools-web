<template>
  <div class="ai-test-platform">
    <!-- 页面标题 -->
    <div class="page-header">
      <h1>AI智能测试平台</h1>
      <p class="subtitle">基于自然语言的智能测试代码生成和执行平台</p>
    </div>

    <!-- 主要内容区域 -->
    <div class="platform-content">
      <!-- 左侧面板 - 需求输入和模板选择 -->
      <div class="left-panel">
        <!-- 需求输入区域 -->
        <div class="requirement-section">
          <h3>测试需求描述</h3>
          <div class="input-group">
            <label>请用中文描述您的测试需求：</label>
            <textarea
              v-model="testRequirement"
              placeholder="例如：我需要对JY5500信号发生器进行THD测试，频率范围1kHz，分析2-10次谐波..."
              rows="4"
              class="requirement-input"
            ></textarea>
          </div>

          <!-- 设备选择 -->
          <div class="device-selection">
            <label>目标设备：</label>
            <select v-model="selectedDevice" class="device-select">
              <option value="">自动识别</option>
              <option value="JY5500">JY5500 信号发生器</option>
              <option value="JYUSB1601">JYUSB1601 数据采集卡</option>
              <option value="通用">通用设备</option>
            </select>
          </div>

          <!-- 测试类型 -->
          <div class="test-type-selection">
            <label>测试类型：</label>
            <div class="test-type-buttons">
              <button
                v-for="type in testTypes"
                :key="type.id"
                :class="['type-btn', { active: selectedTestType === type.id }]"
                @click="selectedTestType = type.id"
              >
                {{ type.name }}
              </button>
            </div>
          </div>

          <!-- 生成按钮 -->
          <button
            @click="generateTestCode"
            :disabled="isGenerating || !testRequirement.trim()"
            class="generate-btn"
          >
            <span v-if="isGenerating" class="loading-spinner"></span>
            {{ isGenerating ? '正在生成...' : '生成测试代码' }}
          </button>
        </div>

        <!-- 模板选择区域 -->
        <div class="template-section">
          <h3>推荐模板</h3>
          <div class="template-list">
            <div
              v-for="template in recommendedTemplates"
              :key="template.id"
              :class="['template-item', { selected: selectedTemplate === template.id }]"
              @click="selectTemplate(template)"
            >
              <div class="template-header">
                <h4>{{ template.name }}</h4>
                <span class="template-rating">⭐ {{ template.rating }}</span>
              </div>
              <p class="template-desc">{{ template.description }}</p>
              <div class="template-tags">
                <span
                  v-for="tag in template.tags"
                  :key="tag"
                  class="tag"
                >
                  {{ tag }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧面板 - 代码编辑和执行 -->
      <div class="right-panel">
        <!-- 代码编辑器 -->
        <div class="code-editor-section">
          <div class="editor-header">
            <h3>测试代码</h3>
            <div class="editor-actions">
              <button @click="copyCode" class="action-btn">
                📋 复制代码
              </button>
              <button @click="saveTemplate" class="action-btn">
                💾 保存为模板
              </button>
            </div>
          </div>
          
          <div class="editor-container">
            <textarea
              v-model="generatedCode"
              class="code-editor"
              placeholder="生成的测试代码将显示在这里..."
              rows="20"
            ></textarea>
          </div>

          <!-- 代码质量信息 -->
          <div v-if="codeQuality" class="code-quality">
            <h4>代码质量评估</h4>
            <div class="quality-score">
              <span class="score">{{ codeQuality.score }}/100</span>
              <div class="score-bar">
                <div 
                  class="score-fill" 
                  :style="{ width: `${codeQuality.score}%` }"
                ></div>
              </div>
            </div>
            
            <!-- 问题和建议 -->
            <div v-if="codeQuality.issues?.length" class="issues">
              <h5>⚠️ 发现的问题：</h5>
              <ul>
                <li v-for="issue in codeQuality.issues" :key="issue">{{ issue }}</li>
              </ul>
            </div>
            
            <div v-if="codeQuality.suggestions?.length" class="suggestions">
              <h5>💡 改进建议：</h5>
              <ul>
                <li v-for="suggestion in codeQuality.suggestions" :key="suggestion">{{ suggestion }}</li>
              </ul>
            </div>
          </div>
        </div>

        <!-- 测试执行区域 -->
        <div class="execution-section">
          <div class="execution-header">
            <h3>测试执行</h3>
            <button
              @click="executeTest"
              :disabled="isExecuting || !generatedCode.trim()"
              class="execute-btn"
            >
              <span v-if="isExecuting" class="loading-spinner"></span>
              {{ isExecuting ? '执行中...' : '▶️ 执行测试' }}
            </button>
          </div>

          <!-- 执行结果 -->
          <div v-if="executionResult" class="execution-result">
            <div class="result-header">
              <h4>执行结果</h4>
              <span :class="['result-status', executionResult.success ? 'success' : 'error']">
                {{ executionResult.success ? '✅ 成功' : '❌ 失败' }}
              </span>
            </div>

            <!-- 结果数据 -->
            <div v-if="executionResult.data" class="result-content">
              <div class="result-summary">
                <h5>测试摘要</h5>
                <div class="summary-grid">
                  <div class="summary-item">
                    <span class="label">设备类型:</span>
                    <span class="value">{{ executionResult.data.deviceType }}</span>
                  </div>
                  <div class="summary-item">
                    <span class="label">分析类型:</span>
                    <span class="value">{{ executionResult.data.analysisType }}</span>
                  </div>
                  <div class="summary-item">
                    <span class="label">执行时间:</span>
                    <span class="value">{{ formatTimestamp(executionResult.data.timestamp) }}</span>
                  </div>
                </div>
              </div>

              <!-- 结果图表 -->
              <div v-if="executionResult.data.spectrumData" class="result-chart">
                <h5>频谱分析结果</h5>
                <canvas ref="chartCanvas" width="400" height="200"></canvas>
              </div>
            </div>

            <!-- 错误信息 -->
            <div v-if="executionResult.error" class="error-info">
              <h5>错误信息</h5>
              <pre class="error-details">{{ executionResult.error }}</pre>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, nextTick } from 'vue'
import { AITestService } from '@/services/AITestService'
import type { CodeQuality, TestExecutionResult } from '@/types/ai'

// 响应式数据
const testRequirement = ref('')
const selectedDevice = ref('')
const selectedTestType = ref('')
const selectedTemplate = ref('')
const generatedCode = ref('')
const isGenerating = ref(false)
const isExecuting = ref(false)
const codeQuality = ref<CodeQuality | null>(null)
const executionResult = ref<any>(null)
const chartCanvas = ref<HTMLCanvasElement>()

// 测试类型选项
const testTypes = ref([
  { id: 'vibration', name: '振动测试' },
  { id: 'electrical', name: '电气测试' },
  { id: 'temperature', name: '温度测量' },
  { id: 'signal', name: '信号分析' },
  { id: 'custom', name: '自定义' }
])

// 推荐模板
const recommendedTemplates = ref([
  {
    id: 'vibration_bearing_analysis',
    name: '轴承故障振动分析',
    description: '检测轴承故障的振动频谱分析程序，支持故障特征频率识别',
    rating: 4.8,
    tags: ['振动测试', 'FFT分析', '故障诊断']
  },
  {
    id: 'electrical_thd_analysis',
    name: '信号THD分析',
    description: '测试信号发生器的总谐波失真(THD)，分析各次谐波成分',
    rating: 4.6,
    tags: ['电气测试', 'THD分析', '信号质量']
  },
  {
    id: 'temperature_monitoring',
    name: '多点温度监控',
    description: '实时监控多个温度传感器，支持趋势分析和报警',
    rating: 4.5,
    tags: ['温度测量', '多点监控', '趋势分析']
  }
])

// 生成测试代码
const generateTestCode = async () => {
  if (!testRequirement.value.trim()) return

  isGenerating.value = true
  codeQuality.value = null
  executionResult.value = null

  try {
    const response = await AITestService.generateTestCode({
      requirement: testRequirement.value,
      deviceType: selectedDevice.value,
      testType: selectedTestType.value,
      templateId: selectedTemplate.value
    })

    if (response.success) {
      generatedCode.value = response.code
      codeQuality.value = response.quality
      
      // 显示成功消息
      showMessage('代码生成成功！', 'success')
    } else {
      showMessage('代码生成失败：' + response.error, 'error')
    }
  } catch (error) {
    console.error('生成代码时出错:', error)
    showMessage('代码生成失败，请检查网络连接', 'error')
  } finally {
    isGenerating.value = false
  }
}

// 选择模板
const selectTemplate = (template: any) => {
  selectedTemplate.value = template.id
  
  // 根据模板设置相关字段
  if (template.id.includes('vibration')) {
    selectedTestType.value = 'vibration'
    selectedDevice.value = 'JYUSB1601'
  } else if (template.id.includes('electrical')) {
    selectedTestType.value = 'electrical'
    selectedDevice.value = 'JY5500'
  } else if (template.id.includes('temperature')) {
    selectedTestType.value = 'temperature'
    selectedDevice.value = 'JYUSB1601'
  }
}

// 执行测试
const executeTest = async () => {
  if (!generatedCode.value.trim()) return

  isExecuting.value = true
  executionResult.value = null

  try {
    const response = await AITestService.executeTestCode(generatedCode.value)

    executionResult.value = {
      success: response.success,
      data: response.result,
      error: response.error
    }

    if (response.success) {
      showMessage('测试执行成功！', 'success')
      
      // 如果有频谱数据，绘制图表
      if (response.result?.spectrumData) {
        await nextTick()
        drawChart(response.result.spectrumData)
      }
    } else {
      showMessage('测试执行失败：' + response.error, 'error')
    }
  } catch (error) {
    console.error('执行测试时出错:', error)
    executionResult.value = {
      success: false,
      error: '测试执行失败，请检查网络连接'
    }
    showMessage('测试执行失败，请检查网络连接', 'error')
  } finally {
    isExecuting.value = false
  }
}

// 复制代码
const copyCode = async () => {
  if (!generatedCode.value.trim()) return

  try {
    await navigator.clipboard.writeText(generatedCode.value)
    showMessage('代码已复制到剪贴板', 'success')
  } catch (error) {
    console.error('复制失败:', error)
    showMessage('复制失败，请手动选择复制', 'error')
  }
}

// 保存为模板
const saveTemplate = async () => {
  if (!generatedCode.value.trim()) return

  const templateName = prompt('请输入模板名称:')
  if (!templateName) return

  try {
    const response = await AITestService.saveUserTemplate({
      name: templateName,
      description: `基于需求生成: ${testRequirement.value.substring(0, 50)}...`,
      deviceType: selectedDevice.value,
      testType: selectedTestType.value,
      code: generatedCode.value
    })

    if (response.success) {
      showMessage('模板保存成功！', 'success')
    } else {
      showMessage('模板保存失败：' + response.error, 'error')
    }
  } catch (error) {
    console.error('保存模板时出错:', error)
    showMessage('模板保存失败，请检查网络连接', 'error')
  }
}

// 绘制图表
const drawChart = (spectrumData: number[]) => {
  if (!chartCanvas.value) return

  const ctx = chartCanvas.value.getContext('2d')
  if (!ctx) return

  const width = chartCanvas.value.width
  const height = chartCanvas.value.height

  // 清空画布
  ctx.clearRect(0, 0, width, height)

  // 绘制坐标轴
  ctx.strokeStyle = '#333'
  ctx.lineWidth = 1
  ctx.beginPath()
  ctx.moveTo(50, height - 30)
  ctx.lineTo(width - 30, height - 30)
  ctx.moveTo(50, 20)
  ctx.lineTo(50, height - 30)
  ctx.stroke()

  // 绘制频谱数据
  if (spectrumData.length > 0) {
    const maxVal = Math.max(...spectrumData)
    const minVal = Math.min(...spectrumData)
    const range = maxVal - minVal || 1

    ctx.strokeStyle = '#409eff'
    ctx.lineWidth = 2
    ctx.beginPath()

    spectrumData.forEach((value, index) => {
      const x = 50 + (index / (spectrumData.length - 1)) * (width - 80)
      const y = height - 30 - ((value - minVal) / range) * (height - 50)
      
      if (index === 0) {
        ctx.moveTo(x, y)
      } else {
        ctx.lineTo(x, y)
      }
    })

    ctx.stroke()
  }

  // 添加标签
  ctx.fillStyle = '#666'
  ctx.font = '12px Arial'
  ctx.fillText('频率 (Hz)', width / 2 - 30, height - 5)
  ctx.save()
  ctx.translate(15, height / 2)
  ctx.rotate(-Math.PI / 2)
  ctx.fillText('幅度 (dB)', -30, 0)
  ctx.restore()
}

// 格式化时间戳
const formatTimestamp = (timestamp: string) => {
  return new Date(timestamp).toLocaleString('zh-CN')
}

// 显示消息
const showMessage = (message: string, type: 'success' | 'error' | 'info' = 'info') => {
  // 简单的消息显示，可以后续改为更好的通知组件
  const className = type === 'success' ? 'success' : type === 'error' ? 'error' : 'info'
  console.log(`[${type.toUpperCase()}] ${message}`)
  
  // 可以添加toast通知
  if (type === 'error') {
    alert(`错误: ${message}`)
  }
}

// 组件挂载
onMounted(() => {
  // 初始化逻辑
  console.log('AI测试平台已加载')
})
</script>

<style scoped>
.ai-test-platform {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  text-align: center;
  margin-bottom: 30px;
}

.page-header h1 {
  color: #2c3e50;
  margin-bottom: 10px;
}

.subtitle {
  color: #7f8c8d;
  font-size: 16px;
}

.platform-content {
  display: grid;
  grid-template-columns: 400px 1fr;
  gap: 30px;
  min-height: 600px;
}

/* 左侧面板样式 */
.left-panel {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 20px;
  border: 1px solid #e9ecef;
}

.requirement-section,
.template-section {
  margin-bottom: 30px;
}

.requirement-section h3,
.template-section h3 {
  color: #2c3e50;
  margin-bottom: 15px;
  border-bottom: 2px solid #3498db;
  padding-bottom: 5px;
}

.input-group {
  margin-bottom: 15px;
}

.input-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: 500;
  color: #495057;
}

.requirement-input {
  width: 100%;
  padding: 10px;
  border: 1px solid #ced4da;
  border-radius: 4px;
  font-size: 14px;
  resize: vertical;
}

.device-selection,
.test-type-selection {
  margin-bottom: 15px;
}

.device-select {
  width: 100%;
  padding: 8px;
  border: 1px solid #ced4da;
  border-radius: 4px;
}

.test-type-buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 8px;
}

.type-btn {
  padding: 6px 12px;
  border: 1px solid #dee2e6;
  background: white;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
}

.type-btn:hover {
  background: #e9ecef;
}

.type-btn.active {
  background: #007bff;
  color: white;
  border-color: #007bff;
}

.generate-btn {
  width: 100%;
  padding: 12px;
  background: #28a745;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
  transition: background 0.2s;
}

.generate-btn:hover:not(:disabled) {
  background: #218838;
}

.generate-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.template-list {
  max-height: 300px;
  overflow-y: auto;
}

.template-item {
  background: white;
  border: 1px solid #dee2e6;
  border-radius: 4px;
  padding: 12px;
  margin-bottom: 10px;
  cursor: pointer;
  transition: all 0.2s;
}

.template-item:hover {
  border-color: #007bff;
  box-shadow: 0 2px 4px rgba(0,123,255,0.1);
}

.template-item.selected {
  border-color: #007bff;
  background: #f8f9ff;
}

.template-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 5px;
}

.template-header h4 {
  margin: 0;
  color: #2c3e50;
  font-size: 14px;
}

.template-rating {
  font-size: 12px;
  color: #f39c12;
}

.template-desc {
  font-size: 12px;
  color: #6c757d;
  margin: 5px 0;
}

.template-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.tag {
  background: #e9ecef;
  color: #495057;
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 11px;
}

/* 右侧面板样式 */
.right-panel {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.code-editor-section,
.execution-section {
  background: white;
  border: 1px solid #dee2e6;
  border-radius: 8px;
  padding: 20px;
}

.editor-header,
.execution-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.editor-header h3,
.execution-header h3 {
  margin: 0;
  color: #2c3e50;
}

.editor-actions {
  display: flex;
  gap: 10px;
}

.action-btn {
  padding: 6px 12px;
  background: #6c757d;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
}

.action-btn:hover {
  background: #5a6268;
}

.code-editor {
  width: 100%;
  min-height: 400px;
  padding: 15px;
  border: 1px solid #dee2e6;
  border-radius: 4px;
  font-family: 'Courier New', monospace;
  font-size: 13px;
  line-height: 1.4;
  resize: vertical;
}

.code-quality {
  margin-top: 15px;
  padding: 15px;
  background: #f8f9fa;
  border-radius: 4px;
}

.quality-score {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}

.score {
  font-weight: bold;
  color: #28a745;
}

.score-bar {
  flex: 1;
  height: 8px;
  background: #e9ecef;
  border-radius: 4px;
  overflow: hidden;
}

.score-fill {
  height: 100%;
  background: linear-gradient(to right, #dc3545, #ffc107, #28a745);
  transition: width 0.3s;
}

.issues,
.suggestions {
  margin-top: 10px;
}

.issues h5,
.suggestions h5 {
  margin: 0 0 5px 0;
  font-size: 14px;
}

.issues ul,
.suggestions ul {
  margin: 0;
  padding-left: 20px;
  font-size: 13px;
}

.execute-btn {
  padding: 10px 20px;
  background: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

.execute-btn:hover:not(:disabled) {
  background: #0056b3;
}

.execute-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.execution-result {
  margin-top: 15px;
}

.result-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.result-status.success {
  color: #28a745;
  font-weight: bold;
}

.result-status.error {
  color: #dc3545;
  font-weight: bold;
}

.result-content {
  background: #f8f9fa;
  padding: 15px;
  border-radius: 4px;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 10px;
  margin-bottom: 15px;
}

.summary-item {
  display: flex;
  justify-content: space-between;
}

.summary-item .label {
  font-weight: 500;
  color: #495057;
}

.summary-item .value {
  color: #2c3e50;
}

.result-chart {
  margin-top: 15px;
}

.result-chart h5 {
  margin-bottom: 10px;
}

.error-info {
  background: #f8d7da;
  border: 1px solid #f5c6cb;
  padding: 15px;
  border-radius: 4px;
}

.error-details {
  background: #fff;
  padding: 10px;
  border-radius: 4px;
  margin-top: 10px;
  font-size: 12px;
  overflow-x: auto;
}

.loading-spinner {
  display: inline-block;
  width: 12px;
  height: 12px;
  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-right: 5px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .platform-content {
    grid-template-columns: 350px 1fr;
  }
}

@media (max-width: 768px) {
  .platform-content {
    grid-template-columns: 1fr;
    gap: 20px;
  }
  
  .left-panel {
    order: 2;
  }
  
  .right-panel {
    order: 1;
  }
}
</style>
