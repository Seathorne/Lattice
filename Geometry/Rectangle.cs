namespace Lattice.Geometry;

public readonly struct Rectangle(int x, int y, int width, int height)
{
    public int X { get; } = x;

    public int Y { get; } = y;

    public int Width { get; } = width;
    
    public int Height { get; } = height;

    public bool Contains(int x, int y) =>
        x >= X && x < X + Width &&
        y >= Y && y < Y + Height;

    public override string ToString() => $"Rectangle({X}, {Y}, {Width}, {Height})";
}