<template>
  <div class="oscilloscope">
    <!-- 控件标题栏 -->
    <div class="oscilloscope-header">
      <div class="header-left">
        <h3 class="oscilloscope-title">
          <el-icon><TrendCharts /></el-icon>
          数字示波器
        </h3>
        <div class="oscilloscope-model">DSO-4000 Series</div>
      </div>
      <div class="header-right">
        <el-button 
          :type="status.running ? 'danger' : 'primary'"
          @click="toggleRunning"
          :loading="runningChanging"
        >
          <el-icon><VideoPlay v-if="!status.running" /><VideoPause v-else /></el-icon>
          {{ status.running ? '停止' : '运行' }}
        </el-button>
        <el-button @click="singleTrigger" :disabled="!status.running">
          <el-icon><Aim /></el-icon>
          单次触发
        </el-button>
        <el-dropdown @command="handleMenuCommand">
          <el-button circle>
            <el-icon><More /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="autoset">自动设置</el-dropdown-item>
              <el-dropdown-item command="default">默认设置</el-dropdown-item>
              <el-dropdown-item command="save">保存波形</el-dropdown-item>
              <el-dropdown-item command="recall">调用波形</el-dropdown-item>
              <el-dropdown-item command="calibrate">自校准</el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>

    <!-- 主要内容区域 -->
    <div class="oscilloscope-content">
      <!-- 左侧控制面板 -->
      <div class="control-panel">
        <!-- 通道控制 -->
        <el-card class="control-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><DataLine /></el-icon>
              <span>通道控制</span>
            </div>
          </template>
          
          <div class="channels-control">
            <div 
              v-for="channel in config.channels" 
              :key="channel.id"
              class="channel-item"
              :class="{ disabled: !channel.enabled }"
            >
              <div class="channel-header">
                <el-checkbox 
                  v-model="channel.enabled"
                  @change="onChannelEnabledChange(channel.id)"
                >
                  <span class="channel-label" :style="{ color: channel.color }">
                    {{ channel.label }}
                  </span>
                </el-checkbox>
                <el-button 
                  size="small" 
                  text 
                  @click="toggleChannelVisible(channel.id)"
                  :class="{ active: channel.visible }"
                >
                  <el-icon><View v-if="channel.visible" /><Hide v-else /></el-icon>
                </el-button>
              </div>
              
              <div class="channel-controls" v-if="channel.enabled">
                <div class="control-row">
                  <label>垂直刻度</label>
                  <el-select 
                    v-model="channel.verticalScale" 
                    @change="onChannelConfigChange(channel.id)"
                    size="small"
                  >
                    <el-option
                      v-for="option in VERTICAL_SCALE_OPTIONS"
                      :key="option.value"
                      :label="option.label"
                      :value="option.value"
                    />
                  </el-select>
                </div>
                
                <div class="control-row">
                  <label>垂直偏移</label>
                  <el-input-number
                    v-model="channel.verticalOffset"
                    :min="-10"
                    :max="10"
                    :precision="3"
                    :step="0.1"
                    @change="onChannelConfigChange(channel.id)"
                    size="small"
                  />
                </div>
                
                <div class="control-row">
                  <label>耦合方式</label>
                  <el-select 
                    v-model="channel.coupling" 
                    @change="onChannelConfigChange(channel.id)"
                    size="small"
                  >
                    <el-option label="DC" value="dc" />
                    <el-option label="AC" value="ac" />
                    <el-option label="GND" value="gnd" />
                  </el-select>
                </div>
                
                <div class="control-row">
                  <label>探头倍数</label>
                  <el-select 
                    v-model="channel.probe" 
                    @change="onChannelConfigChange(channel.id)"
                    size="small"
                  >
                    <el-option
                      v-for="option in PROBE_OPTIONS"
                      :key="option.value"
                      :label="option.label"
                      :value="option.value"
                    />
                  </el-select>
                </div>
              </div>
            </div>
          </div>
        </el-card>

        <!-- 时基控制 -->
        <el-card class="control-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Timer /></el-icon>
              <span>时基控制</span>
            </div>
          </template>
          
          <div class="timebase-controls">
            <div class="control-row">
              <label>时间刻度</label>
              <el-select 
                v-model="config.timebase.scale" 
                @change="onTimebaseChange"
                size="small"
              >
                <el-option
                  v-for="option in TIMEBASE_OPTIONS"
                  :key="option.value"
                  :label="option.label"
                  :value="option.value"
                />
              </el-select>
            </div>
            
            <div class="control-row">
              <label>水平位置</label>
              <el-slider
                v-model="config.timebase.position"
                :min="0"
                :max="100"
                @change="onTimebaseChange"
                :show-tooltip="true"
                :format-tooltip="(val: number) => `${val}%`"
              />
            </div>
            
            <div class="control-row">
              <label>显示模式</label>
              <el-radio-group 
                v-model="config.timebase.mode" 
                @change="onTimebaseChange"
                size="small"
              >
                <el-radio-button label="yt">Y-T</el-radio-button>
                <el-radio-button label="xy">X-Y</el-radio-button>
                <el-radio-button label="roll">滚动</el-radio-button>
              </el-radio-group>
            </div>
          </div>
        </el-card>

        <!-- 触发控制 -->
        <el-card class="control-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Aim /></el-icon>
              <span>触发控制</span>
            </div>
          </template>
          
          <div class="trigger-controls">
            <div class="control-row">
              <label>触发类型</label>
              <el-select 
                v-model="config.trigger.type" 
                @change="onTriggerChange"
                size="small"
              >
                <el-option label="边沿触发" value="edge" />
                <el-option label="脉宽触发" value="pulse" />
                <el-option label="视频触发" value="video" />
                <el-option label="外部触发" value="external" />
              </el-select>
            </div>
            
            <div class="control-row">
              <label>触发模式</label>
              <el-radio-group 
                v-model="config.trigger.mode" 
                @change="onTriggerChange"
                size="small"
              >
                <el-radio-button label="auto">自动</el-radio-button>
                <el-radio-button label="normal">常规</el-radio-button>
                <el-radio-button label="single">单次</el-radio-button>
              </el-radio-group>
            </div>
            
            <div class="control-row">
              <label>触发源</label>
              <el-select 
                v-model="config.trigger.source" 
                @change="onTriggerChange"
                size="small"
              >
                <el-option
                  v-for="channel in config.channels.filter(ch => ch.enabled)"
                  :key="channel.id"
                  :label="channel.label"
                  :value="channel.id"
                />
                <el-option label="外部" value="ext" />
                <el-option label="电源线" value="line" />
              </el-select>
            </div>
            
            <div class="control-row">
              <label>触发电平</label>
              <el-input-number
                v-model="config.trigger.level"
                :min="-10"
                :max="10"
                :precision="3"
                :step="0.1"
                @change="onTriggerChange"
                size="small"
              />
            </div>
            
            <div v-if="config.trigger.type === 'edge'" class="control-row">
              <label>触发边沿</label>
              <el-radio-group 
                v-model="config.trigger.edge" 
                @change="onTriggerChange"
                size="small"
              >
                <el-radio-button label="rising">上升</el-radio-button>
                <el-radio-button label="falling">下降</el-radio-button>
                <el-radio-button label="both">双边</el-radio-button>
              </el-radio-group>
            </div>
          </div>
        </el-card>

        <!-- 采集控制 -->
        <el-card class="control-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><DataAnalysis /></el-icon>
              <span>采集控制</span>
            </div>
          </template>
          
          <div class="acquisition-controls">
            <div class="control-row">
              <label>采集模式</label>
              <el-select 
                v-model="config.acquisition.mode" 
                @change="onAcquisitionChange"
                size="small"
              >
                <el-option label="正常" value="normal" />
                <el-option label="平均" value="average" />
                <el-option label="峰值检测" value="peak" />
                <el-option label="包络" value="envelope" />
              </el-select>
            </div>
            
            <div class="control-row">
              <label>采样率</label>
              <el-select 
                v-model="config.acquisition.sampleRate" 
                @change="onAcquisitionChange"
                size="small"
              >
                <el-option
                  v-for="option in SAMPLE_RATE_OPTIONS"
                  :key="option.value"
                  :label="option.label"
                  :value="option.value"
                />
              </el-select>
            </div>
            
            <div class="control-row">
              <label>内存深度</label>
              <el-select 
                v-model="config.acquisition.memoryDepth" 
                @change="onAcquisitionChange"
                size="small"
              >
                <el-option
                  v-for="option in MEMORY_DEPTH_OPTIONS"
                  :key="option.value"
                  :label="option.label"
                  :value="option.value"
                />
              </el-select>
            </div>
            
            <div v-if="config.acquisition.mode === 'average'" class="control-row">
              <label>平均次数</label>
              <el-input-number
                v-model="config.acquisition.averageCount"
                :min="2"
                :max="1024"
                :step="1"
                @change="onAcquisitionChange"
                size="small"
              />
            </div>
          </div>
        </el-card>
      </div>

      <!-- 右侧显示区域 -->
      <div class="display-panel">
        <!-- 波形显示 -->
        <el-card class="display-card waveform-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><TrendCharts /></el-icon>
              <span>波形显示</span>
              <div class="header-actions">
                <el-button-group size="small">
                  <el-button @click="zoomIn">
                    <el-icon><ZoomIn /></el-icon>
                  </el-button>
                  <el-button @click="zoomOut">
                    <el-icon><ZoomOut /></el-icon>
                  </el-button>
                  <el-button @click="autoScale">
                    <el-icon><FullScreen /></el-icon>
                    自动缩放
                  </el-button>
                </el-button-group>
                <el-button size="small" @click="toggleCursors">
                  <el-icon><Aim /></el-icon>
                  游标
                </el-button>
              </div>
            </div>
          </template>
          
          <div class="waveform-display">
            <!-- 这里集成现有的图表组件 -->
            <ProfessionalEasyChart
              ref="waveformChart"
              :data="waveformData"
              :config="chartConfig"
              :width="chartWidth"
              :height="chartHeight"
              @cursor-change="onCursorChange"
              @measurement-change="onMeasurementChange"
            />
            
            <!-- 触发指示器 -->
            <div class="trigger-indicator" v-if="status.triggered">
              <el-icon class="trigger-icon"><Aim /></el-icon>
              <span>已触发</span>
            </div>
          </div>
        </el-card>

        <!-- 测量结果 -->
        <el-card class="display-card measurements-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><DataAnalysis /></el-icon>
              <span>测量结果</span>
              <div class="header-actions">
                <el-button size="small" @click="addMeasurement">
                  <el-icon><Plus /></el-icon>
                  添加测量
                </el-button>
                <el-button size="small" @click="clearMeasurements">
                  <el-icon><Delete /></el-icon>
                  清除
                </el-button>
              </div>
            </div>
          </template>
          
          <div class="measurements-content">
            <!-- 游标测量 -->
            <div v-if="config.cursors.enabled" class="cursor-measurements">
              <h4>游标测量</h4>
              <div class="cursor-results">
                <div class="cursor-item">
                  <span class="label">ΔX:</span>
                  <span class="value">{{ formatTimeValue(config.cursors.delta.x) }}</span>
                </div>
                <div class="cursor-item">
                  <span class="label">ΔY:</span>
                  <span class="value">{{ formatVoltageValue(config.cursors.delta.y) }}</span>
                </div>
                <div class="cursor-item">
                  <span class="label">频率:</span>
                  <span class="value">{{ formatFrequencyValue(config.cursors.frequency) }}</span>
                </div>
              </div>
            </div>
            
            <!-- 自动测量 -->
            <div class="auto-measurements">
              <h4>自动测量</h4>
              <div class="measurement-list">
                <div 
                  v-for="measurement in config.measurements" 
                  :key="measurement.id"
                  class="measurement-item"
                >
                  <div class="measurement-header">
                    <span class="measurement-name">{{ getMeasurementName(measurement.type) }}</span>
                    <span class="measurement-channel">CH{{ measurement.channel }}</span>
                    <el-button 
                      size="small" 
                      text 
                      @click="removeMeasurement(measurement.id)"
                    >
                      <el-icon><Close /></el-icon>
                    </el-button>
                  </div>
                  <div class="measurement-value">
                    {{ formatMeasurementValue(measurement.value, measurement.unit) }}
                  </div>
                  <div class="measurement-stats">
                    <span>最小: {{ formatMeasurementValue(measurement.statistics.minimum, measurement.unit) }}</span>
                    <span>最大: {{ formatMeasurementValue(measurement.statistics.maximum, measurement.unit) }}</span>
                    <span>平均: {{ formatMeasurementValue(measurement.statistics.mean, measurement.unit) }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </el-card>

        <!-- 状态信息 -->
        <el-card class="display-card status-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Monitor /></el-icon>
              <span>状态信息</span>
            </div>
          </template>
          
          <div class="status-grid">
            <div class="status-item">
              <div class="status-label">运行状态</div>
              <div class="status-value" :class="{ active: status.running }">
                <el-icon><VideoPlay v-if="status.running" /><VideoPause v-else /></el-icon>
                {{ status.running ? '运行中' : '停止' }}
              </div>
            </div>

            <div class="status-item">
              <div class="status-label">触发状态</div>
              <div class="status-value" :class="{ active: status.triggered }">
                <el-icon><Aim /></el-icon>
                {{ status.triggered ? '已触发' : '等待触发' }}
              </div>
            </div>

            <div class="status-item">
              <div class="status-label">触发频率</div>
              <div class="status-value">{{ formatFrequencyValue(status.triggerRate) }}</div>
            </div>

            <div class="status-item">
              <div class="status-label">采样率</div>
              <div class="status-value">{{ formatSampleRate(status.sampleRate) }}</div>
            </div>

            <div class="status-item">
              <div class="status-label">内存使用</div>
              <div class="status-value" :class="{ warning: status.memoryUsage > 80 }">
                {{ status.memoryUsage.toFixed(1) }}%
              </div>
            </div>

            <div class="status-item">
              <div class="status-label">设备温度</div>
              <div class="status-value" :class="{ warning: status.temperature > 60 }">
                {{ status.temperature.toFixed(1) }}°C
              </div>
            </div>
          </div>
        </el-card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { 
  TrendCharts, VideoPlay, VideoPause, Aim, More, DataLine, Timer,
  DataAnalysis, View, Hide, ZoomIn, ZoomOut, FullScreen, Plus, Delete,
  Close, Monitor
} from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import ProfessionalEasyChart from '@/components/charts/ProfessionalEasyChart.vue'
import type {
  OscilloscopeConfig,
  OscilloscopeStatus,
  WaveformData,
  AutoMeasurement,
  MeasurementType
} from '@/types/oscilloscope'
import {
  DEFAULT_OSCILLOSCOPE_CONFIG,
  TIMEBASE_OPTIONS,
  VERTICAL_SCALE_OPTIONS,
  PROBE_OPTIONS,
  SAMPLE_RATE_OPTIONS,
  MEMORY_DEPTH_OPTIONS,
  MEASUREMENT_TYPES,
  formatTimeValue,
  formatVoltageValue,
  formatFrequencyValue
} from '@/types/oscilloscope'

// Props
interface Props {
  width?: number
  height?: number
  config?: Partial<OscilloscopeConfig>
}

const props = withDefaults(defineProps<Props>(), {
  width: 1200,
  height: 800,
  config: () => ({})
})

// Emits
const emit = defineEmits<{
  'config-change': [config: OscilloscopeConfig]
  'waveform-data': [data: WaveformData[]]
  'measurement-change': [measurements: AutoMeasurement[]]
  'trigger-change': [triggered: boolean]
  'error': [error: string]
}>()

// 响应式数据
const config = reactive<OscilloscopeConfig>({
  ...DEFAULT_OSCILLOSCOPE_CONFIG,
  ...props.config
})

const status = reactive<OscilloscopeStatus>({
  running: false,
  triggered: false,
  triggerRate: 0,
  sampleRate: 1000000000,
  memoryUsage: 25.6,
  temperature: 28.5,
  calibrationDate: '2024-01-15',
  errorCode: 0,
  errorMessage: ''
})

// 控制状态
const runningChanging = ref(false)
const waveformChart = ref<InstanceType<typeof ProfessionalEasyChart>>()

// 图表配置
const chartWidth = computed(() => props.width - 400)
const chartHeight = computed(() => 400)

const chartConfig = computed(() => ({
  title: '示波器波形显示',
  xAxis: {
    name: '时间',
    unit: 's',
    min: -config.timebase.scale * 5,
    max: config.timebase.scale * 5
  },
  yAxis: {
    name: '电压',
    unit: 'V'
  },
  grid: true,
  legend: true,
  toolbar: true,
  cursors: config.cursors.enabled,
  measurements: true
}))

// 波形数据
const waveformData = ref<{ series: any[] }>({ series: [] })

// 定时器
let statusTimer: number | null = null
let dataTimer: number | null = null

// 方法
const toggleRunning = async () => {
  runningChanging.value = true
  try {
    await new Promise(resolve => setTimeout(resolve, 500))
    status.running = !status.running
    
    if (status.running) {
      startDataGeneration()
    } else {
      stopDataGeneration()
    }
    
    emit('config-change', config)
    ElMessage.success(status.running ? '示波器已启动' : '示波器已停止')
  } catch (error) {
    ElMessage.error('状态切换失败')
    emit('error', '状态切换失败')
  } finally {
    runningChanging.value = false
  }
}

const singleTrigger = () => {
  if (!status.running) return
  
  status.triggered = true
  generateWaveformData()
  
  setTimeout(() => {
    status.triggered = false
  }, 1000)
  
  ElMessage.success('单次触发完成')
}

const onChannelEnabledChange = (channelId: number) => {
  const channel = config.channels.find(ch => ch.id === channelId)
  if (channel) {
    if (!channel.enabled) {
      channel.visible = false
    }
    updateWaveformData()
    emit('config-change', config)
  }
}

const toggleChannelVisible = (channelId: number) => {
  const channel = config.channels.find(ch => ch.id === channelId)
  if (channel && channel.enabled) {
    channel.visible = !channel.visible
    updateWaveformData()
    emit('config-change', config)
  }
}

const onChannelConfigChange = (channelId: number) => {
  updateWaveformData()
  emit('config-change', config)
}

const onTimebaseChange = () => {
  updateWaveformData()
  emit('config-change', config)
}

const onTriggerChange = () => {
  emit('config-change', config)
}

const onAcquisitionChange = () => {
  status.sampleRate = config.acquisition.sampleRate
  updateWaveformData()
  emit('config-change', config)
}

const zoomIn = () => {
  const currentIndex = TIMEBASE_OPTIONS.findIndex(opt => opt.value === config.timebase.scale)
  if (currentIndex > 0) {
    config.timebase.scale = TIMEBASE_OPTIONS[currentIndex - 1].value
    onTimebaseChange()
  }
}

const zoomOut = () => {
  const currentIndex = TIMEBASE_OPTIONS.findIndex(opt => opt.value === config.timebase.scale)
  if (currentIndex < TIMEBASE_OPTIONS.length - 1) {
    config.timebase.scale = TIMEBASE_OPTIONS[currentIndex + 1].value
    onTimebaseChange()
  }
}

const autoScale = () => {
  // 自动缩放逻辑
  config.timebase.scale = 0.001 // 1ms/div
  config.channels.forEach(channel => {
    if (channel.enabled) {
      channel.verticalScale = 1.0 // 1V/div
      channel.verticalOffset = 0
    }
  })
  updateWaveformData()
  emit('config-change', config)
  ElMessage.success('自动缩放完成')
}

const toggleCursors = () => {
  config.cursors.enabled = !config.cursors.enabled
  emit('config-change', config)
}

const addMeasurement = async () => {
  try {
    // 简化测量添加逻辑，使用默认值
    const measurementType = 'frequency' // 默认频率测量
    const channelId = config.channels.find(ch => ch.enabled)?.id || 1
    
    const measurement: AutoMeasurement = {
      id: `measurement_${Date.now()}`,
      type: measurementType as MeasurementType,
      channel: channelId,
      enabled: true,
      value: 0,
      unit: MEASUREMENT_TYPES.find(t => t.value === measurementType)?.unit || '',
      statistics: {
        current: 0,
        minimum: 0,
        maximum: 0,
        mean: 0,
        stdDev: 0,
        count: 0
      }
    }
    
    config.measurements.push(measurement)
    updateMeasurements()
    emit('config-change', config)
    ElMessage.success('测量已添加')
  } catch (error) {
    // 用户取消
  }
}

const removeMeasurement = (measurementId: string) => {
  const index = config.measurements.findIndex(m => m.id === measurementId)
  if (index !== -1) {
    config.measurements.splice(index, 1)
    emit('config-change', config)
    ElMessage.success('测量已删除')
  }
}

const clearMeasurements = () => {
  config.measurements.length = 0
  emit('config-change', config)
  ElMessage.success('所有测量已清除')
}

const onCursorChange = (cursors: any) => {
  config.cursors.cursor1 = cursors.cursor1
  config.cursors.cursor2 = cursors.cursor2
  config.cursors.delta = cursors.delta
  config.cursors.frequency = cursors.frequency
  emit('config-change', config)
}

const onMeasurementChange = (measurements: any[]) => {
  // 更新测量结果
  measurements.forEach(result => {
    const measurement = config.measurements.find(m => m.id === result.id)
    if (measurement) {
      measurement.value = result.value
      measurement.statistics = result.statistics
    }
  })
  emit('measurement-change', config.measurements)
}

const handleMenuCommand = async (command: string) => {
  switch (command) {
    case 'autoset':
      autoSet()
      break
    case 'default':
      await ElMessageBox.confirm('确定要恢复默认设置吗？', '确认重置', {
        type: 'warning'
      })
      Object.assign(config, DEFAULT_OSCILLOSCOPE_CONFIG)
      updateWaveformData()
      ElMessage.success('已恢复默认设置')
      break
    case 'save':
      ElMessage.success('波形已保存')
      break
    case 'recall':
      ElMessage.success('波形已调用')
      break
    case 'calibrate':
      ElMessage.info('自校准功能开发中...')
      break
  }
}

const autoSet = () => {
  // 自动设置逻辑
  ElMessage.success('自动设置完成')
}

// 工具函数
const getMeasurementName = (type: MeasurementType): string => {
  const typeInfo = MEASUREMENT_TYPES.find(t => t.value === type)
  return typeInfo?.label || type
}

const formatMeasurementValue = (value: number, unit: string): string => {
  if (unit === 'Hz') {
    return formatFrequencyValue(value)
  } else if (unit === 'V') {
    return formatVoltageValue(value)
  } else if (unit === 's') {
    return formatTimeValue(value)
  } else if (unit === '%') {
    return `${value.toFixed(2)}%`
  }
  return `${value.toFixed(3)} ${unit}`
}

const formatSampleRate = (rate: number): string => {
  if (rate >= 1000000000) {
    return `${(rate / 1000000000).toFixed(1)}GSa/s`
  } else if (rate >= 1000000) {
    return `${(rate / 1000000).toFixed(1)}MSa/s`
  } else if (rate >= 1000) {
    return `${(rate / 1000).toFixed(1)}kSa/s`
  } else {
    return `${rate}Sa/s`
  }
}

// 数据生成和更新
const generateWaveformData = () => {
  const data: any[] = []
  const timeSpan = config.timebase.scale * 10 // 10个时间格
  const sampleCount = 1000
  const timeStep = timeSpan / sampleCount
  
  config.channels.forEach(channel => {
    if (!channel.enabled || !channel.visible) return
    
    const channelData = {
      name: channel.label,
      color: channel.color,
      data: [] as Array<[number, number]>
    }
    
    for (let i = 0; i < sampleCount; i++) {
      const time = -timeSpan / 2 + i * timeStep
      let value = 0
      
      // 生成模拟波形数据
      switch (channel.id) {
        case 1:
          // CH1: 正弦波
          value = Math.sin(2 * Math.PI * 1000 * time) * channel.verticalScale + channel.verticalOffset
          break
        case 2:
          // CH2: 方波
          value = (Math.sin(2 * Math.PI * 500 * time) > 0 ? 1 : -1) * channel.verticalScale + channel.verticalOffset
          break
        case 3:
          // CH3: 三角波
          value = (2 / Math.PI) * Math.asin(Math.sin(2 * Math.PI * 200 * time)) * channel.verticalScale + channel.verticalOffset
          break
        case 4:
          // CH4: 噪声
          value = (Math.random() - 0.5) * 2 * channel.verticalScale + channel.verticalOffset
          break
      }
      
      // 应用探头倍数
      value *= channel.probe
      
      // 应用反相
      if (channel.invert) {
        value = -value
      }
      
      channelData.data.push([time, value])
    }
    
    data.push(channelData)
  })
  
  waveformData.value = { series: data }
  emit('waveform-data', data as any)
}

const updateWaveformData = () => {
  if (status.running) {
    generateWaveformData()
  }
}

const startDataGeneration = () => {
  generateWaveformData()
  dataTimer = setInterval(() => {
    if (status.running) {
      generateWaveformData()
      updateMeasurements()
      updateTriggerStatus()
    }
  }, 100) // 10fps更新
}

const stopDataGeneration = () => {
  if (dataTimer) {
    clearInterval(dataTimer)
    dataTimer = null
  }
}

const updateMeasurements = () => {
  // 更新自动测量结果
  config.measurements.forEach(measurement => {
    const channelData = waveformData.value.series.find((d: any) => d.name === `CH${measurement.channel}`)
    if (channelData && channelData.data.length > 0) {
      const values = channelData.data.map((point: [number, number]) => point[1])
      
      switch (measurement.type) {
        case 'frequency':
          measurement.value = 1000 + Math.random() * 100 // 模拟频率测量
          break
        case 'amplitude':
          measurement.value = Math.max(...values) - Math.min(...values)
          break
        case 'rms':
          const rms = Math.sqrt(values.reduce((sum: number, v: number) => sum + v * v, 0) / values.length)
          measurement.value = rms
          break
        case 'mean':
          measurement.value = values.reduce((sum: number, v: number) => sum + v, 0) / values.length
          break
        default:
          measurement.value = Math.random() * 10
      }
      
      // 更新统计信息
      measurement.statistics.current = measurement.value
      measurement.statistics.count++
      
      if (measurement.statistics.count === 1) {
        measurement.statistics.minimum = measurement.value
        measurement.statistics.maximum = measurement.value
        measurement.statistics.mean = measurement.value
      } else {
        measurement.statistics.minimum = Math.min(measurement.statistics.minimum, measurement.value)
        measurement.statistics.maximum = Math.max(measurement.statistics.maximum, measurement.value)
        measurement.statistics.mean = (measurement.statistics.mean * (measurement.statistics.count - 1) + measurement.value) / measurement.statistics.count
      }
    }
  })
}

const updateTriggerStatus = () => {
  // 模拟触发状态更新
  if (config.trigger.mode === 'auto') {
    status.triggered = Math.random() > 0.7
    status.triggerRate = 50 + Math.random() * 100
  } else if (config.trigger.mode === 'normal') {
    status.triggered = Math.random() > 0.8
    status.triggerRate = 20 + Math.random() * 50
  }
  
  emit('trigger-change', status.triggered)
}

const updateStatus = () => {
  // 更新设备状态
  status.temperature = 28 + Math.random() * 5
  status.memoryUsage = 20 + Math.random() * 30
  
  if (status.running) {
    status.sampleRate = config.acquisition.sampleRate
  }
}

// 生命周期
onMounted(() => {
  nextTick(() => {
    updateWaveformData()
    
    // 启动状态更新定时器
    statusTimer = setInterval(updateStatus, 1000)
  })
})

onUnmounted(() => {
  stopDataGeneration()
  if (statusTimer) {
    clearInterval(statusTimer)
    statusTimer = null
  }
})

// 监听配置变化
watch(() => config.timebase.scale, () => {
  updateWaveformData()
}, { deep: true })

watch(() => config.channels, () => {
  updateWaveformData()
}, { deep: true })
</script>

<style lang="scss" scoped>
.oscilloscope {
  width: 100%;
  height: 100%;
  background: var(--instrument-bg);
  border: 1px solid var(--instrument-border);
  border-radius: 8px;
  overflow: hidden;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

.oscilloscope-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  background: linear-gradient(135deg, #2E86AB 0%, #A23B72 100%);
  color: white;
  
  .header-left {
    .oscilloscope-title {
      display: flex;
      align-items: center;
      gap: 8px;
      margin: 0;
      font-size: 18px;
      font-weight: 600;
    }
    
    .oscilloscope-model {
      font-size: 12px;
      opacity: 0.8;
      margin-top: 4px;
    }
  }
  
  .header-right {
    display: flex;
    gap: 12px;
    align-items: center;
  }
}

.oscilloscope-content {
  display: grid;
  grid-template-columns: 350px 1fr;
  gap: 16px;
  padding: 16px;
  height: calc(100% - 80px);
  overflow: hidden;
}

.control-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
  overflow-y: auto;
  
  .control-card {
    .card-header {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 600;
    }
  }
  
  .channels-control {
    display: flex;
    flex-direction: column;
    gap: 16px;
    
    .channel-item {
      border: 1px solid var(--border-color);
      border-radius: 8px;
      padding: 12px;
      
      &.disabled {
        opacity: 0.5;
      }
      
      .channel-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 8px;
        
        .channel-label {
          font-weight: 600;
          font-size: 14px;
        }
        
        .el-button.active {
          color: var(--primary-color);
        }
      }
      
      .channel-controls {
        display: flex;
        flex-direction: column;
        gap: 8px;
        
        .control-row {
          display: flex;
          justify-content: space-between;
          align-items: center;
          
          label {
            font-size: 12px;
            color: var(--text-secondary);
            min-width: 60px;
          }
          
          .el-select,
          .el-input-number {
            width: 120px;
          }
        }
      }
    }
  }
  
  .timebase-controls,
  .trigger-controls,
  .acquisition-controls {
    display: flex;
    flex-direction: column;
    gap: 12px;
    
    .control-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      
      label {
        font-size: 13px;
        color: var(--text-secondary);
        min-width: 70px;
      }
      
      .el-select,
      .el-input-number {
        width: 140px;
      }
      
      .el-slider {
        flex: 1;
        margin: 0 12px;
      }
    }
  }
}

.display-panel {
  display: flex;
  flex-direction: column;
  gap: 16px;
  overflow-y: auto;
  
  .display-card {
    .card-header {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 600;
      
      .header-actions {
        margin-left: auto;
        display: flex;
        gap: 8px;
      }
    }
  }
  
  .waveform-card {
    flex: 1;
    min-height: 400px;
    
    .waveform-display {
      position: relative;
      height: 400px;
      
      .trigger-indicator {
        position: absolute;
        top: 10px;
        right: 10px;
        display: flex;
        align-items: center;
        gap: 4px;
        background: rgba(16, 185, 129, 0.9);
        color: white;
        padding: 4px 8px;
        border-radius: 4px;
        font-size: 12px;
        
        .trigger-icon {
          animation: pulse 1s infinite;
        }
      }
    }
  }
  
  .measurements-card {
    .measurements-content {
      display: flex;
      flex-direction: column;
      gap: 16px;
      
      h4 {
        margin: 0 0 8px 0;
        font-size: 14px;
        color: var(--text-primary);
      }
      
      .cursor-results {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 12px;
        
        .cursor-item {
          display: flex;
          justify-content: space-between;
          padding: 8px;
          background: var(--background-color);
          border-radius: 4px;
          
          .label {
            font-weight: 600;
            color: var(--text-secondary);
          }
          
          .value {
            font-family: 'Consolas', monospace;
            color: var(--text-primary);
          }
        }
      }
      
      .measurement-list {
        display: flex;
        flex-direction: column;
        gap: 8px;
        
        .measurement-item {
          border: 1px solid var(--border-color);
          border-radius: 6px;
          padding: 12px;
          
          .measurement-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 8px;
            
            .measurement-name {
              font-weight: 600;
              color: var(--text-primary);
            }
            
            .measurement-channel {
              font-size: 12px;
              color: var(--text-secondary);
              background: var(--primary-color);
              color: white;
              padding: 2px 6px;
              border-radius: 3px;
            }
          }
          
          .measurement-value {
            font-size: 18px;
            font-weight: 700;
            font-family: 'Consolas', monospace;
            color: var(--primary-color);
            margin-bottom: 8px;
          }
          
          .measurement-stats {
            display: flex;
            gap: 12px;
            font-size: 11px;
            color: var(--text-secondary);
            
            span {
              white-space: nowrap;
            }
          }
        }
      }
    }
  }
  
  .status-card {
    .status-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 16px;
      
      .status-item {
        .status-label {
          font-size: 12px;
          color: var(--text-secondary);
          margin-bottom: 4px;
        }
        
        .status-value {
          font-size: 14px;
          font-weight: 600;
          color: var(--text-primary);
          display: flex;
          align-items: center;
          gap: 4px;
          
          &.active {
            color: #10b981;
          }
          
          &.warning {
            color: #f59e0b;
          }
          
          &.error {
            color: #ef4444;
          }
        }
      }
    }
  }
}

// 动画
@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

// 响应式设计
@media (max-width: 1200px) {
  .oscilloscope-content {
    grid-template-columns: 1fr;
    grid-template-rows: auto 1fr;
  }
  
  .control-panel {
    max-height: 300px;
  }
}

@media (max-width: 768px) {
  .oscilloscope-header {
    flex-direction: column;
    gap: 12px;
    text-align: center;
    
    .header-right {
      width: 100%;
      justify-content: center;
    }
  }
  
  .oscilloscope-content {
    padding: 12px;
    gap: 12px;
  }
  
  .control-row {
    flex-direction: column;
    align-items: stretch;
    gap: 8px;
    
    label {
      min-width: auto;
    }
    
    .el-select,
    .el-input-number {
      width: 100%;
    }
  }
  
  .status-grid {
    grid-template-columns: 1fr;
  }
}

// Element Plus 样式覆盖
:deep(.el-card__header) {
  padding: 12px 16px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
}

:deep(.el-card__body) {
  padding: 16px;
}

:deep(.el-checkbox) {
  margin-right: 0;
  
  .el-checkbox__label {
    font-size: 13px;
  }
}

:deep(.el-radio-group) {
  .el-radio-button__inner {
    padding: 4px 8px;
    font-size: 12px;
  }
}

:deep(.el-slider) {
  .el-slider__runway {
    height: 4px;
  }
  
  .el-slider__button {
    width: 12px;
    height: 12px;
  }
}
</style>
