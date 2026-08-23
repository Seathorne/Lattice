using System;
using Lattice.Measure;

namespace Lattice.Elements;

public abstract class DimensionAttribute(Axis axis, SizeMode mode) : Attribute
{
    public Axis Axis { get; } = axis;

    public SizeMode Mode { get; } = mode;
}