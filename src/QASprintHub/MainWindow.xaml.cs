using Microsoft.Extensions.DependencyInjection;
using QASprintHub.Services;
using QASprintHub.ViewModels;
using QASprintHub.Views;
using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace QASprintHub;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITrayService _trayService;
    private readonly INotificationService _notificationService;
    private readonly ISprintService _sprintService;
    private DateTime _currentMonth;

    public MainWindow(
        IServiceProvider serviceProvider,
        ITrayService trayService,
        INotificationService notificationService,
        ISprintService sprintService)
    {
        InitializeComponent();

        _serviceProvider = serviceProvider;
        _trayService = trayService;
        _notificationService = notificationService;
        _sprintService = sprintService;

        _currentMonth = DateTime.Today;
        UpdateMonthDisplay();

        // Set up tray service events
        _trayService.OpenRequested += (s, e) => ShowMainWindow();
        _trayService.ExitRequested += (s, e) => ExitApplication();

        // Select first menu item and navigate to Calendar Diary by default
        if (NavigationView.MenuItems.Count > 0 && NavigationView.MenuItems[0] is NavigationViewItem firstItem)
        {
            NavigationView.SelectedItem = firstItem;
        }
        NavigateTo("CalendarDiary");

        // Show current watcher notification on startup
        _ = ShowStartupNotificationAsync();
    }

    private async System.Threading.Tasks.Task ShowStartupNotificationAsync()
    {
        try
        {
            var currentSprint = await _sprintService.GetCurrentSprintAsync();
            if (currentSprint != null)
            {
                var nextSprint = await _sprintService.GetNextSprintAsync();
                _notificationService.ShowWatcherNotification(currentSprint, nextSprint?.Watcher);

                // Update tray icon with current watcher
                _trayService.UpdateCurrentWatcherInfo(currentSprint.Watcher?.Name ?? "No watcher");
            }
        }
        catch (Exception ex)
        {
            // Log error (in production, use a proper logging framework)
            System.Windows.MessageBox.Show($"Error loading sprint information: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }

    private void NavigationView_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (NavigationView.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            NavigateTo(tag);
        }
    }

    private void NavigateTo(string page)
    {
        try
        {
            switch (page)
            {
                case "CalendarDiary":
                    var calendarView = _serviceProvider.GetRequiredService<CalendarDiaryView>();
                    var calendarViewModel = _serviceProvider.GetRequiredService<CalendarDiaryViewModel>();
                    calendarView.DataContext = calendarViewModel;
                    _ = calendarViewModel.LoadDataAsync();
                    ContentFrame.Navigate(calendarView);
                    break;

                case "Dashboard":
                    var dashboardView = _serviceProvider.GetRequiredService<DashboardView>();
                    var dashboardViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
                    dashboardView.DataContext = dashboardViewModel;
                    _ = dashboardViewModel.LoadDataAsync();
                    ContentFrame.Navigate(dashboardView);
                    break;

                case "SprintPRs":
                    var sprintPRsView = _serviceProvider.GetRequiredService<SprintPRsView>();
                    var sprintPRsViewModel = _serviceProvider.GetRequiredService<SprintPRsViewModel>();
                    sprintPRsView.DataContext = sprintPRsViewModel;
                    _ = sprintPRsViewModel.LoadDataAsync();
                    ContentFrame.Navigate(sprintPRsView);
                    break;

                case "Watchers":
                    var watcherView = _serviceProvider.GetRequiredService<WatcherManagementView>();
                    var watcherViewModel = _serviceProvider.GetRequiredService<WatcherManagementViewModel>();
                    watcherView.DataContext = watcherViewModel;
                    _ = watcherViewModel.LoadDataAsync();
                    ContentFrame.Navigate(watcherView);
                    break;

                case "History":
                    var historyView = _serviceProvider.GetRequiredService<HistoryView>();
                    var historyViewModel = _serviceProvider.GetRequiredService<HistoryViewModel>();
                    historyView.DataContext = historyViewModel;
                    _ = historyViewModel.LoadDataAsync();
                    ContentFrame.Navigate(historyView);
                    break;

                case "Settings":
                    var settingsView = _serviceProvider.GetRequiredService<SettingsView>();
                    var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
                    settingsView.DataContext = settingsViewModel;
                    settingsViewModel.LoadSettings();
                    ContentFrame.Navigate(settingsView);
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Navigation error: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void PreviousMonth_Click(object sender, RoutedEventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(-1);
        UpdateMonthDisplay();
    }

    private void NextMonth_Click(object sender, RoutedEventArgs e)
    {
        _currentMonth = _currentMonth.AddMonths(1);
        UpdateMonthDisplay();
    }

    private void Today_Click(object sender, RoutedEventArgs e)
    {
        _currentMonth = DateTime.Today;
        UpdateMonthDisplay();
    }

    private void UpdateMonthDisplay()
    {
        MonthYearText.Text = _currentMonth.ToString("MMMM yyyy");
        CalendarDatePicker.SelectedDate = _currentMonth;
    }

    private async void CalendarDatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (CalendarDatePicker.SelectedDate.HasValue)
        {
            var selectedDate = CalendarDatePicker.SelectedDate.Value;
            _currentMonth = selectedDate;
            UpdateMonthDisplay();

            // If we're on the Calendar Diary view, navigate to the selected date
            if (ContentFrame.Content is CalendarDiaryView calendarView &&
                calendarView.DataContext is CalendarDiaryViewModel viewModel)
            {
                await viewModel.GoToDateAsync(selectedDate);
            }
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // Minimize to tray instead of closing (configurable in settings)
        e.Cancel = true;
        Hide();
    }

    private void ShowMainWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        Application.Current.Shutdown();
    }
}
