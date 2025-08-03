<template>
  <div class="comprehensive-test-view">
    <!-- 页面标题 -->
    <div class="page-header">
      <h1 class="page-title">
        <span class="icon">🧪</span>
        AI功能综合测试平台
      </h1>
      <p class="page-subtitle">独立的端到端AI功能验证仪表板</p>
    </div>

    <!-- 系统状态卡片 -->
    <div class="status-cards">
      <div class="status-card">
        <div class="status-icon">🖥️</div>
        <div class="status-info">
          <span class="status-label">系统状态</span>
          <span :class="['status-value', systemStatus.overall]">
            {{ systemStatus.overall === 'healthy' ? '正常' : '异常' }}
          </span>
        </div>
      </div>
      <div class="status-card">
        <div class="status-icon">🔌</div>
        <div class="status-info">
          <span class="status-label">硬件设备</span>
          <span class="status-value">{{ systemStatus.devices }}个</span>
        </div>
      </div>
      <div class="status-card">
        <div class="status-icon">🤖</div>
        <div class="status-info">
          <span class="status-label">AI服务</span>
          <span :class="['status-value', systemStatus.aiStatus === 'Connected' ? 'healthy' : 'error']">
            {{ systemStatus.aiStatus === 'Connected' ? '已连接' : '断开' }}
          </span>
        </div>
      </div>
    </div>

    <!-- 测试场景选择 -->
    <div class="test-scenarios">
      <h2>预设测试场景</h2>
      <div class="scenarios-grid">
        <div 
          v-for="scenario in scenarios" 
          :key="scenario.id"
          :class="['scenario-card', { 'selected': selectedScenario?.id === scenario.id }]"
          @click="selectScenario(scenario)"
        >
          <div class="scenario-icon">{{ scenario.icon }}</div>
          <div class="scenario-info">
            <h3>{{ scenario.name }}</h3>
            <p>{{ scenario.description }}</p>
            <div class="scenario-meta">
              <span class="device">{{ scenario.expectedDevice }}</span>
              <span class="time">~{{ scenario.estimatedTime }}秒</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 自定义测试输入 -->
    <div class="custom-test">
      <h2>自定义测试</h2>
      <div class="test-form">
        <div class="form-group">
          <label>测试需求描述</label>
          <textarea 
            v-model="customTest.userInput"
            placeholder="请详细描述您的测试需求，例如：使用JYUSB1601进行振动测试，采样频率25.6kHz..."
            rows="4"
          ></textarea>
        </div>
        <div class="form-row">
          <div class="form-group">
            <label>测试类型</label>
            <select v-model="customTest.testType">
              <option value="">请选择</option>
              <option value="vibration">振动分析</option>
              <option value="thd">THD分析</option>
              <option value="temperature">温度监控</option>
              <option value="power">电能分析</option>
              <option value="signal">信号生成</option>
              <option value="acquisition">数据采集</option>
            </select>
          </div>
          <div class="form-group">
            <label>目标设备</label>
            <select v-model="customTest.deviceType">
              <option value="">请选择</option>
              <option value="JYUSB1601">JYUSB1601</option>
              <option value="JY5500">JY5500</option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <!-- 开始测试按钮 -->
    <div class="test-controls">
      <button 
        :class="['start-btn', { 'disabled': !canStartTest }]"
        @click="startTest"
        :disabled="!canStartTest || isTestRunning"
      >
        <span v-if="!isTestRunning">🚀 开始综合测试</span>
        <span v-else>⏳ 测试进行中...</span>
      </button>
    </div>

    <!-- 测试进度 -->
    <div v-if="testSession" class="test-progress">
      <h2>测试进度</h2>
      <div class="progress-container">
        <div class="progress-header">
          <span class="session-id">会话ID: {{ testSession.sessionId }}</span>
          <span class="progress-percentage">{{ testSession.progress }}%</span>
        </div>
        <div class="progress-bar">
          <div 
            class="progress-fill" 
            :style="{ width: testSession.progress + '%' }"
          ></div>
        </div>
        <div class="current-phase">
          当前阶段: {{ testSession.currentPhase }}
        </div>
      </div>

      <!-- 阶段指示器 -->
      <div class="phases">
        <div 
          v-for="(phase, index) in testPhases" 
          :key="phase.name"
          :class="['phase', { 
            'completed': testSession.progress > phase.threshold,
            'active': testSession.progress >= phase.threshold && testSession.progress < (testPhases[index + 1]?.threshold || 100)
          }]"
        >
          <div class="phase-icon">{{ phase.icon }}</div>
          <span class="phase-name">{{ phase.name }}</span>
        </div>
      </div>
    </div>

    <!-- 测试结果 -->
    <div v-if="testResults" class="test-results">
      <h2>测试结果</h2>
      
      <!-- 总体结果 -->
      <div class="overall-results">
        <div class="score-card">
          <div class="score-value">{{ Math.round(testResults.overallScore) }}</div>
          <div class="score-label">总体评分</div>
        </div>
        <div class="test-summary">
          <div class="summary-item">
            <span class="label">测试项目</span>
            <span class="value">{{ testResults.testCount }}</span>
          </div>
          <div class="summary-item">
            <span class="label">通过项目</span>
            <span class="value">{{ testResults.passedTests }}</span>
          </div>
          <div class="summary-item">
            <span class="label">成功率</span>
            <span class="value">{{ Math.round((testResults.passedTests / testResults.testCount) * 100) }}%</span>
          </div>
        </div>
      </div>

      <!-- 详细结果 -->
      <div class="detailed-results">
        <!-- NLP测试结果 -->
        <div v-if="testResults.nlpTest" class="result-section">
          <h3>🧠 AI语义理解测试</h3>
          <div :class="['result-status', testResults.nlpTest.success ? 'success' : 'error']">
            {{ testResults.nlpTest.success ? '✅ 通过' : '❌ 失败' }}
          </div>
          <div class="result-details">
            <div class="detail-item">
              <span>处理时间:</span>
              <span>{{ testResults.nlpTest.processingTime }}ms</span>
            </div>
            <div class="detail-item">
              <span>准确度评分:</span>
              <span>{{ testResults.nlpTest.accuracyScore }}%</span>
            </div>
            <div v-if="testResults.nlpTest.extractedParameters" class="extracted-params">
              <h4>提取参数:</h4>
              <div 
                v-for="(value, key) in testResults.nlpTest.extractedParameters" 
                :key="key"
                class="param-item"
              >
                <span class="param-key">{{ key }}:</span>
                <span class="param-value">{{ value }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 代码生成测试结果 -->
        <div v-if="testResults.codeGenerationTest" class="result-section">
          <h3>⚙️ 智能代码生成测试</h3>
          <div :class="['result-status', testResults.codeGenerationTest.success ? 'success' : 'error']">
            {{ testResults.codeGenerationTest.success ? '✅ 通过' : '❌ 失败' }}
          </div>
          <div class="result-details">
            <div class="detail-item">
              <span>处理时间:</span>
              <span>{{ testResults.codeGenerationTest.processingTime }}ms</span>
            </div>
            <div class="detail-item">
              <span>质量评分:</span>
              <span>{{ testResults.codeGenerationTest.qualityScore }}%</span>
            </div>
            <div class="detail-item">
              <span>复杂度:</span>
              <span>{{ testResults.codeGenerationTest.complexityLevel }}</span>
            </div>
            <div v-if="testResults.codeGenerationTest.generatedCode" class="code-preview">
              <h4>生成代码预览:</h4>
              <pre>{{ testResults.codeGenerationTest.generatedCode.substring(0, 500) }}...</pre>
            </div>
          </div>
        </div>

        <!-- 安全验证测试结果 -->
        <div v-if="testResults.safetyValidationTest" class="result-section">
          <h3>🛡️ 代码安全验证测试</h3>
          <div :class="['result-status', testResults.safetyValidationTest.success ? 'success' : 'error']">
            {{ testResults.safetyValidationTest.success ? '✅ 通过' : '❌ 失败' }}
          </div>
          <div class="result-details">
            <div class="detail-item">
              <span>风险等级:</span>
              <span>{{ testResults.safetyValidationTest.riskLevel }}</span>
            </div>
            <div class="detail-item">
              <span>发现问题:</span>
              <span>{{ testResults.safetyValidationTest.issuesFound }}个</span>
            </div>
          </div>
        </div>

        <!-- 硬件模拟测试结果 -->
        <div v-if="testResults.hardwareSimulationTest" class="result-section">
          <h3>🔧 硬件模拟测试</h3>
          <div :class="['result-status', testResults.hardwareSimulationTest.success ? 'success' : 'error']">
            {{ testResults.hardwareSimulationTest.success ? '✅ 通过' : '❌ 失败' }}
          </div>
          <div class="result-details">
            <div class="detail-item">
              <span>设备类型:</span>
              <span>{{ testResults.hardwareSimulationTest.deviceType }}</span>
            </div>
            <div class="detail-item">
              <span>设备状态:</span>
              <span>{{ testResults.hardwareSimulationTest.deviceStatus }}</span>
            </div>
            <div class="detail-item">
              <span>连接延迟:</span>
              <span>{{ testResults.hardwareSimulationTest.connectionLatency }}ms</span>
            </div>
          </div>
        </div>

        <!-- 代码执行测试结果 -->
        <div v-if="testResults.codeExecutionTest" class="result-section">
          <h3>🚀 代码执行测试</h3>
          <div :class="['result-status', testResults.codeExecutionTest.success ? 'success' : 'error']">
            {{ testResults.codeExecutionTest.success ? '✅ 通过' : '❌ 失败' }}
          </div>
          <div class="result-details">
            <div class="detail-item">
              <span>执行时间:</span>
              <span>{{ testResults.codeExecutionTest.executionTime }}ms</span>
            </div>
            <div v-if="testResults.codeExecutionTest.output" class="execution-output">
              <h4>执行输出:</h4>
              <pre>{{ testResults.codeExecutionTest.output }}</pre>
            </div>
          </div>
        </div>
      </div>

      <!-- 可视化数据 -->
      <div v-if="testResults.visualizationData" class="visualization-section">
        <h3>📊 数据可视化</h3>
        <div class="charts-container">
          <div class="chart-card">
            <h4>时域波形</h4>
            <canvas ref="waveformChart" width="400" height="200"></canvas>
          </div>
          <div class="chart-card">
            <h4>频谱分析</h4>
            <canvas ref="spectrumChart" width="400" height="200"></canvas>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch, nextTick } from 'vue'

// 响应式数据
const systemStatus = reactive({
  overall: 'healthy',
  devices: 0,
  aiStatus: 'Connected'
})

const scenarios = ref([])
const selectedScenario = ref(null)
const customTest = reactive({
  userInput: '',
  testType: '',
  deviceType: ''
})

const testSession = ref(null)
const testResults = ref(null)
const isTestRunning = ref(false)

// 测试阶段配置
const testPhases = [
  { name: 'AI语义理解', icon: '🧠', threshold: 10 },
  { name: '智能代码生成', icon: '⚙️', threshold: 30 },
  { name: '安全验证', icon: '🛡️', threshold: 50 },
  { name: '硬件模拟', icon: '🔧', threshold: 70 },
  { name: '代码执行', icon: '🚀', threshold: 90 }
]

// 计算属性
const canStartTest = computed(() => {
  return selectedScenario.value || (customTest.userInput.trim() && customTest.testType && customTest.deviceType)
})

// API基础URL
const API_BASE = 'http://localhost:5000/api'

// 方法
const loadSystemStatus = async () => {
  try {
    const response = await fetch(`${API_BASE}/ComprehensiveTest/system-status`)
    if (response.ok) {
      const data = await response.json()
      systemStatus.overall = 'healthy'
      systemStatus.devices = data.hardware?.availableDevices || 0
      systemStatus.aiStatus = data.services?.aiApiStatus || 'Disconnected'
    }
  } catch (error) {
    console.error('获取系统状态失败:', error)
    systemStatus.overall = 'error'
  }
}

const loadScenarios = async () => {
  try {
    const response = await fetch(`${API_BASE}/ComprehensiveTest/scenarios`)
    if (response.ok) {
      scenarios.value = await response.json()
    }
  } catch (error) {
    console.error('获取测试场景失败:', error)
  }
}

const selectScenario = (scenario) => {
  selectedScenario.value = scenario
  // 清空自定义测试输入
  customTest.userInput = scenario.requirement
  customTest.testType = scenario.id.split('_')[0]
  customTest.deviceType = scenario.expectedDevice
}

const startTest = async () => {
  if (!canStartTest.value || isTestRunning.value) return

  try {
    isTestRunning.value = true
    testResults.value = null

    const testRequest = selectedScenario.value ? {
      userInput: selectedScenario.value.requirement,
      testType: selectedScenario.value.id.split('_')[0],
      testObject: selectedScenario.value.name,
      deviceType: selectedScenario.value.expectedDevice
    } : {
      userInput: customTest.userInput,
      testType: customTest.testType,
      testObject: customTest.testType,
      deviceType: customTest.deviceType
    }

    const response = await fetch(`${API_BASE}/ComprehensiveTest/start`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(testRequest)
    })

    if (response.ok) {
      const result = await response.json()
      testSession.value = {
        sessionId: result.sessionId,
        progress: 0,
        currentPhase: '初始化',
        status: 'running'
      }

      // 开始轮询测试状态
      pollTestStatus(result.sessionId)
    } else {
      throw new Error('启动测试失败')
    }
  } catch (error) {
    console.error('启动测试错误:', error)
    isTestRunning.value = false
    alert('启动测试失败: ' + error.message)
  }
}

const pollTestStatus = async (sessionId) => {
  try {
    const response = await fetch(`${API_BASE}/ComprehensiveTest/status/${sessionId}`)
    if (response.ok) {
      const status = await response.json()
      
      testSession.value = {
        sessionId: status.sessionId,
        progress: status.progress,
        currentPhase: status.currentPhase,
        status: status.status
      }

      if (status.status === 'Completed' || status.status === 'Failed') {
        isTestRunning.value = false
        // 获取详细结果
        await loadTestResults(sessionId)
      } else if (status.status === 'Running') {
        // 继续轮询
        setTimeout(() => pollTestStatus(sessionId), 2000)
      }
    }
  } catch (error) {
    console.error('获取测试状态失败:', error)
    isTestRunning.value = false
  }
}

const loadTestResults = async (sessionId) => {
  try {
    const response = await fetch(`${API_BASE}/ComprehensiveTest/results/${sessionId}`)
    if (response.ok) {
      testResults.value = await response.json()
      
      // 绘制图表
      nextTick(() => {
        drawCharts()
      })
    }
  } catch (error) {
    console.error('获取测试结果失败:', error)
  }
}

const drawCharts = () => {
  if (!testResults.value?.visualizationData) return

  // 绘制波形图
  const waveformCanvas = document.querySelector('[ref="waveformChart"]')
  if (waveformCanvas && testResults.value.visualizationData.waveformData) {
    const ctx = waveformCanvas.getContext('2d')
    const data = testResults.value.visualizationData.waveformData
    
    ctx.clearRect(0, 0, waveformCanvas.width, waveformCanvas.height)
    ctx.strokeStyle = '#007bff'
    ctx.lineWidth = 1
    ctx.beginPath()
    
    for (let i = 0; i < data.length && i < 400; i++) {
      const x = (i / 400) * waveformCanvas.width
      const y = waveformCanvas.height / 2 - (data[i] * waveformCanvas.height / 4)
      
      if (i === 0) {
        ctx.moveTo(x, y)
      } else {
        ctx.lineTo(x, y)
      }
    }
    ctx.stroke()
  }

  // 绘制频谱图
  const spectrumCanvas = document.querySelector('[ref="spectrumChart"]')
  if (spectrumCanvas && testResults.value.visualizationData.spectrumData) {
    const ctx = spectrumCanvas.getContext('2d')
    const data = testResults.value.visualizationData.spectrumData
    
    ctx.clearRect(0, 0, spectrumCanvas.width, spectrumCanvas.height)
    ctx.fillStyle = '#28a745'
    
    for (let i = 0; i < data.length && i < 200; i++) {
      const x = (i / 200) * spectrumCanvas.width
      const height = Math.max(1, (-data[i] + 60) / 60 * spectrumCanvas.height)
      const y = spectrumCanvas.height - height
      
      ctx.fillRect(x, y, spectrumCanvas.width / 200, height)
    }
  }
}

// 生命周期
onMounted(() => {
  loadSystemStatus()
  loadScenarios()
})
</script>

<style scoped>
.comprehensive-test-view {
  padding: 24px;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  text-align: center;
  margin-bottom: 32px;
}

.page-title {
  font-size: 2.5rem;
  color: #2c3e50;
  margin: 0 0 8px 0;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
}

.page-title .icon {
  font-size: 3rem;
}

.page-subtitle {
  color: #7f8c8d;
  font-size: 1.1rem;
  margin: 0;
}

.status-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 32px;
}

.status-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
  display: flex;
  align-items: center;
  gap: 16px;
}

.status-icon {
  font-size: 2rem;
}

.status-info {
  display: flex;
  flex-direction: column;
}

.status-label {
  font-size: 0.9rem;
  color: #7f8c8d;
  margin-bottom: 4px;
}

.status-value {
  font-size: 1.2rem;
  font-weight: 600;
}

.status-value.healthy {
  color: #27ae60;
}

.status-value.error {
  color: #e74c3c;
}

.test-scenarios, .custom-test {
  background: white;
  border-radius: 12px;
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.test-scenarios h2, .custom-test h2 {
  margin: 0 0 20px 0;
  color: #2c3e50;
}

.scenarios-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 16px;
}

.scenario-card {
  border: 2px solid #e9ecef;
  border-radius: 12px;
  padding: 20px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  gap: 16px;
}

.scenario-card:hover {
  border-color: #007bff;
  box-shadow: 0 4px 12px rgba(0,123,255,0.15);
}

.scenario-card.selected {
  border-color: #007bff;
  background: #f8f9ff;
}

.scenario-icon {
  font-size: 2.5rem;
  flex-shrink: 0;
}

.scenario-info h3 {
  margin: 0 0 8px 0;
  color: #2c3e50;
}

.scenario-info p {
  margin: 0 0 12px 0;
  color: #7f8c8d;
  font-size: 0.9rem;
}

.scenario-meta {
  display: flex;
  gap: 16px;
  font-size: 0.8rem;
}

.scenario-meta .device {
  background: #e9ecef;
  padding: 4px 8px;
  border-radius: 4px;
  color: #495057;
}

.scenario-meta .time {
  color: #7f8c8d;
}

.test-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-weight: 500;
  color: #2c3e50;
}

.form-group textarea,
.form-group select {
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.3s ease;
}

.form-group textarea:focus,
.form-group select:focus {
  outline: none;
  border-color: #007bff;
}

.test-controls {
  text-align: center;
  margin-bottom: 32px;
}

.start-btn {
  background: linear-gradient(135deg, #007bff, #0056b3);
  color: white;
  border: none;
  padding: 16px 32px;
  border-radius: 12px;
  font-size: 1.1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px rgba(0,123,255,0.3);
}

.start-btn:hover:not(.disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(0,123,255,0.4);
}

.start-btn.disabled {
  background: #6c757d;
  cursor: not-allowed;
  box-shadow: none;
}

.test-progress, .test-results {
  background: white;
  border-radius: 12px;
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.progress-container {
  margin-bottom: 24px;
}

.progress-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.session-id {
  font-family: monospace;
  font-size: 0.9rem;
  color: #7f8c8d;
}

.progress-percentage {
  font-weight: 600;
  color: #007bff;
}

.progress-bar {
  height: 8px;
  background: #e9ecef;
  border-radius: 4px;
  overflow: hidden;
  margin-bottom: 8px;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #007bff, #28a745);
  transition: width 0.5s ease;
}

.current-phase {
  font-size: 0.9rem;
  color: #495057;
}

.phases {
  display: flex;
  justify-content: space-between;
  gap: 8px;
}

.phase {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 12px;
  border-radius: 8px;
  background: #f8f9fa;
  flex: 1;
  transition: all 0.3s ease;
}

.phase.active {
  background: #e3f2fd;
  color: #1976d2;
}

.phase.completed {
  background: #e8f5e8;
  color: #2e7d32;
}

.phase-icon {
  font-size: 1.5rem;
}

.phase-name {
  font-size: 0.8rem;
  text-align: center;
}

.overall-results {
  display: flex;
  gap: 24px;
  align-items: center;
  margin-bottom: 32px;
  padding: 24px;
  background: linear-gradient(135deg, #f8f9fa, #e9ecef);
  border-radius: 12px;
}

.score-card {
  text-align: center;
}

.score-value {
  font-size: 3rem;
  font-weight: 700;
  color: #28a745;
  margin-bottom: 8px;
}

.score-label {
  font-size: 1.1rem;
  color: #495057;
}

.test-summary {
  display: flex;
  gap: 32px;
}

.summary-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.summary-item .label {
  font-size: 0.9rem;
  color: #7f8c8d;
}

.summary-item .value {
  font-size: 1.2rem;
  font-weight: 600;
  color: #2c3e50;
}

.detailed-results {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.result-section {
  border: 1px solid #e9ecef;
  border-radius: 8px;
  padding: 20px;
}

.result-section h3 {
  margin: 0 0 16px 0;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.result-status {
  font-weight: 600;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 0.9rem;
}

.result-status.success {
  background: #d4edda;
  color: #155724;
}

.result-status.error {
  background: #f8d7da;
  color: #721c24;
}

.result-details {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-top: 16px;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid #f8f9fa;
}

.extracted-params, .code-preview, .execution-output {
  margin-top: 16px;
}

.param-item {
  display: flex;
  gap: 8px;
  padding: 4px 0;
  font-size: 0.9rem;
}

.param-key {
  font-weight: 500;
  color: #495057;
  min-width: 120px;
}

.param-value {
  color: #007bff;
}

.code-preview pre, .execution-output pre {
  background: #f8f9fa;
  padding: 12px;
  border-radius: 6px;
  font-size: 0.8rem;
  overflow-x: auto;
  margin: 8px 0 0 0;
}

.visualization-section {
  margin-top: 32px;
  padding-top: 24px;
  border-top: 1px solid #e9ecef;
}

.charts-container {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  margin-top: 16px;
}

.chart-card {
  background: #f8f9fa;
  padding: 20px;
  border-radius: 8px;
  text-align: center;
}

.chart-card h4 {
  margin: 0 0 16px 0;
  color: #495057;
}

.chart-card canvas {
  border: 1px solid #dee2e6;
  border-radius: 4px;
  background: white;
  max-width: 100%;
}

@media (max-width: 768px) {
  .comprehensive-test-view {
    padding: 16px;
  }
  
  .scenarios-grid {
    grid-template-columns: 1fr;
  }
  
  .form-row {
    grid-template-columns: 1fr;
  }
  
  .phases {
    flex-wrap: wrap;
  }
  
  .overall-results {
    flex-direction: column;
    text-align: center;
  }
  
  .test-summary {
    justify-content: center;
  }
  
  .charts-container {
    grid-template-columns: 1fr;
  }
}
</style>
