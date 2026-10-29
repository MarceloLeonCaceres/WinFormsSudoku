using System.Text;

namespace Infrastructure.Persistence;

/// <summary>Escritura atómica: se escribe un temporal en la misma carpeta y luego se reemplaza el destino.</summary>
public static class AtomicFile
{
    private static readonly UTF8Encoding Utf8NoBom = new(false);

    public static void WriteAllText(string path, string content)
    {
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);

        var temp = Path.Combine(directory, $"{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temp, content, Utf8NoBom);
            if (File.Exists(fullPath))
                File.Replace(temp, fullPath, null);
            else
                File.Move(temp, fullPath);
        }
        catch
        {
            TryDelete(temp);
            throw;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
