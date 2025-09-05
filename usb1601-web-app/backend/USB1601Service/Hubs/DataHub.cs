using Microsoft.AspNetCore.SignalR;
using USB1601Service.Services;

namespace USB1601Service.Hubs
{
    /// <summary>
    /// SignalR数据传输Hub
    /// </summary>
    public class DataHub : Hub
    {
        private readonly ILogger<DataHub> _logger;
        private readonly USB1601Manager _usb1601Manager;

        public DataHub(ILogger<DataHub> logger, USB1601Manager usb1601Manager)
        {
            _logger = logger;
            _usb1601Manager = usb1601Manager;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"客户端连接: {Context.ConnectionId}");
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"客户端断开: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 开始数据流
        /// </summary>
        public async Task StartDataStream(int channelCount, double sampleRate)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "DataReceivers");
                _logger.LogInformation($"客户端 {Context.ConnectionId} 加入数据接收组");
                
                // 通知客户端流已启动
                await Clients.Caller.SendAsync("StreamStarted", new
                {
                    ChannelCount = channelCount,
                    SampleRate = sampleRate,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动数据流失败");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        /// <summary>
        /// 停止数据流
        /// </summary>
        public async Task StopDataStream()
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "DataReceivers");
                _logger.LogInformation($"客户端 {Context.ConnectionId} 离开数据接收组");
                
                await Clients.Caller.SendAsync("StreamStopped", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止数据流失败");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        public async Task SendCommand(string command, object parameters)
        {
            try
            {
                _logger.LogInformation($"收到命令: {command}");
                
                switch (command.ToLower())
                {
                    case "getStatus":
                        await Clients.Caller.SendAsync("Status", new
                        {
                            IsConnected = true,
                            IsAcquiring = false,
                            Timestamp = DateTime.Now
                        });
                        break;
                        
                    case "ping":
                        await Clients.Caller.SendAsync("Pong", DateTime.Now);
                        break;
                        
                    default:
                        await Clients.Caller.SendAsync("UnknownCommand", command);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理命令失败: {command}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }
    }
}