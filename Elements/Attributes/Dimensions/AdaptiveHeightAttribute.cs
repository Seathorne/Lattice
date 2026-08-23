using Lattice.Measure;
using System;

namespace Lattice.Elements;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property,
    AllowMultiple = false,
    Inherited = true)]
public sealed class AdaptiveHeightAttribute() : AdaptiveDimensionAttribute(Axis.Height)
{
}