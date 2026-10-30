using Domain;

namespace Sudoku.Application.Statistics;

/// <summary>Una partida terminada, tal como aparece en la lista de últimas partidas.</summary>
public sealed record GameRecord(Difficulty Difficulty, GameOutcome Outcome, TimeSpan Elapsed, DateTimeOffset FinishedAt);
