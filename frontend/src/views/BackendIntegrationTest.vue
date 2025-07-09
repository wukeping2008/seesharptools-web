<template>
  <div class="backend-integration-test">
    <el-card class="test-card">
      <template #header>
        <div class="card-header">
          <h2>🔗 前后端集成测试</h2>
          <el-tag :type="connectionStatus.type" size="large">
            {{ connectionStatus.text }}
          </el-tag>
        </div>
      </template>

      <!-- 连接状态 -->
      <el-row :gutter="20" class="status-section">
        <el-col :span="12">
          <el-card shadow="hover">
            <template #header>
              <h3>🌐 API连接状态</h3>
            </template>
            <div class="status-content">
              <el-tag :type="apiStatus.connected ? 'success' : 'danger'" size="large">
                {{ apiStatus.connected ? '已连接' : '未连接' }}
              </el-tag>
              <p class="status-url">{{ apiStatus.url }}</p>
              <el-button 
                type="primary" 
                @click="testApiConnection"
                :loading="apiStatus.testing"
                size="small"
              >
                测试连接
              </el-button>
            </div>
          </el-card>
        </el-col>

        <el-col :span="12">
          <el-card shadow="hover">
            <template #header>
              <h3>⚡ SignalR连接状态</h3>
            </template>
            <div class="status-content">
              <el-tag :type="signalRStatus.connected ? 'success' : 'danger'" size="large">
                {{ signalRStatus.connected ? '已连接' : '未连接' }}
              </el-tag>
              <p class="status-url">{{ signalRStatus.url }}</p>
              <el-button 
                type="primary" 
                @click="toggleSignalRConnection"
                :loading="signalRStatus.connecting"
                size="small"
              >
                {{ signalRStatus.connected ? '断开连接' : '建立连接' }}
              </el-button>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <!-- 设备管理测试 -->
      <el-card shadow="hover" class="test-section">
        <template #header>
          <h3>🔧 设备管理测试</h3>
        </template>
        
        <div class="test-controls">
          <el-button 
            type="primary" 
            @click="loadDevices"
            :loading="deviceTest.loading"
            icon="Refresh"
          >
            获取设备列表
          </el-button>
          <el-button 
            type="success" 
            @click="refreshDevices"
            :loading="deviceTest.refreshing"
            icon="Search"
          >
            刷新设备发现
          </el-button>
        </div>

        <div v-if="deviceTest.devices.length > 0" class="devices-list">
          <h4>发现的设备 ({{ deviceTest.devices.length }}个):</h4>
          <el-table :data="deviceTest.devices" stripe style="width: 100%">
            <el-table-column prop="id" label="ID" width="80" />
            <el-table-column prop="model" label="型号" width="150" />
            <el-table-column prop="serialNumber" label="序列号" width="150" />
            <el-table-column prop="status" label="状态" width="100">
              <template #default="scope">
                <el-tag :type="getStatusType(scope.row.status)" size="small">
                  {{ scope.row.status }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="deviceType" label="设备类型" width="120" />
            <el-table-column prop="capabilities" label="功能" min-width="200">
              <template #default="scope">
                <el-tag 
                  v-for="capability in scope.row.capabilities" 
                  :key="capability"
                  size="small"
                  style="margin-right: 5px;"
                >
                  {{ capability }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="150">
              <template #default="scope">
                <el-button 
                  type="primary" 
                  size="small"
                  @click="testDevice(scope.row)"
                >
                  测试设备
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </div>

        <div v-else-if="!deviceTest.loading" class="no-devices">
          <el-empty description="暂无设备" />
        </div>
      </el-card>

      <!-- 任务管理测试 -->
      <el-card shadow="hover" class="test-section">
        <template #header>
          <h3>📋 任务管理测试</h3>
        </template>

        <div class="test-controls">
          <el-button 
            type="primary" 
            @click="createTestTask"
            :loading="taskTest.creating"
            :disabled="deviceTest.devices.length === 0"
            icon="Plus"
          >
            创建测试任务
          </el-button>
          <el-button 
            v-if="taskTest.currentTask"
            type="success" 
            @click="startTask"
            :loading="taskTest.starting"
            icon="VideoPlay"
          >
            启动任务
          </el-button>
          <el-button 
            v-if="taskTest.currentTask"
            type="warning" 
            @click="stopTask"
            :loading="taskTest.stopping"
            icon="VideoPause"
          >
            停止任务
          </el-button>
          <el-button 
            v-if="taskTest.currentTask"
            type="danger" 
            @click="deleteTask"
            :loading="taskTest.deleting"
            icon="Delete"
          >
            删除任务
          </el-button>
        </div>

        <div v-if="taskTest.currentTask" class="task-info">
          <h4>当前任务信息:</h4>
          <el-descriptions :column="2" border>
            <el-descriptions-item label="任务ID">{{ taskTest.currentTask.id }}</el-descriptions-item>
            <el-descriptions-item label="任务名称">{{ taskTest.currentTask.taskName }}</el-descriptions-item>
            <el-descriptions-item label="设备ID">{{ taskTest.currentTask.deviceId }}</el-descriptions-item>
            <el-descriptions-item label="任务类型">{{ taskTest.currentTask.taskType }}</el-descriptions-item>
            <el-descriptions-item label="采样率">{{ taskTest.currentTask.sampling.sampleRate }} Hz</el-descriptions-item>
            <el-descriptions-item label="采样点数">{{ taskTest.currentTask.sampling.samplesToAcquire }}</el-descriptions-item>
          </el-descriptions>
        </div>

        <div v-if="taskTest.status" class="task-status">
          <h4>任务状态:</h4>
          <el-descriptions :column="3" border>
            <el-descriptions-item label="状态">
              <el-tag :type="getTaskStatusType(taskTest.status.status)">
                {{ taskTest.status.status }}
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="进度">{{ taskTest.status.progress }}%</el-descriptions-item>
            <el-descriptions-item label="已采集">{{ taskTest.status.samplesAcquired }}</el-descriptions-item>
            <el-descriptions-item label="总样本">{{ taskTest.status.totalSamples }}</el-descriptions-item>
            <el-descriptions-item label="运行时间">{{ taskTest.status.elapsedTime }}ms</el-descriptions-item>
            <el-descriptions-item label="错误信息">{{ taskTest.status.errorMessage || '无' }}</el-descriptions-item>
          </el-descriptions>
        </div>
      </el-card>

      <!-- 实时数据测试 -->
      <el-card shadow="hover" class="test-section">
        <template #header>
          <h3>📊 实时数据测试</h3>
        </template>

        <div class="test-controls">
          <el-button 
            type="primary" 
            @click="startDataMonitoring"
            :loading="dataTest.monitoring"
            :disabled="!taskTest.currentTask"
            icon="Monitor"
          >
            开始数据监控
          </el-button>
          <el-button 
            type="warning" 
            @click="stopDataMonitoring"
            :disabled="!dataTest.monitoring"
            icon="Close"
          >
            停止监控
          </el-button>
          <el-button 
            type="info" 
            @click="clearDataLog"
            icon="Delete"
          >
            清除日志
          </el-button>
        </div>

        <div v-if="dataTest.monitoring" class="data-stats">
          <el-row :gutter="20">
            <el-col :span="6">
              <el-statistic title="接收数据包" :value="dataTest.packetsReceived" />
            </el-col>
            <el-col :span="6">
              <el-statistic title="总样本数" :value="dataTest.totalSamples" />
            </el-col>
            <el-col :span="6">
              <el-statistic title="数据率" :value="dataTest.dataRate" suffix="Hz" />
            </el-col>
            <el-col :span="6">
              <el-statistic title="最后更新" :value="dataTest.lastUpdate" />
            </el-col>
          </el-row>
        </div>

        <!-- 实时波形图表 -->
        <div v-if="dataTest.monitoring" class="waveform-chart">
          <h4>实时波形显示</h4>
          <div class="chart-container">
            <div ref="chartRef" class="chart" style="width: 100%; height: 400px;"></div>
          </div>
          <div class="chart-controls">
            <el-row :gutter="10">
              <el-col :span="4">
                <el-select v-model="chartConfig.timeRange" size="small" @change="updateTimeRange">
                  <el-option label="1秒" value="1" />
                  <el-option label="5秒" value="5" />
                  <el-option label="10秒" value="10" />
                  <el-option label="30秒" value="30" />
                </el-select>
              </el-col>
              <el-col :span="4">
                <el-select v-model="chartConfig.amplitude" size="small" @change="updateAmplitude">
                  <el-option label="±1V" value="1" />
                  <el-option label="±5V" value="5" />
                  <el-option label="±10V" value="10" />
                  <el-option label="自动" value="auto" />
                </el-select>
              </el-col>
              <el-col :span="4">
                <el-button size="small" @click="pauseChart" :type="chartConfig.paused ? 'success' : 'warning'">
                  {{ chartConfig.paused ? '继续' : '暂停' }}
                </el-button>
              </el-col>
              <el-col :span="4">
                <el-button size="small" @click="clearChart" type="info">清除</el-button>
              </el-col>
              <el-col :span="8">
                <span class="chart-info">通道数: {{ chartConfig.channels }} | 采样率: {{ chartConfig.sampleRate }}Hz</span>
              </el-col>
            </el-row>
          </div>
        </div>

        <div class="data-log">
          <h4>数据日志 (最近{{ Math.min(dataTest.log.length, 50) }}条):</h4>
          <el-scrollbar height="300px">
            <div 
              v-for="(entry, index) in dataTest.log.slice(-50)" 
              :key="index"
              class="log-entry"
              :class="entry.type"
            >
              <span class="log-time">{{ entry.timestamp }}</span>
              <span class="log-message">{{ entry.message }}</span>
            </div>
          </el-scrollbar>
        </div>
      </el-card>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { backendApi } from '@/services/BackendApiService'
import { signalRService } from '@/services/SignalRService'
import * as echarts from 'echarts'
import type { 
  HardwareDevice, 
  MISDTaskConfiguration, 
  TaskStatus 
} from '@/services/BackendApiService'
import type { 
  RealTimeData, 
  DeviceStatusUpdate, 
  TaskStatusUpdate 
} from '@/services/SignalRService'

// 连接状态
const apiStatus = ref({
  connected: false,
  testing: false,
  url: backendApi.getBaseURL()
})

const signalRStatus = ref({
  connected: false,
  connecting: false,
  url: signalRService.getBaseURL() + '/hubs/datastream'
})

// 设备测试
const deviceTest = ref({
  loading: false,
  refreshing: false,
  devices: [] as HardwareDevice[]
})

// 任务测试
const taskTest = ref({
  creating: false,
  starting: false,
  stopping: false,
  deleting: false,
  currentTask: null as MISDTaskConfiguration | null,
  taskId: null as number | null,
  status: null as TaskStatus | null
})

// 数据测试
const dataTest = ref({
  monitoring: false,
  packetsReceived: 0,
  totalSamples: 0,
  dataRate: 0,
  lastUpdate: '',
  log: [] as Array<{
    timestamp: string,
    message: string,
    type: 'info' | 'success' | 'warning' | 'error'
  }>
})

// 图表配置
const chartRef = ref<HTMLDivElement>()
const chartConfig = ref({
  timeRange: '5',
  amplitude: 'auto',
  paused: false,
  channels: 1,
  sampleRate: 1000
})

// 图表实例和数据
let chartInstance: echarts.ECharts | null = null
const chartData = ref({
  timeData: [] as number[],
  channelData: [] as number[][],
  maxPoints: 5000
})

let dataSimulationTimer: number | null = null

// 计算属性
const connectionStatus = computed(() => {
  if (apiStatus.value.connected && signalRStatus.value.connected) {
    return { type: 'success', text: '全部连接正常' }
  } else if (apiStatus.value.connected || signalRStatus.value.connected) {
    return { type: 'warning', text: '部分连接正常' }
  } else {
    return { type: 'danger', text: '连接异常' }
  }
})

// 方法
const addLog = (message: string, type: 'info' | 'success' | 'warning' | 'error' = 'info') => {
  dataTest.value.log.push({
    timestamp: new Date().toLocaleTimeString(),
    message,
    type
  })
}

const testApiConnection = async () => {
  apiStatus.value.testing = true
  try {
    const connected = await backendApi.checkConnection()
    apiStatus.value.connected = connected
    if (connected) {
      ElMessage.success('API连接测试成功')
      addLog('API连接测试成功', 'success')
    } else {
      ElMessage.error('API连接测试失败')
      addLog('API连接测试失败', 'error')
    }
  } catch (error) {
    console.error('API connection test failed:', error)
    apiStatus.value.connected = false
    ElMessage.error('API连接测试异常')
    addLog(`API连接测试异常: ${error}`, 'error')
  } finally {
    apiStatus.value.testing = false
  }
}

const toggleSignalRConnection = async () => {
  if (signalRStatus.value.connected) {
    await signalRService.disconnect()
  } else {
    signalRStatus.value.connecting = true
    try {
      const connected = await signalRService.connect()
      if (connected) {
        ElMessage.success('SignalR连接成功')
        addLog('SignalR连接成功', 'success')
      } else {
        ElMessage.error('SignalR连接失败')
        addLog('SignalR连接失败', 'error')
      }
    } catch (error) {
      console.error('SignalR connection failed:', error)
      ElMessage.error('SignalR连接异常')
      addLog(`SignalR连接异常: ${error}`, 'error')
    } finally {
      signalRStatus.value.connecting = false
    }
  }
}

const loadDevices = async () => {
  deviceTest.value.loading = true
  try {
    const devices = await backendApi.getDevices()
    deviceTest.value.devices = devices
    ElMessage.success(`成功获取${devices.length}个设备`)
    addLog(`成功获取${devices.length}个设备`, 'success')
  } catch (error) {
    console.error('Failed to load devices:', error)
    ElMessage.error('获取设备列表失败')
    addLog(`获取设备列表失败: ${error}`, 'error')
  } finally {
    deviceTest.value.loading = false
  }
}

const refreshDevices = async () => {
  deviceTest.value.refreshing = true
  try {
    const devices = await backendApi.refreshDevices()
    deviceTest.value.devices = devices
    ElMessage.success(`设备发现完成，找到${devices.length}个设备`)
    addLog(`设备发现完成，找到${devices.length}个设备`, 'success')
  } catch (error) {
    console.error('Failed to refresh devices:', error)
    ElMessage.error('刷新设备发现失败')
    addLog(`刷新设备发现失败: ${error}`, 'error')
  } finally {
    deviceTest.value.refreshing = false
  }
}

const testDevice = async (device: HardwareDevice) => {
  try {
    const functions = await backendApi.getDeviceFunctions(device.id)
    ElMessage.success(`设备${device.model}支持${functions.length}个功能`)
    addLog(`设备${device.model}支持功能: ${functions.join(', ')}`, 'info')
  } catch (error) {
    console.error('Failed to test device:', error)
    ElMessage.error('设备测试失败')
    addLog(`设备测试失败: ${error}`, 'error')
  }
}

const createTestTask = async () => {
  if (deviceTest.value.devices.length === 0) {
    ElMessage.warning('请先获取设备列表')
    return
  }

  taskTest.value.creating = true
  try {
    const device = deviceTest.value.devices[0]
    const config: MISDTaskConfiguration = {
      taskName: `测试任务_${Date.now()}`,
      deviceId: device.id,
      taskType: 'AI',
      channels: [
        {
          channelId: 0,
          rangeLow: -10.0,
          rangeHigh: 10.0,
          terminal: 'RSE',
          coupling: 'DC',
          enableIEPE: false
        }
      ],
      sampling: {
        sampleRate: 1000.0,
        samplesToAcquire: 1000,
        mode: 'Finite',
        bufferSize: 10000
      },
      trigger: {
        type: 'Immediate',
        source: '',
        edge: 'Rising',
        level: 0.0,
        preTriggerSamples: 0
      }
    }

    const taskId = await backendApi.createTask(config)
    taskTest.value.currentTask = { ...config, id: taskId }
    taskTest.value.taskId = taskId
    ElMessage.success(`任务创建成功，ID: ${taskId}`)
    addLog(`任务创建成功，ID: ${taskId}`, 'success')
  } catch (error) {
    console.error('Failed to create task:', error)
    ElMessage.error('创建任务失败')
    addLog(`创建任务失败: ${error}`, 'error')
  } finally {
    taskTest.value.creating = false
  }
}

const startTask = async () => {
  if (!taskTest.value.taskId) return

  taskTest.value.starting = true
  try {
    await backendApi.startTask(taskTest.value.taskId)
    ElMessage.success('任务启动成功')
    addLog('任务启动成功', 'success')
    
    // 开始监控任务状态
    startTaskStatusMonitoring()
  } catch (error) {
    console.error('Failed to start task:', error)
    ElMessage.error('启动任务失败')
    addLog(`启动任务失败: ${error}`, 'error')
  } finally {
    taskTest.value.starting = false
  }
}

const stopTask = async () => {
  if (!taskTest.value.taskId) return

  taskTest.value.stopping = true
  try {
    await backendApi.stopTask(taskTest.value.taskId)
    ElMessage.success('任务停止成功')
    addLog('任务停止成功', 'success')
  } catch (error) {
    console.error('Failed to stop task:', error)
    ElMessage.error('停止任务失败')
    addLog(`停止任务失败: ${error}`, 'error')
  } finally {
    taskTest.value.stopping = false
  }
}

const deleteTask = async () => {
  if (!taskTest.value.taskId) return

  try {
    await ElMessageBox.confirm('确定要删除当前任务吗？', '确认删除', {
      type: 'warning'
    })

    taskTest.value.deleting = true
    await backendApi.deleteTask(taskTest.value.taskId)
    
    taskTest.value.currentTask = null
    taskTest.value.taskId = null
    taskTest.value.status = null
    
    ElMessage.success('任务删除成功')
    addLog('任务删除成功', 'success')
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Failed to delete task:', error)
      ElMessage.error('删除任务失败')
      addLog(`删除任务失败: ${error}`, 'error')
    }
  } finally {
    taskTest.value.deleting = false
  }
}

let taskStatusInterval: number | null = null

const startTaskStatusMonitoring = () => {
  if (taskStatusInterval) {
    clearInterval(taskStatusInterval)
  }

  taskStatusInterval = setInterval(async () => {
    if (taskTest.value.taskId) {
      try {
        const status = await backendApi.getTaskStatus(taskTest.value.taskId)
        taskTest.value.status = status
      } catch (error) {
        console.error('Failed to get task status:', error)
      }
    }
  }, 1000)
}

const startDataMonitoring = async () => {
  if (!taskTest.value.taskId) {
    ElMessage.warning('请先创建任务')
    return
  }

  if (!signalRStatus.value.connected) {
    ElMessage.warning('请先连接SignalR')
    return
  }

  dataTest.value.monitoring = true
  dataTest.value.packetsReceived = 0
  dataTest.value.totalSamples = 0
  
  // 加入数据组
  await signalRService.joinDataGroup(taskTest.value.taskId)
  
  // 初始化图表
  await nextTick()
  await initChart()
  
  ElMessage.success('开始数据监控')
  addLog('开始数据监控', 'success')
}

const stopDataMonitoring = async () => {
  if (taskTest.value.taskId) {
    await signalRService.leaveDataGroup(taskTest.value.taskId)
  }
  
  dataTest.value.monitoring = false
  
  // 清理图表
  destroyChart()
  
  ElMessage.success('停止数据监控')
  addLog('停止数据监控', 'warning')
}

const clearDataLog = () => {
  dataTest.value.log = []
  ElMessage.success('日志已清除')
}

// 图表相关方法
const initChart = async () => {
  await nextTick()
  if (!chartRef.value) return

  chartInstance = echarts.init(chartRef.value)
  
  const option = {
    title: {
      text: '实时波形数据',
      left: 'center',
      textStyle: {
        color: '#333',
        fontSize: 16
      }
    },
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        let result = `时间: ${params[0].axisValue.toFixed(3)}s<br/>`
        params.forEach((param: any) => {
          result += `${param.seriesName}: ${param.value.toFixed(4)}V<br/>`
        })
        return result
      }
    },
    legend: {
      data: ['通道 0'],
      top: 30
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      top: '15%',
      containLabel: true
    },
    xAxis: {
      type: 'value',
      name: '时间 (s)',
      nameLocation: 'middle',
      nameGap: 30,
      axisLine: {
        lineStyle: { color: '#666' }
      },
      splitLine: {
        show: true,
        lineStyle: { color: '#eee', type: 'dashed' }
      }
    },
    yAxis: {
      type: 'value',
      name: '电压 (V)',
      nameLocation: 'middle',
      nameGap: 50,
      axisLine: {
        lineStyle: { color: '#666' }
      },
      splitLine: {
        show: true,
        lineStyle: { color: '#eee', type: 'dashed' }
      }
    },
    series: [
      {
        name: '通道 0',
        type: 'line',
        data: [],
        smooth: false,
        symbol: 'none',
        lineStyle: {
          color: '#2E86AB',
          width: 1.5
        },
        animation: false
      }
    ]
  }
  
  chartInstance.setOption(option)
  
  // 开始模拟数据
  startDataSimulation()
}

const startDataSimulation = () => {
  if (dataSimulationTimer) {
    clearInterval(dataSimulationTimer)
  }
  
  let time = 0
  const sampleRate = chartConfig.value.sampleRate
  const timeStep = 1 / sampleRate
  
  dataSimulationTimer = setInterval(() => {
    if (chartConfig.value.paused || !dataTest.value.monitoring) return
    
    // 生成模拟数据 - 混合正弦波
    const amplitude = 5
    const freq1 = 10 // 10Hz
    const freq2 = 50 // 50Hz
    const noise = (Math.random() - 0.5) * 0.5
    
    const value = amplitude * Math.sin(2 * Math.PI * freq1 * time) + 
                  amplitude * 0.3 * Math.sin(2 * Math.PI * freq2 * time) + 
                  noise
    
    // 添加数据点
    chartData.value.timeData.push(time)
    if (!chartData.value.channelData[0]) {
      chartData.value.channelData[0] = []
    }
    chartData.value.channelData[0].push(value)
    
    // 限制数据点数量
    const maxPoints = parseInt(chartConfig.value.timeRange) * sampleRate
    if (chartData.value.timeData.length > maxPoints) {
      chartData.value.timeData.shift()
      chartData.value.channelData[0].shift()
    }
    
    // 更新图表
    updateChart()
    
    time += timeStep
  }, 1000 / sampleRate)
}

const updateChart = () => {
  if (!chartInstance || chartConfig.value.paused) return
  
  const data = chartData.value.timeData.map((time, index) => [
    time,
    chartData.value.channelData[0][index]
  ])
  
  const option = {
    xAxis: {
      min: Math.max(0, chartData.value.timeData[chartData.value.timeData.length - 1] - parseInt(chartConfig.value.timeRange)),
      max: chartData.value.timeData[chartData.value.timeData.length - 1] || parseInt(chartConfig.value.timeRange)
    },
    yAxis: {
      min: chartConfig.value.amplitude === 'auto' ? 'dataMin' : -parseInt(chartConfig.value.amplitude),
      max: chartConfig.value.amplitude === 'auto' ? 'dataMax' : parseInt(chartConfig.value.amplitude)
    },
    series: [{
      data: data
    }]
  }
  
  chartInstance.setOption(option)
}

const updateTimeRange = () => {
  updateChart()
  addLog(`时间范围更新为: ${chartConfig.value.timeRange}秒`, 'info')
}

const updateAmplitude = () => {
  updateChart()
  addLog(`幅度范围更新为: ${chartConfig.value.amplitude === 'auto' ? '自动' : '±' + chartConfig.value.amplitude + 'V'}`, 'info')
}

const pauseChart = () => {
  chartConfig.value.paused = !chartConfig.value.paused
  addLog(`图表${chartConfig.value.paused ? '暂停' : '继续'}`, 'info')
}

const clearChart = () => {
  chartData.value.timeData = []
  chartData.value.channelData = [[]]
  if (chartInstance) {
    chartInstance.setOption({
      series: [{
        data: []
      }]
    })
  }
  addLog('图表数据已清除', 'info')
}

const destroyChart = () => {
  if (dataSimulationTimer) {
    clearInterval(dataSimulationTimer)
    dataSimulationTimer = null
  }
  if (chartInstance) {
    chartInstance.dispose()
    chartInstance = null
  }
}

// 工具函数
const getStatusType = (status: string) => {
  switch (status) {
    case 'Online': case 'Ready': return 'success'
    case 'Busy': return 'warning'
    case 'Error': case 'Offline': return 'danger'
    default: return 'info'
  }
}

const getTaskStatusType = (status: string) => {
  switch (status) {
    case 'Running': case 'Completed': return 'success'
    case 'Started': return 'warning'
    case 'Error': return 'danger'
    default: return 'info'
  }
}

// SignalR事件处理
let unsubscribeCallbacks: Array<() => void> = []

onMounted(async () => {
  // 初始化连接状态
  await testApiConnection()
  
  // 设置SignalR事件监听
  unsubscribeCallbacks.push(
    signalRService.onConnectionChange((connected) => {
      signalRStatus.value.connected = connected
      addLog(`SignalR连接状态: ${connected ? '已连接' : '已断开'}`, connected ? 'success' : 'warning')
    }),
    
    signalRService.onDataUpdate((data: RealTimeData) => {
      dataTest.value.packetsReceived++
      dataTest.value.totalSamples += data.totalSamples
      dataTest.value.dataRate = data.sampleRate
      dataTest.value.lastUpdate = new Date().toLocaleTimeString()
      addLog(`接收数据: 任务${data.taskId}, ${data.totalSamples}样本`, 'info')
    }),
    
    signalRService.onTaskStatusUpdate((update: TaskStatusUpdate) => {
      addLog(`任务状态更新: ${update.status} (${update.progress}%)`, 'info')
    }),
    
    signalRService.onDeviceStatusUpdate((update: DeviceStatusUpdate) => {
      addLog(`设备状态更新: 设备${update.deviceId} - ${update.status}`, 'info')
    })
  )
})

onUnmounted(() => {
  // 清理定时器
  if (taskStatusInterval) {
    clearInterval(taskStatusInterval)
  }
  
  // 取消事件订阅
  unsubscribeCallbacks.forEach(unsubscribe => unsubscribe())
  
  // 断开SignalR连接
  signalRService.disconnect()
})
</script>

<style scoped lang="scss">
.backend-integration-test {
  padding: 20px;
  
  .test-card {
    max-width: 1200px;
    margin: 0 auto;
  }
  
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    
    h2 {
      margin: 0;
      color: #409eff;
    }
  }
  
  .status-section {
    margin-bottom: 20px;
  }
  
  .status-content {
    text-align: center;
    
    .status-url {
      margin: 10px 0;
      color: #666;
      font-size: 12px;
    }
  }
  
  .test-section {
    margin-bottom: 20px;
  }
  
  .test-controls {
    margin-bottom: 20px;
    
    .el-button {
      margin-right: 10px;
      margin-bottom: 10px;
    }
  }
  
  .devices-list {
    margin-top: 20px;
  }
  
  .no-devices {
    text-align: center;
    padding: 40px;
  }
  
  .task-info, .task-status {
    margin-top: 20px;
  }
  
  .data-stats {
    margin-bottom: 20px;
  }
  
  .waveform-chart {
    margin-bottom: 20px;
    
    .chart-container {
      background: #fff;
      border: 1px solid #e4e7ed;
      border-radius: 4px;
      margin-bottom: 10px;
      
      .chart {
        border-radius: 4px;
      }
    }
    
    .chart-controls {
      padding: 10px;
      background: #f5f7fa;
      border-radius: 4px;
      
      .chart-info {
        font-size: 12px;
        color: #666;
        line-height: 32px;
      }
    }
  }
  
  .data-log {
    .log-entry {
      padding: 5px 10px;
      margin-bottom: 2px;
      border-radius: 4px;
      font-family: monospace;
      font-size: 12px;
      
      &.info {
        background-color: #f0f9ff;
        border-left: 3px solid #409eff;
      }
      
      &.success {
        background-color: #f0f9f0;
        border-left: 3px solid #67c23a;
      }
      
      &.warning {
        background-color: #fdf6ec;
        border-left: 3px solid #e6a23c;
      }
      
      &.error {
        background-color: #fef0f0;
        border-left: 3px solid #f56c6c;
      }
      
      .log-time {
        color: #666;
        margin-right: 10px;
      }
      
      .log-message {
        color: #333;
      }
    }
  }
}
</style>
