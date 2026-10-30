using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Statistics;
using System.Collections.Immutable;

namespace Infrastructure.Persistence;

internal static class StatisticsMapper
{
    public static StatisticsDto ToDto(GameStatistics statistics) => new()
    {
        Version = StatisticsDto.CurrentVersion,
        ByDifficulty = statistics.ByDifficulty.Select(p => new DifficultyStatisticsDto
        {
            Difficulty = p.Key.ToString(),
            Started = p.Value.Started,
            Won = p.Value.Won,
            Lost = p.Value.Lost,
            BestTime = p.Value.BestTime,
            TotalWonTime = p.Value.TotalWonTime
        }).ToList(),
        Recent = statistics.Recent.Select(r => new GameRecordDto
        {
            Difficulty = r.Difficulty.ToString(),
            Outcome = r.Outcome.ToString(),
            Elapsed = r.Elapsed,
            FinishedAt = r.FinishedAt
        }).ToList()
    };

    public static GameStatistics ToModel(StatisticsDto dto)
    {
        SchemaVersion.Ensure(dto.Version, StatisticsDto.CurrentVersion, "statistics.json");

        var byDifficulty = dto.ByDifficulty.ToImmutableDictionary(
            d => EnumParser.Parse<Difficulty>(d.Difficulty, nameof(d.Difficulty)),
            ToDifficultyStatistics);
        var recent = dto.Recent.Select(ToRecord).ToImmutableList();

        return new GameStatistics(byDifficulty, recent);
    }

    private static DifficultyStatistics ToDifficultyStatistics(DifficultyStatisticsDto d)
    {
        if (d.Started < 0 || d.Won < 0 || d.Lost < 0)
            throw new CorruptSaveException("Contadores negativos en statistics.json.");

        return new DifficultyStatistics(d.Started, d.Won, d.Lost, d.BestTime, d.TotalWonTime);
    }

    private static GameRecord ToRecord(GameRecordDto r) => new(
        EnumParser.Parse<Difficulty>(r.Difficulty, nameof(r.Difficulty)),
        EnumParser.Parse<GameOutcome>(r.Outcome, nameof(r.Outcome)),
        r.Elapsed,
        r.FinishedAt);
}
