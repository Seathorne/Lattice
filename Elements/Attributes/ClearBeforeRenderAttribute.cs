using System;

namespace Lattice.Elements.Attributes;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property,
    AllowMultiple = false,
    Inherited = true)]
public sealed class ClearBeforeRenderAttribute(bool clearBeforeRender = true) : Attribute
{
    public bool ClearBeforeRender { get; } = clearBeforeRender;
}