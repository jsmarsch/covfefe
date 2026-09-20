# Replace the custom window chrome with system chrome and built-in dark mode

The settings form's hand-drawn title bar and hard-coded colors are replaced by the standard Windows title bar plus `Application.SetColorMode`, which is non-experimental in .NET 10.

## Context

The custom chrome predates any framework dark-mode support, and it is the reason the form carries hand-coded colors, a drag handler built on `ReleaseCapture` / `SendMessage`, and a custom close button. None of it adapts to the system theme, and the drag handler's P/Invoke signatures use `int` where they should use `nint` — a latent x64 truncation bug.

## Consequences

`ApiImports/WindowsUIApi.cs` is deleted, taking that defect with it. `CloseButton_Click` disappears, and it is currently the only code path that saves settings — so settings persistence must move in the same pass, or settings stop persisting entirely. A user-facing theme setting (Light / Dark / Follow System, defaulting to Follow System) is added; the tray icon follows the taskbar theme rather than that setting.
