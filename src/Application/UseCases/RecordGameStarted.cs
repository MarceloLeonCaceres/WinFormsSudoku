using Domain;
using Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;

/// <summary>Registra que empezó una partida nueva. Las reanudadas no se vuelven a contar.</summary>
public sealed class RecordGameStarted(IStatisticsRepository repository)
{
    public void Execute(Difficulty difficulty) =>
        repository.Save(repository.Load().WithGameStarted(difficulty));
}
