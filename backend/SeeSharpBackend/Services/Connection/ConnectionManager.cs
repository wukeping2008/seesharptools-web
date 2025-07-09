using Microsoft.AspNetCore.SignalR;
using SeeSharpBackend.Hubs;
using System.Collections.Concurrent;

namespace SeeSharpBackend.Services.Connection
{
    /// <summary>
    /// 连接管理服务实现
    /// 提供高性能的SignalR连接管理和监控功能
    /// </summary>
    public class ConnectionManager : IConnectionManager
    {
        private readonly ILogger<ConnectionManager> _logger;
        private readonly IHubContext<DataStreamHub> _hubContext;
        private readonly ConcurrentDictionary<string, Hubs.ConnectionInfo> _connections;
        private readonly ConcurrentDictionary<string, HashSet<string>> _groups;
        private readonly Timer _cleanupTimer;

        public ConnectionManager(
            ILogger<ConnectionManager> logger,
            IHubContext<DataStreamHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
            _connections = new ConcurrentDictionary<string, Hubs.ConnectionInfo>();
            _groups = new ConcurrentDictionary<string, HashSet<string>>();

            // 启动定期清理任务（每5分钟执行一次）
            _cleanupTimer = new Timer(async _ => await CleanupExpiredConnectionsAsync(), 
                null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// 注册新连接
        /// </summary>
        public async Task RegisterConnectionAsync(Hubs.ConnectionInfo connectionInfo)
        {
            try
            {
                _connections.TryAdd(connectionInfo.ConnectionId, connectionInfo);
                
                _logger.LogInformation("连接已注册: {ConnectionId} from {RemoteIP}", 
                    connectionInfo.ConnectionId, connectionInfo.RemoteIpAddress);

                // 发送欢迎消息
                await _hubContext.Clients.Client(connectionInfo.ConnectionId)
                    .SendAsync("Welcome", new
                    {
                        Message = "欢迎连接到SeeSharpTools数据流服务",
                        ServerTime = DateTime.UtcNow,
                        ConnectionId = connectionInfo.ConnectionId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注册连接失败: {ConnectionId}", connectionInfo.ConnectionId);
                throw;
            }
        }

        /// <summary>
        /// 注销连接
        /// </summary>
        public async Task UnregisterConnectionAsync(Hubs.ConnectionInfo connectionInfo)
        {
            try
            {
                if (_connections.TryRemove(connectionInfo.ConnectionId, out var removedConnection))
                {
                    // 从所有组中移除连接
                    foreach (var group in _groups)
                    {
                        group.Value.Remove(connectionInfo.ConnectionId);
                        if (group.Value.Count == 0)
                        {
                            _groups.TryRemove(group.Key, out _);
                        }
                    }

                    var duration = DateTime.UtcNow - removedConnection.ConnectedAt;
                    _logger.LogInformation("连接已注销: {ConnectionId}, 持续时间: {Duration}, 传输数据: {Bytes} bytes",
                        connectionInfo.ConnectionId, duration, removedConnection.DataTransferStats.TotalBytesTransferred);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注销连接失败: {ConnectionId}", connectionInfo.ConnectionId);
                throw;
            }
        }

        /// <summary>
        /// 获取连接信息
        /// </summary>
        public async Task<Hubs.ConnectionInfo?> GetConnectionAsync(string connectionId)
        {
            await Task.CompletedTask;
            return _connections.TryGetValue(connectionId, out var connection) ? connection : null;
        }

        /// <summary>
        /// 获取所有活跃连接
        /// </summary>
        public async Task<IEnumerable<Hubs.ConnectionInfo>> GetActiveConnectionsAsync()
        {
            await Task.CompletedTask;
            return _connections.Values.Where(c => c.IsActive).ToList();
        }

        /// <summary>
        /// 获取连接统计信息
        /// </summary>
        public async Task<ConnectionStatistics> GetConnectionStatisticsAsync()
        {
            await Task.CompletedTask;

            var activeConnections = _connections.Values.Where(c => c.IsActive).ToList();
            var totalBytesTransferred = activeConnections.Sum(c => c.DataTransferStats.TotalBytesTransferred);
            var totalPacketsSent = activeConnections.Sum(c => c.DataTransferStats.PacketsSent);

            var connectionsByUserAgent = activeConnections
                .GroupBy(c => c.UserAgent)
                .ToDictionary(g => g.Key, g => g.Count());

            var connectionsByRemoteIP = activeConnections
                .GroupBy(c => c.RemoteIpAddress)
                .ToDictionary(g => g.Key, g => g.Count());

            var averageDuration = activeConnections.Any() 
                ? TimeSpan.FromTicks((long)activeConnections.Average(c => (DateTime.UtcNow - c.ConnectedAt).Ticks))
                : TimeSpan.Zero;

            return new ConnectionStatistics
            {
                TotalConnections = _connections.Count,
                ActiveConnections = activeConnections.Count,
                TotalDataGroups = _groups.Count,
                TotalBytesTransferred = totalBytesTransferred,
                TotalPacketsSent = totalPacketsSent,
                LastUpdated = DateTime.UtcNow,
                AverageConnectionDuration = averageDuration,
                ConnectionsByUserAgent = connectionsByUserAgent,
                ConnectionsByRemoteIP = connectionsByRemoteIP
            };
        }

        /// <summary>
        /// 更新连接心跳
        /// </summary>
        public async Task UpdateHeartbeatAsync(string connectionId)
        {
            if (_connections.TryGetValue(connectionId, out var connection))
            {
                connection.LastHeartbeat = DateTime.UtcNow;
                _logger.LogDebug("更新连接心跳: {ConnectionId}", connectionId);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 清理过期连接
        /// </summary>
        public async Task CleanupExpiredConnectionsAsync(int timeoutMinutes = 30)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-timeoutMinutes);
                var expiredConnections = _connections.Values
                    .Where(c => c.LastHeartbeat < cutoffTime || (!c.IsActive && c.DisconnectedAt < cutoffTime))
                    .ToList();

                foreach (var expiredConnection in expiredConnections)
                {
                    if (_connections.TryRemove(expiredConnection.ConnectionId, out _))
                    {
                        // 从所有组中移除
                        foreach (var group in _groups)
                        {
                            group.Value.Remove(expiredConnection.ConnectionId);
                        }

                        _logger.LogInformation("清理过期连接: {ConnectionId}", expiredConnection.ConnectionId);
                    }
                }

                // 清理空的组
                var emptyGroups = _groups.Where(g => g.Value.Count == 0).Select(g => g.Key).ToList();
                foreach (var emptyGroup in emptyGroups)
                {
                    _groups.TryRemove(emptyGroup, out _);
                }

                if (expiredConnections.Any())
                {
                    _logger.LogInformation("清理了 {Count} 个过期连接", expiredConnections.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理过期连接时发生错误");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取数据组成员
        /// </summary>
        public async Task<IEnumerable<string>> GetGroupMembersAsync(string groupName)
        {
            await Task.CompletedTask;
            return _groups.TryGetValue(groupName, out var members) ? members.ToList() : Enumerable.Empty<string>();
        }

        /// <summary>
        /// 广播消息到所有连接
        /// </summary>
        public async Task BroadcastToAllAsync(string method, object message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync(method, message);
                _logger.LogDebug("广播消息到所有连接: {Method}", method);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "广播消息失败: {Method}", method);
                throw;
            }
        }

        /// <summary>
        /// 广播消息到指定组
        /// </summary>
        public async Task BroadcastToGroupAsync(string groupName, string method, object message)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync(method, message);
                _logger.LogDebug("广播消息到组 {GroupName}: {Method}", groupName, method);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "广播消息到组失败: {GroupName}, {Method}", groupName, method);
                throw;
            }
        }

        /// <summary>
        /// 添加连接到组
        /// </summary>
        public async Task AddToGroupAsync(string connectionId, string groupName)
        {
            try
            {
                await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
                
                _groups.AddOrUpdate(groupName,
                    new HashSet<string> { connectionId },
                    (key, existing) => { existing.Add(connectionId); return existing; });

                _logger.LogDebug("连接 {ConnectionId} 加入组 {GroupName}", connectionId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加连接到组失败: {ConnectionId} -> {GroupName}", connectionId, groupName);
                throw;
            }
        }

        /// <summary>
        /// 从组中移除连接
        /// </summary>
        public async Task RemoveFromGroupAsync(string connectionId, string groupName)
        {
            try
            {
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
                
                if (_groups.TryGetValue(groupName, out var members))
                {
                    members.Remove(connectionId);
                    if (members.Count == 0)
                    {
                        _groups.TryRemove(groupName, out _);
                    }
                }

                _logger.LogDebug("连接 {ConnectionId} 离开组 {GroupName}", connectionId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从组中移除连接失败: {ConnectionId} -> {GroupName}", connectionId, groupName);
                throw;
            }
        }

        /// <summary>
        /// 获取连接的详细信息（包括性能指标）
        /// </summary>
        public async Task<object> GetConnectionDetailsAsync(string connectionId)
        {
            await Task.CompletedTask;

            if (!_connections.TryGetValue(connectionId, out var connection))
                return new { Error = "连接不存在" };

            return new
            {
                ConnectionId = connection.ConnectionId,
                ConnectedAt = connection.ConnectedAt,
                Duration = DateTime.UtcNow - connection.ConnectedAt,
                IsActive = connection.IsActive,
                UserAgent = connection.UserAgent,
                RemoteIpAddress = connection.RemoteIpAddress,
                LastHeartbeat = connection.LastHeartbeat,
                SubscribedGroups = connection.SubscribedGroups.ToArray(),
                DataTransferStats = new
                {
                    PacketsSent = connection.DataTransferStats.PacketsSent,
                    TotalBytesTransferred = connection.DataTransferStats.TotalBytesTransferred,
                    LastTransferTime = connection.DataTransferStats.LastTransferTime,
                    AverageTransferRate = connection.DataTransferStats.AverageTransferRate
                }
            };
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }
    }
}
