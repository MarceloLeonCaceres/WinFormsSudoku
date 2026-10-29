using Domain;

namespace Sudoku.Application.UseCases;

/// <summary>Pista: el valor correcto de una celda vacía.</summary>
public readonly record struct Hint(Position Position, int Value);
