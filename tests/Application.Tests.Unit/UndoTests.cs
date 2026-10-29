using Application.Tests.Unit.TestData;
using global::Sudoku.Application.UseCases;

namespace Application.Tests.Unit;

public class UndoTests
{
    [Fact]
    public void RevertsLastAction()
    {
        var session = SamplePuzzle.NewSession();
        var original = session.Board;
        new PlaceValue().Execute(session, SamplePuzzle.EmptyCell, 4);

        Assert.True(new Undo().Execute(session));
        Assert.Equal(original, session.Board);
    }

    [Fact]
    public void WithoutHistory_ReturnsFalse()
        => Assert.False(new Undo().Execute(SamplePuzzle.NewSession()));
}

public class RedoTests
{
    [Fact]
    public void ReappliesUndoneAction()
    {
        var session = SamplePuzzle.NewSession();
        new PlaceValue().Execute(session, SamplePuzzle.EmptyCell, 4);
        new Undo().Execute(session);

        Assert.True(new Redo().Execute(session));
        Assert.Equal(4, session.Board[SamplePuzzle.EmptyCell].Value);
    }

    [Fact]
    public void WithoutUndoneActions_ReturnsFalse()
        => Assert.False(new Redo().Execute(SamplePuzzle.NewSession()));
}
