using System.Collections.Concurrent;

namespace SeeSharpBackend.Services.DataAcquisition
{
    /// <summary>
    /// 环形缓冲区管理器
    /// 高性能的线程安全环形缓冲区实现
    /// </summary>
    public class CircularBufferManager : IDisposable
    {
        private readonly object _lock = new();
        private DataPacket[] _buffer;
        private int _head = 0;
        private int _tail = 0;
        private int _count = 0;
        private readonly int _capacity;
        private BufferConfiguration _config;
        private long _totalSamples = 0;
        private long _transferredSamples = 0;
        private int _overflowCount = 0;
        private bool _disposed = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity">缓冲区容量</param>
        public CircularBufferManager(int capacity)
        {
            _capacity = capacity;
            _buffer = new DataPacket[capacity];
            _config = new BufferConfiguration { Size = capacity };
        }

        /// <summary>
        /// 配置缓冲区
        /// </summary>
        /// <param name="config">缓冲区配置</param>
        public void Configure(BufferConfiguration config)
        {
            lock (_lock)
            {
                _config = config;
                
                // 如果需要调整大小
                if (config.Size != _capacity && config.Size > 0)
                {
                    ResizeBuffer(config.Size);
                }
            }
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dataPacket">数据包</param>
        /// <returns>是否成功添加</returns>
        public bool AddData(DataPacket dataPacket)
        {
            if (_disposed) return false;

            lock (_lock)
            {
                // 检查缓冲区是否已满
                if (_count >= _capacity)
                {
                    switch (_config.OverflowStrategy)
                    {
                        case OverflowStrategy.Overwrite:
                            // 覆盖最旧的数据
                            _head = (_head + 1) % _capacity;
                            _count--;
                            _overflowCount++;
                            break;

                        case OverflowStrategy.Drop:
                            // 丢弃新数据
                            _overflowCount++;
                            return false;

                        case OverflowStrategy.Stop:
                            // 停止添加
                            return false;

                        case OverflowStrategy.Warning:
                            // 触发警告但继续添加
                            _overflowCount++;
                            break;
                    }
                }

                // 添加数据到缓冲区
                _buffer[_tail] = dataPacket;
                _tail = (_tail + 1) % _capacity;
                _count++;
                _totalSamples += dataPacket.SampleCount;

                return true;
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="maxCount">最大读取数量</param>
        /// <returns>数据包列表</returns>
        public List<DataPacket> ReadData(int maxCount = int.MaxValue)
        {
            if (_disposed) return new List<DataPacket>();

            var result = new List<DataPacket>();

            lock (_lock)
            {
                var readCount = Math.Min(_count, maxCount);
                
                for (int i = 0; i < readCount; i++)
                {
                    var dataPacket = _buffer[_head];
                    result.Add(dataPacket);
                    
                    _head = (_head + 1) % _capacity;
                    _count--;
                    _transferredSamples += dataPacket.SampleCount;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取缓冲区状态
        /// </summary>
        /// <returns>缓冲区状态</returns>
        public BufferStatus GetStatus()
        {
            lock (_lock)
            {
                return new BufferStatus
                {
                    BufferSize = _capacity,
                    AvailableSamples = _count,
                    TransferredSamples = _transferredSamples,
                    IsOverflow = _overflowCount > 0,
                    OverflowCount = _overflowCount,
                    LastUpdated = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _head = 0;
                _tail = 0;
                _count = 0;
                _totalSamples = 0;
                _transferredSamples = 0;
                _overflowCount = 0;
                
                // 清空数组引用
                Array.Clear(_buffer, 0, _buffer.Length);
            }
        }

        /// <summary>
        /// 获取可用数据数量
        /// </summary>
        /// <returns>可用数据数量</returns>
        public int GetAvailableCount()
        {
            lock (_lock)
            {
                return _count;
            }
        }

        /// <summary>
        /// 检查是否为空
        /// </summary>
        /// <returns>是否为空</returns>
        public bool IsEmpty()
        {
            lock (_lock)
            {
                return _count == 0;
            }
        }

        /// <summary>
        /// 检查是否已满
        /// </summary>
        /// <returns>是否已满</returns>
        public bool IsFull()
        {
            lock (_lock)
            {
                return _count >= _capacity;
            }
        }

        /// <summary>
        /// 获取使用率
        /// </summary>
        /// <returns>使用率百分比</returns>
        public double GetUsagePercentage()
        {
            lock (_lock)
            {
                return _capacity > 0 ? (double)_count / _capacity * 100 : 0;
            }
        }

        /// <summary>
        /// 检查水位标记
        /// </summary>
        /// <returns>水位状态</returns>
        public WaterMarkStatus CheckWaterMark()
        {
            var usage = GetUsagePercentage();
            
            if (usage >= _config.HighWaterMark)
                return WaterMarkStatus.High;
            else if (usage <= _config.LowWaterMark)
                return WaterMarkStatus.Low;
            else
                return WaterMarkStatus.Normal;
        }

        /// <summary>
        /// 调整缓冲区大小
        /// </summary>
        /// <param name="newSize">新大小</param>
        private void ResizeBuffer(int newSize)
        {
            if (newSize <= 0) return;

            var newBuffer = new DataPacket[newSize];
            var copyCount = Math.Min(_count, newSize);

            // 复制现有数据
            for (int i = 0; i < copyCount; i++)
            {
                newBuffer[i] = _buffer[(_head + i) % _capacity];
            }

            _buffer = newBuffer;
            _head = 0;
            _tail = copyCount;
            _count = copyCount;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                Clear();
                _buffer = null!;
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// 水位状态
    /// </summary>
    public enum WaterMarkStatus
    {
        /// <summary>
        /// 低水位
        /// </summary>
        Low,

        /// <summary>
        /// 正常水位
        /// </summary>
        Normal,

        /// <summary>
        /// 高水位
        /// </summary>
        High
    }
}
