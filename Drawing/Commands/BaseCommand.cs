using Lattice.Geometry;

namespace Lattice.Drawing;

// A single drawing instruction produced by an element and executed by the
//  renderer.
public abstract record BaseCommand
{
    public required Rectangle Extent { get; init; }
}