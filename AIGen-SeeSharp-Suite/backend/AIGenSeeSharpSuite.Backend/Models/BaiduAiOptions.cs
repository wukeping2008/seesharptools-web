namespace AIGenSeeSharpSuite.Backend.Models
{
    /// <summary>
    /// Configuration options for Baidu AI service
    /// </summary>
    public class BaiduAiOptions
    {
        public const string SectionName = "BaiduAI";
        
        /// <summary>
        /// Bearer token for API authentication
        /// Can be overridden by BAIDU_AI_BEARER_TOKEN environment variable
        /// </summary>
        public string BearerToken { get; set; } = string.Empty;
        
        /// <summary>
        /// API endpoint URL
        /// </summary>
        public string ApiUrl { get; set; } = "https://qianfan.baidubce.com/v2/chat/completions";
        
        /// <summary>
        /// Default model to use if not specified
        /// </summary>
        public string DefaultModel { get; set; } = "ernie-4.5-turbo-128k";
        
        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Max retries for failed requests
        /// </summary>
        public int MaxRetries { get; set; } = 3;
        
        /// <summary>
        /// Enable request/response logging (be careful with sensitive data)
        /// </summary>
        public bool EnableLogging { get; set; } = false;
    }
}