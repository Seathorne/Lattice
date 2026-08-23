namespace Lattice.Text;

// An inclusive range of Unicode codepoints.
public readonly struct CodepointRange(int start, int end)
{
    public int Start { get; } = start;

    public int End { get; } = end;

    public int Count => End - Start + 1;

    public bool Contains(int codepoint)
        => codepoint >= Start && codepoint <= End;

    public override string ToString()
        => $"U+{Start:X4}-U+{End:X4}";
}