using Domain;
using global::Infrastructure.Persistence;
using global::Infrastructure.Tests.Unit.TestSupport;
using Sudoku.Application.Games;
using Sudoku.Application.Statistics;

namespace Infrastructure.Tests.Unit.Persistence;


public sealed class JsonStatisticsRepositoryTests : IDisposable
{
    private readonly TempAppData _data = new();
    private readonly AppDataPaths _paths;
    private readonly JsonStatisticsRepository _repository;

    public JsonStatisticsRepositoryTests()
    {
        _paths = _data.Paths;
        _repository = new JsonStatisticsRepository(_paths);
    }

    public void Dispose() => _data.Dispose();

    private static GameStatistics Sample() => GameStatistics.Empty
        .WithGameStarted(Difficulty.Easy)
        .WithGameFinished(new GameRecord(Difficulty.Easy, GameOutcome.Won, TimeSpan.FromSeconds(754),
            new DateTimeOffset(2026, 10, 29, 18, 30, 0, TimeSpan.FromHours(-5))))
        .WithGameStarted(Difficulty.Hard)
        .WithGameFinished(new GameRecord(Difficulty.Hard, GameOutcome.Lost, TimeSpan.FromMinutes(3),
            new DateTimeOffset(2026, 10, 29, 19, 0, 0, TimeSpan.FromHours(-5))));

    [Fact]
    public void Load_returns_empty_statistics_when_the_file_does_not_exist()
    {
        Assert.Equal(GameStatistics.Empty, _repository.Load());
    }

    [Fact]
    public void Save_then_Load_round_trips_aggregates_and_recent_games()
    {
        var original = Sample();

        _repository.Save(original);
        var loaded = _repository.Load();

        foreach (var difficulty in Enum.GetValues<Difficulty>())
            Assert.Equal(original.For(difficulty), loaded.For(difficulty));
        Assert.Equal(original.Recent, loaded.Recent);
    }

    [Fact]
    public void Load_throws_CorruptSaveException_for_invalid_json_and_leaves_the_file_untouched()
    {
        TempAppData.WriteRaw(_paths.StatisticsPath, "{ esto no es json");

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
        Assert.Equal("{ esto no es json", File.ReadAllText(_paths.StatisticsPath));
    }

    [Fact]
    public void Load_throws_CorruptSaveException_for_an_unknown_version()
    {
        _repository.Save(Sample());
        JsonTamper.SetVersion(_paths.StatisticsPath, 99);

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }

    [Fact]
    public void Load_throws_CorruptSaveException_for_an_unknown_difficulty_name()
    {
        _repository.Save(Sample());
        JsonTamper.ReplaceText(_paths.StatisticsPath, "\"Easy\"", "\"Imposible\"");

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }
}
