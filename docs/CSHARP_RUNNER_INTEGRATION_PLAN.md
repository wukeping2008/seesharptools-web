# 🚀 C# Runner MCP 服务集成方案

## 📋 项目概述

将 C# Runner MCP 服务集成到 SeeSharpTools-Web 项目中，实现通过 Web 界面调用简仪的 C# 程序来控制仪器并在 Web 控件上实时显示采集的数据。

## 🎯 核心目标

- **在线执行 C# 代码**：在 Web 界面中编写和执行简仪仪器控制代码
- **实时数据采集**：通过 C# Runner 调用简仪 DLL 获取硬件数据
- **Web 控件显示**：将采集的数据实时显示在 Vue 仪表控件中
- **安全隔离执行**：在 Docker 容器中安全运行用户代码

## 🏗️ 架构设计

### 系统架构图
```
┌─────────────────────────────────────────────────────────────────┐
│                    SeeSharpTools Web 平台                       │
├─────────────────┬─────────────────┬─────────────────┬─────────────┤
│   前端 Web UI   │ SeeSharp Backend │ C# Runner MCP   │   硬件层    │
│  (Vue3 + TS)   │  (.NET Core)    │  (Docker)       │  简仪设备   │
└─────────────────┴─────────────────┴─────────────────┴─────────────┘
         │                │                │               │
         │                │                │               │
    ┌────▼────┐     ┌─────▼─────┐    ┌─────▼─────┐   ┌─────▼─────┐
    │实时仪表盘│     │MCP客户端  │    │C#代码执行  │   │JY5500/    │
    │图表控件 │     │HTTP API  │    │Docker容器  │   │USB1601   │
    │数据显示 │     │SignalR   │    │安全沙箱   │   │硬件驱动   │
    └─────────┘     └───────────┘    └───────────┘   └───────────┘
```

### 数据流程图
```
Web界面 → 编写C#代码 → SeeSharp Backend → C# Runner MCP
   ↑                                              ↓
实时数据显示 ← SignalR实时推送 ← 数据处理服务 ← Docker容器执行
   ↑                                              ↓
仪表控件更新 ← 格式化数据 ← 数据采集结果 ← 调用简仪DLL
```

## 🔧 集成方案

### 第一阶段：环境准备和基础集成

#### 1.1 创建 MCP 客户端服务
在 SeeSharp Backend 中添加 MCP 客户端：

```csharp
// backend/SeeSharpBackend/Services/CSharpRunner/ICSharpRunnerService.cs
public interface ICSharpRunnerService
{
    Task<CSharpExecutionResult> ExecuteCodeAsync(string code, int timeout = 30000);
    Task<CSharpExecutionResult> ExecuteInstrumentCodeAsync(string code, string deviceType, Dictionary<string, object> parameters);
    Task<bool> IsServiceAvailableAsync();
}

// backend/SeeSharpBackend/Services/CSharpRunner/CSharpRunnerService.cs
public class CSharpRunnerService : ICSharpRunnerService
{
    private readonly HttpClient _httpClient;
    private readonly string _csharpRunnerUrl;
    
    public async Task<CSharpExecutionResult> ExecuteCodeAsync(string code, int timeout = 30000)
    {
        // HTTP API 调用 C# Runner
        var request = new { code, timeout };
        var response = await _httpClient.PostAsync("/api/run", JsonContent.Create(request));
        
        // 处理 SSE 流式响应
        return await ProcessStreamResponse(response);
    }
}
```

#### 1.2 修改 Worker 容器以支持简仪驱动
创建自定义 Docker 镜像，包含简仪 DLL：

```dockerfile
# csharp-runner/Docker/Dockerfile.worker-jytek
FROM sdcb/csharp-runner-worker:latest

# 添加简仪驱动文件
COPY JYDrivers/ /app/JYDrivers/
COPY JYDrivers/*.dll /app/

# 设置权限
RUN chmod +x /app/JYDrivers/*

# 预装简仪相关程序集
ENV JYTEK_DRIVER_PATH=/app/JYDrivers
```

#### 1.3 扩展 docker-compose.yml
```yaml
version: '3.8'
services:
  seesharp-backend:
    build: ./backend/SeeSharpBackend
    ports:
      - "5001:5001"
    depends_on:
      - csharp-runner-host
      
  csharp-runner-host:
    image: sdcb/csharp-runner-host:latest
    ports:
      - "5050:8080"
    restart: unless-stopped

  csharp-runner-worker:
    build: 
      context: ./csharp-runner
      dockerfile: Docker/Dockerfile.worker-jytek
    environment:
      - MaxRuns=5
      - Register=true
      - RegisterHostUrl=http://csharp-runner-host:8080
      - WarmUp=true
      - JYTEK_DRIVER_PATH=/app/JYDrivers
    restart: unless-stopped
    depends_on:
      - csharp-runner-host
    deploy:
      replicas: 2
```

### 第二阶段：Web 界面开发

#### 2.1 创建在线代码编辑器组件
```vue
<!-- frontend/src/components/instruments/CSharpCodeEditor.vue -->
<template>
  <div class="csharp-code-editor">
    <div class="editor-toolbar">
      <el-button @click="executeCode" :loading="executing" type="primary">
        <i class="el-icon-play"></i> 执行代码 (Ctrl+Enter)
      </el-button>
      <el-button @click="loadTemplate">加载模板</el-button>
      <el-select v-model="selectedDevice" placeholder="选择设备">
        <el-option label="JY5500" value="JY5500" />
        <el-option label="JYUSB1601" value="JYUSB1601" />
      </el-select>
    </div>
    
    <!-- Monaco Editor 或 CodeMirror -->
    <div ref="editorContainer" class="code-editor-container"></div>
    
    <!-- 执行结果显示区域 -->
    <div class="execution-results">
      <el-tabs v-model="activeTab">
        <el-tab-pane label="输出" name="output">
          <pre class="console-output">{{ consoleOutput }}</pre>
        </el-tab-pane>
        <el-tab-pane label="数据" name="data">
          <div class="data-visualization">
            <!-- 实时数据图表 -->
            <ECharts ref="dataChart" :options="chartOptions" />
          </div>
        </el-tab-pane>
        <el-tab-pane label="错误" name="error" v-if="hasError">
          <pre class="error-output">{{ errorOutput }}</pre>
        </el-tab-pane>
      </el-tabs>
    </div>
  </div>
</template>
```

#### 2.2 创建 C# Runner 服务调用
```typescript
// frontend/src/services/CSharpRunnerService.ts
export class CSharpRunnerService {
  private baseUrl = '/api/csharp-runner';
  
  async executeCode(code: string, deviceType?: string): Promise<ExecutionResult> {
    const response = await fetch(`${this.baseUrl}/execute`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ code, deviceType })
    });
    
    // 处理 SSE 流式响应
    return this.handleStreamResponse(response);
  }
  
  private async handleStreamResponse(response: Response): Promise<ExecutionResult> {
    const reader = response.body?.getReader();
    const decoder = new TextDecoder();
    let result: ExecutionResult = { output: '', data: null, success: true };
    
    while (true) {
      const { done, value } = await reader!.read();
      if (done) break;
      
      const chunk = decoder.decode(value);
      const lines = chunk.split('\n');
      
      for (const line of lines) {
        if (line.startsWith('data: ')) {
          const data = JSON.parse(line.slice(6));
          this.processStreamData(data, result);
        }
      }
    }
    
    return result;
  }
}
```

### 第三阶段：预置代码模板

#### 3.1 简仪设备控制模板
```csharp
// JY5500 数据采集模板
var jy5500 = new JY5500AITask("JY5500");
jy5500.Channels.AddRange(JY5500AIChannel.CreateByPhysicalChannel("0:7"));
jy5500.SampleRate = 1000;
jy5500.Mode = JY5500AIMode.Continuous;

// 开始采集
jy5500.Start();

// 采集 1000 个样本
var data = jy5500.ReadData(1000);

// 输出到Web界面
Console.WriteLine($"采集到 {data.GetLength(0)} 个通道，{data.GetLength(1)} 个样本");

// 返回数据用于图表显示
return new {
    channels = data.GetLength(0),
    samples = data.GetLength(1),
    data = data,
    sampleRate = 1000,
    timestamp = DateTime.Now
};
```

#### 3.2 JYUSB1601 控制模板
```csharp
// JYUSB1601 数据采集模板
var usb1601 = new JYUSB1601AITask(0);
usb1601.Channels.AddRange(JYUSB1601AIChannel.CreateByPhysicalChannel("0:3"));
usb1601.SampleRate = 2000;
usb1601.SamplesToAcquire = 2000;

// 配置触发
usb1601.TriggerParameters.TriggerSource = AITriggerSource.Immediate;

// 开始采集
usb1601.Start();

// 等待采集完成
usb1601.WaitUntilDone();

// 读取数据
var data = usb1601.ReadData(usb1601.SamplesToAcquire);

// 停止任务
usb1601.Stop();

// 返回结构化数据
return new {
    deviceType = "JYUSB1601",
    channelCount = data.GetLength(0),
    sampleCount = data.GetLength(1),
    sampleRate = 2000,
    rawData = data,
    statistics = new {
        min = data.Cast<double>().Min(),
        max = data.Cast<double>().Max(),
        average = data.Cast<double>().Average()
    }
};
```

### 第四阶段：实时数据显示

#### 4.1 数据处理和转换
```csharp
// backend/SeeSharpBackend/Services/DataProcessor/InstrumentDataProcessor.cs
public class InstrumentDataProcessor
{
    public async Task<ProcessedInstrumentData> ProcessExecutionResult(CSharpExecutionResult result)
    {
        if (result.ReturnValue == null) return null;
        
        // 解析返回的仪器数据
        var instrumentData = JsonSerializer.Deserialize<InstrumentDataResult>(result.ReturnValue.ToString());
        
        // 转换为Web控件所需格式
        return new ProcessedInstrumentData
        {
            DeviceType = instrumentData.DeviceType,
            Timestamp = DateTime.Now,
            ChannelData = ConvertToChannelData(instrumentData.RawData),
            Statistics = instrumentData.Statistics,
            ChartData = GenerateChartData(instrumentData.RawData)
        };
    }
}
```

#### 4.2 实时数据推送
```csharp
// backend/SeeSharpBackend/Hubs/InstrumentDataHub.cs
public class InstrumentDataHub : Hub
{
    public async Task ExecuteInstrumentCode(string code, string deviceType)
    {
        try
        {
            // 执行代码
            var result = await _csharpRunner.ExecuteCodeAsync(code);
            
            // 处理数据
            var processedData = await _dataProcessor.ProcessExecutionResult(result);
            
            // 实时推送到前端
            await Clients.All.SendAsync("InstrumentDataReceived", processedData);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ExecutionError", ex.Message);
        }
    }
}
```

## 🎛️ 预期效果展示

### 用户操作流程
1. **打开代码编辑器页面**
   - 选择设备类型（JY5500/JYUSB1601）
   - 加载预置模板或编写自定义代码

2. **执行代码**
   - 点击执行按钮或按 Ctrl+Enter
   - 代码在 Docker 容器中安全执行
   - 实时显示控制台输出

3. **查看采集数据**
   - 自动解析返回的仪器数据
   - 在图表控件中实时显示波形
   - 显示统计信息（最值、均值等）

4. **数据交互**
   - 缩放、平移图表查看细节
   - 导出数据为 CSV/JSON 格式
   - 保存代码模板供后续使用

### Web 界面效果
```
┌─────────────────────────────────────────────────────────────────┐
│  🔧 简仪设备在线控制平台                                          │
├─────────────────────────┬───────────────────────────────────────┤
│  📝 代码编辑器           │  📊 实时数据显示                       │
│  ┌─────────────────────┐ │  ┌─────────────────────────────────┐ │
│  │ // JY5500 数据采集   │ │  │     📈 实时波形图表              │ │
│  │ var device = new    │ │  │  ┌─────────────────────────────┐ │ │
│  │ JY5500AITask();     │ │  │  │       /\    /\    /\       │ │ │
│  │ device.Start();     │ │  │  │      /  \  /  \  /  \      │ │ │
│  │ var data = device.  │ │  │  │     /    \/    \/    \     │ │ │
│  │ ReadData(1000);     │ │  │  └─────────────────────────────┘ │ │
│  │ return data;        │ │  │  通道: 8  样本: 1000  频率: 1kHz │ │
│  └─────────────────────┘ │  └─────────────────────────────────┘ │
│  [▶️ 执行] [📄 模板]      │                                    │
├─────────────────────────┼───────────────────────────────────────┤
│  💻 控制台输出           │  📋 数据统计                           │
│  ✅ 设备连接成功         │  最大值: 4.85V                        │
│  📊 采集8通道数据...     │  最小值: -4.92V                       │
│  ✅ 采集完成: 1000样本   │  平均值: 0.02V                        │
│                         │  标准差: 2.31V                        │
└─────────────────────────┴───────────────────────────────────────┘
```

## 🚀 实施计划

### Phase 1: 基础架构 (1-2周)
- [x] 复制 C# Runner 到项目
- [ ] 配置 Docker 环境
- [ ] 创建 MCP 客户端服务
- [ ] 基础 API 接口开发

### Phase 2: 前端开发 (2-3周)
- [ ] 代码编辑器组件
- [ ] 执行结果显示
- [ ] 实时数据图表
- [ ] 设备模板管理

### Phase 3: 驱动集成 (2-3周)
- [ ] 简仪 DLL 集成到容器
- [ ] 设备发现和连接
- [ ] 数据采集优化
- [ ] 错误处理完善

### Phase 4: 功能完善 (1-2周)
- [ ] 用户权限管理
- [ ] 代码版本控制
- [ ] 性能监控
- [ ] 文档和测试

## 🔒 安全考虑

1. **代码执行安全**
   - Docker 容器隔离
   - 资源限制（CPU/内存/超时）
   - 网络访问控制

2. **硬件访问控制**
   - 设备权限验证
   - 操作日志记录
   - 危险操作拦截

3. **数据安全**
   - 敏感数据脱敏
   - 传输加密
   - 访问审计

## 📊 性能优化

1. **执行性能**
   - Worker 预热机制
   - 连接池复用
   - 负载均衡

2. **数据传输**
   - 数据压缩
   - 增量更新
   - 缓存策略

3. **用户体验**
   - 异步执行
   - 进度提示
   - 错误恢复

通过这个集成方案，您将能够在 Web 界面中直接编写和执行 C# 代码来控制简仪设备，并将采集的数据实时显示在专业的仪表控件中，实现真正的 Web 化测试测量平台。
