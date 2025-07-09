<template>
  <div class="indicators-view">
    <div class="page-header">
      <h1 class="page-title">指示控件</h1>
      <p class="page-subtitle">专业状态指示和显示控件，适用于监控和报警系统</p>
    </div>

    <div class="indicators-grid">
      <!-- LED指示器 -->
      <div class="indicator-card">
        <div class="card-header">
          <h3 class="card-title">LED指示器</h3>
          <p class="card-description">多种颜色和状态的LED指示灯</p>
        </div>
        <div class="card-content">
          <div class="led-demo">
            <div class="led-group">
              <div class="led-item">
                <div class="led-indicator led-green active"></div>
                <span>正常</span>
              </div>
              <div class="led-item">
                <div class="led-indicator led-yellow active"></div>
                <span>警告</span>
              </div>
              <div class="led-item">
                <div class="led-indicator led-red active"></div>
                <span>故障</span>
              </div>
              <div class="led-item">
                <div class="led-indicator led-blue active"></div>
                <span>运行</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 数字显示器 -->
      <div class="indicator-card">
        <div class="card-header">
          <h3 class="card-title">数字显示器</h3>
          <p class="card-description">高精度数值显示控件</p>
        </div>
        <div class="card-content">
          <div class="digital-display-demo">
            <div class="digital-display">
              <div class="display-value">{{ currentValue.toFixed(3) }}</div>
              <div class="display-unit">V</div>
            </div>
            <div class="display-status">
              <div class="status-indicator status-normal">
                <div class="status-dot"></div>
                <span>正常</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 状态面板 -->
      <div class="indicator-card">
        <div class="card-header">
          <h3 class="card-title">状态面板</h3>
          <p class="card-description">系统状态综合显示面板</p>
        </div>
        <div class="card-content">
          <div class="status-panel">
            <div class="status-row">
              <span class="status-label">电源状态</span>
              <div class="status-value status-ok">正常</div>
            </div>
            <div class="status-row">
              <span class="status-label">通信状态</span>
              <div class="status-value status-ok">连接</div>
            </div>
            <div class="status-row">
              <span class="status-label">温度状态</span>
              <div class="status-value status-warning">偏高</div>
            </div>
            <div class="status-row">
              <span class="status-label">系统状态</span>
              <div class="status-value status-ok">运行</div>
            </div>
          </div>
        </div>
      </div>

      <!-- 进度指示器 -->
      <div class="indicator-card">
        <div class="card-header">
          <h3 class="card-title">进度指示器</h3>
          <p class="card-description">任务进度和完成状态显示</p>
        </div>
        <div class="card-content">
          <div class="progress-demo">
            <div class="progress-item">
              <div class="progress-label">数据采集</div>
              <div class="progress-bar">
                <div class="progress-fill" :style="{ width: progress1 + '%' }"></div>
              </div>
              <div class="progress-text">{{ progress1 }}%</div>
            </div>
            <div class="progress-item">
              <div class="progress-label">信号处理</div>
              <div class="progress-bar">
                <div class="progress-fill" :style="{ width: progress2 + '%' }"></div>
              </div>
              <div class="progress-text">{{ progress2 }}%</div>
            </div>
          </div>
        </div>
      </div>

      <!-- 报警指示器 -->
      <div class="indicator-card">
        <div class="card-header">
          <h3 class="card-title">报警指示器</h3>
          <p class="card-description">多级报警状态指示</p>
        </div>
        <div class="card-content">
          <div class="alarm-demo">
            <div class="alarm-item">
              <div class="alarm-icon alarm-info">
                <el-icon><InfoFilled /></el-icon>
              </div>
              <div class="alarm-content">
                <div class="alarm-title">信息提示</div>
                <div class="alarm-message">系统运行正常</div>
              </div>
            </div>
            <div class="alarm-item">
              <div class="alarm-icon alarm-warning">
                <el-icon><WarningFilled /></el-icon>
              </div>
              <div class="alarm-content">
                <div class="alarm-title">警告</div>
                <div class="alarm-message">温度超过阈值</div>
              </div>
            </div>
            <div class="alarm-item">
              <div class="alarm-icon alarm-error">
                <el-icon><CircleCloseFilled /></el-icon>
              </div>
              <div class="alarm-content">
                <div class="alarm-title">错误</div>
                <div class="alarm-message">通信连接中断</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 连接状态指示器 -->
      <div class="indicator-card">
        <div class="card-header">
          <h3 class="card-title">连接状态</h3>
          <p class="card-description">设备连接和通信状态指示</p>
        </div>
        <div class="card-content">
          <div class="connection-demo">
            <div class="connection-item">
              <div class="connection-icon connected">
                <el-icon><Link /></el-icon>
              </div>
              <div class="connection-info">
                <div class="connection-name">设备 #001</div>
                <div class="connection-status">已连接</div>
              </div>
              <div class="connection-signal">
                <div class="signal-bar active"></div>
                <div class="signal-bar active"></div>
                <div class="signal-bar active"></div>
                <div class="signal-bar"></div>
              </div>
            </div>
            <div class="connection-item">
              <div class="connection-icon disconnected">
                <el-icon><Close /></el-icon>
              </div>
              <div class="connection-info">
                <div class="connection-name">设备 #002</div>
                <div class="connection-status">连接中断</div>
              </div>
              <div class="connection-signal">
                <div class="signal-bar"></div>
                <div class="signal-bar"></div>
                <div class="signal-bar"></div>
                <div class="signal-bar"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { InfoFilled, WarningFilled, CircleCloseFilled, Link, Close } from '@element-plus/icons-vue'

// 响应式数据
const currentValue = ref(12.345)
const progress1 = ref(75)
const progress2 = ref(45)

// 动画定时器
let animationTimer: number | null = null

// 启动动画
const startAnimations = () => {
  animationTimer = setInterval(() => {
    // 数字显示器动画
    currentValue.value = 12 + Math.sin(Date.now() / 1000) * 2 + Math.random() * 0.1
    
    // 进度条动画
    progress1.value = 75 + Math.sin(Date.now() / 2000) * 20
    progress2.value = 45 + Math.cos(Date.now() / 1500) * 30
  }, 100)
}

// 停止动画
const stopAnimations = () => {
  if (animationTimer) {
    clearInterval(animationTimer)
    animationTimer = null
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
@import "@/styles/variables.scss";

.indicators-view {
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  text-align: center;
  margin-bottom: 48px;
  
  .page-title {
    font-size: 36px;
    font-weight: 700;
    color: var(--text-primary);
    margin-bottom: 12px;
  }
  
  .page-subtitle {
    font-size: 18px;
    color: var(--text-secondary);
    line-height: 1.6;
  }
}

.indicators-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 24px;
}

.indicator-card {
  background: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 24px;
  transition: var(--transition-base);

  &:hover {
    box-shadow: var(--shadow-medium);
    transform: translateY(-2px);
  }

  .card-header {
    margin-bottom: 20px;

    .card-title {
      font-size: 18px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 8px;
    }

    .card-description {
      font-size: 14px;
      color: var(--text-secondary);
      line-height: 1.5;
    }
  }

  .card-content {
    min-height: 120px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}

// LED指示器样式
.led-demo {
  width: 100%;

  .led-group {
    display: flex;
    justify-content: space-around;
    align-items: center;
  }

  .led-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;

    span {
      font-size: 12px;
      color: var(--text-secondary);
    }
  }

  .led-indicator {
    width: 16px;
    height: 16px;
    border-radius: 50%;
    border: 2px solid transparent;
    position: relative;

    &.active {
      box-shadow: 0 0 10px currentColor;
      
      &::before {
        content: '';
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        width: 8px;
        height: 8px;
        border-radius: 50%;
        background: currentColor;
        animation: pulse 2s infinite;
      }
    }

    &.led-green {
      background: #10B981;
      border-color: #10B981;
      color: #10B981;
    }

    &.led-yellow {
      background: #F59E0B;
      border-color: #F59E0B;
      color: #F59E0B;
    }

    &.led-red {
      background: #EF4444;
      border-color: #EF4444;
      color: #EF4444;
    }

    &.led-blue {
      background: #3B82F6;
      border-color: #3B82F6;
      color: #3B82F6;
    }
  }
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

// 数字显示器样式
.digital-display-demo {
  width: 100%;
  text-align: center;

  .digital-display {
    background: var(--digital-bg);
    border: 2px solid var(--border-color);
    border-radius: 8px;
    padding: 16px;
    margin-bottom: 12px;
    display: flex;
    align-items: baseline;
    justify-content: center;
    gap: 8px;

    .display-value {
      font-family: var(--font-family-mono);
      font-size: 32px;
      font-weight: bold;
      color: var(--digital-value);
      line-height: 1;
    }

    .display-unit {
      font-size: 16px;
      color: var(--digital-unit);
    }
  }

  .display-status {
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

      .status-dot {
        width: 8px;
        height: 8px;
        border-radius: 50%;
        background: currentColor;
      }
    }
  }
}

// 状态面板样式
.status-panel {
  width: 100%;

  .status-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 0;
    border-bottom: 1px solid var(--border-color);

    &:last-child {
      border-bottom: none;
    }

    .status-label {
      font-size: 14px;
      color: var(--text-secondary);
    }

    .status-value {
      font-size: 14px;
      font-weight: 500;
      padding: 2px 8px;
      border-radius: 4px;

      &.status-ok {
        background: rgba(16, 185, 129, 0.1);
        color: #10B981;
      }

      &.status-warning {
        background: rgba(245, 158, 11, 0.1);
        color: #F59E0B;
      }

      &.status-error {
        background: rgba(239, 68, 68, 0.1);
        color: #EF4444;
      }
    }
  }
}

// 进度指示器样式
.progress-demo {
  width: 100%;

  .progress-item {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 16px;

    &:last-child {
      margin-bottom: 0;
    }

    .progress-label {
      font-size: 14px;
      color: var(--text-secondary);
      min-width: 80px;
    }

    .progress-bar {
      flex: 1;
      height: 8px;
      background: var(--border-color);
      border-radius: 4px;
      overflow: hidden;

      .progress-fill {
        height: 100%;
        background: linear-gradient(90deg, var(--primary-color), var(--primary-light));
        border-radius: 4px;
        transition: width 0.3s ease;
      }
    }

    .progress-text {
      font-size: 12px;
      color: var(--text-secondary);
      min-width: 40px;
      text-align: right;
    }
  }
}

// 报警指示器样式
.alarm-demo {
  width: 100%;

  .alarm-item {
    display: flex;
    align-items: flex-start;
    gap: 12px;
    margin-bottom: 16px;

    &:last-child {
      margin-bottom: 0;
    }

    .alarm-icon {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 14px;

      &.alarm-info {
        background: rgba(59, 130, 246, 0.1);
        color: #3B82F6;
      }

      &.alarm-warning {
        background: rgba(245, 158, 11, 0.1);
        color: #F59E0B;
      }

      &.alarm-error {
        background: rgba(239, 68, 68, 0.1);
        color: #EF4444;
      }
    }

    .alarm-content {
      flex: 1;

      .alarm-title {
        font-size: 14px;
        font-weight: 500;
        color: var(--text-primary);
        margin-bottom: 2px;
      }

      .alarm-message {
        font-size: 12px;
        color: var(--text-secondary);
        line-height: 1.4;
      }
    }
  }
}

// 连接状态样式
.connection-demo {
  width: 100%;

  .connection-item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 12px;
    border: 1px solid var(--border-color);
    border-radius: 8px;
    margin-bottom: 12px;

    &:last-child {
      margin-bottom: 0;
    }

    .connection-icon {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 16px;

      &.connected {
        background: rgba(16, 185, 129, 0.1);
        color: #10B981;
      }

      &.disconnected {
        background: rgba(239, 68, 68, 0.1);
        color: #EF4444;
      }
    }

    .connection-info {
      flex: 1;

      .connection-name {
        font-size: 14px;
        font-weight: 500;
        color: var(--text-primary);
        margin-bottom: 2px;
      }

      .connection-status {
        font-size: 12px;
        color: var(--text-secondary);
      }
    }

    .connection-signal {
      display: flex;
      gap: 2px;
      align-items: flex-end;

      .signal-bar {
        width: 4px;
        background: var(--border-color);
        border-radius: 2px;

        &:nth-child(1) { height: 8px; }
        &:nth-child(2) { height: 12px; }
        &:nth-child(3) { height: 16px; }
        &:nth-child(4) { height: 20px; }

        &.active {
          background: #10B981;
        }
      }
    }
  }
}

// 响应式设计
@media (max-width: 768px) {
  .indicators-grid {
    grid-template-columns: 1fr;
  }

  .indicator-card {
    padding: 16px;
  }

  .page-header {
    .page-title {
      font-size: 28px;
    }

    .page-subtitle {
      font-size: 16px;
    }
  }
}
</style>
