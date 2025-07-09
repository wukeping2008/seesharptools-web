<template>
  <div class="digital-multimeter-wrapper professional-instrument">
    <!-- 数字万用表标题栏 -->
    <div class="dmm-header">
      <div class="header-left">
        <h3>数字万用表</h3>
        <div class="model-info">DMM-6500 Series</div>
      </div>
      <div class="header-right">
        <div class="status-indicators">
          <div class="status-item" :class="{ active: isConnected }">
            <div class="status-dot"></div>
            <span>CONN</span>
          </div>
          <div class="status-item" :class="{ active: isReady }">
            <div class="status-dot"></div>
            <span>RDY</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 主显示屏 -->
    <div class="main-display">
      <div class="display-screen">
        <div class="primary-reading">
          <div class="reading-value">{{ formatPrimaryReading() }}</div>
          <div class="reading-unit">{{ primaryUnit }}</div>
        </div>
        
        <div class="secondary-reading" v-if="showSecondaryReading">
          <div class="reading-value">{{ formatSecondaryReading() }}</div>
          <div class="reading-unit">{{ secondaryUnit }}</div>
        </div>
        
        <div class="display-indicators">
          <div class="indicator" :class="{ active: autoRange }">AUTO</div>
          <div class="indicator" :class="{ active: holdMode }">HOLD</div>
          <div class="indicator" :class="{ active: relativeMode }">REL</div>
          <div class="indicator" :class="{ active: minMaxMode }">MIN/MAX</div>
          <div class="indicator" :class="{ active: isOverload }">OL</div>
        </div>
      </div>
    </div>

    <!-- 功能选择面板 -->
    <div class="function-panel">
      <el-row :gutter="16">
        <el-col :span="12">
          <div class="function-section">
            <h4>测量功能</h4>
            <div class="function-buttons">
              <el-button 
                v-for="func in measurementFunctions" 
                :key="func.value"
                :type="currentFunction === func.value ? 'primary' : ''"
                @click="setFunction(func.value)"
                size="small"
                class="function-btn"
              >
                {{ func.label }}
              </el-button>
            </div>
          </div>
        </el-col>
        
        <el-col :span="12">
          <div class="function-section">
            <h4>量程设置</h4>
            <div class="range-controls">
              <el-checkbox v-model="autoRange" @change="updateRange">
                自动量程
              </el-checkbox>
              <div class="manual-range" v-if="!autoRange">
                <el-select v-model="currentRange" @change="updateRange" size="small">
                  <el-option 
                    v-for="range in availableRanges" 
                    :key="range.value"
                    :label="range.label" 
                    :value="range.value" 
                  />
                </el-select>
              </div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 测量控制面板 -->
    <div class="measurement-panel">
      <el-row :gutter="16">
        <el-col :span="8">
          <div class="control-section">
            <h4>测量控制</h4>
            <div class="measurement-controls">
              <el-button @click="startMeasurement" :disabled="isMeasuring" size="small">
                <el-icon><VideoPlay /></el-icon>
                开始测量
              </el-button>
              <el-button @click="stopMeasurement" :disabled="!isMeasuring" size="small">
                <el-icon><VideoPause /></el-icon>
                停止测量
              </el-button>
              <el-button @click="singleMeasurement" :disabled="isMeasuring" size="small">
                <el-icon><Position /></el-icon>
                单次测量
              </el-button>
            </div>
          </div>
        </el-col>
        
        <el-col :span="8">
          <div class="control-section">
            <h4>数据处理</h4>
            <div class="data-controls">
              <el-button @click="holdMode = !holdMode" :type="holdMode ? 'warning' : ''" size="small">
                <el-icon><Lock /></el-icon>
                数据保持
              </el-button>
              <el-button @click="toggleRelative" :type="relativeMode ? 'info' : ''" size="small">
                <el-icon><Minus /></el-icon>
                相对测量
              </el-button>
              <el-button @click="toggleMinMax" :type="minMaxMode ? 'success' : ''" size="small">
                <el-icon><TrendCharts /></el-icon>
                最值记录
              </el-button>
            </div>
          </div>
        </el-col>
        
        <el-col :span="8">
          <div class="control-section">
            <h4>设置选项</h4>
            <div class="setting-controls">
              <div class="control-row">
                <label>积分时间:</label>
                <el-select v-model="integrationTime" @change="updateSettings" size="small">
                  <el-option label="快速" value="fast" />
                  <el-option label="中等" value="medium" />
                  <el-option label="慢速" value="slow" />
                </el-select>
              </div>
              <div class="control-row">
                <label>触发模式:</label>
                <el-select v-model="triggerMode" @change="updateSettings" size="small">
                  <el-option label="立即" value="immediate" />
                  <el-option label="外部" value="external" />
                  <el-option label="定时" value="timer" />
                </el-select>
              </div>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- 统计信息面板 -->
    <div class="statistics-panel">
      <div class="panel-header">
        <h4>统计信息</h4>
        <div class="stats-controls">
          <el-button @click="resetStatistics" size="small">
            <el-icon><Refresh /></el-icon>
            重置统计
          </el-button>
          <el-button @click="exportData" size="small">
            <el-icon><Download /></el-icon>
            导出数据
          </el-button>
        </div>
      </div>
      
      <div class="statistics-grid">
        <div class="stat-item">
          <div class="stat-label">当前值</div>
          <div class="stat-value">{{ formatValue(statistics.current, primaryUnit) }}</div>
        </div>
        <div class="stat-item">
          <div class="stat-label">平均值</div>
          <div class="stat-value">{{ formatValue(statistics.average, primaryUnit) }}</div>
        </div>
        <div class="stat-item">
          <div class="stat-label">最大值</div>
          <div class="stat-value">{{ formatValue(statistics.maximum, primaryUnit) }}</div>
        </div>
        <div class="stat-item">
          <div class="stat-label">最小值</div>
          <div class="stat-value">{{ formatValue(statistics.minimum, primaryUnit) }}</div>
        </div>
        <div class="stat-item">
          <div class="stat-label">标准差</div>
          <div class="stat-value">{{ formatValue(statistics.stdDev, primaryUnit) }}</div>
        </div>
        <div class="stat-item">
          <div class="stat-label">测量次数</div>
          <div class="stat-value">{{ statistics.count }}</div>
        </div>
      </div>
    </div>

    <!-- 历史数据图表 -->
    <div class="chart-panel" v-if="showChart">
      <div class="panel-header">
        <h4>测量趋势</h4>
        <div class="chart-controls">
          <el-button @click="clearHistory" size="small">
            <el-icon><Delete /></el-icon>
            清除历史
          </el-button>
          <el-button @click="showChart = !showChart" size="small">
            <el-icon><View /></el-icon>
            {{ showChart ? '隐藏' : '显示' }}图表
          </el-button>
        </div>
      </div>
      
      <div class="chart-container">
        <WaveformChart
          :data="chartData"
          :series-configs="chartSeriesConfigs"
          :options="chartOptions"
          :show-toolbar="false"
          :show-controls="false"
          :show-status="false"
          :show-measurements="false"
          :show-cursors="false"
          :height="200"
        />
      </div>
    </div>

    <!-- 状态栏 -->
    <div class="status-bar">
      <div class="status-left">
        <span>功能: {{ getFunctionName(currentFunction) }}</span>
        <span>量程: {{ autoRange ? '自动' : getCurrentRangeName() }}</span>
        <span>积分: {{ getIntegrationName(integrationTime) }}</span>
      </div>
      <div class="status-right">
        <span>测量率: {{ measurementRate }}次/秒</span>
        <span>时间: {{ currentTime }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, watch } from 'vue'
import { 
  VideoPlay, VideoPause, Position, Lock, Minus, TrendCharts,
  Refresh, Download, Delete, View
} from '@element-plus/icons-vue'
import WaveformChart from '@/components/charts/WaveformChart.vue'
import type { WaveformData, WaveformOptions, WaveformSeriesConfig } from '@/types/chart'

// Props
interface Props {
  width?: string | number
  height?: string | number
}

const props = withDefaults(defineProps<Props>(), {
  width: '100%',
  height: '800px'
})

// 响应式数据
const isConnected = ref(true)
const isReady = ref(true)
const isMeasuring = ref(false)
const showChart = ref(true)
const currentTime = ref('')

// 测量状态
const currentFunction = ref('dcv') // DC电压
const autoRange = ref(true)
const currentRange = ref('auto')
const holdMode = ref(false)
const relativeMode = ref(false)
const minMaxMode = ref(false)
const isOverload = ref(false)

// 显示数据
const primaryReading = ref(0)
const secondaryReading = ref(0)
const primaryUnit = ref('V')
const secondaryUnit = ref('Hz')
const showSecondaryReading = ref(false)

// 设置参数
const integrationTime = ref('medium')
const triggerMode = ref('immediate')
const measurementRate = ref(10)

// 相对测量基准值
const relativeReference = ref(0)

// 测量功能选项
const measurementFunctions = [
  { label: 'DC电压', value: 'dcv' },
  { label: 'AC电压', value: 'acv' },
  { label: 'DC电流', value: 'dci' },
  { label: 'AC电流', value: 'aci' },
  { label: '电阻', value: 'res' },
  { label: '电容', value: 'cap' },
  { label: '频率', value: 'freq' },
  { label: '温度', value: 'temp' }
]

// 统计数据
const statistics = ref({
  current: 0,
  average: 0,
  maximum: -Infinity,
  minimum: Infinity,
  stdDev: 0,
  count: 0,
  sum: 0,
  sumSquares: 0
})

// 历史数据
const measurementHistory = ref<Array<{ time: number, value: number }>>([])

// 定时器
let measurementTimer: number | null = null
let timeTimer: number | null = null

// 计算属性
const availableRanges = computed(() => {
  const ranges: Record<string, Array<{ label: string, value: string }>> = {
    dcv: [
      { label: '自动', value: 'auto' },
      { label: '200mV', value: '0.2' },
      { label: '2V', value: '2' },
      { label: '20V', value: '20' },
      { label: '200V', value: '200' },
      { label: '1000V', value: '1000' }
    ],
    acv: [
      { label: '自动', value: 'auto' },
      { label: '200mV', value: '0.2' },
      { label: '2V', value: '2' },
      { label: '20V', value: '20' },
      { label: '200V', value: '200' },
      { label: '750V', value: '750' }
    ],
    dci: [
      { label: '自动', value: 'auto' },
      { label: '200μA', value: '0.0002' },
      { label: '2mA', value: '0.002' },
      { label: '20mA', value: '0.02' },
      { label: '200mA', value: '0.2' },
      { label: '2A', value: '2' },
      { label: '10A', value: '10' }
    ],
    aci: [
      { label: '自动', value: 'auto' },
      { label: '200μA', value: '0.0002' },
      { label: '2mA', value: '0.002' },
      { label: '20mA', value: '0.02' },
      { label: '200mA', value: '0.2' },
      { label: '2A', value: '2' },
      { label: '10A', value: '10' }
    ],
    res: [
      { label: '自动', value: 'auto' },
      { label: '200Ω', value: '200' },
      { label: '2kΩ', value: '2000' },
      { label: '20kΩ', value: '20000' },
      { label: '200kΩ', value: '200000' },
      { label: '2MΩ', value: '2000000' },
      { label: '20MΩ', value: '20000000' }
    ],
    cap: [
      { label: '自动', value: 'auto' },
      { label: '2nF', value: '2e-9' },
      { label: '20nF', value: '20e-9' },
      { label: '200nF', value: '200e-9' },
      { label: '2μF', value: '2e-6' },
      { label: '20μF', value: '20e-6' },
      { label: '200μF', value: '200e-6' }
    ],
    freq: [
      { label: '自动', value: 'auto' }
    ],
    temp: [
      { label: '自动', value: 'auto' }
    ]
  }
  return ranges[currentFunction.value] || []
})

const chartSeriesConfigs = computed<WaveformSeriesConfig[]>(() => [
  {
    name: '测量值',
    color: '#409eff',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'circle',
    markerSize: 4,
    visible: true,
    yOffset: 0,
    yScale: 1,
    coupling: 'DC',
    bandwidth: 100000000,
    probe: 1
  }
])

const chartOptions = computed<WaveformOptions>(() => ({
  autoScale: true,
  logarithmic: false,
  splitView: false,
  legendVisible: false,
  cursorMode: 'zoom',
  gridEnabled: true,
  minorGridEnabled: false,
  theme: 'light',
  realTime: isMeasuring.value,
  bufferSize: 1000,
  triggerMode: 'auto',
  triggerLevel: 0,
  triggerChannel: 0
}))

const chartData = computed<WaveformData>(() => {
  const values = measurementHistory.value.map(item => item.value)
  const times = measurementHistory.value.map(item => item.time)
  
  return {
    series: values,
    xStart: times[0] || 0,
    xInterval: times.length > 1 ? (times[times.length - 1] - times[0]) / (times.length - 1) : 1,
    sampleRate: measurementRate.value
  }
})

// 格式化函数
const formatValue = (value: number, unit: string) => {
  if (isOverload.value) return 'OL'
  if (Math.abs(value) === Infinity) return '---'
  
  // 根据单位和数值大小选择合适的显示格式
  switch (unit) {
    case 'V':
      if (Math.abs(value) >= 1000) return `${(value / 1000).toFixed(3)}kV`
      if (Math.abs(value) >= 1) return `${value.toFixed(4)}V`
      if (Math.abs(value) >= 0.001) return `${(value * 1000).toFixed(1)}mV`
      return `${(value * 1000000).toFixed(0)}μV`
    
    case 'A':
      if (Math.abs(value) >= 1) return `${value.toFixed(4)}A`
      if (Math.abs(value) >= 0.001) return `${(value * 1000).toFixed(1)}mA`
      return `${(value * 1000000).toFixed(0)}μA`
    
    case 'Ω':
      if (Math.abs(value) >= 1000000) return `${(value / 1000000).toFixed(3)}MΩ`
      if (Math.abs(value) >= 1000) return `${(value / 1000).toFixed(1)}kΩ`
      return `${value.toFixed(1)}Ω`
    
    case 'F':
      if (Math.abs(value) >= 0.001) return `${(value * 1000).toFixed(3)}mF`
      if (Math.abs(value) >= 0.000001) return `${(value * 1000000).toFixed(1)}μF`
      if (Math.abs(value) >= 0.000000001) return `${(value * 1000000000).toFixed(0)}nF`
      return `${(value * 1000000000000).toFixed(0)}pF`
    
    case 'Hz':
      if (Math.abs(value) >= 1000000) return `${(value / 1000000).toFixed(3)}MHz`
      if (Math.abs(value) >= 1000) return `${(value / 1000).toFixed(1)}kHz`
      return `${value.toFixed(1)}Hz`
    
    case '°C':
      return `${value.toFixed(1)}°C`
    
    default:
      return value.toFixed(4)
  }
}

const formatPrimaryReading = () => {
  let value = primaryReading.value
  if (relativeMode.value) {
    value = value - relativeReference.value
  }
  return formatValue(value, primaryUnit.value)
}

const formatSecondaryReading = () => {
  return formatValue(secondaryReading.value, secondaryUnit.value)
}

const getFunctionName = (func: string) => {
  const names: Record<string, string> = {
    dcv: 'DC电压',
    acv: 'AC电压',
    dci: 'DC电流',
    aci: 'AC电流',
    res: '电阻',
    cap: '电容',
    freq: '频率',
    temp: '温度'
  }
  return names[func] || func
}

const getCurrentRangeName = () => {
  const range = availableRanges.value.find(r => r.value === currentRange.value)
  return range?.label || currentRange.value
}

const getIntegrationName = (time: string) => {
  const names: Record<string, string> = {
    fast: '快速',
    medium: '中等',
    slow: '慢速'
  }
  return names[time] || time
}

// 生成测量值
const generateMeasurement = () => {
  let baseValue = 0
  let unit = 'V'
  
  switch (currentFunction.value) {
    case 'dcv':
      baseValue = 5.0 + Math.sin(Date.now() / 10000) * 0.5
      unit = 'V'
      break
    case 'acv':
      baseValue = 230.0 + Math.sin(Date.now() / 5000) * 2.0
      unit = 'V'
      showSecondaryReading.value = true
      secondaryReading.value = 50.0 + Math.random() * 0.1
      secondaryUnit.value = 'Hz'
      break
    case 'dci':
      baseValue = 0.1 + Math.sin(Date.now() / 8000) * 0.01
      unit = 'A'
      break
    case 'aci':
      baseValue = 0.5 + Math.sin(Date.now() / 6000) * 0.05
      unit = 'A'
      showSecondaryReading.value = true
      secondaryReading.value = 50.0 + Math.random() * 0.1
      secondaryUnit.value = 'Hz'
      break
    case 'res':
      baseValue = 1000 + Math.sin(Date.now() / 12000) * 10
      unit = 'Ω'
      break
    case 'cap':
      baseValue = 0.000001 + Math.sin(Date.now() / 15000) * 0.0000001
      unit = 'F'
      break
    case 'freq':
      baseValue = 1000 + Math.sin(Date.now() / 7000) * 50
      unit = 'Hz'
      break
    case 'temp':
      baseValue = 25.0 + Math.sin(Date.now() / 20000) * 2.0
      unit = '°C'
      break
  }
  
  // 添加噪声
  const noise = (Math.random() - 0.5) * baseValue * 0.001
  baseValue += noise
  
  // 检查过载
  if (Math.abs(baseValue) > getMaxRange()) {
    isOverload.value = true
  } else {
    isOverload.value = false
  }
  
  primaryReading.value = baseValue
  primaryUnit.value = unit
  
  // 更新统计
  updateStatistics(baseValue)
  
  // 添加到历史
  if (measurementHistory.value.length >= 1000) {
    measurementHistory.value.shift()
  }
  measurementHistory.value.push({
    time: Date.now(),
    value: baseValue
  })
}

const getMaxRange = () => {
  if (autoRange.value) return Infinity
  
  const rangeValue = parseFloat(currentRange.value)
  return isNaN(rangeValue) ? Infinity : rangeValue
}

// 控制方法
const setFunction = (func: string) => {
  currentFunction.value = func
  currentRange.value = 'auto'
  autoRange.value = true
  showSecondaryReading.value = ['acv', 'aci'].includes(func)
  resetStatistics()
}

const updateRange = () => {
  if (autoRange.value) {
    currentRange.value = 'auto'
  }
}

const updateSettings = () => {
  // 更新测量设置
  switch (integrationTime.value) {
    case 'fast':
      measurementRate.value = 100
      break
    case 'medium':
      measurementRate.value = 10
      break
    case 'slow':
      measurementRate.value = 1
      break
  }
}

const startMeasurement = () => {
  if (isMeasuring.value) return
  
  isMeasuring.value = true
  measurementTimer = setInterval(() => {
    if (!holdMode.value) {
      generateMeasurement()
    }
  }, 1000 / measurementRate.value)
}

const stopMeasurement = () => {
  if (measurementTimer) {
    clearInterval(measurementTimer)
    measurementTimer = null
  }
  isMeasuring.value = false
}

const singleMeasurement = () => {
  if (isMeasuring.value) return
  generateMeasurement()
}

const toggleRelative = () => {
  if (!relativeMode.value) {
    relativeReference.value = primaryReading.value
    relativeMode.value = true
  } else {
    relativeMode.value = false
    relativeReference.value = 0
  }
}

const toggleMinMax = () => {
  minMaxMode.value = !minMaxMode.value
  if (!minMaxMode.value) {
    statistics.value.maximum = -Infinity
    statistics.value.minimum = Infinity
  }
}

const updateStatistics = (value: number) => {
  statistics.value.current = value
  statistics.value.count++
  statistics.value.sum += value
  statistics.value.sumSquares += value * value
  
  statistics.value.average = statistics.value.sum / statistics.value.count
  
  if (minMaxMode.value || statistics.value.count === 1) {
    statistics.value.maximum = Math.max(statistics.value.maximum, value)
    statistics.value.minimum = Math.min(statistics.value.minimum, value)
  }
  
  if (statistics.value.count > 1) {
    const variance = (statistics.value.sumSquares - statistics.value.sum * statistics.value.sum / statistics.value.count) / (statistics.value.count - 1)
    statistics.value.stdDev = Math.sqrt(Math.max(0, variance))
  }
}

const resetStatistics = () => {
  statistics.value = {
    current: 0,
    average: 0,
    maximum: -Infinity,
    minimum: Infinity,
    stdDev: 0,
    count: 0,
    sum: 0,
    sumSquares: 0
  }
}

const clearHistory = () => {
  measurementHistory.value = []
}

const exportData = () => {
  const data = measurementHistory.value.map(item => ({
    time: new Date(item.time).toISOString(),
    value: item.value,
    unit: primaryUnit.value
  }))
  
  const csvContent = 'Time,Value,Unit\n' + 
    data.map(row => `${row.time},${row.value},${row.unit}`).join('\n')
  
  const blob = new Blob([csvContent], { type: 'text/csv' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.download = `dmm_data_${Date.now()}.csv`
  link.href = url
  link.click()
  URL.revokeObjectURL(url)
}

const updateTime = () => {
  currentTime.value = new Date().toLocaleTimeString()
}

// 生命周期
onMounted(() => {
  updateTime()
  timeTimer = setInterval(updateTime, 1000)
  
  // 开始默认测量
  startMeasurement()
})

onUnmounted(() => {
  stopMeasurement()
  if (timeTimer) {
    clearInterval(timeTimer)
  }
})

// 监听功能变化
watch(currentFunction, () => {
  resetStatistics()
  clearHistory()
})
</script>

<style lang="scss" scoped>
.digital-multimeter-wrapper {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: #f8f9fa;
  border: 2px solid #dee2e6;
  border-radius: 12px;
  overflow: hidden;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  
  .dmm-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px 20px;
    background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
    color: white;
    
    .header-left {
      h3 {
        margin: 0;
        font-size: 18px;
        font-weight: 600;
      }
      
      .model-info {
        font-size: 12px;
        opacity: 0.8;
        margin-top: 2px;
      }
    }
    
    .header-right {
      .status-indicators {
        display: flex;
        gap: 16px;
        
        .status-item {
          display: flex;
          align-items: center;
          gap: 6px;
          padding: 4px 8px;
          border-radius: 4px;
          background: rgba(255, 255, 255, 0.1);
          font-size: 12px;
          font-weight: 500;
          
          .status-dot {
            width: 8px;
            height: 8px;
            border-radius: 50%;
            background: #6c757d;
            transition: background-color 0.3s;
          }
          
          &.active .status-dot {
            background: #28a745;
            box-shadow: 0 0 8px rgba(40, 167, 69, 0.6);
          }
        }
      }
    }
  }
  
  .main-display {
    padding: 20px;
    background: #000;
    border-bottom: 1px solid #dee2e6;
    
    .display-screen {
      background: #001100;
      border: 3px solid #333;
      border-radius: 8px;
      padding: 20px;
      position: relative;
      
      .primary-reading {
        display: flex;
        align-items: baseline;
        justify-content: center;
        margin-bottom: 10px;
        
        .reading-value {
          font-size: 48px;
          font-weight: 700;
          color: #00ff00;
          font-family: 'Courier New', monospace;
          text-shadow: 0 0 10px rgba(0, 255, 0, 0.5);
          margin-right: 10px;
        }
        
        .reading-unit {
          font-size: 24px;
          font-weight: 600;
          color: #00cc00;
          font-family: 'Courier New', monospace;
        }
      }
      
      .secondary-reading {
        display: flex;
        align-items: baseline;
        justify-content: center;
        margin-bottom: 15px;
        
        .reading-value {
          font-size: 20px;
          font-weight: 600;
          color: #ffff00;
          font-family: 'Courier New', monospace;
          margin-right: 8px;
        }
        
        .reading-unit {
          font-size: 14px;
          font-weight: 500;
          color: #cccc00;
          font-family: 'Courier New', monospace;
        }
      }
      
      .display-indicators {
        display: flex;
        justify-content: center;
        gap: 15px;
        
        .indicator {
          padding: 2px 8px;
          border-radius: 3px;
          font-size: 10px;
          font-weight: 600;
          color: #666;
          background: rgba(255, 255, 255, 0.1);
          font-family: 'Courier New', monospace;
          
          &.active {
            color: #00ff00;
            background: rgba(0, 255, 0, 0.2);
            text-shadow: 0 0 5px rgba(0, 255, 0, 0.8);
          }
        }
      }
    }
  }
  
  .function-panel,
  .measurement-panel,
  .statistics-panel,
  .chart-panel {
    padding: 16px 20px;
    background: #ffffff;
    border-bottom: 1px solid #dee2e6;
    
    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
      
      h4 {
        margin: 0;
        font-size: 14px;
        font-weight: 600;
        color: #495057;
      }
      
      .stats-controls,
      .chart-controls {
        display: flex;
        gap: 8px;
      }
    }
    
    .function-section,
    .control-section {
      h4 {
        margin: 0 0 12px 0;
        font-size: 14px;
        font-weight: 600;
        color: #495057;
        border-bottom: 2px solid #e9ecef;
        padding-bottom: 4px;
      }
      
      .function-buttons {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
        
        .function-btn {
          min-width: 80px;
        }
      }
      
      .range-controls {
        display: flex;
        flex-direction: column;
        gap: 8px;
        
        .manual-range {
          margin-top: 8px;
        }
      }
      
      .measurement-controls,
      .data-controls {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
      }
      
      .setting-controls {
        .control-row {
          display: flex;
          align-items: center;
          margin-bottom: 8px;
          
          label {
            min-width: 80px;
            font-size: 12px;
            color: #6c757d;
            margin-right: 8px;
          }
        }
      }
    }
    
    .statistics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 12px;
      
      .stat-item {
        padding: 12px;
        border: 1px solid #e9ecef;
        border-radius: 6px;
        background: #f8f9fa;
        text-align: center;
        
        .stat-label {
          font-size: 11px;
          color: #6c757d;
          margin-bottom: 4px;
          font-weight: 500;
        }
        
        .stat-value {
          font-size: 14px;
          font-weight: 700;
          color: #212529;
          font-family: 'Courier New', monospace;
        }
      }
    }
    
    .chart-container {
      margin-top: 12px;
    }
  }
  
  .status-bar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 20px;
    background: #e9ecef;
    font-size: 11px;
    color: #6c757d;
    font-family: 'Courier New', monospace;
    
    .status-left,
    .status-right {
      display: flex;
      gap: 16px;
    }
    
    span {
      white-space: nowrap;
    }
  }
}

// 专业仪器样式
.professional-instrument {
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}

// 响应式设计
@media (max-width: 1200px) {
  .digital-multimeter-wrapper {
    .function-panel,
    .measurement-panel {
      .function-section,
      .control-section {
        margin-bottom: 16px;
      }
    }
  }
}

@media (max-width: 768px) {
  .digital-multimeter-wrapper {
    .dmm-header {
      flex-direction: column;
      gap: 8px;
      text-align: center;
    }
    
    .main-display {
      .display-screen {
        .primary-reading {
          .reading-value {
            font-size: 36px;
          }
          
          .reading-unit {
            font-size: 18px;
          }
        }
        
        .secondary-reading {
          .reading-value {
            font-size: 16px;
          }
          
          .reading-unit {
            font-size: 12px;
          }
        }
        
        .display-indicators {
          flex-wrap: wrap;
          gap: 8px;
        }
      }
    }
    
    .function-panel,
    .measurement-panel {
      .function-section,
      .control-section {
        .function-buttons,
        .measurement-controls,
        .data-controls {
          justify-content: center;
        }
        
        .setting-controls {
          .control-row {
            flex-direction: column;
            align-items: flex-start;
            
            label {
              margin-bottom: 4px;
            }
          }
        }
      }
    }
    
    .statistics-panel {
      .statistics-grid {
        grid-template-columns: repeat(2, 1fr);
      }
    }
    
    .status-bar {
      flex-direction: column;
      gap: 8px;
      
      .status-left,
      .status-right {
        justify-content: center;
        flex-wrap: wrap;
      }
    }
  }
}
</style>
