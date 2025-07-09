<template>
  <div class="temperature-acquisition-card">
    <!-- 卡片头部 -->
    <div class="card-header">
      <div class="header-left">
        <h3 class="card-title">
          <el-icon><svg viewBox="0 0 1024 1024" width="1em" height="1em"><path d="M512 64c-70.692 0-128 57.308-128 128v448c-53.02 36.788-88 97.564-88 166.4 0 111.046 89.954 201 201 201s201-89.954 201-201c0-68.836-34.98-129.612-88-166.4V192c0-70.692-57.308-128-128-128zm0 64c35.346 0 64 28.654 64 64v448c0 17.673 14.327 32 32 32 70.692 0 128 57.308 128 128s-57.308 128-128 128-128-57.308-128-128c0-70.692 57.308-128 128-128 17.673 0 32-14.327 32-32V192c0-35.346 28.654-64 64-64z" fill="currentColor"/></svg></el-icon>
          温度采集卡
        </h3>
        <el-tag :type="acquisitionStatus === 'running' ? 'success' : 'info'" size="small">
          {{ acquisitionStatusText }}
        </el-tag>
      </div>
      
      <div class="header-right">
        <el-button-group>
          <el-button 
            :type="acquisitionStatus === 'running' ? 'danger' : 'primary'"
            @click="toggleAcquisition"
            :loading="isStarting"
          >
            <el-icon><VideoPlay v-if="acquisitionStatus !== 'running'" /><VideoPause v-else /></el-icon>
            {{ acquisitionStatus === 'running' ? '停止采集' : '开始采集' }}
          </el-button>
          
          <el-button @click="showConfigDialog = true">
            <el-icon><Setting /></el-icon>
            配置
          </el-button>
          
          <el-button @click="exportData">
            <el-icon><Download /></el-icon>
            导出
          </el-button>
        </el-button-group>
      </div>
    </div>

    <!-- 全局状态面板 -->
    <div class="global-status-panel">
      <el-row :gutter="16">
        <el-col :span="6">
          <div class="status-item">
            <span class="label">采样率:</span>
            <span class="value">{{ config.sampleRate }} Hz</span>
          </div>
        </el-col>
        <el-col :span="6">
          <div class="status-item">
            <span class="label">活动通道:</span>
            <span class="value">{{ activeChannelCount }}/{{ config.channels.length }}</span>
          </div>
        </el-col>
        <el-col :span="6">
          <div class="status-item">
            <span class="label">冷端补偿:</span>
            <span class="value">{{ coldJunctionText }}</span>
          </div>
        </el-col>
        <el-col :span="6">
          <div class="status-item">
            <span class="label">数据点数:</span>
            <span class="value">{{ totalDataPoints }}</span>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 通道显示区域 -->
    <div class="channels-container">
      <el-row :gutter="16">
        <el-col 
          v-for="channel in config.channels" 
          :key="channel.id"
          :span="channelSpan"
        >
          <div 
            class="channel-card"
            :class="{ 
              'channel-disabled': !channel.enabled,
              'channel-alarm': getChannelAlarmStatus(channel.id) !== 'normal'
            }"
          >
            <!-- 通道头部 -->
            <div class="channel-header">
              <div class="channel-info">
                <span class="channel-name">{{ channel.name }}</span>
                <el-tag size="small" :color="channel.color">{{ channel.thermocoupleType }}</el-tag>
              </div>
              
              <div class="channel-controls">
                <el-switch 
                  v-model="channel.enabled" 
                  size="small"
                  @change="onChannelEnabledChange(channel.id, $event)"
                />
                <el-button 
                  size="small" 
                  text 
                  @click="showChannelConfig(channel)"
                >
                  <el-icon><Setting /></el-icon>
                </el-button>
              </div>
            </div>

            <!-- 数字显示 -->
            <div class="temperature-display">
              <DigitalDisplay
                :data="{
                  value: getCurrentTemperature(channel.id),
                  unit: channel.unit,
                  title: ''
                }"
                :options="{
                  digitCount: 6,
                  decimalPlaces: 1,
                  size: 40,
                  color: getDisplayColor(channel),
                  backgroundColor: '#1a1a1a',
                  segmentColor: '#333',
                  activeColor: getDisplayColor(channel),
                  showControls: false
                }"
              />
            </div>

            <!-- 统计信息 -->
            <div class="channel-stats">
              <div class="stat-row">
                <span class="stat-label">最小:</span>
                <span class="stat-value">{{ getChannelStats(channel.id).min.toFixed(1) }}°{{ channel.unit }}</span>
              </div>
              <div class="stat-row">
                <span class="stat-label">最大:</span>
                <span class="stat-value">{{ getChannelStats(channel.id).max.toFixed(1) }}°{{ channel.unit }}</span>
              </div>
              <div class="stat-row">
                <span class="stat-label">平均:</span>
                <span class="stat-value">{{ getChannelStats(channel.id).average.toFixed(1) }}°{{ channel.unit }}</span>
              </div>
              <div class="stat-row">
                <span class="stat-label">趋势:</span>
                <span class="stat-value" :class="getTrendClass(channel.id)">
                  {{ getTrendText(channel.id) }}
                </span>
              </div>
            </div>

            <!-- 报警状态 -->
            <div v-if="channel.alarmEnabled" class="alarm-status">
              <el-alert
                v-if="getChannelAlarmStatus(channel.id) !== 'normal'"
                :type="getChannelAlarmStatus(channel.id) === 'high' ? 'error' : 'warning'"
                :title="getAlarmMessage(channel.id)"
                size="small"
                show-icon
                :closable="false"
              />
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 温度趋势图表 -->
    <div class="trend-chart-container">
      <el-card>
        <template #header>
          <div class="chart-header">
            <span>温度趋势图</span>
            <div class="chart-controls">
              <el-select v-model="trendTimeRange" size="small" style="width: 120px">
                <el-option label="1分钟" value="1" />
                <el-option label="5分钟" value="5" />
                <el-option label="15分钟" value="15" />
                <el-option label="30分钟" value="30" />
                <el-option label="1小时" value="60" />
              </el-select>
              
              <el-button size="small" @click="clearTrendData">
                <el-icon><Delete /></el-icon>
                清除
              </el-button>
            </div>
          </div>
        </template>
        
        <EasyChart
          :data="trendChartData"
          :series-configs="trendSeriesConfigs"
          :options="trendChartOptions"
          :show-toolbar="false"
          :show-controls="false"
          :show-status="false"
          style="height: 300px"
        />
      </el-card>
    </div>

    <!-- 配置对话框 -->
    <el-dialog
      v-model="showConfigDialog"
      title="温度采集卡配置"
      width="800px"
      :close-on-click-modal="false"
    >
      <el-tabs v-model="configActiveTab">
        <!-- 全局配置 -->
        <el-tab-pane label="全局配置" name="global">
          <el-form :model="config" label-width="120px">
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="采样率 (Hz):">
                  <el-input-number 
                    v-model="config.sampleRate" 
                    :min="0.1" 
                    :max="1000" 
                    :step="0.1"
                    style="width: 100%"
                  />
                </el-form-item>
              </el-col>
              
              <el-col :span="12">
                <el-form-item label="平均次数:">
                  <el-input-number 
                    v-model="config.averagingCount" 
                    :min="1" 
                    :max="100"
                    style="width: 100%"
                  />
                </el-form-item>
              </el-col>
            </el-row>
            
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="冷端补偿源:">
                  <el-select v-model="config.coldJunctionSource" style="width: 100%">
                    <el-option label="内部传感器" value="internal" />
                    <el-option label="外部传感器" value="external" />
                    <el-option label="固定温度" value="fixed" />
                  </el-select>
                </el-form-item>
              </el-col>
              
              <el-col :span="12">
                <el-form-item label="冷端温度 (°C):">
                  <el-input-number 
                    v-model="config.coldJunctionTemperature" 
                    :min="-50" 
                    :max="100" 
                    :step="0.1"
                    :disabled="config.coldJunctionSource !== 'fixed'"
                    style="width: 100%"
                  />
                </el-form-item>
              </el-col>
            </el-row>
            
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="数字滤波器:">
                  <el-switch v-model="config.filterEnabled" />
                </el-form-item>
              </el-col>
              
              <el-col :span="12">
                <el-form-item label="截止频率 (Hz):">
                  <el-input-number 
                    v-model="config.filterCutoff" 
                    :min="0.1" 
                    :max="100" 
                    :step="0.1"
                    :disabled="!config.filterEnabled"
                    style="width: 100%"
                  />
                </el-form-item>
              </el-col>
            </el-row>
          </el-form>
        </el-tab-pane>

        <!-- 通道配置 -->
        <el-tab-pane label="通道配置" name="channels">
          <div class="channel-config-list">
            <div 
              v-for="channel in config.channels" 
              :key="channel.id"
              class="channel-config-item"
            >
              <el-card>
                <template #header>
                  <div class="channel-config-header">
                    <span>通道 {{ channel.id }} - {{ channel.name }}</span>
                    <el-switch v-model="channel.enabled" />
                  </div>
                </template>
                
                <el-form :model="channel" label-width="100px" size="small">
                  <el-row :gutter="16">
                    <el-col :span="8">
                      <el-form-item label="通道名称:">
                        <el-input v-model="channel.name" />
                      </el-form-item>
                    </el-col>
                    
                    <el-col :span="8">
                      <el-form-item label="热电偶类型:">
                        <el-select v-model="channel.thermocoupleType">
                          <el-option 
                            v-for="(type, key) in THERMOCOUPLE_TYPES" 
                            :key="key"
                            :label="type.name" 
                            :value="key"
                          />
                        </el-select>
                      </el-form-item>
                    </el-col>
                    
                    <el-col :span="8">
                      <el-form-item label="温度单位:">
                        <el-select v-model="channel.unit">
                          <el-option label="摄氏度 (°C)" value="C" />
                          <el-option label="华氏度 (°F)" value="F" />
                          <el-option label="开尔文 (K)" value="K" />
                        </el-select>
                      </el-form-item>
                    </el-col>
                  </el-row>
                  
                  <el-row :gutter="16">
                    <el-col :span="6">
                      <el-form-item label="最小值:">
                        <el-input-number v-model="channel.range.min" :step="1" />
                      </el-form-item>
                    </el-col>
                    
                    <el-col :span="6">
                      <el-form-item label="最大值:">
                        <el-input-number v-model="channel.range.max" :step="1" />
                      </el-form-item>
                    </el-col>
                    
                    <el-col :span="6">
                      <el-form-item label="偏移校准:">
                        <el-input-number v-model="channel.offset" :step="0.1" />
                      </el-form-item>
                    </el-col>
                    
                    <el-col :span="6">
                      <el-form-item label="增益校准:">
                        <el-input-number v-model="channel.gain" :step="0.01" :min="0.1" :max="10" />
                      </el-form-item>
                    </el-col>
                  </el-row>
                  
                  <el-row :gutter="16">
                    <el-col :span="8">
                      <el-form-item label="冷端补偿:">
                        <el-switch v-model="channel.coldJunctionCompensation" />
                      </el-form-item>
                    </el-col>
                    
                    <el-col :span="8">
                      <el-form-item label="线性化:">
                        <el-switch v-model="channel.linearization" />
                      </el-form-item>
                    </el-col>
                    
                    <el-col :span="8">
                      <el-form-item label="报警使能:">
                        <el-switch v-model="channel.alarmEnabled" />
                      </el-form-item>
                    </el-col>
                  </el-row>
                  
                  <el-row v-if="channel.alarmEnabled" :gutter="16">
                    <el-col :span="6">
                      <el-form-item label="低报警:">
                        <el-input-number v-model="channel.alarmLow" :step="1" />
                      </el-form-item>
                    </el-col>
                    
                    <el-col :span="6">
                      <el-form-item label="高报警:">
                        <el-input-number v-model="channel.alarmHigh" :step="1" />
                      </el-form-item>
                    </el-col>
                    
                    <el-col :span="12">
                      <el-form-item label="显示颜色:">
                        <el-color-picker v-model="channel.color" />
                      </el-form-item>
                    </el-col>
                  </el-row>
                </el-form>
              </el-card>
            </div>
          </div>
        </el-tab-pane>
      </el-tabs>
      
      <template #footer>
        <el-button @click="showConfigDialog = false">取消</el-button>
        <el-button type="primary" @click="saveConfig">保存配置</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { 
  VideoPlay, VideoPause, Setting, Download, Delete 
} from '@element-plus/icons-vue'
import DigitalDisplay from '@/components/professional/indicators/DigitalDisplay.vue'
import EasyChart from '@/components/professional/charts/EasyChart.vue'
import type { ChartData, ChartOptions, SeriesConfig } from '@/types/chart'
import type {
  TemperatureAcquisitionConfig,
  TemperatureChannelConfig,
  TemperatureData,
  TemperatureStatistics,
  ThermocoupleType,
  TemperatureUnit
} from '@/types/temperature'
import {
  THERMOCOUPLE_TYPES,
  convertTemperature,
  calculateThermocoupleTemperature
} from '@/types/temperature'

// Props
const props = withDefaults(defineProps<{
  initialConfig?: Partial<TemperatureAcquisitionConfig>
  autoStart?: boolean
}>(), {
  autoStart: false
})

// Emits
const emit = defineEmits<{
  'data-update': [data: TemperatureData[]]
  'alarm': [channelId: number, type: 'low' | 'high', value: number]
  'config-change': [config: TemperatureAcquisitionConfig]
}>()

// 响应式数据
const acquisitionStatus = ref<'stopped' | 'running' | 'error'>('stopped')
const isStarting = ref(false)
const showConfigDialog = ref(false)
const configActiveTab = ref('global')
const trendTimeRange = ref('5')

// 默认配置
const defaultConfig: TemperatureAcquisitionConfig = {
  channels: [
    {
      id: 1,
      name: '通道1',
      enabled: true,
      thermocoupleType: 'K',
      unit: 'C',
      range: { min: -200, max: 1200 },
      offset: 0,
      gain: 1,
      coldJunctionCompensation: true,
      linearization: true,
      alarmEnabled: false,
      alarmLow: 0,
      alarmHigh: 100,
      color: '#ff6b6b'
    },
    {
      id: 2,
      name: '通道2',
      enabled: true,
      thermocoupleType: 'K',
      unit: 'C',
      range: { min: -200, max: 1200 },
      offset: 0,
      gain: 1,
      coldJunctionCompensation: true,
      linearization: true,
      alarmEnabled: false,
      alarmLow: 0,
      alarmHigh: 100,
      color: '#4ecdc4'
    },
    {
      id: 3,
      name: '通道3',
      enabled: false,
      thermocoupleType: 'J',
      unit: 'C',
      range: { min: -200, max: 1000 },
      offset: 0,
      gain: 1,
      coldJunctionCompensation: true,
      linearization: true,
      alarmEnabled: false,
      alarmLow: 0,
      alarmHigh: 100,
      color: '#45b7d1'
    },
    {
      id: 4,
      name: '通道4',
      enabled: false,
      thermocoupleType: 'T',
      unit: 'C',
      range: { min: -200, max: 400 },
      offset: 0,
      gain: 1,
      coldJunctionCompensation: true,
      linearization: true,
      alarmEnabled: false,
      alarmLow: 0,
      alarmHigh: 100,
      color: '#f7b731'
    }
  ],
  sampleRate: 1,
  averagingCount: 10,
  autoRange: false,
  coldJunctionSource: 'internal',
  coldJunctionTemperature: 25,
  filterEnabled: true,
  filterCutoff: 1
}

const config = ref<TemperatureAcquisitionConfig>({
  ...defaultConfig,
  ...props.initialConfig
})

// 数据存储
const temperatureData = ref<Map<number, TemperatureData[]>>(new Map())
const statistics = ref<Map<number, TemperatureStatistics>>(new Map())
const trendData = ref<Map<number, Array<{ timestamp: number; value: number }>>>(new Map())

// 定时器
let acquisitionTimer: number | null = null
let trendUpdateTimer: number | null = null

// 计算属性
const acquisitionStatusText = computed(() => {
  switch (acquisitionStatus.value) {
    case 'running': return '采集中'
    case 'stopped': return '已停止'
    case 'error': return '错误'
    default: return '未知'
  }
})

const activeChannelCount = computed(() => {
  return config.value.channels.filter(ch => ch.enabled).length
})

const coldJunctionText = computed(() => {
  switch (config.value.coldJunctionSource) {
    case 'internal': return '内部传感器'
    case 'external': return '外部传感器'
    case 'fixed': return `固定 ${config.value.coldJunctionTemperature}°C`
    default: return '未知'
  }
})

const totalDataPoints = computed(() => {
  let total = 0
  temperatureData.value.forEach(data => {
    total += data.length
  })
  return total
})

const channelSpan = computed(() => {
  const enabledCount = activeChannelCount.value
  if (enabledCount <= 2) return 12
  if (enabledCount <= 4) return 6
  return 4
})

// 趋势图表数据
const trendChartData = computed<ChartData>(() => {
  const series: number[][] = []
  const enabledChannels = config.value.channels.filter(ch => ch.enabled)
  
  enabledChannels.forEach(channel => {
    const channelTrend = trendData.value.get(channel.id) || []
    const values = channelTrend.map(point => point.value)
    series.push(values)
  })
  
  return {
    series,
    xStart: 0,
    xInterval: parseInt(trendTimeRange.value) * 60 / 100 // 假设100个数据点
  }
})

const trendSeriesConfigs = computed<SeriesConfig[]>(() => {
  return config.value.channels
    .filter(ch => ch.enabled)
    .map(channel => ({
      name: channel.name,
      color: channel.color,
      lineWidth: 2,
      lineType: 'solid',
      markerType: 'none',
      markerSize: 4,
      visible: true
    }))
})

const trendChartOptions = computed<Partial<ChartOptions>>(() => ({
  autoScale: true,
  legendVisible: true,
  cursorMode: 'zoom',
  gridEnabled: true,
  theme: 'light'
}))

// 方法
const toggleAcquisition = async () => {
  if (acquisitionStatus.value === 'running') {
    stopAcquisition()
  } else {
    await startAcquisition()
  }
}

const startAcquisition = async () => {
  isStarting.value = true
  
  try {
    // 初始化数据存储
    config.value.channels.forEach(channel => {
      if (channel.enabled) {
        temperatureData.value.set(channel.id, [])
        statistics.value.set(channel.id, {
          channelId: channel.id,
          current: 0,
          min: Infinity,
          max: -Infinity,
          average: 0,
          standardDeviation: 0,
          trend: 'stable',
          trendRate: 0
        })
        trendData.value.set(channel.id, [])
      }
    })
    
    // 启动数据采集
    acquisitionTimer = setInterval(() => {
      generateTemperatureData()
    }, 1000 / config.value.sampleRate)
    
    // 启动趋势数据更新
    trendUpdateTimer = setInterval(() => {
      updateTrendData()
    }, 5000) // 每5秒更新一次趋势数据
    
    acquisitionStatus.value = 'running'
  } catch (error) {
    console.error('启动采集失败:', error)
    acquisitionStatus.value = 'error'
  } finally {
    isStarting.value = false
  }
}

const stopAcquisition = () => {
  if (acquisitionTimer) {
    clearInterval(acquisitionTimer)
    acquisitionTimer = null
  }
  
  if (trendUpdateTimer) {
    clearInterval(trendUpdateTimer)
    trendUpdateTimer = null
  }
  
  acquisitionStatus.value = 'stopped'
}

const generateTemperatureData = () => {
  const now = Date.now()
  const newData: TemperatureData[] = []
  
  config.value.channels.forEach(channel => {
    if (!channel.enabled) return
    
    // 模拟温度数据生成
    const baseTemp = 25 + Math.sin(now / 10000) * 20 + Math.random() * 5
    const voltage = baseTemp * 0.041 // 模拟K型热电偶电压
    
    let temperature = calculateThermocoupleTemperature(
      voltage,
      channel.thermocoupleType,
      config.value.coldJunctionTemperature
    )
    
    // 应用校准
    temperature = (temperature + channel.offset) * channel.gain
    
    // 转换单位
    if (channel.unit !== 'C') {
      temperature = convertTemperature(temperature, 'C', channel.unit)
    }
    
    // 检查报警
    let alarmStatus: 'normal' | 'low' | 'high' = 'normal'
    if (channel.alarmEnabled) {
      if (temperature < channel.alarmLow) {
        alarmStatus = 'low'
        emit('alarm', channel.id, 'low', temperature)
      } else if (temperature > channel.alarmHigh) {
        alarmStatus = 'high'
        emit('alarm', channel.id, 'high', temperature)
      }
    }
    
    const data: TemperatureData = {
      channelId: channel.id,
      value: temperature,
      unit: channel.unit,
      timestamp: now,
      quality: 'good',
      alarmStatus
    }
    
    newData.push(data)
    
    // 更新数据存储
    const channelData = temperatureData.value.get(channel.id) || []
    channelData.push(data)
    
    // 限制数据长度
    if (channelData.length > 1000) {
      channelData.splice(0, channelData.length - 1000)
    }
    
    temperatureData.value.set(channel.id, channelData)
    
    // 更新统计信息
    updateStatistics(channel.id, channelData)
  })
  
  emit('data-update', newData)
}

const updateStatistics = (channelId: number, data: TemperatureData[]) => {
  if (data.length === 0) return
  
  const values = data.map(d => d.value)
  const current = values[values.length - 1]
  const min = Math.min(...values)
  const max = Math.max(...values)
  const average = values.reduce((sum, val) => sum + val, 0) / values.length
  
  // 计算标准差
  const variance = values.reduce((sum, val) => sum + Math.pow(val - average, 2), 0) / values.length
  const standardDeviation = Math.sqrt(variance)
  
  // 计算趋势
  let trend: 'rising' | 'falling' | 'stable' = 'stable'
  let trendRate = 0
  
  if (values.length >= 10) {
    const recent = values.slice(-10)
    const older = values.slice(-20, -10)
    
    if (recent.length > 0 && older.length > 0) {
      const recentAvg = recent.reduce((sum, val) => sum + val, 0) / recent.length
      const olderAvg = older.reduce((sum, val) => sum + val, 0) / older.length
      
      trendRate = (recentAvg - olderAvg) * 6 // 转换为每分钟的变化率
      
      if (Math.abs(trendRate) > 0.1) {
        trend = trendRate > 0 ? 'rising' : 'falling'
      }
    }
  }
  
  statistics.value.set(channelId, {
    channelId,
    current,
    min,
    max,
    average,
    standardDeviation,
    trend,
    trendRate
  })
}

const updateTrendData = () => {
  const now = Date.now()
  const timeRangeMs = parseInt(trendTimeRange.value) * 60 * 1000
  
  config.value.channels.forEach(channel => {
    if (!channel.enabled) return
    
    const channelData = temperatureData.value.get(channel.id) || []
    if (channelData.length === 0) return
    
    const currentTemp = channelData[channelData.length - 1].value
    const trend = trendData.value.get(channel.id) || []
    
    trend.push({ timestamp: now, value: currentTemp })
    
    // 清理过期数据
    const cutoffTime = now - timeRangeMs
    const filteredTrend = trend.filter(point => point.timestamp > cutoffTime)
    
    trendData.value.set(channel.id, filteredTrend)
  })
}

const getCurrentTemperature = (channelId: number): number => {
  const data = temperatureData.value.get(channelId)
  return data && data.length > 0 ? data[data.length - 1].value : 0
}

const getChannelStats = (channelId: number): TemperatureStatistics => {
  return statistics.value.get(channelId) || {
    channelId,
    current: 0,
    min: 0,
    max: 0,
    average: 0,
    standardDeviation: 0,
    trend: 'stable',
    trendRate: 0
  }
}

const getChannelAlarmStatus = (channelId: number): 'normal' | 'low' | 'high' => {
  const data = temperatureData.value.get(channelId)
  if (!data || data.length === 0) return 'normal'
  return data[data.length - 1].alarmStatus
}

const getDisplayColor = (channel: TemperatureChannelConfig): string => {
  const alarmStatus = getChannelAlarmStatus(channel.id)
  if (alarmStatus === 'high') return '#ff4757'
  if (alarmStatus === 'low') return '#ffa502'
  return channel.color
}

const getTrendClass = (channelId: number): string => {
  const stats = getChannelStats(channelId)
  switch (stats.trend) {
    case 'rising': return 'trend-rising'
    case 'falling': return 'trend-falling'
    default: return 'trend-stable'
  }
}

const getTrendText = (channelId: number): string => {
  const stats = getChannelStats(channelId)
  switch (stats.trend) {
    case 'rising': return `↗ +${Math.abs(stats.trendRate).toFixed(1)}°/min`
    case 'falling': return `↘ -${Math.abs(stats.trendRate).toFixed(1)}°/min`
    default: return '→ 稳定'
  }
}

const getAlarmMessage = (channelId: number): string => {
  const channel = config.value.channels.find(ch => ch.id === channelId)
  const alarmStatus = getChannelAlarmStatus(channelId)
  const currentTemp = getCurrentTemperature(channelId)
  
  if (!channel) return ''
  
  if (alarmStatus === 'high') {
    return `${channel.name} 高温报警: ${currentTemp.toFixed(1)}°${channel.unit} > ${channel.alarmHigh}°${channel.unit}`
  } else if (alarmStatus === 'low') {
    return `${channel.name} 低温报警: ${currentTemp.toFixed(1)}°${channel.unit} < ${channel.alarmLow}°${channel.unit}`
  }
  
  return ''
}

const onChannelEnabledChange = (channelId: number, enabled: boolean) => {
  const channel = config.value.channels.find(ch => ch.id === channelId)
  if (channel) {
    channel.enabled = enabled
    emit('config-change', config.value)
  }
}

const showChannelConfig = (channel: TemperatureChannelConfig) => {
  showConfigDialog.value = true
  configActiveTab.value = 'channels'
}

const clearTrendData = () => {
  trendData.value.clear()
}

const exportData = () => {
  // 导出温度数据为CSV格式
  const csvData: string[] = []
  const headers = ['时间戳', '通道ID', '通道名称', '温度值', '单位', '质量', '报警状态']
  csvData.push(headers.join(','))
  
  temperatureData.value.forEach((data, channelId) => {
    const channel = config.value.channels.find(ch => ch.id === channelId)
    data.forEach(point => {
      const row = [
        new Date(point.timestamp).toISOString(),
        point.channelId.toString(),
        channel?.name || `通道${channelId}`,
        point.value.toFixed(3),
        point.unit,
        point.quality,
        point.alarmStatus
      ]
      csvData.push(row.join(','))
    })
  })
  
  const blob = new Blob([csvData.join('\n')], { type: 'text/csv;charset=utf-8;' })
  const link = document.createElement('a')
  const url = URL.createObjectURL(blob)
  link.setAttribute('href', url)
  link.setAttribute('download', `temperature_data_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.csv`)
  link.style.visibility = 'hidden'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}

const saveConfig = () => {
  emit('config-change', config.value)
  showConfigDialog.value = false
}

// 生命周期
onMounted(() => {
  if (props.autoStart) {
    startAcquisition()
  }
})

onUnmounted(() => {
  stopAcquisition()
})

// 监听器
watch(() => props.initialConfig, (newConfig) => {
  if (newConfig) {
    Object.assign(config.value, newConfig)
  }
}, { deep: true })
</script>

<style lang="scss" scoped>
.temperature-acquisition-card {
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
  
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
    padding: 16px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    
    .header-left {
      display: flex;
      align-items: center;
      gap: 12px;
      
      .card-title {
        margin: 0;
        display: flex;
        align-items: center;
        gap: 8px;
        color: #303133;
        font-size: 18px;
        font-weight: 600;
      }
    }
  }
  
  .global-status-panel {
    margin-bottom: 20px;
    padding: 16px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    
    .status-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      
      .label {
        color: #606266;
        font-size: 14px;
      }
      
      .value {
        color: #303133;
        font-weight: 600;
        font-size: 14px;
      }
    }
  }
  
  .channels-container {
    margin-bottom: 20px;
    
    .channel-card {
      background: white;
      border-radius: 8px;
      padding: 16px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      transition: all 0.3s ease;
      border: 2px solid transparent;
      
      &.channel-disabled {
        opacity: 0.6;
        background: #f5f5f5;
      }
      
      &.channel-alarm {
        border-color: #f56c6c;
        box-shadow: 0 0 10px rgba(245, 108, 108, 0.3);
      }
      
      .channel-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 16px;
        
        .channel-info {
          display: flex;
          align-items: center;
          gap: 8px;
          
          .channel-name {
            font-weight: 600;
            color: #303133;
          }
        }
        
        .channel-controls {
          display: flex;
          align-items: center;
          gap: 8px;
        }
      }
      
      .temperature-display {
        margin-bottom: 16px;
        display: flex;
        justify-content: center;
      }
      
      .channel-stats {
        .stat-row {
          display: flex;
          justify-content: space-between;
          align-items: center;
          margin-bottom: 8px;
          
          &:last-child {
            margin-bottom: 0;
          }
          
          .stat-label {
            color: #909399;
            font-size: 12px;
          }
          
          .stat-value {
            color: #303133;
            font-size: 12px;
            font-weight: 500;
            
            &.trend-rising {
              color: #f56c6c;
            }
            
            &.trend-falling {
              color: #409eff;
            }
            
            &.trend-stable {
              color: #67c23a;
            }
          }
        }
      }
      
      .alarm-status {
        margin-top: 12px;
      }
    }
  }
  
  .trend-chart-container {
    margin-bottom: 20px;
    
    .chart-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      
      .chart-controls {
        display: flex;
        align-items: center;
        gap: 12px;
      }
    }
  }
  
  .channel-config-list {
    .channel-config-item {
      margin-bottom: 16px;
      
      &:last-child {
        margin-bottom: 0;
      }
      
      .channel-config-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        font-weight: 600;
      }
    }
  }
}

@media (max-width: 1200px) {
  .temperature-acquisition-card {
    .channels-container {
      .channel-card {
        margin-bottom: 16px;
      }
    }
  }
}

@media (max-width: 768px) {
  .temperature-acquisition-card {
    padding: 12px;
    
    .card-header {
      flex-direction: column;
      gap: 12px;
      align-items: stretch;
      
      .header-left {
        justify-content: center;
      }
    }
    
    .global-status-panel {
      .status-item {
        flex-direction: column;
        gap: 4px;
        text-align: center;
      }
    }
    
    .channels-container {
      .channel-card {
        .channel-header {
          flex-direction: column;
          gap: 12px;
          align-items: stretch;
        }
        
        .temperature-display {
          :deep(.digital-display) {
            transform: scale(0.8);
          }
        }
      }
    }
  }
}
</style>
