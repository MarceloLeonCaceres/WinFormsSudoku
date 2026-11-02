using Sudoku.Application.Settings;

namespace Infrastructure.Persistence;

internal static class SettingsMapper
{
    public static SettingsDto ToDto(AppSettings settings) => new()
    {
        Version = SettingsDto.CurrentVersion,
        Theme = settings.Theme.ToString(),
        ShowMistakes = settings.ShowMistakes
    };

    public static AppSettings ToModel(SettingsDto dto)
    {
        SchemaVersion.Ensure(dto.Version, SettingsDto.CurrentVersion, "settings.json");
        return new AppSettings(EnumParser.Parse<ThemeKind>(dto.Theme, nameof(dto.Theme)), dto.ShowMistakes);
    }
}
