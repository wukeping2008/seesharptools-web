using System.Diagnostics;
using System.Text.Json;
using SeeSharpBackend.Services.AI.Models;

namespace SeeSharpBackend.Services.AI
{
    /// <summary>
    /// 模板仓库实现
    /// 基于JSON文件的模板存储和管理
    /// </summary>
    public class TemplateRepository : ITemplateRepository
    {
        private readonly ILogger<TemplateRepository> _logger;
        private readonly string _templatesPath;
        private readonly Dictionary<string, TestTemplate> _templateCache = new();
        private readonly object _cacheLock = new();
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(30);

        public TemplateRepository(ILogger<TemplateRepository> logger, IConfiguration configuration)
        {
            _logger = logger;
            _templatesPath = configuration["TemplateLibrary:BasePath"] 
                ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
            
            // 确保模板目录存在
            Directory.CreateDirectory(_templatesPath);
            
            // 初始化内置模板
            _ = Task.Run(InitializeBuiltInTemplatesAsync);
        }

        public async Task<List<TestTemplate>> GetAllTemplatesAsync()
        {
            await EnsureCacheUpdatedAsync();
            
            lock (_cacheLock)
            {
                return _templateCache.Values.Where(t => t.IsEnabled).ToList();
            }
        }

        public async Task<TestTemplate?> GetTemplateByIdAsync(string templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId))
                return null;

            await EnsureCacheUpdatedAsync();
            
            lock (_cacheLock)
            {
                return _templateCache.TryGetValue(templateId, out var template) ? template : null;
            }
        }

        public async Task<TemplateSearchResult> SearchTemplatesAsync(TemplateQueryOptions options)
        {
            var stopwatch = Stopwatch.StartNew();
            
            await EnsureCacheUpdatedAsync();
            
            List<TestTemplate> templates;
            lock (_cacheLock)
            {
                templates = _templateCache.Values.ToList();
            }

            // 过滤条件
            if (options.OnlyEnabled)
                templates = templates.Where(t => t.IsEnabled).ToList();

            if (!string.IsNullOrWhiteSpace(options.DeviceType))
                templates = templates.Where(t => t.MatchesDevice(options.DeviceType)).ToList();

            if (!string.IsNullOrWhiteSpace(options.Category))
                templates = templates.Where(t => t.Category.Equals(options.Category, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(options.ComplexityLevel))
                templates = templates.Where(t => string.Equals(t.ComplexityLevel, options.ComplexityLevel, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(options.ApplicationDomain))
                templates = templates.Where(t => t.ApplicationDomains.Any(d => string.Equals(d, options.ApplicationDomain, StringComparison.OrdinalIgnoreCase))).ToList();

            if (options.Tags?.Any() == true)
                templates = templates.Where(t => options.Tags.Any(tag => t.Tags.Any(tTag => string.Equals(tTag, tag, StringComparison.OrdinalIgnoreCase)))).ToList();

            // 关键词搜索
            if (!string.IsNullOrWhiteSpace(options.SearchKeyword))
            {
                var keyword = options.SearchKeyword.ToLower();
                templates = templates.Where(t =>
                    t.Name.ToLower().Contains(keyword) ||
                    t.Description.ToLower().Contains(keyword) ||
                    t.Keywords.Any(k => k.ToLower().Contains(keyword)) ||
                    t.Tags.Any(tag => tag.ToLower().Contains(keyword))
                ).ToList();
            }

            // 排序
            templates = options.SortBy switch
            {
                TemplateSortBy.Name => options.SortDirection == SortDirection.Ascending 
                    ? templates.OrderBy(t => t.Name).ToList()
                    : templates.OrderByDescending(t => t.Name).ToList(),
                TemplateSortBy.CreatedAt => options.SortDirection == SortDirection.Ascending
                    ? templates.OrderBy(t => t.CreatedAt).ToList()
                    : templates.OrderByDescending(t => t.CreatedAt).ToList(),
                TemplateSortBy.LastUpdated => options.SortDirection == SortDirection.Ascending
                    ? templates.OrderBy(t => t.LastUpdated).ToList()
                    : templates.OrderByDescending(t => t.LastUpdated).ToList(),
                TemplateSortBy.UsageCount => options.SortDirection == SortDirection.Ascending
                    ? templates.OrderBy(t => t.UsageCount).ToList()
                    : templates.OrderByDescending(t => t.UsageCount).ToList(),
                TemplateSortBy.Rating => options.SortDirection == SortDirection.Ascending
                    ? templates.OrderBy(t => t.Rating).ToList()
                    : templates.OrderByDescending(t => t.Rating).ToList(),
                _ => templates.OrderByDescending(t => t.Rating).ToList()
            };

            // 分页
            var totalCount = templates.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / options.PageSize);
            var skip = (options.Page - 1) * options.PageSize;
            var pagedTemplates = templates.Skip(skip).Take(options.PageSize).ToList();

            stopwatch.Stop();

            return new TemplateSearchResult
            {
                Templates = pagedTemplates,
                TotalCount = totalCount,
                CurrentPage = options.Page,
                PageSize = options.PageSize,
                TotalPages = totalPages,
                QueryTime = stopwatch.ElapsedMilliseconds
            };
        }

        public async Task<List<TestTemplate>> GetTemplatesByDeviceAsync(string deviceType)
        {
            await EnsureCacheUpdatedAsync();
            
            lock (_cacheLock)
            {
                return _templateCache.Values
                    .Where(t => t.IsEnabled && t.MatchesDevice(deviceType))
                    .ToList();
            }
        }

        public async Task<List<TestTemplate>> GetTemplatesByCategoryAsync(string category)
        {
            await EnsureCacheUpdatedAsync();
            
            lock (_cacheLock)
            {
                return _templateCache.Values
                    .Where(t => t.IsEnabled && string.Equals(t.Category, category, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        public async Task<List<TestTemplate>> SearchByKeywordsAsync(List<string> keywords)
        {
            if (!keywords?.Any() == true)
                return await GetAllTemplatesAsync();

            await EnsureCacheUpdatedAsync();
            
            lock (_cacheLock)
            {
                return _templateCache.Values
                    .Where(t => t.IsEnabled && t.MatchesKeywords(keywords))
                    .OrderByDescending(t => CalculateKeywordMatchScore(t, keywords))
                    .ToList();
            }
        }

        public async Task<bool> AddTemplateAsync(TestTemplate template)
        {
            try
            {
                if (template == null || string.IsNullOrWhiteSpace(template.Id))
                    return false;

                template.CreatedAt = DateTime.Now;
                template.LastUpdated = DateTime.Now;
                template.IsBuiltIn = false;

                // 保存到文件
                var filePath = Path.Combine(_templatesPath, $"{template.Id}.json");
                var json = JsonSerializer.Serialize(template, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                await File.WriteAllTextAsync(filePath, json);

                // 更新缓存
                lock (_cacheLock)
                {
                    _templateCache[template.Id] = template;
                }

                _logger.LogInformation("模板已添加: {TemplateId} - {TemplateName}", template.Id, template.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加模板失败: {TemplateId}", template?.Id);
                return false;
            }
        }

        public async Task<bool> UpdateTemplateAsync(TestTemplate template)
        {
            try
            {
                if (template == null || string.IsNullOrWhiteSpace(template.Id))
                    return false;

                // 检查模板是否存在
                if (!await ExistsAsync(template.Id))
                    return false;

                template.LastUpdated = DateTime.Now;

                // 保存到文件
                var filePath = Path.Combine(_templatesPath, $"{template.Id}.json");
                var json = JsonSerializer.Serialize(template, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                await File.WriteAllTextAsync(filePath, json);

                // 更新缓存
                lock (_cacheLock)
                {
                    _templateCache[template.Id] = template;
                }

                _logger.LogInformation("模板已更新: {TemplateId} - {TemplateName}", template.Id, template.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新模板失败: {TemplateId}", template?.Id);
                return false;
            }
        }

        public async Task<bool> DeleteTemplateAsync(string templateId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(templateId))
                    return false;

                // 不允许删除内置模板
                var template = await GetTemplateByIdAsync(templateId);
                if (template?.IsBuiltIn == true)
                {
                    _logger.LogWarning("不能删除内置模板: {TemplateId}", templateId);
                    return false;
                }

                // 删除文件
                var filePath = Path.Combine(_templatesPath, $"{templateId}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // 从缓存中移除
                lock (_cacheLock)
                {
                    _templateCache.Remove(templateId);
                }

                _logger.LogInformation("模板已删除: {TemplateId}", templateId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除模板失败: {TemplateId}", templateId);
                return false;
            }
        }

        public async Task<bool> IncrementUsageCountAsync(string templateId)
        {
            try
            {
                var template = await GetTemplateByIdAsync(templateId);
                if (template == null)
                    return false;

                template.UsageCount++;
                return await UpdateTemplateAsync(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新使用次数失败: {TemplateId}", templateId);
                return false;
            }
        }

        public async Task<bool> UpdateRatingAsync(string templateId, double rating)
        {
            try
            {
                if (rating < 1 || rating > 5)
                    return false;

                var template = await GetTemplateByIdAsync(templateId);
                if (template == null)
                    return false;

                // 简单的评分更新，实际应该考虑评分历史
                template.Rating = (template.Rating + rating) / 2;
                return await UpdateTemplateAsync(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新评分失败: {TemplateId}", templateId);
                return false;
            }
        }

        public async Task<List<TestTemplate>> GetPopularTemplatesAsync(int count = 10)
        {
            await EnsureCacheUpdatedAsync();
            
            lock (_cacheLock)
            {
                return _templateCache.Values
                    .Where(t => t.IsEnabled)
                    .OrderByDescending(t => t.UsageCount)
                    .ThenByDescending(t => t.Rating)
                    .Take(count)
                    .ToList();
            }
        }

        public async Task<List<TestTemplate>> GetRecentTemplatesAsync(int count = 10)
        {
            await EnsureCacheUpdatedAsync();
            
            lock (_cacheLock)
            {
                return _templateCache.Values
                    .Where(t => t.IsEnabled)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(count)
                    .ToList();
            }
        }

        public async Task<bool> ExistsAsync(string templateId)
        {
            if (string.IsNullOrWhiteSpace(templateId))
                return false;

            await EnsureCacheUpdatedAsync();
            
            lock (_cacheLock)
            {
                return _templateCache.ContainsKey(templateId);
            }
        }

        public async Task<int> BulkImportTemplatesAsync(List<TestTemplate> templates)
        {
            int successCount = 0;
            
            foreach (var template in templates)
            {
                if (await AddTemplateAsync(template))
                {
                    successCount++;
                }
            }

            _logger.LogInformation("批量导入模板完成: {SuccessCount}/{TotalCount}", successCount, templates.Count);
            return successCount;
        }

        public async Task<TemplateStatistics> GetStatisticsAsync()
        {
            await EnsureCacheUpdatedAsync();
            
            lock (_cacheLock)
            {
                var templates = _templateCache.Values.ToList();
                
                return new TemplateStatistics
                {
                    TotalTemplates = templates.Count,
                    EnabledTemplates = templates.Count(t => t.IsEnabled),
                    BuiltInTemplates = templates.Count(t => t.IsBuiltIn),
                    CustomTemplates = templates.Count(t => !t.IsBuiltIn),
                    ByDevice = templates
                        .SelectMany(t => t.SupportedDevices)
                        .GroupBy(d => d)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    ByCategory = templates
                        .Where(t => !string.IsNullOrWhiteSpace(t.Category))
                        .GroupBy(t => t.Category)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    ByComplexity = templates
                        .GroupBy(t => t.ComplexityLevel)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    TotalUsageCount = templates.Sum(t => t.UsageCount),
                    AverageRating = templates.Count > 0 ? templates.Average(t => t.Rating) : 0,
                    LastUpdated = DateTime.Now
                };
            }
        }

        #region Private Methods

        private async Task EnsureCacheUpdatedAsync()
        {
            if (DateTime.Now - _lastCacheUpdate < _cacheExpiry)
                return;

            await LoadTemplatesFromDiskAsync();
        }

        private async Task LoadTemplatesFromDiskAsync()
        {
            try
            {
                var jsonFiles = Directory.GetFiles(_templatesPath, "*.json");
                var loadedTemplates = new Dictionary<string, TestTemplate>();

                foreach (var file in jsonFiles)
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(file);
                        var template = JsonSerializer.Deserialize<TestTemplate>(json, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                        if (template != null && !string.IsNullOrWhiteSpace(template.Id))
                        {
                            loadedTemplates[template.Id] = template;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "加载模板文件失败: {FilePath}", file);
                    }
                }

                lock (_cacheLock)
                {
                    _templateCache.Clear();
                    foreach (var kvp in loadedTemplates)
                    {
                        _templateCache[kvp.Key] = kvp.Value;
                    }
                    _lastCacheUpdate = DateTime.Now;
                }

                _logger.LogInformation("已加载 {Count} 个模板", loadedTemplates.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载模板缓存失败");
            }
        }

        private async Task InitializeBuiltInTemplatesAsync()
        {
            try
            {
                var builtInTemplates = GetBuiltInTemplates();
                
                foreach (var template in builtInTemplates)
                {
                    var filePath = Path.Combine(_templatesPath, $"{template.Id}.json");
                    if (!File.Exists(filePath))
                    {
                        await AddTemplateAsync(template);
                    }
                }

                _logger.LogInformation("内置模板初始化完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化内置模板失败");
            }
        }

        private List<TestTemplate> GetBuiltInTemplates()
        {
            return new List<TestTemplate>
            {
                // 振动测试模板
                new TestTemplate
                {
                    Id = "vibration_bearing_analysis",
                    Name = "轴承故障振动分析",
                    Description = "检测轴承故障的振动频谱分析程序，支持故障特征频率识别",
                    Category = "振动测试",
                    SupportedDevices = new List<string> { "JYUSB1601", "通用" },
                    Tags = new List<string> { "振动测试", "故障诊断", "FFT分析", "轴承" },
                    ComplexityLevel = "中级",
                    ApplicationDomains = new List<string> { "汽车", "机械", "航空" },
                    Keywords = new List<string> { "振动", "轴承", "故障", "FFT", "频谱" },
                    DefaultParameters = new Dictionary<string, object>
                    {
                        ["sampleRate"] = 25000,
                        ["channelCount"] = 1,
                        ["analysisWindow"] = "Hanning",
                        ["fftSize"] = 4096,
                        ["frequencyRange"] = "1-10kHz"
                    },
                    CodeTemplate = GetVibrationTestTemplate(),
                    UsageInstructions = "适用于电机轴承故障检测，需要连接振动传感器到指定通道",
                    Dependencies = new List<string> { "System.Numerics", "MathNet.Numerics" },
                    IsBuiltIn = true,
                    Rating = 4.8
                },

                // 电气测试模板
                new TestTemplate
                {
                    Id = "electrical_thd_analysis",
                    Name = "信号THD分析",
                    Description = "测试信号发生器的总谐波失真(THD)，分析各次谐波成分",
                    Category = "电气测试",
                    SupportedDevices = new List<string> { "JY5500", "JYUSB1601" },
                    Tags = new List<string> { "电气测试", "THD分析", "谐波检测", "信号质量" },
                    ComplexityLevel = "中级",
                    ApplicationDomains = new List<string> { "电子", "音频", "通用" },
                    Keywords = new List<string> { "THD", "谐波", "信号质量", "失真", "电气" },
                    DefaultParameters = new Dictionary<string, object>
                    {
                        ["sampleRate"] = 100000,
                        ["channelCount"] = 1,
                        ["testFrequency"] = 1000,
                        ["testAmplitude"] = 1.0,
                        ["analysisWindow"] = "Kaiser"
                    },
                    CodeTemplate = GetElectricalTestTemplate(),
                    UsageInstructions = "连接信号发生器输出到采集卡输入，设置测试频率和幅度",
                    Dependencies = new List<string> { "System.Numerics" },
                    IsBuiltIn = true,
                    Rating = 4.6
                },

                // 温度测量模板
                new TestTemplate
                {
                    Id = "temperature_monitoring",
                    Name = "多点温度监控",
                    Description = "实时监控多个温度传感器，支持趋势分析和报警",
                    Category = "温度测量",
                    SupportedDevices = new List<string> { "JYUSB1601", "通用" },
                    Tags = new List<string> { "温度测量", "多点监控", "趋势分析", "报警" },
                    ComplexityLevel = "初级",
                    ApplicationDomains = new List<string> { "工业", "环境", "汽车", "通用" },
                    Keywords = new List<string> { "温度", "监控", "传感器", "报警", "趋势" },
                    DefaultParameters = new Dictionary<string, object>
                    {
                        ["sampleRate"] = 1,
                        ["channelCount"] = 4,
                        ["samplingInterval"] = "1s",
                        ["alarmThreshold"] = 80.0,
                        ["averagingCount"] = 10
                    },
                    CodeTemplate = GetTemperatureTestTemplate(),
                    UsageInstructions = "连接温度传感器到多个通道，设置报警阈值",
                    Dependencies = new List<string> { "System.Collections.Generic" },
                    IsBuiltIn = true,
                    Rating = 4.5
                }
            };
        }

        private string GetVibrationTestTemplate()
        {
            return @"
// AI自动生成的轴承故障振动分析代码
// 测试类型: 振动测试
// 测试对象: 轴承故障检测
// 推荐设备: JYUSB1601
// 频率范围: 1-10kHz
// 分析方法: FFT频谱分析

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class VibrationBearingAnalysis
{
    public static async Task<object> RunTest()
    {
        try
        {
            Console.WriteLine(""开始轴承故障振动分析"");
            Console.WriteLine(""设备: {{DEVICE_TYPE}}"");
            Console.WriteLine(""采样率: {{SAMPLE_RATE}} Hz"");
            Console.WriteLine(""分析频率范围: {{FREQUENCY_RANGE}}"");
            
            // 配置采集参数
            int sampleRate = {{SAMPLE_RATE}};
            int bufferSize = {{BUFFER_SIZE}};
            string analysisWindow = ""{{ANALYSIS_WINDOW}}"";
            
            // 模拟振动数据采集（实际应调用设备API）
            var vibrationData = GenerateVibrationData(sampleRate, bufferSize);
            
            // 应用窗函数
            var windowedData = ApplyWindow(vibrationData, analysisWindow);
            
            // 执行FFT分析
            var fftResult = PerformFFT(windowedData);
            
            // 故障特征频率检测
            var faultFrequencies = DetectBearingFaults(fftResult, sampleRate);
            
            // 计算健康度评分
            var healthScore = CalculateHealthScore(faultFrequencies);
            
            // 生成频谱数据用于显示
            var spectrumData = GenerateSpectrumData(fftResult, sampleRate);
            
            Console.WriteLine($""检测到 {faultFrequencies.Count} 个潜在故障频率"");
            Console.WriteLine($""轴承健康度评分: {healthScore:F2}%"");
            
            return new {
                deviceType = ""{{DEVICE_TYPE}}"",
                analysisType = ""BearingFaultDetection"",
                frequencyRange = ""{{FREQUENCY_RANGE}}"",
                faultFrequencies = faultFrequencies,
                healthScore = healthScore,
                spectrumData = spectrumData,
                analysisWindow = analysisWindow,
                sampleRate = sampleRate,
                dataPoints = bufferSize,
                timestamp = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($""振动分析错误: {ex.Message}"");
            throw;
        }
    }
    
    private static double[] GenerateVibrationData(int sampleRate, int bufferSize)
    {
        // 模拟包含故障特征的振动数据
        var data = new double[bufferSize];
        var random = new Random();
        var time = new double[bufferSize];
        
        for (int i = 0; i < bufferSize; i++)
        {
            time[i] = (double)i / sampleRate;
            
            // 基础振动信号（转频）
            data[i] = 0.5 * Math.Sin(2 * Math.PI * 25 * time[i]);
            
            // 轴承外圈故障频率 (BPFO)
            data[i] += 0.3 * Math.Sin(2 * Math.PI * 162.5 * time[i]);
            
            // 轴承内圈故障频率 (BPFI)  
            data[i] += 0.2 * Math.Sin(2 * Math.PI * 237.5 * time[i]);
            
            // 添加噪声
            data[i] += 0.1 * random.NextDouble();
        }
        
        return data;
    }
    
    private static double[] ApplyWindow(double[] data, string windowType)
    {
        var windowed = new double[data.Length];
        
        for (int i = 0; i < data.Length; i++)
        {
            double w = windowType.ToLower() switch
            {
                ""hanning"" => 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (data.Length - 1)),
                ""hamming"" => 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (data.Length - 1)),
                ""kaiser"" => KaiserWindow(i, data.Length, 8.6),
                _ => 1.0
            };
            windowed[i] = data[i] * w;
        }
        
        return windowed;
    }
    
    private static double KaiserWindow(int n, int N, double beta)
    {
        double arg = beta * Math.Sqrt(1 - Math.Pow(2.0 * n / (N - 1) - 1, 2));
        return ModifiedBesselI0(arg) / ModifiedBesselI0(beta);
    }
    
    private static double ModifiedBesselI0(double x)
    {
        double sum = 1.0;
        double term = 1.0;
        for (int k = 1; k < 50; k++)
        {
            term *= (x / (2 * k)) * (x / (2 * k));
            sum += term;
            if (term < 1e-15) break;
        }
        return sum;
    }
    
    private static Complex[] PerformFFT(double[] data)
    {
        int n = data.Length;
        var complex = new Complex[n];
        
        for (int i = 0; i < n; i++)
        {
            complex[i] = new Complex(data[i], 0);
        }
        
        // 简化的FFT实现（实际应使用优化算法）
        return FFT(complex);
    }
    
    private static Complex[] FFT(Complex[] x)
    {
        int N = x.Length;
        if (N <= 1) return x;
        
        // 分治FFT
        var even = new Complex[N / 2];
        var odd = new Complex[N / 2];
        
        for (int i = 0; i < N / 2; i++)
        {
            even[i] = x[2 * i];
            odd[i] = x[2 * i + 1];
        }
        
        var evenFFT = FFT(even);
        var oddFFT = FFT(odd);
        
        var result = new Complex[N];
        for (int k = 0; k < N / 2; k++)
        {
            var t = Complex.Exp(-2 * Math.PI * Complex.ImaginaryOne * k / N) * oddFFT[k];
            result[k] = evenFFT[k] + t;
            result[k + N / 2] = evenFFT[k] - t;
        }
        
        return result;
    }
    
    private static List<(double frequency, double amplitude, string type)> DetectBearingFaults(Complex[] fft, int sampleRate)
    {
        var faults = new List<(double frequency, double amplitude, string type)>();
        var magnitude = fft.Take(fft.Length / 2).Select(c => c.Magnitude).ToArray();
        
        // 轴承故障特征频率（基于典型轴承参数）
        var faultFrequencies = new Dictionary<string, double[]>
        {
            [""BPFO""] = new[] { 162.5, 325.0, 487.5 }, // 外圈故障及其谐波
            [""BPFI""] = new[] { 237.5, 475.0, 712.5 }, // 内圈故障及其谐波
            [""BSF""] = new[] { 112.5, 225.0, 337.5 },  // 滚动体故障及其谐波
            [""FTF""] = new[] { 12.5, 25.0, 37.5 }      // 保持架故障及其谐波
        };
        
        foreach (var faultType in faultFrequencies)
        {
            foreach (var targetFreq in faultType.Value)
            {
                if (targetFreq > sampleRate / 2) continue;
                
                int binIndex = (int)(targetFreq * fft.Length / sampleRate);
                if (binIndex < magnitude.Length)
                {
                    double amplitude = magnitude[binIndex];
                    
                    // 故障判断阈值（相对于背景噪声）
                    double avgNoise = GetLocalAverage(magnitude, binIndex, 10);
                    if (amplitude > avgNoise * 3) // 3倍噪声阈值
                    {
                        faults.Add((targetFreq, amplitude, faultType.Key));
                    }
                }
            }
        }
        
        return faults.OrderByDescending(f => f.amplitude).ToList();
    }
    
    private static double GetLocalAverage(double[] data, int centerIndex, int windowSize)
    {
        int start = Math.Max(0, centerIndex - windowSize);
        int end = Math.Min(data.Length - 1, centerIndex + windowSize);
        
        double sum = 0;
        int count = 0;
        
        for (int i = start; i <= end; i++)
        {
            if (Math.Abs(i - centerIndex) > 2) // 排除中心附近的点
            {
                sum += data[i];
                count++;
            }
        }
        
        return count > 0 ? sum / count : 0;
    }
    
    private static double CalculateHealthScore(List<(double frequency, double amplitude, string type)> faults)
    {
        if (!faults.Any()) return 100.0; // 无故障特征，健康度100%
        
        // 基于故障严重程度计算健康度
        double faultSeverity = faults.Sum(f => f.amplitude * GetSeverityWeight(f.type));
        double maxSeverity = 10.0; // 假设的最大严重程度
        
        double healthScore = Math.Max(0, 100 - (faultSeverity / maxSeverity * 100));
        return Math.Round(healthScore, 2);
    }
    
    private static double GetSeverityWeight(string faultType)
    {
        return faultType switch
        {
            ""BPFI"" => 1.5, // 内圈故障更严重
            ""BPFO"" => 1.2, // 外圈故障
            ""BSF"" => 1.0,  // 滚动体故障
            ""FTF"" => 0.8,  // 保持架故障相对较轻
            _ => 1.0
        };
    }
    
    private static double[] GenerateSpectrumData(Complex[] fft, int sampleRate)
    {
        int halfLength = fft.Length / 2;
        var spectrum = new double[halfLength];
        
        for (int i = 0; i < halfLength; i++)
        {
            spectrum[i] = 20 * Math.Log10(fft[i].Magnitude + 1e-12); // 转换为dB
        }
        
        return spectrum;
    }
}";
        }

        private string GetElectricalTestTemplate()
        {
            return @"
// AI自动生成的信号THD分析代码
// 测试类型: 电气测试
// 测试对象: 信号质量分析
// 推荐设备: JY5500
// 分析方法: THD谐波分析

using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

public class ElectricalTHDAnalysis
{
    public static async Task<object> RunTest()
    {
        try
        {
            Console.WriteLine(""开始信号THD分析"");
            Console.WriteLine(""设备: {{DEVICE_TYPE}}"");
            Console.WriteLine(""采样率: {{SAMPLE_RATE}} Hz"");
            Console.WriteLine(""测试频率: {{TEST_FREQUENCY}} Hz"");
            
            // 配置测试参数
            int sampleRate = {{SAMPLE_RATE}};
            double testFreq = {{TEST_FREQUENCY}};
            double testAmplitude = {{TEST_AMPLITUDE}};
            int bufferSize = {{BUFFER_SIZE}};
            
            // 模拟信号采集（实际应调用设备API）
            var signalData = GenerateTestSignal(sampleRate, testFreq, testAmplitude, bufferSize);
            
            // 执行FFT分析
            var fftResult = PerformFFT(signalData);
            
            // THD分析
            var thdAnalysis = CalculateTHD(fftResult, sampleRate, testFreq);
            
            // 生成频谱数据
            var spectrumData = GenerateSpectrumData(fftResult, sampleRate);
            
            // 信号质量评估
            var qualityRating = EvaluateSignalQuality(thdAnalysis.THD_Percent, thdAnalysis.SINAD);
            
            Console.WriteLine($""基波频率: {testFreq} Hz"");
            Console.WriteLine($""THD: {thdAnalysis.THD_Percent:F3}% ({thdAnalysis.THD_dB:F2} dB)"");
            Console.WriteLine($""SINAD: {thdAnalysis.SINAD:F2} dB"");
            Console.WriteLine($""信号质量: {qualityRating}"");
            
            return new {
                deviceType = ""{{DEVICE_TYPE}}"",
                analysisType = ""THD_Analysis"",
                testFrequency = testFreq,
                testAmplitude = testAmplitude,
                THD_Percent = thdAnalysis.THD_Percent,
                THD_dB = thdAnalysis.THD_dB,
                SINAD = thdAnalysis.SINAD,
                harmonics = thdAnalysis.Harmonics,
                spectrumData = spectrumData,
                qualityRating = qualityRating,
                sampleRate = sampleRate,
                dataPoints = bufferSize,
                timestamp = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($""THD分析错误: {ex.Message}"");
            throw;
        }
    }
    
    private static double[] GenerateTestSignal(int sampleRate, double testFreq, double amplitude, int bufferSize)
    {
        var data = new double[bufferSize];
        var random = new Random();
        
        for (int i = 0; i < bufferSize; i++)
        {
            double time = (double)i / sampleRate;
            
            // 基波
            data[i] = amplitude * Math.Sin(2 * Math.PI * testFreq * time);
            
            // 添加谐波失真（模拟真实信号发生器的不完美）
            data[i] += 0.02 * amplitude * Math.Sin(2 * Math.PI * testFreq * 2 * time); // 2次谐波
            data[i] += 0.01 * amplitude * Math.Sin(2 * Math.PI * testFreq * 3 * time); // 3次谐波
            data[i] += 0.005 * amplitude * Math.Sin(2 * Math.PI * testFreq * 4 * time); // 4次谐波
            
            // 添加噪声
            data[i] += 0.001 * amplitude * (random.NextDouble() - 0.5);
        }
        
        return data;
    }
    
    private static Complex[] PerformFFT(double[] data)
    {
        int n = data.Length;
        var complex = new Complex[n];
        
        for (int i = 0; i < n; i++)
        {
            complex[i] = new Complex(data[i], 0);
        }
        
        return FFT(complex);
    }
    
    private static Complex[] FFT(Complex[] x)
    {
        int N = x.Length;
        if (N <= 1) return x;
        
        var even = new Complex[N / 2];
        var odd = new Complex[N / 2];
        
        for (int i = 0; i < N / 2; i++)
        {
            even[i] = x[2 * i];
            odd[i] = x[2 * i + 1];
        }
        
        var evenFFT = FFT(even);
        var oddFFT = FFT(odd);
        
        var result = new Complex[N];
        for (int k = 0; k < N / 2; k++)
        {
            var t = Complex.Exp(-2 * Math.PI * Complex.ImaginaryOne * k / N) * oddFFT[k];
            result[k] = evenFFT[k] + t;
            result[k + N / 2] = evenFFT[k] - t;
        }
        
        return result;
    }
    
    private static (double THD_Percent, double THD_dB, double SINAD, List<(int harmonic, double amplitude_dB)> Harmonics) 
        CalculateTHD(Complex[] fft, int sampleRate, double fundamentalFreq)
    {
        var magnitude = fft.Take(fft.Length / 2).Select(c => c.Magnitude).ToArray();
        
        // 找到基波频率对应的bin
        int fundamentalBin = (int)Math.Round(fundamentalFreq * fft.Length / sampleRate);
        double fundamentalAmplitude = magnitude[fundamentalBin];
        
        // 计算谐波幅度
        var harmonics = new List<(int harmonic, double amplitude_dB)>();
        double harmonicPowerSum = 0;
        
        for (int h = 2; h <= 10; h++) // 分析2-10次谐波
        {
            int harmonicBin = (int)Math.Round(fundamentalFreq * h * fft.Length / sampleRate);
            if (harmonicBin < magnitude.Length)
            {
                double harmonicAmplitude = magnitude[harmonicBin];
                double harmonicAmplitude_dB = 20 * Math.Log10(harmonicAmplitude / fundamentalAmplitude);
                
                harmonics.Add((h, harmonicAmplitude_dB));
                harmonicPowerSum += harmonicAmplitude * harmonicAmplitude;
            }
        }
        
        // 计算THD
        double fundamentalPower = fundamentalAmplitude * fundamentalAmplitude;
        double THD_ratio = Math.Sqrt(harmonicPowerSum / fundamentalPower);
        double THD_Percent = THD_ratio * 100;
        double THD_dB = 20 * Math.Log10(THD_ratio);
        
        // 计算噪声功率（排除基波和谐波）
        double noisePower = 0;
        int noiseCount = 0;
        
        for (int i = 1; i < magnitude.Length / 2; i++)
        {
            bool isHarmonic = false;
            for (int h = 1; h <= 10; h++)
            {
                int harmonicBin = (int)Math.Round(fundamentalFreq * h * fft.Length / sampleRate);
                if (Math.Abs(i - harmonicBin) <= 2) // 考虑频率泄漏
                {
                    isHarmonic = true;
                    break;
                }
            }
            
            if (!isHarmonic)
            {
                noisePower += magnitude[i] * magnitude[i];
                noiseCount++;
            }
        }
        
        // 计算SINAD (Signal to Noise and Distortion Ratio)
        double totalNoiseAndDistortion = harmonicPowerSum + noisePower;
        double SINAD = 10 * Math.Log10(fundamentalPower / totalNoiseAndDistortion);
        
        return (THD_Percent, THD_dB, SINAD, harmonics);
    }
    
    private static double[] GenerateSpectrumData(Complex[] fft, int sampleRate)
    {
        int halfLength = fft.Length / 2;
        var spectrum = new double[halfLength];
        
        for (int i = 0; i < halfLength; i++)
        {
            spectrum[i] = 20 * Math.Log10(fft[i].Magnitude + 1e-12); // 转换为dB
        }
        
        return spectrum;
    }
    
    private static string EvaluateSignalQuality(double thdPercent, double sinad)
    {
        if (thdPercent < 0.01 && sinad > 80)
            return ""优秀"";
        else if (thdPercent < 0.05 && sinad > 70)
            return ""良好"";
        else if (thdPercent < 0.1 && sinad > 60)
            return ""一般"";
        else if (thdPercent < 1.0 && sinad > 40)
            return ""较差"";
        else
            return ""很差"";
    }
}";
        }

        private string GetTemperatureTestTemplate()
        {
            return @"
// AI自动生成的多点温度监控代码
// 测试类型: 温度测量
// 测试对象: 多点温度监控
// 推荐设备: JYUSB1601
// 监控通道: 4路温度传感器

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TemperatureMonitoring
{
    public static async Task<object> RunTest()
    {
        try
        {
            Console.WriteLine(""开始多点温度监控"");
            Console.WriteLine(""设备: {{DEVICE_TYPE}}"");
            Console.WriteLine(""监控通道数: {{CHANNEL_COUNT}}"");
            Console.WriteLine(""采样间隔: {{SAMPLING_INTERVAL}}"");
            Console.WriteLine(""报警阈值: {{ALARM_THRESHOLD}}°C"");
            
            // 配置监控参数
            int channelCount = {{CHANNEL_COUNT}};
            double alarmThreshold = {{ALARM_THRESHOLD}};
            int averagingCount = {{AVERAGING_COUNT}};
            string samplingInterval = ""{{SAMPLING_INTERVAL}}"";
            
            // 初始化温度监控系统
            var temperatureMonitor = new MultiChannelTemperatureMonitor(channelCount, alarmThreshold);
            
            // 模拟温度数据采集
            var monitoringResults = await PerformTemperatureMonitoring(temperatureMonitor, averagingCount);
            
            // 分析温度趋势
            var trendAnalysis = AnalyzeTemperatureTrends(monitoringResults.TemperatureHistory);
            
            // 生成报警信息
            var alarmStatus = CheckAlarmConditions(monitoringResults.CurrentTemperatures, alarmThreshold);
            
            // 计算统计信息
            var statistics = CalculateStatistics(monitoringResults.TemperatureHistory);
            
            Console.WriteLine(""当前温度读数:"");
            for (int i = 0; i < channelCount; i++)
            {
                Console.WriteLine($""  通道{i + 1}: {monitoringResults.CurrentTemperatures[i]:F2}°C"");
            }
            
            if (alarmStatus.Any(a => a.IsAlarm))
            {
                Console.WriteLine(""⚠️ 温度报警:"");
                foreach (var alarm in alarmStatus.Where(a => a.IsAlarm))
                {
                    Console.WriteLine($""  通道{alarm.Channel}: {alarm.Message}"");
                }
            }
            
            return new {
                deviceType = ""{{DEVICE_TYPE}}"",
                analysisType = ""TemperatureMonitoring"",
                channelCount = channelCount,
                currentTemperatures = monitoringResults.CurrentTemperatures,
                temperatureHistory = monitoringResults.TemperatureHistory,
                trendAnalysis = trendAnalysis,
                alarmStatus = alarmStatus,
                statistics = statistics,
                alarmThreshold = alarmThreshold,
                samplingInterval = samplingInterval,
                averagingCount = averagingCount,
                timestamp = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($""温度监控错误: {ex.Message}"");
            throw;
        }
    }
    
    private static async Task<(double[] CurrentTemperatures, List<TemperatureReading> TemperatureHistory)> 
        PerformTemperatureMonitoring(MultiChannelTemperatureMonitor monitor, int averagingCount)
    {
        var temperatureHistory = new List<TemperatureReading>();
        var currentTemperatures = new double[monitor.ChannelCount];
        
        // 模拟连续监控过程
        for (int cycle = 0; cycle < averagingCount; cycle++)
        {
            var reading = monitor.ReadTemperatures();
            temperatureHistory.Add(reading);
            
            // 更新当前温度（移动平均）
            for (int ch = 0; ch < monitor.ChannelCount; ch++)
            {
                currentTemperatures[ch] = temperatureHistory
                    .TakeLast(Math.Min(10, temperatureHistory.Count))
                    .Average(r => r.Temperatures[ch]);
            }
            
            // 模拟采样间隔
            await Task.Delay(100); // 实际应根据配置的采样间隔
        }
        
        return (currentTemperatures, temperatureHistory);
    }
    
    private static List<TrendInfo> AnalyzeTemperatureTrends(List<TemperatureReading> history)
    {
        var trends = new List<TrendInfo>();
        
        if (history.Count < 5) return trends; // 数据不足以分析趋势
        
        for (int ch = 0; ch < history[0].Temperatures.Length; ch++)
        {
            var channelData = history.Select(h => h.Temperatures[ch]).ToList();
            var trend = CalculateTrend(channelData);
            
            trends.Add(new TrendInfo
            {
                Channel = ch + 1,
                TrendDirection = trend.Direction,
                ChangeRate = trend.Rate,
                Stability = trend.Stability,
                Prediction = PredictTemperature(channelData, 5) // 预测未来5个周期
            });
        }
        
        return trends;
    }
    
    private static (string Direction, double Rate, string Stability) CalculateTrend(List<double> data)
    {
        if (data.Count < 3) return (""未知"", 0, ""数据不足"");
        
        // 简单线性回归计算趋势
        int n = data.Count;
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        
        for (int i = 0; i < n; i++)
        {
            sumX += i;
            sumY += data[i];
            sumXY += i * data[i];
            sumX2 += i * i;
        }
        
        double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        double intercept = (sumY - slope * sumX) / n;
        
        // 计算R²（决定系数）
        double totalSumSquares = 0, residualSumSquares = 0;
        double mean = sumY / n;
        
        for (int i = 0; i < n; i++)
        {
            double predicted = slope * i + intercept;
            totalSumSquares += Math.Pow(data[i] - mean, 2);
            residualSumSquares += Math.Pow(data[i] - predicted, 2);
        }
        
        double rSquared = 1 - (residualSumSquares / totalSumSquares);
        
        string direction;
        if (Math.Abs(slope) < 0.01) direction = ""稳定"";
        else if (slope > 0) direction = ""上升"";
        else direction = ""下降"";
        
        string stability;
        if (rSquared > 0.8) stability = ""趋势明显"";
        else if (rSquared > 0.5) stability = ""趋势一般"";
        else stability = ""波动较大"";
        
        return (direction, Math.Abs(slope), stability);
    }
    
    private static double PredictTemperature(List<double> data, int periods)
    {
        if (data.Count < 3) return data.LastOrDefault();
        
        // 使用简单移动平均和趋势预测
        var recent = data.TakeLast(5).ToList();
        double average = recent.Average();
        
        // 计算最近的变化率
        double changeRate = 0;
        if (recent.Count >= 2)
        {
            changeRate = (recent.Last() - recent.First()) / (recent.Count - 1);
        }
        
        return average + changeRate * periods;
    }
    
    private static List<AlarmInfo> CheckAlarmConditions(double[] temperatures, double threshold)
    {
        var alarms = new List<AlarmInfo>();
        
        for (int i = 0; i < temperatures.Length; i++)
        {
            bool isAlarm = temperatures[i] > threshold;
            string severity = ""正常"";
            string message = $""{temperatures[i]:F2}°C - 正常"";
            
            if (isAlarm)
            {
                double exceedPercent = (temperatures[i] - threshold) / threshold * 100;
                
                if (exceedPercent > 20)
                {
                    severity = ""严重"";
                    message = $""{temperatures[i]:F2}°C - 严重超温 (+{exceedPercent:F1}%)"";
                }
                else if (exceedPercent > 10)
                {
                    severity = ""警告"";
                    message = $""{temperatures[i]:F2}°C - 超温警告 (+{exceedPercent:F1}%)"";
                }
                else
                {
                    severity = ""轻微"";
                    message = $""{temperatures[i]:F2}°C - 轻微超温 (+{exceedPercent:F1}%)"";
                }
            }
            
            alarms.Add(new AlarmInfo
            {
                Channel = i + 1,
                IsAlarm = isAlarm,
                Severity = severity,
                Message = message,
                Temperature = temperatures[i],
                Threshold = threshold
            });
        }
        
        return alarms;
    }
    
    private static object CalculateStatistics(List<TemperatureReading> history)
    {
        if (!history.Any()) return new { };
        
        int channelCount = history[0].Temperatures.Length;
        var statistics = new List<object>();
        
        for (int ch = 0; ch < channelCount; ch++)
        {
            var channelData = history.Select(h => h.Temperatures[ch]).ToList();
            
            statistics.Add(new {
                channel = ch + 1,
                current = channelData.Last(),
                average = channelData.Average(),
                minimum = channelData.Min(),
                maximum = channelData.Max(),
                standardDeviation = CalculateStandardDeviation(channelData),
                range = channelData.Max() - channelData.Min()
            });
        }
        
        return new {
            channels = statistics,
            monitoringDuration = history.Count,
            dataPoints = history.Count * channelCount,
            startTime = history.First().Timestamp,
            endTime = history.Last().Timestamp
        };
    }
    
    private static double CalculateStandardDeviation(List<double> values)
    {
        if (values.Count < 2) return 0;
        
        double average = values.Average();
        double sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
        return Math.Sqrt(sumOfSquaresOfDifferences / (values.Count - 1));
    }
    
    // 辅助类定义
    public class MultiChannelTemperatureMonitor
    {
        public int ChannelCount { get; }
        public double AlarmThreshold { get; }
        private Random _random = new Random();
        
        public MultiChannelTemperatureMonitor(int channelCount, double alarmThreshold)
        {
            ChannelCount = channelCount;
            AlarmThreshold = alarmThreshold;
        }
        
        public TemperatureReading ReadTemperatures()
        {
            var temperatures = new double[ChannelCount];
            
            // 模拟不同通道的温度读数
            for (int i = 0; i < ChannelCount; i++)
            {
                // 基础温度 + 随机波动 + 通道特性
                double baseTemp = 25.0 + i * 5.0; // 每个通道不同的基础温度
                double variation = (_random.NextDouble() - 0.5) * 4.0; // ±2°C随机波动
                double trend = Math.Sin(DateTime.Now.Millisecond / 1000.0 * Math.PI) * 3.0; // 缓慢变化趋势
                
                temperatures[i] = baseTemp + variation + trend;
                
                // 模拟偶发的高温情况
                if (_random.NextDouble() < 0.1) // 10%概率出现高温
                {
                    temperatures[i] += _random.NextDouble() * 20.0;
                }
            }
            
            return new TemperatureReading
            {
                Temperatures = temperatures,
                Timestamp = DateTime.Now
            };
        }
    }
    
    public class TemperatureReading
    {
        public double[] Temperatures { get; set; } = Array.Empty<double>();
        public DateTime Timestamp { get; set; }
    }
    
    public class TrendInfo
    {
        public int Channel { get; set; }
        public string TrendDirection { get; set; } = string.Empty;
        public double ChangeRate { get; set; }
        public string Stability { get; set; } = string.Empty;
        public double Prediction { get; set; }
    }
    
    public class AlarmInfo
    {
        public int Channel { get; set; }
        public bool IsAlarm { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double Threshold { get; set; }
    }
}";
        }

        private double CalculateKeywordMatchScore(TestTemplate template, List<string> keywords)
        {
            double score = 0;
            var templateKeywords = template.Keywords.Concat(template.Tags).Select(k => k.ToLower()).ToList();
            
            foreach (var keyword in keywords.Select(k => k.ToLower()))
            {
                if (templateKeywords.Any(tk => tk.Contains(keyword) || keyword.Contains(tk)))
                {
                    score += 1.0;
                }
            }
            
            return score / keywords.Count;
        }

        #endregion
    }
}
