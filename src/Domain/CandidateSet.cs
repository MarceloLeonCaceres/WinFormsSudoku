using System.Collections;
using System.Numerics;

namespace Domain;

/// <summary>Conjunto inmutable de candidatos (1-9) representado con un bitmask.</summary>
public readonly struct CandidateSet : IEquatable<CandidateSet>, IEnumerable<int>
{
    private const ushort AllBits = 0b1_1111_1111;

    private readonly ushort _bits;

    private CandidateSet(ushort bits) => _bits = bits;

    public static CandidateSet Empty => default;

    public static CandidateSet All => new(AllBits);

    public int Count => BitOperations.PopCount(_bits);

    public bool IsEmpty => _bits == 0;

    public static CandidateSet Of(params int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var set = Empty;
        foreach (var value in values)
        {
            set = set.Add(value);
        }

        return set;
    }

    public bool Contains(int value)
    {
        ValueGuard.EnsureValid(value, nameof(value));
        return (_bits & Mask(value)) != 0;
    }

    public CandidateSet Add(int value)
    {
        ValueGuard.EnsureValid(value, nameof(value));
        return new CandidateSet((ushort)(_bits | Mask(value)));
    }

    public CandidateSet Remove(int value)
    {
        ValueGuard.EnsureValid(value, nameof(value));
        return new CandidateSet((ushort)(_bits & ~Mask(value)));
    }

    public CandidateSet Toggle(int value)
    {
        ValueGuard.EnsureValid(value, nameof(value));
        return new CandidateSet((ushort)(_bits ^ Mask(value)));
    }
    public CandidateSet Union(CandidateSet other) => new((ushort)(_bits | other._bits));

    public CandidateSet Except(CandidateSet other) => new((ushort)(_bits & ~other._bits));
    public IEnumerator<int> GetEnumerator()
    {
        var bits = _bits;
        for (var value = SudokuConstants.MinValue; value <= SudokuConstants.MaxValue; value++)
        {
            if ((bits & Mask(value)) != 0)
            {
                yield return value;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(CandidateSet other) => _bits == other._bits;

    public override bool Equals(object? obj) => obj is CandidateSet other && Equals(other);

    public override int GetHashCode() => _bits;

    public override string ToString() => "{" + string.Join(", ", this) + "}";

    public static bool operator ==(CandidateSet left, CandidateSet right) => left.Equals(right);

    public static bool operator !=(CandidateSet left, CandidateSet right) => !left.Equals(right);

    private static int Mask(int value) => 1 << (value - 1);
}
