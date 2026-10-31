using Sudoku.Application.Games;

namespace Sudoku.WinForms.Services;

/// <summary>Única fuente de qué fallos de persistencia se muestran al usuario en lugar de cerrar la app.</summary>
public static class PersistenceErrors
{
    public static bool IsRecoverable(Exception exception) =>
        exception is CorruptSaveException or IOException or UnauthorizedAccessException;
}
