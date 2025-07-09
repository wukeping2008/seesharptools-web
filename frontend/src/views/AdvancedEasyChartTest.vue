<template>
  <div class="advanced-easy-chart-test">
    <div class="page-header">
      <h1>🧮 高级数学分析图表测试</h1>
      <p>集成统计分析、数据拟合、数字滤波器的专业图表组件</p>
    </div>

    <!-- 数据生成控制面板 -->
    <el-card class="control-panel">
      <template #header>
        <div class="panel-header">
          <span>📊 数据生成器</span>
          <el-button @click="generateData" type="primary">
            <el-icon><Refresh /></el-icon>
            生成新数据
          </el-button>
        </div>
      </template>

      <el-row :gutter="16">
        <el-col :span="6">
          <div class="control-group">
            <label>信号类型:</label>
            <el-select v-model="signalType" @change="generateData">
              <el-option label="正弦波" value="sine" />
              <el-option label="正弦波+噪声" value="sine_noise" />
              <el-option label="多项式" value="polynomial" />
              <el-option label="指数衰减" value="exponential" />
              <el-option label="随机游走" value="random_walk" />
              <el-option label="复合信号" value="composite" />
            </el-select>
          </div>
        </el-col>

        <el-col :span="6">
          <div class="control-group">
            <label>数据点数:</label>
            <el-input-number 
              v-model="dataPoints" 
              :min="50" 
              :max="2000" 
              :step="50"
              @change="generateData"
            />
          </div>
        </el-col>

        <el-col :span="6">
          <div class="control-group">
            <label>噪声强度:</label>
            <el-slider 
              v-model="noiseLevel" 
              :min="0" 
              :max="1" 
              :step="0.1"
              @change="generateData"
              show-input
            />
          </div>
        </el-col>

        <el-col :span="6">
          <div class="control-group">
            <label>频率 (Hz):</label>
            <el-input-number 
              v-model="frequency" 
              :min="0.1" 
              :max="10" 
              :step="0.1"
              @change="generateData"
            />
          </div>
        </el-col>
      </el-row>
    </el-card>

    <!-- 高级图表组件 -->
    <el-card class="chart-card">
      <template #header>
        <div class="chart-header">
          <span>📈 高级数学分析图表</span>
          <div class="chart-info">
            <el-tag v-if="lastStatistics" type="info">
              平均值: {{ lastStatistics.mean.toFixed(3) }}
            </el-tag>
            <el-tag v-if="lastFitting" type="success">
              R²: {{ lastFitting.rSquared.toFixed(3) }}
            </el-tag>
          </div>
        </div>
      </template>

      <AdvancedEasyChart
        ref="chartRef"
        :data="chartData"
        :options="chartOptions"
        :series-configs="seriesConfigs"
        :height="500"
        @statistics-calculated="onStatisticsCalculated"
        @fitting-completed="onFittingCompleted"
        @data-update="onDataUpdate"
      />
    </el-card>

    <!-- 功能演示面板 -->
    <el-row :gutter="16" class="demo-panels">
      <!-- 统计分析演示 -->
      <el-col :span="8">
        <el-card>
          <template #header>
            <span>📊 统计分析演示</span>
          </template>
          
          <div class="demo-content">
            <p>点击"数学分析"按钮，然后点击"计算"来查看完整的统计分析结果：</p>
            <ul>
              <li>基本统计量：平均值、中位数、标准差、方差</li>
              <li>分布特征：偏度、峰度、四分位数</li>
              <li>异常值检测：基于IQR方法</li>
              <li>RMS值和极差计算</li>
            </ul>
            
            <div v-if="lastStatistics" class="stats-summary">
              <h4>最新统计结果:</h4>
              <div class="stats-grid">
                <div class="stat-item">
                  <span class="label">平均值:</span>
                  <span class="value">{{ lastStatistics.mean.toFixed(4) }}</span>
                </div>
                <div class="stat-item">
                  <span class="label">标准差:</span>
                  <span class="value">{{ lastStatistics.standardDeviation.toFixed(4) }}</span>
                </div>
                <div class="stat-item">
                  <span class="label">偏度:</span>
                  <span class="value">{{ lastStatistics.skewness.toFixed(4) }}</span>
                </div>
                <div class="stat-item">
                  <span class="label">异常值:</span>
                  <span class="value">{{ lastStatistics.outliers.length }}个</span>
                </div>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 数据拟合演示 -->
      <el-col :span="8">
        <el-card>
          <template #header>
            <span>📈 数据拟合演示</span>
          </template>
          
          <div class="demo-content">
            <p>点击"数据拟合"按钮来体验强大的拟合功能：</p>
            <ul>
              <li>线性拟合：最小二乘法线性回归</li>
              <li>多项式拟合：2-6阶多项式拟合</li>
              <li>拟合质量评估：R²、RMSE、MAE</li>
              <li>残差分析：可视化拟合误差</li>
            </ul>
            
            <div v-if="lastFitting" class="fitting-summary">
              <h4>最新拟合结果:</h4>
              <div class="fitting-info">
                <div class="equation">{{ lastFitting.equation }}</div>
                <div class="metrics">
                  <span>R² = {{ lastFitting.rSquared.toFixed(6) }}</span>
                  <span>RMSE = {{ lastFitting.rmse.toFixed(6) }}</span>
                  <span>MAE = {{ lastFitting.mae.toFixed(6) }}</span>
                </div>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 数字滤波演示 -->
      <el-col :span="8">
        <el-card>
          <template #header>
            <span>🔧 数字滤波演示</span>
          </template>
          
          <div class="demo-content">
            <p>点击"滤波器"按钮来体验多种滤波算法：</p>
            <ul>
              <li>移动平均滤波：平滑噪声数据</li>
              <li>中值滤波：去除脉冲噪声</li>
              <li>高斯滤波：保边缘平滑</li>
              <li>低通/高通滤波：频域滤波</li>
              <li>数据平滑：Savitzky-Golay类似算法</li>
            </ul>
            
            <div class="filter-tips">
              <h4>滤波器选择建议:</h4>
              <ul>
                <li><strong>移动平均</strong>：适用于平滑随机噪声</li>
                <li><strong>中值滤波</strong>：适用于去除脉冲干扰</li>
                <li><strong>高斯滤波</strong>：适用于保持边缘特征</li>
                <li><strong>低通滤波</strong>：适用于去除高频噪声</li>
              </ul>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 技术说明 -->
    <el-card class="tech-info">
      <template #header>
        <span>🔬 技术实现说明</span>
      </template>
      
      <el-row :gutter="16">
        <el-col :span="12">
          <h3>🧮 数学算法库</h3>
          <p>基于自研的 <code>MathAnalyzer</code> 类，实现了完整的数学分析功能：</p>
          <ul>
            <li><strong>统计分析</strong>：完整的描述性统计，包括高阶矩计算</li>
            <li><strong>数据拟合</strong>：最小二乘法线性和多项式拟合</li>
            <li><strong>数字滤波</strong>：多种时域和频域滤波算法</li>
            <li><strong>数值计算</strong>：矩阵运算、高斯消元法等</li>
          </ul>
        </el-col>
        
        <el-col :span="12">
          <h3>📊 图表集成</h3>
          <p>基于 ECharts 的高级图表组件，提供专业的数据可视化：</p>
          <ul>
            <li><strong>实时更新</strong>：数学分析结果实时显示在图表上</li>
            <li><strong>交互操作</strong>：游标测量、缩放、导出等功能</li>
            <li><strong>多层显示</strong>：原始数据、拟合曲线、滤波结果同时显示</li>
            <li><strong>专业工具</strong>：完整的数据分析工具栏</li>
          </ul>
        </el-col>
      </el-row>
      
      <el-divider />
      
      <div class="performance-info">
        <h3>⚡ 性能特点</h3>
        <el-row :gutter="16">
          <el-col :span="6">
            <div class="perf-item">
              <div class="perf-title">算法效率</div>
              <div class="perf-desc">O(N log N) FFT，O(N) 统计计算</div>
            </div>
          </el-col>
          <el-col :span="6">
            <div class="perf-item">
              <div class="perf-title">内存优化</div>
              <div class="perf-desc">流式处理，避免大数组复制</div>
            </div>
          </el-col>
          <el-col :span="6">
            <div class="perf-item">
              <div class="perf-title">实时响应</div>
              <div class="perf-desc">异步计算，不阻塞UI线程</div>
            </div>
          </el-col>
          <el-col :span="6">
            <div class="perf-item">
              <div class="perf-title">精度保证</div>
              <div class="perf-desc">双精度浮点，数值稳定算法</div>
            </div>
          </el-col>
        </el-row>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Refresh } from '@element-plus/icons-vue'
import AdvancedEasyChart from '@/components/charts/AdvancedEasyChart.vue'
import type { ChartData, ChartOptions, SeriesConfig } from '@/types/chart'
import type { StatisticsResult, FittingResult } from '@/utils/math/MathAnalyzer'

// 响应式数据
const chartRef = ref<InstanceType<typeof AdvancedEasyChart>>()
const chartData = ref<ChartData>()
const lastStatistics = ref<StatisticsResult>()
const lastFitting = ref<FittingResult>()

// 数据生成参数
const signalType = ref<'sine' | 'sine_noise' | 'polynomial' | 'exponential' | 'random_walk' | 'composite'>('sine_noise')
const dataPoints = ref(500)
const noiseLevel = ref(0.2)
const frequency = ref(2)

// 图表配置
const chartOptions: ChartOptions = {
  autoScale: true,
  logarithmic: false,
  splitView: false,
  legendVisible: true,
  cursorMode: 'zoom',
  gridEnabled: true,
  minorGridEnabled: false,
  theme: 'light'
}

const seriesConfigs: SeriesConfig[] = [
  {
    name: '测试数据',
    color: '#409eff',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'none',
    markerSize: 4,
    visible: true
  }
]

// 生成测试数据
const generateData = () => {
  const points = dataPoints.value
  const data: number[] = []
  
  switch (signalType.value) {
    case 'sine':
      // 纯正弦波
      for (let i = 0; i < points; i++) {
        const x = (i / points) * 4 * Math.PI
        data.push(Math.sin(frequency.value * x))
      }
      break
      
    case 'sine_noise':
      // 正弦波 + 噪声
      for (let i = 0; i < points; i++) {
        const x = (i / points) * 4 * Math.PI
        const signal = Math.sin(frequency.value * x)
        const noise = (Math.random() - 0.5) * 2 * noiseLevel.value
        data.push(signal + noise)
      }
      break
      
    case 'polynomial':
      // 多项式函数
      for (let i = 0; i < points; i++) {
        const x = (i / points - 0.5) * 4
        const signal = 0.1 * x * x * x - 0.5 * x * x + x + 2
        const noise = (Math.random() - 0.5) * 2 * noiseLevel.value
        data.push(signal + noise)
      }
      break
      
    case 'exponential':
      // 指数衰减
      for (let i = 0; i < points; i++) {
        const x = i / points * 5
        const signal = Math.exp(-x) * Math.cos(frequency.value * x * 2 * Math.PI)
        const noise = (Math.random() - 0.5) * 2 * noiseLevel.value
        data.push(signal + noise)
      }
      break
      
    case 'random_walk':
      // 随机游走
      let value = 0
      for (let i = 0; i < points; i++) {
        value += (Math.random() - 0.5) * 0.2
        const noise = (Math.random() - 0.5) * 2 * noiseLevel.value
        data.push(value + noise)
      }
      break
      
    case 'composite':
      // 复合信号
      for (let i = 0; i < points; i++) {
        const x = (i / points) * 6 * Math.PI
        const signal1 = Math.sin(frequency.value * x)
        const signal2 = 0.5 * Math.sin(frequency.value * 3 * x)
        const signal3 = 0.3 * Math.sin(frequency.value * 5 * x)
        const trend = 0.001 * i
        const noise = (Math.random() - 0.5) * 2 * noiseLevel.value
        data.push(signal1 + signal2 + signal3 + trend + noise)
      }
      break
  }
  
  chartData.value = {
    series: data,
    xStart: 0,
    xInterval: 1,
    labels: Array.from({ length: points }, (_, i) => i.toString())
  }
}

// 事件处理
const onStatisticsCalculated = (result: StatisticsResult) => {
  lastStatistics.value = result
  console.log('统计分析完成:', result)
}

const onFittingCompleted = (result: FittingResult) => {
  lastFitting.value = result
  console.log('数据拟合完成:', result)
}

const onDataUpdate = (data: ChartData) => {
  chartData.value = data
  console.log('数据已更新:', data)
}

// 初始化
onMounted(() => {
  generateData()
})
</script>

<style lang="scss" scoped>
.advanced-easy-chart-test {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
  
  .page-header {
    text-align: center;
    margin-bottom: 24px;
    
    h1 {
      color: #2c3e50;
      margin-bottom: 8px;
    }
    
    p {
      color: #7f8c8d;
      font-size: 16px;
    }
  }
  
  .control-panel {
    margin-bottom: 20px;
    
    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    
    .control-group {
      display: flex;
      flex-direction: column;
      gap: 8px;
      
      label {
        font-weight: 500;
        color: #606266;
        font-size: 14px;
      }
    }
  }
  
  .chart-card {
    margin-bottom: 20px;
    
    .chart-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      
      .chart-info {
        display: flex;
        gap: 8px;
      }
    }
  }
  
  .demo-panels {
    margin-bottom: 20px;
    
    .demo-content {
      p {
        margin-bottom: 12px;
        color: #606266;
      }
      
      ul {
        margin-bottom: 16px;
        padding-left: 20px;
        
        li {
          margin-bottom: 4px;
          color: #606266;
        }
      }
      
      .stats-summary,
      .fitting-summary {
        margin-top: 16px;
        padding: 12px;
        background: #f8f9fa;
        border-radius: 6px;
        
        h4 {
          margin-bottom: 12px;
          color: #409eff;
        }
        
        .stats-grid {
          display: grid;
          grid-template-columns: 1fr 1fr;
          gap: 8px;
          
          .stat-item {
            display: flex;
            justify-content: space-between;
            
            .label {
              color: #606266;
            }
            
            .value {
              font-family: 'Courier New', monospace;
              color: #409eff;
              font-weight: bold;
            }
          }
        }
        
        .fitting-info {
          .equation {
            font-family: 'Courier New', monospace;
            color: #e74c3c;
            font-weight: bold;
            margin-bottom: 8px;
            font-size: 14px;
          }
          
          .metrics {
            display: flex;
            gap: 16px;
            
            span {
              font-family: 'Courier New', monospace;
              color: #409eff;
              font-size: 12px;
            }
          }
        }
      }
      
      .filter-tips {
        margin-top: 16px;
        
        h4 {
          margin-bottom: 8px;
          color: #67c23a;
        }
        
        ul {
          li {
            margin-bottom: 6px;
            
            strong {
              color: #409eff;
            }
          }
        }
      }
    }
  }
  
  .tech-info {
    h3 {
      color: #2c3e50;
      margin-bottom: 12px;
    }
    
    p {
      color: #606266;
      margin-bottom: 12px;
      line-height: 1.6;
    }
    
    ul {
      padding-left: 20px;
      
      li {
        margin-bottom: 8px;
        color: #606266;
        line-height: 1.5;
        
        strong {
          color: #409eff;
        }
        
        code {
          background: #f1f2f3;
          padding: 2px 6px;
          border-radius: 3px;
          font-family: 'Courier New', monospace;
          color: #e74c3c;
        }
      }
    }
    
    .performance-info {
      .perf-item {
        text-align: center;
        padding: 16px;
        background: #f8f9fa;
        border-radius: 8px;
        
        .perf-title {
          font-weight: bold;
          color: #409eff;
          margin-bottom: 8px;
        }
        
        .perf-desc {
          font-size: 12px;
          color: #606266;
          line-height: 1.4;
        }
      }
    }
  }
}

@media (max-width: 768px) {
  .advanced-easy-chart-test {
    padding: 12px;
    
    .demo-panels {
      .el-col {
        margin-bottom: 16px;
      }
    }
    
    .tech-info {
      .performance-info {
        .el-col {
          margin-bottom: 12px;
        }
      }
    }
  }
}
</style>
