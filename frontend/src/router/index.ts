import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'

const routes: Array<RouteRecordRaw> = [
  // 首页
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/HomeView.vue')
  },
  {
    path: '/download',
    name: 'Download',
    component: () => import('../views/DownloadView.vue')
  },
  {
    path: '/backend-integration-test',
    name: 'BackendIntegrationTest',
    component: () => import('../views/BackendIntegrationTest.vue')
  },
  {
    path: '/data-storage-test',
    name: 'DataStorageTest',
    component: () => import('../views/DataStorageTest.vue')
  },
  // 基础控件
  {
    path: '/instruments',
    name: 'Instruments',
    component: () => import('@/views/InstrumentsView.vue')
  },
  {
    path: '/indicators',
    name: 'Indicators',
    component: () => import('@/views/IndicatorsView.vue')
  },
  {
    path: '/controls',
    name: 'Controls',
    component: () => import('@/views/ControlsView.vue')
  },
  // 图表控件
  {
    path: '/waveform',
    name: 'Waveform',
    component: () => import('@/views/WaveformView.vue')
  },
  {
    path: '/stripchart',
    name: 'StripChart',
    component: () => import('@/views/StripChartView.vue')
  },
  {
    path: '/enhanced-charts',
    name: 'EnhancedCharts',
    component: () => import('@/views/EnhancedChartsView.vue')
  },
  {
    path: '/spectrum',
    name: 'Spectrum',
    component: () => import('@/views/SpectrumView.vue')
  },
  // 仪器控件
  {
    path: '/oscilloscope',
    name: 'Oscilloscope',
    component: () => import('@/views/OscilloscopeView.vue')
  },
  {
    path: '/signal-generator',
    name: 'SignalGenerator',
    component: () => import('@/views/SignalGeneratorView.vue')
  },
  {
    path: '/data-acquisition',
    name: 'DataAcquisition',
    component: () => import('@/views/DataAcquisitionView.vue')
  },
  {
    path: '/power-supply',
    name: 'PowerSupply',
    component: () => import('@/views/PowerSupplyView.vue')
  },
  // 综合示例
  {
    path: '/examples',
    name: 'Examples',
    component: () => import('@/views/examples/AllExamples.vue')
  },
  {
    path: '/charts',
    name: 'Charts',
    component: () => import('@/views/examples/ChartsExample.vue')
  },
  {
    path: '/gauges',
    name: 'Gauges',
    component: () => import('@/views/examples/GaugesExample.vue')
  },
  {
    path: '/indicators-example',
    name: 'IndicatorsExample',
    component: () => import('@/views/examples/IndicatorsExample.vue')
  },
  // 测试页面
  {
    path: '/linear-gauge-test',
    name: 'LinearGaugeTest',
    component: () => import('@/views/LinearGaugeTest.vue')
  },
  {
    path: '/thermometer-test',
    name: 'ThermometerTest',
    component: () => import('@/views/ThermometerTest.vue')
  },
  {
    path: '/digital-display-test',
    name: 'DigitalDisplayTest',
    component: () => import('@/views/DigitalDisplayTest.vue')
  },
  {
    path: '/switch-test',
    name: 'SwitchTest',
    component: () => import('@/views/SwitchTest.vue')
  },
  {
    path: '/button-array-test',
    name: 'ButtonArrayTest',
    component: () => import('@/views/ButtonArrayTest.vue')
  },
  {
    path: '/enhanced-stripchart-test',
    name: 'EnhancedStripChartTest',
    component: () => import('@/views/EnhancedStripChartTest.vue')
  },
  {
    path: '/spectrum-chart-test',
    name: 'SpectrumChartTest',
    component: () => import('@/views/SpectrumChartTest.vue')
  },
  {
    path: '/advanced-easy-chart-test',
    name: 'AdvancedEasyChartTest',
    component: () => import('@/views/AdvancedEasyChartTest.vue')
  },
  {
    path: '/professional-easy-chart-test',
    name: 'ProfessionalEasyChartTest',
    component: () => import('@/views/ProfessionalEasyChartTest.vue')
  },
  {
    path: '/dual-axis-easychart-test',
    name: 'DualAxisEasyChartTest',
    component: () => import('@/views/DualAxisEasyChartTest.vue')
  },
    {
      path: '/temperature-acquisition-card-test',
      name: 'TemperatureAcquisitionCardTest',
      component: () => import('@/views/TemperatureAcquisitionCardTest.vue')
    },
    {
      path: '/signal-generator-test',
      name: 'SignalGeneratorTest',
      component: () => import('@/views/SignalGeneratorTest.vue')
    },
    {
      path: '/digital-multimeter-test',
      name: 'DigitalMultimeterTest',
      component: () => import('@/views/DigitalMultimeterTest.vue')
    },
    {
      path: '/oscilloscope-test',
      name: 'OscilloscopeTest',
      component: () => import('@/views/OscilloscopeTest.vue')
    },
    {
      path: '/dio-card-test',
      name: 'DIOCardTest',
      component: () => import('@/views/DIOCardTest.vue')
    },
    {
      path: '/backend-integration-test',
      name: 'BackendIntegrationTest',
      component: () => import('@/views/BackendIntegrationTest.vue')
    },
    {
      path: '/hardware-driver-test',
      name: 'HardwareDriverTest',
      component: () => import('@/views/HardwareDriverTest.vue')
    },
    {
      path: '/ai-control-generator',
      name: 'AIControlGenerator',
      component: () => import('@/views/AIControlGeneratorTest.vue')
    },
    {
      path: '/performance-monitoring',
      name: 'PerformanceMonitoring',
      component: () => import('@/views/PerformanceMonitoringView.vue')
    },
    {
      path: '/data-analysis-test',
      name: 'DataAnalysisTest',
      component: () => import('@/views/DataAnalysisTest.vue')
    },
    {
      path: '/project-developer',
      name: 'ProjectDeveloper',
      component: () => import('@/views/ProjectDeveloperView.vue')
    },
    {
      path: '/csharp-runner-test',
      name: 'CSharpRunnerTest',
      component: () => import('@/views/CSharpRunnerTest.vue')
    },
]

const router = createRouter({
  history: createWebHistory('/'),
  routes
})

export default router
