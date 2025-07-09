<template>
  <div class="download-view">
    <div class="download-header">
      <div class="header-content">
        <h1 class="download-title">
          <el-icon><Download /></el-icon>
          SeeSharpTools Web 控件下载
        </h1>
        <p class="download-subtitle">下载专业测试测量仪器控件和使用文档</p>
      </div>
    </div>

    <div class="download-content">
      <section class="download-section">
        <h2 class="section-title">控件下载</h2>
        
        <el-tabs v-model="activeTab" type="border-card">
          <el-tab-pane label="基础控件" name="basic">
            <div class="components-grid">
              <div 
                v-for="component in basicComponents" 
                :key="component.name"
                class="component-card"
              >
                <div class="component-header">
                  <h3>{{ component.name }}</h3>
                  <el-tag type="success">已开发</el-tag>
                </div>
                
                <div class="component-content">
                  <p>{{ component.description }}</p>
                  
                  <div class="component-actions">
                    <el-button type="primary" @click="downloadComponent(component)">
                      <el-icon><Download /></el-icon>
                      下载控件
                    </el-button>
                    <el-button @click="downloadDoc(component)">
                      <el-icon><Document /></el-icon>
                      下载文档
                    </el-button>
                    <el-button @click="viewDemo(component)" link>
                      查看演示
                    </el-button>
                  </div>
                </div>
              </div>
            </div>
          </el-tab-pane>

          <el-tab-pane label="高性能图表" name="charts">
            <div class="components-grid">
              <div 
                v-for="component in chartComponents" 
                :key="component.name"
                class="component-card"
              >
                <div class="component-header">
                  <h3>{{ component.name }}</h3>
                  <el-tag type="success">已开发</el-tag>
                </div>
                
                <div class="component-content">
                  <p>{{ component.description }}</p>
                  
                  <div class="component-actions">
                    <el-button type="primary" @click="downloadComponent(component)">
                      <el-icon><Download /></el-icon>
                      下载控件
                    </el-button>
                    <el-button @click="downloadDoc(component)">
                      <el-icon><Document /></el-icon>
                      下载文档
                    </el-button>
                    <el-button @click="viewDemo(component)" link>
                      查看演示
                    </el-button>
                  </div>
                </div>
              </div>
            </div>
          </el-tab-pane>

          <el-tab-pane label="模块仪器" name="instruments">
            <div class="components-grid">
              <div 
                v-for="component in instrumentComponents" 
                :key="component.name"
                class="component-card"
              >
                <div class="component-header">
                  <h3>{{ component.name }}</h3>
                  <el-tag type="success">已开发</el-tag>
                </div>
                
                <div class="component-content">
                  <p>{{ component.description }}</p>
                  
                  <div class="component-actions">
                    <el-button type="primary" @click="downloadComponent(component)">
                      <el-icon><Download /></el-icon>
                      下载控件
                    </el-button>
                    <el-button @click="downloadDoc(component)">
                      <el-icon><Document /></el-icon>
                      下载文档
                    </el-button>
                    <el-button @click="viewDemo(component)" link>
                      查看演示
                    </el-button>
                  </div>
                </div>
              </div>
            </div>
          </el-tab-pane>

          <el-tab-pane label="AI智能控件" name="ai">
            <div class="components-grid">
              <div 
                v-for="component in aiComponents" 
                :key="component.name"
                class="component-card"
              >
                <div class="component-header">
                  <h3>{{ component.name }}</h3>
                  <el-tag type="success">已开发</el-tag>
                </div>
                
                <div class="component-content">
                  <p>{{ component.description }}</p>
                  
                  <div class="component-actions">
                    <el-button type="primary" @click="downloadComponent(component)">
                      <el-icon><Download /></el-icon>
                      下载控件
                    </el-button>
                    <el-button @click="downloadDoc(component)">
                      <el-icon><Document /></el-icon>
                      下载文档
                    </el-button>
                    <el-button @click="viewDemo(component)" link>
                      查看演示
                    </el-button>
                  </div>
                </div>
              </div>
            </div>
          </el-tab-pane>
        </el-tabs>
      </section>

      <section class="download-section">
        <h2 class="section-title">批量下载</h2>
        
        <div class="batch-download-grid">
          <el-card>
            <template #header>
              <h3>🎯 自定义下载包</h3>
            </template>
            
            <div class="batch-content">
              <p>选择需要的控件，生成自定义下载包</p>
              
              <div class="component-selector">
                <el-checkbox-group v-model="selectedComponents">
                  <div class="selector-category">
                    <h4>基础控件</h4>
                    <el-checkbox 
                      v-for="component in basicComponents" 
                      :key="component.name"
                      :label="component.name"
                      :value="component.name"
                    >
                      {{ component.name }}
                    </el-checkbox>
                  </div>
                </el-checkbox-group>
              </div>
              
              <div class="batch-actions">
                <el-button 
                  type="primary" 
                  size="large"
                  @click="downloadCustomPackage"
                  :disabled="selectedComponents.length === 0"
                >
                  <el-icon><Download /></el-icon>
                  下载自定义包 ({{ selectedComponents.length }}个控件)
                </el-button>
              </div>
            </div>
          </el-card>

          <el-card>
            <template #header>
              <h3>🚀 完整项目下载</h3>
            </template>
            
            <div class="project-download">
              <div class="project-options">
                <div class="option-item">
                  <h4>前端控件库</h4>
                  <p>Vue 3 + TypeScript + 所有控件源码</p>
                  <el-button type="primary" @click="downloadProject">
                    <el-icon><Download /></el-icon>
                    下载前端项目
                  </el-button>
                </div>
                
                <div class="option-item">
                  <h4>完整解决方案</h4>
                  <p>前端 + 后端 + 文档 + 示例</p>
                  <el-button type="primary" @click="downloadProject">
                    <el-icon><Download /></el-icon>
                    下载完整项目
                  </el-button>
                </div>
              </div>
            </div>
          </el-card>
        </div>
      </section>

      <section class="download-section">
        <h2 class="section-title">使用文档</h2>
        
        <div class="docs-grid">
          <el-card>
            <template #header>
              <h3>📖 快速开始指南</h3>
            </template>
            <div class="doc-content">
              <p>包含安装、配置、基础使用的完整指南</p>
              <el-button type="primary" @click="downloadGuide">
                <el-icon><Download /></el-icon>
                下载指南
              </el-button>
            </div>
          </el-card>

          <el-card>
            <template #header>
              <h3>📚 API参考文档</h3>
            </template>
            <div class="doc-content">
              <p>详细的API接口文档和参数说明</p>
              <el-button type="primary" @click="downloadApiDoc">
                <el-icon><Download /></el-icon>
                下载文档
              </el-button>
            </div>
          </el-card>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { Download, Document } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

const router = useRouter()
const activeTab = ref('basic')
const selectedComponents = ref<string[]>([])

const basicComponents = reactive([
  {
    name: '数字显示器',
    description: '高精度数字显示控件，支持多种数值格式和单位显示',
    demoRoute: '/digital-display-test'
  },
  {
    name: '线性仪表',
    description: '线性刻度仪表，适用于压力、温度等物理量显示',
    demoRoute: '/linear-gauge-test'
  },
  {
    name: '温度计',
    description: '温度显示控件，支持摄氏度和华氏度转换',
    demoRoute: '/thermometer-test'
  },
  {
    name: '圆形仪表',
    description: '精密圆形仪表，支持多种刻度和指针样式',
    demoRoute: '/instruments'
  },
  {
    name: '开关控件',
    description: '多样式开关控件，支持多种交互模式',
    demoRoute: '/switch-test'
  },
  {
    name: '按钮阵列',
    description: '可配置的按钮阵列控件，支持多种布局',
    demoRoute: '/button-array-test'
  },
  {
    name: 'LED指示器',
    description: '多色LED指示器，支持闪烁和渐变效果',
    demoRoute: '/indicators'
  }
])

const chartComponents = reactive([
  {
    name: '高性能StripChart',
    description: '实时数据条带图，支持大数据量高频更新',
    demoRoute: '/enhanced-stripchart-test'
  },
  {
    name: 'FFT频谱分析',
    description: '快速傅里叶变换频谱分析图表',
    demoRoute: '/spectrum-chart-test'
  },
  {
    name: '高级数学分析图表',
    description: '支持复杂数学运算和分析的图表控件',
    demoRoute: '/advanced-easy-chart-test'
  },
  {
    name: '专业测量图表',
    description: '专业测量工具图表，支持多种测量功能',
    demoRoute: '/professional-easy-chart-test'
  },
  {
    name: '双Y轴图表',
    description: '支持双Y轴显示的高性能图表控件',
    demoRoute: '/dual-axis-easychart-test'
  },
  {
    name: '波形图表',
    description: '专业波形显示图表，支持多通道波形',
    demoRoute: '/waveform'
  },
  {
    name: '条带图',
    description: '实时数据条带显示图表',
    demoRoute: '/stripchart'
  }
])

const instrumentComponents = reactive([
  {
    name: '数字示波器',
    description: '专业数字示波器控件，支持多通道波形显示和触发',
    demoRoute: '/oscilloscope-test'
  },
  {
    name: '信号发生器',
    description: '多功能信号发生器，支持多种波形类型和调制',
    demoRoute: '/signal-generator-test'
  },
  {
    name: '数字万用表',
    description: '高精度数字万用表，支持多种测量功能',
    demoRoute: '/digital-multimeter-test'
  },
  {
    name: '温度采集卡',
    description: '多通道温度采集卡控件，支持多种传感器类型',
    demoRoute: '/temperature-acquisition-card-test'
  },
  {
    name: 'DIO控制卡',
    description: '数字输入输出控制卡，支持多通道数字IO控制',
    demoRoute: '/dio-card-test'
  },
  {
    name: '数据采集卡',
    description: '高速数据采集卡控件，支持多通道同步采集',
    demoRoute: '/data-acquisition'
  },
  {
    name: '频谱分析仪',
    description: '专业频谱分析仪界面，模拟真实仪器操作',
    demoRoute: '/spectrum'
  }
])

const aiComponents = reactive([
  {
    name: 'AI控件生成器',
    description: '基于自然语言的AI控件生成系统，自动生成Vue组件代码',
    demoRoute: '/ai-control-generator-test'
  },
  {
    name: 'AI控件预览器',
    description: 'AI生成控件的实时预览和编辑器',
    demoRoute: '/ai-control-generator-test'
  }
])

function downloadComponent(component: any) {
  const code = '<template>\n  <div class="component-container">\n    <!-- ' + component.description + ' -->\n    <div class="component-content">\n      <!-- 控件实现 -->\n    </div>\n  </div>\n</template>\n\n<script setup lang="ts">\nimport { ref, reactive } from \'vue\'\n\n// 控件属性\nconst props = defineProps<{\n  // 定义属性\n}>()\n\n// 控件事件\nconst emit = defineEmits<{\n  // 定义事件\n}>()\n\n// 控件状态\nconst state = reactive({\n  // 状态数据\n})\n</script>\n\n<style lang="scss" scoped>\n.component-container {\n  /* 控件样式 */\n}\n</style>'

  const blob = new Blob([code], { type: 'text/plain;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = component.name + '.vue'
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
  
  ElMessage.success(component.name + ' 控件下载完成')
}

function downloadDoc(item: any) {
  const doc = '# ' + item.name + '\n\n' +
    '## 概述\n' + (item.description || '详细的使用说明和API文档') + '\n\n' +
    '## 安装\n```bash\nnpm install seesharptools-vue\n```\n\n' +
    '## 基础使用\n```vue\n<template>\n  <ComponentName />\n</template>\n\n' +
    '<script setup lang="ts">\nimport { ComponentName } from "seesharptools-vue"\n' +
    '</script>\n```\n\n' +
    '## API参考\n详细的API文档和使用示例\n\n' +
    '## 许可证\nMIT License'

  const blob = new Blob([doc], { type: 'text/markdown;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = item.name + '-文档.md'
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
  
  ElMessage.success(item.name + ' 文档下载完成')
}

function viewDemo(component: any) {
  router.push(component.demoRoute)
}

function downloadCustomPackage() {
  if (selectedComponents.value.length === 0) {
    ElMessage.warning('请选择至少一个控件')
    return
  }
  
  const content = '# SeeSharpTools Web 自定义控件包\n\n' +
    '## 包含的控件\n' +
    selectedComponents.value.map(name => '- ' + name).join('\n') + '\n\n' +
    '## 使用说明\n' +
    '1. 下载控件文件\n' +
    '2. 复制到项目的 components 目录\n' +
    '3. 在需要的地方导入使用\n\n' +
    '生成时间: ' + new Date().toLocaleString()

  const blob = new Blob([content], { type: 'text/plain;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = 'SeeSharpTools-自定义包-' + selectedComponents.value.length + '个控件.txt'
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
  
  ElMessage.success('自定义包下载完成 (' + selectedComponents.value.length + '个控件)')
}

function downloadProject() {
  const url = 'https://github.com/wukeping2008/seesharptools-web/archive/refs/heads/main.zip'
  window.open(url, '_blank')
  ElMessage.success('项目下载已开始')
}

function downloadGuide() {
  const guide = '# SeeSharpTools Web 快速开始指南\n\n' +
    '## 安装\n```bash\nnpm install seesharptools-vue\n```\n\n' +
    '## 基础使用\n详细的使用说明...\n\n' +
    '## 配置\n配置说明...\n\n' +
    '## 示例\n示例代码...'

  const blob = new Blob([guide], { type: 'text/markdown;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = '快速开始指南.md'
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
  
  ElMessage.success('快速开始指南下载完成')
}

function downloadApiDoc() {
  const apiDoc = '# SeeSharpTools Web API参考文档\n\n' +
    '## API概述\n详细的API接口文档...\n\n' +
    '## 组件API\n各组件的API说明...\n\n' +
    '## 事件API\n事件处理说明...'

  const blob = new Blob([apiDoc], { type: 'text/markdown;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = 'API参考文档.md'
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
  
  ElMessage.success('API参考文档下载完成')
}
</script>

<style lang="scss" scoped>
.download-view {
  min-height: 100vh;
  background: var(--background-color);
}

.download-header {
  background: linear-gradient(135deg, #2E86AB 0%, #A23B72 100%);
  color: white;
  padding: 60px 0;
  
  .header-content {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 24px;
    text-align: center;
    
    .download-title {
      font-size: 48px;
      font-weight: 700;
      margin-bottom: 16px;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 16px;
    }
    
    .download-subtitle {
      font-size: 20px;
      opacity: 0.9;
    }
  }
}

.download-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
  
  .download-section {
    margin: 80px 0;
    
    .section-title {
      font-size: 32px;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 40px;
    }
  }
}

.components-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: 24px;
  
  .component-card {
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 12px;
    padding: 24px;
    transition: all 0.3s ease;
    
    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
    }
    
    .component-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
      
      h3 {
        margin: 0;
        font-size: 18px;
        font-weight: 600;
      }
    }
    
    .component-content {
      p {
        color: var(--text-secondary);
        margin-bottom: 20px;
        line-height: 1.5;
      }
      
      .component-actions {
        display: flex;
        gap: 8px;
        flex-wrap: wrap;
        
        .el-button {
          flex: 1;
          min-width: 100px;
        }
      }
    }
  }
}

.batch-download-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 32px;
  
  .batch-content {
    .component-selector {
      margin-bottom: 24px;
      
      .selector-category {
        margin-bottom: 16px;
        
        h4 {
          font-size: 16px;
          font-weight: 600;
          margin-bottom: 8px;
          color: var(--text-primary);
        }
        
        .el-checkbox {
          display: block;
          margin-bottom: 8px;
        }
      }
    }
    
    .batch-actions {
      text-align: center;
    }
  }
  
  .project-download {
    .project-options {
      display: grid;
      gap: 20px;
      
      .option-item {
        padding: 20px;
        border: 1px solid var(--border-color);
        border-radius: 8px;
        text-align: center;
        
        h4 {
          margin: 0 0 8px 0;
          font-size: 16px;
          font-weight: 600;
        }
        
        p {
          margin: 0 0 16px 0;
          color: var(--text-secondary);
          font-size: 14px;
        }
      }
    }
  }
}

.docs-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 24px;
  
  .doc-content {
    p {
      color: var(--text-secondary);
      margin-bottom: 16px;
    }
  }
}

@media (max-width: 768px) {
  .download-header {
    padding: 40px 0;
    
    .header-content {
      .download-title {
        font-size: 32px;
      }
      
      .download-subtitle {
        font-size: 16px;
      }
    }
  }
  
  .download-content {
    padding: 0 16px;
    
    .download-section {
      margin: 40px 0;
      
      .section-title {
        font-size: 24px;
      }
    }
  }
  
  .components-grid,
  .batch-download-grid,
  .docs-grid {
    grid-template-columns: 1fr;
  }
}
</style>
