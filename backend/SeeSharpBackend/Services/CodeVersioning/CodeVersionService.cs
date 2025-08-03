using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeeSharpBackend.Data;
using SeeSharpBackend.Models;

namespace SeeSharpBackend.Services.CodeVersioning
{
    /// <summary>
    /// Code version management service implementation
    /// </summary>
    public class CodeVersionService : ICodeVersionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CodeVersionService> _logger;

        public CodeVersionService(
            ApplicationDbContext context,
            ILogger<CodeVersionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> CreateVersionAsync(CodeVersionRequest request)
        {
            try
            {
                // Get next version number
                var nextVersionNumber = await GetNextVersionNumberAsync(request.CodeBaseId, request.BranchName);

                // Generate code hash
                var codeHash = GenerateCodeHash(request.SourceCode);

                // Check if identical code already exists
                var existingVersion = await _context.CodeVersions
                    .Where(v => v.CodeBaseId == request.CodeBaseId && 
                               v.CodeHash == codeHash && 
                               !v.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingVersion != null)
                {
                    _logger.LogWarning($"Identical code version already exists: {existingVersion.Id}");
                    return existingVersion.Id;
                }

                var version = new CodeVersionRecord
                {
                    CodeBaseId = request.CodeBaseId,
                    VersionName = request.VersionName,
                    VersionNumber = nextVersionNumber,
                    SourceCode = request.SourceCode,
                    CodeHash = codeHash,
                    Description = request.Description,
                    CreatedBy = request.CreatedBy,
                    ParentVersionId = request.ParentVersionId,
                    BranchName = request.BranchName ?? "main",
                    TestRequirement = request.TestRequirement,
                    DeviceType = request.DeviceType,
                    TestType = request.TestType,
                    Tags = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.CodeVersions.Add(version);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created code version {version.Id} for codebase {request.CodeBaseId}");
                return version.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating code version");
                throw;
            }
        }

        public async Task<CodeVersion?> GetVersionAsync(int versionId)
        {
            try
            {
                var record = await _context.CodeVersions
                    .Where(v => v.Id == versionId && !v.IsDeleted)
                    .FirstOrDefaultAsync();

                return record != null ? MapToCodeVersion(record) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting code version {versionId}");
                throw;
            }
        }

        public async Task<List<CodeVersion>> GetVersionHistoryAsync(string codeBaseId, int maxCount = 50)
        {
            try
            {
                var records = await _context.CodeVersions
                    .Where(v => v.CodeBaseId == codeBaseId && !v.IsDeleted)
                    .OrderByDescending(v => v.CreatedAt)
                    .Take(maxCount)
                    .ToListAsync();

                return records.Select(MapToCodeVersion).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting version history for codebase {codeBaseId}");
                throw;
            }
        }

        public async Task<CodeVersion?> GetLatestVersionAsync(string codeBaseId)
        {
            try
            {
                var record = await _context.CodeVersions
                    .Where(v => v.CodeBaseId == codeBaseId && v.IsActive && !v.IsDeleted)
                    .OrderByDescending(v => v.VersionNumber)
                    .FirstOrDefaultAsync();

                return record != null ? MapToCodeVersion(record) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting latest version for codebase {codeBaseId}");
                throw;
            }
        }

        public async Task<bool> UpdateVersionResultsAsync(int versionId, string resultData, bool success, string? errorMessage = null)
        {
            try
            {
                var version = await _context.CodeVersions.FindAsync(versionId);
                if (version == null) return false;

                version.ResultData = resultData;
                version.ExecutionSuccess = success;
                version.ExecutionError = errorMessage;
                version.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated execution results for version {versionId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating version results {versionId}");
                throw;
            }
        }

        public async Task<CodeVersionComparison> CompareVersionsAsync(int version1Id, int version2Id)
        {
            try
            {
                var version1 = await GetVersionAsync(version1Id);
                var version2 = await GetVersionAsync(version2Id);

                if (version1 == null || version2 == null)
                {
                    throw new ArgumentException("One or both versions not found");
                }

                var comparison = new CodeVersionComparison
                {
                    SourceVersion = version1,
                    TargetVersion = version2
                };

                // Simple line-by-line comparison
                var lines1 = version1.SourceCode.Split('\n');
                var lines2 = version2.SourceCode.Split('\n');

                var differences = new List<CodeDifference>();
                var maxLines = Math.Max(lines1.Length, lines2.Length);
                
                int linesAdded = 0, linesDeleted = 0, linesModified = 0;

                for (int i = 0; i < maxLines; i++)
                {
                    var line1 = i < lines1.Length ? lines1[i] : null;
                    var line2 = i < lines2.Length ? lines2[i] : null;

                    if (line1 == null && line2 != null)
                    {
                        // Line added
                        differences.Add(new CodeDifference
                        {
                            Type = DifferenceType.Added,
                            TargetLineNumber = i + 1,
                            TargetContent = line2
                        });
                        linesAdded++;
                    }
                    else if (line1 != null && line2 == null)
                    {
                        // Line deleted
                        differences.Add(new CodeDifference
                        {
                            Type = DifferenceType.Deleted,
                            SourceLineNumber = i + 1,
                            SourceContent = line1
                        });
                        linesDeleted++;
                    }
                    else if (line1 != null && line2 != null && line1 != line2)
                    {
                        // Line modified
                        differences.Add(new CodeDifference
                        {
                            Type = DifferenceType.Modified,
                            SourceLineNumber = i + 1,
                            TargetLineNumber = i + 1,
                            SourceContent = line1,
                            TargetContent = line2
                        });
                        linesModified++;
                    }
                }

                comparison.Differences = differences;
                comparison.LinesAdded = linesAdded;
                comparison.LinesDeleted = linesDeleted;
                comparison.LinesModified = linesModified;

                // Calculate similarity percentage
                var totalChanges = linesAdded + linesDeleted + linesModified;
                var totalLines = Math.Max(lines1.Length, lines2.Length);
                comparison.SimilarityPercentage = totalLines > 0 ? 
                    (1.0 - (double)totalChanges / totalLines) * 100 : 100;

                return comparison;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error comparing versions {version1Id} and {version2Id}");
                throw;
            }
        }

        public async Task<int> RestoreVersionAsync(int versionId, string restoredBy, string? notes = null)
        {
            try
            {
                var sourceVersion = await GetVersionAsync(versionId);
                if (sourceVersion == null)
                {
                    throw new ArgumentException("Source version not found");
                }

                var restoreRequest = new CodeVersionRequest
                {
                    CodeBaseId = sourceVersion.CodeBaseId,
                    VersionName = $"Restore-{sourceVersion.VersionName}-{DateTime.Now:yyyyMMdd-HHmmss}",
                    SourceCode = sourceVersion.SourceCode,
                    Description = $"Restored from version {sourceVersion.VersionNumber}. {notes}",
                    CreatedBy = restoredBy,
                    ParentVersionId = versionId,
                    BranchName = sourceVersion.BranchName,
                    TestRequirement = sourceVersion.TestRequirement,
                    DeviceType = sourceVersion.DeviceType,
                    TestType = sourceVersion.TestType
                };

                var newVersionId = await CreateVersionAsync(restoreRequest);
                
                _logger.LogInformation($"Restored version {versionId} as new version {newVersionId}");
                return newVersionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error restoring version {versionId}");
                throw;
            }
        }

        public async Task<bool> DeleteVersionAsync(int versionId)
        {
            try
            {
                var version = await _context.CodeVersions.FindAsync(versionId);
                if (version == null) return false;

                version.IsDeleted = true;
                version.IsActive = false;
                version.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted version {versionId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting version {versionId}");
                throw;
            }
        }

        public async Task<CodeVersionStatistics> GetVersionStatisticsAsync(string codeBaseId)
        {
            try
            {
                var versions = await _context.CodeVersions
                    .Where(v => v.CodeBaseId == codeBaseId && !v.IsDeleted)
                    .ToListAsync();

                var statistics = new CodeVersionStatistics
                {
                    TotalVersions = versions.Count,
                    ActiveVersions = versions.Count(v => v.IsActive),
                    SuccessfulExecutions = versions.Count(v => v.ExecutionSuccess == true),
                    FailedExecutions = versions.Count(v => v.ExecutionSuccess == false)
                };

                if (versions.Any())
                {
                    statistics.SuccessRate = versions.Count(v => v.ExecutionSuccess.HasValue) > 0 ?
                        (double)statistics.SuccessfulExecutions / versions.Count(v => v.ExecutionSuccess.HasValue) * 100 : 0;

                    var qualityScores = versions.Where(v => v.QualityScore.HasValue).Select(v => v.QualityScore!.Value);
                    statistics.AverageQualityScore = qualityScores.Any() ? qualityScores.Average() : 0;

                    // Group by branch
                    statistics.VersionsByBranch = versions
                        .GroupBy(v => v.BranchName ?? "main")
                        .ToDictionary(g => g.Key, g => g.Count());

                    // Group by device type
                    statistics.VersionsByDeviceType = versions
                        .Where(v => !string.IsNullOrEmpty(v.DeviceType))
                        .GroupBy(v => v.DeviceType!)
                        .ToDictionary(g => g.Key, g => g.Count());

                    // Contributor stats
                    statistics.ContributorStats = versions
                        .GroupBy(v => v.CreatedBy)
                        .ToDictionary(g => g.Key, g => g.Count());

                    // Latest version
                    var latestRecord = versions
                        .Where(v => v.IsActive)
                        .OrderByDescending(v => v.VersionNumber)
                        .FirstOrDefault();

                    if (latestRecord != null)
                    {
                        statistics.LatestVersion = MapToCodeVersion(latestRecord);
                    }
                }

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting version statistics for codebase {codeBaseId}");
                throw;
            }
        }

        public async Task<List<CodeVersion>> SearchVersionsAsync(CodeVersionSearchQuery query)
        {
            try
            {
                var queryable = _context.CodeVersions.Where(v => !v.IsDeleted);

                // Apply filters
                if (!string.IsNullOrEmpty(query.CodeBaseId))
                    queryable = queryable.Where(v => v.CodeBaseId == query.CodeBaseId);

                if (!string.IsNullOrEmpty(query.VersionNameContains))
                    queryable = queryable.Where(v => v.VersionName.Contains(query.VersionNameContains));

                if (!string.IsNullOrEmpty(query.CreatedBy))
                    queryable = queryable.Where(v => v.CreatedBy == query.CreatedBy);

                if (!string.IsNullOrEmpty(query.BranchName))
                    queryable = queryable.Where(v => v.BranchName == query.BranchName);

                if (!string.IsNullOrEmpty(query.DeviceType))
                    queryable = queryable.Where(v => v.DeviceType == query.DeviceType);

                if (!string.IsNullOrEmpty(query.TestType))
                    queryable = queryable.Where(v => v.TestType == query.TestType);

                if (query.ExecutionSuccess.HasValue)
                    queryable = queryable.Where(v => v.ExecutionSuccess == query.ExecutionSuccess.Value);

                if (query.MinQualityScore.HasValue)
                    queryable = queryable.Where(v => v.QualityScore >= query.MinQualityScore.Value);

                if (query.StartDate.HasValue)
                    queryable = queryable.Where(v => v.CreatedAt >= query.StartDate.Value);

                if (query.EndDate.HasValue)
                    queryable = queryable.Where(v => v.CreatedAt <= query.EndDate.Value);

                // Apply sorting
                queryable = query.SortOrder switch
                {
                    VersionSortOrder.NewestFirst => queryable.OrderByDescending(v => v.CreatedAt),
                    VersionSortOrder.OldestFirst => queryable.OrderBy(v => v.CreatedAt),
                    VersionSortOrder.VersionNumberDesc => queryable.OrderByDescending(v => v.VersionNumber),
                    VersionSortOrder.VersionNumberAsc => queryable.OrderBy(v => v.VersionNumber),
                    VersionSortOrder.QualityScoreDesc => queryable.OrderByDescending(v => v.QualityScore),
                    VersionSortOrder.QualityScoreAsc => queryable.OrderBy(v => v.QualityScore),
                    VersionSortOrder.NameAsc => queryable.OrderBy(v => v.VersionName),
                    VersionSortOrder.NameDesc => queryable.OrderByDescending(v => v.VersionName),
                    _ => queryable.OrderByDescending(v => v.CreatedAt)
                };

                var records = await queryable.Take(query.MaxResults).ToListAsync();
                return records.Select(MapToCodeVersion).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching versions");
                throw;
            }
        }

        public async Task<int> CreateVersionBranchAsync(int sourceVersionId, string branchName, string createdBy, string? notes = null)
        {
            try
            {
                var sourceVersion = await GetVersionAsync(sourceVersionId);
                if (sourceVersion == null)
                {
                    throw new ArgumentException("Source version not found");
                }

                var branchRequest = new CodeVersionRequest
                {
                    CodeBaseId = sourceVersion.CodeBaseId,
                    VersionName = $"{branchName}-1",
                    SourceCode = sourceVersion.SourceCode,
                    Description = $"Branch '{branchName}' created from version {sourceVersion.VersionNumber}. {notes}",
                    CreatedBy = createdBy,
                    ParentVersionId = sourceVersionId,
                    BranchName = branchName,
                    TestRequirement = sourceVersion.TestRequirement,
                    DeviceType = sourceVersion.DeviceType,
                    TestType = sourceVersion.TestType
                };

                var newVersionId = await CreateVersionAsync(branchRequest);
                
                _logger.LogInformation($"Created branch '{branchName}' from version {sourceVersionId} as version {newVersionId}");
                return newVersionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating branch from version {sourceVersionId}");
                throw;
            }
        }

        public async Task<List<VersionPerformanceTrend>> GetPerformanceTrendsAsync(string codeBaseId, int days = 30)
        {
            try
            {
                var startDate = DateTime.Now.AddDays(-days);
                var versions = await _context.CodeVersions
                    .Where(v => v.CodeBaseId == codeBaseId && 
                               v.CreatedAt >= startDate && 
                               !v.IsDeleted)
                    .ToListAsync();

                var trends = versions
                    .GroupBy(v => v.CreatedAt.Date)
                    .Select(g => new VersionPerformanceTrend
                    {
                        Date = g.Key,
                        VersionsCreated = g.Count(),
                        SuccessRate = g.Count(v => v.ExecutionSuccess.HasValue) > 0 ?
                            (double)g.Count(v => v.ExecutionSuccess == true) / g.Count(v => v.ExecutionSuccess.HasValue) * 100 : 0,
                        AverageQualityScore = g.Where(v => v.QualityScore.HasValue).Select(v => v.QualityScore!.Value).DefaultIfEmpty(0).Average(),
                        AverageExecutionTime = g.Where(v => v.ExecutionTimeMs.HasValue).Select(v => v.ExecutionTimeMs!.Value).DefaultIfEmpty(0).Average()
                    })
                    .OrderBy(t => t.Date)
                    .ToList();

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting performance trends for codebase {codeBaseId}");
                throw;
            }
        }

        #region Private Methods

        private async Task<int> GetNextVersionNumberAsync(string codeBaseId, string? branchName)
        {
            var branch = branchName ?? "main";
            var maxVersion = await _context.CodeVersions
                .Where(v => v.CodeBaseId == codeBaseId && v.BranchName == branch && !v.IsDeleted)
                .MaxAsync(v => (int?)v.VersionNumber);

            return (maxVersion ?? 0) + 1;
        }

        private static string GenerateCodeHash(string sourceCode)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(sourceCode);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private static CodeVersion MapToCodeVersion(CodeVersionRecord record)
        {
            return new CodeVersion
            {
                Id = record.Id,
                CodeBaseId = record.CodeBaseId,
                VersionName = record.VersionName,
                VersionNumber = record.VersionNumber,
                SourceCode = record.SourceCode,
                CodeHash = record.CodeHash,
                Description = record.Description,
                CreatedBy = record.CreatedBy,
                ParentVersionId = record.ParentVersionId,
                BranchName = record.BranchName,
                TestRequirement = record.TestRequirement,
                DeviceType = record.DeviceType,
                TestType = record.TestType,
                ResultData = record.ResultData,
                ExecutionSuccess = record.ExecutionSuccess,
                ExecutionError = record.ExecutionError,
                ExecutionTimeMs = record.ExecutionTimeMs,
                QualityScore = record.QualityScore,
                Tags = record.Tags,
                IsActive = record.IsActive,
                IsDeleted = record.IsDeleted,
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            };
        }

        #endregion
    }
}