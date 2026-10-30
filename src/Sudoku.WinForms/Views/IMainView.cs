using Domain;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Views;

/// <summary>Contrato de la vista principal. No expone controles WinForms.</summary>
public interface IMainView
{
    Difficulty SelectedDifficulty { get; }
    ThemeKind SelectedTheme { get; }
    bool NotesMode { get; }

    event EventHandler? NewGameRequested;
    event EventHandler? DifficultyChanged;
    event EventHandler? UndoRequested;
    event EventHandler? RedoRequested;
    event EventHandler? NotesModeChanged;
    event EventHandler? HintRequested;
    event EventHandler? SolveRequested;
    event EventHandler? PauseRequested;
    event EventHandler? StatisticsRequested;
    event EventHandler? ThemeChangeRequested;

    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    event EventHandler<ValueEnteredEventArgs>? ValueEntered;
    event EventHandler? ClearRequested;

    void SetBoard(Board board);
    void SetDifficulty(Difficulty difficulty);
    void SetElapsedTime(TimeSpan elapsedTime);
    void SetStatus(string message);
    void SetNotesMode(bool enabled);
    void SetPaused(bool paused);
    void SetBusy(bool busy);
    void SetUndoRedoState(bool canUndo, bool canRedo);
    void SetTheme(Theme theme);
}
