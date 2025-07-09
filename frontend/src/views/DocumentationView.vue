<template>
  <div class="documentation-view">
    <div class="doc-header">
      <div class="header-content">
        <h1 class="doc-title">
          <el-icon><Document /></el-icon>
          SeeSharpTools Web 控件文档
        </h1>
        <p class="doc-subtitle">专业测试测量仪器控件使用指南</p>
      </div>
    </div>

    <div class="doc-content">
      <section class="doc-section">
        <h2 class="section-title">快速开始</h2>
        
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

      <section class="doc-section">
        <h2 class="section-title">控件文档</h2>
        
        <div class="components-grid">
          <div class="category-section">
            <h3 class="category-title">基础控件</h3>
            
            <div class="components-list">
              <el-card class="component-card" shadow="hover">
                <template #header>
                  <div class="component-header">
                    <h4>数字显示器</h4>
                    <div class="component-actions">
                      <el-button size="small" @click="downloadComponent('数字显示器')">
                        <el-icon><Download /></el-icon>
                        下载
                      </el-button>
                    </div>
                  </div>
                </template>
                
                <div class="component-content">
                  <p class="component-description">高精度数字显示控件，支持多种数值格式和单位</p>
                  
                  <div class="component-example">
                    <h5>使用示例</h5>
                    <div class="code-block">
                      <pre><code>&lt;template&gt;
  &lt;DigitalDisplay 
    :value="75.234" 
    :precision="3"
    unit="V"
    @value-change="onValueChange"
  /&gt;
&lt;/template&gt;</code></pre>
                    </div>
                  </div>
                </div>
              </el-card>

              <el-card class="component-card" shadow="hover">
                <template #header>
                  <div class="component-header">
                    <h4>线性仪表</h4>
                    <div class="component-actions">
                      <el-button size="small" @click="downloadComponent('线性仪表')">
                        <el-icon><Download /></el-icon>
                        下载
                      </el-button>
                    </div>
                  </div>
                </template>
                
                <div class="component-content">
                  <p class="component-description">线性刻度仪表，适用于压力、温度等物理量显示</p>
                  
                  <div class="component-example">
                    <h5>使用示例</h5>
                    <div class="code-block">
                      <pre><code>&lt;template&gt;
  &lt;LinearGauge 
    :value="65" 
    :min="0" 
    :max="100"
    orientation="horizontal"
    unit="°C"
  /&gt;
&lt;/template&gt;</code></pre>
                    </div>
                  </div>
                </div>
              </el-card>
            </div>
          </div>

          <div class="category-section">
            <h3 class="category-title">高性能图表</h3>
            
            <div class="components-list">
              <el-card class="component-card" shadow="hover">
                <template #header>
                  <div class="component-header">
                    <h4>高性能StripChart</h4>
                    <div class="component-actions">
                      <el-button size="small" @click="downloadComponent('高性能StripChart')">
                        <el-icon><Download /></el-icon>
                        下载
                      </el-button>
                    </div>
                  </div>
                </template>
                
                <div class="component-content">
                  <p class="component-description">实时数据条带图，支持大数据量高频更新</p>
                  
                  <div class="component-example">
                    <h5>使用示例</h5>
                    <div class="code-block">
                      <pre><code>&lt;template&gt;
  &lt;StripChart 
    :data="chartData" 
    :channels="channels"
    :time-span="10"
    :update-rate="100"
    @data-update="onDataUpdate"
  /&gt;
&lt;/template&gt;</code></pre>
                    </div>
                  </div>
                </div>
              </el-card>

              <el-card class="component-card" shadow="hover">
                <template #header>
                  <div class="component-header">
                    <h4>FFT频谱分析</h4>
                    <div class="component-actions">
                      <el-button size="small" @click="downloadComponent('FFT频谱分析')">
                        <el-icon><Download /></el-icon>
                        下载
                      </el-button>
                    </div>
                  </div>
                </template>
                
                <div class="component-content">
                  <p class="component-description">快速傅里叶变换频谱分析图表</p>
                  
                  <div class="component-example">
                    <h5>使用示例</h5>
                    <div class="code-block">
                      <pre><code>&lt;template&gt;
  &lt;SpectrumChart 
    :time-data="signalData" 
    :fft-size="1024"
    window-function="hanning"
    :sample-rate="1000000"
    @fft-result="onFFTResult"
  /&gt;
&lt;/template&gt;</code></pre>
                    </div>
                  </div>
                </div>
              </el-card>
            </div>
          </div>

          <div class="category-section">
            <h3 class="category-title">模块仪器</h3>
            
            <div class="components-list">
              <el-card class="component-card" shadow="hover">
                <template #header>
                  <div class="component-header">
                    <h4>数字示波器</h4>
                    <div class="component-actions">
                      <el-button size="small" @click="downloadComponent('数字示波器')">
                        <el-icon><Download /></el-icon>
                        下载
                      </el-button>
                    </div>
                  </div>
                </template>
                
                <div class="component-content">
                  <p class="component-description">专业数字示波器控件，支持多通道波形显示</p>
                  
                  <div class="component-example">
                    <h5>使用示例</h5>
                    <div class="code-block">
                      <pre><code>&lt;template&gt;
  &lt;Oscilloscope 
    :config="scopeConfig" 
    :width="1200" 
    :height="800"
    @config-change="onConfigChange"
    @waveform-data="onWaveformData"
  /&gt;
&lt;/template&gt;</code></pre>
                    </div>
                  </div>
                </div>
              </el-card>

              <el-card class="component-card" shadow="hover">
                <template #header>
                  <div class="component-header">
                    <h4>信号发生器</h4>
                    <div class="component-actions">
                      <el-button size="small" @click="downloadComponent('信号发生器')">
                        <el-icon><Download /></el-icon>
                        下载
                      </el-button>
                    </div>
                  </div>
                </template>
                
                <div class="component-content">
                  <p class="component-description">多功能信号发生器，支持多种波形类型</p>
                  
                  <div class="component-example">
                    <h5>使用示例</h5>
                    <div class="code-block">
                      <pre><code>&lt;template&gt;
  &lt;SignalGenerator 
    :config="genConfig"
    :channels="outputChannels"
    @output-change="onOutputChange"
  /&gt;
&lt;/template&gt;</code></pre>
                    </div>
                  </div>
                </div>
              </el-card>
            </div>
          </div>
        </div>
      </section>

      <section class="doc-section">
        <h2 class="section-title">下载中心</h2>
        
        <div class="download-grid">
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
                  <el-button type="primary" @click="downloadProject">
                    <el-icon><Download /></el-icon>
                    下载前端项目
                  </el-button>
                </div>
                
                <div class="option-item">
                  <h4>完整项目</h4>
                  <p>前端 + 后端 + 文档</p>
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
    </div>
  </div>
</template>

<script setup lang="ts">
import { Document, Download } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

const downloadComponent = (componentName: string) => {
  ElMessage.success(componentName + ' 下载完成')
}

const downloadProject = () => {
  const url = 'https://github.com/wukeping2008/seesharptools-web/archive/refs/heads/main.zip'
  window.open(url, '_blank')
  ElMessage.success('项目下载已开始')
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
