namespace Sudoku.WinForms.Services;

/// <summary>Puerto del temporizador de UI: permite que los presenters refresquen el tiempo sin depender de WinForms.</summary>
public interface IUiTimer
{
    event EventHandler? Tick;

    void Start();

    void Stop();
}
