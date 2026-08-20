namespace Lattice.Elements;

public sealed class ChildAddedEventArgs(Element child) : ElementChangedEventArgs
{
    public Element Child { get; } = child;
}