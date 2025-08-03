using System.ComponentModel.DataAnnotations;

namespace SeeSharpBackend.Models
{
    /// <summary>
    /// AI API密钥配置
    /// </summary>
    public class ApiKeyConfiguration
    {
        public int Id { get; set; }
        
        /// <summary>
        /// API提供商名称
        /// </summary>
        [Required]
        public string Provider { get; set; } = string.Empty;
        
        /// <summary>
        /// API密钥（加密存储）
        /// </summary>
        [Required]
        public string ApiKey { get; set; } = string.Empty;
        
        /// <summary>
        /// API端点URL
        /// </summary>
        [Required]
        public string ApiUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// 模型名称
        /// </summary>
        public string Model { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// 优先级（数字越小优先级越高）
        /// </summary>
        public int Priority { get; set; } = 0;
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 备注
        /// </summary>
        public string? Notes { get; set; }
    }
    
    /// <summary>
    /// API使用统计
    /// </summary>
    public class ApiUsageStatistics
    {
        public int Id { get; set; }
        
        /// <summary>
        /// API配置ID
        /// </summary>
        public int ApiKeyConfigurationId { get; set; }
        
        /// <summary>
        /// 调用时间
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 请求Token数
        /// </summary>
        public int RequestTokens { get; set; }
        
        /// <summary>
        /// 响应Token数
        /// </summary>
        public int ResponseTokens { get; set; }
        
        /// <summary>
        /// 总Token数
        /// </summary>
        public int TotalTokens { get; set; }
        
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// 响应时间（毫秒）
        /// </summary>
        public long ResponseTimeMs { get; set; }
        
        /// <summary>
        /// 使用场景
        /// </summary>
        public string UseCase { get; set; } = string.Empty;
        
        public virtual ApiKeyConfiguration? ApiKeyConfiguration { get; set; }
    }
}