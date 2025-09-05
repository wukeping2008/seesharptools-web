using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using USB1601Service.Hubs;
using USB1601Service.Services;

namespace USB1601Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimpleDataController : ControllerBase
    {
        private readonly ILogger<SimpleDataController> _logger;
        private readonly SimulationManager _simulationManager;
        private readonly IHubContext<DataHub> _hubContext;
        private static bool _isRunning = false;
        private static bool _useSimulation = true; // 默认使用模拟模式

        public SimpleDataController(
            ILogger<SimpleDataController> logger,
            SimulationManager simulationManager,
            IHubContext<DataHub> hubContext)
        {
            _logger = logger;
            _simulationManager = simulationManager;
            _hubContext = hubContext;
            
            // 订阅模拟数据事件
            _simulationManager.DataReceived += OnSimulationDataReceived;
        }

        /// <summary>
        /// 初始化（模拟模式）
        /// </summary>
        [HttpPost("initialize")]
        public async Task<IActionResult> Initialize()
        {
            try
            {
                _logger.LogInformation("初始化数据采集系统（模拟模式）");
                await _simulationManager.InitializeAsync();
                
                return Ok(new 
                { 
                    success = true, 
                    message = "系统初始化成功（模拟模式）",
                    mode = "simulation"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化失败");
                return Ok(new 
                { 
                    success = true, 
                    message = "切换到模拟模式",
                    mode = "simulation",
                    warning = ex.Message
                });
            }
        }

        /// <summary>
        /// 开始采集
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] SimpleStartRequest request)
        {
            try
            {
                if (_isRunning)
                {
                    return BadRequest(new { success = false, message = "采集已在运行中" });
                }

                _logger.LogInformation($"开始数据采集: {request.ChannelCount}通道, {request.SampleRate}Hz");
                
                // 配置模拟器
                await _simulationManager.ConfigureAsync(
                    request.SampleRate,
                    request.ChannelCount,
                    Enum.Parse<SignalType>(request.SignalType ?? "Sine"),
                    request.SignalFrequency,
                    request.SignalAmplitude);
                
                // 启动采集
                await _simulationManager.StartAsync();
                _isRunning = true;
                
                // 通知所有客户端
                await _hubContext.Clients.All.SendAsync("AcquisitionStarted", new
                {
                    success = true,
                    mode = "simulation",
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
                    message = "数据采集已启动（模拟模式）",
                    mode = "simulation"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动采集失败");
                return StatusCode(500, new { success = false, message = $"启动失败: {ex.Message}" });
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

                await _simulationManager.StopAsync();
                _isRunning = false;
                
                // 通知所有客户端
                await _hubContext.Clients.All.SendAsync("AcquisitionStopped", new
                {
                    success = true,
                    timestamp = DateTime.Now
                });
                
                return Ok(new { success = true, message = "数据采集已停止" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止采集失败");
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
                isInitialized = true,
                isAcquiring = _isRunning,
                mode = _useSimulation ? "simulation" : "hardware",
                timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// 处理模拟数据
        /// </summary>
        private async void OnSimulationDataReceived(object? sender, DataReceivedEventArgs e)
        {
            try
            {
                // 通过SignalR推送数据
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

    // 简化的请求模型
    public class SimpleStartRequest
    {
        public int ChannelCount { get; set; } = 1;
        public double SampleRate { get; set; } = 1000;
        public double MinVoltage { get; set; } = -10;
        public double MaxVoltage { get; set; } = 10;
        public string SignalType { get; set; } = "Sine";
        public double SignalFrequency { get; set; } = 100;
        public double SignalAmplitude { get; set; } = 5;
    }
}