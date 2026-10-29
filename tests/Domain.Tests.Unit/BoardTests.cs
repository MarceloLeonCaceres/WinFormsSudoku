namespace Domain.Tests.Unit;

public class BoardTests
{
    private const string Puzzle =
        "530070000600195000098000060800060003400803001700020006060000280000419005000080079";

    private static Position P(int row, int column) => new(row, column);

    [Fact]
    public void Empty_HasAllCellsEmpty()
    {
        Assert.All(Position.All, position => Assert.True(Board.Empty[position].IsEmpty));
    }

    [Fact]
    public void FromGivens_ParsesGivensAndEmptyCells()
    {
        var board = Board.FromGivens(Puzzle);

        Assert.Equal((int?)5, board[P(0, 0)].Value);
        Assert.True(board[P(0, 0)].IsGiven);
        Assert.Equal((int?)3, board[P(0, 1)].Value);
        Assert.True(board[P(0, 2)].IsEmpty);
        Assert.Equal((int?)7, board[P(8, 7)].Value);
        Assert.Equal((int?)9, board[P(8, 8)].Value);
    }

    [Fact]
    public void FromGivens_TreatsDotAsEmpty()
    {
        Assert.Equal(Board.FromGivens(Puzzle), Board.FromGivens(Puzzle.Replace('0', '.')));
    }

    [Fact]
    public void FromGivens_WrongLength_Throws()
    {
        Assert.Throws<FormatException>(() => Board.FromGivens("123"));
    }

    [Fact]
    public void FromGivens_InvalidCharacter_Throws()
    {
        var invalid = "x" + Puzzle[1..];

        Assert.Throws<FormatException>(() => Board.FromGivens(invalid));
    }

    [Fact]
    public void FromGivens_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Board.FromGivens(null!));
    }

    [Fact]
    public void WithValue_ReturnsNewBoard_AndLeavesOriginalUntouched()
    {
        var original = Board.Empty;

        var updated = original.WithValue(P(0, 0), 5);

        Assert.True(original[P(0, 0)].IsEmpty);
        Assert.Equal((int?)5, updated[P(0, 0)].Value);
    }

    [Fact]
    public void WithValue_ChangesOnlyTheTargetCell()
    {
        var original = Board.FromGivens(Puzzle);
        var target = P(0, 2);

        var updated = original.WithValue(target, 4);

        Assert.All(
            Position.All.Where(position => position != target),
            position => Assert.Equal(original[position], updated[position]));
    }

    [Fact]
    public void WithValue_OnGivenCell_Throws()
    {
        var board = Board.FromGivens(Puzzle);

        Assert.Throws<InvalidOperationException>(() => board.WithValue(P(0, 0), 1));
    }

    [Fact]
    public void WithNotes_SetsNotesOnEmptyCell()
    {
        var board = Board.Empty.WithNotes(P(3, 3), CandidateSet.Of(1, 9));

        Assert.Equal(CandidateSet.Of(1, 9), board[P(3, 3)].Notes);
    }

    [Fact]
    public void WithNotes_OnFilledCell_Throws()
    {
        var board = Board.Empty.WithValue(P(3, 3), 4);

        Assert.Throws<InvalidOperationException>(() => board.WithNotes(P(3, 3), CandidateSet.Of(1)));
    }

    [Fact]
    public void WithCleared_RemovesPlayerValue()
    {
        var board = Board.Empty.WithValue(P(1, 1), 8).WithCleared(P(1, 1));

        Assert.True(board[P(1, 1)].IsEmpty);
    }

    [Fact]
    public void WithCleared_OnGivenCell_Throws()
    {
        var board = Board.FromGivens(Puzzle);

        Assert.Throws<InvalidOperationException>(() => board.WithCleared(P(0, 0)));
    }

    [Fact]
    public void Equality_IsByContent()
    {
        var first = Board.Empty.WithValue(P(0, 0), 5);
        var second = Board.Empty.WithValue(P(0, 0), 5);
        var different = Board.Empty.WithValue(P(0, 0), 6);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.NotEqual(first, different);
        Assert.False(first.Equals(null));
    }

    [Fact]
    public void ToValuesString_RoundTripsGivens()
    {
        Assert.Equal(Puzzle, Board.FromGivens(Puzzle).ToValuesString());
    }

    [Fact]
    public void ToValuesString_IncludesPlayerValues()
    {
        var board = Board.Empty.WithValue(P(0, 1), 7);

        Assert.Equal(81, board.ToValuesString().Length);
        Assert.StartsWith("07", board.ToValuesString());
    }
}
