namespace Infrastructure.Persistence;

internal sealed class DifficultyStatisticsDto
{
    public required string Difficulty { get; init; }
    public required int Started { get; init; }
    public required int Won { get; init; }
    public required int Lost { get; init; }
    public TimeSpan? BestTime { get; init; }
    public required TimeSpan TotalWonTime { get; init; }
}
