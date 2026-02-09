using QASprintHub.Models.Enums;
using System;
using System.Collections.Generic;

namespace QASprintHub.Models;

public class TeamMember
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public string? Email { get; set; }

    public int RotationOrder { get; set; }

    public MemberStatus Status { get; set; } = MemberStatus.Active;

    public DateTime? DepartedDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // For future sync support
    public Guid SyncId { get; set; } = Guid.NewGuid();

    // Navigation properties
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    public ICollection<BackupWatcher> BackupAssignments { get; set; } = new List<BackupWatcher>();
    public ICollection<WatcherSwap> ScheduledSwaps { get; set; } = new List<WatcherSwap>();
    public ICollection<WatcherSwap> ActualSwaps { get; set; } = new List<WatcherSwap>();
}
