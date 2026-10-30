using Sudoku.Application.Ports;

namespace Application.Tests.Unit.TestData;

internal sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; private set; } = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    public void Advance(TimeSpan by) => UtcNow += by;
}
