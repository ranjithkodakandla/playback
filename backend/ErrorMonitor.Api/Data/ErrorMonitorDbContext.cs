using ErrorMonitor.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ErrorMonitor.Api.Data;

public class ErrorMonitorDbContext(DbContextOptions<ErrorMonitorDbContext> options) : DbContext(options)
{
    public DbSet<ErrorIssue> ErrorIssues => Set<ErrorIssue>();
    public DbSet<ErrorEvent> ErrorEvents => Set<ErrorEvent>();
    public DbSet<SourceMapFile> SourceMaps => Set<SourceMapFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ErrorIssue>()
            .HasIndex(x => x.Fingerprint)
            .IsUnique();

        modelBuilder.Entity<ErrorIssue>()
            .HasMany(x => x.Events)
            .WithOne(x => x.ErrorIssue)
            .HasForeignKey(x => x.ErrorIssueId);

        modelBuilder.Entity<SourceMapFile>()
            .HasIndex(x => new { x.Release, x.MinifiedFileUrl })
            .IsUnique();

        modelBuilder.Entity<ErrorEvent>()
            .HasIndex(x => x.TimestampUtc);

        modelBuilder.Entity<ErrorEvent>()
            .Property(x => x.ContextJson)
            .HasColumnType("jsonb");
    }
}
