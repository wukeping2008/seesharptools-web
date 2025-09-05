# USB-1601 智能数据采集Web应用

## 项目概述
独立的Web应用，用于控制简仪科技USB-1601数据采集卡，并集成百度AI大模型进行智能数据分析。

## 主要功能
- ✅ 实时数据采集（16通道AI/2通道AO）
- ✅ 波形实时显示
- ✅ AI智能分析（百度文心一言）
- ✅ 异常检测与预警
- ✅ 自动报告生成

## 技术栈
- **前端**: Vue 3 + TypeScript + Element Plus + ECharts
- **后端**: ASP.NET Core 8.0 + SignalR
- **硬件**: JYUSB1601.dll驱动
- **AI**: 百度文心一言API

## 快速开始

### 1. 安装依赖
```bash
# 后端
cd backend
dotnet restore

# 前端
cd frontend
npm install
```

### 2. 配置
- 配置USB-1601硬件连接
- 设置百度AI API密钥（在appsettings.json中）

### 3. 启动应用
```bash
# 启动后端
cd backend
dotnet run

# 启动前端
cd frontend
npm run dev
```

### 4. 访问应用
打开浏览器访问: http://localhost:5173

## 项目结构
```
usb1601-web-app/
├── frontend/          # Vue 3前端应用
├── backend/           # ASP.NET Core后端服务
├── drivers/           # USB-1601驱动文件
└── docs/             # 项目文档
```

## API文档
详见 [API文档](./docs/API.md)

## 开发者
SeeSharpTools Team

## 许可证
MIT License