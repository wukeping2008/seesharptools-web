using Microsoft.Extensions.Logging;
using SeeSharpBackend.Models.MISD;
using System.Collections.Concurrent;

namespace SeeSharpBackend.Services.Drivers
{
    /// <summary>
    /// 模拟驱动适配器
    /// 用于在没有真实硬件的情况下进行测试和开发
    /// </summary>
    public class MockDriverAdapter : IDriverAdapter
    {
        private readonly ILogger<MockDriverAdapter> _logger;
        private readonly ConcurrentDictionary<string, object> _mockTasks = new();
        private readonly Random _random = new();
        private bool _isInitialized = false;
        
        public DriverType Type => DriverType.CSharpDll;
        public string Name => "MockDriver";
        public string Version => "1.0.0";
        public bool IsInitialized => _isInitialized;
        
        public MockDriverAdapter(ILogger<MockDriverAdapter> logger)
        {
            _logger = logger;
        }
        
        public async Task<bool> InitializeAsync(DriverConfiguration config)
        {
            _logger.LogInformation("正在初始化模拟驱动适配器...");
            
            try
            {
                // 模拟初始化延迟
                await Task.Delay(100);
                
                _isInitialized = true;
                _logger.LogInformation("模拟驱动适配器初始化成功");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "模拟驱动适配器初始化失败");
                return false;
            }
        }
        
        public async Task<bool> UnloadAsync()
        {
            _logger.LogInformation("正在卸载模拟驱动适配器...");
            
            try
            {
                // 清理所有模拟任务
                _mockTasks.Clear();
                
                await Task.Delay(50);
                
                _isInitialized = false;
                _logger.LogInformation("模拟驱动适配器卸载成功");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "模拟驱动适配器卸载失败");
                return false;
            }
        }
        
        public async Task<List<HardwareDevice>> DiscoverDevicesAsync()
        {
            _logger.LogInformation("开始模拟设备发现...");
            
            await Task.Delay(200); // 模拟设备扫描时间
            
            var devices = new List<HardwareDevice>
            {
                new HardwareDevice
                {
                    Id = 0,
                    Name = "模拟数据采集卡 #0",
                    Model = "MockDevice",
                    DeviceType = "Mock",
                    SlotNumber = 0,
                    Status = DeviceStatus.Online,
                    SupportedFunctions = new List<string>
                    {
                        "AnalogInput",
                        "AnalogOutput", 
                        "DigitalInput",
                        "DigitalOutput"
                    },
                    Configuration = new Dictionary<string, object>
                    {
                        ["MaxSampleRate"] = 1000000.0,
                        ["ChannelCount"] = 16,
                        ["Resolution"] = 16,
                        ["VoltageRanges"] = new[] { "±10V", "±5V", "±1V" },
                        ["SerialNumber"] = "MOCK-001"
                    }
                }
            };
            
            _logger.LogInformation($"模拟设备发现完成，发现 {devices.Count} 个设备");
            return devices;
        }
        
        public async Task<object> CreateTaskAsync(string taskType, int deviceId, Dictionary<string, object> parameters)
        {
            _logger.LogInformation($"创建模拟任务: {taskType}, 设备ID: {deviceId}");
            
            await Task.Delay(50); // 模拟任务创建时间
            
            var taskId = Guid.NewGuid().ToString();
            var mockTask = new MockTask
            {
                Id = taskId,
                TaskType = taskType,
                DeviceId = deviceId,
                Parameters = parameters,
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };
            
            _mockTasks.TryAdd(taskId, mockTask);
            
            _logger.LogInformation($"模拟任务创建成功: {taskId}");
            return mockTask;
        }
        
        public async Task<object?> ExecuteMethodAsync(object taskInstance, string methodName, object[] parameters)
        {
            if (taskInstance is not MockTask mockTask)
            {
                throw new ArgumentException("无效的任务实例");
            }
            
            _logger.LogInformation($"执行模拟方法: {methodName} on task {mockTask.Id}");
            
            await Task.Delay(10); // 模拟方法执行时间
            
            return methodName switch
            {
                "Start" => await StartTaskAsync(mockTask),
                "Stop" => await StopTaskAsync(mockTask),
                "AddChannel" => await AddChannelAsync(mockTask, parameters),
                "ReadData" => await ReadDataAsync(mockTask, parameters),
                "WriteData" => await WriteDataAsync(mockTask, parameters),
                "SendSoftwareTrigger" => await SendSoftwareTriggerAsync(mockTask),
                "WaitUntilDone" => await WaitUntilDoneAsync(mockTask, parameters),
                _ => throw new NotSupportedException($"不支持的方法: {methodName}")
            };
        }
        
        public async Task<object?> GetPropertyAsync(object taskInstance, string propertyName)
        {
            if (taskInstance is not MockTask mockTask)
            {
                throw new ArgumentException("无效的任务实例");
            }
            
            _logger.LogInformation($"获取模拟属性: {propertyName} from task {mockTask.Id}");
            
            await Task.Delay(5);
            
            return propertyName switch
            {
                "SampleRate" => mockTask.SampleRate,
                "Mode" => mockTask.Mode,
                "SamplesToAcquire" => mockTask.SamplesToAcquire,
                "AvailableSamples" => _random.Next(0, 1000),
                "IsTaskDone" => mockTask.Status == "Completed",
                _ => null
            };
        }
        
        public async Task<bool> SetPropertyAsync(object taskInstance, string propertyName, object value)
        {
            if (taskInstance is not MockTask mockTask)
            {
                throw new ArgumentException("无效的任务实例");
            }
            
            _logger.LogInformation($"设置模拟属性: {propertyName} = {value} on task {mockTask.Id}");
            
            await Task.Delay(5);
            
            switch (propertyName)
            {
                case "SampleRate":
                    mockTask.SampleRate = Convert.ToDouble(value);
                    break;
                case "Mode":
                    mockTask.Mode = value.ToString() ?? "Continuous";
                    break;
                case "SamplesToAcquire":
                    mockTask.SamplesToAcquire = Convert.ToInt32(value);
                    break;
                default:
                    _logger.LogWarning($"未知属性: {propertyName}");
                    return false;
            }
            
            return true;
        }
        
        public async Task<bool> DisposeTaskAsync(object taskInstance)
        {
            if (taskInstance is not MockTask mockTask)
            {
                throw new ArgumentException("无效的任务实例");
            }
            
            _logger.LogInformation($"释放模拟任务: {mockTask.Id}");
            
            await Task.Delay(10);
            
            _mockTasks.TryRemove(mockTask.Id, out _);
            mockTask.Status = "Disposed";
            
            return true;
        }
        
        #region 私有方法
        
        private async Task<bool> StartTaskAsync(MockTask mockTask)
        {
            _logger.LogInformation($"启动模拟任务: {mockTask.Id}");
            
            await Task.Delay(20);
            
            mockTask.Status = "Running";
            mockTask.StartedAt = DateTime.UtcNow;
            
            return true;
        }
        
        private async Task<bool> StopTaskAsync(MockTask mockTask)
        {
            _logger.LogInformation($"停止模拟任务: {mockTask.Id}");
            
            await Task.Delay(20);
            
            mockTask.Status = "Stopped";
            mockTask.StoppedAt = DateTime.UtcNow;
            
            return true;
        }
        
        private async Task<bool> AddChannelAsync(MockTask mockTask, object[] parameters)
        {
            if (parameters.Length < 4)
            {
                throw new ArgumentException("AddChannel需要4个参数: channelId, rangeLow, rangeHigh, terminal");
            }
            
            var channelId = Convert.ToInt32(parameters[0]);
            var rangeLow = Convert.ToDouble(parameters[1]);
            var rangeHigh = Convert.ToDouble(parameters[2]);
            var terminal = parameters[3].ToString();
            
            _logger.LogInformation($"添加模拟通道: {channelId}, 范围: {rangeLow} - {rangeHigh}, 终端: {terminal}");
            
            await Task.Delay(5);
            
            mockTask.Channels.Add(new MockChannel
            {
                Id = channelId,
                RangeLow = rangeLow,
                RangeHigh = rangeHigh,
                Terminal = terminal ?? "RSE"
            });
            
            return true;
        }
        
        private async Task<object> ReadDataAsync(MockTask mockTask, object[] parameters)
        {
            if (parameters.Length < 2)
            {
                throw new ArgumentException("ReadData需要至少2个参数: buffer, samplesPerChannel");
            }
            
            var buffer = parameters[0] as double[,];
            var samplesPerChannel = Convert.ToInt32(parameters[1]);
            
            _logger.LogInformation($"读取模拟数据: {samplesPerChannel} 样本/通道");
            
            await Task.Delay(10);
            
            if (buffer != null)
            {
                var channelCount = buffer.GetLength(1);
                
                // 生成模拟数据
                for (int sample = 0; sample < samplesPerChannel; sample++)
                {
                    for (int channel = 0; channel < channelCount; channel++)
                    {
                        // 生成正弦波 + 噪声
                        var time = sample / mockTask.SampleRate;
                        var frequency = 1000.0 + channel * 500; // 不同通道不同频率
                        var amplitude = 1.0;
                        var noise = (_random.NextDouble() - 0.5) * 0.1;
                        
                        buffer[sample, channel] = amplitude * Math.Sin(2 * Math.PI * frequency * time) + noise;
                    }
                }
            }
            
            return buffer ?? new double[samplesPerChannel, mockTask.Channels.Count];
        }
        
        private async Task<bool> WriteDataAsync(MockTask mockTask, object[] parameters)
        {
            if (parameters.Length < 1)
            {
                throw new ArgumentException("WriteData需要至少1个参数: data");
            }
            
            var data = parameters[0] as double[,];
            
            _logger.LogInformation($"写入模拟数据: {data?.GetLength(0)} 样本");
            
            await Task.Delay(10);
            
            return true;
        }
        
        private async Task<bool> SendSoftwareTriggerAsync(MockTask mockTask)
        {
            _logger.LogInformation($"发送模拟软件触发: {mockTask.Id}");
            
            await Task.Delay(5);
            
            return true;
        }
        
        private async Task<bool> WaitUntilDoneAsync(MockTask mockTask, object[] parameters)
        {
            var timeout = parameters.Length > 0 ? Convert.ToInt32(parameters[0]) : -1;
            
            _logger.LogInformation($"等待模拟任务完成: {mockTask.Id}, 超时: {timeout}ms");
            
            // 模拟任务完成
            await Task.Delay(Math.Min(timeout > 0 ? timeout : 1000, 1000));
            
            mockTask.Status = "Completed";
            mockTask.CompletedAt = DateTime.UtcNow;
            
            return true;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 模拟任务类
    /// </summary>
    public class MockTask
    {
        public string Id { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public int DeviceId { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public string Status { get; set; } = "Created";
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? StoppedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        // 任务配置
        public double SampleRate { get; set; } = 1000000.0;
        public string Mode { get; set; } = "Continuous";
        public int SamplesToAcquire { get; set; } = -1;
        public List<MockChannel> Channels { get; set; } = new();
    }
    
    /// <summary>
    /// 模拟通道类
    /// </summary>
    public class MockChannel
    {
        public int Id { get; set; }
        public double RangeLow { get; set; }
        public double RangeHigh { get; set; }
        public string Terminal { get; set; } = "RSE";
    }
}
