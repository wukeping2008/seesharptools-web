using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Services.Hardware;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// Device detection and management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceDetectionController : ControllerBase
    {
        private readonly IDeviceAutoDetectionService _detectionService;
        private readonly IHardwareErrorHandler _errorHandler;
        private readonly ILogger<DeviceDetectionController> _logger;

        public DeviceDetectionController(
            IDeviceAutoDetectionService detectionService,
            IHardwareErrorHandler errorHandler,
            ILogger<DeviceDetectionController> logger)
        {
            _detectionService = detectionService;
            _errorHandler = errorHandler;
            _logger = logger;
        }

        /// <summary>
        /// Scan for all available devices
        /// </summary>
        [HttpGet("scan")]
        public async Task<IActionResult> ScanDevices([FromQuery] DeviceScanOptions? options = null)
        {
            try
            {
                var result = await _detectionService.ScanAllDevicesAsync(options);
                return Ok(new
                {
                    success = true,
                    data = result,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning devices");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to scan devices",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Scan for specific device type
        /// </summary>
        [HttpGet("scan/{deviceType}")]
        public async Task<IActionResult> ScanDevicesByType(string deviceType, [FromQuery] DeviceScanOptions? options = null)
        {
            try
            {
                var result = await _detectionService.ScanDevicesByTypeAsync(deviceType, options);
                return Ok(new
                {
                    success = true,
                    data = result,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scanning {deviceType} devices");
                return StatusCode(500, new
                {
                    success = false,
                    error = $"Failed to scan {deviceType} devices",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get currently connected devices
        /// </summary>
        [HttpGet("connected")]
        public async Task<IActionResult> GetConnectedDevices()
        {
            try
            {
                var devices = await _detectionService.GetConnectedDevicesAsync();
                return Ok(new
                {
                    success = true,
                    data = devices,
                    count = devices.Count,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connected devices");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to get connected devices",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Auto-configure a detected device
        /// </summary>
        [HttpPost("configure/{deviceId}")]
        public async Task<IActionResult> AutoConfigureDevice(string deviceId)
        {
            try
            {
                var success = await _detectionService.AutoConfigureDeviceAsync(deviceId);
                
                if (success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Device {deviceId} configured successfully",
                        timestamp = DateTime.Now
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "Failed to configure device",
                        message = "Device configuration failed. Check if compatible drivers are available."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error configuring device {deviceId}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to configure device",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get device detection history
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetDetectionHistory([FromQuery] int maxCount = 100)
        {
            try
            {
                var history = await _detectionService.GetDetectionHistoryAsync(maxCount);
                return Ok(new
                {
                    success = true,
                    data = history,
                    count = history.Count,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detection history");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to get detection history",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Start device monitoring
        /// </summary>
        [HttpPost("monitoring/start")]
        public async Task<IActionResult> StartMonitoring()
        {
            try
            {
                await _detectionService.StartMonitoringAsync();
                return Ok(new
                {
                    success = true,
                    message = "Device monitoring started",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting device monitoring");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to start monitoring",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Stop device monitoring
        /// </summary>
        [HttpPost("monitoring/stop")]
        public async Task<IActionResult> StopMonitoring()
        {
            try
            {
                await _detectionService.StopMonitoringAsync();
                return Ok(new
                {
                    success = true,
                    message = "Device monitoring stopped",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping device monitoring");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to stop monitoring",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get hardware error statistics
        /// </summary>
        [HttpGet("errors/statistics")]
        public async Task<IActionResult> GetErrorStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Now.AddDays(-30);
                var end = endDate ?? DateTime.Now;
                
                var statistics = await _errorHandler.GetErrorStatisticsAsync(start, end);
                return Ok(new
                {
                    success = true,
                    data = statistics,
                    period = new { start, end },
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting error statistics");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to get error statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get error history for a specific device
        /// </summary>
        [HttpGet("errors/{deviceId}")]
        public async Task<IActionResult> GetDeviceErrorHistory(string deviceId, [FromQuery] int maxCount = 100)
        {
            try
            {
                var history = await _errorHandler.GetErrorHistoryAsync(deviceId, maxCount);
                return Ok(new
                {
                    success = true,
                    data = history,
                    deviceId,
                    count = history.Count,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting error history for device {deviceId}");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to get error history",
                    message = ex.Message
                });
            }
        }
    }
}