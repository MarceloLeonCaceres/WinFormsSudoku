using Domain;
using Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;

/// <summary>
/// Devuelve las celdas del jugador cuyo valor difiere de la solución del puzzle, aunque todavía no choquen
/// con ningún otro valor visible. La solución se calcula con <see cref="SolveGame"/> a partir de las celdas
/// dadas y se conserva mientras estas no cambien, así que solo se resuelve una vez por partida.
/// Solo se usa desde el hilo de UI.
/// </summary>
/// <exception cref="InvalidOperationException">Las celdas dadas no tienen solución.</exception>
public sealed class GetMistakes(SolveGame solveGame)
{
    private Board? _cachedGivens;
    private Board? _cachedSolution;

    public IReadOnlySet<Position> Execute(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var board = session.Board;
        var givens = board.GivensOnly();

        if (_cachedSolution is null || !givens.Equals(_cachedGivens))
        {
            _cachedSolution = solveGame.Execute(session);
            _cachedGivens = givens;
        }

        return SudokuRules.FindMistakes(board, _cachedSolution);
    }
}
