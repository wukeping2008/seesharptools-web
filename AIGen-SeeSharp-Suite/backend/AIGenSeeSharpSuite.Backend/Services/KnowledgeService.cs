using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AIGenSeeSharpSuite.Backend.Models;

namespace AIGenSeeSharpSuite.Backend.Services
{
    public class KnowledgeService
    {
        private readonly MLContext _mlContext;
        private readonly List<Document> _knowledgeBase = new List<Document>();
        private readonly MisdExcelReader _misdReader;
        private readonly ILogger<KnowledgeService> _logger;
        private ITransformer? _textTransformer;
        private VBuffer<float>[]? _knowledgeBaseVectors;
        private List<MisdKnowledgeEntry> _misdEntries = new List<MisdKnowledgeEntry>();

        public KnowledgeService(IWebHostEnvironment env, ILogger<KnowledgeService> logger, MisdExcelReader misdReader)
        {
            _mlContext = new MLContext();
            _logger = logger;
            _misdReader = misdReader;
            var contentRootPath = env.ContentRootPath;
            
            LoadAndProcessDocuments(contentRootPath);
            LoadMisdKnowledge();
            BuildVectorIndex();
        }

        private void LoadAndProcessDocuments(string contentRootPath)
        {
            try
            {
                // Try to load example files if they exist
                var docsPath = Path.Combine(contentRootPath, "..", "..", "docs", "JYUSB-1601_V1.0.0_Examples");
                
                if (Directory.Exists(docsPath))
                {
                    var exampleFiles = Directory.GetFiles(docsPath, "*.cs", SearchOption.AllDirectories)
                        .Where(f => !f.Contains("Designer.cs") && !f.Contains("AssemblyInfo.cs"))
                        .Take(20); // Limit to avoid too many files
                    
                    foreach (var file in exampleFiles)
                    {
                        try
                        {
                            var content = File.ReadAllText(file);
                            var fileName = Path.GetFileName(file);
                            var category = GetExampleCategory(file);
                            
                            _knowledgeBase.Add(new Document 
                            { 
                                Content = $"// Example: {fileName} - {category}\n{content}"
                            });
                        }
                        catch
                        {
                            // Skip files that can't be read
                        }
                    }
                    
                    _logger.LogInformation($"Loaded {_knowledgeBase.Count} C# example files from {docsPath}");
                }
                
                // Always add essential JYUSB-1601 patterns
                AddEssentialPatterns();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading documents");
                // Add fallback patterns even if loading fails
                AddEssentialPatterns();
            }
        }
        
        private string GetExampleCategory(string filePath)
        {
            if (filePath.Contains("Analog Input")) return "Analog Input";
            if (filePath.Contains("Analog Output")) return "Analog Output";
            if (filePath.Contains("Digital")) return "Digital IO";
            if (filePath.Contains("Counter")) return "Counter";
            return "General";
        }
        
        private void AddEssentialPatterns()
        {
            // Single Mode Pattern
            _knowledgeBase.Add(new Document 
            { 
                Content = @"// JYUSB-1601 Single Mode Pattern
using System;
using JYUSB1601;
using System.Windows.Forms;

// Single point data acquisition
JYUSB1601AITask aiTask = new JYUSB1601AITask(""0"");
aiTask.Mode = AIMode.Single;
aiTask.AddChannel(0, -10, 10);
try
{
    aiTask.Start();
    double readValue = 0;
    aiTask.ReadSinglePoint(ref readValue, 0);
    Console.WriteLine($""Channel 0: {readValue} V"");
}
catch (JYDriverException ex)
{
    MessageBox.Show(ex.Message);
}
finally
{
    aiTask.Stop();
    aiTask.Channels.Clear();
}"
            });
            
            // Continuous Mode Pattern
            _knowledgeBase.Add(new Document 
            { 
                Content = @"// JYUSB-1601 Continuous Mode Pattern
using JYUSB1601;

// Continuous data acquisition
JYUSB1601AITask aiTask = new JYUSB1601AITask(""0"");
aiTask.Mode = AIMode.Continuous;
aiTask.SampleRate = 1000;
aiTask.AddChannel(0, -10, 10);

double[] readValue = new double[1000];
aiTask.Start();

// In timer tick event (10ms interval)
if (aiTask.AvailableSamples >= (ulong)readValue.Length)
{
    aiTask.ReadData(ref readValue, readValue.Length, -1);
    // Process data
}"
            });
            
            // Finite Mode Pattern
            _knowledgeBase.Add(new Document 
            { 
                Content = @"// JYUSB-1601 Finite Mode Pattern
using JYUSB1601;

// Finite data acquisition
JYUSB1601AITask aiTask = new JYUSB1601AITask(""0"");
aiTask.Mode = AIMode.Finite;
aiTask.SamplesToAcquire = 5000;
aiTask.SampleRate = 1000;
aiTask.AddChannel(0, -10, 10);

double[] readValue = new double[5000];
aiTask.Start();

// Wait for acquisition to complete
while (aiTask.AvailableSamples < 5000)
{
    System.Threading.Thread.Sleep(10);
}
aiTask.ReadData(ref readValue, 5000, -1);
aiTask.Stop();"
            });
            
            // Multi-channel Pattern
            _knowledgeBase.Add(new Document 
            { 
                Content = @"// JYUSB-1601 Multi-Channel Pattern
using JYUSB1601;

// Multi-channel acquisition
JYUSB1601AITask aiTask = new JYUSB1601AITask(""0"");
aiTask.Mode = AIMode.Continuous;
aiTask.SampleRate = 1000;

// Add multiple channels
for (int i = 0; i < 4; i++)
{
    aiTask.AddChannel(i, -10, 10);
}

// Read multi-channel data
double[,] readValue = new double[1000, 4];
aiTask.Start();

if (aiTask.AvailableSamples >= 1000)
{
    aiTask.ReadData(ref readValue, 1000, -1);
}"
            });
            
            // Digital Trigger Pattern
            _knowledgeBase.Add(new Document 
            { 
                Content = @"// JYUSB-1601 Digital Trigger Pattern
using JYUSB1601;

// Configure digital trigger
JYUSB1601AITask aiTask = new JYUSB1601AITask(""0"");
aiTask.Mode = AIMode.Finite;
aiTask.SamplesToAcquire = 1000;
aiTask.SampleRate = 1000;

// Setup trigger
aiTask.Trigger.Type = AITriggerType.Digital;
aiTask.Trigger.Digital.Edge = AIDigitalTriggerEdge.Rising;
aiTask.Trigger.Digital.Source = AIDigitalTriggerSource.PFI0;

aiTask.AddChannel(0, -10, 10);
aiTask.Start(); // Wait for trigger"
            });
            
            // Analog Output Pattern
            _knowledgeBase.Add(new Document 
            { 
                Content = @"// JYUSB-1601 Analog Output Pattern
using System;
using JYUSB1601;

// Analog output single point
JYUSB1601AOTask aoTask = new JYUSB1601AOTask(""0"");
aoTask.Mode = AOMode.Single;
aoTask.AddChannel(0, -10, 10);

try
{
    aoTask.Start();
    aoTask.WriteSinglePoint(5.0, 0); // Output 5V on channel 0
    Console.WriteLine(""Output 5V to channel 0"");
}
catch (JYDriverException ex)
{
    Console.WriteLine($""Error: {ex.Message}"");
}
finally
{
    aoTask.Stop();
    aoTask.Channels.Clear();
}"
            });
            
            // Analog Output Continuous Pattern
            _knowledgeBase.Add(new Document 
            { 
                Content = @"// JYUSB-1601 Analog Output Continuous Pattern
using System;
using JYUSB1601;

// Generate sine wave output
JYUSB1601AOTask aoTask = new JYUSB1601AOTask(""0"");
aoTask.Mode = AOMode.ContinuousWrapping;
aoTask.UpdateRate = 1000;
aoTask.AddChannel(0, -10, 10);

// Generate sine wave data
double[] waveform = new double[1000];
for (int i = 0; i < waveform.Length; i++)
{
    waveform[i] = 5 * Math.Sin(2 * Math.PI * i / waveform.Length);
}

aoTask.WriteData(waveform, waveform.Length);
aoTask.Start();"
            });
            
            // Digital Input Pattern
            _knowledgeBase.Add(new Document 
            { 
                Content = @"// JYUSB-1601 Digital Input Pattern
using System;
using JYUSB1601;

// Digital input reading
JYUSB1601DITask diTask = new JYUSB1601DITask(""0"");
diTask.Mode = DIMode.Single;

// Add digital lines (Port 0, Line 0-7)
for (int i = 0; i < 8; i++)
{
    diTask.AddChannel(0, i);
}

try
{
    diTask.Start();
    byte[] digitalData = new byte[8];
    diTask.ReadSinglePoint(ref digitalData);
    
    for (int i = 0; i < digitalData.Length; i++)
    {
        Console.WriteLine($""Line {i}: {digitalData[i]}"");
    }
}
finally
{
    diTask.Stop();
    diTask.Channels.Clear();
}"
            });
            
            // Digital Output Pattern
            _knowledgeBase.Add(new Document 
            { 
                Content = @"// JYUSB-1601 Digital Output Pattern
using System;
using JYUSB1601;

// Digital output control
JYUSB1601DOTask doTask = new JYUSB1601DOTask(""0"");
doTask.Mode = DOMode.Single;

// Add digital lines
for (int i = 0; i < 8; i++)
{
    doTask.AddChannel(0, i);
}

try
{
    doTask.Start();
    
    // Set pattern (10101010 binary)
    byte[] outputData = new byte[] { 1, 0, 1, 0, 1, 0, 1, 0 };
    doTask.WriteSinglePoint(outputData);
    
    Console.WriteLine(""Digital pattern output complete"");
}
finally
{
    doTask.Stop();
    doTask.Channels.Clear();
}"
            });
            
            // Counter/Timer Pattern
            _knowledgeBase.Add(new Document 
            { 
                Content = @"// JYUSB-1601 Counter Pattern
using System;
using JYUSB1601;

// Frequency measurement using counter
JYUSB1601CITask ciTask = new JYUSB1601CITask(""0"");
ciTask.Mode = CIMode.Frequency;
ciTask.Channel = 0;
ciTask.SampleRate = 1000;

try
{
    ciTask.Start();
    double frequency = 0;
    ciTask.ReadSinglePoint(ref frequency);
    Console.WriteLine($""Measured frequency: {frequency} Hz"");
}
finally
{
    ciTask.Stop();
}"
            });
        }
        
        private void LoadMisdKnowledge()
        {
            try
            {
                // Load MISD knowledge entries from Excel files
                _misdEntries = _misdReader.ExtractKnowledgeEntries();
                
                // Add MISD entries to knowledge base
                foreach (var entry in _misdEntries)
                {
                    _knowledgeBase.Add(new Document 
                    { 
                        Content = $"[MISD:{entry.Category}] {entry.Content}"
                    });
                }
                
                _logger.LogInformation($"Loaded {_misdEntries.Count} MISD knowledge entries");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading MISD knowledge");
            }
        }

        private void BuildVectorIndex()
        {
            if (_knowledgeBase.Count == 0) return;

            var textData = _mlContext.Data.LoadFromEnumerable(_knowledgeBase);

            var textPipeline = _mlContext.Transforms.Text.FeaturizeText("Features", "Content");
            _textTransformer = textPipeline.Fit(textData);

            var transformedData = _textTransformer.Transform(textData);
            var features = transformedData.GetColumn<VBuffer<float>>("Features").ToArray();
            _knowledgeBaseVectors = features;
        }

        public List<string> Search(string query, int topK = 2, int maxLength = 500)
        {
            if (_knowledgeBase.Count == 0 || _textTransformer == null || _knowledgeBaseVectors == null)
            {
                // Return a simple default example if no knowledge base
                return new List<string> 
                { 
                    @"// JYUSB-1601 Example
using JYUSB1601;
JYUSB1601AITask aiTask = new JYUSB1601AITask(""0"");
aiTask.Mode = AIMode.Finite;
aiTask.AddChannel(0, -10, 10);
aiTask.SampleRate = 1000;"
                };
            }

            var queryDoc = new Document { Content = query };
            var queryData = _mlContext.Data.LoadFromEnumerable(new[] { queryDoc });
            var transformedQueryData = _textTransformer.Transform(queryData);
            var queryVector = transformedQueryData.GetColumn<VBuffer<float>>("Features").First();

            var similarities = new List<(int index, float score)>();
            for (int i = 0; i < _knowledgeBaseVectors.Length; i++)
            {
                var score = CalculateCosineSimilarity(queryVector.GetValues().ToArray(), _knowledgeBaseVectors[i].GetValues().ToArray());
                similarities.Add((i, score));
            }

            var topResults = similarities
                .OrderByDescending(s => s.score)
                .Take(topK)
                .Select(r => {
                    var content = _knowledgeBase[r.index].Content;
                    // Truncate long content
                    if (content.Length > maxLength)
                    {
                        content = content.Substring(0, maxLength) + "\n// ... truncated";
                    }
                    return content;
                })
                .ToList();
                
            return topResults;
        }

        private float CalculateCosineSimilarity(float[] vecA, float[] vecB)
        {
            float dotProduct = 0.0f;
            float magA = 0.0f;
            float magB = 0.0f;
            for (int i = 0; i < vecA.Length; i++)
            {
                dotProduct += vecA[i] * vecB[i];
                magA += vecA[i] * vecA[i];
                magB += vecB[i] * vecB[i];
            }
            magA = (float)System.Math.Sqrt(magA);
            magB = (float)System.Math.Sqrt(magB);
            if (magA == 0 || magB == 0) return 0;
            return dotProduct / (magA * magB);
        }
    }

    public class Document
    {
        public string Content { get; set; } = "";
    }
}
