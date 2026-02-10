using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QASprintHub.Models;
using QASprintHub.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QASprintHub.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ISprintService _sprintService;
    private readonly IPRService _prService;
    private readonly IWatcherService _watcherService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private Sprint? _currentSprint;

    [ObservableProperty]
    private TeamMember? _currentWatcher;

    [ObservableProperty]
    private BackupWatcher? _currentBackup;

    [ObservableProperty]
    private Sprint? _nextSprint;

    [ObservableProperty]
    private TeamMember? _nextWatcher;

    [ObservableProperty]
    private int _totalPRs;

    [ObservableProperty]
    private int _pendingPRs;

    [ObservableProperty]
    private int _mergedPRs;

    [ObservableProperty]
    private int _blockedPRs;

    [ObservableProperty]
    private int _currentDay;

    [ObservableProperty]
    private int _totalDays;

    [ObservableProperty]
    private bool _isWatcherSwapped;

    public DashboardViewModel(
        ISprintService sprintService,
        IPRService prService,
        IWatcherService watcherService,
        INotificationService notificationService)
    {
        _sprintService = sprintService;
        _prService = prService;
        _watcherService = watcherService;
        _notificationService = notificationService;
    }

    public async Task LoadDataAsync()
    {
        CurrentSprint = await _sprintService.GetCurrentSprintAsync();

        if (CurrentSprint != null)
        {
            CurrentWatcher = CurrentSprint.Watcher;
            CurrentBackup = await _watcherService.GetActiveBackupWatcherAsync(CurrentSprint.Id);

            // Check if watcher was swapped
            var swap = await _watcherService.GetSwapForSprintAsync(CurrentSprint.Id);
            IsWatcherSwapped = swap != null;

            // Load PR stats
            var stats = await _prService.GetPRStatsBySprintIdAsync(CurrentSprint.Id);
            TotalPRs = stats.Values.Sum();
            PendingPRs = stats[Models.Enums.PRStatus.Pending];
            MergedPRs = stats[Models.Enums.PRStatus.Merged];
            BlockedPRs = stats[Models.Enums.PRStatus.Blocked];

            // Calculate current day and total days
            var today = DateTime.Today;
            CurrentDay = (today - CurrentSprint.StartDate).Days + 1;
            TotalDays = (CurrentSprint.EndDate - CurrentSprint.StartDate).Days + 1;
        }

        NextSprint = await _sprintService.GetNextSprintAsync();
        if (NextSprint != null)
        {
            NextWatcher = NextSprint.Watcher;
        }
    }

    [RelayCommand]
    private async Task SwapWatcherAsync()
    {
        if (CurrentSprint == null || CurrentWatcher == null) return;

        var teamService = App.GetService<ITeamService>();
        var allMembers = await teamService.GetActiveMembersAsync();
        var availableMembers = allMembers.Where(m => m.Id != CurrentWatcher.Id).ToList();

        var dialog = new Views.Dialogs.SwapWatcherDialog(CurrentWatcher.Name, availableMembers);
        if (dialog.ShowDialog() == true && dialog.SelectedMember != null)
        {
            await _watcherService.SwapWatcherAsync(CurrentSprint.Id, CurrentWatcher.Id, dialog.SelectedMember.Id, dialog.Reason);
            await LoadDataAsync();
            _notificationService.ShowWatcherNotification(CurrentSprint, NextWatcher);
        }
    }

    [RelayCommand]
    private async Task AssignBackupAsync()
    {
        if (CurrentSprint == null || CurrentWatcher == null) return;

        var teamService = App.GetService<ITeamService>();
        var allMembers = await teamService.GetActiveMembersAsync();
        var availableMembers = allMembers.Where(m => m.Id != CurrentWatcher.Id).ToList();

        var dialog = new Views.Dialogs.AssignBackupDialog(availableMembers);
        if (dialog.ShowDialog() == true && dialog.SelectedMember != null)
        {
            await _watcherService.AssignBackupWatcherAsync(
                CurrentSprint.Id,
                dialog.SelectedMember.Id,
                CurrentSprint.StartDate,
                CurrentSprint.EndDate,
                Models.Enums.CoverageType.FullSprint);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task RemoveBackupAsync()
    {
        if (CurrentBackup != null)
        {
            await _watcherService.RemoveBackupWatcherAsync(CurrentBackup.Id);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousSprintAsync()
    {
        if (CurrentSprint == null) return;

        var previousSprint = await _sprintService.GetPreviousSprintAsync(CurrentSprint.Id);
        if (previousSprint != null)
        {
            // Load data for the previous sprint by temporarily setting it as current
            await LoadSprintDataAsync(previousSprint);
        }
    }

    [RelayCommand]
    private async Task NextSprintAsync()
    {
        if (CurrentSprint == null) return;

        var nextSprint = await _sprintService.GetNextSprintAsync(CurrentSprint.Id);
        if (nextSprint != null)
        {
            // Load data for the next sprint
            await LoadSprintDataAsync(nextSprint);
        }
    }

    [RelayCommand]
    private async Task GoToCurrentSprintAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadSprintDataAsync(Sprint sprint)
    {
        CurrentSprint = sprint;
        CurrentWatcher = sprint.Watcher;
        CurrentBackup = await _watcherService.GetActiveBackupWatcherAsync(sprint.Id);

        // Check if watcher was swapped
        var swap = await _watcherService.GetSwapForSprintAsync(sprint.Id);
        IsWatcherSwapped = swap != null;

        // Load PR stats
        var stats = await _prService.GetPRStatsBySprintIdAsync(sprint.Id);
        TotalPRs = stats.Values.Sum();
        PendingPRs = stats[Models.Enums.PRStatus.Pending];
        MergedPRs = stats[Models.Enums.PRStatus.Merged];
        BlockedPRs = stats[Models.Enums.PRStatus.Blocked];

        // Calculate current day and total days
        var today = DateTime.Today;
        CurrentDay = (today - sprint.StartDate).Days + 1;
        TotalDays = (sprint.EndDate - sprint.StartDate).Days + 1;

        // Load next sprint info
        NextSprint = await _sprintService.GetNextSprintAsync(sprint.Id);
        if (NextSprint != null)
        {
            NextWatcher = NextSprint.Watcher;
        }
    }
}
