using System.Threading.Channels;

namespace SeeSharpBackend.Services.DataAcquisition
{
    /// <summary>
    /// 数据采集任务
    /// </summary>
    public class AcquisitionTask : IDisposable
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// 配置信息
        /// </summary>
        public AcquisitionConfiguration Configuration { get; set; } = new();

        /// <summary>
        /// 任务状态
        /// </summary>
        public AcquisitionTaskStatus Status { get; set; } = new();

        /// <summary>
        /// 数据通道
        /// </summary>
        public Channel<DataPacket> DataChannel { get; set; } = Channel.CreateUnbounded<DataPacket>();

        /// <summary>
        /// 缓冲区管理器
        /// </summary>
        public CircularBufferManager BufferManager { get; set; } = new(10000);

        /// <summary>
        /// 数据质量检查器
        /// </summary>
        public DataQualityChecker? QualityChecker { get; set; }

        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPaused { get; set; } = false;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        private bool _disposed = false;

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            DataChannel?.Writer?.Complete();
            BufferManager?.Dispose();
            QualityChecker?.Dispose();

            _disposed = true;
        }
    }
}
