namespace Lattice.Drawing;

public readonly struct Rectangle(int x, int y, int width, int height)
{
    public int X { get; } = x;

    public int Y { get; } = y;

    public int Width { get; } = width;

    public int Height { get; } = height;

    public int Left => X;

    public int Top => Y;

    public int Right => X + Width - 1;

    public int Bottom => Y + Height - 1;

    public bool IsEmpty => Width <= 0 || Height <= 0;

    public bool Contains(int x, int y) =>
        x >= Left && x <= Right &&
        y >= Top && y <= Bottom;

    public bool Contains(Rectangle other) =>
        other.Left >= Left && other.Top >= Top &&
        other.Right <= Right && other.Bottom <= Bottom;

    public override string ToString() => $"Rectangle({X}, {Y}, {Width}, {Height})";
}