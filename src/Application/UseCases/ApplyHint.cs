using Domain;
using global::Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;

/// <summary>
/// Calcula una pista con <see cref="GetHint"/> y la coloca con <see cref="PlaceValue"/>.
/// Es un único paso de deshacer y limpia las notas de los peers igual que una jugada normal.
/// Devuelve la pista aplicada, o <c>null</c> si no queda ninguna celda vacía.
/// </summary>
/// <exception cref="InvalidOperationException">Las celdas dadas no tienen solución (lo lanza <see cref="GetHint"/>).</exception>
public sealed class ApplyHint(GetHint getHint, PlaceValue placeValue)
{
    public Hint? Execute(GameSession session, Position? preferred = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (getHint.Execute(session.Board, preferred) is not { } hint) return null;

        placeValue.Execute(session, hint.Position, hint.Value);
        return hint;
    }
}