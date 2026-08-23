using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lattice.Text;

namespace Lattice.Drawing;

public sealed class DrawSurface(Rectangle extent)
{
    private readonly List<BaseCommand> _commands = [];

    public Rectangle Extent { get; } = extent;

    public int Width => Extent.Width;

    public int Height => Extent.Height;

    public IReadOnlyList<BaseCommand> Commands => _commands;

    public DrawSurface Text(int x, int y, int width, string text)
        => Text(x, y, width, text, ConsoleColor.Gray, null);

    public DrawSurface Text(int x, int y, int width, string text, ConsoleColor foreground, ConsoleColor? background)
    {
        if (string.IsNullOrEmpty(text))
            return this;

        if (!TryClampArea(x, y, width, 1, out Rectangle local, nameof(Text)))
            return this;

        _commands.Add(new TextCommand
        {
            Extent = Absolute(local),
            Text = text,
            Foreground = foreground,
            Background = background,
        });

        return this;
    }

    public DrawSurface Fill(int x, int y, int width, int height, Glyph glyph)
        => Fill(x, y, width, height, glyph, ConsoleColor.Gray, null);

    public DrawSurface Fill(int x, int y, int width, int height, Glyph glyph, ConsoleColor foreground, ConsoleColor? background)
    {
        if (!TryClampArea(x, y, width, height, out Rectangle local, nameof(Fill)))
            return this;

        _commands.Add(new FillCommand
        {
            Extent = Absolute(local),
            Glyph = glyph,
            Foreground = foreground,
            Background = background,
        });

        return this;
    }

    public DrawSurface Frame(Border border)
        => Frame(0, 0, Width, Height, border, ConsoleColor.Gray, null);

    public DrawSurface Frame(Border border, ConsoleColor foreground, ConsoleColor? background)
        => Frame(0, 0, Width, Height, border, foreground, background);

    public DrawSurface Frame(int x, int y, int width, int height, Border border)
        => Frame(x, y, width, height, border, ConsoleColor.Gray, null);

    public DrawSurface Frame(int x, int y, int width, int height, Border border, ConsoleColor foreground, ConsoleColor? background)
    {
        if (!TryClampArea(x, y, width, height, out Rectangle local, nameof(Frame)))
            return this;

        if (local.Width < 2 || local.Height < 2)
        {
            Trace.TraceWarning($"Frame area {local} is too small to draw; dropped.");
            return this;
        }

        _commands.Add(new BorderCommand
        {
            Extent = Absolute(local),
            Border = border,
            Foreground = foreground,
            Background = background,
        });

        return this;
    }

    public DrawSurface Clear()
        => Clear(0, 0, Width, Height);

    public DrawSurface Clear(int x, int y, int width, int height)
    {
        if (!TryClampArea(x, y, width, height, out Rectangle local, nameof(Clear)))
            return this;

        _commands.Add(new ClearCommand { Extent = Absolute(local) });

        return this;
    }

    public DrawSurface Draw(int x, int y, DrawCommand.Cell[,] cells)
    {
        int columns = cells.GetLength(0);
        int rows = cells.GetLength(1);

        if (!TryClampArea(x, y, columns, rows, out Rectangle local, nameof(Draw)))
            return this;

        int offsetX = local.X - Math.Min(x, local.X);
        int offsetY = local.Y - Math.Min(y, local.Y);

        _commands.Add(new DrawCommand
        {
            Extent = Absolute(local),
            Cells = local.Width == columns && local.Height == rows
                ? cells
                : Slice(cells, offsetX, offsetY, local.Width, local.Height),
        });

        return this;
    }

    private bool TryClampOrigin(int x, int y, out int localX, out int localY, string caller)
    {
        localX = Math.Max(0, x);
        localY = Math.Max(0, y);

        if (localX != x || localY != y)
            Trace.TraceWarning($"{caller} origin ({x}, {y}) is outside the surface; clamped to ({localX}, {localY}).");

        if (localX >= Width || localY >= Height)
        {
            Trace.TraceWarning($"{caller} origin ({x}, {y}) is past the surface bounds {Extent}; dropped.");
            return false;
        }

        return true;
    }

    private bool TryClampArea(int x, int y, int width, int height, out Rectangle local, string caller)
    {
        local = default;

        if (width <= 0 || height <= 0)
            return false;

        if (!TryClampOrigin(x, y, out int localX, out int localY, caller))
            return false;

        int clampedWidth = Math.Min(width - (localX - x), Width - localX);
        int clampedHeight = Math.Min(height - (localY - y), Height - localY);

        if (clampedWidth <= 0 || clampedHeight <= 0)
            return false;

        if (clampedWidth != width || clampedHeight != height)
        {
            Trace.TraceWarning(
                $"{caller} area {width}x{height} at ({x}, {y}) exceeds the surface {Extent}; "
                + $"clamped to {clampedWidth}x{clampedHeight}.");
        }

        local = new Rectangle(localX, localY, clampedWidth, clampedHeight);

        return true;
    }

    private Rectangle Absolute(Rectangle local)
        => Absolute(local.X, local.Y, local.Width, local.Height);

    private Rectangle Absolute(int localX, int localY, int width, int height)
        => new(Extent.Left + localX, Extent.Top + localY, width, height);

    private static DrawCommand.Cell[,] Slice(DrawCommand.Cell[,] cells, int offsetX, int offsetY, int width, int height)
    {
        DrawCommand.Cell[,] sliced = new DrawCommand.Cell[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                sliced[x, y] = cells[offsetX + x, offsetY + y];
        }

        return sliced;
    }
}