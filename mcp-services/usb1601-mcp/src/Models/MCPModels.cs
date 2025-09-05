using Newtonsoft.Json;

namespace USB1601MCP.Models
{
    /// <summary>
    /// MCP请求模型
    /// </summary>
    public class MCPRequest
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";
        
        [JsonProperty("id")]
        public string? Id { get; set; }
        
        [JsonProperty("method")]
        public string Method { get; set; } = string.Empty;
        
        [JsonProperty("params")]
        public dynamic? Params { get; set; }
    }

    /// <summary>
    /// MCP响应模型
    /// </summary>
    public class MCPResponse
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";
        
        [JsonProperty("id")]
        public string? Id { get; set; }
        
        [JsonProperty("result")]
        public object? Result { get; set; }
        
        [JsonProperty("error")]
        public MCPError? Error { get; set; }
    }

    /// <summary>
    /// MCP错误模型
    /// </summary>
    public class MCPError
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
        
        [JsonProperty("data")]
        public object? Data { get; set; }
    }

    /// <summary>
    /// 设备信息模型
    /// </summary>
    public class DeviceInfo
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public bool IsConnected { get; set; }
        public DeviceCapabilities Capabilities { get; set; } = new();
    }

    /// <summary>
    /// 设备能力模型
    /// </summary>
    public class DeviceCapabilities
    {
        public int AnalogInputChannels { get; set; } = 16;
        public int AnalogOutputChannels { get; set; } = 2;
        public int DigitalIOPorts { get; set; } = 2;
        public int CounterChannels { get; set; } = 2;
        public double MaxSampleRate { get; set; } = 200000;
        public double MinVoltage { get; set; } = -10;
        public double MaxVoltage { get; set; } = 10;
        public int Resolution { get; set; } = 12;
    }

    /// <summary>
    /// 采集数据模型
    /// </summary>
    public class AcquisitionData
    {
        public string DeviceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int[] Channels { get; set; } = Array.Empty<int>();
        public double[][] Data { get; set; } = Array.Empty<double[]>();
        public double SampleRate { get; set; }
        public int SampleCount { get; set; }
    }

    /// <summary>
    /// 流数据模型
    /// </summary>
    public class StreamData
    {
        public string StreamId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public StreamStatus Status { get; set; }
        public int[] Channels { get; set; } = Array.Empty<int>();
        public double SampleRate { get; set; }
        public long TotalSamples { get; set; }
        public DateTime StartTime { get; set; }
    }

    /// <summary>
    /// 流状态枚举
    /// </summary>
    public enum StreamStatus
    {
        Idle,
        Starting,
        Running,
        Stopping,
        Stopped,
        Error
    }
}