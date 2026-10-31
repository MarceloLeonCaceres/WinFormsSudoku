namespace Sudoku.WinForms.Services;

/// <summary>Puerto de diálogos: permite que los presenters muestren mensajes sin depender de WinForms.</summary>
public interface IDialogService
{
    void ShowError(string title, string message);
    void ShowInfo(string title, string message);

    /// <summary>Pregunta sí/no. Devuelve <c>true</c> si el usuario acepta.</summary>
    bool Confirm(string title, string message);
}
