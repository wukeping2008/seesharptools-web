using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SeeSharpBackend.Services.Security;

namespace SeeSharpBackend.Services.AI
{
    public interface IBaiduSolutionGeneratorService
    {
        Task<string> GenerateCodeAsync(string prompt, string model = "ernie-3.5-8k");
        Task<byte[]> CreateSolutionZipAsync(string generatedCode, string projectName = "GeneratedSolution");
        Task<StreamingGenerationResult> GenerateAndStreamAsync(string prompt);
        bool ValidatePrompt(string prompt);
    }

    public class BaiduSolutionGeneratorService : IBaiduSolutionGeneratorService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BaiduSolutionGeneratorService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISecureApiKeyService _secureApiKeyService;
        
        private string? _accessToken;
        private DateTime _tokenExpiry = DateTime.MinValue;
        private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

        private readonly Dictionary<string, string> _modelEndpoints = new()
        {
            ["ernie-tiny-8k"] = "ernie-tiny-8k",
            ["ernie-speed-8k"] = "ernie-speed-8k",
            ["ernie-3.5-8k"] = "completions",
            ["ernie-4.0-8k"] = "completions_pro"
        };

        public BaiduSolutionGeneratorService(
            IConfiguration configuration,
            ILogger<BaiduSolutionGeneratorService> logger,
            IHttpClientFactory httpClientFactory,
            ISecureApiKeyService secureApiKeyService)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _secureApiKeyService = secureApiKeyService;
        }

        public async Task<string> GenerateCodeAsync(string prompt, string model = "ernie-3.5-8k")
        {
            try
            {
                _logger.LogInformation("开始生成C#解决方案，模型: {Model}", model);

                var enhancedPrompt = BuildSolutionPrompt(prompt);
                var code = await CallBaiduAPIAsync(model, enhancedPrompt);

                if (string.IsNullOrWhiteSpace(code))
                {
                    throw new InvalidOperationException("生成的代码为空");
                }

                code = ExtractCodeFromResponse(code);
                _logger.LogInformation("成功生成代码，长度: {Length} 字符", code.Length);

                return code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成代码失败");
                throw;
            }
        }

        public async Task<byte[]> CreateSolutionZipAsync(string generatedCode, string projectName = "GeneratedSolution")
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    // 创建解决方案文件
                    var slnEntry = archive.CreateEntry($"{projectName}.sln");
                    using (var slnStream = slnEntry.Open())
                    using (var writer = new StreamWriter(slnStream))
                    {
                        await writer.WriteAsync(GenerateSolutionFile(projectName));
                    }

                    // 创建项目文件夹
                    var projectFolder = $"{projectName}/";
                    
                    // 创建项目文件
                    var csprojEntry = archive.CreateEntry($"{projectFolder}{projectName}.csproj");
                    using (var csprojStream = csprojEntry.Open())
                    using (var writer = new StreamWriter(csprojStream))
                    {
                        await writer.WriteAsync(GenerateProjectFile());
                    }

                    // 创建Program.cs
                    var programEntry = archive.CreateEntry($"{projectFolder}Program.cs");
                    using (var programStream = programEntry.Open())
                    using (var writer = new StreamWriter(programStream))
                    {
                        await writer.WriteAsync(generatedCode);
                    }

                    // 创建README.md
                    var readmeEntry = archive.CreateEntry($"{projectFolder}README.md");
                    using (var readmeStream = readmeEntry.Open())
                    using (var writer = new StreamWriter(readmeStream))
                    {
                        await writer.WriteAsync(GenerateReadme(projectName));
                    }
                }

                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建解决方案ZIP文件失败");
                throw;
            }
        }

        public async Task<StreamingGenerationResult> GenerateAndStreamAsync(string prompt)
        {
            try
            {
                _logger.LogInformation("开始流式生成代码");

                var result = new StreamingGenerationResult
                {
                    SessionId = Guid.NewGuid().ToString(),
                    StartTime = DateTime.UtcNow,
                    Status = StreamingStatus.Generating
                };

                // 生成代码
                var code = await GenerateCodeAsync(prompt, "ernie-speed-8k");
                result.GeneratedCode = code;

                // TODO: 实现实时编译和执行
                // 这里需要集成编译服务和SignalR推送

                result.Status = StreamingStatus.Completed;
                result.EndTime = DateTime.UtcNow;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "流式生成失败");
                return new StreamingGenerationResult
                {
                    Status = StreamingStatus.Failed,
                    Error = ex.Message
                };
            }
        }

        public bool ValidatePrompt(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return false;

            if (prompt.Length < 10 || prompt.Length > 2000)
                return false;

            // 检查是否包含恶意内容
            var blacklist = new[] { "hack", "virus", "malware", "exploit" };
            var lowerPrompt = prompt.ToLower();
            
            return !blacklist.Any(word => lowerPrompt.Contains(word));
        }

        private string BuildSolutionPrompt(string userPrompt)
        {
            return $@"你是一个专业的C#开发专家。请根据以下需求生成一个完整的C#控制台应用程序代码。

需求：{userPrompt}

要求：
1. 生成完整的、可直接运行的C#代码
2. 使用.NET 8.0的最新特性
3. 包含适当的错误处理
4. 添加必要的注释说明
5. 遵循C#编码规范
6. 如果需要外部库，使用NuGet包管理
7. 代码结构清晰，易于理解和维护

请直接返回Program.cs的完整代码，不需要其他解释。";
        }

        private string ExtractCodeFromResponse(string response)
        {
            // 移除markdown代码块标记
            response = response.Replace("```csharp", "").Replace("```c#", "").Replace("```", "");
            
            // 移除可能的解释文字（通常在代码前后）
            var lines = response.Split('\n');
            var codeLines = new List<string>();
            bool inCode = false;

            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("using ") || line.TrimStart().StartsWith("namespace "))
                {
                    inCode = true;
                }

                if (inCode)
                {
                    codeLines.Add(line);
                }
            }

            return codeLines.Count > 0 ? string.Join('\n', codeLines) : response.Trim();
        }

        private async Task<string> CallBaiduAPIAsync(string model, string prompt)
        {
            try
            {
                await EnsureAccessTokenAsync();
                
                var httpClient = _httpClientFactory.CreateClient();
                var endpoint = _modelEndpoints.GetValueOrDefault(model, "completions");
                var url = $"https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/{endpoint}?access_token={_accessToken}";
                
                var requestBody = new
                {
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.3,
                    top_p = 0.8,
                    penalty_score = 1.0
                };
                
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("百度AI API调用失败: {StatusCode}, {Content}", 
                        response.StatusCode, responseContent);
                    throw new Exception($"API调用失败: {response.StatusCode}");
                }
                
                using var doc = JsonDocument.Parse(responseContent);
                if (doc.RootElement.TryGetProperty("result", out var result))
                {
                    return result.GetString() ?? string.Empty;
                }
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用百度AI API失败");
                throw;
            }
        }

        private async Task EnsureAccessTokenAsync()
        {
            await _tokenSemaphore.WaitAsync();
            try
            {
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
                {
                    return;
                }
                
                var apiKey = await _secureApiKeyService.GetApiKeyAsync("Baidu");
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("百度AI API密钥未配置");
                }
                
                var keys = apiKey.Split(':');
                if (keys.Length != 2)
                {
                    throw new InvalidOperationException("百度AI API密钥格式错误");
                }
                
                var httpClient = _httpClientFactory.CreateClient();
                var tokenUrl = $"https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id={keys[0]}&client_secret={keys[1]}";
                
                var response = await httpClient.PostAsync(tokenUrl, null);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                using var doc = JsonDocument.Parse(responseContent);
                if (doc.RootElement.TryGetProperty("access_token", out var token))
                {
                    _accessToken = token.GetString();
                    var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
                    _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 300);
                }
                else
                {
                    throw new Exception("获取百度AI访问令牌失败");
                }
            }
            finally
            {
                _tokenSemaphore.Release();
            }
        }

        private string GenerateSolutionFile(string projectName)
        {
            return $@"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
MinimumVisualStudioVersion = 10.0.40219.1
Project(""{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}"") = ""{projectName}"", ""{projectName}\{projectName}.csproj"", ""{{8A2F4B9E-67B7-4EDB-9C91-5F2D3C8B9A10}}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{{8A2F4B9E-67B7-4EDB-9C91-5F2D3C8B9A10}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{{8A2F4B9E-67B7-4EDB-9C91-5F2D3C8B9A10}}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{{8A2F4B9E-67B7-4EDB-9C91-5F2D3C8B9A10}}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{{8A2F4B9E-67B7-4EDB-9C91-5F2D3C8B9A10}}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal";
        }

        private string GenerateProjectFile()
        {
            return @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>";
        }

        private string GenerateReadme(string projectName)
        {
            return $@"# {projectName}

此项目由 SeeSharpTools AI 代码生成器自动生成。

## 运行要求
- .NET 8.0 SDK

## 如何运行
1. 打开终端并导航到项目目录
2. 运行 `dotnet restore` 安装依赖
3. 运行 `dotnet run` 启动程序

## 生成信息
- 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
- 生成服务: SeeSharpTools AI Solution Generator
- AI模型: 百度文心一言

## 注意事项
生成的代码可能需要根据实际需求进行调整和优化。
";
        }
    }

    public class StreamingGenerationResult
    {
        public string SessionId { get; set; } = string.Empty;
        public string GeneratedCode { get; set; } = string.Empty;
        public StreamingStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Error { get; set; }
        public List<string> OutputLines { get; set; } = new();
    }

    public enum StreamingStatus
    {
        Generating,
        Compiling,
        Running,
        Completed,
        Failed
    }
}