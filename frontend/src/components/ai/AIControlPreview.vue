<template>
  <div class="ai-control-preview">
    <div class="preview-header">
      <h3>🔍 实时预览</h3>
      <div class="preview-controls">
        <el-button-group>
          <el-button 
            :type="viewMode === 'preview' ? 'primary' : 'default'"
            @click="viewMode = 'preview'"
            size="small"
          >
            <el-icon><View /></el-icon>
            预览
          </el-button>
          <el-button 
            :type="viewMode === 'code' ? 'primary' : 'default'"
            @click="viewMode = 'code'"
            size="small"
          >
            <el-icon><Document /></el-icon>
            代码
          </el-button>
        </el-button-group>
        <el-button @click="refreshPreview" size="small">
          <el-icon><Refresh /></el-icon>
          刷新
        </el-button>
      </div>
    </div>

    <div class="preview-content">
      <!-- 预览模式 -->
      <div v-if="viewMode === 'preview'" class="preview-panel">
        <div v-if="!control" class="empty-preview">
          <el-icon size="48" color="#ddd"><DocumentAdd /></el-icon>
          <p>暂无控件预览</p>
          <p class="hint">生成控件后将在此处显示实时预览</p>
        </div>
        
        <div v-else class="control-preview">
          <!-- 控件交互面板 -->
          <div class="interaction-panel">
            <h4>🎛️ 交互控制</h4>
            <div class="control-params">
              <div class="param-group">
                <label>数值:</label>
                <el-slider 
                  v-model="previewParams.value" 
                  :min="previewParams.min" 
                  :max="previewParams.max"
                  :step="0.1"
                  show-input
                  @change="updatePreview"
                />
              </div>
              
              <div class="param-group">
                <label>标题:</label>
                <el-input 
                  v-model="previewParams.title" 
                  placeholder="控件标题"
                  @input="updatePreview"
                />
              </div>
              
              <div class="param-group">
                <label>单位:</label>
                <el-select v-model="previewParams.unit" @change="updatePreview">
                  <el-option label="V" value="V" />
                  <el-option label="A" value="A" />
                  <el-option label="°C" value="°C" />
                  <el-option label="°F" value="°F" />
                  <el-option label="Hz" value="Hz" />
                  <el-option label="%" value="%" />
                </el-select>
              </div>
              
              <div class="param-group">
                <label>主题:</label>
                <el-select v-model="previewParams.theme" @change="updatePreview">
                  <el-option label="浅色" value="light" />
                  <el-option label="深色" value="dark" />
                  <el-option label="专业" value="professional" />
                </el-select>
              </div>
              
              <div class="param-group">
                <el-checkbox v-model="previewParams.interactive" @change="updatePreview">
                  交互模式
                </el-checkbox>
                <el-checkbox v-model="previewParams.showHeader" @change="updatePreview">
                  显示标题
                </el-checkbox>
                <el-checkbox v-model="previewParams.animation" @change="updatePreview">
                  动画效果
                </el-checkbox>
              </div>
            </div>
          </div>

          <!-- 实际预览区域 -->
          <div class="preview-viewport" :class="previewParams.theme">
            <div class="viewport-content">
              <!-- 动态渲染生成的控件 -->
              <component 
                v-if="renderedComponent" 
                :is="renderedComponent"
                v-bind="previewParams"
                @action="handleControlAction"
                @change="handleControlChange"
              />
              
              <!-- 模拟预览（当无法动态渲染时） -->
              <div v-else class="mock-preview">
                <div class="mock-control" :class="getMockControlClass()">
                  <div v-if="previewParams.showHeader" class="mock-header">
                    <h4>{{ previewParams.title || '生成的控件' }}</h4>
                  </div>
                  
                  <div class="mock-content">
                    <!-- 根据控件类型显示不同的模拟预览 -->
                    <div v-if="isGaugeType" class="mock-gauge">
                      <svg width="320" height="320" viewBox="0 0 200 200">
                        <!-- 外圆环 -->
                        <circle cx="100" cy="100" r="80" fill="none" stroke="#E5E7EB" stroke-width="4"/>
                        <!-- 进度弧 -->
                        <path 
                          :d="getGaugeArc()" 
                          fill="none" 
                          stroke="#2E86AB" 
                          stroke-width="6" 
                          stroke-linecap="round"
                        />
                        <!-- 刻度线 -->
                        <g stroke="#6B7280" stroke-width="2">
                          <line v-for="i in 9" :key="i" 
                            :x1="getTickX1(i)" :y1="getTickY1(i)"
                            :x2="getTickX2(i)" :y2="getTickY2(i)" 
                          />
                        </g>
                        <!-- 指针 -->
                        <line 
                          x1="100" y1="100" 
                          :x2="getPointerX()" :y2="getPointerY()" 
                          stroke="#1F2937" stroke-width="3" stroke-linecap="round"
                        />
                        <circle cx="100" cy="100" r="6" fill="#2E86AB"/>
                        <!-- 数值显示 -->
                        <text x="100" y="140" text-anchor="middle" 
                              font-family="Consolas" font-size="20" font-weight="bold" 
                              fill="#2E86AB">
                          {{ previewParams.value.toFixed(1) }}
                        </text>
                        <text x="100" y="155" text-anchor="middle" 
                              font-family="Segoe UI" font-size="14" fill="#6B7280">
                          {{ previewParams.unit }}
                        </text>
                      </svg>
                    </div>
                    
                    <div v-else-if="isDisplayType" class="mock-display">
                      <div class="digital-display">
                        <div class="digital-value">{{ previewParams.value.toFixed(3) }}</div>
                        <div class="digital-unit">{{ previewParams.unit }}</div>
                      </div>
                      <div class="status-indicators">
                        <div class="status-dot" :class="getStatusClass()"></div>
                        <span>{{ getStatusText() }}</span>
                      </div>
                    </div>
                    
                    <div v-else-if="isChartType" class="mock-chart">
                      <svg width="400" height="200" viewBox="0 0 300 150">
                        <!-- 网格 -->
                        <g stroke="#E5E7EB" stroke-width="0.5" opacity="0.5">
                          <line v-for="i in 6" :key="`h${i}`" 
                                :x1="0" :y1="i * 25" :x2="300" :y2="i * 25" />
                          <line v-for="i in 12" :key="`v${i}`" 
                                :x1="i * 25" :y1="0" :x2="i * 25" :y2="150" />
                        </g>
                        <!-- 波形 -->
                        <path :d="getWaveformPath()" fill="none" stroke="#2E86AB" stroke-width="2"/>
                      </svg>
                    </div>
                    
                    <div v-else class="mock-generic">
                      <div class="generic-content">
                        <div class="content-header">
                          <el-icon size="32" color="#2E86AB"><Setting /></el-icon>
                        </div>
                        <div class="content-body">
                          <div class="value-display">{{ previewParams.value }}</div>
                          <div class="unit-display">{{ previewParams.unit }}</div>
                        </div>
                        <div class="content-footer">
                          <el-button v-if="previewParams.interactive" size="small" type="primary">
                            操作
                          </el-button>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- 预览信息 -->
            <div class="preview-info">
              <div class="info-item">
                <span class="label">尺寸:</span>
                <span class="value">{{ previewParams.width || 'auto' }} × {{ previewParams.height || 'auto' }}</span>
              </div>
              <div class="info-item">
                <span class="label">类型:</span>
                <span class="value">{{ control?.type || 'custom' }}</span>
              </div>
              <div class="info-item">
                <span class="label">主题:</span>
                <span class="value">{{ previewParams.theme }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 代码模式 -->
      <div v-else-if="viewMode === 'code'" class="code-panel">
        <el-tabs v-model="activeCodeTab">
          <el-tab-pane label="Vue组件" name="component">
            <div class="code-container">
              <pre><code class="language-vue">{{ control?.componentCode || '// 暂无代码' }}</code></pre>
            </div>
          </el-tab-pane>
          <el-tab-pane label="样式" name="styles">
            <div class="code-container">
              <pre><code class="language-scss">{{ control?.styleCode || '// 暂无样式' }}</code></pre>
            </div>
          </el-tab-pane>
        </el-tabs>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { View, Document, Refresh, DocumentAdd, Setting } from '@element-plus/icons-vue'

// Props
interface Props {
  control?: any
}

const props = defineProps<Props>()

// 响应式数据
const viewMode = ref<'preview' | 'code'>('preview')
const activeCodeTab = ref('component')
const renderedComponent = ref<any>(null)

// 预览参数
const previewParams = ref({
  value: 75.5,
  min: 0,
  max: 100,
  title: '温度控件',
  unit: '°C',
  theme: 'light',
  interactive: true,
  showHeader: true,
  animation: true,
  width: 300,
  height: 200
})

// 计算属性
const isGaugeType = computed(() => {
  return props.control?.type === 'gauge' || 
         props.control?.description?.includes('仪表') ||
         props.control?.description?.includes('gauge')
})

const isDisplayType = computed(() => {
  return props.control?.type === 'indicator' || 
         props.control?.description?.includes('显示') ||
         props.control?.description?.includes('数字')
})

const isChartType = computed(() => {
  return props.control?.type === 'chart' || 
         props.control?.description?.includes('图表') ||
         props.control?.description?.includes('波形')
})

// 方法
const updatePreview = () => {
  // 触发预览更新
  console.log('Preview updated:', previewParams.value)
}

const refreshPreview = () => {
  // 刷新预览
  renderedComponent.value = null
  setTimeout(() => {
    // 尝试重新渲染
  }, 100)
}

const handleControlAction = (value: any) => {
  console.log('Control action:', value)
}

const handleControlChange = (value: any) => {
  previewParams.value.value = value
}

const getMockControlClass = () => {
  return [
    'mock-control-base',
    `theme-${previewParams.value.theme}`,
    { 'interactive': previewParams.value.interactive },
    { 'animated': previewParams.value.animation }
  ]
}

const getGaugeArc = () => {
  const percentage = previewParams.value.value / previewParams.value.max
  const angle = percentage * 180 // 半圆
  const radians = (angle - 90) * Math.PI / 180
  const x = 100 + 80 * Math.cos(radians)
  const y = 100 + 80 * Math.sin(radians)
  const largeArc = angle > 180 ? 1 : 0
  return `M 20 100 A 80 80 0 ${largeArc} 1 ${x} ${y}`
}

const getPointerX = () => {
  const percentage = previewParams.value.value / previewParams.value.max
  const angle = (percentage * 180 - 90) * Math.PI / 180
  return 100 + 60 * Math.cos(angle)
}

const getPointerY = () => {
  const percentage = previewParams.value.value / previewParams.value.max
  const angle = (percentage * 180 - 90) * Math.PI / 180
  return 100 + 60 * Math.sin(angle)
}

const getTickX1 = (i: number) => {
  const angle = (i * 22.5 - 90) * Math.PI / 180
  return 100 + 75 * Math.cos(angle)
}

const getTickY1 = (i: number) => {
  const angle = (i * 22.5 - 90) * Math.PI / 180
  return 100 + 75 * Math.sin(angle)
}

const getTickX2 = (i: number) => {
  const angle = (i * 22.5 - 90) * Math.PI / 180
  return 100 + 85 * Math.cos(angle)
}

const getTickY2 = (i: number) => {
  const angle = (i * 22.5 - 90) * Math.PI / 180
  return 100 + 85 * Math.sin(angle)
}

const getStatusClass = () => {
  const value = previewParams.value.value
  if (value > 80) return 'status-warning'
  if (value < 20) return 'status-info'
  return 'status-success'
}

const getStatusText = () => {
  const value = previewParams.value.value
  if (value > 80) return '高温警告'
  if (value < 20) return '低温提示'
  return '正常运行'
}

const getWaveformPath = () => {
  const points = []
  const time = Date.now() / 1000
  
  for (let x = 0; x <= 300; x += 5) {
    const t = (x / 300) * 4 * Math.PI + time
    const y = 75 + Math.sin(t) * 30 + Math.sin(t * 3) * 15
    points.push(`${x},${y}`)
  }
  
  return `M ${points.join(' L ')}`
}

// 监听控件变化
watch(() => props.control, (newControl) => {
  if (newControl) {
    // 根据控件类型调整预览参数
    if (newControl.description?.includes('温度')) {
      previewParams.value.unit = '°C'
      previewParams.value.title = '温度传感器'
    } else if (newControl.description?.includes('示波器')) {
      previewParams.value.unit = 'V'
      previewParams.value.title = '示波器'
    } else if (newControl.description?.includes('电压')) {
      previewParams.value.unit = 'V'
      previewParams.value.title = '电压表'
    } else if (newControl.description?.includes('频率')) {
      previewParams.value.unit = 'Hz'
      previewParams.value.title = '频率计'
    } else if (newControl.description?.includes('压力')) {
      previewParams.value.unit = 'bar'
      previewParams.value.title = '压力表'
    } else if (newControl.description?.includes('数字显示') || newControl.description?.includes('数码管')) {
      previewParams.value.unit = ''
      previewParams.value.title = '数字显示器'
    } else if (newControl.description?.includes('LED')) {
      previewParams.value.unit = ''
      previewParams.value.title = 'LED指示灯'
    } else if (newControl.description?.includes('开关')) {
      previewParams.value.unit = ''
      previewParams.value.title = '开关控件'
    } else if (newControl.description?.includes('图表') || newControl.description?.includes('波形')) {
      previewParams.value.unit = ''
      previewParams.value.title = '实时图表'
    } else if (newControl.description?.includes('万用表')) {
      previewParams.value.unit = 'V'
      previewParams.value.title = '数字万用表'
    } else if (newControl.description?.includes('信号发生器')) {
      previewParams.value.unit = 'Hz'
      previewParams.value.title = '信号发生器'
    } else if (newControl.description?.includes('电源')) {
      previewParams.value.unit = 'V'
      previewParams.value.title = '电源控制器'
    }
  }
}, { immediate: true })

// 组件挂载
onMounted(() => {
  // 初始化预览
  updatePreview()
})
</script>

<style lang="scss" scoped>
.ai-control-preview {
  height: 100%;
  display: flex;
  flex-direction: column;

  .preview-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 16px;
    border-bottom: 1px solid var(--border-color);
    background: var(--surface-color);

    h3 {
      margin: 0;
      font-size: 16px;
      color: var(--text-primary);
    }

    .preview-controls {
      display: flex;
      gap: 12px;
      align-items: center;
    }
  }

  .preview-content {
    flex: 1;
    overflow: hidden;

    .preview-panel {
      height: 100%;
      display: flex;
      flex-direction: column;

      .empty-preview {
        flex: 1;
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: center;
        color: var(--text-secondary);

        p {
          margin: 8px 0;
        }

        .hint {
          font-size: 12px;
          opacity: 0.7;
        }
      }

        .control-preview {
          height: 100%;
          display: flex;
          flex-direction: column;

          .interaction-panel {
            padding: 8px 12px;
            border-bottom: 1px solid var(--border-color);
            background: var(--background-color);

            h4 {
              margin: 0 0 6px 0;
              font-size: 12px;
              color: var(--text-primary);
            }

            .control-params {
              display: grid;
              grid-template-columns: 1fr 1fr 1fr 1fr;
              gap: 6px;

              .param-group {
                display: flex;
                flex-direction: column;
                gap: 1px;

                label {
                  font-size: 10px;
                  color: var(--text-secondary);
                  font-weight: 500;
                }

                .el-slider {
                  margin: 2px 0;
                }

                .el-input, .el-select {
                  font-size: 11px;
                }

                &:last-child {
                  grid-column: 1 / -1;
                  flex-direction: row;
                  gap: 8px;
                  align-items: center;
                  margin-top: 2px;

                  .el-checkbox {
                    font-size: 10px;
                  }
                }
              }
            }
          }

          .preview-viewport {
            flex: 1;
            padding: 4px;
            overflow: auto;
            min-height: 540px;

            &.dark {
              background: #1a1a1a;
              color: #fff;
            }

            &.professional {
              background: #f5f7fa;
              color: #2c3e50;
            }

            .viewport-content {
              display: flex;
              justify-content: center;
              align-items: flex-start;
              min-height: 480px;
              width: 100%;
              padding: 8px;

            .mock-preview {
              .mock-control {
                background: var(--surface-color);
                border: 1px solid var(--border-color);
                border-radius: 8px;
                padding: 20px;
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
                transition: all 0.3s ease;

                &.interactive:hover {
                  transform: translateY(-2px);
                  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
                }

                &.animated {
                  .mock-gauge svg,
                  .digital-value,
                  .status-dot {
                    transition: all 0.3s ease;
                  }
                }

                .mock-header {
                  text-align: center;
                  margin-bottom: 16px;

                  h4 {
                    margin: 0;
                    color: var(--text-primary);
                    font-size: 16px;
                  }
                }

                .mock-content {
                  .mock-gauge {
                    text-align: center;
                  }

                  .mock-display {
                    text-align: center;

                    .digital-display {
                      background: #000;
                      color: #00ff00;
                      padding: 16px;
                      border-radius: 4px;
                      margin-bottom: 12px;
                      font-family: 'Courier New', monospace;

                      .digital-value {
                        font-size: 32px;
                        font-weight: bold;
                        line-height: 1;
                      }

                      .digital-unit {
                        font-size: 16px;
                        margin-top: 4px;
                        opacity: 0.8;
                      }
                    }

                    .status-indicators {
                      display: flex;
                      justify-content: center;
                      align-items: center;
                      gap: 8px;
                      font-size: 14px;

                      .status-dot {
                        width: 12px;
                        height: 12px;
                        border-radius: 50%;

                        &.status-success {
                          background: #67c23a;
                        }

                        &.status-warning {
                          background: #e6a23c;
                        }

                        &.status-info {
                          background: #409eff;
                        }
                      }
                    }
                  }

                  .mock-chart {
                    text-align: center;
                  }

                  .mock-generic {
                    .generic-content {
                      text-align: center;

                      .content-header {
                        margin-bottom: 16px;
                      }

                      .content-body {
                        margin-bottom: 16px;

                        .value-display {
                          font-size: 24px;
                          font-weight: bold;
                          color: var(--primary-color);
                          margin-bottom: 4px;
                        }

                        .unit-display {
                          font-size: 14px;
                          color: var(--text-secondary);
                        }
                      }
                    }
                  }
                }
              }
            }
          }

          .preview-info {
            margin-top: 8px;
            padding: 6px 8px;
            background: var(--background-color);
            border-radius: 4px;
            display: flex;
            gap: 12px;
            font-size: 10px;

            .info-item {
              .label {
                color: var(--text-secondary);
                margin-right: 2px;
              }

              .value {
                color: var(--text-primary);
                font-weight: 500;
              }
            }
          }
        }
      }
    }

    .code-panel {
      height: 100%;
      padding: 16px;

      .code-container {
        height: 400px;
        overflow: auto;

        pre {
          margin: 0;
          padding: 16px;
          background: #f8f9fa;
          border-radius: 6px;
          font-size: 13px;
          line-height: 1.5;

          code {
            color: #333;
            font-family: 'Fira Code', 'Monaco', 'Consolas', monospace;
          }
        }
      }
    }
  }
}

// 深色主题适配
@media (prefers-color-scheme: dark) {
  .ai-control-preview {
    .code-container pre {
      background: #1e1e1e;

      code {
        color: #d4d4d4;
      }
    }
  }
}
</style>
