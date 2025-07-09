<template>
  <div class="oscilloscope professional-control">
    <!-- 控件标题 -->
    <div class="oscilloscope-header">
      <div class="header-left">
        <h3 class="oscilloscope-title">
          <el-icon class="title-icon"><TrendCharts /></el-icon>
          数字示波器
        </h3>
        <div class="oscilloscope-status">
          <span class="status-indicator" :class="running ? 'running' : 'stopped'">
            {{ running ? '运行中' : '已停止' }}
          </span>
          <span class="trigger-status" :class="triggerStatus">
            {{ getTriggerStatusText() }}
          </span>
        </div>
      </div>
      <div class="header-right">
        <el-button-group>
          <el-button 
            :type="running ? 'danger' : 'primary'" 
            @click="toggleRunning"
            size="large"
          >
            <el-icon><VideoPlay v-if="!running" /><VideoPause v-else /></el-icon>
            {{ running ? '停止' : '运行' }}
          </el-button>
          <el-button @click="singleCapture" :disabled="running" size="large">
            <el-icon><Camera /></el-icon>
            单次
          </el-button>
        </el-button-group>
      </div>
    </div>

    <!-- 主显示区域 -->
    <div class="oscilloscope-body">
      <el-row :gutter="16">
        <!-- 左侧：波形显示区域 -->
        <el-col :span="18">
          <div class="waveform-display">
            <!-- 波形图表 -->
            <div class="waveform-chart">
              <StripChart
                ref="stripChartRef"
                :data="waveformData"
                :series-configs="channelConfigs"
                :options="chartOptions"
                :height="400"
                @cursor-move="handleCursorMove"
                @zoom="handleZoom"
              />
            </div>
            
            <!-- 测量结果显示 -->
            <div class="measurement-display" v-if="showMeasurements">
              <div class="measurement-grid">
                <div 
                  v-for="(measurement, index) in activeMeasurements" 
                  :key="index"
                  class="measurement-item"
                  :style="{ borderColor: getChannelColor(measurement.channel) }"
                >
                  <div class="measurement-label">{{ measurement.name }}</div>
                  <div class="measurement-value">{{ formatMeasurementValue(measurement) }}</div>
                  <div class="measurement-unit">{{ measurement.unit }}</div>
                </div>
              </div>
            </div>
          </div>
        </el-col>

        <!-- 右侧：控制面板 -->
        <el-col :span="6">
          <div class="control-panel">
            <!-- 时基控制 -->
            <div class="control-section">
              <h4 class="section-title">时基控制</h4>
              <div class="control-grid">
                <div class="control-item">
                  <label>时间/格</label>
                  <el-select v-model="timebase.scale" size="small" @change="updateTimebase">
                    <el-option 
                      v-for="scale in timebaseScales" 
                      :key="scale.value"
                      :label="scale.label" 
                      :value="scale.value" 
                    />
                  </el-select>
                </div>
                <div class="control-item">
                  <label>水平位置</label>
                  <el-slider
                    v-model="timebase.position"
                    :min="-50"
                    :max="50"
                    :step="1"
                    size="small"
                    @change="updateTimebase"
                  />
                </div>
                <div class="control-item">
                  <label>显示模式</label>
                  <el-select v-model="timebase.mode" size="small" @change="updateTimebase">
                    <el-option label="正常" value="normal" />
                    <el-option label="滚动" value="roll" />
                    <el-option label="XY" value="xy" />
                  </el-select>
                </div>
              </div>
            </div>

            <!-- 通道控制 -->
            <div class="control-section">
              <h4 class="section-title">通道控制</h4>
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
                      <label>电压/格</label>
                      <el-select v-model="channel.scale" size="small" @change="updateChannel(index)">
                        <el-option 
                          v-for="scale in voltageScales" 
                          :key="scale.value"
                          :label="scale.label" 
                          :value="scale.value" 
                        />
                      </el-select>
                    </div>
                    <div class="setting-item">
                      <label>垂直位置</label>
                      <el-slider
                        v-model="channel.position"
                        :min="-4"
                        :max="4"
                        :step="0.1"
                        size="small"
                        @change="updateChannel(index)"
                      />
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
                      <label>带宽限制</label>
                      <el-select v-model="channel.bandwidth" size="small" @change="updateChannel(index)">
                        <el-option label="全带宽" :value="0" />
                        <el-option label="20MHz" :value="20" />
                        <el-option label="200MHz" :value="200" />
                      </el-select>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- 触发控制 -->
            <div class="control-section">
              <h4 class="section-title">触发控制</h4>
              <div class="control-grid">
                <div class="control-item">
                  <label>触发源</label>
                  <el-select v-model="trigger.source" size="small" @change="updateTrigger">
                    <el-option label="CH1" :value="0" />
                    <el-option label="CH2" :value="1" />
                    <el-option label="CH3" :value="2" />
                    <el-option label="CH4" :value="3" />
                    <el-option label="外部" value="external" />
                  </el-select>
                </div>
                <div class="control-item">
                  <label>触发类型</label>
                  <el-select v-model="trigger.type" size="small" @change="updateTrigger">
                    <el-option label="边沿" value="edge" />
                    <el-option label="脉宽" value="pulse" />
                    <el-option label="视频" value="video" />
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
                  <label>触发模式</label>
                  <el-select v-model="trigger.mode" size="small" @change="updateTrigger">
                    <el-option label="自动" value="auto" />
                    <el-option label="正常" value="normal" />
                    <el-option label="单次" value="single" />
                  </el-select>
                </div>
              </div>
            </div>

            <!-- 测量控制 -->
            <div class="control-section">
              <h4 class="section-title">
                自动测量
                <el-switch 
                  v-model="showMeasurements" 
                  size="small"
                  @change="updateMeasurements"
                />
              </h4>
              <div v-if="showMeasurements" class="measurement-controls">
                <div class="measurement-config">
                  <el-button 
                    size="small" 
                    @click="addMeasurement"
                    :disabled="activeMeasurements.length >= 8"
                  >
                    添加测量
                  </el-button>
                  <el-button size="small" @click="clearMeasurements">
                    清除全部
                  </el-button>
                </div>
              </div>
            </div>

            <!-- 采集控制 -->
            <div class="control-section">
              <h4 class="section-title">采集控制</h4>
              <div class="control-grid">
                <div class="control-item">
                  <label>采集模式</label>
                  <el-select v-model="acquisition.mode" size="small" @change="updateAcquisition">
                    <el-option label="采样" value="sample" />
                    <el-option label="平均" value="average" />
                    <el-option label="包络" value="envelope" />
                  </el-select>
                </div>
                <div class="control-item">
                  <label>采集深度</label>
                  <el-select v-model="acquisition.depth" size="small" @change="updateAcquisition">
                    <el-option label="1K" :value="1000" />
                    <el-option label="10K" :value="10000" />
                    <el-option label="100K" :value="100000" />
                    <el-option label="1M" :value="1000000" />
                  </el-select>
                </div>
                <div class="control-item">
                  <label>采样率</label>
                  <el-select v-model="acquisition.sampleRate" size="small" @change="updateAcquisition">
                    <el-option label="1MS/s" :value="1000000" />
                    <el-option label="10MS/s" :value="10000000" />
                    <el-option label="100MS/s" :value="100000000" />
                    <el-option label="1GS/s" :value="1000000000" />
                  </el-select>
                </div>
              </div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 状态栏 -->
    <div class="oscilloscope-status-bar">
      <div class="status-left">
        <span>采样率: {{ formatSampleRate(acquisition.sampleRate) }}</span>
        <span>时基: {{ getCurrentTimebaseLabel() }}</span>
        <span>触发: {{ trigger.mode.toUpperCase() }}</span>
      </div>
      <div class="status-right">
        <span>内存深度: {{ formatMemoryDepth(acquisition.depth) }}</span>
        <span>波形更新率: {{ waveformUpdateRate }} wfm/s</span>
      </div>
    </div>

    <!-- 测量配置对话框 -->
    <el-dialog v-model="showMeasurementDialog" title="添加测量" width="400px">
      <el-form :model="newMeasurement" label-width="80px">
        <el-form-item label="测量类型">
          <el-select v-model="newMeasurement.type" placeholder="选择测量类型">
            <el-option label="频率" value="frequency" />
            <el-option label="周期" value="period" />
            <el-option label="幅度" value="amplitude" />
            <el-option label="峰峰值" value="peak-peak" />
            <el-option label="RMS" value="rms" />
            <el-option label="平均值" value="mean" />
            <el-option label="最大值" value="max" />
            <el-option label="最小值" value="min" />
            <el-option label="上升时间" value="rise-time" />
            <el-option label="下降时间" value="fall-time" />
            <el-option label="占空比" value="duty-cycle" />
          </el-select>
        </el-form-item>
        <el-form-item label="通道">
          <el-select v-model="newMeasurement.channel" placeholder="选择通道">
            <el-option 
              v-for="(channel, index) in channels" 
              :key="index"
              v-show="channel.enabled"
              :label="`CH${index + 1}`" 
              :value="index" 
            />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showMeasurementDialog = false">取消</el-button>
        <el-button type="primary" @click="confirmAddMeasurement">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch, computed, nextTick } from 'vue'
import { TrendCharts, VideoPlay, VideoPause, Camera } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import StripChart from '@/components/charts/StripChart.vue'
import type { ChartData, SeriesConfig, ChartOptions } from '@/types/chart'

// 时基配置
interface TimebaseConfig {
  scale: number // 时间/格 (秒)
  position: number // 水平位置 (%)
  mode: 'normal' | 'roll' | 'xy'
}

// 通道配置
interface ChannelConfig {
  enabled: boolean
  scale: number // 电压/格 (V)
  position: number // 垂直位置 (格)
  coupling: 'DC' | 'AC' | 'GND'
  bandwidth: number // 带宽限制 (MHz, 0表示全带宽)
  probe: number // 探头倍数
  invert: boolean // 反相
  color: string
}

// 触发配置
interface TriggerConfig {
  source: number | 'external'
  type: 'edge' | 'pulse' | 'video'
  slope: 'rising' | 'falling' | 'both'
  level: number // 触发电平 (V)
  mode: 'auto' | 'normal' | 'single'
  holdoff: number // 触发释抑 (s)
}

// 采集配置
interface AcquisitionConfig {
  mode: 'sample' | 'average' | 'envelope'
  depth: number // 采集深度
  sampleRate: number // 采样率 (S/s)
  averages?: number // 平均次数
}

// 测量配置
interface MeasurementConfig {
  id: string
  name: string
  type: string
  channel: number
  value: number
  unit: string
  enabled: boolean
}

// Props
interface Props {
  width?: number
  height?: number
  channels?: number
}

const props = withDefaults(defineProps<Props>(), {
  width: 1200,
  height: 600,
  channels: 4
})

// Emits
const emit = defineEmits<{
  runningChange: [running: boolean]
  triggerEvent: [triggerInfo: any]
  measurementUpdate: [measurements: MeasurementConfig[]]
}>()

// 响应式数据
const running = ref(false)
const triggerStatus = ref<'waiting' | 'triggered' | 'stopped'>('stopped')

// 时基配置
const timebase = ref<TimebaseConfig>({
  scale: 0.001, // 1ms/div
  position: 0,
  mode: 'normal'
})

// 通道配置
const channels = ref<ChannelConfig[]>([
  {
    enabled: true,
    scale: 1, // 1V/div
    position: 0,
    coupling: 'DC',
    bandwidth: 0,
    probe: 1,
    invert: false,
    color: '#FFD700'
  },
  {
    enabled: true,
    scale: 1,
    position: 0,
    coupling: 'DC',
    bandwidth: 0,
    probe: 1,
    invert: false,
    color: '#00CED1'
  },
  {
    enabled: false,
    scale: 1,
    position: 0,
    coupling: 'DC',
    bandwidth: 0,
    probe: 1,
    invert: false,
    color: '#FF69B4'
  },
  {
    enabled: false,
    scale: 1,
    position: 0,
    coupling: 'DC',
    bandwidth: 0,
    probe: 1,
    invert: false,
    color: '#32CD32'
  }
])

// 触发配置
const trigger = ref<TriggerConfig>({
  source: 0,
  type: 'edge',
  slope: 'rising',
  level: 0,
  mode: 'auto',
  holdoff: 0
})

// 采集配置
const acquisition = ref<AcquisitionConfig>({
  mode: 'sample',
  depth: 10000,
  sampleRate: 100000000 // 100MS/s
})

// 测量相关
const showMeasurements = ref(false)
const activeMeasurements = ref<MeasurementConfig[]>([])
const showMeasurementDialog = ref(false)
const newMeasurement = ref({
  type: '',
  channel: 0
})

// 波形数据
const waveformData = ref<ChartData>({
  series: [],
  xStart: 0,
  xInterval: 0.00001, // 10μs
  sampleRate: 100000
})

const waveformUpdateRate = ref(30)

// 图表引用
const stripChartRef = ref()

// 时基刻度选项
const timebaseScales = [
  { label: '1ns/div', value: 0.000000001 },
  { label: '2ns/div', value: 0.000000002 },
  { label: '5ns/div', value: 0.000000005 },
  { label: '10ns/div', value: 0.00000001 },
  { label: '20ns/div', value: 0.00000002 },
  { label: '50ns/div', value: 0.00000005 },
  { label: '100ns/div', value: 0.0000001 },
  { label: '200ns/div', value: 0.0000002 },
  { label: '500ns/div', value: 0.0000005 },
  { label: '1μs/div', value: 0.000001 },
  { label: '2μs/div', value: 0.000002 },
  { label: '5μs/div', value: 0.000005 },
  { label: '10μs/div', value: 0.00001 },
  { label: '20μs/div', value: 0.00002 },
  { label: '50μs/div', value: 0.00005 },
  { label: '100μs/div', value: 0.0001 },
  { label: '200μs/div', value: 0.0002 },
  { label: '500μs/div', value: 0.0005 },
  { label: '1ms/div', value: 0.001 },
  { label: '2ms/div', value: 0.002 },
  { label: '5ms/div', value: 0.005 },
  { label: '10ms/div', value: 0.01 },
  { label: '20ms/div', value: 0.02 },
  { label: '50ms/div', value: 0.05 },
  { label: '100ms/div', value: 0.1 },
  { label: '200ms/div', value: 0.2 },
  { label: '500ms/div', value: 0.5 },
  { label: '1s/div', value: 1 },
  { label: '2s/div', value: 2 },
  { label: '5s/div', value: 5 },
  { label: '10s/div', value: 10 }
]

// 电压刻度选项
const voltageScales = [
  { label: '1mV/div', value: 0.001 },
  { label: '2mV/div', value: 0.002 },
  { label: '5mV/div', value: 0.005 },
  { label: '10mV/div', value: 0.01 },
  { label: '20mV/div', value: 0.02 },
  { label: '50mV/div', value: 0.05 },
  { label: '100mV/div', value: 0.1 },
  { label: '200mV/div', value: 0.2 },
  { label: '500mV/div', value: 0.5 },
  { label: '1V/div', value: 1 },
  { label: '2V/div', value: 2 },
  { label: '5V/div', value: 5 },
  { label: '10V/div', value: 10 }
]

// 计算属性
const enabledChannels = computed((): ChannelConfig[] => {
  return channels.value.filter((ch: ChannelConfig) => ch.enabled)
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
  autoScale: false,
  logarithmic: false,
  splitView: false,
  legendVisible: true,
  cursorMode: 'cursor',
  gridEnabled: true,
  minorGridEnabled: true,
  theme: 'dark' as const
}))

// 定时器
let dataGenerationTimer: number | null = null
let measurementTimer: number | null = null

// 方法
const toggleRunning = () => {
  running.value = !running.value
  emit('runningChange', running.value)
  
  if (running.value) {
    startDataGeneration()
    triggerStatus.value = 'waiting'
    ElMessage.success('示波器开始运行')
  } else {
    stopDataGeneration()
    triggerStatus.value = 'stopped'
    ElMessage.info('示波器已停止')
  }
}

const singleCapture = () => {
  if (!running.value) {
    generateSingleWaveform()
    triggerStatus.value = 'triggered'
    ElMessage.success('单次采集完成')
  }
}

const startDataGeneration = () => {
  if (dataGenerationTimer) {
    clearInterval(dataGenerationTimer)
  }
  
  dataGenerationTimer = setInterval(() => {
    generateWaveformData()
    updateMeasurements()
  }, 1000 / 30) // 30fps更新率
}

const stopDataGeneration = () => {
  if (dataGenerationTimer) {
    clearInterval(dataGenerationTimer)
    dataGenerationTimer = null
  }
}

const generateWaveformData = () => {
  const pointsPerDiv = 100
  const totalPoints = pointsPerDiv * 10 // 10个时间格
  const dt = timebase.value.scale / pointsPerDiv
  
  const series: number[][] = []
  
  enabledChannels.value.forEach((channel, channelIndex) => {
    const actualChannelIndex = channels.value.indexOf(channel)
    const data: number[] = []
    
    for (let i = 0; i < totalPoints; i++) {
      const t = i * dt
      let value = 0
      
      // 生成不同类型的测试信号
      switch (actualChannelIndex) {
        case 0: // CH1: 正弦波
          value = Math.sin(2 * Math.PI * 1000 * t) * channel.scale * 2
          break
        case 1: // CH2: 方波
          value = Math.sign(Math.sin(2 * Math.PI * 500 * t)) * channel.scale * 1.5
          break
        case 2: // CH3: 三角波
          const period = 1 / 200
          const phase = (t % period) / period
          value = (phase < 0.5 ? 4 * phase - 1 : 3 - 4 * phase) * channel.scale
          break
        case 3: // CH4: 锯齿波
          const sawPeriod = 1 / 100
          const sawPhase = (t % sawPeriod) / sawPeriod
          value = (2 * sawPhase - 1) * channel.scale * 0.8
          break
      }
      
      // 添加噪声
      value += (Math.random() - 0.5) * channel.scale * 0.05
      
      // 应用垂直位置偏移
      value += channel.position * channel.scale
      
      data.push(value)
    }
    
    series.push(data)
  })
  
  waveformData.value = {
    series,
    xStart: 0,
    xInterval: dt,
    sampleRate: 1 / dt
  }
}

const generateSingleWaveform = () => {
  generateWaveformData()
  updateMeasurements()
}

const updateTimebase = () => {
  if (running.value) {
    generateWaveformData()
  }
}

const updateChannel = (index: number) => {
  if (running.value) {
    generateWaveformData()
  }
}

const updateTrigger = () => {
  // 触发配置更新逻辑
  console.log('触发配置更新:', trigger.value)
}

const updateAcquisition = () => {
  // 采集配置更新逻辑
  console.log('采集配置更新:', acquisition.value)
}

const updateMeasurements = () => {
  if (!showMeasurements.value || activeMeasurements.value.length === 0) return
  
  activeMeasurements.value.forEach(measurement => {
    const channelData = waveformData.value.series[measurement.channel] as number[]
    if (!channelData || channelData.length === 0) return
    
    switch (measurement.type) {
      case 'frequency':
        measurement.value = calculateFrequency(channelData)
        measurement.unit = 'Hz'
        break
      case 'period':
        measurement.value = 1 / calculateFrequency(channelData)
        measurement.unit = 's'
        break
      case 'amplitude':
        measurement.value = calculateAmplitude(channelData)
        measurement.unit = 'V'
        break
      case 'peak-peak':
        measurement.value = Math.max(...channelData) - Math.min(...channelData)
        measurement.unit = 'V'
        break
      case 'rms':
        measurement.value = calculateRMS(channelData)
        measurement.unit = 'V'
        break
      case 'mean':
        measurement.value = channelData.reduce((sum, val) => sum + val, 0) / channelData.length
        measurement.unit = 'V'
        break
      case 'max':
        measurement.value = Math.max(...channelData)
        measurement.unit = 'V'
        break
      case 'min':
        measurement.value = Math.min(...channelData)
        measurement.unit = 'V'
        break
      case 'rise-time':
        measurement.value = calculateRiseTime(channelData)
        measurement.unit = 's'
        break
      case 'fall-time':
        measurement.value = calculateFallTime(channelData)
        measurement.unit = 's'
        break
      case 'duty-cycle':
        measurement.value = calculateDutyCycle(channelData)
        measurement.unit = '%'
        break
    }
  })
  
  emit('measurementUpdate', activeMeasurements.value)
}

// 测量计算函数
const calculateFrequency = (data: number[]): number => {
  // 简化的频率计算：通过零点交叉检测
  let crossings = 0
  for (let i = 1; i < data.length; i++) {
    if ((data[i] >= 0 && data[i - 1] < 0) || (data[i] < 0 && data[i - 1] >= 0)) {
      crossings++
    }
  }
  const dt = waveformData.value.xInterval || 0.00001
  const totalTime = data.length * dt
  return crossings / (2 * totalTime) // 每个周期有2个零点交叉
}

const calculateAmplitude = (data: number[]): number => {
  const max = Math.max(...data)
  const min = Math.min(...data)
  return (max - min) / 2
}

const calculateRMS = (data: number[]): number => {
  const sumSquares = data.reduce((sum, val) => sum + val * val, 0)
  return Math.sqrt(sumSquares / data.length)
}

const calculateRiseTime = (data: number[]): number => {
  // 简化的上升时间计算（10%-90%）
  const max = Math.max(...data)
  const min = Math.min(...data)
  const level10 = min + (max - min) * 0.1
  const level90 = min + (max - min) * 0.9
  
  let start = -1, end = -1
  for (let i = 0; i < data.length; i++) {
    if (start === -1 && data[i] >= level10) start = i
    if (start !== -1 && data[i] >= level90) {
      end = i
      break
    }
  }
  
  if (start === -1 || end === -1) return 0
  const dt = waveformData.value.xInterval || 0.00001
  return (end - start) * dt
}

const calculateFallTime = (data: number[]): number => {
  // 简化的下降时间计算（90%-10%）
  const max = Math.max(...data)
  const min = Math.min(...data)
  const level90 = min + (max - min) * 0.9
  const level10 = min + (max - min) * 0.1
  
  let start = -1, end = -1
  for (let i = 0; i < data.length; i++) {
    if (start === -1 && data[i] <= level90) start = i
    if (start !== -1 && data[i] <= level10) {
      end = i
      break
    }
  }
  
  if (start === -1 || end === -1) return 0
  const dt = waveformData.value.xInterval || 0.00001
  return (end - start) * dt
}

const calculateDutyCycle = (data: number[]): number => {
  // 简化的占空比计算
  const threshold = (Math.max(...data) + Math.min(...data)) / 2
  let highCount = 0
  for (const value of data) {
    if (value > threshold) highCount++
  }
  return (highCount / data.length) * 100
}

// 事件处理函数
const handleCursorMove = (position: { x: number; y: number }) => {
  // 游标移动处理
  console.log('游标位置:', position)
}

const handleZoom = (range: { xMin: number; xMax: number; yMin: number; yMax: number }) => {
  // 缩放处理
  console.log('缩放范围:', range)
}

// 工具函数
const getTriggerStatusText = (): string => {
  switch (triggerStatus.value) {
    case 'waiting': return '等待触发'
    case 'triggered': return '已触发'
    case 'stopped': return '已停止'
    default: return '未知'
  }
}

const getChannelColor = (channelIndex: number): string => {
  return channels.value[channelIndex]?.color || '#FFD700'
}

const formatMeasurementValue = (measurement: MeasurementConfig): string => {
  if (measurement.unit === 'Hz' && measurement.value >= 1000) {
    return `${(measurement.value / 1000).toFixed(2)}k`
  } else if (measurement.unit === 's' && measurement.value < 0.001) {
    return `${(measurement.value * 1000000).toFixed(1)}μ`
  } else if (measurement.unit === 's' && measurement.value < 1) {
    return `${(measurement.value * 1000).toFixed(1)}m`
  }
  return measurement.value.toFixed(3)
}

const formatSampleRate = (rate: number): string => {
  if (rate >= 1000000000) {
    return `${(rate / 1000000000).toFixed(1)}GS/s`
  } else if (rate >= 1000000) {
    return `${(rate / 1000000).toFixed(1)}MS/s`
  } else if (rate >= 1000) {
    return `${(rate / 1000).toFixed(1)}kS/s`
  }
  return `${rate}S/s`
}

const getCurrentTimebaseLabel = (): string => {
  const scale = timebaseScales.find(s => s.value === timebase.value.scale)
  return scale ? scale.label : `${timebase.value.scale}s/div`
}

const formatMemoryDepth = (depth: number): string => {
  if (depth >= 1000000) {
    return `${(depth / 1000000).toFixed(1)}M`
  } else if (depth >= 1000) {
    return `${(depth / 1000).toFixed(1)}K`
  }
  return depth.toString()
}

// 测量管理函数
const addMeasurement = () => {
  showMeasurementDialog.value = true
}

const confirmAddMeasurement = () => {
  if (!newMeasurement.value.type || newMeasurement.value.channel === undefined) {
    ElMessage.warning('请选择测量类型和通道')
    return
  }
  
  const measurement: MeasurementConfig = {
    id: Date.now().toString(),
    name: getMeasurementName(newMeasurement.value.type),
    type: newMeasurement.value.type,
    channel: newMeasurement.value.channel,
    value: 0,
    unit: '',
    enabled: true
  }
  
  activeMeasurements.value.push(measurement)
  showMeasurementDialog.value = false
  newMeasurement.value = { type: '', channel: 0 }
  
  ElMessage.success('测量添加成功')
}

const clearMeasurements = () => {
  activeMeasurements.value = []
  ElMessage.success('已清除所有测量')
}

const getMeasurementName = (type: string): string => {
  const nameMap: Record<string, string> = {
    'frequency': '频率',
    'period': '周期',
    'amplitude': '幅度',
    'peak-peak': '峰峰值',
    'rms': 'RMS',
    'mean': '平均值',
    'max': '最大值',
    'min': '最小值',
    'rise-time': '上升时间',
    'fall-time': '下降时间',
    'duty-cycle': '占空比'
  }
  return nameMap[type] || type
}

// 生命周期
onMounted(() => {
  generateWaveformData()
})

onUnmounted(() => {
  stopDataGeneration()
  if (measurementTimer) {
    clearInterval(measurementTimer)
  }
})

// 暴露方法
defineExpose({
  toggleRunning,
  singleCapture,
  updateTimebase,
  updateChannel,
  updateTrigger,
  addMeasurement,
  clearMeasurements
})
</script>

<style lang="scss" scoped>
.oscilloscope {
  background: var(--surface-color);
  border: 1px solid var(--border-color);
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  
  .oscilloscope-header {
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
      
      .oscilloscope-title {
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
      
      .oscilloscope-status {
        display: flex;
        gap: 16px;
        
        .status-indicator {
          padding: 2px 8px;
          border-radius: 8px;
          font-size: 12px;
          font-weight: 500;
          
          &.running {
            background: rgba(103, 194, 58, 0.1);
            color: #67c23a;
          }
          
          &.stopped {
            background: rgba(144, 147, 153, 0.1);
            color: #909399;
          }
        }
        
        .trigger-status {
          padding: 2px 8px;
          border-radius: 8px;
          font-size: 12px;
          font-weight: 500;
          
          &.waiting {
            background: rgba(230, 162, 60, 0.1);
            color: #e6a23c;
          }
          
          &.triggered {
            background: rgba(103, 194, 58, 0.1);
            color: #67c23a;
          }
          
          &.stopped {
            background: rgba(144, 147, 153, 0.1);
            color: #909399;
          }
        }
      }
    }
  }
  
  .oscilloscope-body {
    margin-bottom: 16px;
    
    .waveform-display {
      background: #1a1a1a;
      border: 1px solid #333;
      border-radius: 8px;
      padding: 8px;
      
      .waveform-chart {
        height: 400px;
      }
      
      .measurement-display {
        margin-top: 8px;
        padding: 8px;
        background: rgba(255, 255, 255, 0.05);
        border-radius: 4px;
        
        .measurement-grid {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
          gap: 8px;
          
          .measurement-item {
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 8px;
            background: rgba(0, 0, 0, 0.3);
            border-radius: 4px;
            border-left: 3px solid;
            
            .measurement-label {
              font-size: 10px;
              color: #ccc;
              margin-bottom: 2px;
            }
            
            .measurement-value {
              font-size: 14px;
              font-weight: 600;
              color: #fff;
              font-family: 'Consolas', 'Monaco', monospace;
            }
            
            .measurement-unit {
              font-size: 10px;
              color: #999;
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
        
        .measurement-controls {
          .measurement-config {
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
  
  .oscilloscope-status-bar {
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

@media (max-width: 768px) {
  .oscilloscope {
    padding: 12px;
    
    .oscilloscope-header {
      flex-direction: column;
      gap: 12px;
      align-items: flex-start;
    }
    
    .oscilloscope-body {
      .waveform-display {
        .measurement-display {
          .measurement-grid {
            grid-template-columns: repeat(2, 1fr);
          }
        }
      }
    }
    
    .oscilloscope-status-bar {
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
