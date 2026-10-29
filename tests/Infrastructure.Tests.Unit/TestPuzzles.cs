namespace Infrastructure.Tests;

internal static class TestPuzzles
{
    /// <summary>Puzzle clásico fácil, con solución única.</summary>
    public const string Easy =
        "530070000600195000098000060800060003400803001700020006060000280000419005000080079";

    public const string EasySolution =
        "534678912672195348198342567859761423426853791713924856961537284287419635345286179";

    /// <summary>Puzzle muy difícil conocido (Arto Inkala), con solución única.</summary>
    public const string Hard =
        "800000000003600000070090200050007000000045700000100030001000068008500010090000400";

    /// <summary>Dos 5 en la misma fila: contradicción directa entre celdas dadas.</summary>
    public static readonly string ConflictingGivens = "55" + new string('0', 79);

    /// <summary>
    /// Sin conflicto directo, pero la celda (0,8) no tiene candidatos:
    /// su fila contiene 1-8 y su columna contiene 9.
    /// </summary>
    public static readonly string CellWithoutCandidates =
        "123456780" + "000000009" + new string('0', 63);
}
