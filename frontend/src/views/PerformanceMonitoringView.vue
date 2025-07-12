<template>
  <div class="performance-monitoring">
    <div class="header">
      <h1>🔍 性能监控仪表板</h1>
      <div class="controls">
        <button @click="toggleAutoRefresh" :class="{ active: autoRefresh }">
          {{ autoRefresh ? '⏸️ 暂停刷新' : '▶️ 自动刷新' }}
        </button>
        <button @click="refreshData">🔄 立即刷新</button>
        <select v-model="refreshInterval" @change="updateRefreshInterval">
          <option value="1000">1秒</option>
          <option value="5000">5秒</option>
          <option value="10000">10秒</option>
          <option value="30000">30秒</option>
        </select>
      </div>
    </div>

    <div class="metrics-grid">
      <!-- 系统性能指标 -->
      <div class="metric-card">
        <h3>🖥️ 系统性能</h3>
        <div class="metric-item">
          <span class="label">CPU使用率:</span>
          <div class="progress-bar">
            <div class="progress" :style="{ width: systemMetrics.cpuUsagePercent + '%' }"></div>
            <span class="value">{{ systemMetrics.cpuUsagePercent?.toFixed(1) }}%</span>
          </div>
        </div>
        <div class="metric-item">
          <span class="label">内存使用率:</span>
          <div class="progress-bar">
            <div class="progress" :style="{ width: systemMetrics.memoryUsagePercent + '%' }"></div>
            <span class="value">{{ systemMetrics.memoryUsagePercent?.toFixed(1) }}%</span>
          </div>
        </div>
        <div class="metric-item">
          <span class="label">磁盘使用率:</span>
          <div class="progress-bar">
            <div class="progress" :style="{ width: systemMetrics.diskUsagePercent + '%' }"></div>
            <span class="value">{{ systemMetrics.diskUsagePercent?.toFixed(1) }}%</span>
          </div>
        </div>
        <div class="metric-item">
          <span class="label">线程数:</span>
          <span class="value">{{ systemMetrics.threadCount }}</span>
        </div>
        <div class="metric-item">
          <span class="label">运行时间:</span>
          <span class="value">{{ formatUptime(systemMetrics.uptime) }}</span>
        </div>
      </div>

      <!-- 应用程序性能指标 -->
      <div class="metric-card">
        <h3>🚀 应用程序性能</h3>
        <div class="metric-item">
          <span class="label">工作集内存:</span>
          <span class="value">{{ formatBytes(applicationMetrics.workingSetBytes) }}</span>
        </div>
        <div class="metric-item">
          <span class="label">私有内存:</span>
          <span class="value">{{ formatBytes(applicationMetrics.privateMemoryBytes) }}</span>
        </div>
        <div class="metric-item">
          <span class="label">GC内存:</span>
          <span class="value">{{ formatBytes(applicationMetrics.gcTotalMemoryBytes) }}</span>
        </div>
        <div class="metric-item">
          <span class="label">GC Gen0:</span>
          <span class="value">{{ applicationMetrics.gcGen0Collections }}</span>
        </div>
        <div class="metric-item">
          <span class="label">GC Gen1:</span>
          <span class="value">{{ applicationMetrics.gcGen1Collections }}</span>
        </div>
        <div class="metric-item">
          <span class="label">GC Gen2:</span>
          <span class="value">{{ applicationMetrics.gcGen2Collections }}</span>
        </div>
        <div class="metric-item">
          <span class="label">线程池线程:</span>
          <span class="value">{{ applicationMetrics.threadPoolThreads }}</span>
        </div>
      </div>

      <!-- 数据采集性能指标 -->
      <div class="metric-card">
        <h3>📊 数据采集性能</h3>
        <div class="metric-item">
          <span class="label">总采样数:</span>
          <span class="value">{{ dataAcquisitionMetrics.totalSamplesAcquired?.toLocaleString() }}</span>
        </div>
        <div class="metric-item">
          <span class="label">采样率:</span>
          <span class="value">{{ formatFrequency(dataAcquisitionMetrics.samplingRateHz) }}</span>
        </div>
        <div class="metric-item">
          <span class="label">数据吞吐量:</span>
          <span class="value">{{ dataAcquisitionMetrics.dataThroughputMBps?.toFixed(2) }} MB/s</span>
        </div>
        <div class="metric-item">
          <span class="label">活跃通道:</span>
          <span class="value">{{ dataAcquisitionMetrics.activeChannels }}</span>
        </div>
        <div class="metric-item">
          <span class="label">缓冲区利用率:</span>
          <div class="progress-bar">
            <div class="progress" :style="{ width: dataAcquisitionMetrics.bufferUtilizationPercent + '%' }"></div>
            <span class="value">{{ dataAcquisitionMetrics.bufferUtilizationPercent }}%</span>
          </div>
        </div>
        <div class="metric-item">
          <span class="label">丢失样本:</span>
          <span class="value">{{ dataAcquisitionMetrics.droppedSamples }}</span>
        </div>
        <div class="metric-item">
          <span class="label">平均延迟:</span>
          <span class="value">{{ dataAcquisitionMetrics.averageLatencyMs?.toFixed(2) }} ms</span>
        </div>
      </div>

      <!-- 健康状态 -->
      <div class="metric-card">
        <h3>💚 健康状态</h3>
        <div class="health-status" :class="healthStatus.status?.toLowerCase()">
          <div class="status-indicator"></div>
          <span class="status-text">{{ healthStatus.status }}</span>
        </div>
        <div class="health-checks">
          <div class="check-item" :class="{ ok: healthStatus.checks?.cpuOk }">
            <span class="check-icon">{{ healthStatus.checks?.cpuOk ? '✅' : '❌' }}</span>
            <span>CPU状态</span>
          </div>
          <div class="check-item" :class="{ ok: healthStatus.checks?.memoryOk }">
            <span class="check-icon">{{ healthStatus.checks?.memoryOk ? '✅' : '❌' }}</span>
            <span>内存状态</span>
          </div>
          <div class="check-item" :class="{ ok: healthStatus.checks?.diskOk }">
            <span class="check-icon">{{ healthStatus.checks?.diskOk ? '✅' : '❌' }}</span>
            <span>磁盘状态</span>
          </div>
        </div>
        <div class="metric-item">
          <span class="label">最后更新:</span>
          <span class="value">{{ formatTime(healthStatus.timestamp) }}</span>
        </div>
      </div>
    </div>

    <!-- 性能图表 -->
    <div class="charts-section">
      <div class="chart-card">
        <h3>📈 CPU使用率趋势</h3>
        <canvas ref="cpuChart" width="400" height="200"></canvas>
      </div>
      <div class="chart-card">
        <h3>📈 内存使用趋势</h3>
        <canvas ref="memoryChart" width="400" height="200"></canvas>
      </div>
    </div>

    <!-- 错误信息 -->
    <div v-if="error" class="error-message">
      <h3>❌ 错误信息</h3>
      <p>{{ error }}</p>
      <button @click="clearError">清除错误</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import { backendApi } from '@/services/BackendApiService'

// 响应式数据
const systemMetrics = ref<any>({})
const applicationMetrics = ref<any>({})
const dataAcquisitionMetrics = ref<any>({})
const healthStatus = ref<any>({})
const error = ref<string>('')
const autoRefresh = ref(true)
const refreshInterval = ref(5000)

// 图表相关
const cpuChart = ref<HTMLCanvasElement>()
const memoryChart = ref<HTMLCanvasElement>()
const cpuData: number[] = []
const memoryData: number[] = []
const maxDataPoints = 50

let refreshTimer: number | null = null

// 获取性能数据
const fetchMetrics = async () => {
  try {
    const [systemRes, appRes, dataRes, healthRes] = await Promise.all([
      backendApi.get('/api/monitoring/system'),
      backendApi.get('/api/monitoring/application'),
      backendApi.get('/api/monitoring/data-acquisition'),
      backendApi.get('/api/monitoring/health')
    ])

    systemMetrics.value = systemRes
    applicationMetrics.value = appRes
    dataAcquisitionMetrics.value = dataRes
    healthStatus.value = healthRes

    // 更新图表数据
    updateChartData()
    
    error.value = ''
  } catch (err: any) {
    error.value = `获取性能数据失败: ${err.message}`
    console.error('Error fetching metrics:', err)
  }
}

// 更新图表数据
const updateChartData = () => {
  // 添加新数据点
  cpuData.push(systemMetrics.value.cpuUsagePercent || 0)
  memoryData.push(systemMetrics.value.memoryUsagePercent || 0)

  // 限制数据点数量
  if (cpuData.length > maxDataPoints) {
    cpuData.shift()
  }
  if (memoryData.length > maxDataPoints) {
    memoryData.shift()
  }

  // 重绘图表
  drawChart(cpuChart.value, cpuData, 'CPU使用率', '#ff6b6b')
  drawChart(memoryChart.value, memoryData, '内存使用率', '#4ecdc4')
}

// 绘制图表
const drawChart = (canvas: HTMLCanvasElement | undefined, data: number[], title: string, color: string) => {
  if (!canvas) return

  const ctx = canvas.getContext('2d')
  if (!ctx) return

  const width = canvas.width
  const height = canvas.height
  const padding = 40

  // 清空画布
  ctx.clearRect(0, 0, width, height)

  // 绘制背景
  ctx.fillStyle = '#f8f9fa'
  ctx.fillRect(0, 0, width, height)

  // 绘制网格
  ctx.strokeStyle = '#e9ecef'
  ctx.lineWidth = 1
  
  // 水平网格线
  for (let i = 0; i <= 10; i++) {
    const y = padding + (height - 2 * padding) * i / 10
    ctx.beginPath()
    ctx.moveTo(padding, y)
    ctx.lineTo(width - padding, y)
    ctx.stroke()
  }

  // 垂直网格线
  for (let i = 0; i <= 10; i++) {
    const x = padding + (width - 2 * padding) * i / 10
    ctx.beginPath()
    ctx.moveTo(x, padding)
    ctx.lineTo(x, height - padding)
    ctx.stroke()
  }

  // 绘制数据线
  if (data.length > 1) {
    ctx.strokeStyle = color
    ctx.lineWidth = 2
    ctx.beginPath()

    for (let i = 0; i < data.length; i++) {
      const x = padding + (width - 2 * padding) * i / (maxDataPoints - 1)
      const y = height - padding - (height - 2 * padding) * data[i] / 100

      if (i === 0) {
        ctx.moveTo(x, y)
      } else {
        ctx.lineTo(x, y)
      }
    }

    ctx.stroke()

    // 绘制数据点
    ctx.fillStyle = color
    for (let i = 0; i < data.length; i++) {
      const x = padding + (width - 2 * padding) * i / (maxDataPoints - 1)
      const y = height - padding - (height - 2 * padding) * data[i] / 100
      
      ctx.beginPath()
      ctx.arc(x, y, 3, 0, 2 * Math.PI)
      ctx.fill()
    }
  }

  // 绘制Y轴标签
  ctx.fillStyle = '#6c757d'
  ctx.font = '12px Arial'
  ctx.textAlign = 'right'
  for (let i = 0; i <= 10; i++) {
    const y = padding + (height - 2 * padding) * i / 10
    const value = 100 - i * 10
    ctx.fillText(`${value}%`, padding - 5, y + 4)
  }

  // 绘制当前值
  if (data.length > 0) {
    const currentValue = data[data.length - 1]
    ctx.fillStyle = color
    ctx.font = 'bold 16px Arial'
    ctx.textAlign = 'left'
    ctx.fillText(`${currentValue.toFixed(1)}%`, padding + 10, padding + 20)
  }
}

// 格式化函数
const formatBytes = (bytes: number): string => {
  if (!bytes) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const formatFrequency = (hz: number): string => {
  if (!hz) return '0 Hz'
  if (hz >= 1000000) return (hz / 1000000).toFixed(2) + ' MHz'
  if (hz >= 1000) return (hz / 1000).toFixed(2) + ' kHz'
  return hz.toFixed(2) + ' Hz'
}

const formatUptime = (uptime: string): string => {
  if (!uptime) return '0秒'
  // 解析 uptime 字符串 (格式: "00:00:00.0000000")
  const parts = uptime.split(':')
  if (parts.length >= 3) {
    const hours = parseInt(parts[0])
    const minutes = parseInt(parts[1])
    const seconds = parseInt(parts[2].split('.')[0])
    
    if (hours > 0) {
      return `${hours}小时${minutes}分钟`
    } else if (minutes > 0) {
      return `${minutes}分钟${seconds}秒`
    } else {
      return `${seconds}秒`
    }
  }
  return uptime
}

const formatTime = (timestamp: string): string => {
  if (!timestamp) return ''
  return new Date(timestamp).toLocaleTimeString()
}

// 控制函数
const toggleAutoRefresh = () => {
  autoRefresh.value = !autoRefresh.value
  if (autoRefresh.value) {
    startAutoRefresh()
  } else {
    stopAutoRefresh()
  }
}

const refreshData = () => {
  fetchMetrics()
}

const updateRefreshInterval = () => {
  if (autoRefresh.value) {
    stopAutoRefresh()
    startAutoRefresh()
  }
}

const startAutoRefresh = () => {
  if (refreshTimer) {
    clearInterval(refreshTimer)
  }
  refreshTimer = setInterval(fetchMetrics, refreshInterval.value)
}

const stopAutoRefresh = () => {
  if (refreshTimer) {
    clearInterval(refreshTimer)
    refreshTimer = null
  }
}

const clearError = () => {
  error.value = ''
}

// 生命周期
onMounted(async () => {
  await fetchMetrics()
  await nextTick()
  if (autoRefresh.value) {
    startAutoRefresh()
  }
})

onUnmounted(() => {
  stopAutoRefresh()
})
</script>

<style scoped>
.performance-monitoring {
  padding: 20px;
  background: #f8f9fa;
  min-height: 100vh;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
  background: white;
  padding: 20px;
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.header h1 {
  margin: 0;
  color: #2c3e50;
  font-size: 28px;
}

.controls {
  display: flex;
  gap: 10px;
  align-items: center;
}

.controls button {
  padding: 8px 16px;
  border: none;
  border-radius: 5px;
  background: #007bff;
  color: white;
  cursor: pointer;
  transition: background 0.3s;
}

.controls button:hover {
  background: #0056b3;
}

.controls button.active {
  background: #28a745;
}

.controls select {
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 5px;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.metric-card {
  background: white;
  padding: 20px;
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.metric-card h3 {
  margin: 0 0 20px 0;
  color: #2c3e50;
  font-size: 18px;
  border-bottom: 2px solid #e9ecef;
  padding-bottom: 10px;
}

.metric-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.metric-item .label {
  font-weight: 500;
  color: #6c757d;
}

.metric-item .value {
  font-weight: bold;
  color: #2c3e50;
}

.progress-bar {
  position: relative;
  width: 120px;
  height: 20px;
  background: #e9ecef;
  border-radius: 10px;
  overflow: hidden;
}

.progress {
  height: 100%;
  background: linear-gradient(90deg, #28a745, #ffc107, #dc3545);
  transition: width 0.3s ease;
}

.progress-bar .value {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-size: 12px;
  font-weight: bold;
  color: white;
  text-shadow: 1px 1px 1px rgba(0,0,0,0.5);
}

.health-status {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 15px;
}

.status-indicator {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #28a745;
}

.health-status.unhealthy .status-indicator {
  background: #dc3545;
}

.health-status.degraded .status-indicator {
  background: #ffc107;
}

.status-text {
  font-weight: bold;
  color: #28a745;
}

.health-status.unhealthy .status-text {
  color: #dc3545;
}

.health-status.degraded .status-text {
  color: #ffc107;
}

.health-checks {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 15px;
}

.check-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
}

.check-icon {
  font-size: 16px;
}

.charts-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.chart-card {
  background: white;
  padding: 20px;
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.chart-card h3 {
  margin: 0 0 20px 0;
  color: #2c3e50;
  font-size: 18px;
}

.chart-card canvas {
  width: 100%;
  height: 200px;
  border: 1px solid #e9ecef;
  border-radius: 5px;
}

.error-message {
  background: #f8d7da;
  color: #721c24;
  padding: 20px;
  border-radius: 10px;
  border: 1px solid #f5c6cb;
}

.error-message h3 {
  margin: 0 0 10px 0;
}

.error-message button {
  margin-top: 10px;
  padding: 8px 16px;
  border: none;
  border-radius: 5px;
  background: #dc3545;
  color: white;
  cursor: pointer;
}

.error-message button:hover {
  background: #c82333;
}
</style>
