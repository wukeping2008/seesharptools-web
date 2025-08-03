using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SeeSharpBackend.Models.MISD;
using SeeSharpBackend.Services.Drivers;

namespace SeeSharpBackend.Services.Hardware
{
    /// <summary>
    /// Device auto-detection service implementation
    /// </summary>
    public class DeviceAutoDetectionService : IDeviceAutoDetectionService, IDisposable
    {
        private readonly ILogger<DeviceAutoDetectionService> _logger;
        private readonly IDriverManager _driverManager;
        private readonly List<Action<DeviceChangeEvent>> _deviceChangeHandlers;
        private readonly Dictionary<string, DetectedDevice> _connectedDevices;
        private readonly SemaphoreSlim _scanSemaphore;
        private CancellationTokenSource? _monitoringCts;
        private Task? _monitoringTask;
        private readonly List<DeviceDetectionLog> _detectionHistory;

        public DeviceAutoDetectionService(
            ILogger<DeviceAutoDetectionService> logger,
            IDriverManager driverManager)
        {
            _logger = logger;
            _driverManager = driverManager;
            _deviceChangeHandlers = new List<Action<DeviceChangeEvent>>();
            _connectedDevices = new Dictionary<string, DetectedDevice>();
            _scanSemaphore = new SemaphoreSlim(1, 1);
            _detectionHistory = new List<DeviceDetectionLog>();
        }

        public async Task StartMonitoringAsync(CancellationToken cancellationToken = default)
        {
            if (_monitoringTask != null)
            {
                _logger.LogWarning("Device monitoring is already running");
                return;
            }

            _monitoringCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _monitoringTask = MonitorDevicesAsync(_monitoringCts.Token);
            
            _logger.LogInformation("Device monitoring started");
            await Task.CompletedTask;
        }

        public async Task StopMonitoringAsync()
        {
            if (_monitoringCts == null || _monitoringTask == null)
            {
                return;
            }

            _monitoringCts.Cancel();
            
            try
            {
                await _monitoringTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            finally
            {
                _monitoringCts.Dispose();
                _monitoringCts = null;
                _monitoringTask = null;
            }

            _logger.LogInformation("Device monitoring stopped");
        }

        public async Task<DeviceScanResult> ScanAllDevicesAsync(DeviceScanOptions? options = null)
        {
            options ??= new DeviceScanOptions();
            var stopwatch = Stopwatch.StartNew();
            var result = new DeviceScanResult
            {
                Timestamp = DateTime.Now
            };

            await _scanSemaphore.WaitAsync();
            try
            {
                var scanTasks = new List<Task<List<DetectedDevice>>>();

                foreach (var connectionType in options.ConnectionTypes)
                {
                    scanTasks.Add(ScanConnectionTypeAsync(connectionType, options));
                }

                var allResults = await Task.WhenAll(scanTasks);
                
                foreach (var devices in allResults)
                {
                    result.Devices.AddRange(devices);
                }

                // Update connected devices cache
                UpdateConnectedDevices(result.Devices);

                // Calculate statistics
                result.Statistics = CalculateStatistics(result.Devices);
                
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;

                // Log detection
                await LogDetectionAsync("FullScan", $"Found {result.Devices.Count} devices", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during device scan");
                result.Errors.Add(new ScanError
                {
                    ErrorCode = "SCAN_ERROR",
                    Message = ex.Message,
                    Timestamp = DateTime.Now
                });

                await LogDetectionAsync("FullScan", $"Scan failed: {ex.Message}", false);
            }
            finally
            {
                _scanSemaphore.Release();
            }

            return result;
        }

        public async Task<DeviceScanResult> ScanDevicesByTypeAsync(string deviceType, DeviceScanOptions? options = null)
        {
            options ??= new DeviceScanOptions();
            options.DeviceTypes = new List<string> { deviceType };
            
            return await ScanAllDevicesAsync(options);
        }

        public async Task<List<DetectedDevice>> GetConnectedDevicesAsync()
        {
            await Task.CompletedTask;
            lock (_connectedDevices)
            {
                return _connectedDevices.Values.ToList();
            }
        }

        public void RegisterDeviceChangeHandler(Action<DeviceChangeEvent> handler)
        {
            lock (_deviceChangeHandlers)
            {
                _deviceChangeHandlers.Add(handler);
            }
        }

        public void UnregisterDeviceChangeHandler(Action<DeviceChangeEvent> handler)
        {
            lock (_deviceChangeHandlers)
            {
                _deviceChangeHandlers.Remove(handler);
            }
        }

        public async Task<bool> AutoConfigureDeviceAsync(string deviceId)
        {
            try
            {
                DetectedDevice? device;
                lock (_connectedDevices)
                {
                    if (!_connectedDevices.TryGetValue(deviceId, out device))
                    {
                        _logger.LogWarning($"Device {deviceId} not found");
                        return false;
                    }
                }

                // Try to find a suitable driver based on device type
                var driverName = GetDriverNameForDevice(device);
                if (string.IsNullOrEmpty(driverName))
                {
                    _logger.LogWarning($"No compatible driver found for device {device.DeviceType}");
                    return false;
                }

                // Initialize driver
                var driverConfig = new Drivers.DriverConfiguration
                {
                    DriverPath = $"Drivers/{driverName}.dll",
                    DeviceModel = device.DeviceType,
                    Parameters = new Dictionary<string, object>
                    {
                        ["SerialNumber"] = device.SerialNumber,
                        ["ConnectionType"] = device.ConnectionType.ToString(),
                        ["Port"] = device.ConnectionDetails.Port
                    }
                };

                var success = await _driverManager.LoadDriverAsync(driverName, driverConfig);
                
                if (success)
                {
                    _logger.LogInformation($"Successfully auto-configured device {deviceId}");
                    await LogDetectionAsync("AutoConfigure", $"Device {deviceId} configured with driver {driverName}", true);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error auto-configuring device {deviceId}");
                await LogDetectionAsync("AutoConfigure", $"Failed to configure device {deviceId}: {ex.Message}", false);
                return false;
            }
        }

        public async Task<List<DeviceDetectionLog>> GetDetectionHistoryAsync(int maxCount = 100)
        {
            await Task.CompletedTask;
            lock (_detectionHistory)
            {
                return _detectionHistory
                    .OrderByDescending(l => l.Timestamp)
                    .Take(maxCount)
                    .ToList();
            }
        }

        public void Dispose()
        {
            StopMonitoringAsync().Wait();
            _scanSemaphore?.Dispose();
            _monitoringCts?.Dispose();
        }

        #region Private Methods

        private string GetDriverNameForDevice(DetectedDevice device)
        {
            // Map device types to driver names
            return device.DeviceType switch
            {
                "JY5500" => "JY5500",
                "JYUSB1601" => "JYUSB1601",
                _ => string.Empty
            };
        }

        private async Task MonitorDevicesAsync(CancellationToken cancellationToken)
        {
            var previousDevices = new Dictionary<string, DetectedDevice>();
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var scanResult = await ScanAllDevicesAsync(new DeviceScanOptions
                    {
                        TimeoutMs = 5000,
                        DeepScan = false
                    });

                    // Detect changes
                    var currentDevices = scanResult.Devices.ToDictionary(d => d.DeviceId);
                    
                    // Check for new devices
                    foreach (var device in currentDevices.Values)
                    {
                        if (!previousDevices.ContainsKey(device.DeviceId))
                        {
                            NotifyDeviceChange(new DeviceChangeEvent
                            {
                                ChangeType = DeviceChangeType.Connected,
                                Device = device,
                                Timestamp = DateTime.Now
                            });
                        }
                    }

                    // Check for removed devices
                    foreach (var device in previousDevices.Values)
                    {
                        if (!currentDevices.ContainsKey(device.DeviceId))
                        {
                            NotifyDeviceChange(new DeviceChangeEvent
                            {
                                ChangeType = DeviceChangeType.Disconnected,
                                Device = device,
                                Timestamp = DateTime.Now
                            });
                        }
                    }

                    // Check for status changes
                    foreach (var device in currentDevices.Values)
                    {
                        if (previousDevices.TryGetValue(device.DeviceId, out var previousDevice))
                        {
                            if (device.Status != previousDevice.Status)
                            {
                                NotifyDeviceChange(new DeviceChangeEvent
                                {
                                    ChangeType = DeviceChangeType.StatusChanged,
                                    Device = device,
                                    PreviousState = previousDevice,
                                    Timestamp = DateTime.Now
                                });
                            }
                        }
                    }

                    previousDevices = currentDevices;
                    
                    // Wait before next scan
                    await Task.Delay(5000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during device monitoring");
                    await Task.Delay(10000, cancellationToken);
                }
            }
        }

        private async Task<List<DetectedDevice>> ScanConnectionTypeAsync(ConnectionType connectionType, DeviceScanOptions options)
        {
            var devices = new List<DetectedDevice>();

            try
            {
                switch (connectionType)
                {
                    case ConnectionType.USB:
                        devices.AddRange(await ScanUsbDevicesAsync(options));
                        break;
                    case ConnectionType.PCI:
                    case ConnectionType.PCIe:
                        devices.AddRange(await ScanPciDevicesAsync(options));
                        break;
                    case ConnectionType.Network:
                        devices.AddRange(await ScanNetworkDevicesAsync(options));
                        break;
                    case ConnectionType.Serial:
                        devices.AddRange(await ScanSerialDevicesAsync(options));
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scanning {connectionType} devices");
            }

            return devices;
        }

        private async Task<List<DetectedDevice>> ScanUsbDevicesAsync(DeviceScanOptions options)
        {
            var devices = new List<DetectedDevice>();

            await Task.Run(() =>
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // Use WMI on Windows
                        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_USBControllerDevice");
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var dependent = obj["Dependent"] as string;
                            if (dependent != null)
                            {
                                var device = ParseWindowsUsbDevice(dependent);
                                if (device != null && MatchesFilter(device, options))
                                {
                                    devices.Add(device);
                                }
                            }
                        }
                    }
                    else
                    {
                        // On Linux/macOS, would use libusb or system calls
                        _logger.LogInformation("USB scanning not implemented for this platform");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scanning USB devices");
                }
            });

            // Add mock devices for testing
            if (options.DeviceTypes.Count == 0 || options.DeviceTypes.Contains("JY5500"))
            {
                devices.Add(CreateMockDevice("JY5500", ConnectionType.USB));
            }
            if (options.DeviceTypes.Count == 0 || options.DeviceTypes.Contains("JYUSB1601"))
            {
                devices.Add(CreateMockDevice("JYUSB1601", ConnectionType.USB));
            }

            return devices;
        }

        private async Task<List<DetectedDevice>> ScanPciDevicesAsync(DeviceScanOptions options)
        {
            var devices = new List<DetectedDevice>();

            await Task.Run(() =>
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // Use WMI on Windows
                        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE PNPClass = 'System'");
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var name = obj["Name"] as string;
                            var deviceId = obj["DeviceID"] as string;
                            
                            if (!string.IsNullOrEmpty(deviceId) && deviceId.StartsWith("PCI"))
                            {
                                var device = CreatePciDevice(name ?? "Unknown PCI Device", deviceId);
                                if (MatchesFilter(device, options))
                                {
                                    devices.Add(device);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scanning PCI devices");
                }
            });

            return devices;
        }

        private async Task<List<DetectedDevice>> ScanNetworkDevicesAsync(DeviceScanOptions options)
        {
            // Network device scanning would involve network discovery protocols
            // For now, returning empty list
            await Task.CompletedTask;
            return new List<DetectedDevice>();
        }

        private async Task<List<DetectedDevice>> ScanSerialDevicesAsync(DeviceScanOptions options)
        {
            var devices = new List<DetectedDevice>();

            await Task.Run(() =>
            {
                try
                {
                    var portNames = System.IO.Ports.SerialPort.GetPortNames();
                    foreach (var portName in portNames)
                    {
                        var device = new DetectedDevice
                        {
                            DeviceId = $"SERIAL_{portName}",
                            DeviceName = $"Serial Port {portName}",
                            DeviceType = "SerialPort",
                            ConnectionType = ConnectionType.Serial,
                            Status = DeviceStatus.Online,
                            DetectedAt = DateTime.Now,
                            ConnectionDetails = new ConnectionDetails
                            {
                                Port = portName
                            }
                        };

                        if (MatchesFilter(device, options))
                        {
                            devices.Add(device);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scanning serial devices");
                }
            });

            return devices;
        }

        private DetectedDevice? ParseWindowsUsbDevice(string devicePath)
        {
            // Parse Windows USB device path
            // This is a simplified implementation
            return null;
        }

        private DetectedDevice CreatePciDevice(string name, string deviceId)
        {
            return new DetectedDevice
            {
                DeviceId = deviceId,
                DeviceName = name,
                DeviceType = "PCI",
                ConnectionType = ConnectionType.PCI,
                Status = DeviceStatus.Online,
                DetectedAt = DateTime.Now
            };
        }

        private DetectedDevice CreateMockDevice(string deviceType, ConnectionType connectionType)
        {
            var serialNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            
            return new DetectedDevice
            {
                DeviceId = $"{deviceType}_{serialNumber}",
                DeviceName = $"简仪 {deviceType}",
                DeviceType = deviceType,
                Manufacturer = "简仪科技",
                Model = deviceType,
                SerialNumber = serialNumber,
                FirmwareVersion = "1.0.0",
                ConnectionType = connectionType,
                Status = DeviceStatus.Online,
                DetectedAt = DateTime.Now,
                ConnectionDetails = new ConnectionDetails
                {
                    Port = connectionType == ConnectionType.USB ? "USB001" : "PCI001",
                    VendorId = "0x1234",
                    ProductId = "0x5678",
                    Speed = connectionType == ConnectionType.USB ? "USB 3.0" : "PCIe 3.0"
                },
                Capabilities = new DeviceCapabilities
                {
                    SupportedOperations = new List<string> { "Generate", "Measure", "Analyze" },
                    MaxChannels = deviceType == "JY5500" ? 2 : 16,
                    MaxSamplingRate = deviceType == "JY5500" ? 1000000 : 500000,
                    ResolutionBits = deviceType == "JY5500" ? 14 : 16
                }
            };
        }

        private bool MatchesFilter(DetectedDevice device, DeviceScanOptions options)
        {
            if (options.DeviceTypes.Any() && !options.DeviceTypes.Contains(device.DeviceType))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(options.ManufacturerFilter) && 
                !device.Manufacturer.Contains(options.ManufacturerFilter, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!options.IncludeOfflineDevices && device.Status == DeviceStatus.Offline)
            {
                return false;
            }

            return true;
        }

        private void UpdateConnectedDevices(List<DetectedDevice> devices)
        {
            lock (_connectedDevices)
            {
                _connectedDevices.Clear();
                foreach (var device in devices)
                {
                    _connectedDevices[device.DeviceId] = device;
                }
            }
        }

        private ScanStatistics CalculateStatistics(List<DetectedDevice> devices)
        {
            var stats = new ScanStatistics
            {
                TotalDevicesFound = devices.Count
            };

            foreach (var device in devices)
            {
                // By connection type
                if (!stats.DevicesByConnectionType.ContainsKey(device.ConnectionType))
                {
                    stats.DevicesByConnectionType[device.ConnectionType] = 0;
                }
                stats.DevicesByConnectionType[device.ConnectionType]++;

                // By manufacturer
                if (!string.IsNullOrEmpty(device.Manufacturer))
                {
                    if (!stats.DevicesByManufacturer.ContainsKey(device.Manufacturer))
                    {
                        stats.DevicesByManufacturer[device.Manufacturer] = 0;
                    }
                    stats.DevicesByManufacturer[device.Manufacturer]++;
                }

                // By status
                if (!stats.DevicesByStatus.ContainsKey(device.Status))
                {
                    stats.DevicesByStatus[device.Status] = 0;
                }
                stats.DevicesByStatus[device.Status]++;
            }

            return stats;
        }

        private void NotifyDeviceChange(DeviceChangeEvent changeEvent)
        {
            _logger.LogInformation($"Device change: {changeEvent.ChangeType} - {changeEvent.Device.DeviceId}");

            List<Action<DeviceChangeEvent>> handlers;
            lock (_deviceChangeHandlers)
            {
                handlers = _deviceChangeHandlers.ToList();
            }

            foreach (var handler in handlers)
            {
                try
                {
                    handler(changeEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in device change handler");
                }
            }
        }

        private async Task LogDetectionAsync(string eventType, string details, bool success)
        {
            await Task.CompletedTask;
            
            var log = new DeviceDetectionLog
            {
                Id = _detectionHistory.Count + 1,
                Timestamp = DateTime.Now,
                EventType = eventType,
                Details = details,
                Success = success
            };

            lock (_detectionHistory)
            {
                _detectionHistory.Add(log);
                
                // Keep only last 1000 entries
                if (_detectionHistory.Count > 1000)
                {
                    _detectionHistory.RemoveAt(0);
                }
            }
        }

        #endregion
    }
}