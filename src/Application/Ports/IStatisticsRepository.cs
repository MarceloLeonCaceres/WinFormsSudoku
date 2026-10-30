using Sudoku.Application.Games;
using Sudoku.Application.Statistics;

namespace Sudoku.Application.Ports;

/// <summary>Puerto de persistencia de las estadísticas. Lo implementa Infrastructure.</summary>
public interface IStatisticsRepository
{
    /// <summary>Devuelve las estadísticas guardadas, o <see cref="GameStatistics.Empty"/> si no hay archivo.</summary>
    /// <exception cref="CorruptSaveException">El archivo existe pero está dañado o su versión no se reconoce.</exception>
    GameStatistics Load();

    void Save(GameStatistics statistics);
}
