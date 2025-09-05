using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using USB1601Service.Hubs;
using USB1601Service.Services;
using System.Reflection;

namespace USB1601Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HybridDataController : ControllerBase
    {
        private readonly ILogger<HybridDataController> _logger;
        private readonly USB1601Manager _hardwareManager;
        private readonly SimulationManager _simulationManager;
        private readonly IHubContext<DataHub> _hubContext;
        
        private static bool _isRunning = false;
        private static bool _useHardware = false;
        private static bool _hardwareInitialized = false;
        private static string _lastError = "";

        public HybridDataController(
            ILogger<HybridDataController> logger,
            USB1601Manager hardwareManager,
            SimulationManager simulationManager,
            IHubContext<DataHub> hubContext)
        {
            _logger = logger;
            _hardwareManager = hardwareManager;
            _simulationManager = simulationManager;
            _hubContext = hubContext;
        }

        /// <summary>
        /// 检测硬件
        /// </summary>
        [HttpGet("detect-hardware")]
        public async Task<IActionResult> DetectHardware()
        {
            try
            {
                _logger.LogInformation("检测USB-1601硬件...");
                
                // 尝试加载驱动DLL
                var dllPaths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JYUSB1601.dll"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\drivers\\JYUSB1601.dll"),
                    Path.Combine(Directory.GetCurrentDirectory(), "drivers\\JYUSB1601.dll"),
                    "D:\\Documents\\seesharptools-web\\usb1601-web-app\\drivers\\JYUSB1601.dll"
                };

                string? validDllPath = null;
                foreach (var path in dllPaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        validDllPath = path;
                        _logger.LogInformation($"找到驱动文件: {path}");
                        break;
                    }
                }

                if (validDllPath == null)
                {
                    return Ok(new
                    {
                        success = false,
                        hardwareAvailable = false,
                        message = "未找到JYUSB1601.dll驱动文件",
                        recommendation = "使用模拟模式"
                    });
                }

                // 尝试加载驱动
                try
                {
                    var assembly = Assembly.LoadFrom(validDllPath);
                    var aiTaskType = assembly.GetType("JYUSB1601.JYUSB1601AITask");
                    
                    if (aiTaskType == null)
                    {
                        return Ok(new
                        {
                            success = false,
                            hardwareAvailable = false,
                            message = "驱动DLL格式不正确",
                            recommendation = "使用模拟模式"
                        });
                    }

                    // 尝试创建任务实例（检测设备）
                    bool deviceFound = false;
                    int foundDeviceIndex = -1;
                    
                    for (int i = 0; i < 4; i++)
                    {
                        try
                        {
                            var testTask = Activator.CreateInstance(aiTaskType, i.ToString());
                            if (testTask != null)
                            {
                                // 设备存在
                                deviceFound = true;
                                foundDeviceIndex = i;
                                
                                // 释放测试任务
                                var disposeMethod = aiTaskType.GetMethod("Dispose");
                                disposeMethod?.Invoke(testTask, null);
                                
                                _logger.LogInformation($"检测到USB-1601设备，索引: {i}");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug($"设备索引{i}不可用: {ex.Message}");
                        }
                    }

                    if (deviceFound)
                    {
                        return Ok(new
                        {
                            success = true,
                            hardwareAvailable = true,
                            deviceIndex = foundDeviceIndex,
                            message = $"检测到USB-1601设备（索引: {foundDeviceIndex}）",
                            driverPath = validDllPath
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            success = false,
                            hardwareAvailable = false,
                            message = "未检测到USB-1601设备",
                            recommendation = "请检查设备连接或使用模拟模式",
                            driverLoaded = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "加载驱动失败");
                    return Ok(new
                    {
                        success = false,
                        hardwareAvailable = false,
                        message = $"驱动加载失败: {ex.Message}",
                        recommendation = "使用模拟模式"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "硬件检测失败");
                return Ok(new
                {
                    success = false,
                    hardwareAvailable = false,
                    message = ex.Message,
                    recommendation = "使用模拟模式"
                });
            }
        }

        /// <summary>
        /// 初始化系统（自动选择模式）
        /// </summary>
        [HttpPost("initialize")]
        public async Task<IActionResult> Initialize([FromBody] InitRequest? request = null)
        {
            try
            {
                bool preferHardware = request?.PreferHardware ?? false;
                
                if (preferHardware)
                {
                    // 尝试硬件初始化
                    try
                    {
                        _logger.LogInformation("尝试初始化硬件模式...");
                        var hardwareResult = await _hardwareManager.InitializeAsync();
                        
                        if (hardwareResult)
                        {
                            _hardwareInitialized = true;
                            _useHardware = true;
                            
                            // 订阅硬件数据事件
                            _hardwareManager.DataReceived -= OnHardwareDataReceived;
                            _hardwareManager.DataReceived += OnHardwareDataReceived;
                            
                            return Ok(new
                            {
                                success = true,
                                message = "硬件模式初始化成功",
                                mode = "hardware"
                            });
                        }
                    }
                    catch (Exception hwEx)
                    {
                        _logger.LogWarning($"硬件初始化失败，切换到模拟模式: {hwEx.Message}");
                        _lastError = hwEx.Message;
                    }
                }

                // 使用模拟模式
                _logger.LogInformation("初始化模拟模式...");
                await _simulationManager.InitializeAsync();
                
                // 订阅模拟数据事件
                _simulationManager.DataReceived -= OnSimulationDataReceived;
                _simulationManager.DataReceived += OnSimulationDataReceived;
                
                _useHardware = false;
                
                return Ok(new
                {
                    success = true,
                    message = preferHardware ? "硬件不可用，已切换到模拟模式" : "模拟模式初始化成功",
                    mode = "simulation",
                    hardwareError = _lastError
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// 开始采集
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] HybridStartRequest request)
        {
            try
            {
                if (_isRunning)
                {
                    return BadRequest(new { success = false, message = "采集已在运行中" });
                }

                _logger.LogInformation($"开始采集 - 模式: {(_useHardware ? "硬件" : "模拟")}");

                if (_useHardware && _hardwareInitialized)
                {
                    // 使用硬件
                    var config = new AcquisitionConfig
                    {
                        ChannelCount = request.ChannelCount,
                        SampleRate = request.SampleRate,
                        MinVoltage = request.MinVoltage,
                        MaxVoltage = request.MaxVoltage
                    };

                    await _hardwareManager.ConfigureAcquisitionAsync(config);
                    
                    if (request.SelfTestMode)
                    {
                        await _hardwareManager.GenerateTestSignalAsync(
                            Enum.Parse<SignalType>(request.SignalType),
                            request.SignalFrequency,
                            request.SignalAmplitude);
                    }

                    await _hardwareManager.StartAcquisitionAsync();
                }
                else
                {
                    // 使用模拟
                    await _simulationManager.ConfigureAsync(
                        request.SampleRate,
                        request.ChannelCount,
                        Enum.Parse<SignalType>(request.SignalType),
                        request.SignalFrequency,
                        request.SignalAmplitude);

                    await _simulationManager.StartAsync();
                }

                _isRunning = true;

                // 通知客户端
                await _hubContext.Clients.All.SendAsync("AcquisitionStarted", new
                {
                    success = true,
                    mode = _useHardware ? "hardware" : "simulation",
                    config = new
                    {
                        channelCount = request.ChannelCount,
                        sampleRate = request.SampleRate,
                        signalType = request.SignalType
                    },
                    timestamp = DateTime.Now
                });

                return Ok(new
                {
                    success = true,
                    message = $"采集已启动（{(_useHardware ? "硬件" : "模拟")}模式）",
                    mode = _useHardware ? "hardware" : "simulation"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动采集失败");
                
                // 如果硬件失败，尝试切换到模拟模式
                if (_useHardware)
                {
                    _logger.LogWarning("硬件采集失败，尝试切换到模拟模式");
                    _useHardware = false;
                    return await Start(request); // 递归调用，使用模拟模式
                }
                
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        [HttpPost("stop")]
        public async Task<IActionResult> Stop()
        {
            try
            {
                if (!_isRunning)
                {
                    return BadRequest(new { success = false, message = "采集未运行" });
                }

                if (_useHardware && _hardwareInitialized)
                {
                    await _hardwareManager.StopAcquisitionAsync();
                }
                else
                {
                    await _simulationManager.StopAsync();
                }

                _isRunning = false;

                await _hubContext.Clients.All.SendAsync("AcquisitionStopped", new
                {
                    success = true,
                    timestamp = DateTime.Now
                });

                return Ok(new { success = true, message = "采集已停止" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止采集失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 切换模式
        /// </summary>
        [HttpPost("switch-mode")]
        public async Task<IActionResult> SwitchMode([FromBody] SwitchModeRequest request)
        {
            try
            {
                if (_isRunning)
                {
                    return BadRequest(new { success = false, message = "请先停止采集" });
                }

                if (request.UseHardware)
                {
                    // 切换到硬件模式
                    if (!_hardwareInitialized)
                    {
                        var result = await _hardwareManager.InitializeAsync();
                        if (!result)
                        {
                            return BadRequest(new
                            {
                                success = false,
                                message = "硬件初始化失败，无法切换",
                                error = _lastError
                            });
                        }
                        _hardwareInitialized = true;
                    }
                    
                    _useHardware = true;
                    _hardwareManager.DataReceived -= OnHardwareDataReceived;
                    _hardwareManager.DataReceived += OnHardwareDataReceived;
                }
                else
                {
                    // 切换到模拟模式
                    _useHardware = false;
                    await _simulationManager.InitializeAsync();
                    _simulationManager.DataReceived -= OnSimulationDataReceived;
                    _simulationManager.DataReceived += OnSimulationDataReceived;
                }

                return Ok(new
                {
                    success = true,
                    message = $"已切换到{(request.UseHardware ? "硬件" : "模拟")}模式",
                    mode = request.UseHardware ? "hardware" : "simulation"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切换模式失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                success = true,
                isInitialized = _useHardware ? _hardwareInitialized : true,
                isAcquiring = _isRunning,
                mode = _useHardware ? "hardware" : "simulation",
                hardwareAvailable = _hardwareInitialized,
                lastError = _lastError,
                timestamp = DateTime.Now
            });
        }

        private async void OnHardwareDataReceived(object? sender, DataReceivedEventArgs e)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("DataReceived", new
                {
                    data = e.Data,
                    timestamp = e.Timestamp,
                    sampleCount = e.Data.Length,
                    mode = "hardware"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "推送硬件数据失败");
            }
        }

        private async void OnSimulationDataReceived(object? sender, DataReceivedEventArgs e)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("DataReceived", new
                {
                    data = e.Data,
                    timestamp = e.Timestamp,
                    sampleCount = e.Data.Length,
                    mode = "simulation"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "推送模拟数据失败");
            }
        }
    }

    public class InitRequest
    {
        public bool PreferHardware { get; set; } = false;
    }

    public class HybridStartRequest
    {
        public int ChannelCount { get; set; } = 1;
        public double SampleRate { get; set; } = 1000;
        public double MinVoltage { get; set; } = -10;
        public double MaxVoltage { get; set; } = 10;
        public bool SelfTestMode { get; set; } = false;
        public string SignalType { get; set; } = "Sine";
        public double SignalFrequency { get; set; } = 100;
        public double SignalAmplitude { get; set; } = 5;
    }

    public class SwitchModeRequest
    {
        public bool UseHardware { get; set; }
    }
}