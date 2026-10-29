using Domain;
using global::Infrastructure.Generation;
using global::Infrastructure.Solving;
using Sudoku.Application.Ports;
using Sudoku.Application.Puzzles;

namespace Application.Tests.Unit.Generarion;

public class PuzzleGeneratorTests
{
    private static readonly BacktrackingSolver Solver = new();

    public static TheoryData<Difficulty> AllDifficulties => new(Enum.GetValues<Difficulty>());

    private static PuzzleGenerator CreateGenerator(int seed = 1) => new(Solver, new Random(seed));

    [Fact]
    public void Constructor_NullSolver_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new PuzzleGenerator(null!));

    [Theory]
    [MemberData(nameof(AllDifficulties))]
    public void Generate_HasUniqueSolution(Difficulty difficulty)
    {
        var puzzle = CreateGenerator().Generate(difficulty);

        Assert.Equal(1, Solver.CountSolutions(puzzle.Board, 2));
    }

    [Theory]
    [MemberData(nameof(AllDifficulties))]
    public void Generate_GivensCountIsWithinProfileRange(Difficulty difficulty)
    {
        var profile = DifficultyProfile.For(difficulty);

        var puzzle = CreateGenerator().Generate(difficulty);

        var givens = Position.All.Count(p => puzzle.Board[p].IsGiven);
        Assert.InRange(givens, profile.MinGivens, profile.MaxGivens);
        Assert.Equal(difficulty, puzzle.Difficulty);
    }

    [Fact]
    public void Generate_EveryFilledCellIsGiven_AndNoNotes()
    {
        var puzzle = CreateGenerator().Generate(Difficulty.Medium);

        foreach (var position in Position.All)
        {
            var cell = puzzle.Board[position];
            Assert.Equal(cell.Value is not null, cell.IsGiven);
            Assert.True(cell.Notes.IsEmpty);
        }
    }

    [Fact]
    public void Generate_SolutionIsSolved_AndConsistentWithGivens()
    {
        var puzzle = CreateGenerator().Generate(Difficulty.Hard);

        Assert.True(SudokuRules.IsSolved(puzzle.Solution));
        foreach (var position in Position.All.Where(p => puzzle.Board[p].Value is not null))
            Assert.Equal(puzzle.Solution[position].Value, puzzle.Board[position].Value);
    }

    [Fact]
    public void Generate_SameSeed_ProducesSamePuzzle()
    {
        var a = CreateGenerator(42).Generate(Difficulty.Medium);
        var b = CreateGenerator(42).Generate(Difficulty.Medium);

        Assert.Equal(a.Board, b.Board);
        Assert.Equal(a.Solution, b.Solution);
    }

    [Fact]
    public void Generate_DifferentSeeds_ProduceDifferentPuzzles()
    {
        var a = CreateGenerator(1).Generate(Difficulty.Medium);
        var b = CreateGenerator(2).Generate(Difficulty.Medium);

        Assert.NotEqual(a.Board, b.Board);
    }

    [Fact]
    public void Generate_CancelledToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.Throws<OperationCanceledException>(
            () => CreateGenerator().Generate(Difficulty.Easy, cts.Token));
    }

    [Fact]
    public void Generate_WhenNoCellCanBeRemoved_ThrowsAfterMaxAttempts()
    {
        var generator = new PuzzleGenerator(new NeverUniqueSolver(), new Random(1));

        Assert.Throws<InvalidOperationException>(() => generator.Generate(Difficulty.Expert));
    }

    /// <summary>Resuelve de verdad, pero dice que siempre hay varias soluciones: ninguna celda es removible.</summary>
    private sealed class NeverUniqueSolver : ISolver
    {
        public Board? Solve(Board board) => Solver.Solve(board);
        public int CountSolutions(Board board, int limit) => 2;
    }
}
