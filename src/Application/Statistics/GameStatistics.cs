using Domain;
using System.Collections.Immutable;

namespace Sudoku.Application.Statistics;

/// <summary>
/// Estadísticas agregadas por dificultad más las últimas <see cref="MaxRecentGames"/> partidas (la más nueva primero).
/// Inmutable: cada cambio devuelve una instancia nueva. Nota: la igualdad de record compara las colecciones por referencia.
/// </summary>
public sealed record GameStatistics(
    ImmutableDictionary<Difficulty, DifficultyStatistics> ByDifficulty,
    ImmutableList<GameRecord> Recent)
{
    public const int MaxRecentGames = 20;

    public static GameStatistics Empty { get; } =
        new(ImmutableDictionary<Difficulty, DifficultyStatistics>.Empty, ImmutableList<GameRecord>.Empty);

    public DifficultyStatistics For(Difficulty difficulty) =>
        ByDifficulty.TryGetValue(difficulty, out var stats) ? stats : DifficultyStatistics.Empty;

    public GameStatistics WithGameStarted(Difficulty difficulty) =>
        this with { ByDifficulty = ByDifficulty.SetItem(difficulty, For(difficulty).WithStarted()) };

    public GameStatistics WithGameFinished(GameRecord record)
    {
        var updated = For(record.Difficulty).WithFinished(record.Outcome, record.Elapsed);
        var recent = Recent.Insert(0, record);
        if (recent.Count > MaxRecentGames)
            recent = recent.RemoveRange(MaxRecentGames, recent.Count - MaxRecentGames);

        return new GameStatistics(ByDifficulty.SetItem(record.Difficulty, updated), recent);
    }
}
