using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;
using Sudoku.Application.UseCases;
using Domain;
using Sudoku.WinForms.Formatting;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;

/// <summary>
/// Resultado de las partidas: cuenta la partida al empezar, registra victoria al resolver el tablero y registra
/// como perdida la partida que se reemplaza o se abandona al cerrar la ventana.
/// Reglas: una partida guardada ("aparcada") no se cuenta como perdida al cerrar; si se reemplaza, sí,
/// y su copia en disco se borra para que no pueda terminar dos veces.
/// </summary>
public sealed class GameOutcomePresenter : IDisposable
{
    private readonly IMainView _view;
    private readonly CurrentGame _current;
    private readonly RecordGameStarted _recordStarted;
    private readonly RecordGameFinished _recordFinished;
    private readonly IGameRepository _games;
    private readonly IDialogService _dialogs;

    public GameOutcomePresenter(
        IMainView view,
        CurrentGame current,
        RecordGameStarted recordStarted,
        RecordGameFinished recordFinished,
        IGameRepository games,
        IDialogService dialogs)
    {
        _view = view;
        _current = current;
        _recordStarted = recordStarted;
        _recordFinished = recordFinished;
        _games = games;
        _dialogs = dialogs;

        _current.SessionReplacing += OnSessionReplacing;
        _current.SessionChanged += OnSessionChanged;
        _current.BoardChanged += OnBoardChanged;
        _view.WindowClosing += OnWindowClosing;
    }

    public void Dispose()
    {
        _current.SessionReplacing -= OnSessionReplacing;
        _current.SessionChanged -= OnSessionChanged;
        _current.BoardChanged -= OnBoardChanged;
        _view.WindowClosing -= OnWindowClosing;
    }

    private void OnSessionReplacing(object? sender, EventArgs e)
    {
        if (_current.Session is not { } previous)
            return;

        if (!_current.IsFinished)
            Try(() => _recordFinished.Execute(previous, GameOutcome.Lost), "Estadísticas");

        DeleteSavedCopy();
    }

    private void OnSessionChanged(object? sender, SessionChangedEventArgs e)
    {
        if (e.IsResumed || _current.Session is not { } session)
            return;

        Try(() => _recordStarted.Execute(session.Difficulty), "Estadísticas");
    }

    private void OnBoardChanged(object? sender, EventArgs e)
    {
        if (_current.Session is not { } session || _current.IsFinished || !SudokuRules.IsSolved(session.Board))
            return;

        session.Pause();
        _current.MarkFinished();
        Try(() => _recordFinished.Execute(session, GameOutcome.Won), "Estadísticas");
        DeleteSavedCopy();

        var time = DurationFormat.Format(session.ElapsedTime);
        _view.SetElapsedTime(session.ElapsedTime);
        _view.SetStatus($"¡Resuelto en {time}!");
        _dialogs.ShowInfo("¡Felicidades!", $"Has resuelto el Sudoku en {time}.");
    }

    private void OnWindowClosing(object? sender, EventArgs e)
    {
        if (_current.Session is not { } session)
            return;

        session.Pause();
        if (_current.IsFinished || _current.SavedBoard is not null)
            return; // ya terminada, o guardada: se puede continuar luego

        _current.MarkFinished();
        Try(() => _recordFinished.Execute(session, GameOutcome.Lost), "Estadísticas");
    }

    private void DeleteSavedCopy()
    {
        if (_current.SavedBoard is null)
            return;

        Try(_games.Delete, "Partida guardada");
        _current.ClearSaved();
    }

    private void Try(Action action, string title)
    {
        try
        {
            action();
        }
        catch (Exception ex) when (PersistenceErrors.IsRecoverable(ex))
        {
            _dialogs.ShowError(title, $"No se pudo completar la operación: {ex.Message}");
        }
    }
}
