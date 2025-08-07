using SeeSharpBackend.Models.MISD;

namespace SeeSharpBackend.Services.DataAcquisition
{
    /// <summary>
    /// 高性能多线程数据采集引擎接口
    /// 支持多设备并发采集、实时数据流处理、内存优化管理
    /// </summary>
    public interface IDataAcquisitionEngine
    {
        /// <summary>
        /// 启动数据采集任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="config">采集配置</param>
        /// <returns>启动结果</returns>
        Task<bool> StartAcquisitionAsync(int taskId, AcquisitionConfiguration config);

        /// <summary>
        /// 停止数据采集任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>停止结果</returns>
        Task<bool> StopAcquisitionAsync(int taskId);

        /// <summary>
        /// 暂停数据采集任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>暂停结果</returns>
        Task<bool> PauseAcquisitionAsync(int taskId);

        /// <summary>
        /// 恢复数据采集任务
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>恢复结果</returns>
        Task<bool> ResumeAcquisitionAsync(int taskId);

        /// <summary>
        /// 获取任务状态
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>任务状态</returns>
        Task<AcquisitionTaskStatus> GetTaskStatusAsync(int taskId);

        /// <summary>
        /// 获取实时数据流
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>数据流</returns>
        IAsyncEnumerable<DataPacket> GetDataStreamAsync(int taskId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取缓冲区状态
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>缓冲区状态</returns>
        Task<BufferStatus> GetBufferStatusAsync(int taskId);

        /// <summary>
        /// 配置缓冲区
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="bufferConfig">缓冲区配置</param>
        /// <returns>配置结果</returns>
        Task<bool> ConfigureBufferAsync(int taskId, BufferConfiguration bufferConfig);

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>清空结果</returns>
        Task<bool> ClearBufferAsync(int taskId);

        /// <summary>
        /// 获取性能统计
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>性能统计</returns>
        Task<AcquisitionPerformanceStats> GetPerformanceStatsAsync(int taskId);

        /// <summary>
        /// 获取所有活跃任务
        /// </summary>
        /// <returns>活跃任务列表</returns>
        Task<IEnumerable<int>> GetActiveTasksAsync();

        /// <summary>
        /// 设置数据质量检查
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="qualityConfig">质量检查配置</param>
        /// <returns>设置结果</returns>
        Task<bool> SetDataQualityCheckAsync(int taskId, DataQualityConfiguration qualityConfig);

        /// <summary>
        /// 获取数据质量报告
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <returns>质量报告</returns>
        Task<DataQualityReport> GetDataQualityReportAsync(int taskId);
    }

    /// <summary>
    /// 数据采集配置
    /// </summary>
    public class AcquisitionConfiguration
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// 采样率 (Hz)
        /// </summary>
        public double SampleRate { get; set; }

        /// <summary>
        /// 通道配置
        /// </summary>
        public List<ChannelConfiguration> Channels { get; set; } = new();

        /// <summary>
        /// 采集模式
        /// </summary>
        public AcquisitionMode Mode { get; set; } = AcquisitionMode.Continuous;

        /// <summary>
        /// 采样点数（有限模式）
        /// </summary>
        public long? SamplesToAcquire { get; set; }

        /// <summary>
        /// 触发配置
        /// </summary>
        public TriggerConfiguration? TriggerConfig { get; set; }

        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public int BufferSize { get; set; } = 10000;

        /// <summary>
        /// 线程优先级
        /// </summary>
        public System.Threading.ThreadPriority ThreadPriority { get; set; } = System.Threading.ThreadPriority.AboveNormal;

        /// <summary>
        /// 启用数据压缩
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// 启用数据质量检查
        /// </summary>
        public bool EnableQualityCheck { get; set; } = true;

        /// <summary>
        /// 自发自收测试模式
        /// </summary>
        public bool SelfTestMode { get; set; } = false;
    }

    /// <summary>
    /// 通道配置
    /// </summary>
    public class ChannelConfiguration
    {
        /// <summary>
        /// 通道ID
        /// </summary>
        public int ChannelId { get; set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 量程最小值
        /// </summary>
        public double RangeMin { get; set; } = -10.0;

        /// <summary>
        /// 量程最大值
        /// </summary>
        public double RangeMax { get; set; } = 10.0;

        /// <summary>
        /// 耦合方式
        /// </summary>
        public CouplingMode Coupling { get; set; } = CouplingMode.DC;

        /// <summary>
        /// 输入阻抗
        /// </summary>
        public InputImpedance Impedance { get; set; } = InputImpedance.HighZ;

        /// <summary>
        /// 校准偏移
        /// </summary>
        public double CalibrationOffset { get; set; } = 0.0;

        /// <summary>
        /// 校准增益
        /// </summary>
        public double CalibrationGain { get; set; } = 1.0;
    }

    /// <summary>
    /// 触发配置
    /// </summary>
    public class TriggerConfiguration
    {
        /// <summary>
        /// 触发类型
        /// </summary>
        public TriggerType Type { get; set; } = TriggerType.Immediate;

        /// <summary>
        /// 触发源通道
        /// </summary>
        public int SourceChannel { get; set; } = 0;

        /// <summary>
        /// 触发电平
        /// </summary>
        public double Level { get; set; } = 0.0;

        /// <summary>
        /// 触发斜率
        /// </summary>
        public TriggerSlope Slope { get; set; } = TriggerSlope.Rising;

        /// <summary>
        /// 预触发样本数
        /// </summary>
        public int PreTriggerSamples { get; set; } = 0;

        /// <summary>
        /// 触发延迟 (秒)
        /// </summary>
        public double Delay { get; set; } = 0.0;

        /// <summary>
        /// 触发超时 (秒)
        /// </summary>
        public double Timeout { get; set; } = 10.0;
    }

    /// <summary>
    /// 缓冲区配置
    /// </summary>
    public class BufferConfiguration
    {
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public int Size { get; set; } = 10000;

        /// <summary>
        /// 缓冲区类型
        /// </summary>
        public BufferType Type { get; set; } = BufferType.Circular;

        /// <summary>
        /// 溢出处理策略
        /// </summary>
        public OverflowStrategy OverflowStrategy { get; set; } = OverflowStrategy.Overwrite;

        /// <summary>
        /// 低水位标记 (%)
        /// </summary>
        public double LowWaterMark { get; set; } = 25.0;

        /// <summary>
        /// 高水位标记 (%)
        /// </summary>
        public double HighWaterMark { get; set; } = 75.0;

        /// <summary>
        /// 启用内存池
        /// </summary>
        public bool EnableMemoryPool { get; set; } = true;
    }

    /// <summary>
    /// 数据质量配置
    /// </summary>
    public class DataQualityConfiguration
    {
        /// <summary>
        /// 启用CRC校验
        /// </summary>
        public bool EnableCrcCheck { get; set; } = true;

        /// <summary>
        /// 启用时间戳验证
        /// </summary>
        public bool EnableTimestampValidation { get; set; } = true;

        /// <summary>
        /// 启用丢包检测
        /// </summary>
        public bool EnablePacketLossDetection { get; set; } = true;

        /// <summary>
        /// 启用数据范围检查
        /// </summary>
        public bool EnableRangeCheck { get; set; } = true;

        /// <summary>
        /// 最大允许丢包率 (%)
        /// </summary>
        public double MaxPacketLossRate { get; set; } = 0.1;

        /// <summary>
        /// 最大时间戳偏差 (ms)
        /// </summary>
        public double MaxTimestampDeviation { get; set; } = 1.0;
    }

    /// <summary>
    /// 数据包
    /// </summary>
    public class DataPacket
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        public long SequenceNumber { get; set; }

        /// <summary>
        /// 通道数据
        /// </summary>
        public Dictionary<int, double[]> ChannelData { get; set; } = new();

        /// <summary>
        /// 样本数
        /// </summary>
        public int SampleCount { get; set; }

        /// <summary>
        /// 采样率
        /// </summary>
        public double SampleRate { get; set; }

        /// <summary>
        /// 数据质量标志
        /// </summary>
        public DataQualityFlags QualityFlags { get; set; } = DataQualityFlags.Good;

        /// <summary>
        /// CRC校验值
        /// </summary>
        public uint Crc32 { get; set; }

        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// 任务状态
    /// </summary>
    public class AcquisitionTaskStatus
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public Models.MISD.TaskStatus Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// 停止时间
        /// </summary>
        public DateTime? StoppedAt { get; set; }

        /// <summary>
        /// 已采集样本数
        /// </summary>
        public long SamplesAcquired { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 配置信息
        /// </summary>
        public AcquisitionConfiguration? Configuration { get; set; }
    }

    /// <summary>
    /// 缓冲区状态
    /// </summary>
    public class BufferStatus
    {
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// 可用样本数
        /// </summary>
        public int AvailableSamples { get; set; }

        /// <summary>
        /// 已传输样本数
        /// </summary>
        public long TransferredSamples { get; set; }

        /// <summary>
        /// 是否溢出
        /// </summary>
        public bool IsOverflow { get; set; }

        /// <summary>
        /// 使用率 (%)
        /// </summary>
        public double UsagePercentage => BufferSize > 0 ? (double)AvailableSamples / BufferSize * 100 : 0;

        /// <summary>
        /// 溢出次数
        /// </summary>
        public int OverflowCount { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// 性能统计
    /// </summary>
    public class AcquisitionPerformanceStats
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// 平均采样率 (Hz)
        /// </summary>
        public double AverageSampleRate { get; set; }

        /// <summary>
        /// 实际采样率 (Hz)
        /// </summary>
        public double ActualSampleRate { get; set; }

        /// <summary>
        /// 数据吞吐量 (MB/s)
        /// </summary>
        public double DataThroughput { get; set; }

        /// <summary>
        /// CPU使用率 (%)
        /// </summary>
        public double CpuUsage { get; set; }

        /// <summary>
        /// 内存使用量 (MB)
        /// </summary>
        public double MemoryUsage { get; set; }

        /// <summary>
        /// 丢包率 (%)
        /// </summary>
        public double PacketLossRate { get; set; }

        /// <summary>
        /// 平均延迟 (ms)
        /// </summary>
        public double AverageLatency { get; set; }

        /// <summary>
        /// 最大延迟 (ms)
        /// </summary>
        public double MaxLatency { get; set; }

        /// <summary>
        /// 线程数
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// 统计时间窗口 (秒)
        /// </summary>
        public double StatisticsWindow { get; set; } = 60.0;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// 数据质量报告
    /// </summary>
    public class DataQualityReport
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// 总样本数
        /// </summary>
        public long TotalSamples { get; set; }

        /// <summary>
        /// 有效样本数
        /// </summary>
        public long ValidSamples { get; set; }

        /// <summary>
        /// CRC错误数
        /// </summary>
        public long CrcErrors { get; set; }

        /// <summary>
        /// 时间戳错误数
        /// </summary>
        public long TimestampErrors { get; set; }

        /// <summary>
        /// 丢包数
        /// </summary>
        public long PacketLoss { get; set; }

        /// <summary>
        /// 范围错误数
        /// </summary>
        public long RangeErrors { get; set; }

        /// <summary>
        /// 数据质量评分 (0-100)
        /// </summary>
        public double QualityScore => TotalSamples > 0 ? (double)ValidSamples / TotalSamples * 100 : 0;

        /// <summary>
        /// 报告生成时间
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// 详细错误信息
        /// </summary>
        public List<string> ErrorDetails { get; set; } = new();
    }

    /// <summary>
    /// 采集模式
    /// </summary>
    public enum AcquisitionMode
    {
        /// <summary>
        /// 连续采集
        /// </summary>
        Continuous,

        /// <summary>
        /// 有限采集
        /// </summary>
        Finite,

        /// <summary>
        /// 触发采集
        /// </summary>
        Triggered
    }

    /// <summary>
    /// 耦合方式
    /// </summary>
    public enum CouplingMode
    {
        /// <summary>
        /// 直流耦合
        /// </summary>
        DC,

        /// <summary>
        /// 交流耦合
        /// </summary>
        AC,

        /// <summary>
        /// 接地
        /// </summary>
        Ground
    }

    /// <summary>
    /// 输入阻抗
    /// </summary>
    public enum InputImpedance
    {
        /// <summary>
        /// 高阻抗 (1MΩ)
        /// </summary>
        HighZ,

        /// <summary>
        /// 50欧姆
        /// </summary>
        Ohm50,

        /// <summary>
        /// 75欧姆
        /// </summary>
        Ohm75
    }

    /// <summary>
    /// 触发类型
    /// </summary>
    public enum TriggerType
    {
        /// <summary>
        /// 立即触发
        /// </summary>
        Immediate,

        /// <summary>
        /// 边沿触发
        /// </summary>
        Edge,

        /// <summary>
        /// 电平触发
        /// </summary>
        Level,

        /// <summary>
        /// 脉宽触发
        /// </summary>
        PulseWidth,

        /// <summary>
        /// 外部触发
        /// </summary>
        External
    }

    /// <summary>
    /// 触发斜率
    /// </summary>
    public enum TriggerSlope
    {
        /// <summary>
        /// 上升沿
        /// </summary>
        Rising,

        /// <summary>
        /// 下降沿
        /// </summary>
        Falling,

        /// <summary>
        /// 双边沿
        /// </summary>
        Both
    }

    /// <summary>
    /// 缓冲区类型
    /// </summary>
    public enum BufferType
    {
        /// <summary>
        /// 环形缓冲区
        /// </summary>
        Circular,

        /// <summary>
        /// 线性缓冲区
        /// </summary>
        Linear,

        /// <summary>
        /// 多级缓冲区
        /// </summary>
        MultiLevel
    }

    /// <summary>
    /// 溢出策略
    /// </summary>
    public enum OverflowStrategy
    {
        /// <summary>
        /// 覆盖旧数据
        /// </summary>
        Overwrite,

        /// <summary>
        /// 丢弃新数据
        /// </summary>
        Drop,

        /// <summary>
        /// 停止采集
        /// </summary>
        Stop,

        /// <summary>
        /// 触发警告
        /// </summary>
        Warning
    }

    /// <summary>
    /// 数据质量标志
    /// </summary>
    [Flags]
    public enum DataQualityFlags
    {
        /// <summary>
        /// 数据良好
        /// </summary>
        Good = 0,

        /// <summary>
        /// CRC错误
        /// </summary>
        CrcError = 1,

        /// <summary>
        /// 时间戳错误
        /// </summary>
        TimestampError = 2,

        /// <summary>
        /// 丢包
        /// </summary>
        PacketLoss = 4,

        /// <summary>
        /// 范围错误
        /// </summary>
        RangeError = 8,

        /// <summary>
        /// 溢出
        /// </summary>
        Overflow = 16,

        /// <summary>
        /// 硬件错误
        /// </summary>
        HardwareError = 32
    }
}
