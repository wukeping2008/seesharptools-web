using System.Collections.Generic;
using System.Text;
using AIGenSeeSharpSuite.Backend.Models;

namespace AIGenSeeSharpSuite.Backend.Services
{
    public class PromptEngineeringService
    {
        public string BuildMegaPrompt(string userPrompt, List<string> knowledgeSnippets, string modelName = "ernie-speed-128k")
        {
            // Get model-specific configuration
            var modelConfig = ModelConfiguration.GetModelConfig(modelName);
            
            var sb = new StringBuilder();

            // 1. Concise Role Setting
            sb.AppendLine("You are a C# expert specializing in JYTEK USB-1601 device and MISD (Modular Instruments Software Dictionary) standard.");
            sb.AppendLine("Generate a complete, compilable C# Program.cs file following USB-1601 best practices.");
            sb.AppendLine("IMPORTANT: Use proper exception handling with JYDriverException and always clean up resources.");
            sb.AppendLine();

            // 2. Limited Context from Knowledge Base (based on model config)
            if (knowledgeSnippets != null && knowledgeSnippets.Count > 0)
            {
                sb.AppendLine("--- REFERENCE CODE ---");
                
                // Use model-specific limits
                var snippetsToInclude = knowledgeSnippets
                    .Take(modelConfig.MaxKnowledgeSnippets)
                    .Select(s => TruncateSnippet(s, modelConfig.MaxSnippetLength));
                
                foreach (var snippet in snippetsToInclude)
                {
                    sb.AppendLine(snippet);
                    sb.AppendLine();
                }
                
                sb.AppendLine("--- END REFERENCE ---");
            }
            sb.AppendLine();

            // 3. User's Core Task
            sb.AppendLine("--- USER'S REQUEST ---");
            sb.AppendLine(userPrompt);
            sb.AppendLine("--- END OF USER'S REQUEST ---");
            sb.AppendLine();

            // 4. Concise Output Constraints
            sb.AppendLine("--- OUTPUT REQUIREMENTS ---");
            sb.AppendLine("1. Start with 'using System;' and necessary namespaces (JYUSB1601, System.Windows.Forms if needed)");
            sb.AppendLine("2. Create a complete Program.cs with proper Main method");
            sb.AppendLine("3. Follow USB-1601 initialization pattern: Create task → Configure mode → Add channels → Start → Read/Write → Stop → Cleanup");
            sb.AppendLine("4. Use try-catch-finally for proper resource management");
            sb.AppendLine("5. Output ONLY compilable C# code, NO markdown, NO explanations");
            sb.AppendLine("--- END ---");

            return sb.ToString();
        }
        
        /// <summary>
        /// Truncates a snippet to the specified maximum length
        /// </summary>
        private string TruncateSnippet(string snippet, int maxLength)
        {
            if (string.IsNullOrEmpty(snippet) || snippet.Length <= maxLength)
                return snippet;
            
            // Try to truncate at a logical boundary (end of line)
            var truncated = snippet.Substring(0, maxLength);
            var lastNewLine = truncated.LastIndexOf('\n');
            
            if (lastNewLine > maxLength * 0.8) // If we have a newline near the end
            {
                return truncated.Substring(0, lastNewLine) + "\n// ... truncated";
            }
            
            return truncated + "... // truncated";
        }
    }
}
