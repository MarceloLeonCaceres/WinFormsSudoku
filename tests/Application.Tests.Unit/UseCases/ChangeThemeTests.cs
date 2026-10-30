using Application.Tests.Unit.TestData;
using Sudoku.Application.Settings;
using Sudoku.Application.UseCases;

namespace Application.Tests.Unit.UseCases;

public class ChangeThemeTests
{
    private readonly InMemorySettingsRepository _repository = new();

    [Fact]
    public void Saves_the_new_theme_and_returns_true()
    {
        var changed = new ChangeTheme(_repository).Execute(ThemeKind.Dark);

        Assert.True(changed);
        Assert.Equal(ThemeKind.Dark, _repository.Current.Theme);
    }

    [Fact]
    public void Returns_false_and_does_not_save_when_the_theme_is_already_selected()
    {
        var changed = new ChangeTheme(_repository).Execute(AppSettings.Default.Theme);

        Assert.False(changed);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public void Rejects_a_theme_value_that_is_not_defined()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeTheme(_repository).Execute((ThemeKind)99));
        Assert.Equal(0, _repository.SaveCount);
    }
}
