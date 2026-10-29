using Application.Tests.Unit.TestData;
using Domain;
using global::Sudoku.Application.Games;
using global::Sudoku.Application.Ports;
using global::Sudoku.Application.Puzzles;
using global::Sudoku.Application.UseCases;

namespace Application.Tests.Unit;

public class ApplyHintTests
{
    private sealed class FixedSolutionSolver(Board? solution) : ISolver
    {
        public Board? Solve(Board board) => solution;
        public int CountSolutions(Board board, int limit) => throw new NotSupportedException();
    }

    private static ApplyHint CreateUseCase(Board? solution)
        => new(new GetHint(new FixedSolutionSolver(solution)), new PlaceValue());

    private static ApplyHint CreateUseCase()
        => CreateUseCase(Board.FromGivens(SamplePuzzle.Solution));

    [Fact]
    public void PlacesSolutionValueInPreferredEmptyCell()
    {
        var session = SamplePuzzle.NewSession();

        var hint = CreateUseCase().Execute(session, SamplePuzzle.EmptyCell);

        Assert.NotNull(hint);
        Assert.Equal(SamplePuzzle.EmptyCell, hint.Value.Position);
        Assert.Equal(4, hint.Value.Value);
        Assert.Equal(4, session.Board[SamplePuzzle.EmptyCell].Value);
        Assert.False(session.Board[SamplePuzzle.EmptyCell].IsGiven);
    }

    [Fact]
    public void WithoutPreferred_UsesFirstEmptyCell()
    {
        var session = SamplePuzzle.NewSession();

        var hint = CreateUseCase().Execute(session);

        Assert.Equal(SamplePuzzle.EmptyCell, hint!.Value.Position);
    }

    [Fact]
    public void PreferredCellNotEmpty_FallsBackToFirstEmptyCell()
    {
        var session = SamplePuzzle.NewSession();

        var hint = CreateUseCase().Execute(session, SamplePuzzle.GivenCell);

        Assert.Equal(SamplePuzzle.EmptyCell, hint!.Value.Position);
    }

    [Fact]
    public void RemovesHintValueFromPeerNotes()
    {
        var session = SamplePuzzle.NewSession();
        var peer = new Position(0, 3);
        session.Apply(session.Board.WithNotes(peer, CandidateSet.Of(4, 9)));

        CreateUseCase().Execute(session, SamplePuzzle.EmptyCell);

        Assert.Equal(CandidateSet.Of(9), session.Board[peer].Notes);
    }

    [Fact]
    public void IsUndoableInOneStep()
    {
        var session = SamplePuzzle.NewSession();
        var before = session.Board;

        CreateUseCase().Execute(session, SamplePuzzle.EmptyCell);
        session.Undo();

        Assert.Equal(before, session.Board);
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void FullBoard_ReturnsNullWithoutChanges()
    {
        var full = Board.FromGivens(SamplePuzzle.Solution);
        var session = new GameSession(new Puzzle(full, full, Difficulty.Easy));

        var hint = CreateUseCase().Execute(session);

        Assert.Null(hint);
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void UnsolvableGivens_PropagatesExceptionAndKeepsBoard()
    {
        var session = SamplePuzzle.NewSession();
        var before = session.Board;

        Assert.Throws<InvalidOperationException>(() => CreateUseCase(solution: null).Execute(session));
        Assert.Equal(before, session.Board);
    }
}
