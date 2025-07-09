<template>
  <div class="home-view">
    <!-- 英雄区域 -->
    <section class="hero-section">
      <div class="hero-content">
        <div class="hero-text">
          <h1 class="hero-title">
            专业测试测量仪器控件库
            <span class="title-highlight">Professional Test & Measurement Controls</span>
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
                革命性的AI辅助开发工具！只需用自然语言描述需求，AI自动生成专业的Vue 3控件代码，
                包含完整的TypeScript类型定义、SCSS样式和使用示例。
              </p>
              <div class="ai-features">
                <span class="ai-feature">自然语言理解</span>
                <span class="ai-feature">秒级代码生成</span>
                <span class="ai-feature">专业工业风格</span>
                <span class="ai-feature">完整类型支持</span>
              </div>
              <el-button type="primary" size="large" class="ai-cta" @click="router.push('/ai-control-generator-test')">
                <el-icon><Cpu /></el-icon>
                立即体验AI生成
              </el-button>
            </div>
          </div>
          
          <div class="hero-features">
            <div class="feature-item">
              <el-icon class="feature-icon"><DataAnalysis /></el-icon>
              <span>精密仪表控件</span>
            </div>
            <div class="feature-item">
              <el-icon class="feature-icon"><TrendCharts /></el-icon>
              <span>实时数据显示</span>
            </div>
            <div class="feature-item">
              <el-icon class="feature-icon"><Setting /></el-icon>
              <span>专业控制组件</span>
            </div>
            <div class="feature-item">
              <el-icon class="feature-icon"><Monitor /></el-icon>
              <span>科学仪器风格</span>
            </div>
          </div>
          <div class="hero-actions">
            <el-button type="primary" size="large" @click="router.push('/instruments')">
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

    <!-- 特性介绍 -->
    <section class="features-section">
      <div class="section-header">
        <h2 class="section-title">核心特性</h2>
        <p class="section-subtitle">为测试测量应用量身定制的专业控件</p>
      </div>
      
      <div class="features-grid">
        <div class="feature-card" v-for="feature in features" :key="feature.id">
          <div class="feature-icon-wrapper">
            <el-icon class="feature-icon-large">
              <component :is="feature.icon" />
            </el-icon>
          </div>
          <h3 class="feature-title">{{ feature.title }}</h3>
          <p class="feature-description">{{ feature.description }}</p>
          <ul class="feature-list">
            <li v-for="item in feature.items" :key="item">{{ item }}</li>
          </ul>
        </div>
      </div>
    </section>

    <!-- 控件分类 -->
    <section class="categories-section">
      <div class="section-header">
        <h2 class="section-title">控件分类</h2>
        <p class="section-subtitle">涵盖测试测量应用的各个方面</p>
      </div>
      
      <div class="categories-grid">
        <router-link 
          v-for="category in categories" 
          :key="category.id"
          :to="category.path"
          class="category-card"
        >
          <div class="category-icon">
            <el-icon>
              <component :is="category.icon" />
            </el-icon>
          </div>
          <h3 class="category-title">{{ category.title }}</h3>
          <p class="category-description">{{ category.description }}</p>
          <div class="category-count">{{ category.count }}+ 控件</div>
        </router-link>
      </div>
    </section>

    <!-- 技术栈 -->
    <section class="tech-section">
      <div class="section-header">
        <h2 class="section-title">技术栈</h2>
        <p class="section-subtitle">基于现代前端技术构建</p>
      </div>
      
      <div class="tech-grid">
        <div class="tech-item" v-for="tech in techStack" :key="tech.name">
          <div class="tech-logo">{{ tech.logo }}</div>
          <div class="tech-name">{{ tech.name }}</div>
          <div class="tech-version">{{ tech.version }}</div>
        </div>
      </div>
    </section>

    <!-- 快速开始 -->
    <section class="quickstart-section">
      <div class="quickstart-content">
        <h2 class="section-title">快速开始</h2>
        <p class="section-subtitle">几分钟内集成到您的项目中</p>
        
        <div class="quickstart-steps">
          <div class="step-item">
            <div class="step-number">1</div>
            <div class="step-content">
              <h4>安装依赖</h4>
              <div class="code-block">
                <code>npm install instrument-controls-web</code>
              </div>
            </div>
          </div>
          
          <div class="step-item">
            <div class="step-number">2</div>
            <div class="step-content">
              <h4>导入组件</h4>
              <div class="code-block">
                <code>import { CircularGauge } from 'instrument-controls-web'</code>
              </div>
            </div>
          </div>
          
          <div class="step-item">
            <div class="step-number">3</div>
            <div class="step-content">
              <h4>开始使用</h4>
              <div class="code-block">
                <code>&lt;CircularGauge :value="75.2" unit="V" /&gt;</code>
              </div>
            </div>
          </div>
        </div>
        
        <div class="quickstart-actions">
          <el-button type="primary" size="large" @click="router.push('/examples')">
            查看示例
          </el-button>
          <el-button size="large" @click="openDocs">
            阅读文档
          </el-button>
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

// 特性数据
const features = [
  {
    id: 1,
    icon: 'DataAnalysis',
    title: '精密仪表控件',
    description: '高精度的圆形和线性仪表，支持多种测量单位和量程自动调节',
    items: ['圆形仪表盘', '线性进度条', '数字显示器', '阈值标记']
  },
  {
    id: 2,
    icon: 'TrendCharts',
    title: '实时数据显示',
    description: '支持高频数据更新的波形和趋势图显示组件',
    items: ['示波器显示', '频谱分析', '数据记录器', '趋势图表']
  },
  {
    id: 3,
    icon: 'Setting',
    title: '专业控制组件',
    description: '精密的控制组件，支持细调和粗调操作',
    items: ['精密旋钮', '范围滑块', '按钮阵列', '开关控件']
  },
  {
    id: 4,
    icon: 'Warning',
    title: '状态指示系统',
    description: '多样化的状态指示和报警组件',
    items: ['LED指示器', '状态显示', '报警系统', '连接状态']
  }
]

// 控件分类
const categories = [
  {
    id: 1,
    title: '仪表控件',
    description: '圆形仪表、线性仪表、数字显示器等精密测量显示组件',
    icon: 'DataAnalysis',
    path: '/instruments',
    count: 8
  },
  {
    id: 2,
    title: '指示控件',
    description: 'LED指示器、状态显示、报警指示等状态反馈组件',
    icon: 'Warning',
    path: '/indicators',
    count: 6
  },
  {
    id: 3,
    title: '控制控件',
    description: '旋钮、滑块、按钮、开关等用户交互控制组件',
    icon: 'Setting',
    path: '/controls',
    count: 10
  },
  {
    id: 4,
    title: '示波器',
    description: '多通道波形显示、触发控制、测量标尺等示波器功能',
    icon: 'TrendCharts',
    path: '/oscilloscope',
    count: 5
  },
  {
    id: 5,
    title: '频谱分析',
    description: '频谱显示、标记功能、参考电平等频域分析组件',
    icon: 'DataLine',
    path: '/spectrum',
    count: 4
  },
  {
    id: 6,
    title: '信号发生器',
    description: '任意波形发生器、调制功能、扫频控制等信号生成组件',
    icon: 'Timer',
    path: '/signal-generator',
    count: 3
  },
  {
    id: 7,
    title: '数据采集卡',
    description: '高精度多通道数据采集系统，支持同步采样和实时数据处理',
    icon: 'DataAnalysis',
    path: '/data-acquisition',
    count: 1
  },
  {
    id: 8,
    title: '电源控制器',
    description: '可编程直流电源、恒压恒流模式、多重保护和序列控制',
    icon: 'Cpu',
    path: '/power-supply',
    count: 2
  },
    {
      id: 8,
      title: 'StripChart条带图',
      description: '高速数据流实时显示控件，支持1GS/s采样率和多通道同步',
      icon: 'TrendCharts',
      path: '/stripchart',
      count: 1
    },
    {
      id: 10,
      title: '高性能StripChart',
      description: 'WebGL渲染 + 多级缓存，支持1GS/s数据流的超高性能图表',
      icon: 'DataLine',
      path: '/enhanced-stripchart-test',
      count: 1
    },
    {
      id: 11,
      title: 'FFT频谱分析',
      description: '集成FFT分析功能的增强版图表，支持多种窗函数和峰值检测',
      icon: 'TrendCharts',
      path: '/spectrum-chart-test',
      count: 1
    },
    {
      id: 12,
      title: '高级数学分析图表',
      description: '集成统计分析、数据拟合、数字滤波器的专业图表组件',
      icon: 'DataAnalysis',
      path: '/advanced-easy-chart-test',
      count: 1
    },
  {
    id: 13,
    title: '专业测量工具图表',
    description: '集成游标测量、峰值检测、频率分析、自动测量的专业图表',
    icon: 'Monitor',
    path: '/professional-easy-chart-test',
    count: 1
  },
  {
    id: 14,
    title: '双Y轴图表',
    description: '支持左右双Y轴的专业图表，可显示不同量程和单位的数据系列',
    icon: 'DataAnalysis',
    path: '/dual-axis-easy-chart-test',
    count: 1
  },
  {
    id: 15,
    title: '温度采集卡',
    description: '专业多通道温度采集卡控件，支持8种热电偶类型和冷端补偿',
    icon: 'Monitor',
    path: '/temperature-acquisition-card-test',
    count: 1
  },
  {
    id: 16,
    title: '数字万用表',
    description: '专业数字万用表控件，支持8种测量功能、自动量程、统计分析',
    icon: 'Monitor',
    path: '/digital-multimeter-test',
    count: 1
  },
  {
    id: 17,
    title: '数字示波器',
    description: '专业4通道数字示波器控件，支持多种触发模式、精确测量、实时分析',
    icon: 'TrendCharts',
    path: '/oscilloscope-test',
    count: 1
  },
  {
    id: 18,
    title: 'DIO控制卡',
    description: '数字I/O控制卡控件，支持多端口、位操作、实时监控、时序控制',
    icon: 'Grid',
    path: '/dio-card-test',
    count: 1
  },
  {
    id: 19,
    title: 'AI智能控件生成器',
    description: '基于自然语言的AI控件生成系统，自动生成专业Vue 3控件代码',
    icon: 'Cpu',
    path: '/ai-control-generator-test',
    count: 1
  },
  {
    id: 20,
    title: '前后端集成测试',
    description: '测试前端与后端API和SignalR的完整集成功能',
    icon: 'Link',
    path: '/backend-integration-test',
    count: 1
  },
  {
    id: 21,
    title: '数据存储和回放系统',
    description: '高性能数据存储、历史数据查询、数据回放等功能测试',
    icon: 'DataAnalysis',
    path: '/data-storage-test',
    count: 1
  }
]

// 技术栈
const techStack = [
  { name: 'Vue 3', version: '3.4+', logo: 'V' },
  { name: 'TypeScript', version: '5.3+', logo: 'TS' },
  { name: 'ECharts', version: '5.4+', logo: 'E' },
  { name: 'Element Plus', version: '2.4+', logo: 'EP' },
  { name: 'Vite', version: '5.0+', logo: 'V' },
  { name: 'SCSS', version: '1.69+', logo: 'S' }
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
