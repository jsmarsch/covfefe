# Keep hand-written `[DllImport]` for Win32 interop, with no `AllowUnsafeBlocks`

Win32 calls stay as hand-written `[DllImport]` declarations in `ApiImports/`. CsWin32, `[LibraryImport]`, and the pre-generated binding packages are all rejected. `<AllowUnsafeBlocks>` is not set anywhere.

## Context

The Power Request migration needs `PowerCreateRequest` / `PowerSetRequest` / `PowerClearRequest` and the `REASON_CONTEXT` struct, which contains a union. The modernization plan assumed `REASON_CONTEXT` was painful enough to hand-marshal that CsWin32 was worth adopting for it. A probe disproved that: the full binding — union modelled with `[FieldOffset(0)]`, blittable struct passed with `in`, handle released through a `SafeHandle` — is about twenty lines, compiles with the unsafe flag off, and runs against the live API.

The standing constraint is that `AllowUnsafeBlocks` must not be enabled project-wide on `Covfefe-core`, where it would apply to every hand-written file.

- **`[LibraryImport]`** hard-requires the flag even for a fully blittable signature like `SetThreadExecutionState` (`SYSLIB1062` + `CS0227`); the generator stamps `unsafe` on the containing type, so the error surfaces in our own file.
- **CsWin32** sets `AllowUnsafeBlocks=true` silently and unconditionally from its own `.props`, with nothing in the csproj recording it. Containing it in a separate interop project does work, but a flag-off consumer still cannot set the reason string through the generated friendly overload (`PWSTR` wants a `char*`), so it buys a second project plus a wrapper we would write anyway.
- **TerraFX.Interop.Windows** requires unsafe at the call site. **dotnet/pinvoke** is archived.
- **Vanara** (`Vanara.PInvoke.Kernel32`) is the viable fallback — zero unsafe, MIT, actively maintained, and it owns the reason-string buffer for you — at the cost of roughly 1.9 MB of assemblies in the portable build.

The usual argument for the source generators is Native AOT and trimming. That argument does not apply here: WinForms cannot be trimmed (`NETSDK1175`) and AOT requires trimming, so untrimmed single-file publish is the only mode available to Covfefe regardless.

## Consequences

`CLAUDE.md`'s rule that P/Invoke lives only in `ApiImports/` survives unchanged. No new project, no new NuGet dependency, no generator in the build. `ApiImports/WindowsPowerManagementApi.cs` gains the Power Request declarations and the `REASON_CONTEXT` types; `ApiImports/WindowsUIApi.cs` is deleted per ADR-0003.

We own signature correctness, which is exactly the failure mode that produced the `int`-instead-of-`nint` defect in `WindowsUIApi.SendMessage`. The Power Request signatures were cross-checked against the Win32 metadata projection that CsWin32 generates and match it.

This decision would be worth reopening if a future native API cannot be bound without unsafe code. The one candidate identified — `RegisterPowerSettingNotification` for lid-close handling — is moot, because that work has been dropped.

Research behind this decision: `docs/research/winapi-interop-options.md`.
