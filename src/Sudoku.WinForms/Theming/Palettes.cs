using Sudoku.Application.Settings;

namespace Sudoku.WinForms.Theming;

/// <summary>Única fuente de las paletas y de la correspondencia ThemeKind → Theme.</summary>
public static class Palettes
{
    public static Theme Light { get; } = new()
    {
        Kind = ThemeKind.Light,
        Background = Hex("#F5F5F7"),
        Surface = Hex("#FFFFFF"),
        Text = Hex("#1D1D1F"),
        MutedText = Hex("#6E6E73"),
        Accent = Hex("#0A5BD3"),
        Control = Hex("#E5E5EA"),
        ControlText = Hex("#1D1D1F"),
        ControlBorder = Hex("#C7C7CC"),
        BoardBackground = Hex("#FFFFFF"),
        BoardLine = Hex("#C7C7CC"),
        BoxLine = Hex("#1D1D1F"),
        GivenText = Hex("#1D1D1F"),
        UserText = Hex("#0A5BD3"),
        SelectedCell = Hex("#A9CCFF"),
        RelatedCell = Hex("#E8EEF8"),
        MatchingValue = Hex("#CFE0FA"),
        ConflictText = Hex("#D70015"),
        NoteText = Hex("#6E6E73"),
    };

    public static Theme Dark { get; } = new()
    {
        Kind = ThemeKind.Dark,
        Background = Hex("#1E1E1E"),
        Surface = Hex("#2B2B2D"),
        Text = Hex("#E6E6E6"),
        MutedText = Hex("#9A9AA0"),
        Accent = Hex("#4C9AFF"),
        Control = Hex("#3A3A3C"),
        ControlText = Hex("#E6E6E6"),
        ControlBorder = Hex("#56565A"),
        BoardBackground = Hex("#252526"),
        BoardLine = Hex("#4A4A4F"),
        BoxLine = Hex("#D0D0D4"),
        GivenText = Hex("#F2F2F2"),
        UserText = Hex("#6CB2FF"),
        SelectedCell = Hex("#2F5C94"),
        RelatedCell = Hex("#2F3036"),
        MatchingValue = Hex("#3C4F6B"),
        ConflictText = Hex("#FF6B6B"),
        NoteText = Hex("#8E8E93"),
    };

    public static Theme For(ThemeKind kind) => kind switch
    {
        ThemeKind.Light => Light,
        ThemeKind.Dark => Dark,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Tema desconocido."),
    };

    private static Color Hex(string html) => ColorTranslator.FromHtml(html);
}
