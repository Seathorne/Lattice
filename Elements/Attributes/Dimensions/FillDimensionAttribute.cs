using Lattice.Measure;

namespace Lattice.Elements;

public abstract class FillDimensionAttribute(Axis axis)
    : FlexibleDimensionAttribute(axis, SizeMode.Fill)
{
}