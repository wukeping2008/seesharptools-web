# 🚀 SeeSharpTools 通用驱动架构设计

## 📋 项目概述

基于简仪科技JY5500多功能数据采集卡的深入分析，我们设计了一个通用的驱动管理架构，支持简仪科技的各种.dll硬件驱动，并为未来扩展Python和C++版本做好准备。

## 🏗️ 架构设计

### 核心组件

```
┌─────────────────────────────────────────────────────────────┐
│                    MISD Service Layer                       │
│                 (标准化硬件抽象层)                            │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                 Driver Manager                              │
│              (统一驱动管理器)                                │
└─────────────────────┬───────────────────────────────────────┘
                      │
        ┌─────────────┼─────────────┬─────────────┐
        │             │             │             │
┌───────▼──────┐ ┌───▼────┐ ┌──────▼──┐ ┌────────▼────┐
│ C# DLL       │ │ Python │ │ C++ DLL │ │   Network   │
│ Adapter      │ │Adapter │ │ Adapter │ │   Adapter   │
│   ✅         │ │  🔄    │ │   🔄    │ │     🔄      │
└──────────────┘ └────────┘ └─────────┘ └─────────────┘
```

### 1. 通用驱动适配器接口 (IDriverAdapter)

```csharp
public interface IDriverAdapter
{
    DriverType Type { get; }
    string Name { get; }
    string Version { get; }
    bool IsInitialized { get; }
    
    Task<bool> InitializeAsync(DriverConfiguration config);
    Task<bool> UnloadAsync();
    Task<List<HardwareDevice>> DiscoverDevicesAsync();
    Task<object> CreateTaskAsync(string taskType, int deviceId, Dictionary<string, object> parameters);
    Task<object?> ExecuteMethodAsync(object taskInstance, string methodName, object[] parameters);
    Task<object?> GetPropertyAsync(object taskInstance, string propertyName);
    Task<bool> SetPropertyAsync(object taskInstance, string propertyName, object value);
    Task<bool> DisposeTaskAsync(object taskInstance);
}
```

### 2. C# DLL驱动适配器 (CSharpDllDriverAdapter)

**特点:**
- ✅ 支持动态加载.dll程序集
- ✅ 反射调用任务类和方法
- ✅ 自动扫描任务类型
- ✅ 支持JY5500、JYUSB等系列设备
- ✅ 完整的错误处理和日志记录

**核心功能:**
```csharp
// 动态加载DLL
_driverAssembly = Assembly.LoadFrom(config.DriverPath);

// 创建任务实例
var instance = Activator.CreateInstance(type, deviceId);

// 反射调用方法
var result = method.Invoke(taskInstance, parameters);
```

### 3. 驱动管理器 (DriverManager)

**功能:**
- 📁 配置文件管理 (`Config/drivers.json`)
- 🔄 自动加载启用的驱动
- 🔍 统一设备发现
- 🏭 驱动适配器工厂
- 📊 驱动状态监控

**配置示例:**
```json
{
  "JY5500": {
    "DriverPath": "C:\\SeeSharp\\JYTEK\\Hardware\\DAQ\\JY5500\\Bin\\JY5500.dll",
    "DeviceModel": "JY5500",
    "Parameters": {
      "AutoDetect": true,
      "MaxDevices": 4
    }
  },
  "JYUSB1601": {
    "DriverPath": "C:\\SeeSharp\\JYTEK\\Hardware\\DAQ\\JYUSB1601\\Bin\\JYUSB1601.dll",
    "DeviceModel": "JYUSB1601",
    "Parameters": {
      "AutoDetect": true,
      "USBTimeout": 5000
    }
  }
}
```

## 🔧 支持的驱动类型

### 当前支持 ✅

#### C# DLL驱动
- **JY5500系列**: 多功能数据采集卡
  - JY5510: 32通道AI, 4通道AO, 2MHz采样率
  - JY5511: 32通道AI, 4通道AO, 1.25MHz采样率
  - JY5515: 16通道AI, 2通道AO, 2MHz采样率
  - JY5516: 16通道AI, 2通道AO, 1.25MHz采样率

- **JYUSB系列**: USB数据采集设备
  - JYUSB1601: USB高速数据采集卡

### 未来扩展 🔄

#### Python驱动适配器
```python
# 预期API结构
import jytek_python_driver as jy

# 创建任务
ai_task = jy.AITask(device_id=0)
ai_task.add_channel(0, -10, 10, "RSE")
ai_task.sample_rate = 1000000
ai_task.start()

# 读取数据
data = ai_task.read_data(1000)
```

#### C++ DLL驱动适配器
```cpp
// 预期API结构
#include "jytek_cpp_driver.h"

// 创建任务
auto ai_task = std::make_unique<JYAITask>(0);
ai_task->AddChannel(0, -10.0, 10.0, RSE);
ai_task->SetSampleRate(1000000);
ai_task->Start();

// 读取数据
std::vector<double> data = ai_task->ReadData(1000);
```

## 📊 JY5500 API分析结果

### 核心任务类
```csharp
// 主要任务类
JY5500AITask aiTask = new JY5500AITask(boardNumber);
JY5500AOTask aoTask = new JY5500AOTask(boardNumber);
JY5500DITask diTask = new JY5500DITask(boardNumber);
JY5500DOTask doTask = new JY5500DOTask(boardNumber);
JY5500CITask ciTask = new JY5500CITask(boardNumber, counterNumber);
JY5500COTask coTask = new JY5500COTask(boardNumber, counterNumber);
```

### 标准工作流程
1. **创建任务** → `new JY5500XXTask(boardNumber)`
2. **添加通道** → `AddChannel(channelId, rangeLow, rangeHigh, terminal)`
3. **配置参数** → `Mode`, `SampleRate`, `SampleClock`等
4. **启动任务** → `Start()`
5. **数据操作** → `ReadData()` / `WriteData()`
6. **停止任务** → `Stop()`
7. **清理资源** → `Channels.Clear()`

### 关键配置参数
- **采样模式**: `Single`, `Continuous`, `Finite`
- **终端类型**: `RSE`, `NRSE`, `Differential`
- **时钟源**: `Internal`, `External`
- **触发方式**: `Software`, `Digital`, `Analog`
- **电压范围**: ±10V, ±5V, ±2V, ±1V, ±0.5V, ±0.2V, ±0.1V

## 🚀 使用示例

### 1. 基本设备发现
```csharp
// 通过驱动管理器发现所有设备
var devices = await driverManager.DiscoverAllDevicesAsync();

foreach (var device in devices)
{
    Console.WriteLine($"发现设备: {device.Model} - {device.Name}");
}
```

### 2. 创建和配置任务
```csharp
// 获取JY5500驱动
var driver = driverManager.GetDriver("JY5500");

// 创建AI任务
var aiTask = await driver.CreateTaskAsync("JY5500AITask", 0, new Dictionary<string, object>
{
    ["DeviceId"] = 0
});

// 配置通道
await driver.ExecuteMethodAsync(aiTask, "AddChannel", new object[] { 0, -10.0, 10.0, "RSE" });

// 设置采样率
await driver.SetPropertyAsync(aiTask, "SampleRate", 1000000.0);

// 启动任务
await driver.ExecuteMethodAsync(aiTask, "Start", Array.Empty<object>());
```

### 3. 数据采集
```csharp
// 读取数据
var buffer = new double[1000, 1];
await driver.ExecuteMethodAsync(aiTask, "ReadData", new object[] { buffer, 1000, -1 });

// 停止任务
await driver.ExecuteMethodAsync(aiTask, "Stop", Array.Empty<object>());

// 释放资源
await driver.DisposeTaskAsync(aiTask);
```

## 🔄 扩展路径

### 阶段一：完善C# DLL支持 (当前)
- ✅ 基础架构完成
- ✅ JY5500系列支持
- 🔄 更多简仪设备型号支持
- 🔄 高级功能优化

### 阶段二：Python驱动支持
- 🔄 Python.NET集成
- 🔄 Python驱动适配器实现
- 🔄 跨语言数据类型转换
- 🔄 Python示例和文档

### 阶段三：C++ DLL支持
- 🔄 P/Invoke封装
- 🔄 C++ DLL适配器实现
- 🔄 内存管理优化
- 🔄 性能基准测试

### 阶段四：网络驱动支持
- 🔄 TCP/UDP通信协议
- 🔄 远程设备控制
- 🔄 分布式系统支持
- 🔄 云端设备管理

## 📈 性能优化

### 内存管理
- 对象池模式减少GC压力
- 大数据缓冲区复用
- 异步操作避免阻塞

### 并发处理
- 任务实例线程安全
- 异步方法支持
- 并行设备操作

### 错误处理
- 分层异常处理
- 详细错误日志
- 自动重试机制

## 🎯 预期成果

完成后将实现：

1. **统一的驱动接口** - 支持多种驱动类型的无缝切换
2. **配置化管理** - 通过JSON配置文件管理所有驱动
3. **热插拔支持** - 运行时加载/卸载驱动
4. **多语言支持** - C#、Python、C++驱动的统一管理
5. **可扩展架构** - 轻松添加新的驱动类型和设备型号
6. **生产就绪** - 完整的错误处理、日志记录和性能优化

这将为简仪科技的测控产品提供现代化、标准化的驱动管理解决方案，大大提升开发效率和产品竞争力！

## 📝 开发状态

- ✅ **已完成**: 通用驱动架构设计
- ✅ **已完成**: C# DLL驱动适配器
- ✅ **已完成**: 驱动管理器核心功能
- ✅ **已完成**: MISD服务集成
- 🔄 **进行中**: 实际硬件驱动集成测试
- 🔄 **计划中**: Python驱动适配器开发
- 🔄 **计划中**: C++ DLL驱动适配器开发
