using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Tests.TestDoubles;
using Sudoku.Application.UseCases;
using Xunit;
using Application.Tests.Unit.TestData;

namespace Sudoku.Application.Tests.UseCases;

public sealed class SaveGameTests
{
    private readonly FakeClock _clock = new();
    private readonly InMemoryGameRepository _repository = new();

    [Fact]
    public void Execute_SavesBoardDifficultyAndElapsedTime()
    {
        var session = new GameSession(SamplePuzzle.Create(Difficulty.Medium), _clock);
        session.Apply(session.Board.WithValue(new Position(0, 2), 4));
        session.Start();
        _clock.Advance(TimeSpan.FromSeconds(90));

        new SaveGame(_repository).Execute(session);

        Assert.NotNull(_repository.Stored);
        Assert.Equal(session.Board, _repository.Stored.Board);
        Assert.Equal(Difficulty.Medium, _repository.Stored.Difficulty);
        Assert.Equal(TimeSpan.FromSeconds(90), _repository.Stored.Elapsed);
    }

    [Fact]
    public void Execute_DoesNotPauseOrChangeTheSession()
    {
        var session = new GameSession(SamplePuzzle.Create(), _clock);
        session.Start();

        new SaveGame(_repository).Execute(session);

        Assert.True(session.IsRunning);
    }

    [Fact]
    public void Execute_WithNullSession_Throws()
        => Assert.Throws<ArgumentNullException>(() => new SaveGame(_repository).Execute(null!));
}
