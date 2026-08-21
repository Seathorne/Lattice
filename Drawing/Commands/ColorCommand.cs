using System;

namespace Lattice.Drawing;

public abstract record ColorCommand : BaseCommand
{
    public ConsoleColor Foreground { get; init; } = ConsoleColor.Gray;

    public ConsoleColor? Background { get; init; }
}