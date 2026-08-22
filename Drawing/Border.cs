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

    public static Border Wide { get; } = new(BorderWeight.Light, BorderStyle.Solid, BorderScale.Wide, false);

    public static Border Ascii { get; } = new(BorderWeight.Light, BorderStyle.Solid, BorderScale.Narrow, true);

    private Border(BorderWeight weight, BorderStyle style, BorderScale scale, bool isAsciiOnly)
    {
        Weight = weight;
        Style = style;
        Scale = scale;
        IsAsciiOnly = isAsciiOnly;
    }

    public BorderWeight Weight { get; }

    public BorderStyle Style { get; }

    public BorderScale Scale { get; }

    public bool IsAsciiOnly { get; }

    public static Border Unicode(BorderWeight weight, BorderStyle style)
        => new(weight, style, BorderScale.Narrow, false);

    public static bool operator ==(Border left, Border right)
        => left.Equals(right);

    public static bool operator !=(Border left, Border right)
        => !left.Equals(right);

    public bool Equals(Border other)
        => Weight == other.Weight
        && Style == other.Style
        && Scale == other.Scale
        && IsAsciiOnly == other.IsAsciiOnly;

    public override bool Equals(object? obj)
        => obj is Border other && Equals(other);

    public override int GetHashCode()
        => (((int)Weight * 397 ^ (int)Style) * 397 ^ (int)Scale) * 397 ^ (IsAsciiOnly ? 1 : 0);

    public override string ToString()
        => IsAsciiOnly
            ? "Border(Ascii)"
            : $"Border({Scale}, {Weight}, {Style})";
}