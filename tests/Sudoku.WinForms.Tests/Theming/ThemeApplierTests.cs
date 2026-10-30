using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Tests.Theming;

public class ThemeApplierTests
{
    [Fact]
    public void Apply_PaintsControlsRecursivelyWithTheme()
    {
        var theme = Palettes.Dark;
        using var root = new Panel();
        using var nested = new Panel();
        using var button = new Button();
        using var label = new Label();
        root.Controls.Add(nested);
        nested.Controls.Add(button);
        nested.Controls.Add(label);

        ThemeApplier.Apply(theme, root);

        Assert.Equal(theme.Surface.ToArgb(), root.BackColor.ToArgb());
        Assert.Equal(theme.Surface.ToArgb(), nested.BackColor.ToArgb());
        Assert.Equal(theme.Control.ToArgb(), button.BackColor.ToArgb());
        Assert.Equal(theme.ControlText.ToArgb(), button.ForeColor.ToArgb());
        Assert.Equal(FlatStyle.Flat, button.FlatStyle);
        Assert.Equal(theme.Text.ToArgb(), label.ForeColor.ToArgb());
    }

    [Fact]
    public void Apply_SwitchingThemes_RepaintsControls()
    {
        using var panel = new Panel();

        ThemeApplier.Apply(Palettes.Light, panel);
        var light = panel.BackColor.ToArgb();
        ThemeApplier.Apply(Palettes.Dark, panel);

        Assert.NotEqual(light, panel.BackColor.ToArgb());
    }

    [Fact]
    public void Apply_NullArguments_Throw()
    {
        using var panel = new Panel();

        Assert.Throws<ArgumentNullException>(() => ThemeApplier.Apply(null!, panel));
        Assert.Throws<ArgumentNullException>(() => ThemeApplier.Apply(Palettes.Light, null!));
    }
}
