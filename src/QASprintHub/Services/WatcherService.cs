using Microsoft.EntityFrameworkCore;
using QASprintHub.Data;
using QASprintHub.Models;
using QASprintHub.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QASprintHub.Services;

public class WatcherService : IWatcherService
{
    private readonly AppDbContext _context;

    public WatcherService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BackupWatcher?> GetActiveBackupWatcherAsync(int sprintId)
    {
        return await _context.BackupWatchers
            .Include(b => b.BackupMember)
            .Where(b => b.SprintId == sprintId)
            .FirstOrDefaultAsync();
    }

    public async Task<BackupWatcher> AssignBackupWatcherAsync(int sprintId, int backupMemberId, DateTime startDate, DateTime endDate, CoverageType coverageType, string? notes = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Remove any existing backup watcher for this sprint (only one at a time)
            var existingBackup = await _context.BackupWatchers
                .Where(b => b.SprintId == sprintId)
                .FirstOrDefaultAsync();

            if (existingBackup != null)
            {
                _context.BackupWatchers.Remove(existingBackup);
            }

            var backup = new BackupWatcher
            {
                SprintId = sprintId,
                BackupMemberId = backupMemberId,
                StartDate = startDate,
                EndDate = endDate,
                CoverageType = coverageType,
                Notes = notes,
                CreatedDate = DateTime.Now
            };

            _context.BackupWatchers.Add(backup);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return backup;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RemoveBackupWatcherAsync(int backupWatcherId)
    {
        var backup = await _context.BackupWatchers.FindAsync(backupWatcherId);
        if (backup != null)
        {
            _context.BackupWatchers.Remove(backup);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<WatcherSwap> SwapWatcherAsync(int sprintId, int scheduledWatcherId, int actualWatcherId, string reason)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var swap = new WatcherSwap
            {
                SprintId = sprintId,
                ScheduledWatcherId = scheduledWatcherId,
                ActualWatcherId = actualWatcherId,
                SwapDate = DateTime.Now,
                Reason = reason,
                CreatedDate = DateTime.Now
            };

            _context.WatcherSwaps.Add(swap);

            // Update the sprint's watcher
            var sprint = await _context.Sprints.FindAsync(sprintId);
            if (sprint != null)
            {
                sprint.WatcherId = actualWatcherId;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return swap;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<WatcherSwap>> GetSwapHistoryAsync()
    {
        return await _context.WatcherSwaps
            .Include(s => s.Sprint)
            .Include(s => s.ScheduledWatcher)
            .Include(s => s.ActualWatcher)
            .OrderByDescending(s => s.SwapDate)
            .ToListAsync();
    }
}
