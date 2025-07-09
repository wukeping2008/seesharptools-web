<template>
  <div class="indicators-example example-page">
    <div class="page-header">
      <h1>指示控件示例</h1>
      <p class="description">
        展示各种专业指示控件的功能，包括LED指示器、数码管、状态灯等工业级指示控件。
      </p>
    </div>

    <!-- LED指示器示例 -->
    <div class="example-section">
      <h2 class="section-title">LED指示器</h2>
      <el-row :gutter="20">
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>基础LED指示器</span>
                <el-button size="small" @click="toggleBasicLeds">
                  <el-icon><Switch /></el-icon>
                  切换状态
                </el-button>
              </div>
            </template>
            <LEDIndicator
              :data="basicLedData"
              :options="basicLedOptions"
              :height="150"
              @state-change="handleBasicLedStateChange"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>状态指示灯</span>
                <el-button size="small" @click="toggleStatusLeds">
                  <el-icon><VideoPlay v-if="!statusLedsStreaming" /><VideoPause v-else /></el-icon>
                  {{ statusLedsStreaming ? '停止' : '开始' }}
                </el-button>
              </div>
            </template>
            <LEDIndicator
              :data="statusLedData"
              :options="statusLedOptions"
              :height="150"
              @state-change="handleStatusLedStateChange"
              @blink-toggle="handleStatusLedBlink"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>可配置LED</span>
                <el-button size="small" @click="changeConfigurableLedLayout">
                  <el-icon><Refresh /></el-icon>
                  切换布局
                </el-button>
              </div>
            </template>
            <LEDIndicator
              :data="configurableLedData"
              :options="configurableLedOptions"
              :height="200"
              :show-controls="true"
              @led-click="handleConfigurableLedClick"
            />
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 数码管显示示例 -->
    <div class="example-section">
      <h2 class="section-title">数码管显示</h2>
      <el-row :gutter="20">
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>基础数码管</span>
                <el-button size="small" @click="updateBasicDigital">
                  <el-icon><Refresh /></el-icon>
                  更新数值
                </el-button>
              </div>
            </template>
            <DigitalDisplay
              :data="basicDigitalData"
              :options="basicDigitalOptions"
              @value-change="handleBasicDigitalChange"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>温度显示</span>
                <el-button size="small" @click="simulateTemperature">
                  <el-icon><Refresh /></el-icon>
                  模拟温度
                </el-button>
              </div>
            </template>
            <DigitalDisplay
              :data="temperatureDigitalData"
              :options="temperatureDigitalOptions"
              @value-change="handleTemperatureChange"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>可控制数码管</span>
                <el-button size="small" @click="toggleDigitalBlinking">
                  <el-icon><Bell /></el-icon>
                  {{ digitalBlinking ? '停止闪烁' : '开始闪烁' }}
                </el-button>
              </div>
            </template>
            <DigitalDisplay
              :data="controllableDigitalData"
              :options="controllableDigitalOptions"
              @value-change="handleControllableDigitalChange"
            />
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 交通灯示例 -->
    <div class="example-section">
      <h2 class="section-title">交通灯系统</h2>
      <el-row :gutter="20">
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>标准交通灯</span>
                <el-button size="small" @click="toggleTrafficLight">
                  <el-icon><VideoPlay v-if="!trafficLightRunning" /><VideoPause v-else /></el-icon>
                  {{ trafficLightRunning ? '停止' : '启动' }}
                </el-button>
              </div>
            </template>
            <LEDIndicator
              :data="trafficLightData"
              :options="trafficLightOptions"
              :height="300"
              @state-change="handleTrafficLightChange"
            />
            <div class="traffic-status">
              <span>当前状态: {{ currentTrafficState }}</span>
              <span>剩余时间: {{ trafficCountdown }}秒</span>
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>设备状态面板</span>
                <el-button size="small" @click="simulateDeviceStatus">
                  <el-icon><Refresh /></el-icon>
                  模拟状态
                </el-button>
              </div>
            </template>
            <LEDIndicator
              :data="deviceStatusData"
              :options="deviceStatusOptions"
              :height="200"
              @state-change="handleDeviceStatusChange"
            />
            <div class="device-status-info">
              <div v-for="(status, index) in deviceStatuses" :key="index" class="status-item">
                <span class="status-label">{{ status.label }}:</span>
                <el-tag :type="status.type" size="small">{{ status.text }}</el-tag>
              </div>
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>报警指示系统</span>
                <el-button size="small" @click="triggerAlarm">
                  <el-icon><Bell /></el-icon>
                  触发报警
                </el-button>
              </div>
            </template>
            <LEDIndicator
              :data="alarmLedData"
              :options="alarmLedOptions"
              :height="200"
              @state-change="handleAlarmLedChange"
            />
            <div class="alarm-info">
              <div class="alarm-level">
                <span>报警级别: </span>
                <el-tag :type="currentAlarmLevel.type" size="small">
                  {{ currentAlarmLevel.text }}
                </el-tag>
              </div>
              <div class="alarm-count">
                <span>活跃报警: {{ activeAlarmsCount }}</span>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 圆形LED阵列 -->
    <div class="example-section">
      <h2 class="section-title">LED阵列展示</h2>
      <el-row :gutter="20">
        <el-col :span="12">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>圆形LED阵列</span>
                <div class="header-controls">
                  <el-button size="small" @click="startCircularAnimation">
                    <el-icon><VideoPlay /></el-icon>
                    动画
                  </el-button>
                  <el-button size="small" @click="randomizeCircularLeds">
                    <el-icon><Refresh /></el-icon>
                    随机
                  </el-button>
                </div>
              </div>
            </template>
            <LEDIndicator
              :data="circularLedData"
              :options="circularLedOptions"
              :height="350"
              @led-click="handleCircularLedClick"
            />
          </el-card>
        </el-col>
        
        <el-col :span="12">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>网格LED阵列</span>
                <div class="header-controls">
                  <el-button size="small" @click="startGridAnimation">
                    <el-icon><VideoPlay /></el-icon>
                    波浪动画
                  </el-button>
                  <el-button size="small" @click="clearGridLeds">
                    <el-icon><Delete /></el-icon>
                    清除
                  </el-button>
                </div>
              </div>
            </template>
            <LEDIndicator
              :data="gridLedData"
              :options="gridLedOptions"
              :height="350"
              @led-click="handleGridLedClick"
            />
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- LED控制面板 -->
    <div class="example-section">
      <h2 class="section-title">LED控制面板</h2>
      <el-card>
        <template #header>
          <div class="card-header">
            <span>综合LED控制系统</span>
            <div class="header-controls">
              <el-button size="small" @click="resetAllLeds">
                <el-icon><Refresh /></el-icon>
                重置全部
              </el-button>
              <el-button size="small" @click="exportLedConfig">
                <el-icon><Download /></el-icon>
                导出配置
              </el-button>
            </div>
          </div>
        </template>
        
        <LEDIndicator
          :data="controlPanelLedData"
          :options="controlPanelLedOptions"
          :height="250"
          :show-controls="true"
          @all-state-change="handleControlPanelAllStateChange"
          @led-click="handleControlPanelLedClick"
        />
      </el-card>
    </div>

    <!-- 状态统计 -->
    <div class="example-section" v-if="showStatistics">
      <h2 class="section-title">LED状态统计</h2>
      <el-card>
        <template #header>
          <div class="card-header">
            <span>系统状态概览</span>
            <el-button size="small" @click="refreshStatistics">
              <el-icon><Refresh /></el-icon>
              刷新统计
            </el-button>
          </div>
        </template>
        
        <el-row :gutter="20">
          <el-col :span="6">
            <div class="stat-card">
              <div class="stat-value">{{ totalLedsCount }}</div>
              <div class="stat-label">LED总数</div>
            </div>
          </el-col>
          <el-col :span="6">
            <div class="stat-card">
              <div class="stat-value">{{ activeLedsCount }}</div>
              <div class="stat-label">激活LED</div>
            </div>
          </el-col>
          <el-col :span="6">
            <div class="stat-card">
              <div class="stat-value">{{ blinkingLedsCount }}</div>
              <div class="stat-label">闪烁LED</div>
            </div>
          </el-col>
          <el-col :span="6">
            <div class="stat-card">
              <div class="stat-value">{{ ((activeLedsCount / totalLedsCount) * 100).toFixed(1) }}%</div>
              <div class="stat-label">激活率</div>
            </div>
          </el-col>
        </el-row>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { Refresh, VideoPlay, VideoPause, Switch, Bell, Delete, Download } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import LEDIndicator from '@/components/professional/indicators/LEDIndicator.vue'
import DigitalDisplay from '@/components/professional/indicators/DigitalDisplay.vue'
import type { GaugeData } from '@/types/gauge'

// 基础LED数据
const basicLedData = ref<GaugeData>({
  value: 0,
  min: 0,
  max: 8,
  unit: '',
  title: '基础LED指示器'
})

const basicLedOptions = ref({
  ledCount: 8,
  ledSize: 25,
  layout: 'horizontal' as const,
  ledShape: 'circle' as const,
  showGlow: true,
  showReflection: true,
  showLabels: true,
  showControlPanel: false,
  showStatusDisplay: true,
  clickable: true,
  defaultColor: '#00ff00',
  offColor: '#333333',
  backgroundColor: 'transparent',
  blinkSpeed: 500,
  animation: true
})

// 状态LED数据
const statusLedData = ref<GaugeData>({
  value: 0,
  min: 0,
  max: 4,
  unit: '',
  title: '状态指示灯'
})

const statusLedOptions = ref({
  ledCount: 4,
  ledSize: 30,
  layout: 'horizontal' as const,
  ledShape: 'circle' as const,
  showGlow: true,
  showReflection: true,
  showLabels: true,
  showControlPanel: false,
  showStatusDisplay: true,
  clickable: false,
  defaultColor: '#00ff00',
  offColor: '#333333',
  backgroundColor: 'transparent',
  blinkSpeed: 800,
  animation: true
})

// 可配置LED数据
const configurableLedData = ref<GaugeData>({
  value: 0,
  min: 0,
  max: 6,
  unit: '',
  title: '可配置LED'
})

const configurableLedOptions = ref({
  ledCount: 6,
  ledSize: 35,
  layout: 'grid' as const,
  ledShape: 'circle' as const,
  showGlow: true,
  showReflection: true,
  showLabels: true,
  showControlPanel: true,
  showStatusDisplay: true,
  clickable: true,
  defaultColor: '#409eff',
  offColor: '#333333',
  backgroundColor: 'transparent',
  blinkSpeed: 600,
  animation: true
})

// 交通灯数据
const trafficLightData = ref<GaugeData>({
  value: 0,
  min: 0,
  max: 3,
  unit: '',
  title: '交通灯'
})

const trafficLightOptions = ref({
  ledCount: 3,
  ledSize: 50,
  layout: 'vertical' as const,
  ledShape: 'circle' as const,
  showGlow: true,
  showReflection: true,
  showLabels: true,
  showControlPanel: false,
  showStatusDisplay: false,
  clickable: false,
  defaultColor: '#ff0000',
  offColor: '#333333',
  backgroundColor: 'transparent',
  blinkSpeed: 500,
  animation: true
})

// 设备状态数据
const deviceStatusData = ref<GaugeData>({
  value: 0,
  min: 0,
  max: 5,
  unit: '',
  title: '设备状态'
})

const deviceStatusOptions = ref({
  ledCount: 5,
  ledSize: 25,
  layout: 'horizontal' as const,
  ledShape: 'circle' as const,
  showGlow: true,
  showReflection: true,
  showLabels: true,
  showControlPanel: false,
  showStatusDisplay: false,
  clickable: false,
  defaultColor: '#00ff00',
  offColor: '#333333',
  backgroundColor: 'transparent',
  blinkSpeed: 1000,
  animation: true
})

// 报警LED数据
const alarmLedData = ref<GaugeData>({
  value: 0,
  min: 0,
  max: 4,
  unit: '',
  title: '报警指示'
})

const alarmLedOptions = ref({
  ledCount: 4,
  ledSize: 30,
  layout: 'grid' as const,
  ledShape: 'circle' as const,
  showGlow: true,
  showReflection: true,
  showLabels: true,
  showControlPanel: false,
  showStatusDisplay: false,
  clickable: false,
  defaultColor: '#ff0000',
  offColor: '#333333',
  backgroundColor: 'transparent',
  blinkSpeed: 300,
  animation: true
})

// 圆形LED数据
const circularLedData = ref<GaugeData>({
  value: 0,
  min: 0,
  max: 12,
  unit: '',
  title: '圆形LED阵列'
})

const circularLedOptions = ref({
  ledCount: 12,
  ledSize: 20,
  layout: 'circular' as const,
  ledShape: 'circle' as const,
  showGlow: true,
  showReflection: true,
  showLabels: false,
  showControlPanel: false,
  showStatusDisplay: false,
  clickable: true,
  defaultColor: '#00ffff',
  offColor: '#333333',
  backgroundColor: 'transparent',
  blinkSpeed: 400,
  animation: true
})

// 网格LED数据
const gridLedData = ref<GaugeData>({
  value: 0,
  min: 0,
  max: 16,
  unit: '',
  title: '网格LED阵列'
})

const gridLedOptions = ref({
  ledCount: 16,
  ledSize: 18,
  layout: 'grid' as const,
  ledShape: 'square' as const,
  showGlow: true,
  showReflection: true,
  showLabels: false,
  showControlPanel: false,
  showStatusDisplay: false,
  clickable: true,
  defaultColor: '#ff00ff',
  offColor: '#333333',
  backgroundColor: 'transparent',
  blinkSpeed: 200,
  animation: true
})

// 控制面板LED数据
const controlPanelLedData = ref<GaugeData>({
  value: 0,
  min: 0,
  max: 10,
  unit: '',
  title: 'LED控制面板'
})

const controlPanelLedOptions = ref({
  ledCount: 10,
  ledSize: 30,
  layout: 'horizontal' as const,
  ledShape: 'circle' as const,
  showGlow: true,
  showReflection: true,
  showLabels: true,
  showControlPanel: true,
  showStatusDisplay: true,
  clickable: true,
  defaultColor: '#67c23a',
  offColor: '#333333',
  backgroundColor: 'transparent',
  blinkSpeed: 500,
  animation: true
})

// 数码管显示数据
const basicDigitalData = ref({
  value: 123.4,
  title: '基础数码管',
  unit: ''
})

const basicDigitalOptions = ref({
  digitCount: 4,
  decimalPlaces: 1,
  size: 60,
  color: '#00ff00',
  backgroundColor: '#000000',
  segmentColor: '#003300',
  activeColor: '#00ff00',
  showControls: false,
  blinkInterval: 500,
  padding: 20,
  borderRadius: 8
})

const temperatureDigitalData = ref({
  value: 25.6,
  title: '温度显示',
  unit: '°C'
})

const temperatureDigitalOptions = ref({
  digitCount: 4,
  decimalPlaces: 1,
  size: 50,
  color: '#ff6600',
  backgroundColor: '#000000',
  segmentColor: '#330000',
  activeColor: '#ff6600',
  showControls: false,
  blinkInterval: 500,
  padding: 20,
  borderRadius: 8
})

const controllableDigitalData = ref({
  value: 888.8,
  title: '可控制数码管',
  unit: ''
})

const controllableDigitalOptions = ref({
  digitCount: 4,
  decimalPlaces: 1,
  size: 55,
  color: '#0099ff',
  backgroundColor: '#000000',
  segmentColor: '#003366',
  activeColor: '#0099ff',
  showControls: true,
  blinkInterval: 300,
  padding: 20,
  borderRadius: 8
})

// 状态控制
const statusLedsStreaming = ref(false)
const trafficLightRunning = ref(false)
const showStatistics = ref(true)
const digitalBlinking = ref(false)

// 交通灯状态
const currentTrafficState = ref('红灯')
const trafficCountdown = ref(30)
const trafficStates = ['红灯', '绿灯', '黄灯']
const trafficDurations = [30, 25, 5] // 秒
let currentTrafficIndex = 0

// 设备状态
const deviceStatuses = ref([
  { label: '电源', type: 'success', text: '正常' },
  { label: '网络', type: 'success', text: '连接' },
  { label: '存储', type: 'warning', text: '警告' },
  { label: '温度', type: 'success', text: '正常' },
  { label: '风扇', type: 'danger', text: '故障' }
])

// 报警状态
const currentAlarmLevel = ref({ type: 'success', text: '正常' })
const activeAlarmsCount = ref(0)

// 定时器
let statusLedsTimer: number | null = null
let trafficLightTimer: number | null = null
let circularAnimationTimer: number | null = null
let gridAnimationTimer: number | null = null

// 计算属性
const totalLedsCount = computed(() => {
  return basicLedOptions.value.ledCount + 
         statusLedOptions.value.ledCount + 
         configurableLedOptions.value.ledCount +
         trafficLightOptions.value.ledCount +
         deviceStatusOptions.value.ledCount +
         alarmLedOptions.value.ledCount +
         circularLedOptions.value.ledCount +
         gridLedOptions.value.ledCount +
         controlPanelLedOptions.value.ledCount
})

const activeLedsCount = ref(0)
const blinkingLedsCount = ref(0)

// 方法
const toggleBasicLeds = () => {
  // 随机切换基础LED状态
  for (let i = 0; i < basicLedOptions.value.ledCount; i++) {
    if (Math.random() > 0.5) {
      basicLedData.value.value = i
      break
    }
  }
}

const toggleStatusLeds = () => {
  if (statusLedsStreaming.value) {
    statusLedsStreaming.value = false
    if (statusLedsTimer) {
      clearInterval(statusLedsTimer)
      statusLedsTimer = null
    }
  } else {
    statusLedsStreaming.value = true
    statusLedsTimer = setInterval(() => {
      statusLedData.value.value = Math.floor(Math.random() * statusLedOptions.value.ledCount)
    }, 1500)
  }
}

const changeConfigurableLedLayout = () => {
  const layouts = ['horizontal', 'vertical', 'grid', 'circular']
  const currentIndex = layouts.indexOf(configurableLedOptions.value.layout)
  configurableLedOptions.value.layout = layouts[(currentIndex + 1) % layouts.length] as any
  ElMessage.info(`布局已切换为: ${configurableLedOptions.value.layout}`)
}

const toggleTrafficLight = () => {
  if (trafficLightRunning.value) {
    trafficLightRunning.value = false
    if (trafficLightTimer) {
      clearInterval(trafficLightTimer)
      trafficLightTimer = null
    }
  } else {
    trafficLightRunning.value = true
    currentTrafficIndex = 0
    updateTrafficLight()
    trafficLightTimer = setInterval(() => {
      trafficCountdown.value--
      if (trafficCountdown.value <= 0) {
        currentTrafficIndex = (currentTrafficIndex + 1) % trafficStates.length
        updateTrafficLight()
      }
    }, 1000)
  }
}

const updateTrafficLight = () => {
  currentTrafficState.value = trafficStates[currentTrafficIndex]
  trafficCountdown.value = trafficDurations[currentTrafficIndex]
  trafficLightData.value.value = currentTrafficIndex
}

const simulateDeviceStatus = () => {
  deviceStatuses.value.forEach(status => {
    const rand = Math.random()
    if (rand < 0.7) {
      status.type = 'success'
      status.text = '正常'
    } else if (rand < 0.9) {
      status.type = 'warning'
      status.text = '警告'
    } else {
      status.type = 'danger'
      status.text = '故障'
    }
  })
  
  // 更新设备状态LED
  const faultCount = deviceStatuses.value.filter(s => s.type === 'danger').length
  deviceStatusData.value.value = faultCount
}

const triggerAlarm = () => {
  const alarmLevels = [
    { type: 'success', text: '正常' },
    { type: 'warning', text: '警告' },
    { type: 'danger', text: '严重' }
  ]
  
  const level = alarmLevels[Math.floor(Math.random() * alarmLevels.length)]
  currentAlarmLevel.value = level
  activeAlarmsCount.value = Math.floor(Math.random() * 5)
  alarmLedData.value.value = activeAlarmsCount.value
}

const startCircularAnimation = () => {
  let currentIndex = 0
  circularAnimationTimer = setInterval(() => {
    circularLedData.value.value = currentIndex
    currentIndex = (currentIndex + 1) % circularLedOptions.value.ledCount
  }, 200)
  
  setTimeout(() => {
    if (circularAnimationTimer) {
      clearInterval(circularAnimationTimer)
      circularAnimationTimer = null
    }
  }, 5000)
}

const randomizeCircularLeds = () => {
  circularLedData.value.value = Math.floor(Math.random() * circularLedOptions.value.ledCount)
}

const startGridAnimation = () => {
  let wave = 0
  gridAnimationTimer = setInterval(() => {
    gridLedData.value.value = wave % gridLedOptions.value.ledCount
    wave++
  }, 150)
  
  setTimeout(() => {
    if (gridAnimationTimer) {
      clearInterval(gridAnimationTimer)
      gridAnimationTimer = null
    }
  }, 4000)
}

const clearGridLeds = () => {
  gridLedData.value.value = 0
}

const resetAllLeds = () => {
  basicLedData.value.value = 0
  statusLedData.value.value = 0
  configurableLedData.value.value = 0
  trafficLightData.value.value = 0
  deviceStatusData.value.value = 0
  alarmLedData.value.value = 0
  circularLedData.value.value = 0
  gridLedData.value.value = 0
  controlPanelLedData.value.value = 0
  
  ElMessage.success('所有LED已重置')
}

const exportLedConfig = () => {
  const config = {
    basic: { data: basicLedData.value, options: basicLedOptions.value },
    status: { data: statusLedData.value, options: statusLedOptions.value },
    configurable: { data: configurableLedData.value, options: configurableLedOptions.value },
    traffic: { data: trafficLightData.value, options: trafficLightOptions.value },
    device: { data: deviceStatusData.value, options: deviceStatusOptions.value },
    alarm: { data: alarmLedData.value, options: alarmLedOptions.value },
    circular: { data: circularLedData.value, options: circularLedOptions.value },
    grid: { data: gridLedData.value, options: gridLedOptions.value },
    controlPanel: { data: controlPanelLedData.value, options: controlPanelLedOptions.value },
    timestamp: new Date()
  }
  
  const blob = new Blob([JSON.stringify(config, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `led_indicators_config_${Date.now()}.json`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

const refreshStatistics = () => {
  // 更新统计数据
  activeLedsCount.value = Math.floor(Math.random() * totalLedsCount.value)
  blinkingLedsCount.value = Math.floor(Math.random() * 10)
}

// 数码管相关方法
const updateBasicDigital = () => {
  basicDigitalData.value.value = Math.random() * 999.9
  ElMessage.info(`基础数码管更新为: ${basicDigitalData.value.value.toFixed(1)}`)
}

const simulateTemperature = () => {
  temperatureDigitalData.value.value = 15 + Math.random() * 40 // 15-55度
  ElMessage.info(`温度更新为: ${temperatureDigitalData.value.value.toFixed(1)}°C`)
}

const toggleDigitalBlinking = () => {
  digitalBlinking.value = !digitalBlinking.value
  controllableDigitalOptions.value.blinkInterval = digitalBlinking.value ? 300 : 0
  ElMessage.info(`数码管闪烁${digitalBlinking.value ? '开启' : '关闭'}`)
}

const handleBasicDigitalChange = (value: number) => {
  ElMessage.info(`基础数码管值变更为: ${value}`)
}

const handleTemperatureChange = (value: number) => {
  ElMessage.info(`温度值变更为: ${value}°C`)
}

const handleControllableDigitalChange = (value: number) => {
  ElMessage.info(`可控制数码管值变更为: ${value}`)
}

// 事件处理
const handleBasicLedStateChange = (index: number, state: boolean) => {
  ElMessage.info(`基础LED ${index + 1} ${state ? '开启' : '关闭'}`)
}

const handleStatusLedStateChange = (index: number, state: boolean) => {
  ElMessage.info(`状态LED ${index + 1} ${state ? '激活' : '关闭'}`)
}

const handleStatusLedBlink = (blinking: boolean) => {
  ElMessage.info(`状态LED ${blinking ? '开始' : '停止'}闪烁`)
}

const handleConfigurableLedClick = (index: number, led: any) => {
  ElMessage.info(`点击了可配置LED ${index + 1}`)
}

const handleTrafficLightChange = (index: number, state: boolean) => {
  // 交通灯状态变化处理
}

const handleDeviceStatusChange = (index: number, state: boolean) => {
  // 设备状态变化处理
}

const handleAlarmLedChange = (index: number, state: boolean) => {
  ElMessage.warning(`报警LED ${index + 1} ${state ? '触发' : '解除'}`)
}

const handleCircularLedClick = (index: number, led: any) => {
  ElMessage.info(`点击了圆形LED ${index + 1}`)
}

const handleGridLedClick = (index: number, led: any) => {
  ElMessage.info(`点击了网格LED ${index + 1}`)
}

const handleControlPanelAllStateChange = (allOn: boolean) => {
  ElMessage.info(`控制面板LED ${allOn ? '全部开启' : '全部关闭'}`)
}

const handleControlPanelLedClick = (index: number, led: any) => {
  ElMessage.info(`控制面板LED ${index + 1} 被点击`)
}

// 生命周期
onMounted(() => {
  refreshStatistics()
})

onUnmounted(() => {
  if (statusLedsTimer) clearInterval(statusLedsTimer)
  if (trafficLightTimer) clearInterval(trafficLightTimer)
  if (circularAnimationTimer) clearInterval(circularAnimationTimer)
  if (gridAnimationTimer) clearInterval(gridAnimationTimer)
})
</script>

<style lang="scss" scoped>
.indicators-example {
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    
    .header-controls {
      display: flex;
      align-items: center;
      gap: 8px;
    }
  }
  
  .traffic-status {
    margin-top: 16px;
    padding: 12px;
    background: #f5f7fa;
    border-radius: 4px;
    display: flex;
    justify-content: space-between;
    font-size: 14px;
    color: #606266;
  }
  
  .device-status-info {
    margin-top: 16px;
    padding: 12px;
    background: #f5f7fa;
    border-radius: 4px;
    
    .status-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;
      
      &:last-child {
        margin-bottom: 0;
      }
      
      .status-label {
        font-size: 12px;
        color: #606266;
        font-weight: 500;
      }
    }
  }
  
  .alarm-info {
    margin-top: 16px;
    padding: 12px;
    background: #f5f7fa;
    border-radius: 4px;
    
    .alarm-level {
      display: flex;
      align-items: center;
      margin-bottom: 8px;
      font-size: 12px;
      color: #606266;
    }
    
    .alarm-count {
      font-size: 12px;
      color: #606266;
    }
  }
  
  .stat-card {
    text-align: center;
    padding: 20px;
    background: #f5f7fa;
    border-radius: 8px;
    border: 1px solid #e4e7ed;
    
    .stat-value {
      font-size: 24px;
      font-weight: bold;
      color: #409eff;
      margin-bottom: 8px;
    }
    
    .stat-label {
      font-size: 12px;
      color: #606266;
      font-weight: 500;
    }
  }
}

@media (max-width: 768px) {
  .indicators-example {
    .card-header {
      flex-direction: column;
      gap: 8px;
      align-items: flex-start;
      
      .header-controls {
        flex-wrap: wrap;
      }
    }
    
    .traffic-status {
      flex-direction: column;
      gap: 8px;
    }
    
    .stat-card {
      margin-bottom: 16px;
    }
  }
}
</style>
