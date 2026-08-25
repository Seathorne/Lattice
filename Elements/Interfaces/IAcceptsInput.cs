using System;
using Lattice.Input;

namespace Lattice.Elements;

public interface IAcceptsInput
{
    bool AcceptsInput { get; set; }

    bool HandleInput(ConsoleKeyInfo key, KeyEvent keyEvent);
}