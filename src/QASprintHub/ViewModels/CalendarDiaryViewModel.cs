using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QASprintHub.Models;
using QASprintHub.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace QASprintHub.ViewModels;

public partial class CalendarDiaryViewModel : ObservableObject
{
    private readonly ISprintService _sprintService;
    private readonly IPRService _prService;
    private readonly IWatcherService _watcherService;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private Sprint? _currentSprint;

    [ObservableProperty]
    private ObservableCollection<DayInfo> _sprintDays = new();

    [ObservableProperty]
    private DayInfo? _selectedDay;

    [ObservableProperty]
    private string _sprintTitle = string.Empty;

    [ObservableProperty]
    private string _watcherName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SprintPR> _dayPRs = new();

    [ObservableProperty]
    private string _dayNotes = string.Empty;

    public CalendarDiaryViewModel(
        ISprintService sprintService,
        IPRService prService,
        IWatcherService watcherService)
    {
        _sprintService = sprintService;
        _prService = prService;
        _watcherService = watcherService;
    }

    public async Task LoadDataAsync()
    {
        await LoadSprintForDateAsync(SelectedDate);
    }

    private async Task LoadSprintForDateAsync(DateTime date)
    {
        // Find sprint containing this date
        var allSprints = await _sprintService.GetAllSprintsAsync();
        CurrentSprint = allSprints.FirstOrDefault(s =>
            s.StartDate.Date <= date.Date && s.EndDate.Date >= date.Date);

        if (CurrentSprint != null)
        {
            SprintTitle = CurrentSprint.DisplayName;
            WatcherName = CurrentSprint.Watcher?.Name ?? "No watcher assigned";

            // Build sprint days
            BuildSprintDays();

            // Load PRs for this sprint
            var prs = await _prService.GetPRsBySprintIdAsync(CurrentSprint.Id);
            DayPRs = new ObservableCollection<SprintPR>(prs);

            // Select the current date if it's in this sprint
            SelectedDay = SprintDays.FirstOrDefault(d => d.Date.Date == date.Date);
        }
        else
        {
            SprintTitle = "No sprint found for this date";
            WatcherName = string.Empty;
            SprintDays.Clear();
            DayPRs.Clear();
        }
    }

    private void BuildSprintDays()
    {
        if (CurrentSprint == null) return;

        SprintDays.Clear();
        var currentDate = CurrentSprint.StartDate;
        var dayNumber = 1;

        while (currentDate <= CurrentSprint.EndDate)
        {
            // Only add working days (Mon-Fri)
            if (currentDate.DayOfWeek != DayOfWeek.Saturday &&
                currentDate.DayOfWeek != DayOfWeek.Sunday)
            {
                SprintDays.Add(new DayInfo
                {
                    Date = currentDate,
                    DayNumber = dayNumber,
                    DayName = currentDate.ToString("ddd, MMM dd"),
                    IsToday = currentDate.Date == DateTime.Today,
                    IsSelected = currentDate.Date == SelectedDate.Date
                });
                dayNumber++;
            }
            currentDate = currentDate.AddDays(1);
        }
    }

    [RelayCommand]
    private async Task SelectDayAsync(DayInfo? day)
    {
        if (day == null) return;

        SelectedDay = day;
        SelectedDate = day.Date;

        // Update IsSelected for all days
        foreach (var d in SprintDays)
        {
            d.IsSelected = d.Date.Date == day.Date.Date;
        }
    }

    [RelayCommand]
    private async Task GoToDateAsync(DateTime date)
    {
        SelectedDate = date;
        await LoadSprintForDateAsync(date);
    }

    [RelayCommand]
    private async Task PreviousSprintAsync()
    {
        if (CurrentSprint == null) return;

        // Go to the day before the current sprint
        var previousDate = CurrentSprint.StartDate.AddDays(-1);
        await GoToDateAsync(previousDate);
    }

    [RelayCommand]
    private async Task NextSprintAsync()
    {
        if (CurrentSprint == null) return;

        // Go to the day after the current sprint
        var nextDate = CurrentSprint.EndDate.AddDays(1);
        await GoToDateAsync(nextDate);
    }

    [RelayCommand]
    private async Task TodayAsync()
    {
        await GoToDateAsync(DateTime.Today);
    }

    [RelayCommand]
    private async Task SaveDayNotesAsync()
    {
        if (CurrentSprint == null) return;

        // TODO: Implement day notes storage
        // For now, we'll add it to sprint notes
        CurrentSprint.Notes = DayNotes;
        await _sprintService.UpdateSprintAsync(CurrentSprint);
    }
}

public class DayInfo : ObservableObject
{
    public DateTime Date { get; set; }
    public int DayNumber { get; set; }
    public string DayName { get; set; } = string.Empty;
    public bool IsToday { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
