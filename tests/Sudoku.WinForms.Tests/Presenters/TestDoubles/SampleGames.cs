using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Puzzles;

namespace Sudoku.WinForms.Tests.Presenters.TestDoubles;

/// <summary>Puzzle conocido y su solución. La celda (0,2) vale 4 en la solución.</summary>
internal static class SampleGames
{
    public const string PuzzleGivens = "530070000600195000098000060800060003400803001700020006060000280000419005000080079";
    public const string SolutionText = "534678912672195348198342567859761423426853791713924856961537284287419635345286179";
    public const string AlmostSolvedGivens = "530678912672195348198342567859761423426853791713924856961537284287419635345286179";

    public static readonly Position EmptyCell = new(0, 2);
    public const int EmptyCellSolution = 4;

    public static Board SolutionBoard => Board.FromGivens(SolutionText);

    /// <summary>Sesión nueva (Easy). Con <paramref name="almostSolved"/> solo falta la celda (0,2).</summary>
    public static GameSession NewSession(IClock clock, bool almostSolved = false, bool started = true)
    {
        var givens = almostSolved ? AlmostSolvedGivens : PuzzleGivens;
        var session = new GameSession(new Puzzle(Board.FromGivens(givens), SolutionBoard, Difficulty.Easy), clock);
        if (started)
            session.Start();
        return session;
    }
}
