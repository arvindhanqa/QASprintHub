using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QASprintHub.Models;
using QASprintHub.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace QASprintHub.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly ISprintService _sprintService;
    private readonly IPRService _prService;

    [ObservableProperty]
    private ObservableCollection<SprintHistoryItem> _sprints = new();

    public HistoryViewModel(ISprintService sprintService, IPRService prService)
    {
        _sprintService = sprintService;
        _prService = prService;
    }

    public async Task LoadDataAsync()
    {
        var sprints = await _sprintService.GetAllSprintsAsync();
        var historyItems = new ObservableCollection<SprintHistoryItem>();

        foreach (var sprint in sprints)
        {
            var stats = await _prService.GetPRStatsBySprintIdAsync(sprint.Id);
            var item = new SprintHistoryItem
            {
                Sprint = sprint,
                TotalPRs = stats.Values.Sum(),
                MergedPRs = stats[Models.Enums.PRStatus.Merged],
                BlockedPRs = stats[Models.Enums.PRStatus.Blocked],
                HasSwap = sprint.WatcherSwaps.Any()
            };
            historyItems.Add(item);
        }

        Sprints = historyItems;
    }

    [RelayCommand]
    private async Task ExportToExcelAsync()
    {
        // TODO: Implement Excel export
        await Task.CompletedTask;
    }
}

public class SprintHistoryItem
{
    public Sprint Sprint { get; set; } = null!;
    public int TotalPRs { get; set; }
    public int MergedPRs { get; set; }
    public int BlockedPRs { get; set; }
    public bool HasSwap { get; set; }
}
