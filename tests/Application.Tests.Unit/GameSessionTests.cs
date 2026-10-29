using Application.Tests.Unit.TestData;
using Domain;
using global::Sudoku.Application.Games;

namespace Application.Tests.Unit;

public class GameSessionTests
{
    private static Board WithFour(GameSession s) => s.Board.WithValue(SamplePuzzle.EmptyCell, 4);

    [Fact]
    public void NewSession_StartsFromPuzzleWithoutHistory()
    {
        var puzzle = SamplePuzzle.Create(Difficulty.Hard);
        var session = new GameSession(puzzle);

        Assert.Equal(puzzle.Board, session.Board);
        Assert.Equal(Difficulty.Hard, session.Difficulty);
        Assert.False(session.CanUndo);
        Assert.False(session.CanRedo);
    }

    [Fact]
    public void Apply_ChangedBoard_UpdatesBoardAndEnablesUndo()
    {
        var session = SamplePuzzle.NewSession();
        var next = WithFour(session);

        Assert.True(session.Apply(next));
        Assert.Equal(next, session.Board);
        Assert.True(session.CanUndo);
    }

    [Fact]
    public void Apply_SameBoard_ReturnsFalseAndKeepsHistoryEmpty()
    {
        var session = SamplePuzzle.NewSession();

        Assert.False(session.Apply(session.Board));
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void Apply_Null_Throws()
    {
        var session = SamplePuzzle.NewSession();
        Assert.Throws<ArgumentNullException>(() => session.Apply(null!));
    }

    [Fact]
    public void Undo_RestoresPreviousBoardAndEnablesRedo()
    {
        var session = SamplePuzzle.NewSession();
        var original = session.Board;
        session.Apply(WithFour(session));

        Assert.True(session.Undo());
        Assert.Equal(original, session.Board);
        Assert.False(session.CanUndo);
        Assert.True(session.CanRedo);
    }

    [Fact]
    public void Redo_ReappliesUndoneBoard()
    {
        var session = SamplePuzzle.NewSession();
        var next = WithFour(session);
        session.Apply(next);
        session.Undo();

        Assert.True(session.Redo());
        Assert.Equal(next, session.Board);
        Assert.False(session.CanRedo);
    }

    [Fact]
    public void Apply_AfterUndo_ClearsRedoHistory()
    {
        var session = SamplePuzzle.NewSession();
        session.Apply(WithFour(session));
        session.Undo();

        session.Apply(session.Board.WithValue(SamplePuzzle.EmptyCell, 7));

        Assert.False(session.CanRedo);
    }

    [Fact]
    public void UndoAndRedo_WithEmptyHistory_ReturnFalse()
    {
        var session = SamplePuzzle.NewSession();

        Assert.False(session.Undo());
        Assert.False(session.Redo());
    }
}
