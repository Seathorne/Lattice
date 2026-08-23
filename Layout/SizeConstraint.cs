using System;

namespace Lattice.Layout;

public readonly struct SizeConstraint : IEquatable<SizeConstraint>
{
    public static SizeConstraint Fill { get; } = new(SizeMode.Fill, 0, null, null);

    private SizeConstraint(SizeMode mode, int fixedSize, int? minimum, int? maximum)
    {
        Mode = mode;
        FixedSize = fixedSize;
        Minimum = minimum;
        Maximum = maximum;
    }

    public SizeMode Mode { get; }

    public int FixedSize { get; }

    public int? Minimum { get; }

    public int? Maximum { get; }

    public static SizeConstraint Fixed(int size)
        => new(SizeMode.Fixed, size, null, null);

    public static SizeConstraint Flexible(int? minimum, int? maximum)
        => new(SizeMode.Fill, 0, minimum, maximum);

    public int Clamp(int available)
    {
        int value = Mode == SizeMode.Fixed ? FixedSize : available;

        if (Minimum.HasValue)
            value = Math.Max(value, Minimum.Value);

        if (Maximum.HasValue)
            value = Math.Max(value, Maximum.Value);

        return Math.Max(0, value);
    }

    public bool Equals(SizeConstraint other)
        => Mode == other.Mode
        && FixedSize == other.FixedSize
        && Minimum == other.Minimum
        && Maximum == other.Maximum;

    public override bool Equals(object? obj)
        => obj is SizeConstraint other && Equals(other);

    public override int GetHashCode()
        => (((int)Mode * 397 ^ FixedSize) * 397 ^ (Minimum ?? -1)) * 397 ^ (Maximum ?? -1);

    public override string ToString()
        => Mode == SizeMode.Fixed
            ? $"Fixed({FixedSize})"
            : $"Fill({Minimum?.ToString() ?? "-"}, {Maximum?.ToString() ?? "-"})";
}