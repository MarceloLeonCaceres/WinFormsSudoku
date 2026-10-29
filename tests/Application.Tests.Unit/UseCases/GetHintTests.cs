using Application.UseCases;
using Domain;

namespace Application.Tests.UseCases;

public sealed class GetHintTests
{
    // Puzzle con solo dos celdas dadas (5 y 3) y su solución completa conocida.
    private static readonly string Puzzle = "53" + new string('0', 79);

    private const string Solution =
        "534678912672195348198342567859761423426853791713924856961537284287419635345286179";

    private static Board PuzzleBoard => Board.FromGivens(Puzzle);

    [Fact]
    public void Execute_PreferredCellIsEmpty_ReturnsSolutionValueForThatCell()
    {
        var useCase = new GetHint(new StubSolver(Board.FromGivens(Solution)));

        var hint = useCase.Execute(PuzzleBoard, new Position(1, 0));

        Assert.Equal((Hint?)new Hint(new Position(1, 0), 6), hint);
    }

    [Fact]
    public void Execute_NoPreferredCell_ReturnsFirstEmptyCell()
    {
        var useCase = new GetHint(new StubSolver(Board.FromGivens(Solution)));

        var hint = useCase.Execute(PuzzleBoard);

        Assert.Equal((Hint?)new Hint(new Position(0, 2), 4), hint);
    }

    [Fact]
    public void Execute_PreferredCellIsFilled_FallsBackToFirstEmptyCell()
    {
        var useCase = new GetHint(new StubSolver(Board.FromGivens(Solution)));

        var hint = useCase.Execute(PuzzleBoard, new Position(0, 0));

        Assert.Equal((Hint?)new Hint(new Position(0, 2), 4), hint);
    }

    [Fact]
    public void Execute_BoardWithoutEmptyCells_ReturnsNullAndDoesNotCallSolver()
    {
        var solver = new StubSolver(Board.FromGivens(Solution));
        var useCase = new GetHint(solver);

        var hint = useCase.Execute(Board.FromGivens(Solution));

        Assert.Null(hint);
        Assert.Equal(0, solver.SolveCalls);
    }

    [Fact]
    public void Execute_SendsOnlyTheGivensToTheSolver()
    {
        var solver = new StubSolver(Board.FromGivens(Solution));
        var boardWithMistake = PuzzleBoard.WithValue(new Position(0, 2), 9);

        new GetHint(solver).Execute(boardWithMistake);

        Assert.Equal(PuzzleBoard, solver.LastBoard);
    }

    [Fact]
    public void Execute_GivensWithoutSolution_Throws()
    {
        var useCase = new GetHint(new StubSolver(null));

        Assert.Throws<InvalidOperationException>(() => useCase.Execute(PuzzleBoard));
    }

    [Fact]
    public void Execute_NullBoard_Throws()
    {
        var useCase = new GetHint(new StubSolver(null));

        Assert.Throws<ArgumentNullException>(() => useCase.Execute(null!));
    }
}
