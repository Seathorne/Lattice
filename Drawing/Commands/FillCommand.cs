using Lattice.Text;

namespace Lattice.Drawing;

public record FillCommand : ColorCommand
{
    public Glyph Glyph { get; init; } = ' ';
}