using System;
using Lattice.Drawing;
using Lattice.Text;

namespace Lattice.Elements;

[AdaptiveWidth]
[FixedHeight(1)]
public sealed class TextElement : Element
{
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set
        {
            string incoming = value ?? string.Empty;

            if (string.Equals(_text, incoming, StringComparison.Ordinal))
                return;

            _text = incoming;
            RaiseChanged(ValueChangedEventArgs.Instance);
        }
    }

    public ConsoleColor Foreground { get; set; } = ConsoleColor.Gray;

    public ConsoleColor? Background { get; set; }

    public override Rectangle MeasureAdaptive()
        => new(0, 0, WidthTable.Current.Measure(_text), 1);

    public override void Render(bool isFocused, DrawSurface surface)
        => surface.Text(0, 0, _text, Foreground, Background);
}