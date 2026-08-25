using System;
using Lattice.Drawing;
using Lattice.Input;
using Lattice.Text;

namespace Lattice.Elements;

[FillWidth]
[FixedHeight(1)]
public sealed class ButtonElement : Element, IEnableable, IFocusable, IAcceptsInput
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

    public bool AcceptsInput { get; set; } = true;

    public bool IsEnableable { get; set; } = true;

    public bool IsEnabled { get; set; } = true;

    public bool IsFocusable { get; set; } = true;

    public bool IsFocused { get; private set; }

    public Action? OnActivate { get; set; }

    public override Rectangle MeasureAdaptive()
        => new(0, 0, WidthTable.Current.Measure(_label) + 2, 1);

    public void OnEnabled()
    {
    }

    public void OnDisabled()
    {
        IsFocused = false;
    }

    public void OnFocusGained()
        => IsFocused = true;

    public void OnFocusLost()
        => IsFocused = false;

    public bool HandleInput(ConsoleKeyInfo key, KeyEvent keyEvent)
    {
        if (IsEnableable && !IsEnabled)
            return false;

        if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Spacebar)
            return false;

        if (keyEvent != KeyEvent.Pressed)
            return true;

        OnActivate?.Invoke();

        return true;
    }

    public override void Render(DrawSurface surface)
    {
        string text = Compose(surface.Width);

        if (IsEnableable && !IsEnabled)
            surface.Text(0, 0, text, ConsoleColor.DarkGray, null);
        else if (IsFocusable && IsFocused)
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