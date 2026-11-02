namespace AnarchyChess.Core;

/// <summary>
/// The game's only source of randomness: SplitMix64, seeded from the seed shown on screen.
/// The algorithm is pinned by golden tests; changing it breaks every old seed (docs/decisions.md D9).
/// </summary>
public sealed class Rng
{
    private ulong _state;

    public Rng(long seed) => _state = unchecked((ulong)seed);

    public ulong NextULong()
    {
        unchecked
        {
            _state += 0x9E3779B97F4A7C15UL;
            var z = _state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    /// <summary>A number from 0 up to, but not including, <paramref name="maxExclusive"/>.</summary>
    public int NextInt(int maxExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxExclusive);
        // Modulo bias is at most maxExclusive / 2^64: negligible for game-sized ranges.
        return (int)(NextULong() % (ulong)maxExclusive);
    }
}
