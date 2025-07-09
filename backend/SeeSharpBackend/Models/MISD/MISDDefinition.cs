using System.ComponentModel.DataAnnotations;

namespace SeeSharpBackend.Models.MISD
{
    /// <summary>
    /// MISD (Modular Instrumentation Software Dictionary) 定义
    /// 基于简仪科技MISD标准的硬件接口定义
    /// </summary>
    public class MISDDefinition
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// 功能名称
        /// </summary>
        [Required]
        public string FunctionName { get; set; } = string.Empty;
        
        /// <summary>
        /// 功能类型 (AI, AO, DI, DO, CI, CO)
        /// </summary>
        [Required]
        public string FunctionType { get; set; } = string.Empty;
        
        /// <summary>
        /// 功能描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 功能分类 (AI, AO, DI, DO, CI, CO, Device)
        /// </summary>
        [Required]
        public string Class { get; set; } = string.Empty;
        
        /// <summary>
        /// 功能组
        /// </summary>
        [Required]
        public string FunctionGroup { get; set; } = string.Empty;
        
        /// <summary>
        /// 公共符号
        /// </summary>
        [Required]
        public string PublicSymbol { get; set; } = string.Empty;
        
        /// <summary>
        /// 公共类、方法、属性等
        /// </summary>
        [Required]
        public string PublicClassMethodProperty { get; set; } = string.Empty;
        
        /// <summary>
        /// 类型 (Constructor, Method, Property)
        /// </summary>
        [Required]
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// 中文注释
        /// </summary>
        public string ChineseComment { get; set; } = string.Empty;
        
        /// <summary>
        /// 英文注释
        /// </summary>
        public string EnglishNotation { get; set; } = string.Empty;
        
        /// <summary>
        /// 支持的硬件型号列表
        /// </summary>
        public List<string> SupportedHardware { get; set; } = new List<string>();
        
        /// <summary>
        /// 参数列表
        /// </summary>
        public List<MISDParameter> Parameters { get; set; } = new List<MISDParameter>();
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 硬件设备信息
    /// </summary>
    public class HardwareDevice
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// 设备型号
        /// </summary>
        [Required]
        public string Model { get; set; } = string.Empty;
        
        /// <summary>
        /// 设备名称
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 设备类型 (PXI, USB, PCIe等)
        /// </summary>
        [Required]
        public string DeviceType { get; set; } = string.Empty;
        
        /// <summary>
        /// 槽位号
        /// </summary>
        public int? SlotNumber { get; set; }
        
        /// <summary>
        /// 设备状态
        /// </summary>
        public DeviceStatus Status { get; set; } = DeviceStatus.Unknown;
        
        /// <summary>
        /// 支持的功能列表
        /// </summary>
        public List<string> SupportedFunctions { get; set; } = new List<string>();
        
        /// <summary>
        /// 设备配置参数
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// 最后检测时间
        /// </summary>
        public DateTime LastDetected { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 设备状态枚举
    /// </summary>
    public enum DeviceStatus
    {
        Unknown = 0,
        Online = 1,
        Offline = 2,
        Error = 3,
        Busy = 4,
        Ready = 5
    }
    
    /// <summary>
    /// MISD任务配置
    /// </summary>
    public class MISDTaskConfiguration
    {
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// 任务名称
        /// </summary>
        [Required]
        public string TaskName { get; set; } = string.Empty;
        
        /// <summary>
        /// 设备ID
        /// </summary>
        [Required]
        public int DeviceId { get; set; }
        
        /// <summary>
        /// 任务类型 (AI, AO, DI, DO, CI, CO)
        /// </summary>
        [Required]
        public string TaskType { get; set; } = string.Empty;
        
        /// <summary>
        /// 通道配置
        /// </summary>
        public List<ChannelConfiguration> Channels { get; set; } = new List<ChannelConfiguration>();
        
        /// <summary>
        /// 采样配置
        /// </summary>
        public SamplingConfiguration? Sampling { get; set; }
        
        /// <summary>
        /// 触发配置
        /// </summary>
        public TriggerConfiguration? Trigger { get; set; }
        
        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus Status { get; set; } = TaskStatus.Created;
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 通道配置
    /// </summary>
    public class ChannelConfiguration
    {
        /// <summary>
        /// 通道ID
        /// </summary>
        public int ChannelId { get; set; }
        
        /// <summary>
        /// 量程下限
        /// </summary>
        public double RangeLow { get; set; }
        
        /// <summary>
        /// 量程上限
        /// </summary>
        public double RangeHigh { get; set; }
        
        /// <summary>
        /// 端口配置 (RSE, NRSE, Differential等)
        /// </summary>
        public string Terminal { get; set; } = "RSE";
        
        /// <summary>
        /// 耦合方式 (AC, DC)
        /// </summary>
        public string Coupling { get; set; } = "DC";
        
        /// <summary>
        /// IEPE激励使能
        /// </summary>
        public bool EnableIEPE { get; set; } = false;
    }
    
    /// <summary>
    /// 采样配置
    /// </summary>
    public class SamplingConfiguration
    {
        /// <summary>
        /// 采样率
        /// </summary>
        public double SampleRate { get; set; }
        
        /// <summary>
        /// 采样点数
        /// </summary>
        public int SamplesToAcquire { get; set; }
        
        /// <summary>
        /// 采样模式 (Single, Finite, Continuous)
        /// </summary>
        public string Mode { get; set; } = "Finite";
        
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public int BufferSize { get; set; } = 1000;
    }
    
    /// <summary>
    /// 触发配置
    /// </summary>
    public class TriggerConfiguration
    {
        /// <summary>
        /// 触发类型 (Immediate, Digital, Analog, Software)
        /// </summary>
        public string Type { get; set; } = "Immediate";
        
        /// <summary>
        /// 触发源
        /// </summary>
        public string Source { get; set; } = string.Empty;
        
        /// <summary>
        /// 触发边沿 (Rising, Falling)
        /// </summary>
        public string Edge { get; set; } = "Rising";
        
        /// <summary>
        /// 触发电平
        /// </summary>
        public double Level { get; set; } = 0.0;
        
        /// <summary>
        /// 预触发点数
        /// </summary>
        public int PreTriggerSamples { get; set; } = 0;
    }
    
    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum TaskStatus
    {
        Created = 0,
        Configured = 1,
        Started = 2,
        Running = 3,
        Stopped = 4,
        Error = 5,
        Completed = 6
    }
    
    /// <summary>
    /// MISD参数定义
    /// </summary>
    public class MISDParameter
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 参数类型
        /// </summary>
        [Required]
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// 参数描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否必需
        /// </summary>
        public bool IsRequired { get; set; } = false;
        
        /// <summary>
        /// 默认值
        /// </summary>
        public object? DefaultValue { get; set; }
        
        /// <summary>
        /// 参数约束
        /// </summary>
        public Dictionary<string, object> Constraints { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// 驱动配置
    /// </summary>
    public class DriverConfiguration
    {
        /// <summary>
        /// 驱动名称
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 驱动类型
        /// </summary>
        [Required]
        public DriverType Type { get; set; }
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// 驱动文件路径
        /// </summary>
        [Required]
        public string DriverPath { get; set; } = string.Empty;
        
        /// <summary>
        /// 设备型号
        /// </summary>
        [Required]
        public string DeviceModel { get; set; } = string.Empty;
        
        /// <summary>
        /// 驱动描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 支持的设备列表
        /// </summary>
        public List<string> SupportedDevices { get; set; } = new List<string>();
        
        /// <summary>
        /// 驱动能力
        /// </summary>
        public List<string> Capabilities { get; set; } = new List<string>();
        
        /// <summary>
        /// 驱动参数
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// 驱动类型枚举
    /// </summary>
    public enum DriverType
    {
        CSharpDll = 0,
        PythonModule = 1,
        CppLibrary = 2,
        WebAPI = 3,
        Mock = 4
    }
}
