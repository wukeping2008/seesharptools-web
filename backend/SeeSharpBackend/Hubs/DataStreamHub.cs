using Microsoft.AspNetCore.SignalR;
using SeeSharpBackend.Services.MISD;
using SeeSharpBackend.Models.MISD;
using SeeSharpBackend.Services.DataCompression;
using SeeSharpBackend.Services.Connection;
using System.Collections.Concurrent;
using System.Text.Json;
using System.IO.Compression;

namespace SeeSharpBackend.Hubs
{
    /// <summary>
    /// 高性能实时数据流Hub
    /// 支持数据压缩、连接管理、批量传输等优化功能
    /// </summary>
    public class DataStreamHub : Hub
    {
        private readonly ILogger<DataStreamHub> _logger;
        private readonly IMISDService _misdService;
        private readonly IDataCompressionService _compressionService;
        private readonly IConnectionManager _connectionManager;
        
        // 静态连接管理
        private static readonly ConcurrentDictionary<string, ConnectionInfo> _connections = new();
        private static readonly ConcurrentDictionary<string, List<string>> _dataGroups = new();

        public DataStreamHub(
            ILogger<DataStreamHub> logger,
            IMISDService misdService,
            IDataCompressionService compressionService,
            IConnectionManager connectionManager)
        {
            _logger = logger;
            _misdService = misdService;
            _compressionService = compressionService;
            _connectionManager = connectionManager;
        }

        #region 连接管理

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
            var remoteIp = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var connectionInfo = new ConnectionInfo
            {
                ConnectionId = connectionId,
                ConnectedAt = DateTime.UtcNow,
                UserAgent = userAgent,
                RemoteIpAddress = remoteIp,
                IsActive = true,
                DataTransferStats = new DataTransferStats()
            };

            _connections.TryAdd(connectionId, connectionInfo);
            await _connectionManager.RegisterConnectionAsync(connectionInfo);

            _logger.LogInformation("客户端连接成功: {ConnectionId} from {RemoteIp}", connectionId, remoteIp);
            
            // 发送连接确认和服务器信息
            await Clients.Caller.SendAsync("ConnectionEstablished", new
            {
                ConnectionId = connectionId,
                ServerTime = DateTime.UtcNow,
                ServerVersion = "1.0.0",
                SupportedFeatures = new[]
                {
                    "DataCompression",
                    "BatchTransfer",
                    "RealTimeStreaming",
                    "ConnectionRecovery"
                }
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            
            if (_connections.TryRemove(connectionId, out var connectionInfo))
            {
                connectionInfo.IsActive = false;
                connectionInfo.DisconnectedAt = DateTime.UtcNow;
                
                await _connectionManager.UnregisterConnectionAsync(connectionInfo);
                
                // 从所有数据组中移除
                foreach (var group in _dataGroups)
                {
                    group.Value.Remove(connectionId);
                }

                var duration = connectionInfo.DisconnectedAt - connectionInfo.ConnectedAt;
                _logger.LogInformation(
                    "客户端断开连接: {ConnectionId}, 连接时长: {Duration}, 传输数据: {BytesTransferred} bytes",
                    connectionId, duration, connectionInfo.DataTransferStats.TotalBytesTransferred);
            }

            if (exception != null)
            {
                _logger.LogWarning(exception, "客户端异常断开: {ConnectionId}", connectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region 数据组管理

        /// <summary>
        /// 加入数据组（用于任务数据订阅）
        /// </summary>
        /// <param name="taskId">任务ID</param>
        /// <param name="options">订阅选项</param>
        [HubMethodName("JoinDataGroup")]
        public async Task JoinDataGroupAsync(int taskId, DataSubscriptionOptions? options = null)
        {
            var connectionId = Context.ConnectionId;
            var groupName = $"Task_{taskId}";

            try
            {
                await Groups.AddToGroupAsync(connectionId, groupName);
                
                // 更新数据组成员列表
                _dataGroups.AddOrUpdate(groupName, 
                    new List<string> { connectionId },
                    (key, list) => { list.Add(connectionId); return list; });

                // 更新连接信息
                if (_connections.TryGetValue(connectionId, out var connectionInfo))
                {
                    connectionInfo.SubscribedGroups.Add(groupName);
                    connectionInfo.SubscriptionOptions[groupName] = options ?? new DataSubscriptionOptions();
                }

                _logger.LogInformation("客户端 {ConnectionId} 加入数据组 {GroupName}", connectionId, groupName);
                
                // 发送确认消息
                await Clients.Caller.SendAsync("DataGroupJoined", new
                {
                    TaskId = taskId,
                    GroupName = groupName,
                    MemberCount = _dataGroups[groupName].Count,
                    Options = options
                });

                // 如果任务正在运行，立即开始数据流
                var taskStatus = await _misdService.GetTaskStatusAsync(taskId);
                if (taskStatus == Models.MISD.TaskStatus.Running)
                {
                    await StartDataStreamForConnection(connectionId, taskId, options);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加入数据组失败: {ConnectionId} -> {GroupName}", connectionId, groupName);
                await Clients.Caller.SendAsync("Error", new { Message = "加入数据组失败", Details = ex.Message });
            }
        }

        /// <summary>
        /// 离开数据组
        /// </summary>
        /// <param name="taskId">任务ID</param>
        [HubMethodName("LeaveDataGroup")]
        public async Task LeaveDataGroupAsync(int taskId)
        {
            var connectionId = Context.ConnectionId;
            var groupName = $"Task_{taskId}";

            try
            {
                await Groups.RemoveFromGroupAsync(connectionId, groupName);
                
                // 更新数据组成员列表
                if (_dataGroups.TryGetValue(groupName, out var members))
                {
                    members.Remove(connectionId);
                }

                // 更新连接信息
                if (_connections.TryGetValue(connectionId, out var connectionInfo))
                {
                    connectionInfo.SubscribedGroups.Remove(groupName);
                    connectionInfo.SubscriptionOptions.Remove(groupName);
                }

                _logger.LogInformation("客户端 {ConnectionId} 离开数据组 {GroupName}", connectionId, groupName);
                
                await Clients.Caller.SendAsync("DataGroupLeft", new
                {
                    TaskId = taskId,
                    GroupName = groupName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "离开数据组失败: {ConnectionId} -> {GroupName}", connectionId, groupName);
                await Clients.Caller.SendAsync("Error", new { Message = "离开数据组失败", Details = ex.Message });
            }
        }

        #endregion

        #region 实时数据传输

        /// <summary>
        /// 为特定连接启动数据流
        /// </summary>
        private async Task StartDataStreamForConnection(string connectionId, int taskId, DataSubscriptionOptions? options)
        {
            if (!_connections.TryGetValue(connectionId, out var connectionInfo))
                return;

            var compressionEnabled = options?.EnableCompression ?? true;
            var batchSize = options?.BatchSize ?? 1000;
            var maxFrequency = options?.MaxFrequency ?? 100; // Hz

            _logger.LogInformation("为连接 {ConnectionId} 启动任务 {TaskId} 的数据流", connectionId, taskId);

            // 这里应该启动实际的数据采集和传输
            // 当前为演示目的，使用模拟数据
            _ = Task.Run(async () =>
            {
                await SimulateDataStream(connectionId, taskId, compressionEnabled, batchSize, maxFrequency);
            });
        }

        /// <summary>
        /// 模拟数据流（实际实现中应该从MISD服务获取真实数据）
        /// </summary>
        private async Task SimulateDataStream(string connectionId, int taskId, bool compressionEnabled, int batchSize, int maxFrequency)
        {
            var random = new Random();
            var sampleRate = 1000; // 1kHz
            var channels = 4;
            var intervalMs = 1000 / maxFrequency;
            
            while (_connections.ContainsKey(connectionId) && 
                   _connections[connectionId].SubscribedGroups.Contains($"Task_{taskId}"))
            {
                try
                {
                    // 生成模拟数据
                    var timestamp = DateTime.UtcNow;
                    var samples = new double[batchSize, channels];
                    
                    for (int i = 0; i < batchSize; i++)
                    {
                        var time = i / (double)sampleRate;
                        for (int ch = 0; ch < channels; ch++)
                        {
                            // 混合正弦波 + 噪声
                            var signal = 5.0 * Math.Sin(2 * Math.PI * 10 * time) + // 10Hz
                                         1.5 * Math.Sin(2 * Math.PI * 50 * time) + // 50Hz
                                         0.5 * (random.NextDouble() - 0.5); // 噪声
                            samples[i, ch] = signal;
                        }
                    }

                    // 创建数据包
                    var dataPacket = new RealTimeDataPacket
                    {
                        TaskId = taskId,
                        Timestamp = timestamp,
                        SampleRate = sampleRate,
                        ChannelCount = channels,
                        SampleCount = batchSize,
                        Data = ConvertToJaggedArray(samples),
                        SequenceNumber = _connections[connectionId].DataTransferStats.PacketsSent,
                        CompressionEnabled = compressionEnabled
                    };

                    // 序列化和压缩
                    byte[] data;
                    if (compressionEnabled)
                    {
                        data = await _compressionService.CompressDataAsync(dataPacket);
                    }
                    else
                    {
                        data = JsonSerializer.SerializeToUtf8Bytes(dataPacket);
                    }

                    // 发送数据
                    await Clients.Client(connectionId).SendAsync("RealTimeData", data);

                    // 更新统计信息
                    var stats = _connections[connectionId].DataTransferStats;
                    stats.PacketsSent++;
                    stats.TotalBytesTransferred += data.Length;
                    stats.LastTransferTime = timestamp;

                    await Task.Delay(intervalMs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "数据流传输错误: {ConnectionId}, Task {TaskId}", connectionId, taskId);
                    break;
                }
            }
        }

        /// <summary>
        /// 将二维数组转换为交错数组（JSON友好）
        /// </summary>
        private double[][] ConvertToJaggedArray(double[,] array2D)
        {
            int rows = array2D.GetLength(0);
            int cols = array2D.GetLength(1);
            double[][] jaggedArray = new double[rows][];

            for (int i = 0; i < rows; i++)
            {
                jaggedArray[i] = new double[cols];
                for (int j = 0; j < cols; j++)
                {
                    jaggedArray[i][j] = array2D[i, j];
                }
            }

            return jaggedArray;
        }

        #endregion

        #region 客户端方法

        /// <summary>
        /// 客户端心跳检测
        /// </summary>
        [HubMethodName("Heartbeat")]
        public async Task HeartbeatAsync()
        {
            var connectionId = Context.ConnectionId;
            if (_connections.TryGetValue(connectionId, out var connectionInfo))
            {
                connectionInfo.LastHeartbeat = DateTime.UtcNow;
            }

            await Clients.Caller.SendAsync("HeartbeatResponse", DateTime.UtcNow);
        }

        /// <summary>
        /// 获取连接统计信息
        /// </summary>
        [HubMethodName("GetConnectionStats")]
        public async Task GetConnectionStatsAsync()
        {
            var connectionId = Context.ConnectionId;
            if (_connections.TryGetValue(connectionId, out var connectionInfo))
            {
                await Clients.Caller.SendAsync("ConnectionStats", new
                {
                    ConnectionId = connectionId,
                    ConnectedAt = connectionInfo.ConnectedAt,
                    Duration = DateTime.UtcNow - connectionInfo.ConnectedAt,
                    SubscribedGroups = connectionInfo.SubscribedGroups.ToArray(),
                    DataTransferStats = connectionInfo.DataTransferStats
                });
            }
        }

        /// <summary>
        /// 更新订阅选项
        /// </summary>
        [HubMethodName("UpdateSubscriptionOptions")]
        public async Task UpdateSubscriptionOptionsAsync(int taskId, DataSubscriptionOptions options)
        {
            var connectionId = Context.ConnectionId;
            var groupName = $"Task_{taskId}";

            if (_connections.TryGetValue(connectionId, out var connectionInfo) &&
                connectionInfo.SubscriptionOptions.ContainsKey(groupName))
            {
                connectionInfo.SubscriptionOptions[groupName] = options;
                
                _logger.LogInformation("更新连接 {ConnectionId} 的订阅选项: {GroupName}", connectionId, groupName);
                
                await Clients.Caller.SendAsync("SubscriptionOptionsUpdated", new
                {
                    TaskId = taskId,
                    Options = options
                });
            }
        }

        #endregion
    }

    #region 数据模型

    /// <summary>
    /// 连接信息
    /// </summary>
    public class ConnectionInfo
    {
        public string ConnectionId { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime? DisconnectedAt { get; set; }
        public string UserAgent { get; set; } = string.Empty;
        public string RemoteIpAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public HashSet<string> SubscribedGroups { get; set; } = new();
        public Dictionary<string, DataSubscriptionOptions> SubscriptionOptions { get; set; } = new();
        public DataTransferStats DataTransferStats { get; set; } = new();
    }

    /// <summary>
    /// 数据传输统计
    /// </summary>
    public class DataTransferStats
    {
        public long PacketsSent { get; set; }
        public long TotalBytesTransferred { get; set; }
        public DateTime? LastTransferTime { get; set; }
        public double AverageTransferRate => 
            LastTransferTime.HasValue && PacketsSent > 0 
                ? TotalBytesTransferred / (DateTime.UtcNow - LastTransferTime.Value).TotalSeconds 
                : 0;
    }

    /// <summary>
    /// 数据订阅选项
    /// </summary>
    public class DataSubscriptionOptions
    {
        public bool EnableCompression { get; set; } = true;
        public int BatchSize { get; set; } = 1000;
        public int MaxFrequency { get; set; } = 100; // Hz
        public string[] ChannelFilter { get; set; } = Array.Empty<string>();
        public double? SampleRateLimit { get; set; }
        public bool EnableStatistics { get; set; } = true;
    }

    /// <summary>
    /// 实时数据包
    /// </summary>
    public class RealTimeDataPacket
    {
        public int TaskId { get; set; }
        public DateTime Timestamp { get; set; }
        public double SampleRate { get; set; }
        public int ChannelCount { get; set; }
        public int SampleCount { get; set; }
        public double[][] Data { get; set; } = Array.Empty<double[]>();
        public long SequenceNumber { get; set; }
        public bool CompressionEnabled { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    #endregion
}
