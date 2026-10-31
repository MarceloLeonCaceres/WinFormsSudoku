### `src/Domain/Board.cs`
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

### `src/Domain/CandidateSet.cs`
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

### `src/Domain/Cell.cs`
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

### `src/Domain/Difficulty.cs`
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

### `src/Domain/Position.cs`
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

### `src/Domain/SudokuConstants.cs`
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

### `src/Domain/SudokuRules.cs`
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

### `src/Domain/ValueGuard.cs`
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

### `tests/Application.Tests.Unit/ApplyHintTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/ClearCellTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/GameSessionTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/PlaceValueTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/StartNewGameTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/ToggleNoteTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/UndoTests.cs`
```csharp
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
```

### `tests/Domain.Tests.Unit/BoardGivensOnlyTests.cs`
```csharp
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
```

### `tests/Domain.Tests.Unit/BoardTests.cs`
```csharp
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
```

### `tests/Domain.Tests.Unit/CandidateSetSetOperationsTests.cs`
```csharp
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
```

### `tests/Domain.Tests.Unit/CandidateSetTests.cs`
```csharp
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
```

### `tests/Domain.Tests.Unit/CellTests.cs`
```csharp
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
```

### `tests/Domain.Tests.Unit/PositionTests.cs`
```csharp
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
```

### `tests/Domain.Tests.Unit/SudokuRulesTests.cs`
```csharp
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

### `src/Application/Games/CorruptSaveException.cs`
```csharp
namespace Sudoku.Application.Games;
/// <summary>El archivo de partida existe pero no se puede interpretar (dañado, incompleto o de una versión desconocida).</summary>
public sealed class CorruptSaveException : Exception
{
    public CorruptSaveException(string message) : base(message);
    public CorruptSaveException(string message, Exception innerException) : base(message, innerException);
}
```

### `src/Application/Games/GameSession.cs`
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

### `src/Application/Games/PlayTimer.cs`
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

### `src/Application/Games/SavedGame.cs`
```csharp
using Domain;

namespace Sudoku.Application.Games;
/// <summary>Estado persistible de una partida: tablero (con dadas, valores y notas), dificultad y tiempo jugado. No incluye historial.</summary>
public sealed record SavedGame(Board Board, Difficulty Difficulty, TimeSpan Elapsed);
```

### `src/Application/Ports/IClock.cs`
```csharp
namespace Sudoku.Application.Ports;
/// <summary>Puerto del reloj. Permite probar tiempos sin depender de la hora real.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
```

### `src/Application/Ports/IGameRepository.cs`
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

### `src/Application/Ports/IPuzzleGenerator.cs`
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

### `src/Application/Ports/ISettingsRepository.cs`
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

### `src/Application/Ports/ISolver.cs`
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

### `src/Application/Ports/IStatisticsRepository.cs`
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

### `src/Application/Puzzles/DifficultyProfile.cs`
```csharp
using Domain;

namespace Sudoku.Application.Puzzles;
/// <summary>Política de dificultad: rango de celdas dadas. Única fuente de estos rangos.</summary>
public sealed record DifficultyProfile(Difficulty Difficulty, int MinGivens, int MaxGivens)
{
    public static DifficultyProfile For(Difficulty difficulty);
}
```

### `src/Application/Puzzles/Puzzle.cs`
```csharp
using Domain;

namespace Sudoku.Application.Puzzles;
/// <summary>Puzzle generado: tablero con solo celdas dadas, su solución completa y su dificultad.</summary>
public sealed record Puzzle(Board Board, Board Solution, Difficulty Difficulty);
```

### `src/Application/Settings/AppSettings.cs`
```csharp
namespace Sudoku.Application.Settings;
/// <summary>Preferencias persistidas de la aplicación.</summary>
public sealed record AppSettings(ThemeKind Theme)
{
    public static AppSettings Default { get; }
}
```

### `src/Application/Settings/ThemeKind.cs`
```csharp
namespace Sudoku.Application.Settings;
/// <summary>Tema elegido por el jugador. La paleta concreta vive en WinForms.</summary>
public enum ThemeKind
{
    Light,
    Dark
}
```

### `src/Application/Statistics/DifficultyStatistics.cs`
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

### `src/Application/Statistics/GameOutcome.cs`
```csharp
namespace Sudoku.Application.Statistics;
/// <summary>Resultado de una partida terminada. Abandonar una partida cuenta como <see cref = "Lost"/>.</summary>
public enum GameOutcome
{
    Won,
    Lost
}
```

### `src/Application/Statistics/GameRecord.cs`
```csharp
using Domain;

namespace Sudoku.Application.Statistics;
/// <summary>Una partida terminada, tal como aparece en la lista de últimas partidas.</summary>
public sealed record GameRecord(Difficulty Difficulty, GameOutcome Outcome, TimeSpan Elapsed, DateTimeOffset FinishedAt);
```

### `src/Application/Statistics/GameStatistics.cs`
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

### `src/Application/UseCases/ApplyHint.cs`
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

### `src/Application/UseCases/ChangeTheme.cs`
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

### `src/Application/UseCases/ClearCell.cs`
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

### `src/Application/UseCases/GetHint.cs`
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

### `src/Application/UseCases/GetSettings.cs`
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

### `src/Application/UseCases/GetStatistics.cs`
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

### `src/Application/UseCases/Hint.cs`
```csharp
using Domain;

namespace Sudoku.Application.UseCases;
/// <summary>Pista: el valor correcto de una celda vacía.</summary>
public readonly record struct Hint(Position Position, int Value);
```

### `src/Application/UseCases/LoadGame.cs`
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

### `src/Application/UseCases/PlaceValue.cs`
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

### `src/Application/UseCases/RecordGameFinished.cs`
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

### `src/Application/UseCases/RecordGameStarted.cs`
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

### `src/Application/UseCases/Redo.cs`
```csharp
using Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
public sealed class Redo
{
    public bool Execute(GameSession session);
}
```

### `src/Application/UseCases/SaveGame.cs`
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

### `src/Application/UseCases/StartNewGame.cs`
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

### `src/Application/UseCases/ToggleNote.cs`
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

### `src/Application/UseCases/Undo.cs`
```csharp
using Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;
public sealed class Undo
{
    public bool Execute(GameSession session);
}
```

### `src/Infrastructure/Generation/PuzzleGenerator.cs`
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

### `src/Infrastructure/Persistence/AppDataPaths.cs`
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

### `src/Infrastructure/Persistence/AtomicFile.cs`
```csharp
using System.Text;

namespace Infrastructure.Persistence;
/// <summary>Escritura atómica: se escribe un temporal en la misma carpeta y luego se reemplaza el destino.</summary>
public static class AtomicFile
{
    public static void WriteAllText(string path, string content);
}
```

### `src/Infrastructure/Persistence/IAppDataPaths.cs`
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

### `src/Infrastructure/Persistence/JsonFileStore.cs`
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

### `src/Infrastructure/Persistence/JsonGameRepository.cs`
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

### `src/Infrastructure/Persistence/JsonSettingsRepository.cs`
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

### `src/Infrastructure/Persistence/JsonStatisticsRepository.cs`
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

### `src/Infrastructure/Solving/BacktrackingSolver.cs`
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

### `src/Infrastructure/Time/SystemClock.cs`
```csharp
using Sudoku.Application.Ports;

namespace Infrastructure.Time;
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
```

### `src/Sudoku.WinForms/Presenters/BoardPresenter.cs`
```csharp
using System.Diagnostics.CodeAnalysis;
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Controls.BoardControl;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;
/// <summary>
/// Traduce la interacción con el tablero (selección, valor, borrar) en casos de uso.
/// Decide entre colocar valor y alternar nota según <see cref = "IBoardView.NotesMode"/>.
/// Ignora la entrada si no hay sesión, no hay selección o la partida está en pausa.
/// </summary>
public sealed class BoardPresenter : IDisposable
{
    public BoardPresenter(IBoardView view, CurrentGame current, PlaceValue placeValue, ToggleNote toggleNote, ClearCell clearCell);
    public void Dispose();
}
```

### `src/Sudoku.WinForms/Presenters/CurrentGame.cs`
```csharp
using Sudoku.Application.Games;

namespace Sudoku.WinForms.Presenters;
/// <summary>Sesión activa compartida por los presenters. Solo se usa desde el hilo de UI.</summary>
public sealed class CurrentGame
{
    public GameSession? Session { get; private set; }

    public void Set(GameSession session);
}
```

### `src/Sudoku.WinForms/Presenters/MainPresenter.cs`
```csharp
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Services;
using Sudoku.WinForms.Views;

namespace Sudoku.WinForms.Presenters;
/// <summary>
/// Coordina la ventana principal: nueva partida (generación en segundo plano, cancelable) y pausa.
/// Solo se usa desde el hilo de UI.
/// </summary>
public sealed class MainPresenter : IDisposable
{
    public MainPresenter(IMainView view, StartNewGame startNewGame, CurrentGame current, IDialogService dialogs);
    public void Dispose();
    /// <summary>Genera una partida nueva. Si ya había una generación en curso, la cancela. No lanza excepciones.</summary>
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

```

### `src/Sudoku.WinForms/Services/IDialogService.cs`
```csharp
namespace Sudoku.WinForms.Services;
/// <summary>Puerto de diálogos: permite que los presenters muestren mensajes sin depender de WinForms.</summary>
public interface IDialogService
{
    void ShowError(string title, string message);
}
```

### `src/Sudoku.WinForms/Services/MessageBoxDialogService.cs`
```csharp
namespace Sudoku.WinForms.Services;
public sealed class MessageBoxDialogService : IDialogService
{
    public void ShowError(string title, string message);
}
```

### `src/Sudoku.WinForms/Theming/IThemeService.cs`
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

### `src/Sudoku.WinForms/Theming/Palettes.cs`
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

### `src/Sudoku.WinForms/Theming/Theme.cs`
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

### `src/Sudoku.WinForms/Theming/ThemeApplier.cs`
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

### `src/Sudoku.WinForms/Theming/ThemeService.cs`
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

### `src/Sudoku.WinForms/Views/IBoardView.cs`
```csharp
using Domain;
using Sudoku.WinForms.Controls.BoardControl;

namespace Sudoku.WinForms.Views;
/// <summary>Contrato mínimo del tablero para su presenter. No expone controles WinForms.</summary>
public interface IBoardView
{
    bool NotesMode { get; }

    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
    event EventHandler<ValueEnteredEventArgs>? ValueEntered;
    event EventHandler? ClearRequested;
    void SetBoard(Board board);
}
```

### `src/Sudoku.WinForms/Views/IGameControlsView.cs`
```csharp
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
    void SetDifficulty(Difficulty difficulty);
    void SetElapsedTime(TimeSpan elapsedTime);
    void SetStatus(string message);
    void SetNotesMode(bool enabled);
    void SetPaused(bool paused);
    /// <summary>Mientras está ocupada, "Nueva partida" pasa a "Cancelar" y el resto de acciones se deshabilita.</summary>
    void SetBusy(bool busy);
    void SetUndoRedoState(bool canUndo, bool canRedo);
    void SetTheme(Theme theme);
}
```

### `src/Sudoku.WinForms/Views/IMainView.cs`
```csharp
using Domain;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Views;
/// <summary>Vista principal: composición del tablero y los controles de juego. Solo la usa MainPresenter.</summary>
public interface IMainView : IBoardView, IGameControlsView
{
}// Antes de extraer IGameControlsView, IMainView era así:
//public interface IMainView : IBoardView
//{
//    Difficulty SelectedDifficulty { get; }
//    ThemeKind SelectedTheme { get; }
//    event EventHandler? NewGameRequested;
//    event EventHandler? DifficultyChanged;
//    event EventHandler? UndoRequested;
//    event EventHandler? RedoRequested;
//    event EventHandler? NotesModeChanged;
//    event EventHandler? HintRequested;
//    event EventHandler? SolveRequested;
//    event EventHandler? PauseRequested;
//    event EventHandler? StatisticsRequested;
//    event EventHandler? ThemeChangeRequested;
//    void SetDifficulty(Difficulty difficulty);
//    void SetElapsedTime(TimeSpan elapsedTime);
//    void SetStatus(string message);
//    void SetNotesMode(bool enabled);
//    void SetPaused(bool paused);
//    void SetBusy(bool busy);
//    void SetUndoRedoState(bool canUndo, bool canRedo);
//    void SetTheme(Theme theme);
//}

```

### `src/Sudoku.WinForms/Views/MainForm.cs`
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
    public event EventHandler? CancelRequested;
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

### `tests/Application.Tests.Unit/Games/GameSessionTimerTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/Puzzles/DifficultyProfileTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/Statistics/DifficultyStatisticsTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/Statistics/GameStatisticsTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/TestDoubles/InMemoryGameRepository.cs`
```csharp
using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Sudoku.Application.Tests.TestDoubles;
public sealed class InMemoryGameRepository : IGameRepository
{
    public SavedGame? Stored { get; set; }
    /// <summary>Si se asigna, <see cref = "Load"/> la lanza (simula un guardado dañado).</summary>
    public Exception? LoadFailure { get; set; }

    public void Save(SavedGame game);
    public SavedGame? Load();
    public void Delete();
}
```

### `tests/Application.Tests.Unit/UseCases/ChangeThemeTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/UseCases/GetHintTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/UseCases/GetSettingsTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/UseCases/GetStatisticsTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/UseCases/LoadGameTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/UseCases/RecordGameFinishedTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/UseCases/RecordGameStartedTests.cs`
```csharp
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
```

### `tests/Application.Tests.Unit/UseCases/SaveGameTests.cs`
```csharp
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
```

### `tests/Infrastructure.Tests.Unit/Generarion/PuzzleGeneratorTests.cs`
```csharp
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
```

### `tests/Infrastructure.Tests.Unit/Persistence/AtomicFileTests.cs`
```csharp
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
```

### `tests/Infrastructure.Tests.Unit/Persistence/JsonGameRepositoryTests.cs`
```csharp
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
```

### `tests/Infrastructure.Tests.Unit/Persistence/JsonSettingsRepositoryTests.cs`
```csharp
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
```

### `tests/Infrastructure.Tests.Unit/Persistence/JsonStatisticsRepositoryTests.cs`
```csharp
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
```

### `tests/Infrastructure.Tests.Unit/Solving/BacktrackingSolverTests.cs`
```csharp
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

### `tests/Sudoku.WinForms.Tests/Theming/PalettesTests.cs`
```csharp
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
```

### `tests/Sudoku.WinForms.Tests/Theming/ThemeApplierTests.cs`
```csharp
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
```

### `tests/Sudoku.WinForms.Tests/Theming/ThemeServiceTests.cs`
```csharp
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

### `src/Sudoku.WinForms/Controls/BoardControl/BoardControl.cs`
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

### `src/Sudoku.WinForms/Controls/BoardControl/BoardGeometry.cs`
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

### `src/Sudoku.WinForms/Controls/BoardControl/SelectionChangedEventArgs.cs`
```csharp
using Domain;

namespace Sudoku.WinForms.Controls.BoardControl;
public sealed class SelectionChangedEventArgs(Position position) : EventArgs
{
    public Position Position { get; }
}
```

### `src/Sudoku.WinForms/Controls/BoardControl/ValueEnteredEventArgs.cs`
```csharp
namespace Sudoku.WinForms.Controls.BoardControl;
public sealed class ValueEnteredEventArgs(int value) : EventArgs
{
    public int Value { get; }
}
```

### `tests/Sudoku.WinForms.Tests/Controls/Board/BoardGeometryTests.cs`
```csharp
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
```

### `tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/BoardPresenterTests.cs`
```csharp
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
```

### `tests/Sudoku.WinForms.Tests/Presenters/TestDoubles/MainPresenterTests.cs`
```csharp
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
```

