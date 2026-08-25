using System;
using Lattice.Drawing;

namespace Lattice.Elements;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property,
    AllowMultiple = false,
    Inherited = true)]
public sealed class BorderAttribute : Attribute
{
    public BorderAttribute()
        => Border = Border.LightSolid;

    public BorderAttribute(BorderWeight weight, BorderStyle style)
        => Border = Border.Unicode(weight, style);

    public BorderAttribute(BorderMode mode)
        => Border = mode switch
        {
            BorderMode.Wide => Border.Wide,
            BorderMode.Simple => Border.Simple,
            _ => Border.LightSolid,
        };

    public Border Border { get; }
}