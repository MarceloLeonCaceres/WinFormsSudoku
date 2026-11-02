using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Puzzles;
using Sudoku.Application.Tests.TestDoubles;
using Sudoku.Application.UseCases;

namespace Application.Tests.Unit.UseCases;

public class GetMistakesTests
{
    private const string SolutionDigits =
        "534678912672195348198342567859761423426853791713924856961537284287419635345286179";

    private static readonly Board Solution = Board.FromGivens(SolutionDigits);
    private static readonly string FiveGiven = "5" + new string('.', 80);
    private static readonly string ThreeGiven = ".3" + new string('.', 79);

    private static GameSession NewSession(string givens) =>
        new(new Puzzle(Board.FromGivens(givens), Solution, Difficulty.Easy));

    private static GetMistakes Create(CountingSolver solver) => new(new SolveGame(solver));

    [Fact]
    public void ReturnsTheCellsThatDifferFromTheSolution()
    {
        var session = NewSession(FiveGiven);
        session.Apply(session.Board.WithValue(new Position(0, 1), 9)); // la solución tiene 3
        session.Apply(session.Board.WithValue(new Position(0, 2), 4)); // correcto

        var mistakes = Create(new CountingSolver(Solution)).Execute(session);

        Assert.Equal(new Position(0, 1), Assert.Single(mistakes));
    }

    [Fact]
    public void SolvesOnlyTheGivens_IgnoringPlayerValues()
    {
        var session = NewSession(FiveGiven);
        session.Apply(session.Board.WithValue(new Position(0, 1), 9));
        var solver = new CountingSolver(Solution);

        Create(solver).Execute(session);

        Assert.Equal(session.Board.GivensOnly(), solver.LastBoard);
    }

    [Fact]
    public void SolvesOnlyOnce_WhileTheGivensDoNotChange()
    {
        var session = NewSession(FiveGiven);
        var solver = new CountingSolver(Solution);
        var useCase = Create(solver);

        useCase.Execute(session);
        session.Apply(session.Board.WithValue(new Position(0, 1), 9));
        useCase.Execute(session);

        Assert.Equal(1, solver.SolveCalls);
    }

    [Fact]
    public void SolvesAgain_WhenTheGivensChange()
    {
        var solver = new CountingSolver(Solution);
        var useCase = Create(solver);

        useCase.Execute(NewSession(FiveGiven));
        useCase.Execute(NewSession(ThreeGiven));

        Assert.Equal(2, solver.SolveCalls);
    }

    [Fact]
    public void GivensWithoutSolution_Throws_AndTheFailureIsNotCached()
    {
        var session = NewSession(FiveGiven);
        var solver = new CountingSolver(null);
        var useCase = Create(solver);

        Assert.Throws<InvalidOperationException>(() => useCase.Execute(session));
        Assert.Throws<InvalidOperationException>(() => useCase.Execute(session));

        Assert.Equal(2, solver.SolveCalls);
    }

    [Fact]
    public void NullSession_Throws()
    {
        var useCase = Create(new CountingSolver(Solution));

        Assert.Throws<ArgumentNullException>(() => useCase.Execute(null!));
    }
}
