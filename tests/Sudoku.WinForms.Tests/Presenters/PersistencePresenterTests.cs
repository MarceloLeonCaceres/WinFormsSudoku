using Sudoku.Application.Games;
using Sudoku.Application.UseCases;
using Domain;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;

public sealed class PersistencePresenterTests : IDisposable
{
    private readonly StubMainView _view = new();
    private readonly CurrentGame _current = new();
    private readonly ManualClock _clock = new();
    private readonly MemoryGameRepository _games = new();
    private readonly RecordingDialogs _dialogs = new();
    private readonly PersistencePresenter _presenter;

    public PersistencePresenterTests() =>
        _presenter = new PersistencePresenter(_view, _current, new SaveGame(_games), new LoadGame(_games, _clock), _dialogs);

    public void Dispose() => _presenter.Dispose();

    private static SavedGame SavedHardGame() =>
        new(Board.FromGivens(SampleGames.PuzzleGivens), Difficulty.Hard, TimeSpan.FromMinutes(3));

    [Fact]
    public void Save_StoresTheGame_AndMarksItSaved()
    {
        var session = SampleGames.NewSession(_clock);
        _current.Set(session);

        _view.RaiseSave();

        Assert.NotNull(_games.Stored);
        Assert.Equal(session.Board, _current.SavedBoard);
        Assert.Equal("Partida guardada", _view.LastStatus);
    }

    [Fact]
    public void Save_WithoutSession_SavesNothing()
    {
        _view.RaiseSave();

        Assert.Null(_games.Stored);
    }

    [Fact]
    public void Save_WhenFinished_SavesNothing()
    {
        _current.Set(SampleGames.NewSession(_clock));
        _current.MarkFinished();

        _view.RaiseSave();

        Assert.Null(_games.Stored);
    }

    [Fact]
    public void Continue_WithoutSavedGame_ShowsInfo()
    {
        _view.RaiseContinue();

        Assert.Single(_dialogs.Infos);
        Assert.Null(_current.Session);
    }

    [Fact]
    public void Continue_CorruptSave_ShowsErrorAndKeepsNoSession()
    {
        _games.LoadFailure = new CorruptSaveException("dañado");

        _view.RaiseContinue();

        Assert.Single(_dialogs.Errors);
        Assert.Null(_current.Session);
    }

    [Fact]
    public void Continue_LoadsARunningSession_AndUpdatesTheView()
    {
        _games.Stored = SavedHardGame();

        _view.RaiseContinue();

        var session = Assert.IsType<GameSession>(_current.Session);
        Assert.True(session.IsRunning);
        Assert.Equal(TimeSpan.FromMinutes(3), session.ElapsedTime);
        Assert.Equal(session.Board, _view.LastBoard);
        Assert.Equal(Difficulty.Hard, _view.ShownDifficulty);
        Assert.False(_view.Paused);
        Assert.Equal(session.Board, _current.SavedBoard);
    }

    [Fact]
    public void Continue_WithAGameInProgress_AsksFirst_AndKeepsItIfDeclined()
    {
        var active = SampleGames.NewSession(_clock);
        _current.Set(active);
        _games.Stored = SavedHardGame();
        _dialogs.ConfirmResult = false;

        _view.RaiseContinue();

        Assert.Equal(1, _dialogs.ConfirmCalls);
        Assert.Same(active, _current.Session);
    }
}
