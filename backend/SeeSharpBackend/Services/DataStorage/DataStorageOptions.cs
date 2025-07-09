using System;

namespace SeeSharpBackend.Services.DataStorage
{
    /// <summary>
    /// 数据存储配置选项
    /// </summary>
    public class DataStorageOptions
    {
        /// <summary>
        /// 存储基础路径
        /// </summary>
        public string StorageBasePath { get; set; } = "Data/Storage";

        /// <summary>
        /// 最大并发写入数
        /// </summary>
        public int MaxConcurrentWrites { get; set; } = 10;

        /// <summary>
        /// 批量写入并发数
        /// </summary>
        public int BatchWriteConcurrency { get; set; } = 5;

        /// <summary>
        /// 压缩阈值（字节）
        /// </summary>
        public int CompressionThreshold { get; set; } = 1024;

        /// <summary>
        /// 优化间隔（分钟）
        /// </summary>
        public int OptimizationIntervalMinutes { get; set; } = 60;

        /// <summary>
        /// 数据保留天数
        /// </summary>
        public int DataRetentionDays { get; set; } = 30;

        /// <summary>
        /// 最大文件大小（MB）
        /// </summary>
        public int MaxFileSizeMB { get; set; } = 100;

        /// <summary>
        /// 是否启用自动清理
        /// </summary>
        public bool EnableAutoCleanup { get; set; } = true;

        /// <summary>
        /// 是否启用数据压缩
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// 缓存大小（MB）
        /// </summary>
        public int CacheSizeMB { get; set; } = 256;
    }

    /// <summary>
    /// 任务存储上下文
    /// </summary>
    public class TaskStorageContext
    {
        public int TaskId { get; set; }
        public string StoragePath { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public long TotalBytes { get; set; }
        public long CompressedBytes { get; set; }
        public int DataPackets { get; set; }
        public DateTime LastWriteTime { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 数据点
    /// </summary>
    public class DataPoint
    {
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public int ChannelId { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
