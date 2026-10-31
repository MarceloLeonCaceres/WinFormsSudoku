namespace Sudoku.WinForms.Services;

/// <summary>Puerto de diálogos: permite que los presenters muestren mensajes sin depender de WinForms.</summary>
public interface IDialogService
{
    void ShowError(string title, string message);
}
