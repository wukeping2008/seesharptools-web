<template>
  <div class="documentation-view">
    <!-- 文档头部 -->
    <div class="doc-header">
      <div class="header-content">
        <h1 class="doc-title">
          <el-icon><Document /></el-icon>
          SeeSharpTools Web 控件文档
        </h1>
        <p class="doc-subtitle">专业测试测量仪器控件使用指南</p>
        
        <!-- 快速导航 -->
        <div class="quick-nav">
          <el-button-group>
            <el-button @click="scrollToSection('getting-started')">
              <el-icon><VideoPlay /></el-icon>
              快速开始
            </el-button>
            <el-button @click="scrollToSection('components')">
              <el-icon><Grid /></el-icon>
              控件列表
            </el-button>
            <el-button @click="scrollToSection('download')">
              <el-icon><Download /></el-icon>
              下载中心
            </el-button>
          </el-button-group>
        </div>
      </div>
    </div>

    <!-- 文档内容 -->
    <div class="doc-content">
      <!-- 快速开始 -->
      <section id="getting-started" class="doc-section">
        <h2 class="section-title">
          <el-icon><VideoPlay /></el-icon>
          快速开始
        </h2>
        
        <div class="getting-started-grid">
          <el-card class="guide-card">
            <template #header>
              <h3>🚀 安装和设置</h3>
            </template>
            <div class="code-block">
              <pre><code># 克隆项目
git clone https://github.com/wukeping2008/seesharptools-web.git

# 进入前端目录
cd seesharptools-web/frontend

# 安装依赖
npm install

# 启动开发服务器
npm run dev</code></pre>
            </div>
          </el-card>

          <el-card class="guide-card">
            <template #header>
              <h3>📦 NPM包安装</h3>
            </template>
            <div class="code-block">
              <pre><code># 安装控件库
npm install seesharptools-vue

# 在项目中导入
import { Oscilloscope, SignalGenerator } from 'seesharptools-vue'</code></pre>
            </div>
          </el-card>

          <el-card class="guide-card">
            <template #header>
              <h3>🎯 基础使用</h3>
            </template>
            <div class="code-block">
              <pre><code>&lt;template&gt;
  &lt;Oscilloscope 
    :width="1200" 
    :height="800"
    :config="config"
    @config-change="onConfigChange"
  /&gt;
&lt;/template&gt;</code></pre>
            </div>
          </el-card>
        </div>
      </section>

      <!-- 控件列表 -->
      <section id="components" class="doc-section">
        <h2 class="section-title">
          <el-icon><Grid /></el-icon>
          控件文档
        </h2>
        
        <!-- 控件分类 -->
        <div class="components-grid">
          <div 
            v-for="category in componentCategories" 
            :key="category.id"
            class="category-section"
          >
            <h3 class="category-title">
              <el-icon>
                <component :is="category.icon" />
              </el-icon>
              {{ category.title }}
            </h3>
            
            <div class="components-list">
              <el-card 
                v-for="component in category.components" 
                :key="component.name"
                class="component-card"
                shadow="hover"
              >
                <template #header>
                  <div class="component-header">
                    <h4>{{ component.name }}</h4>
                    <div class="component-actions">
                      <el-button size="small" @click="downloadComponent(component)">
                        <el-icon><Download /></el-icon>
                        下载
                      </el-button>
                    </div>
                  </div>
                </template>
                
                <div class="component-content">
                  <p class="component-description">{{ component.description }}</p>
                  
                  <!-- 使用示例 -->
                  <div class="component-example">
                    <h5>使用示例</h5>
                    <div class="code-block">
                      <pre><code>{{ component.example }}</code></pre>
                    </div>
                  </div>
                </div>
              </el-card>
            </div>
          </div>
        </div>
      </section>

      <!-- 下载中心 -->
      <section id="download" class="doc-section">
        <h2 class="section-title">
          <el-icon><Download /></el-icon>
          下载中心
        </h2>
        
        <div class="download-grid">
          <!-- 单个控件下载 -->
          <el-card class="download-card">
            <template #header>
              <h3>🎯 单个控件下载</h3>
            </template>
            
            <div class="download-content">
              <p>选择需要的控件，生成自定义下载包</p>
              
              <div class="component-selector">
                <h4>选择控件</h4>
                <div class="selector-grid">
                  <el-checkbox-group v-model="selectedComponents">
                    <div 
                      v-for="category in componentCategories" 
                      :key="category.id"
                      class="selector-category"
                    >
                      <h5>{{ category.title }}</h5>
                      <el-checkbox 
                        v-for="component in category.components" 
                        :key="component.name"
                        :label="component.name"
                        :value="component.name"
                      >
                        {{ component.name }}
                      </el-checkbox>
                    </div>
                  </el-checkbox-group>
                </div>
              </div>
              
              <div class="download-actions">
                <el-button 
                  type="primary" 
                  size="large"
                  @click="generateCustomDownload"
                  :disabled="selectedComponents.length === 0"
                >
                  <el-icon><Download /></el-icon>
                  生成下载包 ({{ selectedComponents.length }}个控件)
                </el-button>
              </div>
            </div>
          </el-card>

          <!-- 预设包下载 -->
          <el-card class="download-card">
            <template #header>
              <h3>📦 预设包下载</h3>
            </template>
            
            <div class="preset-packages">
              <div 
                v-for="preset in presetPackages" 
                :key="preset.id"
                class="preset-item"
              >
                <div class="preset-info">
                  <h4>{{ preset.name }}</h4>
                  <p>{{ preset.description }}</p>
                  <div class="preset-components">
                    <el-tag 
                      v-for="comp in preset.components.slice(0, 3)" 
                      :key="comp"
                      size="small"
                    >
                      {{ comp }}
                    </el-tag>
                    <span v-if="preset.components.length > 3">
                      +{{ preset.components.length - 3 }}个
                    </span>
                  </div>
                </div>
                <div class="preset-actions">
                  <el-button @click="downloadPreset(preset)">
                    <el-icon><Download /></el-icon>
                    下载
                  </el-button>
                </div>
              </div>
            </div>
          </el-card>

          <!-- 完整项目下载 -->
          <el-card class="download-card">
            <template #header>
              <h3>🚀 完整项目</h3>
            </template>
            
            <div class="project-download">
              <p>下载完整的SeeSharpTools Web项目，包含所有控件和示例</p>
              
              <div class="project-options">
                <div class="option-item">
                  <h4>前端项目</h4>
                  <p>Vue 3 + TypeScript + 所有控件</p>
                  <el-button type="primary" @click="downloadProject('frontend')">
                    <el-icon><Download /></el-icon>
                    下载前端项目
                  </el-button>
                </div>
                
                <div class="option-item">
                  <h4>完整项目</h4>
                  <p>前端 + 后端 + 文档</p>
                  <el-button type="primary" @click="downloadProject('full')">
                    <el-icon><Download /></el-icon>
                    下载完整项目
                  </el-button>
                </div>
              </div>
            </div>
          </el-card>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { 
  Document, VideoPlay, Grid, Download,
  TrendCharts, Monitor, DataAnalysis
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

// 响应式数据
const selectedComponents = ref<string[]>([])

// 控件分类数据
const componentCategories = reactive([
  {
    id: 1,
    title: '基础控件',
    icon: 'DataAnalysis',
    components: [
      {
        name: '数字显示器',
        description: '高精度数字显示控件，支持多种数值格式和单位',
        example: `<template>
  <DigitalDisplay 
    :value="75.234" 
    :precision="3"
    unit="V"
    @value-change="onValueChange"
  />
</template>`
      },
      {
        name: '线性仪表',
        description: '线性刻度仪表，适用于压力、温度等物理量显示',
        example: `<template>
  <LinearGauge 
    :value="65" 
    :min="0" 
    :max="100"
    orientation="horizontal"
    unit="°C"
  />
</template>`
      },
      {
        name: '温度计',
        description: '温度显示控件，支持摄氏度和华氏度',
        example: `<template>
  <Thermometer 
    :temperature="25.6" 
    unit="C"
    :range="{ min: -20, max: 100 }"
  />
</template>`
      }
    ]
  },
  {
    id: 2,
    title: '高性能图表',
    icon: 'TrendCharts',
    components: [
      {
        name: '高性能StripChart',
        description: '实时数据条带图，支持大数据量高频更新',
        example: `<template>
  <StripChart 
    :data="chartData" 
    :channels="channels"
    :time-span="10"
    :update-rate="100"
    @data-update="onDataUpdate"
  />
</template>`
      },
      {
        name: 'FFT频谱分析',
        description: '快速傅里叶变换频谱分析图表',
        example: `<template>
  <SpectrumChart 
    :time-data="signalData" 
    :fft-size="1024"
    window-function="hanning"
    :sample-rate="1000000"
    @fft-result="onFFTResult"
  />
</template>`
      },
      {
        name: '频谱分析仪',
        description: '专业频谱分析仪界面，模拟真实仪器',
        example: `<template>
  <SpectrumAnalyzer 
    :frequency="{ start: 0, stop: 1000000 }"
    :amplitude="{ ref: 0, scale: 10 }"
    :span="1000000"
    :rbw="1000"
  />
</template>`
      }
    ]
  },
  {
    id: 3,
    title: '模块仪器',
    icon: 'Monitor',
    components: [
      {
        name: '数字示波器',
        description: '专业数字示波器控件，支持多通道波形显示',
        example: `<template>
  <Oscilloscope 
    :config="scopeConfig" 
    :width="1200" 
    :height="800"
    @config-change="onConfigChange"
    @waveform-data="onWaveformData"
  />
</template>`
      },
      {
        name: '信号发生器',
        description: '多功能信号发生器，支持多种波形类型',
        example: `<template>
  <SignalGenerator 
    :config="genConfig"
    :channels="outputChannels"
    @output-change="onOutputChange"
  />
</template>`
      },
      {
        name: '数字万用表',
        description: '高精度数字万用表，支持多种测量功能',
        example: `<template>
  <DigitalMultimeter 
    mode="voltage"
    range="auto"
    :auto-range="true"
    @measurement-change="onMeasurement"
  />
</template>`
      }
    ]
  }
])

// 预设包
const presetPackages = reactive([
  {
    id: 1,
    name: '基础测量包',
    description: '包含基础测量所需的核心控件',
    components: ['数字显示器', '线性仪表', '温度计', '数字万用表']
  },
  {
    id: 2,
    name: '信号分析包',
    description: '专业信号分析和频谱分析工具',
    components: ['FFT频谱分析', '频谱分析仪', '高性能StripChart', '信号发生器']
  },
  {
    id: 3,
    name: '完整仪器包',
    description: '所有专业仪器控件的完整集合',
    components: ['数字示波器', '信号发生器', '数字万用表', 'FFT频谱分析', '频谱分析仪']
  }
])

// 方法
const scrollToSection = (sectionId: string) => {
  const element = document.getElementById(sectionId)
  if (element) {
    element.scrollIntoView({ behavior: 'smooth' })
  }
}

const downloadComponent = (component: any) => {
  const componentCode = generateComponentCode(component)
  downloadFile(`${component.name}.vue`, componentCode)
  ElMessage.success(`${component.name} 下载完成`)
}

const generateCustomDownload = () => {
  if (selectedComponents.value.length === 0) {
    ElMessage.warning('请选择至少一个控件')
    return
  }
  
  const packageData = generatePackage(selectedComponents.value)
  downloadFile('seesharptools-custom.zip', packageData)
  ElMessage.success(`自定义包下载完成 (${selectedComponents.value.length}个控件)`)
}

const downloadPreset = (preset: any) => {
  const packageData = generatePackage(preset.components)
  downloadFile(`${preset.name}.zip`, packageData)
  ElMessage.success(`${preset.name} 下载完成`)
}

const downloadProject = (type: string) => {
  const url = 'https://github.com/wukeping2008/seesharptools-web/archive/refs/heads/main.zip'
  window.open(url, '_blank')
  ElMessage.success('项目下载已开始')
}

// 工具函数
const generateComponentCode = (component: any) => {
  return `<!-- ${component.name} -->
${component.example}

<script setup lang="ts">
// ${component.description}
// 更多配置请参考文档
</script>

<style scoped>
/* 自定义样式 */
</style>`
}

const generatePackage = (components: string[]) => {
  return `Package for: ${components.join(', ')}`
}

const downloadFile = (filename: string, content: string) => {
  const blob = new Blob([content], { type: 'text/plain' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}
</script>

<style lang="scss" scoped>
.documentation-view {
  min-height: 100vh;
  background: var(--background-color);
}

.doc-header {
  background: linear-gradient(135deg, #2E86AB 0%, #A23B72 100%);
  color: white;
  padding: 60px 0;
  
  .header-content {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 24px;
    text-align: center;
    
    .doc-title {
      font-size: 48px;
      font-weight: 700;
      margin-bottom: 16px;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 16px;
    }
    
    .doc-subtitle {
      font-size: 20px;
      opacity: 0.9;
      margin-bottom: 40px;
    }
    
    .quick-nav {
      .el-button-group {
        .el-button {
          background: rgba(255, 255, 255, 0.1);
          border-color: rgba(255, 255, 255, 0.2);
          color: white;
          
          &:hover {
            background: rgba(255, 255, 255, 0.2);
          }
        }
      }
    }
  }
}

.doc-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
  
  .doc-section {
    margin: 80px 0;
    
    .section-title {
      font-size: 32px;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 40px;
      display: flex;
      align-items: center;
      gap: 12px;
    }
  }
}

.getting-started-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: 24px;
  
  .guide-card {
    .code-block {
      background: #1e1e1e;
      border-radius: 8px;
      padding: 16px;
      
      pre {
        margin: 0;
        color: #d4d4d4;
        font-family: 'Consolas', 'Monaco', monospace;
        font-size: 14px;
        line-height: 1.5;
      }
    }
  }
}

.components-grid {
  .category-section {
    margin-bottom: 60px;
    
    .category-title {
      font-size: 24px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 24px;
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .components-list {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
      gap: 24px;
      
      .component-card {
        .component-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          
          h4 {
            margin: 0;
            font-size: 18px;
            font-weight: 600;
          }
          
          .component-actions {
            display: flex;
            gap: 8px;
          }
        }
        
        .component-content {
          .component-description {
            color: var(--text-secondary);
            margin-bottom: 20px;
          }
          
          .component-example {
            .code-block {
              background: #1e1e1e;
              border-radius: 8px;
              padding: 12px;
              
              pre {
                margin: 0;
                color: #d4d4d4;
                font-family: 'Consolas', monospace;
                font-size: 12px;
                line-height: 1.4;
              }
            }
          }
        }
      }
    }
  }
}

.download-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 32px;
  
  .download-card {
    .download-content {
      .component-selector {
        margin-bottom: 24px;
        
        .selector-grid {
          .selector-category {
            margin-bottom: 16px;
            
            h5 {
              font-size: 14px;
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
      }
      
      .download-actions {
        text-align: center;
      }
    }
    
    .preset-packages {
      .preset-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 16px;
        border: 1px solid var(--border-color);
        border-radius: 8px;
        margin-bottom: 12px;
        
        .preset-info {
          flex: 1;
          
          h4 {
            margin: 0 0 8px 0;
            font-size: 16px;
            font-weight: 600;
          }
          
          p {
            margin: 0 0 8px 0;
            color: var(--text-secondary);
            font-size: 14px;
          }
          
          .preset-components {
            display: flex;
            flex-wrap: wrap;
            gap: 4px;
            align-items: center;
            
            span {
              font-size: 12px;
              color: var(--text-secondary);
            }
          }
        }
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
}

// 响应式设计
@media (max-width: 768px) {
  .doc-header {
    padding: 40px 0;
    
    .header-content {
      .doc-title {
        font-size: 32px;
      }
      
      .doc-subtitle {
        font-size: 16px;
      }
      
      .quick-nav {
        .el-button-group {
          display: flex;
          flex-direction: column;
          gap: 8px;
          
          .el-button {
            width: 100%;
          }
        }
      }
    }
  }
  
  .doc-content {
    padding: 0 16px;
    
    .doc-section {
      margin: 40px 0;
      
      .section-title {
        font-size: 24px;
      }
    }
  }
  
  .getting-started-grid,
  .components-list,
  .download-grid {
    grid-template-columns: 1fr;
  }
}
</style>
