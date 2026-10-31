using Sudoku.Application.Statistics;
using Sudoku.WinForms.Services;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

public sealed class StubStatisticsDialog : IStatisticsDialog
{
    public GameStatistics? Shown { get; private set; }

    public void Show(GameStatistics statistics) => Shown = statistics;
}
