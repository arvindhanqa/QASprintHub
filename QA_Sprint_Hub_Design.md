# QA Sprint Hub — Design Document

**Version:** 1.0  
**Author:** Aravindhan  
**Date:** February 2026  
**Status:** Design Phase

---

## 1. Problem Statement

The QA team (7 members) manages sprint operations using a combination of Excel files and chat messages, leading to:

- **QA Watcher duty is hard to track** — Stored in Excel, not immediately visible. Team members waste time finding who's on duty.
- **Sprint PR lists get lost** — Merge lists shared via chat disappear in the conversation history. No persistent, searchable record.
- **Watcher swaps are painful** — When a watcher is unavailable, there's no structured way to reassign, track, or communicate the change.

---

## 2. Solution Overview

**QA Sprint Hub** is a portable Windows desktop application (WPF, .NET 8) that provides:

- At-a-glance dashboard showing the current QA watcher and sprint status
- Sprint PR tracking with links and notes
- Watcher rotation management with swap history
- Sprint history and reporting

### Design Principles

1. **Zero friction** — Open the app, immediately see who's the watcher and what needs attention
2. **Portable** — Single-folder deployment, no installer, no admin rights needed
3. **Offline-first** — Local SQLite database, sync-ready architecture for future
4. **Simple over clever** — No integrations to break; links and notes over API coupling

---

## 3. Tech Stack

| Component | Technology |
|-----------|-----------|
| Framework | .NET 8, WPF |
| UI Library | WPF UI (Fluent / Windows 11 style) |
| Architecture | MVVM (CommunityToolkit.Mvvm) |
| Database | SQLite via Entity Framework Core |
| Deployment | Single-folder portable (self-contained publish) |
| Package Manager | NuGet |

### Key NuGet Packages

- `Microsoft.EntityFrameworkCore.Sqlite`
- `CommunityToolkit.Mvvm`
- `WPF-UI` (lepoco/wpfui)
- `Microsoft.Toolkit.Uwp.Notifications` (Windows toast notifications)
- `Microsoft.Extensions.DependencyInjection`
- `Hardcodet.NotifyIcon.Wpf` (system tray icon support)

---

## 4. Data Model

### 4.1 Team Members

| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment |
| Name | string | Full name (display name for rotation) |
| Email | string | Email address (optional) |
| RotationOrder | int | Current position in rotation (1-based, auto-compresses on removal) |
| Status | enum | Active, Departed |
| DepartedDate | DateTime? | When the member was removed (null if active) |
| CreatedDate | DateTime | When the member was added |

**Key behavior:**
- `RotationOrder` is a living value — when someone is removed, everyone below shifts up
- `Departed` members are kept in DB for historical records but excluded from rotation
- Adding a new member assigns `RotationOrder = max + 1` by default

### 4.2 Sprints

| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment |
| StartDate | DateTime | Sprint start date |
| EndDate | DateTime | Sprint end date |
| WatcherId | int (FK) | Currently assigned QA watcher |
| Status | enum | Planning, Active, Completed |
| Notes | string | General sprint notes |
| CreatedDate | DateTime | When the sprint was created |

> **Display Name** is auto-generated from dates: `"MMM D – MMM D, YYYY"` (e.g., "Feb 3 – Feb 14, 2026"). Not stored in DB.

### 4.3 Backup Watchers

Tracks when a backup watcher is assigned to cover part or all of a sprint.

| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment |
| SprintId | int (FK) | Which sprint this backup belongs to |
| BackupMemberId | int (FK) | Who is the backup watcher |
| StartDate | DateTime | First day of backup coverage |
| EndDate | DateTime | Last day of backup coverage |
| CoverageType | enum | PartialDays, FullWeek, FullSprint |
| Notes | string | Context (e.g., "Covering Mon–Wed while John is in training") |
| CreatedDate | DateTime | When the record was created |

**Key behavior:**
- A sprint can have zero or one backup watcher at a time
- Backup does NOT change the primary watcher — they're still "the watcher"
- Coverage dates must fall within the sprint date range
- Backup is visible on the Dashboard alongside the primary watcher
- If backup covers the full sprint, it's still different from a swap (primary remains credited)

### 4.4 Watcher Swaps

Tracks when the scheduled watcher was unavailable and someone else took over.

| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment |
| SprintId | int (FK) | Which sprint this swap belongs to |
| ScheduledWatcherId | int (FK) | Who was originally next in rotation |
| ActualWatcherId | int (FK) | Who actually took over |
| SwapDate | DateTime | When the swap happened |
| Reason | string | Why (PTO, Sick, Emergency, Other) |
| CreatedDate | DateTime | When the record was created |

**Key behavior:**
- A swap means the scheduled person was skipped for THIS sprint only
- The skipped person remains in their rotation position — they'll come up again next cycle
- If someone is removed from the team entirely, that's handled via TeamMember.Status = Departed, not via swaps

### 4.4 Sprint PRs

| Column | Type | Description |
|--------|------|-------------|
| Id | int (PK) | Auto-increment |
| SprintId | int (FK) | Which sprint this PR belongs to |
| Title | string | Short description of the PR |
| Link | string | URL to the PR (Bitbucket or any platform) |
| Author | string | Who created the PR |
| Status | enum | Pending, Merged, Blocked, Declined |
| Priority | enum | Low, Normal, High, Critical |
| Notes | string | Additional context, merge notes, blockers |
| AddedDate | DateTime | When the PR was added to tracking |
| StatusChangedDate | DateTime | Last status change |
| CreatedDate | DateTime | When the record was created |

### ER Diagram (Text)

```
TeamMember (1) ──── (*) Sprint.WatcherId
TeamMember (1) ──── (*) BackupWatcher.BackupMemberId
TeamMember (1) ──── (*) WatcherSwap.ScheduledWatcherId
TeamMember (1) ──── (*) WatcherSwap.ActualWatcherId
Sprint     (1) ──── (*) BackupWatcher
Sprint     (1) ──── (*) WatcherSwap
Sprint     (1) ──── (*) SprintPR
```

**Rotation logic (pseudo-code):**
```
activeMembers = TeamMembers.Where(Status == Active).OrderBy(RotationOrder)
lastWatcher = MostRecentSprint.WatcherId
nextIndex = (activeMembers.IndexOf(lastWatcher) + 1) % activeMembers.Count
nextWatcher = activeMembers[nextIndex]
```

---

## 5. UI Design

### 5.1 Navigation Structure

**Top bar**: Month-based calendar navigation (like Outlook's month picker). All views filter data by the selected month.

**Left sidebar**: Fluent-style navigation for switching between views.

```
┌──────────────────────────────────────────────────────┐
│  ◄  February 2026  ►                    [Today] 🔔  │
├──────────────┬───────────────────────────────────────┤
│              │                                      │
│  🏠 Dashboard │  [Active View Content]               │
│              │  (filtered to selected month)         │
│  📋 Sprint PRs│                                      │
│              │                                      │
│  👁 Watchers  │                                      │
│              │                                      │
│  📊 History   │                                      │
│              │                                      │
│  ⚙ Settings  │                                      │
│              │                                      │
└──────────────┴───────────────────────────────────────┘
```

**Month Navigation Rules:**
- `◄` / `►` arrows move one month back/forward
- `[Today]` button jumps back to the current month
- All views (Dashboard, Sprint PRs, History) respect the selected month
- If a sprint spans two months (e.g., Jan 27 – Feb 7), it appears in BOTH months
- Default on app launch: current month

### 5.2 System Tray & Background Mode

The app runs in the background and lives in the Windows system tray (notification area).

**Behavior:**
- Closing the window (X button) minimizes to system tray, does NOT exit
- System tray icon shows the app logo
- Right-click tray icon menu:
  - **Open QA Sprint Hub** — restore the window
  - **Current Watcher: [Name]** — info display (non-clickable)
  - **Exit** — fully close the app

**Windows Toast Notifications:**

On app startup (or when the app detects a new sprint has started), a Windows toast notification is shown:

```
┌──────────────────────────────────────────┐
│ 🛡️ QA Sprint Hub                         │
│                                          │
│ John Smith is the QA Watcher             │
│ for the next 10 working days             │
│ (Feb 3 – Feb 14, 2026)                  │
│                                          │
│ Next watcher: Jane Doe                   │
│                                          │
│              [Dismiss]  [Open App]        │
└──────────────────────────────────────────┘
```

**Notification Triggers:**
- **App startup**: Shows current watcher info (and backup if assigned)
- **Sprint start day**: If the app is running in background when a new sprint begins
- **Watcher swap**: When a swap is made, notifies with the change
- **Backup assigned/removed**: When a backup watcher is assigned or removed
- **Sprint ending soon**: 1 working day before sprint ends, reminder notification

**Implementation Notes:**
- Use `Microsoft.Toolkit.Uwp.Notifications` for Windows toast notifications
- Use `System.Windows.Forms.NotifyIcon` for system tray (or WPF UI tray support)
- App startup behavior: Start minimized to tray (optional setting) or open window
- Add to Windows Startup (optional setting in Settings view)

### 5.2 Dashboard (Home View)

The first thing anyone sees when opening the app. Answers the question: **"Who's the watcher and what's happening?"**

```
┌──────────────────────────────────────────────────────┐
│                                                      │
│  CURRENT QA WATCHER                                  │
│  ┌────────────────────────────────────────────────┐  │
│  │  🛡️  John Smith                                │  │
│  │  Feb 3 – Feb 14, 2026                         │  │
│  │  Day 6 of 10 working days                      │  │
│  │                                                │  │
│  │  🔄 Backup: Sarah Kim (Feb 5–7)               │  │
│  │                                                │  │
│  │  [Swap Watcher]  [Assign Backup]               │  │
│  └────────────────────────────────────────────────┘  │
```

**Backup display rules:**
- If no backup assigned → "Backup: None" or hide the row entirely
- If backup covers specific days → show "Backup: Name (Feb 5–7)"
- If backup covers full sprint → show "Backup: Name (Full Sprint)"
- Backup row only visible when one is assigned
│                                                      │
│  SPRINT SNAPSHOT                                     │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐  │
│  │ 12 Total │ │ 5 Pending│ │ 6 Merged │ │1 Block │  │
│  │    PRs   │ │          │ │          │ │        │  │
│  └──────────┘ └──────────┘ └──────────┘ └────────┘  │
│                                                      │
│  UPCOMING                                            │
│  Next Watcher: Jane Doe (Feb 17 – Feb 28, 2026)     │
│  Sprint ends in: 4 working days                      │
│                                                      │
│  RECENT ACTIVITY                                     │
│  • PR #4521 merged by Mike (2 hours ago)             │
│  • Watcher swapped: John → Sarah (yesterday)         │
│  • PR #4518 marked as blocked (yesterday)            │
│                                                      │
└──────────────────────────────────────────────────────┘
```

**Key interactions:**
- Click watcher card → opens swap dialog
- Click PR stats → navigates to Sprint PRs view filtered by status
- Sprint progress bar shows time elapsed

### 5.3 Sprint PRs View

Manages the list of PRs to be merged within the current sprint.

```
┌──────────────────────────────────────────────────────┐
│  Sprint PRs — Feb 3 – Feb 14, 2026          [v]    │
│                                                      │
│  [+ Add PR]  [Paste Link]   Filter: [All Statuses v] │
│                                                      │
│  ┌────┬────────────┬──────────┬────────┬──────────┐  │
│  │ #  │ Title      │ Author   │ Status │ Priority │  │
│  ├────┼────────────┼──────────┼────────┼──────────┤  │
│  │ 1  │ Fix login  │ Mike R.  │🟡 Pend │ Normal   │  │
│  │    │ timeout    │          │        │          │  │
│  ├────┼────────────┼──────────┼────────┼──────────┤  │
│  │ 2  │ Update     │ Sarah K. │🟢 Mrgd │ High     │  │
│  │    │ API schema │          │        │          │  │
│  ├────┼────────────┼──────────┼────────┼──────────┤  │
│  │ 3  │ Refactor   │ Jane D.  │🔴 Blkd │ Critical │  │
│  │    │ auth flow  │          │        │          │  │
│  └────┴────────────┴──────────┴────────┴──────────┘  │
│                                                      │
│  Selected PR Details                                 │
│  ┌────────────────────────────────────────────────┐  │
│  │ Title: Fix login timeout                       │  │
│  │ Link: https://bitbucket.org/team/repo/PR/4521  │  │
│  │ Author: Mike R.                                │  │
│  │ Status: Pending  [Change Status v]             │  │
│  │ Priority: Normal [Change Priority v]           │  │
│  │ Notes: Waiting for backend fix to deploy first │  │
│  │ Added: Feb 5, 2026                             │  │
│  │                                                │  │
│  │ [Open Link]  [Edit]  [Delete]                  │  │
│  └────────────────────────────────────────────────┘  │
│                                                      │
└──────────────────────────────────────────────────────┘
```

**Key interactions:**
- **Add PR**: Opens dialog with Title, Link (optional), Author, Priority, Notes
- **Paste Link**: Paste any URL, it stores as-is (no parsing, platform-agnostic)
- **Open Link**: Launches the link in default browser
- **Inline status change**: Click status badge to cycle through statuses
- **Drag to reorder** (optional v2)
- **Sprint selector dropdown**: View PRs from past sprints
- **Keyboard shortcut**: Ctrl+N to add new PR, Ctrl+V to paste link

### 5.4 Watcher Management View

Manages the rotation schedule and handles swaps.

**Core concept:** The rotation is a simple ordered list of people. The app cycles through the list in order, sprint by sprint. No slots, no complexity.

- **Add person** → appended to end of list, can be repositioned
- **Remove person** → list compresses automatically (no gaps, order adjusts)
- **Person unavailable (vacation/sick)** → skipped, next person takes it, logged as a swap
- **Reorder** → move up/down to change rotation position anytime

```
┌──────────────────────────────────────────────────────┐
│  QA Watcher Rotation                                 │
│                                                      │
│  ROTATION ORDER              UPCOMING SCHEDULE       │
│  ┌───┬──────────────────┐   ┌──────────────────────┐ │
│  │ 1 │ John Smith    [x]│   │ Feb 3–14:   John     │ │
│  │ 2 │ Jane Doe      [x]│   │ Feb 17–28:  Jane     │ │
│  │ 3 │ Mike Ross     [x]│   │ Mar 3–14:   Mike     │ │
│  │ 4 │ Sarah Kim     [x]│   │ Mar 17–28:  Sarah    │ │
│  │ 5 │ Alex Chen     [x]│   │ Mar 31–Apr 11: Alex  │ │
│  │ 6 │ Priya Patel   [x]│   │ Apr 14–25:  Priya    │ │
│  │ 7 │ Tom Wilson    [x]│   │ Apr 28–May 9: Tom    │ │
│  └───┴──────────────────┘   └──────────────────────┘ │
│  [↑ Up] [↓ Down] [+ Add Member] [x] = Remove        │
│                                                      │
│  SWAP HISTORY                                        │
│  ┌─────────────────┬───────────┬────────────┬──────┐ │
│  │ Sprint          │ Scheduled │ Actual     │Reason│ │
│  ├─────────────────┼───────────┼────────────┼──────┤ │
│  │ Feb 3–14, 2026  │ Tom W.    │ John S.    │ PTO  │ │
│  │ Jan 6–17, 2026  │ Mike R.   │ Alex C.    │ Sick │ │
│  └─────────────────┴───────────┴────────────┴──────┘ │
│                                                      │
│  Note: Skipping a person due to vacation does NOT     │
│  change the rotation order. They come back in their   │
│  normal position next cycle.                          │
│                                                      │
└──────────────────────────────────────────────────────┘
```

**Remove behavior:**
- Removing "Mike" (position 3) → Sarah becomes 3, Alex becomes 4, etc.
- If Mike was scheduled for an upcoming sprint, that sprint's watcher auto-reassigns to the next person
- Mike's name stays in historical sprint records (he was the watcher, that fact doesn't change)

**Add behavior:**
- New member "Raj" added → becomes position 8 (or wherever you move him)
- Immediately eligible for upcoming sprint rotation

### 5.5 History View

Browse past sprints and their data.

```
┌──────────────────────────────────────────────────────┐
│  Sprint History                                      │
│                                                      │
│  ┌─────────────────┬──────────┬────┬────┬────┬──────────┐ │
│  │ Sprint          │ Watcher  │ PR │Mrgd│Blkd│ Status   │ │
│  ├─────────────────┼──────────┼────┼────┼────┼──────────┤ │
│  │ Feb 3–14, 2026  │ John S.  │ 12 │ 6  │ 1  │ Active   │ │
│  │ Jan 20–31, 2026 │ Tom W.   │  8 │ 8  │ 0  │ Complete │ │
│  │ Jan 6–17, 2026  │ Alex C.* │ 15 │ 14 │ 1  │ Complete │ │
│  └─────────────────┴──────────┴────┴────┴────┴──────────┘ │
│  * = swap occurred                                   │
│                                                      │
│  [Export to Excel]                                    │
│                                                      │
└──────────────────────────────────────────────────────┘
```

### 5.6 Settings View

```
┌──────────────────────────────────────────────────────┐
│  Settings                                            │
│                                                      │
│  SPRINT DEFAULTS                                     │
│  Sprint Duration: [10 working days]                  │
│                                                      │
│  APP BEHAVIOR                                        │
│  ☑ Minimize to system tray on close                  │
│  ☑ Start minimized to tray                           │
│  ☐ Launch on Windows startup                         │
│                                                      │
│  NOTIFICATIONS                                       │
│  ☑ Show watcher notification on startup              │
│  ☑ Notify when sprint is ending (1 day before)       │
│  ☑ Notify on watcher swap                            │
│                                                      │
│  DATA                                                │
│  Database Location: C:\QASprintHub\data.db           │
│  [Export All Data to Excel]                          │
│  [Backup Database]                                   │
│  [Restore Database]                                  │
│                                                      │
│  ABOUT                                               │
│  QA Sprint Hub v1.0.0                                │
│                                                      │
└──────────────────────────────────────────────────────┘
```

---

## 6. Key Workflows

### 6.1 Starting a New Sprint

1. User clicks **"New Sprint"** (button on Dashboard or Sprint PRs view)
2. App auto-fills:
   - Sprint name auto-generated from dates (e.g., "Feb 3 – Feb 14, 2026")
   - Start date = today (or next Monday)
   - End date = start + 2 weeks
   - Watcher = next person in rotation
3. User reviews and confirms
4. Previous sprint auto-marked as **Completed**
5. Dashboard updates immediately

### 6.2 Swapping a Watcher

1. User clicks **"Swap Watcher"** on Dashboard
2. Dialog shows:
   - Current watcher (read-only)
   - Dropdown: Select replacement (active members only)
   - Reason field (required): PTO, Sick, Emergency, Other
3. User confirms
4. Swap is logged in history
5. Dashboard updates to show new watcher
6. Rotation order is NOT affected — next sprint still follows the original order

### 6.3 Assigning a Backup Watcher

1. User clicks **"Assign Backup"** on Dashboard or Watcher Management view
2. Dialog shows:
   - Current sprint & primary watcher (read-only)
   - Dropdown: Select backup person (active members, excluding primary watcher)
   - Coverage type:
     - **Specific days**: Date picker for start and end date (within sprint range)
     - **Full week**: Pick Week 1 or Week 2 of the sprint
     - **Full sprint**: Auto-fills sprint start and end dates
   - Notes (optional): Context for why backup is needed
3. User confirms
4. Dashboard updates to show backup alongside primary watcher
5. Notification toast: "Sarah Kim is backup watcher Feb 5–7"

**Rules:**
- Only one backup at a time per sprint (assign a new one replaces the old)
- To remove a backup, click "Remove Backup" on the Dashboard card
- Backup person is NOT affected in rotation — this is a temporary assist role

### 6.4 Adding a PR to Track

1. User clicks **"+ Add PR"** or **Ctrl+N**
2. Dialog with fields:
   - Title (required)
   - Link (optional — paste any URL)
   - Author (optional — dropdown of team members or free text)
   - Priority (default: Normal)
   - Notes (optional)
3. PR appears in the Sprint PRs list with status **Pending**

### 6.5 Updating PR Status

1. User clicks status badge on PR row OR opens PR details
2. Selects new status: Pending → Merged / Blocked / Declined
3. StatusChangedDate auto-updates
4. Dashboard stats refresh

---

## 7. Project Structure

```
QASprintHub/
├── QASprintHub.sln
├── src/
│   └── QASprintHub/
│       ├── App.xaml / App.xaml.cs
│       ├── MainWindow.xaml / MainWindow.xaml.cs
│       │
│       ├── Models/
│       │   ├── TeamMember.cs
│       │   ├── Sprint.cs
│       │   ├── WatcherSwap.cs
│       │   ├── SprintPR.cs
│       │   └── Enums/
│       │       ├── SprintStatus.cs
│       │       ├── PRStatus.cs
│       │       └── PRPriority.cs
│       │
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── Migrations/
│       │
│       ├── Services/
│       │   ├── ISprintService.cs
│       │   ├── SprintService.cs
│       │   ├── ITeamService.cs
│       │   ├── TeamService.cs
│       │   ├── IWatcherService.cs
│       │   ├── WatcherService.cs
│       │   ├── IPRService.cs
│       │   ├── PRService.cs
│       │   ├── INotificationService.cs
│       │   ├── NotificationService.cs
│       │   ├── ITrayService.cs
│       │   ├── TrayService.cs
│       │   └── IExportService.cs
│       │   └── ExportService.cs
│       │
│       ├── ViewModels/
│       │   ├── DashboardViewModel.cs
│       │   ├── SprintPRsViewModel.cs
│       │   ├── WatcherManagementViewModel.cs
│       │   ├── HistoryViewModel.cs
│       │   ├── SettingsViewModel.cs
│       │   └── Dialogs/
│       │       ├── NewSprintDialogViewModel.cs
│       │       ├── SwapWatcherDialogViewModel.cs
│       │       ├── AddPRDialogViewModel.cs
│       │       └── EditMemberDialogViewModel.cs
│       │
│       ├── Views/
│       │   ├── DashboardView.xaml
│       │   ├── SprintPRsView.xaml
│       │   ├── WatcherManagementView.xaml
│       │   ├── HistoryView.xaml
│       │   ├── SettingsView.xaml
│       │   └── Dialogs/
│       │       ├── NewSprintDialog.xaml
│       │       ├── SwapWatcherDialog.xaml
│       │       ├── AddPRDialog.xaml
│       │       └── EditMemberDialog.xaml
│       │
│       ├── Converters/
│       │   ├── StatusToColorConverter.cs
│       │   ├── PriorityToColorConverter.cs
│       │   └── BoolToVisibilityConverter.cs
│       │
│       └── Assets/
│           └── app-icon.ico
│
├── app.manifest
├── build.ps1
├── README.md
├── README_IT.txt
└── THIRD_PARTY_LICENSES.txt
```

---

## 8. Data Durability & Protection

Data must survive app restarts, system crashes, Windows updates, and unexpected power loss. This section covers the strategy to ensure zero data loss.

### 8.1 SQLite Durability Configuration

SQLite is already crash-safe when configured correctly. The app will use these settings:

```csharp
// Connection string
"Data Source=qasprinthub.db;Mode=ReadWriteCreate;"

// On DbContext configuration
connection.Execute("PRAGMA journal_mode=WAL;");      // Write-Ahead Logging — crash-safe writes
connection.Execute("PRAGMA synchronous=FULL;");       // Flush to disk on every commit — no data loss on power failure
connection.Execute("PRAGMA busy_timeout=5000;");      // Wait 5s if DB is locked
connection.Execute("PRAGMA foreign_keys=ON;");        // Enforce referential integrity
```

**What WAL + FULL sync gives us:**
- If the app crashes mid-write → SQLite rolls back the incomplete transaction, DB stays consistent
- If Windows crashes or power is lost → committed data is already on disk, WAL file replays on next open
- If Windows Update reboots the machine → same as above, no data lost

### 8.2 Database File Location

The database file should NOT live next to the .exe (which might be on a USB drive or temp location). Instead:

```
Primary DB location:  %APPDATA%\QASprintHub\qasprinthub.db
                      (e.g., C:\Users\John\AppData\Roaming\QASprintHub\qasprinthub.db)
```

**Why `%APPDATA%`:**
- Survives app deletion/reinstallation (exe can be replaced, data stays)
- Protected user directory — less likely to be accidentally deleted
- Included in standard Windows backup/restore
- Persists through Windows Updates
- Separate from the portable exe location

**Fallback**: If `%APPDATA%` is inaccessible (rare), fall back to the exe directory with a warning.

### 8.3 Automatic Backups

The app will create automatic rolling backups of the database:

**Backup Strategy:**
- **On every app startup**: Copy `qasprinthub.db` → `qasprinthub.backup.db` (same directory)
- **Daily backup**: Once per day, copy to `backups/qasprinthub_YYYY-MM-DD.db`
- **Retain last 30 daily backups**: Auto-delete older ones to save space
- **Before destructive operations**: Before deleting a team member or sprint, snapshot the DB

**Backup location:**
```
%APPDATA%\QASprintHub\
├── qasprinthub.db              ← live database
├── qasprinthub.backup.db       ← latest startup backup
└── backups/
    ├── qasprinthub_2026-02-08.db
    ├── qasprinthub_2026-02-07.db
    ├── qasprinthub_2026-02-06.db
    └── ... (last 30 days)
```

### 8.4 Manual Backup & Restore (Settings Page)

Users can trigger manual actions from the Settings page:

- **[Backup Now]** → Creates a timestamped copy in a user-chosen location
- **[Restore from Backup]** → File picker to select a `.db` file, validates it, then replaces the live DB
- **[Open Data Folder]** → Opens `%APPDATA%\QASprintHub\` in Explorer

### 8.5 Transaction Safety

All write operations are wrapped in explicit transactions:

```csharp
using var transaction = await db.Database.BeginTransactionAsync();
try
{
    // Multiple related writes (e.g., create sprint + assign watcher)
    await db.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    // Show user-friendly error, no partial data written
    throw;
}
```

**Rules:**
- No partial writes — either everything commits or nothing does
- Rotation reorder (moving 7 members) is a single transaction
- Sprint creation + watcher assignment is a single transaction
- Swap creation + sprint watcher update is a single transaction

### 8.6 Crash Recovery on Startup

When the app starts, it performs a health check:

1. **Check DB exists** → If missing, check for `qasprinthub.backup.db` and offer to restore
2. **Check DB integrity** → Run `PRAGMA integrity_check;` — if corrupt, offer to restore from latest backup
3. **Check WAL recovery** → SQLite handles this automatically, but log if WAL replay occurs
4. **Verify schema version** → If DB is from an older version, run EF migrations to update

### 8.7 Summary of Protection Layers

| Threat | Protection |
|--------|-----------|
| App crash mid-write | SQLite WAL journal auto-rollback |
| Power loss / hard reboot | `PRAGMA synchronous=FULL` ensures disk flush |
| Windows Update reboot | DB in `%APPDATA%` persists, WAL replays on next open |
| Accidental file deletion | Automatic daily backups (30-day retention) |
| DB corruption | Startup integrity check + restore from backup |
| Bad user action (delete wrong thing) | Pre-destructive-operation snapshot + manual restore |
| App reinstall / exe replaced | DB in `%APPDATA%`, separate from exe location |

---


## 9. Security, Compliance & Corporate Safety

This app runs on corporate machines managed by IT policies, antivirus software (e.g., CrowdStrike, Windows Defender, Symantec), and endpoint detection systems. It must be completely clean, transparent, and non-threatening.

### 9.1 Zero Network Activity

The app makes **absolutely no network calls** — no HTTP, no DNS, no sockets, nothing.

**Enforced at code level:**
```csharp
// No HttpClient, WebClient, HttpWebRequest, Socket, TcpClient anywhere in the codebase
// No NuGet packages that phone home (telemetry, analytics, crash reporting)
// No update checker — updates are manual (replace exe)
```

**Enforced at build level:**
```xml
<!-- In .csproj — explicitly disable any runtime networking -->
<EnableUnsafeBinaryFormatterSerialization>false</EnableUnsafeBinaryFormatterSerialization>
```

**What this means:**
- No telemetry or analytics of any kind
- No crash reporting to external services
- No auto-update mechanism (no phoning home)
- No license validation calls
- No cloud sync (v1 is purely local)
- Firewall can fully block this app — it won't care

### 9.2 Code Signing (Recommended)

Unsigned executables trigger Windows SmartScreen warnings and antivirus flags. For corporate deployment:

**Option A: Self-signed certificate (minimum)**
```powershell
# Create a self-signed code signing certificate
New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=QA Sprint Hub" `
  -CertStoreLocation Cert:\CurrentUser\My -NotAfter (Get-Date).AddYears(5)

# Sign the exe
Set-AuthenticodeSignature -FilePath "QASprintHub.exe" -Certificate $cert -TimestampServer "http://timestamp.digicert.com"
```

**Option B: Company certificate (ideal)**
- If Acumatica has an internal code signing certificate, use that
- IT can whitelist the certificate across all managed machines

**Option C: IT whitelisting**
- Provide IT with the exe hash (SHA-256) for whitelisting in endpoint protection
- Include app documentation for IT review

### 9.3 Windows SmartScreen & Antivirus Compatibility

**Why unsigned apps get flagged:**
- SmartScreen warns on "unknown publisher" executables
- AV heuristics flag self-extracting single-file .NET apps (they unpack to temp on first run)

**Mitigations built into the app:**

| Trigger | Mitigation |
|---------|-----------|
| Single-file exe unpacks to temp | Use `PublishSingleFile` with `IncludeNativeLibrariesForSelfExtract=true` to minimize temp extraction |
| No digital signature | Code sign with self-signed or company cert (see 9.2) |
| Writes to AppData | Normal app behavior — not suspicious |
| Runs on startup (optional) | Uses standard registry key `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` — not a hidden mechanism |
| System tray icon | Uses standard `NotifyIcon` API — well-known, not suspicious |
| Toast notifications | Uses official `Microsoft.Toolkit.Uwp.Notifications` — signed Microsoft library |
| SQLite native DLL | Well-known library, widely used, not flagged |

**Build configuration to minimize AV false positives:**
```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  <EnableCompressionInSingleFile>false</EnableCompressionInSingleFile>  <!-- Compressed exes look suspicious to AV -->
  <DebugType>embedded</DebugType>  <!-- Include PDB for transparency -->
  <AssemblyName>QASprintHub</AssemblyName>
  <Product>QA Sprint Hub</Product>
  <Company>Internal QA Tool</Company>
  <Description>Local QA sprint watcher rotation and PR tracking tool</Description>
  <Copyright>Internal Use Only</Copyright>
</PropertyGroup>
```

### 9.4 App Manifest & Permissions

The app requests **minimum permissions** — no admin rights, no elevated access.

**Application manifest (`app.manifest`):**
```xml
<?xml version="1.0" encoding="utf-8"?>
<assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0">
  <trustInfo xmlns="urn:schemas-microsoft-com:asm.v3">
    <security>
      <requestedPrivileges>
        <!-- Run as standard user — no admin elevation -->
        <requestedExecutionLevel level="asInvoker" uiAccess="false" />
      </requestedPrivileges>
    </security>
  </trustInfo>
  <compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1">
    <application>
      <!-- Windows 10/11 compatibility -->
      <supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
    </application>
  </compatibility>
</assembly>
```

**What the app accesses:**
- ✅ Read/write `%APPDATA%\QASprintHub\` (user data directory — normal)
- ✅ Read/write its own directory (for portable mode fallback)
- ✅ System tray (standard Windows API)
- ✅ Toast notifications (standard Windows API)
- ✅ Registry `HKCU` for optional startup entry (user-level, no admin needed)
- ❌ No admin rights required
- ❌ No access to other users' data
- ❌ No access to system directories
- ❌ No network access
- ❌ No camera, microphone, or peripheral access
- ❌ No clipboard monitoring or keylogging
- ❌ No injection into other processes
- ❌ No Windows service installation
- ❌ No driver installation
- ❌ No firewall or security setting modifications

### 9.5 Transparency for IT Review

Include a `README_IT.txt` alongside the exe for IT teams to review:

```
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
  QASprintHub.exe: [hash]
  e_sqlite3.dll:   [hash]

Dependencies (all NuGet packages, open source):
  - Microsoft.EntityFrameworkCore.Sqlite (Microsoft)
  - CommunityToolkit.Mvvm (Microsoft)
  - WPF-UI (open source, MIT license)
  - Hardcodet.NotifyIcon.Wpf (open source)
  - Microsoft.Toolkit.Uwp.Notifications (Microsoft)

This application:
  ✓ Makes no internet connections
  ✓ Collects no telemetry
  ✓ Stores no passwords or credentials
  ✓ Accesses only its own data directory
  ✓ Requires no admin privileges
  ✓ Can be fully blocked by firewall with no impact
```

### 9.6 NuGet Package Audit

Only use well-known, trusted packages. No obscure or low-download-count packages.

| Package | Publisher | Downloads | License | Network? |
|---------|-----------|-----------|---------|----------|
| Microsoft.EntityFrameworkCore.Sqlite | Microsoft | 100M+ | MIT | No |
| CommunityToolkit.Mvvm | Microsoft | 50M+ | MIT | No |
| WPF-UI (lepoco/wpfui) | Community | 5M+ | MIT | No |
| Hardcodet.NotifyIcon.Wpf | Community | 10M+ | CPOL | No |
| Microsoft.Toolkit.Uwp.Notifications | Microsoft | 20M+ | MIT | No |
| Microsoft.Extensions.DependencyInjection | Microsoft | 500M+ | MIT | No |

**Rules:**
- No packages with telemetry or phone-home behavior
- No packages that require API keys
- No packages with network dependencies
- Verify all packages are from trusted publishers before adding

### 9.7 Build Reproducibility

To allow IT to verify the build:

- Include full source code in a Git repository (internal)
- Provide a `build.ps1` script that produces the exact same output
- Document the exact .NET SDK version used
- Include a `THIRD_PARTY_LICENSES.txt` file listing all dependency licenses

```powershell
# build.ps1 — reproducible build script
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=false `
  -o ./publish

# Generate hash for IT verification
Get-FileHash ./publish/QASprintHub.exe -Algorithm SHA256 | Format-List
```

---

## 10. Sync-Ready Architecture (Future v2)

> **Note:** v1 has ZERO network activity. This section is for future planning only.

### Option A: Shared Folder Sync (Simplest)

- SQLite database file lives on a SharePoint/OneDrive synced folder
- App opens with read/write and uses WAL mode for better concurrency
- Risk: Concurrent writes can corrupt; mitigate with file locking and retry

### Option B: JSON File Sync (Conflict-Friendly)

- Each entity type exports to a JSON file in a shared folder
- App loads from JSON on startup, writes back on changes
- Conflict resolution: Last-write-wins with timestamp comparison
- Each record gets a GUID and LastModified timestamp

### Option C: Shared API (Most Robust)

- Lightweight ASP.NET Core API hosted internally
- All instances talk to a central SQL Server or PostgreSQL
- Real-time updates via SignalR

### Sync-Ready Design Decisions in v1

- All entities have `CreatedDate` fields (future merge support)
- Primary keys use `int` locally but models include a `Guid SyncId` property (unused in v1, ready for v2)
- Services use interfaces (`ISprintService`, etc.) — swap implementations when sync is added
- Database access goes through EF Core — easy to swap SQLite for SQL Server later

---

## 11. Publish & Deployment

### Build Command

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

### Output

- Single `.exe` file (~60–80 MB)
- Database file (`qasprinthub.db`) created on first run in the same directory
- Copy the folder anywhere — USB, shared drive, any Windows PC

### First Run

1. User launches `QASprintHub.exe`
2. App detects no database → runs EF migrations → creates `qasprinthub.db`
3. Welcome wizard:
   - Add 7 team members
   - Set rotation order
   - Create first sprint
4. Dashboard loads

---

## 12. Future Enhancements (v2+)

- **Data sync** across team (see Section 8)
- **Notifications** — Windows toast when sprint is ending, or PR is blocked too long
- **Bitbucket integration** — Optional API connection to auto-pull PR details
- **Email notifications** — Notify next watcher before sprint starts
- **Reports** — PR throughput per sprint, watcher load balance charts
- **Sprint templates** — Pre-fill recurring PR types
- **Dark/Light theme toggle**
- **Sprint retrospective notes** section
- **Export to Excel** — Full sprint report with PR details and watcher history

---

## 13. Acceptance Criteria (v1 MVP)

**Dashboard & Navigation**
- [ ] App launches showing current month's data by default
- [ ] Month navigation (◄ ►) filters all views by selected month
- [ ] [Today] button returns to current month
- [ ] Dashboard shows current watcher, sprint info, and PR stats on launch
- [ ] Sprints spanning two months appear in both months

**System Tray & Notifications**
- [ ] App minimizes to system tray on close (does not exit)
- [ ] System tray right-click menu: Open, Current Watcher info, Exit
- [ ] Toast notification on startup: shows current watcher name, duration, and next watcher
- [ ] Toast notification on watcher swap
- [ ] Toast notification 1 day before sprint ends
- [ ] Settings to toggle each notification type and tray behavior
- [ ] Optional: Launch on Windows startup setting

**Team & Rotation**
- [ ] Can add, edit, and deactivate team members
- [ ] Can set and modify watcher rotation order
- [ ] Can create new sprints with auto-assigned watcher and auto-calculated dates
- [ ] Sprint display name auto-generated from date range (e.g., "Feb 3 – Feb 14, 2026")

**Watcher Management**
- [ ] Can swap watchers with mandatory reason tracking
- [ ] Swap history is fully visible and auditable
- [ ] Swap does not affect rotation order
- [ ] Can assign a backup watcher for specific days, a full week, or full sprint
- [ ] Backup watcher displayed on Dashboard alongside primary watcher
- [ ] Can remove backup assignment
- [ ] Only one backup per sprint at a time

**PR Tracking**
- [ ] Can add PRs with title, optional link, author, priority, and notes
- [ ] Can change PR status (Pending, Merged, Blocked, Declined)
- [ ] Can open PR links in default browser

**History & Data**
- [ ] Can view sprint history with PR summaries
- [ ] Data persists in local SQLite database
- [ ] Database stored in %APPDATA% (survives app reinstall, Windows Updates)
- [ ] SQLite WAL mode + synchronous=FULL (crash-safe, power-loss safe)
- [ ] Automatic backup on every app startup
- [ ] Daily rolling backups with 30-day retention
- [ ] Startup integrity check with auto-restore offer if DB is corrupt
- [ ] Manual backup and restore from Settings page
- [ ] All writes wrapped in transactions (no partial data)
- [ ] App is portable — single-folder deployment, no installer

**Security & Corporate Safety**
- [ ] Zero network activity — no HTTP, DNS, socket, or any outbound connections
- [ ] No telemetry, analytics, or crash reporting
- [ ] Runs as standard user (asInvoker) — no admin rights required
- [ ] App manifest declares minimum permissions
- [ ] No compression in single-file publish (avoids AV heuristic flags)
- [ ] Embedded PDB for transparency
- [ ] Assembly metadata populated (Product, Company, Description, Copyright)
- [ ] README_IT.txt included with SHA-256 hashes and dependency list
- [ ] All NuGet packages from trusted publishers (Microsoft or well-known OSS)
- [ ] No packages with telemetry or network dependencies
- [ ] Reproducible build script (`build.ps1`) included
- [ ] THIRD_PARTY_LICENSES.txt included
- [ ] Optional: Code signed with self-signed or company certificate

**UI**
- [ ] Fluent/Windows 11 UI style via WPF UI library
