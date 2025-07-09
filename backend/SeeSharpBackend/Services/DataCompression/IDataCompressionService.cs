using System.IO.Compression;
using System.Text.Json;

namespace SeeSharpBackend.Services.DataCompression
{
    /// <summary>
    /// 数据压缩服务接口
    /// 提供高效的实时数据压缩和解压缩功能
    /// </summary>
    public interface IDataCompressionService
    {
        /// <summary>
        /// 压缩数据对象
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">要压缩的数据</param>
        /// <param name="compressionLevel">压缩级别</param>
        /// <returns>压缩后的字节数组</returns>
        Task<byte[]> CompressDataAsync<T>(T data, CompressionLevel compressionLevel = CompressionLevel.Optimal);

        /// <summary>
        /// 解压缩数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="compressedData">压缩的数据</param>
        /// <returns>解压缩后的数据对象</returns>
        Task<T?> DecompressDataAsync<T>(byte[] compressedData);

        /// <summary>
        /// 压缩字节数组
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="compressionLevel">压缩级别</param>
        /// <returns>压缩后的数据</returns>
        Task<byte[]> CompressBytesAsync(byte[] data, CompressionLevel compressionLevel = CompressionLevel.Optimal);

        /// <summary>
        /// 解压缩字节数组
        /// </summary>
        /// <param name="compressedData">压缩的数据</param>
        /// <returns>解压缩后的数据</returns>
        Task<byte[]> DecompressBytesAsync(byte[] compressedData);

        /// <summary>
        /// 获取压缩统计信息
        /// </summary>
        /// <param name="originalSize">原始大小</param>
        /// <param name="compressedSize">压缩后大小</param>
        /// <returns>压缩统计</returns>
        CompressionStats GetCompressionStats(long originalSize, long compressedSize);

        /// <summary>
        /// 批量压缩数据流
        /// </summary>
        /// <param name="dataStream">数据流</param>
        /// <param name="compressionLevel">压缩级别</param>
        /// <returns>压缩后的数据流</returns>
        IAsyncEnumerable<byte[]> CompressStreamAsync(IAsyncEnumerable<byte[]> dataStream, CompressionLevel compressionLevel = CompressionLevel.Optimal);
    }

    /// <summary>
    /// 压缩统计信息
    /// </summary>
    public class CompressionStats
    {
        public long OriginalSize { get; set; }
        public long CompressedSize { get; set; }
        public double CompressionRatio => OriginalSize > 0 ? (double)CompressedSize / OriginalSize : 0;
        public double SpaceSavings => 1 - CompressionRatio;
        public double CompressionPercentage => SpaceSavings * 100;
    }
}
