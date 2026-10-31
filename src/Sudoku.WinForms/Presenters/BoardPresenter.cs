using System.Diagnostics.CodeAnalysis;
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;

/// <summary>
/// Traduce la interacción con el tablero (selección, valor, borrar) en casos de uso.
/// Decide entre colocar valor y alternar nota según <see cref="IBoardView.NotesMode"/>.
/// Ignora la entrada si no hay sesión, no hay selección o la partida está en pausa.
/// </summary>
public sealed class BoardPresenter : IDisposable
{
    private readonly IBoardView _view;
    private readonly CurrentGame _current;
    private readonly PlaceValue _placeValue;
    private readonly ToggleNote _toggleNote;
    private readonly ClearCell _clearCell;
    private Position? _selected;

    public BoardPresenter(
        IBoardView view,
        CurrentGame current,
        PlaceValue placeValue,
        ToggleNote toggleNote,
        ClearCell clearCell)
    {
        _view = view;
        _current = current;
        _placeValue = placeValue;
        _toggleNote = toggleNote;
        _clearCell = clearCell;

        _view.SelectionChanged += OnSelectionChanged;
        _view.ValueEntered += OnValueEntered;
        _view.ClearRequested += OnClearRequested;
    }

    public void Dispose()
    {
        _view.SelectionChanged -= OnSelectionChanged;
        _view.ValueEntered -= OnValueEntered;
        _view.ClearRequested -= OnClearRequested;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) => _selected = e.Position;

    private void OnValueEntered(object? sender, ValueEnteredEventArgs e)
    {
        if (!TryGetTarget(out var session, out var position))
            return;

        bool changed = _view.NotesMode
            ? _toggleNote.Execute(session, position, e.Value)
            : _placeValue.Execute(session, position, e.Value);

        if (changed)
            _view.SetBoard(session.Board);
    }

    private void OnClearRequested(object? sender, EventArgs e)
    {
        if (!TryGetTarget(out var session, out var position))
            return;

        if (_clearCell.Execute(session, position))
            _view.SetBoard(session.Board);
    }

    private bool TryGetTarget([NotNullWhen(true)] out GameSession? session, out Position position)
    {
        session = _current.Session;
        position = default;

        if (session is not { IsRunning: true } || _selected is not { } selected)
            return false;

        position = selected;
        return true;
    }
}
