using Lattice.Drawing;

public sealed record BorderCommand : ColorCommand
{
    public required BorderStyle BorderStyle { get; init; }

    public required BorderWeight BorderWeight { get; init; }
}