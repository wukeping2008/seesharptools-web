using System.IO.Compression;

namespace AIGenSeeSharpSuite.Backend.Services
{
    public class ProjectTemplateService
    {
        private readonly string _templateBasePath;
        private readonly ILogger<ProjectTemplateService> _logger;
        private readonly CodeCleanerService _codeCleaner;

        public ProjectTemplateService(IWebHostEnvironment env, ILogger<ProjectTemplateService> logger, CodeCleanerService codeCleaner)
        {
            _templateBasePath = Path.Combine(env.ContentRootPath, "Templates");
            _logger = logger;
            _codeCleaner = codeCleaner;
        }

        public byte[] CreateSolutionZip(string generatedCode)
        {
            var templatePath = Path.Combine(_templateBasePath, "ConsoleAppTemplate");
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            // Copy solution file
            File.Copy(Path.Combine(templatePath, "Template.sln"), Path.Combine(tempDir, "GeneratedSolution.sln"));

            // Create project directory
            var projectDir = Path.Combine(tempDir, "GeneratedProject");
            Directory.CreateDirectory(projectDir);

            // Copy csproj
            File.Copy(Path.Combine(templatePath, "GeneratedProject", "GeneratedProject.csproj"), Path.Combine(projectDir, "GeneratedProject.csproj"));
            
            // Copy README if exists
            var readmePath = Path.Combine(templatePath, "GeneratedProject", "README.md");
            if (File.Exists(readmePath))
            {
                File.Copy(readmePath, Path.Combine(projectDir, "README.md"));
            }

            // Clean and extract the generated code using the new service
            var cleanedCode = _codeCleaner.CleanCode(generatedCode);
            
            // If the cleaned code is a complete Program.cs, use it directly
            if (cleanedCode.Contains("class Program") && cleanedCode.Contains("static void Main"))
            {
                // It's a complete program, write it directly
                File.WriteAllText(Path.Combine(projectDir, "Program.cs"), cleanedCode);
            }
            else
            {
                // It's just code snippet, insert it into the template
                var programCsTemplate = File.ReadAllText(Path.Combine(templatePath, "GeneratedProject", "Program.cs"));
                var finalProgramCs = programCsTemplate.Replace("// {{AI_GENERATED_CODE}}", cleanedCode);
                File.WriteAllText(Path.Combine(projectDir, "Program.cs"), finalProgramCs);
            }

            // Zip the directory
            var zipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
            ZipFile.CreateFromDirectory(tempDir, zipPath);

            // Clean up temp directory
            Directory.Delete(tempDir, true);

            var zipBytes = File.ReadAllBytes(zipPath);
            File.Delete(zipPath);

            return zipBytes;
        }
        
        /// <summary>
        /// Extracts clean C# code from AI response, removing markdown and extra text
        /// </summary>
        private string ExtractCSharpCode(string aiResponse)
        {
            if (string.IsNullOrWhiteSpace(aiResponse))
                return string.Empty;
            
            _logger.LogInformation("Extracting C# code from AI response...");
            
            // Remove markdown code blocks
            var codeBlockPattern = @"```(?:csharp|cs|c#)?\s*([\s\S]*?)```";
            var match = System.Text.RegularExpressions.Regex.Match(aiResponse, codeBlockPattern);
            
            string cleanedCode;
            if (match.Success && match.Groups.Count > 1)
            {
                cleanedCode = match.Groups[1].Value;
                _logger.LogInformation("Found markdown code block, extracted content");
            }
            else
            {
                cleanedCode = aiResponse;
            }
            
            // Split into lines for processing
            var lines = cleanedCode.Split('\n');
            var codeLines = new List<string>();
            bool inCode = false;
            int braceCount = 0;
            
            foreach (var line in lines)
            {
                var trimmedLine = line.TrimStart();
                
                // Start collecting when we see C# code indicators
                if (!inCode && (trimmedLine.StartsWith("using ") || 
                                trimmedLine.StartsWith("namespace ") ||
                                trimmedLine.StartsWith("public ") ||
                                trimmedLine.StartsWith("class ") ||
                                trimmedLine.StartsWith("//") ||
                                trimmedLine.StartsWith("/*")))
                {
                    inCode = true;
                }
                
                if (inCode)
                {
                    // Count braces to determine when code ends
                    braceCount += line.Count(c => c == '{');
                    braceCount -= line.Count(c => c == '}');
                    
                    // Stop if we encounter non-code patterns after closing all braces
                    if (braceCount == 0 && codeLines.Count > 0)
                    {
                        // Check if this is the last closing brace
                        if (line.Contains("}"))
                        {
                            codeLines.Add(line);
                            // Check next line - if it's not code, stop
                            continue;
                        }
                        
                        // Stop if line looks like explanatory text
                        if (line.Length > 0 && !trimmedLine.StartsWith("//") && 
                            !trimmedLine.StartsWith("using") && !trimmedLine.StartsWith("namespace") &&
                            (line.Contains("This ") || line.Contains("The ") || 
                             line.Contains("program") || line.Contains("code") ||
                             line.Contains("```") || line.StartsWith("Based on")))
                        {
                            break;
                        }
                    }
                    
                    codeLines.Add(line);
                }
            }
            
            if (codeLines.Count > 0)
            {
                cleanedCode = string.Join("\n", codeLines);
            }
            
            // Final cleanup - ensure using statements are at the top
            if (!cleanedCode.StartsWith("using"))
            {
                // Add common using statements if missing
                cleanedCode = "using System;\nusing JYUSB1601;\nusing System.Windows.Forms;\n\n" + cleanedCode;
            }
            
            // Trim and clean
            cleanedCode = cleanedCode.Trim();
            
            // Remove any remaining backticks
            cleanedCode = cleanedCode.Replace("`", "");
            
            _logger.LogInformation($"Extracted {cleanedCode.Length} characters of clean code");
            
            return cleanedCode;
        }
    }
}
