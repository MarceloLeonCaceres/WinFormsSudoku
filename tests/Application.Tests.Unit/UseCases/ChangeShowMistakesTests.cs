using Sudoku.Application.Settings;
using Sudoku.Application.Tests.TestDoubles;
using Sudoku.Application.UseCases;

namespace Application.Tests.Unit.UseCases;

public class ChangeShowMistakesTests
{
    [Fact]
    public void Saves_the_new_value_and_returns_true()
    {
        var repository = new FakeSettingsRepository();

        var changed = new ChangeShowMistakes(repository).Execute(true);

        Assert.True(changed);
        Assert.True(repository.Stored.ShowMistakes);
    }

    [Fact]
    public void Returns_false_and_does_not_save_when_the_value_is_already_set()
    {
        var repository = new FakeSettingsRepository { Stored = AppSettings.Default with { ShowMistakes = true } };

        var changed = new ChangeShowMistakes(repository).Execute(true);

        Assert.False(changed);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public void Keeps_the_theme()
    {
        var repository = new FakeSettingsRepository { Stored = new AppSettings(ThemeKind.Dark) };

        new ChangeShowMistakes(repository).Execute(true);

        Assert.Equal(ThemeKind.Dark, repository.Stored.Theme);
    }
}
