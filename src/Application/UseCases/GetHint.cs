using Sudoku.Application.Ports;
using Domain;

namespace Sudoku.Application.UseCases;

/// <summary>
/// Calcula una pista tomando el valor de la solución del puzzle. No modifica el tablero:
/// aplicar la pista es responsabilidad del llamador (p. ej. el caso de uso PlaceValue).
/// </summary>
public sealed class GetHint(ISolver solver)
{
    /// <summary>
    /// Devuelve la pista para <paramref name="preferred"/> si esa celda está vacía; si no, para la primera celda vacía.
    /// Devuelve <c>null</c> cuando no queda ninguna celda vacía.
    /// </summary>
    /// <exception cref="InvalidOperationException">Las celdas dadas no tienen solución.</exception>
    public Hint? Execute(Board board, Position? preferred = null)
    {
        ArgumentNullException.ThrowIfNull(board);

        if (ChooseTarget(board, preferred) is not { } target)
        {
            return null;
        }

        // Se resuelve solo con las celdas dadas: así la pista es correcta aunque el jugador haya cometido errores.
        var solution = solver.Solve(board.GivensOnly())
            ?? throw new InvalidOperationException("The puzzle givens have no solution.");

        return solution[target].Value is { } value
            ? new Hint(target, value)
            : throw new InvalidOperationException("The solver returned an incomplete board.");
    }

    private static Position? ChooseTarget(Board board, Position? preferred)
    {
        if (preferred is { } selected && board[selected].IsEmpty)
        {
            return selected;
        }

        foreach (var position in Position.All)
        {
            if (board[position].IsEmpty)
            {
                return position;
            }
        }

        return null;
    }
}
