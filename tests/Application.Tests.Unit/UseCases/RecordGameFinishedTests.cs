using Application.Tests.Unit.TestData;
using Domain;
using Sudoku.Application.Games;
using Sudoku.Application.Puzzles;
using Sudoku.Application.Statistics;
using Sudoku.Application.UseCases;

namespace Application.Tests.Unit.UseCases;

public class RecordGameFinishedTests
{
    private const string SolvedGrid =
        "534678912672195348198342567859761423426853791713924856961537284287419635345286179";

    private readonly FakeClock _clock = new();
    private readonly InMemoryStatisticsRepository _repository = new();

    private RecordGameFinished CreateUseCase() => new(_repository, _clock);

    private GameSession SessionWith(Board board, Difficulty difficulty, TimeSpan played)
    {
        var session = new GameSession(new Puzzle(board, board, difficulty), _clock);
        session.Start();
        _clock.Advance(played);
        return session;
    }

    [Fact]
    public void Won_records_elapsed_time_difficulty_and_finish_moment()
    {
        var session = SessionWith(Board.FromGivens(SolvedGrid), Difficulty.Hard, TimeSpan.FromMinutes(5));

        CreateUseCase().Execute(session, GameOutcome.Won);

        var stats = _repository.Current.For(Difficulty.Hard);
        Assert.Equal(1, stats.Won);
        Assert.Equal(TimeSpan.FromMinutes(5), stats.BestTime);
        var record = Assert.Single(_repository.Current.Recent);
        Assert.Equal(GameOutcome.Won, record.Outcome);
        Assert.Equal(_clock.UtcNow, record.FinishedAt);
    }

    [Fact]
    public void Lost_is_recorded_for_an_unfinished_board()
    {
        var session = SessionWith(Board.Empty, Difficulty.Easy, TimeSpan.FromMinutes(2));

        CreateUseCase().Execute(session, GameOutcome.Lost);

        Assert.Equal(1, _repository.Current.For(Difficulty.Easy).Lost);
        Assert.Equal(0, _repository.Current.For(Difficulty.Easy).Won);
    }

    [Fact]
    public void Won_with_an_unsolved_board_throws_and_saves_nothing()
    {
        var session = SessionWith(Board.Empty, Difficulty.Easy, TimeSpan.FromMinutes(1));

        Assert.Throws<InvalidOperationException>(() => CreateUseCase().Execute(session, GameOutcome.Won));
        Assert.Equal(0, _repository.SaveCount);
    }
}
