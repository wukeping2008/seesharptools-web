using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using JYUSB1601;

namespace USB1601MCP.Tools
{
    /// <summary>
    /// 数字I/O工具类
    /// 处理USB-1601的数字输入输出操作
    /// </summary>
    public class DigitalTools
    {
        private readonly ILogger<DigitalTools> _logger;
        private readonly DeviceTools _deviceTools;

        public DigitalTools(ILogger<DigitalTools> logger, DeviceTools deviceTools)
        {
            _logger = logger;
            _deviceTools = deviceTools;
        }

        /// <summary>
        /// 读取数字输入端口
        /// </summary>
        public async Task<object> ReadPort(string? deviceId, int port)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }

            if (port < 0 || port > 1)
            {
                return new { success = false, error = "Port must be 0 or 1" };
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

                    // 创建或重用DI任务
                    if (context.DITask == null)
                    {
                        context.DITask = new JYUSB1601DITask(context.BoardNumber);
                    }

                    // 读取端口值
                    var portBits = new bool[8];
                    context.DITask.ReadSinglePoint(ref portBits);
                    
                    // 转换为字节值
                    byte portValue = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        if (portBits[i])
                            portValue |= (byte)(1 << i);
                    }

                    _logger.LogDebug("Read DI port: Port={Port}, Value={Value:X2}", port, portValue);

                    // 将字节值转换为位数组
                    var bits = new bool[8];
                    for (int i = 0; i < 8; i++)
                    {
                        bits[i] = (portValue & (1 << i)) != 0;
                    }

                    return new
                    {
                        success = true,
                        deviceId = deviceId,
                        port = port,
                        value = portValue,
                        bits = bits,
                        hexValue = $"0x{portValue:X2}",
                        binaryValue = Convert.ToString(portValue, 2).PadLeft(8, '0'),
                        timestamp = DateTime.UtcNow
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to read digital input port");
                    return new
                    {
                        success = false,
                        error = $"Failed to read digital input: {ex.Message}"
                    };
                }
            });
        }

        /// <summary>
        /// 写入数字输出端口
        /// </summary>
        public async Task<object> WritePort(string? deviceId, int port, byte value)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }

            if (port < 0 || port > 1)
            {
                return new { success = false, error = "Port must be 0 or 1" };
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

                    // 创建或重用DO任务
                    if (context.DOTask == null)
                    {
                        context.DOTask = new JYUSB1601DOTask(context.BoardNumber);
                    }

                    // 转换字节为位数组
                    var portBits = new bool[8];
                    for (int i = 0; i < 8; i++)
                    {
                        portBits[i] = (value & (1 << i)) != 0;
                    }
                    
                    // 写入端口值
                    context.DOTask.WriteSinglePoint(portBits);

                    _logger.LogDebug("Wrote DO port: Port={Port}, Value={Value:X2}", port, value);

                    // 将字节值转换为位数组
                    var bits = new bool[8];
                    for (int i = 0; i < 8; i++)
                    {
                        bits[i] = (value & (1 << i)) != 0;
                    }

                    return new
                    {
                        success = true,
                        deviceId = deviceId,
                        port = port,
                        value = value,
                        bits = bits,
                        hexValue = $"0x{value:X2}",
                        binaryValue = Convert.ToString(value, 2).PadLeft(8, '0'),
                        timestamp = DateTime.UtcNow
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to write digital output port");
                    return new
                    {
                        success = false,
                        error = $"Failed to write digital output: {ex.Message}"
                    };
                }
            });
        }

        /// <summary>
        /// 读取单个数字输入线
        /// </summary>
        public async Task<object> ReadLine(string? deviceId, int port, int line)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }

            if (port < 0 || port > 1)
            {
                return new { success = false, error = "Port must be 0 or 1" };
            }

            if (line < 0 || line > 7)
            {
                return new { success = false, error = "Line must be between 0 and 7" };
            }

            var portResult = await ReadPort(deviceId, port);
            
            if (portResult is IDictionary<string, object> dict && dict.ContainsKey("success"))
            {
                if ((bool)dict["success"] && dict.ContainsKey("bits"))
                {
                    var bits = (bool[])dict["bits"];
                    return new
                    {
                        success = true,
                        deviceId = deviceId,
                        port = port,
                        line = line,
                        value = bits[line],
                        timestamp = DateTime.UtcNow
                    };
                }
            }

            return portResult;
        }

        /// <summary>
        /// 写入单个数字输出线
        /// </summary>
        public async Task<object> WriteLine(string? deviceId, int port, int line, bool value)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }

            if (port < 0 || port > 1)
            {
                return new { success = false, error = "Port must be 0 or 1" };
            }

            if (line < 0 || line > 7)
            {
                return new { success = false, error = "Line must be between 0 and 7" };
            }

            return await Task.Run(async () =>
            {
                try
                {
                    // 先读取当前端口值
                    var readResult = await ReadPort(deviceId, port);
                    if (readResult is IDictionary<string, object> dict && dict.ContainsKey("success"))
                    {
                        if (!(bool)dict["success"])
                        {
                            return readResult;
                        }

                        byte currentValue = (byte)dict["value"];
                        
                        // 修改指定位
                        if (value)
                        {
                            currentValue |= (byte)(1 << line);
                        }
                        else
                        {
                            currentValue &= (byte)~(1 << line);
                        }

                        // 写回端口
                        return await WritePort(deviceId, port, currentValue);
                    }

                    return new { success = false, error = "Failed to read current port value" };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to write digital output line");
                    return new
                    {
                        success = false,
                        error = $"Failed to write digital output line: {ex.Message}"
                    };
                }
            });
        }
    }
}