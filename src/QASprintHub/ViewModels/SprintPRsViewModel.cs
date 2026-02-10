using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QASprintHub.Models;
using QASprintHub.Models.Enums;
using QASprintHub.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace QASprintHub.ViewModels;

public partial class SprintPRsViewModel : ObservableObject
{
    private readonly ISprintService _sprintService;
    private readonly IPRService _prService;

    [ObservableProperty]
    private Sprint? _currentSprint;

    [ObservableProperty]
    private ObservableCollection<SprintPR> _PRs = new();

    [ObservableProperty]
    private SprintPR? _selectedPR;

    [ObservableProperty]
    private PRStatus? _statusFilter;

    public SprintPRsViewModel(ISprintService sprintService, IPRService prService)
    {
        _sprintService = sprintService;
        _prService = prService;
    }

    public async Task LoadDataAsync()
    {
        CurrentSprint = await _sprintService.GetCurrentSprintAsync();

        if (CurrentSprint != null)
        {
            var prs = await _prService.GetPRsBySprintIdAsync(CurrentSprint.Id);
            PRs = new ObservableCollection<SprintPR>(prs);
        }
    }

    [RelayCommand]
    private async Task AddPRAsync()
    {
        if (CurrentSprint == null) return;

        var dialog = new Views.Dialogs.AddPRDialog();
        if (dialog.ShowDialog() == true)
        {
            await _prService.AddPRAsync(
                CurrentSprint.Id,
                dialog.PRTitle,
                dialog.Link,
                dialog.Author,
                dialog.Priority);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditPRAsync(SprintPR? pr)
    {
        if (pr == null) return;

        var dialog = new Views.Dialogs.EditPRDialog(pr);
        if (dialog.ShowDialog() == true)
        {
            // Update the PR with the edited values
            pr.Title = dialog.PRTitle;
            pr.Author = dialog.Author;
            pr.Link = dialog.Link;
            pr.Status = dialog.Status;
            pr.Priority = dialog.Priority;

            await _prService.UpdatePRAsync(pr);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task DeletePRAsync(SprintPR? pr)
    {
        if (pr == null || CurrentSprint == null) return;

        await _prService.DeletePRAsync(pr.Id);
        await LoadDataAsync();
    }

    private async Task UpdatePRStatusAsync(SprintPR? pr, PRStatus newStatus)
    {
        if (pr == null) return;

        await _prService.UpdatePRStatusAsync(pr.Id, newStatus);
        await LoadDataAsync();
    }

    [RelayCommand]
    private void OpenLink(SprintPR? pr)
    {
        if (string.IsNullOrWhiteSpace(pr?.Link))
        {
            System.Windows.MessageBox.Show("No link available for this PR.", "Open Link",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            return;
        }

        // Validate URL format
        if (!Uri.TryCreate(pr.Link, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            System.Windows.MessageBox.Show("Invalid URL format. Please enter a valid http or https URL.",
                "Invalid Link", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = pr.Link,
                UseShellExecute = true
            });
        }
        catch (System.Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to open link: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task PreviousSprintAsync()
    {
        if (CurrentSprint == null) return;

        var previousSprint = await _sprintService.GetPreviousSprintAsync(CurrentSprint.Id);
        if (previousSprint != null)
        {
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
        var prs = await _prService.GetPRsBySprintIdAsync(sprint.Id);
        PRs = new ObservableCollection<SprintPR>(prs);
    }
}
