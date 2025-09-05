<template>
  <div class="ai-model-manager">
    <div class="page-header">
      <h1>🤖 AI模型管理中心</h1>
      <p class="subtitle">统一管理和配置多个AI模型服务</p>
    </div>

    <div class="manager-content">
      <!-- AI模型配置区 -->
      <div class="models-grid">
        <!-- Claude模型卡片 -->
        <div class="model-card" :class="{ active: modelStatus.Claude }">
          <div class="model-header">
            <span class="model-emoji">🤖</span>
            <h3>Claude API</h3>
            <span class="status-badge" :class="modelStatus.Claude ? 'active' : 'inactive'">
              {{ modelStatus.Claude ? '已配置' : '未配置' }}
            </span>
          </div>
          
          <div class="model-body">
            <p class="description">Anthropic Claude AI - 高质量代码生成和分析</p>
            
            <div class="api-key-section">
              <label>API密钥：</label>
              <div class="key-input-group">
                <input 
                  :type="showKeys.Claude ? 'text' : 'password'"
                  v-model="apiKeys.Claude"
                  placeholder="sk-ant-xxx..."
                  class="api-key-input"
                />
                <button @click="toggleKeyVisibility('Claude')" class="toggle-btn">
                  {{ showKeys.Claude ? '🙈' : '👁️' }}
                </button>
              </div>
            </div>

            <div class="model-features">
              <span class="feature">✨ 代码生成</span>
              <span class="feature">🔍 代码分析</span>
              <span class="feature">📝 文档生成</span>
            </div>

            <div class="model-stats">
              <div class="stat">
                <span class="label">调用次数：</span>
                <span class="value">{{ modelStats.Claude.calls || 0 }}</span>
              </div>
              <div class="stat">
                <span class="label">成功率：</span>
                <span class="value">{{ modelStats.Claude.successRate || '100%' }}</span>
              </div>
              <div class="stat">
                <span class="label">响应时间：</span>
                <span class="value">{{ modelStats.Claude.avgResponseTime || '1.2s' }}</span>
              </div>
            </div>

            <div class="model-actions">
              <button @click="saveApiKey('Claude')" class="save-btn">保存配置</button>
              <button @click="testConnection('Claude')" class="test-btn">测试连接</button>
              <button @click="viewLogs('Claude')" class="log-btn">查看日志</button>
            </div>
          </div>
        </div>

        <!-- DeepSeek模型卡片 -->
        <div class="model-card" :class="{ active: modelStatus.DeepSeek }">
          <div class="model-header">
            <div class="model-icon deepseek">🌊</div>
            <h3>DeepSeek API</h3>
            <span class="status-badge" :class="modelStatus.DeepSeek ? 'active' : 'inactive'">
              {{ modelStatus.DeepSeek ? '已配置' : '未配置' }}
            </span>
          </div>
          
          <div class="model-body">
            <p class="description">深度求索AI - 中文优化的代码生成</p>
            
            <div class="api-key-section">
              <label>API密钥：</label>
              <div class="key-input-group">
                <input 
                  :type="showKeys.DeepSeek ? 'text' : 'password'"
                  v-model="apiKeys.DeepSeek"
                  placeholder="输入DeepSeek API密钥..."
                  class="api-key-input"
                />
                <button @click="toggleKeyVisibility('DeepSeek')" class="toggle-btn">
                  {{ showKeys.DeepSeek ? '🙈' : '👁️' }}
                </button>
              </div>
            </div>

            <div class="model-features">
              <span class="feature">🇨🇳 中文优化</span>
              <span class="feature">⚡ 快速响应</span>
              <span class="feature">💰 成本优势</span>
            </div>

            <div class="model-stats">
              <div class="stat">
                <span class="label">调用次数：</span>
                <span class="value">{{ modelStats.DeepSeek.calls || 0 }}</span>
              </div>
              <div class="stat">
                <span class="label">成功率：</span>
                <span class="value">{{ modelStats.DeepSeek.successRate || '98%' }}</span>
              </div>
              <div class="stat">
                <span class="label">响应时间：</span>
                <span class="value">{{ modelStats.DeepSeek.avgResponseTime || '0.8s' }}</span>
              </div>
            </div>

            <div class="model-actions">
              <button @click="saveApiKey('DeepSeek')" class="save-btn">保存配置</button>
              <button @click="testConnection('DeepSeek')" class="test-btn">测试连接</button>
              <button @click="viewLogs('DeepSeek')" class="log-btn">查看日志</button>
            </div>
          </div>
        </div>

        <!-- 百度文心一言模型卡片 -->
        <div class="model-card" :class="{ active: modelStatus.Baidu }">
          <div class="model-header">
            <div class="model-icon baidu">🎯</div>
            <h3>百度文心一言</h3>
            <span class="status-badge" :class="modelStatus.Baidu ? 'active' : 'inactive'">
              {{ modelStatus.Baidu ? '已配置' : '未配置' }}
            </span>
          </div>
          
          <div class="model-body">
            <p class="description">百度AI - 国产大语言模型</p>
            
            <div class="api-key-section">
              <label>API Key：</label>
              <div class="key-input-group">
                <input 
                  :type="showKeys.BaiduKey ? 'text' : 'password'"
                  v-model="apiKeys.BaiduKey"
                  placeholder="输入API Key..."
                  class="api-key-input"
                />
                <button @click="toggleKeyVisibility('BaiduKey')" class="toggle-btn">
                  {{ showKeys.BaiduKey ? '🙈' : '👁️' }}
                </button>
              </div>
              
              <label>Secret Key：</label>
              <div class="key-input-group">
                <input 
                  :type="showKeys.BaiduSecret ? 'text' : 'password'"
                  v-model="apiKeys.BaiduSecret"
                  placeholder="输入Secret Key..."
                  class="api-key-input"
                />
                <button @click="toggleKeyVisibility('BaiduSecret')" class="toggle-btn">
                  {{ showKeys.BaiduSecret ? '🙈' : '👁️' }}
                </button>
              </div>
            </div>

            <div class="model-features">
              <span class="feature">🇨🇳 国产模型</span>
              <span class="feature">🔒 数据安全</span>
              <span class="feature">📊 行业优化</span>
            </div>

            <div class="model-stats">
              <div class="stat">
                <span class="label">调用次数：</span>
                <span class="value">{{ modelStats.Baidu.calls || 0 }}</span>
              </div>
              <div class="stat">
                <span class="label">成功率：</span>
                <span class="value">{{ modelStats.Baidu.successRate || '95%' }}</span>
              </div>
              <div class="stat">
                <span class="label">响应时间：</span>
                <span class="value">{{ modelStats.Baidu.avgResponseTime || '1.0s' }}</span>
              </div>
            </div>

            <div class="model-actions">
              <button @click="saveApiKey('Baidu')" class="save-btn">保存配置</button>
              <button @click="testConnection('Baidu')" class="test-btn">测试连接</button>
              <button @click="viewLogs('Baidu')" class="log-btn">查看日志</button>
            </div>
          </div>
        </div>

        <!-- OpenAI模型卡片 -->
        <div class="model-card" :class="{ active: modelStatus.OpenAI }">
          <div class="model-header">
            <div class="model-icon openai">🧠</div>
            <h3>OpenAI GPT</h3>
            <span class="status-badge" :class="modelStatus.OpenAI ? 'active' : 'inactive'">
              {{ modelStatus.OpenAI ? '已配置' : '未配置' }}
            </span>
          </div>
          
          <div class="model-body">
            <p class="description">OpenAI GPT - 通用AI能力</p>
            
            <div class="api-key-section">
              <label>API密钥：</label>
              <div class="key-input-group">
                <input 
                  :type="showKeys.OpenAI ? 'text' : 'password'"
                  v-model="apiKeys.OpenAI"
                  placeholder="sk-xxx..."
                  class="api-key-input"
                />
                <button @click="toggleKeyVisibility('OpenAI')" class="toggle-btn">
                  {{ showKeys.OpenAI ? '🙈' : '👁️' }}
                </button>
              </div>
            </div>

            <div class="model-features">
              <span class="feature">🌍 全球领先</span>
              <span class="feature">🎨 多模态</span>
              <span class="feature">🚀 持续更新</span>
            </div>

            <div class="model-stats">
              <div class="stat">
                <span class="label">调用次数：</span>
                <span class="value">{{ modelStats.OpenAI.calls || 0 }}</span>
              </div>
              <div class="stat">
                <span class="label">成功率：</span>
                <span class="value">{{ modelStats.OpenAI.successRate || '99%' }}</span>
              </div>
              <div class="stat">
                <span class="label">响应时间：</span>
                <span class="value">{{ modelStats.OpenAI.avgResponseTime || '1.5s' }}</span>
              </div>
            </div>

            <div class="model-actions">
              <button @click="saveApiKey('OpenAI')" class="save-btn">保存配置</button>
              <button @click="testConnection('OpenAI')" class="test-btn">测试连接</button>
              <button @click="viewLogs('OpenAI')" class="log-btn">查看日志</button>
            </div>
          </div>
        </div>
      </div>

      <!-- 全局配置区 -->
      <div class="global-config">
        <h2>🔧 全局配置</h2>
        
        <div class="config-section">
          <h3>模型优先级设置</h3>
          <div class="priority-list">
            <draggable v-model="modelPriority" item-key="id">
              <template #item="{element}">
                <div class="priority-item">
                  <span class="handle">☰</span>
                  <span class="model-name">{{ element.name }}</span>
                  <span class="priority-badge">优先级 {{ element.priority }}</span>
                </div>
              </template>
            </draggable>
          </div>
        </div>

        <div class="config-section">
          <h3>智能切换策略</h3>
          <div class="strategy-options">
            <label>
              <input type="radio" v-model="switchStrategy" value="failover" />
              失败自动切换
            </label>
            <label>
              <input type="radio" v-model="switchStrategy" value="loadbalance" />
              负载均衡
            </label>
            <label>
              <input type="radio" v-model="switchStrategy" value="cost" />
              成本优先
            </label>
            <label>
              <input type="radio" v-model="switchStrategy" value="performance" />
              性能优先
            </label>
          </div>
        </div>

        <div class="config-section">
          <h3>性能监控</h3>
          <div class="performance-chart">
            <canvas ref="performanceChart"></canvas>
          </div>
        </div>

        <div class="config-actions">
          <button @click="saveGlobalConfig" class="primary-btn">保存全局配置</button>
          <button @click="exportConfig" class="secondary-btn">导出配置</button>
          <button @click="importConfig" class="secondary-btn">导入配置</button>
        </div>
      </div>
    </div>

    <!-- 测试对话框 -->
    <el-dialog v-model="testDialogVisible" title="API连接测试" width="600px">
      <div class="test-content">
        <p>正在测试 {{ currentTestModel }} API连接...</p>
        <div v-if="testResult" class="test-result" :class="testResult.success ? 'success' : 'error'">
          <span class="icon">{{ testResult.success ? '✅' : '❌' }}</span>
          <span class="message">{{ testResult.message }}</span>
          <div v-if="testResult.details" class="details">
            <pre>{{ testResult.details }}</pre>
          </div>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElDialog } from 'element-plus'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { Chart } from 'chart.js/auto'

// 响应式数据
const apiKeys = reactive({
  Claude: '',
  DeepSeek: '',
  BaiduKey: '',
  BaiduSecret: '',
  OpenAI: ''
})

const showKeys = reactive({
  Claude: false,
  DeepSeek: false,
  BaiduKey: false,
  BaiduSecret: false,
  OpenAI: false
})

const modelStatus = reactive({
  Claude: false,
  DeepSeek: false,
  Baidu: false,
  OpenAI: false
})

const modelStats = reactive({
  Claude: { calls: 0, successRate: '100%', avgResponseTime: '1.2s' },
  DeepSeek: { calls: 0, successRate: '98%', avgResponseTime: '0.8s' },
  Baidu: { calls: 0, successRate: '95%', avgResponseTime: '1.0s' },
  OpenAI: { calls: 0, successRate: '99%', avgResponseTime: '1.5s' }
})

const modelPriority = ref([
  { id: 1, name: 'Claude', priority: 1 },
  { id: 2, name: 'DeepSeek', priority: 2 },
  { id: 3, name: '百度文心', priority: 3 },
  { id: 4, name: 'OpenAI', priority: 4 }
])

const switchStrategy = ref('failover')
const testDialogVisible = ref(false)
const currentTestModel = ref('')
const testResult = ref<any>(null)
const performanceChart = ref<HTMLCanvasElement>()

// 方法
const toggleKeyVisibility = (key: string) => {
  showKeys[key as keyof typeof showKeys] = !showKeys[key as keyof typeof showKeys]
}

const saveApiKey = async (provider: string) => {
  try {
    let apiKey = ''
    if (provider === 'Baidu') {
      // 百度需要两个密钥
      if (!apiKeys.BaiduKey || !apiKeys.BaiduSecret) {
        ElMessage.warning('请输入完整的百度API密钥')
        return
      }
      apiKey = `${apiKeys.BaiduKey}:${apiKeys.BaiduSecret}`
    } else {
      apiKey = apiKeys[provider as keyof typeof apiKeys]
    }

    if (!apiKey) {
      ElMessage.warning('请输入API密钥')
      return
    }

    const response = await axios.post('/api/secure-keys/set', {
      provider,
      apiKey
    })

    if (response.data.success) {
      modelStatus[provider as keyof typeof modelStatus] = true
      ElMessage.success(`${provider} API密钥保存成功`)
    }
  } catch (error) {
    console.error('保存API密钥失败:', error)
    ElMessage.error('保存失败，请重试')
  }
}

const testConnection = async (provider: string) => {
  currentTestModel.value = provider
  testResult.value = null
  testDialogVisible.value = true

  try {
    const response = await axios.post('/api/secure-keys/test', { provider })
    
    testResult.value = {
      success: response.data.success,
      message: response.data.message || (response.data.success ? '连接成功！' : '连接失败'),
      details: response.data.details
    }
  } catch (error) {
    testResult.value = {
      success: false,
      message: '测试失败',
      details: error
    }
  }
}

const viewLogs = (provider: string) => {
  // 跳转到日志查看页面
  console.log('查看日志:', provider)
}

const saveGlobalConfig = async () => {
  try {
    const config = {
      priority: modelPriority.value,
      strategy: switchStrategy.value
    }
    
    const response = await axios.post('/api/ai-config/global', config)
    
    if (response.data.success) {
      ElMessage.success('全局配置保存成功')
    }
  } catch (error) {
    ElMessage.error('保存配置失败')
  }
}

const exportConfig = () => {
  const config = {
    models: modelStatus,
    priority: modelPriority.value,
    strategy: switchStrategy.value
  }
  
  const blob = new Blob([JSON.stringify(config, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = 'ai-models-config.json'
  a.click()
}

const importConfig = () => {
  const input = document.createElement('input')
  input.type = 'file'
  input.accept = '.json'
  input.onchange = (e: any) => {
    const file = e.target.files[0]
    const reader = new FileReader()
    reader.onload = (e: any) => {
      try {
        const config = JSON.parse(e.target.result)
        // 应用导入的配置
        Object.assign(modelStatus, config.models || {})
        modelPriority.value = config.priority || modelPriority.value
        switchStrategy.value = config.strategy || 'failover'
        ElMessage.success('配置导入成功')
      } catch (error) {
        ElMessage.error('配置文件格式错误')
      }
    }
    reader.readAsText(file)
  }
  input.click()
}

const loadApiKeyStatus = async () => {
  try {
    const response = await axios.get('/api/secure-keys/status')
    if (response.data) {
      Object.assign(modelStatus, response.data)
    }
  } catch (error) {
    console.error('加载API密钥状态失败:', error)
  }
}

const initPerformanceChart = () => {
  if (!performanceChart.value) return

  new Chart(performanceChart.value, {
    type: 'line',
    data: {
      labels: ['00:00', '04:00', '08:00', '12:00', '16:00', '20:00'],
      datasets: [
        {
          label: 'Claude',
          data: [1.2, 1.1, 1.3, 1.2, 1.4, 1.2],
          borderColor: 'rgb(255, 99, 132)',
          tension: 0.1
        },
        {
          label: 'DeepSeek',
          data: [0.8, 0.7, 0.9, 0.8, 0.85, 0.8],
          borderColor: 'rgb(54, 162, 235)',
          tension: 0.1
        },
        {
          label: '百度',
          data: [1.0, 0.95, 1.1, 1.0, 1.05, 1.0],
          borderColor: 'rgb(255, 206, 86)',
          tension: 0.1
        }
      ]
    },
    options: {
      responsive: true,
      plugins: {
        legend: {
          position: 'top',
        },
        title: {
          display: true,
          text: 'API响应时间监控 (秒)'
        }
      },
      scales: {
        y: {
          beginAtZero: true
        }
      }
    }
  })
}

onMounted(() => {
  loadApiKeyStatus()
  initPerformanceChart()
})
</script>

<style scoped lang="scss">
.ai-model-manager {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;

  .page-header {
    text-align: center;
    margin-bottom: 40px;
    
    h1 {
      font-size: 32px;
      color: #333;
      margin-bottom: 10px;
    }
    
    .subtitle {
      color: #666;
      font-size: 16px;
    }
  }

  .manager-content {
    display: flex;
    gap: 30px;

    .models-grid {
      flex: 2;
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
      gap: 20px;

      .model-card {
        background: white;
        border-radius: 12px;
        box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        transition: all 0.3s ease;

        &.active {
          box-shadow: 0 4px 12px rgba(0,0,0,0.15);
          border: 2px solid #409eff;
        }

        .model-header {
          display: flex;
          align-items: center;
          padding: 20px;
          border-bottom: 1px solid #eee;

          .model-emoji {
            font-size: 40px;
            margin-right: 10px;
          }
          
          .model-icon {
            width: 40px;
            height: 40px;
            margin-right: 12px;
            
            &.deepseek {
              font-size: 32px;
              display: flex;
              align-items: center;
              justify-content: center;
            }
            
            &.baidu {
              font-size: 32px;
              display: flex;
              align-items: center;
              justify-content: center;
            }
            
            &.openai {
              font-size: 32px;
              display: flex;
              align-items: center;
              justify-content: center;
            }
          }

          h3 {
            flex: 1;
            margin: 0;
            font-size: 18px;
          }

          .status-badge {
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 12px;
            
            &.active {
              background: #e6f7ff;
              color: #1890ff;
            }
            
            &.inactive {
              background: #f0f0f0;
              color: #999;
            }
          }
        }

        .model-body {
          padding: 20px;

          .description {
            color: #666;
            margin-bottom: 20px;
            font-size: 14px;
          }

          .api-key-section {
            margin-bottom: 20px;

            label {
              display: block;
              margin-bottom: 8px;
              color: #333;
              font-weight: 500;
            }

            .key-input-group {
              display: flex;
              margin-bottom: 12px;

              .api-key-input {
                flex: 1;
                padding: 8px 12px;
                border: 1px solid #ddd;
                border-radius: 4px 0 0 4px;
                font-family: monospace;
              }

              .toggle-btn {
                padding: 8px 12px;
                border: 1px solid #ddd;
                border-left: none;
                border-radius: 0 4px 4px 0;
                background: #f5f5f5;
                cursor: pointer;
                
                &:hover {
                  background: #e8e8e8;
                }
              }
            }
          }

          .model-features {
            display: flex;
            gap: 10px;
            margin-bottom: 20px;

            .feature {
              padding: 4px 8px;
              background: #f0f9ff;
              color: #1890ff;
              border-radius: 4px;
              font-size: 12px;
            }
          }

          .model-stats {
            display: flex;
            justify-content: space-between;
            margin-bottom: 20px;
            padding: 12px;
            background: #fafafa;
            border-radius: 8px;

            .stat {
              text-align: center;

              .label {
                display: block;
                color: #999;
                font-size: 12px;
                margin-bottom: 4px;
              }

              .value {
                display: block;
                color: #333;
                font-weight: bold;
                font-size: 16px;
              }
            }
          }

          .model-actions {
            display: flex;
            gap: 10px;

            button {
              flex: 1;
              padding: 8px 16px;
              border: none;
              border-radius: 4px;
              cursor: pointer;
              font-size: 14px;
              transition: all 0.3s;

              &.save-btn {
                background: #52c41a;
                color: white;
                
                &:hover {
                  background: #45a813;
                }
              }

              &.test-btn {
                background: #1890ff;
                color: white;
                
                &:hover {
                  background: #0e7bd8;
                }
              }

              &.log-btn {
                background: #f0f0f0;
                color: #333;
                
                &:hover {
                  background: #e0e0e0;
                }
              }
            }
          }
        }
      }
    }

    .global-config {
      flex: 1;
      min-width: 350px;
      background: white;
      border-radius: 12px;
      padding: 24px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);

      h2 {
        font-size: 20px;
        margin-bottom: 24px;
        color: #333;
      }

      .config-section {
        margin-bottom: 30px;

        h3 {
          font-size: 16px;
          margin-bottom: 16px;
          color: #555;
        }

        .priority-list {
          .priority-item {
            display: flex;
            align-items: center;
            padding: 12px;
            background: #f8f8f8;
            margin-bottom: 8px;
            border-radius: 6px;
            cursor: move;

            .handle {
              margin-right: 12px;
              color: #999;
            }

            .model-name {
              flex: 1;
              font-weight: 500;
            }

            .priority-badge {
              padding: 2px 8px;
              background: #e6f7ff;
              color: #1890ff;
              border-radius: 4px;
              font-size: 12px;
            }
          }
        }

        .strategy-options {
          display: flex;
          flex-direction: column;
          gap: 12px;

          label {
            display: flex;
            align-items: center;
            padding: 10px;
            background: #f8f8f8;
            border-radius: 6px;
            cursor: pointer;
            transition: background 0.3s;

            &:hover {
              background: #f0f0f0;
            }

            input {
              margin-right: 10px;
            }
          }
        }

        .performance-chart {
          height: 250px;
          padding: 12px;
          background: #fafafa;
          border-radius: 8px;
        }
      }

      .config-actions {
        display: flex;
        gap: 10px;

        button {
          flex: 1;
          padding: 10px 16px;
          border: none;
          border-radius: 6px;
          cursor: pointer;
          font-size: 14px;
          transition: all 0.3s;

          &.primary-btn {
            background: #409eff;
            color: white;
            
            &:hover {
              background: #3a8df0;
            }
          }

          &.secondary-btn {
            background: #f0f0f0;
            color: #333;
            
            &:hover {
              background: #e0e0e0;
            }
          }
        }
      }
    }
  }

  .test-content {
    padding: 20px;

    .test-result {
      margin-top: 20px;
      padding: 16px;
      border-radius: 8px;
      
      &.success {
        background: #f0f9ff;
        border: 1px solid #91d5ff;
      }
      
      &.error {
        background: #fff2e8;
        border: 1px solid #ffbb96;
      }

      .icon {
        font-size: 24px;
        margin-right: 12px;
      }

      .message {
        font-size: 16px;
      }

      .details {
        margin-top: 12px;
        
        pre {
          background: #f5f5f5;
          padding: 12px;
          border-radius: 4px;
          font-size: 12px;
          overflow-x: auto;
        }
      }
    }
  }
}
</style>