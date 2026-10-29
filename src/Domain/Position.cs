using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;

public readonly record struct Position
{
    public Position(int row, int column)
    {
        if (row is < 0 or >= SudokuConstants.Size)
        {
            throw new ArgumentOutOfRangeException(nameof(row), row, "Row must be between 0 and 8.");
        }

        if (column is < 0 or >= SudokuConstants.Size)
        {
            throw new ArgumentOutOfRangeException(nameof(column), column, "Column must be between 0 and 8.");
        }

        Row = row;
        Column = column;
    }

    public int Row { get; }

    public int Column { get; }

    /// <summary>Índice de la caja 3x3 (0-8, de izquierda a derecha y de arriba abajo).</summary>
    public int Box => (Row / SudokuConstants.BoxSize) * SudokuConstants.BoxSize + Column / SudokuConstants.BoxSize;

    /// <summary>Índice lineal (0-80), por filas.</summary>
    public int Index => Row * SudokuConstants.Size + Column;

    /// <summary>Las 81 posiciones del tablero, en orden por filas.</summary>
    public static IReadOnlyList<Position> All { get; } = BuildAll();

    public static Position FromIndex(int index)
    {
        if (index is < 0 or >= SudokuConstants.CellCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be between 0 and 80.");
        }

        return new Position(index / SudokuConstants.Size, index % SudokuConstants.Size);
    }

    private static IReadOnlyList<Position> BuildAll()
    {
        var positions = new Position[SudokuConstants.CellCount];
        for (var index = 0; index < positions.Length; index++)
        {
            positions[index] = FromIndex(index);
        }

        return Array.AsReadOnly(positions);
    }
}
