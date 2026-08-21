using System;
using Lattice.Text;

namespace Lattice.Drawing;

public readonly struct Cell(Glyph glyph, ConsoleColor foreground, ConsoleColor? background)
{
    public Glyph Glyph { get; } = glyph;

    public ConsoleColor Foreground { get; } = foreground;

    public ConsoleColor? Background { get; } = background;

    public override string ToString() => $"Cell({Glyph}, {Foreground}, {Background})";
}