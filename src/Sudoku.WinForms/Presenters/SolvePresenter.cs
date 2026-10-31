using Sudoku.Application.UseCases;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;

/// <summary>
/// «Resolver»: muestra la solución en un diálogo modal. No toca el tablero de la sesión ni las estadísticas
/// (decisión del 2026-10-30). Detiene el reloj mientras el diálogo está abierto.
/// </summary>
public sealed class SolvePresenter : IDisposable
{
    private readonly IGameControlsView _view;
    private readonly CurrentGame _current;
    private readonly SolveGame _solveGame;
    private readonly ISolutionDialog _dialog;
    private readonly IDialogService _dialogs;

    public SolvePresenter(IGameControlsView view, CurrentGame current, SolveGame solveGame, ISolutionDialog dialog, IDialogService dialogs)
    {
        _view = view;
        _current = current;
        _solveGame = solveGame;
        _dialog = dialog;
        _dialogs = dialogs;

        _view.SolveRequested += OnSolveRequested;
    }

    public void Dispose() => _view.SolveRequested -= OnSolveRequested;

    private void OnSolveRequested(object? sender, EventArgs e)
    {
        if (_current.Session is not { } session || _current.IsFinished)
        {
            _view.SetStatus("No hay ninguna partida en curso.");
            return;
        }

        var wasRunning = session.IsRunning;
        session.Pause();
        try
        {
            _dialog.Show(_solveGame.Execute(session));
        }
        catch (InvalidOperationException ex)
        {
            _dialogs.ShowError("Resolver", ex.Message);
        }
        finally
        {
            if (wasRunning)
                session.Start();
        }
    }
}
