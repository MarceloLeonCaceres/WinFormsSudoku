using Application.Tests.Unit.TestData;
using Domain;
using global::Sudoku.Application.UseCases;

namespace Application.Tests.Unit;

public class PlaceValueTests
{
    private readonly PlaceValue _placeValue = new();

    [Fact]
    public void PlacesPlayerValueInEmptyCell()
    {
        var session = SamplePuzzle.NewSession();

        Assert.True(_placeValue.Execute(session, SamplePuzzle.EmptyCell, 4));

        var cell = session.Board[SamplePuzzle.EmptyCell];
        Assert.Equal(4, cell.Value);
        Assert.False(cell.IsGiven);
    }

    [Fact]
    public void GivenCell_ReturnsFalseAndKeepsBoardAndHistory()
    {
        var session = SamplePuzzle.NewSession();
        var before = session.Board;

        Assert.False(_placeValue.Execute(session, SamplePuzzle.GivenCell, 9));
        Assert.Equal(before, session.Board);
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void ReplacesExistingPlayerValue()
    {
        var session = SamplePuzzle.NewSession();
        _placeValue.Execute(session, SamplePuzzle.EmptyCell, 4);

        Assert.True(_placeValue.Execute(session, SamplePuzzle.EmptyCell, 7));
        Assert.Equal(7, session.Board[SamplePuzzle.EmptyCell].Value);
    }

    [Fact]
    public void SameValueTwice_SecondCallReturnsFalseWithoutExtraHistory()
    {
        var session = SamplePuzzle.NewSession();
        _placeValue.Execute(session, SamplePuzzle.EmptyCell, 4);

        Assert.False(_placeValue.Execute(session, SamplePuzzle.EmptyCell, 4));

        session.Undo();
        Assert.False(session.CanUndo);
    }

    [Fact]
    public void ClearsOwnNotes()
    {
        var session = SamplePuzzle.NewSession();
        session.Apply(session.Board.WithNotes(SamplePuzzle.EmptyCell, CandidateSet.Of(1, 4)));

        _placeValue.Execute(session, SamplePuzzle.EmptyCell, 4);

        Assert.True(session.Board[SamplePuzzle.EmptyCell].Notes.IsEmpty);
    }

    [Fact]
    public void RemovesValueFromPeerNotes_AndKeepsNonPeersAndOtherNotes()
    {
        var peers = new[] { new Position(0, 3), new Position(1, 2), new Position(8, 2) }; // fila, caja/columna, columna
        var nonPeer = new Position(8, 0);
        var session = SamplePuzzle.NewSession();
        foreach (var p in peers.Append(nonPeer))
            session.Apply(session.Board.WithNotes(p, CandidateSet.Of(4, 9)));

        _placeValue.Execute(session, SamplePuzzle.EmptyCell, 4);

        foreach (var p in peers)
            Assert.Equal(CandidateSet.Of(9), session.Board[p].Notes);
        Assert.Equal(CandidateSet.Of(4, 9), session.Board[nonPeer].Notes);
    }

    [Fact]
    public void AllowsConflictingValue_AndBothCellsAreReportedAsConflicts()
    {
        var session = SamplePuzzle.NewSession();

        Assert.True(_placeValue.Execute(session, SamplePuzzle.EmptyCell, 5)); // 5 ya está en (0,0)

        var conflicts = SudokuRules.FindConflicts(session.Board);
        Assert.Contains(SamplePuzzle.EmptyCell, conflicts);
        Assert.Contains(SamplePuzzle.GivenCell, conflicts);
    }

    [Fact]
    public void ValueAndPeerNotesCleanup_UndoTogetherInOneStep()
    {
        var session = SamplePuzzle.NewSession();
        session.Apply(session.Board.WithNotes(new Position(0, 3), CandidateSet.Of(4, 9)));
        var before = session.Board;

        _placeValue.Execute(session, SamplePuzzle.EmptyCell, 4);
        session.Undo();

        Assert.Equal(before, session.Board);
    }
}
