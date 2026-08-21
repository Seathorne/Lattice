namespace Lattice.Geometry;

public readonly struct DrawSurface(Rectangle contentBounds, Rectangle focusBounds, Rectangle inputBounds)
{
    public DrawSurface(Rectangle contentBounds) : this(contentBounds, contentBounds, contentBounds) { }

    public Rectangle ContentBounds { get; } = contentBounds;

    public Rectangle FocusBounds { get; } = focusBounds;

    public Rectangle InputBounds { get; } = inputBounds;

    public override string ToString() => $"DrawSurface({ContentBounds}, {FocusBounds}, {InputBounds})";
}