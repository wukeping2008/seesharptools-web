<template>
  <div class="data-analysis-test">
    <div class="page-header">
      <h1>数据分析测试</h1>
      <p>测试数据分析和报告生成功能</p>
    </div>

    <div class="analysis-container">
      <!-- 数据输入区域 -->
      <div class="data-input-section">
        <h2>数据输入</h2>
        <div class="input-controls">
          <div class="control-group">
            <label>数据生成方式：</label>
            <select v-model="dataGenerationType" @change="generateData">
              <option value="random">随机数据</option>
              <option value="trend">趋势数据</option>
              <option value="seasonal">季节性数据</option>
              <option value="anomaly">包含异常值</option>
              <option value="custom">自定义数据</option>
            </select>
          </div>
          
          <div class="control-group">
            <label>数据点数量：</label>
            <input 
              type="number" 
              v-model.number="dataSize" 
              min="10" 
              max="1000" 
              @change="generateData"
            />
          </div>
          
          <button @click="generateData" class="generate-btn">
            重新生成数据
          </button>
        </div>

        <!-- 自定义数据输入 -->
        <div v-if="dataGenerationType === 'custom'" class="custom-data">
          <label>输入数据（用逗号分隔）：</label>
          <textarea 
            v-model="customDataInput" 
            @input="parseCustomData"
            placeholder="例如：1,2,3,4,5,6,7,8,9,10"
            rows="3"
          ></textarea>
        </div>

        <!-- 数据预览 -->
        <div class="data-preview">
          <h3>数据预览（前20个点）</h3>
          <div class="data-points">
            <span 
              v-for="(point, index) in currentData.slice(0, 20)" 
              :key="index"
              class="data-point"
            >
              {{ point.toFixed(2) }}
            </span>
            <span v-if="currentData.length > 20" class="more-indicator">
              ... 还有 {{ currentData.length - 20 }} 个数据点
            </span>
          </div>
        </div>
      </div>

      <!-- 分析控制区域 -->
      <div class="analysis-controls">
        <h2>分析选项</h2>
        <div class="analysis-types">
          <label class="checkbox-label">
            <input 
              type="checkbox" 
              v-model="selectedAnalyses" 
              value="statistical"
            />
            统计分析
          </label>
          
          <label class="checkbox-label">
            <input 
              type="checkbox" 
              v-model="selectedAnalyses" 
              value="trend"
            />
            趋势分析
          </label>
          
          <label class="checkbox-label">
            <input 
              type="checkbox" 
              v-model="selectedAnalyses" 
              value="anomaly"
            />
            异常检测
          </label>
          
          <label class="checkbox-label">
            <input 
              type="checkbox" 
              v-model="selectedAnalyses" 
              value="frequency"
            />
            频率分析
          </label>
        </div>

        <button 
          @click="performAnalysis" 
          :disabled="!selectedAnalyses.length || isAnalyzing"
          class="analyze-btn"
        >
          {{ isAnalyzing ? '分析中...' : '开始分析' }}
        </button>
      </div>

      <!-- 分析结果区域 -->
      <div v-if="analysisResults.length" class="analysis-results">
        <h2>分析结果</h2>
        
        <div 
          v-for="result in analysisResults" 
          :key="result.analysisType"
          class="result-card"
        >
          <h3>{{ getAnalysisTitle(result.analysisType) }}</h3>
          
          <!-- 统计分析结果 -->
          <div v-if="result.analysisType === 'statistical'" class="statistical-result">
            <div class="stats-grid">
              <div class="stat-item">
                <span class="stat-label">数据点数：</span>
                <span class="stat-value">{{ result.result.count }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">平均值：</span>
                <span class="stat-value">{{ result.result.mean.toFixed(3) }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">中位数：</span>
                <span class="stat-value">{{ result.result.median.toFixed(3) }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">标准差：</span>
                <span class="stat-value">{{ result.result.standardDeviation.toFixed(3) }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">最小值：</span>
                <span class="stat-value">{{ result.result.min.toFixed(3) }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">最大值：</span>
                <span class="stat-value">{{ result.result.max.toFixed(3) }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">偏度：</span>
                <span class="stat-value">{{ result.result.skewness.toFixed(3) }}</span>
              </div>
              <div class="stat-item">
                <span class="stat-label">峰度：</span>
                <span class="stat-value">{{ result.result.kurtosis.toFixed(3) }}</span>
              </div>
            </div>
          </div>

          <!-- 趋势分析结果 -->
          <div v-if="result.analysisType === 'trend'" class="trend-result">
            <div class="trend-info">
              <div class="trend-direction" :class="result.result.direction">
                趋势方向：{{ getTrendDirection(result.result.direction) }}
              </div>
              <div class="trend-stats">
                <p>斜率：{{ result.result.slope.toFixed(4) }}</p>
                <p>R²：{{ result.result.rSquared.toFixed(3) }}</p>
                <p>变化率：{{ result.result.changeRate.toFixed(2) }}%</p>
                <p>置信度：{{ getConfidenceText(result.result.confidence) }}</p>
              </div>
              <div class="prediction">
                <h4>预测值（未来5个点）：</h4>
                <div class="prediction-values">
                  <span 
                    v-for="(pred, index) in result.result.prediction" 
                    :key="index"
                    class="prediction-value"
                  >
                    {{ pred.toFixed(2) }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- 异常检测结果 -->
          <div v-if="result.analysisType === 'anomaly'" class="anomaly-result">
            <div class="anomaly-summary">
              <p>检测到 <strong>{{ result.result.anomalies.length }}</strong> 个异常值</p>
              <p>异常率：<strong>{{ result.result.anomalyRate.toFixed(2) }}%</strong></p>
            </div>
            
            <div v-if="result.result.anomalies.length" class="anomaly-list">
              <h4>异常值详情：</h4>
              <div class="anomaly-items">
                <div 
                  v-for="anomaly in result.result.anomalies.slice(0, 10)" 
                  :key="anomaly.index"
                  class="anomaly-item"
                  :class="anomaly.severity"
                >
                  <span class="anomaly-index">位置 {{ anomaly.index }}</span>
                  <span class="anomaly-value">值: {{ anomaly.value.toFixed(3) }}</span>
                  <span class="anomaly-severity">{{ getSeverityText(anomaly.severity) }}</span>
                </div>
                <div v-if="result.result.anomalies.length > 10" class="more-anomalies">
                  还有 {{ result.result.anomalies.length - 10 }} 个异常值...
                </div>
              </div>
            </div>
          </div>

          <!-- 频率分析结果 -->
          <div v-if="result.analysisType === 'frequency'" class="frequency-result">
            <div class="frequency-summary">
              <p>唯一值数量：<strong>{{ result.result.uniqueValues }}</strong></p>
              <p>信息熵：<strong>{{ result.result.entropy.toFixed(3) }}</strong></p>
              <p>最频繁值：<strong>{{ result.result.mostFrequent.value }}</strong> 
                (出现 {{ result.result.mostFrequent.count }} 次)</p>
            </div>
            
            <div class="frequency-distribution">
              <h4>频率分布（前10个）：</h4>
              <div class="distribution-items">
                <div 
                  v-for="item in result.result.distribution.slice(0, 10)" 
                  :key="item.value"
                  class="distribution-item"
                >
                  <span class="dist-value">{{ item.value.toFixed(2) }}</span>
                  <span class="dist-count">{{ item.count }}次</span>
                  <span class="dist-percentage">{{ item.percentage.toFixed(1) }}%</span>
                </div>
              </div>
            </div>
          </div>

          <div class="result-meta">
            <small>
              分析时间：{{ new Date(result.timestamp).toLocaleString() }} | 
              数据源：{{ result.source || '前端分析' }}
            </small>
          </div>
        </div>
      </div>

      <!-- 报告生成区域 -->
      <div class="report-section">
        <h2>生成报告</h2>
        <div class="report-controls">
          <div class="control-group">
            <label>报告格式：</label>
            <select v-model="reportFormat">
              <option value="json">JSON</option>
              <option value="html">HTML</option>
              <option value="pdf">PDF</option>
            </select>
          </div>
          
          <label class="checkbox-label">
            <input type="checkbox" v-model="includeCharts" />
            包含图表
          </label>
          
          <button 
            @click="generateReport" 
            :disabled="!analysisResults.length || isGeneratingReport"
            class="report-btn"
          >
            {{ isGeneratingReport ? '生成中...' : '生成报告' }}
          </button>
        </div>

        <div v-if="reportData" class="report-preview">
          <h3>报告预览</h3>
          <div class="report-summary">
            <p><strong>报告摘要：</strong></p>
            <p>{{ reportData.data?.summary }}</p>
          </div>
          
          <div class="report-metadata">
            <p><strong>报告信息：</strong></p>
            <ul>
              <li>生成时间：{{ new Date(reportData.data?.metadata.generatedAt || '').toLocaleString() }}</li>
              <li>数据点数：{{ reportData.data?.metadata.dataPoints }}</li>
              <li>分析数量：{{ reportData.data?.metadata.analysisCount }}</li>
            </ul>
          </div>
          
          <button @click="downloadReport" class="download-btn">
            下载报告
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { dataAnalysisService } from '@/services/DataAnalysisService'
import type { AnalysisResult, ReportData } from '@/types/analysis'

// 响应式数据
const dataGenerationType = ref('random')
const dataSize = ref(100)
const customDataInput = ref('')
const currentData = ref<number[]>([])
const selectedAnalyses = ref<string[]>(['statistical'])
const isAnalyzing = ref(false)
const analysisResults = ref<AnalysisResult[]>([])
const reportFormat = ref('json')
const includeCharts = ref(true)
const isGeneratingReport = ref(false)
const reportData = ref<ReportData | null>(null)

// 生成数据
const generateData = () => {
  switch (dataGenerationType.value) {
    case 'random':
      currentData.value = Array.from({ length: dataSize.value }, () => Math.random() * 100)
      break
    case 'trend':
      currentData.value = Array.from({ length: dataSize.value }, (_, i) => 
        10 + i * 0.5 + (Math.random() - 0.5) * 5
      )
      break
    case 'seasonal':
      currentData.value = Array.from({ length: dataSize.value }, (_, i) => 
        50 + 20 * Math.sin(i * 0.1) + (Math.random() - 0.5) * 10
      )
      break
    case 'anomaly':
      currentData.value = Array.from({ length: dataSize.value }, (_, i) => {
        const base = 50 + (Math.random() - 0.5) * 20
        // 添加一些异常值
        return Math.random() < 0.05 ? base + (Math.random() > 0.5 ? 50 : -50) : base
      })
      break
    case 'custom':
      // 自定义数据在parseCustomData中处理
      break
  }
}

// 解析自定义数据
const parseCustomData = () => {
  try {
    const values = customDataInput.value
      .split(',')
      .map(v => parseFloat(v.trim()))
      .filter(v => !isNaN(v))
    
    if (values.length > 0) {
      currentData.value = values
      dataSize.value = values.length
    }
  } catch (error) {
    console.error('解析自定义数据失败:', error)
  }
}

// 执行分析
const performAnalysis = async () => {
  if (!currentData.value.length || !selectedAnalyses.value.length) return
  
  isAnalyzing.value = true
  analysisResults.value = []
  
  try {
    for (const analysisType of selectedAnalyses.value) {
      const result = await dataAnalysisService.analyzeData({
        data: currentData.value,
        analysisType: analysisType as any
      })
      
      if (result.success) {
        analysisResults.value.push(result)
      }
    }
  } catch (error) {
    console.error('分析失败:', error)
  } finally {
    isAnalyzing.value = false
  }
}

// 生成报告
const generateReport = async () => {
  if (!analysisResults.value.length) return
  
  isGeneratingReport.value = true
  
  try {
    const config = {
      title: '数据分析报告',
      dataSource: 'test-data',
      analysisTypes: selectedAnalyses.value,
      format: reportFormat.value as any,
      includeCharts: includeCharts.value
    }
    
    reportData.value = await dataAnalysisService.generateReport(config)
  } catch (error) {
    console.error('报告生成失败:', error)
  } finally {
    isGeneratingReport.value = false
  }
}

// 下载报告
const downloadReport = () => {
  if (!reportData.value?.data) return
  
  const content = dataAnalysisService.exportData(reportData.value.data, reportFormat.value as any)
  const blob = content instanceof Blob ? content : new Blob([content], { type: 'text/plain' })
  
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `data-analysis-report.${reportFormat.value}`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

// 辅助函数
const getAnalysisTitle = (type: string): string => {
  const titles: Record<string, string> = {
    statistical: '统计分析',
    trend: '趋势分析',
    anomaly: '异常检测',
    frequency: '频率分析'
  }
  return titles[type] || type
}

const getTrendDirection = (direction: string): string => {
  const directions: Record<string, string> = {
    increasing: '上升趋势',
    decreasing: '下降趋势',
    stable: '稳定趋势'
  }
  return directions[direction] || direction
}

const getConfidenceText = (confidence: string): string => {
  const confidences: Record<string, string> = {
    high: '高置信度',
    medium: '中等置信度',
    low: '低置信度'
  }
  return confidences[confidence] || confidence
}

const getSeverityText = (severity: string): string => {
  const severities: Record<string, string> = {
    high: '高风险',
    medium: '中等风险',
    low: '低风险'
  }
  return severities[severity] || severity
}

// 初始化
onMounted(() => {
  generateData()
})
</script>

<style scoped>
.data-analysis-test {
  padding: 20px;
  max-width: 1200px;
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

.page-header p {
  color: #7f8c8d;
  font-size: 16px;
}

.analysis-container {
  display: flex;
  flex-direction: column;
  gap: 30px;
}

.data-input-section,
.analysis-controls,
.analysis-results,
.report-section {
  background: white;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.data-input-section h2,
.analysis-controls h2,
.analysis-results h2,
.report-section h2 {
  color: #2c3e50;
  margin-bottom: 20px;
  border-bottom: 2px solid #3498db;
  padding-bottom: 10px;
}

.input-controls {
  display: flex;
  gap: 20px;
  align-items: center;
  flex-wrap: wrap;
  margin-bottom: 20px;
}

.control-group {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.control-group label {
  font-weight: 600;
  color: #34495e;
}

.control-group select,
.control-group input {
  padding: 8px 12px;
  border: 1px solid #bdc3c7;
  border-radius: 4px;
  font-size: 14px;
}

.generate-btn,
.analyze-btn,
.report-btn,
.download-btn {
  background: #3498db;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 600;
  transition: background 0.3s ease;
}

.generate-btn:hover,
.analyze-btn:hover,
.report-btn:hover,
.download-btn:hover {
  background: #2980b9;
}

.generate-btn:disabled,
.analyze-btn:disabled,
.report-btn:disabled {
  background: #bdc3c7;
  cursor: not-allowed;
}

.custom-data {
  margin-bottom: 20px;
}

.custom-data label {
  display: block;
  margin-bottom: 5px;
  font-weight: 600;
  color: #34495e;
}

.custom-data textarea {
  width: 100%;
  padding: 10px;
  border: 1px solid #bdc3c7;
  border-radius: 4px;
  font-family: monospace;
  resize: vertical;
}

.data-preview {
  margin-top: 20px;
}

.data-preview h3 {
  color: #2c3e50;
  margin-bottom: 10px;
}

.data-points {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.data-point {
  background: #ecf0f1;
  padding: 4px 8px;
  border-radius: 4px;
  font-family: monospace;
  font-size: 12px;
}

.more-indicator {
  color: #7f8c8d;
  font-style: italic;
}

.analysis-types {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
  margin-bottom: 20px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-weight: 500;
}

.checkbox-label input[type="checkbox"] {
  width: 16px;
  height: 16px;
}

.result-card {
  border: 1px solid #ecf0f1;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 20px;
}

.result-card h3 {
  color: #2c3e50;
  margin-bottom: 15px;
  padding-bottom: 8px;
  border-bottom: 1px solid #ecf0f1;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 15px;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  padding: 8px 12px;
  background: #f8f9fa;
  border-radius: 4px;
}

.stat-label {
  font-weight: 600;
  color: #34495e;
}

.stat-value {
  font-family: monospace;
  color: #2c3e50;
}

.trend-direction {
  font-size: 18px;
  font-weight: 600;
  margin-bottom: 15px;
  padding: 10px;
  border-radius: 4px;
  text-align: center;
}

.trend-direction.increasing {
  background: #d5f4e6;
  color: #27ae60;
}

.trend-direction.decreasing {
  background: #fadbd8;
  color: #e74c3c;
}

.trend-direction.stable {
  background: #fef9e7;
  color: #f39c12;
}

.trend-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 10px;
  margin-bottom: 15px;
}

.trend-stats p {
  margin: 0;
  padding: 8px;
  background: #f8f9fa;
  border-radius: 4px;
}

.prediction-values {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.prediction-value {
  background: #e8f4fd;
  color: #2980b9;
  padding: 4px 8px;
  border-radius: 4px;
  font-family: monospace;
}

.anomaly-summary {
  background: #fdf2e9;
  padding: 15px;
  border-radius: 4px;
  margin-bottom: 15px;
}

.anomaly-summary p {
  margin: 5px 0;
}

.anomaly-items {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.anomaly-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  border-radius: 4px;
  border-left: 4px solid;
}

.anomaly-item.high {
  background: #fadbd8;
  border-left-color: #e74c3c;
}

.anomaly-item.medium {
  background: #fef9e7;
  border-left-color: #f39c12;
}

.anomaly-item.low {
  background: #eaf2f8;
  border-left-color: #3498db;
}

.anomaly-index,
.anomaly-value,
.anomaly-severity {
  font-size: 14px;
}

.anomaly-value {
  font-family: monospace;
}

.frequency-summary {
  background: #f4f6f7;
  padding: 15px;
  border-radius: 4px;
  margin-bottom: 15px;
}

.distribution-items {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.distribution-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: #f8f9fa;
  border-radius: 4px;
}

.dist-value {
  font-family: monospace;
  font-weight: 600;
}

.dist-count {
  color: #7f8c8d;
}

.dist-percentage {
  color: #3498db;
  font-weight: 600;
}

.result-meta {
  margin-top: 15px;
  padding-top: 10px;
  border-top: 1px solid #ecf0f1;
  color: #7f8c8d;
}

.report-controls {
  display: flex;
  gap: 20px;
  align-items: center;
  flex-wrap: wrap;
  margin-bottom: 20px;
}

.report-preview {
  background: #f8f9fa;
  padding: 20px;
  border-radius: 8px;
  margin-top: 20px;
}

.report-summary,
.report-metadata {
  margin-bottom: 15px;
}

.report-metadata ul {
  margin: 10px 0;
  padding-left: 20px;
}

.report-metadata li {
  margin: 5px 0;
}

@media (max-width: 768px) {
  .input-controls,
  .report-controls {
    flex-direction: column;
    align-items: stretch;
  }
  
  .stats-grid {
    grid-template-columns: 1fr;
  }
  
  .trend-stats {
    grid-template-columns: 1fr;
  }
  
  .anomaly-item,
  .distribution-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 5px;
  }
}
</style>
