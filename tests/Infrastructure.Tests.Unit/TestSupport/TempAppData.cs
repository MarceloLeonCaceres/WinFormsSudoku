using global::Infrastructure.Persistence;

namespace Infrastructure.Tests.Unit.TestSupport;

internal sealed class TempAppData : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "SudokuTests", Guid.NewGuid().ToString("N"));

    public AppDataPaths Paths => new(_root);

    /// <summary>Escribe contenido arbitrario en <paramref name="path"/>, creando la carpeta si hace falta.</summary>
    public static void WriteRaw(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
