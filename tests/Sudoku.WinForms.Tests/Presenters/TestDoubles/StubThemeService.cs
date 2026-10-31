using Sudoku.Application.Settings;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

public sealed class StubThemeService : IThemeService
{
    public Theme Current { get; private set; } = Palettes.Dark;

    /// <summary>Si se asigna, <see cref="SetTheme"/> la lanza (simula un fallo al guardar).</summary>
    public Exception? Failure { get; set; }

    public event EventHandler? ThemeChanged;

    public bool SetTheme(ThemeKind kind)
    {
        if (Failure is not null)
            throw Failure;
        if (kind == Current.Kind)
            return false;

        Current = Palettes.For(kind);
        ThemeChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }
}
