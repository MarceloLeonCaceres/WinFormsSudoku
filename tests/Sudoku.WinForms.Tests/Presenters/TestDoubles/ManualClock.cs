using Sudoku.Application.Ports;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

public sealed class ManualClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 10, 31, 12, 0, 0, TimeSpan.Zero);

    public void Advance(TimeSpan time) => UtcNow += time;
}
