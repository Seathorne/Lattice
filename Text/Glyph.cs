using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Lattice.Text;

public readonly struct Glyph(string value) : IEquatable<Glyph>
{
    public const string NarrowReplacement = "\u25A1";  // WHITE SQUARE (TOFU) □

    public const string WideReplacement = "\u3013";    // GETA MARK 〓

    private const int ZeroWidthJoiner = 0x200D;

    private readonly string? _value = Validate(value);

    public static Glyph Narrow { get; } = new Glyph(NarrowReplacement);

    public static Glyph Wide { get; } = new Glyph(WideReplacement);

    // A default-constructed Glyph bypasses the primary constructor and holds
    //  null. Reading as tofu means a missed initialization draws wrong rather
    //  than throwing mid-render.
    public string Value => _value ?? NarrowReplacement;

    public bool IsDefault => _value is null;

    public bool IsAscii
    {
        get
        {
            string value = Value;
            return value.Length == 1 && value[0] < 0x80;
        }
    }

    public bool IsZeroWidth
        => TryGetSingleCodepoint(out int cp) && IsZeroWidthCodepoint(cp);

    public static implicit operator Glyph(char c)
        => new(c.ToString());

    public static bool operator ==(Glyph left, Glyph right)
        => left.Equals(right);

    public static bool operator !=(Glyph left, Glyph right)
        => !left.Equals(right);

    public static bool IsZeroWidthCodepoint(int cp)
        => cp == 0x00AD                          // SOFT HYPHEN
        || cp == 0x200B                          // ZERO WIDTH SPACE
        || cp == 0x200C                          // ZERO WIDTH NON-JOINER
        || cp == 0x200D                          // ZERO WIDTH JOINER
        || (cp >= 0x200E && cp <= 0x200F)        // LRM, RLM
        || (cp >= 0x202A && cp <= 0x202E)        // bidirectional embedding controls
        || (cp >= 0x2060 && cp <= 0x2064)        // word joiner, invisible operators
        || (cp >= 0x2066 && cp <= 0x2069)        // bidirectional isolates
        || cp == 0xFEFF;                         // ZERO WIDTH NO-BREAK SPACE

    // Width 0 means the probe failed outright, which is treated as narrow.
    public static Glyph Replacement(int width)
        => width == 2 ? Wide : Narrow;

    // Segments a string into glyphs using the same cluster rules as Validate.
    // Unlike the constructor, this never throws: a malformed sequence yields
    // its parts as separate glyphs rather than rejecting the whole string.
    public static List<Glyph> Split(string text)
    {
        List<Glyph> glyphs = [];

        if (string.IsNullOrEmpty(text))
            return glyphs;

        List<int> codepoints = ToCodepoints(text);
        int i = 0;

        while (i < codepoints.Count)
        {
            int start = i;
            i++;

            if (IsRegionalIndicator(codepoints[start])
                && i < codepoints.Count
                && IsRegionalIndicator(codepoints[i]))
            {
                i++;
            }
            else
            {
                while (i < codepoints.Count)
                {
                    int cp = codepoints[i];

                    if (IsCombining(cp) || IsVariationSelector(cp))
                    {
                        i++;
                        continue;
                    }

                    if (cp == ZeroWidthJoiner && i + 1 < codepoints.Count)
                    {
                        i += 2;
                        continue;
                    }

                    break;
                }
            }

            glyphs.Add(new Glyph(FromCodepoints(codepoints, start, i - start)));
        }

        return glyphs;
    }

    public bool TryGetSingleCodepoint(out int codepoint)
    {
        string value = Value;

        if (value.Length == 1)
        {
            codepoint = value[0];
            return true;
        }

        if (value.Length == 2
            && char.IsHighSurrogate(value[0])
            && char.IsLowSurrogate(value[1]))
        {
            codepoint = char.ConvertToUtf32(value[0], value[1]);
            return true;
        }

        codepoint = 0;
        return false;
    }

    public bool Equals(Glyph other)
        => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj)
        => obj is Glyph other && Equals(other);

    public override int GetHashCode()
        => Value.GetHashCode();

    public override string ToString() => Value;

    private static string Validate(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Glyph cannot be empty.", nameof(value));

        List<int> codepoints = ToCodepoints(value);

        if (codepoints.Count == 1)
            return value;

        if (IsRegionalIndicator(codepoints[0]))
        {
            if (codepoints.Count == 2 && IsRegionalIndicator(codepoints[1]))
                return value;

            throw new ArgumentException("A regional indicator must be followed by exactly one more.", nameof(value));
        }

        for (int i = 1; i < codepoints.Count; i++)
        {
            int cp = codepoints[i];

            if (IsCombining(cp) || IsVariationSelector(cp))
                continue;

            if (cp == ZeroWidthJoiner)
            {
                if (i + 1 >= codepoints.Count)
                    throw new ArgumentException("Glyph ends with a zero-width joiner.", nameof(value));

                i++;
                continue;
            }

            throw new ArgumentException("Glyph must be exactly one grapheme cluster.", nameof(value));
        }

        return value;
    }

    private static bool IsRegionalIndicator(int cp)
        => cp >= 0x1F1E6 && cp <= 0x1F1FF;

    private static bool IsVariationSelector(int cp)
        => cp >= 0xFE00 && cp <= 0xFE0F;

    private static bool IsCombining(int cp)
    {
        if (cp > 0xFFFF)
            return false;

        UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory((char)cp);

        return category == UnicodeCategory.NonSpacingMark
            || category == UnicodeCategory.SpacingCombiningMark
            || category == UnicodeCategory.EnclosingMark;
    }

    private static List<int> ToCodepoints(string s)
    {
        // Warns with suggestion to change to ' = [with(s.Length)];'
        #pragma warning disable IDE0028
        List<int> result = new(s.Length);
        #pragma warning restore IDE0028

        for (int i = 0; i < s.Length; i++)
        {
            if (char.IsHighSurrogate(s[i]) && i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
            {
                result.Add(char.ConvertToUtf32(s[i], s[i + 1]));
                i++;
            }
            else
            {
                result.Add(s[i]);
            }
        }

        return result;
    }

    private static string FromCodepoints(List<int> codepoints, int start, int count)
    {
        StringBuilder builder = new(count + 2);

        for (int i = start; i < start + count; i++)
        {
            int cp = codepoints[i];

            if (cp <= 0xFFFF)
                builder.Append((char)cp);
            else
                builder.Append(char.ConvertFromUtf32(cp));
        }

        return builder.ToString();
    }
}