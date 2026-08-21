using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Lattice.Text;

public sealed class WidthTable
{
    // Returned when a glyph could not be classified.
    //  Callers draw Glyph.Replacement in its place.
    public const int Unclassified = 0;

    // Far enough from column 0 that a displacement read is unambiguous.
    private const int ScratchColumn = 4;

    // Reprobed periodically to detect a font family change.
    //   U+00B6 PILCROW SIGN (narrow, Latin-1)
    //   U+2560 BOX DRAWINGS DOUBLE VERTICAL AND RIGHT (structural)
    //   U+3007 IDEOGRAPHIC NUMBER ZERO (East Asian Wide)
    private static readonly int[] Canaries = [0x00B6, 0x2560, 0x3007];

    private readonly Dictionary<int, int> _codepointWidths = [];

    private readonly Dictionary<string, int> _glyphWidths = [];

    private readonly Dictionary<int, int> _canaryWidths = [];

    private bool _isEnabled;

    // Raised when a canary check finds a changed width, meaning the tables
    //  were dumped and every cached layout is suspect. Subscribers should force
    //  a full re-render.
    public event Action? Invalidated;

    // isEnabled false makes every measurement return 1, which
    //  is correct for an ASCII-only host.
    public void Initialize(bool isEnabled)
    {
        _isEnabled = isEnabled;
    }

    // Runs once at startup, before the first render.
    public void ProbeRanges(IEnumerable<CodepointRange> ranges)
    {
        if (!_isEnabled)
            return;

        foreach (CodepointRange range in ranges)
        {
            for (int codepoint = range.Start; codepoint <= range.End; codepoint++)
                _codepointWidths[codepoint] = ProbeCodepoint(codepoint);
        }

        CaptureCanaries();
    }

    public int Measure(Glyph glyph)
    {
        // 0. Zero width. Will not display.
        if (glyph.IsZeroWidth)
            return 0;

        // 1. ASCII fast path. No lookup, no probe.
        if (glyph.IsAscii)
            return 1;

        string value = glyph.Value;

        // 2. A cluster probed earlier this session.
        if (_glyphWidths.TryGetValue(value, out int cached))
            return cached;

        // 3. A single codepoint from the startup table.
        if (glyph.TryGetSingleCodepoint(out int codepoint)
            && _codepointWidths.TryGetValue(codepoint, out int width))
        {
            return width;
        }

        // 4. Unseen. Probe now and cache by string.
        int probed = ProbeString(value);
        _glyphWidths[value] = probed;

        return probed;
    }

    // An unclassified glyph counts as the width of the replacement that will
    //  be drawn for it it, so layout math stays correct.
    public int Measure(IEnumerable<Glyph> glyphs)
    {
        int total = 0;

        foreach (Glyph glyph in glyphs)
        {
            if (glyph.IsZeroWidth)
                continue;

            int width = Measure(glyph);
            total += width == Unclassified ? 1 : width;
        }

        return total;
    }

    public int Measure(string text)
        => Measure(Glyph.Split(text));

    // The glyph that should actually be drawn: itself when classified, a
    //  replacement when not.
    public Glyph Resolve(Glyph glyph)
    {
        if (glyph.IsZeroWidth)
            return glyph;

        int width = Measure(glyph);

        return width == Unclassified
            ? Glyph.Replacement(width)
            : glyph;
    }

    // Substitutes a replacement for any glyph the probe could not classify, so
    //  the string that reaches the console has a known cell width.
    public string Resolve(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        
        List<Glyph> glyphs = Glyph.Split(text);
        StringBuilder builder = new(text.Length);
        bool substituted = false;

        foreach (Glyph glyph in glyphs)
        {
            if (glyph.IsZeroWidth)
                continue;

            Glyph resolved = Resolve(glyph);

            if (resolved != glyph)
                substituted = true;
            
            builder.Append(resolved.Value);
        }

        if (substituted)
            Trace.TraceWarning($"Unclassified glyphs in '{text}' were replaced.");
        
        return builder.ToString();
    }

    // Cuts a string to fit maxWidth cells, measuring by glyph so a wide glyph
    //  is never split across the boundary. Returns the original when it fits.
    public string Truncate(string text, int maxWidth, out bool wasTruncated)
    {
        wasTruncated = false;

        if (string.IsNullOrEmpty(text) || maxWidth <= 0)
        {
            wasTruncated = !string.IsNullOrEmpty(text);
            return string.Empty;
        }

        List<Glyph> glyphs = Glyph.Split(text);
        StringBuilder builder = new(text.Length);
        int used = 0;

        foreach (Glyph glyph in glyphs)
        {
            if (glyph.IsZeroWidth)
                continue;

            int width = Measure(glyph);

            if (width == WidthTable.Unclassified)
                width = 1;
            
            if (used + width > maxWidth)
            {
                wasTruncated = true;
                break;
            }

            builder.Append(glyph.Value);
            used += width;
        }

        return builder.ToString();
    }

    // Cheap enough to call from the tick loop on an interval.
    public void RevalidateCanaries()
    {
        if (!_isEnabled || _canaryWidths.Count == 0)
            return;

        foreach (int codepoint in Canaries)
        {
            int current = ProbeCodepoint(codepoint);

            if (_canaryWidths.TryGetValue(codepoint, out int expected)
                && current == expected)
            {
                continue;
            }

            Trace.TraceWarning(
                $"Width canary U+{codepoint:X4} changed from {expected} to {current}; "
                + "font family likely changed. Width tables dumped.");

            Dump();
            Invalidated?.Invoke();

            return;
        }
    }

    public void Dump()
    {
        _codepointWidths.Clear();
        _glyphWidths.Clear();
        _canaryWidths.Clear();
    }

    // Every probed codepoint and its measured width, for the diagnostic dump.
    public IEnumerable<KeyValuePair<int, int>> ProbedCodepoints()
        => _codepointWidths;

    public IEnumerable<KeyValuePair<string, int>> ProbedClusters()
        => _glyphWidths;

    private void CaptureCanaries()
    {
        foreach (int codepoint in Canaries)
            _canaryWidths[codepoint] = ProbeCodepoint(codepoint);
    }

    private int ProbeCodepoint(int codepoint)
    {
        if (Glyph.IsZeroWidthCodepoint(codepoint))
            return Unclassified;

        try
        {
            string value = codepoint <= 0xFFFF
                ? ((char)codepoint).ToString()
                : char.ConvertFromUtf32(codepoint);

            return ProbeString(value);
        }
        catch (ArgumentOutOfRangeException)
        {
            return Unclassified;
        }
    }

    private int ProbeString(string value)
    {
        if (!_isEnabled)
            return 1;

        int originalBufferHeight = System.Console.BufferHeight;
        int row = AcquireScratchRow();

        try
        {
            System.Console.SetCursorPosition(ScratchColumn, row);
            System.Console.Write(value);

            int displacement = System.Console.CursorLeft - ScratchColumn;

            System.Console.SetCursorPosition(ScratchColumn, row);
            System.Console.Write("   ");

            return displacement == 1 || displacement == 2
                ? displacement
                : Unclassified;
        }
        catch (Exception ex)
        {
            Trace.TraceWarning($"Width probe failed for '{value}': {ex.Message}");
            return Unclassified;
        }
        finally
        {
            ReleaseScratchRow(originalBufferHeight);
        }
    }

    // Grows the buffer by one row so the probe can write below the visible
    //  window, leaving rendered content untouched. Falls back to the last
    //  visible row if the host refuses to resize.
    private static int AcquireScratchRow()
    {
        int windowHeight = System.Console.WindowHeight;

        try
        {
            if (System.Console.BufferHeight <= windowHeight)
                System.Console.BufferHeight = windowHeight + 1;

            return windowHeight;
        }
        catch (ArgumentOutOfRangeException)
        {
            return windowHeight - 1;
        }
        catch (System.IO.IOException)
        {
            return windowHeight - 1;
        }
    }

    private static void ReleaseScratchRow(int originalBufferHeight)
    {
        try
        {
            System.Console.SetWindowPosition(0, 0);

            if (System.Console.BufferHeight != originalBufferHeight)
                System.Console.BufferHeight = originalBufferHeight;
        }
        catch (Exception ex)
        {
            Trace.TraceWarning($"Could not restore scratch row state: {ex.Message}");
        }
    }
}