using System.IO;
using Microsoft.Win32;

namespace Covfefe.Tests.Support;

/// <summary>
/// Sweeps up the two containers the scratch helpers create — <c>%TEMP%\Covfefe.Tests</c> and
/// <c>HKCU\Software\Covfefe.Tests</c> — once the whole run is over, so a test run leaves the
/// machine exactly as it found it.
/// <para>
/// Each container is removed only when it is already empty. Anything still inside belongs to a
/// test run happening at the same time, or to a scratch location whose own cleanup could not
/// finish; neither is ours to delete.
/// </para>
/// </summary>
public static class ScratchRootCleanup
{
    private const string RegistryContainer = @"Software\Covfefe.Tests";

    [After(Assembly)]
    public static void RemoveEmptyScratchRoots()
    {
        RemoveDirectoryIfEmpty(ScratchDirectory.Root);
        RemoveRegistryKeyIfEmpty(ScratchRegistryKey.Root);
        RemoveRegistryKeyIfEmpty(RegistryContainer);
    }

    private static void RemoveDirectoryIfEmpty(string path)
    {
        try
        {
            if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any())
                Directory.Delete(path);
        }
        catch (IOException)
        {
            // intent: tidying up must never fail a run that has already passed.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static void RemoveRegistryKeyIfEmpty(string hkcuRelativePath)
    {
        try
        {
            using (var key = Registry.CurrentUser.OpenSubKey(hkcuRelativePath, writable: false))
            {
                if (key is null)
                    return;
                if (key.SubKeyCount > 0 || key.ValueCount > 0)
                    return;
            }

            Registry.CurrentUser.DeleteSubKey(hkcuRelativePath, throwOnMissingSubKey: false);
        }
        catch (UnauthorizedAccessException)
        {
            // intent: tidying up must never fail a run that has already passed.
        }
        catch (InvalidOperationException)
        {
            // thrown when the key gained a child between the check above and the delete
        }
    }
}
