using Domain;
using Sudoku.Application.Games;

namespace Sudoku.WinForms.Presenters;

/// <summary>
/// Sesión activa compartida por los presenters, más su estado de ciclo de vida (terminada / guardada).
/// Solo se usa desde el hilo de UI.
/// </summary>
public sealed class CurrentGame
{
    public GameSession? Session { get; private set; }

    /// <summary>La partida ya terminó (victoria registrada o cierre contabilizado).</summary>
    public bool IsFinished { get; private set; }

    /// <summary>Tablero escrito en disco por la última acción de guardar de esta sesión; <c>null</c> si nunca se guardó.</summary>
    public Board? SavedBoard { get; private set; }

    /// <summary>Se dispara justo ANTES de reemplazar una sesión existente: los manejadores aún ven la anterior.</summary>
    public event EventHandler? SessionReplacing;

    /// <summary>Se dispara después de adoptar una sesión nueva o reanudada.</summary>
    public event EventHandler<SessionChangedEventArgs>? SessionChanged;

    /// <summary>Se dispara cuando el tablero de la sesión cambió (jugada, deshacer, rehacer, pista).</summary>
    public event EventHandler? BoardChanged;

    public void Set(GameSession session, bool isResumed = false)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (Session is not null)
            SessionReplacing?.Invoke(this, EventArgs.Empty);

        Session = session;
        IsFinished = false;
        SavedBoard = isResumed ? session.Board : null;
        SessionChanged?.Invoke(this, new SessionChangedEventArgs(isResumed));
    }

    public void MarkFinished() => IsFinished = true;

    public void MarkSaved() => SavedBoard = Session?.Board;

    public void ClearSaved() => SavedBoard = null;

    public void NotifyBoardChanged() => BoardChanged?.Invoke(this, EventArgs.Empty);
}