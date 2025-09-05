# Claude Desktop 集成配置指南

## 快速配置

### 1. 构建发布版本
```bash
cd mcp-services/usb1601-mcp
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish
```

### 2. 配置Claude Desktop

找到Claude Desktop配置文件：
- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
- 通常路径: `C:\Users\你的用户名\AppData\Roaming\Claude\claude_desktop_config.json`

编辑配置文件，添加USB-1601 MCP服务：

```json
{
  "mcpServers": {
    "usb1601": {
      "command": "D:\\Documents\\seesharptools-web\\mcp-services\\usb1601-mcp\\publish\\USB1601MCP.exe"
    }
  }
}
```

### 3. 重启Claude Desktop
完全关闭并重新启动Claude Desktop应用程序。

### 4. 验证集成
在Claude中输入以下命令测试：
```
请发现所有连接的USB-1601设备
```

## 配置选项

### 基本配置
```json
{
  "mcpServers": {
    "usb1601": {
      "command": "路径\\USB1601MCP.exe"
    }
  }
}
```

### 高级配置（带参数）
```json
{
  "mcpServers": {
    "usb1601": {
      "command": "路径\\USB1601MCP.exe",
      "args": ["--log-level", "debug"],
      "env": {
        "USB1601_CONFIG": "custom.json"
      }
    }
  }
}
```

### 多设备配置
```json
{
  "mcpServers": {
    "usb1601_device1": {
      "command": "路径\\USB1601MCP.exe",
      "args": ["--device", "0"]
    },
    "usb1601_device2": {
      "command": "路径\\USB1601MCP.exe",
      "args": ["--device", "1"]
    }
  }
}
```

## 使用示例

### 基本硬件控制
```
# 发现设备
发现所有USB-1601设备

# 连接设备
连接到USB-1601设备0

# 读取模拟输入
读取通道0的模拟输入值

# 写入模拟输出
将5V写入模拟输出通道0

# 断开设备
断开USB-1601设备
```

### 数据采集
```
# 开始数据流
开始从通道0,1,2,3采集数据，采样率10000Hz

# 停止数据流
停止数据采集
```

## 故障排除

### 问题：Claude显示"工具不可用"
**解决方案**：
1. 检查路径是否正确
2. 确保USB1601MCP.exe存在
3. 检查JYUSB1601.dll是否在同目录

### 问题：无法连接到设备
**解决方案**：
1. 确认USB-1601已连接
2. 安装JYUSB1601驱动
3. 检查设备管理器

### 问题：MCP服务未响应
**解决方案**：
1. 查看日志：`logs/usb1601-mcp-{Date}.log`
2. 以调试模式运行：`USB1601MCP.exe --debug`
3. 检查防火墙设置

## 日志位置
- 开发模式：控制台输出
- 生产模式：`logs/usb1601-mcp-YYYYMMDD.log`

## 更新说明
更新MCP服务时：
1. 重新构建项目
2. 替换publish目录中的文件
3. 重启Claude Desktop

## 注意事项
- 确保.NET 8.0 Runtime已安装
- JYUSB1601.dll必须与exe在同一目录
- 首次使用需要管理员权限安装驱动