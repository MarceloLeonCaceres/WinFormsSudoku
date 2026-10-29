using Domain;
using Sudoku.Application.Ports;
using Sudoku.Application.Puzzles;

namespace Infrastructure.Generation;

/// <summary>
/// Genera puzzles en dos pasos: (1) solución completa aleatoria, (2) quitar celdas en orden aleatorio
/// conservando la unicidad. Pasa un <see cref="Random"/> con semilla para resultados deterministas.
/// <see cref="Random"/> no es thread-safe: usa una instancia por hilo si pasas una propia.
/// </summary>
public sealed class PuzzleGenerator(ISolver solver, Random? random = null) : IPuzzleGenerator
{
    private const int MaxAttempts = 50;
    private const char EmptyChar = '0';

    private readonly ISolver _solver = solver ?? throw new ArgumentNullException(nameof(solver));
    private readonly Random _random = random ?? Random.Shared;

    public Puzzle Generate(Difficulty difficulty, CancellationToken ct = default)
    {
        var profile = DifficultyProfile.For(difficulty);

        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            var solution = BuildSolution();
            var target = _random.Next(profile.MinGivens, profile.MaxGivens + 1);
            var (board, givens) = RemoveCells(solution, target, ct);

            // Si se llegó a un puzzle mínimo con más pistas que el máximo, se descarta y se reintenta.
            if (givens <= profile.MaxGivens)
                return new Puzzle(board, solution, difficulty);
        }

        throw new InvalidOperationException(
            $"No se pudo generar un puzzle {difficulty} tras {MaxAttempts} intentos.");
    }

    /// <summary>Rellena al azar las 3 cajas de la diagonal (son independientes) y deja que el solver complete el resto.</summary>
    private Board BuildSolution()
    {
        var seed = new char[SudokuConstants.CellCount];
        Array.Fill(seed, EmptyChar);

        for (var box = 0; box < SudokuConstants.BoxSize; box++)
        {
            var digits = Enumerable.Range(SudokuConstants.MinValue, SudokuConstants.MaxValue).ToArray();
            _random.Shuffle(digits);

            var k = 0;
            for (var r = 0; r < SudokuConstants.BoxSize; r++)
                for (var c = 0; c < SudokuConstants.BoxSize; c++)
                {
                    var row = box * SudokuConstants.BoxSize + r;
                    var col = box * SudokuConstants.BoxSize + c;
                    seed[row * SudokuConstants.Size + col] = (char)(EmptyChar + digits[k++]);
                }
        }

        var solved = _solver.Solve(Board.FromGivens(new string(seed)))
            ?? throw new InvalidOperationException("El solver no completó una cuadrícula válida.");

        // Todas las celdas como dadas, para que Solution sea comparable con Board.
        return Board.FromGivens(solved.ToValuesString());
    }

    private (Board Board, int Givens) RemoveCells(Board solution, int targetGivens, CancellationToken ct)
    {
        var values = solution.ToValuesString().ToCharArray();
        var order = Enumerable.Range(0, SudokuConstants.CellCount).ToArray();
        _random.Shuffle(order);

        var givens = SudokuConstants.CellCount;
        foreach (var index in order)
        {
            if (givens <= targetGivens) break;
            ct.ThrowIfCancellationRequested();

            var original = values[index];
            values[index] = EmptyChar;

            if (_solver.CountSolutions(Board.FromGivens(new string(values)), 2) == 1)
                givens--;
            else
                values[index] = original;
        }

        return (Board.FromGivens(new string(values)), givens);
    }
}
