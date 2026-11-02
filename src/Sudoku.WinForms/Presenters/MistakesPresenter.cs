using Sudoku.Application.Games;
using Sudoku.Application.Settings;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;

/// <summary>
/// Aviso de errores opcional. Con la opción activa envía al tablero las celdas que difieren de la solución;
/// con la opción apagada el tablero solo marca los duplicados visibles. Recalcula al cambiar la sesión,
/// el tablero o la opción, y persiste la opción con <see cref="ChangeShowMistakes"/>.
/// </summary>
public sealed class MistakesPresenter : IDisposable
{
    private readonly IBoardView _board;
    private readonly IGameControlsView _controls;
    private readonly CurrentGame _current;
    private readonly GetMistakes _getMistakes;
    private readonly ChangeShowMistakes _changeShowMistakes;
    private readonly IDialogService _dialogs;
    private bool _enabled;

    public MistakesPresenter(
        IBoardView board,
        IGameControlsView controls,
        CurrentGame current,
        GetMistakes getMistakes,
        GetSettings getSettings,
        ChangeShowMistakes changeShowMistakes,
        IDialogService dialogs)
    {
        _board = board;
        _controls = controls;
        _current = current;
        _getMistakes = getMistakes;
        _changeShowMistakes = changeShowMistakes;
        _dialogs = dialogs;

        _enabled = LoadInitialOption(getSettings);
        _controls.SetShowMistakes(_enabled);

        _controls.ShowMistakesChanged += OnShowMistakesChanged;
        _current.SessionChanged += OnSessionChanged;
        _current.BoardChanged += OnBoardChanged;
    }

    public void Dispose()
    {
        _controls.ShowMistakesChanged -= OnShowMistakesChanged;
        _current.SessionChanged -= OnSessionChanged;
        _current.BoardChanged -= OnBoardChanged;
    }

    // Un archivo de ajustes dañado ya se avisó al resolver el tema inicial: aquí se usa el valor por defecto.
    private static bool LoadInitialOption(GetSettings getSettings)
    {
        try
        {
            return getSettings.Execute().ShowMistakes;
        }
        catch (CorruptSaveException)
        {
            return AppSettings.Default.ShowMistakes;
        }
    }

    private void OnSessionChanged(object? sender, SessionChangedEventArgs e) => Refresh();

    private void OnBoardChanged(object? sender, EventArgs e) => Refresh();

    private void OnShowMistakesChanged(object? sender, EventArgs e)
    {
        var requested = _controls.ShowMistakes;
        if (requested == _enabled)
            return;

        try
        {
            _changeShowMistakes.Execute(requested);
        }
        catch (Exception ex) when (PersistenceErrors.IsRecoverable(ex))
        {
            _controls.SetShowMistakes(_enabled);
            _dialogs.ShowError("Errores", $"No se pudo guardar la preferencia: {ex.Message}");
            return;
        }

        _enabled = requested;
        Refresh();
    }

    private void Refresh()
    {
        if (!_enabled || _current.Session is not { } session)
        {
            _board.SetMistakes(null);
            return;
        }

        try
        {
            _board.SetMistakes(_getMistakes.Execute(session));
        }
        catch (InvalidOperationException)
        {
            _board.SetMistakes(null);
            _controls.SetStatus("No se pudieron comprobar los errores: las celdas dadas no tienen solución.");
        }
    }
}
