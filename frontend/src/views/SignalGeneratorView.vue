<template>
  <div class="signal-generator-view">
    <div class="page-header">
      <h1 class="page-title">任意波形信号发生器</h1>
      <p class="page-subtitle">专业级信号发生器控件，支持多种波形、调制和扫频功能</p>
    </div>

    <!-- 信号发生器演示 -->
    <section class="demo-section">
      <div class="section-header">
        <h2 class="section-title">信号发生器控件演示</h2>
        <p class="section-description">
          完整的信号发生器功能，包括波形生成、参数控制、调制和扫频
        </p>
      </div>

      <div class="demo-container">
        <SignalGenerator
          @output-change="handleOutputChange"
          @waveform-change="handleWaveformChange"
          @frequency-change="handleFrequencyChange"
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
          <div class="feature-icon">🌊</div>
          <h4>多种波形类型</h4>
          <p>支持正弦波、方波、三角波、锯齿波、脉冲和白噪声等6种标准波形</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">⚙️</div>
          <h4>精确参数控制</h4>
          <p>频率范围1mHz-100MHz，幅度1mVpp-10Vpp，直流偏置±5V，相位0°-360°</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">📡</div>
          <h4>调制功能</h4>
          <p>支持AM、FM、PM三种调制方式，调制频率0.1Hz-1kHz，调制深度0-100%</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">📈</div>
          <h4>扫频功能</h4>
          <p>线性和对数扫频，扫频范围可配置，扫频时间0.1s-60s</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">👁️</div>
          <h4>实时波形预览</h4>
          <p>Canvas绘制的实时波形预览，支持网格显示和参数标注</p>
        </div>

        <div class="feature-item">
          <div class="feature-icon">💾</div>
          <h4>预设和导出</h4>
          <p>快速预设加载，参数保存，波形数据CSV导出功能</p>
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
          <h4>频率特性</h4>
          <div class="spec-items">
            <div class="spec-item">
              <span class="spec-label">频率范围:</span>
              <span class="spec-value">1mHz - 100MHz</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">频率精度:</span>
              <span class="spec-value">±0.001%</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">频率分辨率:</span>
              <span class="spec-value">1mHz</span>
            </div>
          </div>
        </div>

        <div class="spec-category">
          <h4>幅度特性</h4>
          <div class="spec-items">
            <div class="spec-item">
              <span class="spec-label">幅度范围:</span>
              <span class="spec-value">1mVpp - 10Vpp</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">幅度精度:</span>
              <span class="spec-value">±1%</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">直流偏置:</span>
              <span class="spec-value">±5V</span>
            </div>
          </div>
        </div>

        <div class="spec-category">
          <h4>波形特性</h4>
          <div class="spec-items">
            <div class="spec-item">
              <span class="spec-label">波形类型:</span>
              <span class="spec-value">6种标准波形</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">相位范围:</span>
              <span class="spec-value">0° - 360°</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">占空比:</span>
              <span class="spec-value">1% - 99%</span>
            </div>
          </div>
        </div>

        <div class="spec-category">
          <h4>调制特性</h4>
          <div class="spec-items">
            <div class="spec-item">
              <span class="spec-label">调制类型:</span>
              <span class="spec-value">AM/FM/PM</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">调制频率:</span>
              <span class="spec-value">0.1Hz - 1kHz</span>
            </div>
            <div class="spec-item">
              <span class="spec-label">调制深度:</span>
              <span class="spec-value">0% - 100%</span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- 应用场景 -->
    <section class="applications-section">
      <div class="section-header">
        <h2 class="section-title">应用场景</h2>
      </div>

      <div class="applications-grid">
        <div class="application-item">
          <div class="application-icon">🔬</div>
          <h4>科研实验</h4>
          <p>为科研实验提供精确的测试信号，支持各种波形和调制方式</p>
        </div>

        <div class="application-item">
          <div class="application-icon">🏭</div>
          <h4>工业测试</h4>
          <p>设备测试和校准，信号完整性测试，频响特性测量</p>
        </div>

        <div class="application-item">
          <div class="application-icon">📚</div>
          <h4>教学培训</h4>
          <p>电子工程教学，信号处理课程，实验室教学演示</p>
        </div>

        <div class="application-item">
          <div class="application-icon">🔧</div>
          <h4>产品开发</h4>
          <p>电路设计验证，产品性能测试，故障诊断和调试</p>
        </div>

        <div class="application-item">
          <div class="application-icon">📡</div>
          <h4>通信测试</h4>
          <p>通信系统测试，调制解调验证，信道特性分析</p>
        </div>

        <div class="application-item">
          <div class="application-icon">🎵</div>
          <h4>音频测试</h4>
          <p>音频设备测试，声学测量，音质评估和优化</p>
        </div>
      </div>
    </section>

    <!-- 状态监控 -->
    <section class="monitoring-section" v-if="currentConfig">
      <div class="section-header">
        <h2 class="section-title">实时状态监控</h2>
      </div>

      <div class="monitoring-grid">
        <div class="monitor-card">
          <h4>输出状态</h4>
          <div class="monitor-value" :class="outputEnabled ? 'active' : 'inactive'">
            {{ outputEnabled ? '输出开启' : '输出关闭' }}
          </div>
        </div>

        <div class="monitor-card">
          <h4>当前波形</h4>
          <div class="monitor-value">{{ getWaveformName(currentConfig.type) }}</div>
        </div>

        <div class="monitor-card">
          <h4>频率</h4>
          <div class="monitor-value">{{ formatFrequency(currentConfig.frequency) }}</div>
        </div>

        <div class="monitor-card">
          <h4>幅度</h4>
          <div class="monitor-value">{{ currentConfig.amplitude.toFixed(3) }} Vpp</div>
        </div>

        <div class="monitor-card">
          <h4>直流偏置</h4>
          <div class="monitor-value">{{ currentConfig.offset.toFixed(3) }} V</div>
        </div>

        <div class="monitor-card">
          <h4>相位</h4>
          <div class="monitor-value">{{ currentConfig.phase.toFixed(1) }}°</div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import SignalGenerator from '@/components/instruments/SignalGenerator.vue'

// 响应式数据
const outputEnabled = ref(false)
const currentConfig = ref<any>(null)
const currentFrequency = ref(1000)

// 波形类型映射
const waveformNames: Record<string, string> = {
  sine: '正弦波',
  square: '方波',
  triangle: '三角波',
  sawtooth: '锯齿波',
  pulse: '脉冲',
  noise: '噪声'
}

// 事件处理
const handleOutputChange = (enabled: boolean) => {
  outputEnabled.value = enabled
  console.log('输出状态变化:', enabled)
}

const handleWaveformChange = (config: any) => {
  currentConfig.value = config
  console.log('波形配置变化:', config)
}

const handleFrequencyChange = (frequency: number) => {
  currentFrequency.value = frequency
  console.log('频率变化:', frequency)
}

// 工具函数
const getWaveformName = (type: string): string => {
  return waveformNames[type] || type
}

const formatFrequency = (freq: number): string => {
  if (freq >= 1000000) {
    return `${(freq / 1000000).toFixed(3)} MHz`
  } else if (freq >= 1000) {
    return `${(freq / 1000).toFixed(3)} kHz`
  } else {
    return `${freq.toFixed(3)} Hz`
  }
}
</script>

<style lang="scss" scoped>
.signal-generator-view {
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
.applications-section,
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

.applications-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 20px;
  
  .application-item {
    background: var(--surface-color);
    border: 1px solid var(--border-color);
    border-radius: 8px;
    padding: 20px;
    text-align: center;
    transition: all 0.3s ease;
    
    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }
    
    .application-icon {
      font-size: 28px;
      margin-bottom: 12px;
    }
    
    h4 {
      font-size: 14px;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 8px;
    }
    
    p {
      font-size: 12px;
      color: var(--text-secondary);
      line-height: 1.4;
      margin: 0;
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
  .signal-generator-view {
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
  
  .applications-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 12px;
  }
  
  .monitoring-grid {
    grid-template-columns: repeat(2, 1fr);
    gap: 12px;
  }
}
</style>
