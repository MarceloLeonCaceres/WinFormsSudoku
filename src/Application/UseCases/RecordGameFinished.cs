using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Ports;
using Sudoku.Application.Statistics;

namespace Sudoku.Application.UseCases;

/// <summary>
/// Registra el final de una partida con el tiempo de la sesión. Abandonar = <see cref="GameOutcome.Lost"/>.
/// </summary>
/// <exception cref="InvalidOperationException">Se pidió <see cref="GameOutcome.Won"/> pero el tablero no está resuelto.</exception>
public sealed class RecordGameFinished(IStatisticsRepository repository, IClock clock)
{
    public void Execute(GameSession session, GameOutcome outcome)
    {
        if (outcome == GameOutcome.Won && !SudokuRules.IsSolved(session.Board))
            throw new InvalidOperationException("No se puede registrar una victoria con el tablero sin resolver.");

        var record = new GameRecord(session.Difficulty, outcome, session.ElapsedTime, clock.UtcNow);
        repository.Save(repository.Load().WithGameFinished(record));
    }
}
