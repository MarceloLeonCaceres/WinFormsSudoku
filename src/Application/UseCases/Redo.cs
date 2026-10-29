using Sudoku.Application.Games;

namespace Sudoku.Application.UseCases;

public sealed class Redo
{
    public bool Execute(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return session.Redo();
    }
}
