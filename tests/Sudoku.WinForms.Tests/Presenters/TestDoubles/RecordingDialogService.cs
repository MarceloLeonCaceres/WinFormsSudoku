using Sudoku.WinForms.Services;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

internal sealed class RecordingDialogService : IDialogService
{
    public List<(string Title, string Message)> Errors { get; } = [];

    public bool Confirm(string title, string message)
    {
        throw new NotImplementedException();
    }

    public void ShowError(string title, string message) => Errors.Add((title, message));

    public void ShowInfo(string title, string message)
    {
        throw new NotImplementedException();
    }
}
