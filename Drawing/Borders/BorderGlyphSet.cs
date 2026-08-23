using Lattice.Text;

namespace Lattice.Drawing;

public readonly struct BorderGlyphSet(Glyph topLeft, Glyph topRight, Glyph bottomLeft, Glyph bottomRight, Glyph horizontal, Glyph vertical)
{
    public Glyph TopLeft { get; } = topLeft;

    public Glyph TopRight { get; } = topRight;

    public Glyph BottomLeft { get; } = bottomLeft;

    public Glyph BottomRight { get; } = bottomRight;

    public Glyph Horizontal { get; } = horizontal;

    public Glyph Vertical { get; } = vertical;
}