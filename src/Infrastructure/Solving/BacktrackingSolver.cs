using Sudoku.Application.Ports;
using Domain;

namespace Infrastructure.Solving;

/// <summary>Solver por backtracking que siempre expande la celda con menos candidatos. Sin estado: es seguro reutilizarlo.</summary>
public sealed class BacktrackingSolver : ISolver
{
    public Board? Solve(Board board)
    {
        ArgumentNullException.ThrowIfNull(board);

        var state = SolverState.FromBoard(board);
        if (state is null)
        {
            return null;
        }

        Board? solution = null;
        Search(state, limit: 1, onSolution: solved => solution = solved.ToBoard(board));
        return solution;
    }

    public int CountSolutions(Board board, int limit)
    {
        ArgumentNullException.ThrowIfNull(board);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        var state = SolverState.FromBoard(board);
        return state is null ? 0 : Search(state, limit, onSolution: null);
    }

    /// <summary>Busca soluciones hasta <paramref name="limit"/>. Devuelve cuántas encontró.</summary>
    private static int Search(SolverState state, int limit, Action<SolverState>? onSolution)
    {
        if (!state.TryGetMostConstrainedCell(out var position, out var candidates))
        {
            onSolution?.Invoke(state);
            return 1;
        }

        var found = 0;
        foreach (var value in candidates)
        {
            state.Place(position, value);
            found += Search(state, limit - found, onSolution);
            state.Remove(position);

            if (found >= limit)
            {
                break;
            }
        }

        return found;
    }
}
