using System;

namespace Lattice.Elements;

[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class FillWidthAttribute(int? minimum = null, int? maximum = null) : WidthAttribute
{
    public int? Minimum { get; } = minimum;
    public int? Maximum { get; } = maximum;
}