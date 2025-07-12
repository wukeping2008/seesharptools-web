<template>
  <div class="ai-generator-page">
    <h1>AI控件生成器</h1>
    
    <!-- API状态提示 -->
    <el-alert
      :title="apiStatusMessage"
      :type="apiStatusType"
      :closable="false"
      show-icon
      style="margin-bottom: 20px"
    />
    
    <!-- 输入区域 -->
    <div class="input-section">
      <h2>描述您需要的控件</h2>
      <el-input
        v-model="description"
        type="textarea"
        :rows="3"
        placeholder="例如：创建一个仪表盘显示温度"
      />
      
      <div class="template-section">
        <h3>或选择预定义模板：</h3>
        <div class="template-list">
          <el-button
            v-for="template in templates"
            :key="template.id"
            @click="selectTemplate(template)"
            size="small"
          >
            {{ template.name }}
          </el-button>
        </div>
      </div>
      
      <el-button 
        type="primary" 
        @click="generateControl"
        :loading="isGenerating"
        :disabled="!description.trim()"
      >
        生成控件
      </el-button>
    </div>
    
    <!-- 结果区域 -->
    <div v-if="generatedCode" class="result-section">
      <h2>生成的控件代码</h2>
      
      <!-- 代码显示 -->
      <div class="code-container">
        <div class="code-header">
          <span>Vue组件代码</span>
          <el-button size="small" @click="copyCode">复制代码</el-button>
        </div>
        <pre class="code-block"><code>{{ generatedCode }}</code></pre>
      </div>
      
      <!-- 预览区域 -->
      <div class="preview-section">
        <h3>控件预览</h3>
        <div class="preview-container">
          <!-- 仪表盘预览 -->
          <div v-if="previewType === 'gauge'" class="preview-item">
            <GaugePreview :value="42" unit="°C" title="温度" />
          </div>
          
          <!-- LED预览 -->
          <div v-else-if="previewType === 'led'" class="preview-item">
            <LEDPreview :is-on="true" color="#00ff00" label="状态" />
          </div>
          
          <!-- 按钮预览 -->
          <div v-else-if="previewType === 'button'" class="preview-item">
            <ButtonPreview text="点击我" @click="handleButtonClick" />
          </div>
        </div>
      </div>
    </div>
    
    <!-- 错误提示 -->
    <el-alert
      v-if="error"
      :title="error"
      type="error"
      :closable="true"
      @close="error = ''"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { aiControlService } from '@/services/AIControlService'
import type { ControlTemplate } from '@/types/ai'
import { backendApi } from '@/services/BackendApiService'

// 预览组件
import GaugePreview from '@/components/ai/GaugePreview.vue'
import LEDPreview from '@/components/ai/LEDPreview.vue'
import ButtonPreview from '@/components/ai/ButtonPreview.vue'

// 响应式数据
const description = ref('')
const isGenerating = ref(false)
const generatedCode = ref('')
const error = ref('')
const templates = ref<ControlTemplate[]>([])
const previewType = ref('')
const hasApiKey = ref(false)

// API状态计算属性
const apiStatusMessage = computed(() => {
  return hasApiKey.value 
    ? '✅ 已配置Claude API，将使用真实AI生成控件' 
    : '⚠️ 未配置Claude API，将使用预定义模板（查看文档了解如何配置）'
})

const apiStatusType = computed(() => {
  return hasApiKey.value ? 'success' : 'warning'
})

// 获取模板列表和检查API状态
onMounted(async () => {
  templates.value = aiControlService.getTemplates()
  
  // 检查API是否可用
  try {
    const response = await backendApi.get('/api/ai/status').catch(() => null)
    hasApiKey.value = response?.hasApiKey || false
  } catch {
    hasApiKey.value = false
  }
})

// 选择模板
const selectTemplate = (template: ControlTemplate) => {
  description.value = template.description
  generatedCode.value = template.code
  previewType.value = template.id
}

// 生成控件
const generateControl = async () => {
  if (!description.value.trim()) {
    ElMessage.warning('请输入控件描述')
    return
  }
  
  isGenerating.value = true
  error.value = ''
  
  try {
    const response = await aiControlService.generateControl({
      description: description.value
    })
    
    if (response.success && response.code) {
      generatedCode.value = response.code
      
      // 根据描述判断预览类型
      const desc = description.value.toLowerCase()
      if (desc.includes('led') || desc.includes('灯')) {
        previewType.value = 'led'
      } else if (desc.includes('按钮') || desc.includes('button')) {
        previewType.value = 'button'
      } else {
        previewType.value = 'gauge'
      }
      
      ElMessage.success('控件生成成功！')
    } else {
      error.value = response.error || '生成失败'
    }
  } catch (err) {
    error.value = err instanceof Error ? err.message : '生成过程中发生错误'
  } finally {
    isGenerating.value = false
  }
}

// 复制代码
const copyCode = async () => {
  try {
    await navigator.clipboard.writeText(generatedCode.value)
    ElMessage.success('代码已复制到剪贴板')
  } catch (err) {
    ElMessage.error('复制失败')
  }
}

// 处理按钮点击
const handleButtonClick = () => {
  ElMessage.info('按钮被点击了！')
}
</script>

<style scoped>
.ai-generator-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

h1 {
  color: #333;
  margin-bottom: 30px;
}

h2 {
  color: #666;
  font-size: 20px;
  margin-bottom: 15px;
}

h3 {
  color: #666;
  font-size: 16px;
  margin-bottom: 10px;
}

.input-section {
  background: #f5f5f5;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 30px;
}

.template-section {
  margin: 20px 0;
}

.template-list {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.result-section {
  margin-top: 30px;
}

.code-container {
  background: #f8f8f8;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
  margin-bottom: 30px;
}

.code-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 15px;
  background: #e8e8e8;
  border-bottom: 1px solid #d0d0d0;
}

.code-block {
  margin: 0;
  padding: 15px;
  overflow-x: auto;
  font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
  font-size: 14px;
  line-height: 1.5;
}

.preview-section {
  background: #f5f5f5;
  padding: 20px;
  border-radius: 8px;
}

.preview-container {
  background: white;
  padding: 30px;
  border-radius: 8px;
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 300px;
}

.preview-item {
  display: flex;
  justify-content: center;
  align-items: center;
}
</style>
