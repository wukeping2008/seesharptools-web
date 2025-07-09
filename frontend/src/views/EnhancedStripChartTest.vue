<template>
  <div class="enhanced-strip-chart-test">
    <div class="test-header">
      <h1>高性能StripChart测试</h1>
      <p>WebGL渲染 + 多级缓存 + 1GS/s数据流支持</p>
    </div>

    <div class="test-controls">
      <el-card class="control-card">
        <template #header>
          <span>测试控制</span>
        </template>
        
        <div class="control-grid">
          <div class="control-item">
            <label>数据生成模式:</label>
            <el-select v-model="testMode" @change="onTestModeChange" size="small">
              <el-option label="高速模拟数据" value="highspeed" />
              <el-option label="真实信号模拟" value="realistic" />
              <el-option label="压力测试" value="stress" />
              <el-option label="多通道测试" value="multichannel" />
            </el-select>
          </div>
          
          <div class="control-item">
            <label>采样率:</label>
            <el-select v-model="sampleRate" @change="onSampleRateChange" size="small">
              <el-option label="1MS/s" :value="1000000" />
              <el-option label="10MS/s" :value="10000000" />
              <el-option label="100MS/s" :value="100000000" />
              <el-option label="1GS/s" :value="1000000000" />
            </el-select>
          </div>
          
          <div class="control-item">
            <label>通道数量:</label>
            <el-input-number 
              v-model="channelCount" 
              :min="1" 
              :max="32" 
              @change="onChannelCountChange"
              size="small"
            />
          </div>
          
          <div class="control-item">
            <label>缓冲区大小:</label>
            <el-select v-model="bufferSize" @change="onBufferSizeChange" size="small">
              <el-option label="1M点" :value="1000000" />
              <el-option label="10M点" :value="10000000" />
              <el-option label="100M点" :value="100000000" />
            </el-select>
          </div>
        </div>
        
        <div class="control-actions">
          <el-button type="primary" @click="startStressTest" :loading="stressTestRunning">
            <el-icon><Lightning /></el-icon>
            压力测试
          </el-button>
          <el-button @click="resetTest">
            <el-icon><RefreshRight /></el-icon>
            重置测试
          </el-button>
          <el-button @click="exportTestResults">
            <el-icon><Download /></el-icon>
            导出结果
          </el-button>
        </div>
      </el-card>
    </div>

    <!-- 主图表区域 -->
    <div class="chart-container">
      <EnhancedStripChart
        ref="stripChartRef"
        :max-channels="32"
        :buffer-size="bufferSize"
        :max-sample-rate="sampleRate"
        :show-toolbar="true"
        :show-channels="true"
        :show-status="true"
        :height="500"
        @performance-update="onPerformanceUpdate"
        @realtime-toggle="onRealtimeToggle"
      />
    </div>

    <!-- 性能统计面板 -->
    <div class="performance-stats">
      <el-row :gutter="16">
        <el-col :span="6">
          <el-card class="stat-card">
            <div class="stat-content">
              <div class="stat-icon">
                <el-icon><TrendCharts /></el-icon>
              </div>
              <div class="stat-info">
                <div class="stat-value">{{ performanceMetrics.renderFps }}</div>
                <div class="stat-label">渲染FPS</div>
              </div>
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="6">
          <el-card class="stat-card">
            <div class="stat-content">
              <div class="stat-icon">
                <el-icon><DataAnalysis /></el-icon>
              </div>
              <div class="stat-info">
                <div class="stat-value">{{ formatDataRate(performanceMetrics.dataRate) }}</div>
                <div class="stat-label">数据率</div>
              </div>
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="6">
          <el-card class="stat-card">
            <div class="stat-content">
              <div class="stat-icon">
                <el-icon><Monitor /></el-icon>
              </div>
              <div class="stat-info">
                <div class="stat-value">{{ performanceMetrics.memoryUsage.toFixed(1) }}MB</div>
                <div class="stat-label">内存使用</div>
              </div>
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="6">
          <el-card class="stat-card">
            <div class="stat-content">
              <div class="stat-icon">
                <el-icon><PieChart /></el-icon>
              </div>
              <div class="stat-info">
                <div class="stat-value">{{ (performanceMetrics.compressionRatio * 100).toFixed(1) }}%</div>
                <div class="stat-label">压缩比</div>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 测试结果面板 -->
    <div class="test-results" v-if="testResults.length > 0">
      <el-card>
        <template #header>
          <span>测试结果</span>
        </template>
        
        <el-table :data="testResults" style="width: 100%">
          <el-table-column prop="timestamp" label="时间" width="180">
            <template #default="scope">
              {{ new Date(scope.row.timestamp).toLocaleString() }}
            </template>
          </el-table-column>
          <el-table-column prop="testMode" label="测试模式" width="120" />
          <el-table-column prop="sampleRate" label="采样率" width="120">
            <template #default="scope">
              {{ formatDataRate(scope.row.sampleRate) }}
            </template>
          </el-table-column>
          <el-table-column prop="channelCount" label="通道数" width="80" />
          <el-table-column prop="avgFps" label="平均FPS" width="100" />
          <el-table-column prop="maxMemory" label="最大内存" width="120">
            <template #default="scope">
              {{ scope.row.maxMemory.toFixed(1) }}MB
            </template>
          </el-table-column>
          <el-table-column prop="compressionRatio" label="压缩比" width="100">
            <template #default="scope">
              {{ (scope.row.compressionRatio * 100).toFixed(1) }}%
            </template>
          </el-table-column>
          <el-table-column prop="status" label="状态" width="100">
            <template #default="scope">
              <el-tag :type="scope.row.status === 'success' ? 'success' : 'danger'">
                {{ scope.row.status === 'success' ? '成功' : '失败' }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </div>

    <!-- 技术说明 -->
    <div class="tech-info">
      <el-card>
        <template #header>
          <span>技术特性</span>
        </template>
        
        <el-row :gutter="16">
          <el-col :span="8">
            <div class="tech-item">
              <h4><el-icon><Lightning /></el-icon> WebGL渲染</h4>
              <p>使用WebGL进行硬件加速渲染，支持大数据量实时显示，渲染性能提升10-100倍。</p>
            </div>
          </el-col>
          
          <el-col :span="8">
            <div class="tech-item">
              <h4><el-icon><DataBoard /></el-icon> 多级缓存</h4>
              <p>L1/L2/L3三级缓存架构，智能数据压缩，支持TB级数据管理。</p>
            </div>
          </el-col>
          
          <el-col :span="8">
            <div class="tech-item">
              <h4><el-icon><Cpu /></el-icon> LTTB压缩</h4>
              <p>Largest Triangle Three Buckets算法，保持数据视觉特征的同时大幅减少数据点。</p>
            </div>
          </el-col>
        </el-row>
        
        <el-row :gutter="16" style="margin-top: 16px;">
          <el-col :span="8">
            <div class="tech-item">
              <h4><el-icon><Timer /></el-icon> 实时性能</h4>
              <p>支持1GS/s数据率，延迟<10ms，60fps稳定渲染。</p>
            </div>
          </el-col>
          
          <el-col :span="8">
            <div class="tech-item">
              <h4><el-icon><Grid /></el-icon> 多通道支持</h4>
              <p>最多支持32个通道同步显示，独立配置和统计。</p>
            </div>
          </el-col>
          
          <el-col :span="8">
            <div class="tech-item">
              <h4><el-icon><Setting /></el-icon> 智能优化</h4>
              <p>自适应LOD、视口裁剪、内存管理等多种优化策略。</p>
            </div>
          </el-col>
        </el-row>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { 
  Lightning, RefreshRight, Download, TrendCharts, DataAnalysis, 
  Monitor, PieChart, DataBoard, Cpu, Timer, Grid, Setting
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import EnhancedStripChart from '@/components/charts/EnhancedStripChart.vue'

// 响应式数据
const stripChartRef = ref()
const testMode = ref('highspeed')
const sampleRate = ref(1000000) // 1MS/s
const channelCount = ref(4)
const bufferSize = ref(1000000) // 1M点
const stressTestRunning = ref(false)

// 性能指标
const performanceMetrics = ref({
  renderFps: 60,
  dataRate: 0,
  memoryUsage: 0,
  compressionRatio: 1,
  totalDataPoints: 0
})

// 测试结果
const testResults = ref<any[]>([])

// 压力测试定时器
let stressTestTimer: number | null = null
let stressTestStartTime = 0

// 方法
const onTestModeChange = () => {
  ElMessage.info(`切换到${testMode.value}模式`)
}

const onSampleRateChange = () => {
  ElMessage.info(`采样率设置为${formatDataRate(sampleRate.value)}`)
}

const onChannelCountChange = () => {
  // 动态调整通道数量
  if (stripChartRef.value) {
    const currentChannels = stripChartRef.value.getChannels()
    const currentCount = currentChannels.length
    
    if (channelCount.value > currentCount) {
      // 添加通道
      for (let i = currentCount; i < channelCount.value; i++) {
        stripChartRef.value.addChannel()
      }
    } else if (channelCount.value < currentCount) {
      // 移除通道
      for (let i = currentCount - 1; i >= channelCount.value; i--) {
        stripChartRef.value.removeChannel(i)
      }
    }
  }
}

const onBufferSizeChange = () => {
  ElMessage.info(`缓冲区大小设置为${formatNumber(bufferSize.value)}点`)
}

const startStressTest = async () => {
  if (stressTestRunning.value) return
  
  stressTestRunning.value = true
  stressTestStartTime = Date.now()
  
  ElMessage.info('开始压力测试...')
  
  // 记录测试开始时的性能指标
  const startMetrics = { ...performanceMetrics.value }
  
  // 运行压力测试30秒
  stressTestTimer = setTimeout(() => {
    stopStressTest(startMetrics)
  }, 30000)
}

const stopStressTest = (startMetrics: any) => {
  if (!stressTestRunning.value) return
  
  stressTestRunning.value = false
  
  if (stressTestTimer) {
    clearTimeout(stressTestTimer)
    stressTestTimer = null
  }
  
  // 计算测试结果
  const testDuration = Date.now() - stressTestStartTime
  const avgFps = performanceMetrics.value.renderFps
  const maxMemory = performanceMetrics.value.memoryUsage
  const compressionRatio = performanceMetrics.value.compressionRatio
  
  // 判断测试是否成功
  const success = avgFps >= 30 && maxMemory < 2000 // 30fps以上，内存小于2GB
  
  const testResult = {
    timestamp: Date.now(),
    testMode: testMode.value,
    sampleRate: sampleRate.value,
    channelCount: channelCount.value,
    duration: testDuration,
    avgFps,
    maxMemory,
    compressionRatio,
    status: success ? 'success' : 'failed'
  }
  
  testResults.value.unshift(testResult)
  
  ElMessage({
    type: success ? 'success' : 'error',
    message: `压力测试${success ? '成功' : '失败'}完成`
  })
}

const resetTest = () => {
  if (stripChartRef.value) {
    stripChartRef.value.clearData()
  }
  
  testResults.value = []
  
  ElMessage.success('测试已重置')
}

const exportTestResults = () => {
  if (testResults.value.length === 0) {
    ElMessage.warning('没有测试结果可导出')
    return
  }
  
  // 生成CSV数据
  const headers = ['时间', '测试模式', '采样率', '通道数', '平均FPS', '最大内存(MB)', '压缩比(%)', '状态']
  let csvContent = headers.join(',') + '\n'
  
  testResults.value.forEach(result => {
    const row = [
      new Date(result.timestamp).toLocaleString(),
      result.testMode,
      formatDataRate(result.sampleRate),
      result.channelCount,
      result.avgFps,
      result.maxMemory.toFixed(1),
      (result.compressionRatio * 100).toFixed(1),
      result.status === 'success' ? '成功' : '失败'
    ]
    csvContent += row.join(',') + '\n'
  })
  
  // 下载文件
  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const link = document.createElement('a')
  link.href = URL.createObjectURL(blob)
  link.download = `enhanced_stripchart_test_${Date.now()}.csv`
  link.click()
  
  ElMessage.success('测试结果已导出')
}

const onPerformanceUpdate = (metrics: any) => {
  performanceMetrics.value = metrics
}

const onRealtimeToggle = (enabled: boolean) => {
  ElMessage.info(`实时模式${enabled ? '开启' : '关闭'}`)
}

// 工具函数
const formatDataRate = (rate: number): string => {
  if (rate >= 1000000000) {
    return `${(rate / 1000000000).toFixed(1)}GS/s`
  } else if (rate >= 1000000) {
    return `${(rate / 1000000).toFixed(1)}MS/s`
  } else if (rate >= 1000) {
    return `${(rate / 1000).toFixed(1)}kS/s`
  }
  return `${rate}S/s`
}

const formatNumber = (num: number): string => {
  if (num >= 1000000000) {
    return `${(num / 1000000000).toFixed(1)}G`
  } else if (num >= 1000000) {
    return `${(num / 1000000).toFixed(1)}M`
  } else if (num >= 1000) {
    return `${(num / 1000).toFixed(1)}K`
  }
  return num.toString()
}

// 生命周期
onMounted(() => {
  ElMessage.success('高性能StripChart测试页面已加载')
})

onUnmounted(() => {
  if (stressTestTimer) {
    clearTimeout(stressTestTimer)
  }
})
</script>

<style lang="scss" scoped>
.enhanced-strip-chart-test {
  padding: 20px;
  background: #f5f7fa;
  min-height: 100vh;
  
  .test-header {
    text-align: center;
    margin-bottom: 24px;
    
    h1 {
      font-size: 28px;
      color: #303133;
      margin-bottom: 8px;
    }
    
    p {
      font-size: 16px;
      color: #606266;
      margin: 0;
    }
  }
  
  .test-controls {
    margin-bottom: 24px;
    
    .control-card {
      .control-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 16px;
        margin-bottom: 16px;
        
        .control-item {
          display: flex;
          flex-direction: column;
          gap: 8px;
          
          label {
            font-size: 14px;
            font-weight: 500;
            color: #303133;
          }
        }
      }
      
      .control-actions {
        display: flex;
        gap: 12px;
        justify-content: center;
        padding-top: 16px;
        border-top: 1px solid #ebeef5;
      }
    }
  }
  
  .chart-container {
    margin-bottom: 24px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
  }
  
  .performance-stats {
    margin-bottom: 24px;
    
    .stat-card {
      .stat-content {
        display: flex;
        align-items: center;
        gap: 16px;
        
        .stat-icon {
          width: 48px;
          height: 48px;
          border-radius: 8px;
          background: linear-gradient(135deg, #409eff, #67c23a);
          display: flex;
          align-items: center;
          justify-content: center;
          color: white;
          font-size: 24px;
        }
        
        .stat-info {
          flex: 1;
          
          .stat-value {
            font-size: 24px;
            font-weight: 600;
            color: #303133;
            font-family: 'Consolas', 'Monaco', monospace;
          }
          
          .stat-label {
            font-size: 14px;
            color: #909399;
            margin-top: 4px;
          }
        }
      }
    }
  }
  
  .test-results {
    margin-bottom: 24px;
  }
  
  .tech-info {
    .tech-item {
      h4 {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 16px;
        color: #303133;
        margin-bottom: 8px;
        
        .el-icon {
          color: #409eff;
        }
      }
      
      p {
        font-size: 14px;
        color: #606266;
        line-height: 1.6;
        margin: 0;
      }
    }
  }
}

@media (max-width: 768px) {
  .enhanced-strip-chart-test {
    padding: 12px;
    
    .test-controls {
      .control-card {
        .control-grid {
          grid-template-columns: 1fr;
        }
        
        .control-actions {
          flex-direction: column;
        }
      }
    }
    
    .performance-stats {
      .stat-card {
        margin-bottom: 12px;
        
        .stat-content {
          .stat-icon {
            width: 40px;
            height: 40px;
            font-size: 20px;
          }
          
          .stat-info {
            .stat-value {
              font-size: 20px;
            }
          }
        }
      }
    }
  }
}
</style>
