using Sudoku.Application.Settings;

namespace Sudoku.WinForms.Theming;

/// <summary>Expone el tema actual y avisa cuando cambia. Úsalo solo desde el hilo de UI.</summary>
public interface IThemeService
{
    Theme Current { get; }

    /// <summary>Se dispara después de cambiar <see cref="Current"/>.</summary>
    event EventHandler? ThemeChanged;

    /// <summary>
    /// Guarda y aplica el tema. Devuelve <c>false</c> si ya era ese tema.
    /// Si guardar falla (p. ej. <see cref="Sudoku.Application.Games.CorruptSaveException"/>), la excepción
    /// se propaga y el tema no cambia: el presenter decide qué mostrar.
    /// </summary>
    bool SetTheme(ThemeKind kind);
}
