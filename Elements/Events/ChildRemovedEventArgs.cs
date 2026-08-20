namespace Lattice.Elements;

public sealed class ChildRemovedEventArgs(Element child) : ElementChangedEventArgs
{
    public Element Child { get; } = child;
}