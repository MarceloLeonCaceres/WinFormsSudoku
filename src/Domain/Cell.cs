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
    private Cell(int? value, bool isGiven, CandidateSet notes)
    {
        Value = value;
        IsGiven = isGiven;
        Notes = notes;
    }

    public int? Value { get; }

    public bool IsGiven { get; }

    public CandidateSet Notes { get; }

    public bool IsEmpty => Value is null;

    public static Cell Empty => default;

    public static Cell Given(int value)
    {
        ValueGuard.EnsureValid(value, nameof(value));
        return new Cell(value, isGiven: true, CandidateSet.Empty);
    }

    /// <summary>Coloca un valor del jugador. Limpia las notas de la propia celda.</summary>
    public Cell WithValue(int value)
    {
        EnsureNotGiven();
        ValueGuard.EnsureValid(value, nameof(value));
        return new Cell(value, isGiven: false, CandidateSet.Empty);
    }

    public Cell WithNotes(CandidateSet notes)
    {
        EnsureNotGiven();
        if (!IsEmpty)
        {
            throw new InvalidOperationException("Notes can only be set on an empty cell.");
        }

        return new Cell(value: null, isGiven: false, notes);
    }

    /// <summary>Borra valor y notas de una celda del jugador.</summary>
    public Cell Cleared()
    {
        EnsureNotGiven();
        return Empty;
    }

    private void EnsureNotGiven()
    {
        if (IsGiven)
        {
            throw new InvalidOperationException("A given cell cannot be modified.");
        }
    }
}
