using System.Diagnostics;

namespace SeeSharpBackend.Services.Monitoring
{
    /// <summary>
    /// 性能监控服务接口
    /// </summary>
    public interface IPerformanceMonitoringService
    {
        /// <summary>
        /// 获取系统性能指标
        /// </summary>
        Task<SystemPerformanceMetrics> GetSystemMetricsAsync();

        /// <summary>
        /// 获取应用程序性能指标
        /// </summary>
        Task<ApplicationPerformanceMetrics> GetApplicationMetricsAsync();

        /// <summary>
        /// 获取数据采集性能指标
        /// </summary>
        Task<DataAcquisitionMetrics> GetDataAcquisitionMetricsAsync();

        /// <summary>
        /// 记录自定义指标
        /// </summary>
        void RecordCustomMetric(string name, double value, Dictionary<string, string>? labels = null);

        /// <summary>
        /// 增加计数器
        /// </summary>
        void IncrementCounter(string name, Dictionary<string, string>? labels = null);

        /// <summary>
        /// 记录直方图数据
        /// </summary>
        void RecordHistogram(string name, double value, Dictionary<string, string>? labels = null);

        /// <summary>
        /// 开始计时
        /// </summary>
        IDisposable StartTimer(string name, Dictionary<string, string>? labels = null);
    }

    /// <summary>
    /// 系统性能指标
    /// </summary>
    public class SystemPerformanceMetrics
    {
        public double CpuUsagePercent { get; set; }
        public long MemoryUsedBytes { get; set; }
        public long MemoryTotalBytes { get; set; }
        public double MemoryUsagePercent { get; set; }
        public long DiskUsedBytes { get; set; }
        public long DiskTotalBytes { get; set; }
        public double DiskUsagePercent { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public TimeSpan Uptime { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 应用程序性能指标
    /// </summary>
    public class ApplicationPerformanceMetrics
    {
        public long WorkingSetBytes { get; set; }
        public long PrivateMemoryBytes { get; set; }
        public long GcTotalMemoryBytes { get; set; }
        public int GcGen0Collections { get; set; }
        public int GcGen1Collections { get; set; }
        public int GcGen2Collections { get; set; }
        public int ThreadPoolThreads { get; set; }
        public int ThreadPoolCompletionPortThreads { get; set; }
        public long RequestsPerSecond { get; set; }
        public double AverageResponseTimeMs { get; set; }
        public int ActiveConnections { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 数据采集性能指标
    /// </summary>
    public class DataAcquisitionMetrics
    {
        public long TotalSamplesAcquired { get; set; }
        public double SamplingRateHz { get; set; }
        public double DataThroughputMBps { get; set; }
        public int ActiveChannels { get; set; }
        public int BufferUtilizationPercent { get; set; }
        public int DroppedSamples { get; set; }
        public double AverageLatencyMs { get; set; }
        public int QueueDepth { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
