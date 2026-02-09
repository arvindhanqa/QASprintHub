using System;

namespace QASprintHub.Services;

public interface ITrayService
{
    void Initialize();
    void UpdateCurrentWatcherInfo(string watcherName);
    void ShowMainWindow();
    void Shutdown();
    event EventHandler? OpenRequested;
    event EventHandler? ExitRequested;
}
