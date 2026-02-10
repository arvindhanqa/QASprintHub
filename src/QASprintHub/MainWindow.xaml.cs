using Microsoft.Extensions.DependencyInjection;
using QASprintHub.Services;
using QASprintHub.ViewModels;
using QASprintHub.Views;
using System;
using System.Windows;

namespace QASprintHub;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITrayService _trayService;
    private readonly INotificationService _notificationService;
    private DateTime _currentMonth;

    public MainWindow(
        IServiceProvider serviceProvider,
        ITrayService trayService,
        INotificationService notificationService)
    {
        InitializeComponent();

        _serviceProvider = serviceProvider;
        _trayService = trayService;
        _notificationService = notificationService;

        _currentMonth = DateTime.Today;
        UpdateMonthDisplay();

        // Set up tray service events
        _trayService.OpenRequested += (s, e) => ShowMainWindow();
        _trayService.ExitRequested += (s, e) => ExitApplication();

        // Set up navigation click handlers
        SetupNavigation();

        // Navigate to Calendar Diary by default
        NavigateTo("CalendarDiary");

        // Show current watcher notification on startup
        _ = ShowStartupNotificationAsync();
    }

    private void SetupNavigation()
    {
        // Attach click handlers to all navigation items
        foreach (var item in NavigationView.MenuItems)
        {
            if (item is Wpf.Ui.Controls.NavigationViewItem navItem)
            {
                navItem.Click += NavigationItem_Click;
            }
        }

        foreach (var item in NavigationView.FooterMenuItems)
        {
            if (item is Wpf.Ui.Controls.NavigationViewItem navItem)
            {
                navItem.Click += NavigationItem_Click;
            }
        }
    }

    private void NavigationItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Wpf.Ui.Controls.NavigationViewItem item && item.Tag is string tag)
        {
            NavigateTo(tag);
        }
    }

    private async System.Threading.Tasks.Task ShowStartupNotificationAsync()
    {
        try
        {
            var sprintService = _serviceProvider.GetRequiredService<ISprintService>();
            var currentSprint = await sprintService.GetCurrentSprintAsync();
            if (currentSprint != null)
            {
                var nextSprint = await sprintService.GetNextSprintAsync();
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
                    _ = settingsViewModel.LoadSettingsAsync();
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

    private async void Today_Click(object sender, RoutedEventArgs e)
    {
        _currentMonth = DateTime.Today;
        UpdateMonthDisplay();

        // Navigate to Calendar Diary view and go to today's date
        await NavigateToDateAsync(DateTime.Today);
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

            // Navigate to the selected date's sprint in Calendar Diary view
            await NavigateToDateAsync(selectedDate);
        }
    }

    private async System.Threading.Tasks.Task NavigateToDateAsync(DateTime date)
    {
        // Ensure we're on the Calendar Diary view
        if (!(ContentFrame.Content is CalendarDiaryView))
        {
            NavigateTo("CalendarDiary");
            // Wait a moment for the navigation to complete
            await System.Threading.Tasks.Task.Delay(100);
        }

        // Navigate to the specified date's sprint
        if (ContentFrame.Content is CalendarDiaryView calendarView &&
            calendarView.DataContext is CalendarDiaryViewModel viewModel)
        {
            await viewModel.GoToDateAsync(date);
        }
    }

    private async void NotificationBell_Click(object sender, RoutedEventArgs e)
    {
        // Get current and next sprint info
        var sprintService = _serviceProvider.GetRequiredService<ISprintService>();
        var currentSprint = await sprintService.GetCurrentSprintAsync();
        var nextSprint = await sprintService.GetNextSprintAsync();

        var message = "📋 Notifications:\n\n";

        if (currentSprint != null)
        {
            message += $"✓ Current Sprint: {currentSprint.DisplayName}\n";
            message += $"  QA Watcher: {currentSprint.Watcher?.Name ?? "None"}\n";
            message += $"  Days remaining: {(currentSprint.EndDate - DateTime.Today).Days}\n\n";
        }

        if (nextSprint != null)
        {
            message += $"→ Next Sprint: {nextSprint.DisplayName}\n";
            message += $"  QA Watcher: {nextSprint.Watcher?.Name ?? "None"}\n";
            message += $"  Starts: {nextSprint.StartDate:MMM dd, yyyy}\n";
        }
        else
        {
            message += "No upcoming sprint scheduled.\n";
        }

        System.Windows.MessageBox.Show(message, "QA Sprint Hub - Notifications",
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
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
