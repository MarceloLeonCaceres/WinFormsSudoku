namespace Sudoku.WinForms.Services;

public sealed class MessageBoxDialogService : IDialogService
{
    public void ShowError(string title, string message) =>
      MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    public void ShowInfo(string title, string message) =>
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public bool Confirm(string title, string message) =>
        MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

}
