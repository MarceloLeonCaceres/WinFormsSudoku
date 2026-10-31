namespace Sudoku.WinForms.Views;

/// <summary>Vista principal: composición del tablero y los controles de juego. Solo la usa MainPresenter.</summary>
public interface IMainView : IBoardView, IGameControlsView, IWindowLifecycleView
{
}
