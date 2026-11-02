using Domain;

namespace Domain.Tests;

public sealed class SudokuRulesMistakesTests
{
    private const string SolutionDigits =
        "534678912672195348198342567859761423426853791713924856961537284287419635345286179";

    private static readonly Board Solution = Board.FromGivens(SolutionDigits);

    // Solo el 5 de la esquina es dado: casi cualquier valor erróneo no choca con nada visible.
    private static Board OnlyFiveGiven() => Board.FromGivens("5" + new string('.', 80));

    [Fact]
    public void BoardWithoutPlayerValues_HasNoMistakes()
    {
        Assert.Empty(SudokuRules.FindMistakes(OnlyFiveGiven(), Solution));
    }

    [Fact]
    public void CorrectPlayerValue_IsNotAMistake()
    {
        var board = OnlyFiveGiven().WithValue(new Position(0, 1), 3);

        Assert.Empty(SudokuRules.FindMistakes(board, Solution));
    }

    [Fact]
    public void WrongValue_IsAMistake_EvenWhenNothingConflictsYet()
    {
        var wrong = new Position(0, 1); // la solución tiene 3
        var board = OnlyFiveGiven().WithValue(wrong, 9);

        Assert.Empty(SudokuRules.FindConflicts(board));
        var mistakes = SudokuRules.FindMistakes(board, Solution);
        Assert.Equal(wrong, Assert.Single(mistakes));
    }

    [Fact]
    public void Duplicate_MarksOnlyTheCellThatDiffersFromTheSolution()
    {
        var wrong = new Position(0, 8); // la solución tiene 2; el 5 duplica al dado de la misma fila
        var board = OnlyFiveGiven().WithValue(wrong, 5);

        Assert.Equal(2, SudokuRules.FindConflicts(board).Count);
        Assert.Equal(wrong, Assert.Single(SudokuRules.FindMistakes(board, Solution)));
    }

    [Fact]
    public void GivenCells_AreNeverMistakes()
    {
        var board = Board.FromGivens("1" + new string('.', 80)); // el dado 1 difiere de la solución (5)

        Assert.Empty(SudokuRules.FindMistakes(board, Solution));
    }

    [Fact]
    public void EmptyCellsWithNotes_AreNotMistakes()
    {
        var board = OnlyFiveGiven().WithNotes(new Position(0, 1), CandidateSet.Of(9));

        Assert.Empty(SudokuRules.FindMistakes(board, Solution));
    }

    [Fact]
    public void NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => SudokuRules.FindMistakes(null!, Solution));
        Assert.Throws<ArgumentNullException>(() => SudokuRules.FindMistakes(OnlyFiveGiven(), null!));
    }
}
