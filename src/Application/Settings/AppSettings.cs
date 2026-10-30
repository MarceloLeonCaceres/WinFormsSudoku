namespace Sudoku.Application.Settings;

/// <summary>Preferencias persistidas de la aplicación.</summary>
public sealed record AppSettings(ThemeKind Theme)
{
    public static AppSettings Default { get; } = new(ThemeKind.Light);
}
