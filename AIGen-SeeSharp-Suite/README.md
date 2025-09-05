# AIGen-SeeSharp-Suite

## 🚀 AI驱动的C#代码生成平台

基于百度文心大模型的智能代码生成系统，专门用于生成JYTEK仪器控制和MISD标准的C#解决方案。

## ✨ 功能特点

- **智能代码生成**：基于自然语言描述自动生成完整的C#项目
- **MISD标准支持**：遵循Modular Instruments Software Dictionary标准
- **多模型选择**：支持ERNIE-4.5、ERNIE-X1、ERNIE-Speed等多个模型
- **知识库增强**：集成JYUSB-1601示例代码和MISD定义
- **代码验证**：使用Roslyn进行语法验证和格式化
- **一键下载**：生成的代码打包为标准.NET解决方案ZIP文件

## 📋 系统要求

- Windows 10/11
- .NET 8.0 SDK
- Node.js 18+
- 百度AI API访问权限

## 🚀 快速开始

### 方式一：使用启动脚本（推荐）

双击运行 `start.bat`，脚本会自动：
1. 检查环境依赖
2. 启动后端API服务（端口5100）
3. 启动前端界面（端口5173）
4. 自动打开浏览器

### 方式二：手动启动

1. **启动后端服务**
   ```bash
   cd backend/AIGenSeeSharpSuite.Backend
   dotnet run --urls http://localhost:5100
   ```

2. **启动前端服务**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

3. **访问应用**
   - 前端界面：http://localhost:5173
   - API测试：打开 test-api.html

## 🎯 使用指南

### 生成代码示例

1. 打开前端界面或测试页面
2. 输入需求描述，例如：
   - "创建一个Hello World控制台程序"
   - "创建JYUSB-1601数据采集程序，通道0，采样率1000Hz"
   - "生成一个使用MISD标准的模拟输入程序"
3. 选择AI模型（推荐使用ERNIE-4.5或ERNIE-Speed）
4. 点击"生成解决方案"
5. 自动下载生成的ZIP文件

### 模型选择建议

| 模型 | Token限制 | 速度 | 质量 | 适用场景 |
|------|----------|------|------|----------|
| ERNIE-Lite-8K | 8K | 最快 | 一般 | 简单程序 |
| ERNIE-Speed-128K | 128K | 快 | 良好 | 复杂程序 |
| ERNIE-X1-Turbo-32K | 32K | 中等 | 良好 | 中等复杂度 |
| ERNIE-4.5-Turbo-128K | 128K | 较慢 | 最佳 | 复杂需求 |

## 📁 项目结构

```
AIGen-SeeSharp-Suite/
├── backend/                 # ASP.NET Core 后端
│   └── AIGenSeeSharpSuite.Backend/
│       ├── Controllers/     # API控制器
│       ├── Services/        # 业务服务
│       └── Models/          # 数据模型
├── frontend/               # Vue 3 前端
│   └── src/
│       ├── views/          # 页面组件
│       └── components/     # UI组件
├── test-api.html          # API测试页面
├── start.bat              # 启动脚本
├── stop.bat               # 停止脚本
└── IMPROVEMENTS.md        # 改进文档
```

## 🔧 配置说明

### API密钥配置

1. **方式一：环境变量（推荐）**
   ```bash
   set BAIDU_AI_BEARER_TOKEN=your_token_here
   ```

2. **方式二：配置文件**
   编辑 `backend/AIGenSeeSharpSuite.Backend/appsettings.json`
   ```json
   {
     "BaiduAiBearerToken": "your_token_here"
   }
   ```

## 📊 性能优化

- **Token优化**：减少80%的prompt长度
- **知识库优化**：智能筛选相关代码片段
- **模型配置**：针对不同模型优化参数
- **缓存机制**：内存缓存提升响应速度

## 🐛 故障排除

### 常见问题

1. **"tokens_too_long"错误**
   - 原因：输入过长
   - 解决：使用较短的需求描述或选择支持更多token的模型

2. **服务无法启动**
   - 检查端口占用：`netstat -ano | findstr :5100`
   - 确保.NET SDK已安装：`dotnet --version`

3. **生成代码格式问题**
   - 系统会自动进行代码验证和格式化
   - 如仍有问题，可手动调整生成的代码

## 📝 更新日志

### v1.1.0 (2025-09-04)
- ✅ 修正MISD定义
- ✅ 优化Token使用
- ✅ 添加模型配置管理
- ✅ 集成Excel读取支持
- ✅ 添加代码验证功能
- ✅ 改进安全性和错误处理

## 📄 许可证

MIT License

## 👥 贡献

欢迎提交Issue和Pull Request！

## 📧 联系方式

如有问题或建议，请联系开发团队。

---

**提示**：首次使用前请确保已配置百度AI API密钥！