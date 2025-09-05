namespace AIGenSeeSharpSuite.Backend.Models
{
    /// <summary>
    /// Configuration for different AI models
    /// </summary>
    public static class ModelConfiguration
    {
        public static readonly Dictionary<string, ModelConfig> Models = new()
        {
            ["ernie-lite-8k"] = new ModelConfig
            {
                Name = "ERNIE Lite 8K",
                MaxInputTokens = 6000,
                MaxOutputTokens = 2000,
                TotalTokenLimit = 8000,
                MaxKnowledgeSnippets = 1,
                MaxSnippetLength = 300
            },
            ["ernie-speed-128k"] = new ModelConfig
            {
                Name = "ERNIE Speed 128K",
                MaxInputTokens = 100000,
                MaxOutputTokens = 28000,
                TotalTokenLimit = 128000,
                MaxKnowledgeSnippets = 3,
                MaxSnippetLength = 800
            },
            ["ernie-x1-turbo-32k"] = new ModelConfig
            {
                Name = "ERNIE X1 Turbo 32K",
                MaxInputTokens = 24000,
                MaxOutputTokens = 8000,
                TotalTokenLimit = 32000,
                MaxKnowledgeSnippets = 2,
                MaxSnippetLength = 600
            },
            ["ernie-4.5-turbo-128k"] = new ModelConfig
            {
                Name = "ERNIE 4.5 Turbo 128K",
                MaxInputTokens = 100000,
                MaxOutputTokens = 28000,
                TotalTokenLimit = 128000,
                MaxKnowledgeSnippets = 3,
                MaxSnippetLength = 1000
            }
        };
        
        public static ModelConfig GetModelConfig(string modelName)
        {
            var key = modelName.ToLower();
            return Models.ContainsKey(key) 
                ? Models[key] 
                : Models["ernie-speed-128k"]; // Default to speed model
        }
    }
    
    public class ModelConfig
    {
        public string Name { get; set; } = string.Empty;
        public int MaxInputTokens { get; set; }
        public int MaxOutputTokens { get; set; }
        public int TotalTokenLimit { get; set; }
        public int MaxKnowledgeSnippets { get; set; }
        public int MaxSnippetLength { get; set; }
    }
}