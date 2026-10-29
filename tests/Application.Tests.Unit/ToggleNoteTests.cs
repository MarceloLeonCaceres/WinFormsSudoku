using Application.Tests.Unit.TestData;
using Domain;
using global::Sudoku.Application.UseCases;

namespace Application.Tests.Unit;

public class ToggleNoteTests
{
    private readonly ToggleNote _toggleNote = new();

    [Fact]
    public void AddsNote_AndTogglingAgainRemovesIt()
    {
        var session = SamplePuzzle.NewSession();

        Assert.True(_toggleNote.Execute(session, SamplePuzzle.EmptyCell, 6));
        Assert.True(session.Board[SamplePuzzle.EmptyCell].Notes.Contains(6));

        Assert.True(_toggleNote.Execute(session, SamplePuzzle.EmptyCell, 6));
        Assert.True(session.Board[SamplePuzzle.EmptyCell].Notes.IsEmpty);
    }

    [Fact]
    public void KeepsOtherNotes()
    {
        var session = SamplePuzzle.NewSession();
        _toggleNote.Execute(session, SamplePuzzle.EmptyCell, 2);
        _toggleNote.Execute(session, SamplePuzzle.EmptyCell, 6);

        Assert.Equal(CandidateSet.Of(2, 6), session.Board[SamplePuzzle.EmptyCell].Notes);
    }

    [Fact]
    public void CellWithPlayerValue_ReturnsFalse()
    {
        var session = SamplePuzzle.NewSession();
        new PlaceValue().Execute(session, SamplePuzzle.EmptyCell, 4);

        Assert.False(_toggleNote.Execute(session, SamplePuzzle.EmptyCell, 6));
        Assert.True(session.Board[SamplePuzzle.EmptyCell].Notes.IsEmpty);
    }

    [Fact]
    public void GivenCell_ReturnsFalse()
    {
        var session = SamplePuzzle.NewSession();

        Assert.False(_toggleNote.Execute(session, SamplePuzzle.GivenCell, 6));
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void IsUndoable()
    {
        var session = SamplePuzzle.NewSession();
        _toggleNote.Execute(session, SamplePuzzle.EmptyCell, 6);

        session.Undo();

        Assert.True(session.Board[SamplePuzzle.EmptyCell].Notes.IsEmpty);
    }
}