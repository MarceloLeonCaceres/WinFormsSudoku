namespace Infrastructure.Persistence;

/// <summary>
/// Esquema JSON de <c>current.json</c>. Los tipos de dominio no se serializan directamente.
/// Givens/Values: 81 caracteres ('0' = vacía). Notes: 81 máscaras de bits (bit 0 = candidato 1).
/// Las propiedades son anulables para detectar archivos incompletos en lugar de fallar con NullReference.
/// </summary>
internal sealed record GameDto(
    int Version,
    string? Difficulty,
    double ElapsedSeconds,
    string? Givens,
    string? Values,
    int[]? Notes)
{
    public const int CurrentVersion = 1;
}
