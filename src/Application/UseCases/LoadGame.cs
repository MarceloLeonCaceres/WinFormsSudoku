using global::Sudoku.Application.Games;
using global::Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;

/// <summary>
/// Reanuda la partida guardada. Devuelve <c>null</c> si no hay ninguna; la sesión vuelve en pausa y con el historial vacío.
/// </summary>
/// <exception cref = "CorruptSaveException">El guardado está dañado o es de una versión desconocida.</exception>
public sealed class LoadGame(IGameRepository repository, IClock clock)
{
    public GameSession? Execute()
        => repository.Load() is { } saved ? GameSession.Restore(saved, clock) : null;
}
