using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Lattice.Console;
using Lattice.Drawing;
using Lattice.Focus;
using Lattice.Input;
using Lattice.Layout;
using Lattice.Rendering;
using Lattice.Screens;
using Lattice.Text;

namespace Lattice;

public class Terminal
{
    public const int MinimumWidth = 48;

    public const int MinimumHeight = 22;

    public ConsoleWriter Writer { get; } = new();

    public HostType HostType { get; private set; }

    public WidthTable WidthTable { get; } = new();

    public bool IsUnicodeEnabled => HostType == HostType.WindowsTerminal;

    public void Run(Screen screen)
    {
        Configure();
        EnforceSizeOrBlock();

        DetectHost();
        WidthTable.Initialize(IsUnicodeEnabled);
        WidthTable.SetCurrent(WidthTable);
        WidthTable.ProbeRanges(ProbeRanges.Default);

        FocusManager focus = new();
        FocusManager.SetCurrent(focus);

        Renderer renderer = new(Writer, WidthTable, HostType);
        LayoutEngine layout = new();
        InputHandler input = new();

        screen.OnEnter();
        focus.Load(screen.Root);

        input.Start();

        try
        {
            renderer.Clear();
            Render(renderer, layout, screen);

            while (!input.ExitRequested)
            {
                if (!input.TryTake(out InputEvent inputEvent, Timeout.Infinite))
                    break;

                input.Route(inputEvent, screen);

                if (input.ExitRequested)
                {
                    renderer.Clear();
                    break;
                }

                Render(renderer, layout, screen);
            }
        }
        finally
        {
            input.Stop();
            screen.OnExit();
            System.Console.CursorVisible = true;
        }
    }

    public void RunDiagnostics(IEnumerable<CodepointRange>? extraRanges = null)
    {
        Configure();
        System.Console.CursorVisible = false;

        DetectHost();
        WidthTable.Initialize(IsUnicodeEnabled);
        WidthTable.SetCurrent(WidthTable);

        List<CodepointRange> ranges = [.. ProbeRanges.Default];

        if (extraRanges is not null)
            ranges.AddRange(extraRanges);

        System.Console.Clear();
        System.Console.WriteLine($"Host:    {HostType}");
        System.Console.WriteLine($"Unicode: {IsUnicodeEnabled}");
        System.Console.WriteLine($"Window:  {System.Console.WindowWidth}x{System.Console.WindowHeight}");
        System.Console.WriteLine();
        System.Console.WriteLine($"Probing {ranges.Sum(r => r.Count)} codepoints across {ranges.Count} ranges...");

        WidthTable.ProbeRanges(ranges);

        System.Console.Write("Done. Press any key to page through results.");

        System.Console.ReadKey(intercept: true);

        DumpWidths(ranges);

        System.Console.CursorVisible = true;
    }

    private static void Configure()
    {
        // Must precede any write. Without it, Unicode either throws or renders
        //  as '?' depending on the active code page.
        System.Console.OutputEncoding = Encoding.UTF8;

        System.Console.CursorVisible = false;
        System.Console.TreatControlCAsInput = true;

        try
        {
            System.Console.BufferHeight = System.Console.WindowHeight;
        }
        catch (ArgumentOutOfRangeException)
        {
            // Buffer cannot shrink below the cursor row; the scrollbar stays
            // until the next resize.
        }
    }

    private static void EnforceSizeOrBlock()
    {
        while (System.Console.WindowWidth < MinimumWidth || System.Console.WindowHeight < MinimumHeight)
        {
            System.Console.Clear();

            string message = $"Resize terminal to at least {MinimumWidth}x{MinimumHeight}";
            int x = Math.Max(0, (System.Console.WindowWidth - message.Length) / 2);
            int y = System.Console.WindowHeight / 2;

            System.Console.SetCursorPosition(x, y);
            System.Console.Write(message);

            System.Threading.Thread.Sleep(200);
        }
    }

    private static void Render(Renderer renderer, LayoutEngine layout, Screen screen)
    {
        Rectangle bounds = new(
            0,
            0,
            System.Console.WindowWidth,
            System.Console.WindowHeight - 1);  // -1 to remove scratch row from width probe

        renderer.Render(layout.Layout(screen, bounds));
    }

    private void DetectHost()
    {
        HostType = Environment.GetEnvironmentVariable("WT_SESSION") is not null
            ? HostType.WindowsTerminal
            : HostType.Conhost;
    }

    private void DumpWidths(IEnumerable<CodepointRange> ranges)
    {
        Dictionary<int, int> widths = WidthTable
            .ProbedCodepoints()
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        foreach (CodepointRange range in ranges)
        {
            System.Console.Clear();
            System.Console.WriteLine($"{range}   [1] single  [2] wide  [.] unclassified");
            System.Console.WriteLine();

            StringBuilder line = new();

            for (int codepoint = range.Start; codepoint <= range.End; codepoint++)
            {
                if ((codepoint - range.Start) % 16 == 0)
                {
                    if (line.Length > 0)
                        System.Console.WriteLine(line.ToString());

                    line.Clear();
                    line.Append($"{codepoint:X4}  ");
                }

                widths.TryGetValue(codepoint, out int width);

                line.Append(width switch
                {
                    1 => "1 ",
                    2 => "2 ",
                    _ => ". ",
                });
            }

            if (line.Length > 0)
                System.Console.WriteLine(line.ToString());

            System.Console.WriteLine();
            System.Console.Write("Any key for next range...");
            System.Console.ReadKey(intercept: true);
        }

        System.Console.Clear();
        System.Console.WriteLine("Sample rendering:");
        System.Console.WriteLine();

        foreach (CodepointRange range in ranges)
        {
            StringBuilder sample = new();

            for (int codepoint = range.Start; codepoint <= Math.Min(range.End, range.Start + 31); codepoint++)
            {
                string value = codepoint <= 0xFFFF
                    ? ((char)codepoint).ToString()
                    : char.ConvertFromUtf32(codepoint);

                sample.Append(WidthTable.Resolve(value));
            }

            System.Console.WriteLine($"{range}  {sample}");
        }

        System.Console.WriteLine();
        System.Console.Write("Any key to exit...");
        System.Console.ReadKey(intercept: true);
    }
}