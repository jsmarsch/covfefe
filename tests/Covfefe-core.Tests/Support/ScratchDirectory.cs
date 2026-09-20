using System.IO;
using System.Runtime.CompilerServices;
using IOPath = System.IO.Path;

namespace Covfefe.Tests.Support;

/// <summary>
/// A directory created for one test and deleted when that test ends.
/// <para>
/// Tests that exercise settings persistence write to a real file rather than a mocked
/// filesystem, so every such test needs its own place to write. The name carries the calling
/// test's name plus a random suffix: the name makes a leftover directory traceable, the suffix
/// is what keeps tests running in parallel out of each other's way.
/// </para>
/// See <c>docs/agents/testing.md</c> for the convention this implements.
/// </summary>
public sealed class ScratchDirectory : IDisposable
{
    /// <summary>Every scratch directory lives under here, so a stray one is obvious and easy to sweep up.</summary>
    public static string Root { get; } = IOPath.Combine(IOPath.GetTempPath(), "Covfefe.Tests");

    private ScratchDirectory(string path) => Path = path;

    /// <summary>The full path of the directory. It exists by the time this returns.</summary>
    public string Path { get; }

    /// <summary>
    /// Create a scratch directory for the calling test.
    /// </summary>
    public static ScratchDirectory Create([CallerMemberName] string callerName = "")
    {
        var path = IOPath.Combine(Root, ScratchNaming.UniqueName(callerName));
        Directory.CreateDirectory(path);
        return new ScratchDirectory(path);
    }

    /// <summary>A path inside this directory. The file itself is not created.</summary>
    public string PathTo(string fileName) => IOPath.Combine(Path, fileName);

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
        catch (IOException)
        {
            // intent: a file left locked by the code under test must not turn a passing test red.
            // The directory is uniquely named, so leaving it behind affects nothing else.
        }
        catch (UnauthorizedAccessException)
        {
            // same intent: a read-only file inside the directory makes the recursive delete throw.
        }
    }
}
