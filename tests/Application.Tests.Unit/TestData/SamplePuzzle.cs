using Domain;
using global::Sudoku.Application.Games;
using global::Sudoku.Application.Puzzles;

namespace Application.Tests.Unit.TestData;

internal static class SamplePuzzle
{
    public const string Givens =
        "530070000600195000098000060800060003400803001700020006060000280000419005000080079";
    public const string Solution =
        "534678912672195348198342567859761423426853791713924856961537284287419635345286179";

    /// <summary>Celda dada (valor 5).</summary>
    public static readonly Position GivenCell = new(0, 0);
    /// <summary>Celda vacía; la solución es 4.</summary>
    public static readonly Position EmptyCell = new(0, 2);

    public static Puzzle Create(Difficulty difficulty = Difficulty.Easy)
        => new(Board.FromGivens(Givens), Board.FromGivens(Solution), difficulty);

    public static GameSession NewSession() => new(Create());
}
