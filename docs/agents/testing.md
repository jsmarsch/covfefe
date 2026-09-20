# Testing

Tests live in **`tests/Covfefe-core.Tests/`** and cover the core project (`src/Covfefe-core/`),
which is the only application project in the repo.

| Thing     | Value                                                         |
| --------- | ------------------------------------------------------------- |
| Framework | [TUnit](https://tunit.dev) (`TUnit` metapackage)              |
| Runner    | Microsoft.Testing.Platform — **not** VSTest                   |
| Target    | `net10.0-windows`, `UseWindowsForms` on, same as core         |
| Solution  | Registered in `CovfefeCore.slnx`                              |

## Running them

```powershell
dotnet test
dotnet test --solution CovfefeCore.slnx
dotnet test --project tests/Covfefe-core.Tests/Covfefe-core.Tests.csproj
dotnet test --project tests/Covfefe-core.Tests/Covfefe-core.Tests.csproj -- --treenode-filter "/*/*/ScratchIsolationTests/*"
```

A bare `dotnet test` at the repo root works: `CovfefeCore.slnx` is the only solution file there, so
there is nothing to disambiguate. It used to fail with _"Found more than one solution file"_ when
`Covfefe.sln` sat beside it; that solution is gone.

One thing about the command line is still easy to trip over.

**The flag is `--project`, not a positional path.** TUnit runs on Microsoft.Testing.Platform, and
`dotnet test` on the .NET 10 SDK refuses the old VSTest bridge outright, so `global.json` at the
repo root opts the whole repo into the newer command:

```json
{ "test": { "runner": "Microsoft.Testing.Platform" } }
```

That command does not accept a bare project path — `dotnet test some.csproj` fails with
_"Specifying a project for 'dotnet test' should be via '--project'"_. Runner flags go after a `--`.

The test project is an `Exe`, not a library: the platform generates its entry point, so
`OutputType`, `IsTestProject` and `IsTestingPlatformApplication` all have to be set in the csproj.

## Isolating tests that touch real machine state

Several parts of Covfefe are only meaningfully testable against the real thing. Settings
persistence writes a real JSON file; Start at Login writes a real registry value. Faking the
filesystem or the registry there would just replay our own assumptions back at us, so these tests
use the real ones.

TUnit runs tests in parallel by default, and we keep it that way. **The convention is a uniquely
named scratch location per test, not serial execution.**

- Filesystem → `ScratchDirectory.Create()`, which gives a directory under
  `%TEMP%\Covfefe.Tests\` and deletes it on dispose.
- Registry → `ScratchRegistryKey.Create()`, which gives a key under
  `HKCU\Software\Covfefe.Tests\Scratch\` and deletes the subtree on dispose.

Both live in `tests/Covfefe-core.Tests/Support/`. Both name the location after the calling test and
add a random suffix, so a leftover is traceable and two tests can never land on the same one. Wrap
them in `using` and let disposal clean up.

```csharp
[Test]
public async Task Saving_writes_a_readable_file()
{
    using var scratch = ScratchDirectory.Create();
    var settingsFilePath = scratch.PathTo("settings.json");
    // ... hand that path to the code under test
}
```

For this to work, the code under test has to let the test say **where**. `SettingsService` does not
yet: as of today its constructor takes no arguments and its Run key path is a hard-coded `const`,
which is exactly why nothing tests it. Ticket #4 turns the settings file path and the Run key path
into constructor arguments, and only then does the example above compile against a real service.

Anything new that reaches for a fixed machine-wide path should take that path as an argument for
the same reason — a hard-coded path is what forces a test to either fake the world or run alone.

`ScratchRegistryKey.PathToAbsentSubKey(name)` names a key the helper owns but has not created, for
covering the case where the code under test has to create a missing key itself.

`ScratchRootCleanup` removes the two containers — `%TEMP%\Covfefe.Tests` and
`HKCU\Software\Covfefe.Tests` — once the whole run is over, but only when they are already empty.
A run therefore leaves the machine as it found it, while anything a concurrent run still owns is
left alone. If you ever do find something under those two paths, a test failed to clean up and the
name tells you which one.

### When a scratch location genuinely isn't possible

Fall back to `[NotInParallel("<key>")]`, which serialises only the tests sharing that key rather
than the whole suite. Use it as the exception, and say in a comment what shared thing forced it —
`[assembly: NotInParallel]` is never the answer.

## Writing tests

- TUnit assertions are awaited: `await Assert.That(actual).IsEqualTo(expected);`. **A forgotten
  `await` passes silently**, so check for it when a test looks suspiciously green.
- Tests that touch WinForms controls need an STA thread: put `[TestExecutor<STAThreadExecutor>]`
  on the test alongside `[Test]` (see `ControlExtensionsTests`).
- **`IsEquivalentTo` ignores order.** It compares membership, so it will not catch a collection
  that comes back in the wrong sequence. When order is the thing under test, assert element by
  element — `await Assert.That(entries[0].Key).IsEqualTo(...)`.
- Lifecycle hooks are `[Before(Test)]` / `[After(Test)]`, and `[Before(Class)]` / `[After(Class)]`.
- Test names read as sentences with underscores —
  `LoadFromEnum_lists_members_in_enum_order_not_alphabetically`.

A test run drops an HTML report in `TestResults/` next to wherever it was invoked from. That
directory is already gitignored.
