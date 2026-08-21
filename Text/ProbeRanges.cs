using System.Collections.Generic;

namespace Lattice.Text;

// Contains codepoint ranges the width probe walks at startup. Anything outside
//  these ranges is measured lazily on first use.
public static class ProbeRanges
{
    // The ranges a terminal application is likely to draw from. Roughly 2,300
    //  codepoints. CJK ideographs, Hangul, and emoji are deliberately absent;
    //  they are large, uniformly wide, and cheap to resolve lazily.
    public static IReadOnlyList<CodepointRange> Default { get; } =
    [
        new CodepointRange(0x00A0, 0x017F), // Latin-1 Supplement, Latin Extended-A
        new CodepointRange(0x2010, 0x205E), // General Punctuation
        new CodepointRange(0x2190, 0x21FF), // Arrows
        new CodepointRange(0x2200, 0x22FF), // Mathematical Operators
        new CodepointRange(0x2300, 0x23FF), // Miscellaneous Technical
        new CodepointRange(0x2500, 0x257F), // Box Drawing
        new CodepointRange(0x2580, 0x259F), // Block Elements
        new CodepointRange(0x25A0, 0x25FF), // Geometric Shapes
        new CodepointRange(0x2600, 0x26FF), // Miscellaneous Symbols
        new CodepointRange(0x2700, 0x27BF), // Dingbats
        new CodepointRange(0x2800, 0x28FF), // Braille Patterns
        new CodepointRange(0x3000, 0x303F), // CJK Symbols and Punctuation
    ];

    // Opt in when the application is localized to Korean.
    public static CodepointRange HangulSyllables { get; }
        = new CodepointRange(0xAC00, 0xD7A3);
    
    // Supplementary plane; Windows Terminal only.
    public static CodepointRange MahjongTiles { get; }
        = new CodepointRange(0x1F000, 0x1F02B);
}