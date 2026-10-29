using System.Globalization;
using Domain;
using Infrastructure.Persistence;
using Sudoku.Application.Games;
using Xunit;

namespace Infrastructure.Tests.Persistence;

public sealed class JsonGameRepositoryTests : IDisposable
{
    private readonly TempDirectory _temp = new();
    private readonly AppDataPaths _paths;
    private readonly JsonGameRepository _repository;

    public JsonGameRepositoryTests()
    {
        _paths = new AppDataPaths(_temp.Path);
        _repository = new JsonGameRepository(_paths);
    }

    public void Dispose() => _temp.Dispose();

    private static SavedGame SampleGame(Difficulty difficulty = Difficulty.Hard, double seconds = 754.5)
    {
        var board = SamplePuzzle.Board()
            .WithValue(new Position(0, 2), 4)
            .WithNotes(new Position(0, 3), CandidateSet.Of(6, 8));
        return new SavedGame(board, difficulty, TimeSpan.FromSeconds(seconds));
    }

    private void WriteRaw(string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_paths.CurrentGamePath)!);
        File.WriteAllText(_paths.CurrentGamePath, content);
    }

    private static string Json(
        int version = 1,
        string difficulty = "Easy",
        double elapsed = 10,
        string? givens = null,
        string? values = null,
        int[]? notes = null)
    {
        givens ??= SamplePuzzle.Givens;
        values ??= givens;
        notes ??= new int[81];
        var seconds = elapsed.ToString(CultureInfo.InvariantCulture);
        return $$"""{ "version": {{version}}, "difficulty": "{{difficulty}}", "elapsedSeconds": {{seconds}}, "givens": "{{givens}}", "values": "{{values}}", "notes": [{{string.Join(",", notes)}}] }""";
    }

    [Fact]
    public void Load_WithoutFile_ReturnsNull()
        => Assert.Null(_repository.Load());

    [Fact]
    public void SaveThenLoad_RoundTripsBoardDifficultyAndElapsed()
    {
        var saved = SampleGame();

        _repository.Save(saved);
        var loaded = _repository.Load();

        Assert.NotNull(loaded);
        Assert.Equal(saved.Board, loaded.Board);
        Assert.Equal(Difficulty.Hard, loaded.Difficulty);
        Assert.Equal(saved.Elapsed, loaded.Elapsed);
    }

    [Fact]
    public void SaveThenLoad_PreservesGivensPlayerValuesAndNotes()
    {
        _repository.Save(SampleGame());

        var board = _repository.Load()!.Board;

        Assert.True(board[new Position(0, 0)].IsGiven);
        Assert.False(board[new Position(0, 2)].IsGiven);
        Assert.Equal(4, board[new Position(0, 2)].Value);
        Assert.Equal(CandidateSet.Of(6, 8), board[new Position(0, 3)].Notes);
        Assert.True(board[new Position(0, 3)].IsEmpty);
    }

    [Theory]
    [InlineData(Difficulty.Easy)]
    [InlineData(Difficulty.Medium)]
    [InlineData(Difficulty.Hard)]
    [InlineData(Difficulty.Expert)]
    public void SaveThenLoad_RoundTripsEveryDifficulty(Difficulty difficulty)
    {
        _repository.Save(SampleGame(difficulty));

        Assert.Equal(difficulty, _repository.Load()!.Difficulty);
    }

    [Fact]
    public void Save_WritesToCurrentGamePath()
    {
        _repository.Save(SampleGame());

        Assert.True(File.Exists(_paths.CurrentGamePath));
        Assert.EndsWith(Path.Combine("games", "current.json"), _paths.CurrentGamePath);
    }

    [Fact]
    public void Save_Twice_KeepsOnlyTheLastGameAndNoTemporaryFiles()
    {
        _repository.Save(SampleGame(Difficulty.Easy, 10));
        _repository.Save(SampleGame(Difficulty.Expert, 20));

        var loaded = _repository.Load()!;

        Assert.Equal(Difficulty.Expert, loaded.Difficulty);
        Assert.Equal(TimeSpan.FromSeconds(20), loaded.Elapsed);
        Assert.Single(Directory.GetFiles(Path.GetDirectoryName(_paths.CurrentGamePath)!));
    }

    [Fact]
    public void Delete_RemovesTheSavedGame()
    {
        _repository.Save(SampleGame());

        _repository.Delete();

        Assert.Null(_repository.Load());
    }

    [Fact]
    public void Delete_WithoutFile_DoesNotThrow()
        => _repository.Delete();
    [Fact]
    public void Delete_WithDirectoryButWithoutFile_DoesNotThrow()
    {
        _repository.Save(SampleGame());
        _repository.Delete();

        _repository.Delete(); // la carpeta ya existe, el archivo no
    }
    [Fact]
    public void Load_ValidHandWrittenJson_Works()
    {
        WriteRaw(Json(difficulty: "Medium", elapsed: 12.5));

        var loaded = _repository.Load();

        Assert.NotNull(loaded);
        Assert.Equal(Difficulty.Medium, loaded.Difficulty);
        Assert.Equal(TimeSpan.FromSeconds(12.5), loaded.Elapsed);
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("{}")]
    public void Load_UnreadableOrIncompleteJson_ThrowsCorruptSaveException(string content)
    {
        WriteRaw(content);

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }

    [Fact]
    public void Load_UnknownVersion_ThrowsCorruptSaveException()
    {
        WriteRaw(Json(version: 99));

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }

    [Theory]
    [InlineData("Nightmare")]
    [InlineData("99")]
    [InlineData("")]
    public void Load_UnknownDifficulty_ThrowsCorruptSaveException(string difficulty)
    {
        WriteRaw(Json(difficulty: difficulty));

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.MaxValue)]
    public void Load_InvalidElapsed_ThrowsCorruptSaveException(double elapsed)
    {
        WriteRaw(Json(elapsed: elapsed));

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }

    [Fact]
    public void Load_WrongBoardLength_ThrowsCorruptSaveException()
    {
        WriteRaw(Json(values: "123"));

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }

    [Fact]
    public void Load_ValueContradictingAGiven_ThrowsCorruptSaveException()
    {
        WriteRaw(Json(values: "1" + SamplePuzzle.Givens[1..]));

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }

    [Fact]
    public void Load_NonDigitValue_ThrowsCorruptSaveException()
    {
        var values = SamplePuzzle.Givens[..2] + "x" + SamplePuzzle.Givens[3..];
        WriteRaw(Json(values: values));

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }

    [Fact]
    public void Load_NotesOnAFilledCell_ThrowsCorruptSaveException()
    {
        var notes = new int[81];
        notes[0] = 1;
        WriteRaw(Json(notes: notes));

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }

    [Fact]
    public void Load_NotesMaskOutOfRange_ThrowsCorruptSaveException()
    {
        var notes = new int[81];
        notes[2] = 1 << 9;
        WriteRaw(Json(notes: notes));

        Assert.Throws<CorruptSaveException>(() => _repository.Load());
    }

    [Fact]
    public void Load_CorruptFile_IsLeftUntouched()
    {
        WriteRaw("{ not json");

        Assert.Throws<CorruptSaveException>(() => _repository.Load());

        Assert.Equal("{ not json", File.ReadAllText(_paths.CurrentGamePath));
    }

    [Fact]
    public void Save_AfterCorruptFile_ReplacesItWithAValidGame()
    {
        WriteRaw("{ not json");

        _repository.Save(SampleGame());

        Assert.NotNull(_repository.Load());
    }
}
