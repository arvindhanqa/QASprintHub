# QA Sprint Hub

A portable Windows desktop application for managing QA sprint watcher rotation and PR tracking.

## Overview

QA Sprint Hub is a .NET 8 WPF application designed to help QA teams manage:
- Sprint watcher rotation scheduling
- Pull request tracking for each sprint
- Watcher swap management with full history
- Backup watcher assignments
- Sprint history and reporting

## Features

- **At-a-glance Dashboard**: See current watcher, sprint status, and PR metrics
- **Sprint PR Tracking**: Track PRs with status, priority, and notes
- **Watcher Rotation**: Automated rotation with manual override support
- **Backup Watchers**: Assign backup coverage for specific days or full sprints
- **Swap History**: Full audit trail of watcher changes
- **System Tray Integration**: Runs in background with Windows notifications
- **Portable**: Single-folder deployment, no installation required
- **Offline-First**: Local SQLite database, no network required
- **Crash-Safe**: WAL journaling with automatic backups

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

### First Time Setup
1. Launch the application
2. Navigate to "Watchers" view
3. Add team members
4. Set rotation order
5. Create your first sprint from the Dashboard

### Daily Operations
1. Open the app (or use system tray icon)
2. Dashboard shows current watcher and sprint status
3. Add PRs to track in "Sprint PRs" view
4. Update PR status as work progresses
5. View sprint history in "History" view

### Backup & Restore
- Navigate to Settings → Backup Database
- Automatic backups are created daily (last 30 days retained)
- Manual backup/restore available in Settings

## Architecture

- **Framework**: .NET 8, WPF
- **UI Library**: WPF-UI (Fluent Design)
- **Architecture**: MVVM with CommunityToolkit.Mvvm
- **Database**: SQLite with Entity Framework Core
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection

## Security & Privacy

- **Zero network activity**: No HTTP, DNS, or socket connections
- **No telemetry**: No data collection or analytics
- **No admin rights**: Runs as standard user
- **Local-only data**: All data stored on your machine
- **Portable**: Database survives app reinstalls

## Project Structure

```
QASprintHub/
├── src/QASprintHub/
│   ├── Models/          # Data models
│   ├── Data/            # EF Core DbContext
│   ├── Services/        # Business logic
│   ├── ViewModels/      # MVVM ViewModels
│   ├── Views/           # XAML views
│   └── Converters/      # Value converters
├── build.ps1            # Build script
├── README.md            # This file
├── README_IT.txt        # IT security documentation
└── THIRD_PARTY_LICENSES.txt
```

## Contributing

This is an internal tool. For questions or feature requests, contact the QA team.

## License

Internal use only. See THIRD_PARTY_LICENSES.txt for open-source dependencies.

## Version

v1.0.0 - Initial Release
