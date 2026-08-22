namespace Lattice.Drawing;

public sealed record BorderCommand : ColorCommand
{
    public required Border Border { get; init; }
}