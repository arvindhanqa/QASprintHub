# QA Sprint Hub

<div align="center">

**A modern, portable Windows desktop application for QA sprint management**

*Built with .NET 8 WPF | Fluent Design | MVVM Architecture*

[Features](#features) • [Demo](#demo) • [Tech Stack](#tech-stack) • [Installation](#installation) • [Building](#building-from-source)

</div>

---

## Overview

QA Sprint Hub is a production-ready WPF application that streamlines QA team workflows by automating sprint watcher rotation, tracking pull requests, and maintaining comprehensive sprint history. Built with modern .NET practices and a focus on reliability, portability, and user experience.

### Key Capabilities
- **Automated Sprint Watcher Rotation** - Intelligent rotation scheduling with manual override support
- **Pull Request Tracking** - Comprehensive PR management with status, priority, and notes
- **Watcher Swap Management** - Full audit trail of all watcher changes and assignments
- **Backup Watcher System** - Flexible backup coverage for specific days or full sprints
- **Sprint History & Analytics** - Complete historical data with reporting capabilities
- **Calendar Diary View** - Visual sprint timeline with date-based navigation

## Demo

> *Screenshots coming soon - Application features a modern Fluent Design UI with dark mode support*

**Application Highlights:**
- 📊 Real-time Dashboard with sprint metrics and status indicators
- 📅 Interactive Calendar Diary with sprint timeline visualization
- 🔄 Watcher Management with drag-and-drop rotation ordering
- 📝 PR Tracking with inline editing and status management
- 🔔 System tray integration with Windows notifications
- ⚙️ Settings panel with database backup/restore functionality

## Features

### Core Functionality
- **📊 Real-time Dashboard**
  - Current watcher and sprint status at a glance
  - PR metrics with visual progress indicators
  - Quick actions for common tasks
  - Live updates without requiring refresh

- **📅 Calendar Diary View**
  - Visual sprint timeline with date navigation
  - "Today" button for instant current sprint access
  - Date picker integration with sprint mapping
  - Working day calculations (excludes weekends)

- **🔄 Smart Watcher Rotation**
  - Automated rotation based on sprint schedule
  - Manual override with full audit trail
  - Backup watcher assignments (daily or full sprint)
  - Conflict detection and resolution

- **📝 PR Tracking**
  - Status management (Open, In Review, Merged, Rejected)
  - Priority levels (Low, Medium, High, Critical)
  - Rich notes with inline editing
  - Per-sprint organization

### Technical Features
- **🚀 Performance**
  - Single instance enforcement with smart window restoration
  - Background system tray operation
  - Efficient SQLite queries with Entity Framework Core
  - Lazy loading for optimal memory usage

- **🛡️ Reliability**
  - WAL (Write-Ahead Logging) journaling for crash safety
  - Automatic daily backups (30-day retention)
  - Global exception handling with detailed logging
  - Graceful error recovery

- **🔒 Security & Privacy**
  - Zero network activity - completely offline
  - No telemetry or analytics
  - All data stored locally in user's AppData
  - No admin rights required

- **📦 Deployment**
  - Self-contained single-file executable
  - No installation or dependencies required
  - Portable - runs from any folder
  - Database location survives app moves

## Requirements

- Windows 10 or Windows 11
- .NET 8 Runtime (included in self-contained build)
- No admin rights required
- No internet connection required

## Building from Source

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or later (optional)

### Build Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/QASprintHub.git
   cd QASprintHub
   ```

2. Run the build script:
   ```powershell
   .\build.ps1
   ```

3. The built application will be in the `./publish` folder

### Manual Build
```bash
dotnet restore
dotnet publish src/QASprintHub/QASprintHub.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

## Installation

1. Copy the entire `publish` folder to any location
2. Run `QASprintHub.exe`
3. No installation or admin rights required

## Data Location

All data is stored in:
```
%APPDATA%\QASprintHub\qasprinthub.db
```

Example: `C:\Users\[YourName]\AppData\Roaming\QASprintHub\qasprinthub.db`

## Usage

### First Time Setup Wizard
The application features an intuitive setup wizard that guides users through:
1. **Sprint Configuration** - Define sprint duration (working days) and start date
2. **Team Setup** - Add team members with customizable names
3. **Initial Sprint Creation** - Automatically creates first sprint with watcher assignment

### Daily Workflow
```
🏠 Dashboard → View current sprint status and metrics
📅 Calendar Diary → Navigate sprints by date
📝 Sprint PRs → Track and manage pull requests
👥 Watcher Management → Manage team and rotation order
📊 History → Review past sprints and performance
⚙️ Settings → Backup/restore and configuration
```

### Advanced Features
- **Watcher Swaps**: Replace primary watcher with backup for specific dates
- **Sprint Navigation**: Use calendar picker or "Today" button for instant access
- **Backup Management**: Automatic daily backups + manual backup/restore
- **System Tray**: Minimize to tray for background operation
- **Single Instance**: Double-click executable to restore existing window

## Technical Challenges & Solutions

### Challenge 1: Single Instance with Window Restoration
**Problem**: Users double-clicking the executable expected to see the window, not just an error message.

**Solution**: Implemented inter-process communication using `EventWaitHandle` to signal the running instance to restore and activate its window.

```csharp
// Second instance signals the first
using var showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "QASprintHub_ShowWindowEvent");
showEvent.Set();

// First instance listens on background thread
showEvent.WaitOne();
Dispatcher.BeginInvoke(() => {
    mainWindow.Show();
    mainWindow.Activate();
});
```

### Challenge 2: WPF-UI Type Ambiguity
**Problem**: WPF-UI 3.x has its own `Window` class that conflicts with `System.Windows.Window`, causing compilation errors.

**Solution**: Removed `using Wpf.Ui.Controls;` and fully qualified WPF-UI types where needed, ensuring `Window` resolves to `System.Windows.Window`.

### Challenge 3: Calendar Navigation to Sprint Dates
**Problem**: Date picker and "Today" button needed to navigate to the sprint containing the selected date.

**Solution**: Created async navigation helper that switches to Calendar Diary view, waits for view initialization, then calls the ViewModel's date navigation method.

```csharp
private async Task NavigateToDateAsync(DateTime date)
{
    if (!(ContentFrame.Content is CalendarDiaryView))
    {
        NavigateTo("CalendarDiary");
        await Task.Delay(100); // Wait for navigation
    }

    if (ContentFrame.Content is CalendarDiaryView calendarView &&
        calendarView.DataContext is CalendarDiaryViewModel viewModel)
    {
        await viewModel.GoToDateAsync(date);
    }
}
```

### Challenge 4: Working Day Calculations
**Problem**: Sprint end dates must account for weekends without including them in the sprint duration.

**Solution**: Implemented smart date calculation that skips weekends while counting working days.

```csharp
private DateTime CalculateEndDate(DateTime startDate, int durationDays)
{
    var endDate = startDate;
    var workingDaysAdded = 0;

    while (workingDaysAdded < durationDays)
    {
        endDate = endDate.AddDays(1);
        if (endDate.DayOfWeek != DayOfWeek.Saturday &&
            endDate.DayOfWeek != DayOfWeek.Sunday)
        {
            workingDaysAdded++;
        }
    }

    return endDate.AddDays(-1);
}
```

## Tech Stack

### Frontend
- **.NET 8 WPF** - Modern Windows Presentation Foundation
- **WPF-UI 3.0** - Fluent Design System implementation
- **MVVM Pattern** - Clean separation of concerns with CommunityToolkit.Mvvm
- **Data Binding** - Two-way binding with INotifyPropertyChanged
- **Value Converters** - Custom converters for UI logic
- **System Tray** - Hardcodet.Wpf.TaskbarNotification for tray integration

### Backend
- **Entity Framework Core 8.0** - ORM with code-first migrations
- **SQLite** - Embedded database with WAL mode
- **Async/Await** - Throughout for responsive UI
- **Dependency Injection** - Microsoft.Extensions.DependencyInjection
- **Service Layer** - Clean architecture with interface-based services

### Development & Build
- **C# 12** - Latest language features
- **Nullable Reference Types** - Enabled for better null safety
- **Single-File Deployment** - Self-contained executable
- **PowerShell Build Script** - Automated build process

## Architecture

### Design Patterns
- **MVVM (Model-View-ViewModel)** - Clean UI/business logic separation
- **Repository Pattern** - Data access abstraction via services
- **Dependency Injection** - Constructor injection throughout
- **Observer Pattern** - INotifyPropertyChanged for reactive UI
- **Single Responsibility** - Each service handles one concern

### Project Structure
```
src/QASprintHub/
├── Models/              # Entity models and DTOs
│   ├── TeamMember.cs
│   ├── Sprint.cs
│   ├── PullRequest.cs
│   └── AppSettings.cs
├── Data/                # EF Core configuration
│   └── AppDbContext.cs
├── Services/            # Business logic layer
│   ├── ITeamService.cs / TeamService.cs
│   ├── ISprintService.cs / SprintService.cs
│   ├── IPRService.cs / PRService.cs
│   ├── IWatcherService.cs / WatcherService.cs
│   ├── ITrayService.cs / TrayService.cs
│   └── INotificationService.cs / NotificationService.cs
├── ViewModels/          # MVVM ViewModels
│   ├── DashboardViewModel.cs
│   ├── CalendarDiaryViewModel.cs
│   ├── SprintPRsViewModel.cs
│   ├── WatcherManagementViewModel.cs
│   ├── HistoryViewModel.cs
│   └── SettingsViewModel.cs
├── Views/               # XAML views
│   ├── DashboardView.xaml
│   ├── CalendarDiaryView.xaml
│   ├── SprintPRsView.xaml
│   └── ...
├── Converters/          # Value converters
│   ├── BoolToVisibilityConverter.cs
│   └── ...
└── App.xaml            # Application entry point
```

### Key Technical Implementations

**Database Design**
- Entity relationships with navigation properties
- Soft deletes with `IsDeleted` flags
- Timestamp tracking (CreatedDate, LastModified)
- Composite keys where appropriate

**Concurrency & Threading**
- Background thread for inter-process communication
- EventWaitHandle for single-instance enforcement
- Dispatcher.Invoke for cross-thread UI updates
- Async database operations throughout

**Error Handling**
- Global exception handlers (AppDomain & Dispatcher)
- Structured error logging to AppData
- User-friendly error messages
- Graceful degradation for non-critical failures

## Security & Privacy

This application was designed with enterprise security in mind:

- ✅ **Zero Network Activity** - No HTTP, DNS, or socket connections
- ✅ **No Telemetry** - No data collection, analytics, or tracking
- ✅ **No Admin Rights** - Runs as standard user with minimal privileges
- ✅ **Local-Only Data** - All data stored in user's AppData folder
- ✅ **Portable Database** - Data survives app reinstalls and moves
- ✅ **Crash-Safe** - SQLite WAL mode prevents data corruption
- ✅ **Error Logging** - Local-only logs in AppData for troubleshooting

**Data Location**: `%APPDATA%\QASprintHub\` (Example: `C:\Users\[YourName]\AppData\Roaming\QASprintHub\`)

## Lessons Learned

### MVVM Best Practices
- **Service Lifetimes Matter**: Learned to use `AddTransient` for services that depend on scoped `DbContext` to avoid capturing scoped instances in singletons
- **UI Thread Awareness**: Always use `Dispatcher.Invoke` when updating UI from background threads
- **RelayCommand**: CommunityToolkit.Mvvm's `[RelayCommand]` attribute dramatically reduces boilerplate

### WPF Gotchas
- **Pack URIs**: Resource loading requires proper `pack://application:,,,/` syntax
- **DataContext Inheritance**: Understanding how DataContext flows through the visual tree
- **Value Converters**: Essential for keeping ViewModels clean of UI logic

### Entity Framework Core
- **Scope Management**: Always create scopes for DbContext to avoid lifetime issues
- **Async All The Way**: Using async methods throughout prevents UI freezing
- **Migration vs. EnsureCreated**: Used `EnsureCreated` for simpler deployment without migration files

### Windows Development
- **Single Instance Apps**: Mutex + EventWaitHandle pattern for robust single-instance enforcement
- **System Tray**: Requires proper lifecycle management to avoid resource leaks
- **File Paths**: Always use `Environment.SpecialFolder` for cross-user compatibility

## Future Enhancements

Potential features for future versions:

- 🎨 **Theming**: Dark/light mode toggle with custom accent colors
- 📊 **Advanced Analytics**: Sprint velocity, PR turnaround time, watcher load distribution
- 📤 **Export**: Excel/CSV export for sprint reports
- 🔔 **Enhanced Notifications**: Toast notifications for sprint transitions and PR updates
- 👥 **Team Profiles**: Member avatars and contact information
- 📅 **Sprint Planning**: PR estimation and capacity planning tools
- 🔍 **Search & Filter**: Advanced filtering for PR history
- 🌐 **GitHub Integration**: Optional PR sync from GitHub API (maintaining offline-first approach)

## Development Highlights

**What I'm Proud Of:**
- 🎯 **Clean Architecture**: Strict MVVM with no code-behind logic
- 🧪 **Robust Error Handling**: Comprehensive exception handling with user-friendly messages
- ⚡ **Performance**: Snappy UI with all database calls async
- 📦 **Deployment**: True single-file executable with no dependencies
- 🔧 **Maintainability**: Interface-based services make testing and updates easy
- 📖 **Self-Documenting**: Clear naming and minimal comments needed

**Lines of Code**: ~5,000+ lines of C# (excluding generated code)

**Development Time**: Built iteratively with focus on quality and user experience

## Dependencies

### NuGet Packages
- **Microsoft.EntityFrameworkCore.Sqlite** (8.0.x) - Database ORM
- **CommunityToolkit.Mvvm** (8.x) - MVVM helpers
- **Wpf.Ui** (3.0.5) - Fluent Design controls
- **Hardcodet.Wpf.TaskbarNotification** - System tray integration

See `THIRD_PARTY_LICENSES.txt` for full license information.

## Project Status

**Current Version**: v1.0.0

**Status**: ✅ Production Ready

**Maintenance**: Active - Accepting feedback and feature requests

## Contact

**Developer**: Aravindhan Rajasekaran
 -  Current Role: Lead Test Engineer @ Acumatica (2020-Present)
 -  Experience: 10+ years in QA, Test Automation, and Software Development
 -  Expertise: C#, Java, Python, Selenium, API Testing, CI/CD
 -  Certifications: ISTQB Certified, Certified Scrum Master
 -  GitHub: @arvindhanqa
 -  LinkedIn: linkedin.com/in/aravindrajsekar
 -  Location: Saskatoon, Saskatchewan, Canada

## Acknowledgments

Built with modern .NET tools and community libraries:
- **WPF-UI** team for the excellent Fluent Design implementation
- **CommunityToolkit** team for MVVM helpers
- **.NET Foundation** for Entity Framework Core
- **Microsoft** for the incredible WPF framework

---

<div align="center">

**⭐ If you find this project interesting, please consider starring it! ⭐**

*Built with ❤️ using .NET 8 and WPF*

</div>
