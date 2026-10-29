namespace Infrastructure.Persistence;

/// <summary>Rutas bajo una carpeta raíz; por defecto <c>%AppData%\SudokuGame\</c>.</summary>
public sealed class AppDataPaths(string root) : IAppDataPaths
{
    public static AppDataPaths ForCurrentUser() => new(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SudokuGame"));

    public string CurrentGamePath => Path.Combine(root, "games", "current.json");
}
