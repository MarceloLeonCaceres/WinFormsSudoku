using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;

public sealed class ClockPresenterTests : IDisposable
{
    private readonly StubMainView _view = new();
    private readonly CurrentGame _current = new();
    private readonly ManualClock _clock = new();
    private readonly ManualUiTimer _timer = new();
    private readonly ClockPresenter _presenter;

    public ClockPresenterTests() => _presenter = new ClockPresenter(_view, _current, _timer);

    public void Dispose() => _presenter.Dispose();

    [Fact]
    public void StartsTheTimer()
    {
        Assert.True(_timer.IsRunning);
    }

    [Fact]
    public void Tick_ShowsTheSessionElapsedTime()
    {
        var session = SampleGames.NewSession(_clock);
        _current.Set(session);
        _clock.Advance(TimeSpan.FromSeconds(5));

        _timer.RaiseTick();

        Assert.Equal(TimeSpan.FromSeconds(5), _view.Elapsed);
    }

    [Fact]
    public void Tick_WithoutSession_ShowsZero()
    {
        _timer.RaiseTick();

        Assert.Equal(TimeSpan.Zero, _view.Elapsed);
    }

    [Fact]
    public void Dispose_StopsTheTimer()
    {
        _presenter.Dispose();

        Assert.False(_timer.IsRunning);
    }
}
