using QASprintHub.Models;
using QASprintHub.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QASprintHub.Services;

public interface IWatcherService
{
    Task<BackupWatcher?> GetActiveBackupWatcherAsync(int sprintId);
    Task<BackupWatcher> AssignBackupWatcherAsync(int sprintId, int backupMemberId, DateTime startDate, DateTime endDate, CoverageType coverageType, string? notes = null);
    Task RemoveBackupWatcherAsync(int backupWatcherId);
    Task<WatcherSwap> SwapWatcherAsync(int sprintId, int scheduledWatcherId, int actualWatcherId, string reason);
    Task<List<WatcherSwap>> GetSwapHistoryAsync();
    Task<WatcherSwap?> GetSwapForSprintAsync(int sprintId);
}
