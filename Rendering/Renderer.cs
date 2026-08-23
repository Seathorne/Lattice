using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Lattice.Console;
using Lattice.Drawing;
using Lattice.Text;

namespace Lattice.Rendering;

public sealed class Renderer(ConsoleWriter writer, WidthTable widths, HostType hostType)
{
    private readonly ConsoleWriter _writer = writer;

    private readonly WidthTable _widths = widths;

    private readonly HostType _hostType = hostType;

    public void Clear()
        => _writer.Clear();

    public void Render(IEnumerable<DrawSurface> surfaces)
    {
        foreach (DrawSurface surface in surfaces)
            Render(surface);
    }

    public void Render(DrawSurface surface)
    {
        foreach (BaseCommand command in surface.Commands)
            Execute(command);
    }

    private void Execute(BaseCommand command)
    {
        if (command.Extent.Width <= 0 || command.Extent.Height <= 0)
            return;

        switch (command)
        {
            case ClearCommand clear:
                Blank(clear.Extent);
                break;

            case FillCommand fill:
                ExecuteFill(fill);
                break;

            case TextCommand text:
                ExecuteText(text);
                break;

            case BorderCommand border:
                ExecuteBorder(border);
                break;

            case DrawCommand draw:
                ExecuteDraw(draw);
                break;

            default:
                Trace.TraceWarning($"Unhandled command type {command.GetType().Name}; dropped.");
                break;
        }
    }

    private void ExecuteFill(FillCommand fill)
    {
        Rectangle extent = fill.Extent;
        Glyph glyph = _widths.Resolve(fill.Glyph);
        int glyphWidth = Math.Max(1, _widths.Measure(glyph));
        int count = extent.Width / glyphWidth;

        if (count <= 0)
            return;

        string row = Repeat(glyph, count, extent.Width - count * glyphWidth);

        for (int y = extent.Top; y <= extent.Bottom; y++)
            _writer.Write(extent.Left, y, row, fill.Foreground, fill.Background);
    }

    private void ExecuteText(TextCommand text)
    {
        Rectangle extent = text.Extent;
        string resolved = _widths.Resolve(text.Text);
        string fitted = _widths.Truncate(resolved, extent.Width, out bool wasTruncated);

        if (wasTruncated)
            Trace.TraceWarning($"Text '{text.Text}' exceeds {extent.Width} cells at {extent}; truncated.");

        if (fitted.Length == 0)
            return;

        _writer.Write(extent.Left, extent.Top, fitted, text.Foreground, text.Background);
    }

    private void ExecuteBorder(BorderCommand border)
    {
        Rectangle extent = border.Extent;

        Border effective = _hostType == HostType.Conhost
            ? Border.Simple
            : border.Border;

        BorderGlyphSet glyphs = BorderGlyphs.GetBorderGlyphs(effective);

        int cornerWidth = Math.Max(1, _widths.Measure(glyphs.TopLeft));
        int spanWidth = Math.Max(1, _widths.Measure(glyphs.Horizontal));
        int sideWidth = Math.Max(1, _widths.Measure(glyphs.Vertical));

        if (extent.Width < cornerWidth * 2 || extent.Height < 2)
        {
            Trace.TraceWarning($"Extent {extent} is too small for {border.Border}; dropped.");
            return;
        }

        int span = extent.Width - cornerWidth * 2;
        int count = span / spanWidth;
        string horizontal = Repeat(glyphs.Horizontal, count, span - count * spanWidth);

        _writer.Write(
            extent.Left, extent.Top,
            glyphs.TopLeft.Value + horizontal + glyphs.TopRight.Value,
            border.Foreground, border.Background);

        int rightColumn = extent.Right - sideWidth + 1;

        for (int y = extent.Top + 1; y < extent.Bottom; y++)
        {
            _writer.Write(extent.Left, y, glyphs.Vertical.Value, border.Foreground, border.Background);
            _writer.Write(rightColumn, y, glyphs.Vertical.Value, border.Foreground, border.Background);
        }

        _writer.Write(
            extent.Left, extent.Bottom,
            glyphs.BottomLeft.Value + horizontal + glyphs.BottomRight.Value,
            border.Foreground, border.Background);
    }

    private void ExecuteDraw(DrawCommand draw)
    {
        Rectangle extent = draw.Extent;
        int columns = draw.Cells.GetLength(0);
        int rows = draw.Cells.GetLength(1);

        if (columns != extent.Width || rows != extent.Height)
        {
            Trace.TraceWarning($"Cell grid {columns}x{rows} does not match extent {extent}; dropped.");
            return;
        }

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                DrawCommand.Cell cell = draw.Cells[x, y];
                Glyph glyph = _widths.Resolve(cell.Glyph);

                _writer.Write(
                    extent.Left + x, extent.Top + y,
                    glyph.Value, cell.Foreground, cell.Background);
            }
        }
    }

    private void Blank(Rectangle extent)
        => _writer.ClearRegion(extent.Left, extent.Top, extent.Width, extent.Height);

    private static string Repeat(Glyph glyph, int count, int padding)
    {
        StringBuilder builder = new(glyph.Value.Length * count + padding);

        for (int i = 0; i < count; i++)
            builder.Append(glyph.Value);

        if (padding > 0)
            builder.Append(' ', padding);

        return builder.ToString();
    }
}