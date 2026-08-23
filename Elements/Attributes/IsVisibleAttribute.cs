using System;

namespace Lattice.Elements;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property,
    AllowMultiple = false,
    Inherited = true)]
public sealed class IsVisibleAttribute(bool isVisible = true) : Attribute
{
    public bool IsVisible { get; } = isVisible;
}