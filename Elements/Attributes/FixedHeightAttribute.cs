using System;

namespace Lattice.Elements;

[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class FixedHeightAttribute(int height) : HeightAttribute
{
    public int Height { get; } = height;
}