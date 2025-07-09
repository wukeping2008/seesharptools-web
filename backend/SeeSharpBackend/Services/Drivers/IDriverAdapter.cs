using SeeSharpBackend.Models.MISD;

namespace SeeSharpBackend.Services.Drivers
{
    /// <summary>
    /// 通用驱动适配器接口
    /// 支持C# DLL、Python、C++等多种驱动类型
    /// </summary>
    public interface IDriverAdapter
    {
        /// <summary>
        /// 驱动类型
        /// </summary>
        DriverType Type { get; }
        
        /// <summary>
        /// 驱动名称
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 驱动版本
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 初始化驱动
        /// </summary>
        Task<bool> InitializeAsync(DriverConfiguration config);
        
        /// <summary>
        /// 卸载驱动
        /// </summary>
        Task<bool> UnloadAsync();
        
        /// <summary>
        /// 发现设备
        /// </summary>
        Task<List<HardwareDevice>> DiscoverDevicesAsync();
        
        /// <summary>
        /// 创建任务实例
        /// </summary>
        Task<object> CreateTaskAsync(string taskType, int deviceId, Dictionary<string, object> parameters);
        
        /// <summary>
        /// 执行方法调用
        /// </summary>
        Task<object?> ExecuteMethodAsync(object taskInstance, string methodName, object[] parameters);
        
        /// <summary>
        /// 获取属性值
        /// </summary>
        Task<object?> GetPropertyAsync(object taskInstance, string propertyName);
        
        /// <summary>
        /// 设置属性值
        /// </summary>
        Task<bool> SetPropertyAsync(object taskInstance, string propertyName, object value);
        
        /// <summary>
        /// 释放任务实例
        /// </summary>
        Task<bool> DisposeTaskAsync(object taskInstance);
    }
    
    /// <summary>
    /// 驱动类型枚举
    /// </summary>
    public enum DriverType
    {
        /// <summary>
        /// C# DLL驱动
        /// </summary>
        CSharpDll,
        
        /// <summary>
        /// Python驱动
        /// </summary>
        Python,
        
        /// <summary>
        /// C++ DLL驱动
        /// </summary>
        CppDll,
        
        /// <summary>
        /// COM组件驱动
        /// </summary>
        COM,
        
        /// <summary>
        /// 网络驱动
        /// </summary>
        Network
    }
    
    /// <summary>
    /// 驱动配置
    /// </summary>
    public class DriverConfiguration
    {
        /// <summary>
        /// 驱动文件路径
        /// </summary>
        public string DriverPath { get; set; } = string.Empty;
        
        /// <summary>
        /// 设备型号
        /// </summary>
        public string DeviceModel { get; set; } = string.Empty;
        
        /// <summary>
        /// 配置参数
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();
        
        /// <summary>
        /// 超时设置(毫秒)
        /// </summary>
        public int TimeoutMs { get; set; } = 10000;
        
        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        public bool DebugMode { get; set; } = false;
    }
}
