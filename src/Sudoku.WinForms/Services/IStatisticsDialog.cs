using Sudoku.Application.Statistics;

namespace Sudoku.WinForms.Services;

/// <summary>Puerto del diálogo de estadísticas (modal).</summary>
public interface IStatisticsDialog
{
    void Show(GameStatistics statistics);
}
