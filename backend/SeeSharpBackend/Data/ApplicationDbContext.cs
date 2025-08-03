using Microsoft.EntityFrameworkCore;
using SeeSharpBackend.Models;

namespace SeeSharpBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApiKeyConfiguration> ApiKeyConfigurations { get; set; }
        public DbSet<ApiUsageStatistics> ApiUsageStatistics { get; set; }
        public DbSet<TestExecutionRecord> TestExecutionRecords { get; set; }
        public DbSet<CodeTemplate> CodeTemplates { get; set; }
        public DbSet<CodeVersionRecord> CodeVersions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ApiKeyConfiguration
            modelBuilder.Entity<ApiKeyConfiguration>()
                .HasIndex(e => e.Provider)
                .HasDatabaseName("IX_ApiKeyConfiguration_Provider");

            // ApiUsageStatistics
            modelBuilder.Entity<ApiUsageStatistics>()
                .HasOne(a => a.ApiKeyConfiguration)
                .WithMany()
                .HasForeignKey(a => a.ApiKeyConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApiUsageStatistics>()
                .HasIndex(e => e.Timestamp)
                .HasDatabaseName("IX_ApiUsageStatistics_Timestamp");

            // TestExecutionRecord
            modelBuilder.Entity<TestExecutionRecord>()
                .HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_TestExecutionRecord_CreatedAt");
                
            modelBuilder.Entity<TestExecutionRecord>()
                .HasIndex(e => e.DeviceType)
                .HasDatabaseName("IX_TestExecutionRecord_DeviceType");
                
            modelBuilder.Entity<TestExecutionRecord>()
                .HasIndex(e => e.TestType)
                .HasDatabaseName("IX_TestExecutionRecord_TestType");
                
            modelBuilder.Entity<TestExecutionRecord>()
                .HasIndex(e => e.AIProvider)
                .HasDatabaseName("IX_TestExecutionRecord_AIProvider");
                
            modelBuilder.Entity<TestExecutionRecord>()
                .HasIndex(e => e.Success)
                .HasDatabaseName("IX_TestExecutionRecord_Success");

            // CodeTemplate
            modelBuilder.Entity<CodeTemplate>()
                .HasIndex(e => new { e.DeviceType, e.TestType })
                .HasDatabaseName("IX_CodeTemplate_DeviceType_TestType");

            // CodeVersionRecord
            modelBuilder.Entity<CodeVersionRecord>()
                .HasIndex(e => e.CodeBaseId)
                .HasDatabaseName("IX_CodeVersionRecord_CodeBaseId");
                
            modelBuilder.Entity<CodeVersionRecord>()
                .HasIndex(e => new { e.CodeBaseId, e.BranchName })
                .HasDatabaseName("IX_CodeVersionRecord_CodeBaseId_BranchName");
                
            modelBuilder.Entity<CodeVersionRecord>()
                .HasIndex(e => e.CodeHash)
                .HasDatabaseName("IX_CodeVersionRecord_CodeHash");
                
            modelBuilder.Entity<CodeVersionRecord>()
                .HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_CodeVersionRecord_CreatedAt");
                
            modelBuilder.Entity<CodeVersionRecord>()
                .HasIndex(e => e.CreatedBy)
                .HasDatabaseName("IX_CodeVersionRecord_CreatedBy");
                
            modelBuilder.Entity<CodeVersionRecord>()
                .HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_CodeVersionRecord_IsActive");
                
            modelBuilder.Entity<CodeVersionRecord>()
                .HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_CodeVersionRecord_IsDeleted");

            // Parent-child relationship for code versions
            modelBuilder.Entity<CodeVersionRecord>()
                .HasOne(e => e.ParentVersion)
                .WithMany(e => e.ChildVersions)
                .HasForeignKey(e => e.ParentVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}