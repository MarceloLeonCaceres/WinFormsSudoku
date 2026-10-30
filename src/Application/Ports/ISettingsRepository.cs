using Sudoku.Application.Games;
using Sudoku.Application.Settings;

namespace Sudoku.Application.Ports;

/// <summary>Puerto de persistencia de las preferencias. Lo implementa Infrastructure.</summary>
public interface ISettingsRepository
{
    /// <summary>Devuelve los ajustes guardados, o <see cref="AppSettings.Default"/> si no hay archivo.</summary>
    /// <exception cref="CorruptSaveException">El archivo existe pero está dañado o su versión no se reconoce.</exception>
    AppSettings Load();

    void Save(AppSettings settings);
}
