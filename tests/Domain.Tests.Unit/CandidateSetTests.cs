namespace Domain.Tests.Unit;

public class CandidateSetTests
{
    [Fact]
    public void Empty_HasNoCandidates()
    {
        Assert.True(CandidateSet.Empty.IsEmpty);
        Assert.Equal(0, CandidateSet.Empty.Count);
        Assert.True(default(CandidateSet).IsEmpty);
    }

    [Fact]
    public void All_ContainsOneThroughNine()
    {
        Assert.Equal(9, CandidateSet.All.Count);
        Assert.Equal(Enumerable.Range(1, 9), CandidateSet.All);
    }

    [Fact]
    public void Add_IncludesValue_AndDoesNotMutateOriginal()
    {
        var original = CandidateSet.Empty;

        var updated = original.Add(5);

        Assert.True(updated.Contains(5));
        Assert.True(original.IsEmpty);
    }

    [Fact]
    public void Add_IsIdempotent()
    {
        Assert.Equal(CandidateSet.Of(3), CandidateSet.Of(3).Add(3));
    }

    [Fact]
    public void Remove_ExcludesValue()
    {
        var set = CandidateSet.Of(1, 2, 3).Remove(2);

        Assert.False(set.Contains(2));
        Assert.Equal(2, set.Count);
    }

    [Fact]
    public void Remove_AbsentValue_IsNoOp()
    {
        Assert.Equal(CandidateSet.Of(1), CandidateSet.Of(1).Remove(9));
    }

    [Fact]
    public void Toggle_Twice_ReturnsOriginal()
    {
        var original = CandidateSet.Of(4, 7);

        Assert.Equal(original, original.Toggle(1).Toggle(1));
        Assert.Equal(original, original.Toggle(4).Toggle(4));
    }

    [Fact]
    public void Enumeration_IsAscending()
    {
        Assert.Equal(new[] { 1, 5, 9 }, CandidateSet.Of(9, 1, 5));
    }

    [Fact]
    public void Equality_IsByContent()
    {
        Assert.Equal(CandidateSet.Of(1, 2), CandidateSet.Of(2, 1));
        Assert.True(CandidateSet.Of(1, 2) == CandidateSet.Of(2, 1));
        Assert.True(CandidateSet.Of(1) != CandidateSet.Of(2));
        Assert.Equal(CandidateSet.Of(1, 2).GetHashCode(), CandidateSet.Of(2, 1).GetHashCode());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(-3)]
    public void InvalidValue_Throws(int value)
    {
        var set = CandidateSet.Empty;

        Assert.Throws<ArgumentOutOfRangeException>(() => set.Contains(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => set.Add(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => set.Remove(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => set.Toggle(value));
    }

    [Fact]
    public void ToString_ListsCandidates()
    {
        Assert.Equal("{1, 5, 9}", CandidateSet.Of(9, 1, 5).ToString());
        Assert.Equal("{}", CandidateSet.Empty.ToString());
    }
}