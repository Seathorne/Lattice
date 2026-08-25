using System;

namespace Lattice.Input;

public readonly struct InputEvent(ConsoleKeyInfo key, KeyEvent keyEvent)
{
    public ConsoleKeyInfo Key { get; } = key;

    public KeyEvent Event { get; } = keyEvent;

    public override string ToString() => $"InputEvent({Key.Key}, {Event})";
}