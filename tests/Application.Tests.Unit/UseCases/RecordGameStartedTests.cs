using Application.Tests.Unit.TestData;
using Domain;
using Sudoku.Application.UseCases;
using Xunit;

namespace Application.Tests.Unit.UseCases;

public class RecordGameStartedTests
{
    [Fact]
    public void Increments_started_for_the_given_difficulty_and_persists()
    {
        var repository = new InMemoryStatisticsRepository();
        var useCase = new RecordGameStarted(repository);

        useCase.Execute(Difficulty.Medium);
        useCase.Execute(Difficulty.Medium);

        Assert.Equal(2, repository.Current.For(Difficulty.Medium).Started);
        Assert.Equal(0, repository.Current.For(Difficulty.Easy).Started);
        Assert.Equal(2, repository.SaveCount);
    }
}
