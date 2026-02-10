using System;

namespace QASprintHub.Models;

public class AppSettings
{
    public int Id { get; set; }
    public int SprintDurationDays { get; set; } = 10;
    public DateTime? FirstSprintStartDate { get; set; }
    public bool IsConfigured { get; set; } = false;

    // App Behavior
    public bool MinimizeToTray { get; set; } = true;
    public bool StartMinimized { get; set; } = false;
    public bool LaunchOnStartup { get; set; } = false;

    // Notifications
    public bool ShowWatcherNotification { get; set; } = true;
    public bool ShowSprintEndingNotification { get; set; } = true;
    public bool ShowSwapNotification { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;
}
