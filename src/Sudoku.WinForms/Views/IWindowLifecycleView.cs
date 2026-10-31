namespace Sudoku.WinForms.Views;

/// <summary>Contrato del ciclo de vida de la ventana (ISP: separado de los controles de juego).</summary>
public interface IWindowLifecycleView
{
    /// <summary>La ventana se está cerrando (se dispara desde FormClosing).</summary>
    event EventHandler? WindowClosing;
}
