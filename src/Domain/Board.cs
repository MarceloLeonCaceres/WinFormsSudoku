using System;
using System.Collections.Generic;
using System.Text;

namespace Domain;

/// <summary>Tablero inmutable de 81 celdas. Cada modificación devuelve un tablero nuevo.</summary>
public sealed class Board : IEquatable<Board>
{
    private readonly Cell[] _cells;

    private Board(Cell[] cells) => _cells = cells;

    public static Board Empty { get; } = new(new Cell[SudokuConstants.CellCount]);

    public Cell this[Position position] => _cells[position.Index];

    /// <summary>
    /// Crea un tablero donde cada dígito 1-9 es una celda dada y '0' o '.' es una celda vacía.
    /// </summary>
    public static Board FromGivens(string puzzle)
    {
        ArgumentNullException.ThrowIfNull(puzzle);

        if (puzzle.Length != SudokuConstants.CellCount)
        {
            throw new FormatException($"Puzzle must have exactly {SudokuConstants.CellCount} characters.");
        }

        var cells = new Cell[SudokuConstants.CellCount];
        for (var index = 0; index < cells.Length; index++)
        {
            cells[index] = ParseCell(puzzle[index], index);
        }

        return new Board(cells);
    }

    public Board WithValue(Position position, int value) =>
        Replace(position, this[position].WithValue(value));

    public Board WithNotes(Position position, CandidateSet notes) =>
        Replace(position, this[position].WithNotes(notes));

    public Board WithCleared(Position position) =>
        Replace(position, this[position].Cleared());

    public Board GivensOnly()
    {
        var cells = new Cell[SudokuConstants.CellCount];
        for (var index = 0; index < cells.Length; index++)
        {
            cells[index] = _cells[index].IsGiven ? _cells[index] : Cell.Empty;
        }

        return new Board(cells);
    }

    /// <summary>81 caracteres con el valor de cada celda (dadas y del jugador); '0' si está vacía.</summary>
    public string ToValuesString()
    {
        var chars = new char[SudokuConstants.CellCount];
        for (var index = 0; index < chars.Length; index++)
        {
            chars[index] = _cells[index].Value is { } value ? (char)('0' + value) : '0';
        }

        return new string(chars);
    }

    public bool Equals(Board? other) =>
        other is not null && _cells.AsSpan().SequenceEqual(other._cells);

    public override bool Equals(object? obj) => Equals(obj as Board);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var cell in _cells)
        {
            hash.Add(cell);
        }

        return hash.ToHashCode();
    }

    private Board Replace(Position position, Cell cell)
    {
        var cells = (Cell[])_cells.Clone();
        cells[position.Index] = cell;
        return new Board(cells);
    }

    private static Cell ParseCell(char character, int index) => character switch
    {
        '0' or '.' => Cell.Empty,
        >= '1' and <= '9' => Cell.Given(character - '0'),
        _ => throw new FormatException($"Invalid character '{character}' at index {index}.")
    };
}
