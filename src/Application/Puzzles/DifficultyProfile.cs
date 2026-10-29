using Domain;

namespace Sudoku.Application.Puzzles;

/// <summary>Política de dificultad: rango de celdas dadas. Única fuente de estos rangos.</summary>
public sealed record DifficultyProfile(Difficulty Difficulty, int MinGivens, int MaxGivens)
{
    private static readonly Dictionary<Difficulty, DifficultyProfile> Profiles = new()
    {
        [Difficulty.Easy] = new(Difficulty.Easy, 40, 45),
        [Difficulty.Medium] = new(Difficulty.Medium, 32, 36),
        [Difficulty.Hard] = new(Difficulty.Hard, 28, 31),
        [Difficulty.Expert] = new(Difficulty.Expert, 24, 27),
    };

    public static DifficultyProfile For(Difficulty difficulty) =>
        Profiles.TryGetValue(difficulty, out var profile)
            ? profile
            : throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Dificultad no soportada.");
}
