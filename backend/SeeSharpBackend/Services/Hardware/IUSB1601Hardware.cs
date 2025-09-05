using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeeSharpBackend.Services.Hardware
{
    /// <summary>
    /// USB-1601硬件抽象层接口
    /// 提供统一的硬件访问接口，供Web API和MCP服务共同使用
    /// </summary>
    public interface IUSB1601Hardware
    {
        /// <summary>
        /// 获取硬件是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 获取硬件是否为模拟模式
        /// </summary>
        bool IsSimulationMode { get; }

        /// <summary>
        /// 获取设备ID
        /// </summary>
        string DeviceId { get; }

        #region 设备管理

        /// <summary>
        /// 初始化硬件
        /// </summary>
        Task<bool> InitializeAsync(string deviceId = "0");

        /// <summary>
        /// 关闭硬件连接
        /// </summary>
        Task<bool> DisconnectAsync();

        /// <summary>
        /// 发现所有可用设备
        /// </summary>
        Task<List<USB1601DeviceInfo>> DiscoverDevicesAsync();

        /// <summary>
        /// 获取设备信息
        /// </summary>
        Task<USB1601DeviceInfo> GetDeviceInfoAsync();

        #endregion

        #region 模拟输入(AI)

        /// <summary>
        /// 配置模拟输入通道
        /// </summary>
        Task<bool> ConfigureAIChannelsAsync(AIConfiguration config);

        /// <summary>
        /// 单点读取模拟输入
        /// </summary>
        Task<double[]> ReadAISingleAsync(int[] channels);

        /// <summary>
        /// 启动连续采集
        /// </summary>
        Task<bool> StartAIContinuousAsync(int[] channels, double sampleRate, Action<double[]> dataCallback);

        /// <summary>
        /// 停止连续采集
        /// </summary>
        Task<bool> StopAIContinuousAsync();

        /// <summary>
        /// 有限点采集
        /// </summary>
        Task<double[]> ReadAIFiniteAsync(int[] channels, int samplesPerChannel, double sampleRate);

        #endregion

        #region 模拟输出(AO)

        /// <summary>
        /// 配置模拟输出通道
        /// </summary>
        Task<bool> ConfigureAOChannelsAsync(AOConfiguration config);

        /// <summary>
        /// 单点输出
        /// </summary>
        Task<bool> WriteAOSingleAsync(int channel, double value);

        /// <summary>
        /// 多通道输出
        /// </summary>
        Task<bool> WriteAOMultipleAsync(int[] channels, double[] values);

        /// <summary>
        /// 启动连续输出
        /// </summary>
        Task<bool> StartAOContinuousAsync(int[] channels, double[] waveform, double updateRate);

        /// <summary>
        /// 停止连续输出
        /// </summary>
        Task<bool> StopAOContinuousAsync();

        #endregion

        #region 数字输入输出(DIO)

        /// <summary>
        /// 配置数字端口方向
        /// </summary>
        Task<bool> ConfigureDIOPortAsync(int port, DIODirection direction);

        /// <summary>
        /// 读取数字输入
        /// </summary>
        Task<byte> ReadDIPortAsync(int port);

        /// <summary>
        /// 写入数字输出
        /// </summary>
        Task<bool> WriteDOPortAsync(int port, byte value);

        /// <summary>
        /// 读取数字线
        /// </summary>
        Task<bool> ReadDILineAsync(int port, int line);

        /// <summary>
        /// 写入数字线
        /// </summary>
        Task<bool> WriteDOLineAsync(int port, int line, bool value);

        #endregion

        #region 计数器

        /// <summary>
        /// 配置计数器
        /// </summary>
        Task<bool> ConfigureCounterAsync(int counter, CounterMode mode);

        /// <summary>
        /// 读取计数值
        /// </summary>
        Task<uint> ReadCounterAsync(int counter);

        /// <summary>
        /// 重置计数器
        /// </summary>
        Task<bool> ResetCounterAsync(int counter);

        #endregion
    }

    /// <summary>
    /// USB-1601设备信息
    /// </summary>
    public class USB1601DeviceInfo
    {
        public string DeviceId { get; set; } = "";
        public string DeviceName { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public string FirmwareVersion { get; set; } = "";
        public bool IsConnected { get; set; }
        public DeviceCapabilities Capabilities { get; set; } = new();
    }

    /// <summary>
    /// USB1601设备能力描述
    /// </summary>
    public class USB1601DeviceCapabilities
    {
        public int AIChannels { get; set; } = 16;
        public int AOChannels { get; set; } = 2;
        public int DIOPorts { get; set; } = 1;
        public int CounterChannels { get; set; } = 2;
        public double MaxAISampleRate { get; set; } = 200000;
        public double MaxAOUpdateRate { get; set; } = 200000;
        public int AIResolution { get; set; } = 16;
        public int AOResolution { get; set; } = 16;
        public double[] AIRanges { get; set; } = { -10, 10, -5, 5, -2, 2, -1, 1 };
        public double[] AORanges { get; set; } = { -10, 10 };
    }

    /// <summary>
    /// AI配置
    /// </summary>
    public class AIConfiguration
    {
        public int[] Channels { get; set; } = Array.Empty<int>();
        public double Range { get; set; } = 10.0;
        public AITerminalMode TerminalMode { get; set; } = AITerminalMode.Differential;
        public bool EnableIEPE { get; set; } = false;
    }

    /// <summary>
    /// AO配置
    /// </summary>
    public class AOConfiguration
    {
        public int[] Channels { get; set; } = Array.Empty<int>();
        public double Range { get; set; } = 10.0;
    }

    /// <summary>
    /// AI端子模式
    /// </summary>
    public enum AITerminalMode
    {
        Differential,
        SingleEnded,
        RSE,
        NRSE
    }

    /// <summary>
    /// 数字端口方向
    /// </summary>
    public enum DIODirection
    {
        Input,
        Output
    }

    /// <summary>
    /// 计数器模式
    /// </summary>
    public enum CounterMode
    {
        EdgeCounting,
        PulseWidth,
        Frequency,
        Period
    }
}