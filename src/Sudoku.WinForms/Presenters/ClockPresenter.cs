using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;

/// <summary>Refresca el tiempo mostrado a partir de <see cref="IUiTimer"/>. Una sola razón de cambio: el reloj de la vista.</summary>
public sealed class ClockPresenter : IDisposable
{
    private readonly IGameControlsView _view;
    private readonly CurrentGame _current;
    private readonly IUiTimer _timer;

    public ClockPresenter(IGameControlsView view, CurrentGame current, IUiTimer timer)
    {
        _view = view;
        _current = current;
        _timer = timer;

        _timer.Tick += OnTick;
        _current.SessionChanged += OnSessionChanged;
        _timer.Start();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
        _current.SessionChanged -= OnSessionChanged;
    }

    private void OnTick(object? sender, EventArgs e) => Refresh();

    private void OnSessionChanged(object? sender, SessionChangedEventArgs e) => Refresh();

    private void Refresh() => _view.SetElapsedTime(_current.Session?.ElapsedTime ?? TimeSpan.Zero);
}
