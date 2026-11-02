using Domain;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Views;

/// <summary>
/// Diálogo modal con la solución. Reutiliza BoardControl; como ningún presenter se suscribe
/// a sus eventos, es de solo lectura en la práctica.
/// </summary>
public sealed class SolutionForm : Form
{
    public SolutionForm(IThemeService themes, Board solution)
    {
        Text = "Solución";
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        MaximizeBox = false;
        ClientSize = new Size(520, 520);
        MinimumSize = new Size(360, 380);

        var board = new BoardControl(themes) { Dock = DockStyle.Fill };
        board.SetBoard(solution);
        Controls.Add(board);

        ThemeApplier.Apply(themes.Current, this);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData != Keys.Escape)
            return base.ProcessCmdKey(ref msg, keyData);

        Close();
        return true;
    }

    private void InitializeComponent()
    {

    }
}
