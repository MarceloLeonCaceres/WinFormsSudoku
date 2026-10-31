using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

public sealed class MemoryGameRepository : IGameRepository
{
    public SavedGame? Stored { get; set; }
    public Exception? LoadFailure { get; set; }
    public int DeleteCalls { get; private set; }

    public void Save(SavedGame game) => Stored = game;

    public SavedGame? Load() => LoadFailure is null ? Stored : throw LoadFailure;

    public void Delete()
    {
        DeleteCalls++;
        Stored = null;
    }
}
