using Domain;
using Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;

/// <summary>Activa o desactiva una nota. Solo aplica a celdas vacías (las dadas y las que tienen valor no admiten notas).</summary>
public sealed class ToggleNote
{
    public bool Execute(GameSession session, Position position, int value)
    {
        ArgumentNullException.ThrowIfNull(session);
        var cell = session.Board[position];
        if (!cell.IsEmpty) return false;

        return session.Apply(session.Board.WithNotes(position, cell.Notes.Toggle(value)));
    }
}
