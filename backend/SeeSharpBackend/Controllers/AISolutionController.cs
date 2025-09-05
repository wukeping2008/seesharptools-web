using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SeeSharpBackend.Hubs;
using SeeSharpBackend.Services.AI;

namespace SeeSharpBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AISolutionController : ControllerBase
    {
        private readonly IBaiduSolutionGeneratorService _solutionService;
        private readonly IHubContext<DataStreamHub> _hubContext;
        private readonly ILogger<AISolutionController> _logger;
        private readonly IConfiguration _configuration;

        public AISolutionController(
            IBaiduSolutionGeneratorService solutionService,
            IHubContext<DataStreamHub> hubContext,
            ILogger<AISolutionController> logger,
            IConfiguration configuration)
        {
            _solutionService = solutionService;
            _hubContext = hubContext;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSolution([FromBody] SolutionGenerationRequest request)
        {
            try
            {
                // 检查功能是否启用
                var isEnabled = _configuration.GetValue<bool>("Features:AICodeGeneration", true);
                if (!isEnabled)
                {
                    return StatusCode(503, new { error = "AI代码生成功能暂时不可用" });
                }

                // 验证请求
                if (!_solutionService.ValidatePrompt(request.Prompt))
                {
                    return BadRequest(new { error = "无效的提示词，请检查内容长度和格式" });
                }

                _logger.LogInformation("开始生成解决方案: {Prompt}", request.Prompt?.Substring(0, Math.Min(50, request.Prompt.Length)));

                // 生成代码
                var code = await _solutionService.GenerateCodeAsync(
                    request.Prompt, 
                    request.Model ?? "ernie-3.5-8k"
                );

                // 根据返回类型决定响应格式
                if (request.OutputFormat == "zip")
                {
                    var zipBytes = await _solutionService.CreateSolutionZipAsync(
                        code, 
                        request.ProjectName ?? "GeneratedSolution"
                    );
                    
                    return File(zipBytes, "application/zip", $"{request.ProjectName ?? "GeneratedSolution"}.zip");
                }
                else
                {
                    return Ok(new SolutionGenerationResponse
                    {
                        Success = true,
                        Code = code,
                        GeneratedAt = DateTime.UtcNow,
                        Model = request.Model ?? "ernie-3.5-8k"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成解决方案失败");
                return StatusCode(500, new { error = "生成失败，请稍后重试" });
            }
        }

        [HttpPost("generate-stream")]
        public async Task<IActionResult> GenerateAndStream([FromBody] StreamGenerationRequest request)
        {
            try
            {
                var isEnabled = _configuration.GetValue<bool>("Features:AICodeGeneration", true);
                if (!isEnabled)
                {
                    return StatusCode(503, new { error = "AI代码生成功能暂时不可用" });
                }

                if (!_solutionService.ValidatePrompt(request.Prompt))
                {
                    return BadRequest(new { error = "无效的提示词" });
                }

                _logger.LogInformation("开始流式生成: ConnectionId={ConnectionId}", request.ConnectionId);

                // 开始异步生成和推送
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // 通知开始生成
                        await _hubContext.Clients.Client(request.ConnectionId)
                            .SendAsync("GenerationStarted", new { sessionId = request.SessionId });

                        // 生成代码
                        var result = await _solutionService.GenerateAndStreamAsync(request.Prompt);

                        // 推送生成的代码
                        await _hubContext.Clients.Client(request.ConnectionId)
                            .SendAsync("CodeGenerated", new 
                            { 
                                sessionId = request.SessionId,
                                code = result.GeneratedCode 
                            });

                        // TODO: 实现编译和执行，推送输出

                        // 通知完成
                        await _hubContext.Clients.Client(request.ConnectionId)
                            .SendAsync("GenerationCompleted", new 
                            { 
                                sessionId = request.SessionId,
                                status = result.Status.ToString() 
                            });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "流式生成过程出错");
                        await _hubContext.Clients.Client(request.ConnectionId)
                            .SendAsync("GenerationError", new 
                            { 
                                sessionId = request.SessionId,
                                error = ex.Message 
                            });
                    }
                });

                return Ok(new { sessionId = request.SessionId, message = "流式生成已启动" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动流式生成失败");
                return StatusCode(500, new { error = "启动失败" });
            }
        }

        [HttpGet("models")]
        public IActionResult GetAvailableModels()
        {
            var models = new[]
            {
                new { id = "ernie-tiny-8k", name = "ERNIE-Tiny (快速)", description = "适合简单任务，响应快速" },
                new { id = "ernie-speed-8k", name = "ERNIE-Speed (均衡)", description = "速度与质量均衡" },
                new { id = "ernie-3.5-8k", name = "ERNIE-3.5 (推荐)", description = "高质量代码生成" },
                new { id = "ernie-4.0-8k", name = "ERNIE-4.0 (高级)", description = "最强能力，适合复杂任务" }
            };

            return Ok(models);
        }

        [HttpGet("templates")]
        public IActionResult GetTemplates()
        {
            var templates = new[]
            {
                new 
                { 
                    id = "console-app",
                    name = "控制台应用",
                    description = "基础的.NET控制台应用程序",
                    prompt = "创建一个简单的控制台应用程序，"
                },
                new 
                { 
                    id = "web-api",
                    name = "Web API",
                    description = "ASP.NET Core Web API项目",
                    prompt = "创建一个RESTful Web API，"
                },
                new 
                { 
                    id = "data-processing",
                    name = "数据处理",
                    description = "数据处理和分析程序",
                    prompt = "创建一个数据处理程序，"
                },
                new 
                { 
                    id = "file-utility",
                    name = "文件工具",
                    description = "文件操作工具程序",
                    prompt = "创建一个文件处理工具，"
                }
            };

            return Ok(templates);
        }

        [HttpGet("history")]
        public IActionResult GetGenerationHistory()
        {
            // TODO: 实现历史记录功能
            return Ok(new[] 
            {
                new 
                {
                    id = "1",
                    prompt = "创建计算器程序",
                    generatedAt = DateTime.UtcNow.AddHours(-1),
                    model = "ernie-3.5-8k",
                    status = "completed"
                }
            });
        }
    }

    public class SolutionGenerationRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? ProjectName { get; set; }
        public string OutputFormat { get; set; } = "json"; // json or zip
    }

    public class StreamGenerationRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
    }

    public class SolutionGenerationResponse
    {
        public bool Success { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public string Model { get; set; } = string.Empty;
    }
}