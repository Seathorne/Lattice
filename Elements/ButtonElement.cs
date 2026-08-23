using System;
using Lattice.Drawing;
using Lattice.Text;

namespace Lattice.Elements;

[FillWidth]
[FixedHeight(1)]
public sealed class ButtonElement : Element
{
    private string _label = string.Empty;

    public string Label
    {
        get => _label;
        set
        {
            string incoming = value ?? string.Empty;

            if (string.Equals(_label, incoming, StringComparison.Ordinal))
                return;

            _label = incoming;
            RaiseChanged(ValueChangedEventArgs.Instance);
        }
    }

    public bool IsEnabled { get; set; } = true;

    public bool IsFocusable => IsEnabled;

    public Action? OnActivate { get; set; }

    public override Rectangle MeasureAdaptive()
        => new(0, 0, WidthTable.Current.Measure(_label) + 2, 1);

    public override void Render(bool isFocused, DrawSurface surface)
    {
        string text = Compose(surface.Width);

        if (!IsEnabled)
            surface.Text(0, 0, text, ConsoleColor.DarkGray, null);
        else if (isFocused)
            surface.Text(0, 0, text, ConsoleColor.Black, ConsoleColor.Gray);
        else
            surface.Text(0, 0, text, ConsoleColor.Gray, null);
    }

    private string Compose(int available)
    {
        int interior = available - 2;

        if (interior <= 0)
            return "[]";

        int labelWidth = WidthTable.Current.Measure(_label);

        if (labelWidth >= interior)
            return $"[{_label}]";

        int padding = interior - labelWidth;
        int left = padding / 2;
        int right = padding - left;

        return $"[{new string(' ', left)}{_label}{new string(' ', right)}]";
    }
}