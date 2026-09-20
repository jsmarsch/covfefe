# Win32 interop options for Covfefe-core

**What this is:** research input for open decision §8.1 of `modernizeplan.md` ("CsWin32 vs. hand-written `LibraryImport`"). It is not the decision. When the call is made it belongs in `docs/adr/` as an ADR; `docs/research/` is a new folder created for this note, since the repo had no convention for research that feeds a decision (`docs/` previously held only `docs/agents/`).

**Constraint driving the analysis:** the user does not want `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` on the main application project (`src/Covfefe-core/Covfefe-core.csproj`), where it would apply to every hand-written file. Containing it elsewhere is acceptable. Not needing it at all is better.

**Verification basis:** SDK **10.0.204**, `Microsoft.WindowsDesktop.App` **10.0.8**, on Windows 11 Pro 26200. Every claim below labelled *(empirical)* is output from a throwaway probe project built and run on this machine. Claims labelled *(documented)* are cited to a first-party source. Nothing was added to the repo's solutions and no application code was changed.

---

## Bottom line

**Yes — this can be done with no unsafe code anywhere, including the `REASON_CONTEXT` union and the `SafeHandle`.** The flag stays off in every project. No containment scheme is needed.

**Recommendation: keep hand-written `[DllImport]` in `ApiImports/`, and write the four Power Request functions by hand.**

The plan's premise for reaching for CsWin32 — that `REASON_CONTEXT` is "genuinely annoying to hand-marshal" — does not survive contact with the compiler. The full struct including the union is 20 lines of ordinary C#, is blittable, and marshals with `in`. A probe that creates a power request with a reason string, sets both `PowerRequestSystemRequired` and `PowerRequestDisplayRequired`, clears them, and releases the handle through a `SafeHandle` **compiles and runs with `AllowUnsafeBlocks` off** *(empirical, Probe B)*. It also publishes self-contained single-file with zero warnings.

Everything else costs more:

- **`[LibraryImport]` hard-requires the flag** even for `SetThreadExecutionState`, whose signature is a blittable enum in and a blittable enum out. The generator stamps `unsafe` on the containing type unconditionally. `error SYSLIB1062` *(empirical, Probe A)*.
- **CsWin32 hard-requires the flag**, and quietly turns it on for you — its `.props` sets `AllowUnsafeBlocks=true` with no condition. Force it back to `false` and you get `warning PInvoke002` plus ten `CS0227` errors *(empirical, Probe C2)*.
- **CsWin32 can be contained** in a separate `Covfefe.Interop` project with the flag on, consumed by a flag-off app project. That works and runs *(empirical, Probe F)*. But you must write a hand-written wrapper anyway: even with `"public": true`, a flag-off consumer **cannot set the reason string**, because `PWSTR` is constructed from a `char*` *(empirical, Probe G)*. So CsWin32 buys you a second project and a wrapper, to replace ~40 lines you would otherwise write once and never touch.
- **The finest containment granularity is the project.** `AllowUnsafeBlocks` maps to the `csc -unsafe` switch, which is per-compilation. A `Directory.Build.props` in a subfolder is never even read — MSBuild walks *up* from the project file, never down *(documented + empirical, Probe D)*.

**Fallback if hand-writing is unwanted: `Vanara.PInvoke.Kernel32`.** It compiles and runs with `AllowUnsafeBlocks=false`, ships a `SafePowerRequestObject`, and has a `REASON_CONTEXT(string)` constructor *(empirical, Probe V)*. MIT, actively maintained. Cost: ~1.9 MB of extra assemblies in the self-contained portable build, for four functions.

**The WinRT route is dead, for a better reason than the plan gives.** §1 rejects `Windows.System.Display.DisplayRequest` as "display-only". True, but the fatal part is different: *"When a display request is activated, the device's display remains on while the app is visible. When the user moves the app out of the foreground, the system deactivates the app's display requests"* ([DisplayRequest docs]). Covfefe normally has **no visible window at all** — it is a tray icon. `DisplayRequest` would be deactivated the entire time the app is doing its job. See §6 below.

**Bonus finding that changes the framing of a different decision:** Windows Forms cannot be trimmed. `error NETSDK1175: Windows Forms is not supported or recommended with trimming enabled` *(empirical, Probe WF)*. Native AOT requires trimming ([Native AOT limitations]), so **NativeAOT and trimming are off the table for Covfefe regardless of interop choice**. The usual headline argument for `[LibraryImport]` and CsWin32 — AOT and trim friendliness — therefore buys Covfefe nothing. Only untrimmed single-file publish is in play, and plain `DllImport` is fine there.

### Assumptions and risks called out

- **Unverified, and deliberately left that way: whether the kernel copies the reason string.** `powercfg /requests` requires an elevated prompt and was not run, so no probe could read back the reason text. The `REASON_CONTEXT` docs say only that the functions *"read from the strings and do not write to them"* — they do not say the buffer may be freed after the call returns. **Resolved by ADR-0005:** the buffers are allocated once into `static readonly IntPtr` fields and never freed, which is correct whether the kernel copies or retains. Probe B (frees immediately) and Probe S (runtime marshaller frees at call exit) are both exposed to this and neither is a safe template as written.
- **Risk if CsWin32 is chosen anyway:** it is at **0.3.333**, still pre-1.0 after five years. The `AllowUnsafeBlocks=true` in its `.props` is unconditional and silent — a reader of `Covfefe-core.csproj` would see no unsafe flag and would be wrong about what the compiler is doing.
- The plan's Modern Standby verification item (§1) is untouched by this decision. It is a behavior question about the API, not about how the API is called.

---

## Comparison

| Option | `AllowUnsafeBlocks` required? | Where the flag lands | Covers `REASON_CONTEXT` + reason string? | `SafeHandle`? | Build-time deps | Verdict |
|---|---|---|---|---|---|---|
| **Hand-written `[DllImport]`** | **No** | nowhere | Yes — union modelled with `[FieldOffset]`, passed by `in` | Yes, yours | none | **Recommended** |
| `[LibraryImport]` | **Yes, unconditionally** | the project holding the `partial` methods | Yes | Yes | in-box generator | Reject — flag cost, no payoff |
| **CsWin32** | **Yes** (package sets it for you) | whichever project has the `PackageReference` | Yes, generated | Yes (`SafeFileHandle`) | NuGet + Roslyn generator | Over-engineered for 4 functions |
| CsWin32 in a separate interop project | Yes, but **contained** | `Covfefe.Interop.csproj` only | Yes, behind a mandatory hand-written wrapper | Yes | as above | Works; costs a project + a wrapper |
| **Vanara.PInvoke.Kernel32** | **No** | nowhere | Yes — `REASON_CONTEXT(string)` ctor | Yes (`SafePowerRequestObject`) | NuGet, ~1.9 MB | **Viable fallback** |
| TerraFX.Interop.Windows | **Yes**, at the call site | the calling project | Pointer-only; no friendly overload | No | NuGet | Reject |
| `PInvoke.*` (dotnet/pinvoke) | n/a | n/a | No power-request coverage worth relying on | — | NuGet | Reject — repo **archived** |
| WinRT `DisplayRequest` | **No** | nowhere | n/a — different API | n/a | none (TFM only) | Reject — see §6 |
| C++/CLI / native shim | n/a | separate native project | Yes | Yes | C++ toolchain | Reject — disproportionate |
| `Microsoft.Windows.SDK.Contracts` | No | nowhere | n/a | n/a | NuGet | Unnecessary on this TFM |

---

## 1. Status quo — hand-written `[DllImport]`

**What it is.** What `ApiImports/WindowsPowerManagementApi.cs` does today: an `extern` method with `[DllImport]`, marshalling performed by an IL stub the runtime generates at first call ([P/Invoke source generation]).

**What the code actually looks like for these APIs.** Verbatim from the probe that built and ran:

```csharp
[StructLayout(LayoutKind.Sequential)]
internal struct REASON_CONTEXT
{
    public uint Version;
    public uint Flags;
    public REASON_CONTEXT_UNION Reason;
}

[StructLayout(LayoutKind.Explicit)]
internal struct REASON_CONTEXT_UNION
{
    [FieldOffset(0)] public REASON_CONTEXT_DETAILED Detailed;
    [FieldOffset(0)] public IntPtr SimpleReasonString;   // LPWSTR
}

[StructLayout(LayoutKind.Sequential)]
internal struct REASON_CONTEXT_DETAILED
{
    public IntPtr LocalizedReasonModule;   // HMODULE
    public uint   LocalizedReasonId;
    public uint   ReasonStringCount;
    public IntPtr ReasonStrings;           // LPWSTR* — the only pointer, and Covfefe never uses this branch
}

internal sealed class PowerRequestHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public PowerRequestHandle() : base(ownsHandle: true) { }
    protected override bool ReleaseHandle() => Native.CloseHandle(handle);
}

[DllImport("kernel32.dll", SetLastError = true)]
internal static extern PowerRequestHandle PowerCreateRequest(in REASON_CONTEXT Context);

[DllImport("kernel32.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
internal static extern bool PowerSetRequest(PowerRequestHandle PowerRequest, POWER_REQUEST_TYPE RequestType);
```

That is the whole thing. `Marshal.SizeOf<REASON_CONTEXT>()` returns **32**, matching the native layout on x64 *(empirical)*. `SafeHandleZeroOrMinusOneIsInvalid` is the correct base: `PowerCreateRequest` returns `INVALID_HANDLE_VALUE` on failure ([PowerCreateRequest docs]).

There is a second shape that skips manual allocation entirely — declare the simple-string branch as `[MarshalAs(UnmanagedType.LPWStr)] public string SimpleReasonString` with two `nint` padding fields covering the rest of the union. `Marshal.SizeOf` is still 32 and it runs *(empirical, Probe S)*. It is less faithful to the header, and it puts the string buffer's lifetime entirely in the marshaller's hands — see the open question in §9. Prefer the union version with a buffer the wrapper owns.

**Pros**
- No unsafe flag anywhere, in any project.
- No new NuGet dependency, no new project, no generator in the build.
- `CLAUDE.md`'s "P/Invoke lives only in `ApiImports/`" rule survives untouched. So does the §1 plan for `Windows7PowerManagementEngine` → a new engine behind `IPowerManagementEngine`.
- Debuggable: the source is the source. (The runtime IL stub itself is not steppable, but there is nothing between your code and the call.)
- Works in untrimmed single-file publish, which is the only publish mode WinForms allows.

**Cons**
- You own the signature correctness. `WindowsUIApi.SendMessage` (§5 of the plan) is exactly the failure mode — `int` where `nint` belongs. Mitigation: the Power Request signatures here were cross-checked against the Win32 metadata projection that CsWin32 generates (§3) and match.
- No compile-time marshalling stub; irrelevant at Covfefe's call rate (a handful of calls per session).

**Unsafe verdict: not required, anywhere. Best available outcome.**

---

## 2. `[LibraryImport]` source generator

**What it is.** `[LibraryImport]` on a `static partial` method; a Roslyn generator emits the marshalling stub at compile time instead of the runtime emitting IL ([P/Invoke source generation]).

**Unsafe verdict: hard requirement. No workaround, no generator option, no friendly overload.**

The docs state it flatly under "Differences from `DllImport`": *"The project must be marked unsafe using AllowUnsafeBlocks."* ([P/Invoke source generation]). This is not conditional on the signature.

Confirmed on the simplest possible case — `SetThreadExecutionState`, a blittable enum in and a blittable enum out, no strings, no arrays *(empirical, Probe A)*:

```
CSC : error SYSLIB1062: LibraryImportAttribute requires unsafe code. Project must be updated with
'<AllowUnsafeBlocks>true</AllowUnsafeBlocks>'.
(https://learn.microsoft.com/dotnet/fundamentals/syslib-diagnostics/syslib1062)
Probe.cs(5,31): error CS0227: Unsafe code may only appear if compiling with /unsafe
generated\...\LibraryImports.g.cs(7,10): error CS0227: Unsafe code may only appear if compiling with /unsafe
```

The reason is visible in the generated stub with the flag on — the generator stamps `unsafe` on the **containing type**, whether or not the stub body uses a pointer. For `SetThreadExecutionState` it does not use one:

```csharp
internal static unsafe partial class Native      // <-- unconditional
{
    internal static partial EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags)
    {
        ...
        __retVal = __PInvoke(esFlags);
        ...
        [DllImportAttribute("kernel32.dll", EntryPoint = "SetThreadExecutionState", ExactSpelling = true)]
        static extern unsafe EXECUTION_STATE __PInvoke(EXECUTION_STATE __esFlags_native);
    }
}
```

Note the `Probe.cs(5,31)` error: the flag requirement infects **your own file**, not just generated output, because your `partial` type is the one being made `unsafe`. That makes `[LibraryImport]` uncontainable within a file or folder — only a separate project would contain it, and the payoff (AOT/trim readiness) is void because WinForms cannot trim.

**Verdict: reject.** All of the cost, none of the benefit.

---

## 3. CsWin32 (`Microsoft.Windows.CsWin32`)

**What it is.** A Roslyn source generator that reads the Win32 metadata from `microsoft/win32metadata` and emits bindings for the API names you list in `NativeMethods.txt`. Version tested: **0.3.333** (latest stable; still 0.x). Repo active as of 2026-09-18.

**It generates everything Covfefe needs.** A `NativeMethods.txt` of six lines produced twelve files *(empirical, Probe C)*, including `REASON_CONTEXT`, `POWER_REQUEST_TYPE`, `POWER_REQUEST_CONTEXT_FLAGS`, `EXECUTION_STATE`, and the kernel32 imports. The friendly overloads are genuinely good:

```csharp
// friendly overload — takes `in`, returns a SafeHandle
internal static unsafe Microsoft.Win32.SafeHandles.SafeFileHandle PowerCreateRequest(in REASON_CONTEXT Context)
{
    fixed (REASON_CONTEXT* ContextLocal = &Context)
    {
        var __resultSafeHandle = new SafeFileHandle(default, ownsHandle: true);
        HANDLE __result = PInvoke.PowerCreateRequest(ContextLocal);
        Marshal.InitHandle(__resultSafeHandle, __result);
        return __resultSafeHandle;
    }
}

// raw
[DllImport("KERNEL32.dll", ExactSpelling = true, SetLastError = true)]
internal static extern unsafe HANDLE PowerCreateRequest(REASON_CONTEXT* Context);

internal static unsafe BOOL PowerSetRequest(SafeHandle PowerRequest, POWER_REQUEST_TYPE RequestType) { ... }
```

Two things worth knowing about that output: the request handle comes back as **`SafeFileHandle`** (odd name for a power request, but behaviorally right — it is `SafeHandleZeroOrMinusOneIsInvalid`-derived and releases via `CloseHandle`), and the *only* pointer in the generated `REASON_CONTEXT` is `PWSTR* ReasonStrings` in the `Detailed` branch that Covfefe will never use.

### Unsafe verdict: hard requirement, and it is set for you silently

The package's `build/Microsoft.Windows.CsWin32.props` contains, with **no condition**:

```xml
<PropertyGroup>
  <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
```

This matches the official docs: *"Your project must allow unsafe code to support the generated code that will likely use pointers"* and *"The source generator NuGet package sets the default value of the `AllowUnsafeBlocks` property for your project to `true`"* ([CsWin32 getting started]).

So Probe C "succeeded without the flag" only because the package turned it on. **Adding `<PackageReference Include="Microsoft.Windows.CsWin32" />` to `Covfefe-core.csproj` enables unsafe code for every hand-written file in the project, and nothing in the csproj says so.** That is precisely the outcome the constraint rules out.

Forcing it back off *(empirical, Probe C2)*:

```
CSC : warning PInvoke002: AllowUnsafeBlocks must be set to 'true' in the project file for many APIs.
Compiler errors may result.
...\Windows.Win32.HANDLE.g.cs(24,43): error CS0227: Unsafe code may only appear if compiling with /unsafe
...\Windows.Win32.HINSTANCE.g.cs(24,43): error CS0227: ...
...\Windows.Win32.HMODULE.g.cs(24,43): error CS0227: ...
...\Windows.Win32.PCWSTR.g.cs(27,43): error CS0227: ...
...\Windows.Win32.PWSTR.g.cs(24,43): error CS0227: ...
...\Windows.Win32.PInvoke.KERNEL32.dll.g.cs(73,69): error CS0227: ...
...\Windows.Win32.PInvoke.KERNEL32.dll.g.cs(92,61): error CS0227: ...
...\Windows.Win32.PInvoke.KERNEL32.dll.g.cs(97,52): error CS0227: ...
...\Windows.Win32.PInvoke.KERNEL32.dll.g.cs(137,52): error CS0227: ...
...\Windows.Win32.REASON_CONTEXT.g.cs(67,50): error CS0227: ...
```

Ten errors, from the handle wrapper types outward. `PInvoke002` is the warning added by [CsWin32 issue #1370] for exactly this case.

### The `NativeMethods.json` options do not help

Checked against the generator's own [settings schema]:

| Option | Default | Effect on the unsafe requirement |
|---|---|---|
| `allowMarshaling` | `true` | **None.** Setting it `false` removes marshalling-dependent constructs and makes output *more* pointer-based, not less. |
| `friendlyOverloads.enabled` | `true` | Produces the `in`/`SafeHandle` overloads above. Removes pointers from the **call site**, not from the generated assembly. |
| `useSafeHandles` | `true` | Gives the `SafeFileHandle` return. Does not affect the flag. |
| `public` | `false` | Visibility only. See below. |

There is no option, in any combination, that emits binding code free of `unsafe`.

### Containment in a separate project — it works, with a catch

*(empirical, Probe F)* A two-project layout builds and runs:

```
Covfefe.Interop.csproj   PackageReference Microsoft.Windows.CsWin32 (PrivateAssets=all)
                         AllowUnsafeBlocks -> true   (set by the package)
                         NativeMethods.txt + a hand-written public wrapper

Covfefe-core.csproj      ProjectReference -> Covfefe.Interop
                         AllowUnsafeBlocks -> false  (verified via `dotnet msbuild -getProperty:AllowUnsafeBlocks`)
```

Verified three ways: `dotnet msbuild -getProperty:AllowUnsafeBlocks` returns `true` for the interop project and `false` for the app project; adding an `unsafe` method to the app project fails with `CS0227`; and the app runs, creating and clearing real power requests. `PrivateAssets="all"` is what stops the package's `.props` — and its `AllowUnsafeBlocks=true` — from flowing across the `ProjectReference`.

**The catch: the hand-written wrapper is mandatory, not optional.** With `"public": true` a flag-off consumer *can* call the friendly overloads and even declare a `REASON_CONTEXT` local *(empirical, Probe G)* — but it **cannot put the reason string in it**, which is the entire point of the migration:

```csharp
ctx.Reason.SimpleReasonString = new Windows.Win32.Foundation.PWSTR((char*)p);
// error CS0214: Pointers and fixed size buffers may only be used in an unsafe context  (x3)
```

`PWSTR` is constructed from a `char*`. So the containment layout is: interop project holds CsWin32 *and* a hand-written `PowerRequest` wrapper class exposing plain `string`/`bool`. Which means you write the wrapper either way — CsWin32 only saves you the `[DllImport]` lines and the struct, which §1 shows is 20 lines.

`InternalsVisibleTo` is not an alternative: it changes accessibility, not the fact that the types carry pointers.

### Build gotcha found while probing

Setting `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` with a `CompilerGeneratedFilesOutputPath` **inside the project folder** breaks the *second* build — the emitted `.g.cs` files get picked up by the SDK's default `Compile` glob and collide with the generator's own output:

```
generated\...\Windows.Win32.PInvoke.KERNEL32.dll.g.cs(24,3): error CS0579: Duplicate 'GeneratedCode' attribute
generated\...\Windows.Win32.PCWSTR.g.cs(25,33): error CS0229: Ambiguity between 'PCWSTR.DebuggerDisplay' and 'PCWSTR.DebuggerDisplay'
```

Fix: `<Compile Remove="generated/**" />`, or point the output path outside the project directory. Worth knowing because `EmitCompilerGeneratedFiles` is the main way to read generated code.

**Pros**
- Signatures come from the Windows SDK metadata, so they are right by construction — no `SendMessage`-style truncation bug possible.
- `SafeHandle` and `in` overloads for free.
- Generated XML docs are the real Win32 remarks, inline in IntelliSense — including the Modern Standby note on `PowerSetRequest`.
- Scales: if the plan later implements `DelaySleepOnLidClose` (§1 — `RegisterPowerSettingNotification`, `WM_POWERBROADCAST`, `GUID_LIDSWITCH_STATE_CHANGE`), the marginal cost is one more line in `NativeMethods.txt`. This is the strongest argument for CsWin32 and the one thing that could change the recommendation.

**Cons**
- Forces the flag on wherever it is referenced; containment costs a second project.
- Pre-1.0 after five years; a build-time dependency on a generator and on win32metadata.
- Debugging generated code needs `EmitCompilerGeneratedFiles` plus the workaround above.
- Breaks the `CLAUDE.md` rule as written. It would have to become "P/Invoke lives only in `Covfefe.Interop`", and `ApiImports/` would be deleted.

**Unsafe verdict: required. Containable to a separate project, at the cost of that project plus a wrapper you would have had to write anyway.**

---

## 4. Pre-generated binding packages

### Vanara (`Vanara.PInvoke.Kernel32`, `Vanara.PInvoke.PowrProf`) — viable

**Unsafe verdict: not required.** Builds and runs with an explicit `<AllowUnsafeBlocks>false</AllowUnsafeBlocks>` *(empirical, Probe V)*. The unsafe code, if any, is already compiled into the shipped assemblies; nothing leaks into your compilation.

The whole job, verbatim from the probe:

```csharp
using static Vanara.PInvoke.Kernel32;

var prev = SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED);

var ctx = new REASON_CONTEXT("Stay Awake requested by user");   // union handled for you
using var h = PowerCreateRequest(ctx);                          // SafePowerRequestObject
PowerSetRequest(h, POWER_REQUEST_TYPE.PowerRequestSystemRequired);
PowerClearRequest(h, POWER_REQUEST_TYPE.PowerRequestSystemRequired);
```

Output:

```
SetThreadExecutionState prev = ES_CONTINUOUS
PowerCreateRequest -> Vanara.PInvoke.Kernel32+SafePowerRequestObject, IsInvalid=False
PowerSetRequest System  = True
PowerSetRequest Display = True
PowerClearRequest Display = True
PowerClearRequest System  = True
```

This is the nicest API surface of any option — a real `SafeHandle` named for what it is, a `REASON_CONTEXT` constructor that takes a plain `string`, and no flag anywhere.

- **License:** MIT (all three assemblies). **Maintenance:** very active — `dahall/Vanara` last pushed 2026-09-19. Ships a `net10.0-windows7.0` target.
- **Cost:** the dependency closure is `Vanara.PInvoke.Kernel32` (793 KB) + `Vanara.Core` (1,058 KB) + `Vanara.PInvoke.PowrProf` (66 KB) ≈ **1.9 MB** *(empirical)*. `PowrProf` is not actually needed — `PowerCreateRequest` is in kernel32 ([PowerCreateRequest docs]) — so realistically ~1.85 MB. §10 of the plan measures the self-contained portable build; this is a real line item there.
- Neither package carries `IsTrimmable`/`IsAotCompatible` metadata, which is moot given `NETSDK1175`.

**Verdict: the fallback.** Choose it over hand-writing only if the team would rather depend than maintain. 1.9 MB for four functions is the trade.

### TerraFX.Interop.Windows — reject

**Unsafe verdict: required, at your call site.** The package sets no MSBuild properties (it ships no `.props`/`.targets` at all), so the flag is not set for you — which means your code simply fails to compile *(empirical, Probe T)*:

```
error CS0214: Pointers and fixed size buffers may only be used in an unsafe context
```

The surface is raw by design — there are no friendly overloads and no `SafeHandle`:

```
HANDLE PowerCreateRequest(REASON_CONTEXT*)
BOOL   PowerSetRequest(HANDLE, POWER_REQUEST_TYPE)
BOOL   PowerClearRequest(HANDLE, POWER_REQUEST_TYPE)
UInt32 SetThreadExecutionState(UInt32)
```

MIT, actively maintained (last push 2026-07-20), version `10.0.26100.6` tracks the SDK. It is a good library aimed at a different consumer — game and graphics code that is already `unsafe` throughout. For Covfefe it is strictly worse than every other option: unsafe required *and* you still hand-roll the `SafeHandle`.

### `PInvoke.*` (dotnet/pinvoke) — reject

**The repository is archived.** `dotnet/pinvoke` shows `"archived": true`, last push **2023-07-26**. Latest `PInvoke.Kernel32` is 0.7.124. Do not take a new dependency on an archived interop library for an API you intend to keep for years.

---

## 5. Where the unsafe flag can and cannot be scoped

Directly answering the containment question.

**`AllowUnsafeBlocks` is the MSBuild spelling of the `csc -unsafe` switch** — *"**AllowUnsafeBlocks** / `-unsafe`: Allow `unsafe` code"*, and *"The **AllowUnsafeBlocks** compiler option allows code that uses the `unsafe` keyword to compile. The default value for this option is `false`"* ([C# compiler options — language]). It is one flag per invocation of the compiler. There is no per-file, per-folder, or attribute-based form. Unlike `DefineConstants` (which has a `#define` counterpart) or `CheckForOverflowUnderflow` (which has `checked`/`unchecked` statements), `unsafe` has a keyword but no way to *permit* it for part of a compilation.

**A `Directory.Build.props` in a subfolder does nothing.** MSBuild *"walks the directory structure upwards from your project location `$(MSBuildProjectFullPath)`, stopping after it locates a Directory.Build.props file"* ([Customize the build by folder]). It never descends. Confirmed *(empirical, Probe D)*: a `Directory.Build.props` in `ApiImports/` setting `AllowUnsafeBlocks=true` was ignored; `dotnet msbuild -getProperty:AllowUnsafeBlocks` returned `false` and the file failed with `CS0227`.

**Conclusion: one compilation = one project = the finest available granularity.** Containment means a separate `.csproj`, or nothing.

**Forward-looking note, not actionable yet.** C# 15 / .NET 11 preview adds "updated memory safety rules" with `MemorySafetyRulesAttribute`, under which *"A project can enable the updated rules without allowing unsafe code, in which case it receives errors when it calls requires-unsafe APIs"* ([C# compiler options — language]). CsWin32 has an open PR to support it ([CsWin32 PR #1819]). This moves toward per-call-site unsafe obligations rather than per-project permission, but it is preview, on a framework Covfefe does not target. Do not plan around it.

---

## 6. The WinRT angle — `Windows.System.Display.DisplayRequest`

Worth a careful look because §5 of the plan proposes `net10.0-windows10.0.26100.0`, which gives WinRT projections with no package reference. If WinRT covered the job, the whole interop question would shrink.

**It does not, and the reason is sharper than "display-only".**

*(empirical, Probe W)* On `net10.0-windows10.0.26100.0` with `SupportedOSPlatformVersion=10.0.22621.0`, in an **unpackaged WinForms desktop process**, with **no package reference** and **no unsafe flag**:

```
PowerManager.EnergySaverStatus  = Disabled
PowerManager.BatteryStatus      = NotPresent
PowerManager.PowerSupplyStatus  = Adequate
PowerManager.RemainingChargePct = 100
DisplayRequest.RequestActive() OK
DisplayRequest.RequestRelease() OK
--- members of DisplayRequest ---
  Void RequestActive()
  Void RequestRelease()
--- types in Windows.System.Power ---
  Windows.System.Power.BackgroundEnergyManager
  Windows.System.Power.BatteryStatus
  Windows.System.Power.EnergySaverStatus
  Windows.System.Power.ForegroundEnergyManager
  Windows.System.Power.PowerManager
  Windows.System.Power.PowerSupplyStatus
```

So: it activates without throwing, from an unpackaged process, no packaged identity needed, no extra package, no unsafe. Good news that leads nowhere.

**What it does not cover:**

1. **Display only.** `DisplayRequest` has exactly two methods. It *"activates a display request… If successful, the screen is guaranteed not to turn off automatically due to user inactivity"* ([DisplayRequest docs]). There is no system-required equivalent, so Covfefe's `StayAwake` mode has no WinRT expression at all.
2. **It is deactivated when the app is not visible — this is the killer.** *"When a display request is activated, the device's display remains on while the app is visible. When the user moves the app out of the foreground, the system deactivates the app's display requests and reactivates them when the app returns to the foreground."* ([DisplayRequest docs]). Covfefe's normal state is **no window at all** — `CLAUDE.md`: *"There is no window on normal startup — only a tray icon."* The one mode `DisplayRequest` could serve (`MonitorsOn`) is precisely the mode in which Covfefe is never in the foreground.
3. **`Windows.System.Power` has no keep-awake API.** `PowerManager` is read-only status plus events (`EnergySaverStatus`, `BatteryStatus`, `PowerSupplyStatus`, `RemainingChargePercent`). `Background/ForegroundEnergyManager` are energy-quota notifications, not power requests.
4. **No named reason string.** Nothing here appears in `powercfg /requests`, which is win #1 in §1 of the plan.

**Caveat on the empirical part:** the probe proves `RequestActive()` *succeeds*. It cannot prove the display stayed on, because the documented behavior is conditional on app visibility and the probe process had no visible window. The documentary evidence is what disqualifies it, not the probe.

**Verdict: confirms the plan's rejection, and supplies the stronger reason.** Recommend updating §1's one-liner from "display-only and buys nothing" to "display-only **and system-deactivated whenever the app is not in the foreground, which for a tray app is always**."

`Windows.System.Power` is still worth remembering for a *different* job: if §1's `DelaySleepOnBatteryStateChange` is ever implemented, `PowerManager.PowerSupplyStatusChanged` / `BatteryStatusChanged` are a no-interop alternative to `RegisterPowerSettingNotification` + `WM_POWERBROADCAST`. Out of scope here.

---

## 7. Other options, briefly

- **C++/CLI shim.** Works on .NET Core, Windows-only. Adds an MSVC toolchain dependency to a repo whose only current native-tooling dependency is the WiX installer, produces a second binary to ship and sign (§10 of the plan makes signing expensive), and is explicitly **incompatible with Native AOT** ([Native AOT limitations]). For four kernel32 calls that need no unsafe code at all: **reject**.
- **Separate hand-written native DLL.** Same objections, plus you now maintain C. **Reject.**
- **`Microsoft.Windows.SDK.Contracts`.** Not deprecated, currently `10.0.28000.2705`, but it targets `netstandard2.0` and exists to bring WinRT projections to frameworks that lack them. On `net10.0-windows10.0.26100.0` the projections are already there — Probe W used `Windows.System.Display` and `Windows.System.Power` with zero package references. **Unnecessary.** And since §6 rules WinRT out for this job, moot twice over.
- **`Microsoft.Windows.CsWinRT`.** Solves consuming and authoring WinRT components. Not a Win32 P/Invoke story. **Not applicable.**

---

## 8. Empirical results

All probes: SDK **10.0.204**, `Microsoft.WindowsDesktop.App` **10.0.8**, Windows 11 Pro 26200, x64. Packages: `Microsoft.Windows.CsWin32` **0.3.333**, `Vanara.PInvoke.Kernel32` / `.PowrProf` **5.0.7**, `TerraFX.Interop.Windows` **10.0.26100.6**. Built in a scratch directory outside the repo; nothing was added to `Covfefe.sln` or `CovfefeCore.slnx`.

| Probe | Setup | Result |
|---|---|---|
| **A** | `[LibraryImport]` on `SetThreadExecutionState`, `net10.0-windows`, no flag | **FAIL** — `SYSLIB1062` + 2 × `CS0227` |
| **A2** | Same, `AllowUnsafeBlocks=true` | Builds. Generated stub has `unsafe` on the type, no pointers in the body |
| **B** | Hand-written `[DllImport]`, full `REASON_CONTEXT` union, `in`, custom `SafeHandle`, no flag | **Builds and runs.** `sizeof=32`; create / set ×2 / clear ×2 all succeed |
| **B-pub** | Probe B published `win-x64`, self-contained, single-file, `PublishTrimmed` + `TrimMode=full` | **Succeeds, 0 IL warnings**, 13.1 MB exe, runs correctly |
| **C** | CsWin32, 6 names in `NativeMethods.txt`, flag not mentioned | Builds — **because the package set the flag**. 12 files generated |
| **C2** | Same + explicit `AllowUnsafeBlocks=false` | **FAIL** — `warning PInvoke002` + 10 × `CS0227` |
| **D** | `Directory.Build.props` in a **subfolder** setting the flag | **Ignored.** `-getProperty` → `false`; `CS0227` |
| **F** | `Covfefe.Interop` (CsWin32, flag `true`) ← `App` (flag `false`) | **Builds and runs.** App rejects `unsafe` with `CS0227`; power requests work |
| **G** | CsWin32 `"public": true`, consumed from a flag-off project | Friendly overloads and `REASON_CONTEXT` **usable**; setting the reason string **FAILS** with 3 × `CS0214` |
| **S** | `[MarshalAs(LPWStr)] string` + padding instead of the union, no flag | **Builds and runs.** `Marshal.SizeOf=32`, no manual allocation |
| **T** | TerraFX, call `PowerCreateRequest(&ctx)`, no flag | **FAIL** — 2 × `CS0214`. Package ships no `.props` |
| **V** | Vanara 5.0.7 with explicit `AllowUnsafeBlocks=false` | **Builds and runs.** `SafePowerRequestObject`, `REASON_CONTEXT(string)`. ~1.9 MB deps |
| **W** | WinRT `DisplayRequest` + `PowerManager`, `net10.0-windows10.0.26100.0`, unpackaged WinForms, no packages, no flag | **Builds and runs.** `DisplayRequest` has only `RequestActive` / `RequestRelease` |
| **WF** | WinForms + `PublishTrimmed` | **FAIL** — `error NETSDK1175` |

Verbatim diagnostics are quoted in §1–§4 next to the claims they support.

### Probe B, full runtime output

```
sizeof(REASON_CONTEXT) = 32
PowerCreateRequest OK (SafeHandle, no unsafe)
PowerSetRequest(SystemRequired) = True (err 0)
PowerSetRequest(DisplayRequired) = True (err 0)
--- powercfg /requests ---
This command requires administrator privileges and must be executed from an elevated command prompt.
PowerClearRequest(Display) = True
PowerClearRequest(System)  = True
```

### Probe WF, verbatim

```
C:\Program Files\dotnet\sdk\10.0.204\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.RuntimeIdentifierInference.targets(305,5):
error NETSDK1175: Windows Forms is not supported or recommended with trimming enabled.
Please go to https://aka.ms/dotnet-illink/windows-forms for more details.
```

---

## 9. Open questions

1. ~~**Does the kernel copy `SimpleReasonString`, or retain the pointer?**~~ **Resolved — moot, see ADR-0005.** Still undocumented (the `REASON_CONTEXT` docs address mutability, not lifetime), but the reason-string buffers are now allocated once into `static readonly IntPtr` fields and never freed, so both branches of the unknown are correct and the question needs no answer. This holds only for the faithful union shape with a buffer we own; the `[MarshalAs(UnmanagedType.LPWStr)] string` shape (Probe S) lets the marshaller free its temporary buffer on return and reintroduces the dependency. Probe B and Probe S both free early and remain unsafe as templates on this point.
2. **Does the reason string actually show up in `powercfg /requests` as `[PROCESS] Covfefe.exe — "…"`?** This is win #1 of §1 of the plan and remains **unverified** — it needs one elevated `powercfg /requests` while Covfefe holds a request. Cheap to check; do it before committing to the migration.
3. **Is the padded-struct shape (Probe S) safe across architectures?** It matched 32 bytes on x64. `PowerCreateRequest` takes no size parameter, so the native side reads by `Flags`. Not re-checked on Arm64, which matters if §10 ever ships an Arm64 portable build. The full-union shape (Probe B / §1) avoids the question.
4. **`Marshal.GetLastWin32Error` through CsWin32's friendly overload.** Its `PowerCreateRequest` wrapper calls `Marshal.InitHandle` after the P/Invoke. Whether the last-error value survives that was not tested. Only relevant if CsWin32 is chosen.
5. ~~Whether §1's eventual `DelaySleepOnLidClose` work (`RegisterPowerSettingNotification`, `GUID_LIDSWITCH_STATE_CHANGE`) can be hand-written without unsafe.~~ **Moot — the lid-close work has been dropped** (2026-09-19). With it goes the one identified finding that could have flipped the recommendation toward CsWin32-in-a-separate-project; see ADR-0004. `DelaySleepOnBatteryStateChange` remains undecided, and `SystemEvents.PowerModeChanged` may cover it with no interop at all.

---

## 10. Effect on the `CLAUDE.md` rule

Today: *"P/Invoke lives only in `ApiImports/`."*

- **Recommended option (hand-written):** rule unchanged. `ApiImports/WindowsPowerManagementApi.cs` gains the Power Request declarations and the `REASON_CONTEXT` types; `ApiImports/WindowsUIApi.cs` is deleted per §5 of the plan. `Windows7PowerManagementEngine` is joined or replaced by a new engine behind `IPowerManagementEngine`, exactly as §1 anticipates.
- **Vanara:** rule effectively retires — there is no P/Invoke in the repo at all. Replace it with something like "native power APIs are reached only through `IPowerManagementEngine` implementations."
- **CsWin32 in a separate project:** rule becomes "P/Invoke lives only in `Covfefe.Interop`", `ApiImports/` is deleted, and the new project needs its own note explaining that `AllowUnsafeBlocks` is on there and why. `CovfefeCore.slnx` and `Covfefe.sln` both need the new project added.

---

## Sources

First-party only. Empirical claims above are this machine's build output and are labelled as such.

**Microsoft Learn**

- [SYSLIB1062 / P-Invoke source-generation diagnostics](https://learn.microsoft.com/en-us/dotnet/fundamentals/syslib-diagnostics/syslib1062) — `SYSLIB1062`: "The project must be updated with `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>`."
- [P/Invoke source generation](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke-source-generation) — "The project must be marked unsafe using AllowUnsafeBlocks."
- [C# compiler options — language feature rules](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/language) — `AllowUnsafeBlocks` / `-unsafe`; C# 15 updated memory safety rules.
- [Customize the build by folder (MSBuild)](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory) — `Directory.Build.props` search walks *upward* from `$(MSBuildProjectFullPath)`.
- [PowerCreateRequest](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-powercreaterequest) — signature, Kernel32.dll, Windows 7+, `INVALID_HANDLE_VALUE` on failure, `CloseHandle` to release.
- [PowerSetRequest](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-powersetrequest) — Modern Standby behavior; best practice to supply a localized reason string.
- [PowerClearRequest](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-powerclearrequest)
- [REASON_CONTEXT](https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-reason_context) — union definition, `POWER_REQUEST_CONTEXT_SIMPLE_STRING` = 0x1, read-only string remark.
- [SetThreadExecutionState](https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-setthreadexecutionstate)
- [Windows.System.Display.DisplayRequest](https://learn.microsoft.com/en-us/uwp/api/windows.system.display.displayrequest) — "the device's display remains on while the app is visible… the system deactivates the app's display requests" when not in the foreground.
- [Windows.System.Power.PowerManager](https://learn.microsoft.com/en-us/uwp/api/windows.system.power.powermanager)
- [Native AOT deployment overview](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/) — requires trimming; no C++/CLI.
- [Known trimming incompatibilities](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/incompatibilities)
- [WinForms trimming guidance](https://aka.ms/dotnet-illink/windows-forms) — target of the `NETSDK1175` message.

**Repositories and packages**

- [microsoft/CsWin32](https://github.com/microsoft/CsWin32) — active, last push 2026-09-18.
- [CsWin32 — Getting started](https://microsoft.github.io/CsWin32/docs/getting-started.html) — "Your project must allow unsafe code…"; "The source generator NuGet package sets the default value of the `AllowUnsafeBlocks` property for your project to `true`."
- [CsWin32 settings.schema.json](https://github.com/microsoft/CsWin32/blob/main/src/Microsoft.Windows.CsWin32/settings.schema.json) — `allowMarshaling`, `friendlyOverloads`, `useSafeHandles`, `public`.
- [CsWin32 issue #1370](https://github.com/microsoft/CsWin32/issues/1370) — "Log a warning if the project does not support unsafe code" (the `PInvoke002` warning).
- [CsWin32 PR #1819](https://github.com/microsoft/CsWin32/pull/1819) — "Support the updated memory safety rules (C# 15 / .NET 11)", open.
- [microsoft/win32metadata](https://github.com/microsoft/win32metadata) — active, last push 2026-09-19.
- [dahall/Vanara](https://github.com/dahall/Vanara) — MIT, active, last push 2026-09-19.
- [Vanara.PInvoke.Kernel32 on nuget.org](https://www.nuget.org/packages/Vanara.PInvoke.Kernel32) — 5.0.7, MIT, ships `net10.0-windows7.0`.
- [terrafx/terrafx.interop.windows](https://github.com/terrafx/terrafx.interop.windows) — MIT, active, last push 2026-07-20.
- [dotnet/pinvoke](https://github.com/dotnet/pinvoke) — **archived**, last push 2023-07-26.
- [Microsoft.Windows.CsWin32 on nuget.org](https://www.nuget.org/packages/Microsoft.Windows.CsWin32) — 0.3.333.
- [Microsoft.Windows.SDK.Contracts on nuget.org](https://www.nuget.org/packages/Microsoft.Windows.SDK.Contracts) — 10.0.28000.2705, `netstandard2.0`.
- [dotnet/sdk issue #34129](https://github.com/dotnet/sdk/issues/34129) — "Native AOT doesn't work with Windows Forms".
