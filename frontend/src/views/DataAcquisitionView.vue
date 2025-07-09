<template>
  <div class="daq-view">
    <div class="page-header">
      <h1 class="page-title">多功能数据采集卡</h1>
      <p class="page-subtitle">专业级多通道数据采集控件，支持高速采样、实时显示和数据分析</p>
    </div>

    <!-- 数据采集卡演示 -->
    <section class="demo-section">
      <div class="section-header">
        <h2 class="section-title">数据采集卡控件演示</h2>
        <p class="section-description">
          完整的数据采集功能，包括多通道配置、触发控制、实时统计和数据导出
        </p>
      </div>

      <div class="demo-container">
        <DataAcquisitionCard
          @acquisition-start="handleAcquisitionStart"
          @acquisition-stop="handleAcquisitionStop"
          @data-ready="handleDataReady"
          @channel-update="handleChannelUpdate"
          @calibration-complete="handleCalibrationComplete"
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
          <h4>多通道采集</h4>
          <p>支持4通道同步采集，每通道独立配置量程、耦合和阻抗</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">⚡</div>
          <h4>高速采样</h4>
          <p>1kS/s到2MS/s采样率，满足不同应用的采样需求</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">🎯</div>
          <h4>智能触发</h4>
          <p>边沿、电平、窗口触发，支持预触发和外部触发源</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">📈</div>
          <h4>实时统计</h4>
          <p>平均值、RMS、最值、标准差等统计分析功能</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">🔧</div>
          <h4>校准诊断</h4>
          <p>自校准、通道测试、噪声测试等专业诊断功能</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">💾</div>
          <h4>数据导出</h4>
          <p>CSV格式数据导出，支持时间戳和多通道数据</p>
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
          <h4>采样特性</h4>
          <div class="spec-items">
            <div class="spec-item">
              <span class="spec-label">通道数量:</span>
              <span class="spec-value">4通道</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">采样率:</span>
              <span class="spec-value">1kS/s - 2MS/s</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">分辨率:</span>
              <span class="spec-value">16位</span>
            </div>
          </div>
        </div>

        <div class="spec-category">
          <h4>输入特性</h4>
          <div class="spec-items">
            <div class="spec-item">
              <span class="spec-label">电压范围:</span>
              <span class="spec-value">±1V - ±20V</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">输入阻抗:</span>
              <span class="spec-value">1MΩ/50Ω/高阻</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">耦合方式:</span>
              <span class="spec-value">DC/AC/GND</span>
            </div>
          </div>
        </div>

        <div class="spec-category">
          <h4>触发特性</h4>
          <div class="spec-items">
            <div class="spec-item">
              <span class="spec-label">触发类型:</span>
              <span class="spec-value">边沿/电平/窗口</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">触发源:</span>
              <span class="spec-value">CH1-4/外部/软件</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">预触发:</span>
              <span class="spec-value">0-50%</span>
            </div>
          </div>
        </div>

        <div class="spec-category">
          <h4>数据处理</h4>
          <div class="spec-items">
            <div class="spec-item">
              <span class="spec-label">缓冲区:</span>
              <span class="spec-value">1MB - 1GB</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">采集模式:</span>
              <span class="spec-value">连续/有限/触发</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">数据格式:</span>
              <span class="spec-value">CSV导出</span>
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
          <h4>采集状态</h4>
          <div class="monitor-value" :class="currentStatus.acquiring ? 'active' : 'inactive'">
            {{ currentStatus.acquiring ? '采集中' : '已停止' }}
          </div>
        </div>

        <div class="monitor-card">
          <h4>采样率</h4>
          <div class="monitor-value">{{ currentStatus.sampleRate || '100kS/s' }}</div>
        </div>

        <div class="monitor-card">
          <h4>活动通道</h4>
          <div class="monitor-value">{{ currentStatus.activeChannels || 2 }}</div>
        </div>

        <div class="monitor-card">
          <h4>数据点数</h4>
          <div class="monitor-value">{{ currentStatus.dataPoints || '0' }}</div>
        </div>

        <div class="monitor-card">
          <h4>缓冲区使用</h4>
          <div class="monitor-value">{{ currentStatus.bufferUsage || '0%' }}</div>
        </div>

        <div class="monitor-card">
          <h4>采集时间</h4>
          <div class="monitor-value">{{ currentStatus.acquisitionTime || '0s' }}</div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import DataAcquisitionCard from '@/components/instruments/DataAcquisitionCard.vue'

// 响应式数据
const currentStatus = ref<any>({
  acquiring: false,
  sampleRate: '100kS/s',
  activeChannels: 2,
  dataPoints: '0',
  bufferUsage: '0%',
  acquisitionTime: '0s'
})

// 事件处理
const handleAcquisitionStart = (config: any) => {
  currentStatus.value.acquiring = true
  currentStatus.value.sampleRate = formatSampleRate(config.sampleRate)
  currentStatus.value.activeChannels = config.channels.length
  console.log('数据采集开始:', config)
}

const handleAcquisitionStop = () => {
  currentStatus.value.acquiring = false
  console.log('数据采集停止')
}

const handleDataReady = (data: any) => {
  currentStatus.value.dataPoints = formatDataCount(data.series.reduce((total: number, series: any) => {
    return total + (Array.isArray(series) ? series.length : 0)
  }, 0))
  console.log('数据就绪:', data)
}

const handleChannelUpdate = (channelIndex: number, config: any) => {
  console.log('通道更新:', channelIndex, config)
}

const handleCalibrationComplete = (results: any[]) => {
  console.log('校准完成:', results)
}

// 工具函数
const formatSampleRate = (rate: number): string => {
  if (rate >= 1000000) {
    return `${(rate / 1000000).toFixed(1)}MS/s`
  } else if (rate >= 1000) {
    return `${(rate / 1000).toFixed(1)}kS/s`
  }
  return `${rate}S/s`
}

const formatDataCount = (count: number): string => {
  if (count >= 1000000) {
    return `${(count / 1000000).toFixed(1)}M`
  } else if (count >= 1000) {
    return `${(count / 1000).toFixed(1)}K`
  }
  return count.toString()
}
</script>

<style lang="scss" scoped>
.daq-view {
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
  .daq-view {
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
