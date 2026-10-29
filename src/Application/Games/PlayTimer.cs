using global::Sudoku.Application.Ports;

namespace Sudoku.Application.Games;

/// <summary>
/// Cronómetro de partida con pausa. Acumula el tiempo de los tramos anteriores y suma el tramo en curso.
/// Separado de <see cref = "GameSession"/> para que cada clase tenga una sola razón de cambio.
/// </summary>
public sealed class PlayTimer
{
    private readonly IClock _clock;
    private TimeSpan _accumulated;
    private DateTimeOffset? _startedAt;

    public PlayTimer(IClock clock, TimeSpan accumulated = default)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentOutOfRangeException.ThrowIfLessThan(accumulated, TimeSpan.Zero);
        _clock = clock;
        _accumulated = accumulated;
    }

    public bool IsRunning => _startedAt is not null;

    public TimeSpan Elapsed => _accumulated + CurrentLeg();

    /// <summary>Arranca o reanuda. Si ya está en marcha no hace nada.</summary>
    public void Start() => _startedAt ??= _clock.UtcNow;

    /// <summary>Detiene el conteo conservando lo acumulado. Si ya está en pausa no hace nada.</summary>
    public void Pause()
    {
        if (_startedAt is null)
            return;
        _accumulated += CurrentLeg();
        _startedAt = null;
    }

    private TimeSpan CurrentLeg()
    {
        if (_startedAt is not { } startedAt)
            return TimeSpan.Zero;
        var leg = _clock.UtcNow - startedAt;
        return leg < TimeSpan.Zero ? TimeSpan.Zero : leg;
    }
}
