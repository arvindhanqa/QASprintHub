using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QASprintHub.Data;
using QASprintHub.Services;
using QASprintHub.ViewModels;
using QASprintHub.Views;
using System;
using System.IO;
using System.Windows;

namespace QASprintHub;

public partial class App : Application
{
    private readonly IHost _host;

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
                services.AddSingleton<ITeamService, TeamService>();
                services.AddSingleton<ISprintService, SprintService>();
                services.AddSingleton<IWatcherService, WatcherService>();
                services.AddSingleton<IPRService, PRService>();
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<ITrayService, TrayService>();
                services.AddSingleton<IExportService, ExportService>();

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
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Ensure database is created
        var dbContext = _host.Services.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Check if setup is needed
        var teamService = _host.Services.GetRequiredService<ITeamService>();
        var activeMembers = await teamService.GetActiveMembersAsync();

        if (activeMembers.Count == 0)
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
        trayService.Initialize();

        // Show main window
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        // Cleanup tray service
        var trayService = _host.Services.GetRequiredService<ITrayService>();
        trayService.Shutdown();

        await _host.StopAsync();
        _host.Dispose();

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
