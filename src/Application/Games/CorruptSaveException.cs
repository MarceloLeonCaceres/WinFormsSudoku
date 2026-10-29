namespace Sudoku.Application.Games;

/// <summary>El archivo de partida existe pero no se puede interpretar (dañado, incompleto o de una versión desconocida).</summary>
public sealed class CorruptSaveException : Exception
{
    public CorruptSaveException(string message) : base(message)
    {
    }

    public CorruptSaveException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
