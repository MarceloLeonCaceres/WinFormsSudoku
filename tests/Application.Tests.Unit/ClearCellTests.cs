using Application.Tests.Unit.TestData;
using Domain;
using global::Sudoku.Application.UseCases;

namespace Application.Tests.Unit;

public class ClearCellTests
{
    private readonly ClearCell _clearCell = new();

    [Fact]
    public void ClearsPlayerValue()
    {
        var session = SamplePuzzle.NewSession();
        new PlaceValue().Execute(session, SamplePuzzle.EmptyCell, 4);

        Assert.True(_clearCell.Execute(session, SamplePuzzle.EmptyCell));
        Assert.True(session.Board[SamplePuzzle.EmptyCell].IsEmpty);
    }

    [Fact]
    public void ClearsNotesOfEmptyCell()
    {
        var session = SamplePuzzle.NewSession();
        session.Apply(session.Board.WithNotes(SamplePuzzle.EmptyCell, CandidateSet.Of(2, 3)));

        Assert.True(_clearCell.Execute(session, SamplePuzzle.EmptyCell));
        Assert.True(session.Board[SamplePuzzle.EmptyCell].Notes.IsEmpty);
    }

    [Fact]
    public void GivenCell_ReturnsFalse()
    {
        var session = SamplePuzzle.NewSession();

        Assert.False(_clearCell.Execute(session, SamplePuzzle.GivenCell));
        Assert.Equal(5, session.Board[SamplePuzzle.GivenCell].Value);
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void AlreadyEmptyCellWithoutNotes_ReturnsFalse()
    {
        var session = SamplePuzzle.NewSession();

        Assert.False(_clearCell.Execute(session, SamplePuzzle.EmptyCell));
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void IsUndoable()
    {
        var session = SamplePuzzle.NewSession();
        new PlaceValue().Execute(session, SamplePuzzle.EmptyCell, 4);
        _clearCell.Execute(session, SamplePuzzle.EmptyCell);

        session.Undo();

        Assert.Equal(4, session.Board[SamplePuzzle.EmptyCell].Value);
    }
}
