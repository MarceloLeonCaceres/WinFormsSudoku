using Sudoku.Application.Games;
using System.Text.Json;

namespace Infrastructure.Persistence;

/// <summary>Traduce los fallos de lectura/mapeo (JSON inválido, claves duplicadas, valores fuera de rango) a <see cref="CorruptSaveException"/>.</summary>
internal static class CorruptSaveGuard
{
    public static T Run<T>(string fileName, Func<T> read)
    {
        try
        {
            return read();
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException)
        {
            throw new CorruptSaveException($"El archivo {fileName} está dañado o no se puede interpretar.", ex);
        }
    }
}
