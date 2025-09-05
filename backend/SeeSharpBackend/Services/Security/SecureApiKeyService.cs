using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace SeeSharpBackend.Services.Security
{
    /// <summary>
    /// 安全的API密钥管理服务
    /// 提供密钥加密存储、安全访问和审计功能
    /// </summary>
    public interface ISecureApiKeyService
    {
        Task<string> GetApiKeyAsync(string provider);
        Task<bool> SetApiKeyAsync(string provider, string apiKey);
        Task<bool> ValidateApiKeyAsync(string provider);
        Task<Dictionary<string, bool>> GetApiKeyStatusAsync();
        Task<bool> RemoveApiKeyAsync(string provider);
    }

    public class SecureApiKeyService : ISecureApiKeyService
    {
        private readonly IDataProtector _protector;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SecureApiKeyService> _logger;
        private readonly Dictionary<string, string> _encryptedKeys = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        // 支持的AI提供商
        private readonly HashSet<string> _supportedProviders = new()
        {
            "Claude",
            "DeepSeek",
            "VolcesDeepSeek",
            "Baidu",
            "OpenAI"
        };

        public SecureApiKeyService(
            IDataProtectionProvider dataProtectionProvider,
            IConfiguration configuration,
            ILogger<SecureApiKeyService> logger)
        {
            _protector = dataProtectionProvider.CreateProtector("SeeSharpTools.ApiKeys");
            _configuration = configuration;
            _logger = logger;
            
            // 初始化时加载已有的密钥
            LoadExistingKeys();
        }

        /// <summary>
        /// 获取解密后的API密钥
        /// </summary>
        public async Task<string> GetApiKeyAsync(string provider)
        {
            if (!_supportedProviders.Contains(provider))
            {
                _logger.LogWarning("不支持的API提供商: {Provider}", provider);
                return string.Empty;
            }

            await _semaphore.WaitAsync();
            try
            {
                // 首先尝试从内存缓存获取
                if (_encryptedKeys.TryGetValue(provider, out var encryptedKey))
                {
                    return _protector.Unprotect(encryptedKey);
                }

                // 尝试从配置中加载
                var configKey = _configuration[$"ApiKeys:{provider}"];
                if (!string.IsNullOrEmpty(configKey))
                {
                    // 加密并缓存
                    var encrypted = _protector.Protect(configKey);
                    _encryptedKeys[provider] = encrypted;
                    
                    _logger.LogInformation("成功加载{Provider} API密钥", provider);
                    return configKey;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取{Provider} API密钥失败", provider);
                return string.Empty;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 设置并加密API密钥
        /// </summary>
        public async Task<bool> SetApiKeyAsync(string provider, string apiKey)
        {
            if (!_supportedProviders.Contains(provider))
            {
                _logger.LogWarning("不支持的API提供商: {Provider}", provider);
                return false;
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("API密钥不能为空");
                return false;
            }

            await _semaphore.WaitAsync();
            try
            {
                // 验证密钥格式
                if (!ValidateApiKeyFormat(provider, apiKey))
                {
                    _logger.LogWarning("{Provider} API密钥格式无效", provider);
                    return false;
                }

                // 加密并存储
                var encrypted = _protector.Protect(apiKey);
                _encryptedKeys[provider] = encrypted;

                // 保存到安全存储（实际应用中应使用数据库或密钥管理服务）
                await SaveToSecureStorage(provider, encrypted);

                _logger.LogInformation("成功设置{Provider} API密钥", provider);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置{Provider} API密钥失败", provider);
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 验证API密钥是否有效
        /// </summary>
        public async Task<bool> ValidateApiKeyAsync(string provider)
        {
            var apiKey = await GetApiKeyAsync(provider);
            if (string.IsNullOrEmpty(apiKey))
            {
                return false;
            }

            // 根据不同提供商进行验证
            return provider switch
            {
                "Claude" => await ValidateClaudeKey(apiKey),
                "DeepSeek" => await ValidateDeepSeekKey(apiKey),
                "Baidu" => await ValidateBaiduKey(apiKey),
                _ => !string.IsNullOrEmpty(apiKey)
            };
        }

        /// <summary>
        /// 获取所有API密钥的状态
        /// </summary>
        public async Task<Dictionary<string, bool>> GetApiKeyStatusAsync()
        {
            var status = new Dictionary<string, bool>();
            
            foreach (var provider in _supportedProviders)
            {
                var apiKey = await GetApiKeyAsync(provider);
                status[provider] = !string.IsNullOrEmpty(apiKey);
            }

            return status;
        }

        /// <summary>
        /// 移除API密钥
        /// </summary>
        public async Task<bool> RemoveApiKeyAsync(string provider)
        {
            if (!_supportedProviders.Contains(provider))
            {
                return false;
            }

            await _semaphore.WaitAsync();
            try
            {
                _encryptedKeys.Remove(provider);
                await RemoveFromSecureStorage(provider);
                
                _logger.LogInformation("成功移除{Provider} API密钥", provider);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除{Provider} API密钥失败", provider);
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #region 私有辅助方法

        private void LoadExistingKeys()
        {
            try
            {
                // 从配置加载现有密钥
                foreach (var provider in _supportedProviders)
                {
                    var configKey = _configuration[$"ApiKeys:{provider}"];
                    if (!string.IsNullOrEmpty(configKey))
                    {
                        var encrypted = _protector.Protect(configKey);
                        _encryptedKeys[provider] = encrypted;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载现有API密钥失败");
            }
        }

        private bool ValidateApiKeyFormat(string provider, string apiKey)
        {
            return provider switch
            {
                "Claude" => apiKey.StartsWith("sk-") && apiKey.Length > 40,
                "OpenAI" => apiKey.StartsWith("sk-") && apiKey.Length > 40,
                "DeepSeek" => apiKey.Length > 30,
                _ => !string.IsNullOrWhiteSpace(apiKey)
            };
        }

        private async Task<bool> ValidateClaudeKey(string apiKey)
        {
            // 实际验证逻辑（调用Claude API验证）
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                var response = await httpClient.GetAsync("https://api.anthropic.com/v1/models");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ValidateDeepSeekKey(string apiKey)
        {
            // DeepSeek API验证逻辑
            return await Task.FromResult(!string.IsNullOrEmpty(apiKey));
        }

        private async Task<bool> ValidateBaiduKey(string apiKey)
        {
            // 百度API验证逻辑
            return await Task.FromResult(!string.IsNullOrEmpty(apiKey));
        }

        private async Task SaveToSecureStorage(string provider, string encryptedKey)
        {
            // 实际应用中应保存到数据库或密钥管理服务
            // 这里暂时保存到文件系统（仅用于演示）
            var secureDir = Path.Combine(Directory.GetCurrentDirectory(), "SecureKeys");
            if (!Directory.Exists(secureDir))
            {
                Directory.CreateDirectory(secureDir);
            }

            var filePath = Path.Combine(secureDir, $"{provider}.key");
            await File.WriteAllTextAsync(filePath, encryptedKey);
        }

        private async Task RemoveFromSecureStorage(string provider)
        {
            var secureDir = Path.Combine(Directory.GetCurrentDirectory(), "SecureKeys");
            var filePath = Path.Combine(secureDir, $"{provider}.key");
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            await Task.CompletedTask;
        }

        #endregion
    }

    /// <summary>
    /// API密钥审计记录
    /// </summary>
    public class ApiKeyAuditLog
    {
        public string Provider { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Set, Get, Remove, Validate
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }
}