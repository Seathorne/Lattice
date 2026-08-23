using System;

namespace Lattice.Measure;

public readonly struct SizeConstraint : IEquatable<SizeConstraint>
{
    public static SizeConstraint Fill { get; } = new(SizeMode.Fill, 0);

    public static SizeConstraint Adaptive { get; } = new(SizeMode.Adaptive, 0);

    private SizeConstraint(SizeMode mode, int fixedSize, int minimum = 0, int maximum = int.MaxValue)
    {
        Mode = mode;
        FixedSize = fixedSize;
        Minimum = minimum;
        Maximum = maximum;
    }

    public SizeMode Mode { get; }

    public int FixedSize { get; }

    public int Minimum { get; }

    public int Maximum { get; }

    public static SizeConstraint Fixed(int size)
        => new(SizeMode.Fixed, size);

    public static SizeConstraint Flexible(int minimum, int maximum)
        => new(SizeMode.Fill, 0, minimum, maximum);

    public static SizeConstraint AdaptiveWithin(int minimum, int maximum)
        => new(SizeMode.Adaptive, 0, minimum, maximum);

    public int Clamp(int candidate)
    {
        int value = Mode == SizeMode.Fixed ? FixedSize : candidate;

        value = Math.Max(value, Minimum);
        value = Math.Min(value, Maximum);

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
        => (((int)Mode * 397 ^ FixedSize) * 397 ^ Minimum) * 397 ^ Maximum;

    public override string ToString()
        => Mode == SizeMode.Fixed
            ? $"Fixed({FixedSize})"
            : $"Fill({Minimum}, {(Maximum == int.MaxValue ? '-' : Maximum)})";
}