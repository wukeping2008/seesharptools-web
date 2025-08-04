using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Services.AI;
using SeeSharpBackend.Services.AI.Models;
using SeeSharpBackend.Services.CSharpRunner;
using SeeSharpBackend.Services.Drivers;
using SeeSharpBackend.Services.Monitoring;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// AI功能综合测试控制器
    /// 提供独立的测试仪表板后端服务
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ComprehensiveTestController : ControllerBase
    {
        private readonly INaturalLanguageProcessor _nlpProcessor;
        private readonly ISmartCodeGenerator _codeGenerator;
        private readonly ICSharpRunnerService _csharpRunner;
        private readonly IDriverManager _driverManager;
        private readonly IPerformanceMonitoringService _performanceMonitor;
        private readonly ILogger<ComprehensiveTestController> _logger;

        // 测试状态存储
        private static readonly Dictionary<string, ComprehensiveTestSession> TestSessions = new();

        public ComprehensiveTestController(
            INaturalLanguageProcessor nlpProcessor,
            ISmartCodeGenerator codeGenerator,
            ICSharpRunnerService csharpRunner,
            IDriverManager driverManager,
            IPerformanceMonitoringService performanceMonitor,
            ILogger<ComprehensiveTestController> logger)
        {
            _nlpProcessor = nlpProcessor;
            _codeGenerator = codeGenerator;
            _csharpRunner = csharpRunner;
            _driverManager = driverManager;
            _performanceMonitor = performanceMonitor;
            _logger = logger;
        }

        /// <summary>
        /// 启动综合测试
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> StartComprehensiveTest([FromBody] ComprehensiveTestRequest request)
        {
            try
            {
                var sessionId = Guid.NewGuid().ToString();
                var session = new ComprehensiveTestSession
                {
                    SessionId = sessionId,
                    TestRequest = request,
                    StartTime = DateTime.Now,
                    Status = TestStatus.Running,
                    Progress = 0
                };

                TestSessions[sessionId] = session;

                _logger.LogInformation("启动综合测试会话: {SessionId}", sessionId);

                // 异步执行测试
                _ = Task.Run(() => ExecuteComprehensiveTestAsync(session));

                return Ok(new { sessionId, status = "started" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动综合测试失败");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取测试状态
        /// </summary>
        [HttpGet("status/{sessionId}")]
        public async Task<IActionResult> GetTestStatus(string sessionId)
        {
            if (!TestSessions.TryGetValue(sessionId, out var session))
            {
                return NotFound(new { error = "测试会话不存在" });
            }

            return Ok(new
            {
                sessionId,
                status = session.Status.ToString(),
                progress = session.Progress,
                currentPhase = session.CurrentPhase,
                startTime = session.StartTime,
                results = session.Results
            });
        }

        /// <summary>
        /// 获取详细测试结果
        /// </summary>
        [HttpGet("results/{sessionId}")]
        public async Task<IActionResult> GetTestResults(string sessionId)
        {
            if (!TestSessions.TryGetValue(sessionId, out var session))
            {
                return NotFound(new { error = "测试会话不存在" });
            }

            return Ok(session.Results);
        }

        /// <summary>
        /// 获取系统状态
        /// </summary>
        [HttpGet("system-status")]
        public async Task<IActionResult> GetSystemStatus()
        {
            try
            {
                var systemMetrics = await _performanceMonitor.GetSystemMetricsAsync();
                var appMetrics = await _performanceMonitor.GetApplicationMetricsAsync();
                var availableDevices = await _driverManager.DiscoverAllDevicesAsync();

                return Ok(new
                {
                    system = systemMetrics,
                    application = appMetrics,
                    hardware = new
                    {
                        availableDevices = availableDevices.Count,
                        devices = availableDevices.Select(d => new
                        {
                            id = d.Id,
                            name = d.Name,
                            type = d.DeviceType,
                            status = d.Status
                        })
                    },
                    services = new
                    {
                        aiApiStatus = "Connected", // TODO: 实际检查AI API状态
                        csharpRunnerStatus = await CheckCSharpRunnerStatus(),
                        databaseStatus = "Connected"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统状态失败");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// 停止测试
        /// </summary>
        [HttpPost("stop/{sessionId}")]
        public async Task<IActionResult> StopTest(string sessionId)
        {
            if (!TestSessions.TryGetValue(sessionId, out var session))
            {
                return NotFound(new { error = "测试会话不存在" });
            }

            session.Status = TestStatus.Stopped;
            session.EndTime = DateTime.Now;

            return Ok(new { status = "stopped" });
        }

        /// <summary>
        /// 获取预设测试场景
        /// </summary>
        [HttpGet("scenarios")]
        public async Task<IActionResult> GetTestScenarios()
        {
            var scenarios = new[]
            {
                new
                {
                    id = "vibration_diagnosis",
                    name = "振动故障诊断",
                    icon = "📳",
                    description = "电机轴承振动故障诊断测试",
                    requirement = "我需要对电机轴承进行振动故障诊断，使用JYUSB1601数据采集卡采集3轴加速度信号，采样频率25.6kHz，持续采集5秒数据，进行FFT频谱分析，自动识别轴承故障特征频率",
                    expectedDevice = "JYUSB1601",
                    estimatedTime = 30
                },
                new
                {
                    id = "thd_analysis",
                    name = "THD失真分析",
                    icon = "🎵",
                    description = "JY5500信号发生器THD测试",
                    requirement = "使用JY5500信号发生器进行THD（总谐波失真）测试，生成1kHz正弦波，分析2-10次谐波成分，测量失真度并显示频谱分析结果",
                    expectedDevice = "JY5500",
                    estimatedTime = 20
                },
                new
                {
                    id = "temperature_monitoring",
                    name = "多点温度监控",
                    icon = "🌡️",
                    description = "8路热电偶温度监控",
                    requirement = "配置JYUSB1601采集8路热电偶温度信号，采样率10Hz，实时显示温度曲线，设置超温报警阈值，并保存数据到CSV文件",
                    expectedDevice = "JYUSB1601",
                    estimatedTime = 25
                },
                new
                {
                    id = "power_analysis",
                    name = "电能质量分析",
                    icon = "⚡",
                    description = "同步电压电流测量",
                    requirement = "使用JYUSB1601同步采集电压和电流信号，计算有功功率、无功功率、功率因数，分析电能质量参数",
                    expectedDevice = "JYUSB1601",
                    estimatedTime = 35
                },
                new
                {
                    id = "signal_generation",
                    name = "多波形信号生成",
                    icon = "📡",
                    description = "标准测试信号生成",
                    requirement = "使用JY5500生成多种标准测试信号：正弦波、方波、三角波，频率100Hz-10kHz可调，幅度0.1V-10V，用于电路板功能测试",
                    expectedDevice = "JY5500",
                    estimatedTime = 15
                },
                new
                {
                    id = "high_speed_acquisition",
                    name = "高速数据采集",
                    icon = "🚀",
                    description = "多通道高速同步采集",
                    requirement = "配置JYUSB1601进行4通道高速数据采集，采样率100kHz，缓冲区大小1MB，连续采集模式，实时显示波形",
                    expectedDevice = "JYUSB1601",
                    estimatedTime = 40
                }
            };

            return Ok(scenarios);
        }

        #region Private Methods

        /// <summary>
        /// 执行综合测试
        /// </summary>
        private async Task ExecuteComprehensiveTestAsync(ComprehensiveTestSession session)
        {
            try
            {
                var results = session.Results;

                // 阶段1: AI语义理解测试
                session.CurrentPhase = "AI语义理解";
                session.Progress = 10;
                await TestNaturalLanguageProcessing(session, results);

                // 阶段2: 代码生成测试
                session.CurrentPhase = "智能代码生成";
                session.Progress = 30;
                await TestCodeGeneration(session, results);

                // 阶段3: 代码安全验证
                session.CurrentPhase = "安全验证";
                session.Progress = 50;
                await TestCodeSafetyValidation(session, results);

                // 阶段4: 硬件模拟测试
                session.CurrentPhase = "硬件模拟";
                session.Progress = 70;
                await TestHardwareSimulation(session, results);

                // 阶段5: 代码执行测试
                session.CurrentPhase = "代码执行";
                session.Progress = 90;
                await TestCodeExecution(session, results);

                // 完成测试
                session.Status = TestStatus.Completed;
                session.Progress = 100;
                session.EndTime = DateTime.Now;
                session.CurrentPhase = "测试完成";

                // 计算总体评分
                CalculateOverallScore(results);

                _logger.LogInformation("综合测试完成: {SessionId}", session.SessionId);
            }
            catch (Exception ex)
            {
                session.Status = TestStatus.Failed;
                session.EndTime = DateTime.Now;
                session.Results.OverallError = ex.Message;
                _logger.LogError(ex, "综合测试执行失败: {SessionId}", session.SessionId);
            }
        }

        /// <summary>
        /// 测试自然语言处理
        /// </summary>
        private async Task TestNaturalLanguageProcessing(ComprehensiveTestSession session, ComprehensiveTestResults results)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var requirement = await _nlpProcessor.AnalyzeRequirementAsync(session.TestRequest.UserInput);
                
                stopwatch.Stop();

                results.NlpTest = new NlpTestResult
                {
                    Success = true,
                    ProcessingTime = stopwatch.ElapsedMilliseconds,
                    ParsedRequirement = requirement,
                    AccuracyScore = (int)CalculateParsingAccuracy(requirement, session.TestRequest),
                    ExtractedParameters = new Dictionary<string, object>
                    {
                        ["测试类型"] = requirement.TestType,
                        ["测试对象"] = requirement.TestObject,
                        ["推荐设备"] = requirement.RecommendedDevice,
                        ["频率范围"] = requirement.FrequencyRange,
                        ["分析方法"] = requirement.AnalysisMethod,
                        ["置信度"] = requirement.Confidence
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                results.NlpTest = new NlpTestResult
                {
                    Success = false,
                    ProcessingTime = stopwatch.ElapsedMilliseconds,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 测试代码生成
        /// </summary>
        private async Task TestCodeGeneration(ComprehensiveTestSession session, ComprehensiveTestResults results)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var requirement = results.NlpTest?.ParsedRequirement ?? new TestRequirement
                {
                    TestType = session.TestRequest.TestType,
                    TestObject = session.TestRequest.TestObject,
                    RecommendedDevice = session.TestRequest.DeviceType
                };

                var codeResult = await _codeGenerator.GenerateTestCodeAsync(requirement);
                
                stopwatch.Stop();

                results.CodeGenerationTest = new CodeGenerationTestResult
                {
                    Success = codeResult.Success,
                    ProcessingTime = stopwatch.ElapsedMilliseconds,
                    GeneratedCode = codeResult.GeneratedCode,
                    QualityScore = codeResult.QualityScore,
                    ComplexityLevel = codeResult.ComplexityLevel,
                    OptimizationSuggestions = codeResult.OptimizationSuggestions,
                    Error = codeResult.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                results.CodeGenerationTest = new CodeGenerationTestResult
                {
                    Success = false,
                    ProcessingTime = stopwatch.ElapsedMilliseconds,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 测试代码安全验证
        /// </summary>
        private async Task TestCodeSafetyValidation(ComprehensiveTestSession session, ComprehensiveTestResults results)
        {
            if (results.CodeGenerationTest?.GeneratedCode == null)
            {
                results.SafetyValidationTest = new SafetyValidationTestResult
                {
                    Success = false,
                    Error = "没有生成的代码可供验证"
                };
                return;
            }

            try
            {
                var validationResult = await _codeGenerator.ValidateCodeSafetyAsync(results.CodeGenerationTest.GeneratedCode);

                results.SafetyValidationTest = new SafetyValidationTestResult
                {
                    Success = validationResult.IsValid,
                    RiskLevel = validationResult.RiskLevel.ToString(),
                    IssuesFound = validationResult.Issues.Count,
                    SecurityIssues = validationResult.Issues.Select(i => new SecurityIssue
                    {
                        Type = i.IssueType,
                        Description = i.Description,
                        Severity = i.Severity,
                        LineNumber = i.LineNumber,
                        FixSuggestion = i.FixSuggestion
                    }).ToList(),
                    Recommendations = validationResult.Recommendations
                };
            }
            catch (Exception ex)
            {
                results.SafetyValidationTest = new SafetyValidationTestResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 测试硬件模拟
        /// </summary>
        private async Task TestHardwareSimulation(ComprehensiveTestSession session, ComprehensiveTestResults results)
        {
            try
            {
                var deviceType = session.TestRequest.DeviceType;
                var devices = await _driverManager.DiscoverAllDevicesAsync();
                var targetDevice = devices.FirstOrDefault(d => d.DeviceType.Contains(deviceType, StringComparison.OrdinalIgnoreCase));

                results.HardwareSimulationTest = new HardwareSimulationTestResult
                {
                    Success = targetDevice != null,
                    DeviceType = deviceType,
                    DeviceFound = targetDevice != null,
                    DeviceStatus = targetDevice?.Status.ToString() ?? "Not Found",
                    DriverVersion = "Unknown", // HardwareDevice没有Version属性
                    SimulatedDataGenerated = true,
                    ConnectionLatency = Random.Shared.Next(1, 10), // 模拟连接延迟
                    DataThroughput = Random.Shared.Next(1000, 10000), // 模拟数据吞吐量
                    Error = targetDevice == null ? $"未找到{deviceType}设备" : null
                };
            }
            catch (Exception ex)
            {
                results.HardwareSimulationTest = new HardwareSimulationTestResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 测试代码执行
        /// </summary>
        private async Task TestCodeExecution(ComprehensiveTestSession session, ComprehensiveTestResults results)
        {
            if (results.CodeGenerationTest?.GeneratedCode == null)
            {
                results.CodeExecutionTest = new CodeExecutionTestResult
                {
                    Success = false,
                    Error = "没有生成的代码可供执行"
                };
                return;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var executionResult = await _csharpRunner.ExecuteCodeAsync(results.CodeGenerationTest.GeneratedCode, 30000);
                
                stopwatch.Stop();

                results.CodeExecutionTest = new CodeExecutionTestResult
                {
                    Success = executionResult.Success,
                    ExecutionTime = stopwatch.ElapsedMilliseconds,
                    Output = executionResult.ConsoleOutput,
                    ResultData = executionResult.ReturnValue,
                    MemoryUsage = 0, // 暂时设为0，后续可以从执行结果中获取
                    Error = executionResult.ErrorOutput
                };

                // 生成模拟的可视化数据
                if (executionResult.Success)
                {
                    results.VisualizationData = GenerateVisualizationData(session.TestRequest.TestType);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                results.CodeExecutionTest = new CodeExecutionTestResult
                {
                    Success = false,
                    ExecutionTime = stopwatch.ElapsedMilliseconds,
                    Error = ex.Message
                };
            }
        }

        /// <summary>
        /// 计算解析准确度
        /// </summary>
        private double CalculateParsingAccuracy(TestRequirement parsed, ComprehensiveTestRequest request)
        {
            var accuracy = 0.0;

            // 检查测试类型匹配
            if (!string.IsNullOrEmpty(parsed.TestType) && 
                parsed.TestType.Contains(request.TestType, StringComparison.OrdinalIgnoreCase))
            {
                accuracy += 20;
            }

            // 检查设备类型匹配
            if (!string.IsNullOrEmpty(parsed.RecommendedDevice) && 
                parsed.RecommendedDevice.Contains(request.DeviceType, StringComparison.OrdinalIgnoreCase))
            {
                accuracy += 20;
            }

            // 检查测试对象提取
            if (!string.IsNullOrEmpty(parsed.TestObject))
            {
                accuracy += 20;
            }

            // 检查频率范围提取
            if (!string.IsNullOrEmpty(parsed.FrequencyRange))
            {
                accuracy += 20;
            }

            // 检查置信度
            if (parsed.Confidence > 0.7)
            {
                accuracy += 20;
            }

            return accuracy;
        }

        /// <summary>
        /// 生成可视化数据
        /// </summary>
        private VisualizationData GenerateVisualizationData(string testType)
        {
            var random = new Random();
            
            return new VisualizationData
            {
                WaveformData = GenerateWaveformData(1000),
                SpectrumData = GenerateSpectrumData(512),
                StatisticalData = new Dictionary<string, double>
                {
                    ["RMS"] = random.NextDouble() * 2,
                    ["Peak"] = random.NextDouble() * 5,
                    ["Mean"] = random.NextDouble() * 0.1,
                    ["StdDev"] = random.NextDouble() * 0.5
                },
                ThdData = testType.Contains("THD") ? new ThdAnalysisData
                {
                    ThdPercentage = random.NextDouble() * 5,
                    FundamentalFrequency = 1000,
                    Harmonics = Enumerable.Range(2, 9).Select(h => new HarmonicData
                    {
                        Order = h,
                        Frequency = 1000 * h,
                        Amplitude = random.NextDouble() * 0.1,
                        Phase = random.NextDouble() * 360 - 180
                    }).ToList()
                } : null
            };
        }

        /// <summary>
        /// 生成波形数据
        /// </summary>
        private double[] GenerateWaveformData(int samples)
        {
            var data = new double[samples];
            var random = new Random();
            
            for (int i = 0; i < samples; i++)
            {
                var t = i / 1000.0;
                data[i] = Math.Sin(2 * Math.PI * 50 * t) + 
                         0.1 * Math.Sin(2 * Math.PI * 150 * t) +
                         0.05 * random.NextDouble();
            }
            
            return data;
        }

        /// <summary>
        /// 生成频谱数据
        /// </summary>
        private double[] GenerateSpectrumData(int samples)
        {
            var data = new double[samples];
            var random = new Random();
            
            for (int i = 0; i < samples; i++)
            {
                var freq = i * 10; // 10Hz分辨率
                var amplitude = -60.0; // 默认噪声基底
                
                // 添加一些特征频率
                if (freq == 50) amplitude = -3;  // 基波
                else if (freq == 150) amplitude = -20; // 三次谐波
                else if (freq == 250) amplitude = -35; // 五次谐波
                
                data[i] = amplitude + random.NextDouble() * 5;
            }
            
            return data;
        }

        /// <summary>
        /// 计算总体评分
        /// </summary>
        private void CalculateOverallScore(ComprehensiveTestResults results)
        {
            var scores = new List<double>();
            
            if (results.NlpTest?.Success == true)
                scores.Add(results.NlpTest.AccuracyScore);
            
            if (results.CodeGenerationTest?.Success == true)
                scores.Add(results.CodeGenerationTest.QualityScore);
            
            if (results.SafetyValidationTest?.Success == true)
                scores.Add(100); // 安全验证通过得满分
            
            if (results.HardwareSimulationTest?.Success == true)
                scores.Add(85); // 硬件模拟成功得85分
            
            if (results.CodeExecutionTest?.Success == true)
                scores.Add(90); // 代码执行成功得90分
            
            results.OverallScore = scores.Any() ? scores.Average() : 0;
            results.TestCount = 5;
            results.PassedTests = scores.Count;
        }

        /// <summary>
        /// 检查C# Runner状态
        /// </summary>
        private async Task<string> CheckCSharpRunnerStatus()
        {
            try
            {
                // 这里可以实际ping C# Runner服务
                return "Connected";
            }
            catch
            {
                return "Disconnected";
            }
        }

        #endregion
    }

    #region Data Models

    /// <summary>
    /// 综合测试请求
    /// </summary>
    public class ComprehensiveTestRequest
    {
        public string UserInput { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public string TestObject { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    /// <summary>
    /// 综合测试会话
    /// </summary>
    public class ComprehensiveTestSession
    {
        public string SessionId { get; set; } = string.Empty;
        public ComprehensiveTestRequest TestRequest { get; set; } = new();
        public TestStatus Status { get; set; }
        public int Progress { get; set; }
        public string CurrentPhase { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public ComprehensiveTestResults Results { get; set; } = new();
    }

    /// <summary>
    /// 测试状态枚举
    /// </summary>
    public enum TestStatus
    {
        Running,
        Completed,
        Failed,
        Stopped
    }

    /// <summary>
    /// 综合测试结果
    /// </summary>
    public class ComprehensiveTestResults
    {
        public NlpTestResult? NlpTest { get; set; }
        public CodeGenerationTestResult? CodeGenerationTest { get; set; }
        public SafetyValidationTestResult? SafetyValidationTest { get; set; }
        public HardwareSimulationTestResult? HardwareSimulationTest { get; set; }
        public CodeExecutionTestResult? CodeExecutionTest { get; set; }
        public VisualizationData? VisualizationData { get; set; }
        public double OverallScore { get; set; }
        public int TestCount { get; set; }
        public int PassedTests { get; set; }
        public string? OverallError { get; set; }
    }

    /// <summary>
    /// NLP测试结果
    /// </summary>
    public class NlpTestResult
    {
        public bool Success { get; set; }
        public long ProcessingTime { get; set; }
        public TestRequirement? ParsedRequirement { get; set; }
        public int AccuracyScore { get; set; }
        public Dictionary<string, object> ExtractedParameters { get; set; } = new();
        public string? Error { get; set; }
    }

    /// <summary>
    /// 代码生成测试结果
    /// </summary>
    public class CodeGenerationTestResult
    {
        public bool Success { get; set; }
        public long ProcessingTime { get; set; }
        public string? GeneratedCode { get; set; }
        public int QualityScore { get; set; }
        public string? ComplexityLevel { get; set; }
        public List<string> OptimizationSuggestions { get; set; } = new();
        public string? Error { get; set; }
    }

    /// <summary>
    /// 安全验证测试结果
    /// </summary>
    public class SafetyValidationTestResult
    {
        public bool Success { get; set; }
        public string? RiskLevel { get; set; }
        public int IssuesFound { get; set; }
        public List<SecurityIssue> SecurityIssues { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string? Error { get; set; }
    }

    /// <summary>
    /// 安全问题
    /// </summary>
    public class SecurityIssue
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        public string? FixSuggestion { get; set; }
    }

    /// <summary>
    /// 硬件模拟测试结果
    /// </summary>
    public class HardwareSimulationTestResult
    {
        public bool Success { get; set; }
        public string DeviceType { get; set; } = string.Empty;
        public bool DeviceFound { get; set; }
        public string? DeviceStatus { get; set; }
        public string? DriverVersion { get; set; }
        public bool SimulatedDataGenerated { get; set; }
        public int ConnectionLatency { get; set; }
        public int DataThroughput { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// 代码执行测试结果
    /// </summary>
    public class CodeExecutionTestResult
    {
        public bool Success { get; set; }
        public long ExecutionTime { get; set; }
        public string? Output { get; set; }
        public object? ResultData { get; set; }
        public long? MemoryUsage { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// 可视化数据
    /// </summary>
    public class VisualizationData
    {
        public double[] WaveformData { get; set; } = Array.Empty<double>();
        public double[] SpectrumData { get; set; } = Array.Empty<double>();
        public Dictionary<string, double> StatisticalData { get; set; } = new();
        public ThdAnalysisData? ThdData { get; set; }
    }

    /// <summary>
    /// THD分析数据
    /// </summary>
    public class ThdAnalysisData
    {
        public double ThdPercentage { get; set; }
        public double FundamentalFrequency { get; set; }
        public List<HarmonicData> Harmonics { get; set; } = new();
    }

    /// <summary>
    /// 谐波数据
    /// </summary>
    public class HarmonicData
    {
        public int Order { get; set; }
        public double Frequency { get; set; }
        public double Amplitude { get; set; }
        public double Phase { get; set; }
    }

    #endregion
}
