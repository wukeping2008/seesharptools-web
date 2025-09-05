# SeeSharpTools-Web 项目记忆文件

## 项目概览
SeeSharpTools-Web 是一个基于 Vue 3 + ASP.NET Core 的数据采集与测试平台，集成了硬件驱动、AI测试、数据可视化等功能。

## 技术栈
- **前端**: Vue 3, TypeScript, Element Plus, ECharts, SignalR
- **后端**: ASP.NET Core 8.0, Entity Framework Core
- **硬件**: JYUSB-1601 数据采集卡, JY5500 信号发生器
- **AI**: Claude API 集成

## 关键目录结构
```
/frontend         - Vue 3 前端应用
  /src/components - UI组件库
  /src/services   - API服务层
  /src/views      - 页面视图
/backend          - ASP.NET Core 后端
  /Controllers    - API控制器
  /Services       - 业务服务
  /Drivers        - 硬件驱动适配器
```

## 开发规范

### 代码风格
- **C#**: 使用 PascalCase，遵循 .NET 规范
- **TypeScript**: 使用 camelCase，严格类型定义
- **Vue组件**: 使用 PascalCase 命名，Composition API
- **注释**: 中文注释，关键功能需详细说明

### Git提交规范
- feat: 新功能
- fix: 修复问题  
- refactor: 重构代码
- docs: 文档更新
- test: 测试相关
- chore: 构建/依赖更新

## 常用命令

### 前端开发
```bash
cd frontend
npm run dev          # 启动开发服务器
npm run build        # 构建生产版本
npm run lint         # 代码检查
npm run type-check   # TypeScript类型检查
```

### 后端开发
```bash
cd backend/SeeSharpBackend
dotnet run           # 启动后端服务
dotnet build         # 构建项目
dotnet test          # 运行测试
```

## 硬件配置
- USB-1601 默认设备ID: 0
- 采样率范围: 1Hz - 200kHz
- 通道数: 16路模拟输入
- 分辨率: 12位

## API密钥配置
Claude API密钥存储在 `appsettings.Local.json` (不纳入版本控制)

## 当前开发重点
1. 数据采集性能优化
2. 实时数据可视化
3. AI辅助测试功能
4. 硬件自动检测

## 已知问题
- 高采样率下数据缓冲区可能溢出
- WebSocket连接偶尔断开需重连
- 硬件模式切换需要重启服务

## 测试覆盖
- 单元测试: Services层
- 集成测试: Controllers层  
- E2E测试: 关键用户流程

## 部署信息
- 生产环境: Windows Server 2019+
- 需要安装: .NET 8.0 Runtime, Node.js 18+
- 硬件驱动: JYUSB1601.dll 需在系统PATH中

## 联系人
- 项目负责人: [待补充]
- 技术支持: [待补充]