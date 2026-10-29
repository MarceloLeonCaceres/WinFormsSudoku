using Domain;
using Infrastructure.Solving;

namespace Infrastructure.Tests.Solving;

public sealed class BacktrackingSolverTests
{
    private readonly BacktrackingSolver _solver = new();

    [Fact]
    public void Solve_EasyPuzzle_ReturnsKnownSolution()
    {
        var solution = _solver.Solve(Board.FromGivens(TestPuzzles.Easy));

        Assert.NotNull(solution);
        Assert.Equal(TestPuzzles.EasySolution, solution.ToValuesString());
    }

    [Fact]
    public void Solve_HardPuzzle_ReturnsSolutionValidAccordingToRules()
    {
        var puzzle = Board.FromGivens(TestPuzzles.Hard);

        var solution = _solver.Solve(puzzle);

        Assert.NotNull(solution);
        Assert.True(SudokuRules.IsSolved(solution));
        Assert.All(Position.All, position =>
        {
            if (puzzle[position].IsGiven)
            {
                Assert.Equal(puzzle[position].Value, solution[position].Value);
            }
        });
    }

    [Fact]
    public void Solve_KeepsGivenFlagsAndMarksSolvedCellsAsPlayerValues()
    {
        var puzzle = Board.FromGivens(TestPuzzles.Easy);

        var solution = _solver.Solve(puzzle);

        Assert.NotNull(solution);
        Assert.All(Position.All, position =>
            Assert.Equal(puzzle[position].IsGiven, solution[position].IsGiven));
    }

    [Fact]
    public void Solve_KeepsExistingPlayerValues()
    {
        var position = new Position(0, 2);
        var puzzle = Board.FromGivens(TestPuzzles.Easy).WithValue(position, 4);

        var solution = _solver.Solve(puzzle);

        Assert.NotNull(solution);
        Assert.Equal(4, solution[position].Value);
        Assert.False(solution[position].IsGiven);
    }

    [Fact]
    public void Solve_EmptyBoard_ReturnsValidSolution()
    {
        var solution = _solver.Solve(Board.Empty);

        Assert.NotNull(solution);
        Assert.True(SudokuRules.IsSolved(solution));
    }

    [Fact]
    public void Solve_AlreadySolvedBoard_ReturnsEqualBoard()
    {
        var solved = Board.FromGivens(TestPuzzles.EasySolution);

        Assert.Equal(solved, _solver.Solve(solved));
    }

    [Fact]
    public void Solve_ConflictingGivens_ReturnsNull()
    {
        Assert.Null(_solver.Solve(Board.FromGivens(TestPuzzles.ConflictingGivens)));
    }

    [Fact]
    public void Solve_CellWithoutCandidates_ReturnsNull()
    {
        Assert.Null(_solver.Solve(Board.FromGivens(TestPuzzles.CellWithoutCandidates)));
    }

    [Fact]
    public void Solve_NullBoard_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _solver.Solve(null!));
    }

    [Theory]
    [InlineData(TestPuzzles.Easy)]
    [InlineData(TestPuzzles.Hard)]
    public void CountSolutions_UniquePuzzle_ReturnsOne(string puzzle)
    {
        Assert.Equal(1, _solver.CountSolutions(Board.FromGivens(puzzle), limit: 2));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void CountSolutions_EmptyBoard_StopsAtLimit(int limit)
    {
        Assert.Equal(limit, _solver.CountSolutions(Board.Empty, limit));
    }

    [Fact]
    public void CountSolutions_ConflictingGivens_ReturnsZero()
    {
        Assert.Equal(0, _solver.CountSolutions(Board.FromGivens(TestPuzzles.ConflictingGivens), limit: 2));
    }

    [Fact]
    public void CountSolutions_CellWithoutCandidates_ReturnsZero()
    {
        Assert.Equal(0, _solver.CountSolutions(Board.FromGivens(TestPuzzles.CellWithoutCandidates), limit: 2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CountSolutions_LimitBelowOne_Throws(int limit)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _solver.CountSolutions(Board.Empty, limit));
    }
}
