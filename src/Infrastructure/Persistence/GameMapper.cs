using Domain;
using Sudoku.Application.Games;

namespace Infrastructure.Persistence;

/// <summary>Convierte entre <see cref = "SavedGame"/> y <see cref = "GameDto"/>. Todo dato inválido se informa como <see cref = "CorruptSaveException"/>.</summary>
internal static class GameMapper
{
    private const int AllNotesMask = (1 << SudokuConstants.MaxValue) - 1;

    public static GameDto ToDto(SavedGame game)
    {
        var board = game.Board;
        var notes = new int[SudokuConstants.CellCount];
        foreach (var position in Position.All)
            notes[position.Index] = ToMask(board[position].Notes);

        return new GameDto(
            GameDto.CurrentVersion,
            game.Difficulty.ToString(),
            game.Elapsed.TotalSeconds,
            board.GivensOnly().ToValuesString(),
            board.ToValuesString(),
            notes);
    }

    public static SavedGame ToSavedGame(GameDto dto)
    {
        if (dto.Version != GameDto.CurrentVersion)
            throw new CorruptSaveException($"Versión de partida no soportada: {dto.Version}.");

        if (dto.Givens is not { Length: SudokuConstants.CellCount } givens
            || dto.Values is not { Length: SudokuConstants.CellCount } values
            || dto.Notes is not { Length: SudokuConstants.CellCount } notes)
            throw new CorruptSaveException("La partida guardada está incompleta.");

        if (!Enum.TryParse<Difficulty>(dto.Difficulty, out var difficulty) || !Enum.IsDefined(difficulty))
            throw new CorruptSaveException($"Dificultad desconocida: '{dto.Difficulty}'.");

        if (!double.IsFinite(dto.ElapsedSeconds) || dto.ElapsedSeconds < 0)
            throw new CorruptSaveException("El tiempo guardado no es válido.");

        try
        {
            return new SavedGame(Rebuild(givens, values, notes), difficulty, TimeSpan.FromSeconds(dto.ElapsedSeconds));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or OverflowException)
        {
            throw new CorruptSaveException("La partida guardada contiene un tablero inválido.", ex);
        }
    }

    private static Board Rebuild(string givens, string values, int[] notes)
    {
        var board = Board.FromGivens(givens);

        foreach (var position in Position.All)
        {
            var index = position.Index;
            var isGiven = givens[index] != '0';

            if (isGiven)
            {
                if (values[index] != givens[index])
                    throw new CorruptSaveException("Una celda dada no coincide con su valor guardado.");
            }
            else if (values[index] != '0')
            {
                board = board.WithValue(position, values[index] - '0');
            }
        }

        foreach (var position in Position.All)
        {
            var mask = notes[position.Index];
            if (mask == 0)
                continue;
            if ((mask & ~AllNotesMask) != 0)
                throw new CorruptSaveException("Una máscara de notas está fuera de rango.");
            if (!board[position].IsEmpty)
                throw new CorruptSaveException("Hay notas en una celda que ya tiene valor.");
            board = board.WithNotes(position, FromMask(mask));
        }

        return board;
    }

    private static int ToMask(CandidateSet notes)
    {
        var mask = 0;
        foreach (var value in notes)
            mask |= 1 << (value - SudokuConstants.MinValue);
        return mask;
    }

    private static CandidateSet FromMask(int mask)
    {
        var set = CandidateSet.Empty;
        for (var value = SudokuConstants.MinValue; value <= SudokuConstants.MaxValue; value++)
        {
            if ((mask & (1 << (value - SudokuConstants.MinValue))) != 0)
                set = set.Add(value);
        }

        return set;
    }
}
