using Sudoku.Application.Games;

namespace Infrastructure.Persistence;

internal static class EnumParser
{
    public static TEnum Parse<TEnum>(string? text, string field) where TEnum : struct, Enum
    {
        if (Enum.TryParse(text, ignoreCase: false, out TEnum value) && Enum.IsDefined(value))
            return value;

        throw new CorruptSaveException($"Valor '{text}' no válido para '{field}'.");
    }
}
