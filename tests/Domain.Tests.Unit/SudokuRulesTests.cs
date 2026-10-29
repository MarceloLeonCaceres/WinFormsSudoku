namespace Domain.Tests.Unit;

public class SudokuRulesTests
{
    private const string Puzzle =
        "530070000600195000098000060800060003400803001700020006060000280000419005000080079";

    private const string Solution =
        "534678912672195348198342567859761423426853791713924856961537284287419635345286179";

    private static Position P(int row, int column) => new(row, column);

    // ---- Peers ----

    [Fact]
    public void Peers_EveryPositionHasTwentyPeers()
    {
        Assert.All(Position.All, position => Assert.Equal(20, SudokuRules.Peers(position).Count));
    }

    [Fact]
    public void Peers_DoesNotIncludeThePositionItself()
    {
        Assert.All(Position.All, position => Assert.DoesNotContain(position, SudokuRules.Peers(position)));
    }

    [Fact]
    public void Peers_IncludesRowColumnAndBoxButNothingElse()
    {
        var peers = SudokuRules.Peers(P(4, 4));

        Assert.Contains(P(4, 0), peers);
        Assert.Contains(P(0, 4), peers);
        Assert.Contains(P(3, 3), peers);
        Assert.Contains(P(5, 5), peers);
        Assert.DoesNotContain(P(0, 0), peers);
        Assert.DoesNotContain(P(3, 2), peers);
    }

    [Fact]
    public void Peers_AreSymmetric()
    {
        Assert.All(
            Position.All,
            position => Assert.All(
                SudokuRules.Peers(position),
                peer => Assert.Contains(position, SudokuRules.Peers(peer))));
    }

    // ---- IsValidPlacement ----

    [Fact]
    public void IsValidPlacement_SameRow_IsInvalid()
    {
        var board = Board.Empty.WithValue(P(0, 0), 5);

        Assert.False(SudokuRules.IsValidPlacement(board, P(0, 8), 5));
    }

    [Fact]
    public void IsValidPlacement_SameColumn_IsInvalid()
    {
        var board = Board.Empty.WithValue(P(0, 0), 5);

        Assert.False(SudokuRules.IsValidPlacement(board, P(8, 0), 5));
    }

    [Fact]
    public void IsValidPlacement_SameBox_IsInvalid()
    {
        var board = Board.Empty.WithValue(P(0, 0), 5);

        Assert.False(SudokuRules.IsValidPlacement(board, P(2, 2), 5));
    }

    [Fact]
    public void IsValidPlacement_UnrelatedCellOrOtherValue_IsValid()
    {
        var board = Board.Empty.WithValue(P(0, 0), 5);

        Assert.True(SudokuRules.IsValidPlacement(board, P(4, 4), 5));
        Assert.True(SudokuRules.IsValidPlacement(board, P(0, 8), 6));
    }

    [Fact]
    public void IsValidPlacement_IgnoresTheCellItself()
    {
        var board = Board.Empty.WithValue(P(0, 0), 5);

        Assert.True(SudokuRules.IsValidPlacement(board, P(0, 0), 5));
    }

    [Fact]
    public void IsValidPlacement_ConsidersGivenCells()
    {
        var board = Board.FromGivens(Puzzle);

        Assert.True(SudokuRules.IsValidPlacement(board, P(0, 2), 4));
        Assert.False(SudokuRules.IsValidPlacement(board, P(0, 2), 3)); // fila
        Assert.False(SudokuRules.IsValidPlacement(board, P(0, 2), 8)); // columna y caja
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void IsValidPlacement_InvalidValue_Throws(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SudokuRules.IsValidPlacement(Board.Empty, P(0, 0), value));
    }

    [Fact]
    public void IsValidPlacement_NullBoard_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SudokuRules.IsValidPlacement(null!, P(0, 0), 1));
    }

    // ---- FindConflicts ----

    [Fact]
    public void FindConflicts_EmptyBoard_ReturnsNone()
    {
        Assert.Empty(SudokuRules.FindConflicts(Board.Empty));
    }

    [Fact]
    public void FindConflicts_ValidPuzzle_ReturnsNone()
    {
        Assert.Empty(SudokuRules.FindConflicts(Board.FromGivens(Puzzle)));
    }

    [Fact]
    public void FindConflicts_RowDuplicate_MarksBothCells()
    {
        var board = Board.Empty.WithValue(P(0, 0), 5).WithValue(P(0, 8), 5);

        var conflicts = SudokuRules.FindConflicts(board);

        Assert.Equal(2, conflicts.Count);
        Assert.Contains(P(0, 0), conflicts);
        Assert.Contains(P(0, 8), conflicts);
    }

    [Fact]
    public void FindConflicts_ColumnDuplicate_MarksBothCells()
    {
        var board = Board.Empty.WithValue(P(0, 0), 5).WithValue(P(8, 0), 5);

        var conflicts = SudokuRules.FindConflicts(board);

        Assert.Equal(2, conflicts.Count);
        Assert.Contains(P(0, 0), conflicts);
        Assert.Contains(P(8, 0), conflicts);
    }

    [Fact]
    public void FindConflicts_BoxDuplicate_MarksBothCells()
    {
        var board = Board.Empty.WithValue(P(0, 0), 5).WithValue(P(2, 2), 5);

        var conflicts = SudokuRules.FindConflicts(board);

        Assert.Equal(2, conflicts.Count);
        Assert.Contains(P(0, 0), conflicts);
        Assert.Contains(P(2, 2), conflicts);
    }

    [Fact]
    public void FindConflicts_ThreeDuplicatesInARow_MarksAllThree()
    {
        var board = Board.Empty
            .WithValue(P(0, 0), 5)
            .WithValue(P(0, 4), 5)
            .WithValue(P(0, 8), 5);

        Assert.Equal(3, SudokuRules.FindConflicts(board).Count);
    }

    [Fact]
    public void FindConflicts_SameValueInUnrelatedCells_IsNotAConflict()
    {
        var board = Board.Empty.WithValue(P(0, 0), 5).WithValue(P(1, 3), 5);

        Assert.Empty(SudokuRules.FindConflicts(board));
    }

    [Fact]
    public void FindConflicts_NotesNeverConflict()
    {
        var board = Board.Empty
            .WithNotes(P(0, 0), CandidateSet.Of(5))
            .WithNotes(P(0, 1), CandidateSet.Of(5));

        Assert.Empty(SudokuRules.FindConflicts(board));
    }

    [Fact]
    public void FindConflicts_GivenVersusPlayerValue_MarksBoth()
    {
        var board = Board.FromGivens("5" + new string('0', 80)).WithValue(P(0, 8), 5);

        var conflicts = SudokuRules.FindConflicts(board);

        Assert.Equal(2, conflicts.Count);
        Assert.Contains(P(0, 0), conflicts);
        Assert.Contains(P(0, 8), conflicts);
    }

    // ---- IsComplete / IsSolved ----

    [Fact]
    public void IsComplete_EmptyBoard_IsFalse()
    {
        Assert.False(SudokuRules.IsComplete(Board.Empty));
    }

    [Fact]
    public void IsComplete_IncompletePuzzle_IsFalse()
    {
        Assert.False(SudokuRules.IsComplete(Board.FromGivens(Puzzle)));
    }

    [Fact]
    public void IsComplete_FullBoard_IsTrue()
    {
        Assert.True(SudokuRules.IsComplete(Board.FromGivens(Solution)));
    }

    [Fact]
    public void IsSolved_ValidSolution_IsTrue()
    {
        Assert.True(SudokuRules.IsSolved(Board.FromGivens(Solution)));
    }

    [Fact]
    public void IsSolved_IncompleteBoard_IsFalse()
    {
        Assert.False(SudokuRules.IsSolved(Board.FromGivens(Puzzle)));
    }

    [Fact]
    public void IsSolved_CompleteWithCorrectPlayerValue_IsTrue()
    {
        var board = Board.FromGivens("0" + Solution[1..]).WithValue(P(0, 0), 5);

        Assert.True(SudokuRules.IsSolved(board));
    }

    [Fact]
    public void IsSolved_CompleteWithConflict_IsFalse()
    {
        var board = Board.FromGivens("0" + Solution[1..]).WithValue(P(0, 0), 3);

        Assert.True(SudokuRules.IsComplete(board));
        Assert.False(SudokuRules.IsSolved(board));
    }
}
