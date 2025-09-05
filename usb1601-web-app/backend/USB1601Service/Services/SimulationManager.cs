using System;
using System.Threading;
using System.Threading.Tasks;

namespace USB1601Service.Services
{
    /// <summary>
    /// 模拟数据管理器 - 用于没有硬件时的测试
    /// </summary>
    public class SimulationManager
    {
        private readonly ILogger<SimulationManager> _logger;
        private Timer? _dataTimer;
        private bool _isRunning = false;
        private double _time = 0;
        private Random _random = new Random();
        
        public event EventHandler<DataReceivedEventArgs>? DataReceived;
        
        // 配置参数
        private double _sampleRate = 1000;
        private int _channelCount = 1;
        private SignalType _signalType = SignalType.Sine;
        private double _frequency = 100;
        private double _amplitude = 5;
        private double _noiseLevel = 0.1;

        public SimulationManager(ILogger<SimulationManager> logger)
        {
            _logger = logger;
        }

        public Task<bool> InitializeAsync()
        {
            _logger.LogInformation("模拟模式初始化成功");
            return Task.FromResult(true);
        }

        public Task<bool> ConfigureAsync(double sampleRate, int channelCount, SignalType signalType, double frequency, double amplitude)
        {
            _sampleRate = sampleRate;
            _channelCount = channelCount;
            _signalType = signalType;
            _frequency = frequency;
            _amplitude = amplitude;
            
            _logger.LogInformation($"模拟配置: {channelCount}通道, {sampleRate}Hz, {signalType}信号, {frequency}Hz, {amplitude}V");
            return Task.FromResult(true);
        }

        public Task<bool> StartAsync()
        {
            if (_isRunning) return Task.FromResult(false);
            
            _isRunning = true;
            _time = 0;
            
            // 计算定时器间隔（毫秒）
            // 优化：降低推送频率以减少前端压力
            int interval = Math.Max(50, (int)(1000.0 / (_sampleRate / 200))); // 降低更新频率
            
            _dataTimer = new Timer(GenerateData, null, 0, interval);
            
            _logger.LogInformation($"模拟数据生成已启动，间隔: {interval}ms");
            return Task.FromResult(true);
        }

        public Task<bool> StopAsync()
        {
            _isRunning = false;
            _dataTimer?.Dispose();
            _dataTimer = null;
            
            _logger.LogInformation("模拟数据生成已停止");
            return Task.FromResult(true);
        }

        private void GenerateData(object? state)
        {
            if (!_isRunning) return;
            
            try
            {
                int samplesPerBatch = 100;
                var data = new double[samplesPerBatch * _channelCount];
                
                for (int i = 0; i < samplesPerBatch; i++)
                {
                    double t = _time + i / _sampleRate;
                    
                    for (int ch = 0; ch < _channelCount; ch++)
                    {
                        double value = GenerateSignalValue(t, ch);
                        // 添加噪声
                        value += (_random.NextDouble() - 0.5) * _noiseLevel;
                        
                        data[i * _channelCount + ch] = value;
                    }
                }
                
                _time += samplesPerBatch / _sampleRate;
                
                // 触发数据接收事件
                DataReceived?.Invoke(this, new DataReceivedEventArgs 
                { 
                    Data = data, 
                    Timestamp = DateTime.Now 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成模拟数据失败");
            }
        }

        private double GenerateSignalValue(double t, int channel)
        {
            // 为不同通道生成不同相位的信号
            double phase = channel * Math.PI / 4;
            double omega = 2 * Math.PI * _frequency;
            
            switch (_signalType)
            {
                case SignalType.Sine:
                    return _amplitude * Math.Sin(omega * t + phase);
                    
                case SignalType.Square:
                    return _amplitude * (Math.Sin(omega * t + phase) >= 0 ? 1 : -1);
                    
                case SignalType.Triangle:
                    double period = 1.0 / _frequency;
                    double localTime = (t % period) / period;
                    return _amplitude * (4 * Math.Abs(localTime - 0.5) - 1);
                    
                case SignalType.Sawtooth:
                    double sawPeriod = 1.0 / _frequency;
                    double sawTime = (t % sawPeriod) / sawPeriod;
                    return _amplitude * (2 * sawTime - 1);
                    
                default:
                    return 0;
            }
        }

        public void Dispose()
        {
            StopAsync().Wait();
        }
    }
}