# Covfefe-core Modernization Plan

**Scope:** `src/Covfefe-core/` only. The legacy .NET Framework project (`src/Covfefe/`) is out of scope and will receive no further work.

**Target platform:** Windows 11 and up. Windows 10 is not supported (decided — see §11).

**Review basis:** Full read of the `Covfefe-core` tree plus a clean build (10 warnings, 0 errors) against SDK 10.0.204 / Microsoft.WindowsDesktop.App.Ref 10.0.8. API availability claims in §6 were verified by probe-compiling against that SDK.

**Status:** Plan only. No code changes have been made.

---

## Constraints

Standing requirements that shape the rest of this document. These are not derivable from the code — they come from how Covfefe is actually deployed.

1. **There must be a no-admin path to running Covfefe.** It is used in environments where many users have no local admin authority.

   **Revised.** This was originally written as "installation must not require admin," which forced *every* channel to be admin-free and drove the `%APPDATA%` install location (§9). The requirement is actually weaker: some route must exist for a user without admin. A **portable standalone binary** satisfies it, which frees the traditional installer to be a normal per-machine install requiring elevation. See §10.

   **Why self-service matters:** Covfefe is sanctioned by IT in those environments, but IT deployment turnaround takes weeks. The point is to let users install it themselves. **Central IT deployment (Intune / SCCM / GPO) is explicitly out of scope** and is not a distribution channel.
2. **Settings must be editable at runtime, from inside the app, without elevation.**
3. **Windows 11 and up only.** Windows 10 is out of support and needs no accommodation. This was reconsidered and confirmed — see §11 for the cost analysis behind the decision.
4. **The legacy `src/Covfefe/` project is frozen.** No further work there.

## Goals

- **Modernize** to current .NET 10 and Windows 11 conventions (§1–§6).
- **Stop tripping antivirus and SmartScreen warnings** on install (§9).
- **Follow current install/deployment best practice**, and distribute via **winget** and **Chocolatey** in addition to a direct installer download (§10).

## Decisions taken so far

- The custom window chrome will be removed and replaced with system chrome plus built-in dark mode (§6).
- A user-facing theme setting — Light / Dark / Follow System, defaulting to Follow System — will be added (§6).
- **Two distribution tracks**: a per-machine installer that requires admin and installs to Program Files, plus a portable standalone binary that requires none. This supersedes the earlier single per-user-install decision (§10).
- **The two tracks are equals.** Each serves its own audience. Neither one is the main channel, and neither is a fallback for the other (§10).
- **No central IT deployment.** Users install Covfefe themselves (Constraint 1).
- **Windows 10 support is dropped.** Windows 11 only; no Windows 10 test pass is needed (§11).
- The portable build is **self-contained** — framework-dependent is not viable, because installing the .NET runtime itself needs admin (§10).
- `%APPDATA%` is abandoned as an install location either way (§9).

---

## 1. Sleep API — there is a better one

**Replace `SetThreadExecutionState` with the Power Request API** (`PowerCreateRequest` / `PowerSetRequest` / `PowerClearRequest`, kernel32).

Three concrete wins:

- **A named reason string.** You pass a `REASON_CONTEXT` containing text, and `powercfg /requests` then shows `[PROCESS] Covfefe.exe — "Stay Awake requested by user"`. Today Covfefe appears in that list with *no* explanation. On Windows 11 this is the convention for "I am deliberately holding this machine awake" — it is how a user diagnoses why their PC won't sleep.
- **Process-scoped, not thread-scoped.** `SetThreadExecutionState` binds to the calling thread and dies with it. Covfefe currently calls it from the UI thread, which lives as long as the app, so it works — but it is a latent trap the moment any of this moves to a task or background thread. Power requests are handles owned by the process.
- **`PowerRequestExecutionRequired` has no `SetThreadExecutionState` equivalent.** On Modern Standby (S0ix) machines — most current Windows 11 laptops — that is the request that keeps the process *running* rather than suspended.

### Mapping

| `CovfefeSleepMode` | Power request |
|---|---|
| `StayAwake` | `PowerRequestSystemRequired` |
| `MonitorsOn` | `PowerRequestDisplayRequired` + `PowerRequestSystemRequired` |
| `Normal` | `PowerClearRequest` + close handle |

### Risk to verify on real hardware

On Modern Standby systems, neither API blocks *user-initiated* sleep (lid close, power button) — only idle timeouts. If a laptop behaves differently from a desktop today, that is why, and switching APIs will not change it. Test on a Modern Standby machine before assuming a regression.

### Architectural fit

The existing design already anticipated this. `Windows7PowerManagementEngine` sitting behind `IPowerManagementEngine` is exactly the intended seam — this becomes a new engine class (`WindowsPowerRequestEngine` or similar) and nothing above it changes.

### Unimplemented facade members

`ISleepManagementFacade.DelaySleepOnBatteryStateChange` and `DelaySleepOnLidClose` currently `throw new NotImplementedException()`. The real implementation is `RegisterPowerSettingNotification` + `WM_POWERBROADCAST` handled in `TrayForm.WndProc`, using `GUID_LIDSWITCH_STATE_CHANGE` / `GUID_ACDC_POWER_SOURCE`.

**Decision needed:** build them or delete them from the interface. As written they are a trap for a future caller.

### Considered and rejected

WinRT's `Windows.System.Display.DisplayRequest` is display-only and buys nothing over power requests for a desktop app.

---

## 2. Runtime configuration

### Current state

`SettingsFacade` uses `ApplicationSettingsBase` (`Settings.settings`), which writes to:

```
%LOCALAPPDATA%\Covfefe\Covfefe.exe_Url_yco3y14cdui42yafisiz340riraem11k\1.0.0.0\user.config
```

That folder was confirmed present on the dev machine, still at **1.0.0.0** while the csproj declares `2.0.0`. The hashed, version-keyed path is the entire reason `UpgradeNeeded` / `Upgrade()` / `Reload()` exists in `SettingsFacade`.

### Recommendation

**Plain `System.Text.Json` to `%LOCALAPPDATA%\Covfefe\settings.json`, with a source-generated `JsonSerializerContext`.**

- Stable path — **the whole upgrade mechanism and the `UpgradeNeeded` setting delete themselves**
- User-writable, no elevation required (meets the no-admin constraint, same as today)
- Hand-editable, backup-able, syncable
- Source-generated serializer: no reflection, trim-friendly

Use `%APPDATA%` instead of `%LOCALAPPDATA%` only if settings should roam with the user profile.

### New field

`CovfefeSettings` gains a `Theme` property (`CovfefeTheme` enum) for the work in §6. It persists in `settings.json` like any other field. Because it must be read *before* the first window is created, it is the one setting whose load order matters — see §6.

### On `Microsoft.Extensions.Configuration`

It is the modern *reading* stack (`AddJsonFile`, `IOptionsMonitor`, `reloadOnChange`), but it has **no write story** — you would still serialize by hand. For a seven-field settings object it is ceremony without payoff. Add `IOptionsMonitor` only if live-reload-when-the-file-changes-underneath is specifically wanted.

### Keep in the registry

`StartAtLogin` stays in the registry. It is not app config — it is a Windows integration point. See §3.

---

## 3. Windows 11 startup convention — a real bug

Windows 11's **Settings → Startup apps** lets the user disable a Run-key entry. When they do, **Windows does not delete the Run value.** It writes a disabled record to:

```
HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run
```

`SettingsFacade.GetSettings()` reads only the Run key, so the settings checkbox reports "Start at login: on" while Windows has it switched off. This needs a `StartupApproved` check.

**Caveat:** the exact byte encoding of that binary value is undocumented (the first byte flags enabled/disabled). Confirm empirically before coding against it.

### Separate defect

`runKey.SetValue(_appName, _exeName)` (`SettingsFacade.cs:68`) writes the path **unquoted**. The value should be quoted — a Windows username containing a space (`C:\Users\Jane Smith\...`) produces an ambiguous Run entry.

*(Correction to an earlier draft of this plan: it claimed the installer places the exe under Program Files. It does not — it installs to `%APPDATA%\Covfefe`. The quoting recommendation stands, but for the reason above. The install location itself is a significant problem in its own right — see §9.)*

---

## 4. Bugs found

1. **`Assembly.GetEntryAssembly().Location` returns an empty string under single-file publish** — `SettingsFacade.cs:20`. Start-at-login would silently write an empty Run value. Use `Environment.ProcessPath`, which also correctly yields the `.exe` rather than the `.dll`.

2. **Unguarded registry null** — `SettingsFacade.cs:64`. `OpenSubKey(..., writable: true)` returns null if the key is missing, causing an NRE on save. Use `CreateSubKey`. Four of the ten build warnings point at exactly this.

3. **Event handler leak in the reminder** — `TrayForm.cs:162-168`. Every tick re-subscribes `BalloonTipClicked` and `BalloonTipClosed`; the only unsubscribe lives in `BalloonTipClosed`, which on Windows 10/11 does *not* reliably fire (see §6). Handlers accumulate, so after N reminders a single click runs `SetSleepMode(Normal)` N times. Subscribe once in the constructor.

4. **Settings mutate in memory even when never saved.** The data bindings write straight into `_settings`; only `CloseButton_Click` calls `SaveSettings`. Dismiss the form any other way (Esc, Alt+F4) and the session runs on uncommitted settings that vanish on restart. Bind to a copy, or save on change.

   **This one is now forced, not optional.** Removing the custom close button (§6) deletes `CloseButton_Click`, which is the only code path that currently saves. The save must move to `FormClosing` — or to save-on-change — as part of that work, or settings will stop persisting entirely.

5. **Power state never explicitly released on exit.** `Application.Exit()` does not clear the execution state. Benign today because Windows drops it at process death, but once a *power request handle* is held it should be cleared explicitly on `FormClosing`. `SingleInstanceMutex` is also never disposed.

6. **The TODO at `Program.cs:16` is already satisfied.** An unprefixed mutex name lives in the `Local\` namespace, which is already per-session — multi-session already works. Only a `Global\` prefix would break it. Delete the comment.

### Do not "fix" this

`ReminderTimeoutMinutes * 60000` (`TrayForm.cs:66`, `TrayForm.cs:149`) is safe: the `NumericUpDown` maximum of 35790 is what keeps that product inside `Int32`. That cap is deliberate.

---

## 5. .NET 10 and language modernization

- **`[LibraryImport]` instead of `[DllImport]`** — compile-time marshalling stubs, no runtime IL generation. Requires `<AllowUnsafeBlocks>` and `partial` methods. Mechanical for the current signatures.

- **Or CsWin32** (`Microsoft.Windows.CsWin32`): a `NativeMethods.txt` listing `PowerCreateRequest` and friends, and `ApiImports/` goes away — correct signatures and a proper `SafeHandle` for the request handle, generated from Win32 metadata. This is the better choice *specifically* for the power-request work, because `REASON_CONTEXT` is a union struct that is genuinely annoying to hand-marshal.

  **Decision needed:** this cuts against the CLAUDE.md rule that P/Invoke lives only in `ApiImports/`. Decide whether generated code satisfies that rule.

- **`WindowsUIApi.SendMessage` has wrong types** — `int` for `wParam`, `lParam`, and the return value. Should be `nint`. Works today only because `WM_NCLBUTTONDOWN`'s values are small; it is a truncation bug on x64 waiting for a different message.

  **Do not bother fixing this — delete it instead.** `ReleaseCapture` and `SendMessage` exist solely to drag the custom title bar, and §6 removes that. `ApiImports/WindowsUIApi.cs` goes away entirely, taking the defect with it.

- **Target `net10.0-windows10.0.26100.0`** with `<SupportedOSPlatformVersion>10.0.22621.0</SupportedOSPlatformVersion>`, since the app is Windows 11 only. Enables the platform-compatibility analyzer and provides direct WinRT projections with no extra packages.

- **Clear the 10 nullable warnings**, then turn on `<TreatWarningsAsErrors>`. Every current warning points at a real null path:
  - `CS8602` × 4 — `SettingsFacade.cs:20,42,68,72`
  - `CS8602` × 1, `CS8618` × 1 — `Program.cs:17,7`
  - `CS8622` × 4 — `TrayForm.cs:158,159,164,165` (event handler signatures need `object?`)

- **`ControlExtensions.LoadFromEnum<T>`** — constrain `where TEnumType : struct, Enum` and use `Enum.GetValues<TEnumType>()` instead of `Enum.GetValues(Type).Cast<>()`. Also, `.OrderBy(v => v.Value)` sorts by *description text*, so the combo reads "Keep Monitors On / Normal / Stay Awake!" — alphabetical, almost certainly not the intent. Declaration order is the likely intent.

- **`CovfefeSettings` has no `INotifyPropertyChanged`**, so bound controls never observe programmatic changes. This is the root of bug #4. `CommunityToolkit.Mvvm`'s `[ObservableProperty]` generator provides it for free.

- **Dead usings** — `using System.CodeDom;` in `TrayForm.cs` and `ControlExtensions.cs`. With `ImplicitUsings` enabled, most other explicit usings are redundant too.

- **Argument parsing** — `args?.FirstOrDefault() == "--ShowSettings"` is case-sensitive and only checks position 0.

---

## 6. Windows 11 UI conventions

### Dark mode is now first-class and non-experimental

`Application.SetColorMode(...)` was verified to compile clean on this SDK with zero warnings. It was `[Experimental("WFO5001")]` in .NET 9; that attribute is gone in .NET 10.

Verified API surface on this SDK:

- `SystemColorMode` has exactly three members: **`Classic`**, **`System`**, **`Dark`**
- `Application.ColorMode` (getter) and `Application.IsDarkModeEnabled` both exist

Note the naming: **`Classic` is the light theme.** There is no `SystemColorMode.Light`.

`TrayForm.Designer.cs` currently hard-codes a dark palette (RGB 40/48/227 literals on every control). Adopting the built-in color mode:

- lets all of those literals be deleted
- fixes the `ContextMenuStrip`, which today renders light against the dark form
- makes the app respect the user's choice instead of forcing dark

### Decided: replace custom chrome with system chrome

The custom chrome existed only to produce a dark look on Windows 7. That constraint is gone, so it goes.

**Removal scope:**

| Remove | Location |
|---|---|
| `titlePanel`, `closeButton`, `settingsLabel` | `TrayForm.Designer.cs` |
| `FormBorderStyle.None` → `FixedDialog` (or `FixedSingle`), `MaximizeBox = false` | `TrayForm.Designer.cs` |
| All hard-coded `BackColor` / `ForeColor` literals | `TrayForm.Designer.cs` |
| `SettingsLabel_MouseDown` (the drag hack) | `TrayForm.cs:135-144` |
| `CloseButton_Click` | `TrayForm.cs:146-152` |
| `ApiImports/WindowsUIApi.cs` — entire file | see §5 |

**Critical consequence — do not miss this.** `Program.cs` calls `Application.Run(mainForm)`, so `TrayForm` is the application's main form. Today the custom X button only calls `Hide()`. A real system title bar's X raises `FormClosing` with `CloseReason.UserClosing`, which would **close the main form and terminate the app** — the exact opposite of the intended tray behavior.

`FormClosing` must therefore:

1. save settings (this is where bug #4's fix lands),
2. set `e.Cancel = true` and `Hide()` when `CloseReason` is `UserClosing`,
3. allow the close through for `ApplicationExitCall` / `WindowsShutDown` so the Exit menu item still works.

Also reconsider `ShowIcon = false` (a real title bar can now show the coffee cup) and `ShowInTaskbar = false` (arguably should be `true` while the settings window is visible, so users can alt-tab back to it). Both are judgement calls, neither is load-bearing.

### Decided: user-facing theme setting

Add a `CovfefeTheme` enum following the existing `CovfefeSleepMode` pattern — `[Description]` attributes feed the combo box through the existing `ControlExtensions.LoadFromEnum`, so this needs a new combo box on the settings form and nothing else new.

| `CovfefeTheme` | `[Description]` | Maps to |
|---|---|---|
| `FollowSystem` **(default)** | "Follow system setting" | `SystemColorMode.System` |
| `Light` | "Light" | `SystemColorMode.Classic` |
| `Dark` | "Dark" | `SystemColorMode.Dark` |

**Load-order constraint — this changes `Program.cs` wiring.** `SetColorMode` must be called next to `ApplicationConfiguration.Initialize()`, before any window exists. But the theme lives in settings, and today `TrayForm` calls `GetSettings()` from its *own constructor*. So `SettingsFacade` must be constructed and read in `Main`, and the resulting `CovfefeSettings` passed into `TrayForm` rather than fetched by it:

```
var settingsFacade = new SettingsFacade();
var settings = settingsFacade.GetSettings();      // now happens in Main
ApplicationConfiguration.Initialize();
Application.SetColorMode(MapTheme(settings.Theme));  // before any window
var mainForm = new TrayForm(sleepFacade, settingsFacade, settings);
```

This is a small, contained change, but it does alter `TrayForm`'s constructor signature.

**Live switching — needs visual verification.** I confirmed `SetColorMode` does *not* throw when called after a window already exists, and `Application.ColorMode` updates correctly. What I could not verify programmatically is whether an already-realized window fully repaints — managed colors resolve to system color references and should follow, but the **non-client area (the title bar) is set via `DwmSetWindowAttribute` at handle creation** and is unlikely to re-theme in place.

Plan for the robust approach: on theme change, dispose and recreate the settings form. If that proves visually clean, good; if not, fall back to applying the new theme on next launch and telling the user so. **Verify this by eye before committing to a design.**

### Tray icon: follow the taskbar, not the app setting

Windows 11 themes apps and the taskbar independently — a user can run light apps with a dark taskbar, or the reverse. So when the icon-variant work below is done, the tray icon must follow the **system/taskbar** theme, *not* the new `CovfefeTheme` setting. These are deliberately two different signals.

### DPI: `SystemAware` → `PerMonitorV2`

Verified to build. For an app whose entire subject is monitors this is the right default — today the settings form is blurry on a mixed-DPI setup.

**This is not a flag flip.** The form uses hard-coded pixel positions, so `PerMonitorV2` will expose that layout and it needs a `TableLayoutPanel` / anchoring pass.

Fold this into the chrome-removal work rather than doing it separately — that pass rewrites the layout regardless, and doing both at once means laying the form out once instead of twice. See §7.

### Balloon tips are not the Windows 11 convention

`NotifyIcon.ShowBalloonTip` is translated by the shell into a toast — which is *why* `BalloonTipClicked` and `BalloonTipClosed` are unreliable (bug #3) — and it is silently suppressed under Focus Assist / Do Not Disturb. The "did you forget?" reminder, the one notification that actually matters, may therefore never appear.

The proper path is `Microsoft.Windows.AppNotifications` (Windows App SDK) or `ToastNotificationManager`. Both require a registered AUMID: a Start Menu shortcut for unpackaged apps, or MSIX packaging.

**Assumption to confirm:** that means installer work, and the WiX project currently packages the legacy exe anyway. Staying on balloon tips and simply fixing the handler leak is a legitimate call if the goal is to stay installer-light.

### What removing the custom chrome buys back

The hand-rolled title bar cost rounded corners, snap layouts, the system drop shadow, the Alt+Space menu, and correct DPI/theme behavior on the non-client area. Using system chrome restores all of it for free — no `DwmSetWindowAttribute` corner-preference workaround needed.

### Tray icon does not adapt to taskbar theme

One icon serves both light and dark taskbars. Windows 11 users can run a light taskbar. Watch `WM_SETTINGCHANGE` / `Personalize\SystemUsesLightTheme` and swap icon variants — driven by the system theme, not the app's `CovfefeTheme` setting (see above).

### Version metadata

The csproj declares `Copyright 2022` and `Version 2.0.0` while the deployed config folder is `1.0.0.0`. Reconcile before shipping.

---

## 7. Suggested sequence

| # | Work | Rationale |
|---|---|---|
| 1 | **Bug fixes + nullable warnings** (§4) | Small, independent, no design decisions required. Defer bug #4 to step 4, where it is forced anyway |
| 2 | **JSON settings** (§2) + **`StartupApproved` fix** (§3) | Deletes the upgrade machinery; self-contained behind `ISettingsFacade`. Add the `Theme` field here so step 4 has somewhere to persist it |
| 3 | **Power Request engine** (§1) | New `IPowerManagementEngine` implementation; nothing above it changes. Make the CsWin32-vs-`LibraryImport` call here |
| 4 | **Chrome removal + dark mode + theme setting + `PerMonitorV2`** (§6) | **Do these as one pass — see below** |
| 5 | **Installer: move off `%APPDATA%`, fill in metadata** (§9) | Free, removes most of the antivirus false-positive signal. Independent of everything above — can be done at any point |
| 6 | **Code signing** (§9) | The only real fix for the SmartScreen prompt. Gated on buying a certificate, so it is last by dependency, not by priority |

### Why step 4 is one pass, not four

These were originally sequenced separately. They should not be. Every one of them rewrites `TrayForm.Designer.cs` layout:

- removing `titlePanel` (docked top, 36px) reflows every control on the form
- deleting the hard-coded colors touches nearly every control declaration
- adding the theme combo box adds a row
- `PerMonitorV2` requires replacing the fixed-pixel positions with a `TableLayoutPanel` / anchoring scheme

Doing them separately means laying out the form three or four times. Do the layout rework **once**, with the final control set and no hard-coded colors, against `PerMonitorV2`.

This makes step 4 the large one — but it is large because it absorbed what was previously step 5, not because it grew.

---

## 8. Open decisions

### Resolved

- **Custom chrome** — removing it, replacing with system chrome plus built-in dark mode (§6).
- **Theme control** — user-selectable Light / Dark / Follow System, defaulting to Follow System (§6).

### Still open

1. **CsWin32 vs. hand-written `LibraryImport`** — this changes what `ApiImports/` means as an architectural rule, and CLAUDE.md would need updating either way.
2. **Are real toast notifications worth installer changes?** — the alternative is keeping balloon tips and fixing only the handler leak.

Lower stakes:

3. Implement or delete `DelaySleepOnBatteryStateChange` / `DelaySleepOnLidClose` (§1).
4. `ShowIcon` / `ShowInTaskbar` on the settings form once it has a real title bar (§6).

### From §9 (antivirus false positives)

5. ~~Install location~~ — **resolved**: per-machine Program Files for the installer, plus a portable binary for the no-admin case (§10).
6. **Code signing** — whether to buy a certificate, and OV vs. EV vs. Azure Trusted Signing. This one gates the most other work: winget submission quality, publisher-based allowlisting, and the SmartScreen prompt all depend on it. The portable track raises its importance rather than lowering it.

### From §10 (distribution)

7. ~~The Chocolatey / per-user conflict~~ — **dissolved** by the two-track split. Chocolatey is admin-oriented and now maps cleanly onto the admin installer track. The dual-purpose MSI is no longer needed.
8. **WiX v5 vs. v6** for the migration — either works; it is a prerequisite for all release automation.
9. **Portable settings location** — beside the exe with a marker-file probe, or always `%LOCALAPPDATA%`. Affects §2, so decide before implementing it.
10. **winget portable vs. zip** for the no-admin entry — `portable` creates a PATH shim but no Start Menu shortcut, which is awkward for a GUI tray app (§10).

### From §11 (earlier Windows versions)

11. ~~Support Windows 10, or stay Windows 11 only?~~ — **resolved: Windows 11 only.** Windows 10 support is dropped (§11).

### Needs hands-on verification, not a decision

- Whether a **live theme switch** repaints an existing window's title bar, or whether the form must be recreated (§6).
- The byte encoding of the `StartupApproved\Run` binary value (§3).
- Modern Standby behavior on laptop hardware (§1).
- Which warning users actually saw — SmartScreen reputation vs. a real Defender detection (§9). The fixes overlap heavily, but the priority differs.
- Whether the no-admin environments **enforce AppLocker / WDAC** (§9). If they do, the portable build will not run there until IT adds an allow rule.

---

## 9. Antivirus / SmartScreen false positives

Users of the legacy installer frequently saw a Windows Defender warning. The cause is very likely **not** the P/Invoke bindings. Reading `CovfefeSetup.wxs` turned up a much stronger explanation.

### First, distinguish the two warnings

They have different causes and different fixes, and "Windows Defender" is used for both.

| | SmartScreen (reputation) | Defender Antivirus (detection) |
|---|---|---|
| Message | "Windows protected your PC — prevented an unrecognized app from starting" | "Threat found", file quarantined |
| Names a threat? | No | Yes — e.g. `Trojan:Win32/Wacatac.B!ml` |
| Cause | Unsigned / low-prevalence binary carrying Mark-of-the-Web | Heuristic or ML model match |
| Fix | Code signing + reputation | Fix the suspicious traits, submit a false-positive report |

The `!ml` suffix indicates a machine-learning verdict — those are the ones that fire on small, unsigned, low-prevalence utilities.

### The primary suspect: the app installs into `%APPDATA%`

`CovfefeSetup.wxs` nests the install directory under `AppDataFolder`, so the payload lands at `%APPDATA%\Covfefe\Covfefe.exe`. Combined with the rest of the shape:

1. an **unsigned** executable
2. **installed into `%APPDATA%`** rather than a programs directory
3. a **`HKCU\...\Run` persistence entry pointing at that AppData executable**
4. the installer **auto-launching the payload** on completion (`Return="asyncNoWait"`)
5. **low prevalence** and a `Manufacturer` of `jsm` with no reputation history

That is close to a textbook description of a dropper. Legitimate software installs to Program Files; malware installs to AppData precisely because it needs no admin rights. "Executable running from `%AppData%`" is a heavily weighted feature in behavioral and ML models, and item 3 is MITRE ATT&CK T1547.001 (Registry Run Keys / Startup Folder).

Individually, none of these is damning. Stacked, they are the whole pattern.

### What is almost certainly *not* the cause

**The P/Invoke bindings.** `SetThreadExecutionState` is a benign, ubiquitous API, and managed `DllImport` declarations are not what these models key on. The signals that matter are things like `VirtualAllocEx`, `WriteProcessMemory`, and `CreateRemoteThread` — none of which Covfefe uses. Migrating to the Power Request API (§1) neither helps nor hurts here.

**The Run key on its own.** It is a real contributor, but a secondary one, and it is unremarkable when it points at a signed executable in a normal install location. Note the current design is already correct on the more important point: the *installer* does not write the Run key — the app writes it when the user opts in. Keep it that way. An installer that establishes persistence is a far worse signal than an app that does so on user request.

### Fixes, in order of effect

**1. Move off `%APPDATA%` — to Program Files, per-machine.** Highest value, zero cost.

> **Superseded by §10.** An earlier draft recommended `%LOCALAPPDATA%\Programs\Covfefe`, because the no-admin requirement was then believed to apply to every channel. With distribution split into two tracks (§10), the installer is free to be a normal per-machine install to Program Files — the best possible reputation position — while the portable binary covers the no-admin case. `%LOCALAPPDATA%\Programs` remains the correct answer *if* a per-user installer is ever wanted; it is simply no longer needed.

Program Files is the strongest option available: it is not user-writable, which is precisely why AV heuristics treat code running from it as normal and code running from `%AppData%` as suspect.

This means dropping the per-user MSI configuration — `InstallPrivileges="limited"` and the blank `ALLUSERS` both go, along with the `AppDataFolder` nesting. The `.wxs` gets shorter.

### Portable app vs. per-user install — pick one

The original intent was a portable-app model. What exists is neither model cleanly, and that mismatch is itself part of the problem:

| | True portable app | Installed app | **Covfefe today** |
|---|---|---|---|
| Installer | none — unzip and run | MSI | **MSI** |
| Location | anywhere, incl. USB | Program Files | **`%APPDATA%`** |
| Settings | next to the exe | `%LOCALAPPDATA%` | **`%LOCALAPPDATA%`** |
| Registry | none | `HKCU` + Run key | **`HKCU` + Run key** |
| Add/Remove entry | no | yes | **yes** |

Covfefe has every signal of an installed app — MSI, registry, Run-key persistence, ARP entry — while living where portable payloads and malware droppers live. That combination is what reads as suspicious.

**The resolution is to stop straddling: ship both models cleanly rather than one blurred hybrid.** §10 does exactly that — a real installed app in Program Files, and a real portable binary with settings beside the exe. Each column above becomes internally consistent instead of mixed.

**2. Code sign the EXE *and* the MSI.** The durable fix, and the only real fix for SmartScreen.

- Signing only one leaves the other flagged. A signed MSI also makes the UAC prompt show a real publisher instead of "Unknown Publisher."
- Since June 2023, publicly trusted code-signing keys must live on FIPS 140-2 Level 2 hardware — a token or cloud HSM. A plain `.pfx` from a CA is no longer available.
- **EV** certificates get immediate SmartScreen reputation. **OV** must accrue it, but reputation attaches to the *certificate*, so once established it carries across releases instead of resetting per build.
- **Azure Trusted Signing** is the cheap modern path for a small project — it manages the HSM and is priced for individuals/small orgs rather than at traditional CA rates. Confirm current pricing and identity-validation eligibility directly, as both have changed repeatedly.

**3. Fill in the version metadata.** `Covfefe-core.csproj` sets `Title`, `Description`, and `Copyright` but has no `<Company>`; the MSI declares `Manufacturer="jsm"`. Populate `Company`, `Product`, `FileVersion`, and `AssemblyVersion`, and make them match the MSI's `Manufacturer` exactly. Sparse or inconsistent version resources are a mild ML feature, and consistency across releases is what builds reputation. Adding `ARPPRODUCTICON` / `ARPURLINFOABOUT` to the MSI helps it look like real software in Add/Remove Programs.

**4. Think twice about single-file publish.** .NET single-file bundles have historically drawn elevated false-positive rates because the bundle format resembles a self-extracting dropper. This is worth weighing against §4's bug #1 — the `Environment.ProcessPath` fix there is correct regardless of publish mode, so adopting it does not commit you to single-file.

**5. If there is a real detection, submit it.** Microsoft's Defender false-positive submission portal is free and typically resolves in days. Sign the binaries *first*, so the resolution attaches to your certificate and covers future releases rather than one file hash.

### Caveat: the portable track does not defeat AppLocker

The environments that motivated the no-admin requirement are also the environments most likely to run application allowlisting, so be precise about what portable distribution does and does not buy.

- **Helps with:** users who simply lack admin rights on an otherwise unmanaged machine.
- **Does *not* help with:** AppLocker / WDAC / Software Restriction Policies. The default AppLocker rule set permits Program Files and Windows and denies everything else — precisely because user-writable paths are user-writable. A portable exe run from the desktop, Downloads, `%LOCALAPPDATA%`, or a USB stick is blocked on exactly the same terms as `%APPDATA%` was.

If a target environment enforces allowlisting, **no** no-admin option works — not portable, not a per-user installer — until IT adds an allow rule. Confirm whether the environments in question actually enforce it, because the answer decides whether the portable track works there at all.

If they do enforce it, code signing makes the fix cheap. A signed binary can be allowed **by publisher**: IT adds one rule, once, and it covers every future release. Because Covfefe is already sanctioned, that is a small, one-time request, unlike the weeks-per-release deployment turnaround the portable track exists to avoid (Constraint 1). An unsigned binary can only be allowed by file hash, so IT would have to repeat the request for every build.

### The option that resolves everything at once: MSIX

If a code-signing certificate is purchased anyway (fix 2), MSIX becomes viable and addresses the entire constraint set simultaneously:

- **Installs per-user without admin** — satisfies the original requirement
- **Signed by definition** — no SmartScreen prompt, and the signing cert can be whitelisted by an admin where allowlisting is enforced
- **Declares startup intent in the manifest** via `StartupTask`, replacing the Run key entirely — this also resolves the `StartupApproved` bug in §3, since the user's toggle in Settings → Startup apps becomes the real state rather than something to reverse-engineer
- **Provides a registered AUMID for free** — which is the blocker on real toast notifications in §6. That open question closes as a side effect.

**Costs and cautions:**

- Requires the certificate, so it is gated on the same decision as fix 2.
- Some enterprise environments restrict MSIX sideloading — the same environments that would restrict everything else.
- Packaged apps have redirected storage; settings written to `%LOCALAPPDATA%` may be virtualized into a package-private location. The `Windows.Storage.ApplicationData.Current.LocalFolder` API is the packaged-app path. This interacts with §2 and needs verifying before committing.
- It is a larger change than relocating the MSI's install directory.

**Suggested approach: do fix 1 now** (a one-line change to the `.wxs` directory structure, immediate benefit, no dependencies), and treat MSIX as the possible end state once the certificate question is settled.

> **Revised by §10.** The decision to support Chocolatey weakens the "resolves everything at once" framing above. Chocolatey consumes MSIX poorly, so an MSIX-only future would mean dropping Chocolatey or shipping two artifacts. The MSI stays primary — see §10.

### Realistic expectation

Fixes 1, 3, and 4 are free and remove most of the behavioral signal. **Only code signing eliminates the SmartScreen "unrecognized app" prompt** — an unsigned installer downloaded from the web will trigger it indefinitely, because with no certificate, reputation accrues per file hash and resets with every build.

---

## 10. Distribution: two tracks

**Decision: split distribution into an admin installer and a portable binary.** This supersedes the earlier attempt to make every channel admin-free.

### Why this is the right shape

It is the dominant pattern for Windows utilities. 7-Zip, Notepad++, ShareX, KeePass, Everything, and HandBrake all ship an installer *and* a portable build; Sysinternals ships portable only. Users expect it and know what it means.

More importantly, **it dissolves the Chocolatey conflict from the previous draft.** Chocolatey installs to `C:\ProgramData\chocolatey` and requires elevation — it is an admin-oriented tool by design. Once the installer track is openly admin-required, Chocolatey fits it naturally, and the dual-purpose MSI (`ALLUSERS` / `MSIINSTALLPERUSER`) with its fiddly two-mode testing matrix is no longer needed. That was the most complex item in the plan and it is now simply deleted.

### The matrix

| Channel | Artifact | Admin | Install location | Audience |
|---|---|---|---|---|
| Direct download — installer | per-machine MSI | **Yes** | Program Files | Users with admin rights |
| Direct download — portable | self-contained single-file `.exe` (or `.zip`) | No | anywhere | Users without admin rights |
| winget | both, in one manifest | Either | per scope | Both |
| Chocolatey | `.nupkg` wrapping the MSI | **Yes** | Program Files | Users with admin rights who use Chocolatey |

**The two tracks are equals.** Each serves a different audience, and neither is the main channel. Every channel is self-service; central IT deployment is out of scope (Constraint 1).

### Portable must be self-contained — measured

Framework-dependent builds require the .NET 10 Desktop Runtime, and **installing that runtime requires admin** — useless for precisely the audience portable exists to serve. So portable has to be self-contained. Measured on this repo, SDK 10.0.204, `win-x64`:

| Build | Size |
|---|---|
| Self-contained, single-file, compressed | **49.2 MB** |
| Self-contained, single-file, uncompressed | 110.8 MB |
| Framework-dependent, single-file | 0.39 MB |

49 MB is chunky for a tray utility but entirely normal for a portable .NET app. Use `EnableCompressionInSingleFile`. Trimming is **not** an option — WinForms does not support it.

> **Build gotcha, verified here.** `dotnet publish --self-contained false` was silently ignored on SDK 10.0.204 and produced a 110 MB self-contained bundle. `-p:SelfContained=false` produced the correct 0.39 MB output. Use the `-p:` form in CI and assert on the resulting artifact size, or a "framework-dependent" build will ship as a 110 MB self-contained one without failing.

### Portable needs a settings-location mode

§2 puts settings in `%LOCALAPPDATA%\Covfefe\settings.json`. A portable build should keep settings **beside the executable** so the whole thing travels on a USB stick.

Standard solution, used by KeePass and Everything among others: probe for a marker or config file next to the exe at startup; if present, run in portable mode and use that path; otherwise use `%LOCALAPPDATA%`. This is a small, contained addition to `SettingsFacade` and should be designed in when §2 is implemented rather than retrofitted.

Start-at-login still works in portable mode — the `HKCU` Run key needs no admin — but it writes outside the app directory, which is a deviation from strict portability. Offer it; consider defaulting it off in portable mode.

### Portable does not escape code signing — it intensifies it

An unsigned standalone `.exe` downloaded from the web, carrying Mark-of-the-Web, is the single most SmartScreen-prone artifact there is. And single-file self-contained bundles have historically drawn elevated false-positive rates because the self-extracting shape resembles a dropper (§9, fix 4).

So the portable track trades *installer* behavioral signals for a *worse* reputation profile on the binary itself. Net effect on §9: moving the installer to per-machine Program Files removes most behavioral signal from that track, while the portable track makes code signing more important, not less. **Signing remains the gating decision.**

### winget: one manifest, both installers

A winget manifest can declare **multiple installers** and select between them, so a single `jsm.Covfefe` package serves both audiences:

- **`InstallerType: msi`** with `Scope: machine` — the elevated path, `winget install jsm.Covfefe --scope machine`
- **`InstallerType: portable`** with `Scope: user` — no admin. winget drops the exe in a links directory, creates a shim on `PATH`, and tracks it for clean uninstall.

`InstallerType: zip` is also available, with `NestedInstallerType` and `NestedInstallerFiles` pointing at the real payload inside the archive — useful if the portable build ever becomes more than one file.

**Caveat on `portable`:** winget's portable install creates a PATH shim but **no Start Menu shortcut**. For a GUI tray app with no command-line use, a shim is near-worthless and the user is left with no obvious way to launch it. Verify this is acceptable, or prefer `zip` and document the launch step.

Use `wingetcreate` for manifest generation and `wingetcreate update --submit` in CI for subsequent releases.

### Chocolatey: the nupkg *is* the package

To be precise about the question: a `.nupkg` is not an alternative to the installer — it is Chocolatey's container format, and it wraps whichever payload you choose.

- **Wrapping the MSI** (`Install-ChocolateyPackage`, URL + `-Checksum sha256`) is the recommended path. Matches Chocolatey's machine-wide, elevated model.
- **Wrapping a zip** (`Install-ChocolateyZipPackage`) is fully supported if you would rather ship the portable payload.
- Chocolatey **auto-shims every `.exe`** it finds in the package's `tools` directory. For a GUI app, add a `Covfefe.exe.gui` marker file so the shim does not block waiting for exit, or a `Covfefe.exe.ignore` to suppress shimming entirely.
- Community repo submissions pass automated validation, verification, and human moderation — budget latency on the first submission.

### Does WiX survive?

Yes, and **it gets simpler.** The current `.wxs` is convoluted largely *because* of the per-user requirement — `InstallPrivileges="limited"`, blank `ALLUSERS`, the `AppDataFolder` nesting. Going per-machine to Program Files deletes all of that.

Still migrate **WiX v3 to v5 or v6** (see below); that is about tooling, not install model.

Alternative if you want less ceremony: **Inno Setup** is popular and easy to script. winget supports `InstallerType: inno` with silent flags, and Chocolatey handles `.exe` installers fine. It produces an `.exe` rather than an `.msi`, and it needs the Inno Setup compiler as a separate tool in CI.

An earlier draft argued MSI was required so IT could deploy it centrally. With central deployment out of scope, that argument is gone, and this is now a preference. **Recommendation: stay with WiX.** The `.wxs` already exists and is about to get smaller, and WiX v5/v6 builds with `dotnet build`, so CI needs no extra tool. Inno is a legitimate choice if you prefer its scripting.

### Still a blocker: the installer project is WiX v3

`CovfefeSetup.wixproj` uses the `schemas.microsoft.com/wix/2006/wi` namespace, `Wix.targets` v3.x, and a hard dependency on a separately installed WiX toolset plus full MSBuild — it cannot be built with `dotnet build`. That blocks the release automation everything here depends on.

Modern WiX is an SDK-style MSBuild project: builds with `dotnet build`, restores the toolset as a package reference so CI needs no bespoke install step, uses the `http://wixtoolset.org/schemas/v4/wxs` namespace, and folds `<Product>` into `<Package>`. The `.wxs` is small and about to get smaller, so this is contained — but it is a prerequisite.

### Single source of truth for version

Today `Covfefe-core.csproj` declares `2.0.0` while the deployed config folder reads `1.0.0.0`. Four more consumers are arriving: the MSI, the portable exe, the winget manifest, and the Chocolatey nuspec.

Put `<Version>` in a root `Directory.Build.props` and derive everything from it. The `.wxs` already does the right thing with `Version="!(bind.fileVersion.Covfefe.exe)"` — keep that pattern. `UpgradeCode` (`0a24d660-a0cd-483f-9f71-6b33d7f0eed1`) must stay stable forever; it already is.

### Release pipeline

GitHub Releases is the artifact host — both package managers need a stable public HTTPS URL with a stable SHA256. On tag push:

```
build (dotnet build)
  -> publish portable   (self-contained + compression; mind the gotcha above)
  -> build MSI          (dotnet build, WiX v5/v6)
  -> sign BOTH the portable exe and the MSI   <- §9
  -> create GitHub Release, attach both
  -> compute SHA256 for each
  -> wingetcreate update --submit    (both installer entries)
  -> choco pack && choco push
```

### Ordering

| Step | Depends on |
|---|---|
| 1. Migrate WiX v3 to v5/v6, simplify to per-machine Program Files | nothing — do first |
| 2. `Directory.Build.props` version single-sourcing | nothing |
| 3. Portable settings-location mode in `SettingsFacade` | §2 |
| 4. GitHub Release + CI build of both artifacts | 1, 2 |
| 5. Code signing in CI (§9) | certificate purchased |
| 6. winget manifest, both installer entries | 4, 5 |
| 7. Chocolatey package | 4, 5 |

### Reference implementation worth a look

**PowerToys Awake** is a Microsoft-shipped module doing essentially this job, open source and in C#. Useful as a reference for the power-request work in §1 and for how a mainstream utility handles the tray and startup conventions — not a reason to stop, since Covfefe is standalone and does things Awake does not.

---

## 11. Supporting earlier Windows versions

**Question:** the engine design (`IPowerManagementEngine`) exists so a different Win32 approach can be swapped in for a different OS generation. How much extra effort would it take to keep supporting Windows versions before 11?

**Status: decided — Windows 11 only. Windows 10 support is dropped.** The analysis below is kept as the record of why.

### The .NET runtime sets the lowest supported Windows version, not the engine

.NET 10's official support list (`dotnet/core`, `release-notes/10.0/supported-os.md`, checked 2026-09-19):

| Windows | .NET 10 support |
|---|---|
| Windows 11 — 24H2, 25H2, 26H1 (plus 23H2 Enterprise) | Supported |
| Windows 10 1607, 1809, 21H2 — **Enterprise LTSC and IoT editions only** | Supported |
| Windows 10 22H2 Home / Pro | **Not listed** |
| Windows 7, 8, 8.1 | **Not supported** |

Two consequences:

**Windows 7, 8, and 8.1 are out of reach at any effort.** The runtime does not support them, so the app never gets as far as the engine. The only route back to those versions is the .NET Framework 4.8 project, which is frozen (Constraint 4). The engine design cannot help here.

**Windows 10 is reachable, with one caveat.** Most Windows 10 machines run 22H2 Home or Pro, and that edition is not on .NET 10's list, because Windows 10 reached end of support on 2025-10-14. It will almost certainly *run*, since the same runtime supports older 1607 builds. But "not supported" means Microsoft will not fix a runtime problem that only shows up there. Windows 10 consumer Extended Security Updates (ESU) end 2026-10-13; commercial ESU runs to October 2028. The environments behind Constraint 1 are the kind that may still be running Windows 10 under commercial ESU, so it is worth finding out.

### Every API in this plan already exists on Windows 10

| Feature | Minimum Windows | Effect on Windows 10 |
|---|---|---|
| Power Request API (§1) | Windows 7 | none |
| `PowerRequestExecutionRequired` (§1) | Windows 8 | none |
| `StartupApproved` registry key (§3) | Windows 8 | none — same code |
| Toast notifications (§6) | Windows 8 | none |
| `PerMonitorV2` DPI (§6) | Windows 10 1703 | none on supported builds |
| System light/dark app setting (§6) | Windows 10 | followed normally |
| WinForms dark mode (§6) | — | **verify** — title-bar darkening differs between Windows 10 builds; at worst it should fall back to light |
| Rounded window corners (§6) | Windows 11 | Windows 10 shows square corners; no code needed, because system chrome handles it |
| winget (§10) | Windows 10 1809, via App Installer | usually present, not guaranteed on older builds |

**So no second engine is needed.** The single modern engine (§1) works on every Windows version .NET 10 runs on, which leaves the engine design nothing to switch between. It was the right design in the .NET Framework era, when the app ran on Windows 7 through 10; moving to .NET 10 has already narrowed that range. Keep the interface anyway — it is still the right place to swap in a test double.

### What Windows 10 support would cost

1. **One project setting.** Lower `<SupportedOSPlatformVersion>` from `10.0.22621.0` (Windows 11 22H2, §5) to `10.0.17763.0` (Windows 10 1809). The build then warns about any call to an API that needs a newer version. Based on the table above, expect few or none; any that appear get an `OperatingSystem.IsWindowsVersionAtLeast(...)` check.
2. **Checking dark mode on Windows 10** — the one "verify" row above.
3. **A Windows 10 test machine or VM, and a test pass on every release.** This is the real cost: small each time, but permanent.
4. **Accepting the support gap** for Windows 10 22H2 Home/Pro described above.

Items 1 and 2 are small, one-time jobs. Item 3 is the ongoing cost to weigh.

**Decision:** Windows 10 support is dropped. `<SupportedOSPlatformVersion>` stays at `10.0.22621.0` (§5), and neither the Windows 10 test pass nor the Windows 10 dark-mode check is needed. The engine design stays as a place to swap in a test double, not for OS versions.
