using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Models;
using SeeSharpBackend.Services;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// API密钥管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly ILogger<ApiKeyController> _logger;

        public ApiKeyController(IApiKeyService apiKeyService, ILogger<ApiKeyController> logger)
        {
            _apiKeyService = apiKeyService;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有API密钥配置
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ApiKeyConfiguration>>> GetAllApiKeys()
        {
            try
            {
                var configs = await _apiKeyService.GetAllApiKeysAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取API密钥配置列表失败");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 获取指定API密钥配置
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiKeyConfiguration>> GetApiKey(int id)
        {
            try
            {
                var config = await _apiKeyService.GetApiKeyAsync(id);
                if (config == null)
                {
                    return NotFound(new { error = "API密钥配置不存在" });
                }
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取API密钥配置失败");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 创建API密钥配置
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiKeyConfiguration>> CreateApiKey([FromBody] ApiKeyConfiguration config)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var created = await _apiKeyService.CreateApiKeyAsync(config);
                return CreatedAtAction(nameof(GetApiKey), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建API密钥配置失败");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 更新API密钥配置
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiKeyConfiguration>> UpdateApiKey(int id, [FromBody] ApiKeyConfiguration config)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updated = await _apiKeyService.UpdateApiKeyAsync(id, config);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新API密钥配置失败");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 删除API密钥配置
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteApiKey(int id)
        {
            try
            {
                await _apiKeyService.DeleteApiKeyAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除API密钥配置失败");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 验证API密钥
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult<object>> ValidateApiKey([FromBody] ValidateApiKeyRequest request)
        {
            try
            {
                var isValid = await _apiKeyService.ValidateApiKeyAsync(request.Provider, request.ApiKey);
                return Ok(new { valid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证API密钥失败");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 获取API使用统计
        /// </summary>
        [HttpGet("{id}/usage")]
        public async Task<ActionResult<List<ApiUsageStatistics>>> GetUsageStatistics(
            int id, 
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var stats = await _apiKeyService.GetUsageStatisticsAsync(id, startDate, endDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取API使用统计失败");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }

        /// <summary>
        /// 获取API使用摘要
        /// </summary>
        [HttpGet("{id}/usage/summary")]
        public async Task<ActionResult<Dictionary<string, object>>> GetUsageSummary(int id)
        {
            try
            {
                var summary = await _apiKeyService.GetUsageSummaryAsync(id);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取API使用摘要失败");
                return StatusCode(500, new { error = "服务器内部错误" });
            }
        }
    }

    /// <summary>
    /// 验证API密钥请求
    /// </summary>
    public class ValidateApiKeyRequest
    {
        public string Provider { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}