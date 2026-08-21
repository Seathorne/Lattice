namespace Lattice.Drawing;

public sealed record TextCommand : ColorCommand
{
    public required string Text { get; init; }
}