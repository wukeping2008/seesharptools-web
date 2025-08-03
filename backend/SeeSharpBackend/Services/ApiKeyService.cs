using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SeeSharpBackend.Models;
using SeeSharpBackend.Data;

namespace SeeSharpBackend.Services
{
    /// <summary>
    /// API密钥管理服务
    /// </summary>
    public interface IApiKeyService
    {
        Task<ApiKeyConfiguration> CreateApiKeyAsync(ApiKeyConfiguration config);
        Task<ApiKeyConfiguration?> GetApiKeyAsync(int id);
        Task<ApiKeyConfiguration?> GetActiveApiKeyByProviderAsync(string provider);
        Task<List<ApiKeyConfiguration>> GetAllApiKeysAsync();
        Task<ApiKeyConfiguration> UpdateApiKeyAsync(int id, ApiKeyConfiguration config);
        Task DeleteApiKeyAsync(int id);
        Task<bool> ValidateApiKeyAsync(string provider, string apiKey);
        Task RecordUsageAsync(int configId, int requestTokens, int responseTokens, bool success, long responseTimeMs, string useCase, string? errorMessage = null);
        Task<List<ApiUsageStatistics>> GetUsageStatisticsAsync(int configId, DateTime? startDate = null, DateTime? endDate = null);
        Task<Dictionary<string, object>> GetUsageSummaryAsync(int configId);
        string EncryptApiKey(string apiKey);
        string DecryptApiKey(string encryptedApiKey);
    }
    
    public class ApiKeyService : IApiKeyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApiKeyService> _logger;
        private readonly IConfiguration _configuration;
        private readonly byte[] _encryptionKey;
        
        public ApiKeyService(ApplicationDbContext context, ILogger<ApiKeyService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            
            // 从配置或环境变量获取加密密钥，如果没有则生成一个
            var keyString = _configuration["Security:ApiKeyEncryptionKey"] ?? Environment.GetEnvironmentVariable("API_KEY_ENCRYPTION_KEY");
            if (string.IsNullOrEmpty(keyString))
            {
                // 生成默认密钥（生产环境应该从安全存储获取）
                keyString = "SeeSharpTools2025SecureKey123456"; // 32字符
            }
            _encryptionKey = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
        }
        
        public async Task<ApiKeyConfiguration> CreateApiKeyAsync(ApiKeyConfiguration config)
        {
            try
            {
                // 加密API密钥
                config.ApiKey = EncryptApiKey(config.ApiKey);
                config.CreatedAt = DateTime.Now;
                config.UpdatedAt = DateTime.Now;
                
                _context.ApiKeyConfigurations.Add(config);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("创建API密钥配置成功: {Provider}", config.Provider);
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建API密钥配置失败");
                throw;
            }
        }
        
        public async Task<ApiKeyConfiguration?> GetApiKeyAsync(int id)
        {
            var config = await _context.ApiKeyConfigurations.FindAsync(id);
            if (config != null)
            {
                // 解密API密钥
                config.ApiKey = DecryptApiKey(config.ApiKey);
            }
            return config;
        }
        
        public async Task<ApiKeyConfiguration?> GetActiveApiKeyByProviderAsync(string provider)
        {
            var configs = await _context.ApiKeyConfigurations
                .Where(c => c.Provider.ToLower() == provider.ToLower() && c.IsEnabled)
                .OrderBy(c => c.Priority)
                .ToListAsync();
                
            if (!configs.Any()) return null;
            
            // 返回优先级最高的配置
            var config = configs.First();
            config.ApiKey = DecryptApiKey(config.ApiKey);
            return config;
        }
        
        public async Task<List<ApiKeyConfiguration>> GetAllApiKeysAsync()
        {
            var configs = await _context.ApiKeyConfigurations.ToListAsync();
            // 不返回解密的密钥，只返回是否已配置的信息
            foreach (var config in configs)
            {
                config.ApiKey = string.IsNullOrEmpty(config.ApiKey) ? "" : "***已配置***";
            }
            return configs;
        }
        
        public async Task<ApiKeyConfiguration> UpdateApiKeyAsync(int id, ApiKeyConfiguration config)
        {
            try
            {
                var existingConfig = await _context.ApiKeyConfigurations.FindAsync(id);
                if (existingConfig == null)
                {
                    throw new InvalidOperationException("API密钥配置不存在");
                }
                
                existingConfig.Provider = config.Provider;
                existingConfig.ApiUrl = config.ApiUrl;
                existingConfig.Model = config.Model;
                existingConfig.IsEnabled = config.IsEnabled;
                existingConfig.Priority = config.Priority;
                existingConfig.Notes = config.Notes;
                existingConfig.UpdatedAt = DateTime.Now;
                
                // 如果提供了新的API密钥，则更新
                if (!string.IsNullOrEmpty(config.ApiKey) && config.ApiKey != "***已配置***")
                {
                    existingConfig.ApiKey = EncryptApiKey(config.ApiKey);
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("更新API密钥配置成功: {Provider}", config.Provider);
                return existingConfig;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新API密钥配置失败");
                throw;
            }
        }
        
        public async Task DeleteApiKeyAsync(int id)
        {
            try
            {
                var config = await _context.ApiKeyConfigurations.FindAsync(id);
                if (config != null)
                {
                    _context.ApiKeyConfigurations.Remove(config);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("删除API密钥配置成功: {Provider}", config.Provider);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除API密钥配置失败");
                throw;
            }
        }
        
        public async Task<bool> ValidateApiKeyAsync(string provider, string apiKey)
        {
            try
            {
                // 这里可以实现具体的API验证逻辑
                // 例如向各个AI服务发送测试请求
                switch (provider.ToLower())
                {
                    case "deepseek":
                    case "volcesdeepseek":
                        return await ValidateDeepSeekApiAsync(apiKey);
                    case "claude":
                        return await ValidateClaudeApiAsync(apiKey);
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证API密钥失败");
                return false;
            }
        }
        
        public async Task RecordUsageAsync(int configId, int requestTokens, int responseTokens, bool success, long responseTimeMs, string useCase, string? errorMessage = null)
        {
            try
            {
                var usage = new ApiUsageStatistics
                {
                    ApiKeyConfigurationId = configId,
                    RequestTokens = requestTokens,
                    ResponseTokens = responseTokens,
                    TotalTokens = requestTokens + responseTokens,
                    Success = success,
                    ResponseTimeMs = responseTimeMs,
                    UseCase = useCase,
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.Now
                };
                
                _context.ApiUsageStatistics.Add(usage);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录API使用统计失败");
            }
        }
        
        public async Task<List<ApiUsageStatistics>> GetUsageStatisticsAsync(int configId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ApiUsageStatistics.Where(u => u.ApiKeyConfigurationId == configId);
            
            if (startDate.HasValue)
                query = query.Where(u => u.Timestamp >= startDate.Value);
                
            if (endDate.HasValue)
                query = query.Where(u => u.Timestamp <= endDate.Value);
                
            return await query.OrderByDescending(u => u.Timestamp).ToListAsync();
        }
        
        public async Task<Dictionary<string, object>> GetUsageSummaryAsync(int configId)
        {
            var stats = await GetUsageStatisticsAsync(configId);
            
            return new Dictionary<string, object>
            {
                ["TotalRequests"] = stats.Count,
                ["SuccessfulRequests"] = stats.Count(s => s.Success),
                ["FailedRequests"] = stats.Count(s => !s.Success),
                ["TotalTokens"] = stats.Sum(s => s.TotalTokens),
                ["AverageResponseTime"] = stats.Any() ? stats.Average(s => s.ResponseTimeMs) : 0,
                ["SuccessRate"] = stats.Any() ? (double)stats.Count(s => s.Success) / stats.Count * 100 : 0
            };
        }
        
        public string EncryptApiKey(string apiKey)
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.GenerateIV();
            
            var encryptor = aes.CreateEncryptor();
            var encrypted = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(apiKey), 0, apiKey.Length);
            
            var result = new byte[aes.IV.Length + encrypted.Length];
            Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
            Array.Copy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
            
            return Convert.ToBase64String(result);
        }
        
        public string DecryptApiKey(string encryptedApiKey)
        {
            try
            {
                var buffer = Convert.FromBase64String(encryptedApiKey);
                
                using var aes = Aes.Create();
                aes.Key = _encryptionKey;
                
                var iv = new byte[aes.IV.Length];
                var encrypted = new byte[buffer.Length - iv.Length];
                
                Array.Copy(buffer, 0, iv, 0, iv.Length);
                Array.Copy(buffer, iv.Length, encrypted, 0, encrypted.Length);
                
                aes.IV = iv;
                
                var decryptor = aes.CreateDecryptor();
                var decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
                
                return Encoding.UTF8.GetString(decrypted);
            }
            catch
            {
                // 如果解密失败，返回原值（可能是未加密的）
                return encryptedApiKey;
            }
        }
        
        private async Task<bool> ValidateDeepSeekApiAsync(string apiKey)
        {
            // 实现DeepSeek API验证逻辑
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                
                var testRequest = new
                {
                    model = "deepseek-coder",
                    messages = new[] { new { role = "user", content = "Hello" } },
                    max_tokens = 1
                };
                
                var response = await httpClient.PostAsJsonAsync("https://api.deepseek.com/v1/chat/completions", testRequest);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        
        private async Task<bool> ValidateClaudeApiAsync(string apiKey)
        {
            // 实现Claude API验证逻辑
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                
                var testRequest = new
                {
                    model = "claude-3-sonnet-20240229",
                    max_tokens = 1,
                    messages = new[] { new { role = "user", content = "Hello" } }
                };
                
                var response = await httpClient.PostAsJsonAsync("https://api.anthropic.com/v1/messages", testRequest);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}