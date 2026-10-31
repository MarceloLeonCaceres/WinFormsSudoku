namespace Sudoku.WinForms.Services;

public sealed class MessageBoxDialogService : IDialogService
{
    public void ShowError(string title, string message)
    {
        Form? owner = Form.ActiveForm;
        MessageBox.Show(owner, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
