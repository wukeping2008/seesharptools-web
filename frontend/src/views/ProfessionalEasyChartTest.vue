<template>
  <div class="professional-easy-chart-test">
    <div class="page-header">
      <h1>📏 专业测量工具图表测试</h1>
      <p>集成游标测量、峰值检测、频率分析、自动测量等专业功能的高级图表组件</p>
    </div>

    <!-- 信号生成器 -->
    <div class="signal-generator">
      <h2>🎛️ 信号生成器</h2>
      <div class="generator-controls">
        <div class="control-group">
          <label>信号类型:</label>
          <el-select v-model="signalType" @change="generateSignal">
            <el-option label="正弦波" value="sine" />
            <el-option label="正弦波+噪声" value="sine-noise" />
            <el-option label="方波" value="square" />
            <el-option label="三角波" value="triangle" />
            <el-option label="锯齿波" value="sawtooth" />
            <el-option label="复合信号" value="composite" />
            <el-option label="脉冲信号" value="pulse" />
            <el-option label="调制信号" value="modulated" />
          </el-select>
        </div>

        <div class="control-group">
          <label>数据点数:</label>
          <el-slider 
            v-model="dataPoints" 
            :min="100" 
            :max="2000" 
            :step="50"
            @change="generateSignal"
            show-input
          />
        </div>

        <div class="control-group">
          <label>频率 (Hz):</label>
          <el-slider 
            v-model="frequency" 
            :min="1" 
            :max="50" 
            :step="0.5"
            @change="generateSignal"
            show-input
          />
        </div>

        <div class="control-group">
          <label>幅度:</label>
          <el-slider 
            v-model="amplitude" 
            :min="0.1" 
            :max="5" 
            :step="0.1"
            @change="generateSignal"
            show-input
          />
        </div>

        <div class="control-group">
          <label>噪声强度:</label>
          <el-slider 
            v-model="noiseLevel" 
            :min="0" 
            :max="1" 
            :step="0.05"
            @change="generateSignal"
            show-input
          />
        </div>

        <div class="control-group">
          <label>采样率 (Hz):</label>
          <el-input-number 
            v-model="sampleRate" 
            :min="100" 
            :max="10000" 
            :step="100"
            @change="generateSignal"
          />
        </div>

        <el-button type="primary" @click="generateSignal">
          <el-icon><Refresh /></el-icon>
          重新生成信号
        </el-button>
      </div>
    </div>

    <!-- 专业图表 -->
    <div class="chart-section">
      <h2>📊 专业测量图表</h2>
      <ProfessionalEasyChart
        :data="chartData"
        :series-configs="seriesConfigs"
        :height="500"
        :sample-rate="sampleRate"
        @cursor-measurement="onCursorMeasurement"
        @peak-detection="onPeakDetection"
        @frequency-analysis="onFrequencyAnalysis"
        @auto-measurement="onAutoMeasurement"
      />
    </div>

    <!-- 测量结果展示 -->
    <div class="results-section">
      <h2>📋 测量结果</h2>
      <el-tabs v-model="activeResultTab" type="border-card">
        <!-- 游标测量结果 -->
        <el-tab-pane label="游标测量" name="cursor">
          <div v-if="cursorResult" class="result-content">
            <div class="result-grid">
              <div class="result-item">
                <span class="label">游标1坐标:</span>
                <span class="value">({{ cursorResult.x1.toFixed(3) }}, {{ cursorResult.y1.toFixed(3) }})</span>
              </div>
              <div class="result-item">
                <span class="label">游标2坐标:</span>
                <span class="value">({{ cursorResult.x2.toFixed(3) }}, {{ cursorResult.y2.toFixed(3) }})</span>
              </div>
              <div class="result-item">
                <span class="label">X轴差值:</span>
                <span class="value">{{ cursorResult.deltaX.toFixed(3) }}</span>
              </div>
              <div class="result-item">
                <span class="label">Y轴差值:</span>
                <span class="value">{{ cursorResult.deltaY.toFixed(3) }}</span>
              </div>
              <div v-if="cursorResult.frequency" class="result-item">
                <span class="label">频率:</span>
                <span class="value">{{ cursorResult.frequency.toFixed(3) }} Hz</span>
              </div>
              <div v-if="cursorResult.period" class="result-item">
                <span class="label">周期:</span>
                <span class="value">{{ cursorResult.period.toFixed(3) }} s</span>
              </div>
              <div v-if="cursorResult.slope" class="result-item">
                <span class="label">斜率:</span>
                <span class="value">{{ cursorResult.slope.toFixed(3) }}</span>
              </div>
            </div>
          </div>
          <div v-else class="no-result">
            启用游标测量模式并在图表上点击两个点进行测量
          </div>
        </el-tab-pane>

        <!-- 峰值检测结果 -->
        <el-tab-pane label="峰值检测" name="peaks">
          <div v-if="peakResult" class="result-content">
            <div class="peak-summary">
              <h4>检测摘要</h4>
              <div class="summary-grid">
                <div class="summary-item">
                  <span class="label">峰值数量:</span>
                  <span class="value">{{ peakResult.statistics.peakCount }}</span>
                </div>
                <div class="summary-item">
                  <span class="label">谷值数量:</span>
                  <span class="value">{{ peakResult.statistics.valleyCount }}</span>
                </div>
                <div class="summary-item">
                  <span class="label">平均峰值:</span>
                  <span class="value">{{ peakResult.statistics.averagePeakValue.toFixed(3) }}</span>
                </div>
                <div class="summary-item">
                  <span class="label">峰峰值:</span>
                  <span class="value">{{ peakResult.statistics.peakToPeakAmplitude.toFixed(3) }}</span>
                </div>
              </div>
            </div>
            
            <div class="peak-details">
              <h4>峰值详情 (前5个)</h4>
              <el-table :data="peakResult.peaks.slice(0, 5)" size="small">
                <el-table-column prop="index" label="索引" width="80" />
                <el-table-column prop="value" label="数值" width="100">
                  <template #default="{ row }">
                    {{ row.value.toFixed(4) }}
                  </template>
                </el-table-column>
                <el-table-column prop="prominence" label="突出度" width="100">
                  <template #default="{ row }">
                    {{ row.prominence.toFixed(4) }}
                  </template>
                </el-table-column>
                <el-table-column prop="width" label="宽度" width="80" />
              </el-table>
            </div>
          </div>
          <div v-else class="no-result">
            点击"峰值检测"按钮进行分析
          </div>
        </el-tab-pane>

        <!-- 频率分析结果 -->
        <el-tab-pane label="频率分析" name="frequency">
          <div v-if="frequencyResult" class="result-content">
            <div class="frequency-summary">
              <h4>频率分析摘要</h4>
              <div class="freq-grid">
                <div class="freq-item">
                  <span class="label">基频:</span>
                  <span class="value">{{ frequencyResult.fundamentalFrequency.toFixed(3) }} Hz</span>
                </div>
                <div class="freq-item">
                  <span class="label">主频:</span>
                  <span class="value">{{ frequencyResult.dominantFrequency.toFixed(3) }} Hz</span>
                </div>
                <div class="freq-item">
                  <span class="label">总谐波失真:</span>
                  <span class="value">{{ frequencyResult.thd.toFixed(2) }}%</span>
                </div>
                <div class="freq-item">
                  <span class="label">信噪比:</span>
                  <span class="value">{{ frequencyResult.snr.toFixed(1) }} dB</span>
                </div>
                <div class="freq-item">
                  <span class="label">带宽:</span>
                  <span class="value">{{ frequencyResult.bandwidth.toFixed(1) }} Hz</span>
                </div>
              </div>
            </div>
            
            <div class="harmonics-details">
              <h4>谐波分量</h4>
              <el-table :data="frequencyResult.harmonics" size="small">
                <el-table-column prop="harmonic" label="次数" width="80" />
                <el-table-column prop="frequency" label="频率 (Hz)" width="120">
                  <template #default="{ row }">
                    {{ row.frequency.toFixed(3) }}
                  </template>
                </el-table-column>
                <el-table-column prop="amplitude" label="幅度" width="100">
                  <template #default="{ row }">
                    {{ row.amplitude.toFixed(4) }}
                  </template>
                </el-table-column>
              </el-table>
            </div>
          </div>
          <div v-else class="no-result">
            点击"频率分析"按钮进行分析
          </div>
        </el-tab-pane>

        <!-- 自动测量结果 -->
        <el-tab-pane label="自动测量" name="auto">
          <div v-if="autoResult" class="result-content">
            <div class="auto-measurement-results">
              <div class="measurement-category">
                <h4>基本参数</h4>
                <div class="param-grid">
                  <div class="param-item">
                    <span class="label">频率:</span>
                    <span class="value">{{ autoResult.frequency.toFixed(3) }} Hz</span>
                  </div>
                  <div class="param-item">
                    <span class="label">周期:</span>
                    <span class="value">{{ autoResult.period.toFixed(3) }} s</span>
                  </div>
                  <div class="param-item">
                    <span class="label">幅度:</span>
                    <span class="value">{{ autoResult.amplitude.toFixed(3) }}</span>
                  </div>
                  <div class="param-item">
                    <span class="label">RMS:</span>
                    <span class="value">{{ autoResult.rms.toFixed(3) }}</span>
                  </div>
                </div>
              </div>
              
              <div class="measurement-category">
                <h4>统计量</h4>
                <div class="param-grid">
                  <div class="param-item">
                    <span class="label">平均值:</span>
                    <span class="value">{{ autoResult.mean.toFixed(3) }}</span>
                  </div>
                  <div class="param-item">
                    <span class="label">最小值:</span>
                    <span class="value">{{ autoResult.min.toFixed(3) }}</span>
                  </div>
                  <div class="param-item">
                    <span class="label">最大值:</span>
                    <span class="value">{{ autoResult.max.toFixed(3) }}</span>
                  </div>
                  <div class="param-item">
                    <span class="label">峰峰值:</span>
                    <span class="value">{{ autoResult.peakToPeak.toFixed(3) }}</span>
                  </div>
                </div>
              </div>
              
              <div class="measurement-category">
                <h4>时序参数</h4>
                <div class="param-grid">
                  <div class="param-item">
                    <span class="label">占空比:</span>
                    <span class="value">{{ autoResult.dutyCycle.toFixed(1) }}%</span>
                  </div>
                  <div class="param-item">
                    <span class="label">上升时间:</span>
                    <span class="value">{{ (autoResult.riseTime * 1000).toFixed(2) }} ms</span>
                  </div>
                  <div class="param-item">
                    <span class="label">下降时间:</span>
                    <span class="value">{{ (autoResult.fallTime * 1000).toFixed(2) }} ms</span>
                  </div>
                  <div class="param-item">
                    <span class="label">脉冲宽度:</span>
                    <span class="value">{{ (autoResult.pulseWidth * 1000).toFixed(2) }} ms</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div v-else class="no-result">
            点击"自动测量"按钮进行分析
          </div>
        </el-tab-pane>
      </el-tabs>
    </div>

    <!-- 功能说明 -->
    <div class="documentation">
      <h2>📖 功能说明</h2>
      <el-collapse v-model="activeDocSection">
        <el-collapse-item title="🎯 游标测量" name="cursor">
          <div class="doc-content">
            <h4>功能特点:</h4>
            <ul>
              <li><strong>双游标系统</strong>: 支持两个独立游标，精确测量任意两点间的差值</li>
              <li><strong>实时计算</strong>: 自动计算ΔX、ΔY、频率、周期、斜率等参数</li>
              <li><strong>可视化游标</strong>: 游标线实时显示在图表上，直观易用</li>
              <li><strong>高精度测量</strong>: 支持亚像素级精度的坐标定位</li>
            </ul>
            <h4>使用方法:</h4>
            <ol>
              <li>点击"游标测量"按钮激活游标模式</li>
              <li>在图表上点击第一个测量点设置游标1</li>
              <li>点击第二个测量点设置游标2</li>
              <li>查看测量结果面板中的详细数据</li>
            </ol>
          </div>
        </el-collapse-item>

        <el-collapse-item title="📈 峰值检测" name="peaks">
          <div class="doc-content">
            <h4>算法特点:</h4>
            <ul>
              <li><strong>智能峰值识别</strong>: 自动识别局部极大值和极小值</li>
              <li><strong>突出度计算</strong>: 评估峰值的显著性和重要性</li>
              <li><strong>峰宽分析</strong>: 计算峰值的半高全宽(FWHM)</li>
              <li><strong>统计分析</strong>: 提供峰值数量、平均值、峰峰值等统计信息</li>
            </ul>
            <h4>应用场景:</h4>
            <ul>
              <li>信号质量评估</li>
              <li>周期性信号分析</li>
              <li>异常值检测</li>
              <li>波形特征提取</li>
            </ul>
          </div>
        </el-collapse-item>

        <el-collapse-item title="🌊 频率分析" name="frequency">
          <div class="doc-content">
            <h4>分析能力:</h4>
            <ul>
              <li><strong>FFT频谱分析</strong>: 基于快速傅里叶变换的频域分析</li>
              <li><strong>谐波检测</strong>: 自动识别基频和各次谐波分量</li>
              <li><strong>失真分析</strong>: 计算总谐波失真(THD)和信噪比(SNR)</li>
              <li><strong>带宽测量</strong>: 自动计算信号的有效带宽</li>
            </ul>
            <h4>技术指标:</h4>
            <ul>
              <li>频率分辨率: 取决于采样率和数据长度</li>
              <li>动态范围: >60dB</li>
              <li>谐波检测: 支持10次谐波分析</li>
              <li>窗函数: 汉宁窗、汉明窗、布莱克曼窗</li>
            </ul>
          </div>
        </el-collapse-item>

        <el-collapse-item title="⚡ 自动测量" name="auto">
          <div class="doc-content">
            <h4>测量参数:</h4>
            <ul>
              <li><strong>基本参数</strong>: 频率、周期、幅度、RMS值</li>
              <li><strong>统计量</strong>: 平均值、最值、峰峰值、标准差</li>
              <li><strong>时序参数</strong>: 占空比、上升时间、下降时间、脉冲宽度</li>
              <li><strong>波形质量</strong>: 过冲、下冲、建立时间</li>
            </ul>
            <h4>算法优势:</h4>
            <ul>
              <li>自适应阈值检测</li>
              <li>噪声抑制算法</li>
              <li>边沿检测优化</li>
              <li>多种信号类型支持</li>
            </ul>
          </div>
        </el-collapse-item>
      </el-collapse>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Refresh } from '@element-plus/icons-vue'
import ProfessionalEasyChart from '@/components/charts/ProfessionalEasyChart.vue'
import type { ChartData, SeriesConfig } from '@/types/chart'
import type { 
  CursorMeasurement, 
  PeakDetectionResult, 
  FrequencyAnalysisResult, 
  AutoMeasurementResult 
} from '@/utils/measurement/MeasurementTools'

// 信号生成参数
const signalType = ref('sine')
const dataPoints = ref(500)
const frequency = ref(10)
const amplitude = ref(2)
const noiseLevel = ref(0.1)
const sampleRate = ref(1000)

// 图表数据
const chartData = ref<ChartData>({
  series: [],
  xStart: 0,
  xInterval: 0.001
})

const seriesConfigs = ref<SeriesConfig[]>([
  {
    name: '测试信号',
    color: '#409eff',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'none',
    markerSize: 4,
    visible: true
  }
])

// 测量结果
const cursorResult = ref<CursorMeasurement>()
const peakResult = ref<PeakDetectionResult>()
const frequencyResult = ref<FrequencyAnalysisResult>()
const autoResult = ref<AutoMeasurementResult>()

// UI状态
const activeResultTab = ref('cursor')
const activeDocSection = ref(['cursor'])

// 信号生成函数
const generateSignal = () => {
  const data: number[] = []
  const dt = 1 / sampleRate.value
  const omega = 2 * Math.PI * frequency.value
  
  for (let i = 0; i < dataPoints.value; i++) {
    const t = i * dt
    let value = 0
    
    switch (signalType.value) {
      case 'sine':
        value = amplitude.value * Math.sin(omega * t)
        break
        
      case 'sine-noise':
        value = amplitude.value * Math.sin(omega * t) + 
                noiseLevel.value * amplitude.value * (Math.random() - 0.5) * 2
        break
        
      case 'square':
        value = amplitude.value * Math.sign(Math.sin(omega * t))
        break
        
      case 'triangle':
        value = amplitude.value * (2 / Math.PI) * Math.asin(Math.sin(omega * t))
        break
        
      case 'sawtooth':
        value = amplitude.value * (2 * (t * frequency.value - Math.floor(t * frequency.value + 0.5)))
        break
        
      case 'composite':
        value = amplitude.value * (
          Math.sin(omega * t) + 
          0.3 * Math.sin(3 * omega * t) + 
          0.1 * Math.sin(5 * omega * t)
        )
        break
        
      case 'pulse':
        const pulseWidth = 0.1 / frequency.value
        const period = 1 / frequency.value
        const phase = (t % period) / period
        value = phase < pulseWidth ? amplitude.value : 0
        break
        
      case 'modulated':
        const carrierFreq = frequency.value * 10
        const modFreq = frequency.value
        value = amplitude.value * Math.sin(2 * Math.PI * carrierFreq * t) * 
                (1 + 0.5 * Math.sin(2 * Math.PI * modFreq * t))
        break
    }
    
    // 添加噪声
    if (signalType.value !== 'sine-noise') {
      value += noiseLevel.value * amplitude.value * (Math.random() - 0.5) * 2
    }
    
    data.push(value)
  }
  
  chartData.value = {
    series: data,
    xStart: 0,
    xInterval: dt
  }
}

// 测量结果处理函数
const onCursorMeasurement = (result: CursorMeasurement) => {
  cursorResult.value = result
  activeResultTab.value = 'cursor'
}

const onPeakDetection = (result: PeakDetectionResult) => {
  peakResult.value = result
  activeResultTab.value = 'peaks'
}

const onFrequencyAnalysis = (result: FrequencyAnalysisResult) => {
  frequencyResult.value = result
  activeResultTab.value = 'frequency'
}

const onAutoMeasurement = (result: AutoMeasurementResult) => {
  autoResult.value = result
  activeResultTab.value = 'auto'
}

// 初始化
onMounted(() => {
  generateSignal()
})
</script>

<style lang="scss" scoped>
.professional-easy-chart-test {
  padding: 20px;
  max-width: 1400px;
  margin: 0 auto;
  
  .page-header {
    text-align: center;
    margin-bottom: 30px;
    
    h1 {
      color: var(--primary-color);
      margin-bottom: 10px;
    }
    
    p {
      color: var(--text-secondary);
      font-size: 16px;
    }
  }
  
  .signal-generator {
    margin-bottom: 30px;
    padding: 20px;
    background: var(--surface-color);
    border-radius: 12px;
    border: 1px solid var(--border-color);
    
    h2 {
      margin-bottom: 20px;
      color: var(--text-primary);
    }
    
    .generator-controls {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 20px;
      align-items: end;
      
      .control-group {
        display: flex;
        flex-direction: column;
        gap: 8px;
        
        label {
          font-weight: 500;
          color: var(--text-primary);
          font-size: 14px;
        }
      }
    }
  }
  
  .chart-section {
    margin-bottom: 30px;
    
    h2 {
      margin-bottom: 20px;
      color: var(--text-primary);
    }
  }
  
  .results-section {
    margin-bottom: 30px;
    
    h2 {
      margin-bottom: 20px;
      color: var(--text-primary);
    }
    
    .result-content {
      padding: 20px;
      
      .result-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
        gap: 16px;
        
        .result-item {
          display: flex;
          justify-content: space-between;
          padding: 12px 16px;
          background: white;
          border-radius: 8px;
          border: 1px solid var(--border-color);
          
          .label {
            font-weight: 500;
            color: var(--text-secondary);
          }
          
          .value {
            font-family: 'Consolas', 'Monaco', monospace;
            color: var(--primary-color);
            font-weight: bold;
          }
        }
      }
      
      .peak-summary, .frequency-summary {
        margin-bottom: 24px;
        
        h4 {
          margin-bottom: 16px;
          color: var(--text-primary);
          border-bottom: 2px solid var(--primary-color);
          padding-bottom: 8px;
        }
        
        .summary-grid, .freq-grid {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
          gap: 12px;
          
          .summary-item, .freq-item {
            display: flex;
            justify-content: space-between;
            padding: 10px 14px;
            background: white;
            border-radius: 6px;
            border: 1px solid var(--border-color);
            
            .label {
              font-size: 14px;
              color: var(--text-secondary);
            }
            
            .value {
              font-family: 'Consolas', 'Monaco', monospace;
              color: var(--primary-color);
              font-weight: bold;
              font-size: 14px;
            }
          }
        }
      }
      
      .peak-details, .harmonics-details {
        h4 {
          margin-bottom: 16px;
          color: var(--text-primary);
        }
      }
      
      .auto-measurement-results {
        .measurement-category {
          margin-bottom: 24px;
          
          h4 {
            margin-bottom: 16px;
            color: var(--text-primary);
            border-bottom: 2px solid var(--primary-color);
            padding-bottom: 8px;
          }
          
          .param-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 12px;
            
            .param-item {
              display: flex;
              justify-content: space-between;
              padding: 10px 14px;
              background: white;
              border-radius: 6px;
              border: 1px solid var(--border-color);
              
              .label {
                font-size: 14px;
                color: var(--text-secondary);
              }
              
              .value {
                font-family: 'Consolas', 'Monaco', monospace;
                color: var(--primary-color);
                font-weight: bold;
                font-size: 14px;
              }
            }
          }
        }
      }
    }
    
    .no-result {
      text-align: center;
      padding: 40px 20px;
      color: var(--text-secondary);
      font-style: italic;
    }
  }
  
  .documentation {
    .doc-content {
      padding: 16px;
      
      h4 {
        margin-bottom: 12px;
        color: var(--primary-color);
      }
      
      ul, ol {
        margin-bottom: 16px;
        padding-left: 20px;
        
        li {
          margin-bottom: 8px;
          line-height: 1.6;
          
          strong {
            color: var(--primary-color);
          }
        }
      }
    }
  }
}

@media (max-width: 768px) {
  .professional-easy-chart-test {
    padding: 16px;
    
    .signal-generator {
      .generator-controls {
        grid-template-columns: 1fr;
      }
    }
    
    .results-section {
      .result-content {
        .result-grid {
          grid-template-columns: 1fr;
        }
        
        .auto-measurement-results {
          .measurement-category {
            .param-grid {
              grid-template-columns: 1fr;
            }
          }
        }
        
        .peak-summary, .frequency-summary {
          .summary-grid, .freq-grid {
            grid-template-columns: 1fr;
          }
        }
      }
    }
  }
}
</style>
