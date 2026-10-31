using Sudoku.Application.Statistics;
using Sudoku.WinForms.Theming;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Services;

public sealed class StatisticsDialogService(IThemeService themes) : IStatisticsDialog
{
    public void Show(GameStatistics statistics)
    {
        using var form = new StatisticsForm(themes, statistics);
        form.ShowDialog(Form.ActiveForm);
    }
}
