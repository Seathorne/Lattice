using System;

namespace Lattice.Elements;

[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class FixedHeightAttribute(int height) : Attribute
{
    public int Height { get; } = height;
}