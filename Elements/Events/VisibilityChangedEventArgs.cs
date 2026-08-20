namespace Lattice.Elements;

public sealed class VisibilityChangedEventArgs(bool isVisible) : ElementChangedEventArgs
{
    public bool IsVisible { get; } = isVisible;
}