using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;

/// <summary>Única fuente de las reglas de fila, columna y caja. Pura y sin estado.</summary>
public static class SudokuRules
{
    private static readonly IReadOnlyList<Position>[] PeersByIndex = BuildPeers();

    /// <summary>Las 20 posiciones que comparten fila, columna o caja con <paramref name="position"/> (sin incluirla).</summary>
    public static IReadOnlyList<Position> Peers(Position position) => PeersByIndex[position.Index];

    /// <summary>Indica si <paramref name="value"/> no aparece en ningún peer de <paramref name="position"/>.</summary>
    public static bool IsValidPlacement(Board board, Position position, int value)
    {
        ArgumentNullException.ThrowIfNull(board);
        ValueGuard.EnsureValid(value, nameof(value));

        foreach (var peer in Peers(position))
        {
            if (board[peer].Value == value)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Posiciones cuyo valor está repetido en su fila, columna o caja (se marcan ambas celdas del par).</summary>
    public static IReadOnlySet<Position> FindConflicts(Board board)
    {
        ArgumentNullException.ThrowIfNull(board);

        var conflicts = new HashSet<Position>();
        foreach (var position in Position.All)
        {
            if (board[position].Value is not { } value)
            {
                continue;
            }

            foreach (var peer in Peers(position))
            {
                if (board[peer].Value == value)
                {
                    conflicts.Add(position);
                    break;
                }
            }
        }

        return conflicts;
    }
    
    /// <summary>Celdas del jugador cuyo valor difiere de la solución. Las dadas, las vacías y las notas nunca cuentan.</summary>
    public static IReadOnlySet<Position> FindMistakes(Board board, Board solution)
    {
        ArgumentNullException.ThrowIfNull(board);
        ArgumentNullException.ThrowIfNull(solution);

        var mistakes = new HashSet<Position>();
        foreach (var position in Position.All)
        {
            var cell = board[position];
            if (cell.IsGiven || cell.Value is not { } value)
                continue;

            if (solution[position].Value != value)
                mistakes.Add(position);
        }

        return mistakes;
    }
    public static bool IsComplete(Board board)
    {
        ArgumentNullException.ThrowIfNull(board);
        return Position.All.All(position => !board[position].IsEmpty);
    }

    public static bool IsSolved(Board board) =>
        IsComplete(board) && FindConflicts(board).Count == 0;

    private static IReadOnlyList<Position>[] BuildPeers()
    {
        var table = new IReadOnlyList<Position>[SudokuConstants.CellCount];
        foreach (var position in Position.All)
        {
            var peers = Position.All.Where(other => AreRelated(position, other)).ToArray();
            table[position.Index] = Array.AsReadOnly(peers);
        }

        return table;
    }

    private static bool AreRelated(Position first, Position second) =>
        first != second
        && (first.Row == second.Row || first.Column == second.Column || first.Box == second.Box);
}
