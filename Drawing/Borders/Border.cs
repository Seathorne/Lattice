using System;

namespace Lattice.Drawing;

public readonly struct Border : IEquatable<Border>
{
    public static Border LightSolid { get; } = Unicode(BorderWeight.Light, BorderStyle.Solid);

    public static Border LightSparse { get; } = Unicode(BorderWeight.Light, BorderStyle.Sparse);

    public static Border LightMedium { get; } = Unicode(BorderWeight.Light, BorderStyle.Medium);

    public static Border LightDense { get; } = Unicode(BorderWeight.Light, BorderStyle.Dense);

    public static Border HeavySolid { get; } = Unicode(BorderWeight.Heavy, BorderStyle.Solid);

    public static Border HeavySparse { get; } = Unicode(BorderWeight.Heavy, BorderStyle.Sparse);

    public static Border HeavyMedium { get; } = Unicode(BorderWeight.Heavy, BorderStyle.Medium);

    public static Border HeavyDense { get; } = Unicode(BorderWeight.Heavy, BorderStyle.Dense);

    public static Border Double { get; } = Unicode(BorderWeight.Double, BorderStyle.Solid);

    public static Border Wide { get; } = new(BorderWeight.Light, BorderStyle.Solid, BorderMode.Wide);

    public static Border Simple { get; } = new(BorderWeight.Light, BorderStyle.Solid, BorderMode.Simple);

    private Border(BorderWeight weight, BorderStyle style, BorderMode mode)
    {
        Weight = weight;
        Style = style;
        Mode = mode;
    }

    public BorderWeight Weight { get; }

    public BorderStyle Style { get; }

    public BorderMode Mode { get; }

    public static Border Unicode(BorderWeight weight, BorderStyle style)
        => new(weight, style, BorderMode.Narrow);

    public static bool operator ==(Border left, Border right)
        => left.Equals(right);

    public static bool operator !=(Border left, Border right)
        => !left.Equals(right);

    public bool Equals(Border other)
        => Weight == other.Weight
        && Style == other.Style
        && Mode == other.Mode;

    public override bool Equals(object? obj)
        => obj is Border other && Equals(other);

    public override int GetHashCode()
        => ((int)Weight * 397 ^ (int)Style) * 397 ^ (int)Mode;

    public override string ToString()
        => Mode == BorderMode.Narrow
            ? $"Border({Mode}, {Weight}, {Style})"
            : $"Border({Mode})";
}