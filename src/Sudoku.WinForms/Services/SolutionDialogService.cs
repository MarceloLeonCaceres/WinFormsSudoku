using Domain;
using Sudoku.WinForms.Theming;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Services;

public sealed class SolutionDialogService(IThemeService themes) : ISolutionDialog
{
    public void Show(Board solution)
    {
        using var form = new SolutionForm(themes, solution);
        form.ShowDialog(Form.ActiveForm);
    }
}
