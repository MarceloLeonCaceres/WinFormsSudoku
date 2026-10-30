using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;

namespace Sudoku.Application.UseCases;

/// <summary>Devuelve las estadísticas guardadas (vacías si aún no hay ninguna).</summary>
/// <exception cref="CorruptSaveException">El archivo de estadísticas está dañado.</exception>
public sealed class GetStatistics(IStatisticsRepository repository)
{
    public GameStatistics Execute() => repository.Load();
}
