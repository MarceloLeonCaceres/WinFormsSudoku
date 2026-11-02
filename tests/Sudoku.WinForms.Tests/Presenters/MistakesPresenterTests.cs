using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Puzzles;
using Sudoku.Application.Settings;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;

public sealed class MistakesPresenterTests : IDisposable
{
    private const string SolutionDigits =
        "534678912672195348198342567859761423426853791713924856961537284287419635345286179";

    private static readonly Board Solution = Board.FromGivens(SolutionDigits);
    private static readonly Position WrongCell = new(0, 1); // la solución tiene 3

    private readonly StubMainView _view = new();
    private readonly CurrentGame _current = new();
    private readonly MemorySettingsRepository _settings = new();
    private readonly RecordingDialogs _dialogs = new();
    private readonly ManualClock _clock = new();
    private MistakesPresenter? _presenter;

    public void Dispose() => _presenter?.Dispose();

    private MistakesPresenter Create(bool solvable = true)
    {
        var solver = new StubSolver(solvable ? Solution : null);
        _presenter = new MistakesPresenter(
            _view,
            _view,
            _current,
            new GetMistakes(new SolveGame(solver)),
            new GetSettings(_settings),
            new ChangeShowMistakes(_settings),
            _dialogs);
        return _presenter;
    }

    private void EnableOption() => _settings.Stored = AppSettings.Default with { ShowMistakes = true };

    private GameSession NewSessionWithWrongValue()
    {
        var puzzle = new Puzzle(Board.FromGivens("5" + new string('.', 80)), Solution, Difficulty.Easy);
        var session = new GameSession(puzzle, _clock);
        session.Apply(session.Board.WithValue(WrongCell, 9));
        return session;
    }

    [Fact]
    public void Constructor_ShowsTheSavedOption()
    {
        EnableOption();

        Create();

        Assert.True(_view.ShownMistakesOption);
    }

    [Fact]
    public void Constructor_WithCorruptSettings_UsesTheOptionDisabled()
    {
        _settings.LoadFailure = new CorruptSaveException("dañado");

        Create();

        Assert.False(_view.ShownMistakesOption);
    }

    [Fact]
    public void OptionDisabled_DoesNotSendMistakes()
    {
        Create();

        _current.Set(NewSessionWithWrongValue());

        Assert.Null(_view.Mistakes);
    }

    [Fact]
    public void OptionEnabled_NewSession_SendsTheWrongCells()
    {
        EnableOption();
        Create();

        _current.Set(NewSessionWithWrongValue());

        Assert.True(_view.Mistakes!.Contains(WrongCell));
    }

    [Fact]
    public void OptionEnabled_BoardChanged_RecalculatesTheMistakes()
    {
        EnableOption();
        Create();
        var session = NewSessionWithWrongValue();
        _current.Set(session);

        session.Apply(session.Board.WithCleared(WrongCell));
        _current.NotifyBoardChanged();

        Assert.Empty(_view.Mistakes!);
    }

    [Fact]
    public void EnablingTheOption_PersistsItAndSendsTheMistakes()
    {
        Create();
        _current.Set(NewSessionWithWrongValue());

        _view.RaiseShowMistakesChanged(true);

        Assert.True(_settings.Stored.ShowMistakes);
        Assert.True(_view.Mistakes!.Contains(WrongCell));
    }

    [Fact]
    public void DisablingTheOption_PersistsItAndClearsTheMistakes()
    {
        EnableOption();
        Create();
        _current.Set(NewSessionWithWrongValue());

        _view.RaiseShowMistakesChanged(false);

        Assert.False(_settings.Stored.ShowMistakes);
        Assert.Null(_view.Mistakes);
    }

    [Fact]
    public void EnablingTheOption_WhenSavingFails_ShowsErrorAndRestoresTheCheckbox()
    {
        _settings.SaveFailure = new IOException("disco lleno");
        Create();
        _current.Set(NewSessionWithWrongValue());

        _view.RaiseShowMistakesChanged(true);

        Assert.Single(_dialogs.Errors);
        Assert.False(_view.ShownMistakesOption);
        Assert.Null(_view.Mistakes);
    }

    [Fact]
    public void GivensWithoutSolution_FallsBackToConflictsAndReportsItInTheStatus()
    {
        EnableOption();
        Create(solvable: false);

        _current.Set(NewSessionWithWrongValue());

        Assert.Null(_view.Mistakes);
        Assert.Contains("errores", _view.LastStatus!);
    }

    [Fact]
    public void AfterDispose_EventsAreIgnored()
    {
        Create();
        _presenter!.Dispose();

        _view.RaiseShowMistakesChanged(true);

        Assert.False(_settings.Stored.ShowMistakes);
    }
}
