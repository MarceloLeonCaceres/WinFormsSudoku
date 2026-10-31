using Sudoku.Application.Games;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;

public sealed class BoardPresenterTests : IDisposable
{
    private readonly FakeBoardView _view = new();
    private readonly CurrentGame _current = new();
    private readonly GameSession _session;
    private readonly BoardPresenter _presenter;

    public BoardPresenterTests()
    {
        _session = new GameSession(SamplePuzzle.Create(), new FakeClock());
        _session.Start();
        _current.Set(_session);
        _presenter = CreatePresenter(_view, _current);
    }

    public void Dispose() => _presenter.Dispose();

    private static BoardPresenter CreatePresenter(FakeBoardView view, CurrentGame current) =>
        new(view, current, new PlaceValue(), new ToggleNote(), new ClearCell());

    [Fact]
    public void ValueEntered_PlacesValueInSelectedCell_AndRefreshesView()
    {
        _view.RaiseSelection(SamplePuzzle.EmptyCell);
        _view.RaiseValueEntered(4);

        Assert.Equal((int?)4, _session.Board[SamplePuzzle.EmptyCell].Value);
        Assert.Same(_session.Board, _view.LastBoard);
    }

    [Fact]
    public void ValueEntered_InNotesMode_TogglesNoteInsteadOfPlacingValue()
    {
        _view.NotesMode = true;
        _view.RaiseSelection(SamplePuzzle.EmptyCell);
        _view.RaiseValueEntered(4);

        var cell = _session.Board[SamplePuzzle.EmptyCell];
        Assert.Null(cell.Value);
        Assert.True(cell.Notes.Contains(4));
        Assert.Equal(1, _view.SetBoardCalls);
    }

    [Fact]
    public void ValueEntered_WithoutSelection_DoesNothing()
    {
        _view.RaiseValueEntered(4);

        Assert.Equal(0, _view.SetBoardCalls);
        Assert.Null(_session.Board[SamplePuzzle.EmptyCell].Value);
    }

    [Fact]
    public void ValueEntered_WhenPaused_IsIgnored()
    {
        _session.Pause();
        _view.RaiseSelection(SamplePuzzle.EmptyCell);
        _view.RaiseValueEntered(4);

        Assert.Equal(0, _view.SetBoardCalls);
        Assert.Null(_session.Board[SamplePuzzle.EmptyCell].Value);
    }

    [Fact]
    public void ValueEntered_OnGivenCell_DoesNotRefreshView()
    {
        _view.RaiseSelection(SamplePuzzle.GivenCell);
        _view.RaiseValueEntered(9);

        Assert.Equal(0, _view.SetBoardCalls);
        Assert.Equal((int?)5, _session.Board[SamplePuzzle.GivenCell].Value);
    }

    [Fact]
    public void ClearRequested_RemovesPlayerValue()
    {
        _view.RaiseSelection(SamplePuzzle.EmptyCell);
        _view.RaiseValueEntered(4);
        _view.RaiseClear();

        Assert.Null(_session.Board[SamplePuzzle.EmptyCell].Value);
        Assert.Equal(2, _view.SetBoardCalls);
    }

    [Fact]
    public void Events_WithoutActiveSession_AreIgnored()
    {
        var view = new FakeBoardView();
        using var presenter = CreatePresenter(view, new CurrentGame());

        view.RaiseSelection(SamplePuzzle.EmptyCell);
        view.RaiseValueEntered(4);
        view.RaiseClear();

        Assert.Equal(0, view.SetBoardCalls);
    }

    [Fact]
    public void AfterDispose_EventsAreIgnored()
    {
        _presenter.Dispose();

        _view.RaiseSelection(SamplePuzzle.EmptyCell);
        _view.RaiseValueEntered(4);

        Assert.Equal(0, _view.SetBoardCalls);
    }
}
