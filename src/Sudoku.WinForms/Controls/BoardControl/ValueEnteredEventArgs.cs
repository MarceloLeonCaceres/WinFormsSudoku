namespace Sudoku.WinForms.Controls.BoardControl;

public sealed class ValueEnteredEventArgs(int value) : EventArgs
{
    public int Value { get; } = value;
}

