using Application.Tests.Unit.TestData;
using Sudoku.Application.Settings;
using Sudoku.Application.UseCases;

namespace Application.Tests.Unit.UseCases;

public class GetSettingsTests
{
    [Fact]
    public void Returns_default_settings_when_nothing_is_saved()
    {
        Assert.Equal(AppSettings.Default, new GetSettings(new InMemorySettingsRepository()).Execute());
    }

    [Fact]
    public void Returns_the_saved_settings()
    {
        var repository = new InMemorySettingsRepository();
        repository.Save(new AppSettings(ThemeKind.Dark));

        Assert.Equal(ThemeKind.Dark, new GetSettings(repository).Execute().Theme);
    }
}
