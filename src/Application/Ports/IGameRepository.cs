using global::Sudoku.Application.Games;

namespace Sudoku.Application.Ports;

/// <summary>Puerto de persistencia de la partida en curso. Lo implementa Infrastructure.</summary>
public interface IGameRepository
{
    void Save(SavedGame game);

    /// <summary>Devuelve la partida guardada, o <c>null</c> si no hay ninguna.</summary>
    /// <exception cref = "CorruptSaveException">El guardado existe pero está dañado o su versión no se reconoce.</exception>
    SavedGame? Load();

    /// <summary>Elimina la partida guardada. No falla si no existe.</summary>
    void Delete();
}
