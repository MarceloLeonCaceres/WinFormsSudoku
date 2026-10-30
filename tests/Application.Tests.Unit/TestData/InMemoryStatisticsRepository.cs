using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;

namespace Application.Tests.Unit.TestData;

internal sealed class InMemoryStatisticsRepository : IStatisticsRepository
{
    public GameStatistics Current { get; private set; } = GameStatistics.Empty;
    public int SaveCount { get; private set; }

    public GameStatistics Load() => Current;

    public void Save(GameStatistics statistics)
    {
        Current = statistics;
        SaveCount++;
    }
}
