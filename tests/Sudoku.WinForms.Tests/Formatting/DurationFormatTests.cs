using Sudoku.WinForms.Formatting;

namespace Sudoku.WinForms.Tests.Formatting;

public class DurationFormatTests
{
    [Fact]
    public void UnderAnHour_UsesMinutesAndSeconds() =>
        Assert.Equal("05:07", DurationFormat.Format(new TimeSpan(0, 5, 7)));

    [Fact]
    public void FromAnHour_AddsHours() =>
        Assert.Equal("1:02:03", DurationFormat.Format(new TimeSpan(1, 2, 3)));

    [Fact]
    public void Null_ShowsADash() =>
        Assert.Equal("—", DurationFormat.Format((TimeSpan?)null));
}
