# SeeSharpTools Web

🚀 **世界首个Web化专业测控硬件驱动管理平台**

SeeSharpTools Web是一个革命性的Web平台，专为简仪科技的专业测控硬件设备设计，提供完整的Web化驱动管理和控制界面。

## 🎯 项目概述

本项目将传统的桌面测控软件完全Web化，实现了：
- **通用硬件驱动管理**：支持简仪科技所有.dll硬件驱动
- **实时数据可视化**：专业级图表和仪表盘组件
- **现代化用户界面**：基于Vue 3 + TypeScript的响应式设计
- **高性能后端服务**：ASP.NET Core 9.0 + SignalR实时通信

## 🏗️ 项目架构

```
SeeSharpTools-Web/
├── frontend/           # Vue 3 + TypeScript 前端应用
├── backend/           # ASP.NET Core 9.0 后端服务
├── docs/             # 项目文档
└── examples/         # 示例和演示
```

## ✨ 核心特性

### 🔧 后端服务 (ASP.NET Core 9.0)
- **MISD标准化接口层**：统一的设备管理接口
- **通用驱动管理架构**：支持运行时热插拔驱动
- **SignalR实时通信**：高性能数据流传输
- **RESTful API + Swagger**：完整的API文档

### 🎨 前端应用 (Vue 3 + TypeScript)
- **专业测控组件库**：仪表盘、图表、控制器
- **实时数据可视化**：WebGL加速的高性能图表
- **响应式设计**：支持桌面和移动设备
- **AI辅助开发**：智能控件生成器

### 📊 专业组件
- **图表组件**：EasyChart、StripChart、SpectrumChart
- **仪表组件**：CircularGauge、LinearGauge、Thermometer
- **指示器**：LED指示器、数字显示器
- **控制器**：开关、按钮阵列、滑块

## 🚀 快速开始

### 前端开发
```bash
cd frontend
npm install --registry https://registry.npmmirror.com
npm run dev
```

### 后端开发
```bash
cd backend/SeeSharpBackend
dotnet restore
dotnet run
```

## 📋 开发计划

详细的开发计划请查看：[DEVELOPMENT_PLAN.md](docs/DEVELOPMENT_PLAN.md)

## 🏆 技术创新

- **世界首创**：Web化专业测控硬件驱动管理
- **标准制定**：为测控行业Web化建立技术标准
- **架构突破**：通用驱动适配器设计模式
- **性能优化**：WebGL + SignalR高性能数据流

## 📖 文档

- [开发计划](docs/DEVELOPMENT_PLAN.md)
- [项目重组计划](docs/PROJECT_REORGANIZATION_PLAN.md)
- [API文档](http://localhost:5152/swagger) (后端运行时访问)

## 🤝 贡献

欢迎提交Issue和Pull Request来帮助改进项目。

## 📄 许可证

本项目采用MIT许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🌟 里程碑

- ✅ **第一阶段**：前端专业组件库开发完成
- ✅ **第二阶段**：后端MISD标准化接口层完成
- 🔄 **第三阶段**：前后端集成联调 (进行中)
- 📋 **第四阶段**：实际硬件驱动集成测试
- 📋 **第五阶段**：性能优化和生产部署

---

**SeeSharpTools Web** - 让专业测控设备的Web化成为现实！
