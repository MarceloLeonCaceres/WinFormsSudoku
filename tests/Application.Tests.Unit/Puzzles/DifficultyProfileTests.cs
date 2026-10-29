using Domain;
using Sudoku.Application.Puzzles;

namespace Application.Tests.Unit.Puzzles;

public class DifficultyProfileTests
{
    public static TheoryData<Difficulty> AllDifficulties => new(Enum.GetValues<Difficulty>());

    [Theory]
    [MemberData(nameof(AllDifficulties))]
    public void For_ReturnsProfileWithValidRange(Difficulty difficulty)
    {
        var profile = DifficultyProfile.For(difficulty);

        Assert.Equal(difficulty, profile.Difficulty);
        Assert.InRange(profile.MinGivens, 17, SudokuConstants.CellCount - 1);
        Assert.True(profile.MinGivens <= profile.MaxGivens);
    }

    [Fact]
    public void For_HarderDifficultyHasFewerGivens_WithoutOverlap()
    {
        var ordered = new[] { Difficulty.Easy, Difficulty.Medium, Difficulty.Hard, Difficulty.Expert }
            .Select(DifficultyProfile.For).ToArray();

        for (var i = 0; i < ordered.Length - 1; i++)
            Assert.True(ordered[i].MinGivens > ordered[i + 1].MaxGivens);
    }

    [Fact]
    public void For_UndefinedDifficulty_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => DifficultyProfile.For((Difficulty)999));
}
