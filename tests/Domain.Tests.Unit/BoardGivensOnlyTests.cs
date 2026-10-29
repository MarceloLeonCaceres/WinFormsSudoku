using Domain;

namespace Domain.Tests;

public sealed class BoardGivensOnlyTests
{
    private static readonly string Puzzle = "5" + new string('0', 80);

    [Fact]
    public void GivensOnly_RemovesPlayerValuesAndNotes()
    {
        var board = Board.FromGivens(Puzzle)
            .WithValue(new Position(0, 1), 3)
            .WithNotes(new Position(0, 2), CandidateSet.Of(1, 2));

        Assert.Equal(Board.FromGivens(Puzzle), board.GivensOnly());
    }

    [Fact]
    public void GivensOnly_KeepsGivenCells()
    {
        var cell = Board.FromGivens(Puzzle).GivensOnly()[new Position(0, 0)];

        Assert.Equal(5, cell.Value);
        Assert.True(cell.IsGiven);
    }

    [Fact]
    public void GivensOnly_DoesNotModifyOriginalBoard()
    {
        var board = Board.FromGivens(Puzzle).WithValue(new Position(0, 1), 3);

        _ = board.GivensOnly();

        Assert.Equal(3, board[new Position(0, 1)].Value);
    }

    [Fact]
    public void GivensOnly_OnEmptyBoard_ReturnsEmptyBoard()
    {
        Assert.Equal(Board.Empty, Board.Empty.GivensOnly());
    }
}
