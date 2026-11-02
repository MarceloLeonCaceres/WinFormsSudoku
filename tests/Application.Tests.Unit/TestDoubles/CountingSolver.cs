using Domain;
using Sudoku.Application.Ports;

namespace Sudoku.Application.Tests.TestDoubles;

/// <summary>Solver de prueba: devuelve siempre el mismo resultado y cuenta cuántas veces se le pidió resolver.</summary>
public sealed class CountingSolver(Board? result) : ISolver
{
    public int SolveCalls { get; private set; }
    public Board? LastBoard { get; private set; }

    public Board? Solve(Board board)
    {
        SolveCalls++;
        LastBoard = board;
        return result;
    }

    public int CountSolutions(Board board, int limit) => throw new NotSupportedException();
}
