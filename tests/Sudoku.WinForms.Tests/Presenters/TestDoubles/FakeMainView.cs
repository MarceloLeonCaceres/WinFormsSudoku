using Domain;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Theming;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

#pragma warning disable CS0067 // Eventos del contrato que estas pruebas no disparan
internal sealed class FakeMainView : IMainView
{
    public Difficulty SelectedDifficulty { get; set; } = Difficulty.Easy;
    public ThemeKind SelectedTheme { get; set; } = ThemeKind.Dark;
    public bool NotesMode { get; set; }

    public event EventHandler? NewGameRequested;
    public event EventHandler? DifficultyChanged;
    public event EventHandler? UndoRequested;
    public event EventHandler? RedoRequested;
    public event EventHandler? NotesModeChanged;
    public event EventHandler? HintRequested;
    public event EventHandler? SolveRequested;
    public event EventHandler? PauseRequested;
    public event EventHandler? StatisticsRequested;
    public event EventHandler? ThemeChangeRequested;
    public event EventHandler? CancelRequested;
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<ValueEnteredEventArgs>? ValueEntered;
    public event EventHandler? ClearRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? ContinueRequested;
    public event EventHandler? WindowClosing;

    public Board? LastBoard { get; private set; }
    public Difficulty? ShownDifficulty { get; private set; }
    public bool? LastPaused { get; private set; }
    public bool IsBusy { get; private set; }
    public List<bool> BusyChanges { get; } = [];
    public List<string> Statuses { get; } = [];
    public List<bool> PausedChanges { get; } = [];

    public void RaiseNewGameRequested() => NewGameRequested?.Invoke(this, EventArgs.Empty);
    public void RaiseCancelRequested() => CancelRequested?.Invoke(this, EventArgs.Empty);
    public void RaisePauseRequested() => PauseRequested?.Invoke(this, EventArgs.Empty);


    public void SetBoard(Board board) => LastBoard = board;
    public void SetDifficulty(Difficulty difficulty) => ShownDifficulty = difficulty;
    public void SetPaused(bool paused)
    {
        LastPaused = paused;
        PausedChanges.Add(paused);
    }
    public void SetStatus(string message) => Statuses.Add(message);
    public void SetBusy(bool busy)
    {
        IsBusy = busy;
        BusyChanges.Add(busy);
    }

    public void SetElapsedTime(TimeSpan elapsedTime) { }
    public void SetNotesMode(bool enabled) { }
    public void SetUndoRedoState(bool canUndo, bool canRedo) { }
    public void SetTheme(Theme theme) { }

    public void SetSelectedTheme(ThemeKind kind)
    {
        throw new NotImplementedException();
    }
}
#pragma warning restore CS0067
