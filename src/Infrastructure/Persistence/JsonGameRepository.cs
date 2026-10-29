using System.Text.Json;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Infrastructure.Persistence;

/// <summary>
/// Guarda la partida en curso en <c>games/current.json</c>. Un archivo dañado nunca se borra ni se renombra:
/// <see cref = "Load"/> lanza <see cref = "CorruptSaveException"/> y el presenter decide qué hacer.
/// </summary>
public sealed class JsonGameRepository(IAppDataPaths paths) : IGameRepository
{
    public void Save(SavedGame game)
    {
        ArgumentNullException.ThrowIfNull(game);
        JsonFileStore.Write(paths.CurrentGamePath, GameMapper.ToDto(game));
    }

    public SavedGame? Load()
    {
        GameDto? dto;
        try
        {
            dto = JsonFileStore.Read<GameDto>(paths.CurrentGamePath);
        }
        catch (JsonException ex)
        {
            throw new CorruptSaveException("El archivo de la partida guardada no es un JSON válido.", ex);
        }

        return dto is null ? null : GameMapper.ToSavedGame(dto);
    }

    public void Delete() => JsonFileStore.Delete(paths.CurrentGamePath);
}
