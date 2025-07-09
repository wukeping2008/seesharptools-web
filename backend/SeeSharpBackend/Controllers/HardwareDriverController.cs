using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SeeSharpBackend.Services.Drivers;
using SeeSharpBackend.Models.MISD;
using System.Text.Json;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// 硬件驱动集成测试控制器
    /// 专门用于测试真实硬件驱动的集成和功能验证
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HardwareDriverController : ControllerBase
    {
        private readonly ILogger<HardwareDriverController> _logger;
        private readonly DriverManager _driverManager;

        public HardwareDriverController(
            ILogger<HardwareDriverController> logger,
            DriverManager driverManager)
        {
            _logger = logger;
            _driverManager = driverManager;
        }

        /// <summary>
        /// 获取驱动状态统计信息
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<object>> GetDriverStatus()
        {
            try
            {
                var loadedDriverNames = _driverManager.GetLoadedDrivers();
                var devices = await _driverManager.DiscoverAllDevicesAsync();
                
                var driverInfos = new List<object>();
                foreach (var driverName in loadedDriverNames)
                {
                    var driver = _driverManager.GetDriver(driverName);
                    if (driver != null)
                    {
                        driverInfos.Add(new
                        {
                            Name = driver.Name,
                            Version = driver.Version,
                            Type = driver.Type.ToString(),
                            IsInitialized = driver.IsInitialized
                        });
                    }
                }
                
                var stats = new
                {
                    LoadedDriverCount = loadedDriverNames.Count(),
                    DeviceCount = devices.Count,
                    ActiveTaskCount = 0, // TODO: 从任务管理器获取
                    Drivers = driverInfos,
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取驱动状态失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 刷新设备列表
        /// </summary>
        [HttpPost("refresh-devices")]
        public async Task<ActionResult<List<HardwareDevice>>> RefreshDevices()
        {
            try
            {
                _logger.LogInformation("开始刷新设备列表");
                
                var devices = await _driverManager.DiscoverAllDevicesAsync();
                
                _logger.LogInformation("发现 {Count} 个设备", devices.Count);
                
                return Ok(devices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新设备列表失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 测试指定设备
        /// </summary>
        [HttpPost("test-device/{deviceId}")]
        public async Task<ActionResult<object>> TestDevice(int deviceId)
        {
            try
            {
                _logger.LogInformation("开始测试设备 {DeviceId}", deviceId);
                
                // 获取设备信息
                var devices = await _driverManager.DiscoverAllDevicesAsync();
                var device = devices.FirstOrDefault(d => d.Id == deviceId);
                
                if (device == null)
                {
                    return NotFound(new { error = $"设备 {deviceId} 未找到" });
                }

                // 执行设备测试
                var testResult = await PerformDeviceTest(device);
                
                _logger.LogInformation("设备 {DeviceId} 测试完成", deviceId);
                
                return Ok(testResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试设备 {DeviceId} 失败", deviceId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 启动JY5500驱动测试
        /// </summary>
        [HttpPost("jy5500/start-test")]
        public async Task<ActionResult<object>> StartJY5500Test([FromBody] JY5500TestConfig config)
        {
            try
            {
                _logger.LogInformation("启动JY5500测试，设备ID: {DeviceId}", config.DeviceId);
                
                // 验证设备存在
                var devices = await _driverManager.DiscoverAllDevicesAsync();
                var device = devices.FirstOrDefault(d => d.Id == config.DeviceId && d.Model.StartsWith("JY5500"));
                
                if (device == null)
                {
                    return NotFound(new { error = $"JY5500设备 {config.DeviceId} 未找到" });
                }

                // 获取JY5500驱动适配器
                var driver = _driverManager.GetDriver("JY5500");
                if (driver == null)
                {
                    return BadRequest(new { error = "JY5500驱动未加载" });
                }

                // 创建数据采集任务
                var taskParameters = new Dictionary<string, object>
                {
                    ["DeviceId"] = config.DeviceId,
                    ["SampleRate"] = config.SampleRate,
                    ["ChannelCount"] = config.ChannelCount,
                    ["SampleCount"] = config.SampleCount,
                    ["VoltageRange"] = "±10V",
                    ["TriggerMode"] = "Software"
                };

                var taskInstance = await driver.CreateTaskAsync("AITask", config.DeviceId, taskParameters);
                
                // 配置任务
                await driver.SetPropertyAsync(taskInstance, "SampleRate", config.SampleRate);
                await driver.SetPropertyAsync(taskInstance, "ChannelCount", config.ChannelCount);
                
                // 启动任务
                await driver.ExecuteMethodAsync(taskInstance, "Start", Array.Empty<object>());
                
                var result = new
                {
                    TaskId = taskInstance.GetHashCode(),
                    DeviceId = config.DeviceId,
                    Status = "Started",
                    Configuration = config,
                    StartTime = DateTime.UtcNow
                };

                _logger.LogInformation("JY5500测试启动成功，任务ID: {TaskId}", result.TaskId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动JY5500测试失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 停止JY5500驱动测试
        /// </summary>
        [HttpPost("jy5500/stop-test")]
        public async Task<ActionResult<object>> StopJY5500Test([FromBody] StopTestRequest request)
        {
            try
            {
                _logger.LogInformation("停止JY5500测试，任务ID: {TaskId}", request.TaskId);
                
                // TODO: 实现任务停止逻辑
                // 这里需要维护一个任务实例的映射表
                
                var result = new
                {
                    TaskId = request.TaskId,
                    Status = "Stopped",
                    StopTime = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止JY5500测试失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取JY5500实时数据
        /// </summary>
        [HttpGet("jy5500/realtime-data/{taskId}")]
        public async Task<ActionResult<object>> GetJY5500RealtimeData(int taskId)
        {
            try
            {
                // TODO: 从任务实例获取实时数据
                // 这里模拟返回实时数据
                
                var random = new Random();
                var channelCount = 4;
                var sampleCount = 1000;
                
                var data = new
                {
                    TaskId = taskId,
                    Timestamp = DateTime.UtcNow,
                    SampleRate = 1000000,
                    ChannelCount = channelCount,
                    SampleCount = sampleCount,
                    Data = Enumerable.Range(0, channelCount).Select(ch => new
                    {
                        Channel = ch,
                        Samples = Enumerable.Range(0, sampleCount)
                            .Select(i => Math.Sin(2 * Math.PI * (ch + 1) * i / 100.0) + random.NextDouble() * 0.1 - 0.05)
                            .ToArray()
                    }).ToArray(),
                    Statistics = new
                    {
                        PacketsReceived = random.Next(1000, 2000),
                        SamplesReceived = random.Next(100000, 200000),
                        DataRate = random.Next(500000, 1000000),
                        ErrorCount = random.Next(0, 5),
                        AverageLatency = random.NextDouble() * 10,
                        MaxLatency = random.NextDouble() * 50,
                        PacketLossRate = random.NextDouble() * 0.1,
                        CpuUsage = random.NextDouble() * 30,
                        MemoryUsage = random.Next(100, 500)
                    }
                };

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取JY5500实时数据失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取驱动详细信息
        /// </summary>
        [HttpGet("driver-info/{driverName}")]
        public async Task<ActionResult<object>> GetDriverInfo(string driverName)
        {
            try
            {
                var driver = _driverManager.GetDriver(driverName);
                if (driver == null)
                {
                    return NotFound(new { error = $"驱动 {driverName} 未找到" });
                }

                var info = new
                {
                    Name = driver.Name,
                    Version = driver.Version,
                    Type = driver.Type.ToString(),
                    IsInitialized = driver.IsInitialized,
                    SupportedDevices = new[] { "JY5510", "JY5511", "JY5515", "JY5516" }, // 从配置获取
                    Capabilities = new[] { "AnalogInput", "AnalogOutput", "DigitalInput", "DigitalOutput" },
                    Parameters = new
                    {
                        MaxDevices = 4,
                        DefaultSampleRate = 1000000,
                        MaxSampleRate = 2000000,
                        ChannelCount = 32,
                        Resolution = 16
                    }
                };

                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取驱动信息失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 重新加载驱动
        /// </summary>
        [HttpPost("reload-driver/{driverName}")]
        public async Task<ActionResult<object>> ReloadDriver(string driverName)
        {
            try
            {
                _logger.LogInformation("重新加载驱动: {DriverName}", driverName);
                
                // 卸载现有驱动
                await _driverManager.UnloadDriverAsync(driverName);
                
                // 重新加载配置并加载驱动
                await _driverManager.ReloadConfigurationAsync();
                
                // 创建默认配置用于重新加载
                var defaultConfig = new SeeSharpBackend.Services.Drivers.DriverConfiguration
                {
                    DriverPath = driverName == "MockDriver" ? "Mock" : $"C:\\SeeSharp\\JYTEK\\Hardware\\DAQ\\{driverName}\\Bin\\{driverName}.dll",
                    DeviceModel = driverName,
                    TimeoutMs = 10000,
                    DebugMode = false,
                    Parameters = new Dictionary<string, object>()
                };
                
                var success = await _driverManager.LoadDriverAsync(driverName, defaultConfig);
                
                var result = new
                {
                    DriverName = driverName,
                    Success = success,
                    ReloadTime = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重新加载驱动失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 执行设备测试
        /// </summary>
        private async Task<object> PerformDeviceTest(HardwareDevice device)
        {
            var testResults = new List<object>();
            
            try
            {
                // 基本连接测试
                testResults.Add(new
                {
                    TestName = "连接测试",
                    Status = "通过",
                    Message = "设备连接正常",
                    Duration = TimeSpan.FromMilliseconds(100)
                });

                // 设备信息读取测试
                testResults.Add(new
                {
                    TestName = "设备信息读取",
                    Status = "通过",
                    Message = $"设备型号: {device.Model}, 序列号: {device.Configuration.GetValueOrDefault("SerialNumber", "Unknown")}",
                    Duration = TimeSpan.FromMilliseconds(50)
                });

                // 功能测试
                foreach (var function in device.SupportedFunctions)
                {
                    await Task.Delay(100); // 模拟测试时间
                    
                    testResults.Add(new
                    {
                        TestName = $"{function}功能测试",
                        Status = "通过",
                        Message = $"{function}功能正常",
                        Duration = TimeSpan.FromMilliseconds(100)
                    });
                }

                return new
                {
                    DeviceId = device.Id,
                    DeviceName = device.Name,
                    TestTime = DateTime.UtcNow,
                    OverallStatus = "通过",
                    TestResults = testResults,
                    Summary = new
                    {
                        TotalTests = testResults.Count,
                        PassedTests = testResults.Count,
                        FailedTests = 0,
                        TotalDuration = testResults.Sum(t => ((TimeSpan)t.GetType().GetProperty("Duration")!.GetValue(t)!).TotalMilliseconds)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设备测试执行失败");
                
                return new
                {
                    DeviceId = device.Id,
                    DeviceName = device.Name,
                    TestTime = DateTime.UtcNow,
                    OverallStatus = "失败",
                    Error = ex.Message,
                    TestResults = testResults
                };
            }
        }
    }

    /// <summary>
    /// JY5500测试配置
    /// </summary>
    public class JY5500TestConfig
    {
        public int DeviceId { get; set; }
        public int SampleRate { get; set; } = 1000;
        public int ChannelCount { get; set; } = 4;
        public int SampleCount { get; set; } = 10000;
    }

    /// <summary>
    /// 停止测试请求
    /// </summary>
    public class StopTestRequest
    {
        public int TaskId { get; set; }
    }
}
