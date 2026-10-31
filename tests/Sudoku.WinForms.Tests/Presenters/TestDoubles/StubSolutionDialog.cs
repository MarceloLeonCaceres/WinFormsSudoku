using Domain;
using Sudoku.WinForms.Services;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

public sealed class StubSolutionDialog : ISolutionDialog
{
    public Board? Shown { get; private set; }

    /// <summary>Se ejecuta mientras el "diálogo" está abierto (para observar el estado de la sesión).</summary>
    public Action? WhileOpen { get; set; }

    public void Show(Board solution)
    {
        Shown = solution;
        WhileOpen?.Invoke();
    }
}
