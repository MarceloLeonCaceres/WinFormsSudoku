using Application.Tests.Unit.TestData;
using Domain;
using global::Sudoku.Application.Ports;
using global::Sudoku.Application.Puzzles;
using global::Sudoku.Application.UseCases;
using Sudoku.Application.Tests.TestDoubles;

namespace Application.Tests.Unit;

public class StartNewGameTests
{
    private sealed class StubPuzzleGenerator(Puzzle puzzle) : IPuzzleGenerator
    {
        public Difficulty? RequestedDifficulty { get; private set; }
        public CancellationToken ReceivedToken { get; private set; }

        public Puzzle Generate(Difficulty difficulty, CancellationToken ct = default)
        {
            RequestedDifficulty = difficulty;
            ReceivedToken = ct;
            return puzzle;
        }
    }

    [Fact]
    public void CreatesSessionFromGeneratedPuzzle()
    {
        var puzzle = SamplePuzzle.Create(Difficulty.Expert);
        var useCase = new StartNewGame(new StubPuzzleGenerator(puzzle), new FakeClock());

        var session = useCase.Execute(Difficulty.Expert);

        Assert.Equal(puzzle.Board, session.Board);
        Assert.Equal(Difficulty.Expert, session.Difficulty);
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void PassesDifficultyAndCancellationTokenToGenerator()
    {
        var generator = new StubPuzzleGenerator(SamplePuzzle.Create(Difficulty.Medium));
        using var cts = new CancellationTokenSource();

        new StartNewGame(generator, new FakeClock()).Execute(Difficulty.Medium, cts.Token);

        Assert.Equal(Difficulty.Medium, generator.RequestedDifficulty);
        Assert.Equal(cts.Token, generator.ReceivedToken);
    }
}
