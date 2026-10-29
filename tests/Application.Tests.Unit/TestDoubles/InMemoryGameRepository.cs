using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Sudoku.Application.Tests.TestDoubles;

public sealed class InMemoryGameRepository : IGameRepository
{
    public SavedGame? Stored { get; set; }

    /// <summary>Si se asigna, <see cref = "Load"/> la lanza (simula un guardado dañado).</summary>
    public Exception? LoadFailure { get; set; }

    public void Save(SavedGame game) => Stored = game;

    public SavedGame? Load() => LoadFailure is null ? Stored : throw LoadFailure;

    public void Delete() => Stored = null;
}
