using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Sudoku.Application.UseCases;

/// <summary>Guarda el tema elegido conservando el resto de preferencias. Devuelve <c>false</c> si ya era ese tema.</summary>
public sealed class ChangeTheme(ISettingsRepository repository)
{
    public bool Execute(ThemeKind theme)
    {
        if (!Enum.IsDefined(theme))
            throw new ArgumentOutOfRangeException(nameof(theme), theme, null);

        var settings = repository.Load();
        if (settings.Theme == theme)
            return false;

        repository.Save(settings with { Theme = theme });
        return true;
    }
}
