<template>
  <div class="enhanced-charts-example example-page">
    <div class="page-header">
      <h1>增强图表控件示例</h1>
      <p class="description">
        展示增强版 EasyChart 控件的高级功能，包括多种图表类型、数据注释、模板保存、数据缓冲等专业特性。
      </p>
    </div>

    <!-- 图表类型演示 -->
    <div class="example-section">
      <h2 class="section-title">多种图表类型</h2>
      <el-card>
        <template #header>
          <div class="card-header">
            <span>图表类型切换演示</span>
            <div class="header-controls">
              <el-select v-model="selectedDataset" @change="switchDataset" size="small">
                <el-option label="正弦波数据" value="sine" />
                <el-option label="随机数据" value="random" />
                <el-option label="股价数据" value="stock" />
                <el-option label="传感器数据" value="sensor" />
              </el-select>
              <el-button size="small" @click="generateNewData">
                <el-icon><Refresh /></el-icon>
                重新生成
              </el-button>
            </div>
          </div>
        </template>
        
        <EnhancedEasyChart
          :data="chartData"
          :series-configs="chartSeriesConfigs"
          :options="chartOptions"
          @annotation-add="handleAnnotationAdd"
          @template-save="handleTemplateSave"
        />
      </el-card>
    </div>

    <!-- 实时数据流演示 -->
    <div class="example-section">
      <h2 class="section-title">实时数据流与缓冲管理</h2>
      <el-card>
        <template #header>
          <div class="card-header">
            <span>实时数据流演示</span>
            <div class="header-controls">
              <el-button size="small" @click="startRealTimeStream" :disabled="isStreaming">
                <el-icon><VideoPlay /></el-icon>
                开始流
              </el-button>
              <el-button size="small" @click="stopRealTimeStream" :disabled="!isStreaming">
                <el-icon><VideoPause /></el-icon>
                停止流
              </el-button>
              <el-input-number 
                v-model="streamSpeed" 
                :min="50" 
                :max="1000" 
                :step="50"
                size="small"
                style="width: 120px;"
              />
              <span>ms</span>
            </div>
          </div>
        </template>
        
        <EnhancedEasyChart
          :data="streamData"
          :series-configs="streamSeriesConfigs"
          :options="streamOptions"
          :show-controls="false"
        />
      </el-card>
    </div>

    <!-- 多系列对比分析 -->
    <div class="example-section">
      <h2 class="section-title">多系列对比分析</h2>
      <el-card>
        <template #header>
          <div class="card-header">
            <span>多系列数据对比</span>
            <div class="header-controls">
              <el-input-number 
                v-model="seriesCount" 
                :min="2" 
                :max="8" 
                size="small"
                @change="generateMultiSeriesData"
              />
              <span>系列数</span>
              <el-select v-model="comparisonType" @change="generateMultiSeriesData" size="small">
                <el-option label="性能对比" value="performance" />
                <el-option label="温度监控" value="temperature" />
                <el-option label="销售数据" value="sales" />
                <el-option label="网络流量" value="network" />
              </el-select>
            </div>
          </div>
        </template>
        
        <EnhancedEasyChart
          :data="multiSeriesData"
          :series-configs="multiSeriesConfigs"
          :options="multiSeriesOptions"
          :show-status="true"
        />
      </el-card>
    </div>

    <!-- 注释和标记功能 -->
    <div class="example-section">
      <h2 class="section-title">数据注释与标记</h2>
      <el-card>
        <template #header>
          <div class="card-header">
            <span>交互式数据标记</span>
            <div class="header-controls">
              <el-tag v-if="annotations.length > 0" type="info" size="small">
                已添加 {{ annotations.length }} 个注释
              </el-tag>
              <el-button size="small" @click="clearAnnotations">
                <el-icon><Delete /></el-icon>
                清除注释
              </el-button>
            </div>
          </div>
        </template>
        
        <EnhancedEasyChart
          :data="annotationData"
          :series-configs="annotationSeriesConfigs"
          :options="annotationOptions"
          :annotations="annotations"
          @annotation-add="handleAnnotationAdd"
        />
        
        <div class="annotation-list" v-if="annotations.length > 0">
          <h4>注释列表:</h4>
          <el-tag 
            v-for="annotation in annotations" 
            :key="annotation.id"
            :color="annotation.color"
            closable
            @close="removeAnnotation(annotation.id)"
            style="margin: 4px;"
          >
            {{ annotation.text }}
          </el-tag>
        </div>
      </el-card>
    </div>

    <!-- 模板管理 -->
    <div class="example-section">
      <h2 class="section-title">图表模板管理</h2>
      <el-card>
        <template #header>
          <div class="card-header">
            <span>保存和加载图表模板</span>
            <div class="header-controls">
              <el-select v-model="selectedTemplate" @change="loadTemplate" size="small" placeholder="选择模板">
                <el-option 
                  v-for="template in savedTemplates" 
                  :key="template.id"
                  :label="template.name"
                  :value="template.id"
                />
              </el-select>
              <el-button size="small" @click="deleteTemplate" :disabled="!selectedTemplate">
                <el-icon><Delete /></el-icon>
                删除模板
              </el-button>
            </div>
          </div>
        </template>
        
        <EnhancedEasyChart
          :data="templateData"
          :series-configs="templateSeriesConfigs"
          :options="templateOptions"
          @template-save="handleTemplateSave"
        />
        
        <div class="template-info" v-if="currentTemplate">
          <h4>当前模板: {{ currentTemplate.name }}</h4>
          <p>{{ currentTemplate.description }}</p>
          <p>创建时间: {{ formatDate(currentTemplate.createdAt) }}</p>
        </div>
      </el-card>
    </div>

    <!-- 性能监控 -->
    <div class="example-section">
      <h2 class="section-title">性能监控</h2>
      <el-card>
        <el-row :gutter="16">
          <el-col :span="6">
            <el-statistic title="总数据点" :value="totalDataPoints" />
          </el-col>
          <el-col :span="6">
            <el-statistic title="渲染时间" :value="renderTime" suffix="ms" />
          </el-col>
          <el-col :span="6">
            <el-statistic title="内存使用" :value="memoryUsage" suffix="MB" :precision="2" />
          </el-col>
          <el-col :span="6">
            <el-statistic title="更新频率" :value="updateFrequency" suffix="Hz" />
          </el-col>
        </el-row>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { Refresh, VideoPlay, VideoPause, Delete } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import EnhancedEasyChart from '@/components/professional/charts/EnhancedEasyChart.vue'
import type { 
  ChartData, 
  ChartOptions, 
  SeriesConfig, 
  DataAnnotation,
  ChartTemplate
} from '@/types/chart'

// 响应式数据
const selectedDataset = ref('sine')
const seriesCount = ref(3)
const comparisonType = ref('performance')
const isStreaming = ref(false)
const streamSpeed = ref(100)
const annotations = ref<DataAnnotation[]>([])
const savedTemplates = ref<ChartTemplate[]>([])
const selectedTemplate = ref<string>('')
const currentTemplate = ref<ChartTemplate | null>(null)

// 性能监控
const totalDataPoints = ref(0)
const renderTime = ref(0)
const memoryUsage = ref(0)
const updateFrequency = ref(0)

// 图表数据
const chartData = ref<ChartData>({
  series: [],
  xStart: 0,
  xInterval: 0.01,
  chartType: 'line'
})

const streamData = ref<ChartData>({
  series: [[]],
  xStart: 0,
  xInterval: 0.1,
  chartType: 'line'
})

const multiSeriesData = ref<ChartData>({
  series: [],
  xStart: 0,
  xInterval: 1,
  chartType: 'line'
})

const annotationData = ref<ChartData>({
  series: [],
  xStart: 0,
  xInterval: 0.02,
  chartType: 'line'
})

const templateData = ref<ChartData>({
  series: [],
  xStart: 0,
  xInterval: 0.01,
  chartType: 'line'
})

// 系列配置
const chartSeriesConfigs = ref<SeriesConfig[]>([
  {
    name: '数据系列',
    color: '#409eff',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'circle',
    markerSize: 4,
    visible: true,
    chartType: 'line'
  }
])

const streamSeriesConfigs = ref<SeriesConfig[]>([
  {
    name: '实时数据',
    color: '#67c23a',
    lineWidth: 3,
    lineType: 'solid',
    markerType: 'none',
    markerSize: 0,
    visible: true,
    chartType: 'line'
  }
])

const multiSeriesConfigs = ref<SeriesConfig[]>([])
const annotationSeriesConfigs = ref<SeriesConfig[]>([])
const templateSeriesConfigs = ref<SeriesConfig[]>([])

// 图表选项
const chartOptions = ref<Partial<ChartOptions>>({
  autoScale: true,
  legendVisible: true,
  cursorMode: 'zoom',
  gridEnabled: true,
  theme: 'light'
})

const streamOptions = ref<Partial<ChartOptions>>({
  autoScale: true,
  legendVisible: false,
  cursorMode: 'disabled',
  gridEnabled: true,
  theme: 'light'
})

const multiSeriesOptions = ref<Partial<ChartOptions>>({
  autoScale: true,
  legendVisible: true,
  cursorMode: 'cursor',
  gridEnabled: true,
  theme: 'light'
})

const annotationOptions = ref<Partial<ChartOptions>>({
  autoScale: true,
  legendVisible: true,
  cursorMode: 'zoom',
  gridEnabled: true,
  theme: 'light'
})

const templateOptions = ref<Partial<ChartOptions>>({
  autoScale: true,
  legendVisible: true,
  cursorMode: 'zoom',
  gridEnabled: true,
  theme: 'light'
})

// 实时流定时器
let streamTimer: number | null = null
let streamIndex = 0

// 生成不同类型的数据
const generateDataByType = (type: string, length: number = 1000) => {
  const data: number[] = []
  
  switch (type) {
    case 'sine':
      for (let i = 0; i < length; i++) {
        const x = i * 0.01
        data.push(Math.sin(2 * Math.PI * 2 * x) + Math.random() * 0.2 - 0.1)
      }
      break
    
    case 'random':
      for (let i = 0; i < length; i++) {
        data.push(Math.random() * 100 - 50)
      }
      break
    
    case 'stock':
      let price = 100
      for (let i = 0; i < length; i++) {
        price += (Math.random() - 0.5) * 2
        data.push(Math.max(0, price))
      }
      break
    
    case 'sensor':
      let temp = 25
      for (let i = 0; i < length; i++) {
        temp += (Math.random() - 0.5) * 0.5
        data.push(temp)
      }
      break
    
    default:
      for (let i = 0; i < length; i++) {
        data.push(Math.sin(i * 0.1) + Math.random() * 0.1)
      }
  }
  
  return data
}

// 切换数据集
const switchDataset = () => {
  const data = generateDataByType(selectedDataset.value)
  chartData.value = {
    series: data,
    xStart: 0,
    xInterval: 0.01,
    chartType: 'line'
  }
  
  // 更新系列配置
  const configs = {
    sine: { name: '正弦波', color: '#409eff' },
    random: { name: '随机数据', color: '#e6a23c' },
    stock: { name: '股价', color: '#67c23a' },
    sensor: { name: '温度传感器', color: '#f56c6c' }
  }
  
  const config = configs[selectedDataset.value as keyof typeof configs]
  chartSeriesConfigs.value = [{
    name: config.name,
    color: config.color,
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'circle',
    markerSize: 4,
    visible: true,
    chartType: 'line'
  }]
}

// 生成新数据
const generateNewData = () => {
  switchDataset()
  updatePerformanceStats()
}

// 开始实时数据流
const startRealTimeStream = () => {
  if (isStreaming.value) return
  
  isStreaming.value = true
  streamIndex = 0
  streamData.value.series = [[]]
  
  streamTimer = setInterval(() => {
    const newValue = Math.sin(streamIndex * 0.1) + Math.random() * 0.5 - 0.25
    const series = streamData.value.series as number[][]
    
    series[0].push(newValue)
    
    // 保持最大1000个点
    if (series[0].length > 1000) {
      series[0].shift()
    }
    
    streamData.value = {
      ...streamData.value,
      series: [...series]
    }
    
    streamIndex++
    updateFrequency.value = Math.round(1000 / streamSpeed.value)
  }, streamSpeed.value)
}

// 停止实时数据流
const stopRealTimeStream = () => {
  if (streamTimer) {
    clearInterval(streamTimer)
    streamTimer = null
  }
  isStreaming.value = false
  updateFrequency.value = 0
}

// 生成多系列数据
const generateMultiSeriesData = () => {
  const series: number[][] = []
  const configs: SeriesConfig[] = []
  const colors = ['#409eff', '#67c23a', '#e6a23c', '#f56c6c', '#909399', '#c71585', '#ff8c00', '#32cd32']
  
  const dataTypes = {
    performance: ['CPU', 'Memory', 'Disk', 'Network'],
    temperature: ['室内', '室外', '设备', '环境'],
    sales: ['产品A', '产品B', '产品C', '产品D'],
    network: ['上行', '下行', '延迟', '丢包率']
  }
  
  const names = dataTypes[comparisonType.value as keyof typeof dataTypes] || ['系列']
  
  for (let i = 0; i < seriesCount.value; i++) {
    const data = generateDataByType('random', 100)
    series.push(data)
    
    configs.push({
      name: names[i] || `系列 ${i + 1}`,
      color: colors[i % colors.length],
      lineWidth: 2,
      lineType: 'solid',
      markerType: 'circle',
      markerSize: 4,
      visible: true,
      chartType: 'line'
    })
  }
  
  multiSeriesData.value = {
    series,
    xStart: 0,
    xInterval: 1,
    chartType: 'line'
  }
  
  multiSeriesConfigs.value = configs
}

// 生成注释数据
const generateAnnotationData = () => {
  const data = generateDataByType('sine', 500)
  annotationData.value = {
    series: data,
    xStart: 0,
    xInterval: 0.02,
    chartType: 'line'
  }
  
  annotationSeriesConfigs.value = [{
    name: '带注释的数据',
    color: '#409eff',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'circle',
    markerSize: 4,
    visible: true,
    chartType: 'line'
  }]
}

// 处理注释添加
const handleAnnotationAdd = (annotation: DataAnnotation) => {
  annotations.value.push(annotation)
  ElMessage.success(`注释 "${annotation.text}" 已添加`)
}

// 清除注释
const clearAnnotations = () => {
  annotations.value = []
  ElMessage.info('所有注释已清除')
}

// 移除注释
const removeAnnotation = (id: string) => {
  const index = annotations.value.findIndex(ann => ann.id === id)
  if (index > -1) {
    annotations.value.splice(index, 1)
    ElMessage.info('注释已删除')
  }
}

// 处理模板保存
const handleTemplateSave = (template: ChartTemplate) => {
  savedTemplates.value.push(template)
  ElMessage.success(`模板 "${template.name}" 已保存`)
}

// 加载模板
const loadTemplate = () => {
  const template = savedTemplates.value.find(t => t.id === selectedTemplate.value)
  if (template) {
    currentTemplate.value = template
    templateOptions.value = { ...template.chartOptions }
    templateSeriesConfigs.value = [...template.seriesConfigs]
    
    // 生成示例数据
    const data = generateDataByType('sine', 800)
    templateData.value = {
      series: data,
      xStart: 0,
      xInterval: 0.01,
      chartType: 'line'
    }
    
    ElMessage.success(`模板 "${template.name}" 已加载`)
  }
}

// 删除模板
const deleteTemplate = () => {
  if (!selectedTemplate.value) return
  
  const index = savedTemplates.value.findIndex(t => t.id === selectedTemplate.value)
  if (index > -1) {
    const templateName = savedTemplates.value[index].name
    savedTemplates.value.splice(index, 1)
    
    // 更新本地存储
    localStorage.setItem('chartTemplates', JSON.stringify(savedTemplates.value))
    
    selectedTemplate.value = ''
    currentTemplate.value = null
    ElMessage.success(`模板 "${templateName}" 已删除`)
  }
}

// 格式化日期
const formatDate = (date: Date) => {
  return new Date(date).toLocaleString('zh-CN')
}

// 更新性能统计
const updatePerformanceStats = () => {
  const startTime = performance.now()
  
  // 计算总数据点
  let total = 0
  if (chartData.value.series) {
    if (Array.isArray(chartData.value.series[0])) {
      total += (chartData.value.series as number[][]).reduce((sum, series) => sum + series.length, 0)
    } else {
      total += (chartData.value.series as number[]).length
    }
  }
  
  totalDataPoints.value = total
  renderTime.value = Math.round(performance.now() - startTime)
  
  // 估算内存使用
  memoryUsage.value = (total * 8) / 1024 / 1024
}

// 生命周期
onMounted(() => {
  // 加载保存的模板
  const saved = localStorage.getItem('chartTemplates')
  if (saved) {
    savedTemplates.value = JSON.parse(saved)
  }
  
  // 初始化数据
  switchDataset()
  generateMultiSeriesData()
  generateAnnotationData()
  
  // 生成模板数据
  const templateDataArray = generateDataByType('sine', 600)
  templateData.value = {
    series: templateDataArray,
    xStart: 0,
    xInterval: 0.01,
    chartType: 'line'
  }
  
  templateSeriesConfigs.value = [{
    name: '模板数据',
    color: '#409eff',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'circle',
    markerSize: 4,
    visible: true,
    chartType: 'line'
  }]
  
  updatePerformanceStats()
})

onUnmounted(() => {
  stopRealTimeStream()
})
</script>

<style lang="scss" scoped>
.enhanced-charts-example {
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
  
  .annotation-list {
    margin-top: 16px;
    padding: 12px;
    background: #f5f7fa;
    border-radius: 4px;
    
    h4 {
      margin: 0 0 8px 0;
      font-size: 14px;
      color: #606266;
    }
  }
  
  .template-info {
    margin-top: 16px;
    padding: 12px;
    background: #f0f9ff;
    border-radius: 4px;
    border-left: 4px solid #409eff;
    
    h4 {
      margin: 0 0 8px 0;
      color: #409eff;
    }
    
    p {
      margin: 4px 0;
      font-size: 12px;
      color: #606266;
    }
  }
}

@media (max-width: 768px) {
  .enhanced-charts-example {
    .card-header {
      flex-direction: column;
      gap: 12px;
      align-items: flex-start;
      
      .header-controls {
        flex-wrap: wrap;
      }
    }
  }
}
</style>
