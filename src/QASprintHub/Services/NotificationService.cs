using QASprintHub.Models;
using System;
using System.Diagnostics;

namespace QASprintHub.Services;

public class NotificationService : INotificationService
{
    public void ShowWatcherNotification(Sprint sprint, TeamMember? nextWatcher = null)
    {
        if (sprint.Watcher == null) return;

        try
        {
            var message = $"[Notification] {sprint.Watcher.Name} is the QA Watcher for {sprint.DisplayName}";
            if (nextWatcher != null)
            {
                message += $" | Next: {nextWatcher.Name}";
            }
            Debug.WriteLine(message);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Notification error: {ex.Message}");
        }
    }

    public void ShowSwapNotification(TeamMember scheduledWatcher, TeamMember actualWatcher, Sprint sprint)
    {
        try
        {
            Debug.WriteLine($"[Swap] {scheduledWatcher.Name} -> {actualWatcher.Name} (Sprint: {sprint.DisplayName})");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Notification error: {ex.Message}");
        }
    }

    public void ShowBackupAssignedNotification(TeamMember backupMember, Sprint sprint)
    {
        try
        {
            Debug.WriteLine($"[Backup Assigned] {backupMember.Name} assigned for {sprint.DisplayName}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Notification error: {ex.Message}");
        }
    }

    public void ShowSprintEndingNotification(Sprint sprint, int daysRemaining)
    {
        try
        {
            Debug.WriteLine($"[Sprint Ending] {sprint.DisplayName} ends in {daysRemaining} working day{(daysRemaining != 1 ? "s" : "")}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Notification error: {ex.Message}");
        }
    }
}
