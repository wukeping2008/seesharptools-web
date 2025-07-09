using System.Diagnostics;

namespace SeeSharpBackend.Services.DataAcquisition
{
    /// <summary>
    /// 数据采集性能监控器
    /// 监控采集任务的性能指标和统计信息
    /// </summary>
    public class AcquisitionPerformanceMonitor
    {
        private readonly int _taskId;
        private readonly object _lock = new();
        private readonly Stopwatch _stopwatch;
        private readonly Queue<DataPacketInfo> _recentPackets;
        private readonly int _maxRecentPackets = 1000;

        // 统计数据
        private long _totalPackets = 0;
        private long _totalSamples = 0;
        private long _totalBytes = 0;
        private double _totalLatency = 0;
        private double _maxLatency = 0;
        private DateTime _lastUpdateTime = DateTime.UtcNow;

        // 性能计算
        private Process? _currentProcess;
        private DateTime _lastCpuTime = DateTime.UtcNow;
        private TimeSpan _lastTotalProcessorTime = TimeSpan.Zero;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskId">任务ID</param>
        public AcquisitionPerformanceMonitor(int taskId)
        {
            _taskId = taskId;
            _stopwatch = Stopwatch.StartNew();
            _recentPackets = new Queue<DataPacketInfo>();

            try
            {
                // 初始化进程信息
                _currentProcess = Process.GetCurrentProcess();
                _lastTotalProcessorTime = _currentProcess.TotalProcessorTime;
            }
            catch (Exception)
            {
                // 进程信息初始化失败
                _currentProcess = null;
            }
        }

        /// <summary>
        /// 记录数据包
        /// </summary>
        /// <param name="dataPacket">数据包</param>
        public void RecordDataPacket(DataPacket dataPacket)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var latency = (now - dataPacket.Timestamp).TotalMilliseconds;

                // 记录基本统计
                _totalPackets++;
                _totalSamples += dataPacket.SampleCount;
                _totalBytes += EstimatePacketSize(dataPacket);
                _totalLatency += latency;
                _maxLatency = Math.Max(_maxLatency, latency);

                // 记录最近的数据包信息
                var packetInfo = new DataPacketInfo
                {
                    Timestamp = now,
                    SampleCount = dataPacket.SampleCount,
                    Latency = latency,
                    Size = EstimatePacketSize(dataPacket)
                };

                _recentPackets.Enqueue(packetInfo);

                // 保持队列大小
                while (_recentPackets.Count > _maxRecentPackets)
                {
                    _recentPackets.Dequeue();
                }

                _lastUpdateTime = now;
            }
        }

        /// <summary>
        /// 更新性能统计
        /// </summary>
        public void UpdateStats()
        {
            lock (_lock)
            {
                // 这里可以添加定期更新的统计计算
                // 例如清理过期的数据包信息等
                var cutoffTime = DateTime.UtcNow.AddMinutes(-5);
                
                while (_recentPackets.Count > 0 && _recentPackets.Peek().Timestamp < cutoffTime)
                {
                    _recentPackets.Dequeue();
                }
            }
        }

        /// <summary>
        /// 获取性能统计
        /// </summary>
        /// <returns>性能统计信息</returns>
        public AcquisitionPerformanceStats GetStats()
        {
            lock (_lock)
            {
                var elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
                var recentPackets = _recentPackets.ToArray();
                
                // 计算最近1分钟的统计
                var recentCutoff = DateTime.UtcNow.AddMinutes(-1);
                var recentPacketsInWindow = recentPackets.Where(p => p.Timestamp >= recentCutoff).ToArray();

                var averageSampleRate = elapsedSeconds > 0 ? _totalSamples / elapsedSeconds : 0;
                var actualSampleRate = CalculateActualSampleRate(recentPacketsInWindow);
                var dataThroughput = CalculateDataThroughput(recentPacketsInWindow);
                var averageLatency = _totalPackets > 0 ? _totalLatency / _totalPackets : 0;

                return new AcquisitionPerformanceStats
                {
                    TaskId = _taskId,
                    AverageSampleRate = averageSampleRate,
                    ActualSampleRate = actualSampleRate,
                    DataThroughput = dataThroughput,
                    CpuUsage = GetCpuUsage(),
                    MemoryUsage = GetMemoryUsage(),
                    PacketLossRate = CalculatePacketLossRate(),
                    AverageLatency = averageLatency,
                    MaxLatency = _maxLatency,
                    ThreadCount = GetThreadCount(),
                    StatisticsWindow = 60.0,
                    LastUpdated = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// 重置统计
        /// </summary>
        public void ResetStats()
        {
            lock (_lock)
            {
                _totalPackets = 0;
                _totalSamples = 0;
                _totalBytes = 0;
                _totalLatency = 0;
                _maxLatency = 0;
                _recentPackets.Clear();
                _stopwatch.Restart();
                _lastUpdateTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// 估算数据包大小
        /// </summary>
        /// <param name="dataPacket">数据包</param>
        /// <returns>估算大小（字节）</returns>
        private long EstimatePacketSize(DataPacket dataPacket)
        {
            long size = 0;
            
            // 基础结构大小
            size += 64; // 基础字段

            // 通道数据大小
            foreach (var channelData in dataPacket.ChannelData.Values)
            {
                size += channelData.Length * sizeof(double);
            }

            // 元数据大小（估算）
            size += dataPacket.Metadata.Count * 32;

            return size;
        }

        /// <summary>
        /// 计算实际采样率
        /// </summary>
        /// <param name="recentPackets">最近的数据包</param>
        /// <returns>实际采样率</returns>
        private double CalculateActualSampleRate(DataPacketInfo[] recentPackets)
        {
            if (recentPackets.Length < 2) return 0;

            var timeSpan = (recentPackets.Last().Timestamp - recentPackets.First().Timestamp).TotalSeconds;
            var totalSamples = recentPackets.Sum(p => p.SampleCount);

            return timeSpan > 0 ? totalSamples / timeSpan : 0;
        }

        /// <summary>
        /// 计算数据吞吐量
        /// </summary>
        /// <param name="recentPackets">最近的数据包</param>
        /// <returns>数据吞吐量（MB/s）</returns>
        private double CalculateDataThroughput(DataPacketInfo[] recentPackets)
        {
            if (recentPackets.Length < 2) return 0;

            var timeSpan = (recentPackets.Last().Timestamp - recentPackets.First().Timestamp).TotalSeconds;
            var totalBytes = recentPackets.Sum(p => p.Size);

            return timeSpan > 0 ? (totalBytes / (1024.0 * 1024.0)) / timeSpan : 0;
        }

        /// <summary>
        /// 获取CPU使用率
        /// </summary>
        /// <returns>CPU使用率百分比</returns>
        private double GetCpuUsage()
        {
            try
            {
                if (_currentProcess != null)
                {
                    var currentTime = DateTime.UtcNow;
                    var currentTotalProcessorTime = _currentProcess.TotalProcessorTime;
                    
                    var cpuUsedMs = (currentTotalProcessorTime - _lastTotalProcessorTime).TotalMilliseconds;
                    var totalMsPassed = (currentTime - _lastCpuTime).TotalMilliseconds;
                    
                    var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                    
                    _lastCpuTime = currentTime;
                    _lastTotalProcessorTime = currentTotalProcessorTime;
                    
                    return cpuUsageTotal * 100;
                }
            }
            catch (Exception)
            {
                // 忽略性能计数器错误
            }

            return 0;
        }

        /// <summary>
        /// 获取内存使用量
        /// </summary>
        /// <returns>内存使用量（MB）</returns>
        private double GetMemoryUsage()
        {
            try
            {
                if (_currentProcess != null)
                {
                    return _currentProcess.WorkingSet64 / (1024.0 * 1024.0);
                }
            }
            catch (Exception)
            {
                // 忽略性能计数器错误
            }

            return 0;
        }

        /// <summary>
        /// 计算丢包率
        /// </summary>
        /// <returns>丢包率百分比</returns>
        private double CalculatePacketLossRate()
        {
            // 这里可以根据实际需求实现丢包率计算
            // 目前返回0，表示没有丢包
            return 0;
        }

        /// <summary>
        /// 获取线程数
        /// </summary>
        /// <returns>线程数</returns>
        private int GetThreadCount()
        {
            try
            {
                return _currentProcess?.Threads.Count ?? 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    /// <summary>
    /// 数据包信息
    /// </summary>
    internal class DataPacketInfo
    {
        public DateTime Timestamp { get; set; }
        public int SampleCount { get; set; }
        public double Latency { get; set; }
        public long Size { get; set; }
    }
}
