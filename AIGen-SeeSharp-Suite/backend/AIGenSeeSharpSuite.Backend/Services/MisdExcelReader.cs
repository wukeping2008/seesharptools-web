using ClosedXML.Excel;
using AIGenSeeSharpSuite.Backend.Models;

namespace AIGenSeeSharpSuite.Backend.Services
{
    /// <summary>
    /// Service for reading and parsing MISD Excel files
    /// </summary>
    public class MisdExcelReader
    {
        private readonly ILogger<MisdExcelReader> _logger;
        private readonly string _misdFilePath;
        private readonly string _misdJyusbFilePath;
        
        public MisdExcelReader(ILogger<MisdExcelReader> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            // Try multiple possible paths
            var possiblePaths = new[]
            {
                Path.Combine(env.ContentRootPath, "..", "..", "docs"),
                Path.Combine(env.ContentRootPath, "..", "..", "..", "docs"),
                Path.Combine(env.ContentRootPath, "docs")
            };
            
            string? docsPath = null;
            foreach (var path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    docsPath = path;
                    break;
                }
            }
            
            if (docsPath != null)
            {
                _misdFilePath = Path.Combine(docsPath, "MISD.xlsx");
                _misdJyusbFilePath = Path.Combine(docsPath, "MISD-JYUSB1601.xlsx");
            }
            else
            {
                _logger.LogWarning("Could not find docs directory, MISD files will not be available");
                _misdFilePath = "";
                _misdJyusbFilePath = "";
            }
        }
        
        /// <summary>
        /// Load MISD definitions from Excel files
        /// </summary>
        public List<MisdDefinition> LoadMisdDefinitions()
        {
            var definitions = new List<MisdDefinition>();
            
            try
            {
                // Load main MISD file
                if (!string.IsNullOrEmpty(_misdFilePath) && File.Exists(_misdFilePath))
                {
                    definitions.AddRange(ReadMisdFile(_misdFilePath));
                    _logger.LogInformation($"Loaded {definitions.Count} definitions from MISD.xlsx");
                }
                
                // Load device-specific MISD file
                if (!string.IsNullOrEmpty(_misdJyusbFilePath) && File.Exists(_misdJyusbFilePath))
                {
                    var deviceDefinitions = ReadMisdFile(_misdJyusbFilePath);
                    definitions.AddRange(deviceDefinitions);
                    _logger.LogInformation($"Loaded {deviceDefinitions.Count} definitions from MISD-JYUSB1601.xlsx");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading MISD definitions from Excel");
            }
            
            return definitions;
        }
        
        /// <summary>
        /// Extract knowledge entries from MISD Excel files
        /// </summary>
        public List<MisdKnowledgeEntry> ExtractKnowledgeEntries()
        {
            var entries = new List<MisdKnowledgeEntry>();
            
            try
            {
                if (!string.IsNullOrEmpty(_misdFilePath) && File.Exists(_misdFilePath))
                {
                    entries.AddRange(ExtractEntriesFromFile(_misdFilePath));
                }
                
                if (!string.IsNullOrEmpty(_misdJyusbFilePath) && File.Exists(_misdJyusbFilePath))
                {
                    entries.AddRange(ExtractEntriesFromFile(_misdJyusbFilePath));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting knowledge entries from MISD files");
            }
            
            return entries;
        }
        
        private List<MisdDefinition> ReadMisdFile(string filePath)
        {
            var definitions = new List<MisdDefinition>();
            
            using (var workbook = new XLWorkbook(filePath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    // Skip non-relevant sheets
                    if (worksheet.Name.Contains("Index") || worksheet.Name.Contains("Overview"))
                        continue;
                    
                    // Try to parse function definitions
                    // Assuming structure: Function | Category | Description | Parameters | Return Type | Example
                    var rows = worksheet.RowsUsed().Skip(1); // Skip header
                    
                    foreach (var row in rows)
                    {
                        try
                        {
                            var definition = new MisdDefinition
                            {
                                FunctionName = row.Cell(1).GetString(),
                                Category = row.Cell(2).GetString(),
                                Description = row.Cell(3).GetString(),
                                ReturnType = row.Cell(5).GetString(),
                                Example = row.Cell(6).GetString()
                            };
                            
                            // Parse parameters if present
                            var paramString = row.Cell(4).GetString();
                            if (!string.IsNullOrEmpty(paramString))
                            {
                                definition.Parameters = ParseParameters(paramString);
                            }
                            
                            definitions.Add(definition);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error parsing row in {worksheet.Name}: {ex.Message}");
                        }
                    }
                }
            }
            
            return definitions;
        }
        
        private List<MisdKnowledgeEntry> ExtractEntriesFromFile(string filePath)
        {
            var entries = new List<MisdKnowledgeEntry>();
            
            using (var workbook = new XLWorkbook(filePath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    // Extract all content as knowledge entries
                    var content = new System.Text.StringBuilder();
                    
                    foreach (var row in worksheet.RowsUsed())
                    {
                        foreach (var cell in row.CellsUsed())
                        {
                            content.AppendLine(cell.GetString());
                        }
                    }
                    
                    if (content.Length > 0)
                    {
                        entries.Add(new MisdKnowledgeEntry
                        {
                            Category = worksheet.Name,
                            Content = content.ToString(),
                            Source = $"{Path.GetFileName(filePath)}/{worksheet.Name}",
                            Metadata = new Dictionary<string, string>
                            {
                                ["FileName"] = Path.GetFileName(filePath),
                                ["SheetName"] = worksheet.Name,
                                ["RowCount"] = worksheet.RowsUsed().Count().ToString()
                            }
                        });
                    }
                }
            }
            
            return entries;
        }
        
        private List<MisdParameter> ParseParameters(string paramString)
        {
            var parameters = new List<MisdParameter>();
            
            // Simple parsing - can be enhanced based on actual format
            var parts = paramString.Split(';');
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part)) continue;
                
                var paramParts = part.Split(':');
                parameters.Add(new MisdParameter
                {
                    Name = paramParts.Length > 0 ? paramParts[0].Trim() : "",
                    Type = paramParts.Length > 1 ? paramParts[1].Trim() : "object",
                    IsRequired = !part.Contains("optional", StringComparison.OrdinalIgnoreCase)
                });
            }
            
            return parameters;
        }
    }
}