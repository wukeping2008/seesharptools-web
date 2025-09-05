using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace USB1601Service.Services
{
    /// <summary>
    /// USB-1601硬件管理服务
    /// </summary>
    public class USB1601Manager
    {
        private readonly ILogger<USB1601Manager> _logger;
        private Assembly? _driverAssembly;
        private object? _aiTask;
        private object? _aoTask;
        private bool _isInitialized = false;
        private bool _isAcquiring = false;
        private readonly int _deviceIndex = 0;
        private Timer? _dataTimer;
        private readonly List<double[]> _dataBuffer = new();

        public event EventHandler<DataReceivedEventArgs>? DataReceived;

        public USB1601Manager(ILogger<USB1601Manager> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 初始化硬件
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("初始化USB-1601硬件...");

                // 加载驱动DLL
                var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\drivers\\JYUSB1601.dll");
                if (!File.Exists(dllPath))
                {
                    dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JYUSB1601.dll");
                }

                _driverAssembly = Assembly.LoadFrom(dllPath);
                _logger.LogInformation($"成功加载驱动: {dllPath}");

                // 创建AI任务
                var aiTaskType = _driverAssembly.GetType("JYUSB1601.JYUSB1601AITask");
                if (aiTaskType != null)
                {
                    _aiTask = Activator.CreateInstance(aiTaskType, _deviceIndex.ToString());
                    _logger.LogInformation("AI任务创建成功");
                }

                // 创建AO任务（用于自发自收测试）
                var aoTaskType = _driverAssembly.GetType("JYUSB1601.JYUSB1601AOTask");
                if (aoTaskType != null)
                {
                    _aoTask = Activator.CreateInstance(aoTaskType, _deviceIndex.ToString());
                    _logger.LogInformation("AO任务创建成功");
                }

                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化USB-1601失败");
                return false;
            }
        }

        /// <summary>
        /// 配置采集参数
        /// </summary>
        public async Task<bool> ConfigureAcquisitionAsync(AcquisitionConfig config)
        {
            try
            {
                if (_aiTask == null) return false;

                var aiTaskType = _aiTask.GetType();

                // 清除现有通道
                var clearChannelsMethod = aiTaskType.GetMethod("RemoveAllChannels");
                clearChannelsMethod?.Invoke(_aiTask, null);

                // 添加通道
                var addChannelMethod = aiTaskType.GetMethod("AddChannel", new[] { typeof(int), typeof(double), typeof(double) });
                for (int i = 0; i < config.ChannelCount; i++)
                {
                    addChannelMethod?.Invoke(_aiTask, new object[] { i, config.MinVoltage, config.MaxVoltage });
                }

                // 设置采样率
                var sampleRateProperty = aiTaskType.GetProperty("SampleRate");
                sampleRateProperty?.SetValue(_aiTask, config.SampleRate);

                // 设置模式为连续采集
                var aiModeType = _driverAssembly?.GetType("JYUSB1601.AIMode");
                var continuousMode = aiModeType?.GetField("Continuous")?.GetValue(null);
                var modeProperty = aiTaskType.GetProperty("Mode");
                modeProperty?.SetValue(_aiTask, continuousMode);

                _logger.LogInformation($"配置成功: {config.ChannelCount}通道, {config.SampleRate}Hz");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "配置采集参数失败");
                return false;
            }
        }

        /// <summary>
        /// 开始采集
        /// </summary>
        public async Task<bool> StartAcquisitionAsync()
        {
            try
            {
                if (_aiTask == null || _isAcquiring) return false;

                var startMethod = _aiTask.GetType().GetMethod("Start");
                startMethod?.Invoke(_aiTask, null);

                _isAcquiring = true;

                // 启动数据读取定时器
                _dataTimer = new Timer(ReadDataCallback, null, 0, 100); // 每100ms读取一次

                _logger.LogInformation("开始数据采集");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动采集失败");
                return false;
            }
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        public async Task<bool> StopAcquisitionAsync()
        {
            try
            {
                _dataTimer?.Dispose();
                _dataTimer = null;

                if (_aiTask != null)
                {
                    var stopMethod = _aiTask.GetType().GetMethod("Stop");
                    stopMethod?.Invoke(_aiTask, null);
                }

                _isAcquiring = false;
                _logger.LogInformation("停止数据采集");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止采集失败");
                return false;
            }
        }

        /// <summary>
        /// 生成测试信号（AO输出）
        /// </summary>
        public async Task<bool> GenerateTestSignalAsync(SignalType signalType, double frequency, double amplitude)
        {
            try
            {
                if (_aoTask == null) return false;

                var aoTaskType = _aoTask.GetType();

                // 清除并添加通道
                var clearMethod = aoTaskType.GetMethod("RemoveAllChannels");
                clearMethod?.Invoke(_aoTask, null);

                var addChannelMethod = aoTaskType.GetMethod("AddChannel", new[] { typeof(int), typeof(double), typeof(double) });
                addChannelMethod?.Invoke(_aoTask, new object[] { 0, -10.0, 10.0 });

                // 生成信号数据
                int sampleCount = 10000;
                double[] signalData = GenerateSignal(signalType, frequency, amplitude, sampleCount, 10000);

                // 写入数据
                var writeMethod = aoTaskType.GetMethod("WriteData", new[] { typeof(double[]), typeof(int) });
                writeMethod?.Invoke(_aoTask, new object[] { signalData, -1 });

                // 启动输出
                var startMethod = aoTaskType.GetMethod("Start");
                startMethod?.Invoke(_aoTask, null);

                _logger.LogInformation($"生成{signalType}信号: {frequency}Hz, {amplitude}V");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成测试信号失败");
                return false;
            }
        }

        private void ReadDataCallback(object? state)
        {
            try
            {
                if (_aiTask == null || !_isAcquiring) return;

                var aiTaskType = _aiTask.GetType();

                // 检查可用样本数
                var availableSamplesProperty = aiTaskType.GetProperty("AvailableSamples");
                var availableSamples = Convert.ToInt32(availableSamplesProperty?.GetValue(_aiTask) ?? 0);

                if (availableSamples > 100)
                {
                    // 读取数据
                    var samplesToRead = Math.Min(availableSamples, 1000);
                    var data = new double[samplesToRead];
                    
                    var readMethod = aiTaskType.GetMethod("ReadData", new[] { typeof(double[]).MakeByRefType(), typeof(int), typeof(int) });
                    var parameters = new object[] { data, samplesToRead, -1 };
                    readMethod?.Invoke(_aiTask, parameters);

                    data = (double[])parameters[0];

                    // 触发数据接收事件
                    DataReceived?.Invoke(this, new DataReceivedEventArgs { Data = data, Timestamp = DateTime.Now });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "读取数据失败");
            }
        }

        private double[] GenerateSignal(SignalType type, double frequency, double amplitude, int sampleCount, double sampleRate)
        {
            var data = new double[sampleCount];
            var omega = 2 * Math.PI * frequency / sampleRate;

            for (int i = 0; i < sampleCount; i++)
            {
                switch (type)
                {
                    case SignalType.Sine:
                        data[i] = amplitude * Math.Sin(omega * i);
                        break;
                    case SignalType.Square:
                        data[i] = amplitude * (Math.Sin(omega * i) >= 0 ? 1 : -1);
                        break;
                    case SignalType.Triangle:
                        var phase = (omega * i) % (2 * Math.PI);
                        data[i] = amplitude * (2 * Math.Abs(2 * (phase / (2 * Math.PI) - Math.Floor(phase / (2 * Math.PI) + 0.5))) - 1);
                        break;
                    case SignalType.Sawtooth:
                        data[i] = amplitude * (2 * ((omega * i / (2 * Math.PI)) - Math.Floor(omega * i / (2 * Math.PI) + 0.5)));
                        break;
                }
            }

            return data;
        }

        public void Dispose()
        {
            StopAcquisitionAsync().Wait();
            
            if (_aiTask != null)
            {
                var disposeMethod = _aiTask.GetType().GetMethod("Dispose");
                disposeMethod?.Invoke(_aiTask, null);
            }

            if (_aoTask != null)
            {
                var disposeMethod = _aoTask.GetType().GetMethod("Dispose");
                disposeMethod?.Invoke(_aoTask, null);
            }
        }
    }

    public class AcquisitionConfig
    {
        public int ChannelCount { get; set; } = 1;
        public double SampleRate { get; set; } = 1000;
        public double MinVoltage { get; set; } = -10;
        public double MaxVoltage { get; set; } = 10;
        public int BufferSize { get; set; } = 1000;
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public double[] Data { get; set; } = Array.Empty<double>();
        public DateTime Timestamp { get; set; }
    }

    public enum SignalType
    {
        Sine,
        Square,
        Triangle,
        Sawtooth
    }
}