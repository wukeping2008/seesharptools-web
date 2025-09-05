using AIGenSeeSharpSuite.Backend.Models;
using AIGenSeeSharpSuite.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIGenSeeSharpSuite.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenerationController : ControllerBase
    {
        private readonly BaiduAiService _baiduAiService;
        private readonly ProjectTemplateService _projectTemplateService;
        private readonly KnowledgeService _knowledgeService;
        private readonly PromptEngineeringService _promptEngineeringService;
        private readonly CodeValidationService _codeValidationService;
        private readonly ILogger<GenerationController> _logger;

        public GenerationController(
            BaiduAiService baiduAiService, 
            ProjectTemplateService projectTemplateService, 
            KnowledgeService knowledgeService, 
            PromptEngineeringService promptEngineeringService,
            CodeValidationService codeValidationService,
            ILogger<GenerationController> logger)
        {
            _baiduAiService = baiduAiService;
            _projectTemplateService = projectTemplateService;
            _knowledgeService = knowledgeService;
            _promptEngineeringService = promptEngineeringService;
            _codeValidationService = codeValidationService;
            _logger = logger;
        }

        [HttpPost("generate-solution")]
        public async Task<IActionResult> GenerateSolution([FromBody] GenerationRequest request)
        {
            // Input validation
            if (request == null)
            {
                return BadRequest(new { error = "Request body is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { error = "Prompt cannot be empty" });
            }

            // Limit prompt length to prevent abuse
            if (request.Prompt.Length > 5000)
            {
                return BadRequest(new { error = "Prompt exceeds maximum length of 5000 characters" });
            }

            if (string.IsNullOrWhiteSpace(request.Model))
            {
                return BadRequest(new { error = "Model cannot be empty" });
            }

            // Validate model name against allowed models
            var allowedModels = new[] { "ernie-4.5-turbo-128k", "ernie-x1-turbo-32k", "ernie-speed-128k", "ernie-lite-8k" };
            if (!allowedModels.Contains(request.Model.ToLower()))
            {
                return BadRequest(new { error = $"Invalid model. Allowed models: {string.Join(", ", allowedModels)}" });
            }
            
            // Sanitize input to prevent injection attacks
            request.Prompt = SanitizeInput(request.Prompt);

            try
            {
                // 1. Search for relevant knowledge (limit based on model)
                var modelConfig = ModelConfiguration.GetModelConfig(request.Model);
                var knowledgeSnippets = _knowledgeService.Search(
                    request.Prompt, 
                    topK: modelConfig.MaxKnowledgeSnippets,
                    maxLength: modelConfig.MaxSnippetLength
                );

                // 2. Build the mega prompt with model-specific configuration
                var megaPrompt = _promptEngineeringService.BuildMegaPrompt(request.Prompt, knowledgeSnippets, request.Model);

                // 3. Generate code
                var generatedCode = await _baiduAiService.GenerateCodeAsync(megaPrompt, request.Model);
                
                if (string.IsNullOrWhiteSpace(generatedCode))
                {
                    return StatusCode(500, new { error = "Failed to generate code. Please try again." });
                }
                
                // 4. Validate generated code
                _logger.LogInformation("Validating generated code...");
                var validationResult = _codeValidationService.ValidateCode(generatedCode);
                
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning($"Generated code has {validationResult.Errors.Count} errors");
                    // Still proceed but log the issues
                    foreach (var error in validationResult.Errors)
                    {
                        _logger.LogWarning($"Code error at line {error.Line}: {error.Message}");
                    }
                }
                
                // 5. Format the code
                generatedCode = _codeValidationService.FormatCode(generatedCode);

                // 6. Create solution zip
                var zipBytes = _projectTemplateService.CreateSolutionZip(generatedCode);

                return File(zipBytes, "application/zip", $"{request.Model}-GeneratedSolution.zip");
            }
            catch (HttpRequestException httpEx)
            {
                // Log the error (logger should be injected)
                return StatusCode(503, new { error = "AI service is temporarily unavailable", details = httpEx.Message });
            }
            catch (InvalidOperationException opEx)
            {
                return StatusCode(500, new { error = "Configuration error", details = opEx.Message });
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, new { error = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpPost("generate-and-stream")]
        public IActionResult GenerateAndStream([FromBody] GenerationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest("Prompt cannot be empty.");
            }

            // Logic for generating, compiling, and running the streaming code will be implemented here.
            // This will involve the StreamingAppTemplate and SignalR broadcasting.
            
            return Ok(new { Message = "Streaming started." });
        }
        
        /// <summary>
        /// Sanitizes user input to prevent injection attacks
        /// </summary>
        private string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            
            // Remove any potential script tags or HTML
            input = System.Text.RegularExpressions.Regex.Replace(input, @"<[^>]*>", string.Empty);
            
            // Remove any potential SQL injection patterns
            input = input.Replace("'", "''");
            input = input.Replace("--", "");
            input = input.Replace("/*", "");
            input = input.Replace("*/", "");
            input = input.Replace("xp_", "");
            input = input.Replace("sp_", "");
            
            // Limit consecutive whitespace
            input = System.Text.RegularExpressions.Regex.Replace(input, @"\s+", " ");
            
            return input.Trim();
        }
    }
}
