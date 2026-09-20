# Windows 11 and up only; Windows 10 support dropped

Covfefe targets Windows 11 and later. Windows 10 is out of support, and the modernization work — power requests with a named reason, `Application.SetColorMode` dark mode, system chrome, `PerMonitorV2` — is aimed squarely at Windows 11 conventions.

## Considered Options

Every Win32 and WinForms API this modernization depends on also exists on Windows 10, so supporting it was technically available. It was rejected because it would buy a second test pass and a second set of UI conventions for a platform that no longer receives updates.

## Consequences

`SupportedOSPlatformVersion` is set to a Windows 11 build, which turns the platform-compatibility analyzer on for anything newer. No Windows 10 test pass is required.

This removes the need for a *compatibility* fallback behind `IPowerManagementEngine`. It does not mean only one implementation exists: the `SetThreadExecutionState` engine is deliberately retained as a second, selectable implementation to keep that seam honest. That is a design choice about the seam, not a compatibility measure.
