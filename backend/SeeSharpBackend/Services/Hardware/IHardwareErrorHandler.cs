using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeeSharpBackend.Services.Hardware
{
    /// <summary>
    /// Hardware error handling interface
    /// </summary>
    public interface IHardwareErrorHandler
    {
        /// <summary>
        /// Handle hardware connection error
        /// </summary>
        Task<HardwareErrorResult> HandleConnectionErrorAsync(HardwareConnectionError error);

        /// <summary>
        /// Handle device initialization error
        /// </summary>
        Task<HardwareErrorResult> HandleInitializationErrorAsync(HardwareInitializationError error);

        /// <summary>
        /// Handle communication error
        /// </summary>
        Task<HardwareErrorResult> HandleCommunicationErrorAsync(HardwareCommunicationError error);

        /// <summary>
        /// Try to recover from error
        /// </summary>
        Task<bool> TryRecoverAsync(HardwareError error);

        /// <summary>
        /// Get error history for a device
        /// </summary>
        Task<List<HardwareErrorLog>> GetErrorHistoryAsync(string deviceId, int maxCount = 100);

        /// <summary>
        /// Get error statistics
        /// </summary>
        Task<HardwareErrorStatistics> GetErrorStatisticsAsync(DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Base hardware error class
    /// </summary>
    public abstract class HardwareError
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public Dictionary<string, object> Context { get; set; } = new();
        public Exception? InnerException { get; set; }
        public int RetryCount { get; set; }
        public int TimeoutMs { get; set; } = 10000;
    }

    /// <summary>
    /// Hardware connection error
    /// </summary>
    public class HardwareConnectionError : HardwareError
    {
        public string ConnectionType { get; set; } = string.Empty; // USB, PCI, Network, etc.
        public string PortInfo { get; set; } = string.Empty;
    }

    /// <summary>
    /// Hardware initialization error
    /// </summary>
    public class HardwareInitializationError : HardwareError
    {
        public string DriverVersion { get; set; } = string.Empty;
        public string FirmwareVersion { get; set; } = string.Empty;
        public string InitializationStep { get; set; } = string.Empty;
        public Dictionary<string, object> InitParameters { get; set; } = new();
    }

    /// <summary>
    /// Hardware communication error
    /// </summary>
    public class HardwareCommunicationError : HardwareError
    {
        public string CommandName { get; set; } = string.Empty;
        public byte[]? CommandData { get; set; }
        public byte[]? ResponseData { get; set; }
        public int ExpectedResponseLength { get; set; }
        public int ActualResponseLength { get; set; }
        public CommunicationErrorType ErrorType { get; set; }
    }

    /// <summary>
    /// Communication error types
    /// </summary>
    public enum CommunicationErrorType
    {
        Timeout,
        InvalidResponse,
        ChecksumError,
        BufferOverflow,
        UnexpectedDisconnection,
        ProtocolError,
        Unknown
    }

    /// <summary>
    /// Hardware error result
    /// </summary>
    public class HardwareErrorResult
    {
        public bool Recovered { get; set; }
        public string RecoveryMethod { get; set; } = string.Empty;
        public string SuggestedAction { get; set; } = string.Empty;
        public Dictionary<string, object> RecoveryContext { get; set; } = new();
        public List<string> LogMessages { get; set; } = new();
    }

    /// <summary>
    /// Hardware error log entry
    /// </summary>
    public class HardwareErrorLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorType { get; set; } = string.Empty;
        public bool Recovered { get; set; }
        public string RecoveryMethod { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty; // JSON
    }

    /// <summary>
    /// Hardware error statistics
    /// </summary>
    public class HardwareErrorStatistics
    {
        public int TotalErrors { get; set; }
        public int RecoveredErrors { get; set; }
        public int UnrecoveredErrors { get; set; }
        public Dictionary<string, int> ErrorsByType { get; set; } = new();
        public Dictionary<string, int> ErrorsByDevice { get; set; } = new();
        public Dictionary<string, int> ErrorsByCode { get; set; } = new();
        public List<HardwareErrorTrend> DailyTrends { get; set; } = new();
    }

    /// <summary>
    /// Hardware error trend data
    /// </summary>
    public class HardwareErrorTrend
    {
        public DateTime Date { get; set; }
        public int ErrorCount { get; set; }
        public int RecoveryCount { get; set; }
        public double RecoveryRate { get; set; }
    }
}