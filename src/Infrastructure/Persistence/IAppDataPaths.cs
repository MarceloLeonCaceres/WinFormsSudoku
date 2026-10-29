namespace Infrastructure.Persistence;

/// <summary>Única fuente de las rutas de archivos de la app. Permite usar una carpeta temporal en las pruebas.</summary>
public interface IAppDataPaths
{
    /// <summary>Ruta de <c>games/current.json</c>.</summary>
    string CurrentGamePath { get; }
}
