# 🏗️ SeeSharpTools Web 系统架构设计

## 概述
SeeSharpTools Web 是一个云原生的专业测控平台，采用前后端分离的微服务架构，支持高速数据采集、实时数据可视化和AI驱动的控件生成。

## 🎯 架构设计原则

### 1. 高性能原则
- **前端**：支持1GS/s数据流实时显示
- **后端**：支持16-32通道并发数据采集
- **网络**：低延迟实时数据传输（<10ms）

### 2. 高可用原则
- **系统可用性**：>99.9%
- **故障恢复**：<30秒自动恢复
- **数据完整性**：100%数据不丢失

### 3. 可扩展原则
- **水平扩展**：支持微服务弹性扩缩容
- **垂直扩展**：支持硬件资源动态调整
- **功能扩展**：插件化架构支持新控件

### 4. 安全性原则
- **数据安全**：端到端加密传输
- **访问控制**：基于角色的权限管理
- **代码安全**：AI生成代码安全扫描

## 🏛️ 整体架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                        用户层 (User Layer)                        │
├─────────────────────────────────────────────────────────────────┤
│  Web浏览器  │  移动端APP  │  桌面应用  │  第三方集成  │  API客户端  │
└─────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                      前端层 (Frontend Layer)                      │
├─────────────────────────────────────────────────────────────────┤
│                    Vue 3 + TypeScript SPA                      │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │
│  │ 控件库      │ │ 图表组件    │ │ AI控件生成  │ │ 实时通信    │ │
│  │ Components  │ │ Charts      │ │ AI Generator│ │ SignalR     │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                      网关层 (Gateway Layer)                       │
├─────────────────────────────────────────────────────────────────┤
│                      API Gateway (Nginx)                       │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │
│  │ 路由管理    │ │ 负载均衡    │ │ 限流控制    │ │ 认证授权    │ │
│  │ Routing     │ │ Load Balance│ │ Rate Limit  │ │ Auth        │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                     服务层 (Service Layer)                        │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │
│  │ 用户服务    │ │ 设备服务    │ │ 数据服务    │ │ AI服务      │ │
│  │ User Service│ │Device Service│ │Data Service │ │ AI Service  │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │
│  │ 配置服务    │ │ 通知服务    │ │ 文件服务    │ │ 监控服务    │ │
│  │Config Service│ │Notify Service│ │File Service │ │Monitor Svc  │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                     数据层 (Data Layer)                           │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │
│  │ 时序数据库  │ │ 关系数据库  │ │ 缓存数据库  │ │ 文件存储    │ │
│  │ InfluxDB    │ │ PostgreSQL  │ │ Redis       │ │ MinIO       │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────┐
│                    硬件层 (Hardware Layer)                        │
├─────────────────────────────────────────────────────────────────┤
│                      简仪科技 PXI 模块                            │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │
│  │ 数采卡      │ │ 信号发生器  │ │ 示波器卡    │ │ 万用表      │ │
│  │ DAQ Card    │ │ Signal Gen  │ │ Scope Card  │ │ DMM         │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │
│  │ 温度卡      │ │ DIO卡       │ │ 开关卡      │ │ 其他模块    │ │
│  │ Temp Card   │ │ DIO Card    │ │ Switch Card │ │ Others      │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

## 🎨 前端架构设计

### 技术栈
```typescript
// 核心框架
Vue 3.4+              // 响应式框架
TypeScript 5.3+       // 类型安全
Vite 5.0+             // 构建工具
Pinia 2.1+            // 状态管理

// UI和图表
Element Plus 2.4+     // UI组件库
ECharts 5.4+          // 图表库
Canvas API            // 高性能渲染
WebGL                 // 3D加速渲染

// 通信和工具
Axios 1.6+            // HTTP客户端
Socket.IO 4.7+        // 实时通信
Lodash 4.17+          // 工具库
Day.js 1.11+          // 日期处理
```

### 目录结构
```
src/
├── components/              # 组件库
│   ├── charts/             # 图表组件
│   │   ├── StripChart.vue  # 条带图
│   │   ├── EasyChart.vue   # 科学图表
│   │   └── SpectrumChart.vue # 频谱图
│   ├── instruments/        # 仪器控件
│   │   ├── DataAcquisition.vue # 数采卡
│   │   ├── SignalGenerator.vue # 信号发生器
│   │   └── Oscilloscope.vue    # 示波器
│   ├── indicators/         # 指示器
│   │   ├── LEDIndicator.vue
│   │   └── DigitalDisplay.vue
│   └── ai/                 # AI控件
│       ├── ControlGenerator.vue
│       └── CodeEditor.vue
├── views/                  # 页面视图
├── stores/                 # 状态管理
├── services/              # 服务层
│   ├── api.ts             # API接口
│   ├── websocket.ts       # WebSocket
│   └── hardware.ts        # 硬件接口
├── types/                 # 类型定义
├── utils/                 # 工具函数
└── styles/                # 样式文件
```

### 状态管理架构
```typescript
// stores/index.ts
export const useAppStore = defineStore('app', {
  state: () => ({
    // 全局状态
    user: null,
    theme: 'light',
    language: 'zh-CN'
  })
})

export const useDeviceStore = defineStore('device', {
  state: () => ({
    // 设备状态
    connectedDevices: [],
    deviceStatus: {},
    acquisitionStatus: 'stopped'
  })
})

export const useDataStore = defineStore('data', {
  state: () => ({
    // 数据状态
    realtimeData: new Map(),
    historicalData: new Map(),
    dataBuffer: new CircularBuffer(1000000)
  })
})
```

### 高性能数据处理
```typescript
// utils/dataProcessor.ts
export class DataProcessor {
  private buffer: CircularBuffer
  private worker: Worker
  
  constructor(bufferSize: number) {
    this.buffer = new CircularBuffer(bufferSize)
    this.worker = new Worker('/workers/dataProcessor.js')
  }
  
  // LTTB采样算法
  downsample(data: number[], targetPoints: number): number[] {
    if (data.length <= targetPoints) return data
    
    const sampled = []
    const every = (data.length - 2) / (targetPoints - 2)
    
    sampled[0] = data[0] // 保留第一个点
    
    for (let i = 0; i < targetPoints - 2; i++) {
      const avgRangeStart = Math.floor((i + 1) * every) + 1
      const avgRangeEnd = Math.floor((i + 2) * every) + 1
      
      // 计算平均点
      const avgX = (avgRangeStart + avgRangeEnd) / 2
      let avgY = 0
      for (let j = avgRangeStart; j < avgRangeEnd; j++) {
        avgY += data[j]
      }
      avgY /= (avgRangeEnd - avgRangeStart)
      
      // 选择最大三角形面积的点
      let maxArea = -1
      let maxAreaIndex = avgRangeStart
      
      for (let j = avgRangeStart; j < avgRangeEnd; j++) {
        const area = Math.abs(
          (sampled[sampled.length - 1] - avgX) * (data[j] - avgY) -
          (sampled[sampled.length - 1] - data[j]) * (avgX - avgY)
        )
        
        if (area > maxArea) {
          maxArea = area
          maxAreaIndex = j
        }
      }
      
      sampled.push(data[maxAreaIndex])
    }
    
    sampled.push(data[data.length - 1]) // 保留最后一个点
    return sampled
  }
}
```

## 🔧 后端架构设计

### 技术栈
```csharp
// 核心框架
.NET 8.0                    // 运行时
ASP.NET Core 8.0           // Web框架
Entity Framework Core 8.0  // ORM
SignalR 8.0                // 实时通信

// 数据库
InfluxDB 2.7+              // 时序数据库
PostgreSQL 16+             // 关系数据库
Redis 7.2+                 // 缓存数据库

// 消息队列
RabbitMQ 3.12+             // 消息中间件
MassTransit 8.1+           // 消息总线

// 监控和日志
Serilog 3.1+               // 结构化日志
Prometheus.NET 8.2+        // 指标收集
OpenTelemetry 1.7+         // 链路追踪
```

### 微服务架构
```
Backend Services/
├── Gateway/                    # API网关
│   ├── Program.cs
│   ├── appsettings.json
│   └── Middleware/
├── Services/
│   ├── UserService/           # 用户服务
│   │   ├── Controllers/
│   │   ├── Models/
│   │   └── Services/
│   ├── DeviceService/         # 设备服务
│   │   ├── Hardware/          # 硬件抽象层
│   │   ├── MISD/             # MISD接口
│   │   └── Drivers/          # 驱动管理
│   ├── DataService/          # 数据服务
│   │   ├── Acquisition/      # 数据采集
│   │   ├── Processing/       # 数据处理
│   │   └── Storage/          # 数据存储
│   └── AIService/            # AI服务
│       ├── Claude/           # Claude集成
│       ├── Generator/        # 代码生成
│       └── Templates/        # 模板管理
├── Shared/                   # 共享库
│   ├── Common/
│   ├── Models/
│   └── Interfaces/
└── Infrastructure/           # 基础设施
    ├── Database/
    ├── Messaging/
    └── Monitoring/
```

### MISD硬件抽象层
```csharp
// Hardware/MISD/IMISDDevice.cs
public interface IMISDDevice
{
    string DeviceId { get; }
    string DeviceType { get; }
    MISDCapabilities Capabilities { get; }
    DeviceStatus Status { get; }
    
    Task<bool> InitializeAsync();
    Task<bool> ConfigureAsync(DeviceConfiguration config);
    Task<bool> StartAsync();
    Task<bool> StopAsync();
    Task<bool> ResetAsync();
    Task<DeviceInfo> GetDeviceInfoAsync();
}

// Hardware/MISD/MISDDeviceManager.cs
public class MISDDeviceManager
{
    private readonly Dictionary<string, IMISDDevice> _devices;
    private readonly MISDDictionary _dictionary;
    
    public async Task<List<IMISDDevice>> DiscoverDevicesAsync()
    {
        var devices = new List<IMISDDevice>();
        
        // PXI设备发现
        var pxiDevices = await PXIDeviceDiscovery.ScanAsync();
        foreach (var device in pxiDevices)
        {
            var misd = _dictionary.GetDeviceDefinition(device.DeviceId);
            if (misd != null)
            {
                var wrapper = new MISDDeviceWrapper(device, misd);
                devices.Add(wrapper);
            }
        }
        
        return devices;
    }
    
    public async Task<T> CreateDeviceAsync<T>(string deviceId) where T : IMISDDevice
    {
        var definition = _dictionary.GetDeviceDefinition(deviceId);
        var driver = await LoadDriverAsync(definition.DriverPath);
        return (T)Activator.CreateInstance(typeof(T), driver, definition);
    }
}
```

### 高速数据采集引擎
```csharp
// DataService/Acquisition/DataAcquisitionEngine.cs
public class DataAcquisitionEngine
{
    private readonly ConcurrentQueue<DataPacket> _dataQueue;
    private readonly CircularBuffer<double> _buffer;
    private readonly Timer _acquisitionTimer;
    private readonly SemaphoreSlim _semaphore;
    
    public async Task StartAcquisitionAsync(AcquisitionConfig config)
    {
        // 配置采集参数
        await ConfigureHardwareAsync(config);
        
        // 启动高优先级采集线程
        var thread = new Thread(AcquisitionLoop)
        {
            Priority = ThreadPriority.Highest,
            IsBackground = false
        };
        thread.Start();
        
        // 启动数据处理线程
        _ = Task.Run(ProcessDataAsync);
    }
    
    private void AcquisitionLoop()
    {
        var stopwatch = Stopwatch.StartNew();
        var targetInterval = 1000.0 / _config.SampleRate; // ms
        
        while (_isRunning)
        {
            try
            {
                // 从硬件读取数据
                var data = _device.ReadData(_config.ChannelCount);
                
                // 添加时间戳
                var packet = new DataPacket
                {
                    Timestamp = DateTime.UtcNow,
                    ChannelData = data,
                    SequenceNumber = _sequenceNumber++
                };
                
                // 入队处理
                _dataQueue.Enqueue(packet);
                
                // 精确定时
                var elapsed = stopwatch.ElapsedMilliseconds;
                var sleepTime = (int)(targetInterval - elapsed);
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
                stopwatch.Restart();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Data acquisition error");
            }
        }
    }
    
    private async Task ProcessDataAsync()
    {
        while (_isRunning)
        {
            if (_dataQueue.TryDequeue(out var packet))
            {
                // 数据验证
                if (ValidateDataPacket(packet))
                {
                    // 存储到缓冲区
                    _buffer.AddRange(packet.ChannelData);
                    
                    // 实时传输
                    await _hubContext.Clients.All.SendAsync("DataUpdate", packet);
                    
                    // 持久化存储
                    await _dataStorage.StoreAsync(packet);
                }
            }
            else
            {
                await Task.Delay(1); // 避免CPU占用过高
            }
        }
    }
}
```

### SignalR实时通信
```csharp
// Hubs/DataHub.cs
public class DataHub : Hub
{
    private readonly IDataService _dataService;
    private readonly ILogger<DataHub> _logger;
    
    public async Task JoinDataGroup(string deviceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"device_{deviceId}");
        _logger.LogInformation($"Client {Context.ConnectionId} joined device group {deviceId}");
    }
    
    public async Task LeaveDataGroup(string deviceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"device_{deviceId}");
    }
    
    public async Task StartDataStream(string deviceId, StreamConfig config)
    {
        try
        {
            await _dataService.StartStreamAsync(deviceId, config);
            await Clients.Caller.SendAsync("StreamStarted", deviceId);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("StreamError", ex.Message);
        }
    }
    
    public async Task StopDataStream(string deviceId)
    {
        await _dataService.StopStreamAsync(deviceId);
        await Clients.Caller.SendAsync("StreamStopped", deviceId);
    }
}

// Services/DataStreamService.cs
public class DataStreamService : BackgroundService
{
    private readonly IHubContext<DataHub> _hubContext;
    private readonly IDataAcquisitionEngine _acquisitionEngine;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var dataPacket in _acquisitionEngine.GetDataStreamAsync(stoppingToken))
        {
            // 数据压缩
            var compressedData = CompressData(dataPacket);
            
            // 广播到对应设备组
            await _hubContext.Clients.Group($"device_{dataPacket.DeviceId}")
                .SendAsync("RealtimeData", compressedData, stoppingToken);
        }
    }
    
    private byte[] CompressData(DataPacket packet)
    {
        // 使用LZ4压缩算法
        var json = JsonSerializer.Serialize(packet);
        var bytes = Encoding.UTF8.GetBytes(json);
        return LZ4Codec.Encode(bytes);
    }
}
```

## 🤖 AI控件生成架构

### Claude API集成
```csharp
// AIService/Claude/ClaudeClient.cs
public class ClaudeClient
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeConfig _config;
    
    public async Task<ControlGenerationResult> GenerateControlAsync(ControlRequest request)
    {
        var prompt = BuildPrompt(request);
        
        var claudeRequest = new
        {
            model = "claude-3-sonnet-20240229",
            max_tokens = 4000,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };
        
        var response = await _httpClient.PostAsJsonAsync("/v1/messages", claudeRequest);
        var result = await response.Content.ReadFromJsonAsync<ClaudeResponse>();
        
        return ParseControlCode(result.Content[0].Text);
    }
    
    private string BuildPrompt(ControlRequest request)
    {
        return $@"
作为一个专业的Vue.js控件开发专家，请根据以下需求生成一个测控仪器控件：

需求描述：{request.Description}
控件类型：{request.ControlType}
技术要求：
- 使用Vue 3 Composition API
- 使用TypeScript
- 使用Element Plus UI库
- 遵循SeeSharpTools设计风格
- 支持响应式设计

请生成完整的Vue组件代码，包括：
1. 模板部分（template）
2. 脚本部分（script setup）
3. 样式部分（style scoped）
4. TypeScript类型定义

约束条件：
- 性能要求：支持实时数据更新
- 兼容性：支持主流浏览器
- 可访问性：支持键盘导航
- 安全性：输入验证和XSS防护

请确保代码质量高、可维护性强、符合最佳实践。
";
    }
}
```

### 动态控件生成器
```typescript
// ai/ControlGenerator.vue
<template>
  <div class="control-generator">
    <div class="input-section">
      <el-input
        v-model="userInput"
        type="textarea"
        :rows="4"
        placeholder="请描述您需要的控件，例如：我需要一个显示温度的圆形仪表，支持报警功能"
        @keyup.ctrl.enter="generateControl"
      />
      <el-button type="primary" @click="generateControl" :loading="generating">
        生成控件
      </el-button>
    </div>
    
    <div class="preview-section" v-if="generatedControl">
      <div class="code-editor">
        <monaco-editor
          v-model="generatedControl.code"
          language="vue"
          :options="editorOptions"
        />
      </div>
      <div class="live-preview">
        <component :is="compiledComponent" v-bind="previewProps" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { compile } from 'vue/compiler-sfc'
import MonacoEditor from '@/components/MonacoEditor.vue'
import { aiService } from '@/services/ai'

const userInput = ref('')
const generating = ref(false)
const generatedControl = ref<GeneratedControl | null>(null)

const compiledComponent = computed(() => {
  if (!generatedControl.value) return null
  
  try {
    const { descriptor } = compile(generatedControl.value.code)
    return defineComponent({
      template: descriptor.template?.content,
      setup: new Function('props', 'context', descriptor.script?.content)
    })
  } catch (error) {
    console.error('Component compilation error:', error)
    return null
  }
})

const generateControl = async () => {
  if (!userInput.value.trim()) return
  
  generating.value = true
  try {
    const result = await aiService.generateControl({
      description: userInput.value,
      controlType: 'auto-detect',
      constraints: {
        performance: 'high',
        compatibility: 'modern',
        accessibility: true
      }
    })
    
    generatedControl.value = result
  } catch (error) {
    ElMessage.error('控件生成失败：' + error.message)
  } finally {
    generating.value = false
  }
}
</script>
```

## 📊 数据存储架构

### 时序数据库设计
```sql
-- InfluxDB Schema
-- 测量数据表
CREATE MEASUREMENT device_data (
  time TIMESTAMP,
  device_id TAG,
  channel_id TAG,
  data_type TAG,
  value FIELD,
  unit TAG,
  quality FIELD
)

-- 设备状态表
CREATE MEASUREMENT device_status (
  time TIMESTAMP,
  device_id TAG,
  status TAG,
  temperature FIELD,
  voltage FIELD,
  current FIELD
)

-- 系统事件表
CREATE MEASUREMENT system_events (
  time TIMESTAMP,
  event_type TAG,
  severity TAG,
  source TAG,
  message FIELD
)
```

### 关系数据库设计
```sql
-- PostgreSQL Schema
-- 用户表
CREATE TABLE users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  username VARCHAR(50) UNIQUE NOT NULL,
  email VARCHAR(100) UNIQUE NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  role VARCHAR(20) NOT NULL DEFAULT 'user',
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW()
);

-- 设备配置表
CREATE TABLE device_configs (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  device_id VARCHAR(100) NOT NULL,
  config_name VARCHAR(100) NOT NULL,
  config_data JSONB NOT NULL,
  created_by UUID REFERENCES users(id),
  created_at TIMESTAMP DEFAULT NOW()
);

-- 项目表
CREATE TABLE projects (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name VARCHAR(100) NOT NULL,
  description TEXT,
  owner_id UUID REFERENCES users(id),
  config JSONB,
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW()
);

-- 控件模板表
CREATE TABLE control_templates (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name VARCHAR(100) NOT NULL,
  category VARCHAR(50) NOT NULL,
  template_code TEXT NOT NULL,
  parameters JSONB,
  created_by UUID REFERENCES users(id),
  created_at TIMESTAMP DEFAULT NOW()
);
```

## 🚀 部署架构

### Kubernetes部署配置
```yaml
# k8s/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: seesharptools

---
# k8s/frontend-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: frontend
  namespace: seesharptools
spec:
  replicas: 3
  selector:
    matchLabels:
      app: frontend
  template:
    metadata:
      labels:
        app: frontend
    spec:
      containers:
      - name: frontend
        image: seesharptools/frontend:latest
        ports:
        - containerPort: 80
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"

---
# k8s/backend-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: backend-api
  namespace: seesharptools
spec:
  replicas: 5
  selector:
    matchLabels:
      app: backend-api
  template:
    metadata:
      labels:
        app: backend-api
    spec:
      containers:
      - name: api
        image: seesharptools/backend-api:latest
        ports:
        - containerPort: 80
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "1Gi"
            cpu: "1000m"

---
# k8s/hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
