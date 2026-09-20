# Keep the power request reason string, with a static process-lifetime buffer

Power requests carry a `POWER_REQUEST_CONTEXT_SIMPLE_STRING` reason. Covfefe uses a single reason string for every sleep mode, allocated once into a `static readonly IntPtr` field and never freed.

## Context

`PowerCreateRequest` takes `REASON_CONTEXT` as a required parameter, and `REASON_CONTEXT.Flags` has only two documented values — `POWER_REQUEST_CONTEXT_SIMPLE_STRING` (0x1) and `POWER_REQUEST_CONTEXT_DETAILED_STRING` (0x2). Neither means "no reason", and the detailed branch is more work, not less. So dropping the reason string is not separable from dropping the Power Request API itself.

It is undocumented whether the kernel copies `SimpleReasonString` or retains the pointer. The `REASON_CONTEXT` documentation states only that the functions read from the strings and do not write to them, which addresses mutability, not lifetime. That unknown was nearly enough to abandon the migration.

Allocating the buffers for the process lifetime makes the question moot rather than answered. If the kernel copies, our buffer is redundant and harmless; if it retains the pointer, our buffer is still alive and the pointer is still valid. The only way to be wrong is to free a buffer while a request handle is outstanding, which we never do.

The buffer is `static`, not an instance field, so that repeatedly constructing the engine cannot leak. One allocation at type initialization is sufficient; the CLR guarantees that initializer runs once and is thread-safe. This holds even if the string is localized later, since a resource lookup still resolves once at type init.

The reason is **one constant shared by every sleep mode**, not one per mode. `PowerCreateRequest` bakes the reason into the handle and no API changes it afterwards, so per-mode strings would force a new handle on every mode change. The mode stays visible in `powercfg /requests` without them: Stay Awake appears under `SYSTEM:` alone, Keep Monitors On under both `SYSTEM:` and `DISPLAY:`. The handle is therefore created once, for the engine's lifetime — see ADR-0006.

## Consequences

`REASON_CONTEXT` must be declared as the faithful union shape with an `IntPtr` we own and populate via `Marshal.StringToHGlobalUni`. It must **not** use the convenience shape `[MarshalAs(UnmanagedType.LPWStr)] public string SimpleReasonString` — that one runs correctly in a probe, but the marshaller frees its temporary buffer when the call returns, which reintroduces exactly the dependency on undocumented behavior that this decision removes.

If the reason string ever becomes genuinely per-request or per-mode, this decision no longer applies: the buffer would have to follow the request handle's lifetime, and the handle would have to be recreated on each mode change.

Separately, the payoff — that the reason text actually appears in `powercfg /requests` as `[PROCESS] Covfefe.exe — "…"` — is still unverified and needs one elevated invocation while a request is held. The remaining benefit of the Power Request migration does not depend on the string's content: handles are process-scoped rather than thread-scoped.

`PowerRequestExecutionRequired` was also cited as a benefit. It is **not used**. It keeps the *calling* process running during Modern Standby, and Covfefe is not the process that needs to keep running — the program it is protecting is, and that protection cannot be requested on another process's behalf. `PowerRequestSystemRequired` prevents the low-power state outright, which is what actually serves Covfefe's purpose. Add `PowerRequestExecutionRequired` only if a Modern Standby machine is observed sleeping anyway; it is one line in the mapping.
