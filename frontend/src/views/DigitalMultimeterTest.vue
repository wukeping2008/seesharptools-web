<template>
  <div class="digital-multimeter-test">
    <!-- 页面标题 -->
    <div class="page-header">
      <div class="header-content">
        <div class="title-section">
          <h1 class="page-title">
            <el-icon><Monitor /></el-icon>
            数字万用表测试
          </h1>
          <p class="page-description">
            专业数字万用表控件演示，支持8种测量功能、自动量程、统计分析等专业特性
          </p>
        </div>
        <div class="header-actions">
          <el-button @click="showHelp">
            <el-icon><QuestionFilled /></el-icon>
            使用说明
          </el-button>
          <el-button @click="exportData">
            <el-icon><Download /></el-icon>
            导出数据
          </el-button>
        </div>
      </div>
    </div>

    <!-- 主要内容区域 -->
    <div class="test-content">
      <!-- 万用表控件 -->
      <div class="multimeter-section">
        <DigitalMultimeter
          :width="'100%'"
          :height="800"
        />
      </div>

      <!-- 测试控制面板 -->
      <div class="test-panel">
        <el-card class="test-card" shadow="hover">
          <template #header>
            <div class="card-header">
              <el-icon><Setting /></el-icon>
              <span>测试控制</span>
            </div>
          </template>
          
          <div class="test-controls">
            <!-- 功能测试 -->
            <div class="control-group">
              <label class="group-label">功能测试</label>
              <div class="function-buttons">
                <el-button 
                  v-for="func in testFunctions" 
                  :key="func.value"
                  @click="testFunction(func)"
                  size="small"
                  :type="func.type"
                >
                  {{ func.label }}
                </el-button>
              </div>
            </div>

            <!-- 测试场景 -->
            <div class="control-group">
              <label class="group-label">测试场景</label>
              <div class="scenario-buttons">
                <el-button @click="runAccuracyTest" size="small" type="primary">
                  精度测试
                </el-button>
                <el-button @click="runStabilityTest" size="small" type="success">
                  稳定性测试
                </el-button>
                <el-button @click="runRangeTest" size="small" type="warning">
                  量程测试
                </el-button>
                <el-button @click="runSpeedTest" size="small" type="info">
                  速度测试
                </el-button>
              </div>
            </div>

            <!-- 实时监控 -->
            <div class="control-group">
              <label class="group-label">实时监控</label>
              <div class="monitor-grid">
                <div class="monitor-item">
                  <div class="monitor-label">测量状态</div>
                  <div class="monitor-value" :class="{ active: isRunning }">
                    {{ isRunning ? '运行中' : '停止' }}
                  </div>
                </div>
                <div class="monitor-item">
                  <div class="monitor-label">测量速率</div>
                  <div class="monitor-value">{{ measurementRate }} 次/秒</div>
                </div>
                <div class="monitor-item">
                  <div class="monitor-label">数据点数</div>
                  <div class="monitor-value">{{ dataPoints }}</div>
                </div>
                <div class="monitor-item">
                  <div class="monitor-label">运行时间</div>
                  <div class="monitor-value">{{ formatRunTime(runTime) }}</div>
                </div>
              </div>
            </div>

            <!-- 测试日志 -->
            <div class="control-group">
              <label class="group-label">测试日志</label>
              <div class="log-container">
                <div 
                  v-for="(log, index) in testLogs" 
                  :key="index"
                  class="log-item"
                  :class="log.type"
                >
                  <span class="log-time">{{ formatTime(log.timestamp) }}</span>
                  <span class="log-message">{{ log.message }}</span>
                </div>
              </div>
              <div class="log-actions">
                <el-button @click="clearLogs" size="small">清空日志</el-button>
                <el-button @click="exportLogs" size="small">导出日志</el-button>
              </div>
            </div>
          </div>
        </el-card>
      </div>
    </div>

    <!-- 技术说明对话框 -->
    <el-dialog
      v-model="helpDialogVisible"
      title="数字万用表使用说明"
      width="800px"
      :before-close="closeHelp"
    >
      <div class="help-content">
        <h3>功能特性</h3>
        <ul>
          <li><strong>8种测量功能</strong>：DC/AC电压、DC/AC电流、电阻、电容、频率、温度</li>
          <li><strong>自动量程</strong>：智能选择最佳测量量程，也支持手动量程设置</li>
          <li><strong>数据处理</strong>：数据保持、相对测量、最值记录功能</li>
          <li><strong>统计分析</strong>：实时计算平均值、最值、标准差等统计信息</li>
          <li><strong>历史图表</strong>：集成趋势图表，可视化测量数据变化</li>
          <li><strong>数据导出</strong>：支持CSV格式的测量数据导出</li>
        </ul>

        <h3>操作指南</h3>
        <ol>
          <li><strong>选择功能</strong>：点击相应的测量功能按钮（DCV、ACV、DCI等）</li>
          <li><strong>设置量程</strong>：选择自动量程或手动设置特定量程</li>
          <li><strong>开始测量</strong>：点击"开始测量"按钮开始连续测量</li>
          <li><strong>数据处理</strong>：使用保持、相对、最值等功能处理测量数据</li>
          <li><strong>查看统计</strong>：在统计面板查看详细的统计信息</li>
          <li><strong>导出数据</strong>：点击导出按钮保存测量数据</li>
        </ol>

        <h3>技术规格</h3>
        <table class="spec-table">
          <tr><td>DC电压</td><td>200mV - 1000V</td></tr>
          <tr><td>AC电压</td><td>200mV - 750V</td></tr>
          <tr><td>DC电流</td><td>200μA - 10A</td></tr>
          <tr><td>AC电流</td><td>200μA - 10A</td></tr>
          <tr><td>电阻</td><td>200Ω - 20MΩ</td></tr>
          <tr><td>电容</td><td>2nF - 200μF</td></tr>
          <tr><td>频率</td><td>自动量程</td></tr>
          <tr><td>温度</td><td>自动量程</td></tr>
        </table>

        <h3>应用场景</h3>
        <ul>
          <li><strong>电路调试</strong>：测量电路中的电压、电流、电阻等参数</li>
          <li><strong>元件测试</strong>：测试电阻、电容等电子元件的参数</li>
          <li><strong>系统监控</strong>：长期监控系统的电气参数变化</li>
          <li><strong>质量检测</strong>：产品质量检测和参数验证</li>
          <li><strong>教学演示</strong>：电子技术教学和实验演示</li>
        </ul>
      </div>
      <template #footer>
        <el-button @click="closeHelp">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted } from 'vue'
import { Monitor, QuestionFilled, Download, Setting } from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import DigitalMultimeter from '@/components/instruments/DigitalMultimeter.vue'

// 响应式数据
const helpDialogVisible = ref(false)
const isRunning = ref(false)
const measurementRate = ref(10)
const dataPoints = ref(0)
const runTime = ref(0)

// 测试日志
interface TestLog {
  timestamp: Date
  type: 'info' | 'success' | 'warning' | 'error'
  message: string
}

const testLogs = ref<TestLog[]>([])

// 测试功能
const testFunctions = [
  { label: 'DC电压测试', value: 'dcv', type: 'primary' },
  { label: 'AC电压测试', value: 'acv', type: 'success' },
  { label: 'DC电流测试', value: 'dci', type: 'warning' },
  { label: 'AC电流测试', value: 'aci', type: 'info' },
  { label: '电阻测试', value: 'res', type: 'primary' },
  { label: '电容测试', value: 'cap', type: 'success' },
  { label: '频率测试', value: 'freq', type: 'warning' },
  { label: '温度测试', value: 'temp', type: 'info' }
]

// 定时器
let runTimer: number | null = null

// 方法
const testFunction = (func: any) => {
  addLog('info', `开始${func.label}...`)
  
  // 模拟测试过程
  setTimeout(() => {
    const success = Math.random() > 0.1 // 90% 成功率
    if (success) {
      addLog('success', `${func.label}完成 - 测试通过`)
    } else {
      addLog('error', `${func.label}失败 - 请检查连接`)
    }
  }, 1000)
}

const runAccuracyTest = async () => {
  addLog('info', '开始精度测试...')
  
  const testPoints = [
    { name: 'DC 1V', expected: 1.000, tolerance: 0.001 },
    { name: 'DC 5V', expected: 5.000, tolerance: 0.005 },
    { name: 'AC 230V', expected: 230.0, tolerance: 0.5 },
    { name: '1kΩ', expected: 1000, tolerance: 1 }
  ]
  
  for (const point of testPoints) {
    await new Promise(resolve => setTimeout(resolve, 1500))
    const measured = point.expected + (Math.random() - 0.5) * point.tolerance * 2
    const error = Math.abs(measured - point.expected)
    const passed = error <= point.tolerance
    
    addLog(
      passed ? 'success' : 'error',
      `${point.name}: 测量值=${measured.toFixed(3)}, 误差=${error.toFixed(3)}, ${passed ? '通过' : '失败'}`
    )
  }
  
  addLog('success', '精度测试完成')
  ElMessage.success('精度测试完成')
}

const runStabilityTest = async () => {
  addLog('info', '开始稳定性测试...')
  
  let testCount = 0
  const maxTests = 20
  const values: number[] = []
  
  const stabilityTimer = setInterval(() => {
    const value = 5.000 + (Math.random() - 0.5) * 0.002 // ±1mV变化
    values.push(value)
    testCount++
    
    if (testCount >= maxTests) {
      clearInterval(stabilityTimer)
      
      const avg = values.reduce((a, b) => a + b) / values.length
      const stdDev = Math.sqrt(values.reduce((a, b) => a + (b - avg) ** 2, 0) / values.length)
      
      addLog('info', `稳定性测试结果: 平均值=${avg.toFixed(6)}V, 标准差=${stdDev.toFixed(6)}V`)
      addLog(stdDev < 0.001 ? 'success' : 'warning', `稳定性${stdDev < 0.001 ? '良好' : '一般'}`)
      ElMessage.success('稳定性测试完成')
    }
  }, 200)
  
  addLog('info', `执行${maxTests}次稳定性测量...`)
}

const runRangeTest = async () => {
  addLog('info', '开始量程测试...')
  
  const ranges = [
    { name: '200mV量程', value: 0.1 },
    { name: '2V量程', value: 1.5 },
    { name: '20V量程', value: 15 },
    { name: '200V量程', value: 150 }
  ]
  
  for (const range of ranges) {
    await new Promise(resolve => setTimeout(resolve, 1000))
    addLog('info', `测试${range.name}: ${range.value}V`)
    
    // 模拟量程切换和测量
    setTimeout(() => {
      addLog('success', `${range.name}测试通过`)
    }, 500)
  }
  
  addLog('success', '量程测试完成')
  ElMessage.success('量程测试完成')
}

const runSpeedTest = async () => {
  addLog('info', '开始速度测试...')
  
  const speeds = [
    { name: '快速模式', rate: 100 },
    { name: '中等模式', rate: 10 },
    { name: '慢速模式', rate: 1 }
  ]
  
  for (const speed of speeds) {
    await new Promise(resolve => setTimeout(resolve, 1000))
    addLog('info', `测试${speed.name}: ${speed.rate}次/秒`)
    
    // 模拟速度测试
    let count = 0
    const speedTimer = setInterval(() => {
      count++
      if (count >= speed.rate) {
        clearInterval(speedTimer)
        addLog('success', `${speed.name}测试完成 - 实际速率: ${speed.rate}次/秒`)
      }
    }, 1000 / speed.rate)
  }
  
  addLog('success', '速度测试完成')
  ElMessage.success('速度测试完成')
}

const addLog = (type: TestLog['type'], message: string) => {
  testLogs.value.unshift({
    timestamp: new Date(),
    type,
    message
  })
  
  // 限制日志数量
  if (testLogs.value.length > 100) {
    testLogs.value = testLogs.value.slice(0, 100)
  }
}

const clearLogs = () => {
  testLogs.value = []
  ElMessage.success('日志已清空')
}

const exportLogs = () => {
  const logText = testLogs.value
    .map(log => `[${formatTime(log.timestamp)}] ${log.type.toUpperCase()}: ${log.message}`)
    .join('\n')
  
  const blob = new Blob([logText], { type: 'text/plain' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `dmm-test-logs-${Date.now()}.txt`
  a.click()
  URL.revokeObjectURL(url)
  
  ElMessage.success('日志已导出')
}

const exportData = () => {
  ElMessage.info('数据导出功能请在万用表控件中使用')
}

const showHelp = () => {
  helpDialogVisible.value = true
}

const closeHelp = () => {
  helpDialogVisible.value = false
}

const formatTime = (date: Date): string => {
  return date.toLocaleTimeString()
}

const formatRunTime = (seconds: number): string => {
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  const secs = seconds % 60
  
  if (hours > 0) {
    return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
  } else {
    return `${minutes}:${secs.toString().padStart(2, '0')}`
  }
}

// 生命周期
onMounted(() => {
  addLog('info', '数字万用表测试页面已加载')
  
  // 启动运行时间计时器
  runTimer = setInterval(() => {
    if (isRunning.value) {
      runTime.value++
      dataPoints.value++
    }
  }, 1000)
  
  // 模拟运行状态
  isRunning.value = true
})

onUnmounted(() => {
  if (runTimer) {
    clearInterval(runTimer)
    runTimer = null
  }
})
</script>

<style lang="scss" scoped>
.digital-multimeter-test {
  min-height: 100vh;
  background: var(--background-color);
}

.page-header {
  background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
  color: white;
  padding: 32px 0;
  
  .header-content {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 24px;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  
  .title-section {
    .page-title {
      display: flex;
      align-items: center;
      gap: 12px;
      margin: 0 0 8px 0;
      font-size: 28px;
      font-weight: 700;
    }
    
    .page-description {
      margin: 0;
      font-size: 16px;
      opacity: 0.9;
      line-height: 1.5;
    }
  }
  
  .header-actions {
    display: flex;
    gap: 12px;
  }
}

.test-content {
  max-width: 1400px;
  margin: 0 auto;
  padding: 24px;
  display: grid;
  grid-template-columns: 1fr 400px;
  gap: 24px;
}

.multimeter-section {
  .digital-multimeter {
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }
}

.test-panel {
  .test-card {
    height: fit-content;
    
    .card-header {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 600;
    }
  }
}

.test-controls {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.control-group {
  .group-label {
    display: block;
    font-size: 14px;
    font-weight: 600;
    color: var(--text-primary);
    margin-bottom: 12px;
  }
  
  .function-buttons,
  .scenario-buttons {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }
  
  .monitor-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
    
    .monitor-item {
      padding: 12px;
      background: var(--surface-color);
      border-radius: 8px;
      border: 1px solid var(--border-color);
      
      .monitor-label {
        font-size: 12px;
        color: var(--text-secondary);
        margin-bottom: 4px;
      }
      
      .monitor-value {
        font-size: 14px;
        font-weight: 600;
        color: var(--text-primary);
        
        &.active {
          color: #10b981;
        }
      }
    }
  }
  
  .log-container {
    max-height: 200px;
    overflow-y: auto;
    border: 1px solid var(--border-color);
    border-radius: 8px;
    background: var(--surface-color);
    
    .log-item {
      padding: 8px 12px;
      border-bottom: 1px solid var(--border-color);
      font-size: 13px;
      display: flex;
      gap: 12px;
      
      &:last-child {
        border-bottom: none;
      }
      
      .log-time {
        color: var(--text-secondary);
        min-width: 80px;
      }
      
      .log-message {
        flex: 1;
      }
      
      &.info {
        .log-message {
          color: var(--text-primary);
        }
      }
      
      &.success {
        .log-message {
          color: #10b981;
        }
      }
      
      &.warning {
        .log-message {
          color: #f59e0b;
        }
      }
      
      &.error {
        .log-message {
          color: #ef4444;
        }
      }
    }
  }
  
  .log-actions {
    display: flex;
    gap: 8px;
    margin-top: 8px;
  }
}

.help-content {
  h3 {
    color: var(--text-primary);
    margin: 20px 0 12px 0;
    font-size: 16px;
    
    &:first-child {
      margin-top: 0;
    }
  }
  
  ul, ol {
    margin: 0 0 16px 0;
    padding-left: 20px;
    
    li {
      margin-bottom: 8px;
      line-height: 1.5;
      
      strong {
        color: var(--text-primary);
      }
    }
  }
  
  .spec-table {
    width: 100%;
    border-collapse: collapse;
    margin: 16px 0;
    
    td {
      padding: 8px 12px;
      border: 1px solid var(--border-color);
      
      &:first-child {
        background: var(--surface-color);
        font-weight: 600;
        width: 30%;
      }
    }
  }
}

// 响应式设计
@media (max-width: 1200px) {
  .test-content {
    grid-template-columns: 1fr;
    grid-template-rows: auto auto;
  }
  
  .multimeter-section {
    order: 2;
  }
  
  .test-panel {
    order: 1;
  }
}

@media (max-width: 768px) {
  .page-header {
    .header-content {
      flex-direction: column;
      gap: 16px;
      text-align: center;
    }
  }
  
  .test-content {
    padding: 16px;
    gap: 16px;
  }
  
  .control-group {
    .monitor-grid {
      grid-template-columns: 1fr;
    }
  }
}

// Element Plus 样式覆盖
:deep(.el-card__header) {
  padding: 16px 20px;
  background: var(--surface-color);
  border-bottom: 1px solid var(--border-color);
}

:deep(.el-card__body) {
  padding: 20px;
}

:deep(.el-button--small) {
  width: 100%;
  justify-content: flex-start;
}

:deep(.el-dialog__body) {
  max-height: 60vh;
  overflow-y: auto;
}
</style>
