using Application.Tests.Unit.TestData;
using Domain;
using Sudoku.Application.Statistics;
using Sudoku.Application.UseCases;
using Xunit;

namespace Application.Tests.Unit.UseCases;

public class GetStatisticsTests
{
    [Fact]
    public void Returns_empty_statistics_when_nothing_is_saved()
    {
        var useCase = new GetStatistics(new InMemoryStatisticsRepository());

        Assert.Equal(GameStatistics.Empty, useCase.Execute());
    }

    [Fact]
    public void Returns_what_the_repository_holds()
    {
        var repository = new InMemoryStatisticsRepository();
        repository.Save(GameStatistics.Empty.WithGameStarted(Difficulty.Hard));

        var result = new GetStatistics(repository).Execute();

        Assert.Equal(1, result.For(Difficulty.Hard).Started);
    }
}
