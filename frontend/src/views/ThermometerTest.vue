<template>
  <div class="thermometer-test">
    <h1>温度计测试</h1>
    
    <el-row :gutter="20">
      <el-col :span="8">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>基础温度计</span>
              <el-button size="small" @click="updateBasicTemp">
                <el-icon><Refresh /></el-icon>
                随机更新
              </el-button>
            </div>
          </template>
          <Thermometer
            :data="basicThermometerData"
            :options="basicThermometerOptions"
            :height="350"
          />
        </el-card>
      </el-col>
      
      <el-col :span="8">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>工业温度计</span>
              <el-button size="small" @click="toggleIndustrialStream">
                <el-icon><VideoPlay v-if="!industrialStreaming" /><VideoPause v-else /></el-icon>
                {{ industrialStreaming ? '停止' : '开始' }}
              </el-button>
            </div>
          </template>
          <Thermometer
            :data="industrialThermometerData"
            :options="industrialThermometerOptions"
            :alarm-zones="industrialAlarms"
            :height="350"
            @alarm-trigger="handleIndustrialAlarm"
          />
        </el-card>
      </el-col>
      
      <el-col :span="8">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>可配置温度计</span>
              <el-button size="small" @click="toggleUnit">
                <el-icon><Switch /></el-icon>
                切换单位
              </el-button>
            </div>
          </template>
          <Thermometer
            :data="configurableThermometerData"
            :options="configurableThermometerOptions"
            :height="350"
            :show-controls="true"
            @unit-change="handleUnitChange"
          />
        </el-card>
      </el-col>
    </el-row>
    
    <div style="margin-top: 20px;">
      <el-button @click="updateAllTemperatures" type="primary">
        <el-icon><Refresh /></el-icon>
        更新所有温度计
      </el-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Refresh, VideoPlay, VideoPause, Switch } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import Thermometer from '@/components/professional/gauges/Thermometer.vue'
import type { GaugeData, ThermometerOptions, AlarmZone } from '@/types/gauge'

// 基础温度计数据
const basicThermometerData = ref<GaugeData>({
  value: 25.5,
  min: -20,
  max: 100,
  unit: '°C',
  title: '室内温度'
})

const basicThermometerOptions = ref<Partial<ThermometerOptions>>({
  height: 280,
  width: 60,
  bulbRadius: 25,
  tubeWidth: 20,
  showScale: true,
  showLabel: true,
  liquidColor: '#ff4757',
  tubeColor: '#ddd',
  animation: true,
  animationDuration: 1000
})

// 工业温度计数据
const industrialThermometerData = ref<GaugeData>({
  value: 85.0,
  min: 0,
  max: 200,
  unit: '°C',
  title: '反应器温度'
})

const industrialThermometerOptions = ref<Partial<ThermometerOptions>>({
  height: 280,
  width: 70,
  bulbRadius: 30,
  tubeWidth: 25,
  showScale: true,
  showLabel: true,
  liquidColor: '#ff6b35',
  tubeColor: '#e0e0e0',
  animation: true,
  animationDuration: 800
})

const industrialAlarms = ref<AlarmZone[]>([
  { min: 150, max: 200, color: '#f56c6c', label: '高温危险' },
  { min: 120, max: 150, color: '#e6a23c', label: '高温警告' },
  { min: 0, max: 10, color: '#409eff', label: '低温警告' }
])

// 可配置温度计数据
const configurableThermometerData = ref<GaugeData>({
  value: 37.2,
  min: 30,
  max: 45,
  unit: '°C',
  title: '体温计'
})

const configurableThermometerOptions = ref<Partial<ThermometerOptions>>({
  height: 280,
  width: 50,
  bulbRadius: 20,
  tubeWidth: 15,
  showScale: true,
  showLabel: true,
  liquidColor: '#2ed573',
  tubeColor: '#f1f2f6',
  animation: true,
  animationDuration: 1200
})

// 状态控制
const industrialStreaming = ref(false)
let industrialTimer: number | null = null

// 方法
const updateBasicTemp = () => {
  basicThermometerData.value.value = Math.random() * 50 - 10 // -10 到 40 度
}

const toggleIndustrialStream = () => {
  if (industrialStreaming.value) {
    industrialStreaming.value = false
    if (industrialTimer) {
      clearInterval(industrialTimer)
      industrialTimer = null
    }
  } else {
    industrialStreaming.value = true
    industrialTimer = setInterval(() => {
      const oldTemp = industrialThermometerData.value.value
      industrialThermometerData.value.value = Math.max(0, Math.min(200, oldTemp + (Math.random() - 0.5) * 10))
    }, 2000)
  }
}

const toggleUnit = () => {
  const units = ['°C', '°F', 'K']
  const currentIndex = units.indexOf(configurableThermometerData.value.unit || '°C')
  configurableThermometerData.value.unit = units[(currentIndex + 1) % units.length]
  ElMessage.info(`温度单位已切换为: ${configurableThermometerData.value.unit}`)
}

const handleIndustrialAlarm = (zone: AlarmZone, value: number) => {
  ElMessage.warning(`工业温度计报警: ${zone.label} - 当前温度: ${value.toFixed(1)}°C`)
}

const handleUnitChange = (unit: string) => {
  ElMessage.info(`温度单位已更改为: ${unit}`)
}

const updateAllTemperatures = () => {
  updateBasicTemp()
  industrialThermometerData.value.value = Math.random() * 200
  configurableThermometerData.value.value = 30 + Math.random() * 15
  ElMessage.success('所有温度计已更新')
}

// 清理定时器
const cleanup = () => {
  if (industrialTimer) {
    clearInterval(industrialTimer)
  }
}

// 组件卸载时清理
import { onUnmounted } from 'vue'
onUnmounted(cleanup)
</script>

<style scoped>
.thermometer-test {
  padding: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
