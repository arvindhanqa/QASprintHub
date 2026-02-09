using QASprintHub.Models;
using System.Threading.Tasks;

namespace QASprintHub.Services;

public interface INotificationService
{
    void ShowWatcherNotification(Sprint sprint, TeamMember? nextWatcher = null);
    void ShowSwapNotification(TeamMember scheduledWatcher, TeamMember actualWatcher, Sprint sprint);
    void ShowBackupAssignedNotification(TeamMember backupMember, Sprint sprint);
    void ShowSprintEndingNotification(Sprint sprint, int daysRemaining);
}
