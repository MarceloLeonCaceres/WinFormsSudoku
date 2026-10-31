using Sudoku.WinForms.Services;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

internal sealed class RecordingDialogService : IDialogService
{
    public List<(string Title, string Message)> Errors { get; } = [];

    public void ShowError(string title, string message) => Errors.Add((title, message));
}
