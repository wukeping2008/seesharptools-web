using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using JYUSB1601;
using USB1601MCP.Models;

namespace USB1601MCP.Tools
{
    /// <summary>
    /// 设备管理工具类
    /// 处理USB-1601设备的发现、连接和断开
    /// </summary>
    public class DeviceTools
    {
        private readonly ILogger<DeviceTools> _logger;
        private readonly Dictionary<string, DeviceContext> _devices;

        public DeviceTools(ILogger<DeviceTools> logger)
        {
            _logger = logger;
            _devices = new Dictionary<string, DeviceContext>();
        }

        /// <summary>
        /// 发现所有连接的USB-1601设备
        /// </summary>
        public async Task<object> DiscoverDevices()
        {
            return await Task.Run<object>(() =>
            {
                var devices = new List<DeviceInfo>();
                
                // 尝试扫描前8个设备槽位
                for (int i = 0; i < 8; i++)
                {
                    try
                    {
                        var boardNum = i.ToString();
                        var testTask = new JYUSB1601AITask(boardNum);
                        
                        // 如果能创建任务，说明设备存在
                        var deviceInfo = new DeviceInfo
                        {
                            DeviceId = boardNum,
                            DeviceName = $"USB-1601 Board {boardNum}",
                            SerialNumber = $"SN-{boardNum:D4}",
                            IsConnected = _devices.ContainsKey(boardNum),
                            Capabilities = new DeviceCapabilities()
                        };
                        
                        devices.Add(deviceInfo);
                        _logger.LogInformation("Discovered device: {DeviceId}", boardNum);
                    }
                    catch
                    {
                        // 该槽位没有设备，继续扫描下一个
                    }
                }
                
                return new
                {
                    success = true,
                    deviceCount = devices.Count,
                    devices = devices
                };
            });
        }

        /// <summary>
        /// 连接到指定的USB-1601设备
        /// </summary>
        public async Task<object> ConnectDevice(string? deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }
            
            return await Task.Run<object>(() =>
            {
                try
                {
                    if (_devices.ContainsKey(deviceId))
                    {
                        return new 
                        { 
                            success = true, 
                            message = "Device already connected",
                            deviceId = deviceId
                        };
                    }
                    
                    // 创建设备上下文
                    var context = new DeviceContext
                    {
                        DeviceId = deviceId,
                        BoardNumber = deviceId
                    };
                    
                    // 测试连接
                    var testTask = new JYUSB1601AITask(deviceId);
                    testTask = null; // JYUSB1601 tasks don't implement IDisposable
                    
                    _devices[deviceId] = context;
                    _logger.LogInformation("Connected to device: {DeviceId}", deviceId);
                    
                    return new
                    {
                        success = true,
                        message = "Device connected successfully",
                        deviceId = deviceId,
                        capabilities = new DeviceCapabilities()
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to connect to device: {DeviceId}", deviceId);
                    return new
                    {
                        success = false,
                        error = $"Failed to connect: {ex.Message}",
                        deviceId = deviceId
                    };
                }
            });
        }

        /// <summary>
        /// 断开与USB-1601设备的连接
        /// </summary>
        public async Task<object> DisconnectDevice(string? deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return new { success = false, error = "Device ID is required" };
            }
            
            return await Task.Run<object>(() =>
            {
                try
                {
                    if (!_devices.ContainsKey(deviceId))
                    {
                        return new 
                        { 
                            success = true, 
                            message = "Device not connected",
                            deviceId = deviceId
                        };
                    }
                    
                    var context = _devices[deviceId];
                    
                    // 清理所有任务
                    context.Dispose();
                    
                    _devices.Remove(deviceId);
                    _logger.LogInformation("Disconnected from device: {DeviceId}", deviceId);
                    
                    return new
                    {
                        success = true,
                        message = "Device disconnected successfully",
                        deviceId = deviceId
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to disconnect from device: {DeviceId}", deviceId);
                    return new
                    {
                        success = false,
                        error = $"Failed to disconnect: {ex.Message}",
                        deviceId = deviceId
                    };
                }
            });
        }

        /// <summary>
        /// 获取设备上下文
        /// </summary>
        public DeviceContext? GetDeviceContext(string deviceId)
        {
            return _devices.TryGetValue(deviceId, out var context) ? context : null;
        }

        /// <summary>
        /// 清理所有设备连接
        /// </summary>
        public void Dispose()
        {
            foreach (var device in _devices.Values)
            {
                device.Dispose();
            }
            _devices.Clear();
        }
    }

    /// <summary>
    /// 设备上下文类
    /// 管理单个设备的任务和资源
    /// </summary>
    public class DeviceContext : IDisposable
    {
        public string DeviceId { get; set; } = string.Empty;
        public string BoardNumber { get; set; } = string.Empty;
        public JYUSB1601AITask? AITask { get; set; }
        public JYUSB1601AOTask? AOTask { get; set; }
        public JYUSB1601DITask? DITask { get; set; }
        public JYUSB1601DOTask? DOTask { get; set; }

        public void Dispose()
        {
            AITask?.Stop();
            AITask = null;
            AOTask?.Stop();
            AOTask = null;
            DITask = null;
            DOTask = null;
        }
    }
}