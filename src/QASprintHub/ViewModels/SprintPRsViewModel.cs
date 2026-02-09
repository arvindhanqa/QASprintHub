using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QASprintHub.Models;
using QASprintHub.Models.Enums;
using QASprintHub.Services;
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
                null, // link
                dialog.Author,
                dialog.Priority);
            await LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task EditPRAsync(SprintPR? pr)
    {
        if (pr == null) return;
        // TODO: Open edit PR dialog
        await Task.CompletedTask;
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
        if (pr?.Link != null)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = pr.Link,
                UseShellExecute = true
            });
        }
    }
}
