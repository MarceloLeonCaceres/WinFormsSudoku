namespace Infrastructure.Persistence;

internal sealed class SettingsDto
{
    public const int CurrentVersion = 1;

    public required int Version { get; init; }
    public required string Theme { get; init; }
    public bool ShowMistakes { get; init; }
}
