using Microsoft.EntityFrameworkCore;
using QASprintHub.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace QASprintHub.Data;

public class AppDbContext : DbContext
{
    public DbSet<TeamMember> TeamMembers { get; set; } = null!;
    public DbSet<Sprint> Sprints { get; set; } = null!;
    public DbSet<BackupWatcher> BackupWatchers { get; set; } = null!;
    public DbSet<WatcherSwap> WatcherSwaps { get; set; } = null!;
    public DbSet<SprintPR> SprintPRs { get; set; } = null!;
    public DbSet<AppSettings> AppSettings { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Get database path from %APPDATA%
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbDirectory = Path.Combine(appDataPath, "QASprintHub");

            // Ensure directory exists
            Directory.CreateDirectory(dbDirectory);

            var dbPath = Path.Combine(dbDirectory, "qasprinthub.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath};Mode=ReadWriteCreate;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TeamMember configuration
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.HasIndex(e => e.RotationOrder);
            entity.HasIndex(e => e.Status);
        });

        // Sprint configuration
        modelBuilder.Entity<Sprint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Watcher)
                  .WithMany(w => w.Sprints)
                  .HasForeignKey(e => e.WatcherId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.Status);
        });

        // BackupWatcher configuration
        modelBuilder.Entity<BackupWatcher>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Sprint)
                  .WithMany(s => s.BackupWatchers)
                  .HasForeignKey(e => e.SprintId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.BackupMember)
                  .WithMany(m => m.BackupAssignments)
                  .HasForeignKey(e => e.BackupMemberId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // WatcherSwap configuration
        modelBuilder.Entity<WatcherSwap>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.Sprint)
                  .WithMany(s => s.WatcherSwaps)
                  .HasForeignKey(e => e.SprintId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ScheduledWatcher)
                  .WithMany(m => m.ScheduledSwaps)
                  .HasForeignKey(e => e.ScheduledWatcherId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ActualWatcher)
                  .WithMany(m => m.ActualSwaps)
                  .HasForeignKey(e => e.ActualWatcherId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // SprintPR configuration
        modelBuilder.Entity<SprintPR>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Link).HasMaxLength(2000);
            entity.Property(e => e.Author).HasMaxLength(200);
            entity.HasOne(e => e.Sprint)
                  .WithMany(s => s.SprintPRs)
                  .HasForeignKey(e => e.SprintId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.Status);
        });
    }

    public override int SaveChanges()
    {
        // Configure SQLite for durability on first save
        ConfigureSqliteDurability();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Configure SQLite for durability on first save
        ConfigureSqliteDurability();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private bool _sqliteConfigured = false;
    private void ConfigureSqliteDurability()
    {
        if (_sqliteConfigured) return;

        try
        {
            // Enable Write-Ahead Logging for crash-safe writes
            Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");

            // Flush to disk on every commit - no data loss on power failure
            Database.ExecuteSqlRaw("PRAGMA synchronous=FULL;");

            // Wait 5 seconds if DB is locked
            Database.ExecuteSqlRaw("PRAGMA busy_timeout=5000;");

            // Enforce referential integrity
            Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");

            _sqliteConfigured = true;
        }
        catch
        {
            // Ignore errors - might already be configured
        }
    }
}
