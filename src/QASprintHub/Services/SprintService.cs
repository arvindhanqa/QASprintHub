using Microsoft.EntityFrameworkCore;
using QASprintHub.Data;
using QASprintHub.Models;
using QASprintHub.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QASprintHub.Services;

public class SprintService : ISprintService
{
    private readonly AppDbContext _context;
    private readonly ITeamService _teamService;

    public SprintService(AppDbContext context, ITeamService teamService)
    {
        _context = context;
        _teamService = teamService;
    }

    public async Task<List<Sprint>> GetAllSprintsAsync()
    {
        return await _context.Sprints
            .Include(s => s.Watcher)
            .Include(s => s.BackupWatchers)
            .Include(s => s.WatcherSwaps)
            .Include(s => s.SprintPRs)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<Sprint?> GetCurrentSprintAsync()
    {
        var today = DateTime.Today;
        return await _context.Sprints
            .Include(s => s.Watcher)
            .Include(s => s.BackupWatchers).ThenInclude(b => b.BackupMember)
            .Include(s => s.WatcherSwaps)
            .Include(s => s.SprintPRs)
            .Where(s => s.StartDate <= today && s.EndDate >= today && s.Status == SprintStatus.Active)
            .FirstOrDefaultAsync();
    }

    public async Task<Sprint?> GetSprintByIdAsync(int id)
    {
        return await _context.Sprints
            .Include(s => s.Watcher)
            .Include(s => s.BackupWatchers).ThenInclude(b => b.BackupMember)
            .Include(s => s.WatcherSwaps)
            .Include(s => s.SprintPRs)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Sprint>> GetSprintsByMonthAsync(int year, int month)
    {
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        // Include sprints that overlap with the month (start before month end and end after month start)
        return await _context.Sprints
            .Include(s => s.Watcher)
            .Include(s => s.BackupWatchers)
            .Include(s => s.SprintPRs)
            .Where(s => s.StartDate <= monthEnd && s.EndDate >= monthStart)
            .OrderBy(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<Sprint> CreateSprintAsync(DateTime startDate, DateTime endDate, int watcherId, string? notes = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Mark previous sprints as completed
            var previousSprints = await _context.Sprints
                .Where(s => s.Status == SprintStatus.Active)
                .ToListAsync();

            foreach (var sprint in previousSprints)
            {
                sprint.Status = SprintStatus.Completed;
            }

            var newSprint = new Sprint
            {
                StartDate = startDate,
                EndDate = endDate,
                WatcherId = watcherId,
                Status = SprintStatus.Active,
                Notes = notes,
                CreatedDate = DateTime.Now
            };

            _context.Sprints.Add(newSprint);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return newSprint;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateSprintAsync(Sprint sprint)
    {
        _context.Sprints.Update(sprint);
        await _context.SaveChangesAsync();
    }

    public async Task<Sprint?> GetNextSprintAsync()
    {
        var today = DateTime.Today;
        return await _context.Sprints
            .Include(s => s.Watcher)
            .Where(s => s.StartDate > today)
            .OrderBy(s => s.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetNextWatcherIdAsync()
    {
        var activeMembers = await _teamService.GetActiveMembersAsync();
        if (!activeMembers.Any())
        {
            throw new InvalidOperationException("No active team members available for rotation.");
        }

        var lastSprint = await _context.Sprints
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync();

        if (lastSprint == null)
        {
            // First sprint - return first member
            return activeMembers.First().Id;
        }

        // Find the next watcher in rotation
        var lastWatcherIndex = activeMembers.FindIndex(m => m.Id == lastSprint.WatcherId);
        var nextIndex = (lastWatcherIndex + 1) % activeMembers.Count;

        return activeMembers[nextIndex].Id;
    }
}
