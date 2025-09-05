namespace AIGenSeeSharpSuite.Backend.Models
{
    /// <summary>
    /// Represents a MISD (Modular Instruments Software Dictionary) function definition
    /// </summary>
    public class MisdDefinition
    {
        public string FunctionName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // AI, AO, DI, DO, CI, CO
        public string Description { get; set; } = string.Empty;
        public List<MisdParameter> Parameters { get; set; } = new List<MisdParameter>();
        public string ReturnType { get; set; } = string.Empty;
        public string Example { get; set; } = string.Empty;
        public List<string> SupportedDevices { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Represents a parameter in a MISD function
    /// </summary>
    public class MisdParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public string? ValidRange { get; set; }
    }
    
    /// <summary>
    /// Represents a MISD knowledge entry extracted from Excel
    /// </summary>
    public class MisdKnowledgeEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Category { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // File path or sheet name
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}