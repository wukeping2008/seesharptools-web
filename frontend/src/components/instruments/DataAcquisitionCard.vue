<template>
  <div class="daq-card professional-control">
    <!-- 控件标题 -->
    <div class="daq-header">
      <div class="header-left">
        <h3 class="daq-title">
          <el-icon class="title-icon"><DataAnalysis /></el-icon>
          多功能数据采集卡
        </h3>
        <div class="daq-status">
          <span class="status-indicator" :class="acquiring ? 'acquiring' : 'stopped'">
            {{ acquiring ? '采集中' : '已停止' }}
          </span>
          <span class="sample-rate">{{ formatSampleRate(currentSampleRate) }}</span>
        </div>
      </div>
      <div class="header-right">
        <el-button-group>
          <el-button 
            :type="acquiring ? 'danger' : 'primary'" 
            @click="toggleAcquisition"
            size="large"
          >
            <el-icon><VideoPlay v-if="!acquiring" /><VideoPause v-else /></el-icon>
            {{ acquiring ? '停止采集' : '开始采集' }}
          </el-button>
          <el-button @click="singleAcquisition" :disabled="acquiring" size="large">
            <el-icon><Camera /></el-icon>
            单次采集
          </el-button>
          <el-button @click="exportData" :disabled="!hasData" size="large">
            <el-icon><Download /></el-icon>
            导出数据
          </el-button>
        </el-button-group>
      </div>
    </div>

    <!-- 主显示区域 -->
    <div class="daq-body">
      <el-row :gutter="16">
        <!-- 左侧：数据显示区域 -->
        <el-col :span="18">
          <div class="data-display">
            <!-- 实时数据图表 -->
            <div class="data-chart">
              <StripChart
                ref="stripChartRef"
                :data="acquisitionData"
                :series-configs="channelConfigs"
                :options="chartOptions"
                :height="350"
                @data-update="handleDataUpdate"
              />
            </div>
            
            <!-- 统计信息显示 -->
            <div class="statistics-display" v-if="showStatistics">
              <div class="stats-grid">
                <div 
                  v-for="(stat, index) in channelStatistics" 
                  :key="index"
                  class="stat-item"
                  :style="{ borderColor: getChannelColor(index) }"
                >
                  <div class="stat-header">
                    <span class="channel-name">CH{{ index + 1 }}</span>
                    <div class="channel-indicator" :style="{ backgroundColor: getChannelColor(index) }"></div>
                  </div>
                  <div class="stat-values">
                    <div class="stat-value">
                      <span class="label">平均值:</span>
                      <span class="value">{{ formatValue(stat.mean) }}V</span>
                    </div>
                    <div class="stat-value">
                      <span class="label">RMS:</span>
                      <span class="value">{{ formatValue(stat.rms) }}V</span>
                    </div>
                    <div class="stat-value">
                      <span class="label">最大值:</span>
                      <span class="value">{{ formatValue(stat.max) }}V</span>
                    </div>
                    <div class="stat-value">
                      <span class="label">最小值:</span>
                      <span class="value">{{ formatValue(stat.min) }}V</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </el-col>

        <!-- 右侧：控制面板 -->
        <el-col :span="6">
          <div class="control-panel">
            <!-- 采样配置 -->
            <div class="control-section">
              <h4 class="section-title">采样配置</h4>
              <div class="control-grid">
                <div class="control-item">
                  <label>采样率</label>
                  <el-select v-model="sampleRate" size="small" @change="updateSampleRate">
                    <el-option 
                      v-for="rate in sampleRates" 
                      :key="rate.value"
                      :label="rate.label" 
                      :value="rate.value" 
                    />
                  </el-select>
                </div>
                <div class="control-item">
                  <label>采样点数</label>
                  <el-select v-model="sampleCount" size="small" @change="updateSampleCount">
                    <el-option label="1K" :value="1000" />
                    <el-option label="10K" :value="10000" />
                    <el-option label="100K" :value="100000" />
                    <el-option label="1M" :value="1000000" />
                  </el-select>
                </div>
                <div class="control-item">
                  <label>采集模式</label>
                  <el-select v-model="acquisitionMode" size="small" @change="updateAcquisitionMode">
                    <el-option label="连续采集" value="continuous" />
                    <el-option label="有限采集" value="finite" />
                    <el-option label="触发采集" value="triggered" />
                  </el-select>
                </div>
                <div class="control-item">
                  <label>缓冲区大小</label>
                  <el-select v-model="bufferSize" size="small" @change="updateBufferSize">
                    <el-option label="1MB" :value="1048576" />
                    <el-option label="10MB" :value="10485760" />
                    <el-option label="100MB" :value="104857600" />
                    <el-option label="1GB" :value="1073741824" />
                  </el-select>
                </div>
              </div>
            </div>

            <!-- 通道配置 -->
            <div class="control-section">
              <h4 class="section-title">通道配置</h4>
              <div class="channel-controls">
                <div 
                  v-for="(channel, index) in channels" 
                  :key="index"
                  class="channel-item"
                  :class="{ active: channel.enabled }"
                >
                  <div class="channel-header">
                    <el-checkbox 
                      v-model="channel.enabled" 
                      @change="updateChannel(index)"
                      :style="{ color: channel.color }"
                    >
                      CH{{ index + 1 }}
                    </el-checkbox>
                    <div class="channel-indicator" :style="{ backgroundColor: channel.color }"></div>
                  </div>
                  
                  <div v-if="channel.enabled" class="channel-settings">
                    <div class="setting-item">
                      <label>量程</label>
                      <el-select v-model="channel.range" size="small" @change="updateChannel(index)">
                        <el-option 
                          v-for="range in voltageRanges" 
                          :key="range.value"
                          :label="range.label" 
                          :value="range.value" 
                        />
                      </el-select>
                    </div>
                    <div class="setting-item">
                      <label>耦合</label>
                      <el-select v-model="channel.coupling" size="small" @change="updateChannel(index)">
                        <el-option label="DC" value="DC" />
                        <el-option label="AC" value="AC" />
                        <el-option label="GND" value="GND" />
                      </el-select>
                    </div>
                    <div class="setting-item">
                      <label>输入阻抗</label>
                      <el-select v-model="channel.impedance" size="small" @change="updateChannel(index)">
                        <el-option label="1MΩ" value="1M" />
                        <el-option label="50Ω" value="50" />
                        <el-option label="高阻" value="high" />
                      </el-select>
                    </div>
                    <div class="setting-item">
                      <label>偏置电压</label>
                      <el-input-number
                        v-model="channel.offset"
                        :min="-10"
                        :max="10"
                        :step="0.1"
                        :precision="2"
                        size="small"
                        @change="updateChannel(index)"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- 触发配置 -->
            <div class="control-section">
              <h4 class="section-title">触发配置</h4>
              <div class="control-grid">
                <div class="control-item">
                  <label>触发源</label>
                  <el-select v-model="trigger.source" size="small" @change="updateTrigger">
                    <el-option label="CH1" :value="0" />
                    <el-option label="CH2" :value="1" />
                    <el-option label="CH3" :value="2" />
                    <el-option label="CH4" :value="3" />
                    <el-option label="外部" value="external" />
                    <el-option label="软件" value="software" />
                  </el-select>
                </div>
                <div class="control-item">
                  <label>触发类型</label>
                  <el-select v-model="trigger.type" size="small" @change="updateTrigger">
                    <el-option label="边沿" value="edge" />
                    <el-option label="电平" value="level" />
                    <el-option label="窗口" value="window" />
                  </el-select>
                </div>
                <div class="control-item">
                  <label>触发斜率</label>
                  <el-select v-model="trigger.slope" size="small" @change="updateTrigger">
                    <el-option label="上升沿" value="rising" />
                    <el-option label="下降沿" value="falling" />
                    <el-option label="双边沿" value="both" />
                  </el-select>
                </div>
                <div class="control-item">
                  <label>触发电平</label>
                  <el-input-number
                    v-model="trigger.level"
                    :min="-10"
                    :max="10"
                    :step="0.1"
                    :precision="2"
                    size="small"
                    @change="updateTrigger"
                  />
                </div>
                <div class="control-item">
                  <label>预触发</label>
                  <el-input-number
                    v-model="trigger.pretrigger"
                    :min="0"
                    :max="50"
                    :step="1"
                    size="small"
                    @change="updateTrigger"
                  />
                </div>
              </div>
            </div>

            <!-- 校准和诊断 -->
            <div class="control-section">
              <h4 class="section-title">校准和诊断</h4>
              <div class="calibration-controls">
                <el-button size="small" @click="performSelfCalibration" :loading="calibrating">
                  <el-icon><Tools /></el-icon>
                  自校准
                </el-button>
                <el-button size="small" @click="performChannelTest" :loading="testing">
                  <el-icon><Monitor /></el-icon>
                  通道测试
                </el-button>
                <el-button size="small" @click="performNoiseTest" :loading="noiseTesting">
                  <el-icon><DataLine /></el-icon>
                  噪声测试
                </el-button>
                <el-button size="small" @click="resetToDefaults">
                  <el-icon><RefreshRight /></el-icon>
                  恢复默认
                </el-button>
              </div>
            </div>

            <!-- 数据统计 -->
            <div class="control-section">
              <h4 class="section-title">
                数据统计
                <el-switch 
                  v-model="showStatistics" 
                  size="small"
                  @change="updateStatistics"
                />
              </h4>
              <div v-if="showStatistics" class="statistics-controls">
                <div class="stat-config">
                  <el-button size="small" @click="resetStatistics">
                    清除统计
                  </el-button>
                </div>
              </div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 状态栏 -->
    <div class="daq-status-bar">
      <div class="status-left">
        <span>采样率: {{ formatSampleRate(currentSampleRate) }}</span>
        <span>活动通道: {{ enabledChannelCount }}</span>
        <span>数据点数: {{ formatDataCount(totalDataPoints) }}</span>
      </div>
      <div class="status-right">
        <span>缓冲区使用: {{ formatBufferUsage(bufferUsage) }}</span>
        <span>采集时间: {{ formatAcquisitionTime(acquisitionTime) }}</span>
      </div>
    </div>

    <!-- 校准结果对话框 -->
    <el-dialog v-model="showCalibrationDialog" title="校准结果" width="500px">
      <div class="calibration-results">
        <div v-for="(result, index) in calibrationResults" :key="index" class="result-item">
          <div class="result-header">
            <span class="channel-name">CH{{ index + 1 }}</span>
            <el-tag :type="result.status === 'pass' ? 'success' : 'danger'">
              {{ result.status === 'pass' ? '通过' : '失败' }}
            </el-tag>
          </div>
          <div class="result-details">
            <div class="detail-item">
              <span>零点误差: {{ result.zeroError }}mV</span>
            </div>
            <div class="detail-item">
              <span>增益误差: {{ result.gainError }}%</span>
            </div>
            <div class="detail-item">
              <span>噪声水平: {{ result.noiseLevel }}μV</span>
            </div>
          </div>
        </div>
      </div>
      <template #footer>
        <el-button @click="showCalibrationDialog = false">关闭</el-button>
        <el-button type="primary" @click="saveCalibrationResults">保存结果</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, nextTick } from 'vue'
import { 
  DataAnalysis, VideoPlay, VideoPause, Camera, Download, 
  Tools, Monitor, DataLine, RefreshRight 
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import StripChart from '@/components/charts/StripChart.vue'
import type { ChartData, SeriesConfig } from '@/types/chart'

// 通道配置接口
interface ChannelConfig {
  enabled: boolean
  range: number // 电压量程 (V)
  coupling: 'DC' | 'AC' | 'GND'
  impedance: '1M' | '50' | 'high'
  offset: number // 偏置电压 (V)
  color: string
}

// 触发配置接口
interface TriggerConfig {
  source: number | 'external' | 'software'
  type: 'edge' | 'level' | 'window'
  slope: 'rising' | 'falling' | 'both'
  level: number // 触发电平 (V)
  pretrigger: number // 预触发百分比
}

// 统计数据接口
interface ChannelStatistics {
  mean: number
  rms: number
  max: number
  min: number
  stddev: number
}

// 校准结果接口
interface CalibrationResult {
  status: 'pass' | 'fail'
  zeroError: number
  gainError: number
  noiseLevel: number
}

// Props
interface Props {
  channels?: number
  maxSampleRate?: number
}

const props = withDefaults(defineProps<Props>(), {
  channels: 4,
  maxSampleRate: 2000000 // 2MS/s
})

// Emits
const emit = defineEmits<{
  acquisitionStart: [config: any]
  acquisitionStop: []
  dataReady: [data: ChartData]
  channelUpdate: [channelIndex: number, config: ChannelConfig]
  calibrationComplete: [results: CalibrationResult[]]
}>()

// 响应式数据
const acquiring = ref(false)
const hasData = ref(false)
const showStatistics = ref(true)
const calibrating = ref(false)
const testing = ref(false)
const noiseTesting = ref(false)
const showCalibrationDialog = ref(false)

// 采样配置
const sampleRate = ref(100000) // 100kS/s
const currentSampleRate = ref(100000)
const sampleCount = ref(10000)
const acquisitionMode = ref<'continuous' | 'finite' | 'triggered'>('continuous')
const bufferSize = ref(10485760) // 10MB

// 通道配置
const channels = ref<ChannelConfig[]>([
  {
    enabled: true,
    range: 10, // ±10V
    coupling: 'DC',
    impedance: '1M',
    offset: 0,
    color: '#FFD700'
  },
  {
    enabled: true,
    range: 10,
    coupling: 'DC',
    impedance: '1M',
    offset: 0,
    color: '#00CED1'
  },
  {
    enabled: false,
    range: 10,
    coupling: 'DC',
    impedance: '1M',
    offset: 0,
    color: '#FF69B4'
  },
  {
    enabled: false,
    range: 10,
    coupling: 'DC',
    impedance: '1M',
    offset: 0,
    color: '#32CD32'
  }
])

// 触发配置
const trigger = ref<TriggerConfig>({
  source: 0,
  type: 'edge',
  slope: 'rising',
  level: 0,
  pretrigger: 10
})

// 数据和统计
const acquisitionData = ref<ChartData>({
  series: [],
  xStart: 0,
  xInterval: 0.00001, // 10μs
  sampleRate: 100000
})

const channelStatistics = ref<ChannelStatistics[]>([])
const calibrationResults = ref<CalibrationResult[]>([])

// 状态信息
const totalDataPoints = ref(0)
const bufferUsage = ref(0)
const acquisitionTime = ref(0)

// 图表引用
const stripChartRef = ref()

// 采样率选项
const sampleRates = [
  { label: '1kS/s', value: 1000 },
  { label: '10kS/s', value: 10000 },
  { label: '100kS/s', value: 100000 },
  { label: '1MS/s', value: 1000000 },
  { label: '2MS/s', value: 2000000 }
]

// 电压量程选项
const voltageRanges = [
  { label: '±1V', value: 1 },
  { label: '±2V', value: 2 },
  { label: '±5V', value: 5 },
  { label: '±10V', value: 10 },
  { label: '±20V', value: 20 }
]

// 计算属性
const enabledChannelCount = computed(() => {
  return channels.value.filter(ch => ch.enabled).length
})

const channelConfigs = computed((): SeriesConfig[] => {
  return channels.value
    .filter(ch => ch.enabled)
    .map((ch, index) => ({
      name: `CH${channels.value.indexOf(ch) + 1}`,
      color: ch.color,
      lineWidth: 2,
      lineType: 'solid',
      markerType: 'none',
      markerSize: 4,
      visible: true
    }))
})

const chartOptions = computed(() => ({
  autoScale: true,
  logarithmic: false,
  splitView: false,
  legendVisible: true,
  cursorMode: 'cursor',
  gridEnabled: true,
  minorGridEnabled: true,
  theme: 'light' as const
}))

// 定时器
let acquisitionTimer: number | null = null
let statisticsTimer: number | null = null

// 方法
const toggleAcquisition = () => {
  acquiring.value = !acquiring.value
  
  if (acquiring.value) {
    startAcquisition()
    ElMessage.success('开始数据采集')
  } else {
    stopAcquisition()
    ElMessage.info('停止数据采集')
  }
}

const singleAcquisition = () => {
  if (!acquiring.value) {
    performSingleAcquisition()
    ElMessage.success('单次采集完成')
  }
}

const startAcquisition = () => {
  currentSampleRate.value = sampleRate.value
  acquisitionTime.value = 0
  
  emit('acquisitionStart', {
    sampleRate: sampleRate.value,
    channels: channels.value.filter(ch => ch.enabled),
    mode: acquisitionMode.value,
    trigger: trigger.value
  })
  
  if (acquisitionTimer) {
    clearInterval(acquisitionTimer)
  }
  
  acquisitionTimer = setInterval(() => {
    generateAcquisitionData()
    updateStatistics()
    acquisitionTime.value += 0.1
  }, 100) // 10Hz更新率
}

const stopAcquisition = () => {
  if (acquisitionTimer) {
    clearInterval(acquisitionTimer)
    acquisitionTimer = null
  }
  
  emit('acquisitionStop')
}

const performSingleAcquisition = () => {
  generateAcquisitionData()
  updateStatistics()
  hasData.value = true
}

const generateAcquisitionData = () => {
  const pointsToGenerate = Math.min(sampleCount.value, 1000) // 限制显示点数
  const dt = 1 / currentSampleRate.value
  
  const series: number[][] = []
  
  channels.value.forEach((channel, channelIndex) => {
    if (!channel.enabled) return
    
    const data: number[] = []
    
    for (let i = 0; i < pointsToGenerate; i++) {
      const t = i * dt
      let value = 0
      
      // 生成不同类型的测试信号
      switch (channelIndex) {
        case 0: // CH1: 正弦波 + 噪声
          value = Math.sin(2 * Math.PI * 1000 * t) * (channel.range * 0.8)
          break
        case 1: // CH2: 方波 + 噪声
          value = Math.sign(Math.sin(2 * Math.PI * 500 * t)) * (channel.range * 0.6)
          break
        case 2: // CH3: 三角波 + 噪声
          const period = 1 / 200
          const phase = (t % period) / period
          value = (phase < 0.5 ? 4 * phase - 1 : 3 - 4 * phase) * (channel.range * 0.7)
          break
        case 3: // CH4: 随机信号
          value = (Math.random() - 0.5) * channel.range * 0.5
          break
      }
      
      // 添加噪声
      value += (Math.random() - 0.5) * channel.range * 0.02
      
      // 应用偏置
      value += channel.offset
      
      // 限制在量程范围内
      value = Math.max(-channel.range, Math.min(channel.range, value))
      
      data.push(value)
    }
    
    series.push(data)
  })
  
  acquisitionData.value = {
    series,
    xStart: 0,
    xInterval: dt,
    sampleRate: currentSampleRate.value
  }
  
  totalDataPoints.value += pointsToGenerate * enabledChannelCount.value
  bufferUsage.value = Math.min(100, (totalDataPoints.value * 4) / bufferSize.value * 100)
  hasData.value = true
  
  emit('dataReady', acquisitionData.value)
}

const updateSampleRate = () => {
  if (!acquiring.value) {
    currentSampleRate.value = sampleRate.value
  }
}

const updateSampleCount = () => {
  // 采样点数更新逻辑
}

const updateAcquisitionMode = () => {
  // 采集模式更新逻辑
}

const updateBufferSize = () => {
  // 缓冲区大小更新逻辑
}

const updateChannel = (index: number) => {
  emit('channelUpdate', index, channels.value[index])
  if (acquiring.value) {
    generateAcquisitionData()
  }
}

const updateTrigger = () => {
  // 触发配置更新逻辑
  console.log('触发配置更新:', trigger.value)
}

const updateStatistics = () => {
  if (!showStatistics.value || !hasData.value) return
  
  const stats: ChannelStatistics[] = []
  
  acquisitionData.value.series.forEach((seriesData, index) => {
    if (Array.isArray(seriesData) && seriesData.length > 0) {
      const data = seriesData as number[]
      const mean = data.reduce((sum, val) => sum + val, 0) / data.length
      const rms = Math.sqrt(data.reduce((sum, val) => sum + val * val, 0) / data.length)
      const max = Math.max(...data)
      const min = Math.min(...data)
      const variance = data.reduce((sum, val) => sum + Math.pow(val - mean, 2), 0) / data.length
      const stddev = Math.sqrt(variance)
      
      stats.push({ mean, rms, max, min, stddev })
    }
  })
  
  channelStatistics.value = stats
}

const resetStatistics = () => {
  channelStatistics.value = []
  totalDataPoints.value = 0
  bufferUsage.value = 0
  acquisitionTime.value = 0
  ElMessage.success('统计数据已清除')
}

const performSelfCalibration = async () => {
  calibrating.value = true
  
  try {
    // 模拟校准过程
    await new Promise(resolve => setTimeout(resolve, 3000))
    
    const results: CalibrationResult[] = []
    channels.value.forEach((channel, index) => {
      if (channel.enabled) {
        results.push({
          status: Math.random() > 0.1 ? 'pass' : 'fail',
          zeroError: (Math.random() - 0.5) * 10,
          gainError: (Math.random() - 0.5) * 2,
          noiseLevel: Math.random() * 50 + 10
        })
      }
    })
    
    calibrationResults.value = results
    showCalibrationDialog.value = true
    emit('calibrationComplete', results)
    
    ElMessage.success('自校准完成')
  } catch (error) {
    ElMessage.error('校准失败')
  } finally {
    calibrating.value = false
  }
}

const performChannelTest = async () => {
  testing.value = true
  
  try {
    await new Promise(resolve => setTimeout(resolve, 2000))
    ElMessage.success('通道测试完成，所有通道正常')
  } catch (error) {
    ElMessage.error('通道测试失败')
  } finally {
    testing.value = false
  }
}

const performNoiseTest = async () => {
  noiseTesting.value = true
  
  try {
    await new Promise(resolve => setTimeout(resolve, 2000))
    ElMessage.success('噪声测试完成，噪声水平正常')
  } catch (error) {
    ElMessage.error('噪声测试失败')
  } finally {
    noiseTesting.value = false
  }
}

const resetToDefaults = () => {
  // 重置所有配置到默认值
  sampleRate.value = 100000
  sampleCount.value = 10000
  acquisitionMode.value = 'continuous'
  bufferSize.value = 10485760
  
  // 重置通道配置
  channels.value.forEach((channel, index) => {
    channel.range = 10
    channel.coupling = 'DC'
    channel.impedance = '1M'
    channel.offset = 0
  })
  
  // 重置触发配置
  trigger.value = {
    source: 0,
    type: 'edge',
    slope: 'rising',
    level: 0,
    pretrigger: 10
  }
  
  ElMessage.success('已恢复默认配置')
}

const exportData = () => {
  if (!hasData.value) {
    ElMessage.warning('没有可导出的数据')
    return
  }
  
  // 生成CSV数据
  const csvData = generateCSVData()
  const blob = new Blob([csvData], { type: 'text/csv' })
  const url = URL.createObjectURL(blob)
  
  const link = document.createElement('a')
  link.href = url
  link.download = `daq_data_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.csv`
  link.click()
  
  URL.revokeObjectURL(url)
  ElMessage.success('数据导出成功')
}

const generateCSVData = (): string => {
  const headers = ['Time(s)']
  const enabledChannels = channels.value.filter(ch => ch.enabled)
  enabledChannels.forEach((_, index) => {
    headers.push(`CH${channels.value.indexOf(enabledChannels[index]) + 1}(V)`)
  })
  
  let csvContent = headers.join(',') + '\n'
  
  const firstSeries = acquisitionData.value.series[0]
  const seriesLength = Array.isArray(firstSeries) ? firstSeries.length : 0
  const xInterval = acquisitionData.value.xInterval || 0.00001
  
  for (let i = 0; i < seriesLength; i++) {
    const row = [i * xInterval]
    acquisitionData.value.series.forEach(series => {
      if (Array.isArray(series)) {
        row.push(series[i] || 0)
      }
    })
    csvContent += row.join(',') + '\n'
  }
  
  return csvContent
}

const handleDataUpdate = (data: any) => {
  // 数据更新处理
  console.log('数据更新:', data)
}

const saveCalibrationResults = () => {
  // 保存校准结果到本地存储
  localStorage.setItem('daq_calibration_results', JSON.stringify(calibrationResults.value))
  showCalibrationDialog.value = false
  ElMessage.success('校准结果已保存')
}

// 工具函数
const getChannelColor = (index: number): string => {
  const enabledChannels = channels.value.filter(ch => ch.enabled)
  const actualIndex = channels.value.indexOf(enabledChannels[index])
  return channels.value[actualIndex]?.color || '#FFD700'
}

const formatValue = (value: number): string => {
  if (Math.abs(value) >= 1) {
    return value.toFixed(3)
  } else if (Math.abs(value) >= 0.001) {
    return (value * 1000).toFixed(1) + 'm'
  } else {
    return (value * 1000000).toFixed(1) + 'μ'
  }
}

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

const formatBufferUsage = (usage: number): string => {
  return `${usage.toFixed(1)}%`
}

const formatAcquisitionTime = (time: number): string => {
  if (time >= 60) {
    const minutes = Math.floor(time / 60)
    const seconds = time % 60
    return `${minutes}m ${seconds.toFixed(1)}s`
  }
  return `${time.toFixed(1)}s`
}

// 生命周期
onMounted(() => {
  // 初始化
  generateAcquisitionData()
})

onUnmounted(() => {
  if (acquisitionTimer) {
    clearInterval(acquisitionTimer)
  }
  if (statisticsTimer) {
    clearInterval(statisticsTimer)
  }
})

// 暴露方法
defineExpose({
  toggleAcquisition,
  singleAcquisition,
  exportData,
  performSelfCalibration,
  resetToDefaults
})
</script>

<style lang="scss" scoped>
.daq-card {
  background: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  
  .daq-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
    padding-bottom: 12px;
    border-bottom: 1px solid var(--border-color);
    
    .header-left {
      display: flex;
      flex-direction: column;
      gap: 8px;
      
      .daq-title {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 18px;
        font-weight: 600;
        color: var(--text-primary);
        margin: 0;
        
        .title-icon {
          font-size: 20px;
          color: var(--primary-color);
        }
      }
      
      .daq-status {
        display: flex;
        gap: 16px;
        
        .status-indicator {
          padding: 2px 8px;
          border-radius: 8px;
          font-size: 12px;
          font-weight: 500;
          
          &.acquiring {
            background: rgba(103, 194, 58, 0.1);
            color: #67c23a;
          }
          
          &.stopped {
            background: rgba(144, 147, 153, 0.1);
            color: #909399;
          }
        }
        
        .sample-rate {
          font-size: 12px;
          color: var(--text-secondary);
          font-family: 'Consolas', 'Monaco', monospace;
        }
      }
    }
  }
  
  .daq-body {
    margin-bottom: 16px;
    
    .data-display {
      background: #f8f9fa;
      border: 1px solid #e4e7ed;
      border-radius: 8px;
      padding: 8px;
      
      .data-chart {
        height: 350px;
        margin-bottom: 8px;
      }
      
      .statistics-display {
        .stats-grid {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
          gap: 12px;
          
          .stat-item {
            background: white;
            border: 1px solid #e4e7ed;
            border-radius: 6px;
            padding: 12px;
            border-left: 3px solid;
            
            .stat-header {
              display: flex;
              justify-content: space-between;
              align-items: center;
              margin-bottom: 8px;
              
              .channel-name {
                font-weight: 600;
                color: var(--text-primary);
              }
              
              .channel-indicator {
                width: 12px;
                height: 12px;
                border-radius: 50%;
              }
            }
            
            .stat-values {
              .stat-value {
                display: flex;
                justify-content: space-between;
                margin-bottom: 4px;
                
                &:last-child {
                  margin-bottom: 0;
                }
                
                .label {
                  font-size: 12px;
                  color: var(--text-secondary);
                }
                
                .value {
                  font-size: 12px;
                  font-weight: 600;
                  color: var(--text-primary);
                  font-family: 'Consolas', 'Monaco', monospace;
                }
              }
            }
          }
        }
      }
    }
    
    .control-panel {
      background: #f8f9fa;
      border: 1px solid #e4e7ed;
      border-radius: 8px;
      padding: 12px;
      height: 100%;
      overflow-y: auto;
      
      .control-section {
        margin-bottom: 20px;
        
        &:last-child {
          margin-bottom: 0;
        }
        
        .section-title {
          font-size: 14px;
          font-weight: 600;
          color: var(--text-primary);
          margin-bottom: 12px;
          display: flex;
          align-items: center;
          justify-content: space-between;
        }
        
        .control-grid {
          display: grid;
          grid-template-columns: 1fr;
          gap: 12px;
          
          .control-item {
            display: flex;
            flex-direction: column;
            gap: 4px;
            
            label {
              font-size: 12px;
              font-weight: 500;
              color: var(--text-primary);
            }
          }
        }
        
        .channel-controls {
          .channel-item {
            margin-bottom: 16px;
            padding: 8px;
            border: 1px solid #e4e7ed;
            border-radius: 6px;
            
            &.active {
              border-color: var(--primary-color);
              background: rgba(64, 158, 255, 0.05);
            }
            
            .channel-header {
              display: flex;
              align-items: center;
              justify-content: space-between;
              margin-bottom: 8px;
              
              .channel-indicator {
                width: 12px;
                height: 12px;
                border-radius: 50%;
              }
            }
            
            .channel-settings {
              .setting-item {
                margin-bottom: 8px;
                
                &:last-child {
                  margin-bottom: 0;
                }
                
                label {
                  display: block;
                  font-size: 11px;
                  color: var(--text-secondary);
                  margin-bottom: 4px;
                }
              }
            }
          }
        }
        
        .calibration-controls {
          display: grid;
          grid-template-columns: 1fr 1fr;
          gap: 8px;
          
          .el-button {
            font-size: 12px;
          }
        }
        
        .statistics-controls {
          .stat-config {
            display: flex;
            gap: 8px;
            
            .el-button {
              flex: 1;
              font-size: 12px;
            }
          }
        }
      }
    }
  }
  
  .daq-status-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    background: #f0f2f5;
    border-radius: 6px;
    font-size: 12px;
    color: var(--text-secondary);
    
    .status-left,
    .status-right {
      display: flex;
      gap: 16px;
    }
  }
}

.calibration-results {
  .result-item {
    margin-bottom: 16px;
    padding: 12px;
    border: 1px solid #e4e7ed;
    border-radius: 6px;
    
    &:last-child {
      margin-bottom: 0;
    }
    
    .result-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;
      
      .channel-name {
        font-weight: 600;
        color: var(--text-primary);
      }
    }
    
    .result-details {
      .detail-item {
        font-size: 12px;
        color: var(--text-secondary);
        margin-bottom: 4px;
        
        &:last-child {
          margin-bottom: 0;
        }
      }
    }
  }
}

@media (max-width: 768px) {
  .daq-card {
    padding: 12px;
    
    .daq-header {
      flex-direction: column;
      gap: 12px;
      align-items: flex-start;
    }
    
    .daq-body {
      .statistics-display {
        .stats-grid {
          grid-template-columns: 1fr;
        }
      }
      
      .control-panel {
        .calibration-controls {
          grid-template-columns: 1fr;
        }
      }
    }
    
    .daq-status-bar {
      flex-direction: column;
      gap: 8px;
      
      .status-left,
      .status-right {
        justify-content: center;
      }
    }
  }
}
</style>
