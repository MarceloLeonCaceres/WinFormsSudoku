using Domain;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Views;

/// <summary>Contrato de la barra lateral y los controles de juego. No expone controles WinForms.</summary>
public interface IGameControlsView
{
    Difficulty SelectedDifficulty { get; }
    ThemeKind SelectedTheme { get; }

    event EventHandler? NewGameRequested;
    /// <summary>El usuario pidió cancelar la generación en curso (el botón de nueva partida hace de "Cancelar").</summary>
    event EventHandler? CancelRequested;
    event EventHandler? DifficultyChanged;
    event EventHandler? UndoRequested;
    event EventHandler? RedoRequested;
    event EventHandler? NotesModeChanged;
    event EventHandler? HintRequested;
    event EventHandler? SolveRequested;
    event EventHandler? PauseRequested;
    event EventHandler? StatisticsRequested;
    event EventHandler? ThemeChangeRequested;
    /// <summary>El usuario pulsó «Guardar».</summary>
    event EventHandler? SaveRequested;
    /// <summary>El usuario pulsó «Continuar» (reanudar la partida guardada).</summary>
    event EventHandler? ContinueRequested;
    void SetDifficulty(Difficulty difficulty);
    void SetElapsedTime(TimeSpan elapsedTime);
    void SetStatus(string message);
    void SetNotesMode(bool enabled);
    void SetPaused(bool paused);
    /// <summary>Mientras está ocupada, "Nueva partida" pasa a "Cancelar" y el resto de acciones se deshabilita.</summary>
    void SetBusy(bool busy);
    void SetUndoRedoState(bool canUndo, bool canRedo);
    void SetTheme(Theme theme);
    /// <summary>Muestra el tema en el combo SIN disparar <see cref="ThemeChangeRequested"/>.</summary>
    void SetSelectedTheme(ThemeKind kind);
}
