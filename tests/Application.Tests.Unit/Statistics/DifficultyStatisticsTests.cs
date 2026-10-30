using Sudoku.Application.Statistics;

namespace Application.Tests.Unit.Statistics;

public class DifficultyStatisticsTests
{
    private static readonly TimeSpan Min = TimeSpan.FromMinutes(1);

    [Fact]
    public void Empty_has_no_games_and_no_times()
    {
        var stats = DifficultyStatistics.Empty;

        Assert.Equal(0, stats.Started);
        Assert.Null(stats.BestTime);
        Assert.Null(stats.AverageWonTime);
    }

    [Fact]
    public void WithStarted_increments_started_only()
    {
        var stats = DifficultyStatistics.Empty.WithStarted().WithStarted();

        Assert.Equal(2, stats.Started);
        Assert.Equal(0, stats.Won);
        Assert.Equal(0, stats.Lost);
    }

    [Fact]
    public void Won_updates_count_total_and_best_time()
    {
        var stats = DifficultyStatistics.Empty.WithFinished(GameOutcome.Won, 10 * Min);

        Assert.Equal(1, stats.Won);
        Assert.Equal(10 * Min, stats.BestTime);
        Assert.Equal(10 * Min, stats.TotalWonTime);
    }

    [Fact]
    public void Won_keeps_best_time_when_new_one_is_slower_and_replaces_it_when_faster()
    {
        var stats = DifficultyStatistics.Empty
            .WithFinished(GameOutcome.Won, 10 * Min)
            .WithFinished(GameOutcome.Won, 12 * Min);
        Assert.Equal(10 * Min, stats.BestTime);

        stats = stats.WithFinished(GameOutcome.Won, 7 * Min);
        Assert.Equal(7 * Min, stats.BestTime);
    }

    [Fact]
    public void Lost_increments_lost_without_touching_times()
    {
        var stats = DifficultyStatistics.Empty.WithFinished(GameOutcome.Lost, 30 * Min);

        Assert.Equal(1, stats.Lost);
        Assert.Equal(0, stats.Won);
        Assert.Null(stats.BestTime);
        Assert.Equal(TimeSpan.Zero, stats.TotalWonTime);
    }

    [Fact]
    public void AverageWonTime_is_total_divided_by_wins()
    {
        var stats = DifficultyStatistics.Empty
            .WithFinished(GameOutcome.Won, 10 * Min)
            .WithFinished(GameOutcome.Won, 20 * Min);

        Assert.Equal(15 * Min, stats.AverageWonTime);
    }
}
