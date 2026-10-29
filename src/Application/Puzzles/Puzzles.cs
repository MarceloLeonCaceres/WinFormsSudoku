using Domain;

namespace Sudoku.Application.Puzzles;

/// <summary>Puzzle generado: tablero con solo celdas dadas, su solución completa y su dificultad.</summary>
public sealed record Puzzle(Board Board, Board Solution, Difficulty Difficulty);
