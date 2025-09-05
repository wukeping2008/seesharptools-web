using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace USB1601Service.Services
{
    /// <summary>
    /// 百度AI服务V2 - 分层处理架构
    /// 使用免费的ERNIE Speed/Lite模型为主，按需使用高级模型
    /// </summary>
    public class BaiduAIServiceV2
    {
        private readonly ILogger<BaiduAIServiceV2> _logger;
        private readonly IConfiguration _configuration;
        private string? _accessToken;
        private DateTime _tokenExpiry = DateTime.MinValue;
        
        // API配置
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _tokenUrl = "https://aip.baidubce.com/oauth/2.0/token";
        
        // 模型端点配置
        private readonly Dictionary<ModelType, ModelConfig> _modelConfigs = new()
        {
            [ModelType.Tiny] = new ModelConfig 
            { 
                Endpoint = "ernie-tiny-8k",
                MaxTokens = 100,
                Temperature = 0.1f,
                Cost = 0, // 免费
                ResponseTime = 100 // ms
            },
            [ModelType.Lite] = new ModelConfig 
            { 
                Endpoint = "ernie-lite-8k",
                MaxTokens = 500,
                Temperature = 0.3f,
                Cost = 0, // 免费
                ResponseTime = 300
            },
            [ModelType.Speed] = new ModelConfig 
            { 
                Endpoint = "ernie-speed-128k",
                MaxTokens = 2000,
                Temperature = 0.5f,
                Cost = 0, // 免费
                ResponseTime = 1000
            },
            [ModelType.Ernie35] = new ModelConfig 
            { 
                Endpoint = "completions",
                MaxTokens = 4000,
                Temperature = 0.7f,
                Cost = 0.012f, // 元/千tokens
                ResponseTime = 3000
            },
            [ModelType.Ernie45TurboVL] = new ModelConfig 
            { 
                Endpoint = "ernie-4.5-turbo-vl",  // 最新的 ERNIE-4.5-turbo-vl 模型
                MaxTokens = 8000,
                Temperature = 0.8f,
                Cost = 0.015f, // 元/千tokens (估算)
                ResponseTime = 2000,
                UseV2API = true  // 使用新的 v2 API
            }
        };
        
        // 速率限制
        private readonly ConcurrentDictionary<ModelType, RateLimiter> _rateLimiters = new();
        
        // 数据缓冲区
        private readonly ConcurrentQueue<AnalysisTask> _analysisQueue = new();
        private readonly Timer _batchProcessor;

        public BaiduAIServiceV2(ILogger<BaiduAIServiceV2> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _apiKey = _configuration["BaiduAI:ApiKey"] ?? "";
            _secretKey = _configuration["BaiduAI:SecretKey"] ?? "";
            
            // 初始化速率限制器
            InitializeRateLimiters();
            
            // 启动批处理器
            _batchProcessor = new Timer(ProcessBatch, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// L1级处理 - 实时数据验证（ERNIE Tiny）
        /// </summary>
        public async Task<DataValidationResult> ValidateDataPoint(double value, double min, double max)
        {
            try
            {
                // 简单规则判断，避免不必要的API调用
                if (value >= min && value <= max)
                {
                    return new DataValidationResult 
                    { 
                        IsValid = true, 
                        Status = "正常",
                        Confidence = 1.0 
                    };
                }
                
                // 异常值才调用AI分析
                var prompt = $"数据点:{value:F3}V,正常范围:[{min:F1},{max:F1}]V。判断:正常/警告/危险";
                
                var response = await CallModelWithRateLimit(ModelType.Tiny, prompt);
                
                return ParseValidationResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据验证失败");
                return new DataValidationResult { IsValid = false, Status = "错误" };
            }
        }

        /// <summary>
        /// L2级处理 - 波形快速分析（ERNIE Lite）
        /// </summary>
        public async Task<WaveformAnalysisResult> AnalyzeWaveformQuick(double[] data, double sampleRate)
        {
            try
            {
                // 数据降采样，减少token使用
                var sampledData = DownsampleData(data, 100);
                var features = CalculateBasicFeatures(sampledData);
                
                var prompt = $@"快速分析波形:
采样率:{sampleRate}Hz
数据点数:{sampledData.Length}
最大值:{features.Max:F3}V
最小值:{features.Min:F3}V
均值:{features.Mean:F3}V
标准差:{features.StdDev:F3}

识别:1)信号类型(正弦/方波/三角/噪声) 2)估计频率 3)信号质量(优/良/差)";

                var response = await CallModelWithRateLimit(ModelType.Lite, prompt);
                
                return new WaveformAnalysisResult
                {
                    Success = true,
                    SignalType = ExtractSignalType(response),
                    Frequency = ExtractFrequency(response),
                    Quality = ExtractQuality(response),
                    Analysis = response,
                    ProcessingLevel = "L2-快速分析",
                    Features = features
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "波形快速分析失败");
                return new WaveformAnalysisResult { Success = false };
            }
        }

        /// <summary>
        /// L3级处理 - 深度趋势分析（ERNIE Speed）
        /// </summary>
        public async Task<TrendAnalysisResult> AnalyzeTrend(DataSegment segment)
        {
            try
            {
                // 计算趋势特征
                var trendFeatures = CalculateTrendFeatures(segment);
                
                var prompt = $@"深度趋势分析:
时间窗口:{segment.Duration}秒
数据点数:{segment.DataPoints.Count}
趋势特征:
- 线性斜率:{trendFeatures.Slope:F4}
- 周期性:{trendFeatures.Periodicity}
- 稳定性:{trendFeatures.Stability}
- 异常点数:{trendFeatures.AnomalyCount}

历史模式:{string.Join(",", segment.HistoricalPatterns)}

分析要求:
1. 判断信号趋势(上升/下降/平稳/周期性)
2. 预测未来5分钟的可能变化
3. 识别潜在风险点
4. 给出操作建议";

                var response = await CallModelWithRateLimit(ModelType.Speed, prompt);
                
                return new TrendAnalysisResult
                {
                    Success = true,
                    Trend = ExtractTrend(response),
                    Prediction = ExtractPrediction(response),
                    Risks = ExtractRisks(response),
                    Recommendations = ExtractRecommendations(response),
                    ProcessingLevel = "L3-深度分析",
                    Confidence = CalculateConfidence(trendFeatures)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "趋势分析失败");
                return new TrendAnalysisResult { Success = false };
            }
        }

        /// <summary>
        /// L4级处理 - 专业报告生成（优先使用 ERNIE-4.5-turbo-vl）
        /// </summary>
        public async Task<ProfessionalReport> GenerateProfessionalReport(TestSession session)
        {
            try
            {
                // 先尝试使用免费的Speed模型
                if (session.DataComplexity < 0.7) // 复杂度不高时
                {
                    return await GenerateReportWithSpeed(session);
                }
                
                // 复杂场景使用最新的 ERNIE-4.5-turbo-vl 模型
                var prompt = BuildProfessionalReportPrompt(session);
                var response = await CallModelWithRateLimit(ModelType.Ernie45TurboVL, prompt);
                
                return new ProfessionalReport
                {
                    Success = true,
                    Title = $"USB-1601数据采集测试报告 - {session.SessionId}",
                    Content = response,
                    GeneratedAt = DateTime.Now,
                    ProcessingLevel = "L4-专业报告",
                    ModelUsed = "ERNIE-4.5-turbo-vl",
                    EstimatedCost = CalculateCost(prompt.Length + response.Length, ModelType.Ernie45TurboVL)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "使用 ERNIE-4.5 生成报告失败，尝试降级到 ERNIE-3.5");
                
                try
                {
                    // 降级到 ERNIE-3.5
                    var prompt = BuildProfessionalReportPrompt(session);
                    var response = await CallModelWithRateLimit(ModelType.Ernie35, prompt);
                    
                    return new ProfessionalReport
                    {
                        Success = true,
                        Title = $"USB-1601数据采集测试报告 - {session.SessionId}",
                        Content = response,
                        GeneratedAt = DateTime.Now,
                        ProcessingLevel = "L4-专业报告",
                        ModelUsed = "ERNIE 3.5",
                        EstimatedCost = CalculateCost(prompt.Length + response.Length, ModelType.Ernie35)
                    };
                }
                catch
                {
                    // 最终降级到Speed模型
                    return await GenerateReportWithSpeed(session);
                }
            }
        }
        
        /// <summary>
        /// L5级处理 - 高级视觉分析（使用 ERNIE-4.5-turbo-vl 的多模态能力）
        /// </summary>
        public async Task<VisualAnalysisResult> AnalyzeWaveformVisual(double[] data, double sampleRate, string? base64Image = null)
        {
            try
            {
                var prompt = $@"请对以下波形数据进行深度视觉分析：
                
采样率: {sampleRate}Hz
数据点数: {data.Length}
数据范围: [{data.Min():F3}, {data.Max():F3}]V
                
分析要求：
1. 识别波形的视觉特征（形状、周期性、对称性）
2. 检测异常模式和噪声特征
3. 评估信号质量和稳定性
4. 提供可视化改进建议";

                if (!string.IsNullOrEmpty(base64Image))
                {
                    prompt += $"\n\n附加波形图像已提供，请结合图像进行分析。";
                }
                
                var response = await CallModelWithRateLimit(ModelType.Ernie45TurboVL, prompt);
                
                return new VisualAnalysisResult
                {
                    Success = true,
                    Analysis = response,
                    ProcessingLevel = "L5-视觉分析",
                    ModelUsed = "ERNIE-4.5-turbo-vl",
                    Timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "视觉分析失败");
                return new VisualAnalysisResult { Success = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// 使用免费Speed模型生成报告
        /// </summary>
        private async Task<ProfessionalReport> GenerateReportWithSpeed(TestSession session)
        {
            var prompt = $@"生成测试报告摘要:
测试时间:{session.StartTime:yyyy-MM-dd HH:mm:ss}
持续时长:{(session.EndTime - session.StartTime).TotalMinutes:F1}分钟
采样参数:率{session.SampleRate}Hz,通道{session.ChannelCount}
数据统计:最大{session.MaxValue:F3}V,最小{session.MinValue:F3}V,均值{session.MeanValue:F3}V
异常事件:{session.AnomalyEvents.Count}次

生成包含:1)测试概况 2)数据分析 3)问题发现 4)改进建议";

            var response = await CallModelWithRateLimit(ModelType.Speed, prompt);
            
            return new ProfessionalReport
            {
                Success = true,
                Title = $"测试报告 - {session.SessionId}",
                Content = response,
                GeneratedAt = DateTime.Now,
                ProcessingLevel = "L3-标准报告",
                ModelUsed = "ERNIE Speed (免费)",
                EstimatedCost = 0
            };
        }

        /// <summary>
        /// 智能模型选择
        /// </summary>
        private ModelType SelectOptimalModel(AnalysisContext context)
        {
            // 根据场景自动选择最优模型
            if (context.RequiredResponseTime < 200)
                return ModelType.Tiny;
            
            if (context.DataComplexity < 0.3 && context.RequiredResponseTime < 500)
                return ModelType.Lite;
            
            if (context.DataComplexity < 0.7 || context.Budget == 0)
                return ModelType.Speed;
            
            return ModelType.Ernie35;
        }

        /// <summary>
        /// 带速率限制的模型调用
        /// </summary>
        private async Task<string> CallModelWithRateLimit(ModelType model, string prompt)
        {
            // 检查速率限制
            if (!_rateLimiters[model].TryAcquire())
            {
                _logger.LogWarning($"模型{model}达到速率限制，降级处理");
                
                // 自动降级
                if (model == ModelType.Ernie35)
                    return await CallModelWithRateLimit(ModelType.Speed, prompt);
                if (model == ModelType.Speed)
                    return await CallModelWithRateLimit(ModelType.Lite, prompt);
                if (model == ModelType.Lite)
                    return await CallModelWithRateLimit(ModelType.Tiny, prompt);
                    
                throw new RateLimitException($"模型{model}速率限制");
            }
            
            return await CallBaiduAPI(model, prompt);
        }

        /// <summary>
        /// 调用百度API
        /// </summary>
        private async Task<string> CallBaiduAPI(ModelType model, string prompt)
        {
            var config = _modelConfigs[model];
            
            // 根据模型选择使用 v1 或 v2 API
            if (config.UseV2API)
            {
                return await CallBaiduV2API(model, prompt);
            }
            
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException("无法获取访问令牌");
            
            var url = $"https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/{config.Endpoint}?access_token={token}";
            
            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddJsonBody(new
            {
                messages = new[] { new { role = "user", content = prompt } },
                temperature = config.Temperature,
                max_output_tokens = config.MaxTokens
            });
            
            var response = await client.PostAsync(request);
            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                var result = JsonConvert.DeserializeObject<ChatResponse>(response.Content);
                return result?.result ?? "";
            }
            
            throw new Exception($"API调用失败: {response.StatusCode}");
        }
        
        /// <summary>
        /// 调用百度 v2 API (支持 ERNIE-4.5-turbo-vl)
        /// </summary>
        private async Task<string> CallBaiduV2API(ModelType model, string prompt)
        {
            var config = _modelConfigs[model];
            var bearerToken = _configuration["BaiduAI:BearerToken"] ?? "";
            
            if (string.IsNullOrEmpty(bearerToken))
            {
                _logger.LogWarning("未配置 Bearer Token，尝试使用 Access Token");
                var accessToken = await GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    bearerToken = accessToken;
                }
                else
                {
                    throw new InvalidOperationException("无法获取认证令牌");
                }
            }
            
            var url = "https://qianfan.baidubce.com/v2/chat/completions";
            
            var client = new RestClient(url);
            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", $"Bearer {bearerToken}");
            
            request.AddJsonBody(new
            {
                model = config.Endpoint,
                messages = new[] 
                { 
                    new 
                    { 
                        role = "user", 
                        content = prompt 
                    } 
                },
                temperature = config.Temperature,
                top_p = 0.2,
                penalty_score = 1,
                max_tokens = config.MaxTokens
            });
            
            var response = await client.PostAsync(request);
            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                var result = JsonConvert.DeserializeObject<V2ChatResponse>(response.Content);
                return result?.choices?.FirstOrDefault()?.message?.content ?? "";
            }
            
            _logger.LogError($"V2 API调用失败: {response.StatusCode}, Content: {response.Content}");
            throw new Exception($"V2 API调用失败: {response.StatusCode}");
        }

        /// <summary>
        /// 获取访问令牌
        /// </summary>
        private async Task<string?> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.Now < _tokenExpiry)
                return _accessToken;
            
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
                    _tokenExpiry = DateTime.Now.AddSeconds(tokenResponse.expires_in - 300);
                    _logger.LogInformation("成功获取百度AI访问令牌");
                    return _accessToken;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 批处理任务
        /// </summary>
        private async void ProcessBatch(object? state)
        {
            var tasks = new List<AnalysisTask>();
            while (_analysisQueue.TryDequeue(out var task) && tasks.Count < 10)
            {
                tasks.Add(task);
            }
            
            if (tasks.Count > 0)
            {
                // 批量处理，提高效率
                await ProcessBatchTasks(tasks);
            }
        }

        private async Task ProcessBatchTasks(List<AnalysisTask> tasks)
        {
            // 按优先级和模型类型分组
            var grouped = tasks.GroupBy(t => t.Model);
            
            foreach (var group in grouped)
            {
                // 并行处理同类型任务
                await Task.WhenAll(group.Select(task => ProcessSingleTask(task)));
            }
        }

        private async Task ProcessSingleTask(AnalysisTask task)
        {
            try
            {
                var result = await CallModelWithRateLimit(task.Model, task.Prompt);
                task.CompletionSource.SetResult(result);
            }
            catch (Exception ex)
            {
                task.CompletionSource.SetException(ex);
            }
        }

        /// <summary>
        /// 初始化速率限制器
        /// </summary>
        private void InitializeRateLimiters()
        {
            _rateLimiters[ModelType.Tiny] = new RateLimiter(1000, TimeSpan.FromMinutes(1));
            _rateLimiters[ModelType.Lite] = new RateLimiter(100, TimeSpan.FromMinutes(1));
            _rateLimiters[ModelType.Speed] = new RateLimiter(10, TimeSpan.FromMinutes(1));
            _rateLimiters[ModelType.Ernie35] = new RateLimiter(1, TimeSpan.FromMinutes(5));
            _rateLimiters[ModelType.Ernie45TurboVL] = new RateLimiter(5, TimeSpan.FromMinutes(1));  // ERNIE-4.5 的速率限制
        }

        /// <summary>
        /// 数据降采样
        /// </summary>
        private double[] DownsampleData(double[] data, int targetSize)
        {
            if (data.Length <= targetSize)
                return data;
            
            var step = data.Length / targetSize;
            var result = new double[targetSize];
            
            for (int i = 0; i < targetSize; i++)
            {
                result[i] = data[i * step];
            }
            
            return result;
        }

        /// <summary>
        /// 计算基础特征
        /// </summary>
        private WaveformFeatures CalculateBasicFeatures(double[] data)
        {
            return new WaveformFeatures
            {
                Max = data.Max(),
                Min = data.Min(),
                Mean = data.Average(),
                StdDev = Math.Sqrt(data.Select(x => Math.Pow(x - data.Average(), 2)).Average()),
                PeakToPeak = data.Max() - data.Min()
            };
        }

        /// <summary>
        /// 计算趋势特征
        /// </summary>
        private TrendFeatures CalculateTrendFeatures(DataSegment segment)
        {
            // 实现趋势特征计算逻辑
            return new TrendFeatures
            {
                Slope = CalculateSlope(segment.DataPoints),
                Periodicity = DetectPeriodicity(segment.DataPoints),
                Stability = CalculateStability(segment.DataPoints),
                AnomalyCount = CountAnomalies(segment.DataPoints)
            };
        }

        private double CalculateSlope(List<double> data)
        {
            if (data.Count < 2) return 0;
            
            var n = data.Count;
            var sumX = Enumerable.Range(0, n).Sum();
            var sumY = data.Sum();
            var sumXY = data.Select((y, x) => x * y).Sum();
            var sumX2 = Enumerable.Range(0, n).Select(x => x * x).Sum();
            
            return (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        }

        private string DetectPeriodicity(List<double> data)
        {
            // 简化的周期性检测
            return "无明显周期";
        }

        private string CalculateStability(List<double> data)
        {
            var stdDev = Math.Sqrt(data.Select(x => Math.Pow(x - data.Average(), 2)).Average());
            if (stdDev < 0.1) return "稳定";
            if (stdDev < 0.5) return "较稳定";
            return "不稳定";
        }

        private int CountAnomalies(List<double> data)
        {
            var mean = data.Average();
            var stdDev = Math.Sqrt(data.Select(x => Math.Pow(x - mean, 2)).Average());
            return data.Count(x => Math.Abs(x - mean) > 3 * stdDev);
        }

        private double CalculateConfidence(TrendFeatures features)
        {
            // 基于特征计算置信度
            return 0.85;
        }

        private double CalculateCost(int tokens, ModelType model)
        {
            var config = _modelConfigs[model];
            return (tokens / 1000.0) * config.Cost;
        }

        private string BuildProfessionalReportPrompt(TestSession session)
        {
            return $@"生成专业的USB-1601数据采集测试报告:

【测试信息】
- 测试ID: {session.SessionId}
- 测试时间: {session.StartTime:yyyy-MM-dd HH:mm:ss} 至 {session.EndTime:yyyy-MM-dd HH:mm:ss}
- 持续时长: {(session.EndTime - session.StartTime).TotalMinutes:F1}分钟
- 设备型号: USB-1601

【采集参数】
- 采样率: {session.SampleRate}Hz
- 通道数: {session.ChannelCount}
- 电压范围: ±{session.VoltageRange}V
- 总采样点: {session.TotalSamples}

【数据统计】
- 最大值: {session.MaxValue:F3}V
- 最小值: {session.MinValue:F3}V
- 平均值: {session.MeanValue:F3}V
- 标准差: {session.StdDev:F3}
- 峰峰值: {session.MaxValue - session.MinValue:F3}V

【异常事件】
{string.Join("\n", session.AnomalyEvents.Select(e => $"- {e.Timestamp:HH:mm:ss} {e.Description}"))}

【要求】
请生成一份专业的测试报告，包含：
1. 执行摘要（测试目的、主要发现、结论）
2. 详细数据分析（信号特征、频谱分析、稳定性评估）
3. 异常分析（异常原因、影响评估、处理建议）
4. 性能评估（采集精度、数据完整性、系统稳定性）
5. 改进建议（硬件配置、参数优化、后续测试计划）

报告要求专业、准确、条理清晰，适合技术人员和管理层阅读。";
        }

        // 辅助方法
        private string ExtractSignalType(string response)
        {
            if (response.Contains("正弦")) return "正弦波";
            if (response.Contains("方波")) return "方波";
            if (response.Contains("三角")) return "三角波";
            if (response.Contains("噪声")) return "噪声";
            return "未知";
        }

        private double ExtractFrequency(string response)
        {
            // 实现频率提取逻辑
            return 0;
        }

        private string ExtractQuality(string response)
        {
            if (response.Contains("优")) return "优";
            if (response.Contains("良")) return "良";
            if (response.Contains("差")) return "差";
            return "未知";
        }

        private string ExtractTrend(string response) => "提取的趋势";
        private string ExtractPrediction(string response) => "提取的预测";
        private List<string> ExtractRisks(string response) => new List<string>();
        private List<string> ExtractRecommendations(string response) => new List<string>();

        private DataValidationResult ParseValidationResult(string response)
        {
            var result = new DataValidationResult();
            if (response.Contains("正常"))
            {
                result.IsValid = true;
                result.Status = "正常";
                result.Confidence = 0.95;
            }
            else if (response.Contains("警告"))
            {
                result.IsValid = true;
                result.Status = "警告";
                result.Confidence = 0.7;
            }
            else
            {
                result.IsValid = false;
                result.Status = "危险";
                result.Confidence = 0.9;
            }
            return result;
        }
    }

    // 数据模型
    public enum ModelType
    {
        Tiny,
        Lite,
        Speed,
        Ernie35,
        Ernie45TurboVL  // 新增 ERNIE-4.5-turbo-vl 模型
    }

    public class ModelConfig
    {
        public string Endpoint { get; set; } = "";
        public int MaxTokens { get; set; }
        public float Temperature { get; set; }
        public float Cost { get; set; }
        public int ResponseTime { get; set; }
        public bool UseV2API { get; set; } = false;  // 是否使用 v2 API
    }

    public class RateLimiter
    {
        private readonly int _maxRequests;
        private readonly TimeSpan _window;
        private readonly Queue<DateTime> _requests = new();
        private readonly object _lock = new();

        public RateLimiter(int maxRequests, TimeSpan window)
        {
            _maxRequests = maxRequests;
            _window = window;
        }

        public bool TryAcquire()
        {
            lock (_lock)
            {
                var now = DateTime.Now;
                
                // 清理过期请求
                while (_requests.Count > 0 && now - _requests.Peek() > _window)
                {
                    _requests.Dequeue();
                }
                
                if (_requests.Count < _maxRequests)
                {
                    _requests.Enqueue(now);
                    return true;
                }
                
                return false;
            }
        }
    }

    public class AnalysisTask
    {
        public ModelType Model { get; set; }
        public string Prompt { get; set; } = "";
        public TaskCompletionSource<string> CompletionSource { get; set; } = new();
        public int Priority { get; set; }
    }

    public class AnalysisContext
    {
        public double DataComplexity { get; set; }
        public int RequiredResponseTime { get; set; }
        public double Budget { get; set; }
    }

    public class DataSegment
    {
        public List<double> DataPoints { get; set; } = new();
        public double Duration { get; set; }
        public List<string> HistoricalPatterns { get; set; } = new();
    }

    public class TrendFeatures
    {
        public double Slope { get; set; }
        public string Periodicity { get; set; } = "";
        public string Stability { get; set; } = "";
        public int AnomalyCount { get; set; }
    }

    public class DataValidationResult
    {
        public bool IsValid { get; set; }
        public string Status { get; set; } = "";
        public double Confidence { get; set; }
    }

    public class TrendAnalysisResult
    {
        public bool Success { get; set; }
        public string Trend { get; set; } = "";
        public string Prediction { get; set; } = "";
        public List<string> Risks { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string ProcessingLevel { get; set; } = "";
        public double Confidence { get; set; }
    }

    public class ProfessionalReport
    {
        public bool Success { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime GeneratedAt { get; set; }
        public string ProcessingLevel { get; set; } = "";
        public string ModelUsed { get; set; } = "";
        public double EstimatedCost { get; set; }
    }

    public class VisualAnalysisResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string Analysis { get; set; } = "";
        public string ProcessingLevel { get; set; } = "";
        public string ModelUsed { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
    
    public class RateLimitException : Exception
    {
        public RateLimitException(string message) : base(message) { }
    }
    
    // V2 API 响应模型
    public class V2ChatResponse
    {
        public string id { get; set; } = "";
        public string @object { get; set; } = "";
        public long created { get; set; }
        public List<V2Choice> choices { get; set; } = new();
        public V2Usage usage { get; set; } = new();
    }
    
    public class V2Choice
    {
        public int index { get; set; }
        public V2Message message { get; set; } = new();
        public string finish_reason { get; set; } = "";
    }
    
    public class V2Message
    {
        public string role { get; set; } = "";
        public string content { get; set; } = "";
    }
    
    public class V2Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }
}