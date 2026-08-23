using System;

namespace Lattice.Elements;

public sealed class GridElement : ContainerElement
{
    public int Columns { get; set; } = 1;

    public int Rows { get; set; } = 1;

    public static GridElement Row(int columns)
        => new() { Columns = Math.Max(1, columns), Rows = 1 };

    public static GridElement Column(int rows)
        => new() { Columns = 1, Rows = Math.Max(1, rows) };

    public static GridElement Of(int columns, int rows)
        => new() { Columns = Math.Max(1, columns), Rows = Math.Max(1, rows) };
}