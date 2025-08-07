using Microsoft.Extensions.Logging;
using SeeSharpBackend.Models.MISD;
using System.Collections.Concurrent;

namespace SeeSharpBackend.Services.Drivers
{
    /// <summary>
    /// 模拟驱动适配器
    /// 用于在没有真实硬件时提供完整的USB-1601演示功能
    /// </summary>
    public class MockDriverAdapter : IDriverAdapter
    {
        private readonly ILogger<MockDriverAdapter> _logger;
        private readonly ConcurrentDictionary<int, MockDevice> _mockDevices = new();
        private readonly ConcurrentDictionary<object, MockTaskInstance> _taskInstances = new();
        private readonly Random _random = new();
        
        public string Name => "MockDriver";
        public string Version => "1.0.0";
        public DriverType Type => DriverType.CSharpDll;
        public bool IsLoaded { get; private set; }
        public bool IsInitialized { get; private set; }

        public MockDriverAdapter(ILogger<MockDriverAdapter> logger)
        {
            _logger = logger;
        }

        public async Task<bool> InitializeAsync(DriverConfiguration configuration)
        {
            try
            {
                _logger.LogInformation("初始化模拟驱动适配器");
                
                // 创建模拟的USB-1601设备
                var mockDevice = new MockDevice
                {
                    Id = 0,
                    Model = "MockUSB-1601",
                    SerialNumber = "MOCK123456",
                    SlotNumber = 0,
                    Manufacturer = "Mock Inc.",
                    IsConnected = true,
                    SupportedFunctions = new List<string> { "AnalogInput", "AnalogOutput", "DigitalInput", "DigitalOutput" },
                    DeviceType = "MockDevice"
                };
                
                _mockDevices.TryAdd(0, mockDevice);
                IsLoaded = true;
                IsInitialized = true;
                
                _logger.LogInformation("模拟驱动适配器初始化完成，创建了模拟USB-1601设备");
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
            _logger.LogInformation("卸载模拟驱动适配器");
            
            // 停止所有任务
            foreach (var taskInstance in _taskInstances.Values)
            {
                taskInstance.Stop();
            }
            
            _taskInstances.Clear();
            _mockDevices.Clear();
            IsLoaded = false;
            IsInitialized = false;
            
            return true;
        }

        public async Task<List<HardwareDevice>> DiscoverDevicesAsync()
        {
            var devices = new List<HardwareDevice>();
            
            foreach (var mockDevice in _mockDevices.Values)
            {
                devices.Add(new HardwareDevice
                {
                    Id = mockDevice.Id,
                    Model = mockDevice.Model,
                    SlotNumber = mockDevice.SlotNumber,
                    SupportedFunctions = mockDevice.SupportedFunctions
                });
            }
            
            return devices;
        }

        public async Task<object> CreateTaskAsync(string taskType, int deviceId, Dictionary<string, object> parameters)
        {
            var taskInstance = new MockTaskInstance
            {
                TaskType = taskType,
                DeviceId = deviceId,
                Parameters = parameters,
                IsStarted = false,
                StartTime = DateTime.UtcNow
            };
            
            _taskInstances.TryAdd(taskInstance, taskInstance);
            _logger.LogInformation("创建模拟任务实例: {TaskType} on Device {DeviceId}", taskType, deviceId);
            
            return taskInstance;
        }

        public async Task<bool> DisposeTaskAsync(object taskInstance)
        {
            if (taskInstance is MockTaskInstance mockTask)
            {
                mockTask.Stop();
                _taskInstances.TryRemove(mockTask, out _);
                _logger.LogInformation("释放模拟任务实例: {TaskType}", mockTask.TaskType);
                return true;
            }
            
            return false;
        }

        public async Task<object?> ExecuteMethodAsync(object taskInstance, string methodName, object[] parameters)
        {
            if (taskInstance is not MockTaskInstance mockTask)
                return null;

            switch (methodName.ToLower())
            {
                case "addchannel":
                    if (parameters.Length >= 4)
                    {
                        var channelId = Convert.ToInt32(parameters[0]);
                        var rangeLow = Convert.ToDouble(parameters[1]);
                        var rangeHigh = Convert.ToDouble(parameters[2]);
                        var terminal = parameters[3].ToString();
                        
                        mockTask.Channels.Add(new MockChannel
                        {
                            ChannelId = channelId,
                            RangeLow = rangeLow,
                            RangeHigh = rangeHigh,
                            Terminal = terminal ?? "RSE"
                        });
                        
                        _logger.LogDebug("添加模拟通道 {ChannelId}: {RangeLow}V to {RangeHigh}V", channelId, rangeLow, rangeHigh);
                    }
                    break;

                case "start":
                    mockTask.IsStarted = true;
                    mockTask.StartTime = DateTime.UtcNow;
                    _logger.LogInformation("启动模拟任务: {TaskType}", mockTask.TaskType);
                    break;

                case "stop":
                    mockTask.IsStarted = false;
                    _logger.LogInformation("停止模拟任务: {TaskType}", mockTask.TaskType);
                    break;

                case "readdata":
                    if (parameters.Length >= 3 && parameters[0] is double[,] buffer)
                    {
                        var samplesPerChannel = Convert.ToInt32(parameters[1]);
                        var timeout = Convert.ToInt32(parameters[2]);
                        
                        // 生成模拟数据
                        GenerateSimulatedData(mockTask, buffer, samplesPerChannel);
                        return buffer;
                    }
                    break;

                case "writedata":
                    if (parameters.Length >= 2 && parameters[0] is double[,] data)
                    {
                        // 模拟数据写入
                        _logger.LogDebug("模拟写入数据: {Samples} samples", data.GetLength(1));
                        return true;
                    }
                    break;

                case "sendsoftwaretrigger":
                    _logger.LogDebug("发送模拟软件触发");
                    return true;

                case "waituntildone":
                    // 模拟等待完成
                    await Task.Delay(100);
                    return true;
            }

            return null;
        }

        public async Task<object?> GetPropertyAsync(object taskInstance, string propertyName)
        {
            if (taskInstance is not MockTaskInstance mockTask)
                return null;

            return propertyName.ToLower() switch
            {
                "availablesamples" => GenerateAvailableSamples(),
                "isstarted" => mockTask.IsStarted,
                "starttime" => mockTask.StartTime,
                "channelcount" => mockTask.Channels.Count,
                _ => null
            };
        }

        public async Task<bool> SetPropertyAsync(object taskInstance, string propertyName, object value)
        {
            if (taskInstance is not MockTaskInstance mockTask)
                return false;

            switch (propertyName.ToLower())
            {
                case "samplerate":
                    mockTask.SampleRate = Convert.ToDouble(value);
                    _logger.LogDebug("设置采样率: {SampleRate} Hz", mockTask.SampleRate);
                    return true;

                case "mode":
                    mockTask.Mode = value.ToString() ?? "Continuous";
                    _logger.LogDebug("设置采集模式: {Mode}", mockTask.Mode);
                    return true;

                case "samplestoacquire":
                    mockTask.SamplesToAcquire = Convert.ToInt64(value);
                    _logger.LogDebug("设置采样数量: {Samples}", mockTask.SamplesToAcquire);
                    return true;

                default:
                    return false;
            }
        }

        #region 私有方法

        /// <summary>
        /// 生成模拟数据
        /// </summary>
        private void GenerateSimulatedData(MockTaskInstance taskInstance, double[,] buffer, int samplesPerChannel)
        {
            var channelCount = taskInstance.Channels.Count;
            var currentTime = DateTime.UtcNow;
            var timeElapsed = (currentTime - taskInstance.StartTime).TotalSeconds;

            for (int ch = 0; ch < channelCount && ch < buffer.GetLength(0); ch++)
            {
                var channel = taskInstance.Channels[ch];
                var amplitude = (channel.RangeHigh - channel.RangeLow) / 4.0; // 使用量程的1/4作为幅度
                var offset = (channel.RangeHigh + channel.RangeLow) / 2.0;
                
                for (int sample = 0; sample < samplesPerChannel && sample < buffer.GetLength(1); sample++)
                {
                    var sampleTime = timeElapsed + (sample / taskInstance.SampleRate);
                    
                    // 生成不同频率的正弦波 + 噪声
                    var frequency = 5.0 * (ch + 1); // 每个通道不同频率
                    var signal = amplitude * Math.Sin(2 * Math.PI * frequency * sampleTime);
                    var noise = (_random.NextDouble() - 0.5) * amplitude * 0.1; // 10%噪声
                    
                    buffer[sample, ch] = offset + signal + noise;
                }
            }
        }

        /// <summary>
        /// 生成可用样本数
        /// </summary>
        private int GenerateAvailableSamples()
        {
            // 模拟缓冲区中有数据
            return _random.Next(100, 1000);
        }

        #endregion

        public void Dispose()
        {
            UnloadAsync().Wait();
        }
    }

    /// <summary>
    /// 模拟设备
    /// </summary>
    internal class MockDevice
    {
        public int Id { get; set; }
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public int? SlotNumber { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public List<string> SupportedFunctions { get; set; } = new();
        public string DeviceType { get; set; } = string.Empty;
    }

    /// <summary>
    /// 模拟任务实例
    /// </summary>
    internal class MockTaskInstance
    {
        public string TaskType { get; set; } = string.Empty;
        public int DeviceId { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new();
        public bool IsStarted { get; set; }
        public DateTime StartTime { get; set; }
        public double SampleRate { get; set; } = 10000;
        public string Mode { get; set; } = "Continuous";
        public long SamplesToAcquire { get; set; } = -1;
        public List<MockChannel> Channels { get; set; } = new();

        public void Stop()
        {
            IsStarted = false;
        }
    }

    /// <summary>
    /// 模拟通道
    /// </summary>
    internal class MockChannel
    {
        public int ChannelId { get; set; }
        public double RangeLow { get; set; }
        public double RangeHigh { get; set; }
        public string Terminal { get; set; } = "RSE";
    }
}
