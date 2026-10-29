using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Tests.TestDoubles;
using Sudoku.Application.UseCases;
using Xunit;
using Application.Tests.Unit.TestData;

namespace Sudoku.Application.Tests.UseCases;

public sealed class LoadGameTests
{
    private readonly FakeClock _clock = new();
    private readonly InMemoryGameRepository _repository = new();

    private LoadGame NewUseCase() => new(_repository, _clock);

    [Fact]
    public void Execute_WithoutSavedGame_ReturnsNull()
        => Assert.Null(NewUseCase().Execute());

    [Fact]
    public void Execute_WithSavedGame_ReturnsPausedSessionWithoutHistory()
    {
        var board = SamplePuzzle.Create().Board.WithValue(new Position(0, 2), 4);
        _repository.Stored = new SavedGame(board, Difficulty.Expert, TimeSpan.FromMinutes(3));

        var session = NewUseCase().Execute();

        Assert.NotNull(session);
        Assert.Equal(board, session.Board);
        Assert.Equal(Difficulty.Expert, session.Difficulty);
        Assert.Equal(TimeSpan.FromMinutes(3), session.ElapsedTime);
        Assert.False(session.IsRunning);
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void Execute_WithCorruptSave_PropagatesTheException()
    {
        _repository.LoadFailure = new CorruptSaveException("dañado");

        Assert.Throws<CorruptSaveException>(() => NewUseCase().Execute());
    }

    [Fact]
    public void SaveThenLoad_ReturnsEquivalentSession()
    {
        var original = new GameSession(SamplePuzzle.Create(Difficulty.Hard), _clock);
        original.Apply(original.Board.WithNotes(new Position(0, 3), CandidateSet.Of(6, 8)));
        original.Start();
        _clock.Advance(TimeSpan.FromSeconds(42));
        new SaveGame(_repository).Execute(original);

        var restored = NewUseCase().Execute();

        Assert.NotNull(restored);
        Assert.Equal(original.Board, restored.Board);
        Assert.Equal(original.Difficulty, restored.Difficulty);
        Assert.Equal(original.ElapsedTime, restored.ElapsedTime);
    }
}
