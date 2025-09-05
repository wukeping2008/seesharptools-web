using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace AIGenSeeSharpSuite.Backend.Services
{
    /// <summary>
    /// Advanced code generation service for complex USB-1601 configurations
    /// </summary>
    public class AdvancedCodeGeneratorService
    {
        private readonly ILogger<AdvancedCodeGeneratorService> _logger;
        
        public AdvancedCodeGeneratorService(ILogger<AdvancedCodeGeneratorService> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Generates code for multi-channel synchronous acquisition
        /// </summary>
        public string GenerateMultiChannelCode(int[] channels, double sampleRate, int samplesToAcquire, string mode = "Finite")
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("using System;");
            sb.AppendLine("using JYUSB1601;");
            sb.AppendLine("using System.Windows.Forms;");
            sb.AppendLine();
            sb.AppendLine("namespace MultiChannelAcquisition");
            sb.AppendLine("{");
            sb.AppendLine("    class Program");
            sb.AppendLine("    {");
            sb.AppendLine("        static void Main(string[] args)");
            sb.AppendLine("        {");
            sb.AppendLine($"            Console.WriteLine(\"Multi-Channel Acquisition: Channels {string.Join(", ", channels)}\");");
            sb.AppendLine();
            sb.AppendLine("            JYUSB1601AITask aiTask = null;");
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                aiTask = new JYUSB1601AITask(\"0\");");
            sb.AppendLine($"                aiTask.Mode = AIMode.{mode};");
            sb.AppendLine($"                aiTask.SampleRate = {sampleRate};");
            
            if (mode == "Finite")
            {
                sb.AppendLine($"                aiTask.SamplesToAcquire = {samplesToAcquire};");
            }
            
            sb.AppendLine();
            sb.AppendLine("                // Add channels with auto-range detection");
            
            foreach (var channel in channels)
            {
                sb.AppendLine($"                aiTask.AddChannel({channel}, -10, 10);");
            }
            
            sb.AppendLine();
            sb.AppendLine($"                double[,] readValue = new double[{samplesToAcquire}, {channels.Length}];");
            sb.AppendLine("                ");
            sb.AppendLine("                aiTask.Start();");
            sb.AppendLine("                Console.WriteLine(\"Acquisition started...\");");
            sb.AppendLine();
            
            if (mode == "Finite")
            {
                sb.AppendLine($"                while (aiTask.AvailableSamples < {samplesToAcquire})");
                sb.AppendLine("                {");
                sb.AppendLine("                    System.Threading.Thread.Sleep(10);");
                sb.AppendLine("                }");
                sb.AppendLine();
                sb.AppendLine($"                aiTask.ReadData(ref readValue, {samplesToAcquire}, -1);");
                sb.AppendLine("                ");
                sb.AppendLine("                // Process data");
                sb.AppendLine("                for (int ch = 0; ch < " + channels.Length + "; ch++)");
                sb.AppendLine("                {");
                sb.AppendLine("                    double sum = 0;");
                sb.AppendLine($"                    for (int i = 0; i < {samplesToAcquire}; i++)");
                sb.AppendLine("                    {");
                sb.AppendLine("                        sum += readValue[i, ch];");
                sb.AppendLine("                    }");
                sb.AppendLine($"                    double avg = sum / {samplesToAcquire};");
                sb.AppendLine("                    Console.WriteLine($\"Channel {channels[ch]}: Average = {avg:F3} V\");");
                sb.AppendLine("                }");
            }
            else // Continuous
            {
                sb.AppendLine("                // Continuous reading loop");
                sb.AppendLine("                int readCount = 0;");
                sb.AppendLine("                while (readCount < 10) // Read 10 times for demo");
                sb.AppendLine("                {");
                sb.AppendLine($"                    if (aiTask.AvailableSamples >= {samplesToAcquire})");
                sb.AppendLine("                    {");
                sb.AppendLine($"                        aiTask.ReadData(ref readValue, {samplesToAcquire}, -1);");
                sb.AppendLine("                        Console.WriteLine($\"Read #{++readCount}: Channel 0 = {readValue[0, 0]:F3} V\");");
                sb.AppendLine("                        System.Threading.Thread.Sleep(100);");
                sb.AppendLine("                    }");
                sb.AppendLine("                }");
            }
            
            sb.AppendLine("            }");
            sb.AppendLine("            catch (JYDriverException ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                MessageBox.Show($\"Driver error: {ex.Message}\", \"Error\", MessageBoxButtons.OK, MessageBoxIcon.Error);");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                Console.WriteLine($\"Error: {ex.Message}\");");
            sb.AppendLine("            }");
            sb.AppendLine("            finally");
            sb.AppendLine("            {");
            sb.AppendLine("                if (aiTask != null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    aiTask.Stop();");
            sb.AppendLine("                    aiTask.Channels.Clear();");
            sb.AppendLine("                    Console.WriteLine(\"Acquisition stopped and resources cleaned.\");");
            sb.AppendLine("                }");
            sb.AppendLine("                Console.WriteLine(\"\\nPress any key to exit...\");");
            sb.AppendLine("                Console.ReadKey();");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Generates code for synchronized AI/AO operations
        /// </summary>
        public string GenerateSynchronizedAIAOCode(double sampleRate, double outputFrequency, double amplitude)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("using System;");
            sb.AppendLine("using JYUSB1601;");
            sb.AppendLine("using System.Threading;");
            sb.AppendLine();
            sb.AppendLine("namespace SynchronizedAIAO");
            sb.AppendLine("{");
            sb.AppendLine("    class Program");
            sb.AppendLine("    {");
            sb.AppendLine("        static void Main(string[] args)");
            sb.AppendLine("        {");
            sb.AppendLine("            Console.WriteLine(\"Synchronized AI/AO Operation\");");
            sb.AppendLine();
            sb.AppendLine("            JYUSB1601AITask aiTask = null;");
            sb.AppendLine("            JYUSB1601AOTask aoTask = null;");
            sb.AppendLine();
            sb.AppendLine("            try");
            sb.AppendLine("            {");
            sb.AppendLine("                // Configure Analog Output");
            sb.AppendLine("                aoTask = new JYUSB1601AOTask(\"0\");");
            sb.AppendLine("                aoTask.Mode = AOMode.ContinuousWrapping;");
            sb.AppendLine($"                aoTask.UpdateRate = {sampleRate};");
            sb.AppendLine("                aoTask.AddChannel(0, -10, 10);");
            sb.AppendLine();
            sb.AppendLine("                // Generate output waveform");
            sb.AppendLine($"                int samples = (int)({sampleRate} / {outputFrequency});");
            sb.AppendLine("                double[] waveform = new double[samples];");
            sb.AppendLine("                for (int i = 0; i < samples; i++)");
            sb.AppendLine("                {");
            sb.AppendLine($"                    waveform[i] = {amplitude} * Math.Sin(2 * Math.PI * i / samples);");
            sb.AppendLine("                }");
            sb.AppendLine("                aoTask.WriteData(waveform, samples);");
            sb.AppendLine();
            sb.AppendLine("                // Configure Analog Input");
            sb.AppendLine("                aiTask = new JYUSB1601AITask(\"0\");");
            sb.AppendLine("                aiTask.Mode = AIMode.Continuous;");
            sb.AppendLine($"                aiTask.SampleRate = {sampleRate};");
            sb.AppendLine("                aiTask.AddChannel(0, -10, 10);");
            sb.AppendLine();
            sb.AppendLine("                // Start both tasks");
            sb.AppendLine("                aoTask.Start();");
            sb.AppendLine("                Console.WriteLine($\"Outputting {outputFrequency} Hz sine wave...\");");
            sb.AppendLine();
            sb.AppendLine("                aiTask.Start();");
            sb.AppendLine("                Console.WriteLine(\"Reading input signal...\");");
            sb.AppendLine();
            sb.AppendLine("                // Read and analyze");
            sb.AppendLine("                double[] readData = new double[1000];");
            sb.AppendLine("                Thread.Sleep(100); // Let buffer fill");
            sb.AppendLine();
            sb.AppendLine("                for (int loop = 0; loop < 5; loop++)");
            sb.AppendLine("                {");
            sb.AppendLine("                    if (aiTask.AvailableSamples >= (ulong)readData.Length)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        aiTask.ReadData(ref readData, readData.Length, -1);");
            sb.AppendLine();
            sb.AppendLine("                        // Calculate RMS");
            sb.AppendLine("                        double sum = 0;");
            sb.AppendLine("                        foreach (var sample in readData)");
            sb.AppendLine("                        {");
            sb.AppendLine("                            sum += sample * sample;");
            sb.AppendLine("                        }");
            sb.AppendLine("                        double rms = Math.Sqrt(sum / readData.Length);");
            sb.AppendLine("                        Console.WriteLine($\"Loop {loop + 1}: RMS = {rms:F3} V\");");
            sb.AppendLine("                    }");
            sb.AppendLine("                    Thread.Sleep(200);");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("            catch (Exception ex)");
            sb.AppendLine("            {");
            sb.AppendLine("                Console.WriteLine($\"Error: {ex.Message}\");");
            sb.AppendLine("            }");
            sb.AppendLine("            finally");
            sb.AppendLine("            {");
            sb.AppendLine("                // Clean up");
            sb.AppendLine("                if (aiTask != null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    aiTask.Stop();");
            sb.AppendLine("                    aiTask.Channels.Clear();");
            sb.AppendLine("                }");
            sb.AppendLine("                if (aoTask != null)");
            sb.AppendLine("                {");
            sb.AppendLine("                    aoTask.Stop();");
            sb.AppendLine("                    aoTask.Channels.Clear();");
            sb.AppendLine("                }");
            sb.AppendLine("                Console.WriteLine(\"\\nPress any key to exit...\");");
            sb.AppendLine("                Console.ReadKey();");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Analyzes user prompt to extract configuration parameters
        /// </summary>
        public DeviceConfiguration AnalyzePrompt(string prompt)
        {
            var config = new DeviceConfiguration();
            prompt = prompt.ToLower();
            
            // Extract mode
            if (prompt.Contains("continuous"))
                config.Mode = "Continuous";
            else if (prompt.Contains("finite") || prompt.Contains("fixed"))
                config.Mode = "Finite";
            else if (prompt.Contains("single"))
                config.Mode = "Single";
            
            // Extract sample rate
            var rateMatch = Regex.Match(prompt, @"(\d+)\s*(hz|khz|samples?/s|sps)", RegexOptions.IgnoreCase);
            if (rateMatch.Success)
            {
                var rate = double.Parse(rateMatch.Groups[1].Value);
                if (rateMatch.Groups[2].Value.ToLower().Contains("khz"))
                    rate *= 1000;
                config.SampleRate = rate;
            }
            
            // Extract channels
            var channelMatches = Regex.Matches(prompt, @"channel[s]?\s*(\d+(?:\s*[-,]\s*\d+)*)", RegexOptions.IgnoreCase);
            if (channelMatches.Count > 0)
            {
                var channels = new List<int>();
                foreach (Match match in channelMatches)
                {
                    var channelStr = match.Groups[1].Value;
                    if (channelStr.Contains("-"))
                    {
                        // Range like "0-3"
                        var parts = channelStr.Split('-');
                        if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int start) && int.TryParse(parts[1].Trim(), out int end))
                        {
                            for (int i = start; i <= end; i++)
                                channels.Add(i);
                        }
                    }
                    else if (channelStr.Contains(","))
                    {
                        // List like "0,2,4"
                        var parts = channelStr.Split(',');
                        foreach (var part in parts)
                        {
                            if (int.TryParse(part.Trim(), out int ch))
                                channels.Add(ch);
                        }
                    }
                    else
                    {
                        // Single channel
                        if (int.TryParse(channelStr.Trim(), out int ch))
                            channels.Add(ch);
                    }
                }
                config.Channels = channels.Distinct().OrderBy(x => x).ToArray();
            }
            
            // Extract task type
            if (prompt.Contains("output") || prompt.Contains("ao") || prompt.Contains("generate"))
                config.TaskType = "AO";
            else if (prompt.Contains("digital in") || prompt.Contains("di"))
                config.TaskType = "DI";
            else if (prompt.Contains("digital out") || prompt.Contains("do"))
                config.TaskType = "DO";
            else
                config.TaskType = "AI"; // Default
            
            // Extract trigger configuration
            if (prompt.Contains("trigger"))
            {
                if (prompt.Contains("digital trigger") || prompt.Contains("external trigger"))
                    config.TriggerType = "Digital";
                else if (prompt.Contains("software trigger"))
                    config.TriggerType = "Software";
            }
            
            _logger.LogInformation($"Analyzed configuration: Mode={config.Mode}, Rate={config.SampleRate}, Channels=[{string.Join(",", config.Channels)}]");
            
            return config;
        }
    }
    
    public class DeviceConfiguration
    {
        public string Mode { get; set; } = "Finite";
        public double SampleRate { get; set; } = 1000;
        public int[] Channels { get; set; } = new[] { 0 };
        public string TaskType { get; set; } = "AI";
        public string TriggerType { get; set; } = "None";
        public int SamplesToAcquire { get; set; } = 1000;
    }
}