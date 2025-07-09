<template>
  <div class="data-storage-test">
    <el-card class="header-card">
      <template #header>
        <div class="card-header">
          <h2>📦 数据存储和回放系统测试</h2>
          <p>测试高性能数据存储、历史数据查询、数据回放等功能</p>
        </div>
      </template>
    </el-card>

    <el-row :gutter="20">
      <!-- 数据写入测试 -->
      <el-col :span="12">
        <el-card class="test-card">
          <template #header>
            <h3>📝 数据写入测试</h3>
          </template>
          
          <div class="test-section">
            <el-form :model="writeForm" label-width="120px">
              <el-form-item label="任务ID">
                <el-input-number v-model="writeForm.taskId" :min="1" :max="100" />
              </el-form-item>
              <el-form-item label="通道数">
                <el-input-number v-model="writeForm.channelCount" :min="1" :max="32" />
              </el-form-item>
              <el-form-item label="样本数">
                <el-input-number v-model="writeForm.sampleCount" :min="10" :max="10000" />
              </el-form-item>
              <el-form-item label="采样率(Hz)">
                <el-input-number v-model="writeForm.sampleRate" :min="1" :max="100000" />
              </el-form-item>
              <el-form-item label="启用压缩">
                <el-switch v-model="writeForm.compress" />
              </el-form-item>
            </el-form>

            <div class="button-group">
              <el-button type="primary" @click="writeSingleData" :loading="writing">
                写入单个数据包
              </el-button>
              <el-button type="success" @click="writeBatchData" :loading="batchWriting">
                批量写入数据
              </el-button>
              <el-button type="info" @click="generateTestData">
                生成测试数据
              </el-button>
            </div>

            <div v-if="writeResult" class="result-display">
              <h4>写入结果:</h4>
              <pre>{{ JSON.stringify(writeResult, null, 2) }}</pre>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 存储状态监控 -->
      <el-col :span="12">
        <el-card class="test-card">
          <template #header>
            <h3>📊 存储状态监控</h3>
          </template>
          
          <div class="test-section">
            <el-button type="primary" @click="getStorageStatus" :loading="statusLoading">
              刷新存储状态
            </el-button>

            <div v-if="storageStatus" class="status-display">
              <el-descriptions title="存储状态" :column="2" border>
                <el-descriptions-item label="总容量">
                  {{ formatBytes(storageStatus.totalCapacity) }}
                </el-descriptions-item>
                <el-descriptions-item label="已用空间">
                  {{ formatBytes(storageStatus.usedSpace) }}
                </el-descriptions-item>
                <el-descriptions-item label="可用空间">
                  {{ formatBytes(storageStatus.availableSpace) }}
                </el-descriptions-item>
                <el-descriptions-item label="使用率">
                  {{ storageStatus.usagePercentage.toFixed(2) }}%
                </el-descriptions-item>
                <el-descriptions-item label="活跃任务">
                  {{ storageStatus.activeTasks }}
                </el-descriptions-item>
                <el-descriptions-item label="总样本数">
                  {{ storageStatus.totalSamples.toLocaleString() }}
                </el-descriptions-item>
                <el-descriptions-item label="平均压缩率">
                  {{ (storageStatus.averageCompressionRatio * 100).toFixed(1) }}%
                </el-descriptions-item>
                <el-descriptions-item label="写入速度">
                  {{ storageStatus.performance.writeSpeed.toFixed(2) }} MB/s
                </el-descriptions-item>
              </el-descriptions>

              <div v-if="Object.keys(storageStatus.taskInfo).length > 0" class="task-info">
                <h4>任务信息:</h4>
                <el-table :data="Object.values(storageStatus.taskInfo)" style="width: 100%">
                  <el-table-column prop="taskId" label="任务ID" width="80" />
                  <el-table-column prop="taskName" label="任务名称" width="120" />
                  <el-table-column prop="totalBytes" label="总字节数" width="120">
                    <template #default="scope">
                      {{ formatBytes(scope.row.totalBytes) }}
                    </template>
                  </el-table-column>
                  <el-table-column prop="compressionRatio" label="压缩率" width="100">
                    <template #default="scope">
                      {{ (scope.row.compressionRatio * 100).toFixed(1) }}%
                    </template>
                  </el-table-column>
                  <el-table-column prop="channelCount" label="通道数" width="80" />
                </el-table>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20" style="margin-top: 20px;">
      <!-- 历史数据查询 -->
      <el-col :span="12">
        <el-card class="test-card">
          <template #header>
            <h3>🔍 历史数据查询</h3>
          </template>
          
          <div class="test-section">
            <el-form :model="queryForm" label-width="120px">
              <el-form-item label="任务ID">
                <el-input-number v-model="queryForm.taskId" :min="1" :max="100" />
              </el-form-item>
              <el-form-item label="开始时间">
                <el-date-picker
                  v-model="queryForm.startTime"
                  type="datetime"
                  placeholder="选择开始时间"
                  format="YYYY-MM-DD HH:mm:ss"
                  value-format="YYYY-MM-DDTHH:mm:ss"
                />
              </el-form-item>
              <el-form-item label="结束时间">
                <el-date-picker
                  v-model="queryForm.endTime"
                  type="datetime"
                  placeholder="选择结束时间"
                  format="YYYY-MM-DD HH:mm:ss"
                  value-format="YYYY-MM-DDTHH:mm:ss"
                />
              </el-form-item>
              <el-form-item label="通道列表">
                <el-input v-model="queryForm.channels" placeholder="例如: 0,1,2 (空表示所有通道)" />
              </el-form-item>
              <el-form-item label="最大点数">
                <el-input-number v-model="queryForm.maxPoints" :min="100" :max="100000" />
              </el-form-item>
            </el-form>

            <div class="button-group">
              <el-button type="primary" @click="queryHistoricalData" :loading="querying">
                查询历史数据
              </el-button>
              <el-button type="warning" @click="setRecentTimeRange">
                设置最近1小时
              </el-button>
            </div>

            <div v-if="queryResult" class="result-display">
              <h4>查询结果:</h4>
              <el-descriptions :column="2" border>
                <el-descriptions-item label="成功">
                  {{ queryResult.success ? '是' : '否' }}
                </el-descriptions-item>
                <el-descriptions-item label="总点数">
                  {{ queryResult.totalPoints?.toLocaleString() }}
                </el-descriptions-item>
                <el-descriptions-item label="返回点数">
                  {{ queryResult.returnedPoints?.toLocaleString() }}
                </el-descriptions-item>
                <el-descriptions-item label="是否降采样">
                  {{ queryResult.isDownsampled ? '是' : '否' }}
                </el-descriptions-item>
                <el-descriptions-item label="查询时间">
                  {{ queryResult.queryTime?.toFixed(2) }}ms
                </el-descriptions-item>
                <el-descriptions-item label="通道数">
                  {{ Object.keys(queryResult.channels || {}).length }}
                </el-descriptions-item>
              </el-descriptions>

              <div v-if="queryResult.channels" class="chart-container">
                <div ref="chartContainer" style="width: 100%; height: 300px;"></div>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 数据回放测试 -->
      <el-col :span="12">
        <el-card class="test-card">
          <template #header>
            <h3>▶️ 数据回放测试</h3>
          </template>
          
          <div class="test-section">
            <el-form :model="replayForm" label-width="120px">
              <el-form-item label="任务ID">
                <el-input-number v-model="replayForm.taskId" :min="1" :max="100" />
              </el-form-item>
              <el-form-item label="回放速度">
                <el-slider v-model="replayForm.speed" :min="0.1" :max="10" :step="0.1" show-input />
              </el-form-item>
              <el-form-item label="通道列表">
                <el-input v-model="replayForm.channels" placeholder="例如: 0,1,2 (空表示所有通道)" />
              </el-form-item>
            </el-form>

            <div class="button-group">
              <el-button type="success" @click="startReplay" :loading="replaying" :disabled="replaying">
                开始回放
              </el-button>
              <el-button type="danger" @click="stopReplay" :disabled="!replaying">
                停止回放
              </el-button>
            </div>

            <div class="replay-status">
              <el-descriptions title="回放状态" :column="2" border>
                <el-descriptions-item label="状态">
                  <el-tag :type="replaying ? 'success' : 'info'">
                    {{ replaying ? '回放中' : '已停止' }}
                  </el-tag>
                </el-descriptions-item>
                <el-descriptions-item label="接收数据点">
                  {{ replayDataCount.toLocaleString() }}
                </el-descriptions-item>
                <el-descriptions-item label="回放速度">
                  {{ replayForm.speed }}x
                </el-descriptions-item>
                <el-descriptions-item label="最后数据时间">
                  {{ lastReplayTime || '无' }}
                </el-descriptions-item>
              </el-descriptions>

              <div v-if="replayData.length > 0" class="replay-chart">
                <div ref="replayChartContainer" style="width: 100%; height: 250px;"></div>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 操作日志 -->
    <el-card class="log-card" style="margin-top: 20px;">
      <template #header>
        <div class="card-header">
          <h3>📋 操作日志</h3>
          <el-button size="small" @click="clearLogs">清空日志</el-button>
        </div>
      </template>
      
      <div class="log-container">
        <div v-for="(log, index) in logs" :key="index" class="log-item" :class="log.type">
          <span class="log-time">{{ log.time }}</span>
          <span class="log-message">{{ log.message }}</span>
        </div>
      </div>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import * as echarts from 'echarts'

// 响应式数据
const writeForm = reactive({
  taskId: 1,
  channelCount: 4,
  sampleCount: 1000,
  sampleRate: 1000,
  compress: true
})

const queryForm = reactive({
  taskId: 1,
  startTime: '',
  endTime: '',
  channels: '',
  maxPoints: 10000
})

const replayForm = reactive({
  taskId: 1,
  speed: 1.0,
  channels: ''
})

const writing = ref(false)
const batchWriting = ref(false)
const statusLoading = ref(false)
const querying = ref(false)
const replaying = ref(false)

const writeResult = ref<any>(null)
const storageStatus = ref<any>(null)
const queryResult = ref<any>(null)
const replayDataCount = ref(0)
const lastReplayTime = ref('')
const replayData = ref<any[]>([])

const logs = ref<Array<{time: string, message: string, type: string}>>([])

const chartContainer = ref<HTMLElement>()
const replayChartContainer = ref<HTMLElement>()
let chart: echarts.ECharts | null = null
let replayChart: echarts.ECharts | null = null
let replayEventSource: EventSource | null = null

// 工具函数
const formatBytes = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const addLog = (message: string, type: string = 'info') => {
  logs.value.unshift({
    time: new Date().toLocaleTimeString(),
    message,
    type
  })
  if (logs.value.length > 100) {
    logs.value = logs.value.slice(0, 100)
  }
}

const clearLogs = () => {
  logs.value = []
}

// 生成测试数据
const generateTestData = () => {
  const data: any = {
    taskId: writeForm.taskId,
    timestamp: new Date().toISOString(),
    sequenceNumber: Math.floor(Math.random() * 10000),
    channelData: {},
    sampleRate: writeForm.sampleRate,
    sampleCount: writeForm.sampleCount,
    metadata: {
      testData: true,
      generatedAt: new Date().toISOString()
    }
  }

  // 生成多通道数据
  for (let ch = 0; ch < writeForm.channelCount; ch++) {
    const channelData = []
    for (let i = 0; i < writeForm.sampleCount; i++) {
      const t = i / writeForm.sampleRate
      const value = Math.sin(2 * Math.PI * (10 + ch * 5) * t) * (1 + ch * 0.5) + 
                   Math.random() * 0.1 - 0.05
      channelData.push(value)
    }
    data.channelData[ch] = channelData
  }

  return data
}

// 写入单个数据包
const writeSingleData = async () => {
  writing.value = true
  try {
    const testData = generateTestData()
    const response = await fetch(`http://localhost:5152/api/datastorage/write/${writeForm.taskId}?compress=${writeForm.compress}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(testData)
    })

    if (response.ok) {
      writeResult.value = await response.json()
      addLog(`成功写入数据包，压缩率: ${(writeResult.value.compressionRatio * 100).toFixed(1)}%`, 'success')
      ElMessage.success('数据写入成功')
    } else {
      const error = await response.json()
      addLog(`写入失败: ${error.error}`, 'error')
      ElMessage.error('数据写入失败')
    }
  } catch (error) {
    addLog(`写入异常: ${error}`, 'error')
    ElMessage.error('写入异常')
  } finally {
    writing.value = false
  }
}

// 批量写入数据
const writeBatchData = async () => {
  batchWriting.value = true
  try {
    const batchData = []
    for (let i = 0; i < 5; i++) {
      batchData.push(generateTestData())
    }

    const response = await fetch(`http://localhost:5152/api/datastorage/write-batch/${writeForm.taskId}?compress=${writeForm.compress}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(batchData)
    })

    if (response.ok) {
      writeResult.value = await response.json()
      addLog(`批量写入完成，成功: ${writeResult.value.successfulPackets}/${writeResult.value.totalPackets}`, 'success')
      ElMessage.success('批量写入成功')
    } else {
      const error = await response.json()
      addLog(`批量写入失败: ${error.error}`, 'error')
      ElMessage.error('批量写入失败')
    }
  } catch (error) {
    addLog(`批量写入异常: ${error}`, 'error')
    ElMessage.error('批量写入异常')
  } finally {
    batchWriting.value = false
  }
}

// 获取存储状态
const getStorageStatus = async () => {
  statusLoading.value = true
  try {
    const response = await fetch('http://localhost:5152/api/datastorage/status')
    if (response.ok) {
      storageStatus.value = await response.json()
      addLog('存储状态更新成功', 'info')
    } else {
      addLog('获取存储状态失败', 'error')
    }
  } catch (error) {
    addLog(`获取存储状态异常: ${error}`, 'error')
  } finally {
    statusLoading.value = false
  }
}

// 设置最近时间范围
const setRecentTimeRange = () => {
  const now = new Date()
  const oneHourAgo = new Date(now.getTime() - 60 * 60 * 1000)
  
  queryForm.endTime = now.toISOString().slice(0, 19)
  queryForm.startTime = oneHourAgo.toISOString().slice(0, 19)
}

// 查询历史数据
const queryHistoricalData = async () => {
  querying.value = true
  try {
    const params = new URLSearchParams({
      startTime: queryForm.startTime,
      endTime: queryForm.endTime,
      maxPoints: queryForm.maxPoints.toString()
    })
    
    if (queryForm.channels) {
      params.append('channels', queryForm.channels)
    }

    const response = await fetch(`http://localhost:5152/api/datastorage/query/${queryForm.taskId}?${params}`)
    if (response.ok) {
      queryResult.value = await response.json()
      addLog(`查询完成，返回 ${queryResult.value.returnedPoints} 个数据点`, 'success')
      
      // 绘制图表
      await nextTick()
      if (chartContainer.value && queryResult.value.channels) {
        drawChart()
      }
    } else {
      const error = await response.json()
      addLog(`查询失败: ${error.error}`, 'error')
    }
  } catch (error) {
    addLog(`查询异常: ${error}`, 'error')
  } finally {
    querying.value = false
  }
}

// 绘制图表
const drawChart = () => {
  if (!chartContainer.value || !queryResult.value?.channels) return

  if (chart) {
    chart.dispose()
  }

  chart = echarts.init(chartContainer.value)
  
  const series: any[] = []
  const channels = queryResult.value.channels

  Object.keys(channels).forEach((channelId) => {
    const channel = channels[channelId]
    const data = channel.timestamps.map((time: number, index: number) => [
      time * 1000, // 转换为毫秒
      channel.values[index]
    ])

    series.push({
      name: `通道 ${channelId}`,
      type: 'line',
      data: data,
      symbol: 'none',
      lineStyle: { width: 1 }
    })
  })

  const option = {
    title: {
      text: '历史数据查询结果',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'cross'
      }
    },
    legend: {
      top: 30
    },
    xAxis: {
      type: 'time',
      name: '时间'
    },
    yAxis: {
      type: 'value',
      name: '数值'
    },
    series: series,
    dataZoom: [
      {
        type: 'inside',
        xAxisIndex: 0
      },
      {
        type: 'slider',
        xAxisIndex: 0,
        height: 20,
        bottom: 10
      }
    ]
  }

  chart.setOption(option)
}

// 开始回放
const startReplay = async () => {
  if (replaying.value) return

  replaying.value = true
  replayDataCount.value = 0
  replayData.value = []

  try {
    const params = new URLSearchParams({
      startTime: queryForm.startTime || new Date(Date.now() - 60 * 60 * 1000).toISOString(),
      endTime: queryForm.endTime || new Date().toISOString(),
      speed: replayForm.speed.toString()
    })

    if (replayForm.channels) {
      params.append('channels', replayForm.channels)
    }

    const url = `http://localhost:5152/api/datastorage/replay/${replayForm.taskId}?${params}`
    replayEventSource = new EventSource(url)

    replayEventSource.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data)
        
        if (data.type === 'end') {
          addLog('数据回放完成', 'success')
          stopReplay()
          return
        }

        if (data.error) {
          addLog(`回放错误: ${data.error}`, 'error')
          stopReplay()
          return
        }

        replayDataCount.value++
        lastReplayTime.value = new Date(data.timestamp).toLocaleTimeString()
        
        // 保存最近的数据用于图表显示
        replayData.value.push(data)
        if (replayData.value.length > 1000) {
          replayData.value = replayData.value.slice(-1000)
        }

        // 更新回放图表
        updateReplayChart()
      } catch (error) {
        console.error('解析回放数据失败:', error)
      }
    }

    replayEventSource.onerror = (error) => {
      addLog('回放连接错误', 'error')
      stopReplay()
    }

    addLog(`开始回放数据，速度: ${replayForm.speed}x`, 'info')
  } catch (error) {
    addLog(`回放启动失败: ${error}`, 'error')
    replaying.value = false
  }
}

// 停止回放
const stopReplay = () => {
  if (replayEventSource) {
    replayEventSource.close()
    replayEventSource = null
  }
  replaying.value = false
  addLog('数据回放已停止', 'info')
}

// 更新回放图表
const updateReplayChart = () => {
  if (!replayChartContainer.value || replayData.value.length === 0) return

  if (!replayChart) {
    replayChart = echarts.init(replayChartContainer.value)
  }

  // 按通道组织数据
  const channelData: { [key: number]: Array<[number, number]> } = {}
  
  replayData.value.forEach(point => {
    if (!channelData[point.channelId]) {
      channelData[point.channelId] = []
    }
    channelData[point.channelId].push([
      new Date(point.timestamp).getTime(),
      point.value
    ])
  })

  const series = Object.keys(channelData).map(channelId => ({
    name: `通道 ${channelId}`,
    type: 'line',
    data: channelData[parseInt(channelId)],
    symbol: 'none',
    lineStyle: { width: 1 }
  }))

  const option = {
    title: {
      text: '实时回放数据',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    legend: {
      top: 30
    },
    xAxis: {
      type: 'time',
      name: '时间'
    },
    yAxis: {
      type: 'value',
      name: '数值'
    },
    series: series
  }

  replayChart.setOption(option)
}

// 生命周期
onMounted(() => {
  // 设置默认时间范围
  setRecentTimeRange()
  
  // 获取初始存储状态
  getStorageStatus()
})

onUnmounted(() => {
  if (chart) {
    chart.dispose()
  }
  if (replayChart) {
    replayChart.dispose()
  }
  stopReplay()
})
</script>

<style scoped>
.data-storage-test {
  padding: 20px;
}

.header-card .card-header {
  text-align: center;
}

.header-card h2 {
  margin: 0 0 10px 0;
  color: #409eff;
}

.header-card p {
  margin: 0;
  color: #666;
}

.test-card {
  height: 100%;
}

.test-section {
  padding: 10px 0;
}

.button-group {
  margin: 20px 0;
  text-align: center;
}

.button-group .el-button {
  margin: 0 5px;
}

.result-display {
  margin-top: 20px;
  padding: 15px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.result-display h4 {
  margin: 0 0 10px 0;
  color: #303133;
}

.result-display pre {
  background-color: #fff;
  padding: 10px;
  border-radius: 4px;
  border: 1px solid #dcdfe6;
  font-size: 12px;
  max-height: 200px;
  overflow-y: auto;
}

.status-display {
  margin-top: 20px;
}

.task-info {
  margin-top: 20px;
}

.task-info h4 {
  margin: 0 0 10px 0;
  color: #303133;
}

.chart-container {
  margin-top: 20px;
}

.replay-status {
  margin-top: 20px;
}

.replay-chart {
  margin-top: 15px;
}

.log-card .card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.log-container {
  max-height: 300px;
  overflow-y: auto;
  font-family: 'Courier New', monospace;
  font-size: 12px;
}

.log-item {
  padding: 5px 0;
  border-bottom: 1px solid #f0f0f0;
}

.log-item:last-child {
  border-bottom: none;
}

.log-time {
  color: #999;
  margin-right: 10px;
}

.log-message {
  color: #333;
}

.log-item.success .log-message {
  color: #67c23a;
}

.log-item.error .log-message {
  color: #f56c6c;
}

.log-item.warning .log-message {
  color: #e6a23c;
}

.log-item.info .log-message {
  color: #409eff;
}
</style>
