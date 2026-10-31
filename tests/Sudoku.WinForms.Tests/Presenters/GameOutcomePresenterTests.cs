using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Statistics;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;

public sealed class GameOutcomePresenterTests : IDisposable
{
    private readonly StubMainView _view = new();
    private readonly CurrentGame _current = new();
    private readonly ManualClock _clock = new();
    private readonly MemoryStatisticsRepository _stats = new();
    private readonly MemoryGameRepository _games = new();
    private readonly RecordingDialogs _dialogs = new();
    private readonly GameOutcomePresenter _presenter;

    public GameOutcomePresenterTests() =>
        _presenter = new GameOutcomePresenter(
            _view, _current, new RecordGameStarted(_stats), new RecordGameFinished(_stats, _clock), _games, _dialogs);

    public void Dispose() => _presenter.Dispose();

    private DifficultyStatistics Easy => _stats.Stored.For(Difficulty.Easy);

    [Fact]
    public void NewSession_RecordsGameStarted()
    {
        _current.Set(SampleGames.NewSession(_clock));

        Assert.Equal(1, Easy.Started);
    }

    [Fact]
    public void ResumedSession_DoesNotRecordGameStarted()
    {
        _current.Set(SampleGames.NewSession(_clock), isResumed: true);

        Assert.Equal(0, Easy.Started);
    }

    [Fact]
    public void ReplacingAnUnfinishedSession_RecordsItAsLost()
    {
        _current.Set(SampleGames.NewSession(_clock));
        _current.Set(SampleGames.NewSession(_clock));

        Assert.Equal(2, Easy.Started);
        Assert.Equal(1, Easy.Lost);
    }

    [Fact]
    public void ReplacingAFinishedSession_DoesNotRecordLost()
    {
        _current.Set(SampleGames.NewSession(_clock));
        _current.MarkFinished();
        _current.Set(SampleGames.NewSession(_clock));

        Assert.Equal(0, Easy.Lost);
    }

    [Fact]
    public void ReplacingASavedSession_DeletesTheSavedCopy()
    {
        _current.Set(SampleGames.NewSession(_clock));
        _current.MarkSaved();

        _current.Set(SampleGames.NewSession(_clock));

        Assert.Equal(1, _games.DeleteCalls);
    }

    [Fact]
    public void SolvedBoard_RecordsWonOnce_StopsTheClockAndAnnounces()
    {
        var session = SampleGames.NewSession(_clock, almostSolved: true);
        _current.Set(session);
        new PlaceValue().Execute(session, SampleGames.EmptyCell, SampleGames.EmptyCellSolution);

        _current.NotifyBoardChanged();
        _current.NotifyBoardChanged();

        Assert.Equal(1, Easy.Won);
        Assert.False(session.IsRunning);
        Assert.True(_current.IsFinished);
        Assert.Single(_dialogs.Infos);
    }

    [Fact]
    public void WinningAResumedSession_DeletesTheSavedCopy()
    {
        var session = SampleGames.NewSession(_clock, almostSolved: true);
        _current.Set(session, isResumed: true);
        new PlaceValue().Execute(session, SampleGames.EmptyCell, SampleGames.EmptyCellSolution);

        _current.NotifyBoardChanged();

        Assert.Equal(1, _games.DeleteCalls);
        Assert.Null(_current.SavedBoard);
    }

    [Fact]
    public void UnsolvedBoard_DoesNothing()
    {
        _current.Set(SampleGames.NewSession(_clock));

        _current.NotifyBoardChanged();

        Assert.False(_current.IsFinished);
        Assert.Empty(_dialogs.Infos);
        Assert.Equal(0, Easy.Won);
    }

    [Fact]
    public void ClosingWithAnUnsavedGame_RecordsLost()
    {
        _current.Set(SampleGames.NewSession(_clock));

        _view.RaiseClosing();
        _view.RaiseClosing();

        Assert.Equal(1, Easy.Lost);
    }

    [Fact]
    public void ClosingWithASavedGame_DoesNotRecordLost()
    {
        _current.Set(SampleGames.NewSession(_clock));
        _current.MarkSaved();

        _view.RaiseClosing();

        Assert.Equal(0, Easy.Lost);
    }

    [Fact]
    public void StatisticsFailure_ShowsErrorWithoutThrowing()
    {
        _stats.Failure = new CorruptSaveException("dañado");

        _current.Set(SampleGames.NewSession(_clock));

        Assert.Single(_dialogs.Errors);
    }
}
