# Resumen de Firmas Públicas (Optimizado para IA)

## Archivos procesados por proyecto
### src
- **Application**: 35 archivo(s)
- **Domain**: 8 archivo(s)
- **Infrastructure**: 10 archivo(s)
- **Sudoku.WinForms**: 38 archivo(s)

### tests
- **Application.Tests.Unit**: 21 archivo(s)
- **Domain.Tests.Unit**: 7 archivo(s)
- **Infrastructure.Tests.Unit**: 6 archivo(s)
- **Sudoku.WinForms.Tests**: 25 archivo(s)

---

## Proyecto: src/Application

```csharp
// === src/Application/Games/CorruptSaveException.cs ===
namespace Sudoku.Application.Games;
public sealed class CorruptSaveException : Exception
{
    public CorruptSaveException(string message) : base(message);
    public CorruptSaveException(string message, Exception innerException) : base(message, innerException);
}

// === src/Application/Games/GameSession.cs ===
using Domain;
using global::Sudoku.Application.Puzzles;
using Sudoku.Application.Ports;

namespace Sudoku.Application.Games;
public sealed class GameSession
{
    public GameSession(Puzzle puzzle, IClock clock) : this(puzzle.Board, puzzle.Difficulty, TimeSpan.Zero, clock);
    public GameSession(Puzzle puzzle);
    public static GameSession Restore(SavedGame saved, IClock clock);
    public Board Board { get; private set; }
    public Difficulty Difficulty { get; }
    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    public TimeSpan ElapsedTime => _timer.Elapsed;
    public bool IsRunning => _timer.IsRunning;

    public void Start();
    public void Pause();
    public bool Apply(Board next);
    public bool Undo();
    public bool Redo();
}

// === src/Application/Games/PlayTimer.cs ===
using global::Sudoku.Application.Ports;

namespace Sudoku.Application.Games;
public sealed class PlayTimer
{
    public PlayTimer(IClock clock, TimeSpan accumulated = default);
    public bool IsRunning => _startedAt is not null;
    public TimeSpan Elapsed => _accumulated + CurrentLeg();

    public void Start();
    public void Pause();
}

// === src/Application/Games/SavedGame.cs ===
using Domain;

namespace Sudoku.Application.Games;
public sealed record SavedGame(Board Board, Difficulty Difficulty, TimeSpan Elapsed);

// === src/Application/Ports/IClock.cs ===
namespace Sudoku.Application.Ports;
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

// === src/Application/Ports/IGameRepository.cs ===
using global::Sudoku.Application.Games;

namespace Sudoku.Application.Ports;
public interface IGameRepository
{
    void Save(SavedGame game);
    SavedGame? Load();
    void Delete();
}

// === src/Application/Ports/IPuzzleGenerator.cs ===
using Domain;
using Sudoku.Application.Puzzles;

namespace Sudoku.Application.Ports;
public interface IPuzzleGenerator
{
    Puzzle Generate(Difficulty difficulty, CancellationToken ct = default);
}

// === src/Application/Ports/ISettingsRepository.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.Settings;

namespace Sudoku.Application.Ports;
public interface ISettingsRepository
{
    AppSettings Load();
    void Save(AppSettings settings);
}

// === src/Application/Ports/ISolver.cs ===
using Domain;

namespace Sudoku.Application.Ports;
public interface ISolver
{
    Board? Solve(Board board);
    int CountSolutions(Board board, int limit);
}

// === src/Application/Ports/IStatisticsRepository.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.Statistics;

namespace Sudoku.Application.Ports;
public interface IStatisticsRepository
{
    GameStatistics Load();
    void Save(GameStatistics statistics);
}

// === src/Application/Puzzles/DifficultyProfile.cs ===
using Domain;

namespace Sudoku.Application.Puzzles;
public sealed record DifficultyProfile(Difficulty Difficulty, int MinGivens, int MaxGivens)
{
    public static DifficultyProfile For(Difficulty difficulty);
}

// === src/Application/Puzzles/Puzzle.cs ===
using Domain;

namespace Sudoku.Application.Puzzles;
public sealed record Puzzle(Board Board, Board Solution, Difficulty Difficulty);

// === src/Application/Settings/AppSettings.cs ===
namespace Sudoku.Application.Settings;
public sealed record AppSettings(ThemeKind Theme)
{
    public static AppSettings Default { get; }
}

// === src/Application/Settings/ThemeKind.cs ===
namespace Sudoku.Application.Settings;
public enum ThemeKind
{
    Light,
    Dark
}

// === src/Application/Statistics/DifficultyStatistics.cs ===
using System;
using System.Collections.Generic;
using System.Text;

namespace Sudoku.Application.Statistics;
public sealed record DifficultyStatistics(int Started, int Won, int Lost, TimeSpan? BestTime, TimeSpan TotalWonTime)
{
    public static DifficultyStatistics Empty { get; }
    public TimeSpan? AverageWonTime => Won == 0 ? null : TotalWonTime / Won;

    public DifficultyStatistics WithStarted();
    public DifficultyStatistics WithFinished(GameOutcome outcome, TimeSpan elapsed);
}

// === src/Application/Statistics/GameOutcome.cs ===
namespace Sudoku.Application.Statistics;
public enum GameOutcome
{
    Won,
    Lost
}

// === src/Application/Statistics/GameRecord.cs ===
using Domain;

namespace Sudoku.Application.Statistics;
public sealed record GameRecord(Difficulty Difficulty, GameOutcome Outcome, TimeSpan Elapsed, DateTimeOffset FinishedAt);

// === src/Application/Statistics/GameStatistics.cs ===
using Domain;
using System.Collections.Immutable;

namespace Sudoku.Application.Statistics;
public sealed record GameStatistics(ImmutableDictionary<Difficulty, DifficultyStatistics> ByDifficulty, ImmutableList<GameRecord> Recent)
{
    public const int MaxRecentGames = 20;
    public static GameStatistics Empty { get; }

    public DifficultyStatistics For(Difficulty difficulty);
    public GameStatistics WithGameStarted(Difficulty difficulty);
    public GameStatistics WithGameFinished(GameRecord record);
}

// === src/Application/UseCases/ApplyHint.cs ===
using Domain;
using global::Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
public sealed class ApplyHint(GetHint getHint, PlaceValue placeValue)
{
    public Hint? Execute(GameSession session, Position? preferred = null);
}

// === src/Application/UseCases/ChangeTheme.cs ===
using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Sudoku.Application.UseCases;
public sealed class ChangeTheme(ISettingsRepository repository)
{
    public bool Execute(ThemeKind theme);
}

// === src/Application/UseCases/ClearCell.cs ===
using Domain;
using global::Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
public sealed class ClearCell
{
    public bool Execute(GameSession session, Position position);
}

// === src/Application/UseCases/GetHint.cs ===
using Sudoku.Application.Ports;
using Domain;

namespace Sudoku.Application.UseCases;
public sealed class GetHint(ISolver solver)
{
    public Hint? Execute(Board board, Position? preferred = null);
}

// === src/Application/UseCases/GetSettings.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Sudoku.Application.UseCases;
public sealed class GetSettings(ISettingsRepository repository)
{
    public AppSettings Execute();
}

// === src/Application/UseCases/GetStatistics.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;

namespace Sudoku.Application.UseCases;
public sealed class GetStatistics(IStatisticsRepository repository)
{
    public GameStatistics Execute();
}

// === src/Application/UseCases/Hint.cs ===
using Domain;

namespace Sudoku.Application.UseCases;
public readonly record struct Hint(Position Position, int Value);

// === src/Application/UseCases/LoadGame.cs ===
using global::Sudoku.Application.Games;
using global::Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;
public sealed class LoadGame(IGameRepository repository, IClock clock)
{
    public GameSession? Execute();
}

// === src/Application/UseCases/PlaceValue.cs ===
using Domain;
using global::Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
public sealed class PlaceValue
{
    public bool Execute(GameSession session, Position position, int value);
}

// === src/Application/UseCases/RecordGameFinished.cs ===
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;

namespace Sudoku.Application.UseCases;
public sealed class RecordGameFinished(IStatisticsRepository repository, IClock clock)
{
    public void Execute(GameSession session, GameOutcome outcome);
}

// === src/Application/UseCases/RecordGameStarted.cs ===
using Domain;
using Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;
public sealed class RecordGameStarted(IStatisticsRepository repository)
{
    public void Execute(Difficulty difficulty);
}

// === src/Application/UseCases/Redo.cs ===
using Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
public sealed class Redo
{
    public bool Execute(GameSession session);
}

// === src/Application/UseCases/SaveGame.cs ===
using global::Sudoku.Application.Games;
using global::Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;
public sealed class SaveGame(IGameRepository repository)
{
    public void Execute(GameSession session);
}

// === src/Application/UseCases/SolveGame.cs ===
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;
public sealed class SolveGame(ISolver solver)
{
    public Board Execute(GameSession session);
}

// === src/Application/UseCases/StartNewGame.cs ===
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;
public sealed class StartNewGame(IPuzzleGenerator generator, IClock clock)
{
    public GameSession Execute(Difficulty difficulty, CancellationToken ct = default);
}

// === src/Application/UseCases/ToggleNote.cs ===
using Domain;
using Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
public sealed class ToggleNote
{
    public bool Execute(GameSession session, Position position, int value);
}

// === src/Application/UseCases/Undo.cs ===
using Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
public sealed class Undo
{
    public bool Execute(GameSession session);
}

```

## Proyecto: src/Domain

```csharp
// === src/Domain/Board.cs ===
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
public sealed class Board : IEquatable<Board>
{
    public static Board Empty { get; }

    public Cell this[Position position] => _cells[position.Index];
    public static Board FromGivens(string puzzle);
    public Board WithValue(Position position, int value);
    public Board WithNotes(Position position, CandidateSet notes);
    public Board WithCleared(Position position);
    public Board GivensOnly();
    public string ToValuesString();
    public bool Equals(Board? other);
    public override bool Equals(object? obj);
    public override int GetHashCode();
}

// === src/Domain/CandidateSet.cs ===
using System.Collections;
using System.Numerics;

namespace Domain;
public readonly struct CandidateSet : IEquatable<CandidateSet>, IEnumerable<int>
{
    public static CandidateSet Empty => default;
    public static CandidateSet All => new(AllBits);
    public int Count => BitOperations.PopCount(_bits);
    public bool IsEmpty => _bits == 0;

    public static CandidateSet Of(params int[] values);
    public bool Contains(int value);
    public CandidateSet Add(int value);
    public CandidateSet Remove(int value);
    public CandidateSet Toggle(int value);
    public CandidateSet Union(CandidateSet other);
    public CandidateSet Except(CandidateSet other);
    public IEnumerator<int> GetEnumerator();
    public bool Equals(CandidateSet other);
    public override bool Equals(object? obj);
    public override int GetHashCode();
    public override string ToString();
    public static bool operator ==(CandidateSet left, CandidateSet right);
    public static bool operator !=(CandidateSet left, CandidateSet right);
}

// === src/Domain/Cell.cs ===
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
public readonly record struct Cell
{
    public int? Value { get; }
    public bool IsGiven { get; }
    public CandidateSet Notes { get; }
    public bool IsEmpty => Value is null;
    public static Cell Empty => default;

    public static Cell Given(int value);
    public Cell WithValue(int value);
    public Cell WithNotes(CandidateSet notes);
    public Cell Cleared();
}

// === src/Domain/Difficulty.cs ===
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
public enum Difficulty
{
    Easy,
    Medium,
    Hard,
    Expert
}

// === src/Domain/Position.cs ===
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
public readonly record struct Position
{
    public Position(int row, int column);
    public int Row { get; }
    public int Column { get; }
    public int Box => (Row / SudokuConstants.BoxSize) * SudokuConstants.BoxSize + Column / SudokuConstants.BoxSize;
    public int Index => Row * SudokuConstants.Size + Column;
    public static IReadOnlyList<Position> All { get; }

    public static Position FromIndex(int index);
}

// === src/Domain/SudokuConstants.cs ===
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
public static class SudokuConstants
{
    public const int Size = 9;
    public const int BoxSize = 3;
    public const int CellCount = Size * Size;
    public const int MinValue = 1;
    public const int MaxValue = Size;
}

// === src/Domain/SudokuRules.cs ===
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
public static class SudokuRules
{
    public static IReadOnlyList<Position> Peers(Position position);
    public static bool IsValidPlacement(Board board, Position position, int value);
    public static IReadOnlySet<Position> FindConflicts(Board board);
    public static bool IsComplete(Board board);
    public static bool IsSolved(Board board);
}

// === src/Domain/ValueGuard.cs ===
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
public static class ValueGuard
{
    public static void EnsureValid(int value, string paramName);
}

```

## Proyecto: src/Infrastructure

```csharp
// === src/Infrastructure/Generation/PuzzleGenerator.cs ===
using Domain;
using Sudoku.Application.Ports;
using Sudoku.Application.Puzzles;

namespace Infrastructure.Generation;
public sealed class PuzzleGenerator(ISolver solver, Random? random = null) : IPuzzleGenerator
{
    public Puzzle Generate(Difficulty difficulty, CancellationToken ct = default);
}

// === src/Infrastructure/Persistence/AppDataPaths.cs ===
namespace Infrastructure.Persistence;
public sealed class AppDataPaths(string root) : IAppDataPaths
{
    public static AppDataPaths ForCurrentUser();
    public string CurrentGamePath => Path.Combine(root, "games", "current.json");
    public string StatisticsPath => Path.Combine(root, "statistics.json");
    public string SettingsPath => Path.Combine(root, "settings.json");
}

// === src/Infrastructure/Persistence/AtomicFile.cs ===
using System.Text;

namespace Infrastructure.Persistence;
public static class AtomicFile
{
    public static void WriteAllText(string path, string content);
}

// === src/Infrastructure/Persistence/IAppDataPaths.cs ===
namespace Infrastructure.Persistence;
public interface IAppDataPaths
{
    string CurrentGamePath { get; }

    string StatisticsPath { get; }

    string SettingsPath { get; }
}

// === src/Infrastructure/Persistence/JsonFileStore.cs ===
using System.Text.Json;

namespace Infrastructure.Persistence;
public static class JsonFileStore
{
    public static T? Read<T>(string path)
        where T : class;
    public static void Write<T>(string path, T value);
    public static void Delete(string path);
}

// === src/Infrastructure/Persistence/JsonGameRepository.cs ===
using System.Text.Json;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Infrastructure.Persistence;
public sealed class JsonGameRepository(IAppDataPaths paths) : IGameRepository
{
    public void Save(SavedGame game);
    public SavedGame? Load();
    public void Delete();
}

// === src/Infrastructure/Persistence/JsonSettingsRepository.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Infrastructure.Persistence;
public sealed class JsonSettingsRepository(IAppDataPaths paths) : ISettingsRepository
{
    public AppSettings Load();
    public void Save(AppSettings settings);
}

// === src/Infrastructure/Persistence/JsonStatisticsRepository.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;

namespace Infrastructure.Persistence;
public sealed class JsonStatisticsRepository(IAppDataPaths paths) : IStatisticsRepository
{
    public GameStatistics Load();
    public void Save(GameStatistics statistics);
}

// === src/Infrastructure/Solving/BacktrackingSolver.cs ===
using Sudoku.Application.Ports;
using Domain;

namespace Infrastructure.Solving;
public sealed class BacktrackingSolver : ISolver
{
    public Board? Solve(Board board);
    public int CountSolutions(Board board, int limit);
}

// === src/Infrastructure/Time/SystemClock.cs ===
using Sudoku.Application.Ports;

namespace Infrastructure.Time;
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

```

## Proyecto: src/Sudoku.WinForms

```csharp
// === src/Sudoku.WinForms/Controls/BoardControl/BoardControl.cs ===
using Domain;
using Sudoku.WinForms.Theming;
using System.ComponentModel;

namespace Sudoku.WinForms.Controls.BoardControl;
public sealed class BoardControl : Control
{
    public BoardControl(IThemeService themeService);
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<ValueEnteredEventArgs>? ValueEntered;
    public event EventHandler? ClearRequested;
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Position? SelectedPosition
    {
        get => _selectedPosition;
        set
        {
            if (_selectedPosition == value)
                return;
            _selectedPosition = value;
            Invalidate();
            // Solo notificamos cuando se selecciona una celda.
            if (value is { } position)
            {
                SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(position));
            }
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool NotesMode
    {
        get => _notesMode;
        set
        {
            if (_notesMode == value)
                return;
            _notesMode = value;
            Invalidate();
        }
    }

    public void SetBoard(Board board);
}

// === src/Sudoku.WinForms/Controls/BoardControl/BoardGeometry.cs ===
using Domain;

namespace Sudoku.WinForms.Controls.BoardControl;
public readonly struct BoardGeometry
{
    public RectangleF Bounds { get; }
    public float CellSize { get; }

    public static BoardGeometry Create(SizeF availableSize);
    public RectangleF GetCellBounds(Position position);
    public Position? HitTest(PointF point);
}

// === src/Sudoku.WinForms/Controls/BoardControl/SelectionChangedEventArgs.cs ===
using Domain;

namespace Sudoku.WinForms.Controls.BoardControl;
public sealed class SelectionChangedEventArgs(Position position) : EventArgs
{
    public Position Position { get; }
}

// === src/Sudoku.WinForms/Controls/BoardControl/ValueEnteredEventArgs.cs ===
namespace Sudoku.WinForms.Controls.BoardControl;
public sealed class ValueEnteredEventArgs(int value) : EventArgs
{
    public int Value { get; }
}

// === src/Sudoku.WinForms/Formatting/DisplayNames.cs ===
using Domain;
using Sudoku.Application.Statistics;

namespace Sudoku.WinForms.Formatting;
public static class DisplayNames
{
    public static string Of(Difficulty difficulty);
    public static string Of(GameOutcome outcome);
}

// === src/Sudoku.WinForms/Formatting/DurationFormat.cs ===
namespace Sudoku.WinForms.Formatting;
public static class DurationFormat
{
    public static string Format(TimeSpan duration);
    public static string Format(TimeSpan? duration);
}

// === src/Sudoku.WinForms/Presenters/BoardPresenter.cs ===
using System.Diagnostics.CodeAnalysis;
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;
public sealed class BoardPresenter : IDisposable
{
    public BoardPresenter(IBoardView view, CurrentGame current, PlaceValue placeValue, ToggleNote toggleNote, ClearCell clearCell);
    public void Dispose();
}

// === src/Sudoku.WinForms/Presenters/ClockPresenter.cs ===
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;
public sealed class ClockPresenter : IDisposable
{
    public ClockPresenter(IGameControlsView view, CurrentGame current, IUiTimer timer);
    public void Dispose();
}

// === src/Sudoku.WinForms/Presenters/CurrentGame.cs ===
using Domain;
using Sudoku.Application.Games;

namespace Sudoku.WinForms.Presenters;
public sealed class CurrentGame
{
    public GameSession? Session { get; private set; }
    public bool IsFinished { get; private set; }
    public Board? SavedBoard { get; private set; }

    public event EventHandler? SessionReplacing;
    public event EventHandler<SessionChangedEventArgs>? SessionChanged;
    public event EventHandler? BoardChanged;
    public void Set(GameSession session, bool isResumed = false);
    public void MarkFinished();
    public void MarkSaved();
    public void ClearSaved();
    public void NotifyBoardChanged();
}

// === src/Sudoku.WinForms/Presenters/GameOutcomePresenter.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;
using Sudoku.Application.UseCases;
using Domain;
using Sudoku.WinForms.Formatting;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;
public sealed class GameOutcomePresenter : IDisposable
{
    public GameOutcomePresenter(IMainView view, CurrentGame current, RecordGameStarted recordStarted, RecordGameFinished recordFinished, IGameRepository games, IDialogService dialogs);
    public void Dispose();
}

// === src/Sudoku.WinForms/Presenters/GameplayPresenter.cs ===
using System.Diagnostics.CodeAnalysis;
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;
public sealed class GameplayPresenter : IDisposable
{
    public GameplayPresenter(IMainView view, CurrentGame current, Undo undo, Redo redo, ApplyHint applyHint, IDialogService dialogs);
    public void Dispose();
}

// === src/Sudoku.WinForms/Presenters/MainPresenter.cs ===
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;
public sealed class MainPresenter : IDisposable
{
    public MainPresenter(IMainView view, StartNewGame startNewGame, CurrentGame current, IDialogService dialogs);
    public void Dispose();
    public async Task StartNewGameAsync();
}// Version anterior, antes de extraer IGameControlsView. Se mantiene para referencia histórica y pruebas.
//public sealed class MainPresenter : IDisposable
//{
//    private readonly IMainView _view;
//    private readonly StartNewGame _startNewGame;
//    private readonly CurrentGame _current;
//    private readonly IDialogService _dialogs;
//    private CancellationTokenSource? _generation;
//    public MainPresenter(IMainView view, StartNewGame startNewGame, CurrentGame current, IDialogService dialogs)
//    {
//        _view = view;
//        _startNewGame = startNewGame;
//        _current = current;
//        _dialogs = dialogs;
//        _view.NewGameRequested += OnNewGameRequested;
//    }
//    public void Dispose()
//    {
//        _view.NewGameRequested -= OnNewGameRequested;
//        var generation = _generation;
//        _generation = null;   // la continuación pendiente ya no toca la vista
//        generation?.Cancel();
//    }
//    // Único async void: es un handler de evento. Todo el trabajo y los errores están en StartNewGameAsync.
//    private async void OnNewGameRequested(object? sender, EventArgs e) => await StartNewGameAsync();
//    /// <summary>Genera una partida nueva. Si ya había una generación en curso, la cancela. No lanza excepciones.</summary>
//    public async Task StartNewGameAsync()
//    {
//        _generation?.Cancel();
//        var cts = new CancellationTokenSource();
//        _generation = cts;
//        var difficulty = _view.SelectedDifficulty;
//        _view.SetBusy(true);
//        _view.SetStatus("Generando partida…");
//        try
//        {
//            var session = await Task.Run(() => _startNewGame.Execute(difficulty, cts.Token), cts.Token);
//            if (cts.IsCancellationRequested)
//                return; // reemplazada por una petición más nueva (o presenter liberado)
//            session.Start(); // las sesiones nacen en pausa; sin esto el tablero no acepta entrada
//            _view.SetPaused(false);
//            _current.Set(session);
//            _view.SetBoard(session.Board);
//            _view.SetDifficulty(session.Difficulty);
//            _view.SetStatus("Partida nueva");
//        }
//        catch (OperationCanceledException)
//        {
//            if (ReferenceEquals(_generation, cts))
//                _view.SetStatus("Generación cancelada");
//        }
//        catch (Exception ex)
//        {
//            if (ReferenceEquals(_generation, cts))
//            {
//                _view.SetStatus("No se pudo generar la partida");
//                _dialogs.ShowError("Nueva partida", $"No se pudo generar la partida: {ex.Message}");
//            }
//        }
//        finally
//        {
//            if (ReferenceEquals(_generation, cts))
//            {
//                _generation = null;
//                _view.SetBusy(false);
//            }
//            cts.Dispose();
//        }
//    }
//}


// === src/Sudoku.WinForms/Presenters/PersistencePresenter.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;
public sealed class PersistencePresenter : IDisposable
{
    public PersistencePresenter(IMainView view, CurrentGame current, SaveGame saveGame, LoadGame loadGame, IDialogService dialogs);
    public void Dispose();
}

// === src/Sudoku.WinForms/Presenters/SessionChangedEventArgs.cs ===
namespace Sudoku.WinForms.Presenters;
public sealed class SessionChangedEventArgs(bool isResumed) : EventArgs
{
    public bool IsResumed { get; }
}

// === src/Sudoku.WinForms/Presenters/SolvePresenter.cs ===
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;
public sealed class SolvePresenter : IDisposable
{
    public SolvePresenter(IGameControlsView view, CurrentGame current, SolveGame solveGame, ISolutionDialog dialog, IDialogService dialogs);
    public void Dispose();
}

// === src/Sudoku.WinForms/Presenters/StatisticsPresenter.cs ===
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;
public sealed class StatisticsPresenter : IDisposable
{
    public StatisticsPresenter(IGameControlsView view, GetStatistics getStatistics, IStatisticsDialog dialog, IDialogService dialogs);
    public void Dispose();
}

// === src/Sudoku.WinForms/Presenters/ThemePresenter.cs ===
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Theming;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;
public sealed class ThemePresenter : IDisposable
{
    public ThemePresenter(IGameControlsView view, IThemeService themes, IDialogService dialogs);
    public void Dispose();
}

// === src/Sudoku.WinForms/Services/IDialogService.cs ===
namespace Sudoku.WinForms.Services;
public interface IDialogService
{
    void ShowError(string title, string message);
    void ShowInfo(string title, string message);
    bool Confirm(string title, string message);
}

// === src/Sudoku.WinForms/Services/ISolutionDialog.cs ===
using Domain;

namespace Sudoku.WinForms.Services;
public interface ISolutionDialog
{
    void Show(Board solution);
}

// === src/Sudoku.WinForms/Services/IStatisticsDialog.cs ===
using Sudoku.Application.Statistics;

namespace Sudoku.WinForms.Services;
public interface IStatisticsDialog
{
    void Show(GameStatistics statistics);
}

// === src/Sudoku.WinForms/Services/IUiTimer.cs ===
namespace Sudoku.WinForms.Services;
public interface IUiTimer
{
    event EventHandler? Tick;
    void Start();
    void Stop();
}

// === src/Sudoku.WinForms/Services/MessageBoxDialogService.cs ===
namespace Sudoku.WinForms.Services;
public sealed class MessageBoxDialogService : IDialogService
{
    public void ShowError(string title, string message);
    public void ShowInfo(string title, string message);
    public bool Confirm(string title, string message);
}

// === src/Sudoku.WinForms/Services/PersistenceErrors.cs ===
using Sudoku.Application.Games;

namespace Sudoku.WinForms.Services;
public static class PersistenceErrors
{
    public static bool IsRecoverable(Exception exception);
}

// === src/Sudoku.WinForms/Services/SolutionDialogService.cs ===
using Domain;
using Sudoku.WinForms.Theming;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Services;
public sealed class SolutionDialogService(IThemeService themes) : ISolutionDialog
{
    public void Show(Board solution);
}

// === src/Sudoku.WinForms/Services/StatisticsDialogService.cs ===
using Sudoku.Application.Statistics;
using Sudoku.WinForms.Theming;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Services;
public sealed class StatisticsDialogService(IThemeService themes) : IStatisticsDialog
{
    public void Show(GameStatistics statistics);
}

// === src/Sudoku.WinForms/Services/WinFormsUiTimer.cs ===
namespace Sudoku.WinForms.Services;
public sealed class WinFormsUiTimer : IUiTimer, IDisposable
{
    public WinFormsUiTimer();
    public event EventHandler? Tick;
    public void Start();
    public void Stop();
    public void Dispose();
}

// === src/Sudoku.WinForms/Theming/IThemeService.cs ===
using Sudoku.Application.Settings;

namespace Sudoku.WinForms.Theming;
public interface IThemeService
{
    Theme Current { get; }

    event EventHandler? ThemeChanged;
    bool SetTheme(ThemeKind kind);
}

// === src/Sudoku.WinForms/Theming/Palettes.cs ===
using Sudoku.Application.Settings;

namespace Sudoku.WinForms.Theming;
public static class Palettes
{
    public static Theme Light { get; }
    public static Theme Dark { get; }

    public static Theme For(ThemeKind kind);
}

// === src/Sudoku.WinForms/Theming/Theme.cs ===
using Sudoku.Application.Settings;

namespace Sudoku.WinForms.Theming;
public sealed record Theme
{
    public required ThemeKind Kind { get; init; }
    // Ventana y barra lateral
    public required Color Background { get; init; }
    public required Color Surface { get; init; }
    public required Color Text { get; init; }
    public required Color MutedText { get; init; }
    public required Color Accent { get; init; }
    // Botones
    public required Color Control { get; init; }
    public required Color ControlText { get; init; }
    public required Color ControlBorder { get; init; }
    // Tablero
    public required Color BoardBackground { get; init; }
    public required Color BoardLine { get; init; }
    public required Color BoxLine { get; init; }
    public required Color GivenText { get; init; }
    public required Color UserText { get; init; }
    public required Color SelectedCell { get; init; }
    public required Color RelatedCell { get; init; }
    public required Color MatchingValue { get; init; }
    public required Color ConflictText { get; init; }
    public required Color NoteText { get; init; }
}

// === src/Sudoku.WinForms/Theming/ThemeApplier.cs ===
namespace Sudoku.WinForms.Theming;
public static class ThemeApplier
{
    public static void Apply(Theme theme, Control root);
}

// === src/Sudoku.WinForms/Theming/ThemeService.cs ===
using Sudoku.Application.Settings;
using Sudoku.Application.UseCases;

namespace Sudoku.WinForms.Theming;
public sealed class ThemeService : IThemeService
{
    public ThemeService(ChangeTheme changeTheme, ThemeKind initial);
    public Theme Current { get; private set; }

    public event EventHandler? ThemeChanged;
    public bool SetTheme(ThemeKind kind);
}

// === src/Sudoku.WinForms/Views/IBoardView.cs ===
using Domain;
using Sudoku.WinForms.Controls.BoardControl;

namespace Sudoku.WinForms.Views;
public interface IBoardView
{
    bool NotesMode { get; }

    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    event EventHandler<ValueEnteredEventArgs>? ValueEntered;
    event EventHandler? ClearRequested;
    void SetBoard(Board board);
}

// === src/Sudoku.WinForms/Views/IGameControlsView.cs ===
using Domain;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Views;
public interface IGameControlsView
{
    Difficulty SelectedDifficulty { get; }

    ThemeKind SelectedTheme { get; }

    event EventHandler? NewGameRequested;
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
    event EventHandler? SaveRequested;
    event EventHandler? ContinueRequested;
    void SetDifficulty(Difficulty difficulty);
    void SetElapsedTime(TimeSpan elapsedTime);
    void SetStatus(string message);
    void SetNotesMode(bool enabled);
    void SetPaused(bool paused);
    void SetBusy(bool busy);
    void SetUndoRedoState(bool canUndo, bool canRedo);
    void SetTheme(Theme theme);
    void SetSelectedTheme(ThemeKind kind);
}

// === src/Sudoku.WinForms/Views/IMainView.cs ===
namespace Sudoku.WinForms.Views;
public interface IMainView : IBoardView, IGameControlsView, IWindowLifecycleView
{
}

// === src/Sudoku.WinForms/Views/IWindowLifecycleView.cs ===
namespace Sudoku.WinForms.Views;
public interface IWindowLifecycleView
{
    event EventHandler? WindowClosing;
}

// === src/Sudoku.WinForms/Views/MainForm.cs ===
using Domain;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Views;
public sealed class MainForm : Form, IMainView
{
    public MainForm(IThemeService themeService);
    public Difficulty SelectedDifficulty => (_difficultyComboBox.SelectedItem as Choice<Difficulty>)?.Value ?? Difficulty.Easy;
    public ThemeKind SelectedTheme => (_themeComboBox.SelectedItem as Choice<ThemeKind>)?.Value ?? ThemeKind.Dark;
    public bool NotesMode => _notesButton.Checked;

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
    public void SetBoard(Board board);
    public void SetDifficulty(Difficulty difficulty);
    public void SetElapsedTime(TimeSpan elapsedTime);
    public void SetStatus(string message);
    public void SetNotesMode(bool enabled);
    public void SetPaused(bool paused);
    public void SetBusy(bool busy);
    public void SetUndoRedoState(bool canUndo, bool canRedo);
    public void SetTheme(Theme theme);
    public void SetSelectedTheme(ThemeKind kind);
}

// === src/Sudoku.WinForms/Views/SolutionForm.cs ===
using Domain;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Views;
public sealed class SolutionForm : Form
{
    public SolutionForm(IThemeService themes, Board solution);
}

// === src/Sudoku.WinForms/Views/StatisticsForm.cs ===
using Domain;
using Sudoku.Application.Statistics;
using Sudoku.WinForms.Formatting;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Views;
public sealed class StatisticsForm : Form
{
    public StatisticsForm(IThemeService themes, GameStatistics statistics);
}

```

## Proyecto: tests/Application.Tests.Unit

```csharp
// === tests/Application.Tests.Unit/ApplyHintTests.cs ===
using Application.Tests.Unit.TestData;
using Domain;
using global::Sudoku.Application.Games;
using global::Sudoku.Application.Ports;
using global::Sudoku.Application.Puzzles;
using global::Sudoku.Application.UseCases;

namespace Application.Tests.Unit;
public class ApplyHintTests
{
    [Fact]
    public void PlacesSolutionValueInPreferredEmptyCell();
    [Fact]
    public void WithoutPreferred_UsesFirstEmptyCell();
    [Fact]
    public void PreferredCellNotEmpty_FallsBackToFirstEmptyCell();
    [Fact]
    public void RemovesHintValueFromPeerNotes();
    [Fact]
    public void IsUndoableInOneStep();
    [Fact]
    public void FullBoard_ReturnsNullWithoutChanges();
    [Fact]
    public void UnsolvableGivens_PropagatesExceptionAndKeepsBoard();
}

// === tests/Application.Tests.Unit/ClearCellTests.cs ===
using Application.Tests.Unit.TestData;
using Domain;
using global::Sudoku.Application.UseCases;

namespace Application.Tests.Unit;
public class ClearCellTests
{
    [Fact]
    public void ClearsPlayerValue();
    [Fact]
    public void ClearsNotesOfEmptyCell();
    [Fact]
    public void GivenCell_ReturnsFalse();
    [Fact]
    public void AlreadyEmptyCellWithoutNotes_ReturnsFalse();
    [Fact]
    public void IsUndoable();
}

// === tests/Application.Tests.Unit/Games/GameSessionTimerTests.cs ===
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Tests.TestDoubles;
using Application.Tests.Unit.TestData;
using Xunit;

namespace Application.Tests.Games;
public sealed class GameSessionTimerTests
{
    [Fact]
    public void NewSession_StartsPausedAtZero();
    [Fact]
    public void ElapsedTime_DoesNotGrowWhilePaused();
    [Fact]
    public void Start_CountsElapsedTime();
    [Fact]
    public void Pause_StopsCounting();
    [Fact]
    public void Start_WhenAlreadyRunning_DoesNotResetTheLeg();
    [Fact]
    public void Pause_WhenAlreadyPaused_KeepsTheAccumulatedTime();
    [Fact]
    public void Resume_AccumulatesBothLegs();
    [Fact]
    public void Restore_KeepsBoardDifficultyAndElapsed_PausedWithEmptyHistory();
    [Fact]
    public void Restore_ThenStart_ContinuesFromSavedTime();
    [Fact]
    public void Restore_WithNull_Throws();
}

// === tests/Application.Tests.Unit/GameSessionTests.cs ===
using Application.Tests.Unit.TestData;
using Domain;
using global::Sudoku.Application.Games;

namespace Application.Tests.Unit;
public class GameSessionTests
{
    [Fact]
    public void NewSession_StartsFromPuzzleWithoutHistory();
    [Fact]
    public void Apply_ChangedBoard_UpdatesBoardAndEnablesUndo();
    [Fact]
    public void Apply_SameBoard_ReturnsFalseAndKeepsHistoryEmpty();
    [Fact]
    public void Apply_Null_Throws();
    [Fact]
    public void Undo_RestoresPreviousBoardAndEnablesRedo();
    [Fact]
    public void Redo_ReappliesUndoneBoard();
    [Fact]
    public void Apply_AfterUndo_ClearsRedoHistory();
    [Fact]
    public void UndoAndRedo_WithEmptyHistory_ReturnFalse();
}

// === tests/Application.Tests.Unit/PlaceValueTests.cs ===
using Application.Tests.Unit.TestData;
using Domain;
using global::Sudoku.Application.UseCases;

namespace Application.Tests.Unit;
public class PlaceValueTests
{
    [Fact]
    public void PlacesPlayerValueInEmptyCell();
    [Fact]
    public void GivenCell_ReturnsFalseAndKeepsBoardAndHistory();
    [Fact]
    public void ReplacesExistingPlayerValue();
    [Fact]
    public void SameValueTwice_SecondCallReturnsFalseWithoutExtraHistory();
    [Fact]
    public void ClearsOwnNotes();
    [Fact]
    public void RemovesValueFromPeerNotes_AndKeepsNonPeersAndOtherNotes();
    [Fact]
    public void AllowsConflictingValue_AndBothCellsAreReportedAsConflicts();
    [Fact]
    public void ValueAndPeerNotesCleanup_UndoTogetherInOneStep();
}

// === tests/Application.Tests.Unit/Puzzles/DifficultyProfileTests.cs ===
using Domain;
using Sudoku.Application.Puzzles;

namespace Application.Tests.Unit.Puzzles;
public class DifficultyProfileTests
{
    public static TheoryData<Difficulty> AllDifficulties => new(Enum.GetValues<Difficulty>());

    [Theory]
    [MemberData(nameof(AllDifficulties))]
    public void For_ReturnsProfileWithValidRange(Difficulty difficulty);
    [Fact]
    public void For_HarderDifficultyHasFewerGivens_WithoutOverlap();
    [Fact]
    public void For_UndefinedDifficulty_Throws();
}

// === tests/Application.Tests.Unit/StartNewGameTests.cs ===
using Application.Tests.Unit.TestData;
using Domain;
using global::Sudoku.Application.Ports;
using global::Sudoku.Application.Puzzles;
using global::Sudoku.Application.UseCases;
using Sudoku.Application.Tests.TestDoubles;

namespace Application.Tests.Unit;
public class StartNewGameTests
{
    [Fact]
    public void CreatesSessionFromGeneratedPuzzle();
    [Fact]
    public void PassesDifficultyAndCancellationTokenToGenerator();
}

// === tests/Application.Tests.Unit/Statistics/DifficultyStatisticsTests.cs ===
using Sudoku.Application.Statistics;

namespace Application.Tests.Unit.Statistics;
public class DifficultyStatisticsTests
{
    [Fact]
    public void Empty_has_no_games_and_no_times();
    [Fact]
    public void WithStarted_increments_started_only();
    [Fact]
    public void Won_updates_count_total_and_best_time();
    [Fact]
    public void Won_keeps_best_time_when_new_one_is_slower_and_replaces_it_when_faster();
    [Fact]
    public void Lost_increments_lost_without_touching_times();
    [Fact]
    public void AverageWonTime_is_total_divided_by_wins();
}

// === tests/Application.Tests.Unit/Statistics/GameStatisticsTests.cs ===
using Domain;
using Sudoku.Application.Statistics;

namespace Application.Tests.Unit.Statistics;
public class GameStatisticsTests
{
    [Fact]
    public void For_returns_empty_statistics_for_a_difficulty_never_played();
    [Fact]
    public void WithGameStarted_only_affects_that_difficulty();
    [Fact]
    public void WithGameFinished_updates_aggregates_and_recent_list();
    [Fact]
    public void Recent_lists_newest_first_and_is_capped();
}

// === tests/Application.Tests.Unit/TestDoubles/InMemoryGameRepository.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Sudoku.Application.Tests.TestDoubles;
public sealed class InMemoryGameRepository : IGameRepository
{
    public SavedGame? Stored { get; set; }
    public Exception? LoadFailure { get; set; }

    public void Save(SavedGame game);
    public SavedGame? Load();
    public void Delete();
}

// === tests/Application.Tests.Unit/ToggleNoteTests.cs ===
using Application.Tests.Unit.TestData;
using Domain;
using global::Sudoku.Application.UseCases;

namespace Application.Tests.Unit;
public class ToggleNoteTests
{
    [Fact]
    public void AddsNote_AndTogglingAgainRemovesIt();
    [Fact]
    public void KeepsOtherNotes();
    [Fact]
    public void CellWithPlayerValue_ReturnsFalse();
    [Fact]
    public void GivenCell_ReturnsFalse();
    [Fact]
    public void IsUndoable();
}

// === tests/Application.Tests.Unit/UndoTests.cs ===
using Application.Tests.Unit.TestData;
using global::Sudoku.Application.UseCases;

namespace Application.Tests.Unit;
public class UndoTests
{
    [Fact]
    public void RevertsLastAction();
    [Fact]
    public void WithoutHistory_ReturnsFalse();
}

public class RedoTests
{
    [Fact]
    public void ReappliesUndoneAction();
    [Fact]
    public void WithoutUndoneActions_ReturnsFalse();
}

// === tests/Application.Tests.Unit/UseCases/ChangeThemeTests.cs ===
using Application.Tests.Unit.TestData;
using Sudoku.Application.Settings;
using Sudoku.Application.UseCases;

namespace Application.Tests.Unit.UseCases;
public class ChangeThemeTests
{
    [Fact]
    public void Saves_the_new_theme_and_returns_true();
    [Fact]
    public void Returns_false_and_does_not_save_when_the_theme_is_already_selected();
    [Fact]
    public void Rejects_a_theme_value_that_is_not_defined();
}

// === tests/Application.Tests.Unit/UseCases/GetHintTests.cs ===
using Sudoku.Application.UseCases;
using Domain;

namespace Application.Tests.UseCases;
public sealed class GetHintTests
{
    [Fact]
    public void Execute_PreferredCellIsEmpty_ReturnsSolutionValueForThatCell();
    [Fact]
    public void Execute_NoPreferredCell_ReturnsFirstEmptyCell();
    [Fact]
    public void Execute_PreferredCellIsFilled_FallsBackToFirstEmptyCell();
    [Fact]
    public void Execute_BoardWithoutEmptyCells_ReturnsNullAndDoesNotCallSolver();
    [Fact]
    public void Execute_SendsOnlyTheGivensToTheSolver();
    [Fact]
    public void Execute_GivensWithoutSolution_Throws();
    [Fact]
    public void Execute_NullBoard_Throws();
}

// === tests/Application.Tests.Unit/UseCases/GetSettingsTests.cs ===
using Application.Tests.Unit.TestData;
using Sudoku.Application.Settings;
using Sudoku.Application.UseCases;

namespace Application.Tests.Unit.UseCases;
public class GetSettingsTests
{
    [Fact]
    public void Returns_default_settings_when_nothing_is_saved();
    [Fact]
    public void Returns_the_saved_settings();
}

// === tests/Application.Tests.Unit/UseCases/GetStatisticsTests.cs ===
using Application.Tests.Unit.TestData;
using Domain;
using Sudoku.Application.Statistics;
using Sudoku.Application.UseCases;
using Xunit;

namespace Application.Tests.Unit.UseCases;
public class GetStatisticsTests
{
    [Fact]
    public void Returns_empty_statistics_when_nothing_is_saved();
    [Fact]
    public void Returns_what_the_repository_holds();
}

// === tests/Application.Tests.Unit/UseCases/LoadGameTests.cs ===
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Tests.TestDoubles;
using Sudoku.Application.UseCases;
using Xunit;
using Application.Tests.Unit.TestData;

namespace Sudoku.Application.Tests.UseCases;
public sealed class LoadGameTests
{
    [Fact]
    public void Execute_WithoutSavedGame_ReturnsNull();
    [Fact]
    public void Execute_WithSavedGame_ReturnsPausedSessionWithoutHistory();
    [Fact]
    public void Execute_WithCorruptSave_PropagatesTheException();
    [Fact]
    public void SaveThenLoad_ReturnsEquivalentSession();
}

// === tests/Application.Tests.Unit/UseCases/RecordGameFinishedTests.cs ===
using Application.Tests.Unit.TestData;
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Puzzles;
using Sudoku.Application.Statistics;
using Sudoku.Application.UseCases;

namespace Application.Tests.Unit.UseCases;
public class RecordGameFinishedTests
{
    [Fact]
    public void Won_records_elapsed_time_difficulty_and_finish_moment();
    [Fact]
    public void Lost_is_recorded_for_an_unfinished_board();
    [Fact]
    public void Won_with_an_unsolved_board_throws_and_saves_nothing();
}

// === tests/Application.Tests.Unit/UseCases/RecordGameStartedTests.cs ===
using Application.Tests.Unit.TestData;
using Domain;
using Sudoku.Application.UseCases;
using Xunit;

namespace Application.Tests.Unit.UseCases;
public class RecordGameStartedTests
{
    [Fact]
    public void Increments_started_for_the_given_difficulty_and_persists();
}

// === tests/Application.Tests.Unit/UseCases/SaveGameTests.cs ===
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Tests.TestDoubles;
using Sudoku.Application.UseCases;
using Xunit;
using Application.Tests.Unit.TestData;

namespace Sudoku.Application.Tests.UseCases;
public sealed class SaveGameTests
{
    [Fact]
    public void Execute_SavesBoardDifficultyAndElapsedTime();
    [Fact]
    public void Execute_DoesNotPauseOrChangeTheSession();
    [Fact]
    public void Execute_WithNullSession_Throws();
}

// === tests/Application.Tests.Unit/UseCases/SolveGameTests.cs ===
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Puzzles;
using Sudoku.Application.UseCases;

namespace Application.Tests.Unit.UseCases;
public class SolveGameTests
{
    [Fact]
    public void SolvesOnlyTheGivens_IgnoringPlayerValues();
    [Fact]
    public void DoesNotModifyTheSession();
    [Fact]
    public void GivensWithoutSolution_Throws();
    [Fact]
    public void NullSession_Throws();
}

```

## Proyecto: tests/Domain.Tests.Unit

```csharp
// === tests/Domain.Tests.Unit/BoardGivensOnlyTests.cs ===
using Domain;

namespace Domain.Tests;
public sealed class BoardGivensOnlyTests
{
    [Fact]
    public void GivensOnly_RemovesPlayerValuesAndNotes();
    [Fact]
    public void GivensOnly_KeepsGivenCells();
    [Fact]
    public void GivensOnly_DoesNotModifyOriginalBoard();
    [Fact]
    public void GivensOnly_OnEmptyBoard_ReturnsEmptyBoard();
}

// === tests/Domain.Tests.Unit/BoardTests.cs ===
namespace Domain.Tests.Unit;
public class BoardTests
{
    [Fact]
    public void Empty_HasAllCellsEmpty();
    [Fact]
    public void FromGivens_ParsesGivensAndEmptyCells();
    [Fact]
    public void FromGivens_TreatsDotAsEmpty();
    [Fact]
    public void FromGivens_WrongLength_Throws();
    [Fact]
    public void FromGivens_InvalidCharacter_Throws();
    [Fact]
    public void FromGivens_Null_Throws();
    [Fact]
    public void WithValue_ReturnsNewBoard_AndLeavesOriginalUntouched();
    [Fact]
    public void WithValue_ChangesOnlyTheTargetCell();
    [Fact]
    public void WithValue_OnGivenCell_Throws();
    [Fact]
    public void WithNotes_SetsNotesOnEmptyCell();
    [Fact]
    public void WithNotes_OnFilledCell_Throws();
    [Fact]
    public void WithCleared_RemovesPlayerValue();
    [Fact]
    public void WithCleared_OnGivenCell_Throws();
    [Fact]
    public void Equality_IsByContent();
    [Fact]
    public void ToValuesString_RoundTripsGivens();
    [Fact]
    public void ToValuesString_IncludesPlayerValues();
}

// === tests/Domain.Tests.Unit/CandidateSetSetOperationsTests.cs ===
using Domain;

namespace Domain.Tests;
public sealed class CandidateSetSetOperationsTests
{
    [Fact]
    public void Union_CombinesValuesOfBothSets();
    [Fact]
    public void Union_WithEmptySet_ReturnsSameSet();
    [Fact]
    public void Except_RemovesValuesPresentInOtherSet();
    [Fact]
    public void Except_FromAll_ReturnsComplement();
    [Fact]
    public void Except_WithEmptySet_ReturnsSameSet();
    [Fact]
    public void Except_WithItself_ReturnsEmptySet();
}

// === tests/Domain.Tests.Unit/CandidateSetTests.cs ===
namespace Domain.Tests.Unit;
public class CandidateSetTests
{
    [Fact]
    public void Empty_HasNoCandidates();
    [Fact]
    public void All_ContainsOneThroughNine();
    [Fact]
    public void Add_IncludesValue_AndDoesNotMutateOriginal();
    [Fact]
    public void Add_IsIdempotent();
    [Fact]
    public void Remove_ExcludesValue();
    [Fact]
    public void Remove_AbsentValue_IsNoOp();
    [Fact]
    public void Toggle_Twice_ReturnsOriginal();
    [Fact]
    public void Enumeration_IsAscending();
    [Fact]
    public void Equality_IsByContent();
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(-3)]
    public void InvalidValue_Throws(int value);
    [Fact]
    public void ToString_ListsCandidates();
}

// === tests/Domain.Tests.Unit/CellTests.cs ===
using Xunit;

namespace Domain.Tests.Unit;
public class CellTests
{
    [Fact]
    public void Empty_HasNoValueNotGivenAndNoNotes();
    [Fact]
    public void Default_EqualsEmpty();
    [Fact]
    public void Given_HasValueAndIsGiven();
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void Given_InvalidValue_Throws(int value);
    [Fact]
    public void WithValue_OnEmpty_SetsPlayerValue();
    [Fact]
    public void WithValue_ClearsNotes();
    [Fact]
    public void WithValue_OnGiven_Throws();
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void WithValue_InvalidValue_Throws(int value);
    [Fact]
    public void WithNotes_OnEmpty_SetsNotes();
    [Fact]
    public void WithNotes_OnFilledCell_Throws();
    [Fact]
    public void WithNotes_OnGiven_Throws();
    [Fact]
    public void Cleared_OnFilledCell_ReturnsEmpty();
    [Fact]
    public void Cleared_OnCellWithNotes_RemovesNotes();
    [Fact]
    public void Cleared_OnGiven_Throws();
}

// === tests/Domain.Tests.Unit/PositionTests.cs ===
namespace Domain.Tests.Unit;
public class PositionTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 2, 0)]
    [InlineData(0, 3, 1)]
    [InlineData(2, 8, 2)]
    [InlineData(3, 0, 3)]
    [InlineData(4, 4, 4)]
    [InlineData(5, 7, 5)]
    [InlineData(8, 8, 8)]
    public void Box_ReturnsExpectedBoxIndex(int row, int column, int expectedBox);
    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(9, 0)]
    [InlineData(0, 9)]
    public void Constructor_OutOfRange_Throws(int row, int column);
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(9)]
    [InlineData(40)]
    [InlineData(80)]
    public void FromIndex_RoundTripsWithIndex(int index);
    [Fact]
    public void FromIndex_MapsRowAndColumn();
    [Theory]
    [InlineData(-1)]
    [InlineData(81)]
    public void FromIndex_OutOfRange_Throws(int index);
    [Fact]
    public void All_ContainsEightyOneDistinctPositions();
    [Fact]
    public void Equality_IsByValue();
}

// === tests/Domain.Tests.Unit/SudokuRulesTests.cs ===
namespace Domain.Tests.Unit;
public class SudokuRulesTests
{
    // ---- Peers ----
    [Fact]
    public void Peers_EveryPositionHasTwentyPeers();
    [Fact]
    public void Peers_DoesNotIncludeThePositionItself();
    [Fact]
    public void Peers_IncludesRowColumnAndBoxButNothingElse();
    [Fact]
    public void Peers_AreSymmetric();
    // ---- IsValidPlacement ----
    [Fact]
    public void IsValidPlacement_SameRow_IsInvalid();
    [Fact]
    public void IsValidPlacement_SameColumn_IsInvalid();
    [Fact]
    public void IsValidPlacement_SameBox_IsInvalid();
    [Fact]
    public void IsValidPlacement_UnrelatedCellOrOtherValue_IsValid();
    [Fact]
    public void IsValidPlacement_IgnoresTheCellItself();
    [Fact]
    public void IsValidPlacement_ConsidersGivenCells();
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void IsValidPlacement_InvalidValue_Throws(int value);
    [Fact]
    public void IsValidPlacement_NullBoard_Throws();
    // ---- FindConflicts ----
    [Fact]
    public void FindConflicts_EmptyBoard_ReturnsNone();
    [Fact]
    public void FindConflicts_ValidPuzzle_ReturnsNone();
    [Fact]
    public void FindConflicts_RowDuplicate_MarksBothCells();
    [Fact]
    public void FindConflicts_ColumnDuplicate_MarksBothCells();
    [Fact]
    public void FindConflicts_BoxDuplicate_MarksBothCells();
    [Fact]
    public void FindConflicts_ThreeDuplicatesInARow_MarksAllThree();
    [Fact]
    public void FindConflicts_SameValueInUnrelatedCells_IsNotAConflict();
    [Fact]
    public void FindConflicts_NotesNeverConflict();
    [Fact]
    public void FindConflicts_GivenVersusPlayerValue_MarksBoth();
    // ---- IsComplete / IsSolved ----
    [Fact]
    public void IsComplete_EmptyBoard_IsFalse();
    [Fact]
    public void IsComplete_IncompletePuzzle_IsFalse();
    [Fact]
    public void IsComplete_FullBoard_IsTrue();
    [Fact]
    public void IsSolved_ValidSolution_IsTrue();
    [Fact]
    public void IsSolved_IncompleteBoard_IsFalse();
    [Fact]
    public void IsSolved_CompleteWithCorrectPlayerValue_IsTrue();
    [Fact]
    public void IsSolved_CompleteWithConflict_IsFalse();
}

```

## Proyecto: tests/Infrastructure.Tests.Unit

```csharp
// === tests/Infrastructure.Tests.Unit/Generarion/PuzzleGeneratorTests.cs ===
using Domain;
using global::Infrastructure.Generation;
using global::Infrastructure.Solving;
using Sudoku.Application.Ports;
using Sudoku.Application.Puzzles;

namespace Application.Tests.Unit.Generarion;
public class PuzzleGeneratorTests
{
    public static TheoryData<Difficulty> AllDifficulties => new(Enum.GetValues<Difficulty>());

    [Fact]
    public void Constructor_NullSolver_Throws();
    [Theory]
    [MemberData(nameof(AllDifficulties))]
    public void Generate_HasUniqueSolution(Difficulty difficulty);
    [Theory]
    [MemberData(nameof(AllDifficulties))]
    public void Generate_GivensCountIsWithinProfileRange(Difficulty difficulty);
    [Fact]
    public void Generate_EveryFilledCellIsGiven_AndNoNotes();
    [Fact]
    public void Generate_SolutionIsSolved_AndConsistentWithGivens();
    [Fact]
    public void Generate_SameSeed_ProducesSamePuzzle();
    [Fact]
    public void Generate_DifferentSeeds_ProduceDifferentPuzzles();
    [Fact]
    public void Generate_CancelledToken_Throws();
    [Fact]
    public void Generate_WhenNoCellCanBeRemoved_ThrowsAfterMaxAttempts();
}

// === tests/Infrastructure.Tests.Unit/Persistence/AtomicFileTests.cs ===
using Infrastructure.Persistence;
using Xunit;

namespace Infrastructure.Tests.Persistence;
public sealed class AtomicFileTests : IDisposable
{
    public void Dispose();
    [Fact]
    public void WriteAllText_CreatesMissingDirectories();
    [Fact]
    public void WriteAllText_ReplacesExistingContent();
    [Fact]
    public void WriteAllText_LeavesNoTemporaryFiles();
    [Fact]
    public void WriteAllText_WritesUtf8WithoutBom();
}

// === tests/Infrastructure.Tests.Unit/Persistence/JsonGameRepositoryTests.cs ===
using System.Globalization;
using Domain;
using Infrastructure.Persistence;
using Sudoku.Application.Games;
using Xunit;

namespace Infrastructure.Tests.Persistence;
public sealed class JsonGameRepositoryTests : IDisposable
{
    public JsonGameRepositoryTests();
    public void Dispose();
    [Fact]
    public void Load_WithoutFile_ReturnsNull();
    [Fact]
    public void SaveThenLoad_RoundTripsBoardDifficultyAndElapsed();
    [Fact]
    public void SaveThenLoad_PreservesGivensPlayerValuesAndNotes();
    [Theory]
    [InlineData(Difficulty.Easy)]
    [InlineData(Difficulty.Medium)]
    [InlineData(Difficulty.Hard)]
    [InlineData(Difficulty.Expert)]
    public void SaveThenLoad_RoundTripsEveryDifficulty(Difficulty difficulty);
    [Fact]
    public void Save_WritesToCurrentGamePath();
    [Fact]
    public void Save_Twice_KeepsOnlyTheLastGameAndNoTemporaryFiles();
    [Fact]
    public void Delete_RemovesTheSavedGame();
    [Fact]
    public void Delete_WithoutFile_DoesNotThrow();
    [Fact]
    public void Delete_WithDirectoryButWithoutFile_DoesNotThrow();
    [Fact]
    public void Load_ValidHandWrittenJson_Works();
    [Theory]
    [InlineData("{ not json")]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    public void Load_UnreadableOrIncompleteJson_ThrowsCorruptSaveException(string content);
    [Fact]
    public void Load_UnknownVersion_ThrowsCorruptSaveException();
    [Theory]
    [InlineData("Nightmare")]
    [InlineData("99")]
    [InlineData("")]
    public void Load_UnknownDifficulty_ThrowsCorruptSaveException(string difficulty);
    [Theory]
    [InlineData(-1)]
    [InlineData(double.MaxValue)]
    public void Load_InvalidElapsed_ThrowsCorruptSaveException(double elapsed);
    [Fact]
    public void Load_WrongBoardLength_ThrowsCorruptSaveException();
    [Fact]
    public void Load_ValueContradictingAGiven_ThrowsCorruptSaveException();
    [Fact]
    public void Load_NonDigitValue_ThrowsCorruptSaveException();
    [Fact]
    public void Load_NotesOnAFilledCell_ThrowsCorruptSaveException();
    [Fact]
    public void Load_NotesMaskOutOfRange_ThrowsCorruptSaveException();
    [Fact]
    public void Load_CorruptFile_IsLeftUntouched();
    [Fact]
    public void Save_AfterCorruptFile_ReplacesItWithAValidGame();
}

// === tests/Infrastructure.Tests.Unit/Persistence/JsonSettingsRepositoryTests.cs ===
using global::Infrastructure.Persistence;
using global::Infrastructure.Tests.Unit.TestSupport;
using Sudoku.Application.Games;
using Sudoku.Application.Settings;

namespace Infrastructure.Tests.Unit.Persistence;
public sealed class JsonSettingsRepositoryTests : IDisposable
{
    public JsonSettingsRepositoryTests();
    public void Dispose();
    [Fact]
    public void Load_returns_default_settings_when_the_file_does_not_exist();
    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void Save_then_Load_round_trips_the_theme(ThemeKind theme);
    [Fact]
    public void Load_throws_CorruptSaveException_for_invalid_json_and_leaves_the_file_untouched();
    [Fact]
    public void Load_throws_CorruptSaveException_for_an_unknown_version();
    [Fact]
    public void Load_throws_CorruptSaveException_for_an_unknown_theme_name();
}

// === tests/Infrastructure.Tests.Unit/Persistence/JsonStatisticsRepositoryTests.cs ===
using Domain;
using global::Infrastructure.Persistence;
using global::Infrastructure.Tests.Unit.TestSupport;
using Sudoku.Application.Games;
using Sudoku.Application.Statistics;

namespace Infrastructure.Tests.Unit.Persistence;
public sealed class JsonStatisticsRepositoryTests : IDisposable
{
    public JsonStatisticsRepositoryTests();
    public void Dispose();
    [Fact]
    public void Load_returns_empty_statistics_when_the_file_does_not_exist();
    [Fact]
    public void Save_then_Load_round_trips_aggregates_and_recent_games();
    [Fact]
    public void Load_throws_CorruptSaveException_for_invalid_json_and_leaves_the_file_untouched();
    [Fact]
    public void Load_throws_CorruptSaveException_for_an_unknown_version();
    [Fact]
    public void Load_throws_CorruptSaveException_for_an_unknown_difficulty_name();
}

// === tests/Infrastructure.Tests.Unit/Solving/BacktrackingSolverTests.cs ===
using Domain;
using Infrastructure.Solving;

namespace Infrastructure.Tests.Solving;
public sealed class BacktrackingSolverTests
{
    [Fact]
    public void Solve_EasyPuzzle_ReturnsKnownSolution();
    [Fact]
    public void Solve_HardPuzzle_ReturnsSolutionValidAccordingToRules();
    [Fact]
    public void Solve_KeepsGivenFlagsAndMarksSolvedCellsAsPlayerValues();
    [Fact]
    public void Solve_KeepsExistingPlayerValues();
    [Fact]
    public void Solve_EmptyBoard_ReturnsValidSolution();
    [Fact]
    public void Solve_AlreadySolvedBoard_ReturnsEqualBoard();
    [Fact]
    public void Solve_ConflictingGivens_ReturnsNull();
    [Fact]
    public void Solve_CellWithoutCandidates_ReturnsNull();
    [Fact]
    public void Solve_NullBoard_Throws();
    [Theory]
    [InlineData(TestPuzzles.Easy)]
    [InlineData(TestPuzzles.Hard)]
    public void CountSolutions_UniquePuzzle_ReturnsOne(string puzzle);
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void CountSolutions_EmptyBoard_StopsAtLimit(int limit);
    [Fact]
    public void CountSolutions_ConflictingGivens_ReturnsZero();
    [Fact]
    public void CountSolutions_CellWithoutCandidates_ReturnsZero();
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CountSolutions_LimitBelowOne_Throws(int limit);
}

```

## Proyecto: tests/Sudoku.WinForms.Tests

```csharp
// === tests/Sudoku.WinForms.Tests/Controls/Board/BoardGeometryTests.cs ===
using Domain;
using Sudoku.WinForms.Controls.BoardControl;

namespace Sudoku.WinForms.Tests.Controls.BoardControl;
public sealed class BoardGeometryTests
{
    [Fact]
    public void Create_WhenWidthIsGreaterThanHeight_CentersSquareBoard();
    [Fact]
    public void Create_WhenHeightIsGreaterThanWidth_CentersSquareBoard();
    [Fact]
    public void Create_WhenAvailableSizeIsEmpty_ReturnsDefaultGeometry();
    [Fact]
    public void GetCellBounds_ReturnsCorrectCellRectangle();
    [Fact]
    public void HitTest_WhenPointIsInsideCell_ReturnsItsPosition();
    [Fact]
    public void HitTest_WhenPointIsOnTopLeftBoundary_ReturnsFirstCell();
    [Fact]
    public void HitTest_WhenPointIsOnRightBoundary_ReturnsNull();
    [Fact]
    public void HitTest_WhenPointIsOnBottomBoundary_ReturnsNull();
    [Fact]
    public void HitTest_WhenPointIsOutsideBoard_ReturnsNull();
    [Fact]
    public void HitTest_WhenGeometryIsNotInitialized_ReturnsNull();
}

// === tests/Sudoku.WinForms.Tests/Formatting/DurationFormatTests.cs ===
using Sudoku.WinForms.Formatting;

namespace Sudoku.WinForms.Tests.Formatting;
public class DurationFormatTests
{
    [Fact]
    public void UnderAnHour_UsesMinutesAndSeconds();
    [Fact]
    public void FromAnHour_AddsHours();
    [Fact]
    public void Null_ShowsADash();
}

// === tests/Sudoku.WinForms.Tests/Presenters/ClockPresenterTests.cs ===
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;
public sealed class ClockPresenterTests : IDisposable
{
    public ClockPresenterTests();
    public void Dispose();
    [Fact]
    public void StartsTheTimer();
    [Fact]
    public void Tick_ShowsTheSessionElapsedTime();
    [Fact]
    public void Tick_WithoutSession_ShowsZero();
    [Fact]
    public void Dispose_StopsTheTimer();
}

// === tests/Sudoku.WinForms.Tests/Presenters/CurrentGameTests.cs ===
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;
public sealed class CurrentGameTests
{
    [Fact]
    public void Set_RaisesReplacingOnlyWhenThereWasASession();
    [Fact]
    public void Replacing_HandlersStillSeeThePreviousSession();
    [Fact]
    public void Set_ResetsFinishedAndSavedBoard();
    [Fact]
    public void Set_Resumed_MarksTheSessionAsSaved_AndFlagsTheEvent();
    [Fact]
    public void MarkSaved_StoresTheCurrentBoard();
    [Fact]
    public void NotifyBoardChanged_RaisesTheEvent();
}

// === tests/Sudoku.WinForms.Tests/Presenters/GameOutcomePresenterTests.cs ===
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Statistics;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;
public sealed class GameOutcomePresenterTests : IDisposable
{
    public GameOutcomePresenterTests();
    public void Dispose();
    [Fact]
    public void NewSession_RecordsGameStarted();
    [Fact]
    public void ResumedSession_DoesNotRecordGameStarted();
    [Fact]
    public void ReplacingAnUnfinishedSession_RecordsItAsLost();
    [Fact]
    public void ReplacingAFinishedSession_DoesNotRecordLost();
    [Fact]
    public void ReplacingASavedSession_DeletesTheSavedCopy();
    [Fact]
    public void SolvedBoard_RecordsWonOnce_StopsTheClockAndAnnounces();
    [Fact]
    public void WinningAResumedSession_DeletesTheSavedCopy();
    [Fact]
    public void UnsolvedBoard_DoesNothing();
    [Fact]
    public void ClosingWithAnUnsavedGame_RecordsLost();
    [Fact]
    public void ClosingWithASavedGame_DoesNotRecordLost();
    [Fact]
    public void StatisticsFailure_ShowsErrorWithoutThrowing();
}

// === tests/Sudoku.WinForms.Tests/Presenters/GameplayPresenterTests.cs ===
using Domain;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;
public sealed class GameplayPresenterTests : IDisposable
{
    public GameplayPresenterTests();
    public void Dispose();
    [Fact]
    public void Undo_RevertsTheLastMove_AndUpdatesButtons();
    [Fact]
    public void Redo_ReappliesTheUndoneMove();
    [Fact]
    public void Undo_WhenPaused_IsIgnored();
    [Fact]
    public void Hint_PlacesTheSolutionValueInTheSelectedCell();
    [Fact]
    public void Hint_WhenGivensHaveNoSolution_ShowsError();
    [Fact]
    public void Hint_WhenFinished_IsIgnored();
}

// === tests/Sudoku.WinForms.Tests/Presenters/PersistencePresenterTests.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.UseCases;
using Domain;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;
public sealed class PersistencePresenterTests : IDisposable
{
    public PersistencePresenterTests();
    public void Dispose();
    [Fact]
    public void Save_StoresTheGame_AndMarksItSaved();
    [Fact]
    public void Save_WithoutSession_SavesNothing();
    [Fact]
    public void Save_WhenFinished_SavesNothing();
    [Fact]
    public void Continue_WithoutSavedGame_ShowsInfo();
    [Fact]
    public void Continue_CorruptSave_ShowsErrorAndKeepsNoSession();
    [Fact]
    public void Continue_LoadsARunningSession_AndUpdatesTheView();
    [Fact]
    public void Continue_WithAGameInProgress_AsksFirst_AndKeepsItIfDeclined();
}

// === tests/Sudoku.WinForms.Tests/Presenters/SolvePresenterTests.cs ===
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;
public sealed class SolvePresenterTests
{
    [Fact]
    public void Solve_ShowsTheSolution_PausesWhileOpen_AndResumesAfterwards();
    [Fact]
    public void Solve_OnAPausedGame_KeepsItPaused();
    [Fact]
    public void Solve_WithoutSolution_ShowsError_AndResumes();
    [Fact]
    public void Solve_WithoutSession_OnlyUpdatesStatus();
}

// === tests/Sudoku.WinForms.Tests/Presenters/StatisticsPresenterTests.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.Statistics;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;
public sealed class StatisticsPresenterTests
{
    [Fact]
    public void Requested_ShowsTheStoredStatistics();
    [Fact]
    public void Requested_WithCorruptFile_ShowsErrorInsteadOfTheDialog();
}

// === tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/BoardPresenterTests.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;
public sealed class BoardPresenterTests : IDisposable
{
    public BoardPresenterTests();
    public void Dispose();
    [Fact]
    public void ValueEntered_PlacesValueInSelectedCell_AndRefreshesView();
    [Fact]
    public void ValueEntered_InNotesMode_TogglesNoteInsteadOfPlacingValue();
    [Fact]
    public void ValueEntered_WithoutSelection_DoesNothing();
    [Fact]
    public void ValueEntered_WhenPaused_IsIgnored();
    [Fact]
    public void ValueEntered_OnGivenCell_DoesNotRefreshView();
    [Fact]
    public void ClearRequested_RemovesPlayerValue();
    [Fact]
    public void Events_WithoutActiveSession_AreIgnored();
    [Fact]
    public void AfterDispose_EventsAreIgnored();
}

// === tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/MainPresenterTests.cs ===
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Puzzles;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;
public sealed class MainPresenterTests
{
    [Fact]
    public async Task StartNewGame_StoresRunningSession_AndShowsBoard();
    [Fact]
    public async Task StartNewGame_UsesSelectedDifficulty();
    [Fact]
    public async Task StartNewGame_WhenGeneratorFails_ShowsErrorAndKeepsNoSession();
    [Fact]
    public async Task StartNewGame_WhenCalledAgain_CancelsPreviousGeneration();
    [Fact]
    public async Task StartNewGame_TellsViewTheGameIsNotPaused();
    [Fact]
    public async Task NewGameRequested_Event_StartsGeneration();
    [Fact]
    public void AfterDispose_NewGameRequested_IsIgnored();
    [Fact]
    public async Task CancelRequested_WhileGenerating_ClearsBusyWithoutShowingError();
    [Fact]
    public void CancelRequested_WithoutGeneration_DoesNothing();
    [Fact]
    public void PauseRequested_RunningSession_PausesAndNotifiesView();
    [Fact]
    public void PauseRequested_PausedSession_ResumesAndNotifiesView();
    [Fact]
    public void PauseRequested_WithoutSession_IsIgnored();
    [Fact]
    public async Task PauseRequested_WhileGenerating_IsIgnored();
    [Fact]
    public void AfterDispose_PauseAndCancel_AreIgnored();
}

// === tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/ManualClock.cs ===
using Sudoku.Application.Ports;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;
public sealed class ManualClock : IClock
{
    public DateTimeOffset UtcNow { get; set; }

    public void Advance(TimeSpan time);
}

// === tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/ManualUiTimer.cs ===
using Sudoku.WinForms.Services;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;
public sealed class ManualUiTimer : IUiTimer
{
    public event EventHandler? Tick;
    public bool IsRunning { get; private set; }

    public void Start();
    public void Stop();
    public void RaiseTick();
}

// === tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/MemoryGameRepository.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;
public sealed class MemoryGameRepository : IGameRepository
{
    public SavedGame? Stored { get; set; }
    public Exception? LoadFailure { get; set; }
    public int DeleteCalls { get; private set; }

    public void Save(SavedGame game);
    public SavedGame? Load();
    public void Delete();
}

// === tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/MemoryStatisticsRepository.cs ===
using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;
public sealed class MemoryStatisticsRepository : IStatisticsRepository
{
    public GameStatistics Stored { get; set; }
    public Exception? Failure { get; set; }

    public GameStatistics Load();
    public void Save(GameStatistics statistics);
}

// === tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/RecordingDialogs.cs ===
using Sudoku.WinForms.Services;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;
public sealed class RecordingDialogs : IDialogService
{
    public List<(string Title, string Message)> Errors { get; }
    public List<(string Title, string Message)> Infos { get; }
    public bool ConfirmResult { get; set; }
    public int ConfirmCalls { get; private set; }

    public void ShowError(string title, string message);
    public void ShowInfo(string title, string message);
    public bool Confirm(string title, string message);
}

// === tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/StubMainView.cs ===
using Domain;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Theming;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;
#pragma warning disable CS0067 // eventos de la interfaz que estos tests no disparan

public sealed class StubMainView : IMainView
{
    public Difficulty SelectedDifficulty { get; set; }
    public ThemeKind SelectedTheme { get; set; }
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
    public List<string> Statuses { get; }
    public string? LastStatus => Statuses.LastOrDefault();
    public (bool CanUndo, bool CanRedo) UndoRedo { get; private set; }
    public bool? Paused { get; private set; }
    public TimeSpan Elapsed { get; private set; }
    public Difficulty? ShownDifficulty { get; private set; }
    public ThemeKind? SelectedThemeSet { get; private set; }
    public Theme? AppliedTheme { get; private set; }

    public void SetBoard(Board board);
    public void SetDifficulty(Difficulty difficulty);
    public void SetElapsedTime(TimeSpan elapsedTime);
    public void SetStatus(string message);
    public void SetNotesMode(bool enabled);
    public void SetPaused(bool paused);
    public void SetBusy(bool busy);
    public void SetUndoRedoState(bool canUndo, bool canRedo);
    public void SetTheme(Theme theme);
    public void SetSelectedTheme(ThemeKind kind);
    public void RaiseSelection(Position position);
    public void RaiseUndo();
    public void RaiseRedo();
    public void RaiseHint();
    public void RaiseSolve();
    public void RaiseStatistics();
    public void RaiseThemeChange();
    public void RaiseSave();
    public void RaiseContinue();
    public void RaiseClosing();
}
#pragma warning restore CS0067


// === tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/StubSolutionDialog.cs ===
using Domain;
using Sudoku.WinForms.Services;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;
public sealed class StubSolutionDialog : ISolutionDialog
{
    public Board? Shown { get; private set; }
    public Action? WhileOpen { get; set; }

    public void Show(Board solution);
}

// === tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/StubSolver.cs ===
using Domain;
using Sudoku.Application.Ports;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;
public sealed class StubSolver(Board? result) : ISolver
{
    public Board? Solve(Board board);
    public int CountSolutions(Board board, int limit);
}

// === tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/StubStatisticsDialog.cs ===
using Sudoku.Application.Statistics;
using Sudoku.WinForms.Services;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;
public sealed class StubStatisticsDialog : IStatisticsDialog
{
    public GameStatistics? Shown { get; private set; }

    public void Show(GameStatistics statistics);
}

// === tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/StubThemeService.cs ===
using Sudoku.Application.Settings;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;
public sealed class StubThemeService : IThemeService
{
    public Theme Current { get; private set; }
    public Exception? Failure { get; set; }

    public event EventHandler? ThemeChanged;
    public bool SetTheme(ThemeKind kind);
}

// === tests/Sudoku.WinForms.Tests/Presenters/ThemePresenterTests.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;
public sealed class ThemePresenterTests
{
    [Fact]
    public void Constructor_ShowsTheCurrentTheme();
    [Fact]
    public void ThemeChangeRequested_AppliesTheNewTheme();
    [Fact]
    public void ThemeChangeRequested_WhenSavingFails_ShowsErrorAndRestoresTheCombo();
}

// === tests/Sudoku.WinForms.Tests/Theming/PalettesTests.cs ===
using Sudoku.Application.Settings;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Tests.Theming;
public class PalettesTests
{
    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void For_ReturnsPaletteOfRequestedKind(ThemeKind kind);
    [Fact]
    public void For_UnknownKind_Throws();
    [Fact]
    public void LightAndDark_AreDifferentPalettes();
    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void EveryColor_IsDefinedAndOpaque(ThemeKind kind);
    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void HighlightColors_AreDistinguishable(ThemeKind kind);
    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void BoardTexts_AreReadableOnBoardBackground(ThemeKind kind);
    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void WindowText_IsReadableOnWindowBackground(ThemeKind kind);
}

// === tests/Sudoku.WinForms.Tests/Theming/ThemeApplierTests.cs ===
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Tests.Theming;
public class ThemeApplierTests
{
    [Fact]
    public void Apply_PaintsControlsRecursivelyWithTheme();
    [Fact]
    public void Apply_SwitchingThemes_RepaintsControls();
    [Fact]
    public void Apply_NullArguments_Throw();
}

// === tests/Sudoku.WinForms.Tests/Theming/ThemeServiceTests.cs ===
using Sudoku.Application.Games;
using Sudoku.Application.Settings;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Tests.TestDoubles;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Tests.Theming;
public class ThemeServiceTests
{
    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void Constructor_ExposesInitialTheme(ThemeKind initial);
    [Fact]
    public void SetTheme_Different_PersistsChangesCurrentAndRaisesEventOnce();
    [Fact]
    public void SetTheme_EventHandlerSeesNewTheme();
    [Fact]
    public void SetTheme_Same_DoesNothing();
    [Fact]
    public void SetTheme_WhenSettingsAreCorrupt_PropagatesAndKeepsCurrentTheme();
}

```

