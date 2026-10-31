namespace Sudoku.WinForms.Presenters;

public sealed class SessionChangedEventArgs(bool isResumed) : EventArgs
{
    /// <summary>La sesión viene de un guardado (no es una partida nueva).</summary>
    public bool IsResumed { get; } = isResumed;
}
