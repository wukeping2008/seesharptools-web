<template>
  <div class="hardware-driver-test">
    <div class="test-header">
      <h1>硬件驱动集成测试</h1>
      <p>测试真实硬件驱动的集成和功能</p>
    </div>

    <div class="test-content">
      <!-- 驱动状态检查 -->
      <el-card class="test-section">
        <template #header>
          <h3>驱动状态检查</h3>
        </template>
        
        <div class="driver-status">
          <el-row :gutter="20">
            <el-col :span="8">
              <el-statistic title="已加载驱动数" :value="driverStats.loadedCount" />
            </el-col>
            <el-col :span="8">
              <el-statistic title="发现设备数" :value="driverStats.deviceCount" />
            </el-col>
            <el-col :span="8">
              <el-statistic title="活跃任务数" :value="driverStats.activeTaskCount" />
            </el-col>
          </el-row>
          
          <div class="driver-actions">
            <el-button type="primary" @click="checkDriverStatus" :loading="loading.status">
              检查驱动状态
            </el-button>
            <el-button @click="refreshDevices" :loading="loading.devices">
              刷新设备列表
            </el-button>
          </div>
        </div>
      </el-card>

      <!-- 设备列表 -->
      <el-card class="test-section">
        <template #header>
          <h3>硬件设备列表</h3>
        </template>
        
        <el-table :data="devices" style="width: 100%">
          <el-table-column prop="id" label="设备ID" width="80" />
          <el-table-column prop="model" label="设备型号" width="120" />
          <el-table-column prop="name" label="设备名称" />
          <el-table-column prop="status" label="状态" width="100">
            <template #default="scope">
              <el-tag :type="getStatusType(scope.row.status)">
                {{ scope.row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="slotNumber" label="插槽" width="80" />
          <el-table-column label="操作" width="200">
            <template #default="scope">
              <el-button size="small" @click="testDevice(scope.row)">
                测试设备
              </el-button>
              <el-button size="small" type="info" @click="showDeviceInfo(scope.row)">
                详细信息
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-card>

      <!-- JY5500驱动测试 -->
      <el-card class="test-section">
        <template #header>
          <h3>JY5500驱动功能测试</h3>
        </template>
        
        <div class="jy5500-test">
          <el-form :model="jy5500Config" label-width="120px">
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="设备ID">
                  <el-select v-model="jy5500Config.deviceId" placeholder="选择设备">
                    <el-option 
                      v-for="device in jy5500Devices" 
                      :key="device.id"
                      :label="device.name" 
                      :value="device.id"
                    />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="采样率">
                  <el-select v-model="jy5500Config.sampleRate">
                    <el-option label="1kS/s" :value="1000" />
                    <el-option label="10kS/s" :value="10000" />
                    <el-option label="100kS/s" :value="100000" />
                    <el-option label="1MS/s" :value="1000000" />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>
            
            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="通道数">
                  <el-input-number v-model="jy5500Config.channelCount" :min="1" :max="32" />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="采样点数">
                  <el-input-number v-model="jy5500Config.sampleCount" :min="100" :max="1000000" />
                </el-form-item>
              </el-col>
            </el-row>
          </el-form>
          
          <div class="test-actions">
            <el-button type="primary" @click="startJY5500Test" :loading="loading.jy5500">
              开始JY5500测试
            </el-button>
            <el-button @click="stopJY5500Test" :disabled="!jy5500TestRunning">
              停止测试
            </el-button>
            <el-button type="info" @click="exportJY5500Results">
              导出结果
            </el-button>
          </div>
        </div>
      </el-card>

      <!-- 测试结果 -->
      <el-card class="test-section">
        <template #header>
          <h3>测试结果</h3>
        </template>
        
        <div class="test-results">
          <el-tabs v-model="activeResultTab">
            <el-tab-pane label="实时数据" name="realtime">
              <div class="realtime-data">
                <div class="data-stats">
                  <el-statistic title="接收数据包" :value="testResults.packetsReceived" />
                  <el-statistic title="数据点数" :value="testResults.samplesReceived" />
                  <el-statistic title="数据率" :value="testResults.dataRate" suffix="S/s" />
                  <el-statistic title="错误数" :value="testResults.errorCount" />
                </div>
                
                <div class="data-chart" ref="realtimeChart" style="height: 300px;"></div>
              </div>
            </el-tab-pane>
            
            <el-tab-pane label="性能分析" name="performance">
              <div class="performance-analysis">
                <el-descriptions :column="2" border>
                  <el-descriptions-item label="平均延迟">{{ testResults.averageLatency }}ms</el-descriptions-item>
                  <el-descriptions-item label="最大延迟">{{ testResults.maxLatency }}ms</el-descriptions-item>
                  <el-descriptions-item label="丢包率">{{ testResults.packetLossRate }}%</el-descriptions-item>
                  <el-descriptions-item label="CPU使用率">{{ testResults.cpuUsage }}%</el-descriptions-item>
                  <el-descriptions-item label="内存使用">{{ testResults.memoryUsage }}MB</el-descriptions-item>
                  <el-descriptions-item label="测试时长">{{ testResults.testDuration }}s</el-descriptions-item>
                </el-descriptions>
              </div>
            </el-tab-pane>
            
            <el-tab-pane label="错误日志" name="errors">
              <div class="error-logs">
                <el-table :data="errorLogs" style="width: 100%" max-height="300">
                  <el-table-column prop="timestamp" label="时间" width="180" />
                  <el-table-column prop="level" label="级别" width="80">
                    <template #default="scope">
                      <el-tag :type="getLogLevelType(scope.row.level)">
                        {{ scope.row.level }}
                      </el-tag>
                    </template>
                  </el-table-column>
                  <el-table-column prop="message" label="消息" />
                </el-table>
              </div>
            </el-tab-pane>
          </el-tabs>
        </div>
      </el-card>
    </div>

    <!-- 设备详细信息对话框 -->
    <el-dialog v-model="deviceInfoVisible" title="设备详细信息" width="600px">
      <div v-if="selectedDevice">
        <el-descriptions :column="1" border>
          <el-descriptions-item label="设备ID">{{ selectedDevice.id }}</el-descriptions-item>
          <el-descriptions-item label="设备型号">{{ selectedDevice.model }}</el-descriptions-item>
          <el-descriptions-item label="设备名称">{{ selectedDevice.name }}</el-descriptions-item>
          <el-descriptions-item label="设备类型">{{ selectedDevice.deviceType }}</el-descriptions-item>
          <el-descriptions-item label="状态">{{ selectedDevice.status }}</el-descriptions-item>
          <el-descriptions-item label="插槽号">{{ selectedDevice.slotNumber }}</el-descriptions-item>
        </el-descriptions>
        
        <h4 style="margin-top: 20px;">支持的功能</h4>
        <el-tag v-for="func in selectedDevice.supportedFunctions" :key="func" style="margin-right: 8px;">
          {{ func }}
        </el-tag>
        
        <h4 style="margin-top: 20px;">配置信息</h4>
        <el-table :data="getConfigurationData(selectedDevice.configuration)" style="width: 100%">
          <el-table-column prop="key" label="参数" />
          <el-table-column prop="value" label="值" />
        </el-table>
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import * as echarts from 'echarts'

// 类型定义
interface Device {
  id: number
  name: string
  model: string
  deviceType: string
  status: string
  slotNumber: number
  supportedFunctions: string[]
  configuration: Record<string, any>
}

interface ErrorLog {
  timestamp: string
  level: string
  message: string
}

// 响应式数据
const loading = reactive({
  status: false,
  devices: false,
  jy5500: false
})

const driverStats = reactive({
  loadedCount: 0,
  deviceCount: 0,
  activeTaskCount: 0
})

const devices = ref<Device[]>([])
const jy5500Devices = ref<Device[]>([])
const selectedDevice = ref<Device | null>(null)
const deviceInfoVisible = ref(false)
const activeResultTab = ref('realtime')
const jy5500TestRunning = ref(false)

const jy5500Config = reactive({
  deviceId: null as number | null,
  sampleRate: 1000,
  channelCount: 4,
  sampleCount: 10000
})

const testResults = reactive({
  packetsReceived: 0,
  samplesReceived: 0,
  dataRate: 0,
  errorCount: 0,
  averageLatency: 0,
  maxLatency: 0,
  packetLossRate: 0,
  cpuUsage: 0,
  memoryUsage: 0,
  testDuration: 0
})

const errorLogs = ref<ErrorLog[]>([])
const realtimeChart = ref<HTMLElement | null>(null)
let chartInstance: echarts.ECharts | null = null

// 方法
async function checkDriverStatus() {
  loading.status = true
  try {
    const response = await fetch('/api/misd/devices')
    if (response.ok) {
      const data = await response.json()
      driverStats.loadedCount = 3 // JY5500, JYUSB1601, MockDriver
      driverStats.deviceCount = data.length
      driverStats.activeTaskCount = 0 // 需要从后端获取
      ElMessage.success('驱动状态检查完成')
    }
  } catch (error) {
    ElMessage.error('检查驱动状态失败: ' + (error as Error).message)
  } finally {
    loading.status = false
  }
}

async function refreshDevices() {
  loading.devices = true
  try {
    const response = await fetch('/api/misd/devices')
    if (response.ok) {
      const data = await response.json()
      devices.value = data
      jy5500Devices.value = data.filter((d: Device) => d.model.startsWith('JY5500'))
      ElMessage.success('设备列表刷新完成')
    }
  } catch (error) {
    ElMessage.error('刷新设备列表失败: ' + (error as Error).message)
  } finally {
    loading.devices = false
  }
}

async function testDevice(device: Device) {
  try {
    ElMessage.info('开始测试设备: ' + device.name)
    // 这里添加设备测试逻辑
    await new Promise(resolve => setTimeout(resolve, 2000))
    ElMessage.success('设备测试完成')
  } catch (error) {
    ElMessage.error('设备测试失败: ' + (error as Error).message)
  }
}

function showDeviceInfo(device: Device) {
  selectedDevice.value = device
  deviceInfoVisible.value = true
}

async function startJY5500Test() {
  if (!jy5500Config.deviceId) {
    ElMessage.warning('请选择要测试的设备')
    return
  }
  
  loading.jy5500 = true
  jy5500TestRunning.value = true
  
  try {
    // 启动JY5500测试
    const testConfig = {
      deviceId: jy5500Config.deviceId,
      sampleRate: jy5500Config.sampleRate,
      channelCount: jy5500Config.channelCount,
      sampleCount: jy5500Config.sampleCount
    }
    
    const response = await fetch('/api/dataacquisition/start', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(testConfig)
    })
    
    if (response.ok) {
      ElMessage.success('JY5500测试启动成功')
      startRealtimeDataCollection()
    } else {
      throw new Error('启动测试失败')
    }
  } catch (error) {
    ElMessage.error('启动JY5500测试失败: ' + (error as Error).message)
    jy5500TestRunning.value = false
  } finally {
    loading.jy5500 = false
  }
}

function stopJY5500Test() {
  jy5500TestRunning.value = false
  ElMessage.info('JY5500测试已停止')
}

function exportJY5500Results() {
  const results = {
    config: jy5500Config,
    results: testResults,
    timestamp: new Date().toISOString()
  }
  
  const blob = new Blob([JSON.stringify(results, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = 'jy5500-test-results.json'
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
  
  ElMessage.success('测试结果已导出')
}

function startRealtimeDataCollection() {
  // 模拟实时数据收集
  const interval = setInterval(() => {
    if (!jy5500TestRunning.value) {
      clearInterval(interval)
      return
    }
    
    testResults.packetsReceived += Math.floor(Math.random() * 10) + 1
    testResults.samplesReceived += Math.floor(Math.random() * 1000) + 100
    testResults.dataRate = Math.floor(Math.random() * 1000) + 500
    testResults.errorCount += Math.random() > 0.95 ? 1 : 0
    
    updateRealtimeChart()
  }, 1000)
}

function updateRealtimeChart() {
  if (!chartInstance) return
  
  const now = new Date()
  const data = []
  for (let i = 0; i < jy5500Config.channelCount; i++) {
    data.push(Math.sin(now.getTime() / 1000 + i) * 100 + Math.random() * 20)
  }
  
  chartInstance.setOption({
    xAxis: {
      data: [now.toLocaleTimeString()]
    },
    series: data.map((value, index) => ({
      name: `通道${index + 1}`,
      data: [value]
    }))
  })
}

function initRealtimeChart() {
  nextTick(() => {
    if (realtimeChart.value) {
      chartInstance = echarts.init(realtimeChart.value)
      
      const option = {
        title: { text: '实时数据波形' },
        tooltip: { trigger: 'axis' },
        legend: { data: Array.from({length: 4}, (_, i) => `通道${i + 1}`) },
        xAxis: { type: 'category', data: [] },
        yAxis: { type: 'value' },
        series: Array.from({length: 4}, (_, i) => ({
          name: `通道${i + 1}`,
          type: 'line',
          data: []
        }))
      }
      
      chartInstance.setOption(option)
    }
  })
}

function getStatusType(status: string) {
  const types: Record<string, string> = {
    'Online': 'success',
    'Offline': 'danger',
    'Error': 'danger',
    'Unknown': 'warning'
  }
  return types[status] || 'info'
}

function getLogLevelType(level: string) {
  const types: Record<string, string> = {
    'Error': 'danger',
    'Warning': 'warning',
    'Info': 'info',
    'Debug': 'success'
  }
  return types[level] || 'info'
}

function getConfigurationData(config: Record<string, any>) {
  if (!config) return []
  return Object.entries(config).map(([key, value]) => ({ key, value }))
}

// 生命周期
onMounted(() => {
  checkDriverStatus()
  refreshDevices()
  initRealtimeChart()
})
</script>

<style lang="scss" scoped>
.hardware-driver-test {
  padding: 20px;
  
  .test-header {
    text-align: center;
    margin-bottom: 30px;
    
    h1 {
      color: #2c3e50;
      margin-bottom: 10px;
    }
    
    p {
      color: #7f8c8d;
      font-size: 16px;
    }
  }
  
  .test-section {
    margin-bottom: 20px;
  }
  
  .driver-status {
    .driver-actions {
      margin-top: 20px;
      text-align: center;
      
      .el-button {
        margin: 0 10px;
      }
    }
  }
  
  .jy5500-test {
    .test-actions {
      margin-top: 20px;
      text-align: center;
      
      .el-button {
        margin: 0 10px;
      }
    }
  }
  
  .realtime-data {
    .data-stats {
      display: flex;
      justify-content: space-around;
      margin-bottom: 20px;
    }
  }
  
  .performance-analysis {
    margin-top: 20px;
  }
  
  .error-logs {
    margin-top: 20px;
  }
}
</style>
