using System.Runtime.CompilerServices;
using Microsoft.Win32;

namespace Covfefe.Tests.Support;

/// <summary>
/// A registry key under HKEY_CURRENT_USER created for one test and deleted when that test ends.
/// <para>
/// Start at Login is a real registry entry, and tests that cover it run against a real key —
/// mocking the registry would only replay our own assumptions about it. This gives each test a
/// key of its own, well away from the developer's actual Run key, so the tests can run in
/// parallel and cannot disturb the machine they run on.
/// </para>
/// See <c>docs/agents/testing.md</c> for the convention this implements.
/// </summary>
public sealed class ScratchRegistryKey : IDisposable
{
    /// <summary>Every scratch key lives under here, HKCU-relative, so a stray one is obvious and easy to sweep up.</summary>
    public const string Root = @"Software\Covfefe.Tests\Scratch";

    private ScratchRegistryKey(string path, RegistryKey key)
    {
        Path = path;
        Key = key;
    }

    /// <summary>The key's path relative to HKEY_CURRENT_USER — what a facade under test takes as its Run key path.</summary>
    public string Path { get; }

    /// <summary>The open, writable key. It exists by the time this returns.</summary>
    public RegistryKey Key { get; }

    /// <summary>
    /// Create a scratch key for the calling test.
    /// </summary>
    public static ScratchRegistryKey Create([CallerMemberName] string callerName = "")
    {
        var path = Root + @"\" + ScratchNaming.UniqueName(callerName);
        var key = Registry.CurrentUser.CreateSubKey(path, writable: true)
                  ?? throw new InvalidOperationException("Could not create the scratch registry key HKCU\\" + path + ".");
        return new ScratchRegistryKey(path, key);
    }

    /// <summary>
    /// The path of a child key that this scratch key owns but that has deliberately not been
    /// created. Use it to prove that code under test copes with a key that is not there yet.
    /// </summary>
    public string PathToAbsentSubKey(string name) => Path + @"\" + name;

    public void Dispose()
    {
        Key.Dispose();
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(Path, throwOnMissingSubKey: false);
        }
        catch (UnauthorizedAccessException)
        {
            // intent: cleanup must never turn a passing test red. The key is uniquely named,
            // so leaving it behind affects nothing else.
        }
    }
}
