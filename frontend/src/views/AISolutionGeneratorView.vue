<template>
  <div class="ai-solution-generator">
    <el-card class="generator-card">
      <template #header>
        <div class="card-header">
          <h2>AI 代码解决方案生成器</h2>
          <el-tag type="success">百度文心一言</el-tag>
        </div>
      </template>

      <el-form :model="form" label-width="120px">
        <el-form-item label="项目名称">
          <el-input 
            v-model="form.projectName" 
            placeholder="例如：MyConsoleApp"
            clearable
          />
        </el-form-item>

        <el-form-item label="AI模型">
          <el-select v-model="form.model" placeholder="选择模型">
            <el-option 
              v-for="model in models" 
              :key="model.id"
              :label="model.name"
              :value="model.id"
            >
              <span style="float: left">{{ model.name }}</span>
              <span style="float: right; color: #8492a6; font-size: 13px">
                {{ model.description }}
              </span>
            </el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="模板">
          <el-select 
            v-model="selectedTemplate" 
            placeholder="选择模板（可选）"
            clearable
            @change="onTemplateChange"
          >
            <el-option 
              v-for="template in templates" 
              :key="template.id"
              :label="template.name"
              :value="template.id"
            >
              <div>
                <div>{{ template.name }}</div>
                <div style="font-size: 12px; color: #8492a6">
                  {{ template.description }}
                </div>
              </div>
            </el-option>
          </el-select>
        </el-form-item>

        <el-form-item label="需求描述">
          <el-input
            v-model="form.prompt"
            type="textarea"
            :rows="10"
            placeholder="请详细描述您的代码需求，例如：创建一个能够读取CSV文件并计算数据统计信息的控制台程序..."
          />
        </el-form-item>

        <el-form-item label="输出格式">
          <el-radio-group v-model="form.outputFormat">
            <el-radio label="json">在线查看</el-radio>
            <el-radio label="zip">下载ZIP包</el-radio>
          </el-radio-group>
        </el-form-item>

        <el-form-item>
          <el-button 
            type="primary" 
            @click="generateSolution"
            :loading="isGenerating"
            :disabled="!form.prompt"
          >
            {{ isGenerating ? '正在生成...' : '生成解决方案' }}
          </el-button>
          <el-button @click="clearForm">清空</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 代码预览区 -->
    <el-card v-if="generatedCode" class="code-preview-card">
      <template #header>
        <div class="card-header">
          <h3>生成的代码</h3>
          <div>
            <el-button 
              size="small" 
              @click="copyCode"
              :icon="CopyDocument"
            >
              复制代码
            </el-button>
            <el-button 
              size="small"
              type="primary"
              @click="downloadAsZip"
              :icon="Download"
            >
              下载ZIP
            </el-button>
          </div>
        </div>
      </template>
      
      <div class="code-container">
        <pre><code class="language-csharp">{{ generatedCode }}</code></pre>
      </div>
    </el-card>

    <!-- 历史记录 -->
    <el-card class="history-card">
      <template #header>
        <h3>生成历史</h3>
      </template>
      
      <el-table :data="history" empty-text="暂无历史记录">
        <el-table-column prop="prompt" label="需求" show-overflow-tooltip />
        <el-table-column prop="model" label="模型" width="120" />
        <el-table-column prop="generatedAt" label="生成时间" width="180">
          <template #default="{ row }">
            {{ formatDate(row.generatedAt) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="100">
          <template #default="{ row }">
            <el-button 
              size="small" 
              @click="loadHistory(row)"
            >
              查看
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { CopyDocument, Download } from '@element-plus/icons-vue'
import axios from 'axios'

interface Model {
  id: string
  name: string
  description: string
}

interface Template {
  id: string
  name: string
  description: string
  prompt: string
}

interface HistoryItem {
  id: string
  prompt: string
  model: string
  generatedAt: string
  code?: string
}

const form = reactive({
  projectName: 'GeneratedSolution',
  model: 'ernie-3.5-8k',
  prompt: '',
  outputFormat: 'json'
})

const models = ref<Model[]>([])
const templates = ref<Template[]>([])
const selectedTemplate = ref('')
const isGenerating = ref(false)
const generatedCode = ref('')
const history = ref<HistoryItem[]>([])

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001'

onMounted(async () => {
  await loadModels()
  await loadTemplates()
  loadHistoryFromStorage()
})

async function loadModels() {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/aisolution/models`)
    models.value = response.data
  } catch (error) {
    console.error('加载模型失败:', error)
    // 使用默认模型列表
    models.value = [
      { id: 'ernie-tiny-8k', name: 'ERNIE-Tiny (快速)', description: '适合简单任务' },
      { id: 'ernie-speed-8k', name: 'ERNIE-Speed (均衡)', description: '速度与质量均衡' },
      { id: 'ernie-3.5-8k', name: 'ERNIE-3.5 (推荐)', description: '高质量代码生成' },
      { id: 'ernie-4.0-8k', name: 'ERNIE-4.0 (高级)', description: '最强能力' }
    ]
  }
}

async function loadTemplates() {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/aisolution/templates`)
    templates.value = response.data
  } catch (error) {
    console.error('加载模板失败:', error)
  }
}

function onTemplateChange(templateId: string) {
  const template = templates.value.find(t => t.id === templateId)
  if (template) {
    form.prompt = template.prompt + form.prompt
  }
}

async function generateSolution() {
  if (!form.prompt.trim()) {
    ElMessage.warning('请输入需求描述')
    return
  }

  isGenerating.value = true
  generatedCode.value = ''

  try {
    const response = await axios.post(
      `${API_BASE_URL}/api/aisolution/generate`,
      {
        prompt: form.prompt,
        model: form.model,
        projectName: form.projectName,
        outputFormat: form.outputFormat
      },
      {
        responseType: form.outputFormat === 'zip' ? 'blob' : 'json'
      }
    )

    if (form.outputFormat === 'zip') {
      // 下载ZIP文件
      const blob = new Blob([response.data], { type: 'application/zip' })
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `${form.projectName}.zip`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      window.URL.revokeObjectURL(url)
      
      ElMessage.success('解决方案已下载')
    } else {
      // 显示代码
      generatedCode.value = response.data.code
      
      // 保存到历史
      saveToHistory({
        prompt: form.prompt,
        model: form.model,
        generatedAt: new Date().toISOString(),
        code: response.data.code
      })
      
      ElMessage.success('代码生成成功')
    }
  } catch (error: any) {
    console.error('生成失败:', error)
    ElMessage.error(error.response?.data?.error || '生成失败，请稍后重试')
  } finally {
    isGenerating.value = false
  }
}

function clearForm() {
  form.prompt = ''
  form.projectName = 'GeneratedSolution'
  selectedTemplate.value = ''
  generatedCode.value = ''
}

function copyCode() {
  if (!generatedCode.value) return
  
  navigator.clipboard.writeText(generatedCode.value).then(() => {
    ElMessage.success('代码已复制到剪贴板')
  }).catch(() => {
    ElMessage.error('复制失败')
  })
}

async function downloadAsZip() {
  if (!generatedCode.value) return
  
  isGenerating.value = true
  try {
    const response = await axios.post(
      `${API_BASE_URL}/api/aisolution/generate`,
      {
        prompt: form.prompt,
        model: form.model,
        projectName: form.projectName,
        outputFormat: 'zip'
      },
      {
        responseType: 'blob'
      }
    )

    const blob = new Blob([response.data], { type: 'application/zip' })
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${form.projectName}.zip`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    window.URL.revokeObjectURL(url)
    
    ElMessage.success('ZIP文件已下载')
  } catch (error) {
    ElMessage.error('下载失败')
  } finally {
    isGenerating.value = false
  }
}

function saveToHistory(item: Omit<HistoryItem, 'id'>) {
  const historyItem = {
    ...item,
    id: Date.now().toString()
  }
  
  history.value.unshift(historyItem)
  if (history.value.length > 10) {
    history.value = history.value.slice(0, 10)
  }
  
  localStorage.setItem('ai-solution-history', JSON.stringify(history.value))
}

function loadHistoryFromStorage() {
  const stored = localStorage.getItem('ai-solution-history')
  if (stored) {
    try {
      history.value = JSON.parse(stored)
    } catch (error) {
      console.error('加载历史记录失败:', error)
    }
  }
}

function loadHistory(item: HistoryItem) {
  form.prompt = item.prompt
  form.model = item.model
  if (item.code) {
    generatedCode.value = item.code
  }
}

function formatDate(dateStr: string) {
  const date = new Date(dateStr)
  return date.toLocaleString('zh-CN')
}
</script>

<style scoped>
.ai-solution-generator {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
}

.generator-card {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.code-preview-card {
  margin-bottom: 20px;
}

.code-container {
  background: #1e1e1e;
  padding: 20px;
  border-radius: 4px;
  overflow-x: auto;
  max-height: 600px;
  overflow-y: auto;
}

.code-container pre {
  margin: 0;
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  font-size: 14px;
  line-height: 1.5;
}

.code-container code {
  color: #d4d4d4;
  white-space: pre;
}

.history-card {
  margin-bottom: 20px;
}

:deep(.el-select-dropdown__item) {
  height: auto;
  padding: 10px 20px;
}
</style>