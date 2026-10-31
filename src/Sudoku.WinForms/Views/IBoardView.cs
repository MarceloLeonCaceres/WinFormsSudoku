using Domain;
using Sudoku.WinForms.Controls.BoardControl;

namespace Sudoku.WinForms.Views;

/// <summary>Contrato mínimo del tablero para su presenter. No expone controles WinForms.</summary>
public interface IBoardView
{
    bool NotesMode { get; }

    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    event EventHandler<ValueEnteredEventArgs>? ValueEntered;
    event EventHandler? ClearRequested;

    void SetBoard(Board board);
}
