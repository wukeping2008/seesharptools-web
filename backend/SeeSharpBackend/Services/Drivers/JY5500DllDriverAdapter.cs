using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SeeSharpBackend.Models.MISD;

namespace SeeSharpBackend.Services.Drivers
{
    /// <summary>
    /// JY5500真实硬件驱动适配器
    /// 支持JY5500系列数据采集卡的完整功能
    /// </summary>
    public class JY5500DllDriverAdapter : IDriverAdapter
    {
        private readonly ILogger<JY5500DllDriverAdapter> _logger;
        private Assembly? _driverAssembly;
        private readonly Dictionary<int, object> _activeTasks = new();
        private readonly Dictionary<int, object> _activeInstances = new();
        private bool _isInitialized = false;

        public DriverType Type => DriverType.CSharpDll;
        public string Name => "JY5500 Driver";
        public string Version => "4.1.5";
        public bool IsInitialized => _isInitialized;

        public JY5500DllDriverAdapter(ILogger<JY5500DllDriverAdapter> logger)
        {
            _logger = logger;
        }

        public async Task<bool> InitializeAsync(DriverConfiguration config)
        {
            try
            {
                _logger.LogInformation("正在初始化JY5500驱动: {DriverPath}", config.DriverPath);

                // 加载JY5500.dll程序集
                _driverAssembly = Assembly.LoadFrom(config.DriverPath);
                _logger.LogInformation("成功加载JY5500驱动程序集");

                // 验证关键类型是否存在
                var aiTaskType = _driverAssembly.GetType("JY5500.JY5500AITask");
                var aoTaskType = _driverAssembly.GetType("JY5500.JY5500AOTask");
                var diTaskType = _driverAssembly.GetType("JY5500.JY5500DITask");
                var doTaskType = _driverAssembly.GetType("JY5500.JY5500DOTask");

                if (aiTaskType == null || aoTaskType == null || diTaskType == null || doTaskType == null)
                {
                    _logger.LogError("JY5500驱动中缺少必要的任务类型");
                    return false;
                }

                _isInitialized = true;
                _logger.LogInformation("JY5500驱动初始化成功");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JY5500驱动初始化失败");
                return false;
            }
        }

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

                _logger.LogInformation("JY5500驱动已卸载");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "卸载JY5500驱动时发生错误");
                return false;
            }
        }

        public async Task<List<HardwareDevice>> DiscoverDevicesAsync()
        {
            var devices = new List<HardwareDevice>();

            try
            {
                _logger.LogInformation("开始发现JY5500设备");

                // JY5500系列设备型号
                var deviceModels = new[]
                {
                    new { Model = "JY5510", Channels = 32, MaxSampleRate = 2000000, AOChannels = 4 },
                    new { Model = "JY5511", Channels = 32, MaxSampleRate = 1250000, AOChannels = 4 },
                    new { Model = "JY5515", Channels = 16, MaxSampleRate = 2000000, AOChannels = 2 },
                    new { Model = "JY5516", Channels = 16, MaxSampleRate = 1250000, AOChannels = 2 }
                };

                // 尝试检测每种型号的设备（板卡索引0-3）
                for (int boardIndex = 0; boardIndex < 4; boardIndex++)
                {
                    foreach (var deviceModel in deviceModels)
                    {
                        try
                        {
                            // 尝试创建AI任务来检测设备是否存在
                            var aiTaskType = _driverAssembly?.GetType("JY5500.JY5500AITask");
                            if (aiTaskType != null)
                            {
                                var aiTask = Activator.CreateInstance(aiTaskType, boardIndex);
                                if (aiTask != null)
                                {
                                    // 设备存在，创建设备定义
                                    var device = new HardwareDevice
                                    {
                                        Id = boardIndex + 1,
                                        Name = $"{deviceModel.Model}_Board{boardIndex}",
                                        Model = deviceModel.Model,
                                        DeviceType = "DataAcquisitionCard",
                                        Status = DeviceStatus.Online,
                                        SlotNumber = boardIndex,
                                        SupportedFunctions = new List<string>
                                        {
                                            "AnalogInput",
                                            "AnalogOutput", 
                                            "DigitalInput",
                                            "DigitalOutput",
                                            "CounterInput",
                                            "CounterOutput"
                                        },
                                        Configuration = new Dictionary<string, object>
                                        {
                                            ["MaxChannels"] = deviceModel.Channels,
                                            ["MaxSampleRate"] = deviceModel.MaxSampleRate,
                                            ["AOChannels"] = deviceModel.AOChannels,
                                            ["BoardIndex"] = boardIndex,
                                            ["VoltageRanges"] = new[] { "±10V", "±5V", "±2V", "±1V", "±0.5V", "±0.2V", "±0.1V" },
                                            ["Coupling"] = new[] { "DC", "AC", "GND" },
                                            ["Terminal"] = new[] { "RSE", "NRSE", "DIFF" }
                                        }
                                    };

                                    devices.Add(device);
                                    _logger.LogInformation("发现JY5500设备: {Model} 板卡{BoardIndex}", deviceModel.Model, boardIndex);

                                    // 清理测试任务
                                    var disposeMethod = aiTaskType.GetMethod("Dispose");
                                    disposeMethod?.Invoke(aiTask, null);
                                    
                                    break; // 找到设备后跳出型号循环
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // 设备不存在或初始化失败，继续尝试下一个
                            _logger.LogDebug("板卡{BoardIndex}上未发现{Model}设备: {Error}", boardIndex, deviceModel.Model, ex.Message);
                        }
                    }
                }

                _logger.LogInformation("JY5500设备发现完成，共发现{Count}个设备", devices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JY5500设备发现过程中发生错误");
            }

            return devices;
        }

        public async Task<object> CreateTaskAsync(string taskType, int deviceId, Dictionary<string, object> parameters)
        {
            try
            {
                if (_driverAssembly == null)
                {
                    _logger.LogError("JY5500驱动未初始化");
                    return null;
                }

                var boardIndex = parameters.ContainsKey("BoardIndex") ? Convert.ToInt32(parameters["BoardIndex"]) : 0;
                var taskId = parameters.ContainsKey("TaskId") ? Convert.ToInt32(parameters["TaskId"]) : 0;

                object? task = null;

                switch (taskType.ToUpper())
                {
                    case "AI":
                    case "ANALOGINPUT":
                        task = await CreateAITaskAsync(boardIndex, parameters);
                        break;
                    case "AO":
                    case "ANALOGOUTPUT":
                        task = await CreateAOTaskAsync(boardIndex, parameters);
                        break;
                    case "DI":
                    case "DIGITALINPUT":
                        task = await CreateDITaskAsync(boardIndex, parameters);
                        break;
                    case "DO":
                    case "DIGITALOUTPUT":
                        task = await CreateDOTaskAsync(boardIndex, parameters);
                        break;
                    default:
                        _logger.LogWarning("不支持的任务类型: {TaskType}", taskType);
                        return null;
                }

                if (task != null && taskId > 0)
                {
                    _activeTasks[taskId] = task;
                    _logger.LogInformation("成功创建JY5500任务: {TaskType}, TaskId: {TaskId}", taskType, taskId);
                }

                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建JY5500任务失败: {TaskType}", taskType);
                return null;
            }
        }

        private async Task<object?> CreateAITaskAsync(int boardIndex, Dictionary<string, object> parameters)
        {
            try
            {
                var aiTaskType = _driverAssembly?.GetType("JY5500.JY5500AITask");
                if (aiTaskType == null) return null;

                // 创建AI任务实例
                var aiTask = Activator.CreateInstance(aiTaskType, boardIndex);
                if (aiTask == null) return null;

                // 配置通道
                var channelCount = parameters.ContainsKey("ChannelCount") ? Convert.ToInt32(parameters["ChannelCount"]) : 1;
                var minRange = parameters.ContainsKey("MinRange") ? Convert.ToDouble(parameters["MinRange"]) : -10.0;
                var maxRange = parameters.ContainsKey("MaxRange") ? Convert.ToDouble(parameters["MaxRange"]) : 10.0;

                // 获取AITerminal枚举
                var aiTerminalType = _driverAssembly?.GetType("JY5500.AITerminal");
                var rseTerminal = aiTerminalType?.GetField("RSE")?.GetValue(null);

                // 添加通道
                var addChannelMethod = aiTaskType.GetMethod("AddChannel", new[] { typeof(int), typeof(double), typeof(double), aiTerminalType });
                for (int i = 0; i < channelCount; i++)
                {
                    addChannelMethod?.Invoke(aiTask, new object[] { i, minRange, maxRange, rseTerminal });
                }

                // 设置采样模式
                var aiModeType = _driverAssembly?.GetType("JY5500.AIMode");
                var continuousMode = aiModeType?.GetField("Continuous")?.GetValue(null);
                var modeProperty = aiTaskType.GetProperty("Mode");
                modeProperty?.SetValue(aiTask, continuousMode);

                // 设置采样率
                var sampleRate = parameters.ContainsKey("SampleRate") ? Convert.ToDouble(parameters["SampleRate"]) : 1000.0;
                var sampleRateProperty = aiTaskType.GetProperty("SampleRate");
                sampleRateProperty?.SetValue(aiTask, sampleRate);

                _logger.LogInformation("成功创建JY5500 AI任务: 板卡{BoardIndex}, 通道数{ChannelCount}, 采样率{SampleRate}", 
                    boardIndex, channelCount, sampleRate);

                return aiTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建JY5500 AI任务失败");
                return null;
            }
        }

        private async Task<object?> CreateAOTaskAsync(int boardIndex, Dictionary<string, object> parameters)
        {
            try
            {
                var aoTaskType = _driverAssembly?.GetType("JY5500.JY5500AOTask");
                if (aoTaskType == null) return null;

                var aoTask = Activator.CreateInstance(aoTaskType, boardIndex);
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

                _logger.LogInformation("成功创建JY5500 AO任务: 板卡{BoardIndex}, 通道数{ChannelCount}", boardIndex, channelCount);
                return aoTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建JY5500 AO任务失败");
                return null;
            }
        }

        private async Task<object?> CreateDITaskAsync(int boardIndex, Dictionary<string, object> parameters)
        {
            try
            {
                var diTaskType = _driverAssembly?.GetType("JY5500.JY5500DITask");
                if (diTaskType == null) return null;

                var diTask = Activator.CreateInstance(diTaskType, boardIndex);
                if (diTask == null) return null;

                // 添加端口
                var portNumber = parameters.ContainsKey("PortNumber") ? Convert.ToInt32(parameters["PortNumber"]) : 0;
                var addChannelMethod = diTaskType.GetMethod("AddChannel", new[] { typeof(int) });
                addChannelMethod?.Invoke(diTask, new object[] { portNumber });

                _logger.LogInformation("成功创建JY5500 DI任务: 板卡{BoardIndex}, 端口{PortNumber}", boardIndex, portNumber);
                return diTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建JY5500 DI任务失败");
                return null;
            }
        }

        private async Task<object?> CreateDOTaskAsync(int boardIndex, Dictionary<string, object> parameters)
        {
            try
            {
                var doTaskType = _driverAssembly?.GetType("JY5500.JY5500DOTask");
                if (doTaskType == null) return null;

                var doTask = Activator.CreateInstance(doTaskType, boardIndex);
                if (doTask == null) return null;

                // 添加端口
                var portNumber = parameters.ContainsKey("PortNumber") ? Convert.ToInt32(parameters["PortNumber"]) : 1;
                var addChannelMethod = doTaskType.GetMethod("AddChannel", new[] { typeof(int) });
                addChannelMethod?.Invoke(doTask, new object[] { portNumber });

                _logger.LogInformation("成功创建JY5500 DO任务: 板卡{BoardIndex}, 端口{PortNumber}", boardIndex, portNumber);
                return doTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建JY5500 DO任务失败");
                return null;
            }
        }

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

                _logger.LogInformation("成功启动JY5500任务: {TaskId}", taskId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动JY5500任务失败: {TaskId}", taskId);
                return false;
            }
        }

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

                _logger.LogInformation("成功停止JY5500任务: {TaskId}", taskId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止JY5500任务失败: {TaskId}", taskId);
                return false;
            }
        }

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
                if (taskType.Name == "JY5500AITask")
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
                _logger.LogError(ex, "读取JY5500数据失败: {TaskId}", taskId);
                return null;
            }
        }

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
                if (taskType.Name == "JY5500AOTask")
                {
                    var writeDataMethod = taskType.GetMethod("WriteData", new[] { typeof(double[]), typeof(int) });
                    writeDataMethod?.Invoke(task, new object[] { data, -1 });
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "写入JY5500数据失败: {TaskId}", taskId);
                return false;
            }
        }

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
                _logger.LogError(ex, "获取JY5500任务状态失败: {TaskId}", taskId);
                return new { Status = "Error", Message = ex.Message };
            }
        }

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
                
                _logger.LogInformation("成功释放JY5500任务实例");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放JY5500任务实例失败");
                return false;
            }
        }

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
                    _logger.LogInformation("成功释放JY5500任务: {TaskId}", taskId);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放JY5500任务失败: {TaskId}", taskId);
                return false;
            }
        }

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

                _logger.LogInformation("JY5500驱动适配器已释放");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放JY5500驱动适配器时发生错误");
            }
        }
    }
}
