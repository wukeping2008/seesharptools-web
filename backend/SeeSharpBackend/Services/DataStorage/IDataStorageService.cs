using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeeSharpBackend.Services.DataStorage
{
    /// <summary>
    /// 数据存储服务接口
    /// 提供高速数据写入、压缩存储、分布式存储等功能
    /// </summary>
    public interface IDataStorageService
    {
        /// <summary>
        /// 写入实时数据
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="data">数据包</param>
        /// <param name="compress">是否压缩</param>
        /// <returns>写入结果</returns>
        Task<DataWriteResult> WriteDataAsync(int taskId, RealTimeDataPacket data, bool compress = true);

        /// <summary>
        /// 批量写入数据
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="dataPackets">数据包列表</param>
        /// <param name="compress">是否压缩</param>
        /// <returns>批量写入结果</returns>
        Task<BatchWriteResult> WriteBatchDataAsync(int taskId, IEnumerable<RealTimeDataPacket> dataPackets, bool compress = true);

        /// <summary>
        /// 查询历史数据
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="channels">通道列表（null表示所有通道）</param>
        /// <param name="maxPoints">最大数据点数（用于降采样）</param>
        /// <returns>历史数据</returns>
        Task<HistoricalDataResult> QueryHistoricalDataAsync(int taskId, DateTime startTime, DateTime endTime, 
            int[]? channels = null, int maxPoints = 10000);

        /// <summary>
        /// 获取数据统计信息
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>数据统计</returns>
        Task<DataStatistics> GetDataStatisticsAsync(int taskId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 删除历史数据
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="beforeTime">删除此时间之前的数据</param>
        /// <returns>删除结果</returns>
        Task<DataDeleteResult> DeleteDataAsync(int taskId, DateTime beforeTime);

        /// <summary>
        /// 获取存储状态
        /// </summary>
        /// <returns>存储状态信息</returns>
        Task<StorageStatus> GetStorageStatusAsync();

        /// <summary>
        /// 优化存储（压缩、整理碎片等）
        /// </summary>
        /// <param name="taskId">任务ID（null表示所有任务）</param>
        /// <returns>优化结果</returns>
        Task<StorageOptimizationResult> OptimizeStorageAsync(int? taskId = null);
    }

    /// <summary>
    /// 实时数据包
    /// </summary>
    public class RealTimeDataPacket
    {
        public int TaskId { get; set; }
        public DateTime Timestamp { get; set; }
        public int SequenceNumber { get; set; }
        public Dictionary<int, double[]> ChannelData { get; set; } = new();
        public double SampleRate { get; set; }
        public int SampleCount { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// 数据写入结果
    /// </summary>
    public class DataWriteResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public long BytesWritten { get; set; }
        public long CompressedSize { get; set; }
        public double CompressionRatio { get; set; }
        public TimeSpan WriteTime { get; set; }
        public string? StorageLocation { get; set; }
    }

    /// <summary>
    /// 批量写入结果
    /// </summary>
    public class BatchWriteResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int TotalPackets { get; set; }
        public int SuccessfulPackets { get; set; }
        public int FailedPackets { get; set; }
        public long TotalBytesWritten { get; set; }
        public long TotalCompressedSize { get; set; }
        public double AverageCompressionRatio { get; set; }
        public TimeSpan TotalWriteTime { get; set; }
        public List<string> FailedPacketErrors { get; set; } = new();
    }

    /// <summary>
    /// 历史数据结果
    /// </summary>
    public class HistoricalDataResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Dictionary<int, ChannelData> Channels { get; set; } = new();
        public int TotalPoints { get; set; }
        public int ReturnedPoints { get; set; }
        public bool IsDownsampled { get; set; }
        public string? DownsamplingMethod { get; set; }
        public TimeSpan QueryTime { get; set; }
    }

    /// <summary>
    /// 通道数据
    /// </summary>
    public class ChannelData
    {
        public int ChannelId { get; set; }
        public string? ChannelName { get; set; }
        public double[] Timestamps { get; set; } = Array.Empty<double>();
        public double[] Values { get; set; } = Array.Empty<double>();
        public string? Unit { get; set; }
        public double SampleRate { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// 数据统计信息
    /// </summary>
    public class DataStatistics
    {
        public int TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public long TotalSamples { get; set; }
        public long TotalBytes { get; set; }
        public long CompressedBytes { get; set; }
        public double CompressionRatio { get; set; }
        public Dictionary<int, ChannelStatistics> ChannelStats { get; set; } = new();
        public double AverageSampleRate { get; set; }
        public int DataPackets { get; set; }
    }

    /// <summary>
    /// 通道统计信息
    /// </summary>
    public class ChannelStatistics
    {
        public int ChannelId { get; set; }
        public long SampleCount { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double AverageValue { get; set; }
        public double RmsValue { get; set; }
        public double StandardDeviation { get; set; }
    }

    /// <summary>
    /// 数据删除结果
    /// </summary>
    public class DataDeleteResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int TaskId { get; set; }
        public DateTime DeletedBefore { get; set; }
        public long DeletedSamples { get; set; }
        public long DeletedBytes { get; set; }
        public TimeSpan DeleteTime { get; set; }
    }

    /// <summary>
    /// 存储状态
    /// </summary>
    public class StorageStatus
    {
        public long TotalCapacity { get; set; }
        public long UsedSpace { get; set; }
        public long AvailableSpace { get; set; }
        public double UsagePercentage { get; set; }
        public int ActiveTasks { get; set; }
        public long TotalSamples { get; set; }
        public long TotalBytes { get; set; }
        public double AverageCompressionRatio { get; set; }
        public Dictionary<int, TaskStorageInfo> TaskInfo { get; set; } = new();
        public StoragePerformanceMetrics Performance { get; set; } = new();
    }

    /// <summary>
    /// 任务存储信息
    /// </summary>
    public class TaskStorageInfo
    {
        public int TaskId { get; set; }
        public string? TaskName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public long TotalSamples { get; set; }
        public long TotalBytes { get; set; }
        public long CompressedBytes { get; set; }
        public double CompressionRatio { get; set; }
        public int ChannelCount { get; set; }
        public double AverageSampleRate { get; set; }
    }

    /// <summary>
    /// 存储性能指标
    /// </summary>
    public class StoragePerformanceMetrics
    {
        public double WriteSpeed { get; set; } // MB/s
        public double ReadSpeed { get; set; } // MB/s
        public double CompressionSpeed { get; set; } // MB/s
        public double AverageWriteLatency { get; set; } // ms
        public double AverageReadLatency { get; set; } // ms
        public long TotalWrites { get; set; }
        public long TotalReads { get; set; }
        public long FailedWrites { get; set; }
        public long FailedReads { get; set; }
    }

    /// <summary>
    /// 存储优化结果
    /// </summary>
    public class StorageOptimizationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int? TaskId { get; set; }
        public long BytesFreed { get; set; }
        public long BytesCompressed { get; set; }
        public double CompressionImprovement { get; set; }
        public TimeSpan OptimizationTime { get; set; }
        public List<string> OptimizationActions { get; set; } = new();
    }
}
