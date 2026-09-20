# Covfefe

A Windows system-tray utility that holds the PC — and optionally the monitors — awake while it runs, and lets the machine's normal power behaviour resume when it exits.

It exists for one situation: some **other** program needs to run uninterrupted, and that program never asks Windows to stay awake on its own behalf. Covfefe makes the request instead, so that program isn't cut short. Covfefe is not the thing that needs to keep running — it only needs to have made the request. Windows holds the request whether or not Covfefe is actively executing.

## Language

### Sleep modes

**Sleep Mode**:
The one piece of state that drives everything the user sees — tray icon, reminder, and what Covfefe asks of Windows. Exactly one is in effect at a time.
_Avoid_: power mode, state, status

**Normal**:
The sleep mode in which Covfefe asks Windows for nothing and the machine's own power settings apply unchanged.
_Avoid_: off, disabled, inactive

**Stay Awake**:
The sleep mode in which the machine is held awake, while the monitors are still free to switch off.
_Avoid_: prevent sleep, no sleep

**Keep Monitors On**:
The sleep mode in which both the machine and the monitors are held awake.
_Avoid_: display on, screen on, presentation mode

**Reminder**:
The periodic prompt asking whether a non-Normal sleep mode is still wanted. It exists to catch a machine left awake by accident.
_Avoid_: notification, alert, nag

**Start at Login**:
Whether Covfefe launches when the user signs in. It is a Windows integration point rather than an app setting: it lives in the registry, and Windows can switch it off without telling Covfefe.
_Avoid_: autostart, run at startup, launch on boot

### Power management

**Power request**:
The current way Covfefe asks Windows to stay awake: a handle held by the process, carrying a reason string that `powercfg /requests` displays next to Covfefe's name.
_Avoid_: execution state, sleep block, keep-awake request

**Legacy power API**:
The `SetThreadExecutionState` path — how Covfefe originally asked Windows to stay awake — retained as a switchable alternative to the power request. See ADR-0006.
_Avoid_: old API, Windows 7 API, fallback

### Distribution

**Installer track**:
The per-machine installer that requires administrator rights and installs into Program Files. See ADR-0002.
_Avoid_: the installer (unqualified), the main build

**Portable track**:
The self-contained single binary that runs with no installer and no administrator rights. See ADR-0002.
_Avoid_: standalone build, xcopy build

**Portable**:
Free of an *install step* — nothing to run, nothing to elevate. It does **not** mean free of a local footprint: the portable track still keeps its settings in `%LOCALAPPDATA%` like the installer track, and it is not designed to run from removable media leaving nothing behind. If that second meaning is ever wanted, it needs its own name.
_Avoid_: using "portable" for removable-media or zero-footprint behaviour

### Versions

**Covfefe v2**:
The .NET Framework application that the current .NET line replaced — a separate codebase, not an earlier state of the current one. It is frozen and no longer present in the repo, archived at tag `2.3.41-final`. See ADR-0007.
_Avoid_: legacy, the old project, the Framework version

Note that **legacy** is already spoken for: *Legacy power API* is the `SetThreadExecutionState` path, which the current application still contains and deliberately keeps. Using "legacy" for v2 as well would make the word mean both a thing that was removed and a thing that was retained.
