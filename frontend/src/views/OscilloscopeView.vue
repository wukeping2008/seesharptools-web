<template>
  <div class="oscilloscope-view">
    <div class="page-header">
      <h1 class="page-title">数字示波器</h1>
      <p class="page-subtitle">专业级数字示波器控件，支持多通道波形显示、触发和测量功能</p>
    </div>

    <!-- 示波器演示 -->
    <section class="demo-section">
      <div class="section-header">
        <h2 class="section-title">示波器控件演示</h2>
        <p class="section-description">
          完整的示波器功能，包括多通道显示、触发控制、自动测量等
        </p>
      </div>

      <div class="demo-container">
        <Oscilloscope
          @running-change="handleRunningChange"
          @trigger-event="handleTriggerEvent"
          @measurement-update="handleMeasurementUpdate"
        />
      </div>
    </section>

    <!-- 功能特性说明 -->
    <section class="features-section">
      <div class="section-header">
        <h2 class="section-title">功能特性</h2>
      </div>

      <div class="features-grid">
        <div class="feature-item">
          <div class="feature-icon">📊</div>
          <h4>多通道显示</h4>
          <p>支持4通道同步显示，每通道独立配置电压刻度、位置和耦合方式</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">⚡</div>
          <h4>触发系统</h4>
          <p>边沿、脉宽、视频触发，支持上升沿、下降沿和双边沿触发</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">📏</div>
          <h4>自动测量</h4>
          <p>频率、周期、幅度、RMS、上升时间、下降时间、占空比等11种测量</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">⏱️</div>
          <h4>时基控制</h4>
          <p>1ns/div到10s/div时基范围，支持正常、滚动、XY显示模式</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">🎯</div>
          <h4>采集控制</h4>
          <p>采样、平均、包络采集模式，1K到1M采集深度，1MS/s到1GS/s采样率</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">🔧</div>
          <h4>专业配置</h4>
          <p>带宽限制、探头倍数、反相显示等专业示波器功能</p>
        </div>
      </div>
    </section>

    <!-- 技术规格 -->
    <section class="specs-section">
      <div class="section-header">
        <h2 class="section-title">技术规格</h2>
      </div>

      <div class="specs-grid">
        <div class="spec-category">
          <h4>通道特性</h4>
          <div class="spec-items">
            <div class="spec-item">
              <span class="spec-label">通道数量:</span>
              <span class="spec-value">4通道</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">垂直分辨率:</span>
              <span class="spec-value">8位</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">电压范围:</span>
              <span class="spec-value">1mV/div - 10V/div</span>
            </div>
          </div>
        </div>

        <div class="spec-category">
          <h4>时基特性</h4>
          <div class="spec-items">
            <div class="spec-item">
              <span class="spec-label">时基范围:</span>
              <span class="spec-value">1ns/div - 10s/div</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">采样率:</span>
              <span class="spec-value">1MS/s - 1GS/s</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">记录长度:</span>
              <span class="spec-value">1K - 1M点</span>
            </div>
          </div>
        </div>

        <div class="spec-category">
          <h4>触发特性</h4>
          <div class="spec-items">
            <div class="spec-item">
              <span class="spec-label">触发类型:</span>
              <span class="spec-value">边沿/脉宽/视频</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">触发源:</span>
              <span class="spec-value">CH1-4/外部</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">触发电平:</span>
              <span class="spec-value">±10V</span>
            </div>
          </div>
        </div>

        <div class="spec-category">
          <h4>测量功能</h4>
          <div class="spec-items">
            <div class="spec-item">
              <span class="spec-label">自动测量:</span>
              <span class="spec-value">11种参数</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">游标测量:</span>
              <span class="spec-value">时间/电压</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">统计功能:</span>
              <span class="spec-value">最值/平均/标准差</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 状态监控 -->
    <section class="monitoring-section" v-if="currentStatus">
      <div class="section-header">
        <h2 class="section-title">实时状态监控</h2>
      </div>

      <div class="monitoring-grid">
        <div class="monitor-card">
          <h4>运行状态</h4>
          <div class="monitor-value" :class="currentStatus.running ? 'active' : 'inactive'">
            {{ currentStatus.running ? '运行中' : '已停止' }}
          </div>
        </div>

        <div class="monitor-card">
          <h4>触发状态</h4>
          <div class="monitor-value">{{ currentStatus.triggerStatus || '已停止' }}</div>
        </div>

        <div class="monitor-card">
          <h4>采样率</h4>
          <div class="monitor-value">{{ currentStatus.sampleRate || '100MS/s' }}</div>
        </div>

        <div class="monitor-card">
          <h4>时基</h4>
          <div class="monitor-value">{{ currentStatus.timebase || '1ms/div' }}</div>
        </div>

        <div class="monitor-card">
          <h4>活动通道</h4>
          <div class="monitor-value">{{ currentStatus.activeChannels || 'CH1, CH2' }}</div>
        </div>

        <div class="monitor-card">
          <h4>测量数量</h4>
          <div class="monitor-value">{{ currentStatus.measurementCount || 0 }}</div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import Oscilloscope from '@/components/instruments/Oscilloscope.vue'

// 响应式数据
const currentStatus = ref<any>({
  running: false,
  triggerStatus: '已停止',
  sampleRate: '100MS/s',
  timebase: '1ms/div',
  activeChannels: 'CH1, CH2',
  measurementCount: 0
})

// 事件处理
const handleRunningChange = (running: boolean) => {
  currentStatus.value.running = running
  console.log('示波器运行状态变化:', running)
}

const handleTriggerEvent = (triggerInfo: any) => {
  console.log('触发事件:', triggerInfo)
}

const handleMeasurementUpdate = (measurements: any[]) => {
  currentStatus.value.measurementCount = measurements.length
  console.log('测量更新:', measurements)
}
</script>

<style lang="scss" scoped>
.oscilloscope-view {
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

.demo-section,
.features-section,
.specs-section,
.monitoring-section {
  margin-bottom: 48px;
  
  .section-header {
    margin-bottom: 24px;
    
    .section-title {
      font-size: 24px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 8px;
    }
    
    .section-description {
      font-size: 16px;
      color: var(--text-secondary);
      line-height: 1.6;
    }
  }
}

.demo-container {
  background: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 24px;
  
  .feature-item {
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 12px;
    padding: 24px;
    text-align: center;
    transition: all 0.3s ease;
    
    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
    }
    
    .feature-icon {
      font-size: 32px;
      margin-bottom: 16px;
    }
    
    h4 {
      font-size: 16px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 8px;
    }
    
    p {
      font-size: 14px;
      color: var(--text-secondary);
      line-height: 1.5;
      margin: 0;
    }
  }
}

.specs-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 24px;
  
  .spec-category {
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 12px;
    padding: 20px;
    
    h4 {
      font-size: 16px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 16px;
      padding-bottom: 8px;
      border-bottom: 1px solid var(--border-color);
    }
    
    .spec-items {
      .spec-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 8px 0;
        border-bottom: 1px solid #f0f0f0;
        
        &:last-child {
          border-bottom: none;
        }
        
        .spec-label {
          font-size: 14px;
          color: var(--text-secondary);
        }
        
        .spec-value {
          font-size: 14px;
          font-weight: 600;
          color: var(--text-primary);
          font-family: 'Consolas', 'Monaco', monospace;
        }
      }
    }
  }
}

.monitoring-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  
  .monitor-card {
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 8px;
    padding: 16px;
    text-align: center;
    
    h4 {
      font-size: 12px;
      font-weight: 500;
      color: var(--text-secondary);
      margin-bottom: 8px;
      text-transform: uppercase;
    }
    
    .monitor-value {
      font-size: 16px;
      font-weight: 600;
      font-family: 'Consolas', 'Monaco', monospace;
      
      &.active {
        color: #67c23a;
      }
      
      &.inactive {
        color: #909399;
      }
    }
  }
}

@media (max-width: 768px) {
  .oscilloscope-view {
    padding: 16px;
  }
  
  .page-header {
    margin-bottom: 32px;
    
    .page-title {
      font-size: 28px;
    }
    
    .page-subtitle {
      font-size: 16px;
    }
  }
  
  .features-grid {
    grid-template-columns: 1fr;
    gap: 16px;
  }
  
  .specs-grid {
    grid-template-columns: 1fr;
    gap: 16px;
  }
  
  .monitoring-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 12px;
  }
}
</style>
