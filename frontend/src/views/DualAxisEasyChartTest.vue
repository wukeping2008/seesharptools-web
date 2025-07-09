<template>
  <div class="dual-axis-chart-test example-page">
    <div class="page-header">
      <h1>双Y轴图表测试</h1>
      <p class="description">
        演示支持左右双Y轴的专业图表控件，可以在同一图表中显示不同量程和单位的数据系列。
      </p>
    </div>

    <!-- 功能说明 -->
    <div class="example-section">
      <h2 class="section-title">功能特点</h2>
      <el-row :gutter="24">
        <el-col :span="8">
          <el-card>
            <h3>双Y轴支持</h3>
            <p>支持左右双Y轴显示，每个Y轴可以独立配置量程、单位和标签</p>
          </el-card>
        </el-col>
        <el-col :span="8">
          <el-card>
            <h3>系列分配</h3>
            <p>可以灵活分配数据系列到不同的Y轴，适合显示不同量级的数据</p>
          </el-card>
        </el-col>
        <el-col :span="8">
          <el-card>
            <h3>独立缩放</h3>
            <p>左右Y轴支持独立的自动缩放和手动量程设置</p>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 信号生成器控制 -->
    <div class="example-section">
      <h2 class="section-title">信号生成器</h2>
      <el-card>
        <div class="generator-controls">
          <div class="control-group">
            <h4>基本参数</h4>
            <div class="control-row">
              <label>数据点数:</label>
              <el-input-number v-model="dataPoints" :min="100" :max="2000" @change="generateData" />
            </div>
            <div class="control-row">
              <label>采样率 (Hz):</label>
              <el-input-number v-model="sampleRate" :min="100" :max="10000" @change="generateData" />
            </div>
          </div>

          <div class="control-group">
            <h4>系列1 (左Y轴) - 电压信号</h4>
            <div class="control-row">
              <label>信号类型:</label>
              <el-select v-model="series1Config.type" @change="generateData">
                <el-option label="正弦波" value="sine" />
                <el-option label="方波" value="square" />
                <el-option label="三角波" value="triangle" />
                <el-option label="锯齿波" value="sawtooth" />
              </el-select>
            </div>
            <div class="control-row">
              <label>频率 (Hz):</label>
              <el-input-number v-model="series1Config.frequency" :min="1" :max="100" @change="generateData" />
            </div>
            <div class="control-row">
              <label>幅度 (V):</label>
              <el-input-number v-model="series1Config.amplitude" :min="0.1" :max="10" :step="0.1" @change="generateData" />
            </div>
            <div class="control-row">
              <label>直流偏置 (V):</label>
              <el-input-number v-model="series1Config.offset" :min="-5" :max="5" :step="0.1" @change="generateData" />
            </div>
          </div>

          <div class="control-group">
            <h4>系列2 (右Y轴) - 电流信号</h4>
            <div class="control-row">
              <label>信号类型:</label>
              <el-select v-model="series2Config.type" @change="generateData">
                <el-option label="正弦波" value="sine" />
                <el-option label="方波" value="square" />
                <el-option label="三角波" value="triangle" />
                <el-option label="锯齿波" value="sawtooth" />
              </el-select>
            </div>
            <div class="control-row">
              <label>频率 (Hz):</label>
              <el-input-number v-model="series2Config.frequency" :min="1" :max="100" @change="generateData" />
            </div>
            <div class="control-row">
              <label>幅度 (mA):</label>
              <el-input-number v-model="series2Config.amplitude" :min="1" :max="1000" :step="1" @change="generateData" />
            </div>
            <div class="control-row">
              <label>直流偏置 (mA):</label>
              <el-input-number v-model="series2Config.offset" :min="-500" :max="500" :step="1" @change="generateData" />
            </div>
          </div>

          <div class="control-group">
            <h4>系列3 (可选) - 功率信号</h4>
            <div class="control-row">
              <el-checkbox v-model="series3Enabled" @change="generateData">启用功率系列</el-checkbox>
            </div>
            <div v-if="series3Enabled" class="control-row">
              <label>功率计算:</label>
              <span>P = V × I (实时计算)</span>
            </div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 双Y轴图表 -->
    <div class="example-section">
      <h2 class="section-title">双Y轴图表演示</h2>
      <DualAxisEasyChart
        :data="chartData"
        :series-configs="seriesConfigs"
        :options="chartOptions"
        :height="500"
        @options-change="onOptionsChange"
        @data-export="onDataExport"
      />
    </div>

    <!-- 使用说明 -->
    <div class="example-section">
      <h2 class="section-title">使用说明</h2>
      <el-card>
        <div class="usage-guide">
          <h4>双Y轴模式操作指南：</h4>
          <ol>
            <li><strong>启用双Y轴</strong>：点击工具栏中的"双Y轴"开关</li>
            <li><strong>配置Y轴</strong>：在Y轴配置面板中设置左右Y轴的标签、单位和量程</li>
            <li><strong>分配系列</strong>：在系列配置面板中选择每个数据系列使用的Y轴</li>
            <li><strong>独立缩放</strong>：每个Y轴可以独立进行自动缩放或手动设置量程</li>
            <li><strong>单位转换</strong>：支持不同单位的数据在同一图表中显示</li>
          </ol>

          <h4>应用场景：</h4>
          <ul>
            <li><strong>电力测量</strong>：同时显示电压(V)和电流(mA)，量级差异很大</li>
            <li><strong>温度监控</strong>：显示温度(°C)和湿度(%)在同一图表中</li>
            <li><strong>过程控制</strong>：监控压力(Pa)和流量(L/min)等不同物理量</li>
            <li><strong>金融数据</strong>：显示价格和成交量等不同量级的数据</li>
          </ul>

          <h4>技术特点：</h4>
          <ul>
            <li><strong>智能布局</strong>：自动调整图表布局以适应双Y轴显示</li>
            <li><strong>交互式配置</strong>：实时调整Y轴参数，立即看到效果</li>
            <li><strong>数据同步</strong>：所有系列共享相同的X轴时间基准</li>
            <li><strong>专业外观</strong>：符合科学仪器的专业显示标准</li>
          </ul>
        </div>
      </el-card>
    </div>

    <!-- 技术实现 -->
    <div class="example-section">
      <h2 class="section-title">技术实现</h2>
      <el-row :gutter="24">
        <el-col :span="12">
          <el-card>
            <h4>核心特性</h4>
            <ul>
              <li>基于ECharts的双Y轴配置</li>
              <li>响应式Y轴配置界面</li>
              <li>系列到Y轴的动态分配</li>
              <li>独立的Y轴缩放控制</li>
              <li>单位自动转换显示</li>
            </ul>
          </el-card>
        </el-col>
        <el-col :span="12">
          <el-card>
            <h4>性能优化</h4>
            <ul>
              <li>高效的数据更新机制</li>
              <li>智能的图表重绘策略</li>
              <li>内存优化的数据管理</li>
              <li>流畅的交互响应</li>
              <li>自适应的布局调整</li>
            </ul>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import DualAxisEasyChart from '@/components/charts/DualAxisEasyChart.vue'
import type { ChartData, ChartOptions, SeriesConfig } from '@/types/chart'

// 响应式数据
const dataPoints = ref(500)
const sampleRate = ref(1000)
const series3Enabled = ref(true)

// 系列配置
const series1Config = reactive({
  type: 'sine',
  frequency: 10,
  amplitude: 5,
  offset: 0
})

const series2Config = reactive({
  type: 'sine',
  frequency: 15,
  amplitude: 200,
  offset: 100
})

// 图表数据
const chartData = ref<ChartData>({
  series: [],
  xStart: 0,
  xInterval: 0.001
})

// 系列配置
const seriesConfigs = ref<SeriesConfig[]>([
  {
    name: '电压 (V)',
    color: '#409eff',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'none',
    markerSize: 4,
    visible: true,
    yAxisIndex: 0 // 左Y轴
  },
  {
    name: '电流 (mA)',
    color: '#67c23a',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'none',
    markerSize: 4,
    visible: true,
    yAxisIndex: 1 // 右Y轴
  },
  {
    name: '功率 (W)',
    color: '#e6a23c',
    lineWidth: 2,
    lineType: 'dashed',
    markerType: 'none',
    markerSize: 4,
    visible: true,
    yAxisIndex: 0 // 左Y轴
  }
])

// 图表选项
const chartOptions = ref<Partial<ChartOptions>>({
  autoScale: true,
  legendVisible: true,
  gridEnabled: true,
  theme: 'light',
  dualYAxis: true,
  yAxes: [
    {
      autoScale: true,
      logarithmic: false,
      label: '电压/功率',
      unit: 'V/W',
      format: '{value}',
      division: 10,
      offset: 0
    },
    {
      autoScale: true,
      logarithmic: false,
      label: '电流',
      unit: 'mA',
      format: '{value}',
      division: 10,
      offset: 0
    }
  ]
})

// 信号生成函数
const generateSignal = (config: any, points: number, sampleRate: number) => {
  const data: number[] = []
  const dt = 1 / sampleRate
  
  for (let i = 0; i < points; i++) {
    const t = i * dt
    let value = 0
    
    switch (config.type) {
      case 'sine':
        value = config.amplitude * Math.sin(2 * Math.PI * config.frequency * t) + config.offset
        break
      case 'square':
        value = config.amplitude * Math.sign(Math.sin(2 * Math.PI * config.frequency * t)) + config.offset
        break
      case 'triangle':
        value = config.amplitude * (2 / Math.PI) * Math.asin(Math.sin(2 * Math.PI * config.frequency * t)) + config.offset
        break
      case 'sawtooth':
        value = config.amplitude * (2 * (config.frequency * t - Math.floor(config.frequency * t + 0.5))) + config.offset
        break
    }
    
    data.push(value)
  }
  
  return data
}

// 生成测试数据
const generateData = () => {
  const voltage = generateSignal(series1Config, dataPoints.value, sampleRate.value)
  const current = generateSignal(series2Config, dataPoints.value, sampleRate.value)
  
  const series = [voltage, current]
  
  // 如果启用功率系列，计算功率 P = V * I
  if (series3Enabled.value) {
    const power = voltage.map((v, i) => (v * current[i]) / 1000) // 转换为瓦特
    series.push(power)
    seriesConfigs.value[2].visible = true
  } else {
    seriesConfigs.value[2].visible = false
  }
  
  chartData.value = {
    series,
    xStart: 0,
    xInterval: 1 / sampleRate.value
  }
}

// 事件处理
const onOptionsChange = (options: ChartOptions) => {
  console.log('图表选项变化:', options)
}

const onDataExport = (data: any) => {
  console.log('数据导出:', data)
  ElMessage.success('图表已导出')
}

// 生命周期
onMounted(() => {
  generateData()
})
</script>

<style lang="scss" scoped>
.dual-axis-chart-test {
  .generator-controls {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: 20px;

    .control-group {
      h4 {
        margin: 0 0 15px 0;
        color: #303133;
        font-size: 14px;
        border-bottom: 1px solid #e4e7ed;
        padding-bottom: 8px;
      }

      .control-row {
        display: flex;
        align-items: center;
        margin-bottom: 12px;
        gap: 10px;

        label {
          min-width: 100px;
          font-size: 13px;
          color: #606266;
        }

        .el-input-number,
        .el-select {
          flex: 1;
          max-width: 150px;
        }
      }
    }
  }

  .usage-guide {
    h4 {
      color: #303133;
      margin: 20px 0 10px 0;
      
      &:first-child {
        margin-top: 0;
      }
    }

    ol, ul {
      margin: 10px 0;
      padding-left: 20px;

      li {
        margin-bottom: 8px;
        line-height: 1.6;

        strong {
          color: #409eff;
        }
      }
    }
  }
}
</style>
