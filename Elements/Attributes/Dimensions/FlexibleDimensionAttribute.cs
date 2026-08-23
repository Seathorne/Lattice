using Lattice.Measure;

namespace Lattice.Elements;

public abstract class FlexibleDimensionAttribute(Axis axis, SizeMode mode)
    : DimensionAttribute(axis, mode)
{
    public int Minimum { get; init; } = 0;

    public int Maximum { get; init; } = int.MaxValue;
}