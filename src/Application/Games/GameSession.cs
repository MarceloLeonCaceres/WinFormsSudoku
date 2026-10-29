using Domain;
using global::Sudoku.Application.Puzzles;
using Sudoku.Application.Ports;

namespace Sudoku.Application.Games;

/// <summary>
/// Partida en curso: tablero actual e historial de deshacer/rehacer.
/// Es la única pieza mutable de Application. Como <see cref="Board"/> es inmutable,
/// el historial guarda snapshots sin copiar datos. No es thread-safe: úsala desde el hilo de UI.
/// </summary>
public sealed class GameSession
{
    private readonly Stack<Board> _undo = new();
    private readonly Stack<Board> _redo = new();
    private readonly PlayTimer _timer;
    public GameSession(Puzzle puzzle, IClock clock)
        : this(puzzle.Board, puzzle.Difficulty, TimeSpan.Zero, clock)
    {
    }

    private GameSession(Board board, Difficulty difficulty, TimeSpan elapsed, IClock clock)
    {
        Board = board;
        Difficulty = difficulty;
        _timer = new PlayTimer(clock, elapsed);
    }
    public GameSession(Puzzle puzzle)
    {
        ArgumentNullException.ThrowIfNull(puzzle);
        Board = puzzle.Board;
        Difficulty = puzzle.Difficulty;
    }
    /// <summary>Reanuda una partida guardada: en pausa y con el historial vacío.</summary>
    public static GameSession Restore(SavedGame saved, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(saved);
        return new GameSession(saved.Board, saved.Difficulty, saved.Elapsed, clock);
    }
    public Board Board { get; private set; }
    public Difficulty Difficulty { get; }
    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public TimeSpan ElapsedTime => _timer.Elapsed;

    public bool IsRunning => _timer.IsRunning;

    public void Start() => _timer.Start();

    public void Pause() => _timer.Pause();

    /// <summary>Adopta <paramref name="next"/> como tablero actual. Devuelve <c>false</c> si no cambia nada.</summary>
    public bool Apply(Board next)
    {
        ArgumentNullException.ThrowIfNull(next);
        if (next.Equals(Board)) return false;

        _undo.Push(Board);
        _redo.Clear();
        Board = next;
        return true;
    }

    public bool Undo()
    {
        if (!CanUndo) return false;
        _redo.Push(Board);
        Board = _undo.Pop();
        return true;
    }

    public bool Redo()
    {
        if (!CanRedo) return false;
        _undo.Push(Board);
        Board = _redo.Pop();
        return true;
    }
}
