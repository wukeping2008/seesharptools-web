using Microsoft.AspNetCore.Mvc;
using USB1601Service.Services;

namespace USB1601Service.Controllers
{
    /// <summary>
    /// AI模型测试控制器 - 用于测试ERNIE-4.5-turbo-vl集成
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AITestController : ControllerBase
    {
        private readonly ILogger<AITestController> _logger;
        private readonly BaiduAIServiceV2 _aiService;

        public AITestController(
            ILogger<AITestController> logger,
            BaiduAIServiceV2 aiService)
        {
            _logger = logger;
            _aiService = aiService;
        }

        /// <summary>
        /// 测试ERNIE-4.5-turbo-vl模型连接
        /// </summary>
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                _logger.LogInformation("测试ERNIE-4.5-turbo-vl模型连接...");
                
                // 生成测试数据
                var testData = GenerateTestWaveform();
                
                // 测试快速分析（使用免费模型）
                var quickResult = await _aiService.AnalyzeWaveformQuick(testData, 1000);
                
                return Ok(new
                {
                    success = true,
                    message = "AI服务连接成功",
                    quickAnalysis = new
                    {
                        success = quickResult.Success,
                        signalType = quickResult.SignalType,
                        quality = quickResult.Quality,
                        processingLevel = quickResult.ProcessingLevel
                    },
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI连接测试失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "AI服务连接失败",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 测试ERNIE-4.5高级分析
        /// </summary>
        [HttpPost("test-advanced-analysis")]
        public async Task<IActionResult> TestAdvancedAnalysis([FromBody] AdvancedAnalysisRequest? request = null)
        {
            try
            {
                _logger.LogInformation("执行ERNIE-4.5-turbo-vl高级分析...");
                
                // 生成测试数据
                var testData = GenerateTestWaveform(
                    request?.SignalType ?? "sine",
                    request?.Frequency ?? 100,
                    request?.Amplitude ?? 5
                );
                
                // 创建测试会话
                var testSession = new TestSession
                {
                    SessionId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.Now.AddMinutes(-5),
                    EndTime = DateTime.Now,
                    SampleRate = 1000,
                    ChannelCount = 1,
                    TotalSamples = testData.Length,
                    MaxValue = testData.Max(),
                    MinValue = testData.Min(),
                    MeanValue = testData.Average(),
                    StdDev = Math.Sqrt(testData.Select(x => Math.Pow(x - testData.Average(), 2)).Average()),
                    DataComplexity = request?.UseAdvancedModel == true ? 0.8 : 0.5
                };
                
                // 生成专业报告（将自动选择ERNIE-4.5或降级模型）
                var report = await _aiService.GenerateProfessionalReport(testSession);
                
                // 如果请求中包含视觉分析
                object? visualAnalysis = null;
                if (request?.EnableVisualAnalysis == true)
                {
                    var visualResult = await _aiService.AnalyzeWaveformVisual(testData, 1000);
                    visualAnalysis = new
                    {
                        success = visualResult.Success,
                        analysis = visualResult.Analysis,
                        modelUsed = visualResult.ModelUsed
                    };
                }
                
                return Ok(new
                {
                    success = true,
                    message = "高级分析完成",
                    report = new
                    {
                        success = report.Success,
                        title = report.Title,
                        content = report.Content,
                        modelUsed = report.ModelUsed,
                        processingLevel = report.ProcessingLevel,
                        estimatedCost = report.EstimatedCost
                    },
                    visualAnalysis,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "高级分析失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "高级分析失败",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 测试模型降级机制
        /// </summary>
        [HttpGet("test-fallback")]
        public async Task<IActionResult> TestFallback()
        {
            try
            {
                _logger.LogInformation("测试AI模型降级机制...");
                
                var results = new List<object>();
                var testData = GenerateTestWaveform();
                
                // L1: 数据验证（Tiny）
                var validationResult = await _aiService.ValidateDataPoint(5.5, -10, 10);
                results.Add(new
                {
                    level = "L1-数据验证",
                    model = "ERNIE-Tiny",
                    result = validationResult
                });
                
                // L2: 快速分析（Lite）
                var quickResult = await _aiService.AnalyzeWaveformQuick(testData, 1000);
                results.Add(new
                {
                    level = "L2-快速分析",
                    model = "ERNIE-Lite",
                    result = new
                    {
                        signalType = quickResult.SignalType,
                        quality = quickResult.Quality
                    }
                });
                
                // L3: 趋势分析（Speed）
                var segment = new DataSegment
                {
                    DataPoints = testData.Take(100).ToList(),
                    Duration = 0.1,
                    HistoricalPatterns = new List<string> { "稳定", "周期性" }
                };
                var trendResult = await _aiService.AnalyzeTrend(segment);
                results.Add(new
                {
                    level = "L3-趋势分析",
                    model = "ERNIE-Speed",
                    result = new
                    {
                        trend = trendResult.Trend,
                        confidence = trendResult.Confidence
                    }
                });
                
                return Ok(new
                {
                    success = true,
                    message = "模型降级测试完成",
                    modelHierarchy = results,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "降级测试失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "降级测试失败",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 测试实时数据流分析
        /// </summary>
        [HttpPost("test-stream-analysis")]
        public async Task<IActionResult> TestStreamAnalysis()
        {
            try
            {
                _logger.LogInformation("测试实时数据流AI分析...");
                
                var analysisResults = new List<object>();
                
                // 模拟5秒数据流
                for (int i = 0; i < 5; i++)
                {
                    var dataPoint = Math.Sin(2 * Math.PI * i / 5) * 5 + Random.Shared.NextDouble() * 0.5;
                    
                    // 实时验证
                    var validation = await _aiService.ValidateDataPoint(dataPoint, -10, 10);
                    
                    analysisResults.Add(new
                    {
                        timestamp = DateTime.Now.AddSeconds(i),
                        value = dataPoint,
                        status = validation.Status,
                        confidence = validation.Confidence
                    });
                    
                    // 模拟实时延迟
                    await Task.Delay(100);
                }
                
                return Ok(new
                {
                    success = true,
                    message = "实时流分析测试完成",
                    dataPoints = analysisResults.Count,
                    results = analysisResults,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "实时流分析测试失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "实时流分析测试失败",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 生成测试波形数据
        /// </summary>
        private double[] GenerateTestWaveform(string type = "sine", double frequency = 100, double amplitude = 5)
        {
            int sampleRate = 1000;
            int duration = 1; // 1秒
            int samples = sampleRate * duration;
            var data = new double[samples];
            
            for (int i = 0; i < samples; i++)
            {
                double t = (double)i / sampleRate;
                
                switch (type.ToLower())
                {
                    case "sine":
                        data[i] = amplitude * Math.Sin(2 * Math.PI * frequency * t);
                        break;
                        
                    case "square":
                        data[i] = amplitude * Math.Sign(Math.Sin(2 * Math.PI * frequency * t));
                        break;
                        
                    case "triangle":
                        double phase = (frequency * t) % 1.0;
                        data[i] = amplitude * (phase < 0.5 ? 4 * phase - 1 : 3 - 4 * phase);
                        break;
                        
                    case "noise":
                        data[i] = amplitude * (Random.Shared.NextDouble() * 2 - 1);
                        break;
                        
                    default:
                        data[i] = amplitude * Math.Sin(2 * Math.PI * frequency * t);
                        break;
                }
                
                // 添加噪声
                data[i] += Random.Shared.NextDouble() * 0.1 - 0.05;
            }
            
            return data;
        }
    }

    public class AdvancedAnalysisRequest
    {
        public string SignalType { get; set; } = "sine";
        public double Frequency { get; set; } = 100;
        public double Amplitude { get; set; } = 5;
        public bool UseAdvancedModel { get; set; } = true;
        public bool EnableVisualAnalysis { get; set; } = false;
    }
}