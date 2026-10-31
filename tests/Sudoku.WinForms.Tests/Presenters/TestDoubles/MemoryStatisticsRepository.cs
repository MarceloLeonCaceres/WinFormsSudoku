using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

public sealed class MemoryStatisticsRepository : IStatisticsRepository
{
    public GameStatistics Stored { get; set; } = GameStatistics.Empty;
    public Exception? Failure { get; set; }

    public GameStatistics Load() => Failure is null ? Stored : throw Failure;

    public void Save(GameStatistics statistics) => Stored = statistics;
}
