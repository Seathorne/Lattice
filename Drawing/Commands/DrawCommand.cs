namespace Lattice.Drawing;

public sealed record DrawCommand : BaseCommand
{
    public required Cell[,] Cells { get; init; }
}