<template>
  <div class="home-view">
    <!-- 英雄区域 -->
    <section class="hero-section">
      <div class="hero-content">
        <div class="hero-text">
          <h1 class="hero-title">
            SeeSharpTools Web 版本
            <span class="title-highlight">专业测试测量仪器控件库</span>
          </h1>
          <p class="hero-description">
            基于 Vue 3 + TypeScript 构建的专业测试测量仪器界面控件库，
            参考 SeeSharpTools 设计风格，为科学仪器和数据采集系统提供高质量的用户界面组件。
          </p>
          
          <!-- AI特色功能突出显示 -->
          <div class="ai-highlight">
            <div class="ai-badge">
              <el-icon class="ai-icon"><Cpu /></el-icon>
              <span class="ai-text">AI驱动</span>
            </div>
            <div class="ai-content">
              <h3 class="ai-title">🤖 AI智能控件生成器</h3>
              <p class="ai-description">
                革命性的AI辅助开发工具！只需用自然语言描述需求，AI自动生成专业的Vue 3控件代码。
              </p>
              <el-button type="primary" size="large" class="ai-cta" @click="router.push('/ai-control-generator-test')">
                <el-icon><Cpu /></el-icon>
                立即体验AI生成
              </el-button>
            </div>
          </div>
          
          <div class="hero-actions">
            <el-button type="primary" size="large" @click="scrollToModules">
              <el-icon><DataAnalysis /></el-icon>
              开始探索
            </el-button>
            <el-button size="large" @click="openGitHub">
              <el-icon><Link /></el-icon>
              GitHub
            </el-button>
          </div>
        </div>
        <div class="hero-visual">
          <div class="instrument-showcase">
            <!-- 示例仪表盘 -->
            <div class="showcase-item gauge-demo">
              <div class="demo-title">精密圆形仪表</div>
              <div class="gauge-container">
                <svg width="200" height="200" viewBox="0 0 200 200">
                  <!-- 外圆环 -->
                  <circle cx="100" cy="100" r="90" fill="none" stroke="#E5E7EB" stroke-width="2"/>
                  <!-- 进度弧 -->
                  <path d="M 30 100 A 70 70 0 0 1 170 100" fill="none" stroke="#2E86AB" stroke-width="8" stroke-linecap="round"/>
                  <!-- 刻度线 -->
                  <g stroke="#6B7280" stroke-width="2">
                    <line x1="100" y1="20" x2="100" y2="35" />
                    <line x1="170" y1="100" x2="155" y2="100" />
                    <line x1="100" y1="180" x2="100" y2="165" />
                    <line x1="30" y1="100" x2="45" y2="100" />
                  </g>
                  <!-- 指针 -->
                  <line x1="100" y1="100" x2="100" y2="40" stroke="#1F2937" stroke-width="3" stroke-linecap="round"/>
                  <circle cx="100" cy="100" r="6" fill="#2E86AB"/>
                  <!-- 数值显示 -->
                  <text x="100" y="130" text-anchor="middle" font-family="Consolas" font-size="18" font-weight="bold" fill="#2E86AB">75.2</text>
                  <text x="100" y="145" text-anchor="middle" font-family="Segoe UI" font-size="12" fill="#6B7280">V</text>
                </svg>
              </div>
            </div>
            
            <!-- 示例数字显示 -->
            <div class="showcase-item digital-demo">
              <div class="demo-title">数字显示器</div>
              <div class="digital-display-demo">
                <div class="digital-value">{{ animatedValue.toFixed(3) }}</div>
                <div class="digital-unit">Hz</div>
              </div>
              <div class="status-indicators">
                <div class="status-indicator normal">
                  <div class="dot"></div>
                  <span>正常</span>
                </div>
              </div>
            </div>
            
            <!-- 示例波形 -->
            <div class="showcase-item waveform-demo">
              <div class="demo-title">波形显示</div>
              <div class="waveform-container">
                <svg width="250" height="120" viewBox="0 0 250 120">
                  <defs>
                    <linearGradient id="waveGradient" x1="0%" y1="0%" x2="0%" y2="100%">
                      <stop offset="0%" style="stop-color:#2E86AB;stop-opacity:0.8" />
                      <stop offset="100%" style="stop-color:#2E86AB;stop-opacity:0.1" />
                    </linearGradient>
                  </defs>
                  <!-- 网格 -->
                  <g stroke="#E5E7EB" stroke-width="0.5" opacity="0.5">
                    <line v-for="i in 5" :key="`h${i}`" :x1="0" :y1="i * 24" :x2="250" :y2="i * 24" />
                    <line v-for="i in 10" :key="`v${i}`" :x1="i * 25" :y1="0" :x2="i * 25" :y2="120" />
                  </g>
                  <!-- 波形 -->
                  <path :d="waveformPath" fill="url(#waveGradient)" stroke="#2E86AB" stroke-width="2"/>
                </svg>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 主要模块 -->
    <section class="modules-section" ref="modulesSection">
      <div class="section-header">
        <h2 class="section-title">主要模块</h2>
        <p class="section-subtitle">前端控件库 + 后端硬件集成平台</p>
      </div>
      
      <div class="modules-grid">
        <!-- 前端控件库模块 -->
        <div class="module-card frontend-module">
          <div class="module-header">
            <div class="module-icon">
              <el-icon><Monitor /></el-icon>
            </div>
            <div class="module-info">
              <h3 class="module-title">前端控件库</h3>
              <p class="module-subtitle">专业测控界面组件</p>
            </div>
          </div>
          
          <div class="module-categories">
            <router-link 
              v-for="category in frontendCategories" 
              :key="category.id"
              :to="category.path"
              class="category-item"
            >
              <div class="category-icon">
                <el-icon>
                  <component :is="category.icon" />
                </el-icon>
              </div>
              <div class="category-content">
                <h4 class="category-title">{{ category.title }}</h4>
                <p class="category-description">{{ category.description }}</p>
                <span class="category-count">{{ category.count }}+ 控件</span>
              </div>
            </router-link>
          </div>
        </div>

        <!-- 后端集成平台模块 -->
        <div class="module-card backend-module">
          <div class="module-header">
            <div class="module-icon">
              <el-icon><Cpu /></el-icon>
            </div>
            <div class="module-info">
              <h3 class="module-title">后端集成平台</h3>
              <p class="module-subtitle">硬件驱动与数据处理</p>
            </div>
          </div>
          
          <div class="module-categories">
            <router-link 
              v-for="category in backendCategories" 
              :key="category.id"
              :to="category.path"
              class="category-item"
            >
              <div class="category-icon">
                <el-icon>
                  <component :is="category.icon" />
                </el-icon>
              </div>
              <div class="category-content">
                <h4 class="category-title">{{ category.title }}</h4>
                <p class="category-description">{{ category.description }}</p>
                <span class="category-status">{{ category.status }}</span>
              </div>
            </router-link>
          </div>
        </div>
      </div>
    </section>

    <!-- 项目信息 -->
    <section class="info-section">
      <div class="info-content">
        <div class="info-item">
          <h3>技术栈</h3>
          <p>Vue 3 + TypeScript + ECharts + ASP.NET Core</p>
        </div>
        <div class="info-item">
          <h3>开源协议</h3>
          <p>MIT License</p>
        </div>
        <div class="info-item">
          <h3>文档</h3>
          <p>完整的API文档和使用指南 <span class="coming-soon">(即将推出)</span></p>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { 
  DataAnalysis, TrendCharts, Setting, Monitor, Link,
  Warning, Grid, Cpu, DataLine, Timer
} from '@element-plus/icons-vue'

// 路由
const router = useRouter()

// 响应式数据
const animatedValue = ref(1000.0)
const waveformPath = ref('')
const modulesSection = ref<HTMLElement>()

// 前端控件分类
const frontendCategories = [
  {
    id: 1,
    title: '基础控件',
    description: '仪表、指示器、控制器等基础界面组件',
    icon: 'DataAnalysis',
    path: '/instruments',
    count: 15
  },
  {
    id: 2,
    title: '高性能图表',
    description: 'StripChart、FFT频谱分析、专业测量工具',
    icon: 'TrendCharts',
    path: '/enhanced-stripchart-test',
    count: 8
  },
  {
    id: 3,
    title: '专业仪器',
    description: '示波器、万用表、信号发生器等专业仪器控件',
    icon: 'Monitor',
    path: '/oscilloscope-test',
    count: 6
  },
  {
    id: 4,
    title: 'AI智能生成',
    description: '基于自然语言的AI控件生成系统',
    icon: 'Cpu',
    path: '/ai-control-generator-test',
    count: 1
  }
]

// 后端集成分类
const backendCategories = [
  {
    id: 1,
    title: '前后端集成测试',
    description: '测试前端与后端API和SignalR的完整集成功能',
    icon: 'Link',
    path: '/backend-integration-test',
    status: '✅ 已完成'
  },
  {
    id: 2,
    title: '数据存储和回放',
    description: '高性能数据存储、历史数据查询、数据回放系统',
    icon: 'DataAnalysis',
    path: '/data-storage-test',
    status: '✅ 已完成'
  },
  {
    id: 3,
    title: '硬件驱动管理',
    description: 'MISD标准化接口层、驱动管理、设备发现',
    icon: 'Cpu',
    path: '/backend-integration-test',
    status: '✅ 已完成'
  },
  {
    id: 4,
    title: '实时数据采集',
    description: '多线程数据采集引擎、SignalR实时通信',
    icon: 'TrendCharts',
    path: '/backend-integration-test',
    status: '✅ 已完成'
  }
]

// 动画定时器
let animationTimer: number | null = null
let waveformTimer: number | null = null

// 方法
const openGitHub = () => {
  window.open('https://github.com/wukeping2008/seesharptools-web', '_blank')
}

const openDocs = () => {
  window.open('/docs', '_blank')
}

const scrollToModules = () => {
  if (modulesSection.value) {
    modulesSection.value.scrollIntoView({ behavior: 'smooth' })
  }
}

const startAnimations = () => {
  // 数值动画
  animationTimer = setInterval(() => {
    animatedValue.value = 1000 + Math.sin(Date.now() / 1000) * 50 + Math.random() * 10
  }, 100)
  
  // 波形动画
  const updateWaveform = () => {
    const points = []
    const time = Date.now() / 1000
    
    for (let x = 0; x <= 250; x += 5) {
      const t = (x / 250) * 4 * Math.PI + time
      const y = 60 + Math.sin(t) * 20 + Math.sin(t * 3) * 10
      points.push(`${x},${y}`)
    }
    
    waveformPath.value = `M ${points.join(' L ')} L 250,120 L 0,120 Z`
  }
  
  updateWaveform()
  waveformTimer = setInterval(updateWaveform, 50)
}

const stopAnimations = () => {
  if (animationTimer) {
    clearInterval(animationTimer)
    animationTimer = null
  }
  if (waveformTimer) {
    clearInterval(waveformTimer)
    waveformTimer = null
  }
}

// 生命周期
onMounted(() => {
  startAnimations()
})

onUnmounted(() => {
  stopAnimations()
})
</script>

<style lang="scss" scoped>
.home-view {
  min-height: 100vh;
}

.hero-section {
  padding: 80px 0;
  background: linear-gradient(135deg, rgba(46, 134, 171, 0.1) 0%, rgba(162, 59, 114, 0.1) 100%);
  
  .hero-content {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 60px;
    align-items: center;
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 24px;
  }
  
  .hero-text {
    .hero-title {
      font-size: 48px;
      font-weight: 700;
      color: var(--text-primary);
      line-height: 1.2;
      margin-bottom: 16px;
      
      .title-highlight {
        display: block;
        font-size: 24px;
        color: var(--primary-color);
        font-weight: 500;
        font-style: italic;
        margin-top: 8px;
      }
    }
    
    .hero-description {
      font-size: 18px;
      color: var(--text-secondary);
      line-height: 1.6;
      margin-bottom: 32px;
    }
    
    // AI特色功能突出显示样式
    .ai-highlight {
      background: linear-gradient(135deg, rgba(46, 134, 171, 0.1), rgba(162, 59, 114, 0.1));
      border: 2px solid var(--primary-color);
      border-radius: 16px;
      padding: 24px;
      margin-bottom: 32px;
      position: relative;
      overflow: hidden;
      
      &::before {
        content: '';
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        height: 4px;
        background: linear-gradient(90deg, var(--primary-color), #A23B72, var(--primary-color));
        animation: shimmer 2s infinite;
      }
      
      .ai-badge {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        background: var(--primary-color);
        color: white;
        padding: 6px 12px;
        border-radius: 20px;
        font-size: 12px;
        font-weight: 600;
        margin-bottom: 16px;
        
        .ai-icon {
          font-size: 14px;
        }
      }
      
      .ai-content {
        .ai-title {
          font-size: 24px;
          font-weight: 700;
          color: var(--text-primary);
          margin-bottom: 12px;
          
          background: linear-gradient(135deg, var(--primary-color), #A23B72);
          -webkit-background-clip: text;
          -webkit-text-fill-color: transparent;
          background-clip: text;
        }
        
        .ai-description {
          color: var(--text-secondary);
          line-height: 1.6;
          margin-bottom: 16px;
        }
        
        .ai-features {
          display: flex;
          flex-wrap: wrap;
          gap: 8px;
          margin-bottom: 20px;
          
          .ai-feature {
            background: rgba(46, 134, 171, 0.1);
            color: var(--primary-color);
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: 500;
            border: 1px solid rgba(46, 134, 171, 0.2);
          }
        }
        
        .ai-cta {
          background: linear-gradient(135deg, var(--primary-color), #A23B72);
          border: none;
          padding: 12px 24px;
          font-size: 16px;
          font-weight: 600;
          box-shadow: 0 4px 15px rgba(46, 134, 171, 0.3);
          transition: all 0.3s ease;
          
          &:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(46, 134, 171, 0.4);
          }
        }
      }
    }
    
    .hero-features {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
      margin-bottom: 40px;
      
      .feature-item {
        display: flex;
        align-items: center;
        gap: 8px;
        
        .feature-icon {
          color: var(--primary-color);
          font-size: 20px;
        }
        
        span {
          font-weight: 500;
          color: var(--text-primary);
        }
      }
    }
    
    .hero-actions {
      display: flex;
      gap: 16px;
      
      .el-button {
        padding: 12px 24px;
        font-size: 16px;
      }
    }
  }
  
  .hero-visual {
    .instrument-showcase {
      display: grid;
      grid-template-columns: 1fr;
      gap: 24px;
      
      .showcase-item {
        background: var(--instrument-bg);
        border: 1px solid var(--instrument-border);
        border-radius: 12px;
        padding: 20px;
        text-align: center;
        backdrop-filter: blur(10px);
        
        .demo-title {
          font-size: 14px;
          font-weight: 600;
          color: var(--text-primary);
          margin-bottom: 16px;
        }
      }
      
      .digital-display-demo {
        background: var(--digital-bg);
        border: 2px solid var(--border-color);
        border-radius: 8px;
        padding: 16px;
        margin-bottom: 12px;
        
        .digital-value {
          font-family: 'Consolas', 'Monaco', monospace;
          font-size: 32px;
          font-weight: bold;
          color: var(--digital-value);
          line-height: 1;
        }
        
        .digital-unit {
          font-size: 16px;
          color: var(--digital-unit);
          margin-top: 4px;
        }
      }
      
      .status-indicators {
        display: flex;
        justify-content: center;
        
        .status-indicator {
          display: flex;
          align-items: center;
          gap: 6px;
          padding: 4px 12px;
          background: rgba(16, 185, 129, 0.1);
          color: var(--status-normal);
          border-radius: 12px;
          font-size: 12px;
          font-weight: 500;
          
          .dot {
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: currentColor;
          }
        }
      }
    }
  }
}

.features-section,
.categories-section,
.tech-section,
.quickstart-section {
  padding: 80px 0;
  
  .section-header {
    text-align: center;
    margin-bottom: 60px;
    
    .section-title {
      font-size: 36px;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 16px;
    }
    
    .section-subtitle {
      font-size: 18px;
      color: var(--text-secondary);
    }
  }
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 32px;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
  
  .feature-card {
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 16px;
    padding: 32px;
    text-align: center;
    transition: all 0.3s ease;
    
    &:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
    }
    
    .feature-icon-wrapper {
      width: 80px;
      height: 80px;
      background: linear-gradient(135deg, var(--primary-color), var(--primary-light));
      border-radius: 20px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 24px;
      
      .feature-icon-large {
        font-size: 40px;
        color: white;
      }
    }
    
    .feature-title {
      font-size: 20px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 12px;
    }
    
    .feature-description {
      color: var(--text-secondary);
      line-height: 1.6;
      margin-bottom: 20px;
    }
    
    .feature-list {
      list-style: none;
      
      li {
        padding: 4px 0;
        color: var(--text-secondary);
        font-size: 14px;
        
        &:before {
          content: '✓';
          color: var(--primary-color);
          font-weight: bold;
          margin-right: 8px;
        }
      }
    }
  }
}

.categories-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 24px;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
  
  .category-card {
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 12px;
    padding: 24px;
    text-decoration: none;
    transition: all 0.3s ease;
    display: block;
    
    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
      border-color: var(--primary-color);
    }
    
    .category-icon {
      width: 60px;
      height: 60px;
      background: var(--primary-color);
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 16px;
      
      .el-icon {
        font-size: 28px;
        color: white;
      }
    }
    
    .category-title {
      font-size: 18px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 8px;
    }
    
    .category-description {
      color: var(--text-secondary);
      line-height: 1.5;
      margin-bottom: 12px;
    }
    
    .category-count {
      color: var(--primary-color);
      font-weight: 500;
      font-size: 14px;
    }
  }
}

.tech-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 24px;
  max-width: 800px;
  margin: 0 auto;
  padding: 0 24px;
  
  .tech-item {
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 12px;
    padding: 24px;
    text-align: center;
    transition: all 0.3s ease;
    
    &:hover {
      border-color: var(--primary-color);
    }
    
    .tech-logo {
      width: 60px;
      height: 60px;
      background: var(--primary-color);
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 12px;
      font-size: 24px;
      font-weight: bold;
      color: white;
    }
    
    .tech-name {
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 4px;
    }
    
    .tech-version {
      font-size: 14px;
      color: var(--text-secondary);
    }
  }
}

// 主要模块样式
.modules-section {
  padding: 80px 0;
  background: var(--background-color);
  
  .section-header {
    text-align: center;
    margin-bottom: 60px;
    
    .section-title {
      font-size: 36px;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 16px;
    }
    
    .section-subtitle {
      font-size: 18px;
      color: var(--text-secondary);
    }
  }
  
  .modules-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 40px;
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 24px;
    
    @media (max-width: 768px) {
      grid-template-columns: 1fr;
      gap: 30px;
    }
  }
  
  .module-card {
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 16px;
    padding: 32px;
    transition: all 0.3s ease;
    
    &:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
    }
    
    &.frontend-module {
      border-left: 4px solid #2E86AB;
    }
    
    &.backend-module {
      border-left: 4px solid #A23B72;
    }
    
    .module-header {
      display: flex;
      align-items: center;
      gap: 16px;
      margin-bottom: 24px;
      
      .module-icon {
        width: 60px;
        height: 60px;
        border-radius: 12px;
        display: flex;
        align-items: center;
        justify-content: center;
        
        .el-icon {
          font-size: 28px;
          color: white;
        }
      }
      
      .module-info {
        .module-title {
          font-size: 24px;
          font-weight: 700;
          color: var(--text-primary);
          margin-bottom: 4px;
        }
        
        .module-subtitle {
          font-size: 14px;
          color: var(--text-secondary);
        }
      }
    }
    
    &.frontend-module .module-header .module-icon {
      background: linear-gradient(135deg, #2E86AB, #4A9FBF);
    }
    
    &.backend-module .module-header .module-icon {
      background: linear-gradient(135deg, #A23B72, #B85A8A);
    }
    
    .module-categories {
      display: grid;
      gap: 16px;
      
      .category-item {
        display: flex;
        align-items: center;
        gap: 16px;
        padding: 16px;
        background: rgba(46, 134, 171, 0.05);
        border: 1px solid rgba(46, 134, 171, 0.1);
        border-radius: 12px;
        text-decoration: none;
        transition: all 0.3s ease;
        
        &:hover {
          background: rgba(46, 134, 171, 0.1);
          border-color: rgba(46, 134, 171, 0.2);
          transform: translateX(4px);
        }
        
        .category-icon {
          width: 40px;
          height: 40px;
          background: var(--primary-color);
          border-radius: 8px;
          display: flex;
          align-items: center;
          justify-content: center;
          flex-shrink: 0;
          
          .el-icon {
            font-size: 20px;
            color: white;
          }
        }
        
        .category-content {
          flex: 1;
          
          .category-title {
            font-size: 16px;
            font-weight: 600;
            color: var(--text-primary);
            margin-bottom: 4px;
          }
          
          .category-description {
            font-size: 14px;
            color: var(--text-secondary);
            line-height: 1.4;
            margin-bottom: 6px;
          }
          
          .category-count {
            font-size: 12px;
            color: var(--primary-color);
            font-weight: 500;
          }
          
          .category-status {
            font-size: 12px;
            color: #10B981;
            font-weight: 500;
          }
        }
      }
    }
  }
}

// 项目信息样式
.info-section {
  padding: 60px 0;
  background: var(--surface-color);
  border-top: 1px solid var(--border-color);
  
  .info-content {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 32px;
    max-width: 1000px;
    margin: 0 auto;
    padding: 0 24px;
    
    .info-item {
      text-align: center;
      
      h3 {
        font-size: 18px;
        font-weight: 600;
        color: var(--text-primary);
        margin-bottom: 8px;
      }
      
      p {
        font-size: 14px;
        color: var(--text-secondary);
        line-height: 1.5;
        
        .coming-soon {
          color: #F59E0B;
          font-style: italic;
        }
      }
    }
  }
}

.quickstart-section {
  background: var(--background-color);
  
  .quickstart-content {
    max-width: 800px;
    margin: 0 auto;
    padding: 0 24px;
    text-align: center;
  }
  
  .quickstart-steps {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 32px;
    margin: 40px 0;
    
    .step-item {
      .step-number {
        width: 40px;
        height: 40px;
        background: var(--primary-color);
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        margin: 0 auto 16px;
        font-size: 18px;
        font-weight: bold;
        color: white;
      }
      
      .step-content {
        h4 {
          font-size: 16px;
          font-weight: 600;
          color: var(--text-primary);
          margin-bottom: 12px;
        }
        
        .code-block {
          background: var(--text-primary);
          border-radius: 8px;
          padding: 12px;
          
          code {
            color: #00ff00;
            font-family: 'Consolas', 'Monaco', monospace;
            font-size: 14px;
          }
        }
      }
    }
  }
  
  .quickstart-actions {
    display: flex;
    gap: 16px;
    justify-content: center;
    
    .el-button {
      padding: 12px 24px;
      font-size: 16px;
    }
  }
}

// 动画定义
@keyframes shimmer {
  0% {
    background-position: -200% 0;
  }
  100% {
    background-position: 200% 0;
  }
}

// 响应式设计
@media (max-width: 1024px) {
  .hero-section {
    .hero-content {
      grid-template-columns: 1fr;
      gap: 40px;
      text-align: center;
    }
    
    .hero-text {
      .hero-title {
        font-size: 36px;
      }
    }
  }
}

@media (max-width: 768px) {
  .hero-section {
    padding: 40px 0;
    
    .hero-text {
      .hero-title {
        font-size: 28px;
        
        .title-highlight {
          font-size: 18px;
        }
      }
      
      .hero-description {
        font-size: 16px;
      }
      
      .hero-features {
        grid-template-columns: 1fr;
      }
      
      .hero-actions {
        flex-direction: column;
        align-items: center;
        
        .el-button {
          width: 200px;
        }
      }
    }
  }
  
  .features-section,
  .categories-section,
  .tech-section,
  .quickstart-section {
    padding: 40px 0;
    
    .section-header {
      margin-bottom: 40px;
      
      .section-title {
        font-size: 28px;
      }
      
      .section-subtitle {
        font-size: 16px;
      }
    }
  }
  
  .quickstart-steps {
    .step-item {
      .step-content {
        .code-block {
          code {
            font-size: 12px;
          }
        }
      }
    }
  }
}
</style>
