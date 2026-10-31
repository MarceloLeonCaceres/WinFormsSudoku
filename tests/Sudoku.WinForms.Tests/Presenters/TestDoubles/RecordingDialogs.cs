using Sudoku.WinForms.Services;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

public sealed class RecordingDialogs : IDialogService
{
    public List<(string Title, string Message)> Errors { get; } = [];
    public List<(string Title, string Message)> Infos { get; } = [];
    public bool ConfirmResult { get; set; } = true;
    public int ConfirmCalls { get; private set; }

    public void ShowError(string title, string message) => Errors.Add((title, message));

    public void ShowInfo(string title, string message) => Infos.Add((title, message));

    public bool Confirm(string title, string message)
    {
        ConfirmCalls++;
        return ConfirmResult;
    }
}
