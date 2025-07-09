# SeeSharpTools Web

基于 Vue 3 + TypeScript + Element Plus 的专业工业控件库，提供丰富的仪表盘、图表和指示器控件。

## 🚀 项目概述

SeeSharpTools Web 是一个专业级的测试测量仪器控件库，旨在将传统桌面测控软件的功能完整移植到Web平台。项目参考简仪科技的SeeSharpTools设计风格，提供世界领先的Web化测控解决方案。

## ✨ 核心特性

- **专业控件库**：工业级控件，包括仪表盘、图表和指示器
- **Vue 3 + TypeScript**：现代前端技术栈
- **响应式设计**：适配各种屏幕尺寸
- **高度可配置**：丰富的配置选项和主题定制
- **实时交互**：支持实时数据更新和用户交互
- **专业外观**：符合传统测控仪器的操作习惯

## 🚀 已实现功能

### 核心图表控件
- ✅ **StripChart 条带图控件** - 高速数据流显示
  - 实时数据流处理和显示
  - 多通道同步显示
  - 高性能渲染优化
  - 数据压缩和缓冲管理

- ✅ **增强型 EasyChart** - FFT频谱分析集成
  - 实时FFT计算和显示
  - 多种窗函数支持
  - 频域和时域联动分析
  - 专业测量工具集成

### 专业仪器控件
- ✅ **信号发生器控件** - 任意波形生成
  - 标准波形生成（正弦、方波、三角波、锯齿波）
  - 参数精确控制（频率、幅度、相位、偏置）
  - 调制功能（AM/FM/PM）
  - 扫频和突发模式

- ✅ **数字示波器控件** - 多通道波形分析
  - 4通道同步显示
  - 完整触发系统
  - 自动测量功能
  - 专业时基控制

- ✅ **多功能数据采集卡控件** - 高速数据采集
  - 4通道同步采集（1kS/s - 2MS/s）
  - 智能触发系统（边沿/电平/窗口）
  - 实时统计分析（平均值、RMS、最值、标准差）
  - 校准诊断功能（自校准、通道测试、噪声测试）
  - CSV数据导出

### 基础控件库
- ✅ **圆形仪表盘** - 经典圆形仪表，支持多种样式和配置
- ✅ **线性仪表** - 线性进度显示，支持水平和垂直布局
- ✅ **温度计** - 温度显示控件，支持摄氏度和华氏度
- ✅ **压力表** - 专业压力显示，支持多种压力单位
- ✅ **水位计** - 液位显示控件，支持各种液体类型
- ✅ **储罐** - 储罐液位显示，支持圆形和矩形储罐
- ✅ **流量计** - 流量监控显示，支持各种流量类型
- ✅ **LED指示器** - 多功能LED指示器，支持各种形状和布局
- ✅ **数字万用表** - 精密测量仪器，8种测量功能

## 🛠️ 技术栈

- **前端框架**：Vue 3
- **开发语言**：TypeScript
- **UI组件库**：Element Plus
- **构建工具**：Vite
- **样式预处理器**：SCSS
- **图表库**：ECharts
- **路由管理**：Vue Router

## 📁 项目结构

```
seesharptools-web/
├── src/
│   ├── components/           # 组件目录
│   │   ├── charts/          # 图表控件
│   │   │   ├── StripChart.vue
│   │   │   ├── EnhancedEasyChart.vue
│   │   │   └── WaveformChart.vue
│   │   ├── instruments/     # 仪器控件
│   │   │   ├── SignalGenerator.vue
│   │   │   ├── Oscilloscope.vue
│   │   │   ├── DataAcquisitionCard.vue
│   │   │   └── DigitalMultimeter.vue
│   │   └── professional/    # 专业控件
│   │       ├── charts/      # 图表控件
│   │       ├── gauges/      # 仪表控件
│   │       └── indicators/  # 指示器控件
│   ├── views/              # 页面视图
│   │   ├── StripChartView.vue
│   │   ├── SignalGeneratorView.vue
│   │   ├── OscilloscopeView.vue
│   │   ├── DataAcquisitionView.vue
│   │   └── examples/       # 示例页面
│   ├── types/              # TypeScript类型定义
│   ├── styles/             # 样式文件
│   └── router/             # 路由配置
├── docs/                   # 文档
│   ├── ARCHITECTURE.md     # 架构文档
│   ├── API_SPECIFICATION.md # API规范
│   └── STRIPCHART_DESIGN.md # StripChart设计文档
├── public/                 # 静态资源
└── DEVELOPMENT_PLAN.md     # 开发计划
```

## 🚀 快速开始

### 安装依赖

```bash
npm install
```

### 启动开发服务器

```bash
npm run dev
```

### 构建生产版本

```bash
npm run build
```

### 预览生产构建

```bash
npm run preview
```

## 📖 使用示例

### 信号发生器

```vue
<template>
  <SignalGenerator
    @waveform-change="handleWaveformChange"
    @parameter-change="handleParameterChange"
    @output-change="handleOutputChange"
  />
</template>

<script setup lang="ts">
import SignalGenerator from '@/components/instruments/SignalGenerator.vue'

const handleWaveformChange = (waveform: any) => {
  console.log('波形变化:', waveform)
}

const handleParameterChange = (params: any) => {
  console.log('参数变化:', params)
}

const handleOutputChange = (enabled: boolean) => {
  console.log('输出状态:', enabled)
}
</script>
```

### 数字示波器

```vue
<template>
  <Oscilloscope
    @running-change="handleRunningChange"
    @trigger-event="handleTriggerEvent"
    @measurement-update="handleMeasurementUpdate"
  />
</template>

<script setup lang="ts">
import Oscilloscope from '@/components/instruments/Oscilloscope.vue'

const handleRunningChange = (running: boolean) => {
  console.log('示波器运行状态:', running)
}

const handleTriggerEvent = (triggerInfo: any) => {
  console.log('触发事件:', triggerInfo)
}

const handleMeasurementUpdate = (measurements: any[]) => {
  console.log('测量更新:', measurements)
}
</script>
```

### 数据采集卡

```vue
<template>
  <DataAcquisitionCard
    @acquisition-start="handleAcquisitionStart"
    @acquisition-stop="handleAcquisitionStop"
    @data-ready="handleDataReady"
    @channel-update="handleChannelUpdate"
  />
</template>

<script setup lang="ts">
import DataAcquisitionCard from '@/components/instruments/DataAcquisitionCard.vue'

const handleAcquisitionStart = (config: any) => {
  console.log('数据采集开始:', config)
}

const handleAcquisitionStop = () => {
  console.log('数据采集停止')
}

const handleDataReady = (data: any) => {
  console.log('数据就绪:', data)
}

const handleChannelUpdate = (channelIndex: number, config: any) => {
  console.log('通道更新:', channelIndex, config)
}
</script>
```

### 圆形仪表盘

```vue
<template>
  <CircularGauge
    :data="gaugeData"
    :options="gaugeOptions"
    @value-change="handleValueChange"
  />
</template>

<script setup lang="ts">
import CircularGauge from '@/components/professional/gauges/CircularGauge.vue'

const gaugeData = ref({
  value: 75,
  min: 0,
  max: 100,
  unit: '%',
  title: 'Progress'
})

const gaugeOptions = ref({
  size: 200,
  showValue: true,
  showTitle: true,
  colorStops: [
    { offset: 0, color: '#00ff00' },
    { offset: 0.8, color: '#ffff00' },
    { offset: 1, color: '#ff0000' }
  ]
})
</script>
```

## 🎯 开发路线图

### 当前进展（第一阶段）
- ✅ **核心图表控件**：StripChart、增强型EasyChart
- ✅ **专业仪器控件**：信号发生器、示波器、数据采集卡
- ✅ **基础控件库**：仪表盘、指示器、数字万用表
- 🔄 **温度采集卡控件**（基于DigitalMultimeter扩展）
- 🔄 **DIO/开关卡控件**（基于LEDIndicator扩展）

### 即将推出
- 🔄 **AI驱动的自定义控件系统**
- 🔄 **更多专业仪器控件**
- 🔄 **高级数学功能**
- 🔄 **数据回放功能**

### 未来计划
- 📊 **后端硬件集成平台**
- 🎨 **主题定制系统**
- 📱 **移动端优化**
- 🌐 **国际化支持**

## 💡 技术创新点

### 1. Web端专业测控平台
- 首个完整的Web版专业测控平台
- 媲美传统桌面软件的功能完整性
- 现代化的Web界面设计

### 2. 高性能数据可视化
- 支持高速数据流的Web实时显示
- 创新的数据压缩和渲染算法
- 基于Canvas/WebGL的高性能图形渲染

### 3. 模块化组件架构
- 完全模块化的组件设计
- 易于扩展和定制
- 支持插件化功能扩展

### 4. 专业级精度
- 纳秒级时基分辨率
- 毫伏级电压分辨率
- 专业级测量算法

## 🎨 设计理念

### 专业美学
- **简洁白色背景**：遵循科学图表标准
- **专业网格系统**：精确的测量网格
- **科学配色方案**：优化数据可视化
- **仪器级样式**：专业测试设备外观

### 性能优化
- **大数据集处理**：LTTB采样实现流畅性能
- **实时更新**：高效数据流传输
- **内存管理**：智能缓冲区管理
- **响应式设计**：适配所有屏幕尺寸

## 🤝 贡献指南

欢迎贡献代码！请遵循以下指南：

1. Fork 仓库
2. 创建功能分支
3. 进行更改
4. 添加测试（如适用）
5. 提交 Pull Request

### 开发指南

- 遵循 TypeScript 最佳实践
- 使用 Vue 3 Composition API
- 保持专业样式标准
- 包含全面的文档
- 在多种屏幕尺寸上测试

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件。

## 🙏 致谢

- **SeeSharpTools**：EasyChart样式和功能的灵感来源
- **LabVIEW**：专业仪器控制概念
- **Element Plus**：现代Vue 3 UI框架
- **ECharts**：高性能图表库

## 📞 支持

如有问题、问题或功能请求：

- 在GitHub上创建issue
- 查看文档
- 查看示例实现

## 🔄 版本历史

### v1.2.0 (当前版本)
- ✅ **多功能数据采集卡控件** - 4通道高速数据采集
- ✅ **数字示波器控件** - 多通道波形分析
- ✅ **信号发生器控件** - 任意波形生成
- ✅ **增强型EasyChart** - FFT频谱分析集成
- ✅ **StripChart条带图控件** - 高速数据流显示

### v1.1.0
- ✅ **专业仪表控件** - 圆形仪表盘、线性仪表、温度计等
- ✅ **指示器控件** - LED指示器、数字显示等
- ✅ **数字万用表** - 8种测量功能

### v1.0.0
- ✅ **基础框架** - Vue 3 + TypeScript + Element Plus
- ✅ **项目架构** - 模块化组件设计
- ✅ **基础控件** - 图表和仪表基础功能

### 主要功能完成情况
- 🎛️ **8个主要导航部分**：首页、仪器控件、指示器、控制器、示波器、频谱分析、信号发生器、数据采集
- 📊 **专业波形图表**：EasyChart风格的科学绘图
- 🔬 **完整仪器套件**：所有主要测试测量仪器
- 🎚️ **综合控制库**：全系列交互控件和指示器
- 📱 **移动响应式设计**：在所有设备尺寸上无缝工作
- 🎨 **科学美学**：专业仪器级样式
- ⚡ **高性能**：针对实时数据可视化优化

## 🔗 相关链接

- [Vue 3 文档](https://vuejs.org/)
- [TypeScript 文档](https://www.typescriptlang.org/)
- [Element Plus 文档](https://element-plus.org/)
- [ECharts 文档](https://echarts.apache.org/)
- [项目GitHub仓库](https://github.com/wukeping2008/seesharptools-web)

---

**SeeSharpTools Web** - 专业工业控件库，让数据可视化更简单！

**为测试测量社区用 ❤️ 构建**
