using SeeSharpBackend.Models.MISD;

namespace SeeSharpBackend.Services.MISD
{
    /// <summary>
    /// MISD服务接口
    /// 提供标准化的硬件抽象层接口
    /// </summary>
    public interface IMISDService
    {
        /// <summary>
        /// 初始化MISD服务
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// 发现并枚举所有可用设备
        /// </summary>
        Task<List<HardwareDevice>> DiscoverDevicesAsync();
        
        /// <summary>
        /// 获取指定设备的详细信息
        /// </summary>
        Task<HardwareDevice?> GetDeviceAsync(int deviceId);
        
        /// <summary>
        /// 获取设备支持的MISD功能列表
        /// </summary>
        Task<List<MISDDefinition>> GetDeviceFunctionsAsync(int deviceId);
        
        /// <summary>
        /// 创建任务配置
        /// </summary>
        Task<MISDTaskConfiguration> CreateTaskAsync(MISDTaskConfiguration taskConfig);
        
        /// <summary>
        /// 启动任务
        /// </summary>
        Task<bool> StartTaskAsync(int taskId);
        
        /// <summary>
        /// 停止任务
        /// </summary>
        Task<bool> StopTaskAsync(int taskId);
        
        /// <summary>
        /// 获取任务状态
        /// </summary>
        Task<Models.MISD.TaskStatus> GetTaskStatusAsync(int taskId);
        
        /// <summary>
        /// 读取数据
        /// </summary>
        Task<double[,]> ReadDataAsync(int taskId, int samplesPerChannel, int timeout = 10000);
        
        /// <summary>
        /// 写入数据
        /// </summary>
        Task<bool> WriteDataAsync(int taskId, double[,] data, int timeout = 10000);
        
        /// <summary>
        /// 获取可用样本数
        /// </summary>
        Task<int> GetAvailableSamplesAsync(int taskId);
        
        /// <summary>
        /// 发送软件触发
        /// </summary>
        Task<bool> SendSoftwareTriggerAsync(int taskId);
        
        /// <summary>
        /// 等待任务完成
        /// </summary>
        Task<bool> WaitUntilDoneAsync(int taskId, int timeout = -1);
        
        /// <summary>
        /// 释放任务资源
        /// </summary>
        Task<bool> DisposeTaskAsync(int taskId);
    }
    
    /// <summary>
    /// 设备发现服务接口
    /// </summary>
    public interface IDeviceDiscoveryService
    {
        /// <summary>
        /// 扫描PXI设备
        /// </summary>
        Task<List<HardwareDevice>> ScanPXIDevicesAsync();
        
        /// <summary>
        /// 扫描USB设备
        /// </summary>
        Task<List<HardwareDevice>> ScanUSBDevicesAsync();
        
        /// <summary>
        /// 扫描PCIe设备
        /// </summary>
        Task<List<HardwareDevice>> ScanPCIeDevicesAsync();
        
        /// <summary>
        /// 验证设备连接状态
        /// </summary>
        Task<DeviceStatus> VerifyDeviceStatusAsync(HardwareDevice device);
        
        /// <summary>
        /// 获取设备驱动信息
        /// </summary>
        Task<Dictionary<string, object>> GetDeviceDriverInfoAsync(HardwareDevice device);
    }
    
    /// <summary>
    /// 硬件抽象层接口
    /// </summary>
    public interface IHardwareAbstractionLayer
    {
        /// <summary>
        /// 加载设备驱动
        /// </summary>
        Task<bool> LoadDriverAsync(string driverPath, string deviceModel);
        
        /// <summary>
        /// 卸载设备驱动
        /// </summary>
        Task<bool> UnloadDriverAsync(string deviceModel);
        
        /// <summary>
        /// 执行MISD方法调用
        /// </summary>
        Task<object?> ExecuteMISDMethodAsync(string deviceModel, string className, string methodName, object[] parameters);
        
        /// <summary>
        /// 获取MISD属性值
        /// </summary>
        Task<object?> GetMISDPropertyAsync(string deviceModel, string className, string propertyName);
        
        /// <summary>
        /// 设置MISD属性值
        /// </summary>
        Task<bool> SetMISDPropertyAsync(string deviceModel, string className, string propertyName, object value);
        
        /// <summary>
        /// 创建MISD对象实例
        /// </summary>
        Task<object?> CreateMISDInstanceAsync(string deviceModel, string className, object[] constructorParams);
        
        /// <summary>
        /// 释放MISD对象实例
        /// </summary>
        Task<bool> DisposeMISDInstanceAsync(object instance);
    }
    
    /// <summary>
    /// 数据采集服务接口
    /// </summary>
    public interface IDataAcquisitionService
    {
        /// <summary>
        /// 开始连续数据采集
        /// </summary>
        Task<bool> StartContinuousAcquisitionAsync(int taskId);
        
        /// <summary>
        /// 停止连续数据采集
        /// </summary>
        Task<bool> StopContinuousAcquisitionAsync(int taskId);
        
        /// <summary>
        /// 获取实时数据流
        /// </summary>
        IAsyncEnumerable<double[,]> GetRealTimeDataStreamAsync(int taskId);
        
        /// <summary>
        /// 配置数据缓冲区
        /// </summary>
        Task<bool> ConfigureBufferAsync(int taskId, int bufferSize);
        
        /// <summary>
        /// 获取缓冲区状态
        /// </summary>
        Task<BufferStatus> GetBufferStatusAsync(int taskId);
        
        /// <summary>
        /// 清空缓冲区
        /// </summary>
        Task<bool> ClearBufferAsync(int taskId);
    }
    
    /// <summary>
    /// 缓冲区状态
    /// </summary>
    public class BufferStatus
    {
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public int BufferSize { get; set; }
        
        /// <summary>
        /// 可用样本数
        /// </summary>
        public int AvailableSamples { get; set; }
        
        /// <summary>
        /// 已传输样本数
        /// </summary>
        public int TransferredSamples { get; set; }
        
        /// <summary>
        /// 缓冲区使用率
        /// </summary>
        public double UsagePercentage => BufferSize > 0 ? (double)AvailableSamples / BufferSize * 100 : 0;
        
        /// <summary>
        /// 是否溢出
        /// </summary>
        public bool IsOverflow { get; set; }
    }
}
