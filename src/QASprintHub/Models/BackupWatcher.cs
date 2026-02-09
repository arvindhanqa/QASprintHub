using QASprintHub.Models.Enums;
using System;

namespace QASprintHub.Models;

public class BackupWatcher
{
    public int Id { get; set; }

    public int SprintId { get; set; }

    public int BackupMemberId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public CoverageType CoverageType { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // For future sync support
    public Guid SyncId { get; set; } = Guid.NewGuid();

    // Navigation properties
    public Sprint? Sprint { get; set; }
    public TeamMember? BackupMember { get; set; }
}
