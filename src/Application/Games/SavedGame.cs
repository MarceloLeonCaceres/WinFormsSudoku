using Domain;

namespace Sudoku.Application.Games;

/// <summary>Estado persistible de una partida: tablero (con dadas, valores y notas), dificultad y tiempo jugado. No incluye historial.</summary>
public sealed record SavedGame(Board Board, Difficulty Difficulty, TimeSpan Elapsed);
