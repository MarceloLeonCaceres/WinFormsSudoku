### `Domain/Board.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
/// <summary>Tablero inmutable de 81 celdas. Cada modificación devuelve un tablero nuevo.</summary>
public sealed class Board : IEquatable<Board>
{
    public static Board Empty { get; }

    public Cell this[Position position] => _cells[position.Index];
    /// <summary>
    /// Crea un tablero donde cada dígito 1-9 es una celda dada y '0' o '.' es una celda vacía.
    /// </summary>
    public static Board FromGivens(string puzzle);
    public Board WithValue(Position position, int value);
    public Board WithNotes(Position position, CandidateSet notes);
    public Board WithCleared(Position position);
    public Board GivensOnly();
    /// <summary>81 caracteres con el valor de cada celda (dadas y del jugador); '0' si está vacía.</summary>
    public string ToValuesString();
    public bool Equals(Board? other);
    public override bool Equals(object? obj);
    public override int GetHashCode();
}
```

### `Domain/CandidateSet.cs`
```csharp
using System.Collections;
using System.Numerics;

namespace Domain;
/// <summary>Conjunto inmutable de candidatos (1-9) representado con un bitmask.</summary>
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
```

### `Domain/Cell.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
/// <summary>
/// Celda inmutable. Invariantes: una celda dada siempre tiene valor, no tiene notas y no se puede modificar;
/// las notas solo existen en celdas vacías.
/// </summary>
public readonly record struct Cell
{
    public int? Value { get; }
    public bool IsGiven { get; }
    public CandidateSet Notes { get; }
    public bool IsEmpty => Value is null;
    public static Cell Empty => default;

    public static Cell Given(int value);
    /// <summary>Coloca un valor del jugador. Limpia las notas de la propia celda.</summary>
    public Cell WithValue(int value);
    public Cell WithNotes(CandidateSet notes);
    /// <summary>Borra valor y notas de una celda del jugador.</summary>
    public Cell Cleared();
}
```

### `Domain/Difficulty.cs`
```csharp
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
```

### `Domain/Position.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
public readonly record struct Position
{
    public Position(int row, int column);
    public int Row { get; }
    public int Column { get; }
    /// <summary>Índice de la caja 3x3 (0-8, de izquierda a derecha y de arriba abajo).</summary>
    public int Box => (Row / SudokuConstants.BoxSize) * SudokuConstants.BoxSize + Column / SudokuConstants.BoxSize;
    /// <summary>Índice lineal (0-80), por filas.</summary>
    public int Index => Row * SudokuConstants.Size + Column;
    /// <summary>Las 81 posiciones del tablero, en orden por filas.</summary>
    public static IReadOnlyList<Position> All { get; }

    public static Position FromIndex(int index);
}
```

### `Domain/SudokuConstants.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
/// <summary>Dimensiones y rango de valores del Sudoku clásico. Única fuente de estas constantes.</summary>
public static class SudokuConstants
{
    public const int Size = 9;
    public const int BoxSize = 3;
    public const int CellCount = Size * Size;
    public const int MinValue = 1;
    public const int MaxValue = Size;
}
```

### `Domain/SudokuRules.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
/// <summary>Única fuente de las reglas de fila, columna y caja. Pura y sin estado.</summary>
public static class SudokuRules
{
    /// <summary>Las 20 posiciones que comparten fila, columna o caja con <paramref name = "position"/> (sin incluirla).</summary>
    public static IReadOnlyList<Position> Peers(Position position);
    /// <summary>Indica si <paramref name = "value"/> no aparece en ningún peer de <paramref name = "position"/>.</summary>
    public static bool IsValidPlacement(Board board, Position position, int value);
    /// <summary>Posiciones cuyo valor está repetido en su fila, columna o caja (se marcan ambas celdas del par).</summary>
    public static IReadOnlySet<Position> FindConflicts(Board board);
    public static bool IsComplete(Board board);
    public static bool IsSolved(Board board);
}
```

### `Domain/ValueGuard.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;
public static class ValueGuard
{
    public static void EnsureValid(int value, string paramName);
}
```

### `Application/Games/CorruptSaveException.cs`
```csharp
namespace Sudoku.Application.Games;
/// <summary>El archivo de partida existe pero no se puede interpretar (dañado, incompleto o de una versión desconocida).</summary>
public sealed class CorruptSaveException : Exception
{
    public CorruptSaveException(string message) : base(message);
    public CorruptSaveException(string message, Exception innerException) : base(message, innerException);
}
```

### `Application/Games/GameSession.cs`
```csharp
using Domain;
using global::Sudoku.Application.Puzzles;
using Sudoku.Application.Ports;

namespace Sudoku.Application.Games;
/// <summary>
/// Partida en curso: tablero actual e historial de deshacer/rehacer.
/// Es la única pieza mutable de Application. Como <see cref = "Board"/> es inmutable,
/// el historial guarda snapshots sin copiar datos. No es thread-safe: úsala desde el hilo de UI.
/// </summary>
public sealed class GameSession
{
    public GameSession(Puzzle puzzle, IClock clock) : this(puzzle.Board, puzzle.Difficulty, TimeSpan.Zero, clock);
    public GameSession(Puzzle puzzle);
    /// <summary>Reanuda una partida guardada: en pausa y con el historial vacío.</summary>
    public static GameSession Restore(SavedGame saved, IClock clock);
    public Board Board { get; private set; }
    public Difficulty Difficulty { get; }
    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    public TimeSpan ElapsedTime => _timer.Elapsed;
    public bool IsRunning => _timer.IsRunning;

    public void Start();
    public void Pause();
    /// <summary>Adopta <paramref name = "next"/> como tablero actual. Devuelve <c>false</c> si no cambia nada.</summary>
    public bool Apply(Board next);
    public bool Undo();
    public bool Redo();
}
```

### `Application/Games/PlayTimer.cs`
```csharp
using global::Sudoku.Application.Ports;

namespace Sudoku.Application.Games;
/// <summary>
/// Cronómetro de partida con pausa. Acumula el tiempo de los tramos anteriores y suma el tramo en curso.
/// Separado de <see cref = "GameSession"/> para que cada clase tenga una sola razón de cambio.
/// </summary>
public sealed class PlayTimer
{
    public PlayTimer(IClock clock, TimeSpan accumulated = default);
    public bool IsRunning => _startedAt is not null;
    public TimeSpan Elapsed => _accumulated + CurrentLeg();

    /// <summary>Arranca o reanuda. Si ya está en marcha no hace nada.</summary>
    public void Start();
    /// <summary>Detiene el conteo conservando lo acumulado. Si ya está en pausa no hace nada.</summary>
    public void Pause();
}
```

### `Application/Games/SavedGame.cs`
```csharp
using Domain;

namespace Sudoku.Application.Games;
/// <summary>Estado persistible de una partida: tablero (con dadas, valores y notas), dificultad y tiempo jugado. No incluye historial.</summary>
public sealed record SavedGame(Board Board, Difficulty Difficulty, TimeSpan Elapsed);
```

### `Application/Ports/IClock.cs`
```csharp
namespace Sudoku.Application.Ports;
/// <summary>Puerto del reloj. Permite probar tiempos sin depender de la hora real.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
```

### `Application/Ports/IGameRepository.cs`
```csharp
using global::Sudoku.Application.Games;

namespace Sudoku.Application.Ports;
/// <summary>Puerto de persistencia de la partida en curso. Lo implementa Infrastructure.</summary>
public interface IGameRepository
{
    void Save(SavedGame game);
    /// <summary>Devuelve la partida guardada, o <c>null</c> si no hay ninguna.</summary>
    /// <exception cref = "CorruptSaveException">El guardado existe pero está dañado o su versión no se reconoce.</exception>
    SavedGame? Load();
    /// <summary>Elimina la partida guardada. No falla si no existe.</summary>
    void Delete();
}
```

### `Application/Ports/IPuzzleGenerator.cs`
```csharp
using Domain;
using Sudoku.Application.Puzzles;

namespace Sudoku.Application.Ports;
/// <summary>Puerto del generador. Lo implementa Infrastructure.</summary>
public interface IPuzzleGenerator
{
    /// <summary>Genera un puzzle de solución única con la dificultad pedida.</summary>
    /// <exception cref = "OperationCanceledException">Se canceló la generación.</exception>
    /// <exception cref = "InvalidOperationException">No se logró un puzzle dentro del rango tras varios intentos.</exception>
    Puzzle Generate(Difficulty difficulty, CancellationToken ct = default);
}
```

### `Application/Ports/ISettingsRepository.cs`
```csharp
using Sudoku.Application.Games;
using Sudoku.Application.Settings;

namespace Sudoku.Application.Ports;
/// <summary>Puerto de persistencia de las preferencias. Lo implementa Infrastructure.</summary>
public interface ISettingsRepository
{
    /// <summary>Devuelve los ajustes guardados, o <see cref = "AppSettings.Default"/> si no hay archivo.</summary>
    /// <exception cref = "CorruptSaveException">El archivo existe pero está dañado o su versión no se reconoce.</exception>
    AppSettings Load();
    void Save(AppSettings settings);
}
```

### `Application/Ports/ISolver.cs`
```csharp
using Domain;

namespace Sudoku.Application.Ports;
/// <summary>Puerto del solver. Lo implementa Infrastructure.</summary>
public interface ISolver
{
    /// <summary>
    /// Resuelve el tablero tal como llega: todos los valores presentes (dados y del jugador) se tratan como fijos.
    /// Devuelve <c>null</c> si no tiene solución. Las celdas dadas conservan <c>IsGiven</c>; las resueltas son valores de jugador.
    /// </summary>
    Board? Solve(Board board);
    /// <summary>
    /// Cuenta las soluciones del tablero y se detiene al llegar a <paramref name = "limit"/> (mínimo 1).
    /// Con <c>limit = 2</c> basta para comprobar unicidad (0 = sin solución, 1 = única, 2 = varias).
    /// </summary>
    int CountSolutions(Board board, int limit);
}
```

### `Application/Ports/IStatisticsRepository.cs`
```csharp
using Sudoku.Application.Games;
using Sudoku.Application.Statistics;

namespace Sudoku.Application.Ports;
/// <summary>Puerto de persistencia de las estadísticas. Lo implementa Infrastructure.</summary>
public interface IStatisticsRepository
{
    /// <summary>Devuelve las estadísticas guardadas, o <see cref = "GameStatistics.Empty"/> si no hay archivo.</summary>
    /// <exception cref = "CorruptSaveException">El archivo existe pero está dañado o su versión no se reconoce.</exception>
    GameStatistics Load();
    void Save(GameStatistics statistics);
}
```

### `Application/Puzzles/DifficultyProfile.cs`
```csharp
using Domain;

namespace Sudoku.Application.Puzzles;
/// <summary>Política de dificultad: rango de celdas dadas. Única fuente de estos rangos.</summary>
public sealed record DifficultyProfile(Difficulty Difficulty, int MinGivens, int MaxGivens)
{
    public static DifficultyProfile For(Difficulty difficulty);
}
```

### `Application/Puzzles/Puzzle.cs`
```csharp
using Domain;

namespace Sudoku.Application.Puzzles;
/// <summary>Puzzle generado: tablero con solo celdas dadas, su solución completa y su dificultad.</summary>
public sealed record Puzzle(Board Board, Board Solution, Difficulty Difficulty);
```

### `Application/Settings/AppSettings.cs`
```csharp
namespace Sudoku.Application.Settings;
/// <summary>Preferencias persistidas de la aplicación.</summary>
public sealed record AppSettings(ThemeKind Theme)
{
    public static AppSettings Default { get; }
}
```

### `Application/Settings/ThemeKind.cs`
```csharp
namespace Sudoku.Application.Settings;
/// <summary>Tema elegido por el jugador. La paleta concreta vive en WinForms.</summary>
public enum ThemeKind
{
    Light,
    Dark
}
```

### `Application/Statistics/DifficultyStatistics.cs`
```csharp
using System;
using System.Collections.Generic;
using System.Text;

namespace Sudoku.Application.Statistics;
/// <summary>
/// Agregados de una dificultad. <see cref = "Started"/> cuenta al empezar; las abandonadas terminan en <see cref = "Lost"/>.
/// Los tiempos (<see cref = "BestTime"/>, <see cref = "TotalWonTime"/>) solo consideran partidas ganadas.
/// </summary>
public sealed record DifficultyStatistics(int Started, int Won, int Lost, TimeSpan? BestTime, TimeSpan TotalWonTime)
{
    public static DifficultyStatistics Empty { get; }
    public TimeSpan? AverageWonTime => Won == 0 ? null : TotalWonTime / Won;

    public DifficultyStatistics WithStarted();
    public DifficultyStatistics WithFinished(GameOutcome outcome, TimeSpan elapsed);
}
```

### `Application/Statistics/GameOutcome.cs`
```csharp
namespace Sudoku.Application.Statistics;
/// <summary>Resultado de una partida terminada. Abandonar una partida cuenta como <see cref = "Lost"/>.</summary>
public enum GameOutcome
{
    Won,
    Lost
}
```

### `Application/Statistics/GameRecord.cs`
```csharp
using Domain;

namespace Sudoku.Application.Statistics;
/// <summary>Una partida terminada, tal como aparece en la lista de últimas partidas.</summary>
public sealed record GameRecord(Difficulty Difficulty, GameOutcome Outcome, TimeSpan Elapsed, DateTimeOffset FinishedAt);
```

### `Application/Statistics/GameStatistics.cs`
```csharp
using Domain;
using System.Collections.Immutable;

namespace Sudoku.Application.Statistics;
/// <summary>
/// Estadísticas agregadas por dificultad más las últimas <see cref = "MaxRecentGames"/> partidas (la más nueva primero).
/// Inmutable: cada cambio devuelve una instancia nueva. Nota: la igualdad de record compara las colecciones por referencia.
/// </summary>
public sealed record GameStatistics(ImmutableDictionary<Difficulty, DifficultyStatistics> ByDifficulty, ImmutableList<GameRecord> Recent)
{
    public const int MaxRecentGames = 20;
    public static GameStatistics Empty { get; }

    public DifficultyStatistics For(Difficulty difficulty);
    public GameStatistics WithGameStarted(Difficulty difficulty);
    public GameStatistics WithGameFinished(GameRecord record);
}
```

### `Application/UseCases/ApplyHint.cs`
```csharp
using Domain;
using global::Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
/// <summary>
/// Calcula una pista con <see cref = "GetHint"/> y la coloca con <see cref = "PlaceValue"/>.
/// Es un único paso de deshacer y limpia las notas de los peers igual que una jugada normal.
/// Devuelve la pista aplicada, o <c>null</c> si no queda ninguna celda vacía.
/// </summary>
/// <exception cref = "InvalidOperationException">Las celdas dadas no tienen solución (lo lanza <see cref = "GetHint"/>).</exception>
public sealed class ApplyHint(GetHint getHint, PlaceValue placeValue)
{
    public Hint? Execute(GameSession session, Position? preferred = null);
}
```

### `Application/UseCases/ChangeTheme.cs`
```csharp
using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Sudoku.Application.UseCases;
/// <summary>Guarda el tema elegido conservando el resto de preferencias. Devuelve <c>false</c> si ya era ese tema.</summary>
public sealed class ChangeTheme(ISettingsRepository repository)
{
    public bool Execute(ThemeKind theme);
}
```

### `Application/UseCases/ClearCell.cs`
```csharp
using Domain;
using global::Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
/// <summary>Borra valor y notas de una celda del jugador. Devuelve <c>false</c> si es dada o ya estaba vacía.</summary>
public sealed class ClearCell
{
    public bool Execute(GameSession session, Position position);
}
```

### `Application/UseCases/GetHint.cs`
```csharp
using Sudoku.Application.Ports;
using Domain;

namespace Sudoku.Application.UseCases;
/// <summary>
/// Calcula una pista tomando el valor de la solución del puzzle. No modifica el tablero:
/// aplicar la pista es responsabilidad del llamador (p. ej. el caso de uso PlaceValue).
/// </summary>
public sealed class GetHint(ISolver solver)
{
    /// <summary>
    /// Devuelve la pista para <paramref name = "preferred"/> si esa celda está vacía; si no, para la primera celda vacía.
    /// Devuelve <c>null</c> cuando no queda ninguna celda vacía.
    /// </summary>
    /// <exception cref = "InvalidOperationException">Las celdas dadas no tienen solución.</exception>
    public Hint? Execute(Board board, Position? preferred = null);
}
```

### `Application/UseCases/GetSettings.cs`
```csharp
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Sudoku.Application.UseCases;
/// <summary>Devuelve las preferencias guardadas (las de por defecto si no hay archivo).</summary>
/// <exception cref = "CorruptSaveException">El archivo de ajustes está dañado.</exception>
public sealed class GetSettings(ISettingsRepository repository)
{
    public AppSettings Execute();
}
```

### `Application/UseCases/GetStatistics.cs`
```csharp
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;

namespace Sudoku.Application.UseCases;
/// <summary>Devuelve las estadísticas guardadas (vacías si aún no hay ninguna).</summary>
/// <exception cref = "CorruptSaveException">El archivo de estadísticas está dañado.</exception>
public sealed class GetStatistics(IStatisticsRepository repository)
{
    public GameStatistics Execute();
}
```

### `Application/UseCases/Hint.cs`
```csharp
using Domain;

namespace Sudoku.Application.UseCases;
/// <summary>Pista: el valor correcto de una celda vacía.</summary>
public readonly record struct Hint(Position Position, int Value);
```

### `Application/UseCases/LoadGame.cs`
```csharp
using global::Sudoku.Application.Games;
using global::Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;
/// <summary>
/// Reanuda la partida guardada. Devuelve <c>null</c> si no hay ninguna; la sesión vuelve en pausa y con el historial vacío.
/// </summary>
/// <exception cref = "CorruptSaveException">El guardado está dañado o es de una versión desconocida.</exception>
public sealed class LoadGame(IGameRepository repository, IClock clock)
{
    public GameSession? Execute();
}
```

### `Application/UseCases/PlaceValue.cs`
```csharp
using Domain;
using global::Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
/// <summary>
/// Coloca un valor del jugador y quita ese candidato de las notas de los peers.
/// Permite valores en conflicto: la UI los marca con <see cref = "SudokuRules.FindConflicts"/>.
/// Devuelve <c>false</c> si la celda es dada o si no hay cambios.
/// </summary>
public sealed class PlaceValue
{
    public bool Execute(GameSession session, Position position, int value);
}
```

### `Application/UseCases/RecordGameFinished.cs`
```csharp
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;

namespace Sudoku.Application.UseCases;
/// <summary>
/// Registra el final de una partida con el tiempo de la sesión. Abandonar = <see cref = "GameOutcome.Lost"/>.
/// </summary>
/// <exception cref = "InvalidOperationException">Se pidió <see cref = "GameOutcome.Won"/> pero el tablero no está resuelto.</exception>
public sealed class RecordGameFinished(IStatisticsRepository repository, IClock clock)
{
    public void Execute(GameSession session, GameOutcome outcome);
}
```

### `Application/UseCases/RecordGameStarted.cs`
```csharp
using Domain;
using Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;
/// <summary>Registra que empezó una partida nueva. Las reanudadas no se vuelven a contar.</summary>
public sealed class RecordGameStarted(IStatisticsRepository repository)
{
    public void Execute(Difficulty difficulty);
}
```

### `Application/UseCases/Redo.cs`
```csharp
using Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
public sealed class Redo
{
    public bool Execute(GameSession session);
}
```

### `Application/UseCases/SaveGame.cs`
```csharp
using global::Sudoku.Application.Games;
using global::Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;
/// <summary>Guarda la partida en curso (tablero, dificultad y tiempo jugado). El historial de deshacer no se guarda.</summary>
public sealed class SaveGame(IGameRepository repository)
{
    public void Execute(GameSession session);
}
```

### `Application/UseCases/StartNewGame.cs`
```csharp
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;
/// <summary>
/// Genera un puzzle y abre una partida nueva. La generación es CPU-bound y síncrona:
/// el presenter debe llamarla con <c>Task.Run</c> y pasar un <see cref = "CancellationToken"/>.
/// </summary>
public sealed class StartNewGame(IPuzzleGenerator generator, IClock clock)
{
    public GameSession Execute(Difficulty difficulty, CancellationToken ct = default);
}
```

### `Application/UseCases/ToggleNote.cs`
```csharp
using Domain;
using Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
/// <summary>Activa o desactiva una nota. Solo aplica a celdas vacías (las dadas y las que tienen valor no admiten notas).</summary>
public sealed class ToggleNote
{
    public bool Execute(GameSession session, Position position, int value);
}
```

### `Application/UseCases/Undo.cs`
```csharp
using Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
public sealed class Undo
{
    public bool Execute(GameSession session);
}
```

### `Infrastructure/Generation/PuzzleGenerator.cs`
```csharp
using Domain;
using Sudoku.Application.Ports;
using Sudoku.Application.Puzzles;

namespace Infrastructure.Generation;
/// <summary>
/// Genera puzzles en dos pasos: (1) solución completa aleatoria, (2) quitar celdas en orden aleatorio
/// conservando la unicidad. Pasa un <see cref = "Random"/> con semilla para resultados deterministas.
/// <see cref = "Random"/> no es thread-safe: usa una instancia por hilo si pasas una propia.
/// </summary>
public sealed class PuzzleGenerator(ISolver solver, Random? random = null) : IPuzzleGenerator
{
    public Puzzle Generate(Difficulty difficulty, CancellationToken ct = default);
}
```

### `Infrastructure/Persistence/AppDataPaths.cs`
```csharp
namespace Infrastructure.Persistence;
/// <summary>Rutas bajo una carpeta raíz; por defecto <c>%AppData%\SudokuGame\</c>.</summary>
public sealed class AppDataPaths(string root) : IAppDataPaths
{
    public static AppDataPaths ForCurrentUser();
    public string CurrentGamePath => Path.Combine(root, "games", "current.json");
    public string StatisticsPath => Path.Combine(root, "statistics.json");
    public string SettingsPath => Path.Combine(root, "settings.json");
}
```

### `Infrastructure/Persistence/AtomicFile.cs`
```csharp
using System.Text;

namespace Infrastructure.Persistence;
/// <summary>Escritura atómica: se escribe un temporal en la misma carpeta y luego se reemplaza el destino.</summary>
public static class AtomicFile
{
    public static void WriteAllText(string path, string content);
}
```

### `Infrastructure/Persistence/IAppDataPaths.cs`
```csharp
namespace Infrastructure.Persistence;
/// <summary>Única fuente de las rutas de archivos de la app. Permite usar una carpeta temporal en las pruebas.</summary>
public interface IAppDataPaths
{
    /// <summary>Ruta de <c>games/current.json</c>.</summary>
    string CurrentGamePath { get; }

    /// <summary>Ruta de <c>statistics.json</c>.</summary>
    string StatisticsPath { get; }

    /// <summary>Ruta de <c>settings.json</c>.</summary>
    string SettingsPath { get; }
}
```

### `Infrastructure/Persistence/JsonFileStore.cs`
```csharp
using System.Text.Json;

namespace Infrastructure.Persistence;
/// <summary>
/// Lectura y escritura de un objeto JSON en un archivo, con opciones compartidas y escritura atómica.
/// La reutilizan los tres repositorios (partida, estadísticas, ajustes).
/// </summary>
public static class JsonFileStore
{
    /// <summary>Devuelve <c>null</c> solo si el archivo no existe.</summary>
    /// <exception cref = "JsonException">El contenido no es un JSON válido del tipo esperado (incluye vacío y <c>null</c>).</exception>
    public static T? Read<T>(string path)
        where T : class;
    public static void Write<T>(string path, T value);
    /// <summary>Elimina el archivo. No falla si el archivo o su carpeta no existen.</summary>
    public static void Delete(string path);
}
```

### `Infrastructure/Persistence/JsonGameRepository.cs`
```csharp
using System.Text.Json;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Infrastructure.Persistence;
/// <summary>
/// Guarda la partida en curso en <c>games/current.json</c>. Un archivo dañado nunca se borra ni se renombra:
/// <see cref = "Load"/> lanza <see cref = "CorruptSaveException"/> y el presenter decide qué hacer.
/// </summary>
public sealed class JsonGameRepository(IAppDataPaths paths) : IGameRepository
{
    public void Save(SavedGame game);
    public SavedGame? Load();
    public void Delete();
}
```

### `Infrastructure/Persistence/JsonSettingsRepository.cs`
```csharp
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Infrastructure.Persistence;
/// <summary>Guarda las preferencias en <c>settings.json</c>. Un archivo dañado lanza <see cref = "CorruptSaveException"/> y no se modifica.</summary>
public sealed class JsonSettingsRepository(IAppDataPaths paths) : ISettingsRepository
{
    public AppSettings Load();
    public void Save(AppSettings settings);
}
```

### `Infrastructure/Persistence/JsonStatisticsRepository.cs`
```csharp
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;

namespace Infrastructure.Persistence;
/// <summary>
/// Guarda las estadísticas en <c>statistics.json</c>. Un archivo dañado nunca se modifica:
/// <see cref = "Load"/> lanza <see cref = "CorruptSaveException"/>.
/// </summary>
public sealed class JsonStatisticsRepository(IAppDataPaths paths) : IStatisticsRepository
{
    public GameStatistics Load();
    public void Save(GameStatistics statistics);
}
```

### `Infrastructure/Solving/BacktrackingSolver.cs`
```csharp
using Sudoku.Application.Ports;
using Domain;

namespace Infrastructure.Solving;
/// <summary>Solver por backtracking que siempre expande la celda con menos candidatos. Sin estado: es seguro reutilizarlo.</summary>
public sealed class BacktrackingSolver : ISolver
{
    public Board? Solve(Board board);
    public int CountSolutions(Board board, int limit);
}
```

### `Infrastructure/Time/SystemClock.cs`
```csharp
using Sudoku.Application.Ports;

namespace Infrastructure.Time;
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
```

### `Sudoku.WinForms/Theming/IThemeService.cs`
```csharp
using Sudoku.Application.Settings;

namespace Sudoku.WinForms.Theming;
/// <summary>Expone el tema actual y avisa cuando cambia. Úsalo solo desde el hilo de UI.</summary>
public interface IThemeService
{
    Theme Current { get; }

    /// <summary>Se dispara después de cambiar <see cref = "Current"/>.</summary>
    event EventHandler? ThemeChanged;
    /// <summary>
    /// Guarda y aplica el tema. Devuelve <c>false</c> si ya era ese tema.
    /// Si guardar falla (p. ej. <see cref = "Sudoku.Application.Games.CorruptSaveException"/>), la excepción
    /// se propaga y el tema no cambia: el presenter decide qué mostrar.
    /// </summary>
    bool SetTheme(ThemeKind kind);
}
```

### `Sudoku.WinForms/Theming/Palettes.cs`
```csharp
using Sudoku.Application.Settings;

namespace Sudoku.WinForms.Theming;
/// <summary>Única fuente de las paletas y de la correspondencia ThemeKind → Theme.</summary>
public static class Palettes
{
    public static Theme Light { get; }
    public static Theme Dark { get; }

    public static Theme For(ThemeKind kind);
}
```

### `Sudoku.WinForms/Theming/Theme.cs`
```csharp
using Sudoku.Application.Settings;

namespace Sudoku.WinForms.Theming;
/// <summary>Paleta inmutable con nombres semánticos. Los controles leen de aquí y nunca usan colores fijos.</summary>
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
```

### `Sudoku.WinForms/Theming/ThemeApplier.cs`
```csharp
namespace Sudoku.WinForms.Theming;
/// <summary>
/// Pinta los controles estándar (formulario, paneles, etiquetas, botones) con el tema, de forma recursiva.
/// Los controles propios (p. ej. BoardControl) leen el tema directamente y no pasan por aquí.
/// </summary>
public static class ThemeApplier
{
    public static void Apply(Theme theme, Control root);
}
```

### `Sudoku.WinForms/Theming/ThemeService.cs`
```csharp
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
```

### `Sudoku.WinForms/Views/IMainView.cs`
```csharp
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
```

### `Sudoku.WinForms/Views/MainForm.cs`
```csharp
using Domain;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Views;
/// <summary>Ventana principal. Solo presenta el estado y comunica las acciones del usuario.</summary>
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
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<ValueEnteredEventArgs>? ValueEntered;
    public event EventHandler? ClearRequested;
    public void SetBoard(Board board);
    public void SetDifficulty(Difficulty difficulty);
    public void SetElapsedTime(TimeSpan elapsedTime);
    public void SetStatus(string message);
    public void SetNotesMode(bool enabled);
    public void SetPaused(bool paused);
    public void SetBusy(bool busy);
    public void SetUndoRedoState(bool canUndo, bool canRedo);
    public void SetTheme(Theme theme);
}
```

### `Sudoku.WinForms/Controls/BoardControl/BoardControl.cs`
```csharp
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
    /// <summary>
    /// Celda seleccionada. Puede ser null cuando no hay selección.
    /// </summary>
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

    /// <summary>
    /// Indica si el jugador está utilizando el modo notas.
    /// La decisión sobre qué operación ejecutar corresponde al Presenter.
    /// </summary>
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

    /// <summary>
    /// Actualiza el tablero y solicita un nuevo dibujo.
    /// </summary>
    public void SetBoard(Board board);
}
```

### `Sudoku.WinForms/Controls/BoardControl/BoardGeometry.cs`
```csharp
using Domain;

namespace Sudoku.WinForms.Controls.BoardControl;
/// <summary>
/// Geometría del tablero. No depende de controles WinForms ni de GDI+.
/// </summary>
public readonly struct BoardGeometry
{
    public RectangleF Bounds { get; }
    public float CellSize { get; }

    /// <summary>
    /// Calcula un tablero cuadrado centrado dentro del espacio disponible.
    /// </summary>
    public static BoardGeometry Create(SizeF availableSize);
    /// <summary>
    /// Devuelve el rectángulo correspondiente a una celda.
    /// </summary>
    public RectangleF GetCellBounds(Position position);
    /// <summary>
    /// Devuelve la posición de la celda que contiene el punto,
    /// o null si el punto está fuera del tablero.
    /// </summary>
    public Position? HitTest(PointF point);
}
```

### `Sudoku.WinForms/Controls/BoardControl/SelectionChangedEventArgs.cs`
```csharp
using Domain;

namespace Sudoku.WinForms.Controls.BoardControl;
public sealed class SelectionChangedEventArgs(Position position) : EventArgs
{
    public Position Position { get; }
}
```

### `Sudoku.WinForms/Controls/BoardControl/ValueEnteredEventArgs.cs`
```csharp
namespace Sudoku.WinForms.Controls.BoardControl;
public sealed class ValueEnteredEventArgs(int value) : EventArgs
{
    public int Value { get; }
}
```

