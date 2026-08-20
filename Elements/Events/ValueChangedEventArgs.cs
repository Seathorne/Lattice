namespace Lattice.Elements;

public sealed class ValueChangedEventArgs : ElementChangedEventArgs
{
    public static readonly ValueChangedEventArgs Instance = new();

    private ValueChangedEventArgs() { }
}