using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace USB1601Service.Services
{
    /// <summary>
    /// 智能数据处理管道 - 分层AI处理架构
    /// </summary>
    public class IntelligentDataPipeline
    {
        private readonly ILogger<IntelligentDataPipeline> _logger;
        private readonly BaiduAIServiceV2 _aiService;
        private readonly USB1601Manager _hardwareManager;
        
        // 数据缓冲区（不同层级）
        private readonly ConcurrentQueue<double> _l1Buffer = new(); // 单点数据
        private readonly ConcurrentQueue<double[]> _l2Buffer = new(); // 短时窗口
        private readonly ConcurrentQueue<DataSegment> _l3Buffer = new(); // 长时段
        private readonly ConcurrentQueue<TestSession> _l4Buffer = new(); // 完整会话
        
        // 处理计数器
        private long _totalDataPoints = 0;
        private long _l1ProcessedCount = 0;
        private long _l2ProcessedCount = 0;
        private long _l3ProcessedCount = 0;
        private long _l4ProcessedCount = 0;
        
        // 处理定时器
        private readonly Timer _l2Timer;
        private readonly Timer _l3Timer;
        
        // 配置参数
        private readonly PipelineConfig _config;
        
        // 事件
        public event EventHandler<DataAlert>? AlertRaised;
        public event EventHandler<AnalysisResult>? AnalysisCompleted;

        public IntelligentDataPipeline(
            ILogger<IntelligentDataPipeline> logger,
            BaiduAIServiceV2 aiService,
            USB1601Manager hardwareManager,
            PipelineConfig config)
        {
            _logger = logger;
            _aiService = aiService;
            _hardwareManager = hardwareManager;
            _config = config;
            
            // 订阅硬件数据事件
            _hardwareManager.DataReceived += OnHardwareDataReceived;
            
            // 启动定时处理器
            _l2Timer = new Timer(ProcessL2Batch, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
            _l3Timer = new Timer(ProcessL3Batch, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// 处理原始数据输入
        /// </summary>
        private async void OnHardwareDataReceived(object? sender, DataReceivedEventArgs e)
        {
            try
            {
                // 更新统计
                Interlocked.Add(ref _totalDataPoints, e.Data.Length);
                
                // L1处理：关键值实时检测
                foreach (var value in e.Data)
                {
                    _l1Buffer.Enqueue(value);
                    
                    // 异步处理，不阻塞数据接收
                    _ = Task.Run(() => ProcessL1DataPoint(value));
                }
                
                // L2缓冲：短时窗口分析
                _l2Buffer.Enqueue(e.Data);
                
                // L3缓冲：趋势分析准备
                if (_l2Buffer.Count >= _config.L3WindowSize)
                {
                    PrepareL3Segment();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据接收处理失败");
            }
        }

        /// <summary>
        /// L1层处理 - 实时数据验证
        /// </summary>
        private async Task ProcessL1DataPoint(double value)
        {
            try
            {
                // 简单规则检测
                if (Math.Abs(value) > _config.CriticalThreshold)
                {
                    // 严重异常，立即AI分析
                    var result = await _aiService.ValidateDataPoint(
                        value, 
                        -_config.NormalRange, 
                        _config.NormalRange);
                    
                    if (!result.IsValid || result.Status == "危险")
                    {
                        RaiseAlert(new DataAlert
                        {
                            Level = AlertLevel.Critical,
                            Value = value,
                            Message = $"检测到危险数据: {value:F3}V",
                            Timestamp = DateTime.Now,
                            AIAnalysis = result.Status
                        });
                    }
                    
                    Interlocked.Increment(ref _l1ProcessedCount);
                }
                else if (Math.Abs(value) > _config.WarningThreshold)
                {
                    // 警告级别，记录但不立即处理
                    RaiseAlert(new DataAlert
                    {
                        Level = AlertLevel.Warning,
                        Value = value,
                        Message = $"数据接近警告阈值: {value:F3}V",
                        Timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"L1处理失败: {value}");
            }
        }

        /// <summary>
        /// L2层批处理 - 波形快速分析
        /// </summary>
        private async void ProcessL2Batch(object? state)
        {
            try
            {
                var batchData = new List<double>();
                
                // 收集批次数据
                while (_l2Buffer.TryDequeue(out var data) && batchData.Count < _config.L2BatchSize)
                {
                    batchData.AddRange(data);
                }
                
                if (batchData.Count >= _config.L2MinBatchSize)
                {
                    // 执行波形分析
                    var result = await _aiService.AnalyzeWaveformQuick(
                        batchData.ToArray(), 
                        _config.SampleRate);
                    
                    if (result.Success)
                    {
                        // 发布分析结果
                        AnalysisCompleted?.Invoke(this, new AnalysisResult
                        {
                            Level = AnalysisLevel.L2,
                            Result = result,
                            DataCount = batchData.Count,
                            Timestamp = DateTime.Now
                        });
                        
                        // 根据分析结果决定是否需要深度分析
                        if (result.Quality == "差" || result.SignalType == "异常")
                        {
                            // 触发L3深度分析
                            _ = Task.Run(() => TriggerL3Analysis(batchData));
                        }
                    }
                    
                    Interlocked.Add(ref _l2ProcessedCount, batchData.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L2批处理失败");
            }
        }

        /// <summary>
        /// L3层处理 - 趋势深度分析
        /// </summary>
        private async void ProcessL3Batch(object? state)
        {
            try
            {
                if (_l3Buffer.TryDequeue(out var segment))
                {
                    // 执行趋势分析
                    var result = await _aiService.AnalyzeTrend(segment);
                    
                    if (result.Success)
                    {
                        // 发布分析结果
                        AnalysisCompleted?.Invoke(this, new AnalysisResult
                        {
                            Level = AnalysisLevel.L3,
                            Result = result,
                            DataCount = segment.DataPoints.Count,
                            Timestamp = DateTime.Now
                        });
                        
                        // 检查是否需要生成报告
                        if (result.Risks.Any() || segment.DataPoints.Count > _config.L4ReportThreshold)
                        {
                            PrepareL4Report(segment, result);
                        }
                    }
                    
                    Interlocked.Add(ref _l3ProcessedCount, segment.DataPoints.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L3处理失败");
            }
        }

        /// <summary>
        /// 准备L3数据段
        /// </summary>
        private void PrepareL3Segment()
        {
            var segment = new DataSegment
            {
                DataPoints = new List<double>(),
                Duration = _config.L3WindowSize / _config.SampleRate
            };
            
            var bufferCount = Math.Min(_config.L3WindowSize, _l2Buffer.Count);
            for (int i = 0; i < bufferCount; i++)
            {
                if (_l2Buffer.TryDequeue(out var data))
                {
                    segment.DataPoints.AddRange(data);
                }
            }
            
            // 添加历史模式识别
            segment.HistoricalPatterns = IdentifyPatterns(segment.DataPoints);
            
            _l3Buffer.Enqueue(segment);
        }

        /// <summary>
        /// 触发L3深度分析
        /// </summary>
        private async Task TriggerL3Analysis(List<double> data)
        {
            var segment = new DataSegment
            {
                DataPoints = data,
                Duration = data.Count / _config.SampleRate,
                HistoricalPatterns = IdentifyPatterns(data)
            };
            
            var result = await _aiService.AnalyzeTrend(segment);
            
            if (result.Success && result.Risks.Any())
            {
                // 发现风险，提升告警级别
                foreach (var risk in result.Risks)
                {
                    RaiseAlert(new DataAlert
                    {
                        Level = AlertLevel.High,
                        Message = $"趋势分析发现风险: {risk}",
                        Timestamp = DateTime.Now,
                        AIAnalysis = result.Prediction
                    });
                }
            }
        }

        /// <summary>
        /// 准备L4报告生成
        /// </summary>
        private void PrepareL4Report(DataSegment segment, TrendAnalysisResult trendResult)
        {
            var session = new TestSession
            {
                SessionId = Guid.NewGuid().ToString(),
                StartTime = DateTime.Now.AddSeconds(-segment.Duration),
                EndTime = DateTime.Now,
                SampleRate = _config.SampleRate,
                ChannelCount = 1,
                TotalSamples = segment.DataPoints.Count,
                MaxValue = segment.DataPoints.Max(),
                MinValue = segment.DataPoints.Min(),
                MeanValue = segment.DataPoints.Average(),
                StdDev = Math.Sqrt(segment.DataPoints.Select(x => Math.Pow(x - segment.DataPoints.Average(), 2)).Average()),
                DataComplexity = CalculateComplexity(segment.DataPoints),
                AnomalyEvents = ExtractAnomalyEvents(segment, trendResult)
            };
            
            _l4Buffer.Enqueue(session);
            
            // 异步生成报告
            _ = Task.Run(() => GenerateReport(session));
        }

        /// <summary>
        /// 生成专业报告
        /// </summary>
        private async Task GenerateReport(TestSession session)
        {
            try
            {
                var report = await _aiService.GenerateProfessionalReport(session);
                
                if (report.Success)
                {
                    AnalysisCompleted?.Invoke(this, new AnalysisResult
                    {
                        Level = AnalysisLevel.L4,
                        Result = report,
                        DataCount = (int)session.TotalSamples,
                        Timestamp = DateTime.Now
                    });
                    
                    Interlocked.Add(ref _l4ProcessedCount, session.TotalSamples);
                    
                    _logger.LogInformation($"生成报告完成，使用模型: {report.ModelUsed}, 预估成本: ¥{report.EstimatedCost:F4}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "报告生成失败");
            }
        }

        /// <summary>
        /// 识别数据模式
        /// </summary>
        private List<string> IdentifyPatterns(List<double> data)
        {
            var patterns = new List<string>();
            
            // 简单模式识别
            var mean = data.Average();
            var stdDev = Math.Sqrt(data.Select(x => Math.Pow(x - mean, 2)).Average());
            
            if (stdDev < 0.1) patterns.Add("稳定");
            if (IsIncreasing(data)) patterns.Add("上升趋势");
            if (IsDecreasing(data)) patterns.Add("下降趋势");
            if (IsPeriodic(data)) patterns.Add("周期性");
            
            return patterns;
        }

        private bool IsIncreasing(List<double> data)
        {
            if (data.Count < 2) return false;
            var diffs = data.Zip(data.Skip(1), (a, b) => b - a);
            return diffs.Count(d => d > 0) > diffs.Count() * 0.7;
        }

        private bool IsDecreasing(List<double> data)
        {
            if (data.Count < 2) return false;
            var diffs = data.Zip(data.Skip(1), (a, b) => b - a);
            return diffs.Count(d => d < 0) > diffs.Count() * 0.7;
        }

        private bool IsPeriodic(List<double> data)
        {
            // 简化的周期性检测
            return false;
        }

        private double CalculateComplexity(List<double> data)
        {
            // 基于方差和变化率计算复杂度
            var variance = data.Select(x => Math.Pow(x - data.Average(), 2)).Average();
            var changes = data.Zip(data.Skip(1), (a, b) => Math.Abs(b - a)).Sum();
            return Math.Min(1.0, variance + changes / data.Count);
        }

        private List<AnomalyEvent> ExtractAnomalyEvents(DataSegment segment, TrendAnalysisResult trendResult)
        {
            var events = new List<AnomalyEvent>();
            
            // 从趋势分析结果提取异常事件
            foreach (var risk in trendResult.Risks)
            {
                events.Add(new AnomalyEvent
                {
                    Timestamp = DateTime.Now,
                    Description = risk
                });
            }
            
            return events;
        }

        /// <summary>
        /// 触发告警
        /// </summary>
        private void RaiseAlert(DataAlert alert)
        {
            AlertRaised?.Invoke(this, alert);
            _logger.LogWarning($"[{alert.Level}] {alert.Message}");
        }

        /// <summary>
        /// 获取处理统计
        /// </summary>
        public PipelineStatistics GetStatistics()
        {
            return new PipelineStatistics
            {
                TotalDataPoints = _totalDataPoints,
                L1ProcessedCount = _l1ProcessedCount,
                L2ProcessedCount = _l2ProcessedCount,
                L3ProcessedCount = _l3ProcessedCount,
                L4ProcessedCount = _l4ProcessedCount,
                L1QueueSize = _l1Buffer.Count,
                L2QueueSize = _l2Buffer.Count,
                L3QueueSize = _l3Buffer.Count,
                L4QueueSize = _l4Buffer.Count
            };
        }

        public void Dispose()
        {
            _l2Timer?.Dispose();
            _l3Timer?.Dispose();
            _hardwareManager.DataReceived -= OnHardwareDataReceived;
        }
    }

    // 配置类
    public class PipelineConfig
    {
        public double SampleRate { get; set; } = 1000;
        public double CriticalThreshold { get; set; } = 9.0;
        public double WarningThreshold { get; set; } = 7.0;
        public double NormalRange { get; set; } = 10.0;
        public int L2BatchSize { get; set; } = 1000;
        public int L2MinBatchSize { get; set; } = 100;
        public int L3WindowSize { get; set; } = 10000;
        public int L4ReportThreshold { get; set; } = 100000;
    }

    // 数据模型
    public enum AlertLevel
    {
        Info,
        Warning,
        High,
        Critical
    }

    public enum AnalysisLevel
    {
        L1,
        L2,
        L3,
        L4
    }

    public class DataAlert
    {
        public AlertLevel Level { get; set; }
        public double Value { get; set; }
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string? AIAnalysis { get; set; }
    }

    public class AnalysisResult
    {
        public AnalysisLevel Level { get; set; }
        public object Result { get; set; } = new();
        public int DataCount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class PipelineStatistics
    {
        public long TotalDataPoints { get; set; }
        public long L1ProcessedCount { get; set; }
        public long L2ProcessedCount { get; set; }
        public long L3ProcessedCount { get; set; }
        public long L4ProcessedCount { get; set; }
        public int L1QueueSize { get; set; }
        public int L2QueueSize { get; set; }
        public int L3QueueSize { get; set; }
        public int L4QueueSize { get; set; }
    }
}