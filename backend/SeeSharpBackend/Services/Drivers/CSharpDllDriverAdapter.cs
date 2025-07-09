using System.Reflection;
using Microsoft.Extensions.Logging;
using SeeSharpBackend.Models.MISD;

namespace SeeSharpBackend.Services.Drivers
{
    /// <summary>
    /// C# DLL驱动适配器
    /// 支持简仪科技的各种.dll硬件驱动
    /// </summary>
    public class CSharpDllDriverAdapter : IDriverAdapter
    {
        private readonly ILogger<CSharpDllDriverAdapter> _logger;
        private Assembly? _driverAssembly;
        private DriverConfiguration? _config;
        private readonly Dictionary<string, Type> _taskTypes = new();
        private readonly Dictionary<object, object> _taskInstances = new();

        public DriverType Type => DriverType.CSharpDll;
        public string Name { get; private set; } = string.Empty;
        public string Version { get; private set; } = string.Empty;
        public bool IsInitialized { get; private set; }

        public CSharpDllDriverAdapter(ILogger<CSharpDllDriverAdapter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 初始化C# DLL驱动
        /// </summary>
        public async Task<bool> InitializeAsync(DriverConfiguration config)
        {
            try
            {
                _config = config;
                
                // 加载DLL程序集
                if (!File.Exists(config.DriverPath))
                {
                    _logger.LogError("驱动文件不存在: {DriverPath}", config.DriverPath);
                    return false;
                }

                _driverAssembly = Assembly.LoadFrom(config.DriverPath);
                Name = Path.GetFileNameWithoutExtension(config.DriverPath);
                Version = _driverAssembly.GetName().Version?.ToString() ?? "Unknown";

                // 扫描任务类型
                await ScanTaskTypesAsync();

                IsInitialized = true;
                _logger.LogInformation("C# DLL驱动初始化成功: {Name} v{Version}", Name, Version);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化C# DLL驱动失败: {DriverPath}", config.DriverPath);
                return false;
            }
        }

        /// <summary>
        /// 扫描驱动中的任务类型
        /// </summary>
        private async Task ScanTaskTypesAsync()
        {
            if (_driverAssembly == null) return;

            await Task.Run(() =>
            {
                var types = _driverAssembly.GetTypes();
                
                foreach (var type in types)
                {
                    // 查找任务类 (通常以Task结尾)
                    if (type.Name.EndsWith("Task") && type.IsClass && !type.IsAbstract)
                    {
                        _taskTypes[type.Name] = type;
                        _logger.LogDebug("发现任务类型: {TaskType}", type.Name);
                    }
                }
                
                _logger.LogInformation("扫描到 {Count} 个任务类型", _taskTypes.Count);
            });
        }

        /// <summary>
        /// 卸载驱动
        /// </summary>
        public async Task<bool> UnloadAsync()
        {
            try
            {
                // 释放所有任务实例
                foreach (var instance in _taskInstances.Values.ToList())
                {
                    await DisposeTaskAsync(instance);
                }
                
                _taskInstances.Clear();
                _taskTypes.Clear();
                _driverAssembly = null;
                IsInitialized = false;
                
                _logger.LogInformation("C# DLL驱动卸载成功: {Name}", Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "卸载C# DLL驱动失败: {Name}", Name);
                return false;
            }
        }

        /// <summary>
        /// 发现设备
        /// </summary>
        public async Task<List<HardwareDevice>> DiscoverDevicesAsync()
        {
            var devices = new List<HardwareDevice>();
            
            try
            {
                // 根据设备型号创建相应的发现逻辑
                if (_config?.DeviceModel.StartsWith("JY5500") == true)
                {
                    devices.AddRange(await DiscoverJY5500DevicesAsync());
                }
                else if (_config?.DeviceModel.StartsWith("JYUSB") == true)
                {
                    devices.AddRange(await DiscoverJYUSBDevicesAsync());
                }
                else
                {
                    // 通用设备发现
                    devices.AddRange(await DiscoverGenericDevicesAsync());
                }
                
                _logger.LogInformation("发现 {Count} 个设备", devices.Count);
                return devices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设备发现失败");
                return devices;
            }
        }

        /// <summary>
        /// 发现JY5500系列设备
        /// </summary>
        private async Task<List<HardwareDevice>> DiscoverJY5500DevicesAsync()
        {
            var devices = new List<HardwareDevice>();
            
            await Task.Run(() =>
            {
                // 模拟设备发现 - 实际应该调用驱动的设备发现方法
                for (int i = 0; i < 4; i++) // 最多4个插槽
                {
                    try
                    {
                        // 这里应该调用实际的设备检测方法
                        // 例如: var exists = CheckDeviceExists(i);
                        
                        var device = new HardwareDevice
                        {
                            Id = i,
                            Model = _config?.DeviceModel ?? "JY5500",
                            Name = $"JY5500 数据采集卡 #{i}",
                            DeviceType = "DAQ",
                            SlotNumber = i,
                            Status = DeviceStatus.Online,
                            SupportedFunctions = new List<string>
                            {
                                "AnalogInput", "AnalogOutput", 
                                "DigitalInput", "DigitalOutput",
                                "CounterInput", "CounterOutput"
                            },
                            Configuration = new Dictionary<string, object>
                            {
                                ["MaxAIChannels"] = 32,
                                ["MaxAOChannels"] = 4,
                                ["MaxSampleRate"] = 2000000,
                                ["Resolution"] = 16,
                                ["SerialNumber"] = $"SN{DateTime.Now.Ticks % 100000:D5}",
                                ["FirmwareVersion"] = "4.1.5",
                                ["DriverVersion"] = Version
                            }
                        };
                        
                        devices.Add(device);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "检测插槽 {Slot} 设备时出错", i);
                    }
                }
            });
            
            return devices;
        }

        /// <summary>
        /// 发现JYUSB系列设备
        /// </summary>
        private async Task<List<HardwareDevice>> DiscoverJYUSBDevicesAsync()
        {
            var devices = new List<HardwareDevice>();
            
            await Task.Run(() =>
            {
                // USB设备发现逻辑
                _logger.LogDebug("扫描JYUSB设备...");
                // 实际实现应该调用USB设备枚举API
            });
            
            return devices;
        }

        /// <summary>
        /// 通用设备发现
        /// </summary>
        private async Task<List<HardwareDevice>> DiscoverGenericDevicesAsync()
        {
            var devices = new List<HardwareDevice>();
            
            await Task.Run(() =>
            {
                _logger.LogDebug("执行通用设备发现...");
                // 通用设备发现逻辑
            });
            
            return devices;
        }

        /// <summary>
        /// 创建任务实例
        /// </summary>
        public async Task<object> CreateTaskAsync(string taskType, int deviceId, Dictionary<string, object> parameters)
        {
            try
            {
                if (!_taskTypes.TryGetValue(taskType, out var type))
                {
                    throw new ArgumentException($"未找到任务类型: {taskType}");
                }

                // 创建任务实例
                object? instance;
                
                // 尝试不同的构造函数
                var constructors = type.GetConstructors();
                
                // 优先尝试带设备ID的构造函数
                var deviceIdConstructor = constructors.FirstOrDefault(c => 
                    c.GetParameters().Length == 1 && 
                    c.GetParameters()[0].ParameterType == typeof(int));
                
                if (deviceIdConstructor != null)
                {
                    instance = Activator.CreateInstance(type, deviceId);
                }
                else
                {
                    // 尝试无参构造函数
                    instance = Activator.CreateInstance(type);
                }

                if (instance == null)
                {
                    throw new InvalidOperationException($"无法创建任务实例: {taskType}");
                }

                // 应用参数配置
                await ApplyParametersAsync(instance, parameters);

                _taskInstances[instance] = instance;
                _logger.LogDebug("创建任务实例成功: {TaskType}", taskType);
                
                return instance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建任务实例失败: {TaskType}", taskType);
                throw;
            }
        }

        /// <summary>
        /// 应用参数配置
        /// </summary>
        private async Task ApplyParametersAsync(object instance, Dictionary<string, object> parameters)
        {
            await Task.Run(() =>
            {
                var type = instance.GetType();
                
                foreach (var param in parameters)
                {
                    try
                    {
                        var property = type.GetProperty(param.Key);
                        if (property != null && property.CanWrite)
                        {
                            property.SetValue(instance, param.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "设置参数失败: {Parameter}", param.Key);
                    }
                }
            });
        }

        /// <summary>
        /// 执行方法调用
        /// </summary>
        public async Task<object?> ExecuteMethodAsync(object taskInstance, string methodName, object[] parameters)
        {
            try
            {
                var type = taskInstance.GetType();
                var method = type.GetMethod(methodName);
                
                if (method == null)
                {
                    throw new ArgumentException($"未找到方法: {methodName}");
                }

                // 检查是否为异步方法
                if (method.ReturnType == typeof(Task) || 
                    (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
                {
                    var task = (Task)method.Invoke(taskInstance, parameters)!;
                    await task;
                    
                    // 如果有返回值
                    if (method.ReturnType.IsGenericType)
                    {
                        var property = typeof(Task<>).MakeGenericType(method.ReturnType.GetGenericArguments()[0]).GetProperty("Result");
                        return property?.GetValue(task);
                    }
                    
                    return null;
                }
                else
                {
                    // 同步方法
                    return method.Invoke(taskInstance, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行方法失败: {Method}", methodName);
                throw;
            }
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        public async Task<object?> GetPropertyAsync(object taskInstance, string propertyName)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var type = taskInstance.GetType();
                    var property = type.GetProperty(propertyName);
                    
                    if (property == null || !property.CanRead)
                    {
                        throw new ArgumentException($"属性不存在或不可读: {propertyName}");
                    }
                    
                    return property.GetValue(taskInstance);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取属性失败: {Property}", propertyName);
                throw;
            }
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        public async Task<bool> SetPropertyAsync(object taskInstance, string propertyName, object value)
        {
            try
            {
                await Task.Run(() =>
                {
                    var type = taskInstance.GetType();
                    var property = type.GetProperty(propertyName);
                    
                    if (property == null || !property.CanWrite)
                    {
                        throw new ArgumentException($"属性不存在或不可写: {propertyName}");
                    }
                    
                    property.SetValue(taskInstance, value);
                });
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置属性失败: {Property}", propertyName);
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
                // 如果实现了IDisposable接口
                if (taskInstance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                
                // 尝试调用Stop方法
                await Task.Run(() =>
                {
                    var type = taskInstance.GetType();
                    var stopMethod = type.GetMethod("Stop");
                    stopMethod?.Invoke(taskInstance, null);
                });
                
                _taskInstances.Remove(taskInstance);
                _logger.LogDebug("释放任务实例成功");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放任务实例失败");
                return false;
            }
        }
    }
}
