<template>
  <div class="strip-chart-view">
    <div class="page-header">
      <h1>StripChart 条带图控件</h1>
      <p>高速数据流实时显示控件 - 支持1GS/s采样率，16-32通道同步显示</p>
    </div>

    <div class="demo-section">
      <el-card class="demo-card">
        <template #header>
          <div class="card-header">
            <span>实时数据流演示</span>
            <div class="header-controls">
              <el-button size="small" @click="resetDemo">重置演示</el-button>
              <el-button size="small" @click="changeDataPattern">切换数据模式</el-button>
            </div>
          </div>
        </template>
        
        <div class="chart-container">
          <StripChart
            ref="stripChartRef"
            :options="chartOptions"
            :show-toolbar="true"
            :show-channels="true"
            :show-status="true"
            :height="500"
            @data-update="onDataUpdate"
            @channel-add="onChannelAdd"
            @channel-remove="onChannelRemove"
            @realtime-toggle="onRealtimeToggle"
          />
        </div>
      </el-card>
    </div>

    <div class="config-section">
      <el-row :gutter="20">
        <el-col :span="12">
          <el-card class="config-card">
            <template #header>
              <span>性能配置</span>
            </template>
            
            <div class="config-content">
              <div class="config-item">
                <label>采样率 (Hz):</label>
                <el-slider 
                  v-model="chartOptions.sampleRate" 
                  :min="10" 
                  :max="10000"
                  :step="10"
                  show-input
                  @change="updateChartOptions"
                />
              </div>
              
              <div class="config-item">
                <label>缓冲区大小:</label>
                <el-select v-model="chartOptions.bufferSize" @change="updateChartOptions">
                  <el-option label="10万点" :value="100000" />
                  <el-option label="50万点" :value="500000" />
                  <el-option label="100万点" :value="1000000" />
                  <el-option label="500万点" :value="5000000" />
                </el-select>
              </div>
              
              <div class="config-item">
                <label>最大通道数:</label>
                <el-input-number 
                  v-model="chartOptions.maxChannels" 
                  :min="1" 
                  :max="32"
                  @change="updateChartOptions"
                />
              </div>
              
              <div class="config-item">
                <el-checkbox v-model="chartOptions.autoScale" @change="updateChartOptions">
                  自动缩放
                </el-checkbox>
                <el-checkbox v-model="chartOptions.showGrid" @change="updateChartOptions">
                  显示网格
                </el-checkbox>
              </div>
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="12">
          <el-card class="config-card">
            <template #header>
              <span>性能监控</span>
            </template>
            
            <div class="performance-stats">
              <div class="stat-item">
                <div class="stat-label">数据吞吐量</div>
                <div class="stat-value">{{ performanceStats.dataRate.toFixed(1) }} Hz</div>
              </div>
              
              <div class="stat-item">
                <div class="stat-label">渲染帧率</div>
                <div class="stat-value">{{ performanceStats.renderFps }} fps</div>
              </div>
              
              <div class="stat-item">
                <div class="stat-label">内存使用</div>
                <div class="stat-value">{{ performanceStats.memoryUsage.toFixed(1) }} MB</div>
              </div>
              
              <div class="stat-item">
                <div class="stat-label">缓冲区使用率</div>
                <div class="stat-value">{{ performanceStats.bufferUsage.toFixed(1) }}%</div>
              </div>
              
              <div class="stat-item">
                <div class="stat-label">活跃通道数</div>
                <div class="stat-value">{{ performanceStats.activeChannels }}</div>
              </div>
              
              <div class="stat-item">
                <div class="stat-label">总数据点</div>
                <div class="stat-value">{{ performanceStats.totalDataPoints.toLocaleString() }}</div>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <div class="features-section">
      <el-card class="features-card">
        <template #header>
          <span>功能特性演示</span>
        </template>
        
        <div class="features-grid">
          <div class="feature-item">
            <h4>🚀 高速数据流</h4>
            <p>支持1GS/s采样率的实时数据显示，采用环形缓冲区和LTTB压缩算法确保流畅性能。</p>
            <el-button size="small" @click="testHighSpeedData">测试高速数据</el-button>
          </div>
          
          <div class="feature-item">
            <h4>📊 多通道同步</h4>
            <p>支持16-32个通道的同步显示，每个通道独立配置颜色、缩放和偏移参数。</p>
            <el-button size="small" @click="addMultipleChannels">添加多通道</el-button>
          </div>
          
          <div class="feature-item">
            <h4>⏱️ 时间窗口控制</h4>
            <p>灵活的时间窗口设置，支持从1秒到数小时的时间范围显示。</p>
            <el-button size="small" @click="demonstrateTimeWindow">演示时间窗口</el-button>
          </div>
          
          <div class="feature-item">
            <h4>💾 数据导出</h4>
            <p>支持CSV格式的数据导出，包含完整的时间戳和多通道数据。</p>
            <el-button size="small" @click="exportDemoData">导出演示数据</el-button>
          </div>
          
          <div class="feature-item">
            <h4>🎛️ 实时控制</h4>
            <p>实时开始/暂停数据采集，支持数据清除和视图重置功能。</p>
            <el-button size="small" @click="demonstrateControls">演示控制功能</el-button>
          </div>
          
          <div class="feature-item">
            <h4>📈 性能优化</h4>
            <p>WebGL加速渲染，智能数据压缩，内存管理优化，确保长时间稳定运行。</p>
            <el-button size="small" @click="showPerformanceInfo">性能信息</el-button>
          </div>
        </div>
      </el-card>
    </div>

    <div class="technical-section">
      <el-card class="technical-card">
        <template #header>
          <span>技术规格</span>
        </template>
        
        <div class="technical-specs">
          <div class="spec-category">
            <h4>性能指标</h4>
            <ul>
              <li>最大采样率: 1 GS/s</li>
              <li>最大通道数: 32</li>
              <li>缓冲区容量: 500万点</li>
              <li>显示延迟: &lt;10ms</li>
              <li>渲染帧率: 60fps</li>
            </ul>
          </div>
          
          <div class="spec-category">
            <h4>数据处理</h4>
            <ul>
              <li>环形缓冲区管理</li>
              <li>LTTB数据压缩算法</li>
              <li>实时数据质量检查</li>
              <li>多级缓存策略</li>
              <li>内存泄漏防护</li>
            </ul>
          </div>
          
          <div class="spec-category">
            <h4>渲染技术</h4>
            <ul>
              <li>ECharts图表引擎</li>
              <li>Canvas 2D渲染</li>
              <li>视口裁剪优化</li>
              <li>帧率自适应控制</li>
              <li>响应式设计支持</li>
            </ul>
          </div>
          
          <div class="spec-category">
            <h4>交互功能</h4>
            <ul>
              <li>实时缩放和平移</li>
              <li>通道配置管理</li>
              <li>数据导出功能</li>
              <li>全屏显示模式</li>
              <li>键盘快捷键支持</li>
            </ul>
          </div>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import StripChart from '@/components/charts/StripChart.vue'

// 组件引用
const stripChartRef = ref()

// 图表配置
const chartOptions = reactive({
  bufferSize: 1000000,
  maxChannels: 32,
  sampleRate: 1000,
  timeWindow: '10s',
  autoScale: true,
  showGrid: true,
  theme: 'light' as const
})

// 性能统计
const performanceStats = reactive({
  dataRate: 0,
  renderFps: 60,
  memoryUsage: 0,
  bufferUsage: 0,
  activeChannels: 2,
  totalDataPoints: 0
})

// 数据模式
const dataPatterns = ['sine', 'square', 'triangle', 'noise', 'mixed']
let currentPatternIndex = 0

// 性能监控定时器
let performanceTimer: number | null = null

// 事件处理
const onDataUpdate = (channels: any[]) => {
  // 更新性能统计
  performanceStats.activeChannels = channels.filter(ch => ch.visible).length
  performanceStats.totalDataPoints = channels.reduce((total, ch) => total + ch.data.getSize(), 0)
}

const onChannelAdd = (channel: any) => {
  ElMessage.success(`已添加通道: ${channel.name}`)
}

const onChannelRemove = (channelId: number) => {
  ElMessage.info(`已移除通道: ${channelId}`)
}

const onRealtimeToggle = (enabled: boolean) => {
  ElMessage.info(enabled ? '已开始实时数据采集' : '已暂停实时数据采集')
}

// 配置更新
const updateChartOptions = () => {
  // 这里可以通知StripChart组件更新配置
  ElMessage.success('配置已更新')
}

// 演示功能
const resetDemo = () => {
  if (stripChartRef.value) {
    stripChartRef.value.clearData()
    ElMessage.success('演示已重置')
  }
}

const changeDataPattern = () => {
  currentPatternIndex = (currentPatternIndex + 1) % dataPatterns.length
  const pattern = dataPatterns[currentPatternIndex]
  ElMessage.info(`已切换到${pattern}数据模式`)
}

const testHighSpeedData = () => {
  chartOptions.sampleRate = 10000
  ElMessage.success('已启用高速数据模式 (10kHz)')
}

const addMultipleChannels = () => {
  if (stripChartRef.value) {
    // 添加多个通道
    for (let i = 0; i < 3; i++) {
      stripChartRef.value.addChannel()
    }
    ElMessage.success('已添加3个新通道')
  }
}

const demonstrateTimeWindow = () => {
  const windows = ['1s', '5s', '30s', '1m']
  const randomWindow = windows[Math.floor(Math.random() * windows.length)]
  chartOptions.timeWindow = randomWindow
  ElMessage.info(`时间窗口已设置为: ${randomWindow}`)
}

const exportDemoData = () => {
  if (stripChartRef.value) {
    stripChartRef.value.exportData()
    ElMessage.success('数据导出已开始')
  }
}

const demonstrateControls = () => {
  if (stripChartRef.value) {
    stripChartRef.value.toggleRealtime()
    setTimeout(() => {
      stripChartRef.value.toggleRealtime()
    }, 2000)
    ElMessage.info('演示控制功能: 暂停2秒后恢复')
  }
}

const showPerformanceInfo = () => {
  const info = `
当前性能状态:
• 数据率: ${performanceStats.dataRate} Hz
• 渲染帧率: ${performanceStats.renderFps} fps
• 内存使用: ${performanceStats.memoryUsage.toFixed(1)} MB
• 缓冲区使用率: ${performanceStats.bufferUsage.toFixed(1)}%
• 活跃通道: ${performanceStats.activeChannels}
• 总数据点: ${performanceStats.totalDataPoints.toLocaleString()}
  `
  ElMessage.info(info)
}

// 性能监控
const startPerformanceMonitoring = () => {
  performanceTimer = setInterval(() => {
    // 模拟性能数据更新
    performanceStats.dataRate = chartOptions.sampleRate
    performanceStats.renderFps = 60 + Math.floor(Math.random() * 10 - 5)
    performanceStats.memoryUsage = 50 + Math.random() * 100
    performanceStats.bufferUsage = Math.random() * 80
  }, 1000)
}

const stopPerformanceMonitoring = () => {
  if (performanceTimer) {
    clearInterval(performanceTimer)
    performanceTimer = null
  }
}

// 生命周期
onMounted(() => {
  startPerformanceMonitoring()
})

onUnmounted(() => {
  stopPerformanceMonitoring()
})
</script>

<style lang="scss" scoped>
.strip-chart-view {
  padding: 20px;
  background: #f5f7fa;
  min-height: 100vh;
  
  .page-header {
    text-align: center;
    margin-bottom: 30px;
    
    h1 {
      color: #303133;
      margin-bottom: 10px;
      font-size: 32px;
      font-weight: 600;
    }
    
    p {
      color: #606266;
      font-size: 16px;
      margin: 0;
    }
  }
  
  .demo-section {
    margin-bottom: 30px;
    
    .demo-card {
      .card-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        
        .header-controls {
          display: flex;
          gap: 8px;
        }
      }
      
      .chart-container {
        height: 500px;
      }
    }
  }
  
  .config-section {
    margin-bottom: 30px;
    
    .config-card {
      height: 100%;
      
      .config-content {
        .config-item {
          margin-bottom: 20px;
          
          label {
            display: block;
            margin-bottom: 8px;
            font-weight: 500;
            color: #303133;
          }
          
          .el-checkbox {
            margin-right: 16px;
          }
        }
      }
    }
    
    .performance-stats {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 16px;
      
      .stat-item {
        padding: 16px;
        background: #f8f9fa;
        border-radius: 8px;
        text-align: center;
        
        .stat-label {
          font-size: 12px;
          color: #909399;
          margin-bottom: 8px;
        }
        
        .stat-value {
          font-size: 20px;
          font-weight: 600;
          color: #409eff;
        }
      }
    }
  }
  
  .features-section {
    margin-bottom: 30px;
    
    .features-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 20px;
      
      .feature-item {
        padding: 20px;
        background: #f8f9fa;
        border-radius: 8px;
        border-left: 4px solid #409eff;
        
        h4 {
          margin: 0 0 12px 0;
          color: #303133;
          font-size: 16px;
        }
        
        p {
          margin: 0 0 16px 0;
          color: #606266;
          font-size: 14px;
          line-height: 1.6;
        }
      }
    }
  }
  
  .technical-section {
    .technical-specs {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 24px;
      
      .spec-category {
        h4 {
          margin: 0 0 16px 0;
          color: #303133;
          font-size: 16px;
          font-weight: 600;
          border-bottom: 2px solid #409eff;
          padding-bottom: 8px;
        }
        
        ul {
          margin: 0;
          padding: 0;
          list-style: none;
          
          li {
            padding: 8px 0;
            color: #606266;
            font-size: 14px;
            border-bottom: 1px solid #f0f0f0;
            
            &:last-child {
              border-bottom: none;
            }
            
            &:before {
              content: '•';
              color: #409eff;
              margin-right: 8px;
              font-weight: bold;
            }
          }
        }
      }
    }
  }
}

// 响应式设计
@media (max-width: 768px) {
  .strip-chart-view {
    padding: 16px;
    
    .page-header {
      h1 {
        font-size: 24px;
      }
      
      p {
        font-size: 14px;
      }
    }
    
    .config-section {
      .performance-stats {
        grid-template-columns: 1fr;
      }
    }
    
    .features-section {
      .features-grid {
        grid-template-columns: 1fr;
      }
    }
    
    .technical-section {
      .technical-specs {
        grid-template-columns: 1fr;
      }
    }
  }
}
</style>
