using Domain;
using global::Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;

/// <summary>
/// Coloca un valor del jugador y quita ese candidato de las notas de los peers.
/// Permite valores en conflicto: la UI los marca con <see cref="SudokuRules.FindConflicts"/>.
/// Devuelve <c>false</c> si la celda es dada o si no hay cambios.
/// </summary>
public sealed class PlaceValue
{
    public bool Execute(GameSession session, Position position, int value)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (session.Board[position].IsGiven) return false;

        var next = session.Board.WithValue(position, value);
        foreach (var peer in SudokuRules.Peers(position))
        {
            var notes = next[peer].Notes;
            if (notes.Contains(value))
                next = next.WithNotes(peer, notes.Remove(value));
        }

        return session.Apply(next);
    }
}