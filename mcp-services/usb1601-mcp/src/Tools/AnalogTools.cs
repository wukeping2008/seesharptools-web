using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using JYUSB1601;

namespace USB1601MCP.Tools
{
    /// <summary>
    /// 模拟I/O工具类
    /// 处理USB-1601的模拟输入输出操作
    /// </summary>
    public class AnalogTools
    {
        private readonly ILogger<AnalogTools> _logger;
        private readonly DeviceTools _deviceTools;

        public AnalogTools(ILogger<AnalogTools> logger, DeviceTools deviceTools)
        {
            _logger = logger;
            _deviceTools = deviceTools;
        }

        /// <summary>
        /// 读取单个模拟输入通道的值
        /// </summary>
        public async Task<object> ReadSingle(string? deviceId, int channel)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }

            if (channel < 0 || channel > 15)
            {
                return new { success = false, error = "Channel must be between 0 and 15" };
            }

            return await Task.Run<object>(() =>
            {
                try
                {
                    var context = _deviceTools.GetDeviceContext(deviceId);
                    if (context == null)
                    {
                        return new { success = false, error = "Device not connected" };
                    }

                    // 创建或重用AI任务
                    if (context.AITask == null)
                    {
                        context.AITask = new JYUSB1601AITask(context.BoardNumber);
                        context.AITask.Mode = AIMode.Single;
                    }

                    // 清除之前的通道配置
                    context.AITask.Channels.Clear();
                    
                    // 添加指定通道
                    context.AITask.AddChannel(channel, -10, 10);
                    
                    // 启动任务
                    context.AITask.Start();
                    
                    // 读取单个值
                    var values = new double[1];
                    context.AITask.ReadSinglePoint(ref values);
                    var value = values[0];
                    
                    // 停止任务
                    context.AITask.Stop();

                    _logger.LogDebug("Read single AI value: Channel={Channel}, Value={Value}", channel, value);

                    return new
                    {
                        success = true,
                        deviceId = deviceId,
                        channel = channel,
                        value = Math.Round(value, 4),
                        unit = "V",
                        timestamp = DateTime.UtcNow
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to read analog input");
                    return new
                    {
                        success = false,
                        error = $"Failed to read analog input: {ex.Message}"
                    };
                }
            });
        }

        /// <summary>
        /// 读取多个模拟输入通道
        /// </summary>
        public async Task<object> ReadMultiple(string? deviceId, int[]? channels, int sampleCount = 1)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }

            if (channels == null || channels.Length == 0)
            {
                return new { success = false, error = "Channels array is required" };
            }

            if (channels.Any(ch => ch < 0 || ch > 15))
            {
                return new { success = false, error = "All channels must be between 0 and 15" };
            }

            return await Task.Run<object>(() =>
            {
                try
                {
                    var context = _deviceTools.GetDeviceContext(deviceId);
                    if (context == null)
                    {
                        return new { success = false, error = "Device not connected" };
                    }

                    // 创建新的AI任务用于有限采集
                    var aiTask = new JYUSB1601AITask(context.BoardNumber);
                    aiTask.Mode = AIMode.Finite;
                    aiTask.SamplesToAcquire = sampleCount;
                    aiTask.SampleRate = 1000; // 默认采样率

                    // 添加所有指定通道
                    foreach (var channel in channels)
                    {
                        aiTask.AddChannel(channel, -10, 10);
                    }

                    // 准备数据缓冲区
                    var totalSamples = channels.Length * sampleCount;
                    var buffer = new double[totalSamples];

                    // 启动采集
                    aiTask.Start();

                    // 读取数据
                    aiTask.ReadData(ref buffer, totalSamples, -1);

                    // 停止任务
                    aiTask.Stop();

                    // 重组数据为二维数组 [通道][样本]
                    var data = new double[channels.Length][];
                    for (int ch = 0; ch < channels.Length; ch++)
                    {
                        data[ch] = new double[sampleCount];
                        for (int s = 0; s < sampleCount; s++)
                        {
                            data[ch][s] = Math.Round(buffer[s * channels.Length + ch], 4);
                        }
                    }

                    _logger.LogDebug("Read multiple AI channels: Channels={Channels}, Samples={SampleCount}", 
                        string.Join(",", channels), sampleCount);

                    return new
                    {
                        success = true,
                        deviceId = deviceId,
                        channels = channels,
                        sampleCount = sampleCount,
                        data = data,
                        unit = "V",
                        timestamp = DateTime.UtcNow
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to read multiple analog inputs");
                    return new
                    {
                        success = false,
                        error = $"Failed to read multiple analog inputs: {ex.Message}"
                    };
                }
            });
        }

        /// <summary>
        /// 写入单个模拟输出值
        /// </summary>
        public async Task<object> WriteSingle(string? deviceId, int channel, double value)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }

            if (channel < 0 || channel > 1)
            {
                return new { success = false, error = "Channel must be 0 or 1" };
            }

            if (value < -10 || value > 10)
            {
                return new { success = false, error = "Value must be between -10V and 10V" };
            }

            return await Task.Run<object>(() =>
            {
                try
                {
                    var context = _deviceTools.GetDeviceContext(deviceId);
                    if (context == null)
                    {
                        return new { success = false, error = "Device not connected" };
                    }

                    // 创建或重用AO任务
                    if (context.AOTask == null)
                    {
                        context.AOTask = new JYUSB1601AOTask(context.BoardNumber);
                        context.AOTask.Mode = AOMode.Single;
                        context.AOTask.UpdateRate = 1000;
                    }

                    // 清除之前的通道配置
                    context.AOTask.Channels.Clear();
                    
                    // 添加指定通道
                    context.AOTask.AddChannel(channel);
                    
                    // 启动任务
                    context.AOTask.Start();
                    
                    // 写入单个值
                    context.AOTask.WriteSinglePoint(new double[] { value });
                    
                    // 停止任务
                    context.AOTask.Stop();

                    _logger.LogDebug("Wrote single AO value: Channel={Channel}, Value={Value}", channel, value);

                    return new
                    {
                        success = true,
                        deviceId = deviceId,
                        channel = channel,
                        value = value,
                        unit = "V",
                        timestamp = DateTime.UtcNow
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to write analog output");
                    return new
                    {
                        success = false,
                        error = $"Failed to write analog output: {ex.Message}"
                    };
                }
            });
        }
    }
}