using System;

namespace QASprintHub.Models;

public class WatcherSwap
{
    public int Id { get; set; }

    public int SprintId { get; set; }

    public int ScheduledWatcherId { get; set; }

    public int ActualWatcherId { get; set; }

    public DateTime SwapDate { get; set; }

    public required string Reason { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // For future sync support
    public Guid SyncId { get; set; } = Guid.NewGuid();

    // Navigation properties
    public Sprint? Sprint { get; set; }
    public TeamMember? ScheduledWatcher { get; set; }
    public TeamMember? ActualWatcher { get; set; }
}
