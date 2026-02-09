QA SPRINT HUB — IT Information Sheet
=====================================

Purpose:     Internal QA team tool for tracking sprint watcher rotation and PR lists
Type:        .NET 8 WPF desktop application (Windows only)
Network:     NONE — zero network activity, fully offline
Data:        Local SQLite database in %APPDATA%\QASprintHub\
Permissions: Standard user (asInvoker), no admin rights
Registry:    Optional HKCU startup entry only
Source:      Built internally by the QA team

File Hashes (SHA-256):
  QASprintHub.exe: [Generated during build - see build.ps1 output]

Dependencies (all NuGet packages, open source):
  - Microsoft.EntityFrameworkCore.Sqlite v8.0.0 (Microsoft, MIT License)
  - CommunityToolkit.Mvvm v8.2.2 (Microsoft, MIT License)
  - WPF-UI v3.0.5 (lepoco/wpfui, MIT License)
  - Hardcodet.NotifyIcon.Wpf v1.1.0 (Community, CPOL License)
  - Microsoft.Toolkit.Uwp.Notifications v7.1.3 (Microsoft, MIT License)
  - Microsoft.Extensions.DependencyInjection v8.0.0 (Microsoft, MIT License)
  - Microsoft.Extensions.Hosting v8.0.0 (Microsoft, MIT License)

This application:
  ✓ Makes no internet connections
  ✓ Collects no telemetry
  ✓ Stores no passwords or credentials
  ✓ Accesses only its own data directory (%APPDATA%\QASprintHub\)
  ✓ Requires no admin privileges
  ✓ Can be fully blocked by firewall with no impact
  ✓ Runs in system tray (optional)
  ✓ Shows Windows toast notifications (can be disabled)

Build Instructions:
  1. Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0
  2. Open PowerShell in the project directory
  3. Run: .\build.ps1
  4. Output will be in the ./publish folder

Database Location:
  %APPDATA%\QASprintHub\qasprinthub.db
  (e.g., C:\Users\[Username]\AppData\Roaming\QASprintHub\qasprinthub.db)

Deployment:
  - Copy the entire ./publish folder to any location
  - Run QASprintHub.exe
  - No installation required
  - Portable application

Security:
  - No network access configured
  - SQLite database with WAL journaling for crash safety
  - All data stored locally
  - No external dependencies at runtime

For questions or concerns, contact the QA team.
