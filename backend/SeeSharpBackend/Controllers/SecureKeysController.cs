using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Services.Security;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// 安全的API密钥管理控制器
    /// </summary>
    [ApiController]
    [Route("api/secure-keys")]
    public class SecureKeysController : ControllerBase
    {
        private readonly ISecureApiKeyService _secureApiKeyService;
        private readonly ILogger<SecureKeysController> _logger;

        public SecureKeysController(
            ISecureApiKeyService secureApiKeyService,
            ILogger<SecureKeysController> logger)
        {
            _secureApiKeyService = secureApiKeyService;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有API密钥的配置状态
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetApiKeyStatus()
        {
            try
            {
                var status = await _secureApiKeyService.GetApiKeyStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取API密钥状态失败");
                return StatusCode(500, new { error = "获取状态失败" });
            }
        }

        /// <summary>
        /// 设置API密钥
        /// </summary>
        [HttpPost("set")]
        public async Task<IActionResult> SetApiKey([FromBody] SetApiKeyRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Provider) || string.IsNullOrEmpty(request.ApiKey))
                {
                    return BadRequest(new { success = false, error = "参数不完整" });
                }

                // 记录审计日志（不记录实际密钥）
                _logger.LogInformation("用户尝试设置{Provider}的API密钥", request.Provider);

                var success = await _secureApiKeyService.SetApiKeyAsync(request.Provider, request.ApiKey);
                
                if (success)
                {
                    _logger.LogInformation("成功设置{Provider}的API密钥", request.Provider);
                    return Ok(new { success = true, message = "API密钥设置成功" });
                }
                else
                {
                    return BadRequest(new { success = false, error = "设置失败，请检查密钥格式" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置API密钥失败");
                return StatusCode(500, new { success = false, error = "服务器错误" });
            }
        }

        /// <summary>
        /// 测试API密钥连接
        /// </summary>
        [HttpPost("test")]
        public async Task<IActionResult> TestConnection([FromBody] TestConnectionRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Provider))
                {
                    return BadRequest(new { success = false, error = "请指定提供商" });
                }

                _logger.LogInformation("测试{Provider}的API连接", request.Provider);

                var isValid = await _secureApiKeyService.ValidateApiKeyAsync(request.Provider);
                
                if (isValid)
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = $"{request.Provider} API连接测试成功",
                        details = new
                        {
                            timestamp = DateTime.UtcNow,
                            provider = request.Provider,
                            status = "Connected"
                        }
                    });
                }
                else
                {
                    return Ok(new 
                    { 
                        success = false, 
                        message = $"{request.Provider} API连接测试失败",
                        details = "请检查API密钥是否正确"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试API连接失败");
                return StatusCode(500, new { success = false, error = "测试失败" });
            }
        }

        /// <summary>
        /// 移除API密钥
        /// </summary>
        [HttpDelete("remove/{provider}")]
        public async Task<IActionResult> RemoveApiKey(string provider)
        {
            try
            {
                _logger.LogInformation("尝试移除{Provider}的API密钥", provider);

                var success = await _secureApiKeyService.RemoveApiKeyAsync(provider);
                
                if (success)
                {
                    return Ok(new { success = true, message = "API密钥已移除" });
                }
                else
                {
                    return BadRequest(new { success = false, error = "移除失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除API密钥失败");
                return StatusCode(500, new { success = false, error = "服务器错误" });
            }
        }
    }

    public class SetApiKeyRequest
    {
        public string Provider { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class TestConnectionRequest
    {
        public string Provider { get; set; } = string.Empty;
    }
}