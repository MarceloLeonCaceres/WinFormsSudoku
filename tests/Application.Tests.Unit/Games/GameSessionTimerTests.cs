using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Tests.TestDoubles;
using Application.Tests.Unit.TestData;
using Xunit;

namespace Application.Tests.Games;

public sealed class GameSessionTimerTests
{
    private readonly FakeClock _clock = new();

    private GameSession NewSession() => new(SamplePuzzle.Create(), _clock);

    [Fact]
    public void NewSession_StartsPausedAtZero()
    {
        var session = NewSession();

        Assert.False(session.IsRunning);
        Assert.Equal(TimeSpan.Zero, session.ElapsedTime);
    }

    [Fact]
    public void ElapsedTime_DoesNotGrowWhilePaused()
    {
        var session = NewSession();

        _clock.Advance(TimeSpan.FromMinutes(10));

        Assert.Equal(TimeSpan.Zero, session.ElapsedTime);
    }

    [Fact]
    public void Start_CountsElapsedTime()
    {
        var session = NewSession();

        session.Start();
        _clock.Advance(TimeSpan.FromSeconds(90));

        Assert.True(session.IsRunning);
        Assert.Equal(TimeSpan.FromSeconds(90), session.ElapsedTime);
    }

    [Fact]
    public void Pause_StopsCounting()
    {
        var session = NewSession();
        session.Start();
        _clock.Advance(TimeSpan.FromSeconds(30));

        session.Pause();
        _clock.Advance(TimeSpan.FromMinutes(5));

        Assert.False(session.IsRunning);
        Assert.Equal(TimeSpan.FromSeconds(30), session.ElapsedTime);
    }

    [Fact]
    public void Start_WhenAlreadyRunning_DoesNotResetTheLeg()
    {
        var session = NewSession();
        session.Start();
        _clock.Advance(TimeSpan.FromSeconds(20));

        session.Start();
        _clock.Advance(TimeSpan.FromSeconds(20));

        Assert.Equal(TimeSpan.FromSeconds(40), session.ElapsedTime);
    }

    [Fact]
    public void Pause_WhenAlreadyPaused_KeepsTheAccumulatedTime()
    {
        var session = NewSession();
        session.Start();
        _clock.Advance(TimeSpan.FromSeconds(15));
        session.Pause();

        session.Pause();

        Assert.Equal(TimeSpan.FromSeconds(15), session.ElapsedTime);
    }

    [Fact]
    public void Resume_AccumulatesBothLegs()
    {
        var session = NewSession();
        session.Start();
        _clock.Advance(TimeSpan.FromSeconds(10));
        session.Pause();
        _clock.Advance(TimeSpan.FromMinutes(1));

        session.Start();
        _clock.Advance(TimeSpan.FromSeconds(5));

        Assert.Equal(TimeSpan.FromSeconds(15), session.ElapsedTime);
    }

    [Fact]
    public void Restore_KeepsBoardDifficultyAndElapsed_PausedWithEmptyHistory()
    {
        var board = SamplePuzzle.Create().Board
            .WithValue(new Position(0, 2), 4)
            .WithNotes(new Position(0, 3), CandidateSet.Of(6, 8));
        var saved = new SavedGame(board, Difficulty.Hard, TimeSpan.FromMinutes(5));

        var session = GameSession.Restore(saved, _clock);

        Assert.Equal(board, session.Board);
        Assert.Equal(Difficulty.Hard, session.Difficulty);
        Assert.Equal(TimeSpan.FromMinutes(5), session.ElapsedTime);
        Assert.False(session.IsRunning);
        Assert.False(session.CanUndo);
        Assert.False(session.CanRedo);
    }

    [Fact]
    public void Restore_ThenStart_ContinuesFromSavedTime()
    {
        var saved = new SavedGame(SamplePuzzle.Create().Board, Difficulty.Easy, TimeSpan.FromMinutes(5));
        var session = GameSession.Restore(saved, _clock);

        session.Start();
        _clock.Advance(TimeSpan.FromSeconds(30));

        Assert.Equal(TimeSpan.FromSeconds(330), session.ElapsedTime);
    }

    [Fact]
    public void Restore_WithNull_Throws()
        => Assert.Throws<ArgumentNullException>(() => GameSession.Restore(null!, _clock));
}
