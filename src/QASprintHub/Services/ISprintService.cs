using QASprintHub.Models;
using QASprintHub.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QASprintHub.Services;

public interface ISprintService
{
    Task<List<Sprint>> GetAllSprintsAsync();
    Task<Sprint?> GetCurrentSprintAsync();
    Task<Sprint?> GetSprintByIdAsync(int id);
    Task<List<Sprint>> GetSprintsByMonthAsync(int year, int month);
    Task<Sprint> CreateSprintAsync(DateTime startDate, DateTime endDate, int watcherId, string? notes = null);
    Task UpdateSprintAsync(Sprint sprint);
    Task<Sprint?> GetNextSprintAsync();
    Task<int> GetNextWatcherIdAsync();
    Task GenerateFutureSprintsAsync(int monthsAhead = 6);
    Task ActivatePlannedSprintsAsync();
}
