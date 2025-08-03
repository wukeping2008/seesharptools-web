using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeeSharpBackend.Models
{
    /// <summary>
    /// Test execution record entity
    /// </summary>
    [Table("TestExecutionRecords")]
    public class TestExecutionRecord
    {
        /// <summary>
        /// Primary key
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Test requirement description
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string TestRequirement { get; set; } = string.Empty;

        /// <summary>
        /// Generated code
        /// </summary>
        [Required]
        public string GeneratedCode { get; set; } = string.Empty;

        /// <summary>
        /// Device type
        /// </summary>
        [MaxLength(100)]
        public string? DeviceType { get; set; }

        /// <summary>
        /// Test type
        /// </summary>
        [MaxLength(100)]
        public string? TestType { get; set; }

        /// <summary>
        /// AI provider used
        /// </summary>
        [MaxLength(50)]
        public string? AIProvider { get; set; }

        /// <summary>
        /// Tokens used for generation
        /// </summary>
        public int? TokensUsed { get; set; }

        /// <summary>
        /// Code quality score (0-100)
        /// </summary>
        public int? CodeQualityScore { get; set; }

        /// <summary>
        /// User ID
        /// </summary>
        [MaxLength(100)]
        public string? UserId { get; set; }

        /// <summary>
        /// Execution result data (JSON)
        /// </summary>
        public string? ResultData { get; set; }

        /// <summary>
        /// Execution success flag
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if execution failed
        /// </summary>
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Execution time in milliseconds
        /// </summary>
        public int? ExecutionTimeMs { get; set; }

        /// <summary>
        /// Record creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Record last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Legacy properties for backward compatibility
        /// <summary>
        /// Test name (legacy)
        /// </summary>
        [NotMapped]
        public string TestName => $"Test_{Id}";
        
        /// <summary>
        /// Test description (legacy)
        /// </summary>
        [NotMapped]
        public string TestDescription => TestRequirement;
        
        /// <summary>
        /// Code version (legacy)
        /// </summary>
        [NotMapped]
        public int CodeVersion => 1;
        
        /// <summary>
        /// Execution result (legacy)
        /// </summary>
        [NotMapped]
        public string? ExecutionResult => ResultData;
        
        /// <summary>
        /// Executed at (legacy)
        /// </summary>
        [NotMapped]
        public DateTime ExecutedAt => CreatedAt;
        
        /// <summary>
        /// API provider (legacy)
        /// </summary>
        [NotMapped]
        public string? ApiProvider => AIProvider;
        
        /// <summary>
        /// Quality score (legacy)
        /// </summary>
        [NotMapped]
        public double QualityScore => CodeQualityScore ?? 0;
        
        /// <summary>
        /// Test parameters (legacy)
        /// </summary>
        [NotMapped]
        public string? TestParameters => null;
        
        /// <summary>
        /// Tags (legacy)
        /// </summary>
        [NotMapped]
        public string? Tags => null;
        
        /// <summary>
        /// Notes (legacy)
        /// </summary>
        [NotMapped]
        public string? Notes => null;
    }
}