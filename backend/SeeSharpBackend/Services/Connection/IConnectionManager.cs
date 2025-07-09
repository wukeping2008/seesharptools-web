using SeeSharpBackend.Hubs;

namespace SeeSharpBackend.Services.Connection
{
    /// <summary>
    /// 连接管理服务接口
    /// 负责管理SignalR连接的生命周期和状态
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// 注册新连接
        /// </summary>
        /// <param name="connectionInfo">连接信息</param>
        Task RegisterConnectionAsync(Hubs.ConnectionInfo connectionInfo);

        /// <summary>
        /// 注销连接
        /// </summary>
        /// <param name="connectionInfo">连接信息</param>
        Task UnregisterConnectionAsync(Hubs.ConnectionInfo connectionInfo);

        /// <summary>
        /// 获取连接信息
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>连接信息</returns>
        Task<Hubs.ConnectionInfo?> GetConnectionAsync(string connectionId);

        /// <summary>
        /// 获取所有活跃连接
        /// </summary>
        /// <returns>活跃连接列表</returns>
        Task<IEnumerable<Hubs.ConnectionInfo>> GetActiveConnectionsAsync();

        /// <summary>
        /// 获取连接统计信息
        /// </summary>
        /// <returns>连接统计</returns>
        Task<ConnectionStatistics> GetConnectionStatisticsAsync();

        /// <summary>
        /// 更新连接心跳
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        Task UpdateHeartbeatAsync(string connectionId);

        /// <summary>
        /// 清理过期连接
        /// </summary>
        /// <param name="timeoutMinutes">超时时间（分钟）</param>
        Task CleanupExpiredConnectionsAsync(int timeoutMinutes = 30);

        /// <summary>
        /// 获取数据组成员
        /// </summary>
        /// <param name="groupName">组名</param>
        /// <returns>组成员连接ID列表</returns>
        Task<IEnumerable<string>> GetGroupMembersAsync(string groupName);

        /// <summary>
        /// 广播消息到所有连接
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="message">消息内容</param>
        Task BroadcastToAllAsync(string method, object message);

        /// <summary>
        /// 广播消息到指定组
        /// </summary>
        /// <param name="groupName">组名</param>
        /// <param name="method">方法名</param>
        /// <param name="message">消息内容</param>
        Task BroadcastToGroupAsync(string groupName, string method, object message);
    }

    /// <summary>
    /// 连接统计信息
    /// </summary>
    public class ConnectionStatistics
    {
        public int TotalConnections { get; set; }
        public int ActiveConnections { get; set; }
        public int TotalDataGroups { get; set; }
        public long TotalBytesTransferred { get; set; }
        public long TotalPacketsSent { get; set; }
        public DateTime LastUpdated { get; set; }
        public TimeSpan AverageConnectionDuration { get; set; }
        public Dictionary<string, int> ConnectionsByUserAgent { get; set; } = new();
        public Dictionary<string, int> ConnectionsByRemoteIP { get; set; } = new();
    }
}
