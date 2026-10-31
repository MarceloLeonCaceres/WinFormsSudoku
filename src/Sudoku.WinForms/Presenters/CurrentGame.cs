using Sudoku.Application.Games;

namespace Sudoku.WinForms.Presenters;

/// <summary>Sesión activa compartida por los presenters. Solo se usa desde el hilo de UI.</summary>
public sealed class CurrentGame
{
    public GameSession? Session { get; private set; }

    public void Set(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        Session = session;
    }
}
