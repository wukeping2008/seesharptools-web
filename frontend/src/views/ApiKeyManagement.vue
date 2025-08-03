<template>
  <div class="api-key-management">
    <div class="page-header">
      <h1>AI API密钥管理</h1>
      <p class="subtitle">配置和管理AI服务的API密钥，支持多个提供商和优先级设置</p>
    </div>

    <div class="management-content">
      <!-- 工具栏 -->
      <div class="toolbar">
        <el-button type="primary" @click="showAddDialog">
          <el-icon><Plus /></el-icon>
          添加API密钥
        </el-button>
        <el-button @click="refreshList">
          <el-icon><Refresh /></el-icon>
          刷新
        </el-button>
      </div>

      <!-- API密钥列表 -->
      <el-table 
        :data="apiKeys" 
        style="width: 100%"
        v-loading="loading"
        empty-text="暂无API密钥配置"
      >
        <el-table-column prop="provider" label="提供商" width="150">
          <template #default="scope">
            <el-tag :type="getProviderTagType(scope.row.provider)">
              {{ scope.row.provider }}
            </el-tag>
          </template>
        </el-table-column>
        
        <el-table-column prop="apiUrl" label="API端点" min-width="300" show-overflow-tooltip />
        
        <el-table-column prop="model" label="模型" width="200" show-overflow-tooltip />
        
        <el-table-column prop="isEnabled" label="状态" width="100" align="center">
          <template #default="scope">
            <el-switch
              v-model="scope.row.isEnabled"
              @change="toggleStatus(scope.row)"
              active-text="启用"
              inactive-text="禁用"
            />
          </template>
        </el-table-column>
        
        <el-table-column prop="priority" label="优先级" width="100" align="center">
          <template #default="scope">
            <el-tag size="small">{{ scope.row.priority }}</el-tag>
          </template>
        </el-table-column>
        
        <el-table-column label="使用统计" width="150" align="center">
          <template #default="scope">
            <el-button 
              size="small" 
              @click="showUsageStats(scope.row)"
              :disabled="!scope.row.id"
            >
              查看统计
            </el-button>
          </template>
        </el-table-column>
        
        <el-table-column label="操作" width="200" align="center" fixed="right">
          <template #default="scope">
            <el-button 
              size="small" 
              @click="validateApiKey(scope.row)"
              :loading="validatingId === scope.row.id"
            >
              验证
            </el-button>
            <el-button size="small" @click="editApiKey(scope.row)">编辑</el-button>
            <el-button 
              size="small" 
              type="danger" 
              @click="deleteApiKey(scope.row)"
            >
              删除
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 预置配置提示 -->
      <div class="preset-config" v-if="showPresetHint">
        <el-alert 
          title="发现预置配置" 
          type="info" 
          show-icon
          :closable="false"
        >
          <template #default>
            <p>系统检测到配置文件中的VolcesDeepseek API密钥，您可以：</p>
            <el-button 
              type="primary" 
              size="small" 
              @click="importPresetConfig"
              style="margin-top: 10px"
            >
              导入预置配置
            </el-button>
          </template>
        </el-alert>
      </div>
    </div>

    <!-- 添加/编辑对话框 -->
    <el-dialog 
      v-model="dialogVisible" 
      :title="isEdit ? '编辑API密钥' : '添加API密钥'"
      width="600px"
      @close="resetForm"
    >
      <el-form 
        ref="formRef" 
        :model="formData" 
        :rules="formRules" 
        label-width="100px"
      >
        <el-form-item label="提供商" prop="provider">
          <el-select 
            v-model="formData.provider" 
            placeholder="选择AI提供商"
            style="width: 100%"
            @change="onProviderChange"
          >
            <el-option label="DeepSeek" value="DeepSeek" />
            <el-option label="VolcesDeepSeek" value="VolcesDeepSeek" />
            <el-option label="Claude" value="Claude" />
            <el-option label="OpenAI" value="OpenAI" />
          </el-select>
        </el-form-item>
        
        <el-form-item label="API密钥" prop="apiKey">
          <el-input 
            v-model="formData.apiKey" 
            :placeholder="isEdit && formData.apiKey === '***已配置***' ? '留空保持原密钥不变' : '请输入API密钥'"
            type="password"
            show-password
          />
        </el-form-item>
        
        <el-form-item label="API端点" prop="apiUrl">
          <el-input 
            v-model="formData.apiUrl" 
            placeholder="请输入API端点URL"
          />
        </el-form-item>
        
        <el-form-item label="模型" prop="model">
          <el-input 
            v-model="formData.model" 
            placeholder="请输入模型名称"
          />
        </el-form-item>
        
        <el-form-item label="优先级" prop="priority">
          <el-input-number 
            v-model="formData.priority" 
            :min="0" 
            :max="999"
            placeholder="数字越小优先级越高"
          />
          <span class="form-tip">数字越小优先级越高</span>
        </el-form-item>
        
        <el-form-item label="启用状态" prop="isEnabled">
          <el-switch v-model="formData.isEnabled" />
        </el-form-item>
        
        <el-form-item label="备注" prop="notes">
          <el-input 
            v-model="formData.notes" 
            type="textarea" 
            :rows="3"
            placeholder="可选备注信息"
          />
        </el-form-item>
      </el-form>
      
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button 
          type="primary" 
          @click="saveApiKey" 
          :loading="saving"
        >
          {{ isEdit ? '更新' : '添加' }}
        </el-button>
      </template>
    </el-dialog>

    <!-- 使用统计对话框 -->
    <el-dialog 
      v-model="statsDialogVisible" 
      title="API使用统计" 
      width="800px"
    >
      <div v-loading="statsLoading">
        <!-- 统计摘要 -->
        <div class="stats-summary" v-if="usageSummary">
          <el-row :gutter="20">
            <el-col :span="6">
              <el-statistic title="总请求数" :value="usageSummary.totalRequests" />
            </el-col>
            <el-col :span="6">
              <el-statistic 
                title="成功率" 
                :value="usageSummary.successRate" 
                suffix="%"
                :precision="2"
              />
            </el-col>
            <el-col :span="6">
              <el-statistic title="总Token数" :value="usageSummary.totalTokens" />
            </el-col>
            <el-col :span="6">
              <el-statistic 
                title="平均响应时间" 
                :value="usageSummary.averageResponseTime" 
                suffix="ms"
                :precision="0"
              />
            </el-col>
          </el-row>
        </div>

        <!-- 使用记录列表 -->
        <el-table 
          :data="usageStats" 
          style="width: 100%; margin-top: 20px"
          max-height="400"
        >
          <el-table-column 
            prop="timestamp" 
            label="时间" 
            width="180"
            :formatter="formatTimestamp"
          />
          <el-table-column prop="useCase" label="使用场景" />
          <el-table-column prop="totalTokens" label="Token数" width="100" align="center" />
          <el-table-column prop="responseTimeMs" label="响应时间(ms)" width="120" align="center" />
          <el-table-column prop="success" label="状态" width="80" align="center">
            <template #default="scope">
              <el-tag :type="scope.row.success ? 'success' : 'danger'" size="small">
                {{ scope.row.success ? '成功' : '失败' }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Refresh } from '@element-plus/icons-vue'
import type { FormInstance } from 'element-plus'
import axios from 'axios'

// API基础URL
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001'

// 数据定义
interface ApiKeyConfig {
  id?: number
  provider: string
  apiKey: string
  apiUrl: string
  model: string
  isEnabled: boolean
  priority: number
  notes?: string
  createdAt?: string
  updatedAt?: string
}

interface UsageStats {
  id: number
  timestamp: string
  requestTokens: number
  responseTokens: number
  totalTokens: number
  success: boolean
  errorMessage?: string
  responseTimeMs: number
  useCase: string
}

interface UsageSummary {
  totalRequests: number
  successfulRequests: number
  failedRequests: number
  totalTokens: number
  averageResponseTime: number
  successRate: number
}

// 响应式数据
const loading = ref(false)
const saving = ref(false)
const validatingId = ref<number | null>(null)
const statsLoading = ref(false)
const dialogVisible = ref(false)
const statsDialogVisible = ref(false)
const isEdit = ref(false)
const apiKeys = ref<ApiKeyConfig[]>([])
const usageStats = ref<UsageStats[]>([])
const usageSummary = ref<UsageSummary | null>(null)
const formRef = ref<FormInstance>()

const formData = ref<ApiKeyConfig>({
  provider: '',
  apiKey: '',
  apiUrl: '',
  model: '',
  isEnabled: true,
  priority: 0,
  notes: ''
})

// 表单验证规则
const formRules = {
  provider: [
    { required: true, message: '请选择提供商', trigger: 'change' }
  ],
  apiKey: [
    { required: true, message: '请输入API密钥', trigger: 'blur', 
      validator: (rule: any, value: string, callback: any) => {
        if (isEdit.value && value === '***已配置***') {
          callback()
        } else if (!value) {
          callback(new Error('请输入API密钥'))
        } else {
          callback()
        }
      }
    }
  ],
  apiUrl: [
    { required: true, message: '请输入API端点', trigger: 'blur' },
    { type: 'url', message: '请输入有效的URL', trigger: 'blur' }
  ],
  model: [
    { required: true, message: '请输入模型名称', trigger: 'blur' }
  ]
}

// 是否显示预置配置提示
const showPresetHint = computed(() => {
  return !apiKeys.value.some(key => key.provider === 'VolcesDeepSeek')
})

// 加载API密钥列表
const loadApiKeys = async () => {
  loading.value = true
  try {
    const response = await axios.get(`${API_BASE_URL}/api/apikey`)
    apiKeys.value = response.data
  } catch (error) {
    console.error('加载API密钥失败:', error)
    ElMessage.error('加载API密钥失败')
  } finally {
    loading.value = false
  }
}

// 刷新列表
const refreshList = () => {
  loadApiKeys()
}

// 显示添加对话框
const showAddDialog = () => {
  isEdit.value = false
  resetForm()
  dialogVisible.value = true
}

// 编辑API密钥
const editApiKey = (row: ApiKeyConfig) => {
  isEdit.value = true
  formData.value = { ...row }
  dialogVisible.value = true
}

// 删除API密钥
const deleteApiKey = async (row: ApiKeyConfig) => {
  try {
    await ElMessageBox.confirm(
      `确定要删除 ${row.provider} 的API密钥配置吗？`,
      '删除确认',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    
    await axios.delete(`${API_BASE_URL}/api/apikey/${row.id}`)
    ElMessage.success('删除成功')
    loadApiKeys()
  } catch (error: any) {
    if (error !== 'cancel') {
      console.error('删除失败:', error)
      ElMessage.error('删除失败')
    }
  }
}

// 切换启用状态
const toggleStatus = async (row: ApiKeyConfig) => {
  try {
    await axios.put(`${API_BASE_URL}/api/apikey/${row.id}`, row)
    ElMessage.success('状态更新成功')
  } catch (error) {
    console.error('状态更新失败:', error)
    ElMessage.error('状态更新失败')
    row.isEnabled = !row.isEnabled
  }
}

// 验证API密钥
const validateApiKey = async (row: ApiKeyConfig) => {
  validatingId.value = row.id || null
  try {
    const response = await axios.post(`${API_BASE_URL}/api/apikey/validate`, {
      provider: row.provider,
      apiKey: row.apiKey
    })
    
    if (response.data.valid) {
      ElMessage.success('API密钥验证成功')
    } else {
      ElMessage.error('API密钥验证失败')
    }
  } catch (error) {
    console.error('验证失败:', error)
    ElMessage.error('验证请求失败')
  } finally {
    validatingId.value = null
  }
}

// 保存API密钥
const saveApiKey = async () => {
  if (!formRef.value) return
  
  await formRef.value.validate(async (valid) => {
    if (!valid) return
    
    saving.value = true
    try {
      if (isEdit.value) {
        // 如果密钥没有修改，不发送密钥字段
        const updateData = { ...formData.value }
        if (updateData.apiKey === '***已配置***') {
          updateData.apiKey = ''
        }
        await axios.put(`${API_BASE_URL}/api/apikey/${formData.value.id}`, updateData)
        ElMessage.success('更新成功')
      } else {
        await axios.post(`${API_BASE_URL}/api/apikey`, formData.value)
        ElMessage.success('添加成功')
      }
      dialogVisible.value = false
      loadApiKeys()
    } catch (error: any) {
      console.error('保存失败:', error)
      ElMessage.error(error.response?.data?.error || '保存失败')
    } finally {
      saving.value = false
    }
  })
}

// 查看使用统计
const showUsageStats = async (row: ApiKeyConfig) => {
  if (!row.id) return
  
  statsDialogVisible.value = true
  statsLoading.value = true
  
  try {
    // 获取统计摘要
    const summaryResponse = await axios.get(`${API_BASE_URL}/api/apikey/${row.id}/usage/summary`)
    usageSummary.value = summaryResponse.data
    
    // 获取详细记录
    const statsResponse = await axios.get(`${API_BASE_URL}/api/apikey/${row.id}/usage`)
    usageStats.value = statsResponse.data
  } catch (error) {
    console.error('加载统计数据失败:', error)
    ElMessage.error('加载统计数据失败')
  } finally {
    statsLoading.value = false
  }
}

// 导入预置配置
const importPresetConfig = async () => {
  try {
    await ElMessageBox.confirm(
      '将导入系统配置文件中的VolcesDeepSeek API密钥，是否继续？',
      '导入确认',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'info'
      }
    )
    
    const presetConfig: ApiKeyConfig = {
      provider: 'VolcesDeepSeek',
      apiKey: '9c4b2be6-1c6f-4da4-b81d-41a1899136ca',
      apiUrl: 'https://ark.cn-beijing.volces.com/api/v3/chat/completions',
      model: 'deepseek-r1-250528',
      isEnabled: true,
      priority: 0,
      notes: '预置配置 - VolcesDeepSeek'
    }
    
    await axios.post(`${API_BASE_URL}/api/apikey`, presetConfig)
    ElMessage.success('导入成功')
    loadApiKeys()
  } catch (error: any) {
    if (error !== 'cancel') {
      console.error('导入失败:', error)
      ElMessage.error('导入失败')
    }
  }
}

// 重置表单
const resetForm = () => {
  formData.value = {
    provider: '',
    apiKey: '',
    apiUrl: '',
    model: '',
    isEnabled: true,
    priority: 0,
    notes: ''
  }
  formRef.value?.resetFields()
}

// 提供商改变时自动填充默认值
const onProviderChange = (provider: string) => {
  switch (provider) {
    case 'DeepSeek':
      formData.value.apiUrl = 'https://api.deepseek.com/v1/chat/completions'
      formData.value.model = 'deepseek-coder'
      break
    case 'VolcesDeepSeek':
      formData.value.apiUrl = 'https://ark.cn-beijing.volces.com/api/v3/chat/completions'
      formData.value.model = 'deepseek-r1-250528'
      break
    case 'Claude':
      formData.value.apiUrl = 'https://api.anthropic.com/v1/messages'
      formData.value.model = 'claude-3-sonnet-20240229'
      break
    case 'OpenAI':
      formData.value.apiUrl = 'https://api.openai.com/v1/chat/completions'
      formData.value.model = 'gpt-4'
      break
  }
}

// 获取提供商标签类型
const getProviderTagType = (provider: string) => {
  const typeMap: Record<string, string> = {
    'DeepSeek': 'primary',
    'VolcesDeepSeek': 'success',
    'Claude': 'warning',
    'OpenAI': 'info'
  }
  return typeMap[provider] || 'info'
}

// 格式化时间戳
const formatTimestamp = (row: any) => {
  return new Date(row.timestamp).toLocaleString('zh-CN')
}

// 组件挂载时加载数据
onMounted(() => {
  loadApiKeys()
})
</script>

<style scoped>
.api-key-management {
  padding: 20px;
}

.page-header {
  margin-bottom: 30px;
}

.page-header h1 {
  font-size: 24px;
  margin: 0 0 10px 0;
}

.subtitle {
  color: #666;
  font-size: 14px;
  margin: 0;
}

.toolbar {
  margin-bottom: 20px;
  display: flex;
  gap: 10px;
}

.preset-config {
  margin-top: 20px;
}

.form-tip {
  color: #999;
  font-size: 12px;
  margin-left: 10px;
}

.stats-summary {
  padding: 20px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

:deep(.el-statistic__content) {
  font-size: 24px;
}

:deep(.el-statistic__head) {
  font-size: 14px;
  color: #666;
}
</style>