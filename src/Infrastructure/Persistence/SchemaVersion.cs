using Sudoku.Application.Games;

namespace Infrastructure.Persistence;

internal static class SchemaVersion
{
    public static void Ensure(int found, int supported, string fileName)
    {
        if (found != supported)
            throw new CorruptSaveException($"Versión {found} no reconocida en {fileName} (se esperaba {supported}).");
    }
}
