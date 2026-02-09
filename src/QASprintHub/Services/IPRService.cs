using QASprintHub.Models;
using QASprintHub.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QASprintHub.Services;

public interface IPRService
{
    Task<List<SprintPR>> GetPRsBySprintIdAsync(int sprintId);
    Task<SprintPR?> GetPRByIdAsync(int id);
    Task<SprintPR> AddPRAsync(int sprintId, string title, string? link = null, string? author = null, PRPriority priority = PRPriority.Normal, string? notes = null);
    Task UpdatePRAsync(SprintPR pr);
    Task UpdatePRStatusAsync(int prId, PRStatus newStatus);
    Task DeletePRAsync(int prId);
    Task<Dictionary<PRStatus, int>> GetPRStatsBySprintIdAsync(int sprintId);
}
