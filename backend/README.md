# SeeSharpTools Backend

基于MISD标准的专业测控仪器后端服务，提供标准化的硬件抽象层和RESTful API。

## 项目概述

SeeSharpTools Backend是一个基于ASP.NET Core 8.0的Web API项目，实现了简仪科技MISD（Modular Instrumentation Software Dictionary）标准，为测控仪器提供统一的软件接口。

### 主要特性

- **MISD标准支持**: 完整实现简仪科技MISD标准
- **硬件抽象层**: 统一的硬件设备访问接口
- **RESTful API**: 标准化的HTTP API接口
- **实时数据流**: 基于SignalR的实时数据传输
- **设备发现**: 自动发现和管理PXI、USB、PCIe设备
- **任务管理**: 完整的数据采集任务生命周期管理
- **高性能**: 优化的数据处理和传输性能
- **可扩展**: 模块化设计，易于扩展新设备支持

## 技术栈

- **框架**: ASP.NET Core 8.0
- **语言**: C# 12
- **数据库**: SQL Server / LocalDB
- **缓存**: Redis
- **时序数据库**: InfluxDB
- **实时通信**: SignalR
- **日志**: Serilog
- **文档**: Swagger/OpenAPI
- **依赖注入**: Microsoft.Extensions.DependencyInjection

## 项目结构

```
SeeSharpBackend/
├── Controllers/           # API控制器
│   └── MISDController.cs  # MISD API控制器
├── Models/               # 数据模型
│   └── MISD/            # MISD相关模型
│       └── MISDDefinition.cs
├── Services/            # 业务服务
│   └── MISD/           # MISD服务实现
│       ├── IMISDService.cs
│       └── MISDService.cs
├── Data/               # 数据文件
├── Drivers/            # 设备驱动
├── Logs/              # 日志文件
├── Program.cs         # 应用程序入口
├── appsettings.json   # 配置文件
└── SeeSharpBackend.csproj
```

## 快速开始

### 环境要求

- .NET 8.0 SDK
- Visual Studio 2022 或 VS Code
- SQL Server 2019+ 或 LocalDB
- Redis (可选)
- InfluxDB (可选)

### 安装步骤

1. **克隆项目**
   ```bash
   git clone <repository-url>
   cd seesharp-backend
   ```

2. **还原依赖**
   ```bash
   cd SeeSharpBackend
   dotnet restore
   ```

3. **配置数据库**
   ```bash
   dotnet ef database update
   ```

4. **运行项目**
   ```bash
   dotnet run
   ```

5. **访问API文档**
   打开浏览器访问: `https://localhost:5001`

## API文档

### 设备管理

- `GET /api/misd/devices` - 获取所有设备
- `GET /api/misd/devices/{id}` - 获取指定设备
- `GET /api/misd/devices/{id}/functions` - 获取设备支持的功能

### 任务管理

- `POST /api/misd/tasks` - 创建任务
- `GET /api/misd/tasks/{id}` - 获取任务信息
- `POST /api/misd/tasks/{id}/start` - 启动任务
- `POST /api/misd/tasks/{id}/stop` - 停止任务
- `GET /api/misd/tasks/{id}/status` - 获取任务状态
- `DELETE /api/misd/tasks/{id}` - 删除任务

### 数据操作

- `GET /api/misd/tasks/{id}/data` - 读取数据
- `POST /api/misd/tasks/{id}/data` - 写入数据
- `POST /api/misd/tasks/{id}/trigger` - 发送软件触发
- `POST /api/misd/tasks/{id}/wait` - 等待任务完成

## MISD标准

### 支持的设备类型

- **PXI设备**: PXI Express模块化仪器
- **USB设备**: USB接口测控设备
- **PCIe设备**: PCIe接口高速数据采集卡

### 支持的功能类型

- **AI (Analog Input)**: 模拟输入
- **AO (Analog Output)**: 模拟输出
- **DI (Digital Input)**: 数字输入
- **DO (Digital Output)**: 数字输出
- **CI (Counter Input)**: 计数器输入
- **CO (Counter Output)**: 计数器输出

### 任务配置示例

```json
{
  "taskName": "AI_Task_Example",
  "deviceId": 1,
  "taskType": "AI",
  "channels": [
    {
      "channelId": 0,
      "rangeLow": -10.0,
      "rangeHigh": 10.0,
      "terminal": "RSE",
      "coupling": "DC",
      "enableIEPE": false
    }
  ],
  "sampling": {
    "sampleRate": 1000.0,
    "samplesToAcquire": 1000,
    "mode": "Finite",
    "bufferSize": 10000
  },
  "trigger": {
    "type": "Immediate",
    "source": "",
    "edge": "Rising",
    "level": 0.0,
    "preTriggerSamples": 0
  }
}
```

## 实时数据流

使用SignalR Hub进行实时数据传输：

```javascript
// 连接到数据流Hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/datastream")
    .build();

// 加入数据组
await connection.invoke("JoinGroup", "task_1");

// 接收实时数据
connection.on("DataUpdate", (data) => {
    console.log("收到实时数据:", data);
});
```

## 配置说明

### appsettings.json

```json
{
  "MISD": {
    "DefinitionFile": "Data/MISD.xlsx",
    "DriversPath": "Drivers",
    "DeviceDiscovery": {
      "ScanInterval": 30000,
      "Timeout": 5000,
      "EnablePXI": true,
      "EnableUSB": true,
      "EnablePCIe": true
    },
    "DataAcquisition": {
      "DefaultBufferSize": 10000,
      "MaxBufferSize": 1000000,
      "DefaultTimeout": 10000,
      "StreamingInterval": 100
    }
  }
}
```

## 开发指南

### 添加新设备支持

1. **创建设备驱动包装器**
   ```csharp
   public class NewDeviceWrapper : IDeviceWrapper
   {
       // 实现设备特定的方法
   }
   ```

2. **注册设备类型**
   ```csharp
   services.AddScoped<IDeviceWrapper, NewDeviceWrapper>();
   ```

3. **更新MISD定义**
   在MISD.xlsx中添加新设备的功能定义

### 扩展API功能

1. **创建新控制器**
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class CustomController : ControllerBase
   {
       // 实现自定义API
   }
   ```

2. **添加服务依赖**
   ```csharp
   services.AddScoped<ICustomService, CustomService>();
   ```

## 部署

### Docker部署

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
EXPOSE 80
ENTRYPOINT ["dotnet", "SeeSharpBackend.dll"]
```

### IIS部署

1. 发布项目: `dotnet publish -c Release`
2. 配置IIS应用程序池
3. 部署到IIS站点

## 监控和日志

### 日志配置

使用Serilog进行结构化日志记录：

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/seesharp-backend-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

### 健康检查

访问 `/health` 端点获取服务健康状态。

## 故障排除

### 常见问题

1. **设备发现失败**
   - 检查设备驱动是否正确安装
   - 确认设备连接状态
   - 查看日志文件获取详细错误信息

2. **数据采集超时**
   - 调整超时配置
   - 检查设备状态
   - 优化采样参数

3. **API响应慢**
   - 启用Redis缓存
   - 优化数据库查询
   - 调整缓冲区大小

## 贡献指南

1. Fork项目
2. 创建功能分支
3. 提交更改
4. 创建Pull Request

## 许可证

版权所有 © 2025 简仪科技

## 联系方式

- 官网: https://www.jytek.com
- 邮箱: support@jytek.com
- 技术支持: tech@jytek.com

## 更新日志

### v1.0.0 (2025-01-09)
- 初始版本发布
- 实现MISD标准基础功能
- 支持PXI、USB、PCIe设备
- 提供完整的RESTful API
- 集成SignalR实时数据流
