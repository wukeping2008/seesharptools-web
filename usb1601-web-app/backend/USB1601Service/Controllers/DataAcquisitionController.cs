using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using USB1601Service.Hubs;
using USB1601Service.Services;

namespace USB1601Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataAcquisitionController : ControllerBase
    {
        private readonly ILogger<DataAcquisitionController> _logger;
        private readonly USB1601Manager _usb1601Manager;
        private readonly BaiduAIService _aiService;
        private readonly IHubContext<DataHub> _hubContext;
        private static TestSession? _currentSession;

        public DataAcquisitionController(
            ILogger<DataAcquisitionController> logger,
            USB1601Manager usb1601Manager,
            BaiduAIService aiService,
            IHubContext<DataHub> hubContext)
        {
            _logger = logger;
            _usb1601Manager = usb1601Manager;
            _aiService = aiService;
            _hubContext = hubContext;

            // 订阅数据接收事件
            _usb1601Manager.DataReceived += OnDataReceived;
        }

        /// <summary>
        /// 初始化硬件
        /// </summary>
        [HttpPost("initialize")]
        public async Task<IActionResult> Initialize()
        {
            try
            {
                var result = await _usb1601Manager.InitializeAsync();
                if (result)
                {
                    return Ok(new { success = true, message = "硬件初始化成功" });
                }
                return BadRequest(new { success = false, message = "硬件初始化失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 配置采集参数
        /// </summary>
        [HttpPost("configure")]
        public async Task<IActionResult> Configure([FromBody] AcquisitionConfig config)
        {
            try
            {
                var result = await _usb1601Manager.ConfigureAcquisitionAsync(config);
                if (result)
                {
                    return Ok(new { success = true, message = "配置成功" });
                }
                return BadRequest(new { success = false, message = "配置失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 开始采集
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartAcquisition([FromBody] StartRequest request)
        {
            try
            {
                // 配置采集参数
                var config = new AcquisitionConfig
                {
                    ChannelCount = request.ChannelCount,
                    SampleRate = request.SampleRate,
                    MinVoltage = request.MinVoltage,
                    MaxVoltage = request.MaxVoltage
                };

                await _usb1601Manager.ConfigureAcquisitionAsync(config);

                // 开始新的测试会话
                _currentSession = new TestSession
                {
                    StartTime = DateTime.Now,
                    SampleRate = request.SampleRate,
                    ChannelCount = request.ChannelCount,
                    TotalSamples = 0
                };

                // 如果是自发自收模式，生成测试信号
                if (request.SelfTestMode)
                {
                    await _usb1601Manager.GenerateTestSignalAsync(
                        request.SignalType,
                        request.SignalFrequency,
                        request.SignalAmplitude);
                }

                // 开始采集
                var result = await _usb1601Manager.StartAcquisitionAsync();
                if (result)
                {
                    await _hubContext.Clients.All.SendAsync("AcquisitionStarted", new
                    {
                        success = true,
                        config = config,
                        timestamp = DateTime.Now
                    });

                    return Ok(new { success = true, message = "采集已启动" });
                }

                return BadRequest(new { success = false, message = "启动采集失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动采集失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        [HttpPost("stop")]
        public async Task<IActionResult> StopAcquisition()
        {
            try
            {
                var result = await _usb1601Manager.StopAcquisitionAsync();
                
                if (_currentSession != null)
                {
                    _currentSession.EndTime = DateTime.Now;
                }

                await _hubContext.Clients.All.SendAsync("AcquisitionStopped", new
                {
                    success = true,
                    timestamp = DateTime.Now
                });

                if (result)
                {
                    return Ok(new { success = true, message = "采集已停止" });
                }

                return BadRequest(new { success = false, message = "停止采集失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止采集失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// AI分析波形
        /// </summary>
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeWaveform([FromBody] AnalyzeRequest request)
        {
            try
            {
                var result = await _aiService.AnalyzeWaveformAsync(
                    request.Data,
                    request.SampleRate,
                    request.ChannelCount);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "波形分析失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 异常检测
        /// </summary>
        [HttpPost("detect-anomaly")]
        public async Task<IActionResult> DetectAnomaly([FromBody] AnomalyRequest request)
        {
            try
            {
                var result = await _aiService.DetectAnomaliesAsync(
                    request.CurrentData,
                    request.BaselineData);

                if (result.Success)
                {
                    // 如果检测到异常，通过SignalR推送
                    if (result.HasAnomaly)
                    {
                        await _hubContext.Clients.All.SendAsync("AnomalyDetected", result);
                    }

                    return Ok(result);
                }

                return BadRequest(new { success = false, message = "异常检测失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "异常检测失败");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 生成测试报告
        /// </summary>
        [HttpPost("generate-report")]
        public async Task<IActionResult> GenerateReport()
        {
            try
            {
                if (_currentSession == null)
                {
                    return BadRequest(new { success = false, message = "没有可用的测试数据" });
                }

                var report = await _aiService.GenerateReportAsync(_currentSession);
                if (report.Success)
                {
                    return Ok(report);
                }

                return BadRequest(new { success = false, message = "报告生成失败" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成报告失败");
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
                isAcquiring = false,
                currentSession = _currentSession,
                timestamp = DateTime.Now
            });
        }

        private async void OnDataReceived(object? sender, DataReceivedEventArgs e)
        {
            try
            {
                // 更新会话统计
                if (_currentSession != null)
                {
                    _currentSession.TotalSamples += e.Data.Length;
                    if (e.Data.Length > 0)
                    {
                        var max = e.Data.Max();
                        var min = e.Data.Min();
                        if (max > _currentSession.MaxValue) _currentSession.MaxValue = max;
                        if (min < _currentSession.MinValue) _currentSession.MinValue = min;
                    }
                }

                // 通过SignalR推送数据
                await _hubContext.Clients.Group("DataReceivers").SendAsync("DataReceived", new
                {
                    data = e.Data,
                    timestamp = e.Timestamp,
                    sampleCount = e.Data.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "推送数据失败");
            }
        }
    }

    // 请求模型
    public class StartRequest
    {
        public int ChannelCount { get; set; } = 1;
        public double SampleRate { get; set; } = 1000;
        public double MinVoltage { get; set; } = -10;
        public double MaxVoltage { get; set; } = 10;
        public bool SelfTestMode { get; set; } = false;
        public SignalType SignalType { get; set; } = SignalType.Sine;
        public double SignalFrequency { get; set; } = 100;
        public double SignalAmplitude { get; set; } = 5;
    }

    public class AnalyzeRequest
    {
        public double[] Data { get; set; } = Array.Empty<double>();
        public double SampleRate { get; set; }
        public int ChannelCount { get; set; }
    }

    public class AnomalyRequest
    {
        public double[] CurrentData { get; set; } = Array.Empty<double>();
        public double[] BaselineData { get; set; } = Array.Empty<double>();
    }
}