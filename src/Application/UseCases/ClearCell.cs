using Domain;
using global::Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;

/// <summary>Borra valor y notas de una celda del jugador. Devuelve <c>false</c> si es dada o ya estaba vacía.</summary>
public sealed class ClearCell
{
    public bool Execute(GameSession session, Position position)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (session.Board[position].IsGiven) return false;

        return session.Apply(session.Board.WithCleared(position));
    }
}
