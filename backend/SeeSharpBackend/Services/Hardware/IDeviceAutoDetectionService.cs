using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SeeSharpBackend.Models.MISD;

namespace SeeSharpBackend.Services.Hardware
{
    /// <summary>
    /// Device auto-detection service interface
    /// </summary>
    public interface IDeviceAutoDetectionService
    {
        /// <summary>
        /// Start continuous device monitoring
        /// </summary>
        Task StartMonitoringAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop device monitoring
        /// </summary>
        Task StopMonitoringAsync();

        /// <summary>
        /// Scan for all available devices
        /// </summary>
        Task<DeviceScanResult> ScanAllDevicesAsync(DeviceScanOptions? options = null);

        /// <summary>
        /// Scan for specific device types
        /// </summary>
        Task<DeviceScanResult> ScanDevicesByTypeAsync(string deviceType, DeviceScanOptions? options = null);

        /// <summary>
        /// Get current connected devices
        /// </summary>
        Task<List<DetectedDevice>> GetConnectedDevicesAsync();

        /// <summary>
        /// Register device change event handler
        /// </summary>
        void RegisterDeviceChangeHandler(Action<DeviceChangeEvent> handler);

        /// <summary>
        /// Unregister device change event handler
        /// </summary>
        void UnregisterDeviceChangeHandler(Action<DeviceChangeEvent> handler);

        /// <summary>
        /// Auto-configure detected device
        /// </summary>
        Task<bool> AutoConfigureDeviceAsync(string deviceId);

        /// <summary>
        /// Get device detection history
        /// </summary>
        Task<List<DeviceDetectionLog>> GetDetectionHistoryAsync(int maxCount = 100);
    }

    /// <summary>
    /// Device scan options
    /// </summary>
    public class DeviceScanOptions
    {
        /// <summary>
        /// Scan timeout in milliseconds
        /// </summary>
        public int TimeoutMs { get; set; } = 10000;

        /// <summary>
        /// Include offline devices
        /// </summary>
        public bool IncludeOfflineDevices { get; set; } = false;

        /// <summary>
        /// Perform deep scan (slower but more thorough)
        /// </summary>
        public bool DeepScan { get; set; } = false;

        /// <summary>
        /// Connection types to scan
        /// </summary>
        public List<ConnectionType> ConnectionTypes { get; set; } = new()
        {
            ConnectionType.USB,
            ConnectionType.PCI,
            ConnectionType.PCIe,
            ConnectionType.Network
        };

        /// <summary>
        /// Device types to include
        /// </summary>
        public List<string> DeviceTypes { get; set; } = new();

        /// <summary>
        /// Manufacturer filter
        /// </summary>
        public string? ManufacturerFilter { get; set; }
    }

    /// <summary>
    /// Connection types
    /// </summary>
    public enum ConnectionType
    {
        USB,
        PCI,
        PCIe,
        Network,
        Serial,
        Bluetooth,
        WiFi,
        Other
    }

    /// <summary>
    /// Device scan result
    /// </summary>
    public class DeviceScanResult
    {
        /// <summary>
        /// Scan timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Total scan duration
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Detected devices
        /// </summary>
        public List<DetectedDevice> Devices { get; set; } = new();

        /// <summary>
        /// Scan errors
        /// </summary>
        public List<ScanError> Errors { get; set; } = new();

        /// <summary>
        /// Scan statistics
        /// </summary>
        public ScanStatistics Statistics { get; set; } = new();
    }

    /// <summary>
    /// Detected device information
    /// </summary>
    public class DetectedDevice
    {
        /// <summary>
        /// Unique device identifier
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Device name
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// Device type (e.g., JY5500, JYUSB1601)
        /// </summary>
        public string DeviceType { get; set; } = string.Empty;

        /// <summary>
        /// Manufacturer
        /// </summary>
        public string Manufacturer { get; set; } = string.Empty;

        /// <summary>
        /// Model number
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Serial number
        /// </summary>
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// Firmware version
        /// </summary>
        public string FirmwareVersion { get; set; } = string.Empty;

        /// <summary>
        /// Connection type
        /// </summary>
        public ConnectionType ConnectionType { get; set; }

        /// <summary>
        /// Connection details
        /// </summary>
        public ConnectionDetails ConnectionDetails { get; set; } = new();

        /// <summary>
        /// Device status
        /// </summary>
        public DeviceStatus Status { get; set; }

        /// <summary>
        /// Detection timestamp
        /// </summary>
        public DateTime DetectedAt { get; set; }

        /// <summary>
        /// Driver information
        /// </summary>
        public DriverInfo? DriverInfo { get; set; }

        /// <summary>
        /// Device capabilities
        /// </summary>
        public DeviceCapabilities Capabilities { get; set; } = new();

        /// <summary>
        /// Additional properties
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    /// <summary>
    /// Connection details
    /// </summary>
    public class ConnectionDetails
    {
        /// <summary>
        /// Port or address
        /// </summary>
        public string Port { get; set; } = string.Empty;

        /// <summary>
        /// USB vendor ID
        /// </summary>
        public string? VendorId { get; set; }

        /// <summary>
        /// USB product ID
        /// </summary>
        public string? ProductId { get; set; }

        /// <summary>
        /// PCI bus number
        /// </summary>
        public int? PciBus { get; set; }

        /// <summary>
        /// PCI device number
        /// </summary>
        public int? PciDevice { get; set; }

        /// <summary>
        /// Network address
        /// </summary>
        public string? NetworkAddress { get; set; }

        /// <summary>
        /// Network port
        /// </summary>
        public int? NetworkPort { get; set; }

        /// <summary>
        /// Connection speed
        /// </summary>
        public string? Speed { get; set; }
    }

    /// <summary>
    /// Driver information
    /// </summary>
    public class DriverInfo
    {
        /// <summary>
        /// Driver name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Driver version
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Driver date
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Driver provider
        /// </summary>
        public string Provider { get; set; } = string.Empty;

        /// <summary>
        /// Driver file path
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Is driver loaded
        /// </summary>
        public bool IsLoaded { get; set; }
    }

    /// <summary>
    /// Device capabilities
    /// </summary>
    public class DeviceCapabilities
    {
        /// <summary>
        /// Supported operations
        /// </summary>
        public List<string> SupportedOperations { get; set; } = new();

        /// <summary>
        /// Maximum channels
        /// </summary>
        public int? MaxChannels { get; set; }

        /// <summary>
        /// Maximum sampling rate
        /// </summary>
        public double? MaxSamplingRate { get; set; }

        /// <summary>
        /// Resolution in bits
        /// </summary>
        public int? ResolutionBits { get; set; }

        /// <summary>
        /// Input ranges
        /// </summary>
        public List<string> InputRanges { get; set; } = new();

        /// <summary>
        /// Output ranges
        /// </summary>
        public List<string> OutputRanges { get; set; } = new();

        /// <summary>
        /// Additional capabilities
        /// </summary>
        public Dictionary<string, object> Additional { get; set; } = new();
    }

    /// <summary>
    /// Scan error information
    /// </summary>
    public class ScanError
    {
        /// <summary>
        /// Error code
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Connection type where error occurred
        /// </summary>
        public ConnectionType? ConnectionType { get; set; }

        /// <summary>
        /// Error timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Additional context
        /// </summary>
        public Dictionary<string, object> Context { get; set; } = new();
    }

    /// <summary>
    /// Scan statistics
    /// </summary>
    public class ScanStatistics
    {
        /// <summary>
        /// Total devices found
        /// </summary>
        public int TotalDevicesFound { get; set; }

        /// <summary>
        /// Devices by connection type
        /// </summary>
        public Dictionary<ConnectionType, int> DevicesByConnectionType { get; set; } = new();

        /// <summary>
        /// Devices by manufacturer
        /// </summary>
        public Dictionary<string, int> DevicesByManufacturer { get; set; } = new();

        /// <summary>
        /// Devices by status
        /// </summary>
        public Dictionary<DeviceStatus, int> DevicesByStatus { get; set; } = new();

        /// <summary>
        /// Scan time by connection type
        /// </summary>
        public Dictionary<ConnectionType, TimeSpan> ScanTimeByConnectionType { get; set; } = new();
    }

    /// <summary>
    /// Device change event
    /// </summary>
    public class DeviceChangeEvent
    {
        /// <summary>
        /// Event type
        /// </summary>
        public DeviceChangeType ChangeType { get; set; }

        /// <summary>
        /// Device information
        /// </summary>
        public DetectedDevice Device { get; set; } = null!;

        /// <summary>
        /// Event timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Previous device state (for updates)
        /// </summary>
        public DetectedDevice? PreviousState { get; set; }
    }

    /// <summary>
    /// Device change types
    /// </summary>
    public enum DeviceChangeType
    {
        Connected,
        Disconnected,
        StatusChanged,
        ConfigurationChanged,
        DriverUpdated,
        Error
    }

    /// <summary>
    /// Device detection log entry
    /// </summary>
    public class DeviceDetectionLog
    {
        /// <summary>
        /// Log ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Event type
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Device ID
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// Device type
        /// </summary>
        public string DeviceType { get; set; } = string.Empty;

        /// <summary>
        /// Event details
        /// </summary>
        public string Details { get; set; } = string.Empty;

        /// <summary>
        /// Success flag
        /// </summary>
        public bool Success { get; set; }
    }
}