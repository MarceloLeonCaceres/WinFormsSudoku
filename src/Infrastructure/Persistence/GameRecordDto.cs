namespace Infrastructure.Persistence;

internal sealed class GameRecordDto
{
    public required string Difficulty { get; init; }
    public required string Outcome { get; init; }
    public required TimeSpan Elapsed { get; init; }
    public required DateTimeOffset FinishedAt { get; init; }
}
