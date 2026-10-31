using Sudoku.Application.Games;
using Sudoku.Application.Statistics;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;

public sealed class StatisticsPresenterTests
{
    private readonly StubMainView _view = new();
    private readonly MemoryStatisticsRepository _stats = new();
    private readonly StubStatisticsDialog _dialog = new();
    private readonly RecordingDialogs _dialogs = new();

    private StatisticsPresenter Create() => new(_view, new GetStatistics(_stats), _dialog, _dialogs);

    [Fact]
    public void Requested_ShowsTheStoredStatistics()
    {
        using var presenter = Create();

        _view.RaiseStatistics();

        Assert.Same(_stats.Stored, _dialog.Shown);
    }

    [Fact]
    public void Requested_WithCorruptFile_ShowsErrorInsteadOfTheDialog()
    {
        _stats.Failure = new CorruptSaveException("dañado");
        using var presenter = Create();

        _view.RaiseStatistics();

        Assert.Null(_dialog.Shown);
        Assert.Single(_dialogs.Errors);
    }
}
