using Microsoft.Extensions.Logging;
using SeeSharpBackend.Models.MISD;
using SeeSharpBackend.Services.Drivers;
using System.Collections.Concurrent;

namespace SeeSharpBackend.Services.MISD
{
    /// <summary>
    /// MISD服务实现
    /// 基于通用驱动管理器的标准化硬件抽象层
    /// </summary>
    public class MISDService : IMISDService
    {
        private readonly ILogger<MISDService> _logger;
        private readonly IDriverManager _driverManager;
        
        // 任务管理
        private readonly ConcurrentDictionary<int, MISDTaskConfiguration> _tasks = new();
        private readonly ConcurrentDictionary<int, object> _taskInstances = new();
        private int _nextTaskId = 1;
        
        // 设备缓存
        private readonly ConcurrentDictionary<int, HardwareDevice> _devices = new();
        
        public MISDService(
            ILogger<MISDService> logger,
            IDriverManager driverManager)
        {
            _logger = logger;
            _driverManager = driverManager;
        }
        
        public async Task InitializeAsync()
        {
            _logger.LogInformation("正在初始化MISD服务...");
            
            try
            {
                // 初始化驱动管理器
                await _driverManager.InitializeAsync();
                
                // 发现设备
                var devices = await DiscoverDevicesAsync();
                _logger.LogInformation($"发现 {devices.Count} 个设备");
                
                _logger.LogInformation("MISD服务初始化完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MISD服务初始化失败");
                throw;
            }
        }
        
        public async Task<List<HardwareDevice>> DiscoverDevicesAsync()
        {
            _logger.LogInformation("开始设备发现...");
            
            try
            {
                // 使用驱动管理器发现所有设备
                var allDevices = await _driverManager.DiscoverAllDevicesAsync();
                
                // 更新设备缓存
                _devices.Clear();
                foreach (var device in allDevices)
                {
                    _devices.TryAdd(device.Id, device);
                }
                
                _logger.LogInformation($"设备发现完成，共发现 {allDevices.Count} 个设备");
                return allDevices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设备发现过程中发生错误");
                throw;
            }
        }
        
        public async Task<HardwareDevice?> GetDeviceAsync(int deviceId)
        {
            if (_devices.TryGetValue(deviceId, out var device))
            {
                return device;
            }
            
            // 如果缓存中没有，尝试重新发现
            await DiscoverDevicesAsync();
            _devices.TryGetValue(deviceId, out device);
            return device;
        }
        
        public async Task<List<MISDDefinition>> GetDeviceFunctionsAsync(int deviceId)
        {
            var device = await GetDeviceAsync(deviceId);
            if (device == null)
            {
                return new List<MISDDefinition>();
            }
            
            // 根据设备能力返回支持的MISD功能
            var functions = new List<MISDDefinition>();
            
            foreach (var capability in device.SupportedFunctions)
            {
                var definition = CreateMISDDefinition(capability, device);
                if (definition != null)
                {
                    functions.Add(definition);
                }
            }
            
            return functions;
        }
        
        public async Task<MISDTaskConfiguration> CreateTaskAsync(MISDTaskConfiguration taskConfig)
        {
            var taskId = Interlocked.Increment(ref _nextTaskId);
            taskConfig.Id = taskId;
            taskConfig.Status = Models.MISD.TaskStatus.Created;
            taskConfig.CreatedAt = DateTime.UtcNow;
            
            _tasks.TryAdd(taskId, taskConfig);
            
            _logger.LogInformation($"创建任务 {taskId}: {taskConfig.TaskName} ({taskConfig.TaskType})");
            
            // 创建硬件任务实例
            var device = await GetDeviceAsync(taskConfig.DeviceId);
            if (device != null)
            {
                var taskInstance = await CreateHardwareTaskInstanceAsync(device, taskConfig);
                if (taskInstance != null)
                {
                    _taskInstances.TryAdd(taskId, taskInstance);
                    taskConfig.Status = Models.MISD.TaskStatus.Configured;
                }
            }
            
            return taskConfig;
        }
        
        public async Task<bool> StartTaskAsync(int taskId)
        {
            if (!_tasks.TryGetValue(taskId, out var taskConfig) || 
                !_taskInstances.TryGetValue(taskId, out var taskInstance))
            {
                return false;
            }
            
            try
            {
                var device = await GetDeviceAsync(taskConfig.DeviceId);
                if (device == null) return false;
                
                // 获取对应的驱动适配器
                var driver = GetDriverForDevice(device);
                if (driver == null) return false;
                
                // 调用Start方法
                await driver.ExecuteMethodAsync(taskInstance, "Start", Array.Empty<object>());
                
                taskConfig.Status = Models.MISD.TaskStatus.Started;
                _logger.LogInformation($"任务 {taskId} 已启动");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"启动任务 {taskId} 失败");
                taskConfig.Status = Models.MISD.TaskStatus.Error;
                return false;
            }
        }
        
        public async Task<bool> StopTaskAsync(int taskId)
        {
            if (!_tasks.TryGetValue(taskId, out var taskConfig) || 
                !_taskInstances.TryGetValue(taskId, out var taskInstance))
            {
                return false;
            }
            
            try
            {
                var device = await GetDeviceAsync(taskConfig.DeviceId);
                if (device == null) return false;
                
                var driver = GetDriverForDevice(device);
                if (driver == null) return false;
                
                // 调用Stop方法
                await driver.ExecuteMethodAsync(taskInstance, "Stop", Array.Empty<object>());
                
                taskConfig.Status = Models.MISD.TaskStatus.Stopped;
                _logger.LogInformation($"任务 {taskId} 已停止");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"停止任务 {taskId} 失败");
                taskConfig.Status = Models.MISD.TaskStatus.Error;
                return false;
            }
        }
        
        public async Task<Models.MISD.TaskStatus> GetTaskStatusAsync(int taskId)
        {
            if (_tasks.TryGetValue(taskId, out var taskConfig))
            {
                return taskConfig.Status;
            }
            
            return Models.MISD.TaskStatus.Error;
        }
        
        public async Task<double[,]> ReadDataAsync(int taskId, int samplesPerChannel, int timeout = 10000)
        {
            if (!_tasks.TryGetValue(taskId, out var taskConfig) || 
                !_taskInstances.TryGetValue(taskId, out var taskInstance))
            {
                throw new InvalidOperationException($"任务 {taskId} 不存在");
            }
            
            try
            {
                var device = await GetDeviceAsync(taskConfig.DeviceId);
                if (device == null) 
                    throw new InvalidOperationException($"设备 {taskConfig.DeviceId} 不存在");
                
                var driver = GetDriverForDevice(device);
                if (driver == null)
                    throw new InvalidOperationException($"设备 {device.Model} 驱动不可用");
                
                // 创建数据缓冲区
                var channelCount = taskConfig.Channels.Count;
                var buffer = new double[samplesPerChannel, channelCount];
                
                // 调用ReadData方法
                await driver.ExecuteMethodAsync(taskInstance, "ReadData", 
                    new object[] { buffer, samplesPerChannel, timeout });
                
                return buffer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取任务 {taskId} 数据失败");
                throw;
            }
        }
        
        public async Task<bool> WriteDataAsync(int taskId, double[,] data, int timeout = 10000)
        {
            if (!_tasks.TryGetValue(taskId, out var taskConfig) || 
                !_taskInstances.TryGetValue(taskId, out var taskInstance))
            {
                return false;
            }
            
            try
            {
                var device = await GetDeviceAsync(taskConfig.DeviceId);
                if (device == null) return false;
                
                var driver = GetDriverForDevice(device);
                if (driver == null) return false;
                
                // 调用WriteData方法
                await driver.ExecuteMethodAsync(taskInstance, "WriteData", 
                    new object[] { data, timeout });
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"写入任务 {taskId} 数据失败");
                return false;
            }
        }
        
        public async Task<int> GetAvailableSamplesAsync(int taskId)
        {
            if (!_tasks.TryGetValue(taskId, out var taskConfig) || 
                !_taskInstances.TryGetValue(taskId, out var taskInstance))
            {
                return 0;
            }
            
            try
            {
                var device = await GetDeviceAsync(taskConfig.DeviceId);
                if (device == null) return 0;
                
                var driver = GetDriverForDevice(device);
                if (driver == null) return 0;
                
                // 获取AvailableSamples属性
                var result = await driver.GetPropertyAsync(taskInstance, "AvailableSamples");
                
                return result is int samples ? samples : 
                       result is ulong ulongSamples ? (int)ulongSamples : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取任务 {taskId} 可用样本数失败");
                return 0;
            }
        }
        
        public async Task<bool> SendSoftwareTriggerAsync(int taskId)
        {
            if (!_tasks.TryGetValue(taskId, out var taskConfig) || 
                !_taskInstances.TryGetValue(taskId, out var taskInstance))
            {
                return false;
            }
            
            try
            {
                var device = await GetDeviceAsync(taskConfig.DeviceId);
                if (device == null) return false;
                
                var driver = GetDriverForDevice(device);
                if (driver == null) return false;
                
                // 调用SendSoftwareTrigger方法
                await driver.ExecuteMethodAsync(taskInstance, "SendSoftwareTrigger", 
                    Array.Empty<object>());
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送软件触发到任务 {taskId} 失败");
                return false;
            }
        }
        
        public async Task<bool> WaitUntilDoneAsync(int taskId, int timeout = -1)
        {
            if (!_tasks.TryGetValue(taskId, out var taskConfig) || 
                !_taskInstances.TryGetValue(taskId, out var taskInstance))
            {
                return false;
            }
            
            try
            {
                var device = await GetDeviceAsync(taskConfig.DeviceId);
                if (device == null) return false;
                
                var driver = GetDriverForDevice(device);
                if (driver == null) return false;
                
                // 调用WaitUntilDone方法
                await driver.ExecuteMethodAsync(taskInstance, "WaitUntilDone", 
                    new object[] { timeout });
                
                taskConfig.Status = Models.MISD.TaskStatus.Completed;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"等待任务 {taskId} 完成失败");
                taskConfig.Status = Models.MISD.TaskStatus.Error;
                return false;
            }
        }
        
        public async Task<bool> DisposeTaskAsync(int taskId)
        {
            try
            {
                if (_taskInstances.TryRemove(taskId, out var taskInstance))
                {
                    var device = await GetDeviceAsync(_tasks[taskId].DeviceId);
                    if (device != null)
                    {
                        var driver = GetDriverForDevice(device);
                        if (driver != null)
                        {
                            await driver.DisposeTaskAsync(taskInstance);
                        }
                    }
                }
                
                _tasks.TryRemove(taskId, out _);
                
                _logger.LogInformation($"任务 {taskId} 已释放");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"释放任务 {taskId} 失败");
                return false;
            }
        }
        
        #region 私有方法
        
        /// <summary>
        /// 获取设备对应的驱动适配器
        /// </summary>
        private IDriverAdapter? GetDriverForDevice(HardwareDevice device)
        {
            // 根据设备型号确定驱动名称
            var driverName = GetDriverNameForDevice(device);
            return _driverManager.GetDriver(driverName);
        }
        
        /// <summary>
        /// 根据设备确定驱动名称
        /// </summary>
        private string GetDriverNameForDevice(HardwareDevice device)
        {
            // 简化的驱动名称映射
            if (device.Model.StartsWith("JY5500"))
                return "JY5500";
            if (device.Model.StartsWith("JYUSB"))
                return "JYUSB1601";
            if (device.Model == "MockDevice")
                return "MockDriver";
            
            return device.Model;
        }
        
        /// <summary>
        /// 创建MISD定义
        /// </summary>
        private MISDDefinition? CreateMISDDefinition(string capability, HardwareDevice device)
        {
            return capability switch
            {
                "AnalogInput" => new MISDDefinition
                {
                    FunctionName = "模拟输入",
                    FunctionType = "AI",
                    Description = "多通道模拟信号采集",
                    SupportedHardware = new List<string> { device.Model },
                    Parameters = new List<MISDParameter>
                    {
                        new() { Name = "SampleRate", Type = "double", Description = "采样率" },
                        new() { Name = "Channels", Type = "int[]", Description = "通道列表" },
                        new() { Name = "Range", Type = "double", Description = "电压范围" }
                    }
                },
                "AnalogOutput" => new MISDDefinition
                {
                    FunctionName = "模拟输出",
                    FunctionType = "AO",
                    Description = "多通道模拟信号输出",
                    SupportedHardware = new List<string> { device.Model },
                    Parameters = new List<MISDParameter>
                    {
                        new() { Name = "UpdateRate", Type = "double", Description = "更新率" },
                        new() { Name = "Channels", Type = "int[]", Description = "通道列表" },
                        new() { Name = "Range", Type = "double", Description = "电压范围" }
                    }
                },
                "DigitalInput" => new MISDDefinition
                {
                    FunctionName = "数字输入",
                    FunctionType = "DI",
                    Description = "数字信号读取",
                    SupportedHardware = new List<string> { device.Model },
                    Parameters = new List<MISDParameter>
                    {
                        new() { Name = "Ports", Type = "int[]", Description = "端口列表" }
                    }
                },
                "DigitalOutput" => new MISDDefinition
                {
                    FunctionName = "数字输出",
                    FunctionType = "DO",
                    Description = "数字信号输出",
                    SupportedHardware = new List<string> { device.Model },
                    Parameters = new List<MISDParameter>
                    {
                        new() { Name = "Ports", Type = "int[]", Description = "端口列表" }
                    }
                },
                _ => null
            };
        }
        
        /// <summary>
        /// 创建硬件任务实例
        /// </summary>
        private async Task<object?> CreateHardwareTaskInstanceAsync(HardwareDevice device, MISDTaskConfiguration taskConfig)
        {
            try
            {
                var driver = GetDriverForDevice(device);
                if (driver == null)
                {
                    _logger.LogError($"设备 {device.Model} 没有可用的驱动");
                    return null;
                }
                
                var taskTypeName = GetTaskTypeName(taskConfig.TaskType);
                var parameters = new Dictionary<string, object>
                {
                    ["DeviceId"] = device.SlotNumber ?? 0
                };
                
                var instance = await driver.CreateTaskAsync(taskTypeName, device.SlotNumber ?? 0, parameters);
                
                if (instance != null)
                {
                    // 配置任务
                    await ConfigureTaskAsync(driver, instance, taskConfig);
                }
                
                return instance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"创建硬件任务实例失败: {taskConfig.TaskType}");
                return null;
            }
        }
        
        /// <summary>
        /// 配置任务
        /// </summary>
        private async Task ConfigureTaskAsync(IDriverAdapter driver, object taskInstance, MISDTaskConfiguration taskConfig)
        {
            try
            {
                // 配置通道
                foreach (var channel in taskConfig.Channels)
                {
                    await driver.ExecuteMethodAsync(taskInstance, "AddChannel", new object[]
                    {
                        channel.ChannelId,
                        channel.RangeLow,
                        channel.RangeHigh,
                        channel.Terminal
                    });
                }
                
                // 配置采样参数
                if (taskConfig.Sampling != null)
                {
                    await driver.SetPropertyAsync(taskInstance, "SampleRate", taskConfig.Sampling.SampleRate);
                    await driver.SetPropertyAsync(taskInstance, "Mode", taskConfig.Sampling.Mode);
                    
                    if (taskConfig.Sampling.SamplesToAcquire > 0)
                    {
                        await driver.SetPropertyAsync(taskInstance, "SamplesToAcquire", taskConfig.Sampling.SamplesToAcquire);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置任务失败");
                throw;
            }
        }
        
        /// <summary>
        /// 获取任务类型名称
        /// </summary>
        private string GetTaskTypeName(string taskType)
        {
            return taskType switch
            {
                "AI" => "JY5500AITask",
                "AO" => "JY5500AOTask",
                "DI" => "JY5500DITask",
                "DO" => "JY5500DOTask",
                "CI" => "JY5500CITask",
                "CO" => "JY5500COTask",
                _ => throw new ArgumentException($"不支持的任务类型: {taskType}")
            };
        }
        
        #endregion
    }
}
