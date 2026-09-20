using System.Reflection;
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
            .IsEquivalentTo(new[] { "Normal", "Stay Awake!", "Keep Monitors On" });
        await Assert.That(entries.Select(e => e.Key))
            .IsEquivalentTo(new[] { CovfefeSleepMode.Normal, CovfefeSleepMode.StayAwake, CovfefeSleepMode.MonitorsOn });
        await Assert.That(comboBox.DisplayMember).IsEqualTo("Value");
        await Assert.That(comboBox.ValueMember).IsEqualTo("Key");
    }

    [Test]
    [TestExecutor<STAThreadExecutor>]
    public async Task LoadFromEnum_lists_members_in_enum_order_not_alphabetically()
    {
        using var comboBox = new ComboBox();

        comboBox.LoadFromEnum<CovfefeSleepMode>();

        // CovfefeSleepMode runs Normal, StayAwake, MonitorsOn. Sorting by the description text
        // instead would put "Keep Monitors On" first, which is not how the modes are meant to read.
        // Asserted position by position: IsEquivalentTo ignores order, so it cannot pin this.
        var entries = (KeyValuePair<CovfefeSleepMode, string>[]) comboBox.DataSource!;
        await Assert.That(entries.Length).IsEqualTo(3);
        await Assert.That(entries[0].Key).IsEqualTo(CovfefeSleepMode.Normal);
        await Assert.That(entries[1].Key).IsEqualTo(CovfefeSleepMode.StayAwake);
        await Assert.That(entries[2].Key).IsEqualTo(CovfefeSleepMode.MonitorsOn);
        await Assert.That(entries[0].Value).IsEqualTo("Normal");
    }

    [Test]
    [TestExecutor<STAThreadExecutor>]
    public async Task LoadFromEnum_omits_members_the_filter_rejects()
    {
        using var comboBox = new ComboBox();

        comboBox.LoadFromEnum<CovfefeSleepMode>(mode => mode != CovfefeSleepMode.Normal);

        var entries = (KeyValuePair<CovfefeSleepMode, string>[]) comboBox.DataSource!;
        await Assert.That(entries.Select(e => e.Key))
            .IsEquivalentTo(new[] { CovfefeSleepMode.StayAwake, CovfefeSleepMode.MonitorsOn });
    }

    /// <summary>
    /// The helper only makes sense for an enum, so the type parameter is constrained to one.
    /// A wrong type argument is then a compile error rather than an empty combo box at runtime,
    /// which is not something a test can assert directly — so it asserts the constraint instead.
    /// </summary>
    [Test]
    public async Task LoadFromEnum_is_constrained_to_enum_type_arguments()
    {
        var overloads = typeof(ControlExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name == nameof(ControlExtensions.LoadFromEnum))
            .ToArray();

        await Assert.That(overloads).IsNotEmpty();

        foreach (var overload in overloads)
        {
            var typeParameter = overload.GetGenericArguments().Single();

            await Assert.That(typeParameter.GetGenericParameterConstraints()).Contains(typeof(Enum));
            await Assert.That(typeParameter.GenericParameterAttributes
                    .HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                .IsTrue();
        }
    }
}
