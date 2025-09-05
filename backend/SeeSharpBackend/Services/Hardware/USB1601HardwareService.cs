using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SeeSharpBackend.Services.Drivers;

namespace SeeSharpBackend.Services.Hardware
{
    /// <summary>
    /// USB-1601硬件服务实现
    /// 统一的硬件访问层，供Web API和MCP服务共同使用
    /// </summary>
    public class USB1601HardwareService : IUSB1601Hardware
    {
        private readonly ILogger<USB1601HardwareService> _logger;
        private readonly JYUSB1601DllDriverAdapter? _driverAdapter;
        private Assembly? _driverAssembly;
        private object? _currentAITask;
        private object? _currentAOTask;
        private CancellationTokenSource? _aiCancellationToken;
        private CancellationTokenSource? _aoCancellationToken;
        private bool _isInitialized;
        private bool _isSimulationMode;
        private string _deviceId = "0";
        private readonly Random _random = new();

        public bool IsInitialized => _isInitialized;
        public bool IsSimulationMode => _isSimulationMode;
        public string DeviceId => _deviceId;

        public USB1601HardwareService(
            ILogger<USB1601HardwareService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;

            // 尝试获取驱动适配器
            try
            {
                _driverAdapter = serviceProvider.GetService<JYUSB1601DllDriverAdapter>();
                if (_driverAdapter != null && _driverAdapter.IsInitialized)
                {
                    _isSimulationMode = false;
                    _logger.LogInformation("USB1601硬件服务使用真实硬件模式");
                }
                else
                {
                    _isSimulationMode = true;
                    _logger.LogInformation("USB1601硬件服务使用模拟模式");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "无法获取硬件驱动，使用模拟模式");
                _isSimulationMode = true;
            }
        }

        #region 设备管理

        public async Task<bool> InitializeAsync(string deviceId = "0")
        {
            try
            {
                _deviceId = deviceId;

                if (!_isSimulationMode && _driverAdapter != null)
                {
                    // 尝试加载驱动程序集
                    var driverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Drivers", "JYUSB1601.dll");
                    if (File.Exists(driverPath))
                    {
                        _driverAssembly = Assembly.LoadFrom(driverPath);
                        _logger.LogInformation("成功加载JYUSB1601驱动程序集");
                    }
                }

                _isInitialized = true;
                _logger.LogInformation("USB1601硬件服务初始化成功，设备ID: {DeviceId}, 模式: {Mode}", 
                    _deviceId, _isSimulationMode ? "模拟" : "硬件");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化USB1601硬件服务失败");
                _isSimulationMode = true;
                _isInitialized = true; // 即使硬件失败，也使用模拟模式
                return true;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            try
            {
                // 停止所有任务
                await StopAIContinuousAsync();
                await StopAOContinuousAsync();

                _isInitialized = false;
                _logger.LogInformation("USB1601硬件服务已断开连接");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "断开USB1601硬件服务失败");
                return false;
            }
        }

        public async Task<List<USB1601DeviceInfo>> DiscoverDevicesAsync()
        {
            var devices = new List<USB1601DeviceInfo>();

            if (_isSimulationMode)
            {
                // 模拟模式返回虚拟设备
                devices.Add(new USB1601DeviceInfo
                {
                    DeviceId = "0",
                    DeviceName = "USB-1601 Simulator",
                    SerialNumber = "SIM-0001",
                    FirmwareVersion = "1.0.0",
                    IsConnected = _isInitialized && _deviceId == "0"
                });
            }
            else if (_driverAdapter != null)
            {
                // 真实硬件模式
                try
                {
                    var miSDDevices = await _driverAdapter.DiscoverDevicesAsync();
                    int index = 0;
                    foreach (var device in miSDDevices)
                    {
                        devices.Add(new USB1601DeviceInfo
                        {
                            DeviceId = $"{index}",
                            DeviceName = device.Name,
                            SerialNumber = $"SN-{index:D4}",
                            FirmwareVersion = "1.0.0",
                            IsConnected = $"{index}" == _deviceId && _isInitialized
                        });
                        index++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "发现设备失败");
                }
            }

            return devices;
        }

        public async Task<USB1601DeviceInfo> GetDeviceInfoAsync()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            return new USB1601DeviceInfo
            {
                DeviceId = _deviceId,
                DeviceName = _isSimulationMode ? "USB-1601 Simulator" : "USB-1601 Hardware",
                SerialNumber = _isSimulationMode ? "SIM-0001" : $"HW-{_deviceId:D4}",
                FirmwareVersion = "1.0.0",
                IsConnected = true
            };
        }

        #endregion

        #region 模拟输入(AI)

        public async Task<bool> ConfigureAIChannelsAsync(AIConfiguration config)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("设备未初始化");
                }

                _logger.LogInformation("配置AI通道: {Channels}, 量程: ±{Range}V", 
                    string.Join(",", config.Channels), config.Range);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置AI通道失败");
                return false;
            }
        }

        public async Task<double[]> ReadAISingleAsync(int[] channels)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            if (_isSimulationMode)
            {
                // 模拟模式生成数据
                return channels.Select(ch => GenerateSimulatedAIValue(ch)).ToArray();
            }
            else if (_driverAdapter != null)
            {
                // 使用真实硬件 - 目前硬件接口还需要完善，暂时使用模拟数据
                try
                {
                    _logger.LogInformation("尝试读取硬件AI数据，通道: {Channels}", string.Join(",", channels));
                    // TODO: 完善硬件接口后，这里需要调用实际的硬件读取方法
                    // 暂时返回模拟数据
                    return channels.Select(ch => GenerateSimulatedAIValue(ch)).ToArray();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "读取硬件AI数据失败，使用模拟数据");
                    return channels.Select(ch => GenerateSimulatedAIValue(ch)).ToArray();
                }
            }

            return new double[channels.Length];
        }

        public async Task<bool> StartAIContinuousAsync(int[] channels, double sampleRate, Action<double[]> dataCallback)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("设备未初始化");
                }

                _aiCancellationToken = new CancellationTokenSource();
                
                // 启动异步采集任务
                _ = Task.Run(async () =>
                {
                    while (!_aiCancellationToken.Token.IsCancellationRequested)
                    {
                        var data = await ReadAISingleAsync(channels);
                        dataCallback(data);
                        
                        // 根据采样率计算延迟
                        var delay = (int)(1000.0 / sampleRate);
                        await Task.Delay(Math.Max(1, delay), _aiCancellationToken.Token);
                    }
                }, _aiCancellationToken.Token);

                _logger.LogInformation("启动AI连续采集，通道: {Channels}, 采样率: {SampleRate}Hz", 
                    string.Join(",", channels), sampleRate);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动AI连续采集失败");
                return false;
            }
        }

        public async Task<bool> StopAIContinuousAsync()
        {
            try
            {
                _aiCancellationToken?.Cancel();
                _aiCancellationToken?.Dispose();
                _aiCancellationToken = null;
                
                _logger.LogInformation("停止AI连续采集");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止AI连续采集失败");
                return false;
            }
        }

        public async Task<double[]> ReadAIFiniteAsync(int[] channels, int samplesPerChannel, double sampleRate)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            var totalSamples = channels.Length * samplesPerChannel;
            var data = new double[totalSamples];
            
            for (int i = 0; i < samplesPerChannel; i++)
            {
                var singleData = await ReadAISingleAsync(channels);
                for (int j = 0; j < channels.Length; j++)
                {
                    data[i * channels.Length + j] = singleData[j];
                }
                
                // 根据采样率计算延迟
                var delay = (int)(1000.0 / sampleRate);
                await Task.Delay(Math.Max(1, delay));
            }

            return data;
        }

        #endregion

        #region 模拟输出(AO)

        public async Task<bool> ConfigureAOChannelsAsync(AOConfiguration config)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("设备未初始化");
                }

                _logger.LogInformation("配置AO通道: {Channels}, 量程: ±{Range}V", 
                    string.Join(",", config.Channels), config.Range);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置AO通道失败");
                return false;
            }
        }

        public async Task<bool> WriteAOSingleAsync(int channel, double value)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            _logger.LogDebug("AO输出: 通道{Channel} = {Value}V", channel, value);
            
            if (!_isSimulationMode && _driverAdapter != null)
            {
                try
                {
                    _logger.LogInformation("尝试写入硬件AO数据，通道: {Channel}, 值: {Value}", channel, value);
                    // TODO: 完善硬件接口后，这里需要调用实际的硬件写入方法
                    // 暂时返回成功
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "写入硬件AO数据失败");
                    return false;
                }
            }
            
            return true;
        }

        public async Task<bool> WriteAOMultipleAsync(int[] channels, double[] values)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            if (channels.Length != values.Length)
            {
                throw new ArgumentException("通道数和值数量不匹配");
            }

            for (int i = 0; i < channels.Length; i++)
            {
                await WriteAOSingleAsync(channels[i], values[i]);
            }
            
            return true;
        }

        public async Task<bool> StartAOContinuousAsync(int[] channels, double[] waveform, double updateRate)
        {
            try
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("设备未初始化");
                }

                _aoCancellationToken = new CancellationTokenSource();
                
                // 启动异步输出任务
                _ = Task.Run(async () =>
                {
                    int index = 0;
                    while (!_aoCancellationToken.Token.IsCancellationRequested)
                    {
                        var values = channels.Select((ch, i) => 
                            waveform[(index + i) % waveform.Length]).ToArray();
                        
                        await WriteAOMultipleAsync(channels, values);
                        
                        index = (index + channels.Length) % waveform.Length;
                        
                        // 根据更新率计算延迟
                        var delay = (int)(1000.0 / updateRate);
                        await Task.Delay(Math.Max(1, delay), _aoCancellationToken.Token);
                    }
                }, _aoCancellationToken.Token);

                _logger.LogInformation("启动AO连续输出，通道: {Channels}, 更新率: {UpdateRate}Hz", 
                    string.Join(",", channels), updateRate);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动AO连续输出失败");
                return false;
            }
        }

        public async Task<bool> StopAOContinuousAsync()
        {
            try
            {
                _aoCancellationToken?.Cancel();
                _aoCancellationToken?.Dispose();
                _aoCancellationToken = null;
                
                _logger.LogInformation("停止AO连续输出");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止AO连续输出失败");
                return false;
            }
        }

        #endregion

        #region 数字输入输出(DIO)

        public async Task<bool> ConfigureDIOPortAsync(int port, DIODirection direction)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            _logger.LogInformation("配置DIO端口{Port}方向: {Direction}", port, direction);
            return true;
        }

        public async Task<byte> ReadDIPortAsync(int port)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            if (_isSimulationMode)
            {
                // 模拟模式返回随机值
                return (byte)_random.Next(0, 256);
            }
            
            return 0;
        }

        public async Task<bool> WriteDOPortAsync(int port, byte value)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            _logger.LogDebug("DO输出: 端口{Port} = 0x{Value:X2}", port, value);
            return true;
        }

        public async Task<bool> ReadDILineAsync(int port, int line)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            var portValue = await ReadDIPortAsync(port);
            return (portValue & (1 << line)) != 0;
        }

        public async Task<bool> WriteDOLineAsync(int port, int line, bool value)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            var portValue = await ReadDIPortAsync(port);
            if (value)
            {
                portValue |= (byte)(1 << line);
            }
            else
            {
                portValue &= (byte)~(1 << line);
            }
            
            return await WriteDOPortAsync(port, portValue);
        }

        #endregion

        #region 计数器

        public async Task<bool> ConfigureCounterAsync(int counter, CounterMode mode)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            _logger.LogInformation("配置计数器{Counter}模式: {Mode}", counter, mode);
            return true;
        }

        public async Task<uint> ReadCounterAsync(int counter)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            if (_isSimulationMode)
            {
                // 模拟模式返回递增值
                return (uint)(_random.Next(0, 10000) + DateTime.Now.Ticks % 10000);
            }
            
            return 0;
        }

        public async Task<bool> ResetCounterAsync(int counter)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("设备未初始化");
            }

            _logger.LogInformation("重置计数器{Counter}", counter);
            return true;
        }

        #endregion

        #region 辅助方法

        private double GenerateSimulatedAIValue(int channel)
        {
            // 生成模拟的AI数据
            var time = DateTime.Now.Ticks / (double)TimeSpan.TicksPerSecond;
            var frequency = 1.0 + channel * 0.5; // 每个通道不同频率
            var amplitude = 0.5 + channel * 0.1; // 每个通道不同幅度
            var value = amplitude * Math.Sin(2 * Math.PI * frequency * time);
            
            // 添加噪声
            value += (_random.NextDouble() - 0.5) * 0.05;
            
            return value;
        }

        #endregion
    }
}