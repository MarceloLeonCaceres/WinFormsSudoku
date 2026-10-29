using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;

/// <summary>
/// Genera un puzzle y abre una partida nueva. La generación es CPU-bound y síncrona:
/// el presenter debe llamarla con <c>Task.Run</c> y pasar un <see cref="CancellationToken"/>.
/// </summary>
public sealed class StartNewGame(IPuzzleGenerator generator)
{
    public GameSession Execute(Difficulty difficulty, CancellationToken ct = default)
        => new(generator.Generate(difficulty, ct));
}