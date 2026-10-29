using Sudoku.Application.Ports;
using Domain;

namespace Application.Tests.UseCases;

/// <summary>Solver de prueba: devuelve una solución fija y registra cómo se le llamó.</summary>
internal sealed class StubSolver(Board? solution) : ISolver
{
    public int SolveCalls { get; private set; }

    public Board? LastBoard { get; private set; }

    public Board? Solve(Board board)
    {
        SolveCalls++;
        LastBoard = board;
        return solution;
    }

    public int CountSolutions(Board board, int limit) => throw new NotSupportedException();
}
