using System;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using RestSharp;

namespace USB1601Service.Services
{
    /// <summary>
    /// 百度AI服务集成
    /// </summary>
    public class BaiduAIService
    {
        private readonly ILogger<BaiduAIService> _logger;
        private readonly IConfiguration _configuration;
        private string? _accessToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        // API配置
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _tokenUrl = "https://aip.baidubce.com/oauth/2.0/token";
        private readonly string _chatUrl = "https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/ernie-4.0-8k";

        public BaiduAIService(ILogger<BaiduAIService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _apiKey = _configuration["BaiduAI:ApiKey"] ?? "";
            _secretKey = _configuration["BaiduAI:SecretKey"] ?? "";
        }

        /// <summary>
        /// 获取访问令牌
        /// </summary>
        private async Task<string?> GetAccessTokenAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.Now < _tokenExpiry)
                {
                    return _accessToken;
                }

                var client = new RestClient(_tokenUrl);
                var request = new RestRequest();
                request.AddParameter("grant_type", "client_credentials");
                request.AddParameter("client_id", _apiKey);
                request.AddParameter("client_secret", _secretKey);

                var response = await client.PostAsync(request);
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(response.Content);
                    if (tokenResponse != null)
                    {
                        _accessToken = tokenResponse.access_token;
                        _tokenExpiry = DateTime.Now.AddSeconds(tokenResponse.expires_in - 300); // 提前5分钟刷新
                        _logger.LogInformation("成功获取百度AI访问令牌");
                        return _accessToken;
                    }
                }

                _logger.LogError("获取百度AI访问令牌失败");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取访问令牌异常");
                return null;
            }
        }

        /// <summary>
        /// 分析波形数据
        /// </summary>
        public async Task<WaveformAnalysisResult> AnalyzeWaveformAsync(double[] data, double sampleRate, int channelCount)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new WaveformAnalysisResult { Success = false, Message = "无法获取API访问令牌" };
                }

                // 计算数据特征
                var features = CalculateFeatures(data);
                
                // 构建提示词
                var prompt = $@"请分析以下波形数据特征：
采样率: {sampleRate}Hz
通道数: {channelCount}
数据点数: {data.Length}
最大值: {features.Max:F3}V
最小值: {features.Min:F3}V
平均值: {features.Mean:F3}V
标准差: {features.StdDev:F3}
峰峰值: {features.PeakToPeak:F3}V
主频率: {features.DominantFrequency:F1}Hz

请完成以下分析：
1. 识别信号类型（正弦波、方波、三角波、噪声等）
2. 判断信号质量和是否存在异常
3. 估算信号的频率成分
4. 给出可能的应用场景或测试建议";

                // 调用文心一言API
                var client = new RestClient($"{_chatUrl}?access_token={token}");
                var request = new RestRequest();
                request.AddJsonBody(new
                {
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    top_p = 0.8,
                    penalty_score = 1.0
                });

                var response = await client.PostAsync(request);
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var aiResponse = JsonConvert.DeserializeObject<ChatResponse>(response.Content);
                    if (aiResponse != null && !string.IsNullOrEmpty(aiResponse.result))
                    {
                        return new WaveformAnalysisResult
                        {
                            Success = true,
                            SignalType = ExtractSignalType(aiResponse.result),
                            Analysis = aiResponse.result,
                            Features = features,
                            Timestamp = DateTime.Now
                        };
                    }
                }

                return new WaveformAnalysisResult { Success = false, Message = "AI分析失败" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "波形分析异常");
                return new WaveformAnalysisResult { Success = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// 异常检测
        /// </summary>
        public async Task<AnomalyDetectionResult> DetectAnomaliesAsync(double[] currentData, double[] baselineData)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new AnomalyDetectionResult { Success = false };
                }

                var currentFeatures = CalculateFeatures(currentData);
                var baselineFeatures = CalculateFeatures(baselineData);

                var prompt = $@"请进行异常检测分析：

基准数据特征：
- 平均值: {baselineFeatures.Mean:F3}V
- 标准差: {baselineFeatures.StdDev:F3}
- 峰峰值: {baselineFeatures.PeakToPeak:F3}V

当前数据特征：
- 平均值: {currentFeatures.Mean:F3}V
- 标准差: {currentFeatures.StdDev:F3}
- 峰峰值: {currentFeatures.PeakToPeak:F3}V

请判断：
1. 是否存在异常（严重/中等/轻微/正常）
2. 异常的可能原因
3. 建议采取的措施";

                var client = new RestClient($"{_chatUrl}?access_token={token}");
                var request = new RestRequest();
                request.AddJsonBody(new
                {
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                });

                var response = await client.PostAsync(request);
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var aiResponse = JsonConvert.DeserializeObject<ChatResponse>(response.Content);
                    if (aiResponse != null)
                    {
                        var hasAnomaly = aiResponse.result?.Contains("异常") ?? false;
                        var severity = ExtractSeverity(aiResponse.result ?? "");
                        
                        return new AnomalyDetectionResult
                        {
                            Success = true,
                            HasAnomaly = hasAnomaly,
                            Severity = severity,
                            Description = aiResponse.result ?? "",
                            Timestamp = DateTime.Now
                        };
                    }
                }

                return new AnomalyDetectionResult { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "异常检测失败");
                return new AnomalyDetectionResult { Success = false };
            }
        }

        /// <summary>
        /// 生成测试报告
        /// </summary>
        public async Task<TestReport> GenerateReportAsync(TestSession session)
        {
            try
            {
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return new TestReport { Success = false };
                }

                var prompt = $@"请生成专业的测试报告：

测试信息：
- 测试时间: {session.StartTime:yyyy-MM-dd HH:mm:ss} 至 {session.EndTime:yyyy-MM-dd HH:mm:ss}
- 测试时长: {(session.EndTime - session.StartTime).TotalMinutes:F1}分钟
- 采样率: {session.SampleRate}Hz
- 通道数: {session.ChannelCount}
- 数据点总数: {session.TotalSamples}

数据统计：
- 最大值: {session.MaxValue:F3}V
- 最小值: {session.MinValue:F3}V
- 平均值: {session.MeanValue:F3}V
- 标准差: {session.StdDev:F3}

请生成包含以下内容的报告：
1. 测试概述
2. 数据分析结果
3. 关键发现
4. 结论与建议

要求格式规范、内容专业。";

                var client = new RestClient($"{_chatUrl}?access_token={token}");
                var request = new RestRequest();
                request.AddJsonBody(new
                {
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                });

                var response = await client.PostAsync(request);
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var aiResponse = JsonConvert.DeserializeObject<ChatResponse>(response.Content);
                    if (aiResponse != null)
                    {
                        return new TestReport
                        {
                            Success = true,
                            Content = aiResponse.result ?? "",
                            GeneratedAt = DateTime.Now,
                            Session = session
                        };
                    }
                }

                return new TestReport { Success = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成报告失败");
                return new TestReport { Success = false };
            }
        }

        private WaveformFeatures CalculateFeatures(double[] data)
        {
            var features = new WaveformFeatures
            {
                Max = data.Max(),
                Min = data.Min(),
                Mean = data.Average(),
                PeakToPeak = data.Max() - data.Min()
            };

            // 计算标准差
            var variance = data.Select(x => Math.Pow(x - features.Mean, 2)).Average();
            features.StdDev = Math.Sqrt(variance);

            // 简单的频率估算（过零点检测）
            int zeroCrossings = 0;
            for (int i = 1; i < data.Length; i++)
            {
                if ((data[i - 1] < 0 && data[i] >= 0) || (data[i - 1] >= 0 && data[i] < 0))
                {
                    zeroCrossings++;
                }
            }
            features.DominantFrequency = zeroCrossings / 2.0; // 简化的频率估算

            return features;
        }

        private string ExtractSignalType(string analysis)
        {
            if (analysis.Contains("正弦") || analysis.Contains("sine"))
                return "正弦波";
            if (analysis.Contains("方波") || analysis.Contains("square"))
                return "方波";
            if (analysis.Contains("三角") || analysis.Contains("triangle"))
                return "三角波";
            if (analysis.Contains("噪声") || analysis.Contains("noise"))
                return "噪声";
            return "未知";
        }

        private string ExtractSeverity(string analysis)
        {
            if (analysis.Contains("严重"))
                return "严重";
            if (analysis.Contains("中等"))
                return "中等";
            if (analysis.Contains("轻微"))
                return "轻微";
            return "正常";
        }
    }

    // 数据模型
    public class TokenResponse
    {
        public string access_token { get; set; } = "";
        public int expires_in { get; set; }
    }

    public class ChatResponse
    {
        public string result { get; set; } = "";
        public bool need_clear_history { get; set; }
        public int usage { get; set; }
    }

    public class WaveformFeatures
    {
        public double Max { get; set; }
        public double Min { get; set; }
        public double Mean { get; set; }
        public double StdDev { get; set; }
        public double PeakToPeak { get; set; }
        public double DominantFrequency { get; set; }
    }

    public class WaveformAnalysisResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string SignalType { get; set; } = "";
        public string Analysis { get; set; } = "";
        public WaveformFeatures? Features { get; set; }
        public DateTime Timestamp { get; set; }
        public double Frequency { get; set; }
        public string Quality { get; set; } = "";
        public string ProcessingLevel { get; set; } = "";
    }

    public class AnomalyDetectionResult
    {
        public bool Success { get; set; }
        public bool HasAnomaly { get; set; }
        public string Severity { get; set; } = "正常";
        public string Description { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }

    public class TestSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double SampleRate { get; set; }
        public int ChannelCount { get; set; }
        public long TotalSamples { get; set; }
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
        public double MeanValue { get; set; }
        public double StdDev { get; set; }
        public double VoltageRange { get; set; } = 10.0;
        public double DataComplexity { get; set; } = 0.5;
        public List<AnomalyEvent> AnomalyEvents { get; set; } = new();
    }
    
    public class AnomalyEvent
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = "";
    }

    public class TestReport
    {
        public bool Success { get; set; }
        public string Content { get; set; } = "";
        public DateTime GeneratedAt { get; set; }
        public TestSession? Session { get; set; }
    }
}