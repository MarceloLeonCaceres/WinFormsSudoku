using Sudoku.Application.Games;
using Sudoku.Application.Settings;
using Sudoku.Application.UseCases;
using Sudoku.WinForms.Tests.TestDoubles;
using Sudoku.WinForms.Theming;

namespace Sudoku.WinForms.Tests.Theming;

public class ThemeServiceTests
{
    private static (ThemeService Service, InMemorySettingsRepository Repository) Create(ThemeKind initial)
    {
        var repository = new InMemorySettingsRepository(new AppSettings(initial));
        return (new ThemeService(new ChangeTheme(repository), initial), repository);
    }

    [Theory]
    [InlineData(ThemeKind.Light)]
    [InlineData(ThemeKind.Dark)]
    public void Constructor_ExposesInitialTheme(ThemeKind initial)
    {
        var (service, _) = Create(initial);

        Assert.Same(Palettes.For(initial), service.Current);
    }

    [Fact]
    public void SetTheme_Different_PersistsChangesCurrentAndRaisesEventOnce()
    {
        var (service, repository) = Create(ThemeKind.Light);
        var raised = 0;
        service.ThemeChanged += (_, _) => raised++;

        var changed = service.SetTheme(ThemeKind.Dark);

        Assert.True(changed);
        Assert.Equal(1, raised);
        Assert.Same(Palettes.Dark, service.Current);
        Assert.Equal(ThemeKind.Dark, repository.Settings.Theme);
    }

    [Fact]
    public void SetTheme_EventHandlerSeesNewTheme()
    {
        var (service, _) = Create(ThemeKind.Light);
        Theme? seen = null;
        service.ThemeChanged += (_, _) => seen = service.Current;

        service.SetTheme(ThemeKind.Dark);

        Assert.Same(Palettes.Dark, seen);
    }

    [Fact]
    public void SetTheme_Same_DoesNothing()
    {
        var (service, repository) = Create(ThemeKind.Dark);
        var raised = 0;
        service.ThemeChanged += (_, _) => raised++;

        var changed = service.SetTheme(ThemeKind.Dark);

        Assert.False(changed);
        Assert.Equal(0, raised);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public void SetTheme_WhenSettingsAreCorrupt_PropagatesAndKeepsCurrentTheme()
    {
        var (service, repository) = Create(ThemeKind.Light);
        repository.LoadException = new CorruptSaveException("dañado");
        var raised = 0;
        service.ThemeChanged += (_, _) => raised++;

        Assert.Throws<CorruptSaveException>(() => service.SetTheme(ThemeKind.Dark));

        Assert.Same(Palettes.Light, service.Current);
        Assert.Equal(0, raised);
        Assert.Equal(0, repository.SaveCount);
    }
}
