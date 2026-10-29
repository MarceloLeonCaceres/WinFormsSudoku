using Domain;

namespace Domain.Tests;

public sealed class CandidateSetSetOperationsTests
{
    [Fact]
    public void Union_CombinesValuesOfBothSets()
    {
        var result = CandidateSet.Of(1, 2).Union(CandidateSet.Of(2, 3));

        Assert.Equal(CandidateSet.Of(1, 2, 3), result);
    }

    [Fact]
    public void Union_WithEmptySet_ReturnsSameSet()
    {
        var set = CandidateSet.Of(4, 9);

        Assert.Equal(set, set.Union(CandidateSet.Empty));
    }

    [Fact]
    public void Except_RemovesValuesPresentInOtherSet()
    {
        var result = CandidateSet.Of(1, 2, 3).Except(CandidateSet.Of(2, 3, 4));

        Assert.Equal(CandidateSet.Of(1), result);
    }

    [Fact]
    public void Except_FromAll_ReturnsComplement()
    {
        var result = CandidateSet.All.Except(CandidateSet.Of(1, 5, 9));

        Assert.Equal(6, result.ToList().Count);
        Assert.False(result.Contains(5));
    }

    [Fact]
    public void Except_WithEmptySet_ReturnsSameSet()
    {
        var set = CandidateSet.Of(2, 7);

        Assert.Equal(set, set.Except(CandidateSet.Empty));
    }

    [Fact]
    public void Except_WithItself_ReturnsEmptySet()
    {
        var set = CandidateSet.Of(2, 7);

        Assert.True(set.Except(set).IsEmpty);
    }
}
