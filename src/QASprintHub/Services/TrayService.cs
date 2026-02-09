using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QASprintHub.Services;

public class TrayService : ITrayService
{
    private TaskbarIcon? _trayIcon;
    private string _currentWatcher = "No active sprint";

    public event EventHandler? OpenRequested;
    public event EventHandler? ExitRequested;

    public void Initialize()
    {
        _trayIcon = new TaskbarIcon
        {
            IconSource = new System.Windows.Media.Imaging.BitmapImage(
                new Uri("pack://application:,,,/Assets/app-icon.ico")),
            ToolTipText = "QA Sprint Hub"
        };

        _trayIcon.TrayMouseDoubleClick += (s, e) => OpenRequested?.Invoke(this, EventArgs.Empty);

        BuildContextMenu();
    }

    public void UpdateCurrentWatcherInfo(string watcherName)
    {
        _currentWatcher = watcherName;
        BuildContextMenu();
    }

    public void ShowMainWindow()
    {
        OpenRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Shutdown()
    {
        _trayIcon?.Dispose();
    }

    private void BuildContextMenu()
    {
        if (_trayIcon == null) return;

        var contextMenu = new ContextMenu();

        var openItem = new MenuItem { Header = "Open QA Sprint Hub" };
        openItem.Click += (s, e) => OpenRequested?.Invoke(this, EventArgs.Empty);
        contextMenu.Items.Add(openItem);

        contextMenu.Items.Add(new Separator());

        var watcherItem = new MenuItem
        {
            Header = $"Current Watcher: {_currentWatcher}",
            IsEnabled = false
        };
        contextMenu.Items.Add(watcherItem);

        contextMenu.Items.Add(new Separator());

        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty);
        contextMenu.Items.Add(exitItem);

        _trayIcon.ContextMenu = contextMenu;
    }
}
