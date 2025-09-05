# USB-1601 MCP调用方式说明

## 当前实现状态

### ✅ 已实现
1. **独立MCP服务** - 完整的MCP服务器，可被Claude Desktop调用
2. **Web API方式** - 通过HTTP API调用硬件
3. **统一硬件抽象层** - 共享的硬件接口定义

### ❌ 未实现
- MCP服务与Web应用的集成
- 从Web页面通过MCP调用硬件

## 两种调用方式对比

### 方式1：Web API调用（当前使用）
```
浏览器 → Vue前端 → HTTP API → ASP.NET Core → 硬件驱动 → USB-1601
```

**优点：**
- 直接集成在Web应用中
- 低延迟，高性能
- 易于调试和监控

**使用场景：**
- Web应用内部使用
- 需要实时数据流
- 需要细粒度控制

### 方式2：MCP服务调用（可选）
```
Claude Desktop → MCP协议 → USB1601MCP.exe → 硬件驱动 → USB-1601
```

**优点：**
- AI可直接控制硬件
- 标准化的MCP协议
- 独立进程，更稳定

**使用场景：**
- AI辅助测试
- 自动化脚本
- 跨应用集成

## 如何启用MCP方式

### 1. 配置Claude Desktop使用MCP

编辑 `%APPDATA%\Claude\claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "usb1601": {
      "command": "D:\\Documents\\seesharptools-web\\mcp-services\\usb1601-mcp\\publish\\USB1601MCP.exe"
    }
  }
}
```

重启Claude Desktop后，可以在Claude中直接说：
- "发现USB-1601设备"
- "读取AI通道0-3的数据"
- "设置AO通道0输出5V"

### 2. 从Web应用调用MCP（需要额外开发）

如果需要Web应用也能通过MCP调用硬件，需要：

1. **实现MCP客户端服务**
```csharp
// 在Program.cs中注册
builder.Services.AddSingleton<IMCPClientService, MCPClientService>();

// 在控制器中使用
[HttpPost("usemcp")]
public async Task<IActionResult> UseMCP([FromServices] IMCPClientService mcp)
{
    if (configuration["USB1601:MCPService:Enabled"] == "true")
    {
        // 通过MCP调用
        var result = await mcp.CallMCPToolAsync("usb1601_read_ai", new { channels = new[] {0,1,2,3} });
        return Ok(result);
    }
    else
    {
        // 直接调用硬件
        var data = await hardware.ReadAISingleAsync(new[] {0,1,2,3});
        return Ok(data);
    }
}
```

2. **修改配置启用MCP**
```json
// appsettings.json
"USB1601": {
  "MCPService": {
    "Enabled": true,  // 改为true
    "AutoStart": true  // 自动启动MCP服务
  }
}
```

## 架构选择建议

### 推荐：保持当前Web API方式
- ✅ 已经完整实现并测试
- ✅ 性能最优，延迟最低
- ✅ 易于维护和调试

### MCP作为补充功能
- 用于AI集成场景
- 用于跨应用调用
- 用于自动化测试

## 测试MCP服务

### 直接测试MCP服务
```bash
# 启动MCP服务
cd mcp-services/usb1601-mcp/publish
USB1601MCP.exe

# 在另一个终端测试（需要MCP客户端工具）
echo '{"jsonrpc":"2.0","method":"tools/list","id":1}' | USB1601MCP.exe
```

### 通过Claude Desktop测试
1. 配置claude_desktop_config.json
2. 重启Claude Desktop
3. 在Claude中输入："请列出所有可用的USB-1601工具"

## 总结

当前系统**支持两种独立的硬件调用方式**：
1. **Web API方式** - 主要使用，完整集成
2. **MCP服务方式** - 可选功能，适合AI集成

两种方式使用相同的硬件驱动（JYUSB1601.dll），但调用路径完全独立。如需要统一集成，需要实现MCP客户端服务。