using Lattice.Measure;

namespace Lattice.Elements;

public abstract class AdaptiveDimensionAttribute(Axis axis)
    : FlexibleDimensionAttribute(axis, SizeMode.Adaptive)
{
}