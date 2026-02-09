using QASprintHub.Models.Enums;
using System;

namespace QASprintHub.Models;

public class SprintPR
{
    public int Id { get; set; }

    public int SprintId { get; set; }

    public required string Title { get; set; }

    public string? Link { get; set; }

    public string? Author { get; set; }

    public PRStatus Status { get; set; } = PRStatus.Pending;

    public PRPriority Priority { get; set; } = PRPriority.Normal;

    public string? Notes { get; set; }

    public DateTime AddedDate { get; set; } = DateTime.Now;

    public DateTime StatusChangedDate { get; set; } = DateTime.Now;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // For future sync support
    public Guid SyncId { get; set; } = Guid.NewGuid();

    // Navigation properties
    public Sprint? Sprint { get; set; }
}
