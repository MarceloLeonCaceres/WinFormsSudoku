using Domain;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;

public sealed class GameplayPresenterTests : IDisposable
{
    private readonly StubMainView _view = new();
    private readonly CurrentGame _current = new();
    private readonly ManualClock _clock = new();
    private readonly RecordingDialogs _dialogs = new();
    private readonly StubSolver _solver = new(SampleGames.SolutionBoard);
    private readonly GameplayPresenter _presenter;

    public GameplayPresenterTests() =>
        _presenter = new GameplayPresenter(
            _view, _current, new Undo(), new Redo(), new ApplyHint(new GetHint(_solver), new PlaceValue()), _dialogs);

    public void Dispose() => _presenter.Dispose();

    [Fact]
    public void Undo_RevertsTheLastMove_AndUpdatesButtons()
    {
        var session = SampleGames.NewSession(_clock);
        _current.Set(session);
        var original = session.Board;
        new PlaceValue().Execute(session, SampleGames.EmptyCell, 9);
        _current.NotifyBoardChanged();
        Assert.Equal((true, false), _view.UndoRedo);

        _view.RaiseUndo();

        Assert.Equal(original, session.Board);
        Assert.Equal(original, _view.LastBoard);
        Assert.Equal((false, true), _view.UndoRedo);
    }

    [Fact]
    public void Redo_ReappliesTheUndoneMove()
    {
        var session = SampleGames.NewSession(_clock);
        _current.Set(session);
        new PlaceValue().Execute(session, SampleGames.EmptyCell, 9);
        _view.RaiseUndo();

        _view.RaiseRedo();

        Assert.Equal(9, session.Board[SampleGames.EmptyCell].Value);
        Assert.Equal((true, false), _view.UndoRedo);
    }

    [Fact]
    public void Undo_WhenPaused_IsIgnored()
    {
        var session = SampleGames.NewSession(_clock);
        _current.Set(session);
        new PlaceValue().Execute(session, SampleGames.EmptyCell, 9);
        session.Pause();

        _view.RaiseUndo();

        Assert.Equal(9, session.Board[SampleGames.EmptyCell].Value);
    }

    [Fact]
    public void Hint_PlacesTheSolutionValueInTheSelectedCell()
    {
        var session = SampleGames.NewSession(_clock);
        _current.Set(session);
        _view.RaiseSelection(SampleGames.EmptyCell);

        _view.RaiseHint();

        Assert.Equal(SampleGames.EmptyCellSolution, session.Board[SampleGames.EmptyCell].Value);
        Assert.Equal(session.Board, _view.LastBoard);
    }

    [Fact]
    public void Hint_WhenGivensHaveNoSolution_ShowsError()
    {
        var presenter = new GameplayPresenter(
            _view, _current, new Undo(), new Redo(), new ApplyHint(new GetHint(new StubSolver(null)), new PlaceValue()), _dialogs);
        _current.Set(SampleGames.NewSession(_clock));

        _view.RaiseHint();

        Assert.Single(_dialogs.Errors);
        presenter.Dispose();
    }

    [Fact]
    public void Hint_WhenFinished_IsIgnored()
    {
        var session = SampleGames.NewSession(_clock);
        _current.Set(session);
        _current.MarkFinished();

        _view.RaiseHint();

        Assert.Null(_view.LastBoard);
    }
}
