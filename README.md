# Lab Control — Windows Lab Management System

A desktop application for managing a Windows computer lab, built with C# .NET 10 WinForms.

## Features

| Module | Description |
|--------|-------------|
| **Dashboard** | At-a-glance overview of station availability, active sessions, and daily usage |
| **Stations** | Add, edit, and delete lab workstations; track IP, location, OS, and status |
| **Users** | Register and manage students/users; filter active/inactive |
| **Sessions** | Start and end lab sessions; filter by date range or active-only |

## Data Storage

The application uses a local **SQLite** database stored at:

```
%APPDATA%\LabControl\labcontrol.db
```

No server or installation required — the database file is created automatically on first launch.

## Building

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Windows (required for WinForms)

### Build (Debug)

```powershell
dotnet build src/LabControl/LabControl.csproj
```

### Publish as a self-contained single `.exe` (Windows x64)

```powershell
dotnet publish src/LabControl/LabControl.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  --output publish/win-x64
```

The output file `publish/win-x64/LabControl.exe` is a fully self-contained executable — no .NET runtime installation is needed on the target machine.

## CI / CD

GitHub Actions automatically builds and publishes the `.exe` artifact on every push.  
The artifact **`LabControl-win-x64`** (containing `LabControl.exe`) is attached to each successful workflow run.

See [`.github/workflows/build.yml`](.github/workflows/build.yml) for details.

## Project Structure

```
control/
├── LabControl.slnx                  Solution file
├── src/
│   └── LabControl/
│       ├── LabControl.csproj        Project file (WinExe, net10.0-windows)
│       ├── Program.cs               Entry point
│       ├── Models/
│       │   ├── Station.cs           Workstation model
│       │   ├── LabUser.cs           Student/user model
│       │   └── Session.cs           Lab session model
│       ├── Data/
│       │   └── DatabaseContext.cs   SQLite schema + connection
│       ├── Services/
│       │   ├── StationService.cs    CRUD for stations
│       │   ├── UserService.cs       CRUD for users
│       │   └── SessionService.cs    Session management
│       └── Forms/
│           ├── MainForm.cs          Main tabbed window
│           ├── StationDialog.cs     Add/edit station dialog
│           ├── UserDialog.cs        Add/edit user dialog
│           └── SessionDialog.cs     Start session dialog
└── .github/
    └── workflows/
        └── build.yml                CI: build + publish .exe artifact
```
