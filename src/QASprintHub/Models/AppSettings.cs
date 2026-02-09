using System;

namespace QASprintHub.Models;

public class AppSettings
{
    public int Id { get; set; }
    public int SprintDurationDays { get; set; } = 10;
    public DateTime? FirstSprintStartDate { get; set; }
    public bool IsConfigured { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;
}
