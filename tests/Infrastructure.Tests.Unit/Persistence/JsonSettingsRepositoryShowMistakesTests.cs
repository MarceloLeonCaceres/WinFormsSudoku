using System.Text.Json.Nodes;
using global::Infrastructure.Persistence;
using Sudoku.Application.Settings;

namespace Infrastructure.Tests.Unit.Persistence;

public sealed class JsonSettingsRepositoryShowMistakesTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "SudokuTests", Guid.NewGuid().ToString("N"));
    private readonly AppDataPaths _paths;
    private readonly JsonSettingsRepository _repository;

    public JsonSettingsRepositoryShowMistakesTests()
    {
        _paths = new AppDataPaths(_root);
        _repository = new JsonSettingsRepository(_paths);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public void Load_without_file_has_the_option_disabled()
    {
        Assert.False(_repository.Load().ShowMistakes);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Save_then_Load_round_trips_the_option_and_keeps_the_theme(bool showMistakes)
    {
        _repository.Save(new AppSettings(ThemeKind.Dark, showMistakes));

        var loaded = _repository.Load();

        Assert.Equal(showMistakes, loaded.ShowMistakes);
        Assert.Equal(ThemeKind.Dark, loaded.Theme);
    }

    [Fact]
    public void Load_a_file_written_before_the_option_existed_reads_it_as_disabled()
    {
        // Se escribe con la opción activa y se le quita la propiedad: así el archivo imita uno antiguo
        // sin depender de cómo se nombran las propiedades en el JSON.
        _repository.Save(new AppSettings(ThemeKind.Light, true));
        var node = JsonNode.Parse(File.ReadAllText(_paths.SettingsPath))!.AsObject();
        var key = node.Select(property => property.Key)
            .Single(name => string.Equals(name, "ShowMistakes", StringComparison.OrdinalIgnoreCase));
        node.Remove(key);
        File.WriteAllText(_paths.SettingsPath, node.ToJsonString());

        var loaded = _repository.Load();

        Assert.False(loaded.ShowMistakes);
        Assert.Equal(ThemeKind.Light, loaded.Theme);
    }
}
