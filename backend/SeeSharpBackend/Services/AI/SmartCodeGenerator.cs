using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SeeSharpBackend.Services.AI.Models;

namespace SeeSharpBackend.Services.AI
{
    /// <summary>
    /// 智能代码生成器实现
    /// 根据测试需求和模板动态生成个性化代码
    /// </summary>
    public class SmartCodeGenerator : ISmartCodeGenerator
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmartCodeGenerator> _logger;

        // 代码模板缓存
        private readonly Dictionary<string, string> _templateCache = new();

        // 危险操作检测模式
        private readonly List<string> _dangerousPatterns = new()
        {
            @"System\.IO\.File\.Delete",
            @"System\.IO\.Directory\.Delete",
            @"System\.Diagnostics\.Process\.Start",
            @"System\.Reflection\.Assembly\.Load",
            @"System\.Net\.WebClient",
            @"System\.Net\.Http\.HttpClient",
            @"Environment\.Exit",
            @"Application\.Exit",
            @"unsafe\s+{",
            @"DllImport"
        };

        public SmartCodeGenerator(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<SmartCodeGenerator> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;

            // 初始化模板缓存
            InitializeTemplateCache();
        }

        public async Task<CodeGenerationResult> GenerateTestCodeAsync(TestRequirement requirement, TemplateMatch templateMatch)
        {
            try
            {
                _logger.LogInformation("开始生成测试代码: {TestType} - {TemplateId}", requirement.TestType, templateMatch.TemplateId);

                // 1. 加载基础模板
                var baseTemplate = await LoadTemplateAsync(templateMatch.TemplateId);
                if (string.IsNullOrEmpty(baseTemplate))
                {
                    return CreateErrorResult("无法加载模板: " + templateMatch.TemplateId);
                }

                // 2. 智能参数映射
                var parameters = await MapParametersAsync(requirement);

                // 3. 代码模板替换
                var parameterizedCode = ReplaceParameters(baseTemplate, parameters);

                // 4. AI 代码优化
                var optimizedCode = await OptimizeCodeAsync(parameterizedCode, requirement);

                // 5. 安全验证
                var validationResult = await ValidateCodeSafetyAsync(optimizedCode);
                if (!validationResult.IsValid)
                {
                    return CreateErrorResult($"代码安全验证失败: {string.Join(", ", validationResult.Issues.Select(i => i.Description))}");
                }

                // 6. 添加注释和文档
                var documentedCode = await AddDocumentationAsync(optimizedCode, requirement);

                // 7. 计算质量评分
                var qualityScore = CalculateQualityScore(documentedCode, requirement);

                return new CodeGenerationResult
                {
                    Success = true,
                    GeneratedCode = documentedCode,
                    TemplateId = templateMatch.TemplateId,
                    GeneratedParameters = parameters,
                    QualityScore = qualityScore,
                    EstimatedExecutionTime = EstimateExecutionTime(requirement),
                    ComplexityLevel = DetermineComplexityLevel(requirement),
                    OptimizationSuggestions = GenerateOptimizationSuggestions(documentedCode, requirement),
                    GeneratedAt = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成代码时发生错误");
                return CreateErrorResult($"代码生成失败: {ex.Message}");
            }
        }

        public async Task<CodeGenerationResult> GenerateTestCodeAsync(TestRequirement requirement)
        {
            // 不使用模板的代码生成，直接基于需求生成
            var templateMatch = new TemplateMatch
            {
                TemplateId = "dynamic_" + requirement.TestType.ToLower().Replace(" ", "_"),
                TemplateName = "动态生成模板",
                TemplateContent = GetDynamicTemplate(requirement),
                Confidence = 0.8
            };

            return await GenerateTestCodeAsync(requirement, templateMatch);
        }

        public async Task<string> OptimizeCodeAsync(string code, TestRequirement requirement)
        {
            try
            {
                // 使用AI API进行代码优化
                var prompt = $@"
请优化以下C#测试代码，要求：
1. 提升性能和效率
2. 增强错误处理
3. 优化内存使用
4. 保持功能不变

测试需求: {requirement.TestType} - {requirement.TestObject}
推荐设备: {requirement.RecommendedDevice}

原始代码：
```csharp
{code}
```

请返回优化后的完整代码：";

                var optimizedCode = await CallAIForCodeOptimization(prompt);
                if (!string.IsNullOrEmpty(optimizedCode))
                {
                    // 提取代码块
                    var codeMatch = Regex.Match(optimizedCode, @"```csharp\s*(.*?)\s*```", RegexOptions.Singleline);
                    if (codeMatch.Success)
                    {
                        return codeMatch.Groups[1].Value.Trim();
                    }
                }

                return code; // 如果优化失败，返回原代码
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI代码优化失败，返回原代码");
                return code;
            }
        }

        public async Task<CodeValidationResult> ValidateCodeSafetyAsync(string code)
        {
            var result = new CodeValidationResult
            {
                IsValid = true,
                RiskLevel = SecurityRiskLevel.Low,
                Issues = new List<ValidationIssue>()
            };

            try
            {
                // 1. 危险操作检测
                foreach (var pattern in _dangerousPatterns)
                {
                    var matches = Regex.Matches(code, pattern, RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        var lineNumber = GetLineNumber(code, match.Index);
                        result.Issues.Add(new ValidationIssue
                        {
                            IssueType = "危险操作",
                            Description = $"检测到潜在危险操作: {match.Value}",
                            Severity = "高",
                            LineNumber = lineNumber,
                            FixSuggestion = "移除或替换此操作为安全的替代方案"
                        });
                        result.RiskLevel = SecurityRiskLevel.High;
                        result.IsValid = false;
                    }
                }

                // 2. 语法基础检查
                if (code.Count(c => c == '{') != code.Count(c => c == '}'))
                {
                    result.Issues.Add(new ValidationIssue
                    {
                        IssueType = "语法错误",
                        Description = "大括号不匹配",
                        Severity = "中",
                        LineNumber = 0,
                        FixSuggestion = "检查代码中的大括号配对"
                    });
                }

                // 3. 简仪设备API使用检查
                ValidateDeviceApiUsage(code, result);

                // 4. 生成修复建议
                if (result.Issues.Any())
                {
                    result.Recommendations = GenerateSecurityRecommendations(result.Issues);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "代码安全验证时发生错误");
                result.IsValid = false;
                result.RiskLevel = SecurityRiskLevel.Critical;
                result.Issues.Add(new ValidationIssue
                {
                    IssueType = "验证错误",
                    Description = $"安全验证过程中发生错误: {ex.Message}",
                    Severity = "严重"
                });
                return result;
            }
        }

        public async Task<string> AddDocumentationAsync(string code, TestRequirement requirement)
        {
            try
            {
                var documentedCode = new StringBuilder();

                // 添加文件头注释
                documentedCode.AppendLine($"// AI自动生成的测试代码");
                documentedCode.AppendLine($"// 测试类型: {requirement.TestType}");
                documentedCode.AppendLine($"// 测试对象: {requirement.TestObject}");
                documentedCode.AppendLine($"// 推荐设备: {requirement.RecommendedDevice}");
                if (!string.IsNullOrEmpty(requirement.FrequencyRange))
                    documentedCode.AppendLine($"// 频率范围: {requirement.FrequencyRange}");
                if (!string.IsNullOrEmpty(requirement.AnalysisMethod))
                    documentedCode.AppendLine($"// 分析方法: {requirement.AnalysisMethod}");
                documentedCode.AppendLine($"// 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                documentedCode.AppendLine($"// 置信度: {requirement.Confidence:P1}");
                documentedCode.AppendLine();

                // 添加必要的using语句
                var requiredUsings = GetRequiredUsings(code);
                foreach (var usingStatement in requiredUsings)
                {
                    documentedCode.AppendLine($"using {usingStatement};");
                }
                if (requiredUsings.Any())
                    documentedCode.AppendLine();

                // 处理原代码，添加智能注释
                var lines = code.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    
                    // 为关键代码行添加解释注释
                    if (ShouldAddComment(line))
                    {
                        var comment = GenerateSmartComment(line, requirement);
                        if (!string.IsNullOrEmpty(comment))
                        {
                            documentedCode.AppendLine($"        // {comment}");
                        }
                    }
                    
                    documentedCode.AppendLine(line);
                }

                return documentedCode.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "添加文档注释失败，返回原代码");
                return code;
            }
        }

        #region Private Methods

        private void InitializeTemplateCache()
        {
            // 初始化内置模板
            _templateCache["vibration_test"] = GetVibrationTestTemplate();
            _templateCache["electrical_test"] = GetElectricalTestTemplate();
            _templateCache["temperature_test"] = GetTemperatureTestTemplate();
        }

        private async Task<string> LoadTemplateAsync(string templateId)
        {
            // 先检查缓存
            if (_templateCache.TryGetValue(templateId, out var cachedTemplate))
            {
                return cachedTemplate;
            }

            // 如果是内置模板，直接返回
            var builtInTemplate = GetBuiltInTemplate(templateId);
            if (!string.IsNullOrEmpty(builtInTemplate))
            {
                _templateCache[templateId] = builtInTemplate;
                return builtInTemplate;
            }

            // 尝试从文件系统加载
            var templatePath = Path.Combine("Templates", $"{templateId}.cs");
            if (File.Exists(templatePath))
            {
                var template = await File.ReadAllTextAsync(templatePath);
                _templateCache[templateId] = template;
                return template;
            }

            return string.Empty;
        }

        private async Task<Dictionary<string, object>> MapParametersAsync(TestRequirement requirement)
        {
            var parameters = new Dictionary<string, object>();

            // 智能参数映射
            if (!string.IsNullOrEmpty(requirement.FrequencyRange))
            {
                var (minFreq, maxFreq) = ParseFrequencyRange(requirement.FrequencyRange);
                parameters["sampleRate"] = CalculateOptimalSampleRate(maxFreq);
                parameters["filterCutoff"] = maxFreq;
                parameters["minFrequency"] = minFreq;
                parameters["maxFrequency"] = maxFreq;
            }

            parameters["deviceType"] = requirement.RecommendedDevice ?? "JYUSB1601";
            parameters["channelCount"] = DetermineChannelCount(requirement.TestType);
            parameters["analysisMethod"] = SelectAnalysisMethod(requirement.AnalysisMethod);
            parameters["bufferSize"] = CalculateBufferSize(parameters);
            parameters["testObject"] = requirement.TestObject;
            parameters["testType"] = requirement.TestType;

            return parameters;
        }

        private string ReplaceParameters(string template, Dictionary<string, object> parameters)
        {
            var result = template;
            
            foreach (var param in parameters)
            {
                var placeholder = "{{" + param.Key.ToUpper() + "}}";
                result = result.Replace(placeholder, param.Value?.ToString() ?? "");
            }

            return result;
        }

        private async Task<string> CallAIForCodeOptimization(string prompt)
        {
            // TODO: 实现AI API调用
            // 这里可以调用DeepSeek或Claude API进行代码优化
            return await Task.FromResult(string.Empty);
        }

        private CodeGenerationResult CreateErrorResult(string errorMessage)
        {
            return new CodeGenerationResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                GeneratedAt = DateTime.Now
            };
        }

        private string GetDynamicTemplate(TestRequirement requirement)
        {
            return requirement.TestType.ToLower() switch
            {
                "振动测试" => GetVibrationTestTemplate(),
                "电气测试" => GetElectricalTestTemplate(),
                "温度测量" => GetTemperatureTestTemplate(),
                _ => GetDefaultTestTemplate()
            };
        }

        private string GetBuiltInTemplate(string templateId)
        {
            return templateId.ToLower() switch
            {
                "vibration_test" => GetVibrationTestTemplate(),
                "electrical_test" => GetElectricalTestTemplate(),
                "temperature_test" => GetTemperatureTestTemplate(),
                _ => string.Empty
            };
        }

        private string GetVibrationTestTemplate()
        {
            return @"
public class VibrationTest
{
    public static async Task<object> RunTest()
    {
        try
        {
            Console.WriteLine(""开始振动测试 - {{TEST_OBJECT}}"");
            Console.WriteLine(""设备: {{DEVICE_TYPE}}"");
            Console.WriteLine(""采样率: {{SAMPLE_RATE}} Hz"");
            
            // 模拟数据采集
            var data = GenerateVibrationData({{SAMPLE_RATE}}, {{BUFFER_SIZE}});
            
            // 执行FFT分析
            var fftResult = PerformFFTAnalysis(data);
            
            // 故障特征检测
            var faults = DetectFaults(fftResult);
            
            return new
            {
                deviceType = ""{{DEVICE_TYPE}}"",
                analysisType = ""振动频谱分析"",
                timestamp = DateTime.Now,
                spectrumData = fftResult,
                detectedFaults = faults,
                summary = $""检测完成，发现 {faults.Count} 个潜在故障""
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($""测试失败: {ex.Message}"");
            throw;
        }
    }
    
    private static double[] GenerateVibrationData(int sampleRate, int bufferSize)
    {
        var random = new Random();
        var data = new double[bufferSize];
        for (int i = 0; i < bufferSize; i++)
        {
            data[i] = Math.Sin(2 * Math.PI * 50 * i / sampleRate) + 0.1 * random.NextDouble();
        }
        return data;
    }
    
    private static double[] PerformFFTAnalysis(double[] data)
    {
        // 简化的FFT结果
        var fftSize = data.Length / 2;
        var result = new double[fftSize];
        for (int i = 0; i < fftSize; i++)
        {
            result[i] = Math.Abs(data[i] * Math.Cos(i * Math.PI / fftSize));
        }
        return result;
    }
    
    private static List<string> DetectFaults(double[] spectrum)
    {
        var faults = new List<string>();
        var maxValue = spectrum.Max();
        var threshold = maxValue * 0.3;
        
        for (int i = 0; i < spectrum.Length; i++)
        {
            if (spectrum[i] > threshold)
            {
                faults.Add($""频率 {i * 10} Hz 处检测到异常"");
            }
        }
        
        return faults;
    }
}";
        }

        private string GetElectricalTestTemplate()
        {
            return @"
public class ElectricalTest
{
    public static async Task<object> RunTest()
    {
        try
        {
            Console.WriteLine(""开始电气测试 - {{TEST_OBJECT}}"");
            Console.WriteLine(""设备: {{DEVICE_TYPE}}"");
            
            // 信号生成和测量
            var signal = GenerateTestSignal();
            var measurements = MeasureSignalParameters(signal);
            
            return new
            {
                deviceType = ""{{DEVICE_TYPE}}"",
                analysisType = ""电气参数测试"",
                timestamp = DateTime.Now,
                measurements = measurements,
                summary = ""电气测试完成""
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($""测试失败: {ex.Message}"");
            throw;
        }
    }
    
    private static double[] GenerateTestSignal()
    {
        var signal = new double[1000];
        for (int i = 0; i < signal.Length; i++)
        {
            signal[i] = Math.Sin(2 * Math.PI * i / 100);
        }
        return signal;
    }
    
    private static Dictionary<string, double> MeasureSignalParameters(double[] signal)
    {
        return new Dictionary<string, double>
        {
            [""RMS""] = Math.Sqrt(signal.Sum(x => x * x) / signal.Length),
            [""Peak""] = signal.Max(),
            [""Frequency""] = 1000.0
        };
    }
}";
        }

        private string GetTemperatureTestTemplate()
        {
            return @"
public class TemperatureTest
{
    public static async Task<object> RunTest()
    {
        try
        {
            Console.WriteLine(""开始温度测试 - {{TEST_OBJECT}}"");
            Console.WriteLine(""设备: {{DEVICE_TYPE}}"");
            
            // 温度数据采集
            var temperatures = CollectTemperatureData();
            var statistics = CalculateStatistics(temperatures);
            
            return new
            {
                deviceType = ""{{DEVICE_TYPE}}"",
                analysisType = ""温度监控"",
                timestamp = DateTime.Now,
                measurements = statistics,
                temperatureData = temperatures,
                summary = $""温度监控完成，平均温度: {statistics[""Average""]:F2}°C""
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($""测试失败: {ex.Message}"");
            throw;
        }
    }
    
    private static double[] CollectTemperatureData()
    {
        var random = new Random();
        var data = new double[100];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 25.0 + random.NextDouble() * 10.0;
        }
        return data;
    }
    
    private static Dictionary<string, double> CalculateStatistics(double[] data)
    {
        return new Dictionary<string, double>
        {
            [""Average""] = data.Average(),
            [""Max""] = data.Max(),
            [""Min""] = data.Min(),
            [""StdDev""] = Math.Sqrt(data.Sum(x => Math.Pow(x - data.Average(), 2)) / data.Length)
        };
    }
}";
        }

        private string GetDefaultTestTemplate()
        {
            return @"
public class DefaultTest
{
    public static async Task<object> RunTest()
    {
        try
        {
            Console.WriteLine(""开始通用测试"");
            
            // 执行基础测试
            var result = PerformBasicTest();
            
            return new
            {
                deviceType = ""通用设备"",
                analysisType = ""通用测试"",
                timestamp = DateTime.Now,
                result = result,
                summary = ""通用测试完成""
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($""测试失败: {ex.Message}"");
            throw;
        }
    }
    
    private static string PerformBasicTest()
    {
        return ""测试执行成功"";
    }
}";
        }

        // 辅助方法的存根实现
        private (double min, double max) ParseFrequencyRange(string range) => (0, 1000);
        private int CalculateOptimalSampleRate(double maxFreq) => (int)(maxFreq * 2.56);
        private int DetermineChannelCount(string testType) => 1;
        private string SelectAnalysisMethod(string method) => method ?? "FFT";
        private int CalculateBufferSize(Dictionary<string, object> parameters) => 1024;
        private int CalculateQualityScore(string code, TestRequirement requirement) => 85;
        private int EstimateExecutionTime(TestRequirement requirement) => 5000;
        private string DetermineComplexityLevel(TestRequirement requirement) => "Medium";
        private List<string> GenerateOptimizationSuggestions(string code, TestRequirement requirement) => new();
        private void ValidateDeviceApiUsage(string code, CodeValidationResult result) { }
        private List<string> GenerateSecurityRecommendations(List<ValidationIssue> issues) => new();
        private int GetLineNumber(string code, int index) => code.Take(index).Count(c => c == '\n') + 1;
        private List<string> GetRequiredUsings(string code) => new() { "System", "System.Collections.Generic", "System.Linq" };
        private bool ShouldAddComment(string line) => line.Contains("new ") || line.Contains("for ") || line.Contains("if ");
        private string GenerateSmartComment(string line, TestRequirement requirement) => "执行关键操作";

        #endregion
    }
}
