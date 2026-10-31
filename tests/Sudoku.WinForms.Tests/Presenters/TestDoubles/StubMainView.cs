using Domain;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Theming;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

#pragma warning disable CS0067 // eventos de la interfaz que estos tests no disparan
public sealed class StubMainView : IMainView
{
    public Difficulty SelectedDifficulty { get; set; } = Difficulty.Easy;
    public ThemeKind SelectedTheme { get; set; } = ThemeKind.Dark;
    public bool NotesMode { get; set; }

    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<ValueEnteredEventArgs>? ValueEntered;
    public event EventHandler? ClearRequested;
    public event EventHandler? NewGameRequested;
    public event EventHandler? CancelRequested;
    public event EventHandler? DifficultyChanged;
    public event EventHandler? UndoRequested;
    public event EventHandler? RedoRequested;
    public event EventHandler? NotesModeChanged;
    public event EventHandler? HintRequested;
    public event EventHandler? SolveRequested;
    public event EventHandler? PauseRequested;
    public event EventHandler? StatisticsRequested;
    public event EventHandler? ThemeChangeRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? ContinueRequested;
    public event EventHandler? WindowClosing;

    public Board? LastBoard { get; private set; }
    public List<string> Statuses { get; } = [];
    public string? LastStatus => Statuses.LastOrDefault();
    public (bool CanUndo, bool CanRedo) UndoRedo { get; private set; }
    public bool? Paused { get; private set; }
    public TimeSpan Elapsed { get; private set; }
    public Difficulty? ShownDifficulty { get; private set; }
    public ThemeKind? SelectedThemeSet { get; private set; }
    public Theme? AppliedTheme { get; private set; }

    public void SetBoard(Board board) => LastBoard = board;
    public void SetDifficulty(Difficulty difficulty) => ShownDifficulty = difficulty;
    public void SetElapsedTime(TimeSpan elapsedTime) => Elapsed = elapsedTime;
    public void SetStatus(string message) => Statuses.Add(message);
    public void SetNotesMode(bool enabled) => NotesMode = enabled;
    public void SetPaused(bool paused) => Paused = paused;
    public void SetBusy(bool busy) { }
    public void SetUndoRedoState(bool canUndo, bool canRedo) => UndoRedo = (canUndo, canRedo);
    public void SetTheme(Theme theme) => AppliedTheme = theme;
    public void SetSelectedTheme(ThemeKind kind) => SelectedThemeSet = kind;

    public void RaiseSelection(Position position) => SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(position));
    public void RaiseUndo() => UndoRequested?.Invoke(this, EventArgs.Empty);
    public void RaiseRedo() => RedoRequested?.Invoke(this, EventArgs.Empty);
    public void RaiseHint() => HintRequested?.Invoke(this, EventArgs.Empty);
    public void RaiseSolve() => SolveRequested?.Invoke(this, EventArgs.Empty);
    public void RaiseStatistics() => StatisticsRequested?.Invoke(this, EventArgs.Empty);
    public void RaiseThemeChange() => ThemeChangeRequested?.Invoke(this, EventArgs.Empty);
    public void RaiseSave() => SaveRequested?.Invoke(this, EventArgs.Empty);
    public void RaiseContinue() => ContinueRequested?.Invoke(this, EventArgs.Empty);
    public void RaiseClosing() => WindowClosing?.Invoke(this, EventArgs.Empty);
}
#pragma warning restore CS0067
