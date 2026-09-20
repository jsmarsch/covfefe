using System.IO;
using Microsoft.Win32;

namespace Covfefe.Tests.Support;

/// <summary>
/// The tickets that follow write to a real file and a real registry key on purpose. These tests
/// prove the scratch helpers they will lean on really do create, isolate, and clean up.
/// </summary>
public class ScratchIsolationTests
{
    private const string QuotedExecutablePath = "\"C:\\Program Files\\Covfefe\\Covfefe.exe\"";

    [Test]
    public async Task A_scratch_directory_exists_while_in_use_and_is_gone_afterwards()
    {
        string path;
        using (var scratch = ScratchDirectory.Create())
        {
            path = scratch.Path;
            await Assert.That(Directory.Exists(path)).IsTrue();

            await File.WriteAllTextAsync(scratch.PathTo("settings.json"), "{}");
            await Assert.That(File.Exists(scratch.PathTo("settings.json"))).IsTrue();
        }

        await Assert.That(Directory.Exists(path)).IsFalse();
    }

    [Test]
    public async Task Disposing_a_scratch_directory_it_cannot_delete_does_not_turn_the_test_red()
    {
        // a read-only file is enough to make a recursive delete refuse
        var scratch = ScratchDirectory.Create();
        var readOnlyFile = scratch.PathTo("read-only.json");
        await File.WriteAllTextAsync(readOnlyFile, "{}");
        File.SetAttributes(readOnlyFile, FileAttributes.ReadOnly);

        try
        {
            scratch.Dispose(); // the assertion: this must not throw
        }
        finally
        {
            ForceDelete(scratch.Path);
        }

        await Assert.That(Directory.Exists(scratch.Path)).IsFalse();
    }

    [Test]
    public async Task Scratch_directories_never_collide_and_stay_under_the_test_root()
    {
        using var first = ScratchDirectory.Create();
        using var second = ScratchDirectory.Create();

        await Assert.That(first.Path).IsNotEqualTo(second.Path);
        await Assert.That(first.Path).StartsWith(ScratchDirectory.Root);
        await Assert.That(second.Path).StartsWith(ScratchDirectory.Root);
    }

    [Test]
    public async Task A_scratch_registry_key_exists_while_in_use_and_is_gone_afterwards()
    {
        string path;
        using (var scratch = ScratchRegistryKey.Create())
        {
            path = scratch.Path;
            scratch.Key.SetValue("Covfefe", QuotedExecutablePath);

            using var reopened = Registry.CurrentUser.OpenSubKey(path, writable: false);
            await Assert.That(reopened).IsNotNull();
            await Assert.That(reopened!.GetValue("Covfefe")).IsEqualTo(QuotedExecutablePath);
        }

        using var afterDisposal = Registry.CurrentUser.OpenSubKey(path, writable: false);
        await Assert.That(afterDisposal).IsNull();
    }

    [Test]
    public async Task Scratch_registry_keys_never_collide_and_stay_clear_of_the_real_Run_key()
    {
        using var first = ScratchRegistryKey.Create();
        using var second = ScratchRegistryKey.Create();

        await Assert.That(first.Path).IsNotEqualTo(second.Path);
        await Assert.That(first.Path).StartsWith(ScratchRegistryKey.Root);
        await Assert.That(second.Path).StartsWith(ScratchRegistryKey.Root);
        await Assert.That(first.Path).DoesNotContain(@"CurrentVersion\Run");
    }

    [Test]
    public async Task A_scratch_key_can_name_a_sub_key_that_does_not_exist_yet()
    {
        using var scratch = ScratchRegistryKey.Create();

        var absentPath = scratch.PathToAbsentSubKey("Run");

        using var absent = Registry.CurrentUser.OpenSubKey(absentPath, writable: false);
        await Assert.That(absent).IsNull();
    }

    /// <summary>Clears whatever stopped an ordinary delete, so the test leaves nothing behind.</summary>
    private static void ForceDelete(string directory)
    {
        if (!Directory.Exists(directory))
            return;

        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            File.SetAttributes(file, FileAttributes.Normal);

        Directory.Delete(directory, recursive: true);
    }
}
