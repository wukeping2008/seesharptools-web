using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using JYUSB1601;
using USB1601MCP.Models;

namespace USB1601MCP.Tools
{
    /// <summary>
    /// 数据流工具类
    /// 处理USB-1601的连续数据采集和流式传输
    /// </summary>
    public class StreamTools
    {
        private readonly ILogger<StreamTools> _logger;
        private readonly DeviceTools _deviceTools;
        private readonly ConcurrentDictionary<string, StreamContext> _activeStreams;

        public StreamTools(ILogger<StreamTools> logger, DeviceTools deviceTools)
        {
            _logger = logger;
            _deviceTools = deviceTools;
            _activeStreams = new ConcurrentDictionary<string, StreamContext>();
        }

        /// <summary>
        /// 开始数据流采集
        /// </summary>
        public async Task<object> StartStream(string? deviceId, int[]? channels, double sampleRate)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }

            if (channels == null || channels.Length == 0)
            {
                return new { success = false, error = "Channels array is required" };
            }

            if (sampleRate < 1 || sampleRate > 200000)
            {
                return new { success = false, error = "Sample rate must be between 1 and 200000 Hz" };
            }

            return await Task.Run<object>(() =>
            {
                try
                {
                    // 检查是否已有活动流
                    if (_activeStreams.ContainsKey(deviceId))
                    {
                        return new { success = false, error = "Stream already active for this device" };
                    }

                    var context = _deviceTools.GetDeviceContext(deviceId);
                    if (context == null)
                    {
                        return new { success = false, error = "Device not connected" };
                    }

                    // 创建流上下文
                    var streamContext = new StreamContext
                    {
                        StreamId = Guid.NewGuid().ToString(),
                        DeviceId = deviceId,
                        Channels = channels,
                        SampleRate = sampleRate,
                        StartTime = DateTime.UtcNow,
                        BufferSize = (int)(sampleRate * channels.Length), // 1秒缓冲
                        DataBuffer = new ConcurrentQueue<double[]>()
                    };

                    // 创建AI任务用于连续采集
                    var aiTask = new JYUSB1601AITask(context.BoardNumber);
                    aiTask.Mode = AIMode.Continuous;
                    aiTask.SampleRate = sampleRate;

                    // 添加所有通道
                    foreach (var channel in channels)
                    {
                        aiTask.AddChannel(channel, -10, 10);
                    }

                    streamContext.AITask = aiTask;

                    // 启动异步数据采集
                    streamContext.CancellationTokenSource = new CancellationTokenSource();
                    streamContext.StreamTask = Task.Run(() => 
                        StreamDataAcquisition(streamContext), 
                        streamContext.CancellationTokenSource.Token);

                    // 保存流上下文
                    _activeStreams[deviceId] = streamContext;

                    _logger.LogInformation("Started stream: DeviceId={DeviceId}, StreamId={StreamId}, Channels={Channels}, Rate={Rate}Hz",
                        deviceId, streamContext.StreamId, string.Join(",", channels), sampleRate);

                    return new
                    {
                        success = true,
                        streamId = streamContext.StreamId,
                        deviceId = deviceId,
                        channels = channels,
                        sampleRate = sampleRate,
                        bufferSize = streamContext.BufferSize,
                        status = "streaming",
                        timestamp = streamContext.StartTime
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to start stream");
                    return new
                    {
                        success = false,
                        error = $"Failed to start stream: {ex.Message}"
                    };
                }
            });
        }

        /// <summary>
        /// 停止数据流采集
        /// </summary>
        public async Task<object> StopStream(string? deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }

            return await Task.Run<object>(async () =>
            {
                try
                {
                    if (!_activeStreams.TryRemove(deviceId, out var streamContext))
                    {
                        return new { success = false, error = "No active stream for this device" };
                    }

                    // 请求取消
                    streamContext.CancellationTokenSource?.Cancel();

                    // 等待任务完成
                    if (streamContext.StreamTask != null)
                    {
                        await streamContext.StreamTask.ConfigureAwait(false);
                    }

                    // 停止并释放AI任务
                    streamContext.AITask?.Stop();
                    streamContext.AITask = null;

                    var duration = DateTime.UtcNow - streamContext.StartTime;

                    _logger.LogInformation("Stopped stream: DeviceId={DeviceId}, StreamId={StreamId}, Duration={Duration}s, TotalSamples={Samples}",
                        deviceId, streamContext.StreamId, duration.TotalSeconds, streamContext.TotalSamples);

                    return new
                    {
                        success = true,
                        streamId = streamContext.StreamId,
                        deviceId = deviceId,
                        totalSamples = streamContext.TotalSamples,
                        duration = duration.TotalSeconds,
                        status = "stopped",
                        timestamp = DateTime.UtcNow
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to stop stream");
                    return new
                    {
                        success = false,
                        error = $"Failed to stop stream: {ex.Message}"
                    };
                }
            });
        }

        /// <summary>
        /// 获取流数据
        /// </summary>
        public async Task<object> GetStreamData(string? deviceId, int maxSamples = 1000)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }

            return await Task.Run<object>(() =>
            {
                try
                {
                    if (!_activeStreams.TryGetValue(deviceId, out var streamContext))
                    {
                        return new { success = false, error = "No active stream for this device" };
                    }

                    var data = new List<double[]>();
                    int samplesRead = 0;

                    // 从缓冲区读取数据
                    while (samplesRead < maxSamples && streamContext.DataBuffer.TryDequeue(out var sample))
                    {
                        data.Add(sample);
                        samplesRead++;
                    }

                    return new
                    {
                        success = true,
                        streamId = streamContext.StreamId,
                        deviceId = deviceId,
                        channels = streamContext.Channels,
                        sampleCount = samplesRead,
                        data = data,
                        bufferSize = streamContext.DataBuffer.Count,
                        totalSamples = streamContext.TotalSamples,
                        timestamp = DateTime.UtcNow
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get stream data");
                    return new
                    {
                        success = false,
                        error = $"Failed to get stream data: {ex.Message}"
                    };
                }
            });
        }

        /// <summary>
        /// 获取流状态
        /// </summary>
        public async Task<object> GetStreamStatus(string? deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }

            return await Task.Run<object>(() =>
            {
                if (!_activeStreams.TryGetValue(deviceId, out var streamContext))
                {
                    return new
                    {
                        success = true,
                        deviceId = deviceId,
                        status = "idle",
                        hasStream = false
                    };
                }

                var duration = DateTime.UtcNow - streamContext.StartTime;

                return new
                {
                    success = true,
                    streamId = streamContext.StreamId,
                    deviceId = deviceId,
                    status = "streaming",
                    hasStream = true,
                    channels = streamContext.Channels,
                    sampleRate = streamContext.SampleRate,
                    totalSamples = streamContext.TotalSamples,
                    bufferCount = streamContext.DataBuffer.Count,
                    duration = duration.TotalSeconds,
                    startTime = streamContext.StartTime,
                    timestamp = DateTime.UtcNow
                };
            });
        }

        /// <summary>
        /// 数据采集任务
        /// </summary>
        private void StreamDataAcquisition(StreamContext context)
        {
            try
            {
                context.AITask?.Start();

                var buffer = new double[context.BufferSize];
                var token = context.CancellationTokenSource?.Token ?? default;

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // 读取数据
                        context.AITask?.ReadData(ref buffer, context.BufferSize, 100);
                        var samplesRead = context.BufferSize; // Assume full buffer read
                        
                        if (samplesRead > 0)
                        {
                            // 将数据添加到缓冲队列
                            int samplesPerChannel = samplesRead / context.Channels.Length;
                            
                            for (int s = 0; s < samplesPerChannel; s++)
                            {
                                var sample = new double[context.Channels.Length];
                                for (int ch = 0; ch < context.Channels.Length; ch++)
                                {
                                    sample[ch] = Math.Round(buffer[s * context.Channels.Length + ch], 4);
                                }
                                
                                context.DataBuffer.Enqueue(sample);
                                context.TotalSamples++;

                                // 限制缓冲区大小（保留最近10秒的数据）
                                var maxBufferSamples = (int)(context.SampleRate * 10);
                                while (context.DataBuffer.Count > maxBufferSamples)
                                {
                                    context.DataBuffer.TryDequeue(out _);
                                }
                            }
                        }
                    }
                    catch (TimeoutException)
                    {
                        // 超时是正常的，继续循环
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in stream data acquisition");
            }
        }

        /// <summary>
        /// 清理所有流
        /// </summary>
        public void Dispose()
        {
            foreach (var deviceId in _activeStreams.Keys.ToList())
            {
                _ = StopStream(deviceId).Result;
            }
            _activeStreams.Clear();
        }
    }

    /// <summary>
    /// 流上下文
    /// </summary>
    internal class StreamContext
    {
        public string StreamId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public int[] Channels { get; set; } = Array.Empty<int>();
        public double SampleRate { get; set; }
        public DateTime StartTime { get; set; }
        public long TotalSamples { get; set; }
        public int BufferSize { get; set; }
        public ConcurrentQueue<double[]> DataBuffer { get; set; } = new();
        public JYUSB1601AITask? AITask { get; set; }
        public Task? StreamTask { get; set; }
        public CancellationTokenSource? CancellationTokenSource { get; set; }
    }
}