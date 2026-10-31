using System.Diagnostics.CodeAnalysis;
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;

/// <summary>
/// Deshacer, rehacer y pista. Tras cada cambio refresca el tablero y avisa con
/// <see cref="CurrentGame.NotifyBoardChanged"/> (de ahí cuelgan la detección de victoria y los botones).
/// </summary>
public sealed class GameplayPresenter : IDisposable
{
    private readonly IMainView _view;
    private readonly CurrentGame _current;
    private readonly Undo _undo;
    private readonly Redo _redo;
    private readonly ApplyHint _applyHint;
    private readonly IDialogService _dialogs;
    private Position? _selected;

    public GameplayPresenter(IMainView view, CurrentGame current, Undo undo, Redo redo, ApplyHint applyHint, IDialogService dialogs)
    {
        _view = view;
        _current = current;
        _undo = undo;
        _redo = redo;
        _applyHint = applyHint;
        _dialogs = dialogs;

        _view.UndoRequested += OnUndoRequested;
        _view.RedoRequested += OnRedoRequested;
        _view.HintRequested += OnHintRequested;
        _view.SelectionChanged += OnSelectionChanged;
        _current.SessionChanged += OnSessionChanged;
        _current.BoardChanged += OnBoardChanged;
    }

    public void Dispose()
    {
        _view.UndoRequested -= OnUndoRequested;
        _view.RedoRequested -= OnRedoRequested;
        _view.HintRequested -= OnHintRequested;
        _view.SelectionChanged -= OnSelectionChanged;
        _current.SessionChanged -= OnSessionChanged;
        _current.BoardChanged -= OnBoardChanged;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => _selected = e.Position;

    private void OnSessionChanged(object? sender, SessionChangedEventArgs e) => UpdateUndoRedoState();

    private void OnBoardChanged(object? sender, EventArgs e) => UpdateUndoRedoState();

    private void OnUndoRequested(object? sender, EventArgs e)
    {
        if (TryGetPlayable(out var session) && _undo.Execute(session))
            Refresh(session);
    }

    private void OnRedoRequested(object? sender, EventArgs e)
    {
        if (TryGetPlayable(out var session) && _redo.Execute(session))
            Refresh(session);
    }

    private void OnHintRequested(object? sender, EventArgs e)
    {
        if (!TryGetPlayable(out var session))
            return;

        try
        {
            var hint = _applyHint.Execute(session, _selected);
            if (hint is null)
            {
                _view.SetStatus("No quedan celdas vacías.");
                return;
            }

            // El estado va antes de Refresh: si la pista completa el tablero, la victoria lo sobrescribe.
            _view.SetStatus($"Pista: fila {hint.Value.Position.Row + 1}, columna {hint.Value.Position.Column + 1}");
            Refresh(session);
        }
        catch (InvalidOperationException ex)
        {
            _dialogs.ShowError("Pista", ex.Message);
        }
    }

    private bool TryGetPlayable([NotNullWhen(true)] out GameSession? session)
    {
        session = _current.Session;
        if (session is null || _current.IsFinished)
            return false;

        if (!session.IsRunning)
        {
            _view.SetStatus("La partida está en pausa.");
            return false;
        }

        return true;
    }

    private void Refresh(GameSession session)
    {
        _view.SetBoard(session.Board);
        _current.NotifyBoardChanged();
    }

    private void UpdateUndoRedoState() =>
        _view.SetUndoRedoState(_current.Session?.CanUndo ?? false, _current.Session?.CanRedo ?? false);
}
