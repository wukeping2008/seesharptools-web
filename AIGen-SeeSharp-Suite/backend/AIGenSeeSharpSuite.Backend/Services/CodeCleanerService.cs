using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AIGenSeeSharpSuite.Backend.Services
{
    public class CodeCleanerService
    {
        private readonly ILogger<CodeCleanerService> _logger;
        
        // Common patterns that indicate non-code text
        private readonly string[] _nonCodeIndicators = 
        {
            "This code", "The code", "This program", "The program",
            "Here's", "Here is", "Based on", "The above", "Below is",
            "generates", "creates", "implements", "demonstrates",
            "```", "* * *", "---", "===",
            "Note:", "Important:", "Usage:", "Example:"
        };
        
        // Required using statements for USB-1601 projects
        private readonly string[] _requiredUsings = 
        {
            "using System;",
            "using JYUSB1601;"
        };
        
        public CodeCleanerService(ILogger<CodeCleanerService> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Cleans and extracts valid C# code from AI response
        /// </summary>
        public string CleanCode(string aiResponse)
        {
            if (string.IsNullOrWhiteSpace(aiResponse))
                return GenerateDefaultTemplate();
            
            try
            {
                // Step 1: Extract code from markdown blocks if present
                var code = ExtractFromMarkdown(aiResponse);
                
                // Step 2: Remove non-code text
                code = RemoveNonCodeText(code);
                
                // Step 3: Fix common formatting issues
                code = FixFormatting(code);
                
                // Step 4: Ensure required structure
                code = EnsureProperStructure(code);
                
                // Step 5: Validate and fix syntax
                code = ValidateAndFixSyntax(code);
                
                _logger.LogInformation($"Successfully cleaned code: {code.Length} characters");
                return code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning code, returning default template");
                return GenerateDefaultTemplate();
            }
        }
        
        private string ExtractFromMarkdown(string text)
        {
            // Try to extract from markdown code blocks
            var markdownPattern = @"```(?:csharp|cs|c#)?\s*([\s\S]*?)```";
            var match = Regex.Match(text, markdownPattern);
            
            if (match.Success && match.Groups.Count > 1)
            {
                return match.Groups[1].Value;
            }
            
            return text;
        }
        
        private string RemoveNonCodeText(string code)
        {
            var lines = code.Split('\n').ToList();
            var cleanLines = new List<string>();
            var inCode = false;
            var braceCount = 0;
            var lastValidCodeIndex = -1;
            
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var trimmedLine = line.Trim();
                
                // Check if this line starts code
                if (!inCode && IsCodeStart(trimmedLine))
                {
                    inCode = true;
                }
                
                if (inCode)
                {
                    // Track brace depth
                    braceCount += line.Count(c => c == '{');
                    braceCount -= line.Count(c => c == '}');
                    
                    // Check if this line is non-code text
                    if (IsNonCodeText(trimmedLine) && braceCount == 0)
                    {
                        // If we've closed all braces and hit non-code text, stop
                        if (lastValidCodeIndex > 0)
                            break;
                    }
                    else
                    {
                        cleanLines.Add(line);
                        if (!string.IsNullOrWhiteSpace(trimmedLine))
                            lastValidCodeIndex = cleanLines.Count - 1;
                    }
                }
            }
            
            // Trim to last valid code line
            if (lastValidCodeIndex >= 0 && lastValidCodeIndex < cleanLines.Count - 1)
            {
                cleanLines = cleanLines.Take(lastValidCodeIndex + 1).ToList();
            }
            
            return string.Join("\n", cleanLines);
        }
        
        private bool IsCodeStart(string line)
        {
            return line.StartsWith("using ") ||
                   line.StartsWith("namespace ") ||
                   line.StartsWith("public ") ||
                   line.StartsWith("class ") ||
                   line.StartsWith("//") ||
                   line.StartsWith("/*");
        }
        
        private bool IsNonCodeText(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;
            
            // Check if line contains non-code indicators
            return _nonCodeIndicators.Any(indicator => 
                line.Contains(indicator, StringComparison.OrdinalIgnoreCase));
        }
        
        private string FixFormatting(string code)
        {
            // Fix common formatting issues
            var lines = code.Split('\n').ToList();
            var fixedLines = new List<string>();
            
            foreach (var line in lines)
            {
                var fixedLine = line;
                
                // Remove excessive spaces
                fixedLine = Regex.Replace(fixedLine, @"\s+", " ");
                
                // Fix spacing around punctuation
                fixedLine = Regex.Replace(fixedLine, @"\s*\(\s*", "(");
                fixedLine = Regex.Replace(fixedLine, @"\s*\)\s*", ")");
                fixedLine = Regex.Replace(fixedLine, @"\s*;\s*", ";");
                
                // Don't add lines that are just whitespace
                if (!string.IsNullOrWhiteSpace(line) || fixedLines.Count == 0 || !string.IsNullOrWhiteSpace(fixedLines.Last()))
                {
                    fixedLines.Add(line); // Preserve original indentation
                }
            }
            
            return string.Join("\n", fixedLines);
        }
        
        private string EnsureProperStructure(string code)
        {
            var lines = code.Split('\n').ToList();
            var hasUsings = lines.Any(l => l.Trim().StartsWith("using "));
            var hasNamespace = lines.Any(l => l.Trim().StartsWith("namespace "));
            var hasClass = lines.Any(l => l.Contains("class "));
            var hasMain = lines.Any(l => l.Contains("Main("));
            
            // If missing using statements, add them
            if (!hasUsings)
            {
                lines.InsertRange(0, _requiredUsings);
                lines.Insert(_requiredUsings.Length, "");
            }
            else
            {
                // Ensure required usings are present
                foreach (var required in _requiredUsings)
                {
                    if (!lines.Any(l => l.Trim().Equals(required, StringComparison.OrdinalIgnoreCase)))
                    {
                        // Find where to insert (after other usings)
                        var lastUsingIndex = lines.FindLastIndex(l => l.Trim().StartsWith("using "));
                        lines.Insert(lastUsingIndex + 1, required);
                    }
                }
            }
            
            // If missing namespace, wrap the code
            if (!hasNamespace && hasClass)
            {
                var firstClassIndex = lines.FindIndex(l => l.Contains("class "));
                if (firstClassIndex > 0)
                {
                    lines.Insert(firstClassIndex, "namespace GeneratedProject");
                    lines.Insert(firstClassIndex + 1, "{");
                    lines.Add("}");
                }
            }
            
            // If missing Main method but has USB-1601 code, wrap it
            if (!hasMain && code.Contains("JYUSB1601"))
            {
                return WrapInMainMethod(string.Join("\n", lines));
            }
            
            return string.Join("\n", lines);
        }
        
        private string ValidateAndFixSyntax(string code)
        {
            // Basic syntax validation and fixes
            var lines = code.Split('\n').ToList();
            var fixedLines = new List<string>();
            var openBraces = 0;
            
            foreach (var line in lines)
            {
                var fixedLine = line;
                
                // Count braces
                openBraces += line.Count(c => c == '{');
                openBraces -= line.Count(c => c == '}');
                
                // Fix common syntax issues
                if (fixedLine.Trim().EndsWith("{") && !fixedLine.Contains("="))
                {
                    // Ensure space before opening brace
                    fixedLine = Regex.Replace(fixedLine, @"\s*{", " {");
                }
                
                fixedLines.Add(fixedLine);
            }
            
            // Balance braces if needed
            while (openBraces > 0)
            {
                fixedLines.Add(new string(' ', (openBraces - 1) * 4) + "}");
                openBraces--;
            }
            
            return string.Join("\n", fixedLines);
        }
        
        private string WrapInMainMethod(string code)
        {
            var sb = new StringBuilder();
            
            // Extract using statements
            var lines = code.Split('\n');
            var usings = lines.Where(l => l.Trim().StartsWith("using ")).ToList();
            var otherCode = lines.Where(l => !l.Trim().StartsWith("using ")).ToList();
            
            // Build proper structure
            foreach (var usingStatement in usings)
            {
                sb.AppendLine(usingStatement);
            }
            
            sb.AppendLine();
            sb.AppendLine("namespace GeneratedProject");
            sb.AppendLine("{");
            sb.AppendLine("    class Program");
            sb.AppendLine("    {");
            sb.AppendLine("        static void Main(string[] args)");
            sb.AppendLine("        {");
            sb.AppendLine("            JYUSB1601AITask aiTask = null;");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            
            // Indent the actual code
            foreach (var line in otherCode)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine("                " + line);
                }
            }
            
            sb.AppendLine("            }");
            sb.AppendLine("            catch (JYDriverException ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                Console.WriteLine($\"Error: {ex.Message}\");");
            sb.AppendLine("            }");
            sb.AppendLine("            finally");
            sb.AppendLine("            {");
            sb.AppendLine("                if (aiTask != null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    aiTask.Stop();");
            sb.AppendLine("                    aiTask.Channels.Clear();");
            sb.AppendLine("                }");
            sb.AppendLine("                Console.WriteLine(\"Press any key to exit...\");");
            sb.AppendLine("                Console.ReadKey();");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        private string GenerateDefaultTemplate()
        {
            return @"using System;
using JYUSB1601;
using System.Windows.Forms;

namespace GeneratedProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""USB-1601 Application Starting..."");
            
            JYUSB1601AITask aiTask = null;
            try
            {
                // TODO: Add your USB-1601 code here
                aiTask = new JYUSB1601AITask(""0"");
                aiTask.Mode = AIMode.Single;
                aiTask.AddChannel(0, -10, 10);
                aiTask.Start();
                
                double value = 0;
                aiTask.ReadSinglePoint(ref value, 0);
                Console.WriteLine($""Channel 0: {value} V"");
            }
            catch (JYDriverException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (aiTask != null)
                {
                    aiTask.Stop();
                    aiTask.Channels.Clear();
                }
                Console.WriteLine(""Press any key to exit..."");
                Console.ReadKey();
            }
        }
    }
}";
        }
    }
}