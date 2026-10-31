using Domain;

namespace Sudoku.WinForms.Services;

/// <summary>Puerto del diálogo que muestra la solución de la partida (modal, solo lectura).</summary>
public interface ISolutionDialog
{
    void Show(Board solution);
}
