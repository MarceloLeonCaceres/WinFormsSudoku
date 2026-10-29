using Domain;

namespace Infrastructure.Tests.Persistence;

/// <summary>Mismo puzzle de ejemplo que en Application.Tests. (0,2) y (0,3) están vacías.</summary>
internal static class SamplePuzzle
{
    public const string Givens = "530070000600195000098000060800060003400803001700020006060000280000419005000080079";

    public static Board Board() => Domain.Board.FromGivens(Givens);
}
