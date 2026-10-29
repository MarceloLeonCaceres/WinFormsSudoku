using global::Sudoku.Application.Games;
using global::Sudoku.Application.Ports;

namespace Sudoku.Application.UseCases;

/// <summary>Guarda la partida en curso (tablero, dificultad y tiempo jugado). El historial de deshacer no se guarda.</summary>
public sealed class SaveGame(IGameRepository repository)
{
    public void Execute(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        repository.Save(new SavedGame(session.Board, session.Difficulty, session.ElapsedTime));
    }
}
