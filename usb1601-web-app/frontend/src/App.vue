<template>
  <div id="app">
    <el-container class="main-container glass-card">
      <!-- 头部 -->
      <el-header height="80px" class="app-header">
        <div class="header-left">
          <el-icon size="32" color="#409EFF"><Cpu /></el-icon>
          <div class="header-title">
            <h1>USB-1601 智能数据采集系统</h1>
            <p>集成百度AI大模型的实时数据分析平台</p>
          </div>
        </div>
        <div class="header-right">
          <el-tag :type="connectionStatus.type" effect="dark" size="large">
            <el-icon><component :is="connectionStatus.icon" /></el-icon>
            {{ connectionStatus.text }}
          </el-tag>
        </div>
      </el-header>

      <el-container>
        <!-- 侧边栏 - 控制面板 -->
        <el-aside width="350px" class="control-sidebar">
          <!-- 硬件控制 -->
          <el-card class="control-card">
            <template #header>
              <div class="card-header">
                <el-icon><Setting /></el-icon>
                <span>硬件控制</span>
              </div>
            </template>
            
            <el-form :model="config" label-width="100px">
              <el-form-item label="通道数">
                <el-select v-model="config.channelCount" :disabled="isAcquiring">
                  <el-option :label="1" :value="1" />
                  <el-option :label="2" :value="2" />
                  <el-option :label="4" :value="4" />
                  <el-option :label="8" :value="8" />
                </el-select>
              </el-form-item>
              
              <el-form-item label="采样率">
                <el-select v-model="config.sampleRate" :disabled="isAcquiring">
                  <el-option label="1 kHz" :value="1000" />
                  <el-option label="10 kHz" :value="10000" />
                  <el-option label="50 kHz" :value="50000" />
                  <el-option label="100 kHz" :value="100000" />
                </el-select>
              </el-form-item>
              
              <el-form-item label="电压范围">
                <el-select v-model="config.voltageRange" :disabled="isAcquiring">
                  <el-option label="±10V" :value="10" />
                  <el-option label="±5V" :value="5" />
                  <el-option label="±2V" :value="2" />
                  <el-option label="±1V" :value="1" />
                </el-select>
              </el-form-item>
              
              <el-form-item label="自测模式">
                <el-switch v-model="config.selfTestMode" :disabled="isAcquiring" />
              </el-form-item>
              
              <el-form-item label="测试信号" v-if="config.selfTestMode">
                <el-select v-model="config.signalType" :disabled="isAcquiring">
                  <el-option label="正弦波" value="Sine" />
                  <el-option label="方波" value="Square" />
                  <el-option label="三角波" value="Triangle" />
                  <el-option label="锯齿波" value="Sawtooth" />
                </el-select>
              </el-form-item>
              
              <el-form-item label="信号频率" v-if="config.selfTestMode">
                <el-input-number 
                  v-model="config.signalFrequency" 
                  :min="1" 
                  :max="10000" 
                  :step="10"
                  :disabled="isAcquiring"
                />
                <span style="margin-left: 8px">Hz</span>
              </el-form-item>
              
              <el-form-item>
                <el-button 
                  type="primary" 
                  @click="toggleAcquisition"
                  :loading="loading"
                  size="large"
                  style="width: 100%"
                >
                  <el-icon><component :is="isAcquiring ? 'VideoPause' : 'VideoPlay'" /></el-icon>
                  {{ isAcquiring ? '停止采集' : '开始采集' }}
                </el-button>
              </el-form-item>
            </el-form>
          </el-card>

          <!-- AI分析 -->
          <el-card class="control-card">
            <template #header>
              <div class="card-header">
                <el-icon><MagicStick /></el-icon>
                <span>AI智能分析</span>
              </div>
            </template>
            
            <el-button @click="showAIPanel = true" type="success" style="width: 100%; margin-bottom: 12px">
              <el-icon><Promotion /></el-icon>
              打开 ERNIE-4.5 AI分析面板
            </el-button>
            
            <el-button @click="analyzeWaveform" :loading="analyzing" style="width: 100%; margin-bottom: 12px">
              <el-icon><DataAnalysis /></el-icon>
              快速分析波形
            </el-button>
            
            <el-button @click="detectAnomalies" :loading="detecting" style="width: 100%; margin-bottom: 12px">
              <el-icon><Warning /></el-icon>
              异常检测
            </el-button>
            
            <el-button @click="generateReport" :loading="generating" style="width: 100%">
              <el-icon><Document /></el-icon>
              生成报告
            </el-button>
            
            <!-- AI分析结果 -->
            <div v-if="aiResult" class="ai-result">
              <el-divider>分析结果</el-divider>
              <div class="result-content">
                <p><strong>信号类型:</strong> {{ aiResult.signalType }}</p>
                <p><strong>分析:</strong></p>
                <p class="analysis-text">{{ aiResult.analysis }}</p>
              </div>
            </div>
          </el-card>
        </el-aside>

        <!-- 主内容区 -->
        <el-main class="main-content">
          <!-- 波形显示 -->
          <el-card class="waveform-card">
            <template #header>
              <div class="card-header">
                <el-icon><Monitor /></el-icon>
                <span>实时波形</span>
                <el-tag style="margin-left: auto">{{ dataStats.sampleCount }} 采样点</el-tag>
              </div>
            </template>
            
            <!-- 使用优化的波形组件 -->
            <OptimizedWaveform 
              ref="waveformRef"
              height="400px"
              :max-points="2000"
              :update-interval="100"
            />
          </el-card>

          <!-- 数据统计 -->
          <el-row :gutter="20" style="margin-top: 20px">
            <el-col :span="6">
              <el-card class="stat-card">
                <el-statistic title="最大值" :value="dataStats.max" suffix="V" />
              </el-card>
            </el-col>
            <el-col :span="6">
              <el-card class="stat-card">
                <el-statistic title="最小值" :value="dataStats.min" suffix="V" />
              </el-card>
            </el-col>
            <el-col :span="6">
              <el-card class="stat-card">
                <el-statistic title="平均值" :value="dataStats.mean" suffix="V" />
              </el-card>
            </el-col>
            <el-col :span="6">
              <el-card class="stat-card">
                <el-statistic title="峰峰值" :value="dataStats.pp" suffix="V" />
              </el-card>
            </el-col>
          </el-row>
        </el-main>
      </el-container>
    </el-container>
    
    <!-- AI分析面板对话框 -->
    <el-dialog 
      v-model="showAIPanel" 
      title="ERNIE-4.5 AI智能分析面板" 
      width="90%"
      fullscreen
      destroy-on-close
    >
      <AIAnalysis />
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
import { ElMessage, ElNotification } from 'element-plus'
import WaveformDisplay from './components/WaveformDisplay.vue'
import OptimizedWaveform from './components/OptimizedWaveform.vue'
import AIAnalysis from './components/AIAnalysis.vue'
import { dataService } from './services/dataService'
import { aiService } from './services/aiService'
import { signalRService } from './services/signalRService'

// 组件引用
const waveformRef = ref()

// AI面板显示状态
const showAIPanel = ref(false)

// 状态管理
const isConnected = ref(false)
const isAcquiring = ref(false)
const loading = ref(false)
const analyzing = ref(false)
const detecting = ref(false)
const generating = ref(false)

// 配置
const config = reactive({
  channelCount: 1,
  sampleRate: 1000,
  voltageRange: 10,
  selfTestMode: false,
  signalType: 'Sine',
  signalFrequency: 100,
  signalAmplitude: 5
})

// 数据统计
const dataStats = reactive({
  max: 0,
  min: 0,
  mean: 0,
  pp: 0,
  sampleCount: 0
})

// AI分析结果
const aiResult = ref(null)

// 连接状态
const connectionStatus = computed(() => {
  if (isConnected.value) {
    return {
      type: 'success',
      text: '已连接',
      icon: 'Connection'
    }
  } else {
    return {
      type: 'danger',
      text: '未连接',
      icon: 'Disconnect'
    }
  }
})

// 初始化连接
const initConnection = async () => {
  try {
    // 初始化硬件
    await dataService.initialize()
    
    // 连接SignalR
    await signalRService.connect()
    
    // 订阅数据接收事件
    signalRService.onDataReceived((data: any) => {
      if (waveformRef.value) {
        waveformRef.value.updateChart(data.data)
        updateStats(data.data)
      }
    })
    
    isConnected.value = true
    ElMessage.success('系统初始化成功')
  } catch (error) {
    console.error('初始化失败:', error)
    ElMessage.error('系统初始化失败，请检查硬件连接')
  }
}

// 切换采集状态
const toggleAcquisition = async () => {
  loading.value = true
  
  try {
    if (isAcquiring.value) {
      // 停止采集
      await dataService.stopAcquisition()
      await signalRService.stopDataStream()
      isAcquiring.value = false
      ElMessage.info('数据采集已停止')
    } else {
      // 开始采集
      const startConfig = {
        channelCount: config.channelCount,
        sampleRate: config.sampleRate,
        minVoltage: -config.voltageRange,
        maxVoltage: config.voltageRange,
        selfTestMode: config.selfTestMode,
        signalType: config.signalType,
        signalFrequency: config.signalFrequency,
        signalAmplitude: config.signalAmplitude
      }
      
      await dataService.startAcquisition(startConfig)
      await signalRService.startDataStream(config.channelCount, config.sampleRate)
      isAcquiring.value = true
      ElMessage.success('数据采集已启动')
    }
  } catch (error) {
    console.error('操作失败:', error)
    ElMessage.error('操作失败，请重试')
  } finally {
    loading.value = false
  }
}

// 更新数据统计
const updateStats = (data: number[]) => {
  if (data.length === 0) return
  
  dataStats.max = Math.max(...data)
  dataStats.min = Math.min(...data)
  dataStats.mean = data.reduce((a, b) => a + b, 0) / data.length
  dataStats.pp = dataStats.max - dataStats.min
  dataStats.sampleCount += data.length
}

// AI分析波形
const analyzeWaveform = async () => {
  analyzing.value = true
  
  try {
    // 获取当前数据（这里需要从waveform组件获取）
    const result = await aiService.analyzeWaveform({
      data: [], // 需要实现数据获取
      sampleRate: config.sampleRate,
      channelCount: config.channelCount
    })
    
    aiResult.value = result
    ElNotification({
      title: 'AI分析完成',
      message: `检测到${result.signalType}信号`,
      type: 'success'
    })
  } catch (error) {
    console.error('分析失败:', error)
    ElMessage.error('AI分析失败')
  } finally {
    analyzing.value = false
  }
}

// 异常检测
const detectAnomalies = async () => {
  detecting.value = true
  
  try {
    const result = await aiService.detectAnomalies({
      currentData: [],
      baselineData: []
    })
    
    if (result.hasAnomaly) {
      ElNotification({
        title: '检测到异常',
        message: result.description,
        type: 'warning',
        duration: 0
      })
    } else {
      ElMessage.success('未检测到异常')
    }
  } catch (error) {
    console.error('检测失败:', error)
    ElMessage.error('异常检测失败')
  } finally {
    detecting.value = false
  }
}

// 生成报告
const generateReport = async () => {
  generating.value = true
  
  try {
    const report = await aiService.generateReport()
    
    // 下载报告
    const blob = new Blob([report.content], { type: 'text/markdown' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `测试报告_${new Date().toISOString()}.md`
    a.click()
    
    ElMessage.success('报告生成成功')
  } catch (error) {
    console.error('生成失败:', error)
    ElMessage.error('报告生成失败')
  } finally {
    generating.value = false
  }
}

// 生命周期
onMounted(() => {
  initConnection()
})

onUnmounted(() => {
  if (isAcquiring.value) {
    dataService.stopAcquisition()
  }
  signalRService.disconnect()
})
</script>

<style scoped>
.main-container {
  max-width: 1600px;
  margin: 0 auto;
  min-height: calc(100vh - 40px);
}

.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 24px;
  border-bottom: 1px solid #e4e7ed;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.header-title h1 {
  font-size: 24px;
  margin: 0;
  color: #303133;
}

.header-title p {
  font-size: 14px;
  color: #909399;
  margin: 4px 0 0 0;
}

.control-sidebar {
  background: #f5f7fa;
  padding: 20px;
  overflow-y: auto;
}

.control-card {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
}

.main-content {
  padding: 20px;
}

.waveform-card {
  margin-bottom: 20px;
}

.stat-card {
  text-align: center;
}

.ai-result {
  margin-top: 20px;
  padding: 12px;
  background: #f0f9ff;
  border-radius: 8px;
}

.analysis-text {
  margin-top: 8px;
  line-height: 1.6;
  color: #606266;
  white-space: pre-wrap;
}
</style>