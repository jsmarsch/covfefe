# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Covfefe is a Windows system-tray utility that keeps the PC (and optionally the monitors) awake while it runs, by calling the Win32 `SetThreadExecutionState` API. Closing the app restores the machine's normal power plan. There is no window on normal startup — only a tray icon with a context menu; the `Form` is a hidden settings screen shown on demand.

## The one project

**`src/Covfefe-core/` (`Covfefe-core.csproj`)** is the only application project. SDK-style, `net10.0-windows`, WinForms, nullable + implicit usings enabled. Source is organized into folders (`ApiImports/`, `Contracts/`, `Models/`). `CovfefeCore.slnx` is the only solution, referencing it and the test project.

The project keeps its `-core` suffix from the .NET Core migration that created it; there is no longer anything to distinguish it *from*. Renaming it is deliberately deferred — see ADR-0007.

**Covfefe v2** — the .NET Framework application this replaced — was removed from the repo along with its WiX v3 installer. It is archived at tag **`2.3.41-final`** on both remotes; `git checkout 2.3.41-final` brings it back in full. Do not reintroduce it. See ADR-0007.

## Commands (PowerShell)

```powershell
# Build / run the active core project
dotnet build src/Covfefe-core/Covfefe-core.csproj
dotnet run   --project src/Covfefe-core/Covfefe-core.csproj
dotnet run   --project src/Covfefe-core/Covfefe-core.csproj -- --ShowSettings  # force the settings screen on launch

# Build the core-only solution
dotnet build CovfefeCore.slnx

# Run the tests
dotnet test                                                              # works at the repo root
dotnet test --project tests/Covfefe-core.Tests/Covfefe-core.Tests.csproj  # just the test project
```

Tests are **TUnit**, in `tests/Covfefe-core.Tests/`. TUnit runs on Microsoft.Testing.Platform rather
than VSTest, so `global.json` at the repo root opts `dotnet test` into the newer command. The
consequence to remember: a project path must go through **`--project`** rather than a bare positional
argument, and runner flags go after a `--`. Read `docs/agents/testing.md` before writing tests; it
carries the convention for isolating tests that touch the real registry or filesystem.

There is currently **no installer**. The WiX v3 project that packaged Covfefe v2 was removed with it;
the replacement is specced in `modernizeplan.md` §10 as WiX v5/v6, per-machine into Program Files.
Two facts from the old installer carry forward: the `UpgradeCode` `0a24d660-a0cd-483f-9f71-6b33d7f0eed1`
must stay stable forever, and the old component GUID must **not** be reused, because the install path
changes and MSI component rules require a fresh one.

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
- **Interfaces in `Contracts/`** exist for this seam and for testability — `SleepManagementFacadeTests` drives `SleepManagementFacade` through a stand-in `IPowerManagementEngine`.
- **P/Invoke lives only in `ApiImports/`** (`WindowsPowerManagementApi`, `WindowsUIApi`). Keep new `DllImport`s there.
- **Settings persistence is split** (`SettingsFacade`): most settings use the generated .NET `Settings.Default` (`Settings.settings`), but **Start-at-login is the Windows Run registry key** (`HKCU\...\CurrentVersion\Run`), read/written directly. `GetSettings()` swallows any load failure and resets to defaults rather than crashing.
- **Sleep modes** (`CovfefeSleepMode`: `Normal`, `StayAwake`, `MonitorsOn`) drive everything in `TrayForm.SetSleepMode` — tray icon, balloon tip, the "did you forget?" reminder timer, and the optional double-click toggle. `[Description]` attributes on the enum are the combo-box display text, surfaced via `ControlExtensions.LoadFromEnum`.

## Gotchas

- Several `ISleepManagementFacade` members (`DelaySleepOnBatteryStateChange`, `DelaySleepOnLidClose`) are declared but `throw new NotImplementedException()`.

## Agent skills

### Issue tracker

Gitea issues on the self-hosted instance at `https://gitea.vault.local` (`jmarsch/Covfefe`), driven by the `tea` CLI — always with `--remote nas`, since `origin` is a GitHub mirror. See `docs/agents/issue-tracker.md`.

### Triage labels

The five canonical roles, label strings unchanged. See `docs/agents/triage-labels.md`.

### Testing

TUnit on Microsoft.Testing.Platform, in `tests/Covfefe-core.Tests/`. Tests that touch the real
registry or filesystem take a uniquely-named scratch location per test rather than running serially.
See `docs/agents/testing.md`.

### Domain docs

Single-context: one `CONTEXT.md` and `docs/adr/` at the repo root. See `docs/agents/domain.md`.
