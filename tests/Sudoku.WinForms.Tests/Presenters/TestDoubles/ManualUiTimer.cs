using Sudoku.WinForms.Services;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

public sealed class ManualUiTimer : IUiTimer
{
    public event EventHandler? Tick;

    public bool IsRunning { get; private set; }

    public void Start() => IsRunning = true;

    public void Stop() => IsRunning = false;

    public void RaiseTick() => Tick?.Invoke(this, EventArgs.Empty);
}
