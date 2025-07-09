<template>
  <div class="temperature-acquisition-card-test">
    <div class="page-header">
      <h1>温度采集卡测试</h1>
      <p class="description">
        专业的多通道温度采集卡控件，支持8种热电偶类型、冷端补偿、实时数据采集和趋势分析。
      </p>
    </div>

    <!-- 功能演示 -->
    <div class="demo-section">
      <el-card>
        <template #header>
          <div class="card-header">
            <span>温度采集卡演示</span>
            <div class="demo-controls">
              <el-button @click="resetDemo">
                <el-icon><Refresh /></el-icon>
                重置演示
              </el-button>
              <el-button @click="enableAllChannels">
                <el-icon><Check /></el-icon>
                启用所有通道
              </el-button>
              <el-button @click="enableAlarms">
                <el-icon><Bell /></el-icon>
                启用报警
              </el-button>
            </div>
          </div>
        </template>

        <TemperatureAcquisitionCard
          :initial-config="demoConfig"
          :auto-start="false"
          @data-update="onDataUpdate"
          @alarm="onAlarm"
          @config-change="onConfigChange"
        />
      </el-card>
    </div>

    <!-- 技术特性 -->
    <div class="features-section">
      <h2>🌡️ 核心技术特性</h2>
      <el-row :gutter="24">
        <el-col :span="8">
          <el-card class="feature-card">
            <template #header>
              <h3>🔥 多热电偶支持</h3>
            </template>
            <ul class="feature-list">
              <li><strong>8种热电偶类型</strong>: K、J、T、E、R、S、B、N型</li>
              <li><strong>宽温度范围</strong>: -270°C 到 1820°C</li>
              <li><strong>高精度测量</strong>: 精度可达 ±0.5°C</li>
              <li><strong>自动线性化</strong>: 内置多项式校正算法</li>
            </ul>
          </el-card>
        </el-col>

        <el-col :span="8">
          <el-card class="feature-card">
            <template #header>
              <h3>❄️ 冷端补偿技术</h3>
            </template>
            <ul class="feature-list">
              <li><strong>内部传感器</strong>: 自动冷端温度检测</li>
              <li><strong>外部传感器</strong>: 支持外部冷端监控</li>
              <li><strong>固定温度</strong>: 手动设置冷端温度</li>
              <li><strong>实时补偿</strong>: 动态温度补偿算法</li>
            </ul>
          </el-card>
        </el-col>

        <el-col :span="8">
          <el-card class="feature-card">
            <template #header>
              <h3>📊 数据处理分析</h3>
            </template>
            <ul class="feature-list">
              <li><strong>实时统计</strong>: 最值、平均值、标准差</li>
              <li><strong>趋势分析</strong>: 智能趋势识别和预测</li>
              <li><strong>数字滤波</strong>: 降噪和信号平滑</li>
              <li><strong>数据导出</strong>: CSV格式数据导出</li>
            </ul>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 实时数据监控 -->
    <div class="monitoring-section">
      <h2>📈 实时数据监控</h2>
      <el-row :gutter="16">
        <el-col :span="12">
          <el-card>
            <template #header>
              <span>数据更新统计</span>
            </template>
            <el-descriptions :column="1" border>
              <el-descriptions-item label="总更新次数">{{ updateCount }}</el-descriptions-item>
              <el-descriptions-item label="报警次数">{{ alarmCount }}</el-descriptions-item>
              <el-descriptions-item label="最后更新">{{ lastUpdateTime }}</el-descriptions-item>
              <el-descriptions-item label="数据质量">{{ dataQuality }}</el-descriptions-item>
            </el-descriptions>
          </el-card>
        </el-col>

        <el-col :span="12">
          <el-card>
            <template #header>
              <span>报警历史</span>
            </template>
            <div class="alarm-history">
              <div 
                v-for="(alarm, index) in alarmHistory" 
                :key="index"
                class="alarm-item"
                :class="alarm.type"
              >
                <el-icon>
                  <Warning v-if="alarm.type === 'high'" />
                  <InfoFilled v-else />
                </el-icon>
                <span class="alarm-text">{{ alarm.message }}</span>
                <span class="alarm-time">{{ formatTime(alarm.timestamp) }}</span>
              </div>
              <div v-if="alarmHistory.length === 0" class="no-alarms">
                暂无报警记录
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { Refresh, Check, Bell, Warning, InfoFilled } from '@element-plus/icons-vue'
import TemperatureAcquisitionCard from '@/components/professional/instruments/TemperatureAcquisitionCard.vue'
import type { 
  TemperatureAcquisitionConfig, 
  TemperatureData 
} from '@/types/temperature'

// 响应式数据
const updateCount = ref(0)
const alarmCount = ref(0)
const lastUpdateTime = ref('')
const alarmHistory = ref<Array<{
  type: 'high' | 'low'
  message: string
  timestamp: number
}>>([])

// 演示配置
const demoConfig = ref<Partial<TemperatureAcquisitionConfig>>({
  channels: [
    {
      id: 1,
      name: '炉温监控',
      enabled: true,
      thermocoupleType: 'K',
      unit: 'C',
      range: { min: 0, max: 1200 },
      offset: 0,
      gain: 1,
      coldJunctionCompensation: true,
      linearization: true,
      alarmEnabled: false,
      alarmLow: 50,
      alarmHigh: 800,
      color: '#ff6b6b'
    },
    {
      id: 2,
      name: '环境温度',
      enabled: true,
      thermocoupleType: 'T',
      unit: 'C',
      range: { min: -50, max: 100 },
      offset: 0,
      gain: 1,
      coldJunctionCompensation: true,
      linearization: true,
      alarmEnabled: false,
      alarmLow: 15,
      alarmHigh: 35,
      color: '#4ecdc4'
    },
    {
      id: 3,
      name: '冷却水温',
      enabled: false,
      thermocoupleType: 'J',
      unit: 'C',
      range: { min: 0, max: 200 },
      offset: 0,
      gain: 1,
      coldJunctionCompensation: true,
      linearization: true,
      alarmEnabled: false,
      alarmLow: 10,
      alarmHigh: 80,
      color: '#45b7d1'
    },
    {
      id: 4,
      name: '排气温度',
      enabled: false,
      thermocoupleType: 'E',
      unit: 'C',
      range: { min: 0, max: 500 },
      offset: 0,
      gain: 1,
      coldJunctionCompensation: true,
      linearization: true,
      alarmEnabled: false,
      alarmLow: 20,
      alarmHigh: 300,
      color: '#f7b731'
    }
  ],
  sampleRate: 2,
  averagingCount: 5,
  autoRange: false,
  coldJunctionSource: 'internal',
  coldJunctionTemperature: 25,
  filterEnabled: true,
  filterCutoff: 2
})

// 计算属性
const dataQuality = computed(() => {
  if (updateCount.value === 0) return '无数据'
  if (alarmCount.value === 0) return '良好'
  if (alarmCount.value < 5) return '一般'
  return '需要关注'
})

// 方法
const onDataUpdate = (data: TemperatureData[]) => {
  updateCount.value++
  lastUpdateTime.value = new Date().toLocaleTimeString()
}

const onAlarm = (channelId: number, type: 'low' | 'high', value: number) => {
  alarmCount.value++
  const channel = demoConfig.value.channels?.find(ch => ch.id === channelId)
  const channelName = channel?.name || `通道${channelId}`
  
  alarmHistory.value.unshift({
    type,
    message: `${channelName} ${type === 'high' ? '高温' : '低温'}报警: ${value.toFixed(1)}°C`,
    timestamp: Date.now()
  })
  
  // 限制历史记录数量
  if (alarmHistory.value.length > 10) {
    alarmHistory.value = alarmHistory.value.slice(0, 10)
  }
}

const onConfigChange = (config: TemperatureAcquisitionConfig) => {
  console.log('配置已更新:', config)
}

const resetDemo = () => {
  updateCount.value = 0
  alarmCount.value = 0
  lastUpdateTime.value = ''
  alarmHistory.value = []
}

const enableAllChannels = () => {
  if (demoConfig.value.channels) {
    demoConfig.value.channels.forEach(channel => {
      channel.enabled = true
    })
  }
}

const enableAlarms = () => {
  if (demoConfig.value.channels) {
    demoConfig.value.channels.forEach(channel => {
      channel.alarmEnabled = true
    })
  }
}

const formatTime = (timestamp: number) => {
  return new Date(timestamp).toLocaleTimeString()
}
</script>

<style lang="scss" scoped>
.temperature-acquisition-card-test {
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;

  .page-header {
    text-align: center;
    margin-bottom: 32px;

    h1 {
      color: #303133;
      margin-bottom: 16px;
    }

    .description {
      color: #606266;
      font-size: 16px;
      line-height: 1.6;
      max-width: 800px;
      margin: 0 auto;
    }
  }

  .demo-section {
    margin-bottom: 32px;

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;

      .demo-controls {
        display: flex;
        gap: 12px;
      }
    }
  }

  .features-section,
  .monitoring-section {
    margin-bottom: 32px;

    h2 {
      color: #303133;
      margin-bottom: 24px;
      font-size: 24px;
    }
  }

  .feature-card {
    height: 100%;

    h3 {
      color: #409eff;
      margin: 0;
      font-size: 18px;
    }

    .feature-list {
      list-style: none;
      padding: 0;
      margin: 0;

      li {
        padding: 8px 0;
        color: #606266;
        line-height: 1.6;
        border-bottom: 1px solid #f0f0f0;

        &:last-child {
          border-bottom: none;
        }

        strong {
          color: #303133;
          font-weight: 600;
        }
      }
    }
  }

  .alarm-history {
    max-height: 200px;
    overflow-y: auto;

    .alarm-item {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 0;
      border-bottom: 1px solid #f0f0f0;

      &:last-child {
        border-bottom: none;
      }

      &.high {
        color: #f56c6c;
      }

      &.low {
        color: #e6a23c;
      }

      .alarm-text {
        flex: 1;
        font-size: 14px;
      }

      .alarm-time {
        font-size: 12px;
        color: #909399;
      }
    }

    .no-alarms {
      text-align: center;
      color: #909399;
      padding: 20px;
      font-style: italic;
    }
  }
}

@media (max-width: 768px) {
  .temperature-acquisition-card-test {
    padding: 16px;

    .demo-section {
      .card-header {
        flex-direction: column;
        gap: 16px;
        align-items: stretch;

        .demo-controls {
          justify-content: center;
        }
      }
    }

    .features-section {
      .el-col {
        margin-bottom: 16px;
      }
    }
  }
}
</style>
