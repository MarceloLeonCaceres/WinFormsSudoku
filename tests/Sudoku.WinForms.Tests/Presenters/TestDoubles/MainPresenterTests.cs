using Domain;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;

public sealed class MainPresenterTests
{
    private readonly FakeMainView _view = new();
    private readonly CurrentGame _current = new();
    private readonly RecordingDialogService _dialogs = new();

    private MainPresenter CreatePresenter(FakeGenerator generator) =>
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
}
