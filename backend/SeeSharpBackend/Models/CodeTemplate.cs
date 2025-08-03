using System.ComponentModel.DataAnnotations;

namespace SeeSharpBackend.Models
{
    /// <summary>
    /// 代码模板
    /// </summary>
    public class CodeTemplate
    {
        public int Id { get; set; }
        
        /// <summary>
        /// 模板名称
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 模板描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 设备类型
        /// </summary>
        [Required]
        public string DeviceType { get; set; } = string.Empty;
        
        /// <summary>
        /// 测试类型
        /// </summary>
        [Required]
        public string TestType { get; set; } = string.Empty;
        
        /// <summary>
        /// 模板代码
        /// </summary>
        [Required]
        public string TemplateCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 参数定义（JSON格式）
        /// </summary>
        public string ParameterDefinitions { get; set; } = string.Empty;
        
        /// <summary>
        /// 默认参数值（JSON格式）
        /// </summary>
        public string DefaultParameters { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否为系统模板
        /// </summary>
        public bool IsSystemTemplate { get; set; } = false;
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// 使用次数
        /// </summary>
        public int UsageCount { get; set; } = 0;
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 创建者
        /// </summary>
        public string? CreatedBy { get; set; }
        
        /// <summary>
        /// 标签
        /// </summary>
        public string? Tags { get; set; }
        
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Template category
        /// </summary>
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Template keywords for search
        /// </summary>
        public List<string> Keywords { get; set; } = new List<string>();

        /// <summary>
        /// Template difficulty level
        /// </summary>
        [MaxLength(50)]
        public string Difficulty { get; set; } = "初级";

        /// <summary>
        /// Estimated completion time
        /// </summary>
        [MaxLength(50)]
        public string EstimatedTime { get; set; } = "1-2分钟";

        /// <summary>
        /// Whether this is a custom user template
        /// </summary>
        public bool IsCustom { get; set; } = false;
    }
}