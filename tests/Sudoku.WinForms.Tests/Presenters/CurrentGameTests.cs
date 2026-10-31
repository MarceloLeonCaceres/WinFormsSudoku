using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;

public sealed class CurrentGameTests
{
    private readonly ManualClock _clock = new();

    [Fact]
    public void Set_RaisesReplacingOnlyWhenThereWasASession()
    {
        var current = new CurrentGame();
        var replacing = 0;
        current.SessionReplacing += (_, _) => replacing++;

        current.Set(SampleGames.NewSession(_clock));
        Assert.Equal(0, replacing);

        current.Set(SampleGames.NewSession(_clock));
        Assert.Equal(1, replacing);
    }

    [Fact]
    public void Replacing_HandlersStillSeeThePreviousSession()
    {
        var current = new CurrentGame();
        var first = SampleGames.NewSession(_clock);
        current.Set(first);
        object? seen = null;
        current.SessionReplacing += (_, _) => seen = current.Session;

        current.Set(SampleGames.NewSession(_clock));

        Assert.Same(first, seen);
    }

    [Fact]
    public void Set_ResetsFinishedAndSavedBoard()
    {
        var current = new CurrentGame();
        current.Set(SampleGames.NewSession(_clock));
        current.MarkSaved();
        current.MarkFinished();

        current.Set(SampleGames.NewSession(_clock));

        Assert.False(current.IsFinished);
        Assert.Null(current.SavedBoard);
    }

    [Fact]
    public void Set_Resumed_MarksTheSessionAsSaved_AndFlagsTheEvent()
    {
        var current = new CurrentGame();
        bool? resumed = null;
        current.SessionChanged += (_, e) => resumed = e.IsResumed;
        var session = SampleGames.NewSession(_clock);

        current.Set(session, isResumed: true);

        Assert.True(resumed);
        Assert.Equal(session.Board, current.SavedBoard);
    }

    [Fact]
    public void MarkSaved_StoresTheCurrentBoard()
    {
        var current = new CurrentGame();
        var session = SampleGames.NewSession(_clock);
        current.Set(session);

        current.MarkSaved();

        Assert.Equal(session.Board, current.SavedBoard);
    }

    [Fact]
    public void NotifyBoardChanged_RaisesTheEvent()
    {
        var current = new CurrentGame();
        var raised = 0;
        current.BoardChanged += (_, _) => raised++;

        current.NotifyBoardChanged();

        Assert.Equal(1, raised);
    }
}
