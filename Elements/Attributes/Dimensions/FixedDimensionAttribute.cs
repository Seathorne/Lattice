using Lattice.Measure;

namespace Lattice.Elements;

public abstract class FixedDimensionAttribute(Axis axis, int value)
    : DimensionAttribute(axis, SizeMode.Fixed)
{
    public int Value { get; } = value;
}