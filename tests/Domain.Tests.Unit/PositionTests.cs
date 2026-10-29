namespace Domain.Tests.Unit;

public class PositionTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 2, 0)]
    [InlineData(0, 3, 1)]
    [InlineData(2, 8, 2)]
    [InlineData(3, 0, 3)]
    [InlineData(4, 4, 4)]
    [InlineData(5, 7, 5)]
    [InlineData(8, 8, 8)]
    public void Box_ReturnsExpectedBoxIndex(int row, int column, int expectedBox)
    {
        Assert.Equal(expectedBox, new Position(row, column).Box);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(9, 0)]
    [InlineData(0, 9)]
    public void Constructor_OutOfRange_Throws(int row, int column)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Position(row, column));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(9)]
    [InlineData(40)]
    [InlineData(80)]
    public void FromIndex_RoundTripsWithIndex(int index)
    {
        Assert.Equal(index, Position.FromIndex(index).Index);
    }

    [Fact]
    public void FromIndex_MapsRowAndColumn()
    {
        var position = Position.FromIndex(10);

        Assert.Equal(1, position.Row);
        Assert.Equal(1, position.Column);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(81)]
    public void FromIndex_OutOfRange_Throws(int index)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Position.FromIndex(index));
    }

    [Fact]
    public void All_ContainsEightyOneDistinctPositions()
    {
        Assert.Equal(81, Position.All.Count);
        Assert.Equal(81, Position.All.Distinct().Count());
    }

    [Fact]
    public void Equality_IsByValue()
    {
        Assert.Equal(new Position(2, 3), new Position(2, 3));
        Assert.NotEqual(new Position(2, 3), new Position(3, 2));
    }
}
