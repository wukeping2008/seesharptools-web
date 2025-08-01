using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SeeSharpBackend.Services.AI.Models;

namespace SeeSharpBackend.Services.AI
{
    /// <summary>
    /// 自然语言处理器实现
    /// </summary>
    public class NaturalLanguageProcessor : INaturalLanguageProcessor
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NaturalLanguageProcessor> _logger;

        // 关键词字典
        private readonly Dictionary<string, List<string>> _keywordMappings = new()
        {
            ["振动测试"] = new() { "振动", "轴承", "电机", "故障", "检测", "振荡", "机械" },
            ["电气测试"] = new() { "信号", "电压", "电流", "功率", "THD", "谐波", "波形", "电气" },
            ["温度测量"] = new() { "温度", "热", "传感器", "监控", "温控", "热电偶", "PT100" },
            ["频谱分析"] = new() { "FFT", "频谱", "频率", "分析", "频域", "功率谱", "相干" },
            ["数据采集"] = new() { "采集", "采样", "连续", "触发", "缓冲", "实时", "流" },
            ["信号发生"] = new() { "信号发生", "波形生成", "正弦波", "方波", "锯齿波", "扫频" }
        };

        // 设备推荐映射
        private readonly Dictionary<string, string> _deviceRecommendations = new()
        {
            ["振动测试"] = "JYUSB1601",
            ["电气测试"] = "JY5500",
            ["温度测量"] = "JYUSB1601",
            ["高频信号"] = "JY5500",
            ["低频信号"] = "JYUSB1601",
            ["多通道"] = "JYUSB1601",
            ["高精度"] = "JY5500"
        };

        public NaturalLanguageProcessor(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<NaturalLanguageProcessor> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<TestRequirement> AnalyzeRequirementAsync(string userInput)
        {
            try
            {
                _logger.LogInformation("开始分析用户需求: {Input}", userInput);

                // 1. 预处理输入
                var preprocessedInput = PreprocessInput(userInput);

                // 2. 提取关键词
                var keywords = await ExtractKeywordsAsync(preprocessedInput);

                // 3. 调用AI API分析
                var aiAnalysis = await CallAIAPIAsync(preprocessedInput);

                // 4. 构建测试需求对象
                var requirement = BuildTestRequirement(userInput, preprocessedInput, keywords, aiAnalysis);

                // 5. 验证和优化
                var optimizedRequirement = await OptimizeRequirementAsync(requirement);

                _logger.LogInformation("需求分析完成，置信度: {Confidence}", optimizedRequirement.Confidence);
                return optimizedRequirement;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分析用户需求时发生错误: {Input}", userInput);
                
                // 返回基础解析结果作为后备
                return GetFallbackRequirement(userInput);
            }
        }

        public async Task<bool> ValidateRequirementAsync(TestRequirement requirement)
        {
            try
            {
                // 基本验证
                if (string.IsNullOrWhiteSpace(requirement.TestObject) ||
                    string.IsNullOrWhiteSpace(requirement.TestType) ||
                    requirement.Confidence < 0.3)
                {
                    return false;
                }

                // 设备兼容性验证
                if (!string.IsNullOrWhiteSpace(requirement.RecommendedDevice))
                {
                    var supportedDevices = new[] { "JYUSB1601", "JY5500", "通用" };
                    if (!supportedDevices.Contains(requirement.RecommendedDevice))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证需求时发生错误");
                return false;
            }
        }

        public async Task<TestRequirement> OptimizeRequirementAsync(TestRequirement requirement)
        {
            try
            {
                // 优化设备推荐
                if (string.IsNullOrWhiteSpace(requirement.RecommendedDevice))
                {
                    requirement.RecommendedDevice = RecommendDevice(requirement);
                }

                // 优化参数配置
                OptimizeParameters(requirement);

                // 优化应用领域
                OptimizeApplicationDomains(requirement);

                // 重新计算置信度
                requirement.Confidence = CalculateConfidence(requirement);

                return requirement;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "优化需求时发生错误");
                return requirement;
            }
        }

        public async Task<List<string>> ExtractKeywordsAsync(string text)
        {
            var keywords = new HashSet<string>();
            var normalizedText = text.ToLower();

            // 基于规则的关键词提取
            foreach (var category in _keywordMappings)
            {
                foreach (var keyword in category.Value)
                {
                    if (normalizedText.Contains(keyword.ToLower()))
                    {
                        keywords.Add(keyword);
                        keywords.Add(category.Key); // 添加分类
                    }
                }
            }

            // 数值和单位提取
            ExtractNumericalKeywords(text, keywords);

            return keywords.ToList();
        }

        public async Task<bool> CheckApiHealthAsync()
        {
            try
            {
                var apiKey = _configuration["VolcesDeepseek:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("未配置DeepSeek API密钥");
                    return false;
                }

                // 发送简单的健康检查请求
                var healthCheckResult = await CallAIAPIAsync("测试连接");
                return !string.IsNullOrEmpty(healthCheckResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查API健康状态时发生错误");
                return false;
            }
        }

        #region Private Methods

        private string PreprocessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // 去除多余空格和特殊字符
            var cleaned = Regex.Replace(input.Trim(), @"\s+", " ");
            
            // 标准化常见术语
            var standardizations = new Dictionary<string, string>
            {
                ["测试"] = "测试",
                ["检测"] = "检测", 
                ["分析"] = "分析",
                ["监控"] = "监控",
                ["频率"] = "频率",
                ["振动"] = "振动",
                ["温度"] = "温度",
                ["信号"] = "信号"
            };

            foreach (var pair in standardizations)
            {
                cleaned = cleaned.Replace(pair.Key, pair.Value);
            }

            return cleaned;
        }

        private async Task<string> CallAIAPIAsync(string input)
        {
            try
            {
                var apiKey = _configuration["VolcesDeepseek:ApiKey"];
                var apiUrl = _configuration["VolcesDeepseek:Url"] ?? "https://ark.cn-beijing.volces.com/api/v3/chat/completions";

                if (string.IsNullOrEmpty(apiKey))
                {
                    _logger.LogWarning("未配置DeepSeek API密钥，使用基于规则的解析");
                    return string.Empty;
                }

                var systemPrompt = @"你是一个专业的测试测量系统分析师。请分析用户的测试需求，提取关键信息并以JSON格式返回。

需要提取的信息：
- testObject: 测试对象
- testType: 测试类型
- frequencyRange: 频率范围
- analysisMethod: 分析方法
- recommendedDevice: 推荐设备(JYUSB1601/JY5500/通用)
- priority: 优先级(低/中/高)
- confidence: 识别置信度(0-1)
- applicationDomains: 应用领域数组
- complexityLevel: 复杂度(初级/中级/高级/专家)

只返回JSON，不要其他内容。";

                var requestBody = new
                {
                    model = "deepseek-r1-250528",
                    max_tokens = 1000,
                    temperature = 0.1,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = $"请分析以下测试需求：{input}" }
                    }
                };

                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("DeepSeek API调用失败: {StatusCode}", response.StatusCode);
                    return string.Empty;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObj = JsonDocument.Parse(responseContent);

                if (responseObj.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == JsonValueKind.Array &&
                    choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var messageContent))
                    {
                        return messageContent.GetString() ?? string.Empty;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用AI API时发生错误");
                return string.Empty;
            }
        }

        private TestRequirement BuildTestRequirement(string originalInput, string preprocessedInput, List<string> keywords, string aiAnalysis)
        {
            var requirement = new TestRequirement
            {
                OriginalInput = originalInput,
                Keywords = keywords,
                Timestamp = DateTime.Now
            };

            // 尝试解析AI分析结果
            if (!string.IsNullOrEmpty(aiAnalysis))
            {
                try
                {
                    ParseAIAnalysis(aiAnalysis, requirement);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "解析AI分析结果失败，使用基于规则的方法");
                }
            }

            // 基于规则的后备解析
            if (string.IsNullOrEmpty(requirement.TestType))
            {
                ParseWithRules(preprocessedInput, keywords, requirement);
            }

            return requirement;
        }

        private void ParseAIAnalysis(string aiAnalysis, TestRequirement requirement)
        {
            // 提取JSON部分
            var jsonMatch = Regex.Match(aiAnalysis, @"\{.*\}", RegexOptions.Singleline);
            if (!jsonMatch.Success) return;

            var jsonContent = jsonMatch.Value;
            var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            if (root.TryGetProperty("testObject", out var testObject))
                requirement.TestObject = testObject.GetString() ?? string.Empty;

            if (root.TryGetProperty("testType", out var testType))
                requirement.TestType = testType.GetString() ?? string.Empty;

            if (root.TryGetProperty("frequencyRange", out var frequencyRange))
                requirement.FrequencyRange = frequencyRange.GetString() ?? string.Empty;

            if (root.TryGetProperty("analysisMethod", out var analysisMethod))
                requirement.AnalysisMethod = analysisMethod.GetString() ?? string.Empty;

            if (root.TryGetProperty("recommendedDevice", out var recommendedDevice))
                requirement.RecommendedDevice = recommendedDevice.GetString() ?? string.Empty;

            if (root.TryGetProperty("priority", out var priority))
                requirement.Priority = priority.GetString() ?? "中";

            if (root.TryGetProperty("confidence", out var confidence))
                requirement.Confidence = confidence.GetDouble();

            if (root.TryGetProperty("complexityLevel", out var complexityLevel))
                requirement.ComplexityLevel = complexityLevel.GetString() ?? "中级";

            if (root.TryGetProperty("applicationDomains", out var domains))
            {
                foreach (var domain in domains.EnumerateArray())
                {
                    var domainStr = domain.GetString();
                    if (!string.IsNullOrEmpty(domainStr))
                    {
                        requirement.ApplicationDomains.Add(domainStr);
                    }
                }
            }
        }

        private void ParseWithRules(string input, List<string> keywords, TestRequirement requirement)
        {
            var lowerInput = input.ToLower();

            // 推断测试类型
            if (keywords.Any(k => _keywordMappings["振动测试"].Contains(k)))
            {
                requirement.TestType = "振动测试";
                requirement.TestObject = ExtractTestObject(input, "振动");
            }
            else if (keywords.Any(k => _keywordMappings["电气测试"].Contains(k)))
            {
                requirement.TestType = "电气测试";
                requirement.TestObject = ExtractTestObject(input, "电气");
            }
            else if (keywords.Any(k => _keywordMappings["温度测量"].Contains(k)))
            {
                requirement.TestType = "温度测量";
                requirement.TestObject = ExtractTestObject(input, "温度");
            }

            // 提取频率范围
            requirement.FrequencyRange = ExtractFrequencyRange(input);

            // 推断分析方法
            if (lowerInput.Contains("fft") || lowerInput.Contains("频谱"))
                requirement.AnalysisMethod = "FFT分析";
            else if (lowerInput.Contains("thd") || lowerInput.Contains("谐波"))
                requirement.AnalysisMethod = "THD分析";
            else if (lowerInput.Contains("统计") || lowerInput.Contains("平均"))
                requirement.AnalysisMethod = "统计分析";

            // 设置基础置信度
            requirement.Confidence = 0.7;
        }

        private string ExtractTestObject(string input, string context)
        {
            var patterns = new Dictionary<string, string[]>
            {
                ["振动"] = new[] { "轴承", "电机", "齿轮", "风机", "泵" },
                ["电气"] = new[] { "信号发生器", "功放", "电源", "变频器", "电机" },
                ["温度"] = new[] { "传感器", "环境", "设备", "炉温", "室温" }
            };

            if (patterns.TryGetValue(context, out var objects))
            {
                foreach (var obj in objects)
                {
                    if (input.Contains(obj))
                        return obj;
                }
            }

            return $"{context}相关设备";
        }

        private string ExtractFrequencyRange(string input)
        {
            // 匹配频率范围模式
            var patterns = new[]
            {
                @"(\d+)-(\d+)\s*[kK]?[hH]z",
                @"(\d+)\s*-\s*(\d+)\s*[kK]?[hH]z",
                @"(\d+(?:\.\d+)?)\s*[kK]?[hH]z\s*-\s*(\d+(?:\.\d+)?)\s*[kK]?[hH]z"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Value;
                }
            }

            return string.Empty;
        }

        private void ExtractNumericalKeywords(string text, HashSet<string> keywords)
        {
            // 提取频率相关数值
            var frequencyMatches = Regex.Matches(text, @"\d+(?:\.\d+)?\s*[kKmM]?[hH]z", RegexOptions.IgnoreCase);
            foreach (Match match in frequencyMatches)
            {
                keywords.Add(match.Value);
            }

            // 提取电压相关数值
            var voltageMatches = Regex.Matches(text, @"\d+(?:\.\d+)?\s*[vV]", RegexOptions.IgnoreCase);
            foreach (Match match in voltageMatches)
            {
                keywords.Add(match.Value);
            }

            // 提取温度相关数值
            var temperatureMatches = Regex.Matches(text, @"\d+(?:\.\d+)?\s*[°℃]?[cC]", RegexOptions.IgnoreCase);
            foreach (Match match in temperatureMatches)
            {
                keywords.Add(match.Value);
            }
        }

        private string RecommendDevice(TestRequirement requirement)
        {
            // 基于测试类型推荐设备
            if (_deviceRecommendations.TryGetValue(requirement.TestType, out var device))
            {
                return device;
            }

            // 基于频率范围推荐
            if (!string.IsNullOrEmpty(requirement.FrequencyRange))
            {
                var frequencyNumbers = Regex.Matches(requirement.FrequencyRange, @"\d+")
                    .Cast<Match>()
                    .Select(m => int.TryParse(m.Value, out var n) ? n : 0)
                    .Where(n => n > 0)
                    .ToList();

                if (frequencyNumbers.Any(f => f > 1000)) // 高频
                {
                    return "JY5500";
                }
                else if (frequencyNumbers.Any(f => f < 1000)) // 低频
                {
                    return "JYUSB1601";
                }
            }

            return "JYUSB1601"; // 默认推荐
        }

        private void OptimizeParameters(TestRequirement requirement)
        {
            if (!string.IsNullOrEmpty(requirement.FrequencyRange))
            {
                var frequencyNumbers = Regex.Matches(requirement.FrequencyRange, @"\d+")
                    .Cast<Match>()
                    .Select(m => int.TryParse(m.Value, out var n) ? n : 0)
                    .Where(n => n > 0)
                    .ToList();

                if (frequencyNumbers.Any())
                {
                    var maxFreq = frequencyNumbers.Max();
                    var recommendedSampleRate = (int)(maxFreq * 2.5); // 奈奎斯特定理 + 安全余量

                    requirement.Parameters["recommendedSampleRate"] = recommendedSampleRate;
                    requirement.Parameters["maxFrequency"] = maxFreq;
                }
            }

            // 根据测试类型设置默认参数
            switch (requirement.TestType)
            {
                case "振动测试":
                    requirement.Parameters["channelCount"] = 1;
                    requirement.Parameters["analysisWindow"] = "Hanning";
                    requirement.Parameters["fftSize"] = 4096;
                    break;

                case "电气测试":
                    requirement.Parameters["channelCount"] = 1;
                    requirement.Parameters["precision"] = "高";
                    requirement.Parameters["coupling"] = "AC";
                    break;

                case "温度测量":
                    requirement.Parameters["samplingInterval"] = "1s";
                    requirement.Parameters["averagingCount"] = 10;
                    break;
            }
        }

        private void OptimizeApplicationDomains(TestRequirement requirement)
        {
            if (!requirement.ApplicationDomains.Any())
            {
                var keywords = requirement.Keywords.Select(k => k.ToLower()).ToList();

                if (keywords.Any(k => k.Contains("汽车") || k.Contains("发动机") || k.Contains("车")))
                    requirement.ApplicationDomains.Add("汽车");

                if (keywords.Any(k => k.Contains("航空") || k.Contains("飞机") || k.Contains("航天")))
                    requirement.ApplicationDomains.Add("航空");

                if (keywords.Any(k => k.Contains("电子") || k.Contains("芯片") || k.Contains("电路")))
                    requirement.ApplicationDomains.Add("电子");

                if (keywords.Any(k => k.Contains("机械") || k.Contains("机器") || k.Contains("设备")))
                    requirement.ApplicationDomains.Add("机械");

                if (!requirement.ApplicationDomains.Any())
                    requirement.ApplicationDomains.Add("通用");
            }
        }

        private double CalculateConfidence(TestRequirement requirement)
        {
            double confidence = 0.5; // 基础置信度

            // 基于填充字段的完整性
            if (!string.IsNullOrEmpty(requirement.TestObject)) confidence += 0.1;
            if (!string.IsNullOrEmpty(requirement.TestType)) confidence += 0.2;
            if (!string.IsNullOrEmpty(requirement.FrequencyRange)) confidence += 0.1;
            if (!string.IsNullOrEmpty(requirement.AnalysisMethod)) confidence += 0.1;
            if (!string.IsNullOrEmpty(requirement.RecommendedDevice)) confidence += 0.05;

            // 基于关键词匹配度
            if (requirement.Keywords.Count > 3) confidence += 0.05;
            if (requirement.Keywords.Count > 5) confidence += 0.05;

            return Math.Min(confidence, 1.0);
        }

        private TestRequirement GetFallbackRequirement(string userInput)
        {
            return new TestRequirement
            {
                OriginalInput = userInput,
                TestObject = "未知设备",
                TestType = "数据采集",
                RecommendedDevice = "JYUSB1601",
                Priority = "中",
                Confidence = 0.3,
                ComplexityLevel = "中级",
                ApplicationDomains = new List<string> { "通用" },
                Keywords = new List<string> { "测试", "采集" }
            };
        }

        #endregion
    }
}
