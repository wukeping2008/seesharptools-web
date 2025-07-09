<template>
  <div class="all-examples example-page">
    <div class="page-header">
      <h1>所有示例</h1>
      <p class="description">
        SeeSharpTools Web 版本的完整控件库展示。
      </p>
    </div>

    <div class="example-section">
      <h2 class="section-title">控件分类</h2>
      <el-row :gutter="24">
        <el-col :xs="24" :sm="12" :md="8" v-for="category in categories" :key="category.name">
          <el-card class="category-card" shadow="hover" @click="$router.push(category.path)">
            <div class="category-content">
              <div class="category-icon">
                <el-icon :size="48" :color="category.color">
                  <component :is="category.icon" />
                </el-icon>
              </div>
              <h3>{{ category.name }}</h3>
              <p>{{ category.description }}</p>
              <div class="category-status">
                <el-tag :type="category.status === 'completed' ? 'success' : 'warning'">
                  {{ category.statusText }}
                </el-tag>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <div class="example-section">
      <h2 class="section-title">快速预览</h2>
      <el-card>
        <div class="preview-section">
          <h4>EasyChart 图表控件</h4>
          <div class="chart-preview">
            <EasyChart
              :data="previewData"
              :series-configs="previewSeriesConfigs"
              :options="previewOptions"
              :show-toolbar="false"
              :show-controls="false"
              :show-status="false"
            />
          </div>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { TrendCharts, Monitor, Connection } from '@element-plus/icons-vue'
import EasyChart from '@/components/professional/charts/EasyChart.vue'
import type { ChartData, ChartOptions, SeriesConfig } from '@/types/chart'

const categories = ref([
  {
    name: '图表控件',
    description: '专业的数据可视化图表组件',
    icon: 'TrendCharts',
    color: '#409eff',
    path: '/charts',
    status: 'completed',
    statusText: '已完成'
  },
  {
    name: '仪表控件',
    description: '各种工业仪表和测量显示',
    icon: 'Monitor',
    color: '#67c23a',
    path: '/gauges',
    status: 'completed',
    statusText: '已完成'
  },
  {
    name: '指示控件',
    description: 'LED、数码管等状态指示组件',
    icon: 'Connection',
    color: '#e6a23c',
    path: '/indicators',
    status: 'completed',
    statusText: '已完成'
  }
])

// 预览图表数据
const previewData = ref<ChartData>({
  series: [],
  xStart: 0,
  xInterval: 0.01
})

const previewSeriesConfigs = ref<SeriesConfig[]>([
  {
    name: '示例数据',
    color: '#409eff',
    lineWidth: 2,
    lineType: 'solid',
    markerType: 'none',
    markerSize: 4,
    visible: true
  }
])

const previewOptions = ref<Partial<ChartOptions>>({
  autoScale: true,
  legendVisible: false,
  cursorMode: 'disabled',
  gridEnabled: true,
  theme: 'light'
})

// 生成预览数据
const generatePreviewData = () => {
  const data: number[] = []
  const points = 200
  
  for (let i = 0; i < points; i++) {
    const x = i * 0.1
    const value = Math.sin(x) * Math.exp(-x * 0.1) + Math.sin(x * 3) * 0.3
    data.push(value)
  }
  
  previewData.value = {
    series: data,
    xStart: 0,
    xInterval: 0.1
  }
}

onMounted(() => {
  generatePreviewData()
})
</script>

<style lang="scss" scoped>
.all-examples {
  .category-card {
    cursor: pointer;
    transition: transform 0.3s ease;
    height: 100%;
    
    &:hover {
      transform: translateY(-5px);
    }
    
    .category-content {
      text-align: center;
      padding: 20px;
      
      .category-icon {
        margin-bottom: 16px;
      }
      
      h3 {
        margin: 16px 0 12px;
        color: #303133;
      }
      
      p {
        color: #606266;
        margin-bottom: 16px;
        line-height: 1.5;
      }
      
      .category-status {
        margin-top: 16px;
      }
    }
  }
  
  .preview-section {
    h4 {
      margin-bottom: 16px;
      color: #303133;
    }
    
    .chart-preview {
      height: 300px;
    }
  }
}
</style>
