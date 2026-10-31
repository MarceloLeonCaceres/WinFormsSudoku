using Sudoku.WinForms.Services;
using Sudoku.WinForms.Theming;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;

/// <summary>
/// Cambio de tema. IThemeService.SetTheme guarda primero y cambia después: si guardar falla, el tema no cambia
/// y aquí se devuelve el combo al tema vigente.
/// </summary>
public sealed class ThemePresenter : IDisposable
{
    private readonly IGameControlsView _view;
    private readonly IThemeService _themes;
    private readonly IDialogService _dialogs;

    public ThemePresenter(IGameControlsView view, IThemeService themes, IDialogService dialogs)
    {
        _view = view;
        _themes = themes;
        _dialogs = dialogs;

        _view.ThemeChangeRequested += OnThemeChangeRequested;
        _themes.ThemeChanged += OnThemeChanged;

        _view.SetSelectedTheme(_themes.Current.Kind);
        _view.SetTheme(_themes.Current);
    }

    public void Dispose()
    {
        _view.ThemeChangeRequested -= OnThemeChangeRequested;
        _themes.ThemeChanged -= OnThemeChanged;
    }

    private void OnThemeChanged(object? sender, EventArgs e) => _view.SetTheme(_themes.Current);

    private void OnThemeChangeRequested(object? sender, EventArgs e)
    {
        try
        {
            _themes.SetTheme(_view.SelectedTheme);
        }
        catch (Exception ex) when (PersistenceErrors.IsRecoverable(ex))
        {
            _view.SetSelectedTheme(_themes.Current.Kind);
            _dialogs.ShowError("Tema", $"No se pudo guardar el tema: {ex.Message}");
        }
    }
}
