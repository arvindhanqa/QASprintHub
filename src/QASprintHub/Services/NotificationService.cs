using Microsoft.Toolkit.Uwp.Notifications;
using QASprintHub.Models;
using System;

namespace QASprintHub.Services;

public class NotificationService : INotificationService
{
    public void ShowWatcherNotification(Sprint sprint, TeamMember? nextWatcher = null)
    {
        if (sprint.Watcher == null) return;

        var builder = new ToastContentBuilder()
            .AddText("🛡️ QA Sprint Hub")
            .AddText($"{sprint.Watcher.Name} is the QA Watcher")
            .AddText($"for {sprint.DisplayName}");

        if (nextWatcher != null)
        {
            builder.AddText($"Next watcher: {nextWatcher.Name}");
        }

        builder.Show();
    }

    public void ShowSwapNotification(TeamMember scheduledWatcher, TeamMember actualWatcher, Sprint sprint)
    {
        new ToastContentBuilder()
            .AddText("Watcher Swap")
            .AddText($"{scheduledWatcher.Name} → {actualWatcher.Name}")
            .AddText($"Sprint: {sprint.DisplayName}")
            .Show();
    }

    public void ShowBackupAssignedNotification(TeamMember backupMember, Sprint sprint)
    {
        new ToastContentBuilder()
            .AddText("Backup Watcher Assigned")
            .AddText($"{backupMember.Name} is backup watcher")
            .AddText($"Sprint: {sprint.DisplayName}")
            .Show();
    }

    public void ShowSprintEndingNotification(Sprint sprint, int daysRemaining)
    {
        new ToastContentBuilder()
            .AddText("Sprint Ending Soon")
            .AddText($"Sprint ends in {daysRemaining} working day{(daysRemaining != 1 ? "s" : "")}")
            .AddText($"{sprint.DisplayName}")
            .Show();
    }
}
