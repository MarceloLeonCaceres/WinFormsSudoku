using System;
using System.Collections.Generic;
using System.Text;

namespace Sudoku.Application.Statistics;

/// <summary>
/// Agregados de una dificultad. <see cref="Started"/> cuenta al empezar; las abandonadas terminan en <see cref="Lost"/>.
/// Los tiempos (<see cref="BestTime"/>, <see cref="TotalWonTime"/>) solo consideran partidas ganadas.
/// </summary>
public sealed record DifficultyStatistics(int Started, int Won, int Lost, TimeSpan? BestTime, TimeSpan TotalWonTime)
{
    public static DifficultyStatistics Empty { get; } = new(0, 0, 0, null, TimeSpan.Zero);

    public TimeSpan? AverageWonTime => Won == 0 ? null : TotalWonTime / Won;

    public DifficultyStatistics WithStarted() => this with { Started = Started + 1 };

    public DifficultyStatistics WithFinished(GameOutcome outcome, TimeSpan elapsed) => outcome switch
    {
        GameOutcome.Won => this with
        {
            Won = Won + 1,
            TotalWonTime = TotalWonTime + elapsed,
            BestTime = BestTime is { } best && best <= elapsed ? best : elapsed
        },
        GameOutcome.Lost => this with { Lost = Lost + 1 },
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
    };
}
