using Domain;
using Sudoku.Application.Statistics;

namespace Application.Tests.Unit.Statistics;

public class GameStatisticsTests
{
    private static GameRecord Record(Difficulty difficulty, GameOutcome outcome, int minutes) =>
        new(difficulty, outcome, TimeSpan.FromMinutes(minutes), new DateTimeOffset(2026, 10, 29, 12, minutes % 60, 0, TimeSpan.Zero));

    [Fact]
    public void For_returns_empty_statistics_for_a_difficulty_never_played()
    {
        Assert.Equal(DifficultyStatistics.Empty, GameStatistics.Empty.For(Difficulty.Expert));
    }

    [Fact]
    public void WithGameStarted_only_affects_that_difficulty()
    {
        var stats = GameStatistics.Empty.WithGameStarted(Difficulty.Easy);

        Assert.Equal(1, stats.For(Difficulty.Easy).Started);
        Assert.Equal(0, stats.For(Difficulty.Hard).Started);
    }

    [Fact]
    public void WithGameFinished_updates_aggregates_and_recent_list()
    {
        var record = Record(Difficulty.Medium, GameOutcome.Won, 9);

        var stats = GameStatistics.Empty.WithGameFinished(record);

        Assert.Equal(1, stats.For(Difficulty.Medium).Won);
        Assert.Equal(record, Assert.Single(stats.Recent));
    }

    [Fact]
    public void Recent_lists_newest_first_and_is_capped()
    {
        var stats = GameStatistics.Empty;
        for (var i = 1; i <= GameStatistics.MaxRecentGames + 5; i++)
            stats = stats.WithGameFinished(Record(Difficulty.Easy, GameOutcome.Lost, i));

        Assert.Equal(GameStatistics.MaxRecentGames, stats.Recent.Count);
        Assert.Equal(TimeSpan.FromMinutes(GameStatistics.MaxRecentGames + 5), stats.Recent[0].Elapsed);
        Assert.Equal(TimeSpan.FromMinutes(6), stats.Recent[^1].Elapsed);
        // Los agregados no se recortan: cuentan todas las partidas.
        Assert.Equal(GameStatistics.MaxRecentGames + 5, stats.For(Difficulty.Easy).Lost);
    }
}
