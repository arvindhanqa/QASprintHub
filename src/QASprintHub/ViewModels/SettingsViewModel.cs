using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using QASprintHub.Data;
using QASprintHub.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace QASprintHub.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IExportService _exportService;
    private readonly AppDbContext _context;
    private readonly ISprintService _sprintService;

    [ObservableProperty]
    private int _sprintDurationDays = 10;

    [ObservableProperty]
    private bool _minimizeToTray = true;

    [ObservableProperty]
    private bool _startMinimized = false;

    [ObservableProperty]
    private bool _launchOnStartup = false;

    [ObservableProperty]
    private bool _showWatcherNotification = true;

    [ObservableProperty]
    private bool _showSprintEndingNotification = true;

    [ObservableProperty]
    private bool _showSwapNotification = true;

    [ObservableProperty]
    private string _databaseLocation = string.Empty;

    public SettingsViewModel(IExportService exportService, AppDbContext context, ISprintService sprintService)
    {
        _exportService = exportService;
        _context = context;
        _sprintService = sprintService;

        // Get database location
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        DatabaseLocation = Path.Combine(appDataPath, "QASprintHub", "qasprinthub.db");
    }

    public async Task LoadSettingsAsync()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        if (settings != null)
        {
            SprintDurationDays = settings.SprintDurationDays;
            MinimizeToTray = settings.MinimizeToTray;
            StartMinimized = settings.StartMinimized;
            LaunchOnStartup = settings.LaunchOnStartup;
            ShowWatcherNotification = settings.ShowWatcherNotification;
            ShowSprintEndingNotification = settings.ShowSprintEndingNotification;
            ShowSwapNotification = settings.ShowSwapNotification;
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        if (settings != null)
        {
            var oldDuration = settings.SprintDurationDays;
            settings.SprintDurationDays = SprintDurationDays;
            settings.MinimizeToTray = MinimizeToTray;
            settings.StartMinimized = StartMinimized;
            settings.LaunchOnStartup = LaunchOnStartup;
            settings.ShowWatcherNotification = ShowWatcherNotification;
            settings.ShowSprintEndingNotification = ShowSprintEndingNotification;
            settings.ShowSwapNotification = ShowSwapNotification;
            settings.LastModified = DateTime.Now;

            await _context.SaveChangesAsync();

            // If sprint duration changed, regenerate future sprints
            if (oldDuration != SprintDurationDays)
            {
                // Delete all planned (not started) sprints
                var plannedSprints = await _context.Sprints
                    .Where(s => s.Status == Models.Enums.SprintStatus.Planning)
                    .ToListAsync();

                _context.Sprints.RemoveRange(plannedSprints);
                await _context.SaveChangesAsync();

                // Regenerate sprints with new duration
                await _sprintService.GenerateFutureSprintsAsync(6);
            }

            System.Windows.MessageBox.Show("Settings saved successfully!", "Success",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private async Task BackupDatabaseAsync()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*",
            FileName = $"qasprinthub_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _exportService.BackupDatabaseAsync(dialog.FileName);
                System.Windows.MessageBox.Show("Database backed up successfully!", "Backup Complete",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Backup failed: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task RestoreDatabaseAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Database Files (*.db)|*.db|All Files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            var result = System.Windows.MessageBox.Show(
                "This will replace your current database. Are you sure?",
                "Confirm Restore",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    await _exportService.RestoreDatabaseAsync(dialog.FileName);
                    System.Windows.MessageBox.Show("Database restored successfully! Please restart the application.",
                        "Restore Complete", System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Restore failed: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    [RelayCommand]
    private void OpenDataFolder()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbDirectory = Path.Combine(appDataPath, "QASprintHub");

        if (Directory.Exists(dbDirectory))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = dbDirectory,
                UseShellExecute = true
            });
        }
    }
}
