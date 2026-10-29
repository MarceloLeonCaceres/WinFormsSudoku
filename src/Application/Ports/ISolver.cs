using Domain;

namespace Application.Ports;

/// <summary>Puerto del solver. Lo implementa Infrastructure.</summary>
public interface ISolver
{
    /// <summary>
    /// Resuelve el tablero tal como llega: todos los valores presentes (dados y del jugador) se tratan como fijos.
    /// Devuelve <c>null</c> si no tiene solución. Las celdas dadas conservan <c>IsGiven</c>; las resueltas son valores de jugador.
    /// </summary>
    Board? Solve(Board board);

    /// <summary>
    /// Cuenta las soluciones del tablero y se detiene al llegar a <paramref name="limit"/> (mínimo 1).
    /// Con <c>limit = 2</c> basta para comprobar unicidad (0 = sin solución, 1 = única, 2 = varias).
    /// </summary>
    int CountSolutions(Board board, int limit);
}
