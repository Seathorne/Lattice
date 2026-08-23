using System;

namespace Lattice.Elements;

[AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class FixedWidthAttribute(int width) : WidthAttribute
{
    public int Width { get; } = width;
}