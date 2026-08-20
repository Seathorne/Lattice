using System;

namespace Lattice.Elements;

[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class FixedAreaAttribute(int area) : Attribute
{
    public int Area { get; } = area;
}