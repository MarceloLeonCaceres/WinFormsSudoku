using Domain;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

internal sealed class FakeBoardView : IBoardView
{
    public bool NotesMode { get; set; }
    public Board? LastBoard { get; private set; }
    public int SetBoardCalls { get; private set; }

    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<ValueEnteredEventArgs>? ValueEntered;
    public event EventHandler? ClearRequested;

    public void SetBoard(Board board)
    {
        LastBoard = board;
        SetBoardCalls++;
    }

    public void RaiseSelection(Position position) => SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(position));
    public void RaiseValueEntered(int value) => ValueEntered?.Invoke(this, new ValueEnteredEventArgs(value));
    public void RaiseClear() => ClearRequested?.Invoke(this, EventArgs.Empty);

    public void SetMistakes(IReadOnlySet<Position>? mistakes)
    {
        throw new NotImplementedException();
    }
}
