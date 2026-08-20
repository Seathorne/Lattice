using System;

namespace Lattice.Elements;

[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class FillHeightAttribute(int? min = null, int? max = null) : Attribute
{
    public int? Min { get; } = min;
    public int? Max { get; } = max;
}