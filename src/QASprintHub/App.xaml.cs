using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QASprintHub.Data;
using QASprintHub.Services;
using QASprintHub.ViewModels;
using QASprintHub.Views;
using System;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Windows;

namespace QASprintHub;

public partial class App : Application
{
    private readonly IHost _host;
    private static Mutex? _instanceMutex;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Database
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dbDirectory = Path.Combine(appDataPath, "QASprintHub");
                Directory.CreateDirectory(dbDirectory);
                var dbPath = Path.Combine(dbDirectory, "qasprinthub.db");

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath};Mode=ReadWriteCreate;"));

                // Services
                // Services that depend on AppDbContext should be transient to avoid capturing scoped DbContext in singletons
                services.AddTransient<ITeamService, TeamService>();
                services.AddTransient<ISprintService, SprintService>();
                services.AddTransient<IWatcherService, WatcherService>();
                services.AddTransient<IPRService, PRService>();
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<ITrayService, TrayService>();
                services.AddTransient<IExportService, ExportService>();

                // ViewModels
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<SprintPRsViewModel>();
                services.AddTransient<WatcherManagementViewModel>();
                services.AddTransient<HistoryViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<CalendarDiaryViewModel>();
                services.AddTransient<ViewModels.Dialogs.SetupWizardViewModel>();

                // Views
                services.AddTransient<DashboardView>();
                services.AddTransient<SprintPRsView>();
                services.AddTransient<WatcherManagementView>();
                services.AddTransient<HistoryView>();
                services.AddTransient<SettingsView>();
                services.AddTransient<CalendarDiaryView>();
                services.AddTransient<Views.Dialogs.SetupWizardDialog>();

                // Main Window
                services.AddSingleton<MainWindow>(sp =>
                {
                    // Resolve required services on demand to avoid capturing scoped services in constructor
                    return ActivatorUtilities.CreateInstance<MainWindow>(sp,
                        sp.GetRequiredService<ITrayService>(),
                        sp.GetRequiredService<INotificationService>(),
                        sp.GetRequiredService<ISprintService>());
                });
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Single instance check
        bool createdNew;
        _instanceMutex = new Mutex(true, "QASprintHub_SingleInstanceMutex", out createdNew);

        if (!createdNew)
        {
            // Another instance is already running
            MessageBox.Show("QA Sprint Hub is already running. Check the system tray.",
                "Already Running",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        await _host.StartAsync();

        // Ensure database is created
        var dbContext = _host.Services.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Check if setup is needed
        var settingsDb = _host.Services.GetRequiredService<AppDbContext>();

        // Ensure AppSettings table exists (avoid EF query exceptions on older DBs)
        var ensureSettingsTableSql = @"CREATE TABLE IF NOT EXISTS AppSettings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SprintDurationDays INTEGER NOT NULL,
    FirstSprintStartDate TEXT NULL,
    IsConfigured INTEGER NOT NULL,
    CreatedDate TEXT NOT NULL,
    LastModified TEXT NOT NULL
);";
        try
        {
            await settingsDb.Database.ExecuteSqlRawAsync(ensureSettingsTableSql);
        }
        catch
        {
            // ignore failures here; we'll treat as not configured below
        }

        QASprintHub.Models.AppSettings? appSettings = null;
        try
        {
            appSettings = await settingsDb.AppSettings.FirstOrDefaultAsync();
        }
        catch
        {
            // ignore - treat as no settings
        }

        var teamService = _host.Services.GetRequiredService<ITeamService>();
        var activeMembers = await teamService.GetActiveMembersAsync();

        var needsSetup = appSettings == null || !appSettings.IsConfigured || activeMembers.Count == 0;

        if (needsSetup)
        {
            // Show setup wizard for first-time users
            var setupWizard = _host.Services.GetRequiredService<Views.Dialogs.SetupWizardDialog>();
            var setupViewModel = _host.Services.GetRequiredService<ViewModels.Dialogs.SetupWizardViewModel>();
            setupWizard.DataContext = setupViewModel;

            setupWizard.ShowDialog();

            if (!setupWizard.SetupCompleted)
            {
                // User cancelled setup, exit application
                Shutdown();
                return;
            }
        }

        // Initialize tray service
        var trayService = _host.Services.GetRequiredService<ITrayService>();
        try
        {
            trayService.Initialize();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error initializing tray service: {ex.Message}", "Startup Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        // Show main window
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        try
        {
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error showing main window: {ex.Message}", "Startup Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        base.OnStartup(e);

        // Save data on system shutdown/session end
        SystemEvents.SessionEnding += (s, ev) =>
        {
            try
            {
                var dbContext = _host.Services.GetRequiredService<AppDbContext>();
                dbContext.SaveChanges();
            }
            catch
            {
                // ignore
            }
        };
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        // Cleanup tray service
        var trayService = _host.Services.GetRequiredService<ITrayService>();
        trayService.Shutdown();

        try
        {
            var dbContext = _host.Services.GetRequiredService<AppDbContext>();
            await dbContext.SaveChangesAsync();
        }
        catch
        {
            // ignore
        }

        await _host.StopAsync();
        _host.Dispose();

        // Release single instance mutex
        _instanceMutex?.ReleaseMutex();
        _instanceMutex?.Dispose();

        base.OnExit(e);
    }

    public static T GetService<T>() where T : class
    {
        if ((Current as App)?._host?.Services?.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }
}
