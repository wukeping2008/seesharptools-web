<template>
  <div class="ai-control-generator">
    <!-- 头部标题 -->
    <div class="generator-header">
      <h2>🤖 AI智能控件生成器</h2>
      <p class="subtitle">使用自然语言描述，AI将为您生成专业的Vue 3控件</p>
    </div>

    <!-- 主要内容区域 -->
    <el-row :gutter="24">
      <!-- 左侧：输入区域 -->
      <el-col :span="8">
        <el-card class="input-card">
          <template #header>
            <div class="card-header">
              <span>📝 控件需求描述</span>
              <el-tag v-if="isGenerating" type="warning" effect="plain">
                <el-icon class="is-loading"><Loading /></el-icon>
                生成中...
              </el-tag>
            </div>
          </template>

          <!-- 需求描述输入 -->
          <div class="input-section">
            <el-form :model="request" label-width="100px">
              <el-form-item label="控件描述">
                <el-input
                  v-model="request.description"
                  type="textarea"
                  :rows="6"
                  placeholder="请详细描述您需要的控件功能，例如：&#10;我需要一个显示温度的仪表盘控件，支持摄氏度和华氏度切换，有报警功能，当温度超过80度时显示红色警告..."
                  :disabled="isGenerating"
                />
                <div class="char-count">
                  {{ request.description.length }}/1000
                </div>
              </el-form-item>

              <el-form-item label="控件类型">
                <el-select v-model="request.type" placeholder="选择控件类型（可选）" :disabled="isGenerating">
                  <el-option label="仪表盘" value="gauge" />
                  <el-option label="图表" value="chart" />
                  <el-option label="指示器" value="indicator" />
                  <el-option label="控制器" value="control" />
                  <el-option label="仪器" value="instrument" />
                  <el-option label="自定义" value="custom" />
                </el-select>
              </el-form-item>

              <el-form-item label="样式风格">
                <el-select v-model="request.stylePreference" placeholder="选择样式风格" :disabled="isGenerating">
                  <el-option label="专业工业风" value="professional" />
                  <el-option label="现代简约" value="modern" />
                  <el-option label="经典传统" value="classic" />
                  <el-option label="极简主义" value="minimal" />
                </el-select>
              </el-form-item>

              <!-- 高级选项 -->
              <el-collapse v-model="activeCollapse">
                <el-collapse-item title="高级选项" name="advanced">
                  <el-form-item label="尺寸约束">
                    <el-row :gutter="12">
                      <el-col :span="12">
                        <el-input-number
                          v-model="request.constraints!.width"
                          placeholder="宽度"
                          :min="100"
                          :max="2000"
                          :disabled="isGenerating"
                        />
                      </el-col>
                      <el-col :span="12">
                        <el-input-number
                          v-model="request.constraints!.height"
                          placeholder="高度"
                          :min="100"
                          :max="1000"
                          :disabled="isGenerating"
                        />
                      </el-col>
                    </el-row>
                  </el-form-item>

                  <el-form-item label="数据类型">
                    <el-select v-model="request.constraints!.dataType" :disabled="isGenerating">
                      <el-option label="数值" value="number" />
                      <el-option label="文本" value="string" />
                      <el-option label="布尔值" value="boolean" />
                      <el-option label="数组" value="array" />
                    </el-select>
                  </el-form-item>

                  <el-form-item label="功能特性">
                    <el-checkbox-group v-model="request.features" :disabled="isGenerating">
                      <el-checkbox label="实时数据更新">实时数据</el-checkbox>
                      <el-checkbox label="用户交互">交互功能</el-checkbox>
                      <el-checkbox label="动画效果">动画效果</el-checkbox>
                      <el-checkbox label="主题切换">主题支持</el-checkbox>
                      <el-checkbox label="数据导出">数据导出</el-checkbox>
                    </el-checkbox-group>
                  </el-form-item>
                </el-collapse-item>
              </el-collapse>

              <!-- 操作按钮 -->
              <div class="action-buttons">
                <el-button
                  type="primary"
                  size="large"
                  @click="generateControl"
                  :loading="isGenerating"
                  :disabled="!canGenerate"
                >
                  <el-icon><Star /></el-icon>
                  {{ isGenerating ? '生成中...' : '生成控件' }}
                </el-button>
                <el-button @click="clearForm" :disabled="isGenerating">
                  清空
                </el-button>
                <el-dropdown @command="loadExampleByType" :disabled="isGenerating">
                  <el-button :disabled="isGenerating">
                    加载示例
                    <el-icon class="el-icon--right"><ArrowDown /></el-icon>
                  </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item command="temperature-gauge">🌡️ 温度仪表盘</el-dropdown-item>
                      <el-dropdown-item command="pressure-gauge">⚡ 压力表控件</el-dropdown-item>
                      <el-dropdown-item command="digital-display">📟 数字显示器</el-dropdown-item>
                      <el-dropdown-item command="led-indicator">💡 LED指示灯</el-dropdown-item>
                      <el-dropdown-item command="switch-control">🔘 开关控件</el-dropdown-item>
                      <el-dropdown-item command="chart-display">📊 实时图表</el-dropdown-item>
                      <el-dropdown-item command="oscilloscope">📈 示波器界面</el-dropdown-item>
                      <el-dropdown-item command="multimeter">🔌 数字万用表</el-dropdown-item>
                      <el-dropdown-item command="signal-generator">📡 信号发生器</el-dropdown-item>
                      <el-dropdown-item command="power-supply">🔋 电源控制器</el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </div>
            </el-form>
          </div>
        </el-card>

        <!-- 历史记录 -->
        <el-card class="history-card" v-if="conversationHistory.length > 0">
          <template #header>
            <span>📚 生成历史</span>
          </template>
          <div class="history-list">
            <div
              v-for="item in conversationHistory.slice(-5)"
              :key="item.id"
              class="history-item"
              @click="loadFromHistory(item)"
            >
              <div class="history-description">{{ item.userMessage.slice(0, 50) }}...</div>
              <div class="history-meta">
                <el-tag :type="item.aiResponse.success ? 'success' : 'danger'" size="small">
                  {{ item.aiResponse.success ? '成功' : '失败' }}
                </el-tag>
                <span class="history-time">{{ formatTime(item.timestamp) }}</span>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 右侧：结果展示区域 -->
      <el-col :span="16">
        <!-- 预览面板 -->
        <el-card class="preview-card" style="margin-bottom: 16px; height: 600px;">
          <AIControlPreview :control="lastResponse?.control" />
        </el-card>
        
        <!-- 结果面板 -->
        <el-card class="result-card">
          <template #header>
            <div class="card-header">
              <span>🎯 生成结果</span>
              <div v-if="lastResponse">
                <el-tag :type="lastResponse.success ? 'success' : 'danger'">
                  {{ lastResponse.success ? '生成成功' : '生成失败' }}
                </el-tag>
                <el-tag v-if="lastResponse.metadata" type="info" style="margin-left: 8px;">
                  {{ lastResponse.metadata.processingTime }}ms
                </el-tag>
              </div>
            </div>
          </template>

          <!-- 生成结果展示 -->
          <div v-if="!lastResponse" class="empty-result">
            <el-empty description="请在左侧输入控件需求并点击生成">
              <el-icon size="64" color="#ddd"><DocumentAdd /></el-icon>
            </el-empty>
          </div>

          <div v-else-if="!lastResponse.success" class="error-result">
            <el-alert
              :title="lastResponse.error"
              type="error"
              :closable="false"
              show-icon
            />
          </div>

          <div v-else-if="lastResponse.control" class="success-result">
            <!-- 控件信息 -->
            <div class="control-info">
              <h3>{{ lastResponse.control.name }}</h3>
              <p class="control-description">{{ lastResponse.control.description }}</p>
              
              <!-- 可行性评估 -->
              <div v-if="lastResponse.feasibility" class="feasibility">
                <el-progress
                  :percentage="lastResponse.feasibility.score"
                  :color="getFeasibilityColor(lastResponse.feasibility.score)"
                  :stroke-width="8"
                />
                <div class="feasibility-text">可行性评分: {{ lastResponse.feasibility.score }}/100</div>
              </div>
            </div>

            <!-- 代码展示 -->
            <el-tabs v-model="activeTab" class="code-tabs">
              <el-tab-pane label="Vue组件" name="component">
                <div class="code-container">
                  <div class="code-header">
                    <span>{{ lastResponse.control.name }}.vue</span>
                    <el-button size="small" @click="copyCode(lastResponse.control.componentCode)">
                      <el-icon><DocumentCopy /></el-icon>
                      复制
                    </el-button>
                  </div>
                  <pre><code class="language-vue">{{ lastResponse.control.componentCode }}</code></pre>
                </div>
              </el-tab-pane>

              <el-tab-pane label="类型定义" name="types">
                <div class="code-container">
                  <div class="code-header">
                    <span>types.ts</span>
                    <el-button size="small" @click="copyCode(lastResponse.control.typeDefinitions)">
                      <el-icon><DocumentCopy /></el-icon>
                      复制
                    </el-button>
                  </div>
                  <pre><code class="language-typescript">{{ lastResponse.control.typeDefinitions }}</code></pre>
                </div>
              </el-tab-pane>

              <el-tab-pane label="样式代码" name="styles">
                <div class="code-container">
                  <div class="code-header">
                    <span>styles.scss</span>
                    <el-button size="small" @click="copyCode(lastResponse.control.styleCode)">
                      <el-icon><DocumentCopy /></el-icon>
                      复制
                    </el-button>
                  </div>
                  <pre><code class="language-scss">{{ lastResponse.control.styleCode }}</code></pre>
                </div>
              </el-tab-pane>

              <el-tab-pane label="使用示例" name="example">
                <div class="code-container">
                  <div class="code-header">
                    <span>Example.vue</span>
                    <el-button size="small" @click="copyCode(lastResponse.control.exampleCode)">
                      <el-icon><DocumentCopy /></el-icon>
                      复制
                    </el-button>
                  </div>
                  <pre><code class="language-vue">{{ lastResponse.control.exampleCode }}</code></pre>
                </div>
              </el-tab-pane>

              <el-tab-pane label="属性文档" name="props">
                <div class="props-container">
                  <el-table :data="lastResponse.control.props" style="width: 100%">
                    <el-table-column prop="name" label="属性名" width="120" />
                    <el-table-column prop="type" label="类型" width="100" />
                    <el-table-column prop="required" label="必需" width="80">
                      <template #default="scope">
                        <el-tag :type="scope.row.required ? 'danger' : 'info'" size="small">
                          {{ scope.row.required ? '是' : '否' }}
                        </el-tag>
                      </template>
                    </el-table-column>
                    <el-table-column prop="description" label="描述" />
                  </el-table>
                </div>
              </el-tab-pane>
            </el-tabs>

            <!-- 操作按钮 -->
            <div class="result-actions">
              <el-button type="primary" @click="downloadControl">
                <el-icon><Download /></el-icon>
                下载控件
              </el-button>
              <el-button @click="previewControl">
                <el-icon><View /></el-icon>
                预览控件
              </el-button>
              <el-button @click="shareControl">
                <el-icon><Share /></el-icon>
                分享控件
              </el-button>
            </div>
          </div>
        </el-card>

        <!-- 统计信息 -->
        <el-card class="stats-card" v-if="stats" style="margin-top: 16px;">
          <template #header>
            <span>📊 生成统计</span>
          </template>
          <el-row :gutter="16">
            <el-col :span="6">
              <div class="stat-item">
                <div class="stat-value">{{ stats.totalRequests }}</div>
                <div class="stat-label">总请求</div>
              </div>
            </el-col>
            <el-col :span="6">
              <div class="stat-item">
                <div class="stat-value">{{ stats.successfulGenerations }}</div>
                <div class="stat-label">成功生成</div>
              </div>
            </el-col>
            <el-col :span="6">
              <div class="stat-item">
                <div class="stat-value">{{ Math.round(stats.averageProcessingTime) }}ms</div>
                <div class="stat-label">平均耗时</div>
              </div>
            </el-col>
            <el-col :span="6">
              <div class="stat-item">
                <div class="stat-value">{{ Math.round(stats.userSatisfactionRating * 10) / 10 }}</div>
                <div class="stat-label">满意度</div>
              </div>
            </el-col>
          </el-row>
        </el-card>
      </el-col>
    </el-row>

    <!-- 预览对话框 -->
    <el-dialog 
      v-model="showPreview" 
      title="🔍 控件预览" 
      width="90%" 
      :before-close="closePreview"
      class="preview-dialog"
    >
      <div class="preview-container" v-if="lastResponse?.control">
        <!-- 预览工具栏 -->
        <div class="preview-toolbar">
          <div class="toolbar-left">
            <el-tag type="info">{{ lastResponse.control.name }}</el-tag>
            <el-tag type="success" v-if="lastResponse.control.type">{{ lastResponse.control.type }}</el-tag>
          </div>
          <div class="toolbar-right">
            <el-button-group>
              <el-button 
                :type="previewMode === 'interactive' ? 'primary' : 'default'"
                @click="previewMode = 'interactive'"
                size="small"
              >
                <el-icon><Setting /></el-icon>
                交互预览
              </el-button>
              <el-button 
                :type="previewMode === 'static' ? 'primary' : 'default'"
                @click="previewMode = 'static'"
                size="small"
              >
                <el-icon><Picture /></el-icon>
                静态预览
              </el-button>
              <el-button 
                :type="previewMode === 'code' ? 'primary' : 'default'"
                @click="previewMode = 'code'"
                size="small"
              >
                <el-icon><DocumentCopy /></el-icon>
                代码预览
              </el-button>
            </el-button-group>
            <el-button @click="refreshPreview" size="small">
              <el-icon><Refresh /></el-icon>
              刷新
            </el-button>
          </div>
        </div>

        <!-- 预览内容 -->
        <div class="preview-content">
          <!-- 交互预览模式 -->
          <div v-if="previewMode === 'interactive'" class="interactive-preview">
            <el-row :gutter="20">
              <el-col :span="16">
                <div class="preview-viewport">
                  <AIControlPreview :control="lastResponse.control" />
                </div>
              </el-col>
              <el-col :span="8">
                <div class="preview-controls">
                  <h4>🎛️ 预览控制</h4>
                  <el-form label-width="80px" size="small">
                    <el-form-item label="主题">
                      <el-select v-model="previewSettings.theme" @change="updatePreview">
                        <el-option label="浅色" value="light" />
                        <el-option label="深色" value="dark" />
                        <el-option label="专业" value="professional" />
                      </el-select>
                    </el-form-item>
                    <el-form-item label="尺寸">
                      <el-slider 
                        v-model="previewSettings.scale" 
                        :min="50" 
                        :max="150" 
                        :step="10"
                        @change="updatePreview"
                      />
                      <span class="scale-text">{{ previewSettings.scale }}%</span>
                    </el-form-item>
                    <el-form-item label="背景">
                      <el-color-picker 
                        v-model="previewSettings.backgroundColor" 
                        @change="updatePreview"
                      />
                    </el-form-item>
                    <el-form-item>
                      <el-checkbox 
                        v-model="previewSettings.showGrid" 
                        @change="updatePreview"
                      >
                        显示网格
                      </el-checkbox>
                    </el-form-item>
                    <el-form-item>
                      <el-checkbox 
                        v-model="previewSettings.showBorder" 
                        @change="updatePreview"
                      >
                        显示边框
                      </el-checkbox>
                    </el-form-item>
                  </el-form>
                  
                  <!-- 预览信息 -->
                  <div class="preview-info">
                    <h5>📊 控件信息</h5>
                    <div class="info-item">
                      <span class="label">类型:</span>
                      <span class="value">{{ lastResponse.control.type || 'custom' }}</span>
                    </div>
                    <div class="info-item">
                      <span class="label">属性数:</span>
                      <span class="value">{{ lastResponse.control.props?.length || 0 }}</span>
                    </div>
                    <div class="info-item">
                      <span class="label">代码行数:</span>
                      <span class="value">{{ getCodeLines() }}</span>
                    </div>
                  </div>
                </div>
              </el-col>
            </el-row>
          </div>

          <!-- 静态预览模式 -->
          <div v-else-if="previewMode === 'static'" class="static-preview">
            <div class="preview-showcase">
              <div class="showcase-item">
                <h4>默认状态</h4>
                <div class="showcase-viewport">
                  <AIControlPreview :control="lastResponse.control" />
                </div>
              </div>
              <div class="showcase-item">
                <h4>深色主题</h4>
                <div class="showcase-viewport dark-theme">
                  <AIControlPreview :control="lastResponse.control" />
                </div>
              </div>
              <div class="showcase-item">
                <h4>小尺寸</h4>
                <div class="showcase-viewport small-size">
                  <AIControlPreview :control="lastResponse.control" />
                </div>
              </div>
            </div>
          </div>

          <!-- 代码预览模式 -->
          <div v-else-if="previewMode === 'code'" class="code-preview">
            <el-tabs v-model="previewCodeTab">
              <el-tab-pane label="Vue组件" name="component">
                <div class="code-container">
                  <div class="code-header">
                    <span>{{ lastResponse.control.name }}.vue</span>
                    <el-button size="small" @click="copyCode(lastResponse.control.componentCode)">
                      <el-icon><DocumentCopy /></el-icon>
                      复制
                    </el-button>
                  </div>
                  <pre><code class="language-vue">{{ lastResponse.control.componentCode }}</code></pre>
                </div>
              </el-tab-pane>
              <el-tab-pane label="样式" name="styles">
                <div class="code-container">
                  <div class="code-header">
                    <span>styles.scss</span>
                    <el-button size="small" @click="copyCode(lastResponse.control.styleCode)">
                      <el-icon><DocumentCopy /></el-icon>
                      复制
                    </el-button>
                  </div>
                  <pre><code class="language-scss">{{ lastResponse.control.styleCode }}</code></pre>
                </div>
              </el-tab-pane>
              <el-tab-pane label="类型定义" name="types">
                <div class="code-container">
                  <div class="code-header">
                    <span>types.ts</span>
                    <el-button size="small" @click="copyCode(lastResponse.control.typeDefinitions)">
                      <el-icon><DocumentCopy /></el-icon>
                      复制
                    </el-button>
                  </div>
                  <pre><code class="language-typescript">{{ lastResponse.control.typeDefinitions }}</code></pre>
                </div>
              </el-tab-pane>
            </el-tabs>
          </div>
        </div>

        <!-- 预览操作栏 -->
        <div class="preview-actions">
          <el-button type="primary" @click="downloadFromPreview">
            <el-icon><Download /></el-icon>
            下载控件
          </el-button>
          <el-button @click="shareFromPreview">
            <el-icon><Share /></el-icon>
            分享控件
          </el-button>
          <el-button @click="closePreview">
            关闭预览
          </el-button>
        </div>
      </div>
      
      <div v-else class="no-control-preview">
        <el-empty description="没有可预览的控件">
          <el-icon size="64" color="#ddd"><DocumentAdd /></el-icon>
        </el-empty>
      </div>
    </el-dialog>

    <!-- 分享对话框 -->
    <el-dialog 
      v-model="showShare" 
      title="🔗 分享控件" 
      width="60%" 
      :before-close="closeShare"
    >
      <div class="share-container" v-if="lastResponse?.control">
        <div class="share-options">
          <h4>选择分享方式</h4>
          <el-row :gutter="16">
            <el-col :span="8">
              <el-card class="share-option" @click="shareViaLink" body-style="padding: 20px; text-align: center;">
                <el-icon size="32" color="#409EFF"><Link /></el-icon>
                <h5>生成链接</h5>
                <p>生成分享链接，其他人可以查看和下载</p>
              </el-card>
            </el-col>
            <el-col :span="8">
              <el-card class="share-option" @click="shareViaCode" body-style="padding: 20px; text-align: center;">
                <el-icon size="32" color="#67C23A"><DocumentCopy /></el-icon>
                <h5>复制代码</h5>
                <p>复制完整代码到剪贴板</p>
              </el-card>
            </el-col>
            <el-col :span="8">
              <el-card class="share-option" @click="shareViaExport" body-style="padding: 20px; text-align: center;">
                <el-icon size="32" color="#E6A23C"><Download /></el-icon>
                <h5>导出文件</h5>
                <p>导出为压缩包文件</p>
              </el-card>
            </el-col>
          </el-row>
        </div>

        <!-- 分享链接结果 -->
        <div v-if="shareResult.type === 'link'" class="share-result">
          <h4>🔗 分享链接已生成</h4>
          <el-input 
            v-model="shareResult.content" 
            readonly
            class="share-link-input"
          >
            <template #append>
              <el-button @click="copyShareLink">
                <el-icon><DocumentCopy /></el-icon>
                复制
              </el-button>
            </template>
          </el-input>
          <div class="share-info">
            <p><el-icon><InfoFilled /></el-icon> 链接有效期：7天</p>
            <p><el-icon><InfoFilled /></el-icon> 访问次数：无限制</p>
          </div>
        </div>

        <!-- 分享代码结果 -->
        <div v-if="shareResult.type === 'code'" class="share-result">
          <h4>📋 代码已复制到剪贴板</h4>
          <el-alert
            title="代码复制成功"
            type="success"
            description="完整的控件代码已复制到剪贴板，可以直接粘贴到项目中使用"
            show-icon
            :closable="false"
          />
        </div>

        <!-- 导出文件结果 -->
        <div v-if="shareResult.type === 'export'" class="share-result">
          <h4>📦 文件导出完成</h4>
          <el-alert
            title="文件下载成功"
            type="success"
            description="控件文件已打包下载，包含Vue组件、样式文件、类型定义和使用示例"
            show-icon
            :closable="false"
          />
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Loading,
  Star,
  DocumentAdd,
  DocumentCopy,
  Download,
  View,
  Share,
  ArrowDown,
  Setting,
  Picture,
  Refresh,
  Link,
  InfoFilled
} from '@element-plus/icons-vue'
import { aiControlService } from '@/services/AIControlService'
import AIControlPreview from './AIControlPreview.vue'
import type {
  AIControlRequest,
  AIControlResponse,
  AIConversationHistory,
  AIGenerationStats
} from '@/types/ai'

// 响应式数据
const isGenerating = ref(false)
const activeCollapse = ref<string[]>([])
const activeTab = ref('component')
const showPreview = ref(false)
const showShare = ref(false)

// 预览相关状态
const previewMode = ref<'interactive' | 'static' | 'code'>('interactive')
const previewCodeTab = ref('component')
const previewSettings = ref({
  theme: 'light',
  scale: 100,
  backgroundColor: '#ffffff',
  showGrid: false,
  showBorder: true
})

// 分享相关状态
const shareResult = ref<{
  type: 'link' | 'code' | 'export' | null
  content: string
}>({
  type: null,
  content: ''
})

// 请求数据
const request = ref<AIControlRequest>({
  description: '',
  type: undefined,
  stylePreference: 'professional',
  constraints: {
    width: 400,
    height: 300,
    dataType: 'number',
    realtime: false,
    interactive: true
  },
  features: []
})

// 响应数据
const lastResponse = ref<AIControlResponse | null>(null)
const conversationHistory = ref<AIConversationHistory[]>([])
const stats = ref<AIGenerationStats | null>(null)

// 计算属性
const canGenerate = computed(() => {
  return request.value.description.trim().length >= 10 && !isGenerating.value
})

// 生成控件
const generateControl = async () => {
  if (!canGenerate.value) {
    ElMessage.warning('请提供详细的控件描述（至少10个字符）')
    return
  }

  isGenerating.value = true
  
  try {
    const response = await aiControlService.generateControl(request.value)
    lastResponse.value = response
    
    if (response.success) {
      ElMessage.success('控件生成成功！')
    } else {
      ElMessage.error(response.error || '控件生成失败')
    }
    
    // 更新历史记录和统计
    conversationHistory.value = aiControlService.getConversationHistory()
    stats.value = aiControlService.getStats()
    
  } catch (error) {
    ElMessage.error('生成过程中发生错误')
    console.error('Generation error:', error)
  } finally {
    isGenerating.value = false
  }
}

// 清空表单
const clearForm = () => {
  request.value = {
    description: '',
    type: undefined,
    stylePreference: 'professional',
    constraints: {
      width: 400,
      height: 300,
      dataType: 'number',
      realtime: false,
      interactive: true
    },
    features: []
  }
  lastResponse.value = null
}

// 加载示例
const loadExample = () => {
  request.value.description = '我需要一个显示温度的圆形仪表盘控件，支持摄氏度和华氏度切换，当温度超过80度时显示红色警告，低于0度时显示蓝色，正常范围显示绿色。仪表盘要有刻度线和数字标记，中央显示当前温度值，底部显示单位。'
  request.value.type = 'gauge'
  request.value.features = ['实时数据更新', '用户交互', '动画效果']
}

// 根据类型加载示例
const loadExampleByType = (command: string) => {
  const examples = {
    'temperature-gauge': {
      description: '我需要一个显示温度的圆形仪表盘控件，支持摄氏度和华氏度切换，当温度超过80度时显示红色警告，低于0度时显示蓝色，正常范围显示绿色。仪表盘要有刻度线和数字标记，中央显示当前温度值，底部显示单位。',
      type: 'gauge',
      features: ['实时数据更新', '用户交互', '动画效果']
    },
    'pressure-gauge': {
      description: '创建一个压力表控件，显示范围0-10bar，带有红色危险区域（8-10bar），黄色警告区域（6-8bar），绿色正常区域（0-6bar）。需要数字显示当前压力值，支持单位切换（bar/psi/kPa）。',
      type: 'gauge',
      features: ['实时数据更新', '用户交互']
    },
    'digital-display': {
      description: '设计一个7段数码管风格的数字显示器，支持显示小数点，可以显示负数，有绿色/红色/蓝色三种颜色模式。背景为黑色，数字为发光效果，类似老式计算器显示屏。',
      type: 'indicator',
      features: ['实时数据更新', '动画效果']
    },
    'led-indicator': {
      description: '制作一个LED指示灯阵列控件，包含8个LED灯，每个LED可以独立控制开关和颜色（红/绿/黄/蓝）。支持闪烁模式，可以显示二进制数据或状态指示。',
      type: 'indicator',
      features: ['用户交互', '动画效果']
    },
    'switch-control': {
      description: '开发一个工业风格的开关控件，包括拨动开关、按钮开关、旋转开关三种类型。支持锁定状态，有明确的开/关状态指示，点击时有触觉反馈动画。',
      type: 'control',
      features: ['用户交互', '动画效果']
    },
    'chart-display': {
      description: '构建一个实时数据图表控件，支持折线图、柱状图、面积图三种模式。可以显示多条数据线，支持缩放、平移、数据点标记。有网格线和坐标轴标签。',
      type: 'chart',
      features: ['实时数据更新', '用户交互', '数据导出']
    },
    'oscilloscope': {
      description: '创建一个示波器界面控件，包含波形显示区域、时基调节、电压调节、触发设置。支持双通道显示，有测量游标功能，可以显示频率、幅值、相位等参数。',
      type: 'instrument',
      features: ['实时数据更新', '用户交互', '数据导出']
    },
    'multimeter': {
      description: '设计一个数字万用表控件，支持电压、电流、电阻、频率、电容测量。有大数字显示屏，量程自动/手动切换，保持功能，相对测量模式。',
      type: 'instrument',
      features: ['实时数据更新', '用户交互']
    },
    'signal-generator': {
      description: '开发一个信号发生器控件，可以生成正弦波、方波、三角波、锯齿波。支持频率、幅值、偏置调节，有波形预览窗口，支持扫频功能。',
      type: 'instrument',
      features: ['实时数据更新', '用户交互', '动画效果']
    },
    'power-supply': {
      description: '制作一个可编程电源控制器界面，包含电压设定、电流限制、输出开关控制。有实时电压电流显示，过压过流保护指示，支持预设值存储。',
      type: 'instrument',
      features: ['实时数据更新', '用户交互', '数据导出']
    }
  }

  const example = examples[command as keyof typeof examples]
  if (example) {
    request.value.description = example.description
    request.value.type = example.type as any
    request.value.features = example.features
    ElMessage.success(`已加载${command}示例`)
  }
}

// 从历史记录加载
const loadFromHistory = (item: AIConversationHistory) => {
  request.value.description = item.userMessage
  lastResponse.value = item.aiResponse
}

// 复制代码
const copyCode = async (code: string) => {
  try {
    await navigator.clipboard.writeText(code)
    ElMessage.success('代码已复制到剪贴板')
  } catch (error) {
    ElMessage.error('复制失败，请手动复制')
  }
}

// 下载控件
const downloadControl = () => {
  if (!lastResponse.value?.control) return
  
  const control = lastResponse.value.control
  const files = [
    { name: `${control.name}.vue`, content: control.componentCode },
    { name: 'types.ts', content: control.typeDefinitions },
    { name: 'styles.scss', content: control.styleCode },
    { name: 'Example.vue', content: control.exampleCode }
  ]
  
  files.forEach(file => {
    const blob = new Blob([file.content], { type: 'text/plain' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = file.name
    a.click()
    URL.revokeObjectURL(url)
  })
  
  ElMessage.success('控件文件已下载')
}

// 预览控件
const previewControl = () => {
  showPreview.value = true
}

// 关闭预览
const closePreview = () => {
  showPreview.value = false
}

// 分享控件
const shareControl = () => {
  showShare.value = true
}

// 预览相关方法
const refreshPreview = () => {
  // 刷新预览
  console.log('Preview refreshed')
}

const updatePreview = () => {
  // 更新预览设置
  console.log('Preview settings updated:', previewSettings.value)
}

const getCodeLines = () => {
  if (!lastResponse.value?.control) return 0
  const control = lastResponse.value.control
  const componentLines = control.componentCode?.split('\n').length || 0
  const styleLines = control.styleCode?.split('\n').length || 0
  const typeLines = control.typeDefinitions?.split('\n').length || 0
  return componentLines + styleLines + typeLines
}

const downloadFromPreview = () => {
  downloadControl()
}

const shareFromPreview = () => {
  closePreview()
  shareControl()
}

// 分享相关方法
const closeShare = () => {
  showShare.value = false
  shareResult.value = { type: null, content: '' }
}

const shareViaLink = async () => {
  if (!lastResponse.value?.control) return
  
  try {
    // 模拟生成分享链接
    const shareId = Math.random().toString(36).substring(2, 15)
    const shareUrl = `${window.location.origin}/shared/${shareId}`
    
    shareResult.value = {
      type: 'link',
      content: shareUrl
    }
    
    ElMessage.success('分享链接已生成')
  } catch (error) {
    ElMessage.error('生成分享链接失败')
  }
}

const shareViaCode = async () => {
  if (!lastResponse.value?.control) return
  
  try {
    const control = lastResponse.value.control
    const fullCode = `// ${control.name}.vue\n${control.componentCode}\n\n// styles.scss\n${control.styleCode}\n\n// types.ts\n${control.typeDefinitions}`
    
    await navigator.clipboard.writeText(fullCode)
    
    shareResult.value = {
      type: 'code',
      content: fullCode
    }
    
    ElMessage.success('代码已复制到剪贴板')
  } catch (error) {
    ElMessage.error('复制代码失败')
  }
}

const shareViaExport = () => {
  downloadControl()
  shareResult.value = {
    type: 'export',
    content: '文件已导出'
  }
}

const copyShareLink = async () => {
  try {
    await navigator.clipboard.writeText(shareResult.value.content)
    ElMessage.success('分享链接已复制到剪贴板')
  } catch (error) {
    ElMessage.error('复制链接失败')
  }
}

// 获取可行性颜色
const getFeasibilityColor = (score: number) => {
  if (score >= 80) return '#67c23a'
  if (score >= 60) return '#e6a23c'
  return '#f56c6c'
}

// 格式化时间
const formatTime = (timestamp: number) => {
  return new Date(timestamp).toLocaleString()
}

// 组件挂载
onMounted(() => {
  stats.value = aiControlService.getStats()
  conversationHistory.value = aiControlService.getConversationHistory()
})
</script>

<style lang="scss" scoped>
.ai-control-generator {
  padding: 20px;
  max-width: 1600px;
  margin: 0 auto;

  .generator-header {
    text-align: center;
    margin-bottom: 30px;

    h2 {
      margin: 0 0 8px 0;
      color: var(--text-primary);
      font-size: 28px;
      font-weight: 600;
    }

    .subtitle {
      margin: 0;
      color: var(--text-secondary);
      font-size: 16px;
    }
  }

  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .input-card {
    margin-bottom: 20px;

    .input-section {
      .char-count {
        text-align: right;
        font-size: 12px;
        color: var(--text-secondary);
        margin-top: 4px;
      }

      .action-buttons {
        text-align: center;
        margin-top: 20px;

        .el-button {
          margin: 0 8px;
        }
      }
    }
  }

  .history-card {
    .history-list {
      max-height: 200px;
      overflow-y: auto;

      .history-item {
        padding: 12px;
        border: 1px solid var(--border-color);
        border-radius: 6px;
        margin-bottom: 8px;
        cursor: pointer;
        transition: all 0.3s;

        &:hover {
          background: var(--hover-color);
          border-color: var(--primary-color);
        }

        .history-description {
          font-size: 14px;
          color: var(--text-primary);
          margin-bottom: 4px;
        }

        .history-meta {
          display: flex;
          justify-content: space-between;
          align-items: center;

          .history-time {
            font-size: 12px;
            color: var(--text-secondary);
          }
        }
      }
    }
  }

  .preview-card {
    height: 500px;
  }

  .result-card {
    min-height: 600px;

    .empty-result {
      text-align: center;
      padding: 60px 20px;
      color: var(--text-secondary);
    }

    .error-result {
      padding: 20px;
    }

    .success-result {
      .control-info {
        margin-bottom: 20px;

        h3 {
          margin: 0 0 8px 0;
          color: var(--text-primary);
          font-size: 20px;
        }

        .control-description {
          margin: 0 0 16px 0;
          color: var(--text-secondary);
          line-height: 1.6;
        }

        .feasibility {
          .feasibility-text {
            text-align: center;
            margin-top: 8px;
            font-size: 14px;
            color: var(--text-secondary);
          }
        }
      }

      .code-tabs {
        .code-container {
          .code-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 12px;
            background: var(--surface-color);
            border: 1px solid var(--border-color);
            border-bottom: none;
            border-radius: 4px 4px 0 0;
            font-size: 14px;
            color: var(--text-secondary);
          }

          pre {
            margin: 0;
            padding: 16px;
            background: #f8f9fa;
            border: 1px solid var(--border-color);
            border-radius: 0 0 4px 4px;
            overflow-x: auto;
            font-size: 13px;
            line-height: 1.5;
            max-height: 300px;

            code {
              color: #333;
              font-family: 'Fira Code', 'Monaco', 'Consolas', monospace;
            }
          }
        }

        .props-container {
          padding: 16px 0;
        }
      }

      .result-actions {
        text-align: center;
        margin-top: 20px;
        padding-top: 20px;
        border-top: 1px solid var(--border-color);

        .el-button {
          margin: 0 8px;
        }
      }
    }
  }

  .stats-card {
    .stat-item {
      text-align: center;
      padding: 16px;

      .stat-value {
        font-size: 24px;
        font-weight: bold;
        color: var(--primary-color);
        margin-bottom: 4px;
      }

      .stat-label {
        font-size: 12px;
        color: var(--text-secondary);
      }
    }
  }

  .preview-container {
    .preview-placeholder {
      text-align: center;
      padding: 60px 20px;
      color: var(--text-secondary);

      p {
        margin: 16px 0 8px 0;
        font-size: 16px;
      }
    }
  }
}

// 预览对话框样式
.preview-dialog {
  .preview-container {
    .preview-toolbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px;
      border-bottom: 1px solid var(--border-color);
      background: var(--surface-color);

      .toolbar-left {
        display: flex;
        gap: 8px;
        align-items: center;
      }

      .toolbar-right {
        display: flex;
        gap: 12px;
        align-items: center;
      }
    }

    .preview-content {
      padding: 20px;
      min-height: 500px;

      .interactive-preview {
        .preview-viewport {
          border: 1px solid var(--border-color);
          border-radius: 8px;
          padding: 16px;
          background: var(--background-color);
          min-height: 400px;
        }

        .preview-controls {
          h4 {
            margin: 0 0 16px 0;
            color: var(--text-primary);
          }

          .scale-text {
            margin-left: 8px;
            font-size: 12px;
            color: var(--text-secondary);
          }

          .preview-info {
            margin-top: 20px;
            padding: 16px;
            background: var(--surface-color);
            border-radius: 6px;

            h5 {
              margin: 0 0 12px 0;
              color: var(--text-primary);
              font-size: 14px;
            }

            .info-item {
              display: flex;
              justify-content: space-between;
              margin-bottom: 8px;
              font-size: 12px;

              .label {
                color: var(--text-secondary);
              }

              .value {
                color: var(--text-primary);
                font-weight: 500;
              }
            }
          }
        }
      }

      .static-preview {
        .preview-showcase {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
          gap: 20px;

          .showcase-item {
            text-align: center;

            h4 {
              margin: 0 0 12px 0;
              color: var(--text-primary);
            }

            .showcase-viewport {
              border: 1px solid var(--border-color);
              border-radius: 8px;
              padding: 16px;
              background: var(--background-color);
              min-height: 200px;

              &.dark-theme {
                background: #1a1a1a;
                color: #fff;
              }

              &.small-size {
                transform: scale(0.7);
                transform-origin: center;
              }
            }
          }
        }
      }

      .code-preview {
        .code-container {
          .code-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 12px;
            background: var(--surface-color);
            border: 1px solid var(--border-color);
            border-bottom: none;
            border-radius: 4px 4px 0 0;
            font-size: 14px;
            color: var(--text-secondary);
          }

          pre {
            margin: 0;
            padding: 16px;
            background: #f8f9fa;
            border: 1px solid var(--border-color);
            border-radius: 0 0 4px 4px;
            overflow-x: auto;
            font-size: 13px;
            line-height: 1.5;
            max-height: 400px;

            code {
              color: #333;
              font-family: 'Fira Code', 'Monaco', 'Consolas', monospace;
            }
          }
        }
      }
    }

    .preview-actions {
      display: flex;
      justify-content: center;
      gap: 12px;
      padding: 16px;
      border-top: 1px solid var(--border-color);
      background: var(--surface-color);
    }
  }

  .no-control-preview {
    text-align: center;
    padding: 60px 20px;
    color: var(--text-secondary);
  }
}

// 分享对话框样式
.share-container {
  .share-options {
    h4 {
      margin: 0 0 20px 0;
      color: var(--text-primary);
      text-align: center;
    }

    .share-option {
      cursor: pointer;
      transition: all 0.3s ease;
      border: 2px solid transparent;

      &:hover {
        border-color: var(--primary-color);
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      }

      h5 {
        margin: 12px 0 8px 0;
        color: var(--text-primary);
        font-size: 16px;
      }

      p {
        margin: 0;
        color: var(--text-secondary);
        font-size: 14px;
        line-height: 1.4;
      }
    }
  }

  .share-result {
    margin-top: 24px;
    padding: 20px;
    background: var(--surface-color);
    border-radius: 8px;

    h4 {
      margin: 0 0 16px 0;
      color: var(--text-primary);
    }

    .share-link-input {
      margin-bottom: 12px;
    }

    .share-info {
      p {
        margin: 8px 0;
        font-size: 14px;
        color: var(--text-secondary);
        display: flex;
        align-items: center;
        gap: 8px;
      }
    }
  }
}

// 深色主题适配
@media (prefers-color-scheme: dark) {
  .ai-control-generator {
    .code-container pre {
      background: #1e1e1e;
      border-color: #333;

      code {
        color: #d4d4d4;
      }
    }
  }

  .preview-dialog {
    .code-preview .code-container pre {
      background: #1e1e1e;
      border-color: #333;

      code {
        color: #d4d4d4;
      }
    }
  }
}
</style>
