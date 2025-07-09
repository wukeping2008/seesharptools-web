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
        </el-tabs>
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
  }
])

function downloadComponent(component: any) {
  const code = 'Vue组件代码示例'
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
  const doc = '文档内容示例'
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
</style>
