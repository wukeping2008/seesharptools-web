# USB-1601 智能数据采集系统 - 安装配置指南

## 系统要求

- Windows 10/11 64位
- .NET 8.0 SDK
- Node.js 18+
- USB-1601硬件设备及驱动

## 快速开始

### 1. 配置百度AI API

编辑 `backend/USB1601Service/appsettings.json`，填入您的百度AI API密钥：

```json
"BaiduAI": {
  "ApiKey": "您的API_KEY",
  "SecretKey": "您的SECRET_KEY"
}
```

获取密钥：访问 [百度智能云控制台](https://console.bce.baidu.com/ai/#/ai/wenxinworkshop/overview/index)

### 2. 硬件连接

1. 连接USB-1601设备到计算机
2. 安装JYUSB驱动（如未安装）
3. 设备管理器中确认设备正常识别

### 3. 启动系统

双击运行 `start.bat` 即可自动：
- 启动后端API服务
- 安装前端依赖（首次运行）
- 启动前端开发服务器
- 打开浏览器

### 4. 手动启动

如需手动启动：

```bash
# 后端
cd backend/USB1601Service
dotnet run

# 前端
cd frontend
npm install  # 首次运行
npm run dev
```

## 功能说明

### 数据采集
- 支持1/2/4/8通道同时采集
- 采样率：1kHz - 100kHz
- 电压范围：±1V/±2V/±5V/±10V
- 自测模式：AO生成测试信号，AI采集验证

### AI分析功能
- **波形分析**：识别信号类型、频率成分
- **异常检测**：实时监测数据异常
- **报告生成**：自动生成测试分析报告

### 实时通信
- 基于SignalR的WebSocket实时数据传输
- 低延迟波形显示
- 异常实时告警

## 常见问题

### Q: 提示找不到USB-1601设备
A: 请检查：
- 设备是否正确连接
- 驱动是否安装
- 设备索引是否正确（默认为0）

### Q: AI分析失败
A: 请检查：
- 百度AI API密钥是否正确
- 网络连接是否正常
- API额度是否充足

### Q: 波形显示不正常
A: 尝试：
- 点击"自动缩放"按钮
- 调整采样率
- 检查电压范围设置

## 技术支持

如遇问题，请查看：
- 系统日志：`backend/USB1601Service/Logs/`
- 浏览器控制台（F12）
- 项目Issue页面