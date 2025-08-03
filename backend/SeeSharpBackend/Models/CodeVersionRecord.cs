using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeeSharpBackend.Models
{
    /// <summary>
    /// Code version record entity
    /// </summary>
    [Table("CodeVersions")]
    public class CodeVersionRecord
    {
        /// <summary>
        /// Version ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Code base identifier
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string CodeBaseId { get; set; } = string.Empty;

        /// <summary>
        /// Version name/tag
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string VersionName { get; set; } = string.Empty;

        /// <summary>
        /// Version number (auto-incremented within branch)
        /// </summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// Source code content
        /// </summary>
        [Required]
        public string SourceCode { get; set; } = string.Empty;

        /// <summary>
        /// Code hash for integrity checking
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string CodeHash { get; set; } = string.Empty;

        /// <summary>
        /// Version description/notes
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Created by user
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Parent version ID (for branching/restore)
        /// </summary>
        public int? ParentVersionId { get; set; }

        /// <summary>
        /// Branch name
        /// </summary>
        [MaxLength(100)]
        public string? BranchName { get; set; } = "main";

        /// <summary>
        /// Test requirement (for AI generated code)
        /// </summary>
        [MaxLength(2000)]
        public string? TestRequirement { get; set; }

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
        /// Execution result data (JSON)
        /// </summary>
        public string? ResultData { get; set; }

        /// <summary>
        /// Execution success flag
        /// </summary>
        public bool? ExecutionSuccess { get; set; }

        /// <summary>
        /// Execution error message
        /// </summary>
        [MaxLength(1000)]
        public string? ExecutionError { get; set; }

        /// <summary>
        /// Execution time in milliseconds
        /// </summary>
        public int? ExecutionTimeMs { get; set; }

        /// <summary>
        /// Code quality score (0-100)
        /// </summary>
        public int? QualityScore { get; set; }

        /// <summary>
        /// Version tags (JSON array)
        /// </summary>
        [MaxLength(500)]
        public string? Tags { get; set; }

        /// <summary>
        /// Is this version active/current
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Is this version deleted (soft delete)
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Foreign key reference to parent version
        /// </summary>
        [ForeignKey(nameof(ParentVersionId))]
        public virtual CodeVersionRecord? ParentVersion { get; set; }

        /// <summary>
        /// Child versions (branches/restores)
        /// </summary>
        public virtual ICollection<CodeVersionRecord> ChildVersions { get; set; } = new List<CodeVersionRecord>();
    }
}