using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeeSharpBackend.Services.CodeVersioning
{
    /// <summary>
    /// Code version management service interface
    /// </summary>
    public interface ICodeVersionService
    {
        /// <summary>
        /// Create a new code version
        /// </summary>
        Task<int> CreateVersionAsync(CodeVersionRequest request);

        /// <summary>
        /// Get code version by ID
        /// </summary>
        Task<CodeVersion?> GetVersionAsync(int versionId);

        /// <summary>
        /// Get all versions for a specific code base
        /// </summary>
        Task<List<CodeVersion>> GetVersionHistoryAsync(string codeBaseId, int maxCount = 50);

        /// <summary>
        /// Get latest version of a code base
        /// </summary>
        Task<CodeVersion?> GetLatestVersionAsync(string codeBaseId);

        /// <summary>
        /// Update version with execution results
        /// </summary>
        Task<bool> UpdateVersionResultsAsync(int versionId, string resultData, bool success, string? errorMessage = null);

        /// <summary>
        /// Compare two code versions
        /// </summary>
        Task<CodeVersionComparison> CompareVersionsAsync(int version1Id, int version2Id);

        /// <summary>
        /// Restore a previous version (create new version from old one)
        /// </summary>
        Task<int> RestoreVersionAsync(int versionId, string restoredBy, string? notes = null);

        /// <summary>
        /// Delete a code version
        /// </summary>
        Task<bool> DeleteVersionAsync(int versionId);

        /// <summary>
        /// Get version statistics
        /// </summary>
        Task<CodeVersionStatistics> GetVersionStatisticsAsync(string codeBaseId);

        /// <summary>
        /// Search versions by criteria
        /// </summary>
        Task<List<CodeVersion>> SearchVersionsAsync(CodeVersionSearchQuery query);

        /// <summary>
        /// Create a branch from existing version
        /// </summary>
        Task<int> CreateVersionBranchAsync(int sourceVersionId, string branchName, string createdBy, string? notes = null);

        /// <summary>
        /// Get version performance trends
        /// </summary>
        Task<List<VersionPerformanceTrend>> GetPerformanceTrendsAsync(string codeBaseId, int days = 30);
    }

    /// <summary>
    /// Code version request model
    /// </summary>
    public class CodeVersionRequest
    {
        /// <summary>
        /// Code base identifier
        /// </summary>
        public string CodeBaseId { get; set; } = string.Empty;

        /// <summary>
        /// Version name/tag
        /// </summary>
        public string VersionName { get; set; } = string.Empty;

        /// <summary>
        /// Source code content
        /// </summary>
        public string SourceCode { get; set; } = string.Empty;

        /// <summary>
        /// Version description/notes
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Created by user
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Parent version ID (for branching)
        /// </summary>
        public int? ParentVersionId { get; set; }

        /// <summary>
        /// Branch name
        /// </summary>
        public string? BranchName { get; set; }

        /// <summary>
        /// Test requirement (for AI generated code)
        /// </summary>
        public string? TestRequirement { get; set; }

        /// <summary>
        /// Device type
        /// </summary>
        public string? DeviceType { get; set; }

        /// <summary>
        /// Test type
        /// </summary>
        public string? TestType { get; set; }

        /// <summary>
        /// Tags for categorization
        /// </summary>
        public List<string>? Tags { get; set; }
    }

    /// <summary>
    /// Code version entity
    /// </summary>
    public class CodeVersion
    {
        /// <summary>
        /// Version ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Code base identifier
        /// </summary>
        public string CodeBaseId { get; set; } = string.Empty;

        /// <summary>
        /// Version name/tag
        /// </summary>
        public string VersionName { get; set; } = string.Empty;

        /// <summary>
        /// Version number (auto-incremented)
        /// </summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// Source code content
        /// </summary>
        public string SourceCode { get; set; } = string.Empty;

        /// <summary>
        /// Code hash for integrity checking
        /// </summary>
        public string CodeHash { get; set; } = string.Empty;

        /// <summary>
        /// Version description/notes
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Created by user
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Parent version ID
        /// </summary>
        public int? ParentVersionId { get; set; }

        /// <summary>
        /// Branch name
        /// </summary>
        public string? BranchName { get; set; }

        /// <summary>
        /// Test requirement
        /// </summary>
        public string? TestRequirement { get; set; }

        /// <summary>
        /// Device type
        /// </summary>
        public string? DeviceType { get; set; }

        /// <summary>
        /// Test type
        /// </summary>
        public string? TestType { get; set; }

        /// <summary>
        /// Execution result data
        /// </summary>
        public string? ResultData { get; set; }

        /// <summary>
        /// Execution success flag
        /// </summary>
        public bool? ExecutionSuccess { get; set; }

        /// <summary>
        /// Execution error message
        /// </summary>
        public string? ExecutionError { get; set; }

        /// <summary>
        /// Execution time in milliseconds
        /// </summary>
        public int? ExecutionTimeMs { get; set; }

        /// <summary>
        /// Code quality score
        /// </summary>
        public int? QualityScore { get; set; }

        /// <summary>
        /// Version tags
        /// </summary>
        public string? Tags { get; set; } // JSON array

        /// <summary>
        /// Is this version active/current
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Is this version deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Code version comparison result
    /// </summary>
    public class CodeVersionComparison
    {
        /// <summary>
        /// Source version
        /// </summary>
        public CodeVersion SourceVersion { get; set; } = new();

        /// <summary>
        /// Target version
        /// </summary>
        public CodeVersion TargetVersion { get; set; } = new();

        /// <summary>
        /// Differences between versions
        /// </summary>
        public List<CodeDifference> Differences { get; set; } = new();

        /// <summary>
        /// Similarity percentage
        /// </summary>
        public double SimilarityPercentage { get; set; }

        /// <summary>
        /// Lines added
        /// </summary>
        public int LinesAdded { get; set; }

        /// <summary>
        /// Lines deleted
        /// </summary>
        public int LinesDeleted { get; set; }

        /// <summary>
        /// Lines modified
        /// </summary>
        public int LinesModified { get; set; }
    }

    /// <summary>
    /// Code difference entry
    /// </summary>
    public class CodeDifference
    {
        /// <summary>
        /// Difference type
        /// </summary>
        public DifferenceType Type { get; set; }

        /// <summary>
        /// Line number in source
        /// </summary>
        public int? SourceLineNumber { get; set; }

        /// <summary>
        /// Line number in target
        /// </summary>
        public int? TargetLineNumber { get; set; }

        /// <summary>
        /// Source line content
        /// </summary>
        public string? SourceContent { get; set; }

        /// <summary>
        /// Target line content
        /// </summary>
        public string? TargetContent { get; set; }
    }

    /// <summary>
    /// Code difference types
    /// </summary>
    public enum DifferenceType
    {
        Added,
        Deleted,
        Modified,
        Unchanged
    }

    /// <summary>
    /// Code version search query
    /// </summary>
    public class CodeVersionSearchQuery
    {
        /// <summary>
        /// Code base ID filter
        /// </summary>
        public string? CodeBaseId { get; set; }

        /// <summary>
        /// Version name contains
        /// </summary>
        public string? VersionNameContains { get; set; }

        /// <summary>
        /// Created by user
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Branch name
        /// </summary>
        public string? BranchName { get; set; }

        /// <summary>
        /// Device type
        /// </summary>
        public string? DeviceType { get; set; }

        /// <summary>
        /// Test type
        /// </summary>
        public string? TestType { get; set; }

        /// <summary>
        /// Execution success filter
        /// </summary>
        public bool? ExecutionSuccess { get; set; }

        /// <summary>
        /// Minimum quality score
        /// </summary>
        public int? MinQualityScore { get; set; }

        /// <summary>
        /// Date range start
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Date range end
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Tags to match
        /// </summary>
        public List<string>? Tags { get; set; }

        /// <summary>
        /// Maximum results to return
        /// </summary>
        public int MaxResults { get; set; } = 50;

        /// <summary>
        /// Sort order
        /// </summary>
        public VersionSortOrder SortOrder { get; set; } = VersionSortOrder.NewestFirst;
    }

    /// <summary>
    /// Version sort order options
    /// </summary>
    public enum VersionSortOrder
    {
        NewestFirst,
        OldestFirst,
        VersionNumberDesc,
        VersionNumberAsc,
        QualityScoreDesc,
        QualityScoreAsc,
        NameAsc,
        NameDesc
    }

    /// <summary>
    /// Code version statistics
    /// </summary>
    public class CodeVersionStatistics
    {
        /// <summary>
        /// Total versions
        /// </summary>
        public int TotalVersions { get; set; }

        /// <summary>
        /// Active versions
        /// </summary>
        public int ActiveVersions { get; set; }

        /// <summary>
        /// Successful executions
        /// </summary>
        public int SuccessfulExecutions { get; set; }

        /// <summary>
        /// Failed executions
        /// </summary>
        public int FailedExecutions { get; set; }

        /// <summary>
        /// Success rate percentage
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Average quality score
        /// </summary>
        public double AverageQualityScore { get; set; }

        /// <summary>
        /// Versions by branch
        /// </summary>
        public Dictionary<string, int> VersionsByBranch { get; set; } = new();

        /// <summary>
        /// Versions by device type
        /// </summary>
        public Dictionary<string, int> VersionsByDeviceType { get; set; } = new();

        /// <summary>
        /// Latest version
        /// </summary>
        public CodeVersion? LatestVersion { get; set; }

        /// <summary>
        /// Most active contributors
        /// </summary>
        public Dictionary<string, int> ContributorStats { get; set; } = new();
    }

    /// <summary>
    /// Version performance trend
    /// </summary>
    public class VersionPerformanceTrend
    {
        /// <summary>
        /// Date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Number of versions created
        /// </summary>
        public int VersionsCreated { get; set; }

        /// <summary>
        /// Success rate for that day
        /// </summary>
        public double SuccessRate { get; set; }

        /// <summary>
        /// Average quality score
        /// </summary>
        public double AverageQualityScore { get; set; }

        /// <summary>
        /// Average execution time
        /// </summary>
        public double AverageExecutionTime { get; set; }
    }
}