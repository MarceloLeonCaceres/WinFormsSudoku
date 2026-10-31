using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;

public sealed class SolvePresenterTests
{
    private readonly StubMainView _view = new();
    private readonly CurrentGame _current = new();
    private readonly ManualClock _clock = new();
    private readonly StubSolutionDialog _dialog = new();
    private readonly RecordingDialogs _dialogs = new();

    private SolvePresenter Create(StubSolver solver) =>
        new(_view, _current, new SolveGame(solver), _dialog, _dialogs);

    [Fact]
    public void Solve_ShowsTheSolution_PausesWhileOpen_AndResumesAfterwards()
    {
        using var presenter = Create(new StubSolver(SampleGames.SolutionBoard));
        var session = SampleGames.NewSession(_clock);
        _current.Set(session);
        var runningWhileOpen = true;
        _dialog.WhileOpen = () => runningWhileOpen = session.IsRunning;
        var before = session.Board;

        _view.RaiseSolve();

        Assert.Equal(SampleGames.SolutionBoard, _dialog.Shown);
        Assert.False(runningWhileOpen);
        Assert.True(session.IsRunning);
        Assert.Same(before, session.Board);
    }

    [Fact]
    public void Solve_OnAPausedGame_KeepsItPaused()
    {
        using var presenter = Create(new StubSolver(SampleGames.SolutionBoard));
        var session = SampleGames.NewSession(_clock, started: false);
        _current.Set(session);

        _view.RaiseSolve();

        Assert.False(session.IsRunning);
    }

    [Fact]
    public void Solve_WithoutSolution_ShowsError_AndResumes()
    {
        using var presenter = Create(new StubSolver(null));
        var session = SampleGames.NewSession(_clock);
        _current.Set(session);

        _view.RaiseSolve();

        Assert.Single(_dialogs.Errors);
        Assert.Null(_dialog.Shown);
        Assert.True(session.IsRunning);
    }

    [Fact]
    public void Solve_WithoutSession_OnlyUpdatesStatus()
    {
        using var presenter = Create(new StubSolver(SampleGames.SolutionBoard));

        _view.RaiseSolve();

        Assert.Null(_dialog.Shown);
        Assert.NotNull(_view.LastStatus);
    }
}
