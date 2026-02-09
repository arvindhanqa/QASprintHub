using QASprintHub.Models.Enums;
using System;
using System.Collections.Generic;

namespace QASprintHub.Models;

public class Sprint
{
    public int Id { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int WatcherId { get; set; }

    public SprintStatus Status { get; set; } = SprintStatus.Planning;

    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // For future sync support
    public Guid SyncId { get; set; } = Guid.NewGuid();

    // Navigation properties
    public TeamMember? Watcher { get; set; }
    public ICollection<BackupWatcher> BackupWatchers { get; set; } = new List<BackupWatcher>();
    public ICollection<WatcherSwap> WatcherSwaps { get; set; } = new List<WatcherSwap>();
    public ICollection<SprintPR> SprintPRs { get; set; } = new List<SprintPR>();

    // Display name is auto-generated from dates (not stored in DB)
    public string DisplayName => $"{StartDate:MMM d} – {EndDate:MMM d, yyyy}";
}
