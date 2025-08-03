using Microsoft.AspNetCore.Mvc;
using SeeSharpBackend.Services.CodeVersioning;
using System.ComponentModel.DataAnnotations;

namespace SeeSharpBackend.Controllers
{
    /// <summary>
    /// Code version management API controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CodeVersionController : ControllerBase
    {
        private readonly ICodeVersionService _codeVersionService;
        private readonly ILogger<CodeVersionController> _logger;

        public CodeVersionController(
            ICodeVersionService codeVersionService,
            ILogger<CodeVersionController> logger)
        {
            _codeVersionService = codeVersionService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new code version
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<int>> CreateVersion([FromBody] CodeVersionRequest request)
        {
            try
            {
                var versionId = await _codeVersionService.CreateVersionAsync(request);
                return Ok(versionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating code version");
                return StatusCode(500, new { error = "Failed to create code version", details = ex.Message });
            }
        }

        /// <summary>
        /// Get code version by ID
        /// </summary>
        [HttpGet("{versionId}")]
        public async Task<ActionResult> GetVersion(int versionId)
        {
            try
            {
                var version = await _codeVersionService.GetVersionAsync(versionId);
                if (version == null)
                {
                    return NotFound(new { error = "Code version not found" });
                }
                return Ok(version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting code version {versionId}");
                return StatusCode(500, new { error = "Failed to get code version", details = ex.Message });
            }
        }

        /// <summary>
        /// Get version history for a code base
        /// </summary>
        [HttpGet("history/{codeBaseId}")]
        public async Task<ActionResult> GetVersionHistory(
            string codeBaseId,
            [FromQuery] int maxCount = 50)
        {
            try
            {
                var versions = await _codeVersionService.GetVersionHistoryAsync(codeBaseId, maxCount);
                return Ok(versions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting version history for {codeBaseId}");
                return StatusCode(500, new { error = "Failed to get version history", details = ex.Message });
            }
        }

        /// <summary>
        /// Get latest version of a code base
        /// </summary>
        [HttpGet("latest/{codeBaseId}")]
        public async Task<ActionResult> GetLatestVersion(string codeBaseId)
        {
            try
            {
                var version = await _codeVersionService.GetLatestVersionAsync(codeBaseId);
                if (version == null)
                {
                    return NotFound(new { error = "No versions found for this code base" });
                }
                return Ok(version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting latest version for {codeBaseId}");
                return StatusCode(500, new { error = "Failed to get latest version", details = ex.Message });
            }
        }

        /// <summary>
        /// Update version with execution results
        /// </summary>
        [HttpPut("{versionId}/results")]
        public async Task<ActionResult> UpdateVersionResults(
            int versionId,
            [FromBody] UpdateVersionResultsRequest request)
        {
            try
            {
                var updated = await _codeVersionService.UpdateVersionResultsAsync(
                    versionId, 
                    request.ResultData, 
                    request.Success, 
                    request.ErrorMessage);
                
                if (!updated)
                {
                    return NotFound(new { error = "Version not found" });
                }
                
                return Ok(new { message = "Version results updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating version results {versionId}");
                return StatusCode(500, new { error = "Failed to update version results", details = ex.Message });
            }
        }

        /// <summary>
        /// Compare two code versions
        /// </summary>
        [HttpGet("compare/{version1Id}/{version2Id}")]
        public async Task<ActionResult> CompareVersions(int version1Id, int version2Id)
        {
            try
            {
                var comparison = await _codeVersionService.CompareVersionsAsync(version1Id, version2Id);
                return Ok(comparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error comparing versions {version1Id} and {version2Id}");
                return StatusCode(500, new { error = "Failed to compare versions", details = ex.Message });
            }
        }

        /// <summary>
        /// Restore a previous version
        /// </summary>
        [HttpPost("{versionId}/restore")]
        public async Task<ActionResult<int>> RestoreVersion(
            int versionId,
            [FromBody] RestoreVersionRequest request)
        {
            try
            {
                var newVersionId = await _codeVersionService.RestoreVersionAsync(
                    versionId, 
                    request.RestoredBy, 
                    request.Notes);
                
                return Ok(newVersionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error restoring version {versionId}");
                return StatusCode(500, new { error = "Failed to restore version", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a code version
        /// </summary>
        [HttpDelete("{versionId}")]
        public async Task<ActionResult> DeleteVersion(int versionId)
        {
            try
            {
                var deleted = await _codeVersionService.DeleteVersionAsync(versionId);
                if (!deleted)
                {
                    return NotFound(new { error = "Version not found" });
                }
                return Ok(new { message = "Version deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting version {versionId}");
                return StatusCode(500, new { error = "Failed to delete version", details = ex.Message });
            }
        }

        /// <summary>
        /// Get version statistics for a code base
        /// </summary>
        [HttpGet("statistics/{codeBaseId}")]
        public async Task<ActionResult> GetVersionStatistics(string codeBaseId)
        {
            try
            {
                var statistics = await _codeVersionService.GetVersionStatisticsAsync(codeBaseId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting version statistics for {codeBaseId}");
                return StatusCode(500, new { error = "Failed to get version statistics", details = ex.Message });
            }
        }

        /// <summary>
        /// Search versions by criteria
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult> SearchVersions([FromBody] CodeVersionSearchQuery query)
        {
            try
            {
                var versions = await _codeVersionService.SearchVersionsAsync(query);
                return Ok(versions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching versions");
                return StatusCode(500, new { error = "Failed to search versions", details = ex.Message });
            }
        }

        /// <summary>
        /// Create a branch from existing version
        /// </summary>
        [HttpPost("{versionId}/branch")]
        public async Task<ActionResult<int>> CreateVersionBranch(
            int versionId,
            [FromBody] CreateBranchRequest request)
        {
            try
            {
                var newVersionId = await _codeVersionService.CreateVersionBranchAsync(
                    versionId, 
                    request.BranchName, 
                    request.CreatedBy, 
                    request.Notes);
                
                return Ok(newVersionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating branch from version {versionId}");
                return StatusCode(500, new { error = "Failed to create branch", details = ex.Message });
            }
        }

        /// <summary>
        /// Get performance trends for a code base
        /// </summary>
        [HttpGet("trends/{codeBaseId}")]
        public async Task<ActionResult> GetPerformanceTrends(
            string codeBaseId,
            [FromQuery] int days = 30)
        {
            try
            {
                var trends = await _codeVersionService.GetPerformanceTrendsAsync(codeBaseId, days);
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting performance trends for {codeBaseId}");
                return StatusCode(500, new { error = "Failed to get performance trends", details = ex.Message });
            }
        }

        /// <summary>
        /// Get version tree (hierarchy) for a code base
        /// </summary>
        [HttpGet("tree/{codeBaseId}")]
        public async Task<ActionResult> GetVersionTree(string codeBaseId)
        {
            try
            {
                // Get all versions and build tree structure
                var versions = await _codeVersionService.GetVersionHistoryAsync(codeBaseId, 1000);
                var tree = BuildVersionTree(versions);
                return Ok(tree);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting version tree for {codeBaseId}");
                return StatusCode(500, new { error = "Failed to get version tree", details = ex.Message });
            }
        }

        /// <summary>
        /// Get code differences between two versions
        /// </summary>
        [HttpGet("diff/{version1Id}/{version2Id}")]
        public async Task<ActionResult> GetVersionDiff(int version1Id, int version2Id)
        {
            try
            {
                var comparison = await _codeVersionService.CompareVersionsAsync(version1Id, version2Id);
                return Ok(new
                {
                    comparison.SimilarityPercentage,
                    comparison.LinesAdded,
                    comparison.LinesDeleted,
                    comparison.LinesModified,
                    differences = comparison.Differences.Take(100) // Limit for performance
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting version diff {version1Id} vs {version2Id}");
                return StatusCode(500, new { error = "Failed to get version diff", details = ex.Message });
            }
        }

        #region Private Methods

        private static object BuildVersionTree(List<CodeVersion> versions)
        {
            var versionDict = versions.ToDictionary(v => v.Id);
            var rootVersions = versions.Where(v => v.ParentVersionId == null).ToList();
            
            return new
            {
                roots = rootVersions.Select(v => BuildVersionNode(v, versionDict)).ToList()
            };
        }

        private static object BuildVersionNode(CodeVersion version, Dictionary<int, CodeVersion> versionDict)
        {
            var children = versionDict.Values
                .Where(v => v.ParentVersionId == version.Id)
                .Select(v => BuildVersionNode(v, versionDict))
                .ToList();

            return new
            {
                version.Id,
                version.VersionName,
                version.VersionNumber,
                version.BranchName,
                version.CreatedBy,
                version.CreatedAt,
                version.ExecutionSuccess,
                version.QualityScore,
                children
            };
        }

        #endregion
    }

    /// <summary>
    /// Update version results request
    /// </summary>
    public class UpdateVersionResultsRequest
    {
        /// <summary>
        /// Result data (JSON)
        /// </summary>
        [Required]
        public string ResultData { get; set; } = string.Empty;

        /// <summary>
        /// Success flag
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Restore version request
    /// </summary>
    public class RestoreVersionRequest
    {
        /// <summary>
        /// User performing the restore
        /// </summary>
        [Required]
        public string RestoredBy { get; set; } = string.Empty;

        /// <summary>
        /// Restore notes
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Create branch request
    /// </summary>
    public class CreateBranchRequest
    {
        /// <summary>
        /// Branch name
        /// </summary>
        [Required]
        public string BranchName { get; set; } = string.Empty;

        /// <summary>
        /// User creating the branch
        /// </summary>
        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Branch notes
        /// </summary>
        public string? Notes { get; set; }
    }
}