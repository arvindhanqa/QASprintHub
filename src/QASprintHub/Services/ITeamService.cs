using QASprintHub.Models;
using QASprintHub.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QASprintHub.Services;

public interface ITeamService
{
    Task<List<TeamMember>> GetAllMembersAsync();
    Task<List<TeamMember>> GetActiveMembersAsync();
    Task<TeamMember?> GetMemberByIdAsync(int id);
    Task<TeamMember> AddMemberAsync(string name, string? email = null);
    Task UpdateMemberAsync(TeamMember member);
    Task DeactivateMemberAsync(int id);
    Task ReorderMembersAsync(List<int> memberIds);
}
