using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Settings;

namespace Sudoku.Application.UseCases;

/// <summary>Devuelve las preferencias guardadas (las de por defecto si no hay archivo).</summary>
/// <exception cref="CorruptSaveException">El archivo de ajustes está dañado.</exception>
public sealed class GetSettings(ISettingsRepository repository)
{
    public AppSettings Execute() => repository.Load();
}
