using System;

namespace Lattice.Elements;

[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class FillAreaAttribute(int? minimum = null, int? maximum = null) : Attribute
{
    public int? Minimum { get; } = minimum;
    public int? Maximum { get; } = maximum;
}