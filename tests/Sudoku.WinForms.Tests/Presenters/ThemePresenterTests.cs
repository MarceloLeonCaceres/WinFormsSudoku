using Sudoku.Application.Games;
using Sudoku.Application.Settings;
using Sudoku.WinForms.Presenters;
using Sudoku.WinForms.Tests.Presenters.TestDoubles;

namespace Sudoku.WinForms.Tests.Presenters;

public sealed class ThemePresenterTests
{
    private readonly StubMainView _view = new();
    private readonly StubThemeService _themes = new();
    private readonly RecordingDialogs _dialogs = new();

    [Fact]
    public void Constructor_ShowsTheCurrentTheme()
    {
        using var presenter = new ThemePresenter(_view, _themes, _dialogs);

        Assert.Equal(_themes.Current.Kind, _view.SelectedThemeSet);
        Assert.Same(_themes.Current, _view.AppliedTheme);
    }

    [Fact]
    public void ThemeChangeRequested_AppliesTheNewTheme()
    {
        using var presenter = new ThemePresenter(_view, _themes, _dialogs);
        _view.SelectedTheme = ThemeKind.Light;

        _view.RaiseThemeChange();

        Assert.Equal(ThemeKind.Light, _themes.Current.Kind);
        Assert.Same(_themes.Current, _view.AppliedTheme);
    }

    [Fact]
    public void ThemeChangeRequested_WhenSavingFails_ShowsErrorAndRestoresTheCombo()
    {
        using var presenter = new ThemePresenter(_view, _themes, _dialogs);
        _themes.Failure = new CorruptSaveException("dañado");
        _view.SelectedTheme = ThemeKind.Light;

        _view.RaiseThemeChange();

        Assert.Single(_dialogs.Errors);
        Assert.Equal(ThemeKind.Dark, _themes.Current.Kind);
        Assert.Equal(ThemeKind.Dark, _view.SelectedThemeSet);
    }
}
