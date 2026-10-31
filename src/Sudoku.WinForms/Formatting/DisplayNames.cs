using Domain;
using Sudoku.Application.Statistics;

namespace Sudoku.WinForms.Formatting;

/// <summary>Única fuente de los textos de UI (en español) para enums del dominio y de Application.</summary>
public static class DisplayNames
{
    public static string Of(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => "Fácil",
        Difficulty.Medium => "Medio",
        Difficulty.Hard => "Difícil",
        Difficulty.Expert => "Experto",
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
    };

    public static string Of(GameOutcome outcome) => outcome switch
    {
        GameOutcome.Won => "Ganada",
        GameOutcome.Lost => "Perdida",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome))
    };
}
