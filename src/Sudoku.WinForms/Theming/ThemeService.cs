using Sudoku.Application.Settings;
using Sudoku.Application.UseCases;

namespace Sudoku.WinForms.Theming;

public sealed class ThemeService : IThemeService
{
    private readonly ChangeTheme _changeTheme;

    public ThemeService(ChangeTheme changeTheme, ThemeKind initial)
    {
        _changeTheme = changeTheme;
        Current = Palettes.For(initial);
    }

    public Theme Current { get; private set; }

    public event EventHandler? ThemeChanged;

    public bool SetTheme(ThemeKind kind)
    {
        if (kind == Current.Kind)
            return false;

        // Primero persistir: si falla, el estado en memoria no cambia.
        _changeTheme.Execute(kind);

        Current = Palettes.For(kind);
        ThemeChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }
}
