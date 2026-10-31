using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Puzzles;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;

public sealed class MainPresenterTests
{
    private readonly FakeMainView _view = new();
    private readonly CurrentGame _current = new();
    private readonly RecordingDialogService _dialogs = new();

    private MainPresenter CreatePresenter(IPuzzleGenerator generator) =>
        new(_view, new StartNewGame(generator, new FakeClock()), _current, _dialogs);

    [Fact]
    public async Task StartNewGame_StoresRunningSession_AndShowsBoard()
    {
        using var presenter = CreatePresenter(new FakeGenerator((d, _) => SamplePuzzle.Create(d)));

        await presenter.StartNewGameAsync();

        Assert.NotNull(_current.Session);
        Assert.True(_current.Session.IsRunning);
        Assert.Same(_current.Session.Board, _view.LastBoard);
        Assert.Equal(new[] { true, false }, _view.BusyChanges);
        Assert.Empty(_dialogs.Errors);
    }

    [Fact]
    public async Task StartNewGame_UsesSelectedDifficulty()
    {
        _view.SelectedDifficulty = Difficulty.Hard;
        using var presenter = CreatePresenter(new FakeGenerator((d, _) => SamplePuzzle.Create(d)));

        await presenter.StartNewGameAsync();

        Assert.Equal(Difficulty.Hard, _current.Session!.Difficulty);
        Assert.Equal(Difficulty.Hard, _view.ShownDifficulty);
    }

    [Fact]
    public async Task StartNewGame_WhenGeneratorFails_ShowsErrorAndKeepsNoSession()
    {
        using var presenter = CreatePresenter(
            new FakeGenerator((_, _) => throw new InvalidOperationException("sin rango")));

        await presenter.StartNewGameAsync();

        Assert.Null(_current.Session);
        Assert.Single(_dialogs.Errors);
        Assert.False(_view.IsBusy);
    }

    [Fact]
    public async Task StartNewGame_WhenCalledAgain_CancelsPreviousGeneration()
    {
        var generator = new FakeGenerator((difficulty, ct) =>
        {
            if (difficulty == Difficulty.Hard)
            {
                ct.WaitHandle.WaitOne();          // bloquea hasta que lo cancelen
                ct.ThrowIfCancellationRequested();
            }
            return SamplePuzzle.Create(difficulty);
        });
        using var presenter = CreatePresenter(generator);

        _view.SelectedDifficulty = Difficulty.Hard;
        var first = presenter.StartNewGameAsync();
        _view.SelectedDifficulty = Difficulty.Easy;
        var second = presenter.StartNewGameAsync();

        await Task.WhenAll(first, second);

        Assert.Equal(Difficulty.Easy, _current.Session!.Difficulty);
        Assert.False(_view.IsBusy);
        Assert.Empty(_dialogs.Errors);
    }
    
    [Fact]
    public async Task StartNewGame_TellsViewTheGameIsNotPaused()
    {
        using var presenter = CreatePresenter(new FakeGenerator((d, _) => SamplePuzzle.Create(d)));

        await presenter.StartNewGameAsync();

        Assert.False(_view.LastPaused);
    }

    [Fact]
    public async Task NewGameRequested_Event_StartsGeneration()
    {
        var called = new TaskCompletionSource();
        var generator = new FakeGenerator((d, _) =>
        {
            called.TrySetResult();
            return SamplePuzzle.Create(d);
        });
        using var presenter = CreatePresenter(generator);

        _view.RaiseNewGameRequested();

        await called.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AfterDispose_NewGameRequested_IsIgnored()
    {
        var generator = new FakeGenerator((d, _) => SamplePuzzle.Create(d));
        var presenter = CreatePresenter(generator);
        presenter.Dispose();

        _view.RaiseNewGameRequested();

        Assert.Equal(0, generator.CallCount);
    }
    private sealed class BlockingGenerator : IPuzzleGenerator
    {
        public ManualResetEventSlim Started { get; } = new();

        public Puzzle Generate(Difficulty difficulty, CancellationToken ct = default)
        {
            Started.Set();
            ct.WaitHandle.WaitOne();
            ct.ThrowIfCancellationRequested();
            throw new InvalidOperationException("Debió cancelarse.");
        }
    }
    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UnixEpoch;
    }

    private sealed class RecordingDialogService : IDialogService
    {
        public List<(string Title, string Message)> Errors { get; } = [];

        public bool Confirm(string title, string message)
        {
            throw new NotImplementedException();
        }

        public void ShowError(string title, string message) => Errors.Add((title, message));

        public void ShowInfo(string title, string message)
        {
            throw new NotImplementedException();
        }
    }

    private static (MainPresenter Sut, FakeMainView View, CurrentGame Current, RecordingDialogService Dialogs)
        CreateSut(IPuzzleGenerator generator)
    {
        var view = new FakeMainView();
        var current = new CurrentGame();
        var dialogs = new RecordingDialogService();
        var sut = new MainPresenter(view, new StartNewGame(generator, new FixedClock()), current, dialogs);
        return (sut, view, current, dialogs);
    }

    private static GameSession CreateRunningSession()
    {
        const string givens = "530070000600195000098000060800060003400803001700020006060000280000419005000080079";
        const string solution = "534678912672195348198342567859761423426853791713924856961537284287419635345286179";

        var puzzle = new Puzzle(Board.FromGivens(givens), Board.FromGivens(solution), Difficulty.Easy);
        var session = new GameSession(puzzle, new FakeClock());
        session.Start();
        return session;
    }
    [Fact]
    public async Task CancelRequested_WhileGenerating_ClearsBusyWithoutShowingError()
    {
        var generator = new BlockingGenerator();
        using var sut = CreatePresenter(generator);

        var generation = sut.StartNewGameAsync();
        generator.Started.Wait();
        _view.RaiseCancelRequested();
        await generation;

        Assert.False(_view.IsBusy);
        Assert.Null(_current.Session);
        Assert.Empty(_dialogs.Errors);
        Assert.Equal("Generación cancelada", _view.Statuses[^1]);
    }

    [Fact]
    public void CancelRequested_WithoutGeneration_DoesNothing()
    {
        using var sut = CreatePresenter(new BlockingGenerator());

        _view.RaiseCancelRequested();

        Assert.Empty(_view.BusyChanges);
    }

    [Fact]
    public void PauseRequested_RunningSession_PausesAndNotifiesView()
    {
        using var sut = CreatePresenter(new BlockingGenerator());
        var session = CreateRunningSession();
        _current.Set(session);

        _view.RaisePauseRequested();

        Assert.False(session.IsRunning);
        Assert.True(_view.LastPaused);
    }

    [Fact]
    public void PauseRequested_PausedSession_ResumesAndNotifiesView()
    {
        using var sut = CreatePresenter(new BlockingGenerator());
        var session = CreateRunningSession();
        session.Pause();
        _current.Set(session);

        _view.RaisePauseRequested();

        Assert.True(session.IsRunning);
        Assert.False(_view.LastPaused);
    }

    [Fact]
    public void PauseRequested_WithoutSession_IsIgnored()
    {
        using var sut = CreatePresenter(new BlockingGenerator());

        _view.RaisePauseRequested();

        Assert.Empty(_view.PausedChanges);
    }

    [Fact]
    public async Task PauseRequested_WhileGenerating_IsIgnored()
    {
        var generator = new BlockingGenerator();
        using var sut = CreatePresenter(generator);
        var session = CreateRunningSession();
        _current.Set(session);

        var generation = sut.StartNewGameAsync();
        generator.Started.Wait();
        _view.RaisePauseRequested();
        _view.RaiseCancelRequested();
        await generation;

        Assert.True(session.IsRunning);
        Assert.Empty(_view.PausedChanges);
    }

    [Fact]
    public void AfterDispose_PauseAndCancel_AreIgnored()
    {
        var sut = CreatePresenter(new BlockingGenerator());
        var session = CreateRunningSession();
        _current.Set(session);
        sut.Dispose();

        _view.RaisePauseRequested();
        _view.RaiseCancelRequested();

        Assert.True(session.IsRunning);
        Assert.Empty(_view.PausedChanges);
    }

}
