using Microsoft.EntityFrameworkCore;
using QASprintHub.Data;
using QASprintHub.Models;
using QASprintHub.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QASprintHub.Services;

public class PRService : IPRService
{
    private readonly AppDbContext _context;

    public PRService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SprintPR>> GetPRsBySprintIdAsync(int sprintId)
    {
        return await _context.SprintPRs
            .Where(pr => pr.SprintId == sprintId)
            .OrderBy(pr => pr.AddedDate)
            .ToListAsync();
    }

    public async Task<SprintPR?> GetPRByIdAsync(int id)
    {
        return await _context.SprintPRs.FindAsync(id);
    }

    public async Task<SprintPR> AddPRAsync(int sprintId, string title, string? link = null, string? author = null, PRPriority priority = PRPriority.Normal, string? notes = null)
    {
        var pr = new SprintPR
        {
            SprintId = sprintId,
            Title = title,
            Link = link,
            Author = author,
            Priority = priority,
            Notes = notes,
            Status = PRStatus.Pending,
            AddedDate = DateTime.Now,
            StatusChangedDate = DateTime.Now,
            CreatedDate = DateTime.Now
        };

        _context.SprintPRs.Add(pr);
        await _context.SaveChangesAsync();

        return pr;
    }

    public async Task UpdatePRAsync(SprintPR pr)
    {
        _context.SprintPRs.Update(pr);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePRStatusAsync(int prId, PRStatus newStatus)
    {
        var pr = await _context.SprintPRs.FindAsync(prId);
        if (pr != null)
        {
            pr.Status = newStatus;
            pr.StatusChangedDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeletePRAsync(int prId)
    {
        var pr = await _context.SprintPRs.FindAsync(prId);
        if (pr != null)
        {
            _context.SprintPRs.Remove(pr);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<PRStatus, int>> GetPRStatsBySprintIdAsync(int sprintId)
    {
        var prs = await GetPRsBySprintIdAsync(sprintId);

        return new Dictionary<PRStatus, int>
        {
            { PRStatus.Pending, prs.Count(p => p.Status == PRStatus.Pending) },
            { PRStatus.Merged, prs.Count(p => p.Status == PRStatus.Merged) },
            { PRStatus.Blocked, prs.Count(p => p.Status == PRStatus.Blocked) },
            { PRStatus.Declined, prs.Count(p => p.Status == PRStatus.Declined) }
        };
    }
}
