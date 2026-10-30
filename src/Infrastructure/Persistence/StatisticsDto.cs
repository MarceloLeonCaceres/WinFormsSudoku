namespace Infrastructure.Persistence;

internal sealed class StatisticsDto
{
    public const int CurrentVersion = 1;

    public required int Version { get; init; }
    public required List<DifficultyStatisticsDto> ByDifficulty { get; init; }
    public required List<GameRecordDto> Recent { get; init; }
}
