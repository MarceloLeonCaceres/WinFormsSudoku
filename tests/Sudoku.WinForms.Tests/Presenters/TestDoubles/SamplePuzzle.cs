using Domain;
using Sudoku.Application.Puzzles;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

internal static class SamplePuzzle
{
    private const string Givens = "530070000600195000098000060800060003400803001700020006060000280000419005000080079";
    private const string Solution = "534678912672195348198342567859761423426853791713924856961537284287419635345286179";

    /// <summary>Celda vacía; su valor en la solución es 4.</summary>
    public static readonly Position EmptyCell = new(0, 2);
    /// <summary>Celda dada (valor 5).</summary>
    public static readonly Position GivenCell = new(0, 0);

    public static Puzzle Create(Difficulty difficulty = Difficulty.Easy) =>
        new(Board.FromGivens(Givens), Board.FromGivens(Solution), difficulty);
}
