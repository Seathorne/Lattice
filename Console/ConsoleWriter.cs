using System;

namespace Lattice.Console;

// The only class that writes to System.Console. Owns cursor positioning
//  and color.
public sealed class ConsoleWriter
{
    public void Write(int x, int y, string text)
    {
        System.Console.SetCursorPosition(x, y);
        System.Console.Write(text);
    }
    
    public void Write(int x, int y, string text, ConsoleColor foreground, ConsoleColor? background)
    {
        ConsoleColor previousForeground = System.Console.ForegroundColor;
        ConsoleColor previousBackground = System.Console.BackgroundColor;

        System.Console.ForegroundColor = foreground;

        if (background.HasValue)
            System.Console.BackgroundColor = background.Value;

        Write(x, y, text);

        System.Console.ForegroundColor = previousForeground;
        System.Console.BackgroundColor = previousBackground;
    }

    public void Clear()
        => System.Console.Clear();

    public void ClearLine(int y)
        => Write(0, y, new string(' ', System.Console.WindowWidth));

    public void ClearRegion(int x, int y, int width, int height)
    {
        string blank = new(' ', width);

        for (int row = y; row < y + height; row++)
            Write(x, row, blank);
    }
}