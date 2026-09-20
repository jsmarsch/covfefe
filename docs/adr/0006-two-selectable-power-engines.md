# Both power management engines are retained and switchable at runtime

Covfefe ships two `IPowerManagementEngine` implementations — the original `Windows7PowerManagementEngine` (`SetThreadExecutionState`) and the new power-request engine — and a settings checkbox switches between them while the app is running. The power-request engine is the default.

## Context

ADR-0001 makes Covfefe Windows 11 only, which removes any *compatibility* reason to keep the older API. It is kept anyway, deliberately. `IPowerManagementEngine` was built as a seam for exactly this substitution, and a seam with a single implementation is an untested claim. Keeping both, and switching between them in a running app, is what shows the seam holds.

A future reader will otherwise see `SetThreadExecutionState` code in a Windows-11-only app and delete it as dead. It is not dead; it is the control case.

## Considered Options

Selecting the engine only at startup — read from settings, restart to change — would have proven the interface for less work. Live switching was chosen because it exercises the part a startup selection hides: releasing the outgoing engine's hold while the process keeps running.

## Consequences

- **`IPowerManagementEngine` gains `IDisposable`.** Live switching is what forces it; process exit does not, since Windows drops handles and power requests at process death anyway. `Windows7PowerManagementEngine` has nothing to release and implements it as a no-op, while the power-request engine closes its handle. That asymmetry is the seam doing its job.
- **`Program.cs` constructs both engines at startup** and hands both to `SleepManagementService`, which owns the active one, performs the switch, and disposes both on shutdown. No factory: two fixed implementations do not need one.
- **The switch sets the incoming engine before disposing the outgoing one**, so a failure leaves the previous engine still holding and the revert is just putting the setting back. Failure is close to hypothetical — these APIs fail only on malformed arguments — but the ordering costs nothing and removes the half-switched state by construction.
- `TrayForm` triggers the switch after `FormClosing` saves settings.
- The checkbox reads "legacy power API" and sits under an Advanced heading, because no user has a reason to want it.
- The class keeps the name `Windows7PowerManagementEngine`. `PowerCreateRequest` also dates from Windows 7, so the name marks which engine came first rather than which platform it needs.
