using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QASprintHub.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace QASprintHub.ViewModels.Dialogs;

public partial class SetupWizardViewModel : ObservableObject
{
    private readonly ITeamService _teamService;
    private readonly ISprintService _sprintService;

    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    private int _sprintDurationDays = 10;

    [ObservableProperty]
    private DateTime _firstSprintStartDate = DateTime.Today;

    [ObservableProperty]
    private int _teamSize = 7;

    [ObservableProperty]
    private ObservableCollection<string> _teamMemberNames = new();

    [ObservableProperty]
    private bool _isComplete = false;

    public SetupWizardViewModel(ITeamService teamService, ISprintService sprintService)
    {
        _teamService = teamService;
        _sprintService = sprintService;

        // Initialize with default team size
        UpdateTeamMembersList();
    }

    partial void OnTeamSizeChanged(int value)
    {
        UpdateTeamMembersList();
    }

    private void UpdateTeamMembersList()
    {
        var currentCount = TeamMemberNames.Count;

        if (TeamSize > currentCount)
        {
            // Add new members
            for (int i = currentCount; i < TeamSize; i++)
            {
                TeamMemberNames.Add($"Team Member {i + 1}");
            }
        }
        else if (TeamSize < currentCount)
        {
            // Remove extra members
            while (TeamMemberNames.Count > TeamSize)
            {
                TeamMemberNames.RemoveAt(TeamMemberNames.Count - 1);
            }
        }
    }

    [RelayCommand]
    private void NextStep()
    {
        if (CurrentStep < 3)
        {
            CurrentStep++;
        }
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
        }
    }

    [RelayCommand]
    private async Task FinishSetupAsync()
    {
        try
        {
            // Add team members
            for (int i = 0; i < TeamMemberNames.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(TeamMemberNames[i]))
                {
                    await _teamService.AddMemberAsync(TeamMemberNames[i]);
                }
            }

            // Create first sprint
            var endDate = CalculateEndDate(FirstSprintStartDate, SprintDurationDays);
            var nextWatcherId = await _sprintService.GetNextWatcherIdAsync();
            await _sprintService.CreateSprintAsync(FirstSprintStartDate, endDate, nextWatcherId);

            IsComplete = true;
        }
        catch (Exception ex)
        {
            // Handle error
            System.Windows.MessageBox.Show($"Setup failed: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private DateTime CalculateEndDate(DateTime startDate, int durationDays)
    {
        var endDate = startDate;
        var workingDaysAdded = 0;

        while (workingDaysAdded < durationDays)
        {
            endDate = endDate.AddDays(1);
            // Skip weekends (Saturday = 6, Sunday = 0)
            if (endDate.DayOfWeek != DayOfWeek.Saturday && endDate.DayOfWeek != DayOfWeek.Sunday)
            {
                workingDaysAdded++;
            }
        }

        return endDate.AddDays(-1); // Subtract 1 because we want the last working day
    }

    [RelayCommand]
    private void SkipSetup()
    {
        IsComplete = true;
    }
}
