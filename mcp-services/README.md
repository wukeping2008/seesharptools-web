# MCP Services

此目录包含各种硬件设备的 MCP (Model Context Protocol) 服务实现。

## 什么是 MCP?

MCP (Model Context Protocol) 是一个标准化协议，允许 AI 模型（如 Claude）通过标准接口与外部工具和服务进行交互。通过 MCP，AI 可以：
- 控制硬件设备
- 访问数据库
- 调用外部API
- 执行系统命令

## 可用服务

### USB-1601 MCP Service
为 JYTEK USB-1601 数据采集卡提供 MCP 接口。

- **目录**: `usb1601-mcp/`
- **功能**: 
  - 16通道模拟输入 (AI)
  - 2通道模拟输出 (AO)
  - 16位数字I/O (DIO)
  - 高速数据采集（最高200kHz）
- **状态**: ✅ 已实现

## 快速开始

### 1. 构建服务
```bash
cd usb1601-mcp
dotnet build
```

### 2. 运行服务
```bash
dotnet run
```

或使用脚本：
```bash
scripts\run.bat
```

### 3. 配置 Claude Desktop

编辑 Claude Desktop 配置文件，添加 MCP 服务器：

**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
```json
{
  "mcpServers": {
    "usb1601": {
      "command": "D:\\path\\to\\mcp-services\\usb1601-mcp\\publish\\USB1601MCP.exe"
    }
  }
}
```

## 开发新的 MCP 服务

### 项目模板结构
```
service-name-mcp/
├── src/
│   ├── Program.cs           # 主程序入口
│   ├── MCPServer.cs         # MCP服务器实现
│   ├── Tools/               # 工具实现
│   ├── Models/              # 数据模型
│   └── Services/            # 业务逻辑
├── config/
│   ├── mcp.json            # MCP配置
│   └── appsettings.json    # 应用配置
├── tests/                   # 单元测试
├── docs/                    # 文档
├── scripts/                 # 构建脚本
└── ServiceName.csproj       # 项目文件
```

### 实现步骤

1. **创建项目**
```bash
dotnet new console -n YourDevice-MCP
```

2. **实现 MCP 协议**
- 继承 `BackgroundService`
- 实现 JSON-RPC 通信
- 注册工具和功能

3. **添加设备控制**
- 创建设备适配器
- 实现具体功能
- 处理错误和异常

4. **测试和文档**
- 编写单元测试
- 创建使用文档
- 提供示例代码

## MCP 协议规范

### 请求格式
```json
{
  "jsonrpc": "2.0",
  "id": "1",
  "method": "tools/call",
  "params": {
    "name": "tool_name",
    "arguments": {
      "param1": "value1"
    }
  }
}
```

### 响应格式
```json
{
  "jsonrpc": "2.0",
  "id": "1",
  "result": {
    "content": [{
      "type": "text",
      "text": "Result data"
    }]
  }
}
```

## 最佳实践

1. **错误处理**
   - 总是返回有意义的错误信息
   - 使用标准错误代码
   - 记录详细日志

2. **性能优化**
   - 使用异步操作
   - 实现数据缓冲
   - 避免阻塞调用

3. **安全考虑**
   - 验证所有输入
   - 限制资源使用
   - 实现访问控制

4. **文档规范**
   - 清晰的API文档
   - 使用示例
   - 故障排除指南

## 支持的硬件

| 设备 | 型号 | 状态 | 文档 |
|------|------|------|------|
| 数据采集卡 | USB-1601 | ✅ 已实现 | [README](usb1601-mcp/docs/README.md) |
| 信号发生器 | JY5500 | 🔄 计划中 | - |
| 示波器 | - | 📋 待定 | - |

## 贡献指南

欢迎贡献新的 MCP 服务实现！请遵循以下步骤：

1. Fork 项目
2. 创建特性分支
3. 实现功能
4. 编写测试
5. 提交 Pull Request

## 许可证

MIT License

## 联系方式

- Issues: GitHub Issues
- Email: support@seesharptools.com
- 文档: [在线文档]

---
*最后更新: 2025-08-28*