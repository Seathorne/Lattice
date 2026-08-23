using System;

namespace Lattice.Elements;

[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class FillHeightAttribute(int? minimum = null, int? maximum = null) : HeightAttribute
{
    public int? Minimum { get; } = minimum;
    public int? Maximum { get; } = maximum;
}