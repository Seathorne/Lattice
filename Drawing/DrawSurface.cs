using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Lattice.Text;

namespace Lattice.Drawing;

public sealed class DrawSurface(Rectangle extent, Rectangle content) : IDirtyable
{
    private readonly List<BaseCommand> _commands = [];

    public DrawSurface(Rectangle extent)
        : this(extent, extent)
    {
    }

    public Rectangle Extent { get; } = extent;

    public Rectangle Content { get; } = content;

    public int Width => Content.Width;

    public int Height => Content.Height;

    public bool IsDirtyable { get; set; } = true;

    public bool IsDirty { get; private set; } = true;  // Initialize as true to render on first pass

    public IReadOnlyList<BaseCommand> Commands => _commands;

    public void Invalidate()
    {
        if (!IsDirtyable)
            return;

        IsDirty = true;
    }

    public void ClearDirty()
        => IsDirty = false;

    public void Reset()
        => _commands.Clear();

    public DrawSurface Frame(Border border)
        => Frame(border, ConsoleColor.Gray, null);

    public DrawSurface Frame(Border border, ConsoleColor foreground, ConsoleColor? background)
    {
        if (Extent.Width < 2 || Extent.Height < 2)
        {
            Trace.TraceWarning($"Frame extent {Extent} is too small to draw; dropped.");
            return this;
        }

        _commands.Add(new BorderCommand
        {
            Extent = Extent,
            Border = border,
            Foreground = foreground,
            Background = background,
        });

        return this;
    }

    public DrawSurface Text(int x, int y, string text)
        => Text(x, y, text, ConsoleColor.Gray, null);

    public DrawSurface Text(int x, int y, string text, ConsoleColor foreground, ConsoleColor? background)
    {
        if (string.IsNullOrEmpty(text))
            return this;

        if (!TryClampOrigin(x, y, out int localX, out int localY))
            return this;

        _commands.Add(new TextCommand
        {
            Extent = Absolute(localX, localY, Width - localX, 1),
            Text = text,
            Foreground = foreground,
            Background = background,
        });

        return this;
    }

    public DrawSurface Text(int x, int y, int width, string text)
        => Text(x, y, width, text, ConsoleColor.Gray, null);

    public DrawSurface Text(int x, int y, int width, string text, ConsoleColor foreground, ConsoleColor? background)
    {
        if (string.IsNullOrEmpty(text))
            return this;

        if (!TryClampArea(x, y, width, 1, out Rectangle local))
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
        if (!TryClampArea(x, y, width, height, out Rectangle local))
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

    public DrawSurface Clear()
        => Clear(0, 0, Width, Height);

    public DrawSurface Clear(int x, int y, int width, int height)
    {
        if (!TryClampArea(x, y, width, height, out Rectangle local))
            return this;

        _commands.Add(new ClearCommand { Extent = Absolute(local) });

        return this;
    }

    public DrawSurface Draw(int x, int y, DrawCommand.Cell[,] cells)
    {
        int columns = cells.GetLength(0);
        int rows = cells.GetLength(1);

        if (!TryClampArea(x, y, columns, rows, out Rectangle local))
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

    private bool TryClampOrigin(int x, int y, out int localX, out int localY, [CallerMemberName] string caller = "")
    {
        localX = Math.Max(0, x);
        localY = Math.Max(0, y);

        if (localX != x || localY != y)
            Trace.TraceWarning($"{caller} origin ({x}, {y}) is outside the surface; clamped to ({localX}, {localY}).");

        if (localX >= Width || localY >= Height)
        {
            Trace.TraceWarning($"{caller} origin ({x}, {y}) is past the surface content {Content}; dropped.");
            return false;
        }

        return true;
    }

    private bool TryClampArea(int x, int y, int width, int height, out Rectangle local, [CallerMemberName] string caller = "")
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
                $"{caller} area {width}x{height} at ({x}, {y}) exceeds the surface content {Content}; "
                + $"clamped to {clampedWidth}x{clampedHeight}.");
        }

        local = new Rectangle(localX, localY, clampedWidth, clampedHeight);

        return true;
    }

    private Rectangle Absolute(Rectangle local)
        => Absolute(local.X, local.Y, local.Width, local.Height);

    private Rectangle Absolute(int localX, int localY, int width, int height)
        => new(Content.Left + localX, Content.Top + localY, width, height);

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