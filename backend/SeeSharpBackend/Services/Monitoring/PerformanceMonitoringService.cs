using System.Diagnostics;
using System.Runtime.InteropServices;
using Prometheus;
using SeeSharpBackend.Services.DataAcquisition;

namespace SeeSharpBackend.Services.Monitoring
{
    /// <summary>
    /// 性能监控服务实现
    /// </summary>
    public class PerformanceMonitoringService : IPerformanceMonitoringService
    {
        private readonly ILogger<PerformanceMonitoringService> _logger;
        private readonly IDataAcquisitionEngine? _dataAcquisitionEngine;
        private readonly Process _currentProcess;
        private readonly DateTime _startTime;

        // Prometheus 指标
        private readonly Counter _requestCounter;
        private readonly Histogram _requestDuration;
        private readonly Gauge _cpuUsage;
        private readonly Gauge _memoryUsage;
        private readonly Gauge _activeConnections;
        private readonly Counter _dataAcquisitionSamples;
        private readonly Gauge _dataAcquisitionRate;
        private readonly Histogram _customHistogram;
        private readonly Counter _customCounter;
        private readonly Gauge _customGauge;

        // 性能计数器
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _memoryCounter;

        public PerformanceMonitoringService(
            ILogger<PerformanceMonitoringService> logger)
        {
            _logger = logger;
            _dataAcquisitionEngine = null;
            _currentProcess = Process.GetCurrentProcess();
            _startTime = DateTime.UtcNow;

            // 初始化 Prometheus 指标
            _requestCounter = Metrics.CreateCounter("seesharp_requests_total", "Total number of requests", new[] { "method", "endpoint", "status" });
            _requestDuration = Metrics.CreateHistogram("seesharp_request_duration_seconds", "Request duration in seconds", new[] { "method", "endpoint" });
            _cpuUsage = Metrics.CreateGauge("seesharp_cpu_usage_percent", "CPU usage percentage");
            _memoryUsage = Metrics.CreateGauge("seesharp_memory_usage_bytes", "Memory usage in bytes", new[] { "type" });
            _activeConnections = Metrics.CreateGauge("seesharp_active_connections", "Number of active connections");
            _dataAcquisitionSamples = Metrics.CreateCounter("seesharp_data_acquisition_samples_total", "Total number of data acquisition samples");
            _dataAcquisitionRate = Metrics.CreateGauge("seesharp_data_acquisition_rate_hz", "Data acquisition rate in Hz");
            _customHistogram = Metrics.CreateHistogram("seesharp_custom_histogram", "Custom histogram metric", new[] { "name", "labels" });
            _customCounter = Metrics.CreateCounter("seesharp_custom_counter", "Custom counter metric", new[] { "name", "labels" });
            _customGauge = Metrics.CreateGauge("seesharp_custom_gauge", "Custom gauge metric", new[] { "name", "labels" });

            InitializePerformanceCounters();
        }

        private void InitializePerformanceCounters()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize performance counters");
            }
        }

        public async Task<SystemPerformanceMetrics> GetSystemMetricsAsync()
        {
            var metrics = new SystemPerformanceMetrics();

            try
            {
                // CPU 使用率
                if (_cpuCounter != null)
                {
                    metrics.CpuUsagePercent = _cpuCounter.NextValue();
                }
                else
                {
                    // 跨平台 CPU 使用率估算
                    metrics.CpuUsagePercent = await EstimateCpuUsageAsync();
                }

                // 内存信息
                var gcInfo = GC.GetTotalMemory(false);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var memInfo = GetWindowsMemoryInfo();
                    metrics.MemoryUsedBytes = memInfo.UsedBytes;
                    metrics.MemoryTotalBytes = memInfo.TotalBytes;
                }
                else
                {
                    var memInfo = await GetLinuxMemoryInfoAsync();
                    metrics.MemoryUsedBytes = memInfo.UsedBytes;
                    metrics.MemoryTotalBytes = memInfo.TotalBytes;
                }

                metrics.MemoryUsagePercent = metrics.MemoryTotalBytes > 0 
                    ? (double)metrics.MemoryUsedBytes / metrics.MemoryTotalBytes * 100 
                    : 0;

                // 磁盘信息
                var diskInfo = GetDiskInfo();
                metrics.DiskUsedBytes = diskInfo.UsedBytes;
                metrics.DiskTotalBytes = diskInfo.TotalBytes;
                metrics.DiskUsagePercent = diskInfo.TotalBytes > 0 
                    ? (double)diskInfo.UsedBytes / diskInfo.TotalBytes * 100 
                    : 0;

                // 进程信息
                metrics.ThreadCount = _currentProcess.Threads.Count;
                metrics.HandleCount = _currentProcess.HandleCount;
                metrics.Uptime = DateTime.UtcNow - _startTime;

                // 更新 Prometheus 指标
                _cpuUsage.Set(metrics.CpuUsagePercent);
                _memoryUsage.WithLabels("used").Set(metrics.MemoryUsedBytes);
                _memoryUsage.WithLabels("total").Set(metrics.MemoryTotalBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system metrics");
            }

            return metrics;
        }

        public async Task<ApplicationPerformanceMetrics> GetApplicationMetricsAsync()
        {
            var metrics = new ApplicationPerformanceMetrics();

            try
            {
                // 进程内存信息
                metrics.WorkingSetBytes = _currentProcess.WorkingSet64;
                metrics.PrivateMemoryBytes = _currentProcess.PrivateMemorySize64;
                metrics.GcTotalMemoryBytes = GC.GetTotalMemory(false);

                // GC 信息
                metrics.GcGen0Collections = GC.CollectionCount(0);
                metrics.GcGen1Collections = GC.CollectionCount(1);
                metrics.GcGen2Collections = GC.CollectionCount(2);

                // 线程池信息
                ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
                ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
                
                metrics.ThreadPoolThreads = maxWorkerThreads - workerThreads;
                metrics.ThreadPoolCompletionPortThreads = maxCompletionPortThreads - completionPortThreads;

                // 这些指标需要从其他服务获取
                metrics.RequestsPerSecond = 0; // 需要从请求统计中获取
                metrics.AverageResponseTimeMs = 0; // 需要从请求统计中获取
                metrics.ActiveConnections = 0; // 需要从连接管理器获取
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application metrics");
            }

            return metrics;
        }

        public async Task<DataAcquisitionMetrics> GetDataAcquisitionMetricsAsync()
        {
            var metrics = new DataAcquisitionMetrics();

            try
            {
                if (_dataAcquisitionEngine != null)
                {
                    // 从数据采集引擎获取指标
                    // 这里需要根据实际的数据采集引擎接口来实现
                    // metrics.TotalSamplesAcquired = await _dataAcquisitionEngine.GetTotalSamplesAsync();
                    // metrics.SamplingRateHz = await _dataAcquisitionEngine.GetCurrentSamplingRateAsync();
                    // 等等...
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data acquisition metrics");
            }

            return metrics;
        }

        public void RecordCustomMetric(string name, double value, Dictionary<string, string>? labels = null)
        {
            try
            {
                var labelString = labels != null ? string.Join(",", labels.Select(kv => $"{kv.Key}={kv.Value}")) : "";
                _customGauge.WithLabels(name, labelString).Set(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording custom metric {MetricName}", name);
            }
        }

        public void IncrementCounter(string name, Dictionary<string, string>? labels = null)
        {
            try
            {
                var labelString = labels != null ? string.Join(",", labels.Select(kv => $"{kv.Key}={kv.Value}")) : "";
                _customCounter.WithLabels(name, labelString).Inc();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing counter {CounterName}", name);
            }
        }

        public void RecordHistogram(string name, double value, Dictionary<string, string>? labels = null)
        {
            try
            {
                var labelString = labels != null ? string.Join(",", labels.Select(kv => $"{kv.Key}={kv.Value}")) : "";
                _customHistogram.WithLabels(name, labelString).Observe(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording histogram {HistogramName}", name);
            }
        }

        public IDisposable StartTimer(string name, Dictionary<string, string>? labels = null)
        {
            var labelArray = labels?.Select(kv => kv.Value).ToArray() ?? Array.Empty<string>();
            return _requestDuration.WithLabels(labelArray).NewTimer();
        }

        private async Task<double> EstimateCpuUsageAsync()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = _currentProcess.TotalProcessorTime;
            
            await Task.Delay(100); // 短暂延迟以获取CPU使用率
            
            var endTime = DateTime.UtcNow;
            var endCpuUsage = _currentProcess.TotalProcessorTime;
            
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            
            return cpuUsageTotal * 100;
        }

        private (long UsedBytes, long TotalBytes) GetWindowsMemoryInfo()
        {
            // Windows 内存信息获取
            var totalMemory = GC.GetTotalMemory(false);
            return (totalMemory, totalMemory * 2); // 简化实现
        }

        private async Task<(long UsedBytes, long TotalBytes)> GetLinuxMemoryInfoAsync()
        {
            try
            {
                var memInfo = await File.ReadAllTextAsync("/proc/meminfo");
                var lines = memInfo.Split('\n');
                
                long totalKb = 0, availableKb = 0;
                
                foreach (var line in lines)
                {
                    if (line.StartsWith("MemTotal:"))
                    {
                        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && long.TryParse(parts[1], out totalKb))
                        {
                            totalKb *= 1024; // 转换为字节
                        }
                    }
                    else if (line.StartsWith("MemAvailable:"))
                    {
                        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && long.TryParse(parts[1], out availableKb))
                        {
                            availableKb *= 1024; // 转换为字节
                        }
                    }
                }
                
                var usedBytes = totalKb - availableKb;
                return (usedBytes, totalKb);
            }
            catch
            {
                var totalMemory = GC.GetTotalMemory(false);
                return (totalMemory, totalMemory * 2); // 回退方案
            }
        }

        private (long UsedBytes, long TotalBytes) GetDiskInfo()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\");
                var totalBytes = drive.TotalSize;
                var freeBytes = drive.TotalFreeSpace;
                var usedBytes = totalBytes - freeBytes;
                
                return (usedBytes, totalBytes);
            }
            catch
            {
                return (0, 0);
            }
        }

        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _memoryCounter?.Dispose();
            _currentProcess?.Dispose();
        }
    }
}
