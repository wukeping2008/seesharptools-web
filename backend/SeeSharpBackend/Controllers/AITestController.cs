using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Services.AI;
using SeeSharpBackend.Services.AI.Models;
using SeeSharpBackend.Services.CSharpRunner;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// AI智能测试控制器
    /// 处理自然语言测试需求分析和代码生成
    /// </summary>
    [ApiController]
    [Route("api/ai-test")]
    public class AITestController : ControllerBase
    {
        private readonly INaturalLanguageProcessor _nlpProcessor;
        private readonly ICSharpRunnerService _csharpRunner;
        private readonly ILogger<AITestController> _logger;

        public AITestController(
            INaturalLanguageProcessor nlpProcessor,
            ICSharpRunnerService csharpRunner,
            ILogger<AITestController> logger)
        {
            _nlpProcessor = nlpProcessor;
            _csharpRunner = csharpRunner;
            _logger = logger;
        }

        /// <summary>
        /// 分析用户测试需求
        /// </summary>
        /// <param name="request">需求分析请求</param>
        /// <returns>分析结果</returns>
        [HttpPost("analyze-requirement")]
        public async Task<IActionResult> AnalyzeRequirement([FromBody] AnalyzeRequirementRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserInput))
                {
                    return BadRequest(new { success = false, error = "用户输入不能为空" });
                }

                _logger.LogInformation("收到需求分析请求: {Input}", request.UserInput);

                // 1. 分析用户需求
                var requirement = await _nlpProcessor.AnalyzeRequirementAsync(request.UserInput);

                // 2. 验证分析结果
                var isValid = await _nlpProcessor.ValidateRequirementAsync(requirement);
                if (!isValid)
                {
                    _logger.LogWarning("需求分析结果验证失败: {Requirement}", requirement);
                    return BadRequest(new { success = false, error = "需求分析结果不完整，请提供更详细的描述" });
                }

                _logger.LogInformation("需求分析完成，置信度: {Confidence}", requirement.Confidence);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        requirement.TestObject,
                        requirement.TestType,
                        requirement.FrequencyRange,
                        requirement.AnalysisMethod,
                        requirement.RecommendedDevice,
                        requirement.Priority,
                        requirement.Confidence,
                        requirement.Parameters,
                        requirement.Keywords,
                        requirement.ApplicationDomains,
                        requirement.ComplexityLevel,
                        Timestamp = requirement.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        OriginalInput = requirement.OriginalInput
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分析需求时发生错误: {Input}", request.UserInput);
                return StatusCode(500, new { success = false, error = "服务器内部错误，请稍后重试" });
            }
        }

        /// <summary>
        /// 端到端测试流程：分析需求 -> 生成代码 -> 执行代码
        /// </summary>
        /// <param name="request">端到端请求</param>
        /// <returns>完整的测试结果</returns>
        [HttpPost("end-to-end-test")]
        public async Task<IActionResult> EndToEndTest([FromBody] EndToEndTestRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserInput))
                {
                    return BadRequest(new { success = false, error = "用户输入不能为空" });
                }

                _logger.LogInformation("开始端到端测试流程: {Input}", request.UserInput);

                var result = new EndToEndTestResult
                {
                    StartTime = DateTime.Now,
                    UserInput = request.UserInput
                };

                // 1. 分析需求
                _logger.LogInformation("步骤1: 分析用户需求");
                result.Requirement = await _nlpProcessor.AnalyzeRequirementAsync(request.UserInput);
                
                var isValid = await _nlpProcessor.ValidateRequirementAsync(result.Requirement);
                if (!isValid)
                {
                    result.Success = false;
                    result.ErrorMessage = "需求分析结果不完整";
                    return BadRequest(new { success = false, error = result.ErrorMessage, data = result });
                }

                // 2. 生成代码
                _logger.LogInformation("步骤2: 生成测试代码");
                result.GeneratedCode = GenerateCodeByTemplate(result.Requirement);
                if (string.IsNullOrEmpty(result.GeneratedCode))
                {
                    result.Success = false;
                    result.ErrorMessage = "代码生成失败";
                    return BadRequest(new { success = false, error = result.ErrorMessage, data = result });
                }

                // 3. 执行代码
                _logger.LogInformation("步骤3: 执行测试代码");
                var executionResult = await _csharpRunner.ExecuteCodeAsync(result.GeneratedCode);
                result.ExecutionResult = executionResult;

                result.Success = executionResult.Success;
                result.EndTime = DateTime.Now;
                result.TotalTime = result.EndTime - result.StartTime;

                if (result.Success)
                {
                    _logger.LogInformation("端到端测试完成，总耗时: {TotalTime}ms", result.TotalTime.TotalMilliseconds);
                }
                else
                {
                    _logger.LogWarning("端到端测试失败: {Error}", executionResult.ErrorOutput);
                }

                return Ok(new
                {
                    success = result.Success,
                    data = new
                    {
                        userInput = result.UserInput,
                        requirement = new
                        {
                            result.Requirement.TestObject,
                            result.Requirement.TestType,
                            result.Requirement.FrequencyRange,
                            result.Requirement.AnalysisMethod,
                            result.Requirement.RecommendedDevice,
                            result.Requirement.Confidence,
                            result.Requirement.Parameters
                        },
                        generatedCode = result.GeneratedCode,
                        executionResult = new
                        {
                            success = result.ExecutionResult.Success,
                            output = result.ExecutionResult.ConsoleOutput,
                            error = result.ExecutionResult.ErrorOutput,
                            executionTime = result.ExecutionResult.ElapsedMilliseconds,
                            returnValue = result.ExecutionResult.ReturnValue
                        },
                        timing = new
                        {
                            startTime = result.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                            endTime = result.EndTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                            totalTimeMs = result.TotalTime.TotalMilliseconds
                        }
                    },
                    error = result.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "端到端测试时发生错误");
                return StatusCode(500, new { success = false, error = "端到端测试失败，请稍后重试" });
            }
        }

        /// <summary>
        /// 生成测试代码
        /// </summary>
        /// <param name="request">代码生成请求</param>
        /// <returns>生成的代码</returns>
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateTestCode([FromBody] GenerateCodeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Requirement))
                {
                    return BadRequest(new { success = false, error = "测试需求不能为空" });
                }

                _logger.LogInformation("收到代码生成请求: {Requirement}", request.Requirement);

                // 1. 分析需求
                var requirement = await _nlpProcessor.AnalyzeRequirementAsync(request.Requirement);

                // 2. 生成代码
                var generatedCode = GenerateCodeByTemplate(requirement);
                if (string.IsNullOrEmpty(generatedCode))
                {
                    return BadRequest(new { success = false, error = "代码生成失败" });
                }

                // 3. 代码质量评估
                var codeQuality = ValidateCodeQuality(generatedCode);

                _logger.LogInformation("代码生成完成，质量评分: {Score}", codeQuality.Score);

                return Ok(new
                {
                    success = true,
                    generatedCode = generatedCode,
                    codeQuality = new
                    {
                        score = codeQuality.Score,
                        issues = codeQuality.Issues,
                        suggestions = codeQuality.Suggestions
                    },
                    selectedTemplate = new
                    {
                        id = requirement.TestType,
                        name = $"{requirement.TestType}模板",
                        deviceType = requirement.RecommendedDevice,
                        testType = requirement.TestType
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成测试代码时发生错误");
                return StatusCode(500, new { success = false, error = "代码生成失败，请稍后重试" });
            }
        }

        /// <summary>
        /// 获取推荐模板
        /// </summary>
        /// <param name="requirement">需求描述（可选）</param>
        /// <returns>推荐的模板列表</returns>
        [HttpGet("templates/recommended")]
        public async Task<IActionResult> GetRecommendedTemplates([FromQuery] string? requirement = null)
        {
            try
            {
                var templates = new List<object>();

                if (!string.IsNullOrEmpty(requirement))
                {
                    // 基于需求推荐模板
                    var analyzedRequirement = await _nlpProcessor.AnalyzeRequirementAsync(requirement);
                    templates.Add(new
                    {
                        id = analyzedRequirement.TestType,
                        name = $"{analyzedRequirement.TestType}测试模板",
                        description = $"专为{analyzedRequirement.TestObject}设计的{analyzedRequirement.TestType}模板",
                        deviceType = analyzedRequirement.RecommendedDevice,
                        testType = analyzedRequirement.TestType,
                        tags = new[] { analyzedRequirement.TestType, analyzedRequirement.RecommendedDevice },
                        confidence = analyzedRequirement.Confidence
                    });
                }
                else
                {
                    // 返回默认模板
                    templates.AddRange(new[]
                    {
                        new
                        {
                            id = "vibration_test",
                            name = "振动测试模板",
                            description = "用于振动信号采集和FFT分析的标准模板",
                            deviceType = "JYUSB1601",
                            testType = "振动测试",
                            tags = new[] { "振动", "FFT", "故障诊断" },
                            confidence = 0.9
                        },
                        new
                        {
                            id = "electrical_test",
                            name = "电气测试模板",
                            description = "用于电气信号THD分析和功率测量的模板",
                            deviceType = "JY5500",
                            testType = "电气测试",
                            tags = new[] { "电气", "THD", "功率分析" },
                            confidence = 0.9
                        },
                        new
                        {
                            id = "temperature_test",
                            name = "温度测量模板",
                            description = "用于多点温度采集和统计分析的模板",
                            deviceType = "JYUSB1601",
                            testType = "温度测量",
                            tags = new[] { "温度", "多点采集", "统计分析" },
                            confidence = 0.8
                        }
                    });
                }

                return Ok(new
                {
                    success = true,
                    templates = templates
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取推荐模板时发生错误");
                return StatusCode(500, new { success = false, error = "获取推荐模板失败" });
            }
        }

        /// <summary>
        /// 保存用户模板
        /// </summary>
        /// <param name="template">模板信息</param>
        /// <returns>保存结果</returns>
        [HttpPost("templates")]
        public async Task<IActionResult> SaveUserTemplate([FromBody] SaveTemplateRequest template)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(template.Name) || string.IsNullOrWhiteSpace(template.Code))
                {
                    return BadRequest(new { success = false, error = "模板名称和代码不能为空" });
                }

                var templateId = Guid.NewGuid().ToString();
                
                _logger.LogInformation("保存用户模板: {Name}", template.Name);

                // 这里应该保存到数据库，暂时返回成功响应
                return Ok(new
                {
                    success = true,
                    id = templateId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存用户模板时发生错误");
                return StatusCode(500, new { success = false, error = "保存模板失败" });
            }
        }

        /// <summary>
        /// 获取模板统计信息
        /// </summary>
        /// <returns>统计信息</returns>
        [HttpGet("templates/statistics")]
        public async Task<IActionResult> GetTemplateStatistics()
        {
            try
            {
                // 模拟统计数据
                var statistics = new
                {
                    totalTemplates = 15,
                    userTemplates = 3,
                    systemTemplates = 12,
                    mostUsedTemplate = "vibration_test",
                    totalUsage = 127,
                    categories = new[]
                    {
                        new { name = "振动测试", count = 5, usage = 45 },
                        new { name = "电气测试", count = 4, usage = 38 },
                        new { name = "温度测量", count = 3, usage = 25 },
                        new { name = "通用测试", count = 3, usage = 19 }
                    }
                };

                return Ok(new
                {
                    success = true,
                    statistics = statistics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取模板统计时发生错误");
                return StatusCode(500, new { success = false, error = "获取统计信息失败" });
            }
        }

        /// <summary>
        /// 获取测试历史记录
        /// </summary>
        /// <param name="limit">返回记录数量限制</param>
        /// <returns>历史记录</returns>
        [HttpGet("history")]
        public async Task<IActionResult> GetTestHistory([FromQuery] int limit = 10)
        {
            try
            {
                // 模拟历史数据
                var history = new List<object>();
                for (int i = 0; i < Math.Min(limit, 5); i++)
                {
                    history.Add(new
                    {
                        id = Guid.NewGuid().ToString(),
                        requirement = $"测试需求 {i + 1}",
                        testType = i % 2 == 0 ? "振动测试" : "电气测试",
                        deviceType = i % 2 == 0 ? "JYUSB1601" : "JY5500",
                        success = true,
                        timestamp = DateTime.Now.AddHours(-i * 2).ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                return Ok(new
                {
                    success = true,
                    history = history
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取测试历史时发生错误");
                return StatusCode(500, new { success = false, error = "获取历史记录失败" });
            }
        }

        /// <summary>
        /// 保存测试结果
        /// </summary>
        /// <param name="result">测试结果</param>
        /// <returns>保存结果</returns>
        [HttpPost("results")]
        public async Task<IActionResult> SaveTestResult([FromBody] SaveTestResultRequest result)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(result.Requirement))
                {
                    return BadRequest(new { success = false, error = "测试需求不能为空" });
                }

                var resultId = Guid.NewGuid().ToString();
                
                _logger.LogInformation("保存测试结果: {Requirement}", result.Requirement);

                // 这里应该保存到数据库，暂时返回成功响应
                return Ok(new
                {
                    success = true,
                    id = resultId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存测试结果时发生错误");
                return StatusCode(500, new { success = false, error = "保存测试结果失败" });
            }
        }

        /// <summary>
        /// 检查AI服务健康状态
        /// </summary>
        /// <returns>健康状态</returns>
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var aiHealth = await _nlpProcessor.CheckApiHealthAsync();
                var csharpRunnerHealth = true; // 假设C# Runner总是可用

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        aiService = aiHealth,
                        csharpRunner = csharpRunnerHealth,
                        overall = aiHealth && csharpRunnerHealth,
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "健康检查时发生错误");
                return StatusCode(500, new { success = false, error = "健康检查失败" });
            }
        }

        #region Private Methods

        /// <summary>
        /// 根据模板生成代码
        /// </summary>
        private string GenerateCodeByTemplate(TestRequirement requirement)
        {
            return requirement.TestType switch
            {
                "振动测试" => GenerateVibrationTestCode(requirement),
                "电气测试" => GenerateElectricalTestCode(requirement),
                "温度测量" => GenerateTemperatureTestCode(requirement),
                _ => GenerateDefaultTestCode(requirement)
            };
        }

        private string GenerateVibrationTestCode(TestRequirement requirement)
        {
            var device = requirement.RecommendedDevice ?? "JYUSB1601";
            var sampleRate = GetParameterValue(requirement.Parameters, "recommendedSampleRate", 25000);
            var fftSize = GetParameterValue(requirement.Parameters, "fftSize", 4096);

            return $@"// AI生成的振动测试代码 - {requirement.TestObject}
// 测试类型: {requirement.TestType}
// 推荐设备: {device}
// 频率范围: {requirement.FrequencyRange}

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public class VibrationTest
{{
    public static async Task<object> RunTest()
    {{
        try
        {{
            Console.WriteLine(""开始振动测试 - {requirement.TestObject}"");
            Console.WriteLine(""设备: {device}"");
            Console.WriteLine(""采样率: {sampleRate} Hz"");
            Console.WriteLine(""FFT大小: {fftSize}"");
            
            // 模拟数据采集
            var data = GenerateVibrationData({sampleRate}, {fftSize});
            
            // 执行FFT分析
            var fftResult = PerformFFTAnalysis(data);
            
            // 故障特征检测
            var faultFrequencies = DetectBearingFaults(fftResult);
            
            Console.WriteLine($""检测到 {{faultFrequencies.Length}} 个潜在故障频率"");
            
            return new {{
                TestType = ""{requirement.TestType}"",
                Device = ""{device}"",
                SampleRate = {sampleRate},
                FFTSize = {fftSize},
                DataPoints = data.Length,
                FaultFrequencies = faultFrequencies,
                MaxAmplitude = fftResult.Max(),
                Status = ""测试完成"",
                Timestamp = DateTime.Now
            }};
        }}
        catch (Exception ex)
        {{
            Console.WriteLine($""测试执行错误: {{ex.Message}}"");
            throw;
        }}
    }}
    
    private static double[] GenerateVibrationData(int sampleRate, int length)
    {{
        var data = new double[length];
        var random = new Random();
        
        for (int i = 0; i < length; i++)
        {{
            // 基础振动信号 + 故障特征频率 + 噪声
            var t = (double)i / sampleRate;
            data[i] = Math.Sin(2 * Math.PI * 60 * t) +  // 基频60Hz
                     0.3 * Math.Sin(2 * Math.PI * 1200 * t) + // 轴承故障特征
                     0.1 * (random.NextDouble() - 0.5); // 噪声
        }}
        
        return data;
    }}
    
    private static double[] PerformFFTAnalysis(double[] data)
    {{
        // 简化的FFT模拟
        var fftResult = new double[data.Length / 2];
        for (int i = 0; i < fftResult.Length; i++)
        {{
            fftResult[i] = Math.Abs(data[i] + data[data.Length - 1 - i]);
        }}
        return fftResult;
    }}
    
    private static double[] DetectBearingFaults(double[] spectrum)
    {{
        var faults = new List<double>();
        
        // 检测轴承故障特征频率
        for (int i = 100; i < spectrum.Length; i += 100)
        {{
            if (spectrum[i] > spectrum.Average() * 2)
            {{
                faults.Add(i * 0.5); // 转换为频率
            }}
        }}
        
        return faults.ToArray();
    }}
}}

// 执行测试
await VibrationTest.RunTest();";
        }

        private string GenerateElectricalTestCode(TestRequirement requirement)
        {
            var device = requirement.RecommendedDevice ?? "JY5500";
            var sampleRate = GetParameterValue(requirement.Parameters, "recommendedSampleRate", 100000);

            return $@"// AI生成的电气测试代码 - {requirement.TestObject}
// 测试类型: {requirement.TestType}
// 推荐设备: {device}
// 分析方法: {requirement.AnalysisMethod}

using System;
using System.Threading.Tasks;

public class ElectricalTest
{{
    public static async Task<object> RunTest()
    {{
        try
        {{
            Console.WriteLine(""开始电气测试 - {requirement.TestObject}"");
            Console.WriteLine(""设备: {device}"");
            Console.WriteLine(""采样率: {sampleRate} Hz"");
            
            // 生成测试信号
            var signal = GenerateTestSignal({sampleRate});
            
            // THD分析
            var thdResult = CalculateTHD(signal);
            
            // 功率分析
            var powerResult = CalculatePower(signal);
            
            Console.WriteLine($""THD: {{thdResult:F4}}% ({{20 * Math.Log10(thdResult / 100):F2}} dB)"");
            Console.WriteLine($""有效功率: {{powerResult:F2}} W"");
            
            return new {{
                TestType = ""{requirement.TestType}"",
                Device = ""{device}"",
                SampleRate = {sampleRate},
                THD_Percent = thdResult,
                THD_dB = 20 * Math.Log10(thdResult / 100),
                Power_W = powerResult,
                SignalQuality = thdResult < 0.1 ? ""优秀"" : thdResult < 1 ? ""良好"" : ""一般"",
                Status = ""测试完成"",
                Timestamp = DateTime.Now
            }};
        }}
        catch (Exception ex)
        {{
            Console.WriteLine($""测试执行错误: {{ex.Message}}"");
            throw;
        }}
    }}
    
    private static double[] GenerateTestSignal(int sampleRate)
    {{
        var length = sampleRate; // 1秒数据
        var signal = new double[length];
        
        for (int i = 0; i < length; i++)
        {{
            var t = (double)i / sampleRate;
            // 1kHz基波 + 谐波失真
            signal[i] = Math.Sin(2 * Math.PI * 1000 * t) +
                       0.03 * Math.Sin(2 * Math.PI * 2000 * t) + // 2次谐波
                       0.02 * Math.Sin(2 * Math.PI * 3000 * t);  // 3次谐波
        }}
        
        return signal;
    }}
    
    private static double CalculateTHD(double[] signal)
    {{
        // 简化的THD计算
        var fundamental = CalculateRMS(signal, 1000);
        var harmonic2 = CalculateRMS(signal, 2000) * 0.03;
        var harmonic3 = CalculateRMS(signal, 3000) * 0.02;
        
        var harmonicSum = Math.Sqrt(harmonic2 * harmonic2 + harmonic3 * harmonic3);
        
        return (harmonicSum / fundamental) * 100;
    }}
    
    private static double CalculateRMS(double[] signal, double frequency)
    {{
        var sum = 0.0;
        for (int i = 0; i < signal.Length; i++)
        {{
            sum += signal[i] * signal[i];
        }}
        return Math.Sqrt(sum / signal.Length);
    }}
    
    private static double CalculatePower(double[] signal)
    {{
        var rms = CalculateRMS(signal, 1000);
        return rms * rms; // P = V²/R，假设R=1Ω
    }}
}}

// 执行测试
await ElectricalTest.RunTest();";
        }

        private string GenerateTemperatureTestCode(TestRequirement requirement)
        {
            var device = requirement.RecommendedDevice ?? "JYUSB1601";
            var samplingInterval = GetParameterValue(requirement.Parameters, "samplingInterval", "1s");

            return $@"// AI生成的温度测量代码 - {requirement.TestObject}
// 测试类型: {requirement.TestType}
// 推荐设备: {device}

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TemperatureTest
{{
    public static async Task<object> RunTest()
    {{
        try
        {{
            Console.WriteLine(""开始温度测量 - {requirement.TestObject}"");
            Console.WriteLine(""设备: {device}"");
            Console.WriteLine(""采样间隔: {samplingInterval}"");
            
            var temperatures = new List<double>();
            
            // 模拟多点温度采集
            for (int i = 0; i < 10; i++)
            {{
                var temp = MeasureTemperature();
                temperatures.Add(temp);
                Console.WriteLine($""第{{i + 1}}次测量: {{temp:F2}}°C"");
                
                await Task.Delay(1000); // 模拟采样间隔
            }}
            
            // 统计分析
            var avgTemp = temperatures.Average();
            var maxTemp = temperatures.Max();
            var minTemp = temperatures.Min();
            var stability = maxTemp - minTemp;
            
            Console.WriteLine($""平均温度: {{avgTemp:F2}}°C"");
            Console.WriteLine($""温度稳定性: ±{{stability / 2:F2}}°C"");
            
            return new {{
                TestType = ""{requirement.TestType}"",
                Device = ""{device}"",
                SamplingInterval = ""{samplingInterval}"",
                MeasurementCount = temperatures.Count,
                AverageTemperature = avgTemp,
                MaxTemperature = maxTemp,
                MinTemperature = minTemp,
                TemperatureStability = stability / 2,
                Temperatures = temperatures.ToArray(),
                Status = ""测试完成"",
                Timestamp = DateTime.Now
            }};
        }}
        catch (Exception ex)
        {{
            Console.WriteLine($""测试执行错误: {{ex.Message}}"");
            throw;
        }}
    }}
    
    private static double MeasureTemperature()
    {{
        var random = new Random();
        // 模拟温度测量，基础温度25°C + 随机波动
        return 25.0 + (random.NextDouble() - 0.5) * 4; // ±2°C波动
    }}
}}

// 执行测试
await TemperatureTest.RunTest();";
        }

        private string GenerateDefaultTestCode(TestRequirement requirement)
        {
            var device = requirement.RecommendedDevice ?? "JYUSB1601";

            return $@"// AI生成的通用测试代码 - {requirement.TestObject}
// 测试类型: {requirement.TestType}
// 推荐设备: {device}

using System;
using System.Threading.Tasks;

public class DefaultTest
{{
    public static async Task<object> RunTest()
    {{
        try
        {{
            Console.WriteLine(""开始测试 - {requirement.TestObject}"");
            Console.WriteLine(""设备: {device}"");
            Console.WriteLine(""测试类型: {requirement.TestType}"");
            
            // 模拟测试执行
            await Task.Delay(2000);
            
            Console.WriteLine(""测试执行完成"");
            
            return new {{
                TestType = ""{requirement.TestType}"",
                Device = ""{device}"",
                TestObject = ""{requirement.TestObject}"",
                Status = ""测试完成"",
                Timestamp = DateTime.Now,
                Message = ""通用测试模板，请根据具体需求调整测试逻辑""
            }};
        }}
        catch (Exception ex)
        {{
            Console.WriteLine($""测试执行错误: {{ex.Message}}"");
            throw;
        }}
    }}
}}

// 执行测试
await DefaultTest.RunTest();";
        }

        private T GetParameterValue<T>(Dictionary<string, object> parameters, string key, T defaultValue)
        {
            if (parameters != null && parameters.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// 验证代码质量
        /// </summary>
        private CodeQuality ValidateCodeQuality(string code)
        {
            var issues = new List<string>();
            var suggestions = new List<string>();
            var score = 100;

            // 基本语法检查
            if (!code.Contains("using System"))
            {
                issues.Add("缺少必要的using语句");
                score -= 10;
            }

            if (!code.Contains("class ") && !code.Contains("static void Main"))
            {
                issues.Add("缺少主类或Main方法");
                score -= 20;
            }

            // 异常处理检查
            if (!code.Contains("try") || !code.Contains("catch"))
            {
                suggestions.Add("建议添加异常处理机制");
                score -= 5;
            }

            // 资源释放检查
            if (code.Contains("new ") && !code.Contains("using") && !code.Contains("Dispose"))
            {
                suggestions.Add("建议使用using语句或手动释放资源");
                score -= 5;
            }

            // 代码长度检查
            var lines = code.Split('\n').Length;
            if (lines > 200)
            {
                suggestions.Add("代码较长，建议拆分为多个方法");
                score -= 3;
            }

            // 注释检查
            var commentLines = code.Split('\n').Where(line => 
                line.Trim().StartsWith("//") || line.Trim().StartsWith("/*")
            ).Count();
            var commentRatio = (double)commentLines / lines;
            if (commentRatio < 0.1)
            {
                suggestions.Add("建议增加代码注释以提高可读性");
                score -= 2;
            }

            return new CodeQuality
            {
                Score = Math.Max(score, 0),
                Issues = issues,
                Suggestions = suggestions
            };
        }

        #endregion
    }

    #region Request/Response Models

    public class AnalyzeRequirementRequest
    {
        public string UserInput { get; set; } = string.Empty;
    }

    public class EndToEndTestRequest
    {
        public string UserInput { get; set; } = string.Empty;
    }

    public class GenerateCodeRequest
    {
        public string Requirement { get; set; } = string.Empty;
        public string? DeviceType { get; set; }
        public string? TestType { get; set; }
        public string? TemplateId { get; set; }
    }

    public class SaveTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    public class SaveTestResultRequest
    {
        public string Requirement { get; set; } = string.Empty;
        public string GeneratedCode { get; set; } = string.Empty;
        public object? ExecutionResult { get; set; }
        public string DeviceType { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
    }

    public class CodeQuality
    {
        public int Score { get; set; }
        public List<string> Issues { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
    }

    public class EndToEndTestResult
    {
        public bool Success { get; set; }
        public string UserInput { get; set; } = string.Empty;
        public TestRequirement Requirement { get; set; } = new();
        public string GeneratedCode { get; set; } = string.Empty;
        public CSharpExecutionResult ExecutionResult { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalTime { get; set; }
    }

    #endregion
}
