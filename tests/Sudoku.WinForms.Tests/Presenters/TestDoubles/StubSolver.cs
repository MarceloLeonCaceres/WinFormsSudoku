using Domain;
using Sudoku.Application.Ports;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

public sealed class StubSolver(Board? result) : ISolver
{
    public Board? Solve(Board board) => result;

    public int CountSolutions(Board board, int limit) => 1;
}
