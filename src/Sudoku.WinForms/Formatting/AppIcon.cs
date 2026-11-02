namespace Sudoku.WinForms.Formatting;

/// <summary>Icono de la aplicación, tomado del ejecutable (el declarado en ApplicationIcon).</summary>
public static class AppIcon
{
    private static readonly Lazy<Icon?> Instance = new(Load);

    public static Icon? Value => Instance.Value;

    private static Icon? Load()
    {
        var path = Environment.ProcessPath;
        return path is null ? null : Icon.ExtractAssociatedIcon(path);
    }
}
