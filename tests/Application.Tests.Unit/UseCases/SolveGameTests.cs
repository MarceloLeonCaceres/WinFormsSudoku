using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Puzzles;
using Sudoku.Application.UseCases;

namespace Application.Tests.Unit.UseCases;

public class SolveGameTests
{
    private const string Givens = "530070000600195000098000060800060003400803001700020006060000280000419005000080079";
    private const string SolutionText = "534678912672195348198342567859761423426853791713924856961537284287419635345286179";

    private static GameSession NewSession() =>
        new(new Puzzle(Board.FromGivens(Givens), Board.FromGivens(SolutionText), Difficulty.Easy));

    [Fact]
    public void SolvesOnlyTheGivens_IgnoringPlayerValues()
    {
        var solver = new FakeSolver(Board.FromGivens(SolutionText));
        var session = NewSession();
        session.Apply(session.Board.WithValue(new Position(0, 2), 9)); // valor erróneo del jugador

        var result = new SolveGame(solver).Execute(session);

        Assert.Equal(Board.FromGivens(SolutionText), result);
        Assert.Equal(session.Board.GivensOnly(), solver.LastInput);
    }

    [Fact]
    public void DoesNotModifyTheSession()
    {
        var session = NewSession();
        var before = session.Board;

        new SolveGame(new FakeSolver(Board.FromGivens(SolutionText))).Execute(session);

        Assert.Same(before, session.Board);
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void GivensWithoutSolution_Throws()
    {
        var useCase = new SolveGame(new FakeSolver(null));

        Assert.Throws<InvalidOperationException>(() => useCase.Execute(NewSession()));
    }

    [Fact]
    public void NullSession_Throws()
    {
        var useCase = new SolveGame(new FakeSolver(null));

        Assert.Throws<ArgumentNullException>(() => useCase.Execute(null!));
    }

    private sealed class FakeSolver(Board? result) : ISolver
    {
        public Board? LastInput { get; private set; }

        public Board? Solve(Board board)
        {
            LastInput = board;
            return result;
        }

        public int CountSolutions(Board board, int limit) => 1;
    }
}
