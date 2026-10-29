using Domain;

namespace Infrastructure.Solving;

/// <summary>
/// Estado mutable de una búsqueda: valores colocados y valores usados por fila, columna y caja.
/// Existe para no crear tableros inmutables en cada paso del backtracking.
/// </summary>
internal sealed class SolverState
{
    private readonly int[] _values = new int[SudokuConstants.CellCount];
    private readonly CandidateSet[] _rowValues = new CandidateSet[SudokuConstants.Size];
    private readonly CandidateSet[] _columnValues = new CandidateSet[SudokuConstants.Size];
    private readonly CandidateSet[] _boxValues = new CandidateSet[SudokuConstants.Size];

    private SolverState()
    {
    }

    /// <summary>Carga los valores del tablero. Devuelve <c>null</c> si dos de ellos entran en conflicto.</summary>
    public static SolverState? FromBoard(Board board)
    {
        var state = new SolverState();
        foreach (var position in Position.All)
        {
            if (board[position].Value is not { } value)
            {
                continue;
            }

            if (!state.CandidatesFor(position).Contains(value))
            {
                return null;
            }

            state.Place(position, value);
        }

        return state;
    }

    public CandidateSet CandidatesFor(Position position)
    {
        var usedValues = _rowValues[position.Row]
        .Union(_columnValues[position.Column])
        .Union(_boxValues[position.Box]);

        return CandidateSet.Of(
            CandidateSet.All
                .Except(usedValues)
                .ToArray());
    }
    public void Place(Position position, int value)
    {
        _values[position.Index] = value;
        _rowValues[position.Row] = _rowValues[position.Row].Add(value);
        _columnValues[position.Column] = _columnValues[position.Column].Add(value);
        _boxValues[position.Box] = _boxValues[position.Box].Add(value);
    }

    public void Remove(Position position)
    {
        var value = _values[position.Index];
        _values[position.Index] = 0;
        _rowValues[position.Row] = _rowValues[position.Row].Remove(value);
        _columnValues[position.Column] = _columnValues[position.Column].Remove(value);
        _boxValues[position.Box] = _boxValues[position.Box].Remove(value);
    }

    /// <summary>
    /// Heurística MRV: busca la celda vacía con menos candidatos.
    /// Devuelve <c>false</c> si no quedan celdas vacías (el estado es una solución).
    /// </summary>
    public bool TryGetMostConstrainedCell(out Position position, out CandidateSet candidates)
    {
        position = default;
        candidates = default;
        var bestCount = int.MaxValue;

        for (var index = 0; index < SudokuConstants.CellCount; index++)
        {
            if (_values[index] != 0)
            {
                continue;
            }

            var current = Position.All[index];
            var options = CandidatesFor(current);
            if (options.Count >= bestCount)
            {
                continue;
            }

            position = current;
            candidates = options;
            bestCount = options.Count;

            if (bestCount <= 1)
            {
                break; // 0 = callejón sin salida, 1 = jugada forzada: no hay mejor celda.
            }
        }

        return bestCount != int.MaxValue;
    }

    /// <summary>Vuelca la solución sobre el tablero original, conservando sus celdas dadas y valores existentes.</summary>
    public Board ToBoard(Board original)
    {
        var result = original;
        foreach (var position in Position.All)
        {
            if (original[position].IsEmpty)
            {
                result = result.WithValue(position, _values[position.Index]);
            }
        }

        return result;
    }
}
