using Domain;
using Sudoku.Application.Ports;
using Sudoku.Application.Puzzles;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

internal sealed class FakeGenerator(Func<Difficulty, CancellationToken, Puzzle> generate) : IPuzzleGenerator
{
    private int _calls;
    public int CallCount => _calls;

    public Puzzle Generate(Difficulty difficulty, CancellationToken ct = default)
    {
        Interlocked.Increment(ref _calls);
        return generate(difficulty, ct);
    }
}
