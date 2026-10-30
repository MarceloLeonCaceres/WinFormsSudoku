using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Infrastructure.Persistence;

/// <summary>Guarda las preferencias en <c>settings.json</c>. Un archivo dañado lanza <see cref="CorruptSaveException"/> y no se modifica.</summary>
public sealed class JsonSettingsRepository(IAppDataPaths paths) : ISettingsRepository
{
    public AppSettings Load() => CorruptSaveGuard.Run("settings.json", () =>
    {
        var dto = JsonFileStore.Read<SettingsDto>(paths.SettingsPath);
        return dto is null ? AppSettings.Default : SettingsMapper.ToModel(dto);
    });

    public void Save(AppSettings settings) =>
        JsonFileStore.Write(paths.SettingsPath, SettingsMapper.ToDto(settings));
}
