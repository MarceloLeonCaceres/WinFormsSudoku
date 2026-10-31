namespace Sudoku.WinForms.Services;

public sealed class WinFormsUiTimer : IUiTimer, IDisposable
{
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 250 };

    public WinFormsUiTimer() => _timer.Tick += (_, _) => Tick?.Invoke(this, EventArgs.Empty);

    public event EventHandler? Tick;

    public void Start() => _timer.Start();

    public void Stop() => _timer.Stop();

    public void Dispose() => _timer.Dispose();
}
