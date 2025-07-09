<template>
  <div class="gauges-example example-page">
    <div class="page-header">
      <h1>仪表控件示例</h1>
      <p class="description">
        展示各种专业仪表控件的功能，包括圆形仪表盘、线性仪表、温度计、压力表等工业级仪表控件。
      </p>
    </div>

    <!-- 圆形仪表盘示例 -->
    <div class="example-section">
      <h2 class="section-title">圆形仪表盘</h2>
      <el-row :gutter="20">
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>基础仪表盘</span>
                <el-button size="small" @click="updateBasicGauge">
                  <el-icon><Refresh /></el-icon>
                  随机更新
                </el-button>
              </div>
            </template>
            <CircularGauge
              :data="basicGaugeData"
              :options="basicGaugeOptions"
              :height="280"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>温度监控</span>
                <el-button size="small" @click="toggleTemperatureStream">
                  <el-icon><VideoPlay v-if="!temperatureStreaming" /><VideoPause v-else /></el-icon>
                  {{ temperatureStreaming ? '停止' : '开始' }}
                </el-button>
              </div>
            </template>
            <CircularGauge
              :data="temperatureData"
              :options="temperatureOptions"
              :alarm-zones="temperatureAlarms"
              :height="280"
              @alarm-trigger="handleTemperatureAlarm"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>压力表</span>
                <el-button size="small" @click="updatePressure">
                  <el-icon><Refresh /></el-icon>
                  模拟压力
                </el-button>
              </div>
            </template>
            <CircularGauge
              :data="pressureData"
              :options="pressureOptions"
              :alarm-zones="pressureAlarms"
              :height="280"
              @alarm-trigger="handlePressureAlarm"
            />
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 线性仪表示例 -->
    <div class="example-section">
      <h2 class="section-title">线性仪表</h2>
      <el-row :gutter="20">
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>水平线性仪表</span>
                <el-button size="small" @click="updateLinearGauge">
                  <el-icon><Refresh /></el-icon>
                  随机更新
                </el-button>
              </div>
            </template>
            <LinearGauge
              :data="linearGaugeData"
              :options="linearGaugeOptions"
              :alarm-zones="linearAlarms"
              :height="150"
              @alarm-trigger="handleLinearAlarm"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>垂直线性仪表</span>
                <el-button size="small" @click="toggleVerticalStream">
                  <el-icon><VideoPlay v-if="!verticalStreaming" /><VideoPause v-else /></el-icon>
                  {{ verticalStreaming ? '停止' : '开始' }}
                </el-button>
              </div>
            </template>
            <LinearGauge
              :data="verticalGaugeData"
              :options="verticalGaugeOptions"
              :alarm-zones="verticalAlarms"
              :height="250"
              @alarm-trigger="handleVerticalAlarm"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>可配置线性仪表</span>
                <el-button size="small" @click="toggleLinearOrientation">
                  <el-icon><Switch /></el-icon>
                  切换方向
                </el-button>
              </div>
            </template>
            <LinearGauge
              :data="configurableLinearData"
              :options="configurableLinearOptions"
              :height="200"
              :show-controls="true"
              @orientation-change="handleOrientationChange"
            />
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 温度计示例 -->
    <div class="example-section">
      <h2 class="section-title">温度计</h2>
      <el-row :gutter="20">
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>基础温度计</span>
                <el-button size="small" @click="updateBasicThermometer">
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
                <el-button size="small" @click="toggleIndustrialThermometer">
                  <el-icon><VideoPlay v-if="!industrialThermometerStreaming" /><VideoPause v-else /></el-icon>
                  {{ industrialThermometerStreaming ? '停止' : '开始' }}
                </el-button>
              </div>
            </template>
            <Thermometer
              :data="industrialThermometerData"
              :options="industrialThermometerOptions"
              :alarm-zones="industrialThermometerAlarms"
              :height="350"
              @alarm-trigger="handleIndustrialThermometerAlarm"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>可配置温度计</span>
                <el-button size="small" @click="toggleThermometerUnit">
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
              @unit-change="handleThermometerUnitChange"
            />
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 压力表示例 -->
    <div class="example-section">
      <h2 class="section-title">压力表</h2>
      <el-row :gutter="20">
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>基础压力表</span>
                <el-button size="small" @click="updateBasicPressureGauge">
                  <el-icon><Refresh /></el-icon>
                  随机更新
                </el-button>
              </div>
            </template>
            <PressureGauge
              :data="basicPressureGaugeData"
              :options="basicPressureGaugeOptions"
              :height="350"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>工业压力表</span>
                <el-button size="small" @click="toggleIndustrialPressureGauge">
                  <el-icon><VideoPlay v-if="!industrialPressureGaugeStreaming" /><VideoPause v-else /></el-icon>
                  {{ industrialPressureGaugeStreaming ? '停止' : '开始' }}
                </el-button>
              </div>
            </template>
            <PressureGauge
              :data="industrialPressureGaugeData"
              :options="industrialPressureGaugeOptions"
              :alarm-zones="industrialPressureGaugeAlarms"
              :safety-zones="industrialPressureGaugeSafetyZones"
              :height="350"
              @alarm-trigger="handleIndustrialPressureGaugeAlarm"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>可配置压力表</span>
                <el-button size="small" @click="togglePressureGaugeUnit">
                  <el-icon><Switch /></el-icon>
                  切换单位
                </el-button>
              </div>
            </template>
            <PressureGauge
              :data="configurablePressureGaugeData"
              :options="configurablePressureGaugeOptions"
              :height="350"
              :show-controls="true"
              @unit-change="handlePressureGaugeUnitChange"
              @min-max-record="handlePressureGaugeMinMaxRecord"
            />
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 液位计示例 -->
    <div class="example-section">
      <h2 class="section-title">液位计</h2>
      <el-row :gutter="20">
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>基础液位计</span>
                <el-button size="small" @click="updateBasicWaterLevel">
                  <el-icon><Refresh /></el-icon>
                  随机更新
                </el-button>
              </div>
            </template>
            <WaterLevel
              :data="basicWaterLevelData"
              :options="basicWaterLevelOptions"
              :height="400"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>工业液位计</span>
                <el-button size="small" @click="toggleIndustrialWaterLevel">
                  <el-icon><VideoPlay v-if="!industrialWaterLevelStreaming" /><VideoPause v-else /></el-icon>
                  {{ industrialWaterLevelStreaming ? '停止' : '开始' }}
                </el-button>
              </div>
            </template>
            <WaterLevel
              :data="industrialWaterLevelData"
              :options="industrialWaterLevelOptions"
              :alarm-zones="industrialWaterLevelAlarms"
              :height="400"
              @alarm-trigger="handleIndustrialWaterLevelAlarm"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>可配置液位计</span>
                <el-button size="small" @click="toggleWaterLevelLiquidType">
                  <el-icon><Switch /></el-icon>
                  切换液体
                </el-button>
              </div>
            </template>
            <WaterLevel
              :data="configurableWaterLevelData"
              :options="configurableWaterLevelOptions"
              :height="400"
              :show-controls="true"
              @liquid-type-change="handleWaterLevelLiquidTypeChange"
            />
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 储罐示例 -->
    <div class="example-section">
      <h2 class="section-title">储罐</h2>
      <el-row :gutter="20">
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>基础储罐</span>
                <el-button size="small" @click="updateBasicTank">
                  <el-icon><Refresh /></el-icon>
                  随机更新
                </el-button>
              </div>
            </template>
            <Tank
              :data="basicTankData"
              :options="basicTankOptions"
              :height="450"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>工业储罐</span>
                <el-button size="small" @click="toggleIndustrialTank">
                  <el-icon><VideoPlay v-if="!industrialTankStreaming" /><VideoPause v-else /></el-icon>
                  {{ industrialTankStreaming ? '停止' : '开始' }}
                </el-button>
              </div>
            </template>
            <Tank
              :data="industrialTankData"
              :options="industrialTankOptions"
              :alarm-zones="industrialTankAlarms"
              :height="450"
              @alarm-trigger="handleIndustrialTankAlarm"
              @valve-change="handleTankValveChange"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>可配置储罐</span>
                <el-button size="small" @click="toggleTankType">
                  <el-icon><Switch /></el-icon>
                  切换类型
                </el-button>
              </div>
            </template>
            <Tank
              :data="configurableTankData"
              :options="configurableTankOptions"
              :height="450"
              :show-controls="true"
              @tank-type-change="handleTankTypeChange"
            />
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 流量计示例 -->
    <div class="example-section">
      <h2 class="section-title">流量计</h2>
      <el-row :gutter="20">
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>基础流量计</span>
                <el-button size="small" @click="updateBasicFlowMeter">
                  <el-icon><Refresh /></el-icon>
                  随机更新
                </el-button>
              </div>
            </template>
            <FlowMeter
              :data="basicFlowMeterData"
              :options="basicFlowMeterOptions"
              :height="250"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>工业流量计</span>
                <el-button size="small" @click="toggleIndustrialFlowMeter">
                  <el-icon><VideoPlay v-if="!industrialFlowMeterStreaming" /><VideoPause v-else /></el-icon>
                  {{ industrialFlowMeterStreaming ? '停止' : '开始' }}
                </el-button>
              </div>
            </template>
            <FlowMeter
              :data="industrialFlowMeterData"
              :options="industrialFlowMeterOptions"
              :alarm-zones="industrialFlowMeterAlarms"
              :height="250"
              @alarm-trigger="handleIndustrialFlowMeterAlarm"
              @flow-type-change="handleFlowTypeChange"
            />
          </el-card>
        </el-col>
        
        <el-col :span="8">
          <el-card>
            <template #header>
              <div class="card-header">
                <span>可配置流量计</span>
                <el-button size="small" @click="toggleFlowMeterType">
                  <el-icon><Switch /></el-icon>
                  切换类型
                </el-button>
              </div>
            </template>
            <FlowMeter
              :data="configurableFlowMeterData"
              :options="configurableFlowMeterOptions"
              :height="250"
              :show-controls="true"
              @total-flow-update="handleTotalFlowUpdate"
            />
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- 仪表盘配置演示 -->
    <div class="example-section">
      <h2 class="section-title">仪表盘配置演示</h2>
      <el-card>
        <template #header>
          <div class="card-header">
            <span>可配置仪表盘</span>
            <div class="header-controls">
              <el-select v-model="selectedStyle" @change="updateConfigurableGauge" size="small">
                <el-option label="经典样式" value="classic" />
                <el-option label="现代样式" value="modern" />
                <el-option label="简约样式" value="minimal" />
                <el-option label="科技样式" value="tech" />
              </el-select>
              <el-select v-model="selectedTheme" @change="updateConfigurableGauge" size="small">
                <el-option label="浅色主题" value="light" />
                <el-option label="深色主题" value="dark" />
              </el-select>
            </div>
          </div>
        </template>
        
        <el-row :gutter="20">
          <el-col :span="16">
            <CircularGauge
              :data="configurableData"
              :options="configurableOptions"
              :style="configurableStyle"
              :pointer="configurablePointer"
              :progress="configurableProgress"
              :height="400"
              :show-controls="true"
            />
          </el-col>
          
          <el-col :span="8">
            <div class="config-panel">
              <h4>配置选项</h4>
              
              <div class="config-group">
                <label>数值范围:</label>
                <el-slider 
                  v-model="configurableData.value" 
                  :min="configurableData.min || 0" 
                  :max="configurableData.max || 100"
                  @change="updateConfigurableValue"
                />
                <span>{{ configurableData.value.toFixed(1) }}{{ configurableData.unit }}</span>
              </div>
              
              <div class="config-group">
                <label>起始角度:</label>
                <el-slider 
                  v-model="configurableOptions.startAngle" 
                  :min="0" 
                  :max="360"
                  @change="updateConfigurableGauge"
                />
                <span>{{ configurableOptions.startAngle }}°</span>
              </div>
              
              <div class="config-group">
                <label>结束角度:</label>
                <el-slider 
                  v-model="configurableOptions.endAngle" 
                  :min="-180" 
                  :max="180"
                  @change="updateConfigurableGauge"
                />
                <span>{{ configurableOptions.endAngle }}°</span>
              </div>
              
              <div class="config-group">
                <label>指针宽度:</label>
                <el-slider 
                  v-model="configurablePointer.width" 
                  :min="2" 
                  :max="20"
                  @change="updateConfigurableGauge"
                />
                <span>{{ configurablePointer.width }}px</span>
              </div>
              
              <div class="config-group">
                <label>进度条宽度:</label>
                <el-slider 
                  v-model="configurableProgress.width" 
                  :min="5" 
                  :max="30"
                  @change="updateConfigurableGauge"
                />
                <span>{{ configurableProgress.width }}px</span>
              </div>
              
              <div class="config-group">
                <el-checkbox v-model="configurableOptions.showPointer" @change="updateConfigurableGauge">
                  显示指针
                </el-checkbox>
              </div>
              
              <div class="config-group">
                <el-checkbox v-model="configurableOptions.showProgress" @change="updateConfigurableGauge">
                  显示进度条
                </el-checkbox>
              </div>
              
              <div class="config-group">
                <el-checkbox v-model="configurableOptions.animation" @change="updateConfigurableGauge">
                  启用动画
                </el-checkbox>
              </div>
            </div>
          </el-col>
        </el-row>
      </el-card>
    </div>

    <!-- 多仪表盘监控 -->
    <div class="example-section">
      <h2 class="section-title">多仪表盘监控面板</h2>
      <el-card>
        <template #header>
          <div class="card-header">
            <span>设备监控面板</span>
            <div class="header-controls">
              <el-button size="small" @click="startMonitoring" :disabled="isMonitoring">
                <el-icon><VideoPlay /></el-icon>
                开始监控
              </el-button>
              <el-button size="small" @click="stopMonitoring" :disabled="!isMonitoring">
                <el-icon><VideoPause /></el-icon>
                停止监控
              </el-button>
              <el-tag :type="isMonitoring ? 'success' : 'info'" size="small">
                {{ isMonitoring ? '监控中' : '已停止' }}
              </el-tag>
            </div>
          </div>
        </template>
        
        <el-row :gutter="16">
          <el-col :span="6" v-for="(gauge, index) in monitoringGauges" :key="index">
            <div class="monitoring-gauge">
              <CircularGauge
                :data="gauge.data"
                :options="gauge.options"
                :alarm-zones="gauge.alarms"
                :height="200"
                :show-toolbar="false"
                :show-controls="false"
                @alarm-trigger="handleMonitoringAlarm"
              />
            </div>
          </el-col>
        </el-row>
      </el-card>
    </div>

    <!-- 报警日志 -->
    <div class="example-section" v-if="alarmLogs.length > 0">
      <h2 class="section-title">报警日志</h2>
      <el-card>
        <template #header>
          <div class="card-header">
            <span>系统报警记录</span>
            <el-button size="small" @click="clearAlarmLogs">
              <el-icon><Delete /></el-icon>
              清除日志
            </el-button>
          </div>
        </template>
        
        <el-table :data="alarmLogs" style="width: 100%" max-height="300">
          <el-table-column prop="timestamp" label="时间" width="180">
            <template #default="scope">
              {{ formatTime(scope.row.timestamp) }}
            </template>
          </el-table-column>
          <el-table-column prop="source" label="来源" width="120" />
          <el-table-column prop="type" label="类型" width="100">
            <template #default="scope">
              <el-tag :type="scope.row.type === 'danger' ? 'danger' : 'warning'" size="small">
                {{ scope.row.type === 'danger' ? '危险' : '警告' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="message" label="消息" />
          <el-table-column prop="value" label="数值" width="100">
            <template #default="scope">
              {{ scope.row.value.toFixed(2) }}{{ scope.row.unit }}
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { Refresh, VideoPlay, VideoPause, Delete, Switch } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import CircularGauge from '@/components/professional/gauges/CircularGauge.vue'
import LinearGauge from '@/components/professional/gauges/LinearGauge.vue'
import Thermometer from '@/components/professional/gauges/Thermometer.vue'
import PressureGauge from '@/components/professional/gauges/PressureGauge.vue'
import WaterLevel from '@/components/professional/gauges/WaterLevel.vue'
import Tank from '@/components/professional/gauges/Tank.vue'
import FlowMeter from '@/components/professional/gauges/FlowMeter.vue'
import type { GaugeData, GaugeOptions, GaugeStyle, PointerConfig, ProgressConfig, AlarmZone, LinearGaugeOptions, ThermometerOptions, PressureGaugeOptions, SafetyZone } from '@/types/gauge'

// 注册组件
const components = {
  CircularGauge,
  LinearGauge,
  Thermometer,
  PressureGauge,
  WaterLevel,
  Tank,
  FlowMeter
}

// 基础仪表盘数据
const basicGaugeData = ref<GaugeData>({
  value: 65,
  min: 0,
  max: 100,
  unit: '%',
  title: '系统负载'
})

const basicGaugeOptions = ref<Partial<GaugeOptions>>({
  startAngle: 225,
  endAngle: -45,
  showPointer: true,
  showProgress: true,
  animation: true,
  theme: 'light'
})

// 温度监控数据
const temperatureData = ref<GaugeData>({
  value: 25.5,
  min: -10,
  max: 50,
  unit: '°C',
  title: '环境温度'
})

const temperatureOptions = ref<Partial<GaugeOptions>>({
  startAngle: 180,
  endAngle: 0,
  showPointer: true,
  showProgress: true,
  animation: true,
  theme: 'light'
})

const temperatureAlarms = ref<AlarmZone[]>([
  { min: 35, max: 50, color: '#f56c6c', label: '高温警告' },
  { min: -10, max: 5, color: '#409eff', label: '低温警告' }
])

// 压力表数据
const pressureData = ref<GaugeData>({
  value: 2.5,
  min: 0,
  max: 10,
  unit: 'MPa',
  title: '系统压力'
})

const pressureOptions = ref<Partial<GaugeOptions>>({
  startAngle: 225,
  endAngle: -45,
  showPointer: true,
  showProgress: true,
  animation: true,
  theme: 'light'
})

const pressureAlarms = ref<AlarmZone[]>([
  { min: 8, max: 10, color: '#f56c6c', label: '超压危险' },
  { min: 6, max: 8, color: '#e6a23c', label: '压力警告' }
])

// 可配置仪表盘
const selectedStyle = ref('classic')
const selectedTheme = ref('light')

const configurableData = ref<GaugeData>({
  value: 75,
  min: 0,
  max: 100,
  unit: '%',
  title: '可配置仪表'
})

const configurableOptions = ref<GaugeOptions>({
  size: 200,
  startAngle: 225,
  endAngle: -45,
  clockwise: true,
  showPointer: true,
  showProgress: true,
  showAxis: true,
  showAxisLabel: true,
  showTitle: true,
  showDetail: true,
  animation: true,
  animationDuration: 1000,
  theme: 'light'
})

const configurableStyle = ref<GaugeStyle>({
  backgroundColor: 'transparent'
})

const configurablePointer = ref<PointerConfig>({
  show: true,
  length: '80%',
  width: 6,
  color: '#409eff'
})

const configurableProgress = ref<ProgressConfig>({
  show: true,
  width: 10,
  color: ['#67c23a', '#e6a23c', '#f56c6c'],
  backgroundColor: '#e4e7ed',
  roundCap: true
})

// 线性仪表数据
const linearGaugeData = ref<GaugeData>({
  value: 45,
  min: 0,
  max: 100,
  unit: '%',
  title: '水平进度'
})

const linearGaugeOptions = ref<Partial<LinearGaugeOptions>>({
  orientation: 'horizontal',
  width: 20,
  showScale: true,
  showPointer: true,
  showProgress: true,
  animation: true,
  theme: 'light'
})

const linearAlarms = ref<AlarmZone[]>([
  { min: 80, max: 100, color: '#f56c6c', label: '高负载警告' }
])

// 垂直线性仪表数据
const verticalGaugeData = ref<GaugeData>({
  value: 30,
  min: 0,
  max: 100,
  unit: 'L',
  title: '液位高度'
})

const verticalGaugeOptions = ref<Partial<LinearGaugeOptions>>({
  orientation: 'vertical',
  width: 25,
  showScale: true,
  showPointer: true,
  showProgress: true,
  animation: true,
  theme: 'light'
})

const verticalAlarms = ref<AlarmZone[]>([
  { min: 0, max: 20, color: '#f56c6c', label: '液位过低' },
  { min: 85, max: 100, color: '#e6a23c', label: '液位过高' }
])

// 可配置线性仪表数据
const configurableLinearData = ref<GaugeData>({
  value: 60,
  min: 0,
  max: 100,
  unit: '%',
  title: '可配置线性仪表'
})

const configurableLinearOptions = ref<Partial<LinearGaugeOptions>>({
  orientation: 'horizontal',
  width: 20,
  showScale: true,
  showPointer: true,
  showProgress: true,
  animation: true,
  theme: 'light'
})

// 监控面板
const isMonitoring = ref(false)
const temperatureStreaming = ref(false)
const verticalStreaming = ref(false)
const monitoringGauges = ref([
  {
    data: { value: 45, min: 0, max: 100, unit: '%', title: 'CPU使用率' },
    options: { startAngle: 225, endAngle: -45, animation: true, theme: 'light' as const },
    alarms: [{ min: 80, max: 100, color: '#f56c6c', label: 'CPU过载' }]
  },
  {
    data: { value: 62, min: 0, max: 100, unit: '%', title: '内存使用率' },
    options: { startAngle: 225, endAngle: -45, animation: true, theme: 'light' as const },
    alarms: [{ min: 85, max: 100, color: '#f56c6c', label: '内存不足' }]
  },
  {
    data: { value: 28, min: 0, max: 100, unit: '%', title: '磁盘使用率' },
    options: { startAngle: 225, endAngle: -45, animation: true, theme: 'light' as const },
    alarms: [{ min: 90, max: 100, color: '#f56c6c', label: '磁盘空间不足' }]
  },
  {
    data: { value: 15, min: 0, max: 100, unit: 'Mbps', title: '网络流量' },
    options: { startAngle: 225, endAngle: -45, animation: true, theme: 'light' as const },
    alarms: [{ min: 80, max: 100, color: '#e6a23c', label: '网络拥堵' }]
  }
])

// 报警日志
const alarmLogs = ref<Array<{
  timestamp: Date
  source: string
  type: string
  message: string
  value: number
  unit: string
}>>([])

// 温度计数据
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

const industrialThermometerAlarms = ref<AlarmZone[]>([
  { min: 150, max: 200, color: '#f56c6c', label: '高温危险' },
  { min: 120, max: 150, color: '#e6a23c', label: '高温警告' },
  { min: 0, max: 10, color: '#409eff', label: '低温警告' }
])

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

// 压力表数据
const basicPressureGaugeData = ref<GaugeData>({
  value: 3.2,
  min: 0,
  max: 10,
  unit: 'MPa',
  title: '基础压力表'
})

const basicPressureGaugeOptions = ref<Partial<PressureGaugeOptions>>({
  size: 200,
  startAngle: 225,
  endAngle: -45,
  showScale: true,
  showPointer: true,
  showValue: true,
  showMinMax: false,
  showSafetyZones: true,
  showPressureType: true,
  pressureType: '表压',
  pointerColor: '#409eff',
  dialColor: '#f5f7fa',
  scaleColor: '#606266',
  animation: true,
  animationDuration: 1000
})

const industrialPressureGaugeData = ref<GaugeData>({
  value: 6.8,
  min: 0,
  max: 15,
  unit: 'MPa',
  title: '工业压力表'
})

const industrialPressureGaugeOptions = ref<Partial<PressureGaugeOptions>>({
  size: 200,
  startAngle: 225,
  endAngle: -45,
  showScale: true,
  showPointer: true,
  showValue: true,
  showMinMax: true,
  showSafetyZones: true,
  showPressureType: true,
  pressureType: '表压',
  pointerColor: '#e6a23c',
  dialColor: '#f5f7fa',
  scaleColor: '#606266',
  animation: true,
  animationDuration: 800
})

const industrialPressureGaugeAlarms = ref<AlarmZone[]>([
  { min: 12, max: 15, color: '#f56c6c', label: '超压危险' },
  { min: 10, max: 12, color: '#e6a23c', label: '压力警告' }
])

const industrialPressureGaugeSafetyZones = ref<SafetyZone[]>([
  { min: 2, max: 8, color: '#67c23a', label: '安全区域' }
])

const configurablePressureGaugeData = ref<GaugeData>({
  value: 4.5,
  min: 0,
  max: 10,
  unit: 'MPa',
  title: '可配置压力表'
})

const configurablePressureGaugeOptions = ref<Partial<PressureGaugeOptions>>({
  size: 200,
  startAngle: 225,
  endAngle: -45,
  showScale: true,
  showPointer: true,
  showValue: true,
  showMinMax: true,
  showSafetyZones: true,
  showPressureType: true,
  pressureType: '表压',
  pointerColor: '#67c23a',
  dialColor: '#f5f7fa',
  scaleColor: '#606266',
  animation: true,
  animationDuration: 1200
})

// 液位计数据
const basicWaterLevelData = ref<GaugeData>({
  value: 45.5,
  min: 0,
  max: 100,
  unit: 'L',
  title: '基础液位计'
})

const basicWaterLevelOptions = ref({
  height: 300,
  width: 150,
  showScale: true,
  showWave: true,
  showBubbles: true,
  showPercentage: true,
  showCapacity: true,
  showValue: true,
  showLiquidType: true,
  showLevelLine: true,
  tankColor: '#e0e0e0',
  animation: true,
  animationDuration: 1000,
  waveSpeed: 3
})

const industrialWaterLevelData = ref<GaugeData>({
  value: 72.3,
  min: 0,
  max: 150,
  unit: 'L',
  title: '工业液位计'
})

const industrialWaterLevelOptions = ref({
  height: 300,
  width: 160,
  showScale: true,
  showWave: true,
  showBubbles: true,
  showPercentage: true,
  showCapacity: true,
  showValue: true,
  showLiquidType: true,
  showLevelLine: true,
  tankColor: '#d0d0d0',
  animation: true,
  animationDuration: 800,
  waveSpeed: 4
})

const industrialWaterLevelAlarms = ref<AlarmZone[]>([
  { min: 120, max: 150, color: '#f56c6c', label: '液位过高' },
  { min: 0, max: 20, color: '#e6a23c', label: '液位过低' }
])

const configurableWaterLevelData = ref<GaugeData>({
  value: 35.8,
  min: 0,
  max: 80,
  unit: 'L',
  title: '可配置液位计'
})

const configurableWaterLevelOptions = ref({
  height: 300,
  width: 140,
  showScale: true,
  showWave: true,
  showBubbles: true,
  showPercentage: true,
  showCapacity: true,
  showValue: true,
  showLiquidType: true,
  showLevelLine: true,
  tankColor: '#e8e8e8',
  animation: true,
  animationDuration: 1200,
  waveSpeed: 2
})

// 储罐数据
const basicTankData = ref<GaugeData>({
  value: 65.2,
  min: 0,
  max: 200,
  unit: 'L',
  title: '基础储罐'
})

const basicTankOptions = ref({
  height: 350,
  diameter: 200,
  showScale: true,
  showWave: true,
  showFlow: true,
  showInfo: true,
  showStatus: true,
  showInlet: true,
  showOutlet: true,
  showDrain: true,
  showVent: true,
  showSupport: true,
  showLevelLine: true,
  tankColor: '#e0e0e0',
  animation: true,
  animationDuration: 1000,
  waveSpeed: 3
})

const industrialTankData = ref<GaugeData>({
  value: 145.8,
  min: 0,
  max: 300,
  unit: 'L',
  title: '工业储罐'
})

const industrialTankOptions = ref({
  height: 350,
  diameter: 220,
  showScale: true,
  showWave: true,
  showFlow: true,
  showInfo: true,
  showStatus: true,
  showInlet: true,
  showOutlet: true,
  showDrain: true,
  showVent: true,
  showSupport: true,
  showLevelLine: true,
  tankColor: '#d0d0d0',
  animation: true,
  animationDuration: 800,
  waveSpeed: 4
})

const industrialTankAlarms = ref<AlarmZone[]>([
  { min: 250, max: 300, color: '#f56c6c', label: '储罐过满' },
  { min: 0, max: 30, color: '#e6a23c', label: '储罐过空' }
])

const configurableTankData = ref<GaugeData>({
  value: 88.5,
  min: 0,
  max: 150,
  unit: 'L',
  title: '可配置储罐'
})

const configurableTankOptions = ref({
  height: 350,
  diameter: 180,
  showScale: true,
  showWave: true,
  showFlow: true,
  showInfo: true,
  showStatus: true,
  showInlet: true,
  showOutlet: true,
  showDrain: true,
  showVent: true,
  showSupport: true,
  showLevelLine: true,
  tankColor: '#e8e8e8',
  animation: true,
  animationDuration: 1200,
  waveSpeed: 2
})

// 流量计数据
const basicFlowMeterData = ref<GaugeData>({
  value: 25.8,
  min: 0,
  max: 100,
  unit: 'L',
  title: '基础流量计'
})

const basicFlowMeterOptions = ref({
  pipeWidth: 40,
  meterSize: 120,
  showDisplay: true,
  showIndicator: true,
  showFlow: true,
  showInternalFlow: true,
  showScale: true,
  showInfo: true,
  showStatus: true,
  pipeColor: '#666666',
  meterColor: '#e0e0e0',
  animation: true,
  animationDuration: 1000,
  flowSpeed: 5
})

const industrialFlowMeterData = ref<GaugeData>({
  value: 68.5,
  min: 0,
  max: 150,
  unit: 'L',
  title: '工业流量计'
})

const industrialFlowMeterOptions = ref({
  pipeWidth: 50,
  meterSize: 140,
  showDisplay: true,
  showIndicator: true,
  showFlow: true,
  showInternalFlow: true,
  showScale: true,
  showInfo: true,
  showStatus: true,
  pipeColor: '#555555',
  meterColor: '#d0d0d0',
  animation: true,
  animationDuration: 800,
  flowSpeed: 6
})

const industrialFlowMeterAlarms = ref<AlarmZone[]>([
  { min: 120, max: 150, color: '#f56c6c', label: '流量过高' },
  { min: 0, max: 10, color: '#e6a23c', label: '流量过低' }
])

const configurableFlowMeterData = ref<GaugeData>({
  value: 42.3,
  min: 0,
  max: 80,
  unit: 'L',
  title: '可配置流量计'
})

const configurableFlowMeterOptions = ref({
  pipeWidth: 35,
  meterSize: 100,
  showDisplay: true,
  showIndicator: true,
  showFlow: true,
  showInternalFlow: true,
  showScale: true,
  showInfo: true,
  showStatus: true,
  pipeColor: '#777777',
  meterColor: '#e8e8e8',
  animation: true,
  animationDuration: 1200,
  flowSpeed: 4
})

// 状态控制
const industrialThermometerStreaming = ref(false)
const industrialPressureGaugeStreaming = ref(false)
const industrialWaterLevelStreaming = ref(false)
const industrialTankStreaming = ref(false)
const industrialFlowMeterStreaming = ref(false)

// 定时器
let monitoringTimer: number | null = null
let temperatureTimer: number | null = null
let verticalTimer: number | null = null
let industrialThermometerTimer: number | null = null
let industrialPressureGaugeTimer: number | null = null
let industrialWaterLevelTimer: number | null = null
let industrialTankTimer: number | null = null
let industrialFlowMeterTimer: number | null = null

// 更新基础仪表盘
const updateBasicGauge = () => {
  basicGaugeData.value.value = Math.random() * 100
}

// 更新压力表
const updatePressure = () => {
  pressureData.value.value = Math.random() * 10
}

// 更新可配置仪表盘数值
const updateConfigurableValue = () => {
  // 数值已通过滑块更新
}

// 更新可配置仪表盘
const updateConfigurableGauge = () => {
  // 应用样式
  switch (selectedStyle.value) {
    case 'modern':
      configurablePointer.value.color = '#67c23a'
      configurableProgress.value.color = ['#409eff', '#67c23a', '#e6a23c']
      break
    case 'minimal':
      configurablePointer.value.color = '#909399'
      configurableProgress.value.color = '#c0c4cc'
      break
    case 'tech':
      configurablePointer.value.color = '#00d4ff'
      configurableProgress.value.color = ['#00d4ff', '#00ff88', '#ff6b6b']
      break
    default:
      configurablePointer.value.color = '#409eff'
      configurableProgress.value.color = ['#67c23a', '#e6a23c', '#f56c6c']
  }
  
  configurableOptions.value.theme = selectedTheme.value as 'light' | 'dark'
}

// 开始监控
const startMonitoring = () => {
  isMonitoring.value = true
  monitoringTimer = setInterval(() => {
    monitoringGauges.value.forEach(gauge => {
      const oldValue = gauge.data.value
      gauge.data.value = Math.max(0, Math.min(100, oldValue + (Math.random() - 0.5) * 10))
    })
  }, 2000)
}

// 停止监控
const stopMonitoring = () => {
  isMonitoring.value = false
  if (monitoringTimer) {
    clearInterval(monitoringTimer)
    monitoringTimer = null
  }
}

// 开始/停止温度流
const toggleTemperatureStream = () => {
  if (temperatureStreaming.value) {
    temperatureStreaming.value = false
    if (temperatureTimer) {
      clearInterval(temperatureTimer)
      temperatureTimer = null
    }
  } else {
    temperatureStreaming.value = true
    temperatureTimer = setInterval(() => {
      const oldTemp = temperatureData.value.value
      temperatureData.value.value = Math.max(-10, Math.min(50, oldTemp + (Math.random() - 0.5) * 2))
    }, 1000)
  }
}

// 处理温度报警
const handleTemperatureAlarm = (zone: AlarmZone, value: number) => {
  addAlarmLog('温度传感器', zone.label || '温度异常', value, '°C', 'danger')
}

// 处理压力报警
const handlePressureAlarm = (zone: AlarmZone, value: number) => {
  addAlarmLog('压力传感器', zone.label || '压力异常', value, 'MPa', 'danger')
}

// 处理监控报警
const handleMonitoringAlarm = (zone: AlarmZone, value: number) => {
  addAlarmLog('系统监控', zone.label || '系统异常', value, '%', 'warning')
}

// 添加报警日志
const addAlarmLog = (source: string, message: string, value: number, unit: string, type: string) => {
  alarmLogs.value.unshift({
    timestamp: new Date(),
    source,
    type,
    message,
    value,
    unit
  })
  
  // 保持最近50条记录
  if (alarmLogs.value.length > 50) {
    alarmLogs.value.pop()
  }
  
  ElMessage.warning(`${source}: ${message}`)
}

// 清除报警日志
const clearAlarmLogs = () => {
  alarmLogs.value = []
  ElMessage.info('报警日志已清除')
}

// 线性仪表方法
const updateLinearGauge = () => {
  linearGaugeData.value.value = Math.random() * 100
}

const handleLinearAlarm = (zone: AlarmZone, value: number) => {
  addAlarmLog('线性仪表', zone.label || '线性仪表异常', value, '%', 'warning')
}

const toggleVerticalStream = () => {
  if (verticalStreaming.value) {
    verticalStreaming.value = false
    if (verticalTimer) {
      clearInterval(verticalTimer)
      verticalTimer = null
    }
  } else {
    verticalStreaming.value = true
    verticalTimer = setInterval(() => {
      const oldLevel = verticalGaugeData.value.value
      verticalGaugeData.value.value = Math.max(0, Math.min(100, oldLevel + (Math.random() - 0.5) * 5))
    }, 1500)
  }
}

const handleVerticalAlarm = (zone: AlarmZone, value: number) => {
  addAlarmLog('液位传感器', zone.label || '液位异常', value, 'L', 'danger')
}

const toggleLinearOrientation = () => {
  configurableLinearOptions.value.orientation = 
    configurableLinearOptions.value.orientation === 'horizontal' ? 'vertical' : 'horizontal'
  ElMessage.info(`已切换到${configurableLinearOptions.value.orientation === 'horizontal' ? '水平' : '垂直'}方向`)
}

const handleOrientationChange = (orientation: 'horizontal' | 'vertical') => {
  ElMessage.info(`方向已更改为: ${orientation === 'horizontal' ? '水平' : '垂直'}`)
}

// 温度计方法
const updateBasicThermometer = () => {
  basicThermometerData.value.value = Math.random() * 50 - 10 // -10 到 40 度
}

const toggleIndustrialThermometer = () => {
  if (industrialThermometerStreaming.value) {
    industrialThermometerStreaming.value = false
    if (industrialThermometerTimer) {
      clearInterval(industrialThermometerTimer)
      industrialThermometerTimer = null
    }
  } else {
    industrialThermometerStreaming.value = true
    industrialThermometerTimer = setInterval(() => {
      const oldTemp = industrialThermometerData.value.value
      industrialThermometerData.value.value = Math.max(0, Math.min(200, oldTemp + (Math.random() - 0.5) * 10))
    }, 2000)
  }
}

const handleIndustrialThermometerAlarm = (zone: AlarmZone, value: number) => {
  addAlarmLog('工业温度计', zone.label || '工业温度异常', value, '°C', 'danger')
}

const toggleThermometerUnit = () => {
  const units = ['°C', '°F', 'K']
  const currentIndex = units.indexOf(configurableThermometerData.value.unit || '°C')
  configurableThermometerData.value.unit = units[(currentIndex + 1) % units.length]
  ElMessage.info(`温度单位已切换为: ${configurableThermometerData.value.unit}`)
}

const handleThermometerUnitChange = (unit: string) => {
  ElMessage.info(`温度单位已更改为: ${unit}`)
}

// 压力表方法
const updateBasicPressureGauge = () => {
  basicPressureGaugeData.value.value = Math.random() * 10 // 0 到 10 MPa
}

const toggleIndustrialPressureGauge = () => {
  if (industrialPressureGaugeStreaming.value) {
    industrialPressureGaugeStreaming.value = false
    if (industrialPressureGaugeTimer) {
      clearInterval(industrialPressureGaugeTimer)
      industrialPressureGaugeTimer = null
    }
  } else {
    industrialPressureGaugeStreaming.value = true
    industrialPressureGaugeTimer = setInterval(() => {
      const oldPressure = industrialPressureGaugeData.value.value
      industrialPressureGaugeData.value.value = Math.max(0, Math.min(15, oldPressure + (Math.random() - 0.5) * 2))
    }, 2000)
  }
}

const handleIndustrialPressureGaugeAlarm = (zone: AlarmZone, value: number) => {
  addAlarmLog('工业压力表', zone.label || '工业压力异常', value, 'MPa', 'danger')
}

const togglePressureGaugeUnit = () => {
  const units = ['MPa', 'kPa', 'Pa', 'bar', 'psi', 'atm']
  const currentIndex = units.indexOf(configurablePressureGaugeData.value.unit || 'MPa')
  configurablePressureGaugeData.value.unit = units[(currentIndex + 1) % units.length]
  ElMessage.info(`压力单位已切换为: ${configurablePressureGaugeData.value.unit}`)
}

const handlePressureGaugeUnitChange = (unit: string) => {
  ElMessage.info(`压力单位已更改为: ${unit}`)
}

const handlePressureGaugeMinMaxRecord = (min: number, max: number) => {
  ElMessage.success(`压力表最值已记录: 最小 ${min.toFixed(2)}MPa, 最大 ${max.toFixed(2)}MPa`)
}

// 液位计方法
const updateBasicWaterLevel = () => {
  basicWaterLevelData.value.value = Math.random() * 100 // 0 到 100 L
}

const toggleIndustrialWaterLevel = () => {
  if (industrialWaterLevelStreaming.value) {
    industrialWaterLevelStreaming.value = false
    if (industrialWaterLevelTimer) {
      clearInterval(industrialWaterLevelTimer)
      industrialWaterLevelTimer = null
    }
  } else {
    industrialWaterLevelStreaming.value = true
    industrialWaterLevelTimer = setInterval(() => {
      const oldLevel = industrialWaterLevelData.value.value
      industrialWaterLevelData.value.value = Math.max(0, Math.min(150, oldLevel + (Math.random() - 0.5) * 8))
    }, 2000)
  }
}

const handleIndustrialWaterLevelAlarm = (zone: AlarmZone, value: number) => {
  addAlarmLog('工业液位计', zone.label || '工业液位异常', value, 'L', 'danger')
}

const toggleWaterLevelLiquidType = () => {
  const liquidTypes = ['水', '油', '酸', '酒精', '汞', '汽油']
  ElMessage.info(`液体类型已切换`)
}

const handleWaterLevelLiquidTypeChange = (liquidType: any) => {
  ElMessage.info(`液体类型已更改为: ${liquidType.name}`)
}

// 储罐方法
const updateBasicTank = () => {
  basicTankData.value.value = Math.random() * 200 // 0 到 200 L
}

const toggleIndustrialTank = () => {
  if (industrialTankStreaming.value) {
    industrialTankStreaming.value = false
    if (industrialTankTimer) {
      clearInterval(industrialTankTimer)
      industrialTankTimer = null
    }
  } else {
    industrialTankStreaming.value = true
    industrialTankTimer = setInterval(() => {
      const oldLevel = industrialTankData.value.value
      industrialTankData.value.value = Math.max(0, Math.min(300, oldLevel + (Math.random() - 0.5) * 15))
    }, 2000)
  }
}

const handleIndustrialTankAlarm = (zone: AlarmZone, value: number) => {
  addAlarmLog('工业储罐', zone.label || '储罐液位异常', value, 'L', 'danger')
}

const handleTankValveChange = (valve: string, state: boolean) => {
  ElMessage.info(`${valve === 'inlet' ? '进料阀' : valve === 'outlet' ? '出料阀' : '排放阀'}已${state ? '开启' : '关闭'}`)
}

const toggleTankType = () => {
  ElMessage.info('储罐类型已切换')
}

const handleTankTypeChange = (tankType: any) => {
  ElMessage.info(`储罐类型已更改为: ${tankType.name}`)
}

// 流量计方法
const updateBasicFlowMeter = () => {
  basicFlowMeterData.value.value = Math.random() * 100 // 0 到 100 L/s
}

const toggleIndustrialFlowMeter = () => {
  if (industrialFlowMeterStreaming.value) {
    industrialFlowMeterStreaming.value = false
    if (industrialFlowMeterTimer) {
      clearInterval(industrialFlowMeterTimer)
      industrialFlowMeterTimer = null
    }
  } else {
    industrialFlowMeterStreaming.value = true
    industrialFlowMeterTimer = setInterval(() => {
      const oldFlow = industrialFlowMeterData.value.value
      industrialFlowMeterData.value.value = Math.max(0, Math.min(150, oldFlow + (Math.random() - 0.5) * 12))
    }, 2000)
  }
}

const handleIndustrialFlowMeterAlarm = (zone: AlarmZone, value: number) => {
  addAlarmLog('工业流量计', zone.label || '流量异常', value, 'L/s', 'danger')
}

const handleFlowTypeChange = (flowType: any) => {
  ElMessage.info(`流量类型已更改为: ${flowType.name}`)
}

const toggleFlowMeterType = () => {
  ElMessage.info('流量计类型已切换')
}

const handleTotalFlowUpdate = (total: number) => {
  ElMessage.info(`累计流量已更新: ${total.toFixed(2)}L`)
}

// 格式化时间
const formatTime = (date: Date) => {
  return date.toLocaleString('zh-CN')
}

// 生命周期
onMounted(() => {
  // 初始化一些示例数据
  updateConfigurableGauge()
})

onUnmounted(() => {
  stopMonitoring()
  if (temperatureTimer) {
    clearInterval(temperatureTimer)
  }
  if (verticalTimer) {
    clearInterval(verticalTimer)
  }
  if (industrialThermometerTimer) {
    clearInterval(industrialThermometerTimer)
  }
  if (industrialPressureGaugeTimer) {
    clearInterval(industrialPressureGaugeTimer)
  }
})
</script>

<style lang="scss" scoped>
.gauges-example {
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
  
  .config-panel {
    padding: 16px;
    background: #f5f7fa;
    border-radius: 4px;
    
    h4 {
      margin: 0 0 16px 0;
      color: #303133;
    }
    
    .config-group {
      margin-bottom: 16px;
      
      label {
        display: block;
        margin-bottom: 8px;
        font-size: 14px;
        font-weight: 500;
        color: #606266;
      }
      
      span {
        margin-left: 8px;
        font-size: 12px;
        color: #909399;
      }
    }
  }
  
  .monitoring-gauge {
    border: 1px solid #e4e7ed;
    border-radius: 4px;
    padding: 8px;
    background: white;
  }
}

@media (max-width: 768px) {
  .gauges-example {
    .card-header {
      flex-direction: column;
      gap: 8px;
      align-items: flex-start;
      
      .header-controls {
        flex-wrap: wrap;
      }
    }
  }
}
</style>
