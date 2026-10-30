using Sudoku.Application.Settings;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Tests.Theming;

public class PalettesTests
{
    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void For_ReturnsPaletteOfRequestedKind(ThemeKind kind)
    {
        Assert.Equal(kind, Palettes.For(kind).Kind);
    }

    [Fact]
    public void For_UnknownKind_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Palettes.For((ThemeKind)999));
    }

    [Fact]
    public void LightAndDark_AreDifferentPalettes()
    {
        Assert.NotEqual(Palettes.Light.Background.ToArgb(), Palettes.Dark.Background.ToArgb());
        Assert.NotEqual(Palettes.Light.BoardBackground.ToArgb(), Palettes.Dark.BoardBackground.ToArgb());
    }

    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void EveryColor_IsDefinedAndOpaque(ThemeKind kind)
    {
        var theme = Palettes.For(kind);
        var colorProperties = typeof(Theme).GetProperties().Where(p => p.PropertyType == typeof(Color)).ToList();

        Assert.NotEmpty(colorProperties);
        foreach (var property in colorProperties)
        {
            var color = (Color)property.GetValue(theme)!;
            Assert.True(!color.IsEmpty && color.A == 255, $"{property.Name} no está definido o no es opaco.");
        }
    }

    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void HighlightColors_AreDistinguishable(ThemeKind kind)
    {
        var theme = Palettes.For(kind);
        var colors = new[]
        {
            theme.BoardBackground.ToArgb(),
            theme.RelatedCell.ToArgb(),
            theme.MatchingValue.ToArgb(),
            theme.SelectedCell.ToArgb(),
        };

        Assert.Equal(colors.Length, colors.Distinct().Count());
    }

    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void BoardTexts_AreReadableOnBoardBackground(ThemeKind kind)
    {
        var theme = Palettes.For(kind);

        Assert.True(ColorContrast.Ratio(theme.GivenText, theme.BoardBackground) >= 4.5, "GivenText");
        Assert.True(ColorContrast.Ratio(theme.UserText, theme.BoardBackground) >= 4.5, "UserText");
        Assert.True(ColorContrast.Ratio(theme.ConflictText, theme.BoardBackground) >= 4.5, "ConflictText");
        Assert.True(ColorContrast.Ratio(theme.NoteText, theme.BoardBackground) >= 3.0, "NoteText");
    }

    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void WindowText_IsReadableOnWindowBackground(ThemeKind kind)
    {
        var theme = Palettes.For(kind);

        Assert.True(ColorContrast.Ratio(theme.Text, theme.Background) >= 4.5, "Text/Background");
        Assert.True(ColorContrast.Ratio(theme.Text, theme.Surface) >= 4.5, "Text/Surface");
        Assert.True(ColorContrast.Ratio(theme.ControlText, theme.Control) >= 4.5, "ControlText/Control");
    }
}
