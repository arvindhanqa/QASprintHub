using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QASprintHub.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace QASprintHub.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IExportService _exportService;

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

    public SettingsViewModel(IExportService exportService)
    {
        _exportService = exportService;

        // Get database location
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        DatabaseLocation = Path.Combine(appDataPath, "QASprintHub", "qasprinthub.db");
    }

    public void LoadSettings()
    {
        // TODO: Load settings from configuration file or registry
    }

    public void SaveSettings()
    {
        // TODO: Save settings to configuration file or registry
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
