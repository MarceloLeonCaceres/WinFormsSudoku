using Sudoku.Application.Ports;

namespace Sudoku.Application.Tests.TestDoubles;

public sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; private set; } = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    public void Advance(TimeSpan by) => UtcNow += by;
}
