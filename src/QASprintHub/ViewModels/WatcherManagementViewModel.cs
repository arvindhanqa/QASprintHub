using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QASprintHub.Models;
using QASprintHub.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace QASprintHub.ViewModels;

public partial class WatcherManagementViewModel : ObservableObject
{
    private readonly ITeamService _teamService;
    private readonly IWatcherService _watcherService;
    private readonly ISprintService _sprintService;

    [ObservableProperty]
    private ObservableCollection<TeamMember> _activeMembers = new();

    [ObservableProperty]
    private ObservableCollection<WatcherSwap> _swapHistory = new();

    [ObservableProperty]
    private ObservableCollection<Sprint> _upcomingSprints = new();

    [ObservableProperty]
    private TeamMember? _selectedMember;

    public WatcherManagementViewModel(
        ITeamService teamService,
        IWatcherService watcherService,
        ISprintService sprintService)
    {
        _teamService = teamService;
        _watcherService = watcherService;
        _sprintService = sprintService;
    }

    public async Task LoadDataAsync()
    {
        var members = await _teamService.GetActiveMembersAsync();
        ActiveMembers = new ObservableCollection<TeamMember>(members);

        var swaps = await _watcherService.GetSwapHistoryAsync();
        SwapHistory = new ObservableCollection<WatcherSwap>(swaps);

        // Load upcoming sprints (for preview)
        var allSprints = await _sprintService.GetAllSprintsAsync();
        var upcoming = allSprints.Where(s => s.StartDate >= System.DateTime.Today)
                                 .OrderBy(s => s.StartDate)
                                 .Take(7)
                                 .ToList();
        UpcomingSprints = new ObservableCollection<Sprint>(upcoming);
    }

    [RelayCommand]
    private async Task AddMemberAsync()
    {
        // TODO: Open add member dialog
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task EditMemberAsync(TeamMember? member)
    {
        if (member == null) return;
        // TODO: Open edit member dialog
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DeactivateMemberAsync(TeamMember? member)
    {
        if (member == null) return;

        await _teamService.DeactivateMemberAsync(member.Id);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task MoveUpAsync(TeamMember? member)
    {
        if (member == null || member.RotationOrder <= 1) return;

        var members = ActiveMembers.OrderBy(m => m.RotationOrder).ToList();
        var index = members.IndexOf(member);
        if (index > 0)
        {
            (members[index], members[index - 1]) = (members[index - 1], members[index]);
            await _teamService.ReorderMembersAsync(members.Select(m => m.Id).ToList());
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task MoveDownAsync(TeamMember? member)
    {
        if (member == null) return;

        var members = ActiveMembers.OrderBy(m => m.RotationOrder).ToList();
        var index = members.IndexOf(member);
        if (index < members.Count - 1)
        {
            (members[index], members[index + 1]) = (members[index + 1], members[index]);
            await _teamService.ReorderMembersAsync(members.Select(m => m.Id).ToList());
            await LoadDataAsync();
        }
    }
}
