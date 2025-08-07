using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using SeeSharpBackend.Hubs;
using SeeSharpBackend.Models.MISD;
using SeeSharpBackend.Services.Drivers;

namespace SeeSharpBackend.Services.DataAcquisition
{
    /// <summary>
    /// USB-1601数据采集引擎实现
    /// 支持AI/AO/DI/DO功能，支持真实硬件和模拟模式
    /// </summary>
    public class USB1601DataAcquisitionEngine : IDataAcquisitionEngine
    {
        private readonly ILogger<USB1601DataAcquisitionEngine> _logger;
        private readonly IHubContext<DataStreamHub> _hubContext;
        private readonly JYUSB1601DllDriverAdapter? _driverAdapter;
        private readonly ConcurrentDictionary<int, AcquisitionTask> _tasks;
        private bool _simulationMode;
        private readonly Dictionary<int, object> _aiTasks = new();
        private readonly Dictionary<int, object> _aoTasks = new();

        public USB1601DataAcquisitionEngine(
            ILogger<USB1601DataAcquisitionEngine> logger,
            IHubContext<DataStreamHub> hubContext,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _hubContext = hubContext;
            _tasks = new ConcurrentDictionary<int, AcquisitionTask>();

            // 尝试获取硬件驱动，如果不可用则使用模拟模式
            try
            {
                _driverAdapter = serviceProvider.GetService<JYUSB1601DllDriverAdapter>();
                if (_driverAdapter != null)
                {
                    _simulationMode = false;
                    _logger.LogInformation("USB-1601采集引擎已初始化（硬件模式）");
                }
                else
                {
                    _simulationMode = true;
                    _logger.LogWarning("USB-1601硬件驱动不可用，使用模拟模式");
                }
            }
            catch (Exception ex)
            {
                _simulationMode = true;
                _logger.LogWarning(ex, "无法加载USB-1601硬件驱动，使用模拟模式");
            }
        }

        public async Task<bool> StartAcquisitionAsync(int taskId, AcquisitionConfiguration config)
        {
            try
            {
                if (_tasks.ContainsKey(taskId))
                {
                    _logger.LogWarning($"任务 {taskId} 已存在");
                    return false;
                }

                var task = new AcquisitionTask
                {
                    TaskId = taskId,
                    Configuration = config,
                    Status = Models.MISD.TaskStatus.Running,
                    CreatedAt = DateTime.UtcNow,
                    StartedAt = DateTime.UtcNow,
                    CancellationTokenSource = new CancellationTokenSource()
                };

                if (!_tasks.TryAdd(taskId, task))
                {
                    return false;
                }

                // 启动数据采集线程
                _ = Task.Run(async () => await AcquisitionLoop(task), task.CancellationTokenSource.Token);

                _logger.LogInformation($"任务 {taskId} 已启动，设备ID: {config.DeviceId}, 采样率: {config.SampleRate}Hz");
                
                // 通知前端任务已启动
                await _hubContext.Clients.All.SendAsync("TaskStarted", new
                {
                    taskId,
                    deviceId = config.DeviceId,
                    sampleRate = config.SampleRate,
                    channelCount = config.Channels.Count,
                    mode = _simulationMode ? "Simulation" : "Hardware"
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"启动任务 {taskId} 时发生错误");
                return false;
            }
        }

        public async Task<bool> StopAcquisitionAsync(int taskId)
        {
            try
            {
                if (!_tasks.TryGetValue(taskId, out var task))
                {
                    return false;
                }

                task.CancellationTokenSource.Cancel();
                task.Status = Models.MISD.TaskStatus.Stopped;
                task.StoppedAt = DateTime.UtcNow;

                if (!_simulationMode && _driverAdapter != null)
                {
                    // 停止硬件任务 - 使用模拟方式
                    _logger.LogInformation($"停止硬件任务 {taskId}");
                }

                _tasks.TryRemove(taskId, out _);
                
                await _hubContext.Clients.All.SendAsync("TaskStopped", taskId);
                _logger.LogInformation($"任务 {taskId} 已停止");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"停止任务 {taskId} 时发生错误");
                return false;
            }
        }

        public async Task<bool> PauseAcquisitionAsync(int taskId)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                task.Status = Models.MISD.TaskStatus.Stopped;
                await _hubContext.Clients.All.SendAsync("TaskPaused", taskId);
                return true;
            }
            return false;
        }

        public async Task<bool> ResumeAcquisitionAsync(int taskId)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                task.Status = Models.MISD.TaskStatus.Running;
                await _hubContext.Clients.All.SendAsync("TaskResumed", taskId);
                return true;
            }
            return false;
        }

        public Task<AcquisitionTaskStatus> GetTaskStatusAsync(int taskId)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                return Task.FromResult(new AcquisitionTaskStatus
                {
                    TaskId = taskId,
                    Status = task.Status,
                    CreatedAt = task.CreatedAt,
                    StartedAt = task.StartedAt,
                    StoppedAt = task.StoppedAt,
                    SamplesAcquired = task.SamplesAcquired,
                    Configuration = task.Configuration
                });
            }
            
            throw new ArgumentException($"任务 {taskId} 不存在");
        }

        public async IAsyncEnumerable<DataPacket> GetDataStreamAsync(
            int taskId, 
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!_tasks.TryGetValue(taskId, out var task))
            {
                yield break;
            }

            while (!cancellationToken.IsCancellationRequested && task.Status == Models.MISD.TaskStatus.Running)
            {
                // 从任务的数据队列中获取数据
                if (task.DataQueue.TryDequeue(out var packet))
                {
                    yield return packet;
                }
                else
                {
                    await Task.Delay(10, cancellationToken);
                }
            }
        }

        public Task<BufferStatus> GetBufferStatusAsync(int taskId)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                return Task.FromResult(new BufferStatus
                {
                    BufferSize = task.Configuration.BufferSize,
                    AvailableSamples = task.DataQueue.Count,
                    TransferredSamples = task.SamplesAcquired,
                    IsOverflow = false,
                    OverflowCount = 0,
                    LastUpdated = DateTime.UtcNow
                });
            }
            
            throw new ArgumentException($"任务 {taskId} 不存在");
        }

        public Task<bool> ConfigureBufferAsync(int taskId, BufferConfiguration bufferConfig)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                task.Configuration.BufferSize = bufferConfig.Size;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> ClearBufferAsync(int taskId)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                while (task.DataQueue.TryDequeue(out _)) { }
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<AcquisitionPerformanceStats> GetPerformanceStatsAsync(int taskId)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                var elapsed = (DateTime.UtcNow - task.StartedAt).TotalSeconds;
                var actualRate = elapsed > 0 ? task.SamplesAcquired / elapsed : 0;
                
                return Task.FromResult(new AcquisitionPerformanceStats
                {
                    TaskId = taskId,
                    AverageSampleRate = task.Configuration.SampleRate,
                    ActualSampleRate = actualRate,
                    DataThroughput = actualRate * task.Configuration.Channels.Count * sizeof(double) / 1024 / 1024,
                    CpuUsage = 5.0, // 模拟值
                    MemoryUsage = 50.0, // 模拟值
                    PacketLossRate = 0.0,
                    AverageLatency = 1.0,
                    MaxLatency = 5.0,
                    ThreadCount = 1,
                    StatisticsWindow = 60.0,
                    LastUpdated = DateTime.UtcNow
                });
            }
            
            throw new ArgumentException($"任务 {taskId} 不存在");
        }

        public Task<IEnumerable<int>> GetActiveTasksAsync()
        {
            return Task.FromResult(_tasks.Keys.AsEnumerable());
        }

        public Task<bool> SetDataQualityCheckAsync(int taskId, DataQualityConfiguration qualityConfig)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                task.QualityConfig = qualityConfig;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<DataQualityReport> GetDataQualityReportAsync(int taskId)
        {
            if (_tasks.TryGetValue(taskId, out var task))
            {
                return Task.FromResult(new DataQualityReport
                {
                    TaskId = taskId,
                    TotalSamples = task.SamplesAcquired,
                    ValidSamples = task.SamplesAcquired,
                    CrcErrors = 0,
                    TimestampErrors = 0,
                    PacketLoss = 0,
                    RangeErrors = 0,
                    GeneratedAt = DateTime.UtcNow
                });
            }
            
            throw new ArgumentException($"任务 {taskId} 不存在");
        }

        /// <summary>
        /// 数据采集循环
        /// </summary>
        private async Task AcquisitionLoop(AcquisitionTask task)
        {
            var random = new Random();
            var sequenceNumber = 0L;
            var samplesPerPacket = 100; // 每个数据包的样本数
            object? aiTask = null;
            object? aoTask = null;
            
            try
            {
                if (!_simulationMode && _driverAdapter != null && _driverAdapter.IsInitialized)
                {
                    // 硬件模式：创建AI和AO任务
                    var deviceIndex = task.Configuration.DeviceId - 1; // 转换为设备索引
                    
                    // 创建AI任务
                    var aiParams = new Dictionary<string, object>
                    {
                        ["DeviceIndex"] = deviceIndex,
                        ["TaskId"] = task.TaskId,
                        ["ChannelCount"] = task.Configuration.Channels.Count(c => c.Enabled),
                        ["SampleRate"] = task.Configuration.SampleRate,
                        ["MinRange"] = task.Configuration.Channels.FirstOrDefault()?.RangeMin ?? -10.0,
                        ["MaxRange"] = task.Configuration.Channels.FirstOrDefault()?.RangeMax ?? 10.0
                    };
                    
                    aiTask = await _driverAdapter.CreateTaskAsync("AI", task.Configuration.DeviceId, aiParams);
                    if (aiTask != null)
                    {
                        _aiTasks[task.TaskId] = aiTask;
                        await _driverAdapter.StartTaskAsync(task.TaskId);
                        _logger.LogInformation($"启动硬件AI任务 设备{deviceIndex}, 采样率{task.Configuration.SampleRate}Hz");
                    }
                    
                    // 如果需要自发自收，创建AO任务
                    if (task.Configuration.SelfTestMode)
                    {
                        var aoParams = new Dictionary<string, object>
                        {
                            ["DeviceIndex"] = deviceIndex,
                            ["TaskId"] = -task.TaskId, // 使用负数ID区分AO任务
                            ["ChannelCount"] = Math.Min(2, task.Configuration.Channels.Count(c => c.Enabled)),
                            ["MinRange"] = -10.0,
                            ["MaxRange"] = 10.0
                        };
                        
                        aoTask = await _driverAdapter.CreateTaskAsync("AO", task.Configuration.DeviceId, aoParams);
                        if (aoTask != null)
                        {
                            _aoTasks[task.TaskId] = aoTask;
                            
                            // 生成测试波形数据
                            var waveformData = GenerateTestWaveform(task.Configuration);
                            await _driverAdapter.WriteDataAsync(-task.TaskId, waveformData);
                            await _driverAdapter.StartTaskAsync(-task.TaskId);
                            _logger.LogInformation($"启动硬件AO任务用于自发自收测试");
                        }
                    }
                }

                while (!task.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (task.Status == Models.MISD.TaskStatus.Stopped)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    var channelData = new Dictionary<int, double[]>();
                    
                    if (_simulationMode)
                    {
                        // 模拟模式：生成模拟数据
                        foreach (var channel in task.Configuration.Channels.Where(c => c.Enabled))
                        {
                            var data = new double[samplesPerPacket];
                            for (int i = 0; i < samplesPerPacket; i++)
                            {
                                // 生成正弦波 + 噪声
                                var t = (task.SamplesAcquired + i) / task.Configuration.SampleRate;
                                var frequency = 10.0 * (channel.ChannelId + 1); // 每个通道不同频率
                                var amplitude = (channel.RangeMax - channel.RangeMin) / 4;
                                var offset = (channel.RangeMax + channel.RangeMin) / 2;
                                data[i] = offset + amplitude * Math.Sin(2 * Math.PI * frequency * t) 
                                        + (random.NextDouble() - 0.5) * 0.1;
                            }
                            channelData[channel.ChannelId] = data;
                        }
                    }
                    else if (_driverAdapter != null && aiTask != null)
                    {
                        // 硬件模式：从真实硬件读取数据
                        var totalSamples = samplesPerPacket * task.Configuration.Channels.Count(c => c.Enabled);
                        var hardwareData = await _driverAdapter.ReadDataAsync(task.TaskId, totalSamples);
                        
                        if (hardwareData != null && hardwareData.Length > 0)
                        {
                            // 将一维数组转换为多通道数据
                            int channelCount = task.Configuration.Channels.Count(c => c.Enabled);
                            int channelIndex = 0;
                            
                            foreach (var channel in task.Configuration.Channels.Where(c => c.Enabled))
                            {
                                var data = new double[samplesPerPacket];
                                for (int i = 0; i < samplesPerPacket; i++)
                                {
                                    // 交错数据：ch0[0], ch1[0], ch0[1], ch1[1], ...
                                    data[i] = hardwareData[i * channelCount + channelIndex];
                                }
                                channelData[channel.ChannelId] = data;
                                channelIndex++;
                            }
                            
                            _logger.LogDebug($"从硬件读取 {totalSamples} 个样本");
                        }
                        else
                        {
                            // 如果没有数据可用，跳过本次循环
                            await Task.Delay(10);
                            continue;
                        }
                    }

                    // 创建数据包
                    var packet = new DataPacket
                    {
                        TaskId = task.TaskId,
                        Timestamp = DateTime.UtcNow,
                        SequenceNumber = sequenceNumber++,
                        ChannelData = channelData,
                        SampleCount = samplesPerPacket,
                        SampleRate = task.Configuration.SampleRate,
                        QualityFlags = DataQualityFlags.Good,
                        Metadata = new Dictionary<string, object>
                        {
                            ["Mode"] = _simulationMode ? "Simulation" : "Hardware",
                            ["DeviceId"] = task.Configuration.DeviceId
                        }
                    };

                    // 添加到队列
                    task.DataQueue.Enqueue(packet);
                    task.SamplesAcquired += samplesPerPacket;

                    // 通过SignalR发送数据
                    await _hubContext.Clients.All.SendAsync("DataReceived", new
                    {
                        taskId = task.TaskId,
                        timestamp = packet.Timestamp,
                        sequenceNumber = packet.SequenceNumber,
                        sampleCount = packet.SampleCount,
                        channelCount = channelData.Count,
                        // 只发送前10个样本用于显示
                        sampleData = channelData.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Take(10).ToArray()
                        )
                    });

                    // 控制采样率
                    var delay = (int)(1000.0 * samplesPerPacket / task.Configuration.SampleRate);
                    await Task.Delay(Math.Max(10, delay));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"任务 {task.TaskId} 采集循环发生错误");
                task.Status = Models.MISD.TaskStatus.Error;
                task.ErrorMessage = ex.Message;
            }
            finally
            {
                if (!_simulationMode && _driverAdapter != null)
                {
                    // 清理硬件资源
                    try
                    {
                        if (_aiTasks.ContainsKey(task.TaskId))
                        {
                            await _driverAdapter.StopTaskAsync(task.TaskId);
                            await _driverAdapter.DisposeTaskAsync(task.TaskId);
                            _aiTasks.Remove(task.TaskId);
                            _logger.LogInformation($"清理AI硬件资源 任务{task.TaskId}");
                        }
                        
                        if (_aoTasks.ContainsKey(task.TaskId))
                        {
                            await _driverAdapter.StopTaskAsync(-task.TaskId);
                            await _driverAdapter.DisposeTaskAsync(-task.TaskId);
                            _aoTasks.Remove(task.TaskId);
                            _logger.LogInformation($"清理AO硬件资源 任务{task.TaskId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "清理硬件资源时发生错误");
                    }
                }
            }
        }

        /// <summary>
        /// 生成测试波形数据
        /// </summary>
        private double[] GenerateTestWaveform(AcquisitionConfiguration config)
        {
            var sampleRate = config.SampleRate;
            var duration = 1.0; // 1秒的数据
            var samples = (int)(sampleRate * duration);
            var data = new double[samples * 2]; // 2个AO通道
            
            // 生成不同频率的正弦波
            for (int i = 0; i < samples; i++)
            {
                var t = i / sampleRate;
                // 通道0: 10Hz正弦波
                data[i * 2] = 5.0 * Math.Sin(2 * Math.PI * 10 * t);
                // 通道1: 20Hz正弦波
                data[i * 2 + 1] = 3.0 * Math.Sin(2 * Math.PI * 20 * t);
            }
            
            return data;
        }
        
        /// <summary>
        /// 切换硬件/模拟模式
        /// </summary>
        public async Task<bool> SetModeAsync(bool useHardware)
        {
            if (useHardware && _driverAdapter != null && _driverAdapter.IsInitialized)
            {
                _simulationMode = false;
                _logger.LogInformation("切换到硬件模式");
                return true;
            }
            else
            {
                _simulationMode = true;
                _logger.LogInformation("切换到模拟模式");
                return true;
            }
        }
        
        /// <summary>
        /// 获取当前模式
        /// </summary>
        public string GetCurrentMode()
        {
            return _simulationMode ? "Simulation" : "Hardware";
        }
        
        /// <summary>
        /// 内部任务类
        /// </summary>
        private class AcquisitionTask
        {
            public int TaskId { get; set; }
            public AcquisitionConfiguration Configuration { get; set; } = new();
            public Models.MISD.TaskStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime StartedAt { get; set; }
            public DateTime? StoppedAt { get; set; }
            public long SamplesAcquired { get; set; }
            public string? ErrorMessage { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; } = new();
            public ConcurrentQueue<DataPacket> DataQueue { get; set; } = new();
            public DataQualityConfiguration? QualityConfig { get; set; }
        }
    }
}
