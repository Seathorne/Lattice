using Lattice.Measure;
using System;

namespace Lattice.Elements;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property,
    AllowMultiple = false,
    Inherited = true)]
public sealed class FixedHeightAttribute(int value) : FixedDimensionAttribute(Axis.Height, value)
{
}