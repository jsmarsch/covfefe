using System.IO;
using System.Text;

namespace Covfefe.Tests.Support;

/// <summary>
/// Builds the per-test names that keep scratch locations from colliding.
/// </summary>
internal static class ScratchNaming
{
    /// <summary>
    /// A name made of the calling test's name — so a leftover is traceable back to the test that
    /// left it — followed by a random suffix, which is what actually guarantees uniqueness when
    /// the same test runs more than once or several tests run at the same time.
    /// </summary>
    public static string UniqueName(string callerName)
    {
        var readablePart = Sanitize(callerName);
        var uniquePart = Guid.NewGuid().ToString("n")[..8];
        return readablePart.Length == 0 ? uniquePart : $"{readablePart}-{uniquePart}";
    }

    private static string Sanitize(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(name.Length);
        foreach (var character in name)
        {
            if (Array.IndexOf(invalid, character) < 0 && character != '\\')
                builder.Append(character);
        }

        // keep names short enough that a nested scratch path stays well inside Windows' path limit
        return builder.Length > 60 ? builder.ToString(0, 60) : builder.ToString();
    }
}
