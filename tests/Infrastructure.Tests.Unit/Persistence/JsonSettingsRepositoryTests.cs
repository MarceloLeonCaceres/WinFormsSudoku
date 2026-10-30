using global::Infrastructure.Persistence;
using global::Infrastructure.Tests.Unit.TestSupport;
using Sudoku.Application.Games;
using Sudoku.Application.Settings;

namespace Infrastructure.Tests.Unit.Persistence;

public sealed class JsonSettingsRepositoryTests : IDisposable
{
    private readonly TempAppData _data = new();
    private readonly AppDataPaths _paths;
    private readonly JsonSettingsRepository _repository;

    public JsonSettingsRepositoryTests()
    {
        _paths = _data.Paths;
        _repository = new JsonSettingsRepository(_paths);
    }

    public void Dispose() => _data.Dispose();

    [Fact]
    public void Load_returns_default_settings_when_the_file_does_not_exist()
    {
        Assert.Equal(AppSettings.Default, _repository.Load());
    }

    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void Save_then_Load_round_trips_the_theme(ThemeKind theme)
    {
        _repository.Save(new AppSettings(theme));

        Assert.Equal(theme, _repository.Load().Theme);
    }

    [Fact]
    public void Load_throws_CorruptSaveException_for_invalid_json_and_leaves_the_file_untouched()
    {
        TempAppData.WriteRaw(_paths.SettingsPath, "[]");

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
        Assert.Equal("[]", File.ReadAllText(_paths.SettingsPath));
    }

    [Fact]
    public void Load_throws_CorruptSaveException_for_an_unknown_version()
    {
        _repository.Save(new AppSettings(ThemeKind.Dark));
        JsonTamper.SetVersion(_paths.SettingsPath, 99);

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }

    [Fact]
    public void Load_throws_CorruptSaveException_for_an_unknown_theme_name()
    {
        _repository.Save(new AppSettings(ThemeKind.Dark));
        JsonTamper.ReplaceText(_paths.SettingsPath, "\"Dark\"", "\"Sepia\"");

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }
}
