using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;

namespace Infrastructure.Persistence;

/// <summary>
/// Guarda las estadísticas en <c>statistics.json</c>. Un archivo dañado nunca se modifica:
/// <see cref="Load"/> lanza <see cref="CorruptSaveException"/>.
/// </summary>
public sealed class JsonStatisticsRepository(IAppDataPaths paths) : IStatisticsRepository
{
    public GameStatistics Load() => CorruptSaveGuard.Run("statistics.json", () =>
    {
        var dto = JsonFileStore.Read<StatisticsDto>(paths.StatisticsPath);
        return dto is null ? GameStatistics.Empty : StatisticsMapper.ToModel(dto);
    });

    public void Save(GameStatistics statistics) =>
        JsonFileStore.Write(paths.StatisticsPath, StatisticsMapper.ToDto(statistics));
}
