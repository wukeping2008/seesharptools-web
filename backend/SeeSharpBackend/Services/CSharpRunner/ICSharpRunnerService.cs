using System.ComponentModel;

namespace SeeSharpBackend.Services.CSharpRunner;

/// <summary>
/// C# Runner MCP 服务接口
/// 提供在线 C# 代码执行功能，支持简仪设备控制
/// </summary>
public interface ICSharpRunnerService
{
    /// <summary>
    /// 执行 C# 代码
    /// </summary>
    /// <param name="code">要执行的 C# 代码</param>
    /// <param name="timeout">超时时间（毫秒），默认30秒</param>
    /// <returns>执行结果</returns>
    Task<CSharpExecutionResult> ExecuteCodeAsync(string code, int timeout = 30000);

    /// <summary>
    /// 执行仪器控制相关的 C# 代码
    /// </summary>
    /// <param name="code">要执行的 C# 代码</param>
    /// <param name="deviceType">设备类型（JY5500, JYUSB1601等）</param>
    /// <param name="parameters">设备参数</param>
    /// <returns>执行结果</returns>
    Task<CSharpExecutionResult> ExecuteInstrumentCodeAsync(string code, string deviceType, Dictionary<string, object> parameters);

    /// <summary>
    /// 检查 C# Runner 服务是否可用
    /// </summary>
    /// <returns>服务是否可用</returns>
    Task<bool> IsServiceAvailableAsync();

    /// <summary>
    /// 获取预置代码模板
    /// </summary>
    /// <param name="deviceType">设备类型</param>
    /// <param name="templateType">模板类型</param>
    /// <returns>代码模板</returns>
    Task<string> GetCodeTemplateAsync(string deviceType, string templateType);
}

/// <summary>
/// C# 代码执行结果
/// </summary>
public class CSharpExecutionResult
{
    /// <summary>
    /// 执行是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 控制台输出
    /// </summary>
    public string ConsoleOutput { get; set; } = string.Empty;

    /// <summary>
    /// 错误输出
    /// </summary>
    public string ErrorOutput { get; set; } = string.Empty;

    /// <summary>
    /// 返回值（JSON 格式）
    /// </summary>
    public object? ReturnValue { get; set; }

    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    public long ElapsedMilliseconds { get; set; }

    /// <summary>
    /// 仪器数据（如果返回值包含仪器数据）
    /// </summary>
    public InstrumentDataResult? InstrumentData { get; set; }

    /// <summary>
    /// 执行过程中的实时消息
    /// </summary>
    public List<ExecutionMessage> Messages { get; set; } = new();
}

/// <summary>
/// 执行过程中的消息
/// </summary>
public class ExecutionMessage
{
    /// <summary>
    /// 消息类型（stdout, stderr, result, end）
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// 仪器数据结果
/// </summary>
public class InstrumentDataResult
{
    /// <summary>
    /// 设备类型
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 通道数
    /// </summary>
    public int ChannelCount { get; set; }

    /// <summary>
    /// 样本数
    /// </summary>
    public int SampleCount { get; set; }

    /// <summary>
    /// 采样率
    /// </summary>
    public double SampleRate { get; set; }

    /// <summary>
    /// 原始数据（二维数组：通道 x 样本）
    /// </summary>
    public double[,]? RawData { get; set; }

    /// <summary>
    /// 统计信息
    /// </summary>
    public DataStatistics? Statistics { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// 数据统计信息
/// </summary>
public class DataStatistics
{
    /// <summary>
    /// 最小值
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    public double Max { get; set; }

    /// <summary>
    /// 平均值
    /// </summary>
    public double Average { get; set; }

    /// <summary>
    /// 标准差
    /// </summary>
    public double StandardDeviation { get; set; }

    /// <summary>
    /// RMS 值
    /// </summary>
    public double RMS { get; set; }
}
