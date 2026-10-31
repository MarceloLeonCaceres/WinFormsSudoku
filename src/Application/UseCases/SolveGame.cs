using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;

/// <summary>
/// Calcula la solución completa de la partida a partir de sus celdas dadas (ignora los valores del jugador,
/// que pueden ser erróneos). No modifica la sesión ni las estadísticas: mostrar la solución es cosa de la vista.
/// </summary>
/// <exception cref="InvalidOperationException">Las celdas dadas no tienen solución.</exception>
public sealed class SolveGame(ISolver solver)
{
    public Board Execute(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return solver.Solve(session.Board.GivensOnly())
            ?? throw new InvalidOperationException("Las celdas dadas no tienen solución.");
    }
}
