using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SeeSharpBackend.Services.AI.Models;
using SeeSharpBackend.Services;

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
        private readonly IApiKeyService _apiKeyService;

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
            ILogger<SmartCodeGenerator> logger,
            IApiKeyService apiKeyService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _apiKeyService = apiKeyService;

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
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // 优先使用配置的API密钥服务
                var deepSeekConfig = await _apiKeyService.GetActiveApiKeyByProviderAsync("DeepSeek");
                if (deepSeekConfig != null)
                {
                    var result = await CallDeepSeekAPI(prompt, deepSeekConfig.ApiKey, deepSeekConfig.ApiUrl, deepSeekConfig.Model);
                    if (!string.IsNullOrEmpty(result))
                    {
                        stopwatch.Stop();
                        await _apiKeyService.RecordUsageAsync(deepSeekConfig.Id, 
                            prompt.Length / 4, // 粗略估算token数
                            result.Length / 4,
                            true,
                            stopwatch.ElapsedMilliseconds,
                            "CodeOptimization");
                        return result;
                    }
                }

                // 备选：使用VolcesDeepseek配置
                var volcesConfig = await _apiKeyService.GetActiveApiKeyByProviderAsync("VolcesDeepseek");
                if (volcesConfig != null)
                {
                    var result = await CallVolcesDeepSeekAPI(prompt, volcesConfig.ApiKey, volcesConfig.ApiUrl, volcesConfig.Model);
                    if (!string.IsNullOrEmpty(result))
                    {
                        stopwatch.Stop();
                        await _apiKeyService.RecordUsageAsync(volcesConfig.Id,
                            prompt.Length / 4,
                            result.Length / 4,
                            true,
                            stopwatch.ElapsedMilliseconds,
                            "CodeOptimization");
                        return result;
                    }
                }
                
                // 尝试使用配置文件中的VolcesDeepseek
                var volcesApiKey = _configuration["VolcesDeepseek:ApiKey"];
                if (!string.IsNullOrEmpty(volcesApiKey))
                {
                    var volcesApiUrl = _configuration["VolcesDeepseek:Url"];
                    var volcesModel = _configuration["VolcesDeepseek:Model"];
                    return await CallVolcesDeepSeekAPI(prompt, volcesApiKey, volcesApiUrl, volcesModel);
                }

                // 备选：使用Claude API
                var claudeConfig = await _apiKeyService.GetActiveApiKeyByProviderAsync("Claude");
                if (claudeConfig != null)
                {
                    var result = await CallClaudeAPI(prompt, claudeConfig.ApiKey, claudeConfig.ApiUrl, claudeConfig.Model);
                    if (!string.IsNullOrEmpty(result))
                    {
                        stopwatch.Stop();
                        await _apiKeyService.RecordUsageAsync(claudeConfig.Id,
                            prompt.Length / 4,
                            result.Length / 4,
                            true,
                            stopwatch.ElapsedMilliseconds,
                            "CodeOptimization");
                        return result;
                    }
                }

                _logger.LogWarning("未配置AI API密钥，跳过AI代码优化");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI代码优化调用失败");
                return string.Empty;
            }
        }

        private async Task<string> CallDeepSeekAPI(string prompt, string apiKey, string apiUrl, string model)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                httpClient.Timeout = TimeSpan.FromSeconds(60);

                var requestBody = new
                {
                    model = model ?? "deepseek-coder",
                    messages = new[]
                    {
                        new { role = "system", content = "你是一个专业的C#代码优化专家，专门优化测控领域的代码。请保持代码的功能不变，只进行性能和质量优化。" },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 4000,
                    temperature = 0.1,
                    stream = false
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent);
                    
                    if (result?.choices?.Length > 0)
                    {
                        _logger.LogInformation("DeepSeek API调用成功");
                        return result.choices[0].message.content;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("DeepSeek API调用失败: {StatusCode}, {Content}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeepSeek API调用异常");
            }

            return string.Empty;
        }

        private async Task<string> CallClaudeAPI(string prompt, string apiKey, string apiUrl, string model)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                httpClient.Timeout = TimeSpan.FromSeconds(60);

                var requestBody = new
                {
                    model = model ?? "claude-3-sonnet-20240229",
                    max_tokens = 4000,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ClaudeResponse>(responseContent);
                    
                    if (result?.content?.Length > 0)
                    {
                        _logger.LogInformation("Claude API调用成功");
                        return result.content[0].text;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Claude API调用失败: {StatusCode}, {Content}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Claude API调用异常");
            }

            return string.Empty;
        }

        private async Task<string> CallVolcesDeepSeekAPI(string prompt, string apiKey, string apiUrl, string model)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                httpClient.Timeout = TimeSpan.FromSeconds(60);

                var requestBody = new
                {
                    model = model ?? "deepseek-r1-250528",
                    messages = new[]
                    {
                        new { role = "system", content = "你是一个专业的C#代码优化专家，专门优化测控领域的代码。请保持代码的功能不变，只进行性能和质量优化。" },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = _configuration.GetValue<int>("VolcesDeepseek:MaxTokens", 4000),
                    temperature = 0.1,
                    stream = false
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent);
                    
                    if (result?.choices?.Length > 0)
                    {
                        _logger.LogInformation("VolcesDeepSeek API调用成功");
                        return result.choices[0].message.content;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("VolcesDeepSeek API调用失败: {StatusCode}, {Content}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VolcesDeepSeek API调用异常");
            }

            return string.Empty;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JYUSB1601;

/// <summary>
/// AI生成的JYUSB1601振动测试代码 - 符合MISD标准
/// 测试对象: {{TEST_OBJECT}}
/// 设备: {{DEVICE_TYPE}}
/// 生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"
/// </summary>
public class VibrationTest
{
    private JYUSB1601AITask aiTask;
    private string boardNumber;
    private int channelId;
    private double sampleRate;
    private int bufferSize;
    
    public static async Task<object> RunTest()
    {
        var test = new VibrationTest();
        return await test.ExecuteTest();
    }
    
    public async Task<object> ExecuteTest()
    {
        try
        {
            Console.WriteLine(""开始振动测试 - {{TEST_OBJECT}}"");
            Console.WriteLine(""设备: {{DEVICE_TYPE}}"");
            
            // MISD标准：设备初始化
            await InitializeDevice();
            
            // MISD标准：配置采集参数
            await ConfigureAcquisition();
            
            // MISD标准：启动数据采集
            await StartAcquisition();
            
            // MISD标准：读取振动数据
            var vibrationData = await AcquireVibrationData();
            
            // MISD标准：停止采集并清理资源
            await StopAndCleanup();
            
            // 执行振动分析
            var analysisResult = await AnalyzeVibrationData(vibrationData);
            
            return new
            {
                deviceType = ""{{DEVICE_TYPE}}"",
                testObject = ""{{TEST_OBJECT}}"",
                analysisType = ""振动频谱分析"",
                timestamp = DateTime.Now,
                sampleRate = sampleRate,
                sampleCount = vibrationData.Length,
                vibrationData = vibrationData.Take(1000).ToArray(), // 只返回前1000个点用于显示
                spectrumData = analysisResult.SpectrumData,
                detectedFaults = analysisResult.DetectedFaults,
                rmsValue = analysisResult.RmsValue,
                peakValue = analysisResult.PeakValue,
                summary = $""振动测试完成，RMS: {analysisResult.RmsValue:F3}V，Peak: {analysisResult.PeakValue:F3}V，发现 {analysisResult.DetectedFaults.Count} 个潜在故障""
            };
        }
        catch (Exception ex)
        {
            await StopAndCleanup();
            Console.WriteLine($""振动测试失败: {ex.Message}"");
            throw new Exception($""振动测试执行失败: {ex.Message}"", ex);
        }
    }
    
    /// <summary>
    /// MISD标准：设备初始化
    /// </summary>
    private async Task InitializeDevice()
    {
        boardNumber = ""{{BOARD_NUMBER}}"";
        channelId = {{CHANNEL_ID}};
        
        try
        {
            // 创建JYUSB1601 AI任务实例
            aiTask = new JYUSB1601AITask(boardNumber);
            Console.WriteLine($""成功初始化JYUSB1601设备，板卡号: {boardNumber}"");
        }
        catch (Exception ex)
        {
            throw new Exception($""JYUSB1601设备初始化失败: {ex.Message}"", ex);
        }
    }
    
    /// <summary>
    /// MISD标准：配置采集参数
    /// </summary>
    private async Task ConfigureAcquisition()
    {
        try
        {
            // 设置采集模式为连续采集
            aiTask.Mode = AIMode.Continuous;
            
            // 添加通道配置
            aiTask.AddChannel(channelId, {{MIN_RANGE}}, {{MAX_RANGE}});
            
            // 设置采样率
            sampleRate = {{SAMPLE_RATE}};
            aiTask.SampleRate = sampleRate;
            
            // 设置缓冲区大小
            bufferSize = {{BUFFER_SIZE}};
            
            Console.WriteLine($""配置完成 - 通道: {channelId}, 量程: {{MIN_RANGE}}V~{{MAX_RANGE}}V, 采样率: {sampleRate}Hz"");
        }
        catch (Exception ex)
        {
            throw new Exception($""采集参数配置失败: {ex.Message}"", ex);
        }
    }
    
    /// <summary>
    /// MISD标准：启动数据采集
    /// </summary>
    private async Task StartAcquisition()
    {
        try
        {
            aiTask.Start();
            Console.WriteLine(""数据采集已启动"");
            
            // 等待数据缓冲区准备就绪
            await Task.Delay(100);
        }
        catch (Exception ex)
        {
            throw new Exception($""启动数据采集失败: {ex.Message}"", ex);
        }
    }
    
    /// <summary>
    /// MISD标准：采集振动数据
    /// </summary>
    private async Task<double[]> AcquireVibrationData()
    {
        try
        {
            var readValue = new double[bufferSize];
            var totalData = new List<double>();
            var acquisitionTime = 2.0; // 采集2秒钟的数据
            var targetSamples = (int)(sampleRate * acquisitionTime);
            
            Console.WriteLine($""开始采集振动数据，目标样本数: {targetSamples}"");
            
            while (totalData.Count < targetSamples)
            {
                // 等待足够的数据可用
                while (aiTask.AvailableSamples < (ulong)readValue.Length)
                {
                    await Task.Delay(10);
                }
                
                // 读取数据
                aiTask.ReadData(ref readValue, readValue.Length, -1);
                totalData.AddRange(readValue);
                
                if (totalData.Count % 5000 == 0)
                {
                    Console.WriteLine($""已采集 {totalData.Count} 个样本..."");
                }
            }
            
            // 截取目标长度的数据
            var result = totalData.Take(targetSamples).ToArray();
            Console.WriteLine($""振动数据采集完成，共 {result.Length} 个样本"");
            
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($""振动数据采集失败: {ex.Message}"", ex);
        }
    }
    
    /// <summary>
    /// MISD标准：停止采集并清理资源
    /// </summary>
    private async Task StopAndCleanup()
    {
        try
        {
            if (aiTask != null)
            {
                aiTask.Stop();
                aiTask.Channels.Clear();
                Console.WriteLine(""数据采集已停止，资源已清理"");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($""清理资源时出现警告: {ex.Message}"");
        }
    }
    
    /// <summary>
    /// 振动数据分析
    /// </summary>
    private async Task<VibrationAnalysisResult> AnalyzeVibrationData(double[] data)
    {
        try
        {
            Console.WriteLine(""开始振动数据分析..."");
            
            // 计算RMS值
            var rmsValue = Math.Sqrt(data.Sum(x => x * x) / data.Length);
            
            // 计算峰值
            var peakValue = data.Max(Math.Abs);
            
            // 执行FFT分析
            var fftResult = PerformFFTAnalysis(data);
            
            // 故障特征检测
            var faults = DetectVibrationFaults(fftResult, rmsValue, peakValue);
            
            Console.WriteLine($""振动分析完成 - RMS: {rmsValue:F3}V, Peak: {peakValue:F3}V, 故障数: {faults.Count}"");
            
            return new VibrationAnalysisResult
            {
                RmsValue = rmsValue,
                PeakValue = peakValue,
                SpectrumData = fftResult.Take(500).ToArray(), // 返回前500个频率点用于显示
                DetectedFaults = faults
            };
        }
        catch (Exception ex)
        {
            throw new Exception($""振动数据分析失败: {ex.Message}"", ex);
        }
    }
    
    private double[] PerformFFTAnalysis(double[] data)
    {
        // 简化的FFT实现（实际应用中应使用专业的FFT库）
        var fftSize = Math.Min(data.Length / 2, 2048);
        var result = new double[fftSize];
        
        for (int k = 0; k < fftSize; k++)
        {
            double real = 0, imag = 0;
            
            for (int n = 0; n < Math.Min(data.Length, 4096); n++)
            {
                var angle = -2.0 * Math.PI * k * n / data.Length;
                real += data[n] * Math.Cos(angle);
                imag += data[n] * Math.Sin(angle);
            }
            
            result[k] = Math.Sqrt(real * real + imag * imag) / data.Length;
        }
        
        return result;
    }
    
    private List<string> DetectVibrationFaults(double[] spectrum, double rmsValue, double peakValue)
    {
        var faults = new List<string>();
        var frequencyResolution = sampleRate / (2.0 * spectrum.Length);
        
        // 检测高RMS值
        if (rmsValue > 2.0)
        {
            faults.Add($""RMS值过高: {rmsValue:F3}V (正常范围: <2.0V)"");
        }
        
        // 检测高峰值
        if (peakValue > 5.0)
        {
            faults.Add($""峰值过高: {peakValue:F3}V (正常范围: <5.0V)"");
        }
        
        // 检测频域异常
        var maxSpectrum = spectrum.Max();
        var avgSpectrum = spectrum.Average();
        var threshold = avgSpectrum + 3 * Math.Sqrt(spectrum.Sum(x => Math.Pow(x - avgSpectrum, 2)) / spectrum.Length);
        
        for (int i = 10; i < spectrum.Length - 10; i++) // 忽略DC和高频噪声
        {
            if (spectrum[i] > threshold && spectrum[i] > maxSpectrum * 0.1)
            {
                var frequency = i * frequencyResolution;
                faults.Add($""频率 {frequency:F1} Hz 处检测到异常峰值: {spectrum[i]:F6}V"");
            }
        }
        
        return faults;
    }
    
    public class VibrationAnalysisResult
    {
        public double RmsValue { get; set; }
        public double PeakValue { get; set; }
        public double[] SpectrumData { get; set; }
        public List<string> DetectedFaults { get; set; }
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

        #region AI API Response Models

        private class DeepSeekResponse
        {
            public DeepSeekChoice[]? choices { get; set; }
        }

        private class DeepSeekChoice
        {
            public DeepSeekMessage message { get; set; } = new();
        }

        private class DeepSeekMessage
        {
            public string content { get; set; } = string.Empty;
        }

        private class ClaudeResponse
        {
            public ClaudeContent[]? content { get; set; }
        }

        private class ClaudeContent
        {
            public string text { get; set; } = string.Empty;
        }

        #endregion
    }
}
