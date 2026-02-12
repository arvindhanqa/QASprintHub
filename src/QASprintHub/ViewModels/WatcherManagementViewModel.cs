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
        var dialog = new Views.Dialogs.InputDialog("Add Team Member", "Enter member name:", "");
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
        {
            await _teamService.AddMemberAsync(dialog.InputValue);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditMemberAsync(TeamMember? member)
    {
        if (member == null) return;

        var dialog = new Views.Dialogs.InputDialog("Edit Team Member", "Enter new name:", member.Name);
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
        {
            member.Name = dialog.InputValue;
            await _teamService.UpdateMemberAsync(member);
            await LoadDataAsync();
        }
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
        var target = member ?? SelectedMember;
        if (target == null) return;

        var members = ActiveMembers.OrderBy(m => m.RotationOrder).ToList();
        var index = members.FindIndex(m => m.Id == target.Id);

        if (index <= 0) return; // Can't move up if already at the top

        // Swap the rotation orders
        var upper = members[index - 1];
        var current = members[index];
        var tempOrder = upper.RotationOrder;
        upper.RotationOrder = current.RotationOrder;
        current.RotationOrder = tempOrder;

        // Update in database
        await _teamService.UpdateMemberAsync(upper);
        await _teamService.UpdateMemberAsync(current);

        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task MoveDownAsync(TeamMember? member)
    {
        var target = member ?? SelectedMember;
        if (target == null) return;

        var members = ActiveMembers.OrderBy(m => m.RotationOrder).ToList();
        var index = members.FindIndex(m => m.Id == target.Id);

        if (index < 0 || index >= members.Count - 1) return; // Can't move down if at the bottom

        // Swap the rotation orders
        var current = members[index];
        var lower = members[index + 1];
        var tempOrder = current.RotationOrder;
        current.RotationOrder = lower.RotationOrder;
        lower.RotationOrder = tempOrder;

        // Update in database
        await _teamService.UpdateMemberAsync(current);
        await _teamService.UpdateMemberAsync(lower);

        await LoadDataAsync();
    }
}
