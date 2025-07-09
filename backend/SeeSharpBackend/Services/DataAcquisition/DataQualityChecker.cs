using System.Diagnostics;

namespace SeeSharpBackend.Services.DataAcquisition
{
    /// <summary>
    /// 数据质量检查器
    /// 负责检查数据包的完整性和质量
    /// </summary>
    public class DataQualityChecker : IDisposable
    {
        private readonly AcquisitionConfiguration _acquisitionConfig;
        private readonly DataQualityConfiguration _qualityConfig;
        private readonly DataQualityReport _report;
        private readonly object _lock = new();
        private long _lastSequenceNumber = -1;
        private DateTime _lastTimestamp = DateTime.MinValue;
        private bool _disposed = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="acquisitionConfig">采集配置</param>
        /// <param name="qualityConfig">质量检查配置</param>
        public DataQualityChecker(AcquisitionConfiguration acquisitionConfig, DataQualityConfiguration? qualityConfig = null)
        {
            _acquisitionConfig = acquisitionConfig ?? throw new ArgumentNullException(nameof(acquisitionConfig));
            _qualityConfig = qualityConfig ?? new DataQualityConfiguration();
            _report = new DataQualityReport
            {
                TaskId = 0, // 将在使用时设置
                GeneratedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 检查数据质量
        /// </summary>
        /// <param name="dataPacket">数据包</param>
        /// <returns>质量标志</returns>
        public DataQualityFlags CheckQuality(DataPacket dataPacket)
        {
            if (_disposed) return DataQualityFlags.HardwareError;

            lock (_lock)
            {
                var flags = DataQualityFlags.Good;
                _report.TotalSamples += dataPacket.SampleCount;

                try
                {
                    // CRC校验
                    if (_qualityConfig.EnableCrcCheck)
                    {
                        var calculatedCrc = CalculateCrc32(dataPacket);
                        if (calculatedCrc != dataPacket.Crc32)
                        {
                            flags |= DataQualityFlags.CrcError;
                            _report.CrcErrors++;
                            _report.ErrorDetails.Add($"CRC错误: 期望 {dataPacket.Crc32:X8}, 实际 {calculatedCrc:X8}");
                        }
                    }

                    // 时间戳验证
                    if (_qualityConfig.EnableTimestampValidation)
                    {
                        if (_lastTimestamp != DateTime.MinValue)
                        {
                            var timeDiff = (dataPacket.Timestamp - _lastTimestamp).TotalMilliseconds;
                            var expectedInterval = 1000.0 / dataPacket.SampleRate * dataPacket.SampleCount;
                            var deviation = Math.Abs(timeDiff - expectedInterval);

                            if (deviation > _qualityConfig.MaxTimestampDeviation)
                            {
                                flags |= DataQualityFlags.TimestampError;
                                _report.TimestampErrors++;
                                _report.ErrorDetails.Add($"时间戳偏差过大: {deviation:F2}ms");
                            }
                        }
                        _lastTimestamp = dataPacket.Timestamp;
                    }

                    // 丢包检测
                    if (_qualityConfig.EnablePacketLossDetection)
                    {
                        if (_lastSequenceNumber >= 0)
                        {
                            var expectedSequence = _lastSequenceNumber + 1;
                            if (dataPacket.SequenceNumber != expectedSequence)
                            {
                                var lostPackets = dataPacket.SequenceNumber - expectedSequence;
                                if (lostPackets > 0)
                                {
                                    flags |= DataQualityFlags.PacketLoss;
                                    _report.PacketLoss += lostPackets;
                                    _report.ErrorDetails.Add($"丢包检测: 丢失 {lostPackets} 个数据包");
                                }
                            }
                        }
                        _lastSequenceNumber = dataPacket.SequenceNumber;
                    }

                    // 数据范围检查
                    if (_qualityConfig.EnableRangeCheck)
                    {
                        foreach (var channelData in dataPacket.ChannelData)
                        {
                            var channelConfig = _acquisitionConfig.Channels.FirstOrDefault(c => c.ChannelId == channelData.Key);
                            if (channelConfig != null)
                            {
                                foreach (var value in channelData.Value)
                                {
                                    if (value < channelConfig.RangeMin || value > channelConfig.RangeMax)
                                    {
                                        flags |= DataQualityFlags.RangeError;
                                        _report.RangeErrors++;
                                        _report.ErrorDetails.Add($"通道 {channelData.Key} 数据超出范围: {value}");
                                        break; // 每个通道只报告一次
                                    }
                                }
                            }
                        }
                    }

                    // 更新统计
                    if (flags == DataQualityFlags.Good)
                    {
                        _report.ValidSamples += dataPacket.SampleCount;
                    }

                    return flags;
                }
                catch (Exception ex)
                {
                    _report.ErrorDetails.Add($"质量检查异常: {ex.Message}");
                    return DataQualityFlags.HardwareError;
                }
            }
        }

        /// <summary>
        /// 生成质量报告
        /// </summary>
        /// <returns>质量报告</returns>
        public DataQualityReport GenerateReport()
        {
            lock (_lock)
            {
                _report.GeneratedAt = DateTime.UtcNow;
                return new DataQualityReport
                {
                    TaskId = _report.TaskId,
                    TotalSamples = _report.TotalSamples,
                    ValidSamples = _report.ValidSamples,
                    CrcErrors = _report.CrcErrors,
                    TimestampErrors = _report.TimestampErrors,
                    PacketLoss = _report.PacketLoss,
                    RangeErrors = _report.RangeErrors,
                    GeneratedAt = _report.GeneratedAt,
                    ErrorDetails = new List<string>(_report.ErrorDetails)
                };
            }
        }

        /// <summary>
        /// 重置统计
        /// </summary>
        public void ResetStatistics()
        {
            lock (_lock)
            {
                _report.TotalSamples = 0;
                _report.ValidSamples = 0;
                _report.CrcErrors = 0;
                _report.TimestampErrors = 0;
                _report.PacketLoss = 0;
                _report.RangeErrors = 0;
                _report.ErrorDetails.Clear();
                _lastSequenceNumber = -1;
                _lastTimestamp = DateTime.MinValue;
            }
        }

        /// <summary>
        /// 设置任务ID
        /// </summary>
        /// <param name="taskId">任务ID</param>
        public void SetTaskId(int taskId)
        {
            lock (_lock)
            {
                _report.TaskId = taskId;
            }
        }

        /// <summary>
        /// 获取质量评分
        /// </summary>
        /// <returns>质量评分 (0-100)</returns>
        public double GetQualityScore()
        {
            lock (_lock)
            {
                return _report.QualityScore;
            }
        }

        /// <summary>
        /// 检查是否超过质量阈值
        /// </summary>
        /// <returns>是否超过阈值</returns>
        public bool IsQualityThresholdExceeded()
        {
            lock (_lock)
            {
                var packetLossRate = _report.TotalSamples > 0 ? 
                    (double)_report.PacketLoss / _report.TotalSamples * 100 : 0;
                
                return packetLossRate > _qualityConfig.MaxPacketLossRate;
            }
        }

        /// <summary>
        /// 计算CRC32校验值
        /// </summary>
        /// <param name="dataPacket">数据包</param>
        /// <returns>CRC32值</returns>
        private uint CalculateCrc32(DataPacket dataPacket)
        {
            // 简化的CRC32计算，与DataAcquisitionEngine中的实现保持一致
            uint crc = 0xFFFFFFFF;
            
            foreach (var channelData in dataPacket.ChannelData.Values)
            {
                foreach (var value in channelData)
                {
                    var bytes = BitConverter.GetBytes(value);
                    foreach (var b in bytes)
                    {
                        crc ^= b;
                        for (int i = 0; i < 8; i++)
                        {
                            if ((crc & 1) != 0)
                                crc = (crc >> 1) ^ 0xEDB88320;
                            else
                                crc >>= 1;
                        }
                    }
                }
            }
            
            return ~crc;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                _report.ErrorDetails.Clear();
                _disposed = true;
            }
        }
    }
}
