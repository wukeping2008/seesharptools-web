<template>
  <div class="home-view">
    <!-- 英雄区域 -->
    <section class="hero-section">
      <div class="hero-content">
        <div class="hero-text">
          <h1 class="hero-title">
            SeeSharp on Web
            <span class="title-highlight">基于Web的AI智能模块仪器平台</span>
          </h1>
          <p class="hero-description">
            革命性的AI驱动测试测量平台！集成专业仪器控件库、智能代码生成和硬件驱动管理，
            让您用自然语言描述测试需求，AI自动生成C#代码并直接调用简仪硬件进行实际测试。
          </p>
          
          <!-- AI特色功能突出显示 -->
          <div class="ai-highlight">
            <div class="ai-badge">
              <el-icon class="ai-icon"><Cpu /></el-icon>
              <span class="ai-text">AI驱动</span>
            </div>
            <div class="ai-content">
              <h3 class="ai-title">🚀 AI智能测试平台</h3>
              <p class="ai-description">
                突破性的AI智能测试解决方案！用中文描述测试需求，AI自动生成C#测试代码，通过C# Runner MCP直接调用简仪硬件进行实际测试，支持JY5500信号发生器和JYUSB1601数据采集卡。
              </p>
              <div class="ai-features">
                <span class="ai-feature">🎯 自然语言需求分析</span>
                <span class="ai-feature">🔧 智能代码生成</span>
                <span class="ai-feature">⚡ 硬件直接调用</span>
                <span class="ai-feature">📊 实时结果展示</span>
              </div>
              <div class="ai-actions">
                <el-button type="primary" size="large" class="ai-cta primary" @click="router.push('/ai-test-platform')">
                  <el-icon><Cpu /></el-icon>
                  立即体验AI测试
                </el-button>
                <el-button size="large" class="ai-cta secondary" @click="router.push('/ai-control-generator')">
                  <el-icon><Setting /></el-icon>
                  AI控件生成器
                </el-button>
              </div>
            </div>
          </div>
          
          <div class="hero-actions">
            <el-button type="primary" size="large" @click="router.push('/ai-test-platform')">
              <el-icon><Cpu /></el-icon>
              AI智能测试
            </el-button>
            <el-button size="large" @click="scrollToModules">
              <el-icon><DataAnalysis /></el-icon>
              探索控件库
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

    <!-- USB-1601 数据采集演示区域 -->
    <section class="usb1601-featured-section">
      <div class="featured-content">
        <div class="featured-header">
          <div class="featured-badge">
            <el-icon class="badge-icon"><Cpu /></el-icon>
            <span class="badge-text">最新功能</span>
          </div>
          <h2 class="featured-title">
            🚀 简仪科技 USB-1601 数据采集演示
          </h2>
          <p class="featured-subtitle">
            专业级USB数据采集卡Web演示平台 - 支持AI/AO/DI/DO多功能测量与控制
          </p>
        </div>
        
        <div class="featured-body">
          <div class="featured-left">
            <div class="feature-highlights">
              <div class="highlight-item">
                <div class="highlight-icon">
                  <el-icon><DataAnalysis /></el-icon>
                </div>
                <div class="highlight-content">
                  <h4>16路AI模拟输入</h4>
                  <p>16位分辨率，1MS/s采样率，实时波形显示</p>
                </div>
              </div>
              
              <div class="highlight-item">
                <div class="highlight-icon">
                  <el-icon><Connection /></el-icon>
                </div>
                <div class="highlight-content">
                  <h4>2路AO模拟输出</h4>
                  <p>16位分辨率，任意波形生成，实时控制</p>
                </div>
              </div>
              
              <div class="highlight-item">
                <div class="highlight-icon">
                  <el-icon><SwitchButton /></el-icon>
                </div>
                <div class="highlight-content">
                  <h4>8路数字IO</h4>
                  <p>TTL/CMOS兼容，高速数字信号处理</p>
                </div>
              </div>
              
              <div class="highlight-item">
                <div class="highlight-icon">
                  <el-icon><Code /></el-icon>
                </div>
                <div class="highlight-content">
                  <h4>C# Runner集成</h4>
                  <p>在线C#代码执行，模拟硬件操作</p>
                </div>
              </div>
            </div>
            
            <div class="featured-stats">
              <div class="stat-item">
                <div class="stat-value">1MS/s</div>
                <div class="stat-label">采样率</div>
              </div>
              <div class="stat-item">
                <div class="stat-value">16位</div>
                <div class="stat-label">分辨率</div>
              </div>
              <div class="stat-item">
                <div class="stat-value">硬件+模拟</div>
                <div class="stat-label">双模式</div>
              </div>
              <div class="stat-item">
                <div class="stat-value">实时</div>
                <div class="stat-label">数据流</div>
              </div>
            </div>
          </div>
          
          <div class="featured-right">
            <div class="demo-preview">
              <div class="preview-header">
                <div class="preview-tabs">
                  <div class="tab active">AI输入</div>
                  <div class="tab">AO输出</div>
                  <div class="tab">数字IO</div>
                </div>
                <div class="preview-status">
                  <div class="status-dot active"></div>
                  <span>模拟模式</span>
                </div>
              </div>
              
              <div class="preview-content">
                <!-- 模拟波形显示 -->
                <div class="waveform-preview">
                  <svg width="280" height="160" viewBox="0 0 280 160">
                    <defs>
                      <linearGradient id="previewGradient" x1="0%" y1="0%" x2="0%" y2="100%">
                        <stop offset="0%" style="stop-color:#667eea;stop-opacity:0.8" />
                        <stop offset="100%" style="stop-color:#667eea;stop-opacity:0.1" />
                      </linearGradient>
                    </defs>
                    <!-- 网格 -->
                    <g stroke="#e0e4e7" stroke-width="0.5" opacity="0.7">
                      <line v-for="i in 9" :key="`h${i}`" :x1="20" :y1="20 + i * 15" :x2="260" :y2="20 + i * 15" />
                      <line v-for="i in 13" :key="`v${i}`" :x1="20 + i * 20" :y1="20" :x2="20 + i * 20" :y2="140" />
                    </g>
                    <!-- 通道1 -->
                    <path d="M20,80 Q60,50 100,80 T180,80 T260,80" fill="none" stroke="#667eea" stroke-width="2"/>
                    <!-- 通道2 -->
                    <path d="M20,100 Q60,120 100,100 T180,100 T260,100" fill="none" stroke="#764ba2" stroke-width="2"/>
                  </svg>
                </div>
                
                <!-- 数字显示 -->
                <div class="digital-readouts">
                  <div class="readout">
                    <span class="readout-label">AI0</span>
                    <span class="readout-value">{{ (Math.sin(Date.now() / 1000) * 5).toFixed(2) }}V</span>
                  </div>
                  <div class="readout">
                    <span class="readout-label">AI1</span>
                    <span class="readout-value">{{ (Math.cos(Date.now() / 1000) * 3).toFixed(2) }}V</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <div class="featured-actions">
          <el-button type="primary" size="large" @click="router.push('/usb1601-demo')">
            <el-icon><VideoPlay /></el-icon>
            立即体验演示
          </el-button>
          <el-button size="large" @click="scrollToModules">
            <el-icon><DataBoard /></el-icon>
            查看技术规格
          </el-button>
          <el-button size="large" @click="openUSB1601Docs">
            <el-icon><Document /></el-icon>
            产品文档
          </el-button>
        </div>
      </div>
    </section>

    <!-- 主要模块 -->
    <section class="modules-section" ref="modulesSection">
      <div class="section-header">
        <h2 class="section-title">核心功能模块</h2>
        <p class="section-subtitle">🚀 AI驱动的智能测试平台 + 60+ 专业控件库 + MISD标准化硬件集成</p>
      </div>
      
      <div class="modules-grid">
        <!-- 前端控件库模块 -->
        <div class="module-card frontend-module">
          <div class="module-header">
            <div class="module-icon">
              <el-icon><Monitor /></el-icon>
            </div>
            <div class="module-info">
              <h3 class="module-title">🎛️ 前端控件库</h3>
              <p class="module-subtitle">60+ 专业测控界面组件</p>
            </div>
          </div>
          
          <div class="module-categories">
            <div 
              v-for="category in frontendCategories" 
              :key="category.id"
              class="category-section"
            >
              <div class="category-header" @click="category.expanded = !category.expanded">
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
                <div class="expand-icon">
                  <el-icon :class="{ rotated: category.expanded }">
                    <ArrowDown />
                  </el-icon>
                </div>
              </div>
              
              <transition name="expand">
                <div v-if="category.expanded" class="subcategory-list">
                  <router-link 
                    v-for="subcategory in category.subcategories" 
                    :key="subcategory.path"
                    :to="subcategory.path"
                    class="subcategory-item"
                  >
                    <div class="subcategory-dot"></div>
                    <span class="subcategory-name">{{ subcategory.name }}</span>
                    <el-icon class="subcategory-arrow"><Right /></el-icon>
                  </router-link>
                </div>
              </transition>
            </div>
          </div>
        </div>

        <!-- 后端集成平台模块 -->
        <div class="module-card backend-module">
          <div class="module-header">
            <div class="module-icon">
              <el-icon><Cpu /></el-icon>
            </div>
            <div class="module-info">
              <h3 class="module-title">🚀 AI智能后端平台</h3>
              <p class="module-subtitle">DeepSeek AI + MISD标准化硬件驱动</p>
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
          <p>完整的API文档和使用指南</p>
          <router-link to="/documentation" class="doc-link">
            <el-button type="primary">
              <el-icon><Document /></el-icon>
              查看文档
            </el-button>
          </router-link>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { 
  DataAnalysis, TrendCharts, Setting, Monitor, Link,
  Warning, Grid, Cpu, DataLine, Timer, ArrowDown, Right, Document,
  Connection, SwitchButton, VideoPlay, DataBoard
} from '@element-plus/icons-vue'

// 路由
const router = useRouter()

// 响应式数据
const animatedValue = ref(1000.0)
const waveformPath = ref('')
const modulesSection = ref<HTMLElement>()

// 前端控件库分类
const frontendCategories = reactive([
  {
    id: 1,
    title: '基础仪表控件',
    description: '专业测控界面基础组件',
    icon: 'DataAnalysis',
    count: 15,
    expanded: false,
    subcategories: [
      { name: '圆形仪表', path: '/instruments' },
      { name: '线性仪表', path: '/linear-gauge-test' },
      { name: '数字显示器', path: '/digital-display-test' },
      { name: '温度计', path: '/thermometer-test' }
    ]
  },
  {
    id: 2,
    title: '指示与控制',
    description: '状态指示和交互控制组件',
    icon: 'Warning',
    count: 12,
    expanded: false,
    subcategories: [
      { name: 'LED指示灯', path: '/indicators' },
      { name: '开关控件', path: '/switch-test' },
      { name: '按钮阵列', path: '/button-array-test' },
      { name: '控制面板', path: '/controls' }
    ]
  },
  {
    id: 3,
    title: '图表可视化',
    description: '实时数据可视化图表组件',
    icon: 'TrendCharts',
    count: 18,
    expanded: false,
    subcategories: [
      { name: 'StripChart实时图表', path: '/enhanced-stripchart-test' },
      { name: 'FFT频谱分析', path: '/spectrum-chart-test' },
      { name: '高级图表', path: '/advanced-easy-chart-test' },
      { name: '专业图表', path: '/professional-easy-chart-test' },
      { name: '双轴图表', path: '/dual-axis-easychart-test' },
      { name: '波形显示', path: '/waveform' }
    ]
  },
  {
    id: 4,
    title: '虚拟仪器',
    description: '专业仪器控制界面组件',
    icon: 'Monitor',
    count: 10,
    expanded: false,
    subcategories: [
      { name: '数字示波器', path: '/oscilloscope-test' },
      { name: '信号发生器', path: '/signal-generator-test' },
      { name: '数字万用表', path: '/digital-multimeter-test' },
      { name: '温度采集卡', path: '/temperature-acquisition-card-test' },
      { name: 'DIO控制卡', path: '/dio-card-test' }
    ]
  }
])

// AI核心模块 - 突出显示
const aiCoreModules = reactive([
  {
    id: 1,
    title: 'AI智能测试平台',
    description: '突破性的AI驱动测试解决方案！用中文描述测试需求，AI自动生成C#代码并直接调用简仪硬件执行测试',
    icon: 'Cpu',
    path: '/ai-test-platform',
    badge: 'NEW',
    progress: 95,
    features: [
      '🎯 DeepSeek AI集成',
      '🔧 智能代码生成', 
      '⚡ 硬件直接调用',
      '📊 实时结果展示'
    ]
  },
  {
    id: 2,
    title: 'C# Runner MCP集成',
    description: 'AI与硬件连接的核心桥梁！基于MCP协议的C#代码执行引擎，实现从AI生成代码到硬件执行的无缝衔接',
    icon: 'Timer',
    path: '/csharp-runner-test',
    badge: 'CORE',
    progress: 95,
    features: [
      '🚀 MCP协议支持',
      '💻 实时代码执行',
      '🔗 设备模板内置',
      '📈 执行状态监控'
    ]
  }
])

// 后端服务架构分类
const backendCategories = reactive([
  {
    id: 1,
    title: '硬件驱动管理',
    description: 'MISD标准化硬件驱动系统，支持JY5500/JYUSB1601',
    icon: 'Setting',
    path: '/hardware-driver-test',
    status: '已实现'
  },
  {
    id: 2,
    title: '数据采集引擎',
    description: '高性能多线程实时数据采集处理',
    icon: 'DataLine',
    path: '/backend-integration-test',
    status: '已实现'
  },
  {
    id: 3,
    title: '数据存储系统',
    description: '海量测试数据存储与历史回放',
    icon: 'Grid',
    path: '/data-storage-test',
    status: '已实现'
  },
  {
    id: 4,
    title: '性能监控平台',
    description: '全方位系统性能监控与运维',
    icon: 'Monitor',
    path: '/performance-monitoring',
    status: '已实现'
  },
  {
    id: 5,
    title: '数据分析服务',
    description: '智能数据分析与处理算法',
    icon: 'DataAnalysis',
    path: '/data-analysis-test',
    status: '已实现'
  },
  {
    id: 6,
    title: 'AI控件生成器',
    description: '基于自然语言的智能UI控件生成',
    icon: 'Setting',
    path: '/ai-control-generator',
    status: '已实现'
  },
  {
    id: 7,
    title: '项目开发者工具',
    description: '完整的开发者辅助工具集',
    icon: 'Setting',
    path: '/project-developer',
    status: '已实现'
  },
  {
    id: 8,
    title: '随机数生成器',
    description: '演示前后端集成的随机数生成应用',
    icon: 'DataAnalysis',
    path: '/random-number',
    status: '已实现'
  }
])

// 核心功能模块（已实现和开发中）
const coreModules = reactive([
  {
    id: 1,
    title: 'AI智能测试平台',
    description: '核心AI驱动测试解决方案，支持自然语言测试需求分析',
    icon: 'Cpu',
    status: '已实现',
    progress: 85,
    path: '/ai-test-platform',
    features: [
      '🎯 中文需求智能分析',
      '🔧 C#测试代码生成',
      '⚡ 硬件直接调用',
      '📊 实时结果展示'
    ],
    badge: 'NEW'
  },
  {
    id: 2,
    title: 'C# Runner集成',
    description: 'MCP协议集成的C#代码执行引擎，连接AI与硬件的桥梁',
    icon: 'Timer',
    status: '已实现',
    progress: 90,
    path: '/csharp-runner-test',
    features: [
      '🚀 MCP协议支持',
      '💻 C#代码实时执行',
      '🔗 硬件驱动调用',
      '📈 执行状态监控'
    ],
    badge: 'CORE'
  },
  {
    id: 3,
    title: '硬件驱动管理',
    description: 'MISD标准化硬件驱动管理系统，支持简仪全系列设备',
    icon: 'Cpu',
    status: '已实现',
    progress: 80,
    path: '/hardware-driver-test',
    features: [
      '📡 JY5500信号发生器',
      '📊 JYUSB1601数据采集',
      '🔄 设备状态监控',
      '⚙️ 驱动自动管理'
    ],
    badge: 'STABLE'
  }
])

// 前端控件库（丰富的UI组件）
const frontendLibrary = reactive([
  {
    id: 1,
    title: '基础仪表控件',
    description: '专业测控界面基础组件库',
    icon: 'DataAnalysis',
    status: '已实现',
    progress: 95,
    path: '/instruments',
    count: '15+',
    items: [
      '圆形仪表', '线性仪表', '数字显示器', 
      'LED指示灯', '开关控件', '按钮阵列'
    ]
  },
  {
    id: 2,
    title: '高性能图表',
    description: '实时数据可视化图表组件',
    icon: 'TrendCharts',
    status: '已实现',
    progress: 90,
    path: '/enhanced-stripchart-test',
    count: '10+',
    items: [
      'StripChart实时图表', 'FFT频谱分析', 
      '波形显示', '双Y轴图表', '专业测量工具'
    ]
  },
  {
    id: 3,
    title: '模块仪器界面',
    description: '专业仪器控制界面组件',
    icon: 'Monitor',
    status: '开发中',
    progress: 70,
    path: '/oscilloscope-test',
    count: '6+',
    items: [
      '数字示波器', '信号发生器', '数字万用表',
      '温度采集卡', 'DIO控制卡'
    ]
  }
])

// 后端服务架构（核心服务支撑）
const backendServices = reactive([
  {
    id: 1,
    title: '数据采集引擎',
    description: '高性能多线程实时数据采集处理',
    icon: 'DataLine',
    status: '已实现',
    progress: 85,
    path: '/backend-integration-test',
    capabilities: [
      '多线程并发采集', 'SignalR实时推送',
      '数据质量检查', '缓冲区优化管理'
    ]
  },
  {
    id: 2,
    title: '数据存储系统',
    description: '海量测试数据存储与历史回放',
    icon: 'DataAnalysis',
    status: '已实现',
    progress: 80,
    path: '/data-storage-test',
    capabilities: [
      '高效数据压缩', '历史数据查询',
      '数据回放功能', '存储优化管理'
    ]
  },
  {
    id: 3,
    title: '系统监控平台',
    description: '全方位系统性能监控与运维',
    icon: 'Monitor',
    status: '已实现',
    progress: 75,
    path: '/performance-monitoring',
    capabilities: [
      'Prometheus指标', '健康状态检查',
      '性能实时监控', '错误日志追踪'
    ]
  }
])

// AI增强功能（创新特色）
const aiFeatures = reactive([
  {
    id: 1,
    title: 'AI控件生成器',
    description: '基于自然语言的智能UI控件生成',
    icon: 'Setting',
    status: '已实现',
    progress: 70,
    path: '/ai-control-generator',
    highlights: [
      '自然语言描述', '智能代码生成',
      '实时预览效果', '一键导出代码'
    ]
  }
])

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

const openUSB1601Docs = () => {
  window.open('/documentation/usb1601', '_blank')
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
          border: none;
          padding: 12px 24px;
          font-size: 16px;
          font-weight: 600;
          transition: all 0.3s ease;
          
          &.primary {
            background: linear-gradient(135deg, var(--primary-color), #A23B72);
            color: white;
            box-shadow: 0 4px 15px rgba(46, 134, 171, 0.3);
            
            &:hover {
              transform: translateY(-2px);
              box-shadow: 0 6px 20px rgba(46, 134, 171, 0.4);
            }
          }
          
          &.secondary {
            background: rgba(46, 134, 171, 0.1);
            color: var(--primary-color);
            border: 2px solid var(--primary-color);
            
            &:hover {
              background: rgba(46, 134, 171, 0.2);
              transform: translateY(-2px);
              box-shadow: 0 4px 15px rgba(46, 134, 171, 0.2);
            }
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

// USB-1601 演示区域样式
.usb1601-featured-section {
  background: linear-gradient(135deg, 
    rgba(102, 126, 234, 0.1) 0%, 
    rgba(118, 75, 162, 0.1) 50%,
    rgba(102, 126, 234, 0.1) 100%
  );
  padding: 80px 0;
  position: relative;
  overflow: hidden;
  
  &::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><defs><pattern id="grid" width="10" height="10" patternUnits="userSpaceOnUse"><path d="M 10 0 L 0 0 0 10" fill="none" stroke="%23667eea" stroke-width="0.5" opacity="0.1"/></pattern></defs><rect width="100" height="100" fill="url(%23grid)"/></svg>');
    pointer-events: none;
  }
}

.featured-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
  position: relative;
  z-index: 1;
}

.featured-header {
  text-align: center;
  margin-bottom: 60px;
}

.featured-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  padding: 8px 20px;
  border-radius: 50px;
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 20px;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
  
  .badge-icon {
    font-size: 16px;
  }
}

.featured-title {
  font-size: 36px;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 16px 0;
  line-height: 1.2;
  
  @media (max-width: 768px) {
    font-size: 28px;
  }
}

.featured-subtitle {
  font-size: 18px;
  color: #666;
  line-height: 1.6;
  max-width: 800px;
  margin: 0 auto;
  
  @media (max-width: 768px) {
    font-size: 16px;
  }
}

.featured-body {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 60px;
  align-items: center;
  margin-bottom: 60px;
  
  @media (max-width: 1024px) {
    grid-template-columns: 1fr;
    gap: 40px;
  }
}

.featured-left {
  .feature-highlights {
    margin-bottom: 40px;
  }
  
  .highlight-item {
    display: flex;
    align-items: flex-start;
    gap: 16px;
    margin-bottom: 24px;
    padding: 20px;
    background: rgba(255, 255, 255, 0.8);
    backdrop-filter: blur(10px);
    border-radius: 12px;
    border: 1px solid rgba(102, 126, 234, 0.1);
    transition: all 0.3s ease;
    
    &:hover {
      transform: translateX(8px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
      border-color: rgba(102, 126, 234, 0.3);
    }
  }
  
  .highlight-icon {
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 48px;
    height: 48px;
    background: linear-gradient(135deg, #667eea, #764ba2);
    border-radius: 12px;
    color: white;
    
    .el-icon {
      font-size: 24px;
    }
  }
  
  .highlight-content {
    h4 {
      margin: 0 0 4px 0;
      font-size: 16px;
      font-weight: 600;
      color: #333;
    }
    
    p {
      margin: 0;
      color: #666;
      font-size: 14px;
      line-height: 1.4;
    }
  }
}

.featured-stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  
  @media (max-width: 768px) {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .stat-item {
    background: rgba(255, 255, 255, 0.9);
    backdrop-filter: blur(10px);
    padding: 20px;
    border-radius: 12px;
    text-align: center;
    border: 1px solid rgba(102, 126, 234, 0.1);
    
    .stat-value {
      display: block;
      font-size: 20px;
      font-weight: 700;
      color: #667eea;
      margin-bottom: 4px;
    }
    
    .stat-label {
      display: block;
      font-size: 12px;
      color: #999;
      font-weight: 500;
    }
  }
}

.featured-right {
  display: flex;
  justify-content: center;
  
  @media (max-width: 1024px) {
    order: -1;
  }
}

.demo-preview {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  border: 1px solid rgba(102, 126, 234, 0.2);
  width: 100%;
  max-width: 400px;
}

.preview-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  
  .preview-tabs {
    display: flex;
    gap: 4px;
    
    .tab {
      padding: 6px 12px;
      font-size: 12px;
      font-weight: 500;
      color: #666;
      background: #f5f5f5;
      border-radius: 6px;
      cursor: pointer;
      transition: all 0.2s ease;
      
      &.active {
        background: linear-gradient(135deg, #667eea, #764ba2);
        color: white;
      }
    }
  }
  
  .preview-status {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 12px;
    color: #666;
    
    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: #ccc;
      
      &.active {
        background: #4CAF50;
        animation: pulse 2s infinite;
      }
    }
  }
}

.preview-content {
  .waveform-preview {
    margin-bottom: 16px;
    background: #fafafa;
    border-radius: 8px;
    padding: 12px;
  }
  
  .digital-readouts {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
    
    .readout {
      background: #1a1a1a;
      color: #00ff00;
      padding: 12px;
      border-radius: 6px;
      font-family: 'Courier New', monospace;
      display: flex;
      justify-content: space-between;
      align-items: center;
      
      .readout-label {
        font-size: 12px;
        opacity: 0.8;
      }
      
      .readout-value {
        font-size: 16px;
        font-weight: bold;
      }
    }
  }
}

.featured-actions {
  display: flex;
  justify-content: center;
  gap: 16px;
  flex-wrap: wrap;
  
  @media (max-width: 768px) {
    .el-button {
      flex: 1;
      min-width: 160px;
    }
  }
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
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
      
      .category-section {
        border: 1px solid rgba(46, 134, 171, 0.1);
        border-radius: 12px;
        overflow: hidden;
        transition: all 0.3s ease;
        
        &:hover {
          border-color: rgba(46, 134, 171, 0.2);
          box-shadow: 0 2px 8px rgba(46, 134, 171, 0.1);
        }
        
        .category-header {
          display: flex;
          align-items: center;
          gap: 16px;
          padding: 16px;
          background: rgba(46, 134, 171, 0.05);
          cursor: pointer;
          transition: all 0.3s ease;
          
          &:hover {
            background: rgba(46, 134, 171, 0.1);
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
          }
          
          .expand-icon {
            .el-icon {
              font-size: 16px;
              color: var(--text-secondary);
              transition: transform 0.3s ease;
              
              &.rotated {
                transform: rotate(180deg);
              }
            }
          }
        }
        
        .subcategory-list {
          background: rgba(46, 134, 171, 0.02);
          border-top: 1px solid rgba(46, 134, 171, 0.1);
          
          .subcategory-item {
            display: flex;
            align-items: center;
            gap: 12px;
            padding: 12px 20px;
            text-decoration: none;
            color: var(--text-primary);
            transition: all 0.2s ease;
            border-bottom: 1px solid rgba(46, 134, 171, 0.05);
            
            &:last-child {
              border-bottom: none;
            }
            
            &:hover {
              background: rgba(46, 134, 171, 0.08);
              transform: translateX(4px);
            }
            
            .subcategory-dot {
              width: 6px;
              height: 6px;
              background: var(--primary-color);
              border-radius: 50%;
              flex-shrink: 0;
            }
            
            .subcategory-name {
              flex: 1;
              font-size: 14px;
              font-weight: 500;
            }
            
            .subcategory-arrow {
              font-size: 12px;
              color: var(--text-secondary);
              opacity: 0;
              transition: opacity 0.2s ease;
            }
            
            &:hover .subcategory-arrow {
              opacity: 1;
            }
          }
        }
      }
      
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

// 展开/收起动画
.expand-enter-active,
.expand-leave-active {
  transition: all 0.3s ease;
  overflow: hidden;
}

.expand-enter-from,
.expand-leave-to {
  max-height: 0;
  opacity: 0;
}

.expand-enter-to,
.expand-leave-from {
  max-height: 500px;
  opacity: 1;
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
