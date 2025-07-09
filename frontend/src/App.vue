<template>
  <div id="app" class="instrument-app">
    <!-- 顶部导航栏 -->
    <header class="app-header">
      <div class="header-content">
        <div class="logo-section">
          <div class="logo">
            <el-icon class="logo-icon"><Monitor /></el-icon>
            <span class="logo-text">测试测量仪器控件库</span>
          </div>
          <div class="subtitle">Professional Test & Measurement Instrument Controls</div>
        </div>
        
        <nav class="main-nav">
          <router-link to="/" class="nav-item" active-class="active">
            <el-icon><House /></el-icon>
            <span>首页</span>
          </router-link>
          
          <!-- 基础控件 -->
          <el-dropdown class="nav-dropdown" trigger="hover">
            <div class="nav-item dropdown-trigger">
              <el-icon><DataAnalysis /></el-icon>
              <span>基础控件</span>
              <el-icon class="dropdown-icon"><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item>
                  <router-link to="/instruments" class="dropdown-link">
                    <el-icon><DataAnalysis /></el-icon>
                    <span>仪表控件</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/indicators" class="dropdown-link">
                    <el-icon><Warning /></el-icon>
                    <span>指示控件</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/controls" class="dropdown-link">
                    <el-icon><Setting /></el-icon>
                    <span>控制控件</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/digital-display-test" class="dropdown-link">
                    <el-icon><Monitor /></el-icon>
                    <span>数字显示器</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/switch-test" class="dropdown-link">
                    <el-icon><Setting /></el-icon>
                    <span>开关测试</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/button-array-test" class="dropdown-link">
                    <el-icon><Setting /></el-icon>
                    <span>按钮阵列</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/linear-gauge-test" class="dropdown-link">
                    <el-icon><DataAnalysis /></el-icon>
                    <span>线性仪表</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/thermometer-test" class="dropdown-link">
                    <el-icon><DataAnalysis /></el-icon>
                    <span>温度计</span>
                  </router-link>
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
          
          <!-- 高性能图表 -->
          <el-dropdown class="nav-dropdown" trigger="hover">
            <div class="nav-item dropdown-trigger">
              <el-icon><TrendCharts /></el-icon>
              <span>高性能图表</span>
              <el-icon class="dropdown-icon"><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item>
                  <router-link to="/enhanced-stripchart-test" class="dropdown-link">
                    <el-icon><TrendCharts /></el-icon>
                    <span>高性能StripChart</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/spectrum-chart-test" class="dropdown-link">
                    <el-icon><DataLine /></el-icon>
                    <span>FFT频谱分析</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/spectrum" class="dropdown-link">
                    <el-icon><DataLine /></el-icon>
                    <span>频谱分析仪</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/advanced-easy-chart-test" class="dropdown-link">
                    <el-icon><DataAnalysis /></el-icon>
                    <span>高级数学分析</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/professional-easy-chart-test" class="dropdown-link">
                    <el-icon><Monitor /></el-icon>
                    <span>专业测量工具</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/dual-axis-easychart-test" class="dropdown-link">
                    <el-icon><DataAnalysis /></el-icon>
                    <span>双Y轴图表</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/waveform" class="dropdown-link">
                    <el-icon><TrendCharts /></el-icon>
                    <span>波形图表</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/stripchart" class="dropdown-link">
                    <el-icon><TrendCharts /></el-icon>
                    <span>条带图</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/enhanced-charts" class="dropdown-link">
                    <el-icon><DataAnalysis /></el-icon>
                    <span>增强图表</span>
                  </router-link>
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
          
          <!-- 模块仪器 -->
          <el-dropdown class="nav-dropdown" trigger="hover">
            <div class="nav-item dropdown-trigger">
              <el-icon><Monitor /></el-icon>
              <span>模块仪器</span>
              <el-icon class="dropdown-icon"><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item>
                  <router-link to="/oscilloscope-test" class="dropdown-link">
                    <el-icon><TrendCharts /></el-icon>
                    <span>数字示波器</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/digital-multimeter-test" class="dropdown-link">
                    <el-icon><Monitor /></el-icon>
                    <span>数字万用表</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/signal-generator-test" class="dropdown-link">
                    <el-icon><Timer /></el-icon>
                    <span>信号发生器</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/temperature-acquisition-card-test" class="dropdown-link">
                    <el-icon><Monitor /></el-icon>
                    <span>温度采集卡</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/dio-card-test" class="dropdown-link">
                    <el-icon><Grid /></el-icon>
                    <span>DIO控制卡</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/data-acquisition" class="dropdown-link">
                    <el-icon><DataAnalysis /></el-icon>
                    <span>数据采集卡</span>
                  </router-link>
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
          
          <!-- 后端集成 -->
          <el-dropdown class="nav-dropdown" trigger="hover">
            <div class="nav-item dropdown-trigger">
              <el-icon><Cpu /></el-icon>
              <span>后端集成</span>
              <el-icon class="dropdown-icon"><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item>
                  <router-link to="/backend-integration-test" class="dropdown-link">
                    <el-icon><Link /></el-icon>
                    <span>前后端集成</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/data-storage-test" class="dropdown-link">
                    <el-icon><DataAnalysis /></el-icon>
                    <span>数据存储回放</span>
                  </router-link>
                </el-dropdown-item>
                <el-dropdown-item>
                  <router-link to="/ai-control-generator-test" class="dropdown-link">
                    <el-icon><Cpu /></el-icon>
                    <span>AI智能生成</span>
                  </router-link>
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
          
          <router-link to="/examples" class="nav-item" active-class="active">
            <el-icon><Grid /></el-icon>
            <span>示例</span>
          </router-link>
          
          <router-link to="/documentation" class="nav-item" active-class="active">
            <el-icon><Document /></el-icon>
            <span>文档</span>
          </router-link>
        </nav>
        
        <div class="header-actions">
          <el-tooltip content="GitHub仓库">
            <el-button circle @click="openGitHub">
              <el-icon><Link /></el-icon>
            </el-button>
          </el-tooltip>
          <el-tooltip content="文档">
            <el-button circle @click="openDocs">
              <el-icon><Document /></el-icon>
            </el-button>
          </el-tooltip>
        </div>
      </div>
    </header>

    <!-- 主内容区域 -->
    <main class="app-main">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>

    <!-- 底部信息 -->
    <footer class="app-footer">
      <div class="footer-content">
        <div class="footer-left">
          <span>基于 Vue 3 + TypeScript + ECharts</span>
          <span class="separator">|</span>
          <span>参考 SeeSharpTools 设计风格</span>
        </div>
        <div class="footer-right">
          <span>© 2025 开源测控专业控件</span>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { 
  Monitor, House, DataAnalysis, Warning, Setting, 
  TrendCharts, DataLine, Grid, Link, Document,
  Timer, Cpu, ArrowDown
} from '@element-plus/icons-vue'

const openGitHub = () => {
  window.open('https://github.com/wukeping2008/seesharptools-web', '_blank')
}

const openDocs = () => {
  window.open('/docs', '_blank')
}
</script>

<style lang="scss" scoped>
.instrument-app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
}

.app-header {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(46, 134, 171, 0.2);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 1000;

  .header-content {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 24px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: 70px;
  }

  .logo-section {
    display: flex;
    flex-direction: column;
    gap: 2px;

    .logo {
      display: flex;
      align-items: center;
      gap: 12px;
      
      .logo-icon {
        font-size: 28px;
        color: #2E86AB;
      }
      
      .logo-text {
        font-size: 20px;
        font-weight: 700;
        color: #1F2937;
        letter-spacing: -0.5px;
      }
    }

    .subtitle {
      font-size: 12px;
      color: #6B7280;
      margin-left: 40px;
      font-style: italic;
    }
  }

  .main-nav {
    display: flex;
    gap: 8px;

    .nav-item {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 8px 16px;
      border-radius: 8px;
      text-decoration: none;
      color: #4B5563;
      font-weight: 500;
      font-size: 14px;
      transition: all 0.2s ease;

      &:hover {
        background: rgba(46, 134, 171, 0.1);
        color: #2E86AB;
      }

      &.active {
        background: #2E86AB;
        color: white;
        box-shadow: 0 2px 4px rgba(46, 134, 171, 0.3);
      }

      .el-icon {
        font-size: 16px;
      }
    }

    .nav-dropdown {
      .dropdown-trigger {
        display: flex;
        align-items: center;
        gap: 6px;
        padding: 8px 16px;
        border-radius: 8px;
        color: #4B5563;
        font-weight: 500;
        font-size: 14px;
        transition: all 0.2s ease;
        cursor: pointer;

        &:hover {
          background: rgba(46, 134, 171, 0.1);
          color: #2E86AB;
        }

        .dropdown-icon {
          font-size: 12px;
          transition: transform 0.2s ease;
        }

        &:hover .dropdown-icon {
          transform: rotate(180deg);
        }
      }
    }
  }

  .header-actions {
    display: flex;
    gap: 8px;

    .el-button {
      border: 1px solid rgba(46, 134, 171, 0.3);
      color: #2E86AB;

      &:hover {
        background: #2E86AB;
        color: white;
        border-color: #2E86AB;
      }
    }
  }
}

.app-main {
  flex: 1;
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
  width: 100%;
  box-sizing: border-box;
}

.app-footer {
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(10px);
  border-top: 1px solid rgba(46, 134, 171, 0.2);
  padding: 16px 0;

  .footer-content {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 24px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 13px;
    color: #6B7280;

    .footer-left {
      display: flex;
      align-items: center;
      gap: 12px;

      .separator {
        color: #D1D5DB;
      }
    }

    .footer-right {
      font-weight: 500;
    }
  }
}

// 页面切换动画
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

// 下拉菜单样式
:deep(.el-dropdown-menu) {
  border: 1px solid rgba(46, 134, 171, 0.2);
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  padding: 4px 0;

  .el-dropdown-menu__item {
    padding: 0;

    .dropdown-link {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 16px;
      text-decoration: none;
      color: #4B5563;
      font-size: 14px;
      transition: all 0.2s ease;
      width: 100%;

      &:hover {
        background: rgba(46, 134, 171, 0.1);
        color: #2E86AB;
      }

      .el-icon {
        font-size: 16px;
      }
    }

    &:hover {
      background: transparent;
    }
  }
}

// 响应式设计
@media (max-width: 1200px) {
  .app-header {
    .header-content {
      padding: 0 16px;
    }

    .main-nav {
      gap: 4px;

      .nav-item {
        padding: 6px 12px;
        font-size: 13px;

        span {
          display: none;
        }
      }
    }

    .logo-section {
      .logo-text {
        font-size: 18px;
      }

      .subtitle {
        font-size: 11px;
      }
    }
  }

  .app-main {
    padding: 16px;
  }
}

@media (max-width: 768px) {
  .app-header {
    .header-content {
      height: 60px;
      flex-wrap: wrap;
      gap: 12px;
    }

    .logo-section {
      .subtitle {
        display: none;
      }
    }

    .main-nav {
      order: 3;
      width: 100%;
      justify-content: center;
      padding-top: 8px;
      border-top: 1px solid rgba(46, 134, 171, 0.1);

      .nav-item {
        flex-direction: column;
        gap: 2px;
        padding: 4px 8px;
        font-size: 11px;

        span {
          display: block;
        }

        .el-icon {
          font-size: 14px;
        }
      }
    }
  }

  .app-footer {
    .footer-content {
      flex-direction: column;
      gap: 8px;
      text-align: center;

      .footer-left {
        flex-direction: column;
        gap: 4px;

        .separator {
          display: none;
        }
      }
    }
  }
}
</style>
