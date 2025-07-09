using System.IO.Compression;
using System.Text.Json;
using Microsoft.IO;

namespace SeeSharpBackend.Services.DataCompression
{
    /// <summary>
    /// 高性能数据压缩服务实现
    /// 使用GZip压缩算法和可回收内存流优化性能
    /// </summary>
    public class DataCompressionService : IDataCompressionService
    {
        private readonly ILogger<DataCompressionService> _logger;
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;
        private readonly JsonSerializerOptions _jsonOptions;

        public DataCompressionService(ILogger<DataCompressionService> logger)
        {
            _logger = logger;
            _memoryStreamManager = new RecyclableMemoryStreamManager();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// 压缩数据对象
        /// </summary>
        public async Task<byte[]> CompressDataAsync<T>(T data, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            try
            {
                // 序列化为JSON字节数组
                var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(data, _jsonOptions);
                
                // 压缩字节数组
                var compressedData = await CompressBytesAsync(jsonBytes, compressionLevel);
                
                _logger.LogDebug("数据压缩完成: 原始大小 {OriginalSize} bytes, 压缩后 {CompressedSize} bytes, 压缩率 {CompressionRatio:P2}",
                    jsonBytes.Length, compressedData.Length, (double)compressedData.Length / jsonBytes.Length);
                
                return compressedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据压缩失败");
                throw;
            }
        }

        /// <summary>
        /// 解压缩数据
        /// </summary>
        public async Task<T?> DecompressDataAsync<T>(byte[] compressedData)
        {
            try
            {
                // 解压缩字节数组
                var decompressedBytes = await DecompressBytesAsync(compressedData);
                
                // 反序列化JSON
                var result = JsonSerializer.Deserialize<T>(decompressedBytes, _jsonOptions);
                
                _logger.LogDebug("数据解压缩完成: 压缩大小 {CompressedSize} bytes, 解压后 {DecompressedSize} bytes",
                    compressedData.Length, decompressedBytes.Length);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据解压缩失败");
                throw;
            }
        }

        /// <summary>
        /// 压缩字节数组
        /// </summary>
        public async Task<byte[]> CompressBytesAsync(byte[] data, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using var outputStream = _memoryStreamManager.GetStream();
            using (var gzipStream = new GZipStream(outputStream, compressionLevel, leaveOpen: true))
            {
                await gzipStream.WriteAsync(data, 0, data.Length);
                await gzipStream.FlushAsync();
            }

            return outputStream.ToArray();
        }

        /// <summary>
        /// 解压缩字节数组
        /// </summary>
        public async Task<byte[]> DecompressBytesAsync(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
                return Array.Empty<byte>();

            using var inputStream = _memoryStreamManager.GetStream(compressedData);
            using var outputStream = _memoryStreamManager.GetStream();
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                await gzipStream.CopyToAsync(outputStream);
            }

            return outputStream.ToArray();
        }

        /// <summary>
        /// 获取压缩统计信息
        /// </summary>
        public CompressionStats GetCompressionStats(long originalSize, long compressedSize)
        {
            return new CompressionStats
            {
                OriginalSize = originalSize,
                CompressedSize = compressedSize
            };
        }

        /// <summary>
        /// 批量压缩数据流
        /// </summary>
        public async IAsyncEnumerable<byte[]> CompressStreamAsync(
            IAsyncEnumerable<byte[]> dataStream, 
            CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            await foreach (var data in dataStream)
            {
                if (data != null && data.Length > 0)
                {
                    var compressedData = await CompressBytesAsync(data, compressionLevel);
                    yield return compressedData;
                }
            }
        }
    }

    /// <summary>
    /// 高级数据压缩服务
    /// 支持多种压缩算法和自适应压缩
    /// </summary>
    public class AdvancedDataCompressionService : IDataCompressionService
    {
        private readonly ILogger<AdvancedDataCompressionService> _logger;
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly Dictionary<CompressionAlgorithm, Func<Stream, CompressionLevel, Stream>> _compressionFactories;

        public enum CompressionAlgorithm
        {
            GZip,
            Deflate,
            Brotli
        }

        public AdvancedDataCompressionService(ILogger<AdvancedDataCompressionService> logger)
        {
            _logger = logger;
            _memoryStreamManager = new RecyclableMemoryStreamManager();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            _compressionFactories = new Dictionary<CompressionAlgorithm, Func<Stream, CompressionLevel, Stream>>
            {
                { CompressionAlgorithm.GZip, (stream, level) => new GZipStream(stream, level) },
                { CompressionAlgorithm.Deflate, (stream, level) => new DeflateStream(stream, level) },
                { CompressionAlgorithm.Brotli, (stream, level) => new BrotliStream(stream, level) }
            };
        }

        public async Task<byte[]> CompressDataAsync<T>(T data, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            // 自动选择最佳压缩算法
            var algorithm = await SelectOptimalCompressionAlgorithmAsync(data);
            return await CompressDataWithAlgorithmAsync(data, algorithm, compressionLevel);
        }

        public async Task<T?> DecompressDataAsync<T>(byte[] compressedData)
        {
            // 检测压缩算法并解压
            var algorithm = DetectCompressionAlgorithm(compressedData);
            return await DecompressDataWithAlgorithmAsync<T>(compressedData, algorithm);
        }

        public async Task<byte[]> CompressBytesAsync(byte[] data, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            return await CompressBytesWithAlgorithmAsync(data, CompressionAlgorithm.GZip, compressionLevel);
        }

        public async Task<byte[]> DecompressBytesAsync(byte[] compressedData)
        {
            var algorithm = DetectCompressionAlgorithm(compressedData);
            return await DecompressBytesWithAlgorithmAsync(compressedData, algorithm);
        }

        public CompressionStats GetCompressionStats(long originalSize, long compressedSize)
        {
            return new CompressionStats
            {
                OriginalSize = originalSize,
                CompressedSize = compressedSize
            };
        }

        public async IAsyncEnumerable<byte[]> CompressStreamAsync(
            IAsyncEnumerable<byte[]> dataStream, 
            CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            await foreach (var data in dataStream)
            {
                if (data != null && data.Length > 0)
                {
                    var compressedData = await CompressBytesAsync(data, compressionLevel);
                    yield return compressedData;
                }
            }
        }

        /// <summary>
        /// 使用指定算法压缩数据
        /// </summary>
        private async Task<byte[]> CompressDataWithAlgorithmAsync<T>(T data, CompressionAlgorithm algorithm, CompressionLevel level)
        {
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(data, _jsonOptions);
            return await CompressBytesWithAlgorithmAsync(jsonBytes, algorithm, level);
        }

        /// <summary>
        /// 使用指定算法压缩字节数组
        /// </summary>
        private async Task<byte[]> CompressBytesWithAlgorithmAsync(byte[] data, CompressionAlgorithm algorithm, CompressionLevel level)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using var outputStream = _memoryStreamManager.GetStream();
            
            // 写入算法标识符（1字节）
            outputStream.WriteByte((byte)algorithm);
            
            using (var compressionStream = _compressionFactories[algorithm](outputStream, level))
            {
                await compressionStream.WriteAsync(data, 0, data.Length);
                await compressionStream.FlushAsync();
            }

            return outputStream.ToArray();
        }

        /// <summary>
        /// 使用指定算法解压数据
        /// </summary>
        private async Task<T?> DecompressDataWithAlgorithmAsync<T>(byte[] compressedData, CompressionAlgorithm algorithm)
        {
            var decompressedBytes = await DecompressBytesWithAlgorithmAsync(compressedData, algorithm);
            return JsonSerializer.Deserialize<T>(decompressedBytes, _jsonOptions);
        }

        /// <summary>
        /// 使用指定算法解压字节数组
        /// </summary>
        private async Task<byte[]> DecompressBytesWithAlgorithmAsync(byte[] compressedData, CompressionAlgorithm algorithm)
        {
            if (compressedData == null || compressedData.Length <= 1)
                return Array.Empty<byte>();

            using var inputStream = _memoryStreamManager.GetStream(compressedData.Skip(1).ToArray());
            using var outputStream = _memoryStreamManager.GetStream();
            
            Stream decompressionStream = algorithm switch
            {
                CompressionAlgorithm.GZip => new GZipStream(inputStream, CompressionMode.Decompress),
                CompressionAlgorithm.Deflate => new DeflateStream(inputStream, CompressionMode.Decompress),
                CompressionAlgorithm.Brotli => new BrotliStream(inputStream, CompressionMode.Decompress),
                _ => throw new NotSupportedException($"不支持的压缩算法: {algorithm}")
            };
            
            using (decompressionStream)
            {
                await decompressionStream.CopyToAsync(outputStream);
            }
            
            return outputStream.ToArray();
        }

        /// <summary>
        /// 自动选择最佳压缩算法
        /// </summary>
        private async Task<CompressionAlgorithm> SelectOptimalCompressionAlgorithmAsync<T>(T data)
        {
            // 简单策略：根据数据类型和大小选择算法
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(data, _jsonOptions);
            
            if (jsonBytes.Length < 1024) // 小于1KB，使用快速压缩
                return CompressionAlgorithm.Deflate;
            else if (jsonBytes.Length < 10240) // 小于10KB，使用平衡压缩
                return CompressionAlgorithm.GZip;
            else // 大于10KB，使用高压缩率
                return CompressionAlgorithm.Brotli;
        }

        /// <summary>
        /// 检测压缩算法
        /// </summary>
        private CompressionAlgorithm DetectCompressionAlgorithm(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
                return CompressionAlgorithm.GZip;

            return (CompressionAlgorithm)compressedData[0];
        }
    }
}
