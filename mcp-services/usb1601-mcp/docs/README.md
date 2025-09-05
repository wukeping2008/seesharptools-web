# USB-1601 MCP Service

## 概述

USB-1601 MCP Service 是一个实现了 Model Context Protocol (MCP) 的服务器，为 JYTEK USB-1601 数据采集卡提供标准化的控制接口。通过 MCP 协议，任何支持 MCP 的客户端（如 Claude Desktop）都可以直接控制 USB-1601 硬件设备。

## 功能特性

- ✅ **设备管理**: 自动发现、连接和断开USB-1601设备
- ✅ **模拟I/O**: 支持16通道模拟输入和2通道模拟输出
- ✅ **数字I/O**: 支持2个8位数字I/O端口
- ✅ **数据流**: 支持高速连续数据采集（最高200kHz）
- ✅ **标准协议**: 完全兼容MCP规范
- ✅ **跨平台**: 支持stdio和TCP通信方式

## 快速开始

### 前置条件

1. **硬件要求**
   - JYTEK USB-1601 数据采集卡
   - USB 2.0/3.0 端口

2. **软件要求**
   - .NET 8.0 Runtime
   - JYUSB1601驱动程序
   - Windows 10/11 (x64)

### 安装步骤

1. **克隆项目**
```bash
git clone https://github.com/yourusername/seesharptools-web.git
cd seesharptools-web/mcp-services/usb1601-mcp
```

2. **构建项目**
```bash
dotnet build
```

3. **运行服务**
```bash
dotnet run
```

### Claude Desktop 集成

1. 编辑 Claude Desktop 配置文件：
   - Windows: `%APPDATA%\Claude\claude_desktop_config.json`
   - macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`

2. 添加 MCP 服务器配置：
```json
{
  "mcpServers": {
    "usb1601": {
      "command": "D:\\path\\to\\USB1601MCP.exe"
    }
  }
}
```

3. 重启 Claude Desktop

## 使用示例

### 基本操作

```javascript
// 发现设备
const devices = await usb1601_discover();

// 连接设备
await usb1601_connect({ deviceId: "0" });

// 读取模拟输入
const value = await ai_read_single({ 
  deviceId: "0", 
  channel: 0 
});

// 写入模拟输出
await ao_write_single({ 
  deviceId: "0", 
  channel: 0, 
  value: 5.0 
});

// 断开连接
await usb1601_disconnect({ deviceId: "0" });
```

### 连续数据采集

```javascript
// 开始数据流
const stream = await stream_start({
  deviceId: "0",
  channels: [0, 1, 2, 3],
  sampleRate: 10000
});

// 读取流数据
const data = await stream_get_data({
  deviceId: "0",
  maxSamples: 1000
});

// 停止数据流
await stream_stop({ deviceId: "0" });
```

## API 参考

### 设备管理

#### usb1601_discover
发现所有连接的USB-1601设备
- 参数: 无
- 返回: 设备列表

#### usb1601_connect
连接到指定设备
- 参数: `deviceId` (string)
- 返回: 连接状态

#### usb1601_disconnect
断开设备连接
- 参数: `deviceId` (string)
- 返回: 断开状态

### 模拟I/O

#### ai_read_single
读取单个模拟输入通道
- 参数: 
  - `deviceId` (string)
  - `channel` (0-15)
- 返回: 电压值 (-10V to +10V)

#### ai_read_multiple
读取多个模拟输入通道
- 参数:
  - `deviceId` (string)
  - `channels` (array of integers)
  - `sampleCount` (optional, default: 1)
- 返回: 二维数组 [通道][样本]

#### ao_write_single
写入模拟输出
- 参数:
  - `deviceId` (string)
  - `channel` (0-1)
  - `value` (-10V to +10V)
- 返回: 写入状态

### 数字I/O

#### dio_read
读取数字输入端口
- 参数:
  - `deviceId` (string)
  - `port` (0-1)
- 返回: 8位端口值

#### dio_write
写入数字输出端口
- 参数:
  - `deviceId` (string)
  - `port` (0-1)
  - `value` (0-255)
- 返回: 写入状态

### 数据流

#### stream_start
开始连续数据采集
- 参数:
  - `deviceId` (string)
  - `channels` (array)
  - `sampleRate` (1-200000 Hz)
- 返回: 流ID和状态

#### stream_stop
停止数据流
- 参数:
  - `deviceId` (string)
- 返回: 停止状态

## 开发指南

### 项目结构
```
usb1601-mcp/
├── src/                    # 源代码
│   ├── Tools/             # MCP工具实现
│   ├── Models/            # 数据模型
│   └── Program.cs         # 主程序
├── config/                # 配置文件
├── docs/                  # 文档
├── tests/                 # 测试
└── scripts/               # 脚本
```

### 扩展开发

1. **添加新工具**
   - 在 `Tools` 目录创建新类
   - 在 `USB1601MCPServer.HandleListTools` 中注册
   - 在 `HandleToolCall` 中添加处理逻辑

2. **自定义配置**
   - 修改 `config/mcp.json`
   - 添加新的配置项到 `appsettings.json`

## 故障排除

### 常见问题

1. **设备未找到**
   - 检查USB连接
   - 确认驱动安装正确
   - 检查设备管理器

2. **连接失败**
   - 确认设备ID正确
   - 检查是否有其他程序占用设备
   - 重启设备和服务

3. **数据采集异常**
   - 检查采样率设置
   - 确认通道配置正确
   - 查看日志文件

### 日志位置
- 开发模式: 控制台输出
- 生产模式: `logs/usb1601-mcp-{Date}.log`

## 性能指标

- 最大采样率: 200 kHz
- 通道数: 16 AI + 2 AO
- 分辨率: 12位
- 缓冲区: 自动管理
- 延迟: <10ms (典型值)

## 许可证

MIT License

## 支持

- GitHub Issues: [项目Issues页面]
- 文档: [在线文档]
- 邮箱: support@seesharptools.com

## 贡献

欢迎提交 Pull Request 和 Issue！

## 更新日志

### v1.0.0 (2025-08-28)
- 初始版本发布
- 实现基本MCP协议
- 支持所有USB-1601基础功能
- 添加数据流支持