using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Services.CSharpRunner;
using System.ComponentModel.DataAnnotations;

namespace SeeSharpBackend.Controllers;

/// <summary>
/// C# Runner 控制器
/// 提供在线 C# 代码执行功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CSharpRunnerController : ControllerBase
{
    private readonly ICSharpRunnerService _csharpRunnerService;
    private readonly ILogger<CSharpRunnerController> _logger;

    public CSharpRunnerController(
        ICSharpRunnerService csharpRunnerService,
        ILogger<CSharpRunnerController> logger)
    {
        _csharpRunnerService = csharpRunnerService;
        _logger = logger;
    }

    /// <summary>
    /// 执行 C# 代码
    /// </summary>
    /// <param name="request">执行请求</param>
    /// <returns>执行结果</returns>
    [HttpPost("execute")]
    public async Task<ActionResult<CSharpExecutionResult>> ExecuteCode([FromBody] ExecuteCodeRequest request)
    {
        try
        {
            _logger.LogInformation("收到代码执行请求，代码长度: {Length}", request.Code?.Length ?? 0);

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(new { error = "代码不能为空" });
            }

            var result = await _csharpRunnerService.ExecuteCodeAsync(request.Code, request.Timeout);
            
            _logger.LogInformation("代码执行完成，成功: {Success}, 耗时: {Elapsed}ms", 
                result.Success, result.ElapsedMilliseconds);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行代码时发生异常");
            return StatusCode(500, new { error = "服务器内部错误", message = ex.Message });
        }
    }

    /// <summary>
    /// 执行仪器控制代码
    /// </summary>
    /// <param name="request">仪器控制请求</param>
    /// <returns>执行结果</returns>
    [HttpPost("execute-instrument")]
    public async Task<ActionResult<CSharpExecutionResult>> ExecuteInstrumentCode([FromBody] ExecuteInstrumentCodeRequest request)
    {
        try
        {
            _logger.LogInformation("收到仪器控制代码执行请求，设备类型: {DeviceType}", request.DeviceType);

            if (string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest(new { error = "代码不能为空" });
            }

            if (string.IsNullOrWhiteSpace(request.DeviceType))
            {
                return BadRequest(new { error = "设备类型不能为空" });
            }

            var result = await _csharpRunnerService.ExecuteInstrumentCodeAsync(
                request.Code, 
                request.DeviceType, 
                request.Parameters ?? new Dictionary<string, object>());

            _logger.LogInformation("仪器控制代码执行完成，成功: {Success}, 设备: {DeviceType}", 
                result.Success, request.DeviceType);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行仪器控制代码时发生异常");
            return StatusCode(500, new { error = "服务器内部错误", message = ex.Message });
        }
    }

    /// <summary>
    /// 获取代码模板
    /// </summary>
    /// <param name="deviceType">设备类型</param>
    /// <param name="templateType">模板类型</param>
    /// <returns>代码模板</returns>
    [HttpGet("template/{deviceType}/{templateType}")]
    public async Task<ActionResult<CodeTemplateResponse>> GetCodeTemplate(
        [FromRoute] string deviceType, 
        [FromRoute] string templateType = "basic")
    {
        try
        {
            _logger.LogInformation("获取代码模板，设备: {DeviceType}, 类型: {TemplateType}", 
                deviceType, templateType);

            var template = await _csharpRunnerService.GetCodeTemplateAsync(deviceType, templateType);

            return Ok(new CodeTemplateResponse
            {
                DeviceType = deviceType,
                TemplateType = templateType,
                Code = template,
                Description = GetTemplateDescription(deviceType, templateType)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取代码模板时发生异常");
            return StatusCode(500, new { error = "服务器内部错误", message = ex.Message });
        }
    }

    /// <summary>
    /// 获取支持的设备类型列表
    /// </summary>
    /// <returns>设备类型列表</returns>
    [HttpGet("devices")]
    public ActionResult<List<DeviceTypeInfo>> GetSupportedDevices()
    {
        var devices = new List<DeviceTypeInfo>
        {
            new() 
            { 
                Type = "JY5500", 
                Name = "JY5500 PXI 数据采集卡", 
                Description = "简仪科技 JY5500 系列 PXI 数据采集卡",
                Templates = ["basic", "triggered"]
            },
            new() 
            { 
                Type = "JYUSB1601", 
                Name = "JYUSB1601 USB 数据采集卡", 
                Description = "简仪科技 JYUSB1601 USB 数据采集卡",
                Templates = ["basic", "continuous", "finite"]
            },
            new() 
            { 
                Type = "Simulated", 
                Name = "模拟设备", 
                Description = "用于测试的模拟数据采集设备",
                Templates = ["basic"]
            }
        };

        return Ok(devices);
    }

    /// <summary>
    /// 检查 C# Runner 服务状态
    /// </summary>
    /// <returns>服务状态</returns>
    [HttpGet("status")]
    public async Task<ActionResult<ServiceStatusResponse>> GetServiceStatus()
    {
        try
        {
            var isAvailable = await _csharpRunnerService.IsServiceAvailableAsync();
            
            return Ok(new ServiceStatusResponse
            {
                IsAvailable = isAvailable,
                ServiceUrl = "http://localhost:5050", // 从配置获取
                CheckTime = DateTime.Now,
                Message = isAvailable ? "服务正常" : "服务不可用"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查服务状态时发生异常");
            return Ok(new ServiceStatusResponse
            {
                IsAvailable = false,
                ServiceUrl = "http://localhost:5050",
                CheckTime = DateTime.Now,
                Message = $"服务检查失败: {ex.Message}"
            });
        }
    }

    #region 私有方法

    /// <summary>
    /// 获取模板描述
    /// </summary>
    private string GetTemplateDescription(string deviceType, string templateType)
    {
        return (deviceType.ToUpper(), templateType.ToLower()) switch
        {
            ("JY5500", "basic") => "JY5500 基础数据采集示例",
            ("JY5500", "triggered") => "JY5500 触发采集示例",
            ("JYUSB1601", "basic") => "JYUSB1601 单点采集示例",
            ("JYUSB1601", "continuous") => "JYUSB1601 连续多通道采集示例",
            ("JYUSB1601", "finite") => "JYUSB1601 有限采样示例",
            ("SIMULATED", "basic") => "模拟设备数据生成示例",
            _ => "通用代码模板"
        };
    }

    #endregion
}

#region 请求/响应数据模型

/// <summary>
/// 执行代码请求
/// </summary>
public class ExecuteCodeRequest
{
    /// <summary>
    /// 要执行的 C# 代码
    /// </summary>
    [Required]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 超时时间（毫秒），默认30秒
    /// </summary>
    public int Timeout { get; set; } = 30000;
}

/// <summary>
/// 执行仪器控制代码请求
/// </summary>
public class ExecuteInstrumentCodeRequest
{
    /// <summary>
    /// 要执行的 C# 代码
    /// </summary>
    [Required]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 设备类型
    /// </summary>
    [Required]
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 设备参数
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }

    /// <summary>
    /// 超时时间（毫秒），默认30秒
    /// </summary>
    public int Timeout { get; set; } = 30000;
}

/// <summary>
/// 代码模板响应
/// </summary>
public class CodeTemplateResponse
{
    /// <summary>
    /// 设备类型
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 模板类型
    /// </summary>
    public string TemplateType { get; set; } = string.Empty;

    /// <summary>
    /// 代码内容
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 模板描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 设备类型信息
/// </summary>
public class DeviceTypeInfo
{
    /// <summary>
    /// 设备类型标识
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 设备描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 支持的模板类型
    /// </summary>
    public List<string> Templates { get; set; } = new();
}

/// <summary>
/// 服务状态响应
/// </summary>
public class ServiceStatusResponse
{
    /// <summary>
    /// 服务是否可用
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// 服务 URL
    /// </summary>
    public string ServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// 检查时间
    /// </summary>
    public DateTime CheckTime { get; set; }

    /// <summary>
    /// 状态消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

#endregion
