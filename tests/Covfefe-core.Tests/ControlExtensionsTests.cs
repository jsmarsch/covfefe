using System.Collections.Generic;
using System.Windows.Forms;
using Covfefe.Models;
using TUnit.Core.Executors;

namespace Covfefe.Tests;

/// <summary>
/// The combo box on the settings screen gets its contents from the sleep mode enum, and the
/// [Description] attributes on that enum are the text the user reads. These tests pin that.
/// They touch WinForms controls, so they run on an STA thread.
/// </summary>
public class ControlExtensionsTests
{
    [Test]
    [TestExecutor<STAThreadExecutor>]
    public async Task LoadFromEnum_shows_the_Description_attribute_and_carries_the_enum_value()
    {
        using var comboBox = new ComboBox();

        comboBox.LoadFromEnum<CovfefeSleepMode>();

        var entries = (KeyValuePair<CovfefeSleepMode, string>[]) comboBox.DataSource!;
        await Assert.That(entries.Select(e => e.Value))
            .IsEquivalentTo(new[] { "Keep Monitors On", "Normal", "Stay Awake!" });
        await Assert.That(entries.Select(e => e.Key))
            .IsEquivalentTo(new[] { CovfefeSleepMode.MonitorsOn, CovfefeSleepMode.Normal, CovfefeSleepMode.StayAwake });
        await Assert.That(comboBox.DisplayMember).IsEqualTo("Value");
        await Assert.That(comboBox.ValueMember).IsEqualTo("Key");
    }

    [Test]
    [TestExecutor<STAThreadExecutor>]
    public async Task LoadFromEnum_sorts_by_the_text_the_user_reads_not_by_the_enum_order()
    {
        using var comboBox = new ComboBox();

        comboBox.LoadFromEnum<CovfefeSleepMode>();

        // Declaration order is Normal, StayAwake, MonitorsOn; display order is alphabetical by description.
        var entries = (KeyValuePair<CovfefeSleepMode, string>[]) comboBox.DataSource!;
        await Assert.That(entries[0].Key).IsEqualTo(CovfefeSleepMode.MonitorsOn);
    }

    [Test]
    [TestExecutor<STAThreadExecutor>]
    public async Task LoadFromEnum_omits_members_the_filter_rejects()
    {
        using var comboBox = new ComboBox();

        comboBox.LoadFromEnum<CovfefeSleepMode>(mode => mode != CovfefeSleepMode.Normal);

        var entries = (KeyValuePair<CovfefeSleepMode, string>[]) comboBox.DataSource!;
        await Assert.That(entries.Select(e => e.Key))
            .IsEquivalentTo(new[] { CovfefeSleepMode.MonitorsOn, CovfefeSleepMode.StayAwake });
    }
}
