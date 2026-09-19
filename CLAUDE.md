# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Covfefe is a Windows system-tray utility that keeps the PC (and optionally the monitors) awake while it runs, by calling the Win32 `SetThreadExecutionState` API. Closing the app restores the machine's normal power plan. There is no window on normal startup — only a tray icon with a context menu; the `Form` is a hidden settings screen shown on demand.

## Two projects — know which one you're editing

The repo contains the **same application twice**. Both use the `Covfefe` root namespace, so file names collide across the two trees.

- **`src/Covfefe-core/` (`Covfefe-core.csproj`) — the ACTIVE project.** SDK-style, `net10.0-windows`, WinForms, nullable + implicit usings enabled. Source is organized into folders (`ApiImports/`, `Contracts/`, `Models/`). This is the migration target of the current `feature/dotnet-core` branch and where new work goes.
- **`src/Covfefe/` (`Covfefe.csproj`) — LEGACY.** Classic (non-SDK) `.NET Framework v4.8` project, files flat in the project root. Kept for the migration; prefer changing `Covfefe-core` unless you are deliberately touching the old build. **Changes made to one project are NOT shared with the other** — there is no shared assembly; the files are duplicated.

Two solution files exist and they are not equivalent:
- `CovfefeCore.slnx` — references only `Covfefe-core`. Use this for core work.
- `Covfefe.sln` — references the legacy `Covfefe`, the WiX installer, and `Covfefe-core`.

## Commands (PowerShell)

```powershell
# Build / run the active core project
dotnet build src/Covfefe-core/Covfefe-core.csproj
dotnet run   --project src/Covfefe-core/Covfefe-core.csproj
dotnet run   --project src/Covfefe-core/Covfefe-core.csproj -- --ShowSettings  # force the settings screen on launch

# Build the core-only solution
dotnet build CovfefeCore.slnx
```

There are **no tests** in this repo (no test project, no test runner) despite the interface-based design — do not assume a `dotnet test` target exists.

The WiX installer (`src/CovfefeSetup/CovfefeSetup.wixproj`) packages the **legacy** `src/Covfefe/bin/Release/Covfefe.exe`, not the core build, and builds x86. It requires the WiX toolset and MSBuild (not `dotnet build`).

## Architecture (core project)

Manual dependency injection in `Program.cs` wires a layered stack; there is no DI container. The entry point also enforces a **single instance** via a named `Mutex` and passes `--ShowSettings` through to the form.

```
Program.cs
  └─ TrayForm (the only UI; NotifyIcon + hidden settings Form)
       ├─ ISleepManagementFacade  → SleepManagementFacade
       │      └─ IPowerManagementEngine → Windows7PowerManagementEngine  → WindowsPowerManagementApi (P/Invoke)
       └─ ISettingsFacade         → SettingsFacade
```

Key patterns:
- **Facade → Engine layering isolates Win32 volatility.** `SleepManagementFacade` is the app-facing API; the `IPowerManagementEngine` implementation holds the actual P/Invoke calls. The engine is deliberately named for an OS generation (`Windows7PowerManagementEngine`) so a different OS strategy can be swapped behind the interface. Put new native power behavior in an engine, not in the facade or the form.
- **Interfaces in `Contracts/`** exist for this seam (and testability), even though no tests consume them yet.
- **P/Invoke lives only in `ApiImports/`** (`WindowsPowerManagementApi`, `WindowsUIApi`). Keep new `DllImport`s there.
- **Settings persistence is split** (`SettingsFacade`): most settings use the generated .NET `Settings.Default` (`Settings.settings`), but **Start-at-login is the Windows Run registry key** (`HKCU\...\CurrentVersion\Run`), read/written directly. `GetSettings()` swallows any load failure and resets to defaults rather than crashing.
- **Sleep modes** (`CovfefeSleepMode`: `Normal`, `StayAwake`, `MonitorsOn`) drive everything in `TrayForm.SetSleepMode` — tray icon, balloon tip, the "did you forget?" reminder timer, and the optional double-click toggle. `[Description]` attributes on the enum are the combo-box display text, surfaced via `ControlExtensions.LoadFromEnum`.

## Gotchas

- `.vscode/launch.json` points at a stale `bin/Debug/net7.0-windows/Covfefe-core.dll` path; the project now targets **net10.0-windows**. Update the path before using F5 debugging.
- `.vscode/tasks.json` builds `Covfefe.sln` (the full/legacy solution), which pulls in the .NET Framework project and the WiX installer — heavier and toolchain-dependent. For core-only work build `CovfefeCore.slnx` or the csproj directly.
- Several `ISleepManagementFacade` members (`DelaySleepOnBatteryStateChange`, `DelaySleepOnLidClose`) are declared but `throw new NotImplementedException()`.

## Agent skills

### Issue tracker

Gitea issues on the self-hosted instance at `http://vault.local:3000` (`jmarsch/Covfefe`), driven by the `tea` CLI — always with `--remote nas`, since `origin` is a GitHub mirror. See `docs/agents/issue-tracker.md`.

### Triage labels

The five canonical roles, label strings unchanged. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context: one `CONTEXT.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
