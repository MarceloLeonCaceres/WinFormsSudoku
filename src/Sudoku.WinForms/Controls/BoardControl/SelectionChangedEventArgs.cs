using Domain;

namespace Sudoku.WinForms.Controls.BoardControl;

public sealed class SelectionChangedEventArgs(Position position) : EventArgs
{
    public Position Position { get; } = position;
}

