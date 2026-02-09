using Microsoft.EntityFrameworkCore;
using QASprintHub.Data;
using QASprintHub.Models;
using QASprintHub.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QASprintHub.Services;

public class TeamService : ITeamService
{
    private readonly AppDbContext _context;

    public TeamService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeamMember>> GetAllMembersAsync()
    {
        return await _context.TeamMembers
            .OrderBy(m => m.RotationOrder)
            .ToListAsync();
    }

    public async Task<List<TeamMember>> GetActiveMembersAsync()
    {
        return await _context.TeamMembers
            .Where(m => m.Status == MemberStatus.Active)
            .OrderBy(m => m.RotationOrder)
            .ToListAsync();
    }

    public async Task<TeamMember?> GetMemberByIdAsync(int id)
    {
        return await _context.TeamMembers.FindAsync(id);
    }

    public async Task<TeamMember> AddMemberAsync(string name, string? email = null)
    {
        // Get the next rotation order
        var maxOrder = await _context.TeamMembers
            .Where(m => m.Status == MemberStatus.Active)
            .MaxAsync(m => (int?)m.RotationOrder) ?? 0;

        var member = new TeamMember
        {
            Name = name,
            Email = email,
            RotationOrder = maxOrder + 1,
            Status = MemberStatus.Active,
            CreatedDate = DateTime.Now
        };

        _context.TeamMembers.Add(member);
        await _context.SaveChangesAsync();

        return member;
    }

    public async Task UpdateMemberAsync(TeamMember member)
    {
        _context.TeamMembers.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task DeactivateMemberAsync(int id)
    {
        var member = await _context.TeamMembers.FindAsync(id);
        if (member == null) return;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var oldOrder = member.RotationOrder;

            // Mark as departed
            member.Status = MemberStatus.Departed;
            member.DepartedDate = DateTime.Now;
            member.RotationOrder = 0; // Remove from rotation

            // Compress the rotation order - shift everyone below up
            var membersToUpdate = await _context.TeamMembers
                .Where(m => m.Status == MemberStatus.Active && m.RotationOrder > oldOrder)
                .ToListAsync();

            foreach (var m in membersToUpdate)
            {
                m.RotationOrder--;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ReorderMembersAsync(List<int> memberIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            for (int i = 0; i < memberIds.Count; i++)
            {
                var member = await _context.TeamMembers.FindAsync(memberIds[i]);
                if (member != null && member.Status == MemberStatus.Active)
                {
                    member.RotationOrder = i + 1;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
