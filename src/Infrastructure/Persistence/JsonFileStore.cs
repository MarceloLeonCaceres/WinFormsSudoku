using System.Text.Json;

namespace Infrastructure.Persistence;

/// <summary>
/// Lectura y escritura de un objeto JSON en un archivo, con opciones compartidas y escritura atómica.
/// La reutilizan los tres repositorios (partida, estadísticas, ajustes).
/// </summary>
public static class JsonFileStore
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    /// <summary>Devuelve <c>null</c> solo si el archivo no existe.</summary>
    /// <exception cref = "JsonException">El contenido no es un JSON válido del tipo esperado (incluye vacío y <c>null</c>).</exception>
    public static T? Read<T>(string path)
        where T : class
    {
        if (!File.Exists(path))
            return null;
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, Options) ?? throw new JsonException("El archivo no contiene datos.");
    }

    public static void Write<T>(string path, T value)
        => AtomicFile.WriteAllText(path, JsonSerializer.Serialize(value, Options));

    /// <summary>Elimina el archivo. No falla si no existe.</summary>
    /// <summary>Elimina el archivo. No falla si el archivo o su carpeta no existen.</summary>
    public static void Delete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (DirectoryNotFoundException)
        {
            // Sin carpeta no hay archivo que borrar.
        }
    }
}
