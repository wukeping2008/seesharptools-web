using System.Threading.Tasks;
using SeeSharpBackend.Services.Hardware;

namespace SeeSharpBackend.Services.MCP
{
    /// <summary>
    /// MCP客户端服务接口
    /// 用于从Web应用调用MCP服务
    /// </summary>
    public interface IMCPClientService
    {
        /// <summary>
        /// 检查MCP服务是否可用
        /// </summary>
        Task<bool> IsMCPAvailableAsync();

        /// <summary>
        /// 启动MCP服务
        /// </summary>
        Task<bool> StartMCPServiceAsync();

        /// <summary>
        /// 停止MCP服务
        /// </summary>
        Task<bool> StopMCPServiceAsync();

        /// <summary>
        /// 通过MCP调用工具
        /// </summary>
        Task<object> CallMCPToolAsync(string toolName, object parameters);

        /// <summary>
        /// 获取MCP服务状态
        /// </summary>
        Task<MCPServiceStatus> GetStatusAsync();
    }

    public class MCPServiceStatus
    {
        public bool IsRunning { get; set; }
        public bool IsConnected { get; set; }
        public string Version { get; set; } = "";
        public string[] AvailableTools { get; set; } = Array.Empty<string>();
    }
}