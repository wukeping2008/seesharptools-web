using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace SeeSharpBackend.Services.CSharpRunner;

/// <summary>
/// C# Runner MCP 服务实现
/// </summary>
public class CSharpRunnerService : ICSharpRunnerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CSharpRunnerService> _logger;
    private readonly CSharpRunnerOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public CSharpRunnerService(
        HttpClient httpClient,
        ILogger<CSharpRunnerService> logger,
        IOptions<CSharpRunnerOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // 配置 HttpClient
        _httpClient.BaseAddress = new Uri(_options.ServiceUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.DefaultTimeout);
    }

    /// <summary>
    /// 执行 C# 代码
    /// </summary>
    public async Task<CSharpExecutionResult> ExecuteCodeAsync(string code, int timeout = 30000)
    {
        try
        {
            _logger.LogInformation("开始执行 C# 代码，超时时间: {Timeout}ms", timeout);

            var request = new
            {
                code = code,
                timeout = timeout
            };

            var response = await _httpClient.PostAsync("/api/run", 
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("C# Runner API 调用失败: {StatusCode}", response.StatusCode);
                return new CSharpExecutionResult
                {
                    Success = false,
                    ErrorOutput = $"API 调用失败: {response.StatusCode}"
                };
            }

            // 处理 SSE 流式响应
            return await ProcessStreamResponseAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行 C# 代码时发生异常");
            return new CSharpExecutionResult
            {
                Success = false,
                ErrorOutput = $"执行异常: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// 执行仪器控制相关的 C# 代码
    /// </summary>
    public async Task<CSharpExecutionResult> ExecuteInstrumentCodeAsync(string code, string deviceType, Dictionary<string, object> parameters)
    {
        try
        {
            _logger.LogInformation("执行仪器控制代码，设备类型: {DeviceType}", deviceType);

            // 预处理代码，添加必要的 using 语句和设备初始化
            var enhancedCode = EnhanceInstrumentCode(code, deviceType, parameters);

            var result = await ExecuteCodeAsync(enhancedCode);

            // 尝试解析仪器数据
            if (result.Success && result.ReturnValue != null)
            {
                result.InstrumentData = TryParseInstrumentData(result.ReturnValue, deviceType);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行仪器控制代码时发生异常");
            return new CSharpExecutionResult
            {
                Success = false,
                ErrorOutput = $"仪器控制执行异常: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// 检查服务是否可用
    /// </summary>
    public async Task<bool> IsServiceAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "检查 C# Runner 服务可用性时发生异常");
            return false;
        }
    }

    /// <summary>
    /// 获取预置代码模板
    /// </summary>
    public async Task<string> GetCodeTemplateAsync(string deviceType, string templateType)
    {
        return deviceType.ToUpper() switch
        {
            "JY5500" => GetJY5500Template(templateType),
            "JYUSB1601" => GetJYUSB1601Template(templateType),
            _ => GetGenericTemplate(templateType)
        };
    }

    #region 私有方法

    /// <summary>
    /// 处理 SSE 流式响应
    /// </summary>
    private async Task<CSharpExecutionResult> ProcessStreamResponseAsync(HttpResponseMessage response)
    {
        var result = new CSharpExecutionResult { Success = true };
        var consoleOutput = new StringBuilder();
        var errorOutput = new StringBuilder();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: "))
            {
                var jsonData = line.Substring(6);
                if (string.IsNullOrWhiteSpace(jsonData)) continue;

                try
                {
                    var messageData = JsonSerializer.Deserialize<JsonElement>(jsonData);
                    var message = ProcessSseMessage(messageData);
                    result.Messages.Add(message);

                    switch (message.Kind)
                    {
                        case "stdout":
                            consoleOutput.AppendLine(message.Content);
                            break;
                        case "stderr":
                            errorOutput.AppendLine(message.Content);
                            result.Success = false;
                            break;
                        case "result":
                            result.ReturnValue = JsonSerializer.Deserialize<object>(message.Content);
                            break;
                        case "end":
                            var endData = JsonSerializer.Deserialize<EndResponse>(message.Content);
                            result.ElapsedMilliseconds = endData?.Elapsed ?? 0;
                            result.ConsoleOutput = consoleOutput.ToString();
                            result.ErrorOutput = errorOutput.ToString();
                            return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "解析 SSE 消息失败: {Message}", jsonData);
                }
            }
        }

        result.ConsoleOutput = consoleOutput.ToString();
        result.ErrorOutput = errorOutput.ToString();
        return result;
    }

    /// <summary>
    /// 处理 SSE 消息
    /// </summary>
    private ExecutionMessage ProcessSseMessage(JsonElement messageData)
    {
        var message = new ExecutionMessage();

        if (messageData.TryGetProperty("kind", out var kindElement))
        {
            message.Kind = kindElement.GetString() ?? string.Empty;
        }

        // 根据消息类型提取内容
        switch (message.Kind)
        {
            case "stdout":
                if (messageData.TryGetProperty("stdOutput", out var stdOutput))
                    message.Content = stdOutput.GetString() ?? string.Empty;
                break;
            case "stderr":
                if (messageData.TryGetProperty("stdError", out var stdError))
                    message.Content = stdError.GetString() ?? string.Empty;
                break;
            case "result":
                if (messageData.TryGetProperty("result", out var result))
                    message.Content = result.ToString();
                break;
            case "end":
                message.Content = messageData.ToString();
                break;
        }

        return message;
    }

    /// <summary>
    /// 增强仪器控制代码
    /// </summary>
    private string EnhanceInstrumentCode(string code, string deviceType, Dictionary<string, object> parameters)
    {
        var enhancedCode = new StringBuilder();

        // 添加必要的 using 语句
        switch (deviceType.ToUpper())
        {
            case "JY5500":
                enhancedCode.AppendLine("using JY5500;");
                break;
            case "JYUSB1601":
                enhancedCode.AppendLine("using JYUSB1601;");
                break;
        }

        enhancedCode.AppendLine("using System;");
        enhancedCode.AppendLine("using System.Linq;");
        enhancedCode.AppendLine();

        // 添加参数设置
        if (parameters != null && parameters.Any())
        {
            enhancedCode.AppendLine("// 设备参数");
            foreach (var param in parameters)
            {
                enhancedCode.AppendLine($"var {param.Key} = {JsonSerializer.Serialize(param.Value)};");
            }
            enhancedCode.AppendLine();
        }

        // 添加用户代码
        enhancedCode.AppendLine("// 用户代码");
        enhancedCode.AppendLine(code);

        return enhancedCode.ToString();
    }

    /// <summary>
    /// 尝试解析仪器数据
    /// </summary>
    private InstrumentDataResult? TryParseInstrumentData(object returnValue, string deviceType)
    {
        try
        {
            if (returnValue == null) return null;

            var json = returnValue.ToString();
            if (string.IsNullOrEmpty(json)) return null;

            var data = JsonSerializer.Deserialize<JsonElement>(json);

            return new InstrumentDataResult
            {
                DeviceType = deviceType,
                ChannelCount = GetJsonInt(data, "channels", "channelCount"),
                SampleCount = GetJsonInt(data, "samples", "sampleCount"),
                SampleRate = GetJsonDouble(data, "sampleRate"),
                Statistics = ParseStatistics(data),
                Timestamp = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "解析仪器数据失败");
            return null;
        }
    }

    /// <summary>
    /// 从 JSON 中获取整数值
    /// </summary>
    private int GetJsonInt(JsonElement data, params string[] propertyNames)
    {
        foreach (var name in propertyNames)
        {
            if (data.TryGetProperty(name, out var element) && element.ValueKind == JsonValueKind.Number)
            {
                return element.GetInt32();
            }
        }
        return 0;
    }

    /// <summary>
    /// 从 JSON 中获取双精度值
    /// </summary>
    private double GetJsonDouble(JsonElement data, string propertyName)
    {
        if (data.TryGetProperty(propertyName, out var element) && element.ValueKind == JsonValueKind.Number)
        {
            return element.GetDouble();
        }
        return 0.0;
    }

    /// <summary>
    /// 解析统计信息
    /// </summary>
    private DataStatistics? ParseStatistics(JsonElement data)
    {
        if (!data.TryGetProperty("statistics", out var statsElement))
            return null;

        return new DataStatistics
        {
            Min = GetJsonDouble(statsElement, "min"),
            Max = GetJsonDouble(statsElement, "max"),
            Average = GetJsonDouble(statsElement, "average"),
            StandardDeviation = GetJsonDouble(statsElement, "standardDeviation"),
            RMS = GetJsonDouble(statsElement, "rms")
        };
    }

    #endregion

    #region 代码模板

    /// <summary>
    /// JY5500 代码模板
    /// </summary>
    private string GetJY5500Template(string templateType)
    {
        return templateType switch
        {
            "basic" => @"// JY5500 基础数据采集
var jy5500 = new JY5500AITask(""JY5500"");
jy5500.Channels.AddRange(JY5500AIChannel.CreateByPhysicalChannel(""0:7""));
jy5500.SampleRate = 1000;
jy5500.Mode = JY5500AIMode.Continuous;

// 开始采集
jy5500.Start();

// 采集 1000 个样本
var data = jy5500.ReadData(1000);

Console.WriteLine($""采集到 {data.GetLength(0)} 个通道，{data.GetLength(1)} 个样本"");

// 返回结构化数据
return new {
    deviceType = ""JY5500"",
    channels = data.GetLength(0),
    samples = data.GetLength(1),
    sampleRate = 1000,
    data = data,
    timestamp = DateTime.Now
};",

            "triggered" => @"// JY5500 触发采集
var jy5500 = new JY5500AITask(""JY5500"");
jy5500.Channels.AddRange(JY5500AIChannel.CreateByPhysicalChannel(""0:3""));
jy5500.SampleRate = 2000;
jy5500.SamplesToAcquire = 2000;

// 配置触发
jy5500.TriggerParameters.TriggerSource = AITriggerSource.AnalogEdge;
jy5500.TriggerParameters.TriggerEdge = AITriggerEdge.Rising;
jy5500.TriggerParameters.TriggerLevel = 1.0;

jy5500.Start();
jy5500.WaitUntilDone();

var data = jy5500.ReadData(jy5500.SamplesToAcquire);
jy5500.Stop();

return new {
    deviceType = ""JY5500"",
    channelCount = data.GetLength(0),
    sampleCount = data.GetLength(1),
    sampleRate = 2000,
    rawData = data
};",

            _ => GetJY5500Template("basic")
        };
    }

    /// <summary>
    /// JYUSB1601 代码模板
    /// </summary>
    private string GetJYUSB1601Template(string templateType)
    {
        return templateType switch
        {
            "basic" => @"// JYUSB1601 基础数据采集
// 创建 AI 任务，使用设备索引 ""0""
var aiTask = new JYUSB1601AITask(""0"");

try
{
    // 添加通道 0，电压范围 ±10V
    aiTask.AddChannel(0, -10, 10);
    
    // 配置为单点采集模式
    aiTask.Mode = AIMode.Single;
    
    // 启动任务
    aiTask.Start();
    
    // 读取单点数据
    double readValue = 0;
    aiTask.ReadSinglePoint(ref readValue, 0);
    
    Console.WriteLine($""通道 0 采集到电压值: {readValue:F3} V"");
    
    // 停止任务
    aiTask.Stop();
    
    return new {
        deviceType = ""JYUSB1601"",
        mode = ""Single"",
        channelCount = 1,
        sampleCount = 1,
        voltage = readValue,
        unit = ""V"",
        timestamp = DateTime.Now
    };
}
catch (Exception ex)
{
    Console.WriteLine($""错误: {ex.Message}"");
    return new {
        deviceType = ""JYUSB1601"",
        error = ex.Message,
        timestamp = DateTime.Now
    };
}
finally
{
    // 清理通道
    aiTask?.Channels.Clear();
}",

            "continuous" => @"// JYUSB1601 连续多通道数据采集
// 创建 AI 任务
var aiTask = new JYUSB1601AITask(""0"");

try
{
    // 添加多个通道，电压范围 ±10V
    for (int i = 0; i < 4; i++)
    {
        aiTask.AddChannel(i, -10, 10);
    }
    
    // 配置连续采集模式
    aiTask.Mode = AIMode.Continuous;
    aiTask.SampleRate = 1000; // 1kHz 采样率
    
    // 启动任务
    aiTask.Start();
    
    // 等待缓冲区有足够数据
    int samplesToRead = 1000;
    while (aiTask.AvailableSamples < (ulong)samplesToRead)
    {
        System.Threading.Thread.Sleep(10);
    }
    
    // 读取数据
    var data = new double[samplesToRead, aiTask.Channels.Count];
    aiTask.ReadData(ref data, samplesToRead, -1);
    
    // 停止任务
    aiTask.Stop();
    
    Console.WriteLine($""采集完成: {aiTask.Channels.Count} 通道，{samplesToRead} 样本"");
    
    // 计算统计信息
    var allValues = new List<double>();
    for (int ch = 0; ch < data.GetLength(1); ch++)
    {
        for (int sample = 0; sample < data.GetLength(0); sample++)
        {
            allValues.Add(data[sample, ch]);
        }
    }
    
    return new {
        deviceType = ""JYUSB1601"",
        mode = ""Continuous"",
        channelCount = aiTask.Channels.Count,
        sampleCount = samplesToRead,
        sampleRate = 1000,
        data = data,
        statistics = new {
            min = allValues.Min(),
            max = allValues.Max(),
            average = allValues.Average(),
            rms = Math.Sqrt(allValues.Average(v => v * v))
        },
        timestamp = DateTime.Now
    };
}
catch (Exception ex)
{
    Console.WriteLine($""错误: {ex.Message}"");
    return new {
        deviceType = ""JYUSB1601"",
        error = ex.Message,
        timestamp = DateTime.Now
    };
}
finally
{
    // 清理通道
    aiTask?.Channels.Clear();
}",

            "finite" => @"// JYUSB1601 有限采样数据采集
// 创建 AI 任务
var aiTask = new JYUSB1601AITask(""0"");

try
{
    // 添加通道
    aiTask.AddChannel(0, -10, 10);
    aiTask.AddChannel(1, -10, 10);
    
    // 配置有限采样模式
    aiTask.Mode = AIMode.Finite;
    aiTask.SampleRate = 2000; // 2kHz 采样率
    aiTask.SamplesToAcquire = 2000; // 采集 2000 个样本
    
    // 启动任务
    aiTask.Start();
    
    // 等待采集完成
    while (!aiTask.IsDone)
    {
        System.Threading.Thread.Sleep(10);
    }
    
    // 读取所有数据
    var data = new double[aiTask.SamplesToAcquire, aiTask.Channels.Count];
    aiTask.ReadData(ref data, aiTask.SamplesToAcquire, -1);
    
    // 停止任务
    aiTask.Stop();
    
    Console.WriteLine($""有限采样完成: {aiTask.Channels.Count} 通道，{aiTask.SamplesToAcquire} 样本"");
    
    // 按通道计算统计信息
    var channelStats = new object[aiTask.Channels.Count];
    for (int ch = 0; ch < aiTask.Channels.Count; ch++)
    {
        var channelData = new double[data.GetLength(0)];
        for (int i = 0; i < data.GetLength(0); i++)
        {
            channelData[i] = data[i, ch];
        }
        
        channelStats[ch] = new {
            channel = ch,
            min = channelData.Min(),
            max = channelData.Max(),
            average = channelData.Average(),
            rms = Math.Sqrt(channelData.Average(v => v * v))
        };
    }
    
    return new {
        deviceType = ""JYUSB1601"",
        mode = ""Finite"",
        channelCount = aiTask.Channels.Count,
        sampleCount = aiTask.SamplesToAcquire,
        sampleRate = 2000,
        data = data,
        channelStatistics = channelStats,
        timestamp = DateTime.Now
    };
}
catch (Exception ex)
{
    Console.WriteLine($""错误: {ex.Message}"");
    return new {
        deviceType = ""JYUSB1601"",
        error = ex.Message,
        timestamp = DateTime.Now
    };
}
finally
{
    // 清理通道
    aiTask?.Channels.Clear();
}",

            _ => GetJYUSB1601Template("basic")
        };
    }

    /// <summary>
    /// 通用代码模板
    /// </summary>
    private string GetGenericTemplate(string templateType)
    {
        return @"// 简单示例代码
Console.WriteLine(""Hello from C# Runner!"");

// 生成示例数据
var random = new Random();
var data = new double[4, 1000];

for (int ch = 0; ch < 4; ch++)
{
    for (int i = 0; i < 1000; i++)
    {
        data[ch, i] = Math.Sin(2 * Math.PI * (ch + 1) * i / 100.0) + random.NextDouble() * 0.1;
    }
}

return new {
    deviceType = ""Simulated"",
    channelCount = 4,
    sampleCount = 1000,
    sampleRate = 1000,
    data = data
};";
    }

    #endregion
}

/// <summary>
/// C# Runner 配置选项
/// </summary>
public class CSharpRunnerOptions
{
    /// <summary>
    /// 服务 URL（默认：http://localhost:5050）
    /// </summary>
    public string ServiceUrl { get; set; } = "http://localhost:5050";

    /// <summary>
    /// 默认超时时间（秒）
    /// </summary>
    public int DefaultTimeout { get; set; } = 60;

    /// <summary>
    /// 是否启用详细日志
    /// </summary>
    public bool EnableVerboseLogging { get; set; } = false;
}

/// <summary>
/// 结束响应数据结构
/// </summary>
public class EndResponse
{
    public long Elapsed { get; set; }
    public string StdOutput { get; set; } = string.Empty;
    public string StdError { get; set; } = string.Empty;
}
