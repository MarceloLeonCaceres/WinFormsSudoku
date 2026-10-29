namespace Sudoku.Application.Ports;

/// <summary>Puerto del reloj. Permite probar tiempos sin depender de la hora real.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
