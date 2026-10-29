using Domain;
using Sudoku.Application.Puzzles;

namespace Sudoku.Application.Ports;

/// <summary>Puerto del generador. Lo implementa Infrastructure.</summary>
public interface IPuzzleGenerator
{
    /// <summary>Genera un puzzle de solución única con la dificultad pedida.</summary>
    /// <exception cref="OperationCanceledException">Se canceló la generación.</exception>
    /// <exception cref="InvalidOperationException">No se logró un puzzle dentro del rango tras varios intentos.</exception>
    Puzzle Generate(Difficulty difficulty, CancellationToken ct = default);
}
