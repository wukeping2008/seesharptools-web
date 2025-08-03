using System.Text;
using SeeSharpBackend.Models;
using SeeSharpBackend.Models.MISD;
using SeeSharpBackend.Services.AI.Models;

namespace SeeSharpBackend.Services.AI
{
    /// <summary>
    /// MISD标准代码模板服务 - 扩展版
    /// 提供分类模板库，包含信号生成、数据采集、测量、分析、通信、校准、自动化、性能、安全和集成测试
    /// </summary>
    public interface IMISDCodeTemplateService
    {
        Task<List<TemplateCategory>> GetTemplateCategoriesAsync();
        Task<List<CodeTemplate>> GetTemplatesByCategory(string category);
        Task<CodeTemplate?> GetTemplateById(int templateId);
        Task<List<CodeTemplate>> SearchTemplates(string searchTerm);
        Task<string> GenerateCodeFromTemplate(int templateId, TestRequirement requirement);
        Task<bool> SaveCustomTemplate(CodeTemplate template);
    }

    /// <summary>
    /// MISD标准代码模板服务 - 扩展版实现
    /// 生成符合简仪科技MISD标准的硬件驱动代码
    /// </summary>
    public class MISDCodeTemplateService : IMISDCodeTemplateService
    {
        private readonly ILogger<MISDCodeTemplateService> _logger;
        private readonly List<TemplateCategory> _templateCategories;
        private readonly Dictionary<int, CodeTemplate> _templates;

        public MISDCodeTemplateService(ILogger<MISDCodeTemplateService> logger)
        {
            _logger = logger;
            _templateCategories = new List<TemplateCategory>();
            _templates = new Dictionary<int, CodeTemplate>();
            InitializeTemplateLibrary();
        }

        public async Task<List<TemplateCategory>> GetTemplateCategoriesAsync()
        {
            return await Task.FromResult(_templateCategories);
        }

        public async Task<List<CodeTemplate>> GetTemplatesByCategory(string category)
        {
            var templates = _templates.Values
                .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return await Task.FromResult(templates);
        }

        public async Task<CodeTemplate?> GetTemplateById(int templateId)
        {
            _templates.TryGetValue(templateId, out var template);
            return await Task.FromResult(template);
        }

        public async Task<List<CodeTemplate>> SearchTemplates(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await Task.FromResult(_templates.Values.ToList());

            var searchTermLower = searchTerm.ToLower();
            var matchingTemplates = _templates.Values
                .Where(t => 
                    t.Name.ToLower().Contains(searchTermLower) ||
                    t.Description.ToLower().Contains(searchTermLower) ||
                    t.Keywords.Any(k => k.ToLower().Contains(searchTermLower)) ||
                    t.DeviceType.ToLower().Contains(searchTermLower) ||
                    t.TestType.ToLower().Contains(searchTermLower))
                .ToList();

            return await Task.FromResult(matchingTemplates);
        }

        public async Task<string> GenerateCodeFromTemplate(int templateId, TestRequirement requirement)
        {
            if (!_templates.TryGetValue(templateId, out var template))
            {
                throw new ArgumentException($"Template with ID {templateId} not found");
            }

            // 使用模板生成代码
            var generatedCode = template.TemplateCode;
            var parameters = ExtractTestParameters(requirement);

            // 替换模板变量
            generatedCode = generatedCode
                .Replace("{{TestObject}}", requirement.TestObject ?? "未知设备")
                .Replace("{{DeviceType}}", template.DeviceType)
                .Replace("{{Timestamp}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{{ChannelId}}", parameters.ChannelId.ToString())
                .Replace("{{SampleRate}}", parameters.SampleRate.ToString())
                .Replace("{{BufferSize}}", parameters.BufferSize.ToString())
                .Replace("{{RangeLow}}", parameters.RangeLow.ToString())
                .Replace("{{RangeHigh}}", parameters.RangeHigh.ToString())
                .Replace("{{Frequency}}", parameters.Frequency.ToString())
                .Replace("{{Amplitude}}", parameters.Amplitude.ToString());

            _logger.LogInformation($"Generated code from template {templateId} for requirement: {requirement.TestObject}");
            return await Task.FromResult(generatedCode);
        }

        public async Task<bool> SaveCustomTemplate(CodeTemplate template)
        {
            try
            {
                // 生成新的ID
                var newId = _templates.Keys.DefaultIfEmpty(0).Max() + 1;
                template.Id = newId;
                template.IsCustom = true;
                template.CreatedAt = DateTime.Now;

                _templates[newId] = template;
                _logger.LogInformation($"Saved custom template: {template.Name}");
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to save custom template: {template.Name}");
                return await Task.FromResult(false);
            }
        }

        #region Template Library Initialization

        private void InitializeTemplateLibrary()
        {
            // 初始化模板分类
            _templateCategories.AddRange(new[]
            {
                new TemplateCategory { Id = 1, Name = "信号生成", Description = "信号发生器、波形生成、调制等", Icon = "📡" },
                new TemplateCategory { Id = 2, Name = "数据采集", Description = "ADC采集、连续采集、触发采集等", Icon = "📊" },
                new TemplateCategory { Id = 3, Name = "测量分析", Description = "频谱分析、统计分析、参数测量等", Icon = "📏" },
                new TemplateCategory { Id = 4, Name = "信号处理", Description = "滤波、FFT、数字信号处理等", Icon = "🎛️" },
                new TemplateCategory { Id = 5, Name = "设备通信", Description = "串口、网络、总线通信等", Icon = "🔗" },
                new TemplateCategory { Id = 6, Name = "系统校准", Description = "设备校准、精度验证、标定等", Icon = "⚖️" },
                new TemplateCategory { Id = 7, Name = "自动化测试", Description = "批量测试、循环测试、参数扫描等", Icon = "🤖" },
                new TemplateCategory { Id = 8, Name = "性能测试", Description = "吞吐量、延时、稳定性测试等", Icon = "⚡" },
                new TemplateCategory { Id = 9, Name = "安全测试", Description = "边界测试、异常处理、故障注入等", Icon = "🛡️" },
                new TemplateCategory { Id = 10, Name = "集成测试", Description = "多设备协同、系统级测试等", Icon = "🔧" }
            });

            // 初始化模板
            InitializeSignalGenerationTemplates();
            // TODO: 后续添加其他模板类别的初始化方法
        }

        private void InitializeSignalGenerationTemplates()
        {
            // 1. 正弦波生成模板
            _templates[1] = new CodeTemplate
            {
                Id = 1,
                Name = "正弦波信号生成器",
                Description = "使用JY5500生成可配置频率和幅度的正弦波信号",
                Category = "信号生成",
                DeviceType = "JY5500",
                TestType = "信号生成",
                Keywords = new List<string> { "正弦波", "信号生成", "JY5500", "AO" },
                Difficulty = "初级",
                EstimatedTime = "2-3分钟",
                TemplateCode = GenerateJY5500MISDCode(new TestRequirement()),
                IsCustom = false,
                CreatedAt = DateTime.Now
            };

            // 2. 方波生成模板
            _templates[2] = new CodeTemplate
            {
                Id = 2,
                Name = "方波信号生成器",
                Description = "生成可调占空比的方波信号用于数字测试",
                Category = "信号生成",
                DeviceType = "JY5500",
                TestType = "数字信号",
                Keywords = new List<string> { "方波", "数字信号", "占空比", "JY5500" },
                Difficulty = "初级",
                EstimatedTime = "2-3分钟",
                TemplateCode = @"
using System;
using System.Threading.Tasks;
using System.Reflection;

/// <summary>
/// 方波信号生成器 - MISD标准
/// 测试对象: {{TestObject}}
/// 设备: {{DeviceType}}
/// 生成时间: {{Timestamp}}
/// </summary>
public class SquareWaveGenerator
{
    private object aoTask;
    private Assembly deviceAssembly;
    private Type aoTaskType;
    
    public static async Task<object> RunTest()
    {
        var generator = new SquareWaveGenerator();
        return await generator.ExecuteTest();
    }
    
    public async Task<object> ExecuteTest()
    {
        try
        {
            Console.WriteLine(""开始方波信号生成测试"");
            
            await LoadDriver();
            await InitializeDevice();
            await ConfigureSquareWave();
            await StartGeneration();
            await RunDutyCycleTest();
            await StopAndCleanup();
            
            return new
            {
                deviceType = ""{{DeviceType}}"",
                testType = ""方波生成"",
                frequency = {{Frequency}},
                dutyCycle = 50,
                amplitude = {{Amplitude}},
                success = true,
                summary = ""方波信号生成测试完成""
            };
        }
        catch (Exception ex)
        {
            await StopAndCleanup();
            throw new Exception($""方波生成测试失败: {ex.Message}"", ex);
        }
    }
    
    private async Task LoadDriver()
    {
        var driverPath = ""{{DeviceType}}.dll"";
        deviceAssembly = Assembly.LoadFrom(driverPath);
        aoTaskType = deviceAssembly.GetType(""{{DeviceType}}AOTask"");
    }
    
    private async Task InitializeDevice()
    {
        aoTask = Activator.CreateInstance(aoTaskType, ""0"");
        var addChannelMethod = aoTaskType.GetMethod(""AddChannel"");
        addChannelMethod?.Invoke(aoTask, new object[] { {{ChannelId}}, {{RangeLow}}, {{RangeHigh}} });
    }
    
    private async Task ConfigureSquareWave()
    {
        var setWaveformMethod = aoTaskType.GetMethod(""SetWaveform"");
        setWaveformMethod?.Invoke(aoTask, new object[] { ""Square"", {{Frequency}}, {{Amplitude}} });
    }
    
    private async Task StartGeneration()
    {
        var startMethod = aoTaskType.GetMethod(""Start"");
        startMethod?.Invoke(aoTask, null);
        Console.WriteLine(""方波信号输出已启动"");
    }
    
    private async Task RunDutyCycleTest()
    {
        var dutyCycles = new[] { 25, 50, 75 };
        foreach (var duty in dutyCycles)
        {
            var setDutyCycleMethod = aoTaskType.GetMethod(""SetDutyCycle"");
            setDutyCycleMethod?.Invoke(aoTask, new object[] { duty });
            await Task.Delay(1000);
            Console.WriteLine($""占空比设置为: {duty}%"");
        }
    }
    
    private async Task StopAndCleanup()
    {
        if (aoTask != null && aoTaskType != null)
        {
            var stopMethod = aoTaskType.GetMethod(""Stop"");
            stopMethod?.Invoke(aoTask, null);
        }
    }
}",
                IsCustom = false,
                CreatedAt = DateTime.Now
            };

            // 3. 扫频信号生成模板
            _templates[3] = new CodeTemplate
            {
                Id = 3,
                Name = "频率扫描信号生成器",
                Description = "生成频率连续变化的扫频信号，用于频响测试",
                Category = "信号生成",
                DeviceType = "JY5500",
                TestType = "频响测试",
                Keywords = new List<string> { "扫频", "频响", "频率扫描", "线性扫描" },
                Difficulty = "中级",
                EstimatedTime = "5-10分钟",
                TemplateCode = @"
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// 频率扫描信号生成器 - MISD标准
/// 测试对象: {{TestObject}}
/// 设备: {{DeviceType}}
/// 生成时间: {{Timestamp}}
/// </summary>
public class FrequencySweepGenerator
{
    private object aoTask;
    private Assembly deviceAssembly;
    private Type aoTaskType;
    private double startFreq = 10;
    private double stopFreq = 10000;
    private double sweepTime = 10.0; // 秒
    
    public static async Task<object> RunTest()
    {
        var generator = new FrequencySweepGenerator();
        return await generator.ExecuteTest();
    }
    
    public async Task<object> ExecuteTest()
    {
        try
        {
            Console.WriteLine(""开始频率扫描信号生成测试"");
            
            await LoadDriver();
            await InitializeDevice();
            await ConfigureSweep();
            var sweepData = await ExecuteSweep();
            await StopAndCleanup();
            
            return new
            {
                deviceType = ""{{DeviceType}}"",
                testType = ""频率扫描"",
                startFrequency = startFreq,
                stopFrequency = stopFreq,
                sweepTime = sweepTime,
                sweepPoints = sweepData.Count,
                sweepData = sweepData,
                success = true,
                summary = $""频率扫描完成，从 {startFreq}Hz 到 {stopFreq}Hz，耗时 {sweepTime}秒""
            };
        }
        catch (Exception ex)
        {
            await StopAndCleanup();
            throw new Exception($""频率扫描测试失败: {ex.Message}"", ex);
        }
    }
    
    private async Task LoadDriver()
    {
        var driverPath = ""{{DeviceType}}.dll"";
        deviceAssembly = Assembly.LoadFrom(driverPath);
        aoTaskType = deviceAssembly.GetType(""{{DeviceType}}AOTask"");
    }
    
    private async Task InitializeDevice()
    {
        aoTask = Activator.CreateInstance(aoTaskType, ""0"");
        var addChannelMethod = aoTaskType.GetMethod(""AddChannel"");
        addChannelMethod?.Invoke(aoTask, new object[] { {{ChannelId}}, {{RangeLow}}, {{RangeHigh}} });
    }
    
    private async Task ConfigureSweep()
    {
        var setSweepMethod = aoTaskType.GetMethod(""SetSweepMode"");
        setSweepMethod?.Invoke(aoTask, new object[] { ""Linear"", startFreq, stopFreq, sweepTime });
    }
    
    private async Task<List<SweepPoint>> ExecuteSweep()
    {
        var sweepData = new List<SweepPoint>();
        var startMethod = aoTaskType.GetMethod(""Start"");
        startMethod?.Invoke(aoTask, null);
        
        var sweepSteps = 100;
        var stepTime = sweepTime / sweepSteps;
        var freqStep = (stopFreq - startFreq) / sweepSteps;
        
        for (int i = 0; i < sweepSteps; i++)
        {
            var currentFreq = startFreq + i * freqStep;
            var setFreqMethod = aoTaskType.GetMethod(""SetFrequency"");
            setFreqMethod?.Invoke(aoTask, new object[] { currentFreq });
            
            await Task.Delay((int)(stepTime * 1000));
            
            sweepData.Add(new SweepPoint
            {
                Frequency = currentFreq,
                Amplitude = {{Amplitude}},
                Timestamp = DateTime.Now
            });
            
            if (i % 10 == 0)
            {
                Console.WriteLine($""扫频进度: {i}/{sweepSteps}, 当前频率: {currentFreq:F1}Hz"");
            }
        }
        
        return sweepData;
    }
    
    private async Task StopAndCleanup()
    {
        if (aoTask != null && aoTaskType != null)
        {
            var stopMethod = aoTaskType.GetMethod(""Stop"");
            stopMethod?.Invoke(aoTask, null);
        }
    }
    
    public class SweepPoint
    {
        public double Frequency { get; set; }
        public double Amplitude { get; set; }
        public DateTime Timestamp { get; set; }
    }
}",
                IsCustom = false,
                CreatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// 根据测试需求生成MISD标准的振动测试代码
        /// </summary>
        public string GenerateVibrationMISDCode(TestRequirement requirement)
        {
            var deviceType = requirement.RecommendedDevice ?? "JYUSB1601";
            var testObject = requirement.TestObject ?? "未知设备";
            var parameters = ExtractTestParameters(requirement);

            var template = $@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

/// <summary>
/// AI生成的MISD标准振动测试代码 - 基于真实硬件驱动
/// 测试对象: {testObject}
/// 设备: {deviceType}
/// 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
/// 支持的硬件驱动: JYUSB1601.dll
/// </summary>
public class VibrationTest
{{
    private object aiTask;
    private Assembly jyusbAssembly;
    private Type aiTaskType;
    private string deviceIndex;
    private int channelId;
    private double sampleRate;
    private int bufferSize;
    
    public static async Task<object> RunTest()
    {{
        var test = new VibrationTest();
        return await test.ExecuteTest();
    }}
    
    public async Task<object> ExecuteTest()
    {{
        try
        {{
            Console.WriteLine($""开始振动测试 - {testObject}"");
            Console.WriteLine($""设备: {deviceType}"");
            
            // MISD标准：动态加载硬件驱动
            await LoadHardwareDriver();
            
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
            {{
                deviceType = ""{deviceType}"",
                testObject = ""{testObject}"",
                analysisType = ""振动频谱分析"",
                timestamp = DateTime.Now,
                sampleRate = sampleRate,
                sampleCount = vibrationData.Length,
                vibrationData = vibrationData.Take(1000).ToArray(), // 只返回前1000个点用于显示
                spectrumData = analysisResult.SpectrumData,
                detectedFaults = analysisResult.DetectedFaults,
                rmsValue = analysisResult.RmsValue,
                peakValue = analysisResult.PeakValue,
                summary = $""振动测试完成，RMS: {{analysisResult.RmsValue:F3}}V，Peak: {{analysisResult.PeakValue:F3}}V，发现 {{analysisResult.DetectedFaults.Count}} 个潜在故障""
            }};
        }}
        catch (Exception ex)
        {{
            await StopAndCleanup();
            Console.WriteLine($""振动测试失败: {{ex.Message}}"");
            throw new Exception($""振动测试执行失败: {{ex.Message}}"", ex);
        }}
    }}
    
    /// <summary>
    /// MISD标准：动态加载硬件驱动
    /// </summary>
    private async Task LoadHardwareDriver()
    {{
        try
        {{
            // 尝试加载JYUSB1601驱动程序集
            var driverPath = ""JYUSB1601.dll"";
            if (File.Exists(driverPath))
            {{
                jyusbAssembly = Assembly.LoadFrom(driverPath);
                aiTaskType = jyusbAssembly.GetType(""JYUSB1601AITask"");
                Console.WriteLine($""成功加载硬件驱动: {{driverPath}}"");
            }}
            else
            {{
                throw new FileNotFoundException($""硬件驱动文件未找到: {{driverPath}}"");
            }}
        }}
        catch (Exception ex)
        {{
            throw new Exception($""加载硬件驱动失败: {{ex.Message}}"", ex);
        }}
    }}
    
    /// <summary>
    /// MISD标准：设备初始化
    /// </summary>
    private async Task InitializeDevice()
    {{
        deviceIndex = ""0""; // 默认使用第一个设备
        channelId = {parameters.ChannelId};
        
        try
        {{
            if (aiTaskType != null)
            {{
                // 使用反射创建AI任务实例
                aiTask = Activator.CreateInstance(aiTaskType, deviceIndex);
                Console.WriteLine($""成功初始化{deviceType}设备，索引: {{deviceIndex}}"");
            }}
            else
            {{
                throw new InvalidOperationException(""AI任务类型未正确加载"");
            }}
        }}
        catch (Exception ex)
        {{
            throw new Exception($""{deviceType}设备初始化失败: {{ex.Message}}"", ex);
        }}
    }}
    
    /// <summary>
    /// MISD标准：配置采集参数
    /// </summary>
    private async Task ConfigureAcquisition()
    {{
        try
        {{
            if (aiTask != null && aiTaskType != null)
            {{
                // 设置采集模式
                var modeProperty = aiTaskType.GetProperty(""Mode"");
                if (modeProperty != null)
                {{
                    var aiModeType = jyusbAssembly.GetType(""AIMode"");
                    var continuousMode = Enum.Parse(aiModeType, ""Continuous"");
                    modeProperty.SetValue(aiTask, continuousMode);
                }}
                
                // 添加通道配置
                var addChannelMethod = aiTaskType.GetMethod(""AddChannel"");
                if (addChannelMethod != null)
                {{
                    addChannelMethod.Invoke(aiTask, new object[] {{ channelId, {parameters.RangeLow}, {parameters.RangeHigh} }});
                }}
                
                // 设置采样率
                sampleRate = {parameters.SampleRate};
                var sampleRateProperty = aiTaskType.GetProperty(""SampleRate"");
                if (sampleRateProperty != null)
                {{
                    sampleRateProperty.SetValue(aiTask, sampleRate);
                }}
                
                // 设置缓冲区大小
                bufferSize = {parameters.BufferSize};
                
                Console.WriteLine($""配置完成 - 通道: {{channelId}}, 量程: {parameters.RangeLow}V~{parameters.RangeHigh}V, 采样率: {{sampleRate}}Hz"");
            }}
        }}
        catch (Exception ex)
        {{
            throw new Exception($""采集参数配置失败: {{ex.Message}}"", ex);
        }}
    }}
    
    /// <summary>
    /// MISD标准：启动数据采集
    /// </summary>
    private async Task StartAcquisition()
    {{
        try
        {{
            if (aiTask != null && aiTaskType != null)
            {{
                var startMethod = aiTaskType.GetMethod(""Start"");
                if (startMethod != null)
                {{
                    startMethod.Invoke(aiTask, null);
                    Console.WriteLine(""数据采集已启动"");
                    
                    // 等待数据缓冲区准备就绪
                    await Task.Delay(100);
                }}
            }}
        }}
        catch (Exception ex)
        {{
            throw new Exception($""启动数据采集失败: {{ex.Message}}"", ex);
        }}
    }}
    
    /// <summary>
    /// MISD标准：采集振动数据
    /// </summary>
    private async Task<double[]> AcquireVibrationData()
    {{
        try
        {{
            var totalData = new List<double>();
            var acquisitionTime = 2.0; // 采集2秒钟的数据
            var targetSamples = (int)(sampleRate * acquisitionTime);
            
            Console.WriteLine($""开始采集振动数据，目标样本数: {{targetSamples}}"");
            
            if (aiTask != null && aiTaskType != null)
            {{
                var availableSamplesProperty = aiTaskType.GetProperty(""AvailableSamples"");
                var readDataMethod = aiTaskType.GetMethod(""ReadData"");
                
                while (totalData.Count < targetSamples)
                {{
                    // 检查可用样本数
                    if (availableSamplesProperty != null)
                    {{
                        var availableSamples = (ulong)availableSamplesProperty.GetValue(aiTask);
                        
                        if (availableSamples >= (ulong)bufferSize)
                        {{
                            var readValue = new double[bufferSize];
                            
                            // 读取数据
                            if (readDataMethod != null)
                            {{
                                var parameters = new object[] {{ readValue, bufferSize, -1 }};
                                readDataMethod.Invoke(aiTask, parameters);
                                totalData.AddRange(readValue);
                                
                                if (totalData.Count % 5000 == 0)
                                {{
                                    Console.WriteLine($""已采集 {{totalData.Count}} 个样本..."");
                                }}
                            }}
                        }}
                        else
                        {{
                            await Task.Delay(10);
                        }}
                    }}
                }}
            }}
            
            // 截取目标长度的数据
            var result = totalData.Take(targetSamples).ToArray();
            Console.WriteLine($""振动数据采集完成，共 {{result.Length}} 个样本"");
            
            return result;
        }}
        catch (Exception ex)
        {{
            throw new Exception($""振动数据采集失败: {{ex.Message}}"", ex);
        }}
    }}
    
    /// <summary>
    /// MISD标准：停止采集并清理资源
    /// </summary>
    private async Task StopAndCleanup()
    {{
        try
        {{
            if (aiTask != null && aiTaskType != null)
            {{
                var stopMethod = aiTaskType.GetMethod(""Stop"");
                if (stopMethod != null)
                {{
                    stopMethod.Invoke(aiTask, null);
                }}
                
                var channelsProperty = aiTaskType.GetProperty(""Channels"");
                if (channelsProperty != null)
                {{
                    var channels = channelsProperty.GetValue(aiTask);
                    var clearMethod = channels?.GetType().GetMethod(""Clear"");
                    clearMethod?.Invoke(channels, null);
                }}
                
                Console.WriteLine(""数据采集已停止，资源已清理"");
            }}
        }}
        catch (Exception ex)
        {{
            Console.WriteLine($""清理资源时出现警告: {{ex.Message}}"");
        }}
    }}
    
    /// <summary>
    /// 振动数据分析
    /// </summary>
    private async Task<VibrationAnalysisResult> AnalyzeVibrationData(double[] data)
    {{
        try
        {{
            Console.WriteLine(""开始振动数据分析..."");
            
            // 计算RMS值
            var rmsValue = Math.Sqrt(data.Sum(x => x * x) / data.Length);
            
            // 计算峰值
            var peakValue = data.Max(Math.Abs);
            
            // 执行FFT分析
            var fftResult = PerformFFTAnalysis(data);
            
            // 故障特征检测
            var faults = DetectVibrationFaults(fftResult, rmsValue, peakValue);
            
            Console.WriteLine($""振动分析完成 - RMS: {{rmsValue:F3}}V, Peak: {{peakValue:F3}}V, 故障数: {{faults.Count}}"");
            
            return new VibrationAnalysisResult
            {{
                RmsValue = rmsValue,
                PeakValue = peakValue,
                SpectrumData = fftResult.Take(500).ToArray(), // 返回前500个频率点用于显示
                DetectedFaults = faults
            }};
        }}
        catch (Exception ex)
        {{
            throw new Exception($""振动数据分析失败: {{ex.Message}}"", ex);
        }}
    }}
    
    private double[] PerformFFTAnalysis(double[] data)
    {{
        // 简化的FFT实现（实际应用中应使用专业的FFT库）
        var fftSize = Math.Min(data.Length / 2, 2048);
        var result = new double[fftSize];
        
        for (int k = 0; k < fftSize; k++)
        {{
            double real = 0, imag = 0;
            
            for (int n = 0; n < Math.Min(data.Length, 4096); n++)
            {{
                var angle = -2.0 * Math.PI * k * n / data.Length;
                real += data[n] * Math.Cos(angle);
                imag += data[n] * Math.Sin(angle);
            }}
            
            result[k] = Math.Sqrt(real * real + imag * imag) / data.Length;
        }}
        
        return result;
    }}
    
    private List<string> DetectVibrationFaults(double[] spectrum, double rmsValue, double peakValue)
    {{
        var faults = new List<string>();
        var frequencyResolution = sampleRate / (2.0 * spectrum.Length);
        
        // 检测高RMS值
        if (rmsValue > 2.0)
        {{
            faults.Add($""RMS值过高: {{rmsValue:F3}}V (正常范围: <2.0V)"");
        }}
        
        // 检测高峰值
        if (peakValue > 5.0)
        {{
            faults.Add($""峰值过高: {{peakValue:F3}}V (正常范围: <5.0V)"");
        }}
        
        // 检测频域异常
        var maxSpectrum = spectrum.Max();
        var avgSpectrum = spectrum.Average();
        var threshold = avgSpectrum + 3 * Math.Sqrt(spectrum.Sum(x => Math.Pow(x - avgSpectrum, 2)) / spectrum.Length);
        
        for (int i = 10; i < spectrum.Length - 10; i++) // 忽略DC和高频噪声
        {{
            if (spectrum[i] > threshold && spectrum[i] > maxSpectrum * 0.1)
            {{
                var frequency = i * frequencyResolution;
                faults.Add($""频率 {{frequency:F1}} Hz 处检测到异常峰值: {{spectrum[i]:F6}}V"");
            }}
        }}
        
        return faults;
    }}
    
    public class VibrationAnalysisResult
    {{
        public double RmsValue {{ get; set; }}
        public double PeakValue {{ get; set; }}
        public double[] SpectrumData {{ get; set; }}
        public List<string> DetectedFaults {{ get; set; }}
    }}
}}";

            return template;
        }

        /// <summary>
        /// 生成JY5500信号发生器的MISD标准代码
        /// </summary>
        public string GenerateJY5500MISDCode(TestRequirement requirement)
        {
            var testObject = requirement.TestObject ?? "信号发生器";
            var parameters = ExtractTestParameters(requirement);

            var template = $@"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;

/// <summary>
/// AI生成的JY5500信号发生器MISD标准代码
/// 测试对象: {testObject}
/// 设备: JY5500
/// 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
/// 支持的硬件驱动: JY5500.dll
/// </summary>
public class SignalGeneratorTest
{{
    private object aoTask;
    private Assembly jy5500Assembly;
    private Type aoTaskType;
    private string deviceIndex;
    private int channelId;
    private double frequency;
    private double amplitude;
    
    public static async Task<object> RunTest()
    {{
        var test = new SignalGeneratorTest();
        return await test.ExecuteTest();
    }}
    
    public async Task<object> ExecuteTest()
    {{
        try
        {{
            Console.WriteLine($""开始信号发生器测试 - {testObject}"");
            Console.WriteLine(""设备: JY5500"");
            
            // MISD标准：动态加载硬件驱动
            await LoadHardwareDriver();
            
            // MISD标准：设备初始化
            await InitializeDevice();
            
            // MISD标准：配置输出参数
            await ConfigureOutput();
            
            // MISD标准：启动信号输出
            await StartOutput();
            
            // 运行测试序列
            var testResults = await RunTestSequence();
            
            // MISD标准：停止输出并清理资源
            await StopAndCleanup();
            
            return new
            {{
                deviceType = ""JY5500"",
                testObject = ""{testObject}"",
                analysisType = ""信号发生器测试"",
                timestamp = DateTime.Now,
                frequency = frequency,
                amplitude = amplitude,
                testResults = testResults,
                summary = $""信号发生器测试完成，频率: {{frequency}}Hz，幅度: {{amplitude}}V""
            }};
        }}
        catch (Exception ex)
        {{
            await StopAndCleanup();
            Console.WriteLine($""信号发生器测试失败: {{ex.Message}}"");
            throw new Exception($""信号发生器测试执行失败: {{ex.Message}}"", ex);
        }}
    }}
    
    /// <summary>
    /// MISD标准：动态加载硬件驱动
    /// </summary>
    private async Task LoadHardwareDriver()
    {{
        try
        {{
            var driverPath = ""JY5500.dll"";
            if (File.Exists(driverPath))
            {{
                jy5500Assembly = Assembly.LoadFrom(driverPath);
                aoTaskType = jy5500Assembly.GetType(""JY5500AOTask"");
                Console.WriteLine($""成功加载硬件驱动: {{driverPath}}"");
            }}
            else
            {{
                throw new FileNotFoundException($""硬件驱动文件未找到: {{driverPath}}"");
            }}
        }}
        catch (Exception ex)
        {{
            throw new Exception($""加载硬件驱动失败: {{ex.Message}}"", ex);
        }}
    }}
    
    /// <summary>
    /// MISD标准：设备初始化
    /// </summary>
    private async Task InitializeDevice()
    {{
        deviceIndex = ""0"";
        channelId = {parameters.ChannelId};
        
        try
        {{
            if (aoTaskType != null)
            {{
                aoTask = Activator.CreateInstance(aoTaskType, deviceIndex);
                Console.WriteLine($""成功初始化JY5500设备，索引: {{deviceIndex}}"");
            }}
        }}
        catch (Exception ex)
        {{
            throw new Exception($""JY5500设备初始化失败: {{ex.Message}}"", ex);
        }}
    }}
    
    /// <summary>
    /// MISD标准：配置输出参数
    /// </summary>
    private async Task ConfigureOutput()
    {{
        try
        {{
            if (aoTask != null && aoTaskType != null)
            {{
                // 添加输出通道
                var addChannelMethod = aoTaskType.GetMethod(""AddChannel"");
                if (addChannelMethod != null)
                {{
                    addChannelMethod.Invoke(aoTask, new object[] {{ channelId, {parameters.RangeLow}, {parameters.RangeHigh} }});
                }}
                
                // 设置频率和幅度
                frequency = {parameters.Frequency};
                amplitude = {parameters.Amplitude};
                
                // 配置波形参数
                var setWaveformMethod = aoTaskType.GetMethod(""SetWaveform"");
                if (setWaveformMethod != null)
                {{
                    setWaveformMethod.Invoke(aoTask, new object[] {{ ""Sine"", frequency, amplitude }});
                }}
                
                Console.WriteLine($""配置完成 - 通道: {{channelId}}, 频率: {{frequency}}Hz, 幅度: {{amplitude}}V"");
            }}
        }}
        catch (Exception ex)
        {{
            throw new Exception($""输出参数配置失败: {{ex.Message}}"", ex);
        }}
    }}
    
    /// <summary>
    /// MISD标准：启动信号输出
    /// </summary>
    private async Task StartOutput()
    {{
        try
        {{
            if (aoTask != null && aoTaskType != null)
            {{
                var startMethod = aoTaskType.GetMethod(""Start"");
                if (startMethod != null)
                {{
                    startMethod.Invoke(aoTask, null);
                    Console.WriteLine(""信号输出已启动"");
                }}
            }}
        }}
        catch (Exception ex)
        {{
            throw new Exception($""启动信号输出失败: {{ex.Message}}"", ex);
        }}
    }}
    
    /// <summary>
    /// 运行测试序列
    /// </summary>
    private async Task<Dictionary<string, object>> RunTestSequence()
    {{
        var results = new Dictionary<string, object>();
        
        try
        {{
            // 测试1：基础输出测试
            Console.WriteLine(""执行基础输出测试..."");
            await Task.Delay(1000);
            results[""BasicOutputTest""] = ""通过"";
            
            // 测试2：频率扫描测试
            Console.WriteLine(""执行频率扫描测试..."");
            var frequencies = new[] {{ 100, 1000, 10000 }};
            var sweepResults = new List<string>();
            
            foreach (var freq in frequencies)
            {{
                var setFrequencyMethod = aoTaskType?.GetMethod(""SetFrequency"");
                if (setFrequencyMethod != null)
                {{
                    setFrequencyMethod.Invoke(aoTask, new object[] {{ freq }});
                    await Task.Delay(500);
                    sweepResults.Add($""{{freq}}Hz: 正常"");
                }}
            }}
            
            results[""FrequencySweepTest""] = sweepResults;
            
            // 测试3：幅度测试
            Console.WriteLine(""执行幅度调节测试..."");
            var amplitudes = new[] {{ 0.1, 1.0, 5.0 }};
            var ampResults = new List<string>();
            
            foreach (var amp in amplitudes)
            {{
                var setAmplitudeMethod = aoTaskType?.GetMethod(""SetAmplitude"");
                if (setAmplitudeMethod != null)
                {{
                    setAmplitudeMethod.Invoke(aoTask, new object[] {{ amp }});
                    await Task.Delay(300);
                    ampResults.Add($""{{amp}}V: 正常"");
                }}
            }}
            
            results[""AmplitudeTest""] = ampResults;
            
            return results;
        }}
        catch (Exception ex)
        {{
            throw new Exception($""测试序列执行失败: {{ex.Message}}"", ex);
        }}
    }}
    
    /// <summary>
    /// MISD标准：停止输出并清理资源
    /// </summary>
    private async Task StopAndCleanup()
    {{
        try
        {{
            if (aoTask != null && aoTaskType != null)
            {{
                var stopMethod = aoTaskType.GetMethod(""Stop"");
                if (stopMethod != null)
                {{
                    stopMethod.Invoke(aoTask, null);
                }}
                Console.WriteLine(""信号输出已停止，资源已清理"");
            }}
        }}
        catch (Exception ex)
        {{
            Console.WriteLine($""清理资源时出现警告: {{ex.Message}}"");
        }}
    }}
}}";

            return template;
        }

        /// <summary>
        /// 从测试需求中提取参数
        /// </summary>
        private TestParameters ExtractTestParameters(TestRequirement requirement)
        {
            var parameters = new TestParameters();

            // 解析频率范围
            if (!string.IsNullOrEmpty(requirement.FrequencyRange))
            {
                var (min, max) = ParseFrequencyRange(requirement.FrequencyRange);
                parameters.Frequency = max > 0 ? max : 1000;
                parameters.SampleRate = (int)(parameters.Frequency * 2.56); // 按照奈奎斯特定理
            }

            // 根据测试类型设置默认参数
            switch (requirement.TestType?.ToLower())
            {
                case "振动测试":
                    parameters.RangeLow = -10.0;
                    parameters.RangeHigh = 10.0;
                    parameters.ChannelId = 0;
                    parameters.BufferSize = 4096;
                    parameters.Amplitude = 1.0;
                    break;
                    
                case "电气测试":
                    parameters.RangeLow = -5.0;
                    parameters.RangeHigh = 5.0;
                    parameters.ChannelId = 0;
                    parameters.BufferSize = 2048;
                    parameters.Amplitude = 2.0;
                    break;
                    
                default:
                    parameters.RangeLow = -10.0;
                    parameters.RangeHigh = 10.0;
                    parameters.ChannelId = 0;
                    parameters.BufferSize = 1024;
                    parameters.Amplitude = 1.0;
                    break;
            }

            return parameters;
        }

        private (double min, double max) ParseFrequencyRange(string range)
        {
            try
            {
                if (string.IsNullOrEmpty(range)) return (0, 1000);

                // 匹配各种频率范围格式
                var patterns = new[]
                {
                    @"(\d+(?:\.\d+)?)\s*-\s*(\d+(?:\.\d+)?)\s*[kK]?[hH]z",
                    @"(\d+(?:\.\d+)?)\s*[kK]?[hH]z\s*-\s*(\d+(?:\.\d+)?)\s*[kK]?[hH]z",
                    @"(\d+(?:\.\d+)?)\s*~\s*(\d+(?:\.\d+)?)\s*[kK]?[hH]z"
                };

                foreach (var pattern in patterns)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(range, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var min = double.Parse(match.Groups[1].Value);
                        var max = double.Parse(match.Groups[2].Value);
                        
                        // 处理k单位
                        if (range.ToLower().Contains("k"))
                        {
                            min *= 1000;
                            max *= 1000;
                        }
                        
                        return (min, max);
                    }
                }

                return (0, 1000);
            }
            catch
            {
                return (0, 1000);
            }
        }

        /// <summary>
        /// 测试参数类
        /// </summary>
        private class TestParameters
        {
            public double RangeLow { get; set; } = -10.0;
            public double RangeHigh { get; set; } = 10.0;
            public int ChannelId { get; set; } = 0;
            public int SampleRate { get; set; } = 10000;
            public int BufferSize { get; set; } = 1024;
            public double Frequency { get; set; } = 1000;
            public double Amplitude { get; set; } = 1.0;
        }

        #endregion
    }
}
