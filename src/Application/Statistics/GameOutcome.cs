namespace Sudoku.Application.Statistics;

/// <summary>Resultado de una partida terminada. Abandonar una partida cuenta como <see cref="Lost"/>.</summary>
public enum GameOutcome
{
    Won,
    Lost
}