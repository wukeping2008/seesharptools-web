using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SeeSharpBackend.Models.MISD;

namespace SeeSharpBackend.Services.Drivers
{
    /// <summary>
    /// JYUSB1601真实硬件驱动适配器
    /// 支持JYUSB1601 USB高速数据采集卡的完整功能
    /// </summary>
    public class JYUSB1601DllDriverAdapter : IDriverAdapter
    {
        private readonly ILogger<JYUSB1601DllDriverAdapter> _logger;
        private Assembly? _driverAssembly;
        private readonly Dictionary<int, object> _activeTasks = new();
        private readonly Dictionary<int, object> _activeInstances = new();
        private bool _isInitialized = false;

        public DriverType Type => DriverType.CSharpDll;
        public string Name => "JYUSB1601 Driver";
        public string Version => "2.1.3";
        public bool IsInitialized => _isInitialized;

        public JYUSB1601DllDriverAdapter(ILogger<JYUSB1601DllDriverAdapter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 初始化JYUSB1601驱动
        /// </summary>
        public async Task<bool> InitializeAsync(DriverConfiguration config)
        {
            try
            {
                _logger.LogInformation("正在初始化JYUSB1601驱动: {DriverPath}", config.DriverPath);

                // 加载JYUSB1601.dll程序集
                _driverAssembly = Assembly.LoadFrom(config.DriverPath);
                _logger.LogInformation("成功加载JYUSB1601驱动程序集");

                // 验证关键类型是否存在
                var aiTaskType = _driverAssembly.GetType("JYUSB1601.JYUSB1601AITask");
                var aoTaskType = _driverAssembly.GetType("JYUSB1601.JYUSB1601AOTask");
                var diTaskType = _driverAssembly.GetType("JYUSB1601.JYUSB1601DITask");
                var doTaskType = _driverAssembly.GetType("JYUSB1601.JYUSB1601DOTask");

                if (aiTaskType == null || aoTaskType == null || diTaskType == null || doTaskType == null)
                {
                    _logger.LogError("JYUSB1601驱动中缺少必要的任务类型");
                    return false;
                }

                _isInitialized = true;
                _logger.LogInformation("JYUSB1601驱动初始化成功");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JYUSB1601驱动初始化失败");
                return false;
            }
        }

        /// <summary>
        /// 卸载JYUSB1601驱动
        /// </summary>
        public async Task<bool> UnloadAsync()
        {
            try
            {
                // 停止并释放所有活跃任务
                foreach (var task in _activeTasks.Values.ToList())
                {
                    await DisposeTaskAsync(task);
                }

                _activeTasks.Clear();
                _activeInstances.Clear();
                _driverAssembly = null;
                _isInitialized = false;

                _logger.LogInformation("JYUSB1601驱动已卸载");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "卸载JYUSB1601驱动时发生错误");
                return false;
            }
        }

        /// <summary>
        /// 发现JYUSB1601设备
        /// </summary>
        public async Task<List<HardwareDevice>> DiscoverDevicesAsync()
        {
            var devices = new List<HardwareDevice>();

            try
            {
                _logger.LogInformation("开始发现JYUSB1601设备");

                // 尝试检测JYUSB1601设备（USB设备索引0-7）
                for (int deviceIndex = 0; deviceIndex < 8; deviceIndex++)
                {
                    try
                    {
                        // 尝试创建AI任务来检测设备是否存在
                        var aiTaskType = _driverAssembly?.GetType("JYUSB1601.JYUSB1601AITask");
                        if (aiTaskType != null)
                        {
                            var aiTask = Activator.CreateInstance(aiTaskType, deviceIndex);
                            if (aiTask != null)
                            {
                                // 设备存在，创建设备定义
                                var device = new HardwareDevice
                                {
                                    Id = deviceIndex + 1,
                                    Name = $"JYUSB1601_Device{deviceIndex}",
                                    Model = "JYUSB1601",
                                    DeviceType = "USBDataAcquisitionCard",
                                    Status = DeviceStatus.Online,
                                    SlotNumber = deviceIndex,
                                    SupportedFunctions = new List<string>
                                    {
                                        "AnalogInput",
                                        "AnalogOutput",
                                        "DigitalInput",
                                        "DigitalOutput"
                                    },
                                    Configuration = new Dictionary<string, object>
                                    {
                                        ["MaxChannels"] = 16,
                                        ["MaxSampleRate"] = 1000000, // 1MS/s
                                        ["AOChannels"] = 2,
                                        ["DeviceIndex"] = deviceIndex,
                                        ["Resolution"] = 16,
                                        ["VoltageRanges"] = new[] { "±10V", "±5V", "±2V", "±1V" },
                                        ["Coupling"] = new[] { "DC", "AC" },
                                        ["USBTimeout"] = 5000
                                    }
                                };

                                devices.Add(device);
                                _logger.LogInformation("发现JYUSB1601设备: 设备{DeviceIndex}", deviceIndex);

                                // 清理测试任务
                                var disposeMethod = aiTaskType.GetMethod("Dispose");
                                disposeMethod?.Invoke(aiTask, null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 设备不存在或初始化失败，继续尝试下一个
                        _logger.LogDebug("设备{DeviceIndex}上未发现JYUSB1601设备: {Error}", deviceIndex, ex.Message);
                    }
                }

                _logger.LogInformation("JYUSB1601设备发现完成，共发现{Count}个设备", devices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JYUSB1601设备发现过程中发生错误");
            }

            return devices;
        }

        /// <summary>
        /// 创建JYUSB1601任务
        /// </summary>
        public async Task<object> CreateTaskAsync(string taskType, int deviceId, Dictionary<string, object> parameters)
        {
            try
            {
                if (_driverAssembly == null)
                {
                    _logger.LogError("JYUSB1601驱动未初始化");
                    return null;
                }

                var deviceIndex = parameters.ContainsKey("DeviceIndex") ? Convert.ToInt32(parameters["DeviceIndex"]) : 0;
                var taskId = parameters.ContainsKey("TaskId") ? Convert.ToInt32(parameters["TaskId"]) : 0;

                object? task = null;

                switch (taskType.ToUpper())
                {
                    case "AI":
                    case "ANALOGINPUT":
                        task = await CreateAITaskAsync(deviceIndex, parameters);
                        break;
                    case "AO":
                    case "ANALOGOUTPUT":
                        task = await CreateAOTaskAsync(deviceIndex, parameters);
                        break;
                    case "DI":
                    case "DIGITALINPUT":
                        task = await CreateDITaskAsync(deviceIndex, parameters);
                        break;
                    case "DO":
                    case "DIGITALOUTPUT":
                        task = await CreateDOTaskAsync(deviceIndex, parameters);
                        break;
                    default:
                        _logger.LogWarning("不支持的任务类型: {TaskType}", taskType);
                        return null;
                }

                if (task != null && taskId > 0)
                {
                    _activeTasks[taskId] = task;
                    _logger.LogInformation("成功创建JYUSB1601任务: {TaskType}, TaskId: {TaskId}", taskType, taskId);
                }

                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建JYUSB1601任务失败: {TaskType}", taskType);
                return null;
            }
        }

        private async Task<object?> CreateAITaskAsync(int deviceIndex, Dictionary<string, object> parameters)
        {
            try
            {
                var aiTaskType = _driverAssembly?.GetType("JYUSB1601.JYUSB1601AITask");
                if (aiTaskType == null) return null;

                // 创建AI任务实例
                var aiTask = Activator.CreateInstance(aiTaskType, deviceIndex);
                if (aiTask == null) return null;

                // 配置通道
                var channelCount = parameters.ContainsKey("ChannelCount") ? Convert.ToInt32(parameters["ChannelCount"]) : 1;
                var minRange = parameters.ContainsKey("MinRange") ? Convert.ToDouble(parameters["MinRange"]) : -10.0;
                var maxRange = parameters.ContainsKey("MaxRange") ? Convert.ToDouble(parameters["MaxRange"]) : 10.0;

                // 添加通道
                var addChannelMethod = aiTaskType.GetMethod("AddChannel", new[] { typeof(int), typeof(double), typeof(double) });
                for (int i = 0; i < channelCount; i++)
                {
                    addChannelMethod?.Invoke(aiTask, new object[] { i, minRange, maxRange });
                }

                // 设置采样模式
                var aiModeType = _driverAssembly?.GetType("JYUSB1601.AIMode");
                var continuousMode = aiModeType?.GetField("Continuous")?.GetValue(null);
                var modeProperty = aiTaskType.GetProperty("Mode");
                modeProperty?.SetValue(aiTask, continuousMode);

                // 设置采样率
                var sampleRate = parameters.ContainsKey("SampleRate") ? Convert.ToDouble(parameters["SampleRate"]) : 1000.0;
                var sampleRateProperty = aiTaskType.GetProperty("SampleRate");
                sampleRateProperty?.SetValue(aiTask, sampleRate);

                _logger.LogInformation("成功创建JYUSB1601 AI任务: 设备{DeviceIndex}, 通道数{ChannelCount}, 采样率{SampleRate}", 
                    deviceIndex, channelCount, sampleRate);

                return aiTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建JYUSB1601 AI任务失败");
                return null;
            }
        }

        private async Task<object?> CreateAOTaskAsync(int deviceIndex, Dictionary<string, object> parameters)
        {
            try
            {
                var aoTaskType = _driverAssembly?.GetType("JYUSB1601.JYUSB1601AOTask");
                if (aoTaskType == null) return null;

                var aoTask = Activator.CreateInstance(aoTaskType, deviceIndex);
                if (aoTask == null) return null;

                // 配置AO任务
                var channelCount = parameters.ContainsKey("ChannelCount") ? Convert.ToInt32(parameters["ChannelCount"]) : 1;
                var minRange = parameters.ContainsKey("MinRange") ? Convert.ToDouble(parameters["MinRange"]) : -10.0;
                var maxRange = parameters.ContainsKey("MaxRange") ? Convert.ToDouble(parameters["MaxRange"]) : 10.0;

                // 添加通道
                var addChannelMethod = aoTaskType.GetMethod("AddChannel", new[] { typeof(int), typeof(double), typeof(double) });
                for (int i = 0; i < channelCount; i++)
                {
                    addChannelMethod?.Invoke(aoTask, new object[] { i, minRange, maxRange });
                }

                _logger.LogInformation("成功创建JYUSB1601 AO任务: 设备{DeviceIndex}, 通道数{ChannelCount}", deviceIndex, channelCount);
                return aoTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建JYUSB1601 AO任务失败");
                return null;
            }
        }

        private async Task<object?> CreateDITaskAsync(int deviceIndex, Dictionary<string, object> parameters)
        {
            try
            {
                var diTaskType = _driverAssembly?.GetType("JYUSB1601.JYUSB1601DITask");
                if (diTaskType == null) return null;

                var diTask = Activator.CreateInstance(diTaskType, deviceIndex);
                if (diTask == null) return null;

                // 添加端口
                var portNumber = parameters.ContainsKey("PortNumber") ? Convert.ToInt32(parameters["PortNumber"]) : 0;
                var addChannelMethod = diTaskType.GetMethod("AddChannel", new[] { typeof(int) });
                addChannelMethod?.Invoke(diTask, new object[] { portNumber });

                _logger.LogInformation("成功创建JYUSB1601 DI任务: 设备{DeviceIndex}, 端口{PortNumber}", deviceIndex, portNumber);
                return diTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建JYUSB1601 DI任务失败");
                return null;
            }
        }

        private async Task<object?> CreateDOTaskAsync(int deviceIndex, Dictionary<string, object> parameters)
        {
            try
            {
                var doTaskType = _driverAssembly?.GetType("JYUSB1601.JYUSB1601DOTask");
                if (doTaskType == null) return null;

                var doTask = Activator.CreateInstance(doTaskType, deviceIndex);
                if (doTask == null) return null;

                // 添加端口
                var portNumber = parameters.ContainsKey("PortNumber") ? Convert.ToInt32(parameters["PortNumber"]) : 1;
                var addChannelMethod = doTaskType.GetMethod("AddChannel", new[] { typeof(int) });
                addChannelMethod?.Invoke(doTask, new object[] { portNumber });

                _logger.LogInformation("成功创建JYUSB1601 DO任务: 设备{DeviceIndex}, 端口{PortNumber}", deviceIndex, portNumber);
                return doTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建JYUSB1601 DO任务失败");
                return null;
            }
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        public async Task<bool> StartTaskAsync(int taskId)
        {
            try
            {
                if (!_activeTasks.ContainsKey(taskId))
                {
                    _logger.LogWarning("任务{TaskId}不存在", taskId);
                    return false;
                }

                var task = _activeTasks[taskId];
                var startMethod = task.GetType().GetMethod("Start");
                startMethod?.Invoke(task, null);

                _logger.LogInformation("成功启动JYUSB1601任务: {TaskId}", taskId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动JYUSB1601任务失败: {TaskId}", taskId);
                return false;
            }
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        public async Task<bool> StopTaskAsync(int taskId)
        {
            try
            {
                if (!_activeTasks.ContainsKey(taskId))
                {
                    _logger.LogWarning("任务{TaskId}不存在", taskId);
                    return false;
                }

                var task = _activeTasks[taskId];
                var stopMethod = task.GetType().GetMethod("Stop");
                stopMethod?.Invoke(task, null);

                _logger.LogInformation("成功停止JYUSB1601任务: {TaskId}", taskId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止JYUSB1601任务失败: {TaskId}", taskId);
                return false;
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        public async Task<double[]?> ReadDataAsync(int taskId, int sampleCount)
        {
            try
            {
                if (!_activeTasks.ContainsKey(taskId))
                {
                    _logger.LogWarning("任务{TaskId}不存在", taskId);
                    return null;
                }

                var task = _activeTasks[taskId];
                var taskType = task.GetType();

                // 检查是否为AI任务
                if (taskType.Name == "JYUSB1601AITask")
                {
                    // 检查可用样本数
                    var availableSamplesProperty = taskType.GetProperty("AvailableSamples");
                    var availableSamples = (ulong)(availableSamplesProperty?.GetValue(task) ?? 0);

                    if (availableSamples >= (ulong)sampleCount)
                    {
                        // 读取数据
                        var data = new double[sampleCount];
                        var readDataMethod = taskType.GetMethod("ReadData", new[] { typeof(double[]).MakeByRefType(), typeof(int), typeof(int) });
                        var parameters = new object[] { data, sampleCount, -1 };
                        readDataMethod?.Invoke(task, parameters);

                        return (double[])parameters[0];
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取JYUSB1601数据失败: {TaskId}", taskId);
                return null;
            }
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        public async Task<bool> WriteDataAsync(int taskId, double[] data)
        {
            try
            {
                if (!_activeTasks.ContainsKey(taskId))
                {
                    _logger.LogWarning("任务{TaskId}不存在", taskId);
                    return false;
                }

                var task = _activeTasks[taskId];
                var taskType = task.GetType();

                // 检查是否为AO任务
                if (taskType.Name == "JYUSB1601AOTask")
                {
                    var writeDataMethod = taskType.GetMethod("WriteData", new[] { typeof(double[]), typeof(int) });
                    writeDataMethod?.Invoke(task, new object[] { data, -1 });
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入JYUSB1601数据失败: {TaskId}", taskId);
                return false;
            }
        }

        /// <summary>
        /// 获取任务状态
        /// </summary>
        public async Task<object?> GetTaskStatusAsync(int taskId)
        {
            try
            {
                if (!_activeTasks.ContainsKey(taskId))
                {
                    return new { Status = "NotFound", Message = "任务不存在" };
                }

                var task = _activeTasks[taskId];
                var taskType = task.GetType();

                // 获取任务状态信息
                var status = new
                {
                    Status = "Running",
                    TaskType = taskType.Name,
                    TaskId = taskId,
                    AvailableSamples = taskType.GetProperty("AvailableSamples")?.GetValue(task) ?? 0,
                    IsRunning = true
                };

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取JYUSB1601任务状态失败: {TaskId}", taskId);
                return new { Status = "Error", Message = ex.Message };
            }
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        public async Task<object?> ExecuteMethodAsync(object taskInstance, string methodName, object[] parameters)
        {
            try
            {
                var taskType = taskInstance.GetType();
                var method = taskType.GetMethod(methodName);
                
                if (method == null)
                {
                    _logger.LogWarning("方法{MethodName}在任务类型{TaskType}中不存在", methodName, taskType.Name);
                    return null;
                }

                var result = method.Invoke(taskInstance, parameters);
                _logger.LogDebug("成功执行方法: {MethodName}", methodName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行方法{MethodName}失败", methodName);
                return null;
            }
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        public async Task<object?> GetPropertyAsync(object taskInstance, string propertyName)
        {
            try
            {
                var taskType = taskInstance.GetType();
                var property = taskType.GetProperty(propertyName);
                
                if (property == null)
                {
                    _logger.LogWarning("属性{PropertyName}在任务类型{TaskType}中不存在", propertyName, taskType.Name);
                    return null;
                }

                var value = property.GetValue(taskInstance);
                _logger.LogDebug("成功获取属性: {PropertyName} = {Value}", propertyName, value);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取属性{PropertyName}失败", propertyName);
                return null;
            }
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        public async Task<bool> SetPropertyAsync(object taskInstance, string propertyName, object value)
        {
            try
            {
                var taskType = taskInstance.GetType();
                var property = taskType.GetProperty(propertyName);
                
                if (property == null)
                {
                    _logger.LogWarning("属性{PropertyName}在任务类型{TaskType}中不存在", propertyName, taskType.Name);
                    return false;
                }

                if (!property.CanWrite)
                {
                    _logger.LogWarning("属性{PropertyName}是只读的", propertyName);
                    return false;
                }

                property.SetValue(taskInstance, value);
                _logger.LogDebug("成功设置属性: {PropertyName} = {Value}", propertyName, value);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置属性{PropertyName}失败", propertyName);
                return false;
            }
        }

        /// <summary>
        /// 释放任务实例
        /// </summary>
        public async Task<bool> DisposeTaskAsync(object taskInstance)
        {
            try
            {
                // 查找并移除任务
                var taskToRemove = _activeTasks.FirstOrDefault(kvp => kvp.Value == taskInstance);
                if (taskToRemove.Key != 0)
                {
                    _activeTasks.Remove(taskToRemove.Key);
                }

                // 停止任务
                var stopMethod = taskInstance.GetType().GetMethod("Stop");
                stopMethod?.Invoke(taskInstance, null);
                
                // 释放资源
                var disposeMethod = taskInstance.GetType().GetMethod("Dispose");
                disposeMethod?.Invoke(taskInstance, null);
                
                _logger.LogInformation("成功释放JYUSB1601任务实例");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放JYUSB1601任务实例失败");
                return false;
            }
        }

        /// <summary>
        /// 释放任务
        /// </summary>
        public async Task<bool> DisposeTaskAsync(int taskId)
        {
            try
            {
                if (_activeTasks.ContainsKey(taskId))
                {
                    var task = _activeTasks[taskId];
                    
                    // 停止任务
                    await StopTaskAsync(taskId);
                    
                    // 释放资源
                    var disposeMethod = task.GetType().GetMethod("Dispose");
                    disposeMethod?.Invoke(task, null);
                    
                    _activeTasks.Remove(taskId);
                    _logger.LogInformation("成功释放JYUSB1601任务: {TaskId}", taskId);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放JYUSB1601任务失败: {TaskId}", taskId);
                return false;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                // 停止并释放所有活跃任务
                foreach (var taskId in _activeTasks.Keys.ToList())
                {
                    DisposeTaskAsync(taskId).Wait();
                }

                _activeTasks.Clear();
                _activeInstances.Clear();
                _isInitialized = false;

                _logger.LogInformation("JYUSB1601驱动适配器已释放");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放JYUSB1601驱动适配器时发生错误");
            }
        }
    }
}
