using System.Diagnostics;
using Lattice.Console;
using Lattice.Drawing;

namespace Lattice.Rendering;

public static class BorderGlyphs
{
    public static readonly BorderGlyphSet LightSolid = new('\u250C', '\u2510', '\u2514', '\u2518', '\u2500', '\u2502');

    public static readonly BorderGlyphSet LightSparse = new('\u250C', '\u2510', '\u2514', '\u2518', '\u254C', '\u254E');

    public static readonly BorderGlyphSet LightMedium = new('\u250C', '\u2510', '\u2514', '\u2518', '\u2504', '\u2506');

    public static readonly BorderGlyphSet LightDense = new('\u250C', '\u2510', '\u2514', '\u2518', '\u2508', '\u250A');

    public static readonly BorderGlyphSet HeavySolid = new('\u250F', '\u2513', '\u2517', '\u251B', '\u2501', '\u2503');

    public static readonly BorderGlyphSet HeavySparse = new('\u250F', '\u2513', '\u2517', '\u251B', '\u254D', '\u254F');

    public static readonly BorderGlyphSet HeavyMedium = new('\u250F', '\u2513', '\u2517', '\u251B', '\u2505', '\u2507');

    public static readonly BorderGlyphSet HeavyDense = new('\u250F', '\u2513', '\u2517', '\u251B', '\u2509', '\u250B');

    public static readonly BorderGlyphSet Double = new('\u2554', '\u2557', '\u255A', '\u255D', '\u2550', '\u2551');

    public static readonly BorderGlyphSet Wide = new('\u2588', '\u2588', '\u2588', '\u2588', '\u3161', '\uFF5C');

    public static readonly BorderGlyphSet Ascii = new('+', '+', '+', '+', '-', '|');

    public static BorderGlyphSet GetBorderGlyphs(Border border, HostType hostType)
    {
        if (border.IsAsciiOnly || hostType == HostType.Conhost)
            return Ascii;
        
        if (border.Scale == BorderScale.Wide)
            return Wide;

        if (border.Weight == BorderWeight.Double)
            return Double;

        return (border.Weight, border.Style) switch
        {
            (BorderWeight.Light, BorderStyle.Solid)  => LightSolid,
            (BorderWeight.Light, BorderStyle.Sparse) => LightSparse,
            (BorderWeight.Light, BorderStyle.Medium) => LightMedium,
            (BorderWeight.Light, BorderStyle.Dense)  => LightDense,
            (BorderWeight.Heavy, BorderStyle.Solid)  => HeavySolid,
            (BorderWeight.Heavy, BorderStyle.Sparse) => HeavySparse,
            (BorderWeight.Heavy, BorderStyle.Medium) => HeavyMedium,
            (BorderWeight.Heavy, BorderStyle.Dense)  => HeavyDense,
            _ => Fallback(border.Style, border.Weight),
        };
    }

    private static BorderGlyphSet Fallback(BorderStyle style, BorderWeight weight)
    {
        Trace.TraceWarning($"No glyph set for {style}/{weight}; using {nameof(LightSolid)}.");
        return LightSolid;
    }
}