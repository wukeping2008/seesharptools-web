using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SeeSharpBackend.Services.MISD;
using SeeSharpBackend.Services.DataCompression;

namespace SeeSharpBackend.Services.DataAcquisition
{
    /// <summary>
    /// 高性能多线程数据采集引擎实现
    /// 支持多设备并发采集、实时数据流处理、内存优化管理
    /// </summary>
    public class DataAcquisitionEngine : IDataAcquisitionEngine, IDisposable
    {
        private readonly ILogger<DataAcquisitionEngine> _logger;
        private readonly IMISDService _misdService;
        private readonly IDataCompressionService _compressionService;
        
        // 任务管理
        private readonly ConcurrentDictionary<int, AcquisitionTask> _activeTasks = new();
        private readonly ConcurrentDictionary<int, CancellationTokenSource> _taskCancellationTokens = new();
        
        // 性能监控
        private readonly ConcurrentDictionary<int, AcquisitionPerformanceMonitor> _performanceMonitors = new();
        private readonly Timer _performanceUpdateTimer;
        
        // 内存池管理
        private readonly ObjectPool<double[]> _dataArrayPool;
        private readonly ObjectPool<DataPacket> _dataPacketPool;
        
        // 配置
        private readonly DataAcquisitionEngineOptions _options;
        
        private bool _disposed = false;

        public DataAcquisitionEngine(
            ILogger<DataAcquisitionEngine> logger,
            IMISDService misdService,
            IDataCompressionService compressionService,
            DataAcquisitionEngineOptions? options = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _misdService = misdService ?? throw new ArgumentNullException(nameof(misdService));
            _compressionService = compressionService ?? throw new ArgumentNullException(nameof(compressionService));
            _options = options ?? new DataAcquisitionEngineOptions();

            // 初始化内存池
            _dataArrayPool = new ObjectPool<double[]>(() => new double[_options.DefaultArraySize]);
            _dataPacketPool = new ObjectPool<DataPacket>(() => new DataPacket());

            // 启动性能监控定时器
            _performanceUpdateTimer = new Timer(UpdatePerformanceStats, null, 
                TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            _logger.LogInformation("数据采集引擎初始化完成");
        }

        /// <summary>
        /// 启动数据采集任务
        /// </summary>
        public async Task<bool> StartAcquisitionAsync(int taskId, AcquisitionConfiguration config)
        {
            try
            {
                _logger.LogInformation("启动数据采集任务 {TaskId}", taskId);

                // 检查任务是否已存在
                if (_activeTasks.ContainsKey(taskId))
                {
                    _logger.LogWarning("任务 {TaskId} 已存在", taskId);
                    return false;
                }

                // 验证配置
                if (!ValidateConfiguration(config))
                {
                    _logger.LogError("任务 {TaskId} 配置验证失败", taskId);
                    return false;
                }

                // 创建取消令牌
                var cancellationTokenSource = new CancellationTokenSource();
                _taskCancellationTokens[taskId] = cancellationTokenSource;

                // 创建采集任务
                var acquisitionTask = new AcquisitionTask
                {
                    TaskId = taskId,
                    Configuration = config,
                    Status = new AcquisitionTaskStatus
                    {
                        TaskId = taskId,
                        Status = Models.MISD.TaskStatus.Created,
                        CreatedAt = DateTime.UtcNow,
                        Configuration = config
                    },
                    DataChannel = Channel.CreateUnbounded<DataPacket>(),
                    BufferManager = new CircularBufferManager(config.BufferSize),
                    QualityChecker = config.EnableQualityCheck ? new DataQualityChecker(config) : null
                };

                _activeTasks[taskId] = acquisitionTask;

                // 创建性能监控器
                _performanceMonitors[taskId] = new AcquisitionPerformanceMonitor(taskId);

                // 启动采集线程
                _ = Task.Run(() => AcquisitionWorkerAsync(acquisitionTask, cancellationTokenSource.Token), 
                    cancellationTokenSource.Token);

                // 更新状态
                acquisitionTask.Status.Status = Models.MISD.TaskStatus.Started;
                acquisitionTask.Status.StartedAt = DateTime.UtcNow;

                _logger.LogInformation("数据采集任务 {TaskId} 启动成功", taskId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动数据采集任务 {TaskId} 失败", taskId);
                return false;
            }
        }

        /// <summary>
        /// 停止数据采集任务
        /// </summary>
        public async Task<bool> StopAcquisitionAsync(int taskId)
        {
            try
            {
                _logger.LogInformation("停止数据采集任务 {TaskId}", taskId);

                if (!_activeTasks.TryGetValue(taskId, out var task))
                {
                    _logger.LogWarning("任务 {TaskId} 不存在", taskId);
                    return false;
                }

                // 取消任务
                if (_taskCancellationTokens.TryRemove(taskId, out var cancellationTokenSource))
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();
                }

                // 更新状态
                task.Status.Status = Models.MISD.TaskStatus.Stopped;
                task.Status.StoppedAt = DateTime.UtcNow;

                // 清理资源
                await Task.Delay(100); // 等待工作线程退出
                
                if (_activeTasks.TryRemove(taskId, out var removedTask))
                {
                    removedTask.Dispose();
                }

                _performanceMonitors.TryRemove(taskId, out _);

                _logger.LogInformation("数据采集任务 {TaskId} 停止成功", taskId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止数据采集任务 {TaskId} 失败", taskId);
                return false;
            }
        }

        /// <summary>
        /// 暂停数据采集任务
        /// </summary>
        public async Task<bool> PauseAcquisitionAsync(int taskId)
        {
            if (_activeTasks.TryGetValue(taskId, out var task))
            {
                task.IsPaused = true;
                task.Status.Status = Models.MISD.TaskStatus.Stopped; // 使用Stopped表示暂停
                _logger.LogInformation("数据采集任务 {TaskId} 已暂停", taskId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 恢复数据采集任务
        /// </summary>
        public async Task<bool> ResumeAcquisitionAsync(int taskId)
        {
            if (_activeTasks.TryGetValue(taskId, out var task))
            {
                task.IsPaused = false;
                task.Status.Status = Models.MISD.TaskStatus.Running;
                _logger.LogInformation("数据采集任务 {TaskId} 已恢复", taskId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取任务状态
        /// </summary>
        public async Task<AcquisitionTaskStatus> GetTaskStatusAsync(int taskId)
        {
            if (_activeTasks.TryGetValue(taskId, out var task))
            {
                return task.Status;
            }
            throw new ArgumentException($"任务 {taskId} 不存在");
        }

        /// <summary>
        /// 获取实时数据流
        /// </summary>
        public async IAsyncEnumerable<DataPacket> GetDataStreamAsync(int taskId, 
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!_activeTasks.TryGetValue(taskId, out var task))
            {
                throw new ArgumentException($"任务 {taskId} 不存在");
            }

            await foreach (var dataPacket in task.DataChannel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return dataPacket;
            }
        }

        /// <summary>
        /// 获取缓冲区状态
        /// </summary>
        public async Task<BufferStatus> GetBufferStatusAsync(int taskId)
        {
            if (_activeTasks.TryGetValue(taskId, out var task))
            {
                return task.BufferManager.GetStatus();
            }
            throw new ArgumentException($"任务 {taskId} 不存在");
        }

        /// <summary>
        /// 配置缓冲区
        /// </summary>
        public async Task<bool> ConfigureBufferAsync(int taskId, BufferConfiguration bufferConfig)
        {
            if (_activeTasks.TryGetValue(taskId, out var task))
            {
                task.BufferManager.Configure(bufferConfig);
                _logger.LogInformation("任务 {TaskId} 缓冲区配置已更新", taskId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        public async Task<bool> ClearBufferAsync(int taskId)
        {
            if (_activeTasks.TryGetValue(taskId, out var task))
            {
                task.BufferManager.Clear();
                _logger.LogInformation("任务 {TaskId} 缓冲区已清空", taskId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取性能统计
        /// </summary>
        public async Task<AcquisitionPerformanceStats> GetPerformanceStatsAsync(int taskId)
        {
            if (_performanceMonitors.TryGetValue(taskId, out var monitor))
            {
                return monitor.GetStats();
            }
            throw new ArgumentException($"任务 {taskId} 不存在");
        }

        /// <summary>
        /// 获取所有活跃任务
        /// </summary>
        public async Task<IEnumerable<int>> GetActiveTasksAsync()
        {
            return _activeTasks.Keys.ToList();
        }

        /// <summary>
        /// 设置数据质量检查
        /// </summary>
        public async Task<bool> SetDataQualityCheckAsync(int taskId, DataQualityConfiguration qualityConfig)
        {
            if (_activeTasks.TryGetValue(taskId, out var task))
            {
                task.QualityChecker = new DataQualityChecker(task.Configuration, qualityConfig);
                _logger.LogInformation("任务 {TaskId} 数据质量检查配置已更新", taskId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取数据质量报告
        /// </summary>
        public async Task<DataQualityReport> GetDataQualityReportAsync(int taskId)
        {
            if (_activeTasks.TryGetValue(taskId, out var task) && task.QualityChecker != null)
            {
                return task.QualityChecker.GenerateReport();
            }
            throw new ArgumentException($"任务 {taskId} 不存在或未启用质量检查");
        }

        /// <summary>
        /// 数据采集工作线程
        /// </summary>
        private async Task AcquisitionWorkerAsync(AcquisitionTask task, CancellationToken cancellationToken)
        {
            var taskId = task.TaskId;
            var config = task.Configuration;
            
            try
            {
                _logger.LogInformation("数据采集工作线程启动 - 任务 {TaskId}", taskId);

                // 设置线程优先级
                Thread.CurrentThread.Priority = config.ThreadPriority;

                // 创建MISD任务
                var misdTaskConfig = CreateMISDTaskConfiguration(config);
                var createdTaskConfig = await _misdService.CreateTaskAsync(misdTaskConfig);
                var misdTaskId = createdTaskConfig.Id; // 使用返回的配置中的ID

                // 启动MISD任务
                await _misdService.StartTaskAsync(misdTaskId);
                task.Status.Status = Models.MISD.TaskStatus.Running;

                var sequenceNumber = 0L;
                var stopwatch = Stopwatch.StartNew();
                var lastSampleTime = stopwatch.Elapsed;

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // 检查暂停状态
                        if (task.IsPaused)
                        {
                            await Task.Delay(100, cancellationToken);
                            continue;
                        }

                        // 计算采样间隔
                        var sampleInterval = TimeSpan.FromSeconds(1.0 / config.SampleRate);
                        var currentTime = stopwatch.Elapsed;
                        var timeSinceLastSample = currentTime - lastSampleTime;

                        if (timeSinceLastSample < sampleInterval)
                        {
                            var delayTime = sampleInterval - timeSinceLastSample;
                            if (delayTime.TotalMilliseconds > 0.1)
                            {
                                await Task.Delay(delayTime, cancellationToken);
                            }
                            continue;
                        }

                        // 读取数据
                        var samplesPerRead = Math.Max(1, (int)(config.SampleRate / 100)); // 10ms批次
                        var data = await _misdService.ReadDataAsync(misdTaskId, samplesPerRead, 1000);

                        if (data != null && data.GetLength(1) > 0)
                        {
                            // 创建数据包
                            var dataPacket = CreateDataPacket(task, data, sequenceNumber++);

                            // 数据质量检查
                            if (task.QualityChecker != null)
                            {
                                dataPacket.QualityFlags = task.QualityChecker.CheckQuality(dataPacket);
                            }

                            // 添加到缓冲区
                            task.BufferManager.AddData(dataPacket);

                            // 发送到数据流
                            if (!task.DataChannel.Writer.TryWrite(dataPacket))
                            {
                                _logger.LogWarning("任务 {TaskId} 数据通道写入失败", taskId);
                            }

                            // 更新性能统计
                            if (_performanceMonitors.TryGetValue(taskId, out var monitor))
                            {
                                monitor.RecordDataPacket(dataPacket);
                            }

                            // 更新任务状态
                            task.Status.SamplesAcquired += dataPacket.SampleCount;
                        }

                        lastSampleTime = currentTime;

                        // 检查有限采集模式
                        if (config.Mode == AcquisitionMode.Finite && 
                            config.SamplesToAcquire.HasValue &&
                            task.Status.SamplesAcquired >= config.SamplesToAcquire.Value)
                        {
                            _logger.LogInformation("任务 {TaskId} 有限采集完成", taskId);
                            break;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "任务 {TaskId} 数据采集过程中发生错误", taskId);
                        
                        // 设置错误状态
                        task.Status.Status = Models.MISD.TaskStatus.Error;
                        task.Status.ErrorMessage = ex.Message;
                        
                        await Task.Delay(1000, cancellationToken); // 错误后等待1秒
                    }
                }

                // 停止MISD任务
                await _misdService.StopTaskAsync(misdTaskId);
                await _misdService.DisposeTaskAsync(misdTaskId);

                _logger.LogInformation("数据采集工作线程结束 - 任务 {TaskId}", taskId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("数据采集任务 {TaskId} 被取消", taskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据采集工作线程异常 - 任务 {TaskId}", taskId);
                task.Status.Status = Models.MISD.TaskStatus.Error;
                task.Status.ErrorMessage = ex.Message;
            }
            finally
            {
                // 关闭数据通道
                task.DataChannel.Writer.Complete();
            }
        }

        /// <summary>
        /// 创建MISD任务配置
        /// </summary>
        private Models.MISD.MISDTaskConfiguration CreateMISDTaskConfiguration(AcquisitionConfiguration config)
        {
            var misdConfig = new Models.MISD.MISDTaskConfiguration
            {
                TaskName = $"AcquisitionTask_{config.DeviceId}_{DateTime.UtcNow:yyyyMMddHHmmss}",
                DeviceId = config.DeviceId,
                TaskType = "AI", // 模拟输入
                Channels = config.Channels.Where(c => c.Enabled).Select(c => new Models.MISD.ChannelConfiguration
                {
                    ChannelId = c.ChannelId,
                    RangeLow = c.RangeMin,
                    RangeHigh = c.RangeMax,
                    Terminal = c.Coupling == CouplingMode.DC ? "RSE" : "NRSE",
                    Coupling = c.Coupling.ToString()
                }).ToList(),
                Sampling = new Models.MISD.SamplingConfiguration
                {
                    SampleRate = config.SampleRate,
                    SamplesToAcquire = (int)(config.SamplesToAcquire ?? -1),
                    Mode = config.Mode.ToString(),
                    BufferSize = config.BufferSize
                }
            };

            // 设置触发配置
            if (config.TriggerConfig != null)
            {
                misdConfig.Trigger = new Models.MISD.TriggerConfiguration
                {
                    Type = config.TriggerConfig.Type.ToString(),
                    Source = $"Channel{config.TriggerConfig.SourceChannel}",
                    Edge = config.TriggerConfig.Slope.ToString(),
                    Level = config.TriggerConfig.Level,
                    PreTriggerSamples = config.TriggerConfig.PreTriggerSamples
                };
            }

            return misdConfig;
        }

        /// <summary>
        /// 创建数据包
        /// </summary>
        private DataPacket CreateDataPacket(AcquisitionTask task, double[,] data, long sequenceNumber)
        {
            var config = task.Configuration;
            var channelCount = data.GetLength(0);
            var sampleCount = data.GetLength(1);

            var dataPacket = _dataPacketPool.Get();
            dataPacket.TaskId = task.TaskId;
            dataPacket.Timestamp = DateTime.UtcNow;
            dataPacket.SequenceNumber = sequenceNumber;
            dataPacket.SampleCount = sampleCount;
            dataPacket.SampleRate = config.SampleRate;
            dataPacket.QualityFlags = DataQualityFlags.Good;
            dataPacket.ChannelData.Clear();

            // 转换数据格式
            for (int ch = 0; ch < channelCount; ch++)
            {
                var channelData = _dataArrayPool.Get();
                if (channelData.Length < sampleCount)
                {
                    _dataArrayPool.Return(channelData);
                    channelData = new double[sampleCount];
                }

                for (int i = 0; i < sampleCount; i++)
                {
                    channelData[i] = data[ch, i];
                }

                dataPacket.ChannelData[ch] = channelData;
            }

            // 计算CRC
            dataPacket.Crc32 = CalculateCrc32(dataPacket);

            return dataPacket;
        }

        /// <summary>
        /// 计算CRC32校验值
        /// </summary>
        private uint CalculateCrc32(DataPacket dataPacket)
        {
            // 简化的CRC32计算，实际应用中应使用标准CRC32算法
            uint crc = 0xFFFFFFFF;
            
            foreach (var channelData in dataPacket.ChannelData.Values)
            {
                foreach (var value in channelData)
                {
                    var bytes = BitConverter.GetBytes(value);
                    foreach (var b in bytes)
                    {
                        crc ^= b;
                        for (int i = 0; i < 8; i++)
                        {
                            if ((crc & 1) != 0)
                                crc = (crc >> 1) ^ 0xEDB88320;
                            else
                                crc >>= 1;
                        }
                    }
                }
            }
            
            return ~crc;
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        private bool ValidateConfiguration(AcquisitionConfiguration config)
        {
            if (config.SampleRate <= 0 || config.SampleRate > 10_000_000) // 最大10MS/s
            {
                _logger.LogError("采样率无效: {SampleRate}", config.SampleRate);
                return false;
            }

            if (config.Channels == null || !config.Channels.Any(c => c.Enabled))
            {
                _logger.LogError("没有启用的通道");
                return false;
            }

            if (config.BufferSize <= 0 || config.BufferSize > 1_000_000)
            {
                _logger.LogError("缓冲区大小无效: {BufferSize}", config.BufferSize);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 更新性能统计
        /// </summary>
        private void UpdatePerformanceStats(object? state)
        {
            try
            {
                foreach (var monitor in _performanceMonitors.Values)
                {
                    monitor.UpdateStats();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新性能统计时发生错误");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _logger.LogInformation("正在释放数据采集引擎资源...");

            // 停止所有任务
            var stopTasks = _activeTasks.Keys.Select(taskId => StopAcquisitionAsync(taskId));
            Task.WaitAll(stopTasks.ToArray(), TimeSpan.FromSeconds(5));

            // 释放定时器
            _performanceUpdateTimer?.Dispose();

            // 释放取消令牌
            foreach (var cts in _taskCancellationTokens.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }

            // 清理集合
            _activeTasks.Clear();
            _taskCancellationTokens.Clear();
            _performanceMonitors.Clear();

            _disposed = true;
            _logger.LogInformation("数据采集引擎资源释放完成");
        }
    }

    /// <summary>
    /// 数据采集引擎选项
    /// </summary>
    public class DataAcquisitionEngineOptions
    {
        /// <summary>
        /// 默认数组大小
        /// </summary>
        public int DefaultArraySize { get; set; } = 1000;

        /// <summary>
        /// 最大并发任务数
        /// </summary>
        public int MaxConcurrentTasks { get; set; } = 16;

        /// <summary>
        /// 性能统计更新间隔 (秒)
        /// </summary>
        public double PerformanceUpdateInterval { get; set; } = 1.0;

        /// <summary>
        /// 启用内存池
        /// </summary>
        public bool EnableMemoryPool { get; set; } = true;
    }
}
