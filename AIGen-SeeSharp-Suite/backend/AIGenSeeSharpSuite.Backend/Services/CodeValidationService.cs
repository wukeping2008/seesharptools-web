using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace AIGenSeeSharpSuite.Backend.Services
{
    /// <summary>
    /// Service for validating and compiling generated C# code
    /// </summary>
    public class CodeValidationService
    {
        private readonly ILogger<CodeValidationService> _logger;
        
        public CodeValidationService(ILogger<CodeValidationService> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Validates C# code syntax and returns validation results
        /// </summary>
        public CodeValidationResult ValidateCode(string code)
        {
            var result = new CodeValidationResult();
            
            try
            {
                // Parse the code
                var tree = CSharpSyntaxTree.ParseText(code);
                var diagnostics = tree.GetDiagnostics();
                
                // Check for syntax errors
                foreach (var diagnostic in diagnostics)
                {
                    if (diagnostic.Severity == DiagnosticSeverity.Error)
                    {
                        result.Errors.Add(new CodeError
                        {
                            Message = diagnostic.GetMessage(),
                            Line = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1,
                            Column = diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1,
                            Severity = "Error"
                        });
                    }
                    else if (diagnostic.Severity == DiagnosticSeverity.Warning)
                    {
                        result.Warnings.Add(new CodeError
                        {
                            Message = diagnostic.GetMessage(),
                            Line = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1,
                            Column = diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1,
                            Severity = "Warning"
                        });
                    }
                }
                
                result.IsValid = result.Errors.Count == 0;
                
                // Additional validation
                if (result.IsValid)
                {
                    result.IsValid = ValidateStructure(tree);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating code");
                result.IsValid = false;
                result.Errors.Add(new CodeError
                {
                    Message = $"Validation error: {ex.Message}",
                    Severity = "Error"
                });
            }
            
            return result;
        }
        
        /// <summary>
        /// Attempts to compile the code and returns compilation results
        /// </summary>
        public async Task<CompilationResult> CompileCode(string code)
        {
            var result = new CompilationResult();
            
            try
            {
                var tree = CSharpSyntaxTree.ParseText(code);
                
                // Add necessary references
                var referenceList = new List<MetadataReference>
                {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location)
                };

                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null && !string.IsNullOrEmpty(entryAssembly.Location))
                {
                    referenceList.Add(MetadataReference.CreateFromFile(entryAssembly.Location));
                }

                // Add runtime references
                var runtimeDirectory = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
                referenceList.AddRange(new[]
                {
                    MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Collections.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Linq.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(runtimeDirectory, "System.Console.dll"))
                });

                var compilation = CSharpCompilation.Create(
                    "GeneratedCode",
                    new[] { tree },
                    referenceList,
                    new CSharpCompilationOptions(OutputKind.ConsoleApplication));
                
                using var ms = new MemoryStream();
                EmitResult emitResult = compilation.Emit(ms);
                
                result.Success = emitResult.Success;
                
                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    if (diagnostic.Severity == DiagnosticSeverity.Error)
                    {
                        result.Errors.Add(diagnostic.GetMessage());
                    }
                    else if (diagnostic.Severity == DiagnosticSeverity.Warning)
                    {
                        result.Warnings.Add(diagnostic.GetMessage());
                    }
                }
                
                if (result.Success)
                {
                    result.CompiledAssembly = ms.ToArray();
                    _logger.LogInformation("Code compiled successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compiling code");
                result.Success = false;
                result.Errors.Add($"Compilation error: {ex.Message}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Validates the structure of the code (has Main method, proper using statements, etc.)
        /// </summary>
        private bool ValidateStructure(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            
            // Check for Main method
            var mainMethod = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == "Main");
            
            if (mainMethod == null)
            {
                _logger.LogWarning("Code does not contain a Main method");
                return false;
            }
            
            // Check for using statements
            var usingDirectives = root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .ToList();
            
            if (usingDirectives.Count == 0)
            {
                _logger.LogWarning("Code does not contain any using statements");
            }
            
            // Check for namespace or top-level program
            var hasNamespace = root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .Any();
            
            var hasFileScopedNamespace = root.DescendantNodes()
                .OfType<FileScopedNamespaceDeclarationSyntax>()
                .Any();
            
            var hasClass = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Any();
            
            // Either needs namespace with class, or top-level program
            if (!hasNamespace && !hasFileScopedNamespace && !hasClass && mainMethod == null)
            {
                _logger.LogWarning("Code structure is invalid");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Formats the code using Roslyn's formatting services
        /// </summary>
        public string FormatCode(string code)
        {
            try
            {
                var tree = CSharpSyntaxTree.ParseText(code);
                var root = tree.GetRoot();
                var formatted = root.NormalizeWhitespace();
                return formatted.ToFullString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting code");
                return code; // Return original if formatting fails
            }
        }
    }
    
    /// <summary>
    /// Result of code validation
    /// </summary>
    public class CodeValidationResult
    {
        public bool IsValid { get; set; }
        public List<CodeError> Errors { get; set; } = new List<CodeError>();
        public List<CodeError> Warnings { get; set; } = new List<CodeError>();
    }
    
    /// <summary>
    /// Represents a code error or warning
    /// </summary>
    public class CodeError
    {
        public string Message { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Column { get; set; }
        public string Severity { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Result of code compilation
    /// </summary>
    public class CompilationResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public byte[]? CompiledAssembly { get; set; }
    }
}
