using Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;

/// <summary>Guarda la preferencia de avisar de errores conservando el resto de ajustes. Devuelve <c>false</c> si ya tenía ese valor.</summary>
public sealed class ChangeShowMistakes(ISettingsRepository repository)
{
    public bool Execute(bool showMistakes)
    {
        var settings = repository.Load();
        if (settings.ShowMistakes == showMistakes)
            return false;

        repository.Save(settings with { ShowMistakes = showMistakes });
        return true;
    }
}
