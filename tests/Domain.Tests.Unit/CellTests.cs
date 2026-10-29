using Xunit;

namespace Domain.Tests.Unit;

public class CellTests
{
    [Fact]
    public void Empty_HasNoValueNotGivenAndNoNotes()
    {
        var cell = Cell.Empty;

        Assert.Null(cell.Value);
        Assert.True(cell.IsEmpty);
        Assert.False(cell.IsGiven);
        Assert.True(cell.Notes.IsEmpty);
    }

    [Fact]
    public void Default_EqualsEmpty()
    {
        Assert.Equal(Cell.Empty, default(Cell));
    }

    [Fact]
    public void Given_HasValueAndIsGiven()
    {
        var cell = Cell.Given(5);

        Assert.Equal((int?)5, cell.Value);
        Assert.True(cell.IsGiven);
        Assert.False(cell.IsEmpty);
        Assert.True(cell.Notes.IsEmpty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void Given_InvalidValue_Throws(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Cell.Given(value));
    }

    [Fact]
    public void WithValue_OnEmpty_SetsPlayerValue()
    {
        var cell = Cell.Empty.WithValue(7);

        Assert.Equal((int?)7, cell.Value);
        Assert.False(cell.IsGiven);
    }

    [Fact]
    public void WithValue_ClearsNotes()
    {
        var cell = Cell.Empty.WithNotes(CandidateSet.Of(1, 2)).WithValue(3);

        Assert.True(cell.Notes.IsEmpty);
    }

    [Fact]
    public void WithValue_OnGiven_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Cell.Given(5).WithValue(6));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void WithValue_InvalidValue_Throws(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Cell.Empty.WithValue(value));
    }

    [Fact]
    public void WithNotes_OnEmpty_SetsNotes()
    {
        var cell = Cell.Empty.WithNotes(CandidateSet.Of(2, 4));

        Assert.Equal(CandidateSet.Of(2, 4), cell.Notes);
        Assert.True(cell.IsEmpty);
    }

    [Fact]
    public void WithNotes_OnFilledCell_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Cell.Empty.WithValue(5).WithNotes(CandidateSet.Of(1)));
    }

    [Fact]
    public void WithNotes_OnGiven_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Cell.Given(5).WithNotes(CandidateSet.Of(1)));
    }

    [Fact]
    public void Cleared_OnFilledCell_ReturnsEmpty()
    {
        Assert.Equal(Cell.Empty, Cell.Empty.WithValue(5).Cleared());
    }

    [Fact]
    public void Cleared_OnCellWithNotes_RemovesNotes()
    {
        Assert.Equal(Cell.Empty, Cell.Empty.WithNotes(CandidateSet.Of(1, 2)).Cleared());
    }

    [Fact]
    public void Cleared_OnGiven_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Cell.Given(5).Cleared());
    }
}
